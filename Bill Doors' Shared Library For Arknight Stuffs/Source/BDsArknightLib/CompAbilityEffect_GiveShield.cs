using RimWorld;
using Verse;

namespace BDsArknightLib
{
    public class CompAbilityEffect_GiveShield : CompAbilityEffect_GiveApparel
    {
        CompProperties_GiveShield Props => props as CompProperties_GiveShield;

        public virtual float shield => Props.shield;

        protected override Apparel MakeApparel()
        {
            var apparel = base.MakeApparel();
            apparel.GetComp<CompShieldOneUse>()?.SetMaxEnergy(shield);
            apparel.GetComp<CompDestroyAfterDelay>()?.PostSpawnSetup(false);
            return apparel;
        }
    }
    public class CompProperties_GiveShield : CompProperties_GiveApparel
    {
        public float shield;

        public CompProperties_GiveShield()
        {
            compClass = typeof(CompAbilityEffect_GiveShield);
        }
    }
}
