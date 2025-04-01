using BillDoorsFramework;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using static HarmonyLib.Code;

namespace BDsArknightLib
{
    [StaticConstructorOnStartup]
    public class CompShieldOneUse : ThingComp, IGenericBar
    {
        protected float energy;

        public CompProperties_ShieldOneUse Props => (CompProperties_ShieldOneUse)props;
        Gizmo_GenericBar gizmo;

        private float EnergyMax = 100;

        public float pct => energy / EnergyMax;

        public float Energy => energy;

        int nextHitEffectTick = 0;

        private static readonly Material ChargBarMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.89f, 0.42f, 0.2f));

        private static readonly Material emptyMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f, 0.75f));

        private static readonly Vector2 BarSize = new Vector2(0.8f, 0.1f);

        public ShieldState ShieldState
        {
            get
            {
                return ShieldState.Active;
            }
        }

        protected Pawn PawnOwner
        {
            get
            {
                if (parent is Apparel apparel)
                {
                    return apparel.Wearer;
                }
                if (parent is Pawn result)
                {
                    return result;
                }
                return null;
            }
        }

        public void SetMaxEnergy(float maxEnergy)
        {
            this.EnergyMax = maxEnergy;
            energy = EnergyMax;
        }

        public bool IsApparel => parent is Apparel;

        private bool IsBuiltIn => !IsApparel;

        public float MaxFillableBarLength => EnergyMax;

        public float CurrentFillableBarLength => Energy;

        public string FillableBarTitle => parent.Label;

        public Color? FillableBarColor => null;

        public Color? FillableBarBGColor => null;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref EnergyMax, "EnergyMax");
            Scribe_Values.Look(ref energy, "energy", 0f);
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            energy = EnergyMax;
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetWornGizmosExtra())
            {
                yield return item;
            }
            if (IsApparel)
            {
                foreach (Gizmo gizmo in GetGizmos())
                {
                    yield return gizmo;
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            if (!IsBuiltIn)
            {
                yield break;
            }
            foreach (Gizmo gizmo in GetGizmos())
            {
                yield return gizmo;
            }
        }

        private IEnumerable<Gizmo> GetGizmos()
        {
            if (((PawnOwner.Faction == Faction.OfPlayer || DebugSettings.ShowDevGizmos) || (parent is Pawn pawn && pawn.RaceProps.IsMechanoid)) && Find.Selector.SingleSelectedThing == PawnOwner)
            {
                if (gizmo == null)
                {
                    gizmo = new Gizmo_GenericBar
                    {
                        mag = this
                    };
                }
                yield return gizmo;
            }
        }

        public override float CompGetSpecialApparelScoreOffset()
        {
            return -114514;
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (!dinfo.Def.harmsHealth || (PawnOwner != null && !dinfo.Def.ExternalViolenceFor(PawnOwner)))
            {
                return;
            }
            if (dinfo.Def == DamageDefOf.SurgicalCut) return;
            if (Props.damageDefBlacklist.NotNullAndContains(dinfo.Def)) return;
            float pctCache = pct;
            energy -= dinfo.Amount;
            if (Find.TickManager.TicksGame > nextHitEffectTick)
            {
                Props.hitEffecter?.Spawn(parent.PositionHeld, parent.MapHeld);
                nextHitEffectTick = Find.TickManager.TicksGame + Props.minHitEffectInterval;
            }
            if (Props.shatterSounds.Any())
            {
                float amount = dinfo.Amount;
                Props.shatterSounds.Where(s => amount > s.minDamage).RandomElement()?.hitSound?.PlayOneShot(new TargetInfo(parent.PositionHeld, parent.MapHeld));
            }
            foreach (float stage in Props.stages)
            {
                if (pctCache >= stage && pct < stage)
                {
                    if (Props.stageShatterRule != null && Props.logEntryDef != null)
                    {
                        Find.BattleLog.Add(new BattleLogEntry_ShieldStageShatter(PawnOwner, Props.stageShatterRule, dinfo.Instigator, pct) { def = Props.logEntryDef });
                    }
                    Props.stageShatterEffecter?.Spawn(parent.PositionHeld, parent.MapHeld);
                    break;
                }
            }
            if (energy < 0f)
            {
                if (Props.shieldShatterRule != null && Props.logEntryDef != null)
                {
                    Find.BattleLog.Add(new BattleLogEntry_EventWithIcon(PawnOwner, Props.shieldShatterRule, dinfo.Instigator) { def = Props.logEntryDef });
                }
                Break();
            }
            absorbed = true;
        }

        private void Break()
        {
            Props.shatterEffecter?.Spawn(parent.PositionHeld, parent.MapHeld);
            parent.Destroy();
        }

        public override void CompDrawWornExtras()
        {
            base.CompDrawWornExtras();
            if (IsApparel)
            {
                Draw();
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (IsBuiltIn)
            {
                Draw();
            }
        }

        private void Draw()
        {
            if (PawnOwner != null && !PawnOwner.Faction.IsPlayer && pct < 1 && Find.TickManager.TicksGame < nextHitEffectTick + 1200)
            {
                GenDraw.FillableBarRequest r = default;
                r.center = PawnOwner.DrawPos + Vector3.up * 1.5f + Vector3.back * 0.5f;
                r.size = BarSize;
                r.fillPercent = pct;
                r.filledMat = ChargBarMat;
                r.unfilledMat = emptyMat;
                r.margin = 0;
                GenDraw.DrawFillableBar(r);
            }
        }

    }

    public class CompProperties_ShieldOneUse : CompProperties
    {
        public CompProperties_ShieldOneUse()
        {
            compClass = typeof(CompShieldOneUse);
        }

        public int minHitEffectInterval = 30;

        public EffecterDef hitEffecter;

        public EffecterDef shatterEffecter;

        public EffecterDef stageShatterEffecter;

        public List<float> stages = new List<float>();

        public List<SoundForDamage> shatterSounds;

        public List<DamageDef> damageDefBlacklist = new List<DamageDef>();

        public RulePackDef stageShatterRule;
        public RulePackDef shieldShatterRule;
        public LogEntryDef logEntryDef;
    }

    public class SoundForDamage
    {
        public SoundDef hitSound;

        public float minDamage = 0;
    }
}
