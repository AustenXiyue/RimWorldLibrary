using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE.Compatibility;

public static class Harmony_VanillaExpandedFramework
{
	public class Harmony_Patch_PawnRenderer
	{
		private const string targetType = "VFECore.Patch_PawnRenderer";

		private const string targetMethod = "IsShell";

		private static MethodBase _target;

		public static bool Prepare()
		{
			return (_target = AccessTools.Method("VFECore.Patch_PawnRenderer:IsShell", (Type[])null, (Type[])null)) != null;
		}

		public static MethodBase TargetMethod()
		{
			return _target;
		}

		public static void Postfix(ApparelLayerDef def, ref bool __result)
		{
			if (def == CE_ApparelLayerDefOf.Backpack)
			{
				__result = false;
			}
			else if (def == CE_ApparelLayerDefOf.Webbing && !Controller.settings.ShowTacticalVests)
			{
				__result = false;
			}
			else
			{
				__result = __result || def.IsVisibleLayer();
			}
		}
	}

	[HarmonyPatch]
	public class Harmony_Patch_YeetVEshieldStat
	{
		private static MethodBase _target;

		public static bool Prepare()
		{
			Type type2 = AccessTools.TypeByName("VFECore.Patch_ThingDef");
			if (type2 == null)
			{
				return false;
			}
			type2 = AccessTools.FindIncludingInnerTypes<Type>(type2, (Func<Type, Type>)((Type type) => (type.Name == "SetFaction") ? type : null));
			return (_target = AccessTools.Method(type2, "Postfix", (Type[])null, (Type[])null)) != null;
		}

		public static MethodBase TargetMethod()
		{
			return _target;
		}

		public static bool Prefix()
		{
			return false;
		}
	}
}
