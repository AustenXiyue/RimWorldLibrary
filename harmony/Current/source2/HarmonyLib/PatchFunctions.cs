using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace HarmonyLib;

internal static class PatchFunctions
{
	internal static List<MethodInfo> GetSortedPatchMethods(MethodBase original, Patch[] patches, bool debug)
	{
		return new PatchSorter(patches, debug).Sort(original);
	}

	internal static MethodInfo UpdateWrapper(MethodBase original, PatchInfo patchInfo)
	{
		bool debug = patchInfo.Debugging || Harmony.DEBUG;
		List<MethodInfo> sortedPatchMethods = GetSortedPatchMethods(original, patchInfo.prefixes, debug);
		List<MethodInfo> sortedPatchMethods2 = GetSortedPatchMethods(original, patchInfo.postfixes, debug);
		List<MethodInfo> sortedPatchMethods3 = GetSortedPatchMethods(original, patchInfo.transpilers, debug);
		List<MethodInfo> sortedPatchMethods4 = GetSortedPatchMethods(original, patchInfo.finalizers, debug);
		MethodPatcher methodPatcher = new MethodPatcher(original, null, sortedPatchMethods, sortedPatchMethods2, sortedPatchMethods3, sortedPatchMethods4, debug);
		Dictionary<int, CodeInstruction> finalInstructions;
		MethodInfo methodInfo = methodPatcher.CreateReplacement(out finalInstructions);
		if ((object)methodInfo == null)
		{
			throw new MissingMethodException("Cannot create replacement for " + original.FullDescription());
		}
		try
		{
			PatchTools.DetourMethod(original, methodInfo);
			return methodInfo;
		}
		catch (Exception ex)
		{
			throw HarmonyException.Create(ex, finalInstructions);
		}
	}

	internal static MethodInfo ReversePatch(HarmonyMethod standin, MethodBase original, MethodInfo postTranspiler)
	{
		if (standin == null)
		{
			throw new ArgumentNullException("standin");
		}
		if ((object)standin.method == null)
		{
			throw new ArgumentNullException("standin", "standin.method is NULL");
		}
		bool debug = standin.debug == true || Harmony.DEBUG;
		List<MethodInfo> list = new List<MethodInfo>();
		if (standin.reversePatchType == HarmonyReversePatchType.Snapshot)
		{
			Patches patchInfo = Harmony.GetPatchInfo(original);
			List<MethodInfo> list2 = list;
			ReadOnlyCollection<Patch> transpilers = patchInfo.Transpilers;
			int num = 0;
			Patch[] array = new Patch[transpilers.Count];
			foreach (Patch item in transpilers)
			{
				array[num] = item;
				num++;
			}
			list2.AddRange(GetSortedPatchMethods(original, array, debug));
		}
		if ((object)postTranspiler != null)
		{
			list.Add(postTranspiler);
		}
		List<MethodInfo> list3 = new List<MethodInfo>();
		MethodPatcher methodPatcher = new MethodPatcher(standin.method, original, list3, list3, list, list3, debug);
		Dictionary<int, CodeInstruction> finalInstructions;
		MethodInfo methodInfo = methodPatcher.CreateReplacement(out finalInstructions);
		if ((object)methodInfo == null)
		{
			throw new MissingMethodException("Cannot create replacement for " + standin.method.FullDescription());
		}
		try
		{
			PatchTools.DetourMethod(standin.method, methodInfo);
			return methodInfo;
		}
		catch (Exception ex)
		{
			throw HarmonyException.Create(ex, finalInstructions);
		}
	}
}
