using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using MonoMod.Core;
using MonoMod.Utils;

namespace HarmonyLib;

internal static class PatchTools
{
	private static readonly Dictionary<MethodBase, ICoreDetour> detours = new Dictionary<MethodBase, ICoreDetour>();

	internal static readonly string harmonyMethodFullName = typeof(HarmonyMethod).FullName;

	internal static readonly string harmonyAttributeFullName = typeof(HarmonyAttribute).FullName;

	internal static readonly string harmonyPatchAllFullName = typeof(HarmonyPatchAll).FullName;

	internal static readonly MethodInfo m_GetExecutingAssemblyReplacementTranspiler = SymbolExtensions.GetMethodInfo(() => GetExecutingAssemblyTranspiler(null));

	internal static readonly MethodInfo m_GetExecutingAssembly = SymbolExtensions.GetMethodInfo(() => Assembly.GetExecutingAssembly());

	internal static readonly MethodInfo m_GetExecutingAssemblyReplacement = SymbolExtensions.GetMethodInfo(() => GetExecutingAssemblyReplacement());

	internal static void DetourMethod(MethodBase method, MethodBase replacement)
	{
		lock (detours)
		{
			if (detours.TryGetValue(method, out var value))
			{
				value.Dispose();
			}
			detours[method] = DetourFactory.Current.CreateDetour(method, replacement);
		}
	}

	private static Assembly GetExecutingAssemblyReplacement()
	{
		StackFrame stackFrame = new StackTrace().GetFrames()?.Skip(1).FirstOrDefault();
		if (stackFrame != null)
		{
			MethodBase methodFromStackframe = Harmony.GetMethodFromStackframe(stackFrame);
			if ((object)methodFromStackframe != null)
			{
				return methodFromStackframe.Module.Assembly;
			}
		}
		return Assembly.GetExecutingAssembly();
	}

	internal static IEnumerable<CodeInstruction> GetExecutingAssemblyTranspiler(IEnumerable<CodeInstruction> instructions)
	{
		return instructions.MethodReplacer(m_GetExecutingAssembly, m_GetExecutingAssemblyReplacement);
	}

	public static MethodInfo CreateMethod(string name, Type returnType, List<KeyValuePair<string, Type>> parameters, Action<ILGenerator> generator)
	{
		Type[] parameterTypes = parameters.Select((KeyValuePair<string, Type> p) => p.Value).ToArray();
		DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition(name, returnType, parameterTypes);
		for (int i = 0; i < parameters.Count; i++)
		{
			dynamicMethodDefinition.Definition.Parameters[i].Name = parameters[i].Key;
		}
		ILGenerator iLGenerator = dynamicMethodDefinition.GetILGenerator();
		generator(iLGenerator);
		return dynamicMethodDefinition.Generate();
	}

	internal static MethodInfo GetPatchMethod(Type patchType, string attributeName)
	{
		MethodInfo methodInfo = patchType.GetMethods(AccessTools.all).FirstOrDefault((MethodInfo m) => m.GetCustomAttributes(inherit: true).Any((object a) => a.GetType().FullName == attributeName));
		if ((object)methodInfo == null)
		{
			string name = attributeName.Replace("HarmonyLib.Harmony", "");
			methodInfo = patchType.GetMethod(name, AccessTools.all);
		}
		return methodInfo;
	}

	internal static AssemblyBuilder DefineDynamicAssembly(string name)
	{
		AssemblyName name2 = new AssemblyName(name);
		return AppDomain.CurrentDomain.DefineDynamicAssembly(name2, AssemblyBuilderAccess.Run);
	}

	internal static List<AttributePatch> GetPatchMethods(Type type)
	{
		return (from attributePatch in AccessTools.GetDeclaredMethods(type).Select(AttributePatch.Create)
			where attributePatch != null
			select attributePatch).ToList();
	}

	internal static MethodBase GetOriginalMethod(this HarmonyMethod attr)
	{
		try
		{
			MethodType? methodType = attr.methodType;
			if (methodType.HasValue)
			{
				switch (methodType.GetValueOrDefault())
				{
				case MethodType.Normal:
					if (attr.methodName == null)
					{
						return null;
					}
					return AccessTools.DeclaredMethod(attr.declaringType, attr.methodName, attr.argumentTypes);
				case MethodType.Getter:
					if (attr.methodName == null)
					{
						return AccessTools.DeclaredIndexer(attr.declaringType, attr.argumentTypes).GetGetMethod(nonPublic: true);
					}
					return AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetGetMethod(nonPublic: true);
				case MethodType.Setter:
					if (attr.methodName == null)
					{
						return AccessTools.DeclaredIndexer(attr.declaringType, attr.argumentTypes).GetSetMethod(nonPublic: true);
					}
					return AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetSetMethod(nonPublic: true);
				case MethodType.Constructor:
					return AccessTools.DeclaredConstructor(attr.declaringType, attr.argumentTypes);
				case MethodType.StaticConstructor:
					return (from c in AccessTools.GetDeclaredConstructors(attr.declaringType, null)
						where c.IsStatic
						select c).FirstOrDefault();
				case MethodType.Enumerator:
				{
					if (attr.methodName == null)
					{
						return null;
					}
					MethodInfo method2 = AccessTools.DeclaredMethod(attr.declaringType, attr.methodName, attr.argumentTypes);
					return AccessTools.EnumeratorMoveNext(method2);
				}
				case MethodType.Async:
				{
					if (attr.methodName == null)
					{
						return null;
					}
					MethodInfo method = AccessTools.DeclaredMethod(attr.declaringType, attr.methodName, attr.argumentTypes);
					return AccessTools.AsyncMoveNext(method);
				}
				}
			}
		}
		catch (AmbiguousMatchException ex)
		{
			throw new HarmonyException("Ambiguous match for HarmonyMethod[" + attr.Description() + "]", ex.InnerException ?? ex);
		}
		return null;
	}
}
