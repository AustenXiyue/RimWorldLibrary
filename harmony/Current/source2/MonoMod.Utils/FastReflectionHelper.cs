using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Logs;

namespace MonoMod.Utils;

internal static class FastReflectionHelper
{
	public delegate object? FastInvoker(object? target, params object?[]? args);

	public delegate void FastStructInvoker(object? target, object? result, params object?[]? args);

	private static class TypedCache<T> where T : struct
	{
		[ThreadStatic]
		public static StrongBox<T?>? NullableStrongBox;
	}

	private enum ReturnTypeClass
	{
		Void,
		ValueType,
		Nullable,
		ReferenceType
	}

	private sealed class FSITuple
	{
		public readonly FastStructInvoker FSI;

		public readonly ReturnTypeClass RTC;

		public readonly Type ReturnType;

		public FSITuple(FastStructInvoker fsi, ReturnTypeClass rtc, Type rt)
		{
			FSI = fsi;
			RTC = rtc;
			ReturnType = rt;
		}
	}

	private static readonly Type[] FastStructInvokerArgs = new Type[3]
	{
		typeof(object),
		typeof(object),
		typeof(object[])
	};

	private static readonly MethodInfo S2FValueType = typeof(FastReflectionHelper).GetMethod("FastInvokerForStructInvokerVT", BindingFlags.Static | BindingFlags.NonPublic);

	private static readonly MethodInfo S2FNullable = typeof(FastReflectionHelper).GetMethod("FastInvokerForStructInvokerNullable", BindingFlags.Static | BindingFlags.NonPublic);

	[ThreadStatic]
	private static WeakBox? CachedWeakBox;

	private static readonly MethodInfo S2FClass = typeof(FastReflectionHelper).GetMethod("FastInvokerForStructInvokerClass", BindingFlags.Static | BindingFlags.NonPublic);

	private static readonly MethodInfo S2FVoid = typeof(FastReflectionHelper).GetMethod("FastInvokerForStructInvokerVoid", BindingFlags.Static | BindingFlags.NonPublic);

	private static ConditionalWeakTable<MemberInfo, FSITuple> fastStructInvokers = new ConditionalWeakTable<MemberInfo, FSITuple>();

	private static ConditionalWeakTable<FSITuple, FastInvoker> fastInvokers = new ConditionalWeakTable<FSITuple, FastInvoker>();

	private static readonly MethodInfo CheckArgsMethod = typeof(FastReflectionHelper).GetMethod("CheckArgs", BindingFlags.Static | BindingFlags.NonPublic);

	private const int TargetArgId = -1;

	private const int ResultArgId = -2;

	private static readonly MethodInfo BadArgExceptionMethod = typeof(FastReflectionHelper).GetMethod("BadArgException", BindingFlags.Static | BindingFlags.NonPublic);

	private static readonly FieldInfo WeakBoxValueField = typeof(WeakBox).GetField("Value");

	private static object? FastInvokerForStructInvokerVT<T>(FastStructInvoker invoker, object? target, params object?[]? args) where T : struct
	{
		object result = default(T);
		invoker(target, result, args);
		return result;
	}

	private static object? FastInvokerForStructInvokerNullable<T>(FastStructInvoker invoker, object? target, params object?[]? args) where T : struct
	{
		StrongBox<T?> strongBox = TypedCache<T>.NullableStrongBox ?? (TypedCache<T>.NullableStrongBox = new StrongBox<T?>(null));
		invoker(target, strongBox, args);
		return strongBox.Value;
	}

	private static object? FastInvokerForStructInvokerClass(FastStructInvoker invoker, object? target, params object?[]? args)
	{
		WeakBox weakBox = CachedWeakBox ?? (CachedWeakBox = new WeakBox());
		invoker(target, weakBox, args);
		return weakBox.Value;
	}

	private static object? FastInvokerForStructInvokerVoid(FastStructInvoker invoker, object? target, params object?[]? args)
	{
		invoker(target, null, args);
		return null;
	}

