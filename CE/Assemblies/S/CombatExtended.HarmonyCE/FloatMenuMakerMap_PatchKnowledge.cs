using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch]
internal static class FloatMenuMakerMap_PatchKnowledge
{
	private static MethodBase knowledgeDemonstrated = AccessTools.Method(typeof(PlayerKnowledgeDatabase), "KnowledgeDemonstrated", (Type[])null, (Type[])null);

	private static FieldInfo equippingWeapons = AccessTools.Field(typeof(ConceptDefOf), "EquippingWeapons");

	private static MethodBase TargetMethod()
	{
		Type[] nestedTypes = typeof(FloatMenuMakerMap).GetNestedTypes(AccessTools.all);
		foreach (Type type in nestedTypes)
		{
			if (AccessTools.Field(type, "equipment")?.FieldType == typeof(ThingWithComps))
			{
				return type.GetMethods(AccessTools.all).FirstOrDefault((MethodInfo m) => m.Name.Contains("Equip"));
			}
		}
		return null;
	}

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> instructionsList = instructions.ToList();
		MethodInfo knowledgeDemonstrated = AccessTools.Method(typeof(PlayerKnowledgeDatabase), "KnowledgeDemonstrated", (Type[])null, (Type[])null);
		for (int i = 0; i < instructionsList.Count; i++)
		{
			yield return instructionsList[i];
			if (CodeInstructionExtensions.Calls(instructionsList[i], knowledgeDemonstrated) && instructionsList[i - 1].opcode == OpCodes.Ldc_I4_6 && CodeInstructionExtensions.LoadsField(instructionsList[i - 2], equippingWeapons, false))
			{
				FieldInfo aimingSystem = AccessTools.Field(typeof(CE_ConceptDefOf), "CE_AimingSystem");
				MethodInfo teachOpportunity = AccessTools.Method(typeof(LessonAutoActivator), "TeachOpportunity", new Type[2]
				{
					typeof(ConceptDef),
					typeof(OpportunityType)
				}, (Type[])null);
				yield return new CodeInstruction(OpCodes.Ldsfld, (object)aimingSystem);
				yield return new CodeInstruction(OpCodes.Ldc_I4, (object)0);
				yield return new CodeInstruction(OpCodes.Call, (object)teachOpportunity);
			}
		}
	}
}
