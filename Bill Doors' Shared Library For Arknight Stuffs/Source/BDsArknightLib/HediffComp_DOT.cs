using RimWorld;
using Verse;

namespace BDsArknightLib
{


    public class HediffComp_DOT : HediffComp_Aura
    {
        public HediffCompProperties_DOT Props => (HediffCompProperties_DOT)props;

        protected override void ApplyToTarget(Thing target)
        {
            target.TakeDamage(new DamageInfo(Props.damageDef, Props.damageRange.RandomInRange, Props.penetrationRange.RandomInRange, instigator: Pawn, intendedTarget: target));
            Props.effecterDef?.SpawnMaintainedIfPossible(target, target);
        }
    }

    public class HediffCompProperties_DOT : HediffCompProperties_Aura
    {
        public HediffCompProperties_DOT()
        {
            compClass = typeof(HediffComp_DOT);
        }

        public IntRange damageRange = new IntRange(1, 2);

        public DamageDef damageDef;

        public FloatRange penetrationRange = new FloatRange(-1, -1);

        public override void ResolveReferences(HediffDef parent)
        {
            base.ResolveReferences(parent);
            if (damageDef == null) damageDef = DamageDefOf.Cut;
        }
    }
}
