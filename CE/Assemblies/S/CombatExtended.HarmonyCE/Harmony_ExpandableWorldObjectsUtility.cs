using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace CombatExtended.HarmonyCE;

public static class Harmony_ExpandableWorldObjectsUtility
{
	[HarmonyPatch(typeof(ExpandableWorldObjectsUtility), "ExpandableWorldObjectsOnGUI")]
	public static class Harmony_ExpandableWorldObjectsOnGUI
	{
		private static List<WorldObject> tmpWorldObjects = new List<WorldObject>();

		private static bool skip = true;

		private static bool showExpandingIcons;

		private static float transitionPct;

		private static MethodBase allWorldObjectGetter = AccessTools.PropertyGetter(typeof(WorldObjectsHolder), "AllWorldObjects");

		private static MethodBase getAllWorldObjectCE = AccessTools.Method(typeof(Harmony_ExpandableWorldObjectsOnGUI), "GetAllWorldObjectCE", (Type[])null, (Type[])null);

		public static List<WorldObject> GetAllWorldObjectCE(List<WorldObject> worldObjects)
		{
			if (!skip)
			{
				return worldObjects.Where((WorldObject o) => o is TravelingShell).ToList();
			}
			return worldObjects;
		}

		[HarmonyPrefix]
		public static void Prefix()
		{
			skip = true;
			transitionPct = ExpandableWorldObjectsUtility.transitionPct;
			showExpandingIcons = Find.PlaySettings.showExpandingIcons;
			if (ExpandableWorldObjectsUtility.TransitionPct == 0f)
			{
				skip = false;
				ExpandableWorldObjectsUtility.transitionPct = 1f;
				Find.PlaySettings.showExpandingIcons = true;
			}
		}

		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> codes = instructions.ToList();
			bool finished = false;
			for (int i = 0; i < codes.Count; i++)
			{
				if (!finished && codes[i].opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(codes[i], (MemberInfo)allWorldObjectGetter))
				{
					Log.Message("patched");
					finished = true;
					yield return codes[i];
					yield return new CodeInstruction(OpCodes.Call, (object)getAllWorldObjectCE);
				}
				else
				{
					yield return codes[i];
				}
			}
		}

		[HarmonyPostfix]
		public static void Postfix()
		{
			ExpandableWorldObjectsUtility.transitionPct = transitionPct;
			Find.PlaySettings.showExpandingIcons = showExpandingIcons;
		}
	}
}
