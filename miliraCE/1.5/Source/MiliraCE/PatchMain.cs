using HarmonyLib;
using System.Reflection;
using Verse;

namespace MiliraCE
{
    [StaticConstructorOnStartup]
    public static class PatchMain
    {
        static PatchMain()
        {
            Harmony harmony = new Harmony("MiliraCE.Main");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
