using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(JobGiver_AIBreaching), "UpdateBreachingTarget")]
internal static class Harmony_JobGiver_AIBreaching
{
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		List<CodeInstruction> instructionsList = instructions.ToList();
		MethodInfo mIsSoloAttackVerb = AccessTools.Method(typeof(BreachingUtility), "IsSoloAttackVerb", (Type[])null, (Type[])null);
		MethodInfo pRaceProps = AccessTools.DeclaredPropertyGetter(typeof(Pawn), "RaceProps");
		MethodInfo pIsMechanoid = AccessTools.DeclaredPropertyGetter(typeof(RaceProperties), "IsMechanoid");
		Label checkNoDesignatedSoloAttackerExistsLabel = generator.DefineLabel();
		Label? checkPawnIsSoloAttackerLabel = default(Label?);
		for (int i = 0; i < instructionsList.Count; i++)
		{
			if (CodeInstructionExtensions.Branches(instructionsList[i], ref checkPawnIsSoloAttackerLabel) && CodeInstructionExtensions.Calls(instructionsList[i - 1], mIsSoloAttackVerb))
			{
				instructionsList[i + 1].labels.Add(checkNoDesignatedSoloAttackerExistsLabel);
				yield return new CodeInstruction(OpCodes.Brfalse_S, (object)checkNoDesignatedSoloAttackerExistsLabel);
				yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
				yield return new CodeInstruction(OpCodes.Callvirt, (object)pRaceProps);
				yield return new CodeInstruction(OpCodes.Callvirt, (object)pIsMechanoid);
				yield return new CodeInstruction(OpCodes.Brfalse_S, (object)checkPawnIsSoloAttackerLabel.Value);
			}
			else
			{
				yield return instructionsList[i];
			}
			checkPawnIsSoloAttackerLabel = null;
		}
	}
}
