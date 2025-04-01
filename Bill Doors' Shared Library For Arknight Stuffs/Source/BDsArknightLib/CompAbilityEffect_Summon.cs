using RimWorld;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace BDsArknightLib
{
    public abstract class CompAbilityEffect_Summon : CompAbilityEffect
    {
        CompProperties_AbilityEffect_Summon Props => props as CompProperties_AbilityEffect_Summon;

        protected Lord lord;

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return true;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            lord = LordMaker.MakeNewLord(parent.pawn.Faction, new LordJob_Variable(ConfigureDuty(target), Props.dutyFocusOnTarget ? target : null, null, null, Props.dutyRadius), parent.pawn.Map);
        }

        public virtual void GeneratePawn(IntVec3 cell, LocalTargetInfo target)
        {
            Pawn p = PawnGenerator.GeneratePawn(Props.pawnkindDef, parent.pawn.Faction);
            GenPlace.TryPlaceThing(p, cell, parent.pawn.Map, ThingPlaceMode.Near);
            Props.spawnEffecter?.SpawnMaintainedIfPossible(target, cell, parent.pawn.Map, Props.effecterScale);
            lord.AddPawn(p);
            if (Props.timedDisappearHediffDef != null)
            {
                var h = p.health.AddHediff(Props.timedDisappearHediffDef);
                if (h.TryGetComp<HediffComp_Disappears>() is HediffComp_Disappears comp)
                {
                    comp.SetDuration(Props.lifetimeSecondsRange.RandomInRange.SecondsToTicks());
                }
            }
        }

        public virtual DutyDef ConfigureDuty(LocalTargetInfo target)
        {
            var p = target.Thing;
            return p == null ? Props.locationDutyDef : (p.HostileTo(parent.pawn) ? Props.hostileDutyDef : Props.dutyDef);
        }
    }

    public class CompProperties_AbilityEffect_Summon : CompProperties_AbilityEffect
    {
        public EffecterDef spawnEffecter;

        public int summonCount = 1;

        public PawnKindDef pawnkindDef;

        public DutyDef dutyDef;

        public DutyDef hostileDutyDef;

        public DutyDef locationDutyDef;

        public float dutyRadius = 5;

        public bool dutyFocusOnTarget = true;

        public float effecterScale = 1;

        public HediffDef timedDisappearHediffDef;

        public FloatRange lifetimeSecondsRange = new FloatRange(60, 90);
    }
}
