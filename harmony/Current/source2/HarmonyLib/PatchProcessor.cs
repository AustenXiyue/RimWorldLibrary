using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using MonoMod.Utils;

namespace HarmonyLib;

public class PatchProcessor(Harmony instance, MethodBase original)
{
	private readonly Harmony instance = instance;

	private readonly MethodBase original = original;

	private HarmonyMethod prefix;

	private HarmonyMethod postfix;

	private HarmonyMethod transpiler;

	private HarmonyMethod finalizer;

	internal static readonly object locker = new object();

	public PatchProcessor AddPrefix(HarmonyMethod prefix)
	{
		this.prefix = prefix;
		return this;
	}

	public PatchProcessor AddPrefix(MethodInfo fixMethod)
	{
		prefix = new HarmonyMethod(fixMethod);
		return this;
	}

	public PatchProcessor AddPostfix(HarmonyMethod postfix)
	{
		this.postfix = postfix;
		return this;
	}

	public PatchProcessor AddPostfix(MethodInfo fixMethod)
	{
		postfix = new HarmonyMethod(fixMethod);
		return this;
	}

	public PatchProcessor AddTranspiler(HarmonyMethod transpiler)
	{
		this.transpiler = transpiler;
		return this;
	}

	public PatchProcessor AddTranspiler(MethodInfo fixMethod)
	{
		transpiler = new HarmonyMethod(fixMethod);
		return this;
	}

	public PatchProcessor AddFinalizer(HarmonyMethod finalizer)
	{
		this.finalizer = finalizer;
		return this;
	}

	public PatchProcessor AddFinalizer(MethodInfo fixMethod)
	{
		finalizer = new HarmonyMethod(fixMethod);
		return this;
	}

	public static IEnumerable<MethodBase> GetAllPatchedMethods()
	{
		lock (locker)
		{
			return HarmonySharedState.GetPatchedMethods();
		}
	}

	public MethodInfo Patch()
	{
		if ((object)original == null)
		{
			throw new NullReferenceException("Null method for " + instance.Id);
		}
		if (!original.IsDeclaredMember())
		{
			MethodBase declaredMember = original.GetDeclaredMember();
			throw new ArgumentException("You can only patch implemented methods/constructors. Patch the declared method " + declaredMember.FullDescription() + " instead.");
		}
		lock (locker)
		{
			PatchInfo patchInfo = HarmonySharedState.GetPatchInfo(original) ?? new PatchInfo();
			patchInfo.AddPrefixes(instance.Id, prefix);
			patchInfo.AddPostfixes(instance.Id, postfix);
			patchInfo.AddTranspilers(instance.Id, transpiler);
			patchInfo.AddFinalizers(instance.Id, finalizer);
			MethodInfo methodInfo = PatchFunctions.UpdateWrapper(original, patchInfo);
			HarmonySharedState.UpdatePatchInfo(original, methodInfo, patchInfo);
			return methodInfo;
		}
	}

	public PatchProcessor Unpatch(HarmonyPatchType type, string harmonyID)
	{
		lock (locker)
		{
			PatchInfo patchInfo = HarmonySharedState.GetPatchInfo(original);
			if (patchInfo == null)
			{
				patchInfo = new PatchInfo();
			}
			if (type == HarmonyPatchType.All || type == HarmonyPatchType.Prefix)
			{
				patchInfo.RemovePrefix(harmonyID);
			}
			if (type == HarmonyPatchType.All || type == HarmonyPatchType.Postfix)
			{
				patchInfo.RemovePostfix(harmonyID);
			}
			if (type == HarmonyPatchType.All || type == HarmonyPatchType.Transpiler)
			{
				patchInfo.RemoveTranspiler(harmonyID);
			}
			if (type == HarmonyPatchType.All || type == HarmonyPatchType.Finalizer)
			{
				patchInfo.RemoveFinalizer(harmonyID);
			}
			MethodInfo replacement = PatchFunctions.UpdateWrapper(original, patchInfo);
			HarmonySharedState.UpdatePatchInfo(original, replacement, patchInfo);
			return this;
		}
	}

	public PatchProcessor Unpatch(MethodInfo patch)
	{
		lock (locker)
		{
			PatchInfo patchInfo = HarmonySharedState.GetPatchInfo(original);
			if (patchInfo == null)
			{
				patchInfo = new PatchInfo();
			}
			patchInfo.RemovePatch(patch);
			MethodInfo replacement = PatchFunctions.UpdateWrapper(original, patchInfo);
			HarmonySharedState.UpdatePatchInfo(original, replacement, patchInfo);
			return this;
		}
	}

