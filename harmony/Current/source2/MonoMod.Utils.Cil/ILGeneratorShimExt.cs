using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace MonoMod.Utils.Cil;

internal static class ILGeneratorShimExt
{
	private static readonly Dictionary<Type, MethodInfo> _Emitters;

	private static readonly Dictionary<Type, MethodInfo> _EmittersShim;

	static ILGeneratorShimExt()
	{
		_Emitters = new Dictionary<Type, MethodInfo>();
		_EmittersShim = new Dictionary<Type, MethodInfo>();
		MethodInfo[] methods = typeof(ILGenerator).GetMethods();
		foreach (MethodInfo methodInfo in methods)
		{
			if (!(methodInfo.Name != "Emit"))
			{
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length == 2 && !(parameters[0].ParameterType != typeof(OpCode)))
				{
					_Emitters[parameters[1].ParameterType] = methodInfo;
				}
			}
		}
		methods = typeof(ILGeneratorShim).GetMethods();
		foreach (MethodInfo methodInfo2 in methods)
		{
			if (!(methodInfo2.Name != "Emit"))
			{
				ParameterInfo[] parameters2 = methodInfo2.GetParameters();
				if (parameters2.Length == 2 && !(parameters2[0].ParameterType != typeof(OpCode)))
				{
					_EmittersShim[parameters2[1].ParameterType] = methodInfo2;
				}
			}
		}
	}

	public static ILGeneratorShim GetProxiedShim(this ILGenerator il)
	{
		return (ILGeneratorShim)(Helpers.ThrowIfNull(il, "il").GetType().GetField("Target", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(il));
	}

	public static T GetProxiedShim<T>(this ILGenerator il) where T : ILGeneratorShim
	{
		return (T)il.GetProxiedShim();
	}

	public static object? DynEmit(this ILGenerator il, OpCode opcode, object operand)
	{
		return il.DynEmit(new object[2] { opcode, operand });
	}

	public static object? DynEmit(this ILGenerator il, object[] emitArgs)
	{
		Helpers.ThrowIfArgumentNull(emitArgs, "emitArgs");
		Type operandType = emitArgs[1].GetType();
		object obj = ((object)il.GetProxiedShim()) ?? ((object)il);
		Dictionary<Type, MethodInfo> dictionary = ((obj is ILGeneratorShim) ? _EmittersShim : _Emitters);
		if (!dictionary.TryGetValue(operandType, out var value))
		{
			value = dictionary.FirstOrDefault((KeyValuePair<Type, MethodInfo> kvp) => kvp.Key.IsAssignableFrom(operandType)).Value;
		}
		if (value == null)
		{
			throw new InvalidOperationException("Unexpected unemittable operand type " + operandType.FullName);
		}
		return value.Invoke(obj, emitArgs);
	}
}
