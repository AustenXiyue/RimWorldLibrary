using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace BDsArknightLib
{
    public class CompAbilityEffect_GiveApparel : CompAbilityEffect
    {
        private Pawn Pawn => parent.pawn;
        public new CompProperties_GiveApparel Props => (CompProperties_GiveApparel)props;

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (Pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }
            return target.Pawn?.apparel != null;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Pawn != null && Props.thingdef != null)
            {
                target.Pawn.apparel?.Wear(MakeApparel(), false, true);
                Props.effecter?.SpawnAttached(target.Pawn, target.Pawn.MapHeld);
            }
        }

        protected virtual Apparel MakeApparel()
        {
            return ThingMaker.MakeThing(Props.thingdef) as Apparel;
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return base.CanApplyOn(target, dest) && target.Pawn?.apparel != null;
        }
    }

    public class CompProperties_GiveApparel : CompProperties_AbilityEffect
    {
        public ThingDef thingdef;

        public EffecterDef effecter;

        public CompProperties_GiveApparel()
        {
            compClass = typeof(CompAbilityEffect_GiveApparel);
        }
    }
}
