using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Core.Platforms.Architectures;
using MonoMod.Core.Platforms.Runtimes;
using MonoMod.Core.Platforms.Systems;
using MonoMod.Core.Utils;
using MonoMod.Logs;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms;

internal sealed class PlatformTriple
{
	public record struct NativeDetour(SimpleNativeDetour Simple, IntPtr AltEntry, IDisposable? AltHandle)
	{
		public bool HasAltEntry => AltEntry != IntPtr.Zero;
	}

	private static object lazyCurrentLock = new object();

	private static PlatformTriple? lazyCurrent;

	private IntPtr ThePreStub = IntPtr.Zero;

	public IArchitecture Architecture { get; }

	public ISystem System { get; }

	public IRuntime Runtime { get; }

	public unsafe static PlatformTriple Current => Helpers.GetOrInitWithLock(ref lazyCurrent, lazyCurrentLock, (delegate*<PlatformTriple>)(&CreateCurrent));

	public (ArchitectureKind Arch, OSKind OS, RuntimeKind Runtime) HostTriple => (Arch: Architecture.Target, OS: System.Target, Runtime: Runtime.Target);

	public FeatureFlags SupportedFeatures { get; }

	public Abi Abi { get; }

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static IRuntime CreateCurrentRuntime(ISystem system, IArchitecture arch)
	{
		Helpers.ThrowIfArgumentNull(system, "system");
		Helpers.ThrowIfArgumentNull(arch, "arch");
		RuntimeKind runtime = PlatformDetection.Runtime;
		return runtime switch
		{
			RuntimeKind.Framework => FxBaseRuntime.CreateForVersion(PlatformDetection.RuntimeVersion, system), 
			RuntimeKind.CoreCLR => CoreBaseRuntime.CreateForVersion(PlatformDetection.RuntimeVersion, system, arch), 
			RuntimeKind.Mono => new MonoRuntime(system), 
			_ => throw new PlatformNotSupportedException($"Runtime kind {runtime} not supported"), 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static IArchitecture CreateCurrentArchitecture(ISystem system)
	{
		Helpers.ThrowIfArgumentNull(system, "system");
		ArchitectureKind architecture = PlatformDetection.Architecture;
		return architecture switch
		{
			ArchitectureKind.x86 => new x86Arch(system), 
			ArchitectureKind.x86_64 => new x86_64Arch(system), 
			ArchitectureKind.Arm => throw new NotImplementedException(), 
			ArchitectureKind.Arm64 => throw new NotImplementedException(), 
			_ => throw new PlatformNotSupportedException($"Architecture kind {architecture} not supported"), 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static ISystem CreateCurrentSystem()
	{
		OSKind oS = PlatformDetection.OS;
		switch (oS)
		{
		case OSKind.Posix:
			throw new NotImplementedException();
		case OSKind.Linux:
			return new LinuxSystem();
		case OSKind.Android:
			throw new NotImplementedException();
		case OSKind.OSX:
			return new MacOSSystem();
		case OSKind.IOS:
			throw new NotImplementedException();
		case OSKind.BSD:
			throw new NotImplementedException();
		case OSKind.Windows:
		case OSKind.Wine:
			return new WindowsSystem();
		default:
			throw new PlatformNotSupportedException($"OS kind {oS} not supported");
		}
	}

	private static PlatformTriple CreateCurrent()
	{
		ISystem system = CreateCurrentSystem();
		IArchitecture architecture = CreateCurrentArchitecture(system);
		IRuntime runtime = CreateCurrentRuntime(system, architecture);
		return new PlatformTriple(architecture, system, runtime);
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static void SetPlatformTriple(PlatformTriple triple)
	{
		Helpers.ThrowIfArgumentNull(triple, "triple");
		if (lazyCurrent == null)
		{
			ThrowTripleAlreadyExists();
		}
		lock (lazyCurrentLock)
		{
			if (lazyCurrent == null)
			{
				ThrowTripleAlreadyExists();
			}
			lazyCurrent = triple;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ThrowTripleAlreadyExists()
	{
		throw new InvalidOperationException("The platform triple has already been initialized; cannot set a new one");
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public PlatformTriple(IArchitecture architecture, ISystem system, IRuntime runtime)
	{
		Helpers.ThrowIfArgumentNull(architecture, "architecture");
		Helpers.ThrowIfArgumentNull(system, "system");
		Helpers.ThrowIfArgumentNull(runtime, "runtime");
		Architecture = architecture;
		System = system;
		Runtime = runtime;
		SupportedFeatures = new FeatureFlags(Architecture.Features, System.Features, Runtime.Features);
		InitIfNeeded(Architecture);
		InitIfNeeded(System);
		InitIfNeeded(Runtime);
		Abi = Runtime.Abi;
	}

	private void InitIfNeeded(object obj)
	{
		(obj as IInitialize<ISystem>)?.Initialize(System);
		(obj as IInitialize<IArchitecture>)?.Initialize(Architecture);
		(obj as IInitialize<IRuntime>)?.Initialize(Runtime);
		(obj as IInitialize<PlatformTriple>)?.Initialize(this);
		(obj as IInitialize)?.Initialize();
	}

	public void Compile(MethodBase method)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		if (method.IsGenericMethodDefinition)
		{
			throw new ArgumentException("Cannot prepare generic method definition", "method");
		}
		method = GetIdentifiable(method);
		if (SupportedFeatures.Has(RuntimeFeature.RequiresCustomMethodCompile))
		{
			Runtime.Compile(method);
			return;
		}
		RuntimeMethodHandle methodHandle = Runtime.GetMethodHandle(method);
		if (method.IsGenericMethod)
		{
			Type[] genericArguments = method.GetGenericArguments();
			RuntimeTypeHandle[] array = new RuntimeTypeHandle[genericArguments.Length];
			for (int i = 0; i < genericArguments.Length; i++)
			{
				array[i] = genericArguments[i].TypeHandle;
			}
			RuntimeHelpers.PrepareMethod(methodHandle, array);
		}
		else
		{
			RuntimeHelpers.PrepareMethod(methodHandle);
		}
	}

	public MethodBase GetIdentifiable(MethodBase method)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		if (SupportedFeatures.Has(RuntimeFeature.RequiresMethodIdentification))
		{
			method = Runtime.GetIdentifiable(method);
		}
		if (method.ReflectedType != method.DeclaringType)
		{
			ParameterInfo[] parameters = method.GetParameters();
			Type[] array = new Type[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				array[i] = parameters[i].ParameterType;
			}
			bool isEnabled;
			AssertionInterpolatedStringHandler message;
			bool isEnabled2;
			if ((object)method.DeclaringType == null)
			{
				MethodInfo method2 = method.Module.GetMethod(method.Name, (BindingFlags)(-1), null, method.CallingConvention, array, null);
				isEnabled = (object)method2 != null;
				bool value = isEnabled;
				message = new AssertionInterpolatedStringHandler(16, 2, isEnabled, out isEnabled2);
				if (isEnabled2)
				{
					message.AppendLiteral("orig: ");
					message.AppendFormatted(method);
					message.AppendLiteral(", module: ");
					message.AppendFormatted(method.Module);
				}
				Helpers.Assert(value, ref message, "got is not null");
				method = method2;
			}
			else if (method.IsConstructor)
			{
				ConstructorInfo constructor = method.DeclaringType.GetConstructor((BindingFlags)(-1), null, method.CallingConvention, array, null);
				isEnabled2 = (object)constructor != null;
				bool value2 = isEnabled2;
				message = new AssertionInterpolatedStringHandler(6, 1, isEnabled2, out isEnabled);
				if (isEnabled)
				{
					message.AppendLiteral("orig: ");
					message.AppendFormatted(method);
				}
				Helpers.Assert(value2, ref message, "got is not null");
				method = constructor;
			}
			else
			{
				MethodInfo method3 = method.DeclaringType.GetMethod(method.Name, (BindingFlags)(-1), null, method.CallingConvention, array, null);
				isEnabled = (object)method3 != null;
				bool value3 = isEnabled;
				message = new AssertionInterpolatedStringHandler(6, 1, isEnabled, out isEnabled2);
				if (isEnabled2)
				{
					message.AppendLiteral("orig: ");
					message.AppendFormatted(method);
				}
				Helpers.Assert(value3, ref message, "got is not null");
				method = method3;
			}
		}
		return method;
	}

	public IDisposable? PinMethodIfNeeded(MethodBase method)
	{
		if (SupportedFeatures.Has(RuntimeFeature.RequiresMethodPinning))
		{
			return Runtime.PinMethodIfNeeded(method);
		}
		return null;
	}

	public bool TryDisableInlining(MethodBase method)
	{
		if (SupportedFeatures.Has(RuntimeFeature.DisableInlining))
		{
			Runtime.DisableInlining(method);
			return true;
		}
		return false;
	}

	public SimpleNativeDetour CreateSimpleDetour(IntPtr from, IntPtr to, int detourMaxSize = -1, IntPtr fromRw = default(IntPtr))
	{
		if (fromRw == (IntPtr)0)
		{
			fromRw = from;
		}
		bool flag = from != to;
		bool isEnabled;
		AssertionInterpolatedStringHandler message = new AssertionInterpolatedStringHandler(48, 2, flag, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("Cannot detour a method to itself! (from: ");
			message.AppendFormatted(from);
			message.AppendLiteral(", to: ");
			message.AppendFormatted(to);
			message.AppendLiteral(")");
		}
		Helpers.Assert(flag, ref message, "from != to");
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(31, 2, out isEnabled);
		if (isEnabled)
		{
			message2.AppendLiteral("Creating simple detour 0x");
			message2.AppendFormatted(from, "x16");
			message2.AppendLiteral(" => 0x");
			message2.AppendFormatted(to, "x16");
		}
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message2);
		NativeDetourInfo nativeDetourInfo = Architecture.ComputeDetourInfo(from, to, detourMaxSize);
		Span<byte> span = stackalloc byte[nativeDetourInfo.Size];
		Architecture.GetDetourBytes(nativeDetourInfo, span, out IDisposable allocationHandle);
		byte[] array = new byte[nativeDetourInfo.Size];
		System.PatchData(PatchTargetKind.Executable, fromRw, span, array);
		return new SimpleNativeDetour(this, nativeDetourInfo, array, allocationHandle);
	}

	public NativeDetour CreateNativeDetour(IntPtr from, IntPtr to, int detourMaxSize = -1, IntPtr fromRw = default(IntPtr))
	{
		if (fromRw == (IntPtr)0)
		{
			fromRw = from;
		}
		bool flag = from != to;
		bool isEnabled;
		AssertionInterpolatedStringHandler message = new AssertionInterpolatedStringHandler(48, 2, flag, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("Cannot detour a method to itself! (from: ");
			message.AppendFormatted(from);
			message.AppendLiteral(", to: ");
			message.AppendFormatted(to);
			message.AppendLiteral(")");
		}
		Helpers.Assert(flag, ref message, "from != to");
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(31, 2, out isEnabled);
		if (isEnabled)
		{
			message2.AppendLiteral("Creating simple detour 0x");
			message2.AppendFormatted(from, "x16");
			message2.AppendLiteral(" => 0x");
			message2.AppendFormatted(to, "x16");
		}
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message2);
		NativeDetourInfo nativeDetourInfo = Architecture.ComputeDetourInfo(from, to, detourMaxSize);
		Span<byte> span = stackalloc byte[nativeDetourInfo.Size];
		IDisposable allocationHandle;
		int detourBytes = Architecture.GetDetourBytes(nativeDetourInfo, span, out allocationHandle);
		IntPtr altEntry = IntPtr.Zero;
		IDisposable handle = null;
		if (SupportedFeatures.Has(ArchitectureFeature.CreateAltEntryPoint))
		{
			altEntry = Architecture.AltEntryFactory.CreateAlternateEntrypoint(from, detourBytes, out handle);
		}
		else
		{
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler message3 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler(67, 2, out isEnabled);
			if (isEnabled)
			{
				message3.AppendLiteral("Cannot create alternate entry point for native detour (from: ");
				message3.AppendFormatted(from, "x16");
				message3.AppendLiteral(", to: ");
				message3.AppendFormatted(to, "x16");
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Warning(ref message3);
		}
		byte[] array = new byte[nativeDetourInfo.Size];
		System.PatchData(PatchTargetKind.Executable, fromRw, span, array);
		return new NativeDetour(new SimpleNativeDetour(this, nativeDetourInfo, array, allocationHandle), altEntry, handle);
	}

	public IntPtr GetNativeMethodBody(MethodBase method)
	{
		if (SupportedFeatures.Has(RuntimeFeature.RequiresBodyThunkWalking))
		{
			return GetNativeMethodBodyWalk(method, reloadPtr: true);
		}
		return GetNativeMethodBodyDirect(method);
	}

	private unsafe IntPtr GetNativeMethodBodyWalk(MethodBase method, bool reloadPtr)
	{
		bool flag = false;
		bool flag2 = false;
		int value = 0;
		BytePatternCollection knownMethodThunks = Architecture.KnownMethodThunks;
		bool isEnabled;
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(32, 1, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("Performing method body walk for ");
			message.AppendFormatted(method);
		}
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message);
		nint num = -1;
		while (true)
		{
			nint num2 = Runtime.GetMethodEntryPoint(method);
			message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(25, 1, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("Starting entry point = 0x");
				message.AppendFormatted(num2, "x16");
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message);
			while (true)
			{
				if (value++ > 20)
				{
					_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler message2 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(70, 4, out isEnabled);
					if (isEnabled)
					{
						message2.AppendLiteral("Could not get entry point for ");
						message2.AppendFormatted(method);
						message2.AppendLiteral("! (tried ");
						message2.AppendFormatted(value);
						message2.AppendLiteral(" times) entry: 0x");
						message2.AppendFormatted(num2, "x16");
						message2.AppendLiteral(" prevEntry: 0x");
						message2.AppendFormatted(num, "x16");
					}
					_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message2);
					FormatInterpolatedStringHandler handler = new FormatInterpolatedStringHandler(47, 1);
					handler.AppendLiteral("Could not get entrypoint for ");
					handler.AppendFormatted(method);
					handler.AppendLiteral(" (stuck in a loop)");
					throw new NotSupportedException(DebugFormatter.Format(ref handler));
				}
				if (flag2 || num != num2)
				{
					num = num2;
					nint sizeOfReadableMemory = System.GetSizeOfReadableMemory(num2, knownMethodThunks.MaxMinLength);
					if (sizeOfReadableMemory <= 0)
					{
						_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler message3 = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler(43, 2, out isEnabled);
						if (isEnabled)
						{
							message3.AppendLiteral("Got zero or negative readable length ");
							message3.AppendFormatted(sizeOfReadableMemory);
							message3.AppendLiteral(" at 0x");
							message3.AppendFormatted(num2, "x16");
						}
						_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Warning(ref message3);
					}
					ReadOnlySpan<byte> data = new ReadOnlySpan<byte>((void*)num2, Math.Min((int)sizeOfReadableMemory, knownMethodThunks.MaxMinLength));
					if (knownMethodThunks.TryFindMatch(data, out ulong address, out BytePattern matchingPattern, out int offset, out int _))
					{
						IntPtr ptrGot = num2;
						flag2 = false;
						AddressMeaning addressMeaning = matchingPattern.AddressMeaning;
						message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(46, 4, out isEnabled);
						if (isEnabled)
						{
							message.AppendLiteral("Matched thunk with ");
							message.AppendFormatted(addressMeaning);
							message.AppendLiteral(" at 0x");
							message.AppendFormatted(num2, "x16");
							message.AppendLiteral(" (addr: 0x");
							message.AppendFormatted(address, "x8");
							message.AppendLiteral(", offset: ");
							message.AppendFormatted(offset);
							message.AppendLiteral(")");
						}
						_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message);
						if (addressMeaning.Kind.IsPrecodeFixup() && !flag)
						{
							nint num3 = addressMeaning.ProcessAddress(num2, offset, address);
							if (reloadPtr)
							{
								message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(56, 1, out isEnabled);
								if (isEnabled)
								{
									message.AppendLiteral("Method thunk reset; regenerating (PrecodeFixupThunk: 0x");
									message.AppendFormatted(num3, "X16");
									message.AppendLiteral(")");
								}
								_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message);
								Compile(method);
								flag2 = true;
								break;
							}
							num2 = num3;
						}
						else
						{
							num2 = addressMeaning.ProcessAddress(num2, offset, address);
						}
						message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(23, 1, out isEnabled);
						if (isEnabled)
						{
							message.AppendLiteral("Got next entry point 0x");
							message.AppendFormatted(num2, "x16");
						}
						_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message);
						num2 = NotThePreStub(ptrGot, num2, out var wasPreStub);
						if (wasPreStub && reloadPtr)
						{
							_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace("Matched ThePreStub");
							Compile(method);
							break;
						}
						continue;
					}
				}
				return num2;
			}
		}
	}

