using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace HarmonyLib;

public static class Transpilers
{
	public static IEnumerable<CodeInstruction> MethodReplacer(this IEnumerable<CodeInstruction> instructions, MethodBase from, MethodBase to)
	{
		if ((object)from == null)
		{
			throw new ArgumentException("Unexpected null argument", "from");
		}
		if ((object)to == null)
		{
			throw new ArgumentException("Unexpected null argument", "to");
		}
		foreach (CodeInstruction instruction in instructions)
		{
			MethodBase methodBase = instruction.operand as MethodBase;
			if (methodBase == from)
			{
				instruction.opcode = (to.IsConstructor ? OpCodes.Newobj : OpCodes.Call);
				instruction.operand = to;
			}
			yield return instruction;
		}
	}

	public static IEnumerable<CodeInstruction> Manipulator(this IEnumerable<CodeInstruction> instructions, Func<CodeInstruction, bool> predicate, Action<CodeInstruction> action)
	{
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return instructions.Select(delegate(CodeInstruction instruction)
		{
			if (predicate(instruction))
			{
				action(instruction);
			}
			return instruction;
		}).AsEnumerable();
	}

	public static IEnumerable<CodeInstruction> DebugLogger(this IEnumerable<CodeInstruction> instructions, string text)
	{
		yield return new CodeInstruction(OpCodes.Ldstr, text);
		yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FileLog), "Debug"));
		foreach (CodeInstruction instruction in instructions)
		{
			yield return instruction;
		}
	}
}
