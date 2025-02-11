using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(LordToil_Siege), "LordToilTick")]
internal static class Harmony_LordToil_Siege_LordToilTick
{
	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Expected O, but got Unknown
		MethodInfo methodInfo = AccessTools.Method(typeof(SiegeUtility), "IsValidShellType", (Type[])null, (Type[])null);
		MethodInfo methodInfo2 = AccessTools.Method(typeof(SiegeUtility), "DropAdditionalShells", (Type[])null, (Type[])null);
		MethodInfo methodInfo3 = AccessTools.PropertyGetter(typeof(ThingDef), "IsShell");
		FieldInfo fieldInfo = AccessTools.Field(typeof(DamageDef), "harmsHealth");
		MethodInfo methodInfo4 = AccessTools.Method(typeof(TurretGunUtility), "TryFindRandomShellDef", (Type[])null, (Type[])null);
		CodeMatcher val = new CodeMatcher(instructions, generator);
		int pos = val.Start().MatchStartForward((CodeMatch[])(object)new CodeMatch[1] { CodeMatch.Calls(methodInfo3) }).ThrowIfInvalid("CombatExtended :: Harmony_LordToil_Siege_LordToilTick couldn't find call to IsShell")
			.Pos;
		int pos2 = val.MatchStartForward((CodeMatch[])(object)new CodeMatch[1] { CodeMatch.LoadsField(fieldInfo, false) }).ThrowIfInvalid("CombatExtended :: Harmony_LordToil_Siege_LordToilTick couldn't find harmsHealth").Pos;
		val.Advance(pos - pos2 - 1).Insert((CodeInstruction[])(object)new CodeInstruction[2]
		{
			CodeInstruction.LoadArgument(0, false),
			new CodeInstruction(OpCodes.Call, (object)methodInfo)
		}).RemoveInstructionsInRange(pos + 1, pos2 + 2);
		int pos3 = val.Start().MatchStartForward((CodeMatch[])(object)new CodeMatch[1] { CodeMatch.Calls(methodInfo4) }).ThrowIfInvalid("CombatExtended :: Harmony_LordToil_Siege_LordToilTick couldn't find call to TryFindRandomShellDef")
			.MatchEndBackwards((CodeMatch[])(object)new CodeMatch[1] { CodeMatch.Branches((string)null) })
			.ThrowIfInvalid("CombatExtended :: Harmony_LordToil_Siege_LordToilTick couldn't find start of enclosing if block")
			.Pos;
		Label ifBlockEndLabel = (Label)val.Operand;
		val.MatchStartForward((CodeMatch[])(object)new CodeMatch[1]
		{
			new CodeMatch((Func<CodeInstruction, bool>)((CodeInstruction instruction) => instruction.labels.Contains(ifBlockEndLabel)), (string)null)
		}).ThrowIfInvalid("CombatExtended :: Harmony_LordToil_Siege_LordToilTick couldn't find end of enclosing if block").Insert((CodeInstruction[])(object)new CodeInstruction[2]
		{
			CodeInstruction.LoadArgument(0, false),
			new CodeInstruction(OpCodes.Call, (object)methodInfo2)
		})
			.RemoveInstructionsInRange(pos3 + 1, val.Pos - 1);
		return val.Instructions();
	}
}
