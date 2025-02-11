using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch]
internal class Harmony_DangerWatcher
{
	private const string className = "<>c";

	private const string methodName = "<CalculateDangerRating>";

	public static MethodBase TargetMethod()
	{
		IEnumerable<Type> source = from x in typeof(DangerWatcher).GetNestedTypes(AccessTools.all)
			where x.Name.Contains("<>c")
			select x;
		if (!source.Any())
		{
			Log.Error("CombatExtended :: Harmony_DangerWatcher couldn't find part `<>c`");
		}
		MethodInfo methodInfo = source.SelectMany((Type x) => x.GetMethods(AccessTools.all)).FirstOrDefault((MethodInfo x) => x.Name.Contains("<CalculateDangerRating>") && x.ReturnType == typeof(float));
		if (methodInfo == null)
		{
			Log.Error("CombatExtended :: Harmony_DangerWatcher couldn't find `<>c` sub-class containing `<CalculateDangerRating>`");
		}
		return methodInfo;
	}

	[HarmonyPostfix]
	public static void PostFix(IAttackTarget t, ref float __result)
	{
		if (t is Building_TurretGunCE building_TurretGunCE && building_TurretGunCE.def.building.IsMortar && !building_TurretGunCE.IsMannable)
		{
			__result = building_TurretGunCE.def.building.combatPower;
		}
	}
}
