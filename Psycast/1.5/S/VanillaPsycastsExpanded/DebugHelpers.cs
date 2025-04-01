using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VanillaPsycastsExpanded;

public static class DebugHelpers
{
	public unsafe static IEnumerable<CodeInstruction> AddLogs(this IEnumerable<CodeInstruction> instructions, MethodBase original, List<MethodInfo> methods)
	{
		string name = default(string);
		Type result = default(Type);
		foreach (CodeInstruction instruction in instructions)
		{
			yield return instruction;
			MethodInfo methodInfo = methods.FirstOrDefault(new Predicate<MethodInfo>(instruction, (nint)(delegate*<CodeInstruction, MethodInfo, bool>)(&CodeInstructionExtensions.Calls)));
			int num;
			if ((object)methodInfo != null)
			{
				name = methodInfo.Name;
				result = methodInfo.ReturnType;
				num = 1;
			}
			else
			{
				num = 0;
			}
			if (num != 0)
			{
				yield return new CodeInstruction(OpCodes.Dup, (object)null);
				yield return new CodeInstruction(OpCodes.Ldstr, (object)name);
				yield return new CodeInstruction(OpCodes.Ldstr, (object)((original.DeclaringType?.Name ?? "Free") + "." + original.Name));
				yield return CodeInstruction.Call(typeof(DebugHelpers), "DoLog", (Type[])null, new Type[1] { result });
			}
			name = null;
			result = null;
		}
	}

	public static void DoLog<T>(T obj, string header, string context)
	{
		Log.Message($"[{context}] {header}: {obj}");
	}
}
