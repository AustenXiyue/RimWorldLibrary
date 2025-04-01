using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace BDsArknightLib
{
    public class HediffComp_DOTFreeze : HediffComp_DOT
    {
        public HediffCompProperties_DOTFreeze Props => (HediffCompProperties_DOTFreeze)props;
        protected override void ApplyToTarget(Thing target)
        {
            IceDamageUtil.ApplyChanceOverride = Props.freezeChance;
            base.ApplyToTarget(target);
        }
    }


    public class HediffCompProperties_DOTFreeze : HediffCompProperties_DOT
    {
        public HediffCompProperties_DOTFreeze()
        {
            compClass = typeof(HediffComp_DOTFreeze);
        }

        public float freezeChance = 0.1f;

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (var v in base.ConfigErrors(parentDef))
            {
                yield return v;
            }
            if (!damageDef.HasModExtension<ModExtension_DamageHediffs>())
            {
                Log.Error($"{parentDef.defName}'s HediffCompProperties_DOTFreeze has damagedef {damageDef.defName} without ModExtension_DamageHediffs");
            }
        }
    }
}
