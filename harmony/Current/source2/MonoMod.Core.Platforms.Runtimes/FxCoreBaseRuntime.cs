using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Runtimes;

internal abstract class FxCoreBaseRuntime : IRuntime
{
	protected Abi? AbiCore;

	private static readonly Type? RTDynamicMethod = typeof(DynamicMethod).GetNestedType("RTDynamicMethod", BindingFlags.NonPublic);

	private static readonly FieldInfo? RTDynamicMethod_m_owner = RTDynamicMethod?.GetField("m_owner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly FieldInfo? _DynamicMethod_m_method = typeof(DynamicMethod).GetField("m_method", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo? _DynamicMethod_GetMethodDescriptor = typeof(DynamicMethod).GetMethod("GetMethodDescriptor", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo? _RuntimeMethodHandle_get_Value = typeof(RuntimeMethodHandle).GetMethod("get_Value", BindingFlags.Instance | BindingFlags.Public);

	private static readonly FieldInfo? _RuntimeMethodHandle_m_value = typeof(RuntimeMethodHandle).GetField("m_value", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo? _IRuntimeMethodInfo_get_Value = typeof(RuntimeMethodHandle).Assembly.GetType("System.IRuntimeMethodInfo")?.GetMethod("get_Value");

	private static readonly MethodInfo? _RuntimeHelpers__CompileMethod = typeof(RuntimeHelpers).GetMethod("_CompileMethod", BindingFlags.Static | BindingFlags.NonPublic) ?? typeof(RuntimeHelpers).GetMethod("CompileMethod", BindingFlags.Static | BindingFlags.NonPublic);

	private static readonly Type? RtH_CM_FirstArg;

	private static readonly bool _RuntimeHelpers__CompileMethod_TakesIntPtr;

	private static readonly bool _RuntimeHelpers__CompileMethod_TakesIRuntimeMethodInfo;

	private static readonly bool _RuntimeHelpers__CompileMethod_TakesRuntimeMethodHandleInternal;

	private Func<DynamicMethod, RuntimeMethodHandle>? lazyGetDmHandleHelper;

	private Action<RuntimeMethodHandle>? lazyBclCompileMethod;

	protected static readonly bool IsDebugClr;

	public abstract RuntimeKind Target { get; }

	public virtual RuntimeFeature Features => RuntimeFeature.PreciseGC | RuntimeFeature.GenericSharing | RuntimeFeature.DisableInlining | RuntimeFeature.RequiresMethodIdentification | RuntimeFeature.RequiresBodyThunkWalking | RuntimeFeature.HasKnownABI | RuntimeFeature.RequiresCustomMethodCompile;

	public Abi Abi => AbiCore ?? throw new PlatformNotSupportedException($"The runtime's Abi field is not set, and is unusable ({GetType()})");

	private Func<DynamicMethod, RuntimeMethodHandle> GetDMHandleHelper => lazyGetDmHandleHelper ?? (lazyGetDmHandleHelper = CreateGetDMHandleHelper());

	private static bool CanCreateGetDMHandleHelper => (object)_DynamicMethod_GetMethodDescriptor != null;

	private Action<RuntimeMethodHandle> BclCompileMethodHelper => lazyBclCompileMethod ?? (lazyBclCompileMethod = CreateBclCompileMethodHelper());

	private static bool CanCreateBclCompileMethodHelper
	{
		get
		{
			if ((object)_RuntimeHelpers__CompileMethod != null)
			{
				if (!_RuntimeHelpers__CompileMethod_TakesIntPtr)
				{
					if ((object)_RuntimeMethodHandle_m_value != null)
					{
						if (!_RuntimeHelpers__CompileMethod_TakesIRuntimeMethodInfo)
						{
							if ((object)_IRuntimeMethodInfo_get_Value != null)
							{
								return _RuntimeHelpers__CompileMethod_TakesRuntimeMethodHandleInternal;
							}
							return false;
						}
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public event OnMethodCompiledCallback? OnMethodCompiled;

	private static TypeClassification ClassifyRyuJitX86(Type type, bool isReturn)
	{
		while (!type.IsPrimitive || type.IsEnum)
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (fields == null || fields.Length != 1)
			{
				break;
			}
			type = fields[0].FieldType;
		}
		TypeCode typeCode = Type.GetTypeCode(type);
		bool flag = ((typeCode == TypeCode.Boolean || (uint)(typeCode - 5) <= 5u) ? true : false);
		if (flag || type == typeof(IntPtr) || type == typeof(UIntPtr))
		{
			return TypeClassification.InRegister;
		}
		flag = isReturn;
		if (flag)
		{
			bool flag2 = (uint)(typeCode - 11) <= 1u;
			flag = flag2;
		}
		if (flag)
		{
			return TypeClassification.InRegister;
		}
		if (isReturn)
		{
			return TypeClassification.ByReference;
		}
		return TypeClassification.OnStack;
	}

	protected FxCoreBaseRuntime()
	{
		if (PlatformDetection.Architecture == ArchitectureKind.x86)
		{
			AbiCore = new Abi(new SpecialArgumentKind[4]
			{
				SpecialArgumentKind.ThisPointer,
				SpecialArgumentKind.ReturnBuffer,
				SpecialArgumentKind.UserArguments,
				SpecialArgumentKind.GenericContext
			}, ClassifyRyuJitX86, ReturnsReturnBuffer: true);
		}
	}

	protected static Abi AbiForCoreFx45X64(Abi baseAbi)
	{
		return baseAbi with
		{
			ArgumentOrder = new SpecialArgumentKind[4]
			{
				SpecialArgumentKind.ThisPointer,
				SpecialArgumentKind.ReturnBuffer,
				SpecialArgumentKind.GenericContext,
				SpecialArgumentKind.UserArguments
			}
		};
	}

	public virtual MethodBase GetIdentifiable(MethodBase method)
	{
		if (RTDynamicMethod_m_owner != null && method.GetType() == RTDynamicMethod)
		{
			return (MethodBase)RTDynamicMethod_m_owner.GetValue(method);
		}
		return method;
	}

	public virtual RuntimeMethodHandle GetMethodHandle(MethodBase method)
	{
		if (method is DynamicMethod dynamicMethod)
		{
			if (TryGetDMHandle(dynamicMethod, out var handle) && TryInvokeBclCompileMethod(handle))
			{
				return handle;
			}
			try
			{
				dynamicMethod.CreateDelegate(typeof(MulticastDelegate));
			}
			catch
			{
			}
			if (TryGetDMHandle(dynamicMethod, out handle))
			{
				return handle;
			}
			if (_DynamicMethod_m_method != null)
			{
				return (RuntimeMethodHandle)_DynamicMethod_m_method.GetValue(method);
			}
		}
		return method.MethodHandle;
	}

	private static Func<DynamicMethod, RuntimeMethodHandle> CreateGetDMHandleHelper()
	{
		Helpers.Assert(CanCreateGetDMHandleHelper, null, "CanCreateGetDMHandleHelper");
		using DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("get DynamicMethod RuntimeMethodHandle", typeof(RuntimeMethodHandle), new Type[1] { typeof(DynamicMethod) });
		_ = dynamicMethodDefinition.Module;
		ILProcessor iLProcessor = dynamicMethodDefinition.GetILProcessor();
		Helpers.Assert((object)_DynamicMethod_GetMethodDescriptor != null, null, "_DynamicMethod_GetMethodDescriptor is not null");
		iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
		iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Call, _DynamicMethod_GetMethodDescriptor);
		iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ret);
		return dynamicMethodDefinition.Generate().CreateDelegate<Func<DynamicMethod, RuntimeMethodHandle>>();
	}

	private static Action<RuntimeMethodHandle> CreateBclCompileMethodHelper()
	{
		Helpers.Assert(CanCreateBclCompileMethodHelper, null, "CanCreateBclCompileMethodHelper");
		using DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("invoke RuntimeHelpers.CompileMethod", null, new Type[1] { typeof(RuntimeMethodHandle) });
		ModuleDefinition module = dynamicMethodDefinition.Module;
		ILProcessor iLProcessor = dynamicMethodDefinition.GetILProcessor();
		iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ldarga_S, (byte)0);
		if (_RuntimeHelpers__CompileMethod_TakesIntPtr)
		{
			iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Call, module.ImportReference(_RuntimeMethodHandle_get_Value));
			iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Call, module.ImportReference(_RuntimeHelpers__CompileMethod));
			iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ret);
			return dynamicMethodDefinition.Generate().CreateDelegate<Action<RuntimeMethodHandle>>();
		}
		Helpers.Assert((object)_RuntimeMethodHandle_m_value != null, null, "_RuntimeMethodHandle_m_value is not null");
		iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ldfld, module.ImportReference(_RuntimeMethodHandle_m_value));
		if (_RuntimeHelpers__CompileMethod_TakesIRuntimeMethodInfo)
		{
			iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Call, module.ImportReference(_RuntimeHelpers__CompileMethod));
			iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ret);
			return dynamicMethodDefinition.Generate().CreateDelegate<Action<RuntimeMethodHandle>>();
		}
		Helpers.Assert((object)_IRuntimeMethodInfo_get_Value != null, null, "_IRuntimeMethodInfo_get_Value is not null");
		iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Callvirt, module.ImportReference(_IRuntimeMethodInfo_get_Value));
		if (_RuntimeHelpers__CompileMethod_TakesRuntimeMethodHandleInternal)
		{
			iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Call, module.ImportReference(_RuntimeHelpers__CompileMethod));
			iLProcessor.Emit(Mono.Cecil.Cil.OpCodes.Ret);
			return dynamicMethodDefinition.Generate().CreateDelegate<Action<RuntimeMethodHandle>>();
		}
		Helpers.Assert(value: false, "Tried to generate BCL CompileMethod helper when it's not possible? (This should never happen if CanCreateBclCompileMethodHelper is correct)", "false");
		throw new InvalidOperationException("UNREACHABLE");
	}

	private bool TryGetDMHandle(DynamicMethod dm, out RuntimeMethodHandle handle)
	{
		if (CanCreateGetDMHandleHelper)
		{
			handle = GetDMHandleHelper(dm);
			return true;
		}
		return TryGetDMHandleRefl(dm, out handle);
	}

	protected bool TryInvokeBclCompileMethod(RuntimeMethodHandle handle)
	{
		if (CanCreateBclCompileMethodHelper)
		{
			BclCompileMethodHelper(handle);
			return true;
		}
		return TryInvokeBclCompileMethodRefl(handle);
	}

	private static bool TryGetDMHandleRefl(DynamicMethod dm, out RuntimeMethodHandle handle)
	{
		handle = default(RuntimeMethodHandle);
		if ((object)_DynamicMethod_GetMethodDescriptor == null)
		{
			return false;
		}
		handle = (RuntimeMethodHandle)_DynamicMethod_GetMethodDescriptor.Invoke(dm, null);
		return true;
	}

	private static bool TryInvokeBclCompileMethodRefl(RuntimeMethodHandle handle)
	{
		if ((object)_RuntimeHelpers__CompileMethod == null)
		{
			return false;
		}
		if (_RuntimeHelpers__CompileMethod_TakesIntPtr)
		{
			_RuntimeHelpers__CompileMethod.Invoke(null, new object[1] { handle.Value });
			return true;
		}
		if ((object)_RuntimeMethodHandle_m_value == null)
		{
			return false;
		}
		object value = _RuntimeMethodHandle_m_value.GetValue(handle);
		if (_RuntimeHelpers__CompileMethod_TakesIRuntimeMethodInfo)
		{
			_RuntimeHelpers__CompileMethod.Invoke(null, new object[1] { value });
			return true;
		}
		if ((object)_IRuntimeMethodInfo_get_Value == null)
		{
			return false;
		}
		object obj = _IRuntimeMethodInfo_get_Value.Invoke(value, null);
		if (_RuntimeHelpers__CompileMethod_TakesRuntimeMethodHandleInternal)
		{
			_RuntimeHelpers__CompileMethod.Invoke(null, new object[1] { obj });
			return true;
		}
		bool isEnabled;
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(81, 1, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("Could not compile DynamicMethod using BCL reflection (_CompileMethod first arg: ");
			message.AppendFormatted(RtH_CM_FirstArg);
			message.AppendLiteral(")");
		}
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
		return false;
	}

	public virtual void Compile(MethodBase method)
	{
		RuntimeMethodHandle handle = GetMethodHandle(method);
		RuntimeHelpers.PrepareMethod(handle);
		Helpers.Assert(TryInvokeBclCompileMethod(handle), null, "TryInvokeBclCompileMethod(handle)");
		if (!method.IsVirtual)
		{
			return;
		}
		Type declaringType = method.DeclaringType;
		if ((object)declaringType == null || !declaringType.IsValueType)
		{
			return;
		}
		if (TryGetCanonicalMethodHandle(ref handle))
		{
			Helpers.Assert(TryInvokeBclCompileMethod(handle), null, "TryInvokeBclCompileMethod(handle)");
			return;
		}
		try
		{
			method.CreateDelegate<Action>();
		}
		catch (Exception value)
		{
			bool isEnabled;
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogSpamStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogSpamStringHandler(91, 1, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("Caught exception while attempting to compile real entry point of virtual method on struct: ");
				message.AppendFormatted(value);
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Spam(ref message);
		}
	}

	protected virtual bool TryGetCanonicalMethodHandle(ref RuntimeMethodHandle handle)
	{
		return false;
	}

	public virtual IDisposable? PinMethodIfNeeded(MethodBase method)
	{
		return null;
	}

	public unsafe virtual void DisableInlining(MethodBase method)
	{
		RuntimeMethodHandle methodHandle = GetMethodHandle(method);
		int num = (IsDebugClr ? (IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size) : 0) + 2 + 1 + 1 + 2;
		ushort* ptr = (ushort*)((byte*)(void*)methodHandle.Value + num);
		*ptr |= 0x2000;
	}

	public virtual IntPtr GetMethodEntryPoint(MethodBase method)
	{
		method = GetIdentifiable(method);
		if (method.IsVirtual)
		{
			Type declaringType = method.DeclaringType;
			if ((object)declaringType != null && declaringType.IsValueType)
			{
				return method.GetLdftnPointer();
			}
		}
		return GetMethodHandle(method).GetFunctionPointer();
	}

	protected virtual void OnMethodCompiledCore(RuntimeTypeHandle declaringType, RuntimeMethodHandle methodHandle, ReadOnlyMemory<RuntimeTypeHandle>? genericTypeArguments, ReadOnlyMemory<RuntimeTypeHandle>? genericMethodArguments, IntPtr methodBodyStart, IntPtr methodBodyRw, ulong methodBodySize)
	{
		bool isEnabled;
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler message2;
		try
		{
			Type type = Type.GetTypeFromHandle(declaringType);
			if (genericTypeArguments.HasValue)
			{
				ReadOnlyMemory<RuntimeTypeHandle> valueOrDefault = genericTypeArguments.GetValueOrDefault();
				if (type.IsGenericTypeDefinition)
				{
					Type[] array = new Type[valueOrDefault.Length];
					for (int i = 0; i < valueOrDefault.Length; i++)
					{
						array[i] = Type.GetTypeFromHandle(valueOrDefault.Span[i]);
					}
					type = type.MakeGenericType(array);
				}
			}
			MethodBase methodBase = MethodBase.GetMethodFromHandle(methodHandle, type.TypeHandle);
			if ((object)methodBase == null)
			{
				MethodInfo[] methods = type.GetMethods((BindingFlags)(-1));
				foreach (MethodInfo methodInfo in methods)
				{
					if (methodInfo.MethodHandle.Value == methodHandle.Value)
					{
						methodBase = methodInfo;
						break;
					}
				}
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogSpamStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogSpamStringHandler(28, 3, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("JIT compiled ");
				message.AppendFormatted(methodBase);
				message.AppendLiteral(" to 0x");
				message.AppendFormatted(methodBodyStart, "x16");
				message.AppendLiteral(" (rw: 0x");
				message.AppendFormatted(methodBodyRw, "x16");
				message.AppendLiteral(")");
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Spam(ref message);
			try
			{
				this.OnMethodCompiled?.Invoke(methodHandle, methodBase, methodBodyStart, methodBodyRw, methodBodySize);
			}
			catch (Exception value)
			{
				message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(40, 1, out isEnabled);
				if (isEnabled)
				{
					message2.AppendLiteral("Error executing OnMethodCompiled event: ");
					message2.AppendFormatted(value);
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message2);
			}
		}
		catch (Exception value2)
		{
			message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(31, 1, out isEnabled);
			if (isEnabled)
			{
				message2.AppendLiteral("Error in OnMethodCompiledCore: ");
				message2.AppendFormatted(value2);
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message2);
		}
	}

	static FxCoreBaseRuntime()
	{
		MethodInfo? runtimeHelpers__CompileMethod = _RuntimeHelpers__CompileMethod;
		RtH_CM_FirstArg = (((object)runtimeHelpers__CompileMethod != null) ? runtimeHelpers__CompileMethod.GetParameters()[0].ParameterType : null);
		_RuntimeHelpers__CompileMethod_TakesIntPtr = RtH_CM_FirstArg?.FullName == "System.IntPtr";
		_RuntimeHelpers__CompileMethod_TakesIRuntimeMethodInfo = RtH_CM_FirstArg?.FullName == "System.IRuntimeMethodInfo";
		_RuntimeHelpers__CompileMethod_TakesRuntimeMethodHandleInternal = RtH_CM_FirstArg?.FullName == "System.RuntimeMethodHandleInternal";
		IsDebugClr = Switches.TryGetSwitchEnabled("DebugClr", out var isEnabled) && isEnabled;
	}
}
