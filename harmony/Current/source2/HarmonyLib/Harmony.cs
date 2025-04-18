using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace HarmonyLib;

public class Harmony
{
	public static bool DEBUG;

	public string Id { get; private set; }

	public Harmony(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentException("id cannot be null or empty");
		}
		try
		{
			string environmentVariable = Environment.GetEnvironmentVariable("HARMONY_DEBUG");
			if (environmentVariable != null && environmentVariable.Length > 0)
			{
				environmentVariable = environmentVariable.Trim();
				DEBUG = environmentVariable == "1" || bool.Parse(environmentVariable);
			}
		}
		catch
		{
		}
		if (DEBUG)
		{
			Assembly assembly = typeof(Harmony).Assembly;
			Version version = assembly.GetName().Version;
			string value = assembly.Location;
			string value2 = Environment.Version.ToString();
			string value3 = Environment.OSVersion.Platform.ToString();
			if (string.IsNullOrEmpty(value))
			{
				value = new Uri(assembly.CodeBase).LocalPath;
			}
			FileLog.Log($"### Harmony id={id}, version={version}, location={value}, env/clr={value2}, platform={value3}");
			MethodBase outsideCaller = AccessTools.GetOutsideCaller();
			if ((object)outsideCaller.DeclaringType != null)
			{
				Assembly assembly2 = outsideCaller.DeclaringType.Assembly;
				value = assembly2.Location;
				if (string.IsNullOrEmpty(value))
				{
					value = new Uri(assembly2.CodeBase).LocalPath;
				}
				FileLog.Log("### Started from " + outsideCaller.FullDescription() + ", location " + value);
				FileLog.Log($"### At {DateTime.Now:yyyy-MM-dd hh.mm.ss}");
			}
		}
		Id = id;
	}

	public void PatchAll()
	{
		MethodBase method = new StackTrace().GetFrame(1).GetMethod();
		Assembly assembly = method.ReflectedType.Assembly;
		PatchAll(assembly);
	}

	public PatchProcessor CreateProcessor(MethodBase original)
	{
		return new PatchProcessor(this, original);
	}

	public PatchClassProcessor CreateClassProcessor(Type type)
	{
		return new PatchClassProcessor(this, type);
	}

	public ReversePatcher CreateReversePatcher(MethodBase original, HarmonyMethod standin)
	{
		return new ReversePatcher(this, original, standin);
	}

	public void PatchAll(Assembly assembly)
	{
		AccessTools.GetTypesFromAssembly(assembly).Do(delegate(Type type)
		{
			CreateClassProcessor(type).Patch();
		});
	}

	public void PatchAllUncategorized()
	{
		MethodBase method = new StackTrace().GetFrame(1).GetMethod();
		Assembly assembly = method.ReflectedType.Assembly;
		PatchAllUncategorized(assembly);
	}

	public void PatchAllUncategorized(Assembly assembly)
	{
		PatchClassProcessor[] sequence = AccessTools.GetTypesFromAssembly(assembly).Select(CreateClassProcessor).ToArray();
		sequence.DoIf((PatchClassProcessor patchClass) => string.IsNullOrEmpty(patchClass.Category), delegate(PatchClassProcessor patchClass)
		{
			patchClass.Patch();
		});
	}

	public void PatchCategory(string category)
	{
		MethodBase method = new StackTrace().GetFrame(1).GetMethod();
		Assembly assembly = method.ReflectedType.Assembly;
		PatchCategory(assembly, category);
	}

	public void PatchCategory(Assembly assembly, string category)
	{
		AccessTools.GetTypesFromAssembly(assembly).Where(delegate(Type type)
		{
			List<HarmonyMethod> fromType = HarmonyMethodExtensions.GetFromType(type);
			HarmonyMethod harmonyMethod = HarmonyMethod.Merge(fromType);
			return harmonyMethod.category == category;
		}).Do(delegate(Type type)
		{
			CreateClassProcessor(type).Patch();
		});
	}

	public MethodInfo Patch(MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null, HarmonyMethod finalizer = null)
	{
		PatchProcessor patchProcessor = CreateProcessor(original);
		patchProcessor.AddPrefix(prefix);
		patchProcessor.AddPostfix(postfix);
		patchProcessor.AddTranspiler(transpiler);
		patchProcessor.AddFinalizer(finalizer);
		return patchProcessor.Patch();
	}

	public static MethodInfo ReversePatch(MethodBase original, HarmonyMethod standin, MethodInfo transpiler = null)
	{
		return PatchFunctions.ReversePatch(standin, original, transpiler);
	}

	public void UnpatchAll(string harmonyID = null)
	{
		List<MethodBase> list = GetAllPatchedMethods().ToList();
		foreach (MethodBase original in list)
		{
			bool flag = original.HasMethodBody();
			Patches patchInfo2 = GetPatchInfo(original);
			if (flag)
			{
				patchInfo2.Postfixes.DoIf(IDCheck, delegate(Patch patchInfo)
				{
					Unpatch(original, patchInfo.PatchMethod);
				});
				patchInfo2.Prefixes.DoIf(IDCheck, delegate(Patch patchInfo)
				{
					Unpatch(original, patchInfo.PatchMethod);
				});
			}
			patchInfo2.Transpilers.DoIf(IDCheck, delegate(Patch patchInfo)
			{
				Unpatch(original, patchInfo.PatchMethod);
			});
			if (flag)
			{
				patchInfo2.Finalizers.DoIf(IDCheck, delegate(Patch patchInfo)
				{
					Unpatch(original, patchInfo.PatchMethod);
				});
			}
		}
		bool IDCheck(Patch patchInfo)
		{
			if (harmonyID != null)
			{
				return patchInfo.owner == harmonyID;
			}
			return true;
		}
	}

	public void Unpatch(MethodBase original, HarmonyPatchType type, string harmonyID = "*")
	{
		PatchProcessor patchProcessor = CreateProcessor(original);
		patchProcessor.Unpatch(type, harmonyID);
	}

	public void Unpatch(MethodBase original, MethodInfo patch)
	{
		PatchProcessor patchProcessor = CreateProcessor(original);
		patchProcessor.Unpatch(patch);
	}

	public void UnpatchCategory(string category)
	{
		MethodBase method = new StackTrace().GetFrame(1).GetMethod();
		Assembly assembly = method.ReflectedType.Assembly;
		UnpatchCategory(assembly, category);
	}

	public void UnpatchCategory(Assembly assembly, string category)
	{
		AccessTools.GetTypesFromAssembly(assembly).Where(delegate(Type type)
		{
			List<HarmonyMethod> fromType = HarmonyMethodExtensions.GetFromType(type);
			HarmonyMethod harmonyMethod = HarmonyMethod.Merge(fromType);
			return harmonyMethod.category == category;
		}).Do(delegate(Type type)
		{
			CreateClassProcessor(type).Unpatch();
		});
	}

	public static bool HasAnyPatches(string harmonyID)
	{
		return GetAllPatchedMethods().Select(GetPatchInfo).Any((Patches info) => info.Owners.Contains(harmonyID));
	}

	public static Patches GetPatchInfo(MethodBase method)
	{
		return PatchProcessor.GetPatchInfo(method);
	}

	public IEnumerable<MethodBase> GetPatchedMethods()
	{
		return from original in GetAllPatchedMethods()
			where GetPatchInfo(original).Owners.Contains(Id)
			select original;
	}

	public static IEnumerable<MethodBase> GetAllPatchedMethods()
	{
		return PatchProcessor.GetAllPatchedMethods();
	}

	public static MethodBase GetOriginalMethod(MethodInfo replacement)
	{
		if (replacement == null)
		{
			throw new ArgumentNullException("replacement");
		}
		return HarmonySharedState.GetRealMethod(replacement, useReplacement: false);
	}

	public static MethodBase GetMethodFromStackframe(StackFrame frame)
	{
		if (frame == null)
		{
			throw new ArgumentNullException("frame");
		}
		return HarmonySharedState.GetStackFrameMethod(frame, useReplacement: true);
	}

	public static MethodBase GetOriginalMethodFromStackframe(StackFrame frame)
	{
		if (frame == null)
		{
			throw new ArgumentNullException("frame");
		}
		return HarmonySharedState.GetStackFrameMethod(frame, useReplacement: false);
	}

	public static Dictionary<string, Version> VersionInfo(out Version currentVersion)
	{
		return PatchProcessor.VersionInfo(out currentVersion);
	}
}
