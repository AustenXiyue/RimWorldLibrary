using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch(typeof(RitualOutcomeEffectWorker_Bestowing), "Apply")]
public class RitualOutcomeEffectWorker_Bestowing_Apply_Patch
{
	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		List<CodeInstruction> list = instructions.ToList();
		MethodInfo info1 = AccessTools.Method(typeof(PawnUtility), "GetPsylinkLevel", (Type[])null, (Type[])null);
		MethodInfo info2 = AccessTools.Method(typeof(PawnUtility), "GetMaxPsylinkLevelByTitle", (Type[])null, (Type[])null);
		int num = list.FindIndex((CodeInstruction ins) => CodeInstructionExtensions.Calls(ins, info1)) - 1;
		int num2 = list.FindIndex((CodeInstruction ins) => CodeInstructionExtensions.Calls(ins, info2)) + 1;
		list.RemoveRange(num, num2 - num + 1);
		list.InsertRange(num, (IEnumerable<CodeInstruction>)(object)new CodeInstruction[4]
		{
			new CodeInstruction(OpCodes.Ldloc_2, (object)null),
			new CodeInstruction(OpCodes.Ldloc, (object)9),
			new CodeInstruction(OpCodes.Ldloc, (object)10),
			CodeInstruction.Call(typeof(RitualOutcomeEffectWorker_Bestowing_Apply_Patch), "ApplyTitlePsylink", (Type[])null, (Type[])null)
		});
		return list;
	}

	public static void ApplyTitlePsylink(Pawn pawn, RoyalTitleDef oldTitle, RoyalTitleDef newTitle)
	{
		Hediff_PsycastAbilities hediff_PsycastAbilities = pawn.Psycasts();
		int maxPsylinkLevel = newTitle.maxPsylinkLevel;
		int num = oldTitle?.maxPsylinkLevel ?? 0;
		if (hediff_PsycastAbilities == null)
		{
			pawn.ChangePsylinkLevel(1, sendLetter: false);
			hediff_PsycastAbilities = pawn.Psycasts();
			hediff_PsycastAbilities.ChangeLevel(maxPsylinkLevel - num, sendLetter: false);
			hediff_PsycastAbilities.maxLevelFromTitles = maxPsylinkLevel;
		}
		else if (hediff_PsycastAbilities.maxLevelFromTitles <= maxPsylinkLevel)
		{
			hediff_PsycastAbilities.ChangeLevel(maxPsylinkLevel - num, sendLetter: false);
			hediff_PsycastAbilities.maxLevelFromTitles = maxPsylinkLevel;
		}
	}
}
