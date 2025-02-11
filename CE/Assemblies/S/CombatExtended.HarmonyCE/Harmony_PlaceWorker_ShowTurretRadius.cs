using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch]
internal class Harmony_PlaceWorker_ShowTurretRadius
{
	private const string className = "<>c";

	private const string methodName = "<AllowsPlacing>";

	public static MethodBase TargetMethod()
	{
		IEnumerable<Type> source = from x in typeof(PlaceWorker_ShowTurretRadius).GetNestedTypes(AccessTools.all)
			where x.Name.Contains("<>c")
			select x;
		if (!source.Any())
		{
			Log.Error("CombatExtended :: Harmony_PlaceWorker_ShowTurretRadius couldn't find part `<>c`");
		}
		MethodInfo methodInfo = source.SelectMany((Type x) => x.GetMethods(AccessTools.all)).FirstOrDefault((MethodInfo x) => x.Name.Contains("<AllowsPlacing>"));
		if (methodInfo == null)
		{
			Log.Error("CombatExtended :: Harmony_PlaceWorker_ShowTurretRadius couldn't find `<>c` sub-class containing `<AllowsPlacing>`");
		}
		return methodInfo;
	}

	[HarmonyPostfix]
	public static void PostFix(VerbProperties v, ref bool __result)
	{
		__result = __result || v.verbClass == typeof(Verb_ShootCE);
	}
}