	private static FastInvoker CreateFastInvoker(FastStructInvoker fsi, ReturnTypeClass retTypeClass, Type returnType)
	{
		return retTypeClass switch
		{
			ReturnTypeClass.Void => S2FVoid.CreateDelegate<FastInvoker>(fsi), 
			ReturnTypeClass.ValueType => S2FValueType.MakeGenericMethod(returnType).CreateDelegate<FastInvoker>(fsi), 
			ReturnTypeClass.Nullable => S2FNullable.MakeGenericMethod(Nullable.GetUnderlyingType(returnType)).CreateDelegate<FastInvoker>(fsi), 
			ReturnTypeClass.ReferenceType => S2FClass.CreateDelegate<FastInvoker>(fsi), 
			_ => throw new NotImplementedException($"Invalid ReturnTypeClass {retTypeClass}"), 
		};
	}

	private static FSITuple GetFSITuple(MethodBase method)
	{
		ReturnTypeClass retTypeClass;
		Type retType;
		return fastStructInvokers.GetValue(method, (MemberInfo _) => new FSITuple(CreateMethodInvoker(method, out retTypeClass, out retType), retTypeClass, retType));
	}

	private static FSITuple GetFSITuple(FieldInfo field)
	{
		ReturnTypeClass retTypeClass;
		Type retType;
		return fastStructInvokers.GetValue(field, (MemberInfo _) => new FSITuple(CreateFieldInvoker(field, out retTypeClass, out retType), retTypeClass, retType));
	}

	private static FSITuple GetFSITuple(MemberInfo member)
	{
		if (!(member is MethodBase method))
		{
			if (member is FieldInfo field)
			{
				return GetFSITuple(field);
			}
			throw new NotSupportedException($"Member type {member.GetType()} is not supported");
		}
		return GetFSITuple(method);
	}

	private static FastInvoker GetFastInvoker(FSITuple tuple)
	{
		return fastInvokers.GetValue(tuple, (FSITuple t) => CreateFastInvoker(t.FSI, t.RTC, t.ReturnType));
	}

	public static FastStructInvoker GetFastStructInvoker(MethodBase method)
	{
		return GetFSITuple(method).FSI;
	}

	public static FastStructInvoker GetFastStructInvoker(FieldInfo field)
	{
		return GetFSITuple(field).FSI;
	}

	public static FastStructInvoker GetFastStructInvoker(MemberInfo member)
	{
		return GetFSITuple(member).FSI;
	}

	public static FastInvoker GetFastInvoker(this MethodBase method)
	{
		return GetFastInvoker(GetFSITuple(method));
	}

	public static FastInvoker GetFastInvoker(this FieldInfo field)
	{
		return GetFastInvoker(GetFSITuple(field));
	}

