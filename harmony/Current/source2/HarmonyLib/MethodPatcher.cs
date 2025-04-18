using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Mono.Cecil;
using MonoMod.Utils;

namespace HarmonyLib;

internal class MethodPatcher
{
	private const string INSTANCE_PARAM = "__instance";

	private const string ORIGINAL_METHOD_PARAM = "__originalMethod";

	private const string ARGS_ARRAY_VAR = "__args";

	private const string RESULT_VAR = "__result";

	private const string RESULT_REF_VAR = "__resultRef";

	private const string STATE_VAR = "__state";

	private const string EXCEPTION_VAR = "__exception";

	private const string RUN_ORIGINAL_VAR = "__runOriginal";

	private const string PARAM_INDEX_PREFIX = "__";

	private const string INSTANCE_FIELD_PREFIX = "___";

	private readonly bool debug;

	private readonly MethodBase original;

	private readonly MethodBase source;

	private readonly List<MethodInfo> prefixes;

	private readonly List<MethodInfo> postfixes;

	private readonly List<MethodInfo> transpilers;

	private readonly List<MethodInfo> finalizers;

	private readonly int idx;

	private readonly Type returnType;

	private readonly DynamicMethodDefinition patch;

	private readonly ILGenerator il;

	private readonly Emitter emitter;

	private static readonly MethodInfo m_GetMethodFromHandle1 = typeof(MethodBase).GetMethod("GetMethodFromHandle", new Type[1] { typeof(RuntimeMethodHandle) });

	private static readonly MethodInfo m_GetMethodFromHandle2 = typeof(MethodBase).GetMethod("GetMethodFromHandle", new Type[2]
	{
		typeof(RuntimeMethodHandle),
		typeof(RuntimeTypeHandle)
	});

	internal MethodPatcher(MethodBase original, MethodBase source, List<MethodInfo> prefixes, List<MethodInfo> postfixes, List<MethodInfo> transpilers, List<MethodInfo> finalizers, bool debug)
	{
		if ((object)original == null)
		{
			throw new ArgumentNullException("original");
		}
		this.debug = debug;
		this.original = original;
		this.source = source;
		this.prefixes = prefixes;
		this.postfixes = postfixes;
		this.transpilers = transpilers;
		this.finalizers = finalizers;
		if (debug)
		{
			FileLog.LogBuffered("### Patch: " + original.FullDescription());
			FileLog.FlushBuffer();
		}
		idx = prefixes.Count + postfixes.Count + finalizers.Count;
		returnType = AccessTools.GetReturnedType(original);
		patch = CreateDynamicMethod(original, $"_Patch{idx}", debug);
		if (patch == null)
		{
			throw new Exception("Could not create replacement method");
		}
		il = patch.GetILGenerator();
		emitter = new Emitter(il, debug);
	}