	public static Patches GetPatchInfo(MethodBase method)
	{
		PatchInfo patchInfo;
		lock (locker)
		{
			patchInfo = HarmonySharedState.GetPatchInfo(method);
		}
		if (patchInfo == null)
		{
			return null;
		}
		return new Patches(patchInfo.prefixes, patchInfo.postfixes, patchInfo.transpilers, patchInfo.finalizers);
	}

	public static List<MethodInfo> GetSortedPatchMethods(MethodBase original, Patch[] patches)
	{
		return PatchFunctions.GetSortedPatchMethods(original, patches, debug: false);
	}

	public static Dictionary<string, Version> VersionInfo(out Version currentVersion)
	{
		currentVersion = typeof(Harmony).Assembly.GetName().Version;
		Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
		GetAllPatchedMethods().Do(delegate(MethodBase method)
		{
			PatchInfo patchInfo;
			lock (locker)
			{
				patchInfo = HarmonySharedState.GetPatchInfo(method);
			}
			patchInfo.prefixes.Do(delegate(Patch fix)
			{
				assemblies[fix.owner] = fix.PatchMethod.DeclaringType.Assembly;
			});
			patchInfo.postfixes.Do(delegate(Patch fix)
			{
				assemblies[fix.owner] = fix.PatchMethod.DeclaringType.Assembly;
			});
			patchInfo.transpilers.Do(delegate(Patch fix)
			{
				assemblies[fix.owner] = fix.PatchMethod.DeclaringType.Assembly;
			});
			patchInfo.finalizers.Do(delegate(Patch fix)
			{
				assemblies[fix.owner] = fix.PatchMethod.DeclaringType.Assembly;
			});
		});
		Dictionary<string, Version> result = new Dictionary<string, Version>();
		assemblies.Do(delegate(KeyValuePair<string, Assembly> info)
		{
			AssemblyName assemblyName = info.Value.GetReferencedAssemblies().FirstOrDefault((AssemblyName a) => a.FullName.StartsWith("0Harmony, Version", StringComparison.Ordinal));
			if (assemblyName != null)
			{
				result[info.Key] = assemblyName.Version;
			}
		});
		return result;
	}

	public static ILGenerator CreateILGenerator()
	{
		DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition($"ILGenerator_{Guid.NewGuid()}", typeof(void), Array.Empty<Type>());
		return dynamicMethodDefinition.GetILGenerator();
	}

	public static ILGenerator CreateILGenerator(MethodBase original)
	{
		Type returnType = ((original is MethodInfo methodInfo) ? methodInfo.ReturnType : typeof(void));
		List<Type> list = (from pi in original.GetParameters()
			select pi.ParameterType).ToList();
		if (!original.IsStatic)
		{
			list.Insert(0, original.DeclaringType);
		}
		DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("ILGenerator_" + original.Name, returnType, list.ToArray());
		return dynamicMethodDefinition.GetILGenerator();
	}

	public static List<CodeInstruction> GetOriginalInstructions(MethodBase original, ILGenerator generator = null)
	{
		return MethodCopier.GetInstructions(generator ?? CreateILGenerator(original), original, 0);
	}

	public static List<CodeInstruction> GetOriginalInstructions(MethodBase original, out ILGenerator generator)
	{
		generator = CreateILGenerator(original);
		return MethodCopier.GetInstructions(generator, original, 0);
	}

	public static List<CodeInstruction> GetCurrentInstructions(MethodBase original, int maxTranspilers = int.MaxValue, ILGenerator generator = null)
	{
		return MethodCopier.GetInstructions(generator ?? CreateILGenerator(original), original, maxTranspilers);
	}

	public static List<CodeInstruction> GetCurrentInstructions(MethodBase original, out ILGenerator generator, int maxTranspilers = int.MaxValue)
	{
		generator = CreateILGenerator(original);
		return MethodCopier.GetInstructions(generator, original, maxTranspilers);
	}

	public static IEnumerable<KeyValuePair<OpCode, object>> ReadMethodBody(MethodBase method)
	{
		return from instr in MethodBodyReader.GetInstructions(CreateILGenerator(method), method)
			select new KeyValuePair<OpCode, object>(instr.opcode, instr.operand);
	}

	public static IEnumerable<KeyValuePair<OpCode, object>> ReadMethodBody(MethodBase method, ILGenerator generator)
	{
		return from instr in MethodBodyReader.GetInstructions(generator, method)
			select new KeyValuePair<OpCode, object>(instr.opcode, instr.operand);
	}
}
