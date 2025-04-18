using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HarmonyLib;

public class ReversePatcher(Harmony instance, MethodBase original, HarmonyMethod standin)
{
	private readonly Harmony instance = instance;

	private readonly MethodBase original = original;

	private readonly HarmonyMethod standin = standin;

	public MethodInfo Patch(HarmonyReversePatchType type = HarmonyReversePatchType.Original)
	{
		if ((object)original == null)
		{
			throw new NullReferenceException("Null method for " + instance.Id);
		}
		standin.reversePatchType = type;
		MethodInfo transpiler = GetTranspiler(standin.method);
		return PatchFunctions.ReversePatch(standin, original, transpiler);
	}

	internal static MethodInfo GetTranspiler(MethodInfo method)
	{
		string methodName = method.Name;
		Type declaringType = method.DeclaringType;
		List<MethodInfo> declaredMethods = AccessTools.GetDeclaredMethods(declaringType);
		Type ici = typeof(IEnumerable<CodeInstruction>);
		return declaredMethods.FirstOrDefault((MethodInfo m) => !(m.ReturnType != ici) && m.Name.StartsWith("<" + methodName + ">"));
	}
}