	internal MethodInfo CreateReplacement(out Dictionary<int, CodeInstruction> finalInstructions)
	{
		LocalBuilder[] existingVariables = DeclareOriginalLocalVariables(il, source ?? original);
		Dictionary<string, LocalBuilder> privateVars = new Dictionary<string, LocalBuilder>();
		List<MethodInfo> list = prefixes.Union(postfixes).Union(finalizers).ToList();
		LocalBuilder localBuilder = null;
		if (idx > 0)
		{
			localBuilder = DeclareLocalVariable(returnType, isReturnValue: true);
			privateVars["__result"] = localBuilder;
		}
		if (list.Any((MethodInfo fix) => fix.GetParameters().Any((ParameterInfo p) => p.Name == "__resultRef")) && returnType.IsByRef)
		{
			LocalBuilder localBuilder2 = il.DeclareLocal(typeof(RefResult<>).MakeGenericType(returnType.GetElementType()));
			emitter.Emit(OpCodes.Ldnull);
			emitter.Emit(OpCodes.Stloc, localBuilder2);
			privateVars["__resultRef"] = localBuilder2;
		}
		LocalBuilder localBuilder3 = null;
		if (list.Any((MethodInfo fix) => fix.GetParameters().Any((ParameterInfo p) => p.Name == "__args")))
		{
			PrepareArgumentArray();
			localBuilder3 = il.DeclareLocal(typeof(object[]));
			emitter.Emit(OpCodes.Stloc, localBuilder3);
			privateVars["__args"] = localBuilder3;
		}
		Label? label = null;
		LocalBuilder localBuilder4 = null;
		bool flag = prefixes.Any(PrefixAffectsOriginal);
		bool flag2 = list.Any((MethodInfo fix) => fix.GetParameters().Any((ParameterInfo p) => p.Name == "__runOriginal"));
		if (flag || flag2)
		{
			localBuilder4 = DeclareLocalVariable(typeof(bool));
			emitter.Emit(OpCodes.Ldc_I4_1);
			emitter.Emit(OpCodes.Stloc, localBuilder4);
			if (flag)
			{
				label = il.DefineLabel();
			}
		}
		list.ForEach(delegate(MethodInfo fix)
		{
			if ((object)fix.DeclaringType != null && !privateVars.ContainsKey(fix.DeclaringType.AssemblyQualifiedName))
			{
				(from patchParam in fix.GetParameters()
					where patchParam.Name == "__state"
					select patchParam).Do(delegate(ParameterInfo patchParam)
				{
					LocalBuilder value = DeclareLocalVariable(patchParam.ParameterType);
					privateVars[fix.DeclaringType.AssemblyQualifiedName] = value;
				});
			}
		});
		LocalBuilder local = null;
		if (finalizers.Count > 0)
		{
			local = DeclareLocalVariable(typeof(bool));
			privateVars["__exception"] = DeclareLocalVariable(typeof(Exception));
			emitter.MarkBlockBefore(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock), out var _);
		}
		AddPrefixes(privateVars, localBuilder4);
		if (label.HasValue)
		{
			emitter.Emit(OpCodes.Ldloc, localBuilder4);
			emitter.Emit(OpCodes.Brfalse, label.Value);
		}
		MethodCopier methodCopier = new MethodCopier(source ?? original, il, existingVariables);
		methodCopier.SetDebugging(debug);
		foreach (MethodInfo transpiler in transpilers)
		{
			methodCopier.AddTranspiler(transpiler);
		}
		methodCopier.AddTranspiler(PatchTools.m_GetExecutingAssemblyReplacementTranspiler);
		List<Label> list2 = new List<Label>();
		methodCopier.Finalize(emitter, list2, out var hasReturnCode, out var methodEndsInDeadCode);
		foreach (Label item in list2)
		{
			emitter.MarkLabel(item);
		}
		if (localBuilder != null && hasReturnCode)
		{
			emitter.Emit(OpCodes.Stloc, localBuilder);
		}
		if (label.HasValue)
		{
			emitter.MarkLabel(label.Value);
		}
		AddPostfixes(privateVars, localBuilder4, passthroughPatches: false);
		if (localBuilder != null && (hasReturnCode || (methodEndsInDeadCode && label.HasValue)))
		{
			emitter.Emit(OpCodes.Ldloc, localBuilder);
		}
		bool flag3 = AddPostfixes(privateVars, localBuilder4, passthroughPatches: true);
		bool flag4 = finalizers.Count > 0;
		if (flag4)
		{
			if (flag3)
			{
				emitter.Emit(OpCodes.Stloc, localBuilder);
				emitter.Emit(OpCodes.Ldloc, localBuilder);
			}
			AddFinalizers(privateVars, localBuilder4, catchExceptions: false);
			emitter.Emit(OpCodes.Ldc_I4_1);
			emitter.Emit(OpCodes.Stloc, local);
			Label label3 = il.DefineLabel();
			emitter.Emit(OpCodes.Ldloc, privateVars["__exception"]);
			emitter.Emit(OpCodes.Brfalse, label3);
			emitter.Emit(OpCodes.Ldloc, privateVars["__exception"]);
			emitter.Emit(OpCodes.Throw);
			emitter.MarkLabel(label3);
			emitter.MarkBlockBefore(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock), out var _);
			emitter.Emit(OpCodes.Stloc, privateVars["__exception"]);
			emitter.Emit(OpCodes.Ldloc, local);
			Label label5 = il.DefineLabel();
			emitter.Emit(OpCodes.Brtrue, label5);
			bool flag5 = AddFinalizers(privateVars, localBuilder4, catchExceptions: true);
			emitter.MarkLabel(label5);
			Label label6 = il.DefineLabel();
			emitter.Emit(OpCodes.Ldloc, privateVars["__exception"]);
			emitter.Emit(OpCodes.Brfalse, label6);
			if (flag5)
			{
				emitter.Emit(OpCodes.Rethrow);
			}
			else
			{
				emitter.Emit(OpCodes.Ldloc, privateVars["__exception"]);
				emitter.Emit(OpCodes.Throw);
			}
			emitter.MarkLabel(label6);
			emitter.MarkBlockAfter(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock));
			if (localBuilder != null)
			{
				emitter.Emit(OpCodes.Ldloc, localBuilder);
			}
		}
		if (!methodEndsInDeadCode || label.HasValue || flag4 || postfixes.Count > 0)
		{
			emitter.Emit(OpCodes.Ret);
		}
		finalInstructions = emitter.GetInstructions();
		if (debug)
		{
			FileLog.LogBuffered("DONE");
			FileLog.LogBuffered("");
			FileLog.FlushBuffer();
		}
		return patch.Generate();
	}

	internal static DynamicMethodDefinition CreateDynamicMethod(MethodBase original, string suffix, bool debug)
	{
		if ((object)original == null)
		{
			throw new ArgumentNullException("original");
		}
		string text = (original.DeclaringType?.FullName ?? "GLOBALTYPE") + "." + original.Name + suffix;
		text = text.Replace("<>", "");
		ParameterInfo[] parameters = original.GetParameters();
		List<Type> list = new List<Type>();
		list.AddRange(parameters.Types());
		if (!original.IsStatic)
		{
			if (AccessTools.IsStruct(original.DeclaringType))
			{
				list.Insert(0, original.DeclaringType.MakeByRefType());
			}
			else
			{
				list.Insert(0, original.DeclaringType);
			}
		}
		Type returnedType = AccessTools.GetReturnedType(original);
		DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition(text, returnedType, list.ToArray());
		int num = ((!original.IsStatic) ? 1 : 0);
		if (!original.IsStatic)
		{
			dynamicMethodDefinition.Definition.Parameters[0].Name = "this";
		}
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterDefinition parameterDefinition = dynamicMethodDefinition.Definition.Parameters[i + num];
			parameterDefinition.Attributes = (Mono.Cecil.ParameterAttributes)parameters[i].Attributes;
			parameterDefinition.Name = parameters[i].Name;
		}
		if (debug)
		{
			List<string> list2 = list.Select((Type p) => p.FullDescription()).ToList();
			if (list.Count == dynamicMethodDefinition.Definition.Parameters.Count)
			{
				for (int j = 0; j < list.Count; j++)
				{
					List<string> list3 = list2;
					int index = j;
					list3[index] = list3[index] + " " + dynamicMethodDefinition.Definition.Parameters[j].Name;
				}
			}
			FileLog.Log($"### Replacement: static {returnedType.FullDescription()} {original.DeclaringType?.FullName ?? "GLOBALTYPE"}::{text}({list2.Join()})");
		}
		return dynamicMethodDefinition;
	}

	internal static LocalBuilder[] DeclareOriginalLocalVariables(ILGenerator il, MethodBase member)
	{
		IList<LocalVariableInfo> list = member.GetMethodBody()?.LocalVariables;
		if (list == null)
		{
			return Array.Empty<LocalBuilder>();
		}
		return list.Select((LocalVariableInfo lvi) => il.DeclareLocal(lvi.LocalType, lvi.IsPinned)).ToArray();
	}

	private LocalBuilder DeclareLocalVariable(Type type, bool isReturnValue = false)
	{
		if (type.IsByRef)
		{
			if (isReturnValue)
			{
				LocalBuilder localBuilder = il.DeclareLocal(type);
				emitter.Emit(OpCodes.Ldc_I4_1);
				emitter.Emit(OpCodes.Newarr, type.GetElementType());
				emitter.Emit(OpCodes.Ldc_I4_0);
				emitter.Emit(OpCodes.Ldelema, type.GetElementType());
				emitter.Emit(OpCodes.Stloc, localBuilder);
				return localBuilder;
			}
			type = type.GetElementType();
		}
		if (type.IsEnum)
		{
			type = Enum.GetUnderlyingType(type);
		}
		if (AccessTools.IsClass(type))
		{
			LocalBuilder localBuilder2 = il.DeclareLocal(type);
			emitter.Emit(OpCodes.Ldnull);
			emitter.Emit(OpCodes.Stloc, localBuilder2);
			return localBuilder2;
		}
		if (AccessTools.IsStruct(type))
		{
			LocalBuilder localBuilder3 = il.DeclareLocal(type);
			emitter.Emit(OpCodes.Ldloca, localBuilder3);
			emitter.Emit(OpCodes.Initobj, type);
			return localBuilder3;
		}
		if (AccessTools.IsValue(type))
		{
			LocalBuilder localBuilder4 = il.DeclareLocal(type);
			if (type == typeof(float))
			{
				emitter.Emit(OpCodes.Ldc_R4, 0f);
			}
			else if (type == typeof(double))
			{
				emitter.Emit(OpCodes.Ldc_R8, 0.0);
			}
			else if (type == typeof(long) || type == typeof(ulong))
			{
				emitter.Emit(OpCodes.Ldc_I8, 0L);
			}
			else
			{
				emitter.Emit(OpCodes.Ldc_I4, 0);
			}
			emitter.Emit(OpCodes.Stloc, localBuilder4);
			return localBuilder4;
		}
		return null;
	}

	private static OpCode LoadIndOpCodeFor(Type type)
	{
		if (type.IsEnum)
		{
			return OpCodes.Ldind_I4;
		}
		if (type == typeof(float))
		{
			return OpCodes.Ldind_R4;
		}
		if (type == typeof(double))
		{
			return OpCodes.Ldind_R8;
		}
		if (type == typeof(byte))
		{
			return OpCodes.Ldind_U1;
		}
		if (type == typeof(ushort))
		{
			return OpCodes.Ldind_U2;
		}
		if (type == typeof(uint))
		{
			return OpCodes.Ldind_U4;
		}
		if (type == typeof(ulong))
		{
			return OpCodes.Ldind_I8;
		}
		if (type == typeof(sbyte))
		{
			return OpCodes.Ldind_I1;
		}
		if (type == typeof(short))
		{
			return OpCodes.Ldind_I2;
		}
		if (type == typeof(int))
		{
			return OpCodes.Ldind_I4;
		}
		if (type == typeof(long))
		{
			return OpCodes.Ldind_I8;
		}
		return OpCodes.Ldind_Ref;
	}

	private static OpCode StoreIndOpCodeFor(Type type)
	{
		if (type.IsEnum)
		{
			return OpCodes.Stind_I4;
		}
		if (type == typeof(float))
		{
			return OpCodes.Stind_R4;
		}
		if (type == typeof(double))
		{
			return OpCodes.Stind_R8;
		}
		if (type == typeof(byte))
		{
			return OpCodes.Stind_I1;
		}
		if (type == typeof(ushort))
		{
			return OpCodes.Stind_I2;
		}
		if (type == typeof(uint))
		{
			return OpCodes.Stind_I4;
		}
		if (type == typeof(ulong))
		{
			return OpCodes.Stind_I8;
		}
		if (type == typeof(sbyte))
		{
			return OpCodes.Stind_I1;
		}
		if (type == typeof(short))
		{
			return OpCodes.Stind_I2;
		}
		if (type == typeof(int))
		{
			return OpCodes.Stind_I4;
		}
		if (type == typeof(long))
		{
			return OpCodes.Stind_I8;
		}
		return OpCodes.Stind_Ref;
	}

	private void InitializeOutParameter(int argIndex, Type type)
	{
		if (type.IsByRef)
		{
			type = type.GetElementType();
		}
		emitter.Emit(OpCodes.Ldarg, argIndex);
		if (AccessTools.IsStruct(type))
		{
			emitter.Emit(OpCodes.Initobj, type);
		}
		else if (AccessTools.IsValue(type))
		{
			if (type == typeof(float))
			{
				emitter.Emit(OpCodes.Ldc_R4, 0f);
				emitter.Emit(OpCodes.Stind_R4);
			}
			else if (type == typeof(double))
			{
				emitter.Emit(OpCodes.Ldc_R8, 0.0);
				emitter.Emit(OpCodes.Stind_R8);
			}
			else if (type == typeof(long))
			{
				emitter.Emit(OpCodes.Ldc_I8, 0L);
				emitter.Emit(OpCodes.Stind_I8);
			}
			else
			{
				emitter.Emit(OpCodes.Ldc_I4, 0);
				emitter.Emit(OpCodes.Stind_I4);
			}
		}
		else
		{
			emitter.Emit(OpCodes.Ldnull);
			emitter.Emit(OpCodes.Stind_Ref);
		}
	}

	private bool EmitOriginalBaseMethod()
	{
		if (original is MethodInfo meth)
		{
			emitter.Emit(OpCodes.Ldtoken, meth);
		}
		else
		{
			if (!(original is ConstructorInfo con))
			{
				return false;
			}
			emitter.Emit(OpCodes.Ldtoken, con);
		}
		Type reflectedType = original.ReflectedType;
		if (reflectedType.IsGenericType)
		{
			emitter.Emit(OpCodes.Ldtoken, reflectedType);
		}
		emitter.Emit(OpCodes.Call, reflectedType.IsGenericType ? m_GetMethodFromHandle2 : m_GetMethodFromHandle1);
		return true;
	}

	private void EmitCallParameter(MethodInfo patch, Dictionary<string, LocalBuilder> variables, LocalBuilder runOriginalVariable, bool allowFirsParamPassthrough, out LocalBuilder tmpInstanceBoxingVar, out LocalBuilder tmpObjectVar, out bool refResultUsed, List<KeyValuePair<LocalBuilder, Type>> tmpBoxVars)
	{
		tmpInstanceBoxingVar = null;
		tmpObjectVar = null;
		refResultUsed = false;
		bool flag = !original.IsStatic;
		ParameterInfo[] parameters = original.GetParameters();
		string[] originalParameterNames = parameters.Select((ParameterInfo p) => p.Name).ToArray();
		Type declaringType = original.DeclaringType;
		List<ParameterInfo> list = patch.GetParameters().ToList();
		if (allowFirsParamPassthrough && patch.ReturnType != typeof(void) && list.Count > 0 && list[0].ParameterType == patch.ReturnType)
		{
			list.RemoveRange(0, 1);
		}
		foreach (ParameterInfo item in list)
		{
			if (item.Name == "__originalMethod")
			{
				if (!EmitOriginalBaseMethod())
				{
					emitter.Emit(OpCodes.Ldnull);
				}
				continue;
			}
			if (item.Name == "__runOriginal")
			{
				if (runOriginalVariable != null)
				{
					emitter.Emit(OpCodes.Ldloc, runOriginalVariable);
				}
				else
				{
					emitter.Emit(OpCodes.Ldc_I4_0);
				}
				continue;
			}
			if (item.Name == "__instance")
			{
				if (original.IsStatic)
				{
					emitter.Emit(OpCodes.Ldnull);
					continue;
				}
				Type parameterType = item.ParameterType;
				bool isByRef = parameterType.IsByRef;
				bool flag2 = parameterType == typeof(object) || parameterType == typeof(object).MakeByRefType();
				if (AccessTools.IsStruct(declaringType))
				{
					if (flag2)
					{
						if (isByRef)
						{
							emitter.Emit(OpCodes.Ldarg_0);
							emitter.Emit(OpCodes.Ldobj, declaringType);
							emitter.Emit(OpCodes.Box, declaringType);
							tmpInstanceBoxingVar = il.DeclareLocal(typeof(object));
							emitter.Emit(OpCodes.Stloc, tmpInstanceBoxingVar);
							emitter.Emit(OpCodes.Ldloca, tmpInstanceBoxingVar);
						}
						else
						{
							emitter.Emit(OpCodes.Ldarg_0);
							emitter.Emit(OpCodes.Ldobj, declaringType);
							emitter.Emit(OpCodes.Box, declaringType);
						}
					}
					else if (isByRef)
					{
						emitter.Emit(OpCodes.Ldarg_0);
					}
					else
					{
						emitter.Emit(OpCodes.Ldarg_0);
						emitter.Emit(OpCodes.Ldobj, declaringType);
					}
				}
				else if (isByRef)
				{
					emitter.Emit(OpCodes.Ldarga, 0);
				}
				else
				{
					emitter.Emit(OpCodes.Ldarg_0);
				}
				continue;
			}
			if (item.Name == "__args")
			{
				if (variables.TryGetValue("__args", out var value))
				{
					emitter.Emit(OpCodes.Ldloc, value);
				}
				else
				{
					emitter.Emit(OpCodes.Ldnull);
				}
				continue;
			}
			if (item.Name.StartsWith("___", StringComparison.Ordinal))
			{
				string text = item.Name.Substring("___".Length);
				FieldInfo fieldInfo;
				if (text.All(char.IsDigit))
				{
					fieldInfo = AccessTools.DeclaredField(declaringType, int.Parse(text));
					if ((object)fieldInfo == null)
					{
						throw new ArgumentException("No field found at given index in class " + (declaringType?.AssemblyQualifiedName ?? "null"), text);
					}
				}
				else
				{
					fieldInfo = AccessTools.Field(declaringType, text);
					if ((object)fieldInfo == null)
					{
						throw new ArgumentException("No such field defined in class " + (declaringType?.AssemblyQualifiedName ?? "null"), text);
					}
				}
				if (fieldInfo.IsStatic)
				{
					emitter.Emit(item.ParameterType.IsByRef ? OpCodes.Ldsflda : OpCodes.Ldsfld, fieldInfo);
					continue;
				}
				emitter.Emit(OpCodes.Ldarg_0);
				emitter.Emit(item.ParameterType.IsByRef ? OpCodes.Ldflda : OpCodes.Ldfld, fieldInfo);
				continue;
			}
			if (item.Name == "__state")
			{
				OpCode opcode = (item.ParameterType.IsByRef ? OpCodes.Ldloca : OpCodes.Ldloc);
				if (variables.TryGetValue(patch.DeclaringType?.AssemblyQualifiedName ?? "null", out var value2))
				{
					emitter.Emit(opcode, value2);
				}
				else
				{
					emitter.Emit(OpCodes.Ldnull);
				}
				continue;
			}
			if (item.Name == "__result")
			{
				if (returnType == typeof(void))
				{
					throw new Exception("Cannot get result from void method " + original.FullDescription());
				}
				Type type = item.ParameterType;
				if (type.IsByRef && !returnType.IsByRef)
				{
					type = type.GetElementType();
				}
				if (!type.IsAssignableFrom(returnType))
				{
					throw new Exception($"Cannot assign method return type {returnType.FullName} to {"__result"} type {type.FullName} for method {original.FullDescription()}");
				}
				OpCode opcode2 = ((item.ParameterType.IsByRef && !returnType.IsByRef) ? OpCodes.Ldloca : OpCodes.Ldloc);
				if (returnType.IsValueType && item.ParameterType == typeof(object).MakeByRefType())
				{
					opcode2 = OpCodes.Ldloc;
				}
				emitter.Emit(opcode2, variables["__result"]);
				if (returnType.IsValueType)
				{
					if (item.ParameterType == typeof(object))
					{
						emitter.Emit(OpCodes.Box, returnType);
					}
					else if (item.ParameterType == typeof(object).MakeByRefType())
					{
						emitter.Emit(OpCodes.Box, returnType);
						tmpObjectVar = il.DeclareLocal(typeof(object));
						emitter.Emit(OpCodes.Stloc, tmpObjectVar);
						emitter.Emit(OpCodes.Ldloca, tmpObjectVar);
					}
				}
				continue;
			}
			if (item.Name == "__resultRef")
			{
				if (!returnType.IsByRef)
				{
					throw new Exception($"Cannot use {"__resultRef"} with non-ref return type {returnType.FullName} of method {original.FullDescription()}");
				}
				Type parameterType2 = item.ParameterType;
				Type type2 = typeof(RefResult<>).MakeGenericType(returnType.GetElementType()).MakeByRefType();
				if (parameterType2 != type2)
				{
					throw new Exception($"Wrong type of {"__resultRef"} for method {original.FullDescription()}. Expected {type2.FullName}, got {parameterType2.FullName}");
				}
				emitter.Emit(OpCodes.Ldloca, variables["__resultRef"]);
				refResultUsed = true;
				continue;
			}
			if (variables.TryGetValue(item.Name, out var value3))
			{
				OpCode opcode3 = (item.ParameterType.IsByRef ? OpCodes.Ldloca : OpCodes.Ldloc);
				emitter.Emit(opcode3, value3);
				continue;
			}
			int result;
			if (item.Name.StartsWith("__", StringComparison.Ordinal))
			{
				string s = item.Name.Substring("__".Length);
				if (!int.TryParse(s, out result))
				{
					throw new Exception("Parameter " + item.Name + " does not contain a valid index");
				}
				if (result < 0 || result >= parameters.Length)
				{
					throw new Exception($"No parameter found at index {result}");
				}
			}
			else
			{
				result = patch.GetArgumentIndex(originalParameterNames, item);
				if (result == -1)
				{
					HarmonyMethod mergedFromType = HarmonyMethodExtensions.GetMergedFromType(item.ParameterType);
					HarmonyMethod harmonyMethod = mergedFromType;
					MethodType valueOrDefault = harmonyMethod.methodType.GetValueOrDefault();
					if (!harmonyMethod.methodType.HasValue)
					{
						valueOrDefault = MethodType.Normal;
						harmonyMethod.methodType = valueOrDefault;
					}
					MethodBase originalMethod = mergedFromType.GetOriginalMethod();
					if (originalMethod is MethodInfo methodInfo)
					{
						ConstructorInfo constructor = item.ParameterType.GetConstructor(new Type[2]
						{
							typeof(object),
							typeof(IntPtr)
						});
						if ((object)constructor != null)
						{
							if (methodInfo.IsStatic)
							{
								emitter.Emit(OpCodes.Ldnull);
							}
							else
							{
								emitter.Emit(OpCodes.Ldarg_0);
								if (declaringType != null && declaringType.IsValueType)
								{
									emitter.Emit(OpCodes.Ldobj, declaringType);
									emitter.Emit(OpCodes.Box, declaringType);
								}
							}
							if (!methodInfo.IsStatic && !mergedFromType.nonVirtualDelegate)
							{
								emitter.Emit(OpCodes.Dup);
								emitter.Emit(OpCodes.Ldvirtftn, methodInfo);
							}
							else
							{
								emitter.Emit(OpCodes.Ldftn, methodInfo);
							}
							emitter.Emit(OpCodes.Newobj, constructor);
							continue;
						}
					}
					throw new Exception("Parameter \"" + item.Name + "\" not found in method " + original.FullDescription());
				}
			}
			Type parameterType3 = parameters[result].ParameterType;
			Type type3 = (parameterType3.IsByRef ? parameterType3.GetElementType() : parameterType3);
			Type parameterType4 = item.ParameterType;
			Type type4 = (parameterType4.IsByRef ? parameterType4.GetElementType() : parameterType4);
			bool flag3 = !parameters[result].IsOut && !parameterType3.IsByRef;
			bool flag4 = !item.IsOut && !parameterType4.IsByRef;
			bool flag5 = type3.IsValueType && !type4.IsValueType;
			int arg = result + (flag ? 1 : 0);
			if (flag3 == flag4)
			{
				emitter.Emit(OpCodes.Ldarg, arg);
				if (flag5)
				{
					if (flag4)
					{
						emitter.Emit(OpCodes.Box, type3);
						continue;
					}
					emitter.Emit(OpCodes.Ldobj, type3);
					emitter.Emit(OpCodes.Box, type3);
					LocalBuilder localBuilder = il.DeclareLocal(type4);
					emitter.Emit(OpCodes.Stloc, localBuilder);
					emitter.Emit(OpCodes.Ldloca_S, localBuilder);
					tmpBoxVars.Add(new KeyValuePair<LocalBuilder, Type>(localBuilder, type3));
				}
			}
			else if (flag3 && !flag4)
			{
				if (flag5)
				{
					emitter.Emit(OpCodes.Ldarg, arg);
					emitter.Emit(OpCodes.Box, type3);
					LocalBuilder local = il.DeclareLocal(type4);
					emitter.Emit(OpCodes.Stloc, local);
					emitter.Emit(OpCodes.Ldloca_S, local);
				}
				else
				{
					emitter.Emit(OpCodes.Ldarga, arg);
				}
			}
			else
			{
				emitter.Emit(OpCodes.Ldarg, arg);
				if (flag5)
				{
					emitter.Emit(OpCodes.Ldobj, type3);
					emitter.Emit(OpCodes.Box, type3);
				}
				else if (type3.IsValueType)
				{
					emitter.Emit(OpCodes.Ldobj, type3);
				}
				else
				{
					emitter.Emit(LoadIndOpCodeFor(parameters[result].ParameterType));
				}
			}
		}
	}

	private static bool PrefixAffectsOriginal(MethodInfo fix)
	{
		if (fix.ReturnType == typeof(bool))
		{
			return true;
		}
		return fix.GetParameters().Any(delegate(ParameterInfo p)
		{
			string name = p.Name;
			Type parameterType = p.ParameterType;
			switch (name)
			{
			case "__instance":
				return false;
			case "__originalMethod":
				return false;
			case "__state":
				return false;
			default:
				if (p.IsOut || p.IsRetval)
				{
					return true;
				}
				if (parameterType.IsByRef)
				{
					return true;
				}
				if (!AccessTools.IsValue(parameterType) && !AccessTools.IsStruct(parameterType))
				{
					return true;
				}
				return false;
			}
		});
	}

	private void AddPrefixes(Dictionary<string, LocalBuilder> variables, LocalBuilder runOriginalVariable)
	{
		prefixes.Do(delegate(MethodInfo fix)
		{
			Label? label = (PrefixAffectsOriginal(fix) ? new Label?(il.DefineLabel()) : ((Label?)null));
			if (label.HasValue)
			{
				emitter.Emit(OpCodes.Ldloc, runOriginalVariable);
				emitter.Emit(OpCodes.Brfalse, label.Value);
			}
			List<KeyValuePair<LocalBuilder, Type>> list = new List<KeyValuePair<LocalBuilder, Type>>();
			EmitCallParameter(fix, variables, runOriginalVariable, allowFirsParamPassthrough: false, out var tmpInstanceBoxingVar, out var tmpObjectVar, out var refResultUsed, list);
			emitter.Emit(OpCodes.Call, fix);
			if (fix.GetParameters().Any((ParameterInfo p) => p.Name == "__args"))
			{
				RestoreArgumentArray(variables);
			}
			if (tmpInstanceBoxingVar != null)
			{
				emitter.Emit(OpCodes.Ldarg_0);
				emitter.Emit(OpCodes.Ldloc, tmpInstanceBoxingVar);
				emitter.Emit(OpCodes.Unbox_Any, original.DeclaringType);
				emitter.Emit(OpCodes.Stobj, original.DeclaringType);
			}
			if (refResultUsed)
			{
				Label label2 = il.DefineLabel();
				emitter.Emit(OpCodes.Ldloc, variables["__resultRef"]);
				emitter.Emit(OpCodes.Brfalse_S, label2);
				emitter.Emit(OpCodes.Ldloc, variables["__resultRef"]);
				emitter.Emit(OpCodes.Callvirt, AccessTools.Method(variables["__resultRef"].LocalType, "Invoke"));
				emitter.Emit(OpCodes.Stloc, variables["__result"]);
				emitter.Emit(OpCodes.Ldnull);
				emitter.Emit(OpCodes.Stloc, variables["__resultRef"]);
				emitter.MarkLabel(label2);
				emitter.Emit(OpCodes.Nop);
			}
			else if (tmpObjectVar != null)
			{
				emitter.Emit(OpCodes.Ldloc, tmpObjectVar);
				emitter.Emit(OpCodes.Unbox_Any, AccessTools.GetReturnedType(original));
				emitter.Emit(OpCodes.Stloc, variables["__result"]);
			}
			list.Do(delegate(KeyValuePair<LocalBuilder, Type> tmpBoxVar)
			{
				emitter.Emit(original.IsStatic ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1);
				emitter.Emit(OpCodes.Ldloc, tmpBoxVar.Key);
				emitter.Emit(OpCodes.Unbox_Any, tmpBoxVar.Value);
				emitter.Emit(OpCodes.Stobj, tmpBoxVar.Value);
			});
			Type type = fix.ReturnType;
			if (type != typeof(void))
			{
				if (type != typeof(bool))
				{
					throw new Exception($"Prefix patch {fix} has not \"bool\" or \"void\" return type: {fix.ReturnType}");
				}
				emitter.Emit(OpCodes.Stloc, runOriginalVariable);
			}
			if (label.HasValue)
			{
				emitter.MarkLabel(label.Value);
				emitter.Emit(OpCodes.Nop);
			}
		});
	}

	private bool AddPostfixes(Dictionary<string, LocalBuilder> variables, LocalBuilder runOriginalVariable, bool passthroughPatches)
	{
		bool result = false;
		postfixes.Where((MethodInfo fix) => passthroughPatches == (fix.ReturnType != typeof(void))).Do(delegate(MethodInfo fix)
		{
			List<KeyValuePair<LocalBuilder, Type>> list = new List<KeyValuePair<LocalBuilder, Type>>();
			EmitCallParameter(fix, variables, runOriginalVariable, allowFirsParamPassthrough: true, out var tmpInstanceBoxingVar, out var tmpObjectVar, out var refResultUsed, list);
			emitter.Emit(OpCodes.Call, fix);
			if (fix.GetParameters().Any((ParameterInfo p) => p.Name == "__args"))
			{
				RestoreArgumentArray(variables);
			}
			if (tmpInstanceBoxingVar != null)
			{
				emitter.Emit(OpCodes.Ldarg_0);
				emitter.Emit(OpCodes.Ldloc, tmpInstanceBoxingVar);
				emitter.Emit(OpCodes.Unbox_Any, original.DeclaringType);
				emitter.Emit(OpCodes.Stobj, original.DeclaringType);
			}
			if (refResultUsed)
			{
				Label label = il.DefineLabel();
				emitter.Emit(OpCodes.Ldloc, variables["__resultRef"]);
				emitter.Emit(OpCodes.Brfalse_S, label);
				emitter.Emit(OpCodes.Ldloc, variables["__resultRef"]);
				emitter.Emit(OpCodes.Callvirt, AccessTools.Method(variables["__resultRef"].LocalType, "Invoke"));
				emitter.Emit(OpCodes.Stloc, variables["__result"]);
				emitter.Emit(OpCodes.Ldnull);
				emitter.Emit(OpCodes.Stloc, variables["__resultRef"]);
				emitter.MarkLabel(label);
				emitter.Emit(OpCodes.Nop);
			}
			else if (tmpObjectVar != null)
			{
				emitter.Emit(OpCodes.Ldloc, tmpObjectVar);
				emitter.Emit(OpCodes.Unbox_Any, AccessTools.GetReturnedType(original));
				emitter.Emit(OpCodes.Stloc, variables["__result"]);
			}
			list.Do(delegate(KeyValuePair<LocalBuilder, Type> tmpBoxVar)
			{
				emitter.Emit(original.IsStatic ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1);
				emitter.Emit(OpCodes.Ldloc, tmpBoxVar.Key);
				emitter.Emit(OpCodes.Unbox_Any, tmpBoxVar.Value);
				emitter.Emit(OpCodes.Stobj, tmpBoxVar.Value);
			});
			if (fix.ReturnType != typeof(void))
			{
				ParameterInfo parameterInfo = fix.GetParameters().FirstOrDefault();
				if (parameterInfo == null || !(fix.ReturnType == parameterInfo.ParameterType))
				{
					if (parameterInfo != null)
					{
						throw new Exception($"Return type of pass through postfix {fix} does not match type of its first parameter");
					}
					throw new Exception($"Postfix patch {fix} must have a \"void\" return type");
				}
				result = true;
			}
		});
		return result;
	}

	private bool AddFinalizers(Dictionary<string, LocalBuilder> variables, LocalBuilder runOriginalVariable, bool catchExceptions)
	{
		bool rethrowPossible = true;
		finalizers.Do(delegate(MethodInfo fix)
		{
			if (catchExceptions)
			{
				emitter.MarkBlockBefore(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock), out var _);
			}
			List<KeyValuePair<LocalBuilder, Type>> list = new List<KeyValuePair<LocalBuilder, Type>>();
			EmitCallParameter(fix, variables, runOriginalVariable, allowFirsParamPassthrough: false, out var tmpInstanceBoxingVar, out var tmpObjectVar, out var refResultUsed, list);
			emitter.Emit(OpCodes.Call, fix);
			if (fix.GetParameters().Any((ParameterInfo p) => p.Name == "__args"))
			{
				RestoreArgumentArray(variables);
			}
			if (tmpInstanceBoxingVar != null)
			{
				emitter.Emit(OpCodes.Ldarg_0);
				emitter.Emit(OpCodes.Ldloc, tmpInstanceBoxingVar);
				emitter.Emit(OpCodes.Unbox_Any, original.DeclaringType);
				emitter.Emit(OpCodes.Stobj, original.DeclaringType);
			}
			if (refResultUsed)
			{
				Label label2 = il.DefineLabel();
				emitter.Emit(OpCodes.Ldloc, variables["__resultRef"]);
				emitter.Emit(OpCodes.Brfalse_S, label2);
				emitter.Emit(OpCodes.Ldloc, variables["__resultRef"]);
				emitter.Emit(OpCodes.Callvirt, AccessTools.Method(variables["__resultRef"].LocalType, "Invoke"));
				emitter.Emit(OpCodes.Stloc, variables["__result"]);
				emitter.Emit(OpCodes.Ldnull);
				emitter.Emit(OpCodes.Stloc, variables["__resultRef"]);
				emitter.MarkLabel(label2);
				emitter.Emit(OpCodes.Nop);
			}
			else if (tmpObjectVar != null)
			{
				emitter.Emit(OpCodes.Ldloc, tmpObjectVar);
				emitter.Emit(OpCodes.Unbox_Any, AccessTools.GetReturnedType(original));
				emitter.Emit(OpCodes.Stloc, variables["__result"]);
			}
			list.Do(delegate(KeyValuePair<LocalBuilder, Type> tmpBoxVar)
			{
				emitter.Emit(original.IsStatic ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1);
				emitter.Emit(OpCodes.Ldloc, tmpBoxVar.Key);
				emitter.Emit(OpCodes.Unbox_Any, tmpBoxVar.Value);
				emitter.Emit(OpCodes.Stobj, tmpBoxVar.Value);
			});
			if (fix.ReturnType != typeof(void))
			{
				emitter.Emit(OpCodes.Stloc, variables["__exception"]);
				rethrowPossible = false;
			}
			if (catchExceptions)
			{
				emitter.MarkBlockBefore(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock), out var _);
				emitter.Emit(OpCodes.Pop);
				emitter.MarkBlockAfter(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock));
			}
		});
		return rethrowPossible;
	}

	private void PrepareArgumentArray()
	{
		ParameterInfo[] parameters = original.GetParameters();
		int num = 0;
		ParameterInfo[] array = parameters;
		foreach (ParameterInfo parameterInfo in array)
		{
			int argIndex = num++ + ((!original.IsStatic) ? 1 : 0);
			if (parameterInfo.IsOut || parameterInfo.IsRetval)
			{
				InitializeOutParameter(argIndex, parameterInfo.ParameterType);
			}
		}
		emitter.Emit(OpCodes.Ldc_I4, parameters.Length);
		emitter.Emit(OpCodes.Newarr, typeof(object));
		num = 0;
		int num2 = 0;
		ParameterInfo[] array2 = parameters;
		foreach (ParameterInfo parameterInfo2 in array2)
		{
			int arg = num++ + ((!original.IsStatic) ? 1 : 0);
			Type type = parameterInfo2.ParameterType;
			bool isByRef = type.IsByRef;
			if (isByRef)
			{
				type = type.GetElementType();
			}
			emitter.Emit(OpCodes.Dup);
			emitter.Emit(OpCodes.Ldc_I4, num2++);
			emitter.Emit(OpCodes.Ldarg, arg);
			if (isByRef)
			{
				if (AccessTools.IsStruct(type))
				{
					emitter.Emit(OpCodes.Ldobj, type);
				}
				else
				{
					emitter.Emit(LoadIndOpCodeFor(type));
				}
			}
			if (type.IsValueType)
			{
				emitter.Emit(OpCodes.Box, type);
			}
			emitter.Emit(OpCodes.Stelem_Ref);
		}
	}

	private void RestoreArgumentArray(Dictionary<string, LocalBuilder> variables)
	{
		ParameterInfo[] parameters = original.GetParameters();
		int num = 0;
		int num2 = 0;
		ParameterInfo[] array = parameters;
		foreach (ParameterInfo parameterInfo in array)
		{
			int arg = num++ + ((!original.IsStatic) ? 1 : 0);
			Type parameterType = parameterInfo.ParameterType;
			if (parameterType.IsByRef)
			{
				parameterType = parameterType.GetElementType();
				emitter.Emit(OpCodes.Ldarg, arg);
				emitter.Emit(OpCodes.Ldloc, variables["__args"]);
				emitter.Emit(OpCodes.Ldc_I4, num2);
				emitter.Emit(OpCodes.Ldelem_Ref);
				if (parameterType.IsValueType)
				{
					emitter.Emit(OpCodes.Unbox_Any, parameterType);
					if (AccessTools.IsStruct(parameterType))
					{
						emitter.Emit(OpCodes.Stobj, parameterType);
					}
					else
					{
						emitter.Emit(StoreIndOpCodeFor(parameterType));
					}
				}
				else
				{
					emitter.Emit(OpCodes.Castclass, parameterType);
					emitter.Emit(OpCodes.Stind_Ref);
				}
			}
			else
			{
				emitter.Emit(OpCodes.Ldloc, variables["__args"]);
				emitter.Emit(OpCodes.Ldc_I4, num2);
				emitter.Emit(OpCodes.Ldelem_Ref);
				if (parameterType.IsValueType)
				{
					emitter.Emit(OpCodes.Unbox_Any, parameterType);
				}
				else
				{
					emitter.Emit(OpCodes.Castclass, parameterType);
				}
				emitter.Emit(OpCodes.Starg, arg);
			}
			num2++;
		}
	}
}
