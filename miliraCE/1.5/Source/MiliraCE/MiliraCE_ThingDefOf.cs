using RimWorld;
using Verse;

namespace MiliraCE
{
    [DefOf]
    public static class MiliraCE_ThingDefOf
    {
        public static ThingDef ExplosionCEDirectional;

        static MiliraCE_ThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MiliraCE_ThingDefOf));
        }
    }
}