	private IntPtr GetNativeMethodBodyDirect(MethodBase method)
	{
		return Runtime.GetMethodEntryPoint(method);
	}

	private IntPtr NotThePreStub(IntPtr ptrGot, IntPtr ptrParsed, out bool wasPreStub)
	{
		if (ThePreStub == IntPtr.Zero)
		{
			ThePreStub = (IntPtr)(-2);
			IntPtr thePreStub = (from m in typeof(HttpWebRequest).Assembly.GetType("System.Net.Connection")?.GetMethods()
				group m by GetNativeMethodBodyWalk(m, reloadPtr: false)).First((IGrouping<IntPtr, MethodInfo> g) => g.Count() > 1).Key ?? ((IntPtr)(-1));
			ThePreStub = thePreStub;
			bool isEnabled;
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(14, 1, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("ThePreStub: 0x");
				message.AppendFormatted(ThePreStub, "X16");
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message);
		}
		wasPreStub = ptrParsed == ThePreStub;
		if (!wasPreStub)
		{
			return ptrParsed;
		}
		return ptrGot;
	}

	public MethodBase GetRealDetourTarget(MethodBase from, MethodBase to)
	{
		Helpers.ThrowIfArgumentNull(from, "from");
		Helpers.ThrowIfArgumentNull(to, "to");
		to = GetIdentifiable(to);
		if (from is MethodInfo methodInfo && to is MethodInfo && !methodInfo.IsStatic && to.IsStatic)
		{
			Type returnType = methodInfo.ReturnType;
			if (Abi.Classify(returnType, isReturn: true) == TypeClassification.ByReference)
			{
				Type thisParamType = from.GetThisParamType();
				Type type = returnType.MakeByRefType();
				Type returnType2 = (Abi.ReturnsReturnBuffer ? type : typeof(void));
				int value = -1;
				int value2 = -1;
				int num = -1;
				ParameterInfo[] parameters = from.GetParameters();
				List<Type> list = new List<Type>();
				ReadOnlySpan<SpecialArgumentKind> span = Abi.ArgumentOrder.Span;
				for (int i = 0; i < span.Length; i++)
				{
					switch (span[i])
					{
					case SpecialArgumentKind.ThisPointer:
						value = list.Count;
						list.Add(thisParamType);
						break;
					case SpecialArgumentKind.ReturnBuffer:
						value2 = list.Count;
						list.Add(type);
						break;
					case SpecialArgumentKind.UserArguments:
						num = list.Count;
						list.AddRange(parameters.Select((ParameterInfo p) => p.ParameterType));
						break;
					}
				}
				FormatInterpolatedStringHandler handler = new FormatInterpolatedStringHandler(16, 2);
				handler.AppendLiteral("Glue:AbiFixup<");
				handler.AppendFormatted(from);
				handler.AppendLiteral(",");
				handler.AppendFormatted(to);
				handler.AppendLiteral(">");
				using DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition(DebugFormatter.Format(ref handler), returnType2, list.ToArray());
				dynamicMethodDefinition.Definition.ImplAttributes |= Mono.Cecil.MethodImplAttributes.NoInlining | Mono.Cecil.MethodImplAttributes.AggressiveOptimization;
				ILProcessor iLProcessor = dynamicMethodDefinition.GetILProcessor();
				iLProcessor.Emit(OpCodes.Ldarg, value2);
				iLProcessor.Emit(OpCodes.Ldarg, value);
				for (int j = 0; j < parameters.Length; j++)
				{
					iLProcessor.Emit(OpCodes.Ldarg, j + num);
				}
				iLProcessor.Emit(OpCodes.Call, iLProcessor.Body.Method.Module.ImportReference(to));
				iLProcessor.Emit(OpCodes.Stobj, iLProcessor.Body.Method.Module.ImportReference(returnType));
				if (Abi.ReturnsReturnBuffer)
				{
					iLProcessor.Emit(OpCodes.Ldarg, value2);
				}
				iLProcessor.Emit(OpCodes.Ret);
				return dynamicMethodDefinition.Generate();
			}
		}
		return to;
	}
}
