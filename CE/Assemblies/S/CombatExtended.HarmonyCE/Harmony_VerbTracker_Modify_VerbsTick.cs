using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace CombatExtended.HarmonyCE;

[HarmonyPatch(typeof(VerbTracker), "VerbsTick")]
internal static class Harmony_VerbTracker_Modify_VerbsTick
{
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
	{
		int patchPhase = 0;
		LocalBuilder verb = il.DeclareLocal(typeof(VerbTracker));
		LocalBuilder verbCE = il.DeclareLocal(typeof(Verb_LaunchProjectileCE));
		Label failBranch = il.DefineLabel();
		foreach (CodeInstruction instruction in instructions)
		{
			if (patchPhase == 1 && instruction.opcode == OpCodes.Ldloc_0)
			{
				yield return new CodeInstruction(OpCodes.Ldloc, (object)verb);
				yield return new CodeInstruction(OpCodes.Isinst, (object)typeof(Verb_LaunchProjectileCE));
				yield return new CodeInstruction(OpCodes.Stloc, (object)verbCE);
				yield return new CodeInstruction(OpCodes.Ldloc, (object)verbCE);
				yield return new CodeInstruction(OpCodes.Brfalse, (object)failBranch);
				yield return new CodeInstruction(OpCodes.Ldloc, (object)verbCE);
				yield return new CodeInstruction(OpCodes.Callvirt, (object)typeof(Verb_LaunchProjectileCE).GetMethod("VerbTickCE", AccessTools.all));
				instruction.labels.Add(failBranch);
				patchPhase = 2;
			}
			if (patchPhase == 0 && instruction.opcode == OpCodes.Callvirt && instruction.operand as MethodInfo != null && (instruction.operand as MethodInfo).Name.Equals("VerbTick"))
			{
				yield return new CodeInstruction(OpCodes.Stloc, (object)verb);
				yield return new CodeInstruction(OpCodes.Ldloc, (object)verb);
				patchPhase = 1;
			}
			yield return instruction;
		}
	}
}
