using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;

namespace HarmonyLib;

internal class MethodCopier
{
	private readonly MethodBodyReader reader;

	private readonly List<MethodInfo> transpilers = new List<MethodInfo>();

	internal MethodCopier(MethodBase fromMethod, ILGenerator toILGenerator, LocalBuilder[] existingVariables = null)
	{
		if ((object)fromMethod == null)
		{
			throw new ArgumentNullException("fromMethod");
		}
		reader = new MethodBodyReader(fromMethod, toILGenerator);
		reader.DeclareVariables(existingVariables);
		reader.GenerateInstructions();
	}

	internal void SetDebugging(bool debug)
	{
		reader.SetDebugging(debug);
	}

	internal void AddTranspiler(MethodInfo transpiler)
	{
		transpilers.Add(transpiler);
	}

	internal List<CodeInstruction> Finalize(Emitter emitter, List<Label> endLabels, out bool hasReturnCode, out bool methodEndsInDeadCode)
	{
		return reader.FinalizeILCodes(emitter, transpilers, endLabels, out hasReturnCode, out methodEndsInDeadCode);
	}

	internal static List<CodeInstruction> GetInstructions(ILGenerator generator, MethodBase method, int maxTranspilers)
	{
		if (generator == null)
		{
			throw new ArgumentNullException("generator");
		}
		if ((object)method == null)
		{
			throw new ArgumentNullException("method");
		}
		LocalBuilder[] existingVariables = MethodPatcher.DeclareOriginalLocalVariables(generator, method);
		MethodCopier methodCopier = new MethodCopier(method, generator, existingVariables);
		Patches patchInfo = Harmony.GetPatchInfo(method);
		if (patchInfo != null)
		{
			ReadOnlyCollection<Patch> readOnlyCollection = patchInfo.Transpilers;
			int num = 0;
			Patch[] array = new Patch[readOnlyCollection.Count];
			foreach (Patch item in readOnlyCollection)
			{
				array[num] = item;
				num++;
			}
			List<MethodInfo> sortedPatchMethods = PatchFunctions.GetSortedPatchMethods(method, array, debug: false);
			for (int i = 0; i < maxTranspilers && i < sortedPatchMethods.Count; i++)
			{
				methodCopier.AddTranspiler(sortedPatchMethods[i]);
			}
		}
		bool hasReturnCode;
		bool methodEndsInDeadCode;
		return methodCopier.Finalize(null, null, out hasReturnCode, out methodEndsInDeadCode);
	}
}