	public static FastInvoker GetFastInvoker(this MemberInfo member)
	{
		return GetFastInvoker(GetFSITuple(member));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void CheckArgs(bool isStatic, object? target, int retTypeClass, object? result, int expectLen, object?[]? args)
	{
		if (!isStatic)
		{
			Helpers.ThrowIfArgumentNull(target, "target");
		}
		if (retTypeClass != 0 && (uint)(retTypeClass - 1) <= 2u)
		{
			Helpers.ThrowIfArgumentNull(result, "result");
		}
		if (expectLen != 0)
		{
			Helpers.ThrowIfArgumentNull(args, "args");
			if (args.Length < expectLen)
			{
				ThrowArgumentOutOfRange();
			}
		}
		[MethodImpl(MethodImplOptions.NoInlining)]
		static void ThrowArgumentOutOfRange()
		{
			throw new ArgumentOutOfRangeException("args", "Argument array has too few arguments!");
		}
	}

	private static Exception BadArgException(int arg, RuntimeTypeHandle expectType, object? target, object? result, object?[] args)
	{
		Type typeFromHandle = Type.GetTypeFromHandle(expectType);
		Type type = arg switch
		{
			-1 => target?.GetType(), 
			-2 => result?.GetType(), 
			_ => args[arg]?.GetType(), 
		};
		FormatInterpolatedStringHandler handler;
		string text;
		switch (arg)
		{
		case -1:
			text = "target";
			break;
		case -2:
			text = "result";
			break;
		default:
			handler = new FormatInterpolatedStringHandler(6, 1);
			handler.AppendLiteral("args[");
			handler.AppendFormatted(arg);
			handler.AppendLiteral("]");
			text = DebugFormatter.Format(ref handler);
			break;
		}
		string paramName = text;
		if ((object)type == null)
		{
			return new ArgumentNullException(paramName);
		}
		switch (arg)
		{
		case -1:
			handler = new FormatInterpolatedStringHandler(48, 2);
			handler.AppendLiteral("Target object is the wrong type; expected ");
			handler.AppendFormatted(typeFromHandle);
			handler.AppendLiteral(", got ");
			handler.AppendFormatted(type);
			text = DebugFormatter.Format(ref handler);
			break;
		case -2:
			handler = new FormatInterpolatedStringHandler(48, 2);
			handler.AppendLiteral("Result object is the wrong type; expected ");
			handler.AppendFormatted(typeFromHandle);
			handler.AppendLiteral(", got ");
			handler.AppendFormatted(type);
			text = DebugFormatter.Format(ref handler);
			break;
		default:
			handler = new FormatInterpolatedStringHandler(44, 3);
			handler.AppendLiteral("Argument ");
			handler.AppendFormatted(arg);
			handler.AppendLiteral(" is the wrong type; expected ");
			handler.AppendFormatted(typeFromHandle);
			handler.AppendLiteral(", got ");
			handler.AppendFormatted(type);
			text = DebugFormatter.Format(ref handler);
			break;
		}
		return new ArgumentException(text, paramName);
	}

	private static ReturnTypeClass ClassifyType(Type returnType)
	{
		if (returnType == typeof(void))
		{
			return ReturnTypeClass.Void;
		}
		if (returnType.IsValueType)
		{
			if ((object)Nullable.GetUnderlyingType(returnType) != null)
			{
				return ReturnTypeClass.Nullable;
			}
			return ReturnTypeClass.ValueType;
		}
		return ReturnTypeClass.ReferenceType;
	}

	private static void EmitCheckArgs(ILCursor il, bool isStatic, ReturnTypeClass rtc, int expectParams)
	{
		il.Emit(OpCodes.Ldc_I4, isStatic ? 1 : 0);
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Ldc_I4, (int)rtc);
		il.Emit(OpCodes.Ldarg_1);
		il.Emit(OpCodes.Ldc_I4, expectParams);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Call, CheckArgsMethod);
	}

	private static void EmitCheckType(ILCursor il, int argId, Type expectType, ILLabel badArgLbl)
	{
		ILLabel iLLabel = il.DefineLabel();
		bool isByRef = expectType.IsByRef;
		VariableDefinition variableDefinition = null;
		if (isByRef)
		{
			expectType = expectType.GetElementType() ?? expectType;
			ReturnTypeClass rtc = ClassifyType(expectType);
			if (!expectType.IsValueType)
			{
				variableDefinition = new VariableDefinition(il.Module.TypeSystem.Object);
				il.Context.Body.Variables.Add(variableDefinition);
				il.Emit(OpCodes.Stloc, variableDefinition);
				il.Emit(OpCodes.Ldloc, variableDefinition);
			}
			EmitCheckByref(il, rtc, expectType, badArgLbl, argId);
			if (expectType.IsValueType)
			{
				return;
			}
			if (variableDefinition != null)
			{
				il.Emit(OpCodes.Ldloc, variableDefinition);
			}
			EmitLoadByref(il, rtc, expectType);
			il.Emit(OpCodes.Ldind_Ref);
		}
		if (expectType != typeof(object))
		{
			il.Emit(OpCodes.Isinst, expectType);
		}
		il.Emit(OpCodes.Brtrue, iLLabel);
		il.Emit(OpCodes.Ldc_I4, argId);
		il.Emit(OpCodes.Ldtoken, expectType);
		il.Emit(OpCodes.Br, badArgLbl);
		il.MarkLabel(iLLabel);
	}

	private static void EmitCheckAllowNull(ILCursor il, int argId, Type expectType, ILLabel badArgLbl)
	{
		ILLabel iLLabel = il.DefineLabel();
		bool isByRef = expectType.IsByRef;
		VariableDefinition variableDefinition = null;
		if (isByRef)
		{
			expectType = expectType.GetElementType() ?? expectType;
			ReturnTypeClass rtc = ClassifyType(expectType);
			if (!expectType.IsValueType)
			{
				variableDefinition = new VariableDefinition(il.Module.TypeSystem.Object);
				il.Context.Body.Variables.Add(variableDefinition);
				il.Emit(OpCodes.Stloc, variableDefinition);
				il.Emit(OpCodes.Ldloc, variableDefinition);
			}
			EmitCheckByref(il, rtc, expectType, badArgLbl, argId);
			if (expectType.IsValueType)
			{
				return;
			}
			if (variableDefinition != null)
			{
				il.Emit(OpCodes.Ldloc, variableDefinition);
			}
			EmitLoadByref(il, rtc, expectType);
			il.Emit(OpCodes.Ldind_Ref);
		}
		if (expectType == typeof(object))
		{
			il.Emit(OpCodes.Pop);
			return;
		}
		if (!expectType.IsValueType || (object)Nullable.GetUnderlyingType(expectType) != null)
		{
			ILLabel iLLabel2 = il.DefineLabel();
			VariableDefinition variableDefinition2 = new VariableDefinition(il.Module.TypeSystem.Object);
			il.Context.Body.Variables.Add(variableDefinition2);
			il.Emit(OpCodes.Stloc, variableDefinition2);
			il.Emit(OpCodes.Ldloc, variableDefinition2);
			il.Emit(OpCodes.Brtrue, iLLabel2);
			il.Emit(OpCodes.Br, iLLabel);
			il.MarkLabel(iLLabel2);
			il.Emit(OpCodes.Ldloc, variableDefinition2);
		}
		if (!expectType.IsValueType || (!isByRef && expectType.IsValueType))
		{
			EmitCheckType(il, argId, expectType, badArgLbl);
		}
		il.MarkLabel(iLLabel);
	}

	private static void EmitBadArgCall(ILCursor il, ILLabel badArgLbl)
	{
		il.MarkLabel(badArgLbl);
		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Ldarg_1);
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Call, BadArgExceptionMethod);
		il.Emit(OpCodes.Throw);
	}

	private static void EmitCheckByref(ILCursor il, ReturnTypeClass rtc, Type returnType, ILLabel badArgLbl, int argId = -2)
	{
		Type expectType;
		switch (rtc)
		{
		default:
			return;
		case ReturnTypeClass.ValueType:
			expectType = returnType;
			break;
		case ReturnTypeClass.Nullable:
			expectType = typeof(StrongBox<>).MakeGenericType(returnType);
			break;
		case ReturnTypeClass.ReferenceType:
			expectType = typeof(WeakBox);
			break;
		case ReturnTypeClass.Void:
			return;
		}
		EmitCheckType(il, argId, expectType, badArgLbl);
	}

	private static void EmitLoadByref(ILCursor il, ReturnTypeClass rtc, Type returnType)
	{
		switch (rtc)
		{
		case ReturnTypeClass.ValueType:
			il.Emit(OpCodes.Unbox, returnType);
			break;
		case ReturnTypeClass.Nullable:
		{
			FieldInfo field = typeof(StrongBox<>).MakeGenericType(returnType).GetField("Value");
			il.Emit(OpCodes.Ldflda, field);
			break;
		}
		case ReturnTypeClass.ReferenceType:
			il.Emit(OpCodes.Ldflda, WeakBoxValueField);
			break;
		case ReturnTypeClass.Void:
			break;
		}
	}

	private static void EmitLoadArgO(ILCursor il, int arg)
	{
		il.Emit(OpCodes.Ldarg_2);
		il.Emit(OpCodes.Ldc_I4, arg);
		il.Emit(OpCodes.Ldelem_Ref);
	}

	private static void EmitStoreByref(ILCursor il, ReturnTypeClass rtc, Type returnType)
	{
		if (rtc != 0)
		{
			if (returnType.IsValueType)
			{
				il.Emit(OpCodes.Stobj, returnType);
			}
			else
			{
				il.Emit(OpCodes.Stind_Ref);
			}
		}
	}

	private static FastStructInvoker CreateMethodInvoker(MethodBase method, out ReturnTypeClass retTypeClass, out Type retType)
	{
		if (!method.IsStatic)
		{
			Type declaringType = method.DeclaringType;
			if ((object)declaringType != null && declaringType.IsByRefLike())
			{
				throw new ArgumentException("Cannot create reflection invoker for instance method on byref-like type", "method");
			}
		}
		Type returnType = ((method is MethodInfo methodInfo) ? methodInfo.ReturnType : method.DeclaringType);
		retType = returnType;
		if (returnType.IsByRef || returnType.IsByRefLike())
		{
			throw new ArgumentException("Cannot create reflection invoker for method with byref or byref-like return type", "method");
		}
		retTypeClass = ClassifyType(returnType);
		ReturnTypeClass typeClass = retTypeClass;
		ParameterInfo[] methParams = method.GetParameters();
		FormatInterpolatedStringHandler handler = new FormatInterpolatedStringHandler(22, 1);
		handler.AppendLiteral("MM:FastStructInvoker<");
		handler.AppendFormatted(method);
		handler.AppendLiteral(">");
		using DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition(DebugFormatter.Format(ref handler), null, FastStructInvokerArgs);
		using ILContext iLContext = new ILContext(dynamicMethodDefinition.Definition);
		iLContext.Invoke(delegate(ILContext ilc)
		{
			ILCursor iLCursor = new ILCursor(ilc);
			EmitCheckArgs(iLCursor, method.IsStatic || method is ConstructorInfo, typeClass, methParams.Length);
			ILLabel badArgLbl = iLCursor.DefineLabel();
			if (!method.IsStatic && !(method is ConstructorInfo))
			{
				Type declaringType2 = method.DeclaringType;
				Helpers.Assert((object)declaringType2 != null, null, "expectType is not null");
				iLCursor.Emit(OpCodes.Ldarg_0);
				EmitCheckType(iLCursor, -1, declaringType2, badArgLbl);
			}
			if (typeClass != 0)
			{
				iLCursor.Emit(OpCodes.Ldarg_1);
				EmitCheckByref(iLCursor, typeClass, returnType, badArgLbl);
			}
			for (int i = 0; i < methParams.Length; i++)
			{
				Type parameterType = methParams[i].ParameterType;
				if (parameterType.IsByRefLike())
				{
					throw new ArgumentException("Cannot create reflection invoker for method with byref-like argument types", "method");
				}
				EmitLoadArgO(iLCursor, i);
				EmitCheckAllowNull(iLCursor, i, parameterType, badArgLbl);
			}
			if (typeClass != 0)
			{
				iLCursor.Emit(OpCodes.Ldarg_1);
				EmitLoadByref(iLCursor, typeClass, returnType);
			}
			if (!method.IsStatic && !(method is ConstructorInfo))
			{
				Type declaringType3 = method.DeclaringType;
				Helpers.Assert((object)declaringType3 != null, null, "declType is not null");
				iLCursor.Emit(OpCodes.Ldarg_0);
				if (declaringType3.IsValueType)
				{
					iLCursor.Emit(OpCodes.Unbox, declaringType3);
				}
			}
			for (int j = 0; j < methParams.Length; j++)
			{
				iLCursor.DefineLabel();
				Type parameterType2 = methParams[j].ParameterType;
				Type type = (parameterType2.IsByRef ? (parameterType2.GetElementType() ?? parameterType2) : parameterType2);
				EmitLoadArgO(iLCursor, j);
				if (parameterType2.IsByRef)
				{
					EmitLoadByref(iLCursor, ClassifyType(type), type);
				}
				else if (parameterType2.IsValueType)
				{
					iLCursor.Emit(OpCodes.Unbox_Any, type);
				}
			}
			if (method is ConstructorInfo method2)
			{
				iLCursor.Emit(OpCodes.Newobj, method2);
			}
			else if (method.IsVirtual)
			{
				iLCursor.Emit(OpCodes.Callvirt, method);
			}
			else
			{
				iLCursor.Emit(OpCodes.Call, method);
			}
			EmitStoreByref(iLCursor, typeClass, returnType);
			iLCursor.Emit(OpCodes.Ret);
			EmitBadArgCall(iLCursor, badArgLbl);
		});
		return dynamicMethodDefinition.Generate().CreateDelegate<FastStructInvoker>();
	}

	private static FastStructInvoker CreateFieldInvoker(FieldInfo field, out ReturnTypeClass retTypeClass, out Type retType)
	{
		if (!field.IsStatic)
		{
			Type declaringType = field.DeclaringType;
			if ((object)declaringType != null && declaringType.IsByRefLike())
			{
				throw new ArgumentException("Cannot create reflection invoker for instance field on byref-like type", "field");
			}
		}
		Type returnType = field.FieldType;
		retType = returnType;
		retTypeClass = ClassifyType(returnType);
		ReturnTypeClass typeClass = retTypeClass;
		FormatInterpolatedStringHandler handler = new FormatInterpolatedStringHandler(22, 1);
		handler.AppendLiteral("MM:FastStructInvoker<");
		handler.AppendFormatted(field);
		handler.AppendLiteral(">");
		using DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition(DebugFormatter.Format(ref handler), null, FastStructInvokerArgs);
		using ILContext iLContext = new ILContext(dynamicMethodDefinition.Definition);
		iLContext.Invoke(delegate(ILContext ilc)
		{
			ILCursor iLCursor = new ILCursor(ilc);
			EmitCheckArgs(iLCursor, field.IsStatic, typeClass, 0);
			ILLabel badArgLbl = iLCursor.DefineLabel();
			if (!field.IsStatic)
			{
				Type declaringType2 = field.DeclaringType;
				iLCursor.Emit(OpCodes.Ldarg_0);
				EmitCheckType(iLCursor, -1, declaringType2, badArgLbl);
			}
			iLCursor.Emit(OpCodes.Ldarg_1);
			EmitCheckByref(iLCursor, typeClass, returnType, badArgLbl);
			ILLabel iLLabel = iLCursor.DefineLabel();
			iLCursor.Emit(OpCodes.Ldarg_2);
			iLCursor.Emit(OpCodes.Brfalse, iLLabel);
			iLCursor.Emit(OpCodes.Ldarg_2);
			iLCursor.Emit(OpCodes.Ldlen);
			iLCursor.Emit(OpCodes.Ldc_I4_1);
			iLCursor.Emit(OpCodes.Blt, iLLabel);
			EmitLoadArgO(iLCursor, 0);
			EmitCheckAllowNull(iLCursor, 0, field.FieldType, badArgLbl);
			iLCursor.Emit(OpCodes.Ldarg_1);
			EmitLoadByref(iLCursor, typeClass, returnType);
			if (!field.IsStatic)
			{
				Type declaringType3 = field.DeclaringType;
				Helpers.Assert((object)declaringType3 != null, null, "declType is not null");
				iLCursor.Emit(OpCodes.Ldarg_0);
				if (declaringType3.IsValueType)
				{
					iLCursor.Emit(OpCodes.Unbox, declaringType3);
				}
			}
			EmitLoadArgO(iLCursor, 0);
			iLCursor.Emit(OpCodes.Unbox_Any, field.FieldType);
			if (field.IsStatic)
			{
				iLCursor.Emit(OpCodes.Stsfld, field);
			}
			else
			{
				iLCursor.Emit(OpCodes.Stfld, field);
			}
			EmitLoadArgO(iLCursor, 0);
			iLCursor.Emit(OpCodes.Unbox_Any, field.FieldType);
			EmitStoreByref(iLCursor, typeClass, returnType);
			iLCursor.Emit(OpCodes.Ret);
			iLCursor.MarkLabel(iLLabel);
			iLCursor.Emit(OpCodes.Ldarg_1);
			EmitLoadByref(iLCursor, typeClass, returnType);
			if (!field.IsStatic)
			{
				Type declaringType4 = field.DeclaringType;
				Helpers.Assert((object)declaringType4 != null, null, "declType is not null");
				iLCursor.Emit(OpCodes.Ldarg_0);
				if (declaringType4.IsValueType)
				{
					iLCursor.Emit(OpCodes.Unbox, declaringType4);
				}
			}
			if (field.IsStatic)
			{
				iLCursor.Emit(OpCodes.Ldsfld, field);
			}
			else
			{
				iLCursor.Emit(OpCodes.Ldfld, field);
			}
			EmitStoreByref(iLCursor, typeClass, returnType);
			iLCursor.Emit(OpCodes.Ret);
			EmitBadArgCall(iLCursor, badArgLbl);
		});
		return dynamicMethodDefinition.Generate().CreateDelegate<FastStructInvoker>();
	}
}
