using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class ThingDef_DescriptionDetailed
{
	private static StringBuilder AddShieldCover(ThingDef thingDef, StringBuilder stringBuilder)
	{
		if (thingDef.GetModExtension<ShieldDefExtension>()?.shieldCoverage != null)
		{
			stringBuilder.Append(string.Format("{0}: {1}", "CE_Shield_Coverage".Translate(), ShieldDefExtension.GetShieldProtectedAreas(BodyDefOf.Human, thingDef)));
		}
		else
		{
			stringBuilder.Append(string.Format("{0}: {1}", "Covers".Translate(), thingDef.apparel.GetCoveredOuterPartsString(BodyDefOf.Human)));
		}
		return stringBuilder;
	}

	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		List<CodeInstruction> code = new List<CodeInstruction>(instructions);
		int startIndex = -1;
		int endIndex = -1;
		bool foundCovers = false;
		for (int i = 0; i < code.Count; i++)
		{
			if (code[i].opcode == OpCodes.Ldloc_0)
			{
				startIndex = i;
				for (int j = i + 1; j < code.Count; j++)
				{
					if (code[j].opcode == OpCodes.Ldstr && code[j].operand as string == "Covers")
					{
						foundCovers = true;
					}
					if (code[j].opcode == OpCodes.Pop)
					{
						if (foundCovers)
						{
							endIndex = j;
						}
						else
						{
							startIndex = -1;
						}
						break;
					}
				}
			}
			if (endIndex > -1)
			{
				break;
			}
		}
		if (startIndex > -1 && endIndex > -1)
		{
			code[startIndex].opcode = OpCodes.Nop;
			code.RemoveRange(startIndex + 1, endIndex - startIndex - 1);
			code.Insert(startIndex + 1, new CodeInstruction(OpCodes.Ldarg_0, (object)null));
			code.Insert(startIndex + 2, new CodeInstruction(OpCodes.Ldloc_0, (object)null));
			code.Insert(startIndex + 3, new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(ThingDef_DescriptionDetailed), "AddShieldCover", (Type[])null, (Type[])null)));
		}
		foreach (CodeInstruction item in code)
		{
			yield return item;
		}
	}
}
