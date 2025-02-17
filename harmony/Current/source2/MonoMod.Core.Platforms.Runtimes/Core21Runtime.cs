using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Mono.Cecil;
using MonoMod.Core.Interop;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Runtimes;

internal class Core21Runtime : CoreBaseRuntime
{
	private sealed class JitHookDelegateHolder
	{
		public readonly Core21Runtime Runtime;

		public readonly INativeExceptionHelper? NativeExceptionHelper;

		public readonly GetExceptionSlot? GetNativeExceptionSlot;

		public readonly JitHookHelpersHolder JitHookHelpers;

		public readonly CoreCLR.InvokeCompileMethodPtr InvokeCompileMethodPtr;

		public readonly IntPtr CompileMethodPtr;

		[ThreadStatic]
		private static int hookEntrancy;

		public unsafe JitHookDelegateHolder(Core21Runtime runtime, CoreCLR.InvokeCompileMethodPtr icmp, IntPtr compileMethod)
		{
			Runtime = runtime;
			NativeExceptionHelper = runtime.NativeExceptionHelper;
			JitHookHelpers = runtime.JitHookHelpers;
			InvokeCompileMethodPtr = icmp;
			CompileMethodPtr = compileMethod;
			delegate*<IntPtr, IntPtr, IntPtr, CoreCLR.V21.CORINFO_METHOD_INFO*, uint, byte**, uint*, CoreCLR.CorJitResult> invokeCompileMethod = icmp.InvokeCompileMethod;
			_ = IntPtr.Zero;
			_ = IntPtr.Zero;
			_ = IntPtr.Zero;
			CoreCLR.V21.CORINFO_METHOD_INFO cORINFO_METHOD_INFO = default(CoreCLR.V21.CORINFO_METHOD_INFO);
			_ = &cORINFO_METHOD_INFO;
			_ = 0;
			byte* ptr = default(byte*);
			_ = &ptr;
			uint num = default(uint);
			_ = &num;
			/*Error near IL_0057: Handle with invalid row number.*/;
		}

		public unsafe CoreCLR.CorJitResult CompileMethodHook(IntPtr jit, IntPtr corJitInfo, CoreCLR.V21.CORINFO_METHOD_INFO* methodInfo, uint flags, byte** pNativeEntry, uint* pNativeSizeOfCode)
		{
			//Discarded unreachable code: IL_00c2, IL_0228, IL_0249
			*pNativeEntry = null;
			*pNativeSizeOfCode = 0u;
			if (jit == IntPtr.Zero)
			{
				return CoreCLR.CorJitResult.CORJIT_OK;
			}
			int lastPInvokeError = MarshalEx.GetLastPInvokeError();
			nint num = 0;
			GetExceptionSlot getNativeExceptionSlot = GetNativeExceptionSlot;
			IntPtr* ptr = ((getNativeExceptionSlot != null) ? getNativeExceptionSlot() : null);
			hookEntrancy++;
			try
			{
				delegate*<IntPtr, IntPtr, IntPtr, CoreCLR.V21.CORINFO_METHOD_INFO*, uint, byte**, uint*, CoreCLR.CorJitResult> invokeCompileMethod = InvokeCompileMethodPtr.InvokeCompileMethod;
				_ = CompileMethodPtr;
				/*Error near IL_0060: Handle with invalid row number.*/;
			}
			finally
			{
				hookEntrancy--;
				if (ptr != null)
				{
					*ptr = num;
				}
				MarshalEx.SetLastPInvokeError(lastPInvokeError);
			}
		}
	}

	protected sealed class JitHookHelpersHolder
	{
		public delegate object MethodHandle_GetLoaderAllocatorD(IntPtr methodHandle);

		public delegate object CreateRuntimeMethodInfoStubD(IntPtr methodHandle, object loaderAllocator);

		public delegate RuntimeMethodHandle CreateRuntimeMethodHandleD(object runtimeMethodInfo);

		public delegate Type GetDeclaringTypeOfMethodHandleD(IntPtr methodHandle);

		public delegate Type GetTypeFromNativeHandleD(IntPtr handle);

		public readonly MethodHandle_GetLoaderAllocatorD MethodHandle_GetLoaderAllocator;

		public readonly CreateRuntimeMethodInfoStubD CreateRuntimeMethodInfoStub;

		public readonly CreateRuntimeMethodHandleD CreateRuntimeMethodHandle;

		public readonly GetDeclaringTypeOfMethodHandleD GetDeclaringTypeOfMethodHandle;

		public readonly GetTypeFromNativeHandleD GetTypeFromNativeHandle;

		public RuntimeMethodHandle CreateHandleForHandlePointer(IntPtr handle)
		{
			return CreateRuntimeMethodHandle(CreateRuntimeMethodInfoStub(handle, MethodHandle_GetLoaderAllocator(handle)));
		}

		public JitHookHelpersHolder(Core21Runtime runtime)
		{
			MethodInfo method = typeof(RuntimeMethodHandle).GetMethod("GetLoaderAllocator", BindingFlags.Static | BindingFlags.NonPublic);
			MethodInfo method2;
			using (DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("MethodHandle_GetLoaderAllocator", typeof(object), new Type[1] { typeof(IntPtr) }))
			{
				ILGenerator iLGenerator = dynamicMethodDefinition.GetILGenerator();
				Type parameterType = method.GetParameters().First().ParameterType;
				iLGenerator.Emit(OpCodes.Ldarga_S, 0);
				iLGenerator.Emit(OpCodes.Ldobj, parameterType);
				iLGenerator.Emit(OpCodes.Call, method);
				iLGenerator.Emit(OpCodes.Ret);
				method2 = dynamicMethodDefinition.Generate();
			}
			MethodHandle_GetLoaderAllocator = method2.CreateDelegate<MethodHandle_GetLoaderAllocatorD>();
			MethodInfo orCreateGetTypeFromHandleUnsafe = GetOrCreateGetTypeFromHandleUnsafe(runtime);
			GetTypeFromNativeHandle = orCreateGetTypeFromHandleUnsafe.CreateDelegate<GetTypeFromNativeHandleD>();
			Type type = typeof(RuntimeMethodHandle).Assembly.GetType("System.RuntimeMethodHandleInternal");
			MethodInfo method3 = typeof(RuntimeMethodHandle).GetMethod("GetDeclaringType", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[1] { type }, null);
			MethodInfo method4;
			using (DynamicMethodDefinition dynamicMethodDefinition2 = new DynamicMethodDefinition("GetDeclaringTypeOfMethodHandle", typeof(Type), new Type[1] { typeof(IntPtr) }))
			{
				ILGenerator iLGenerator2 = dynamicMethodDefinition2.GetILGenerator();
				iLGenerator2.Emit(OpCodes.Ldarga_S, 0);
				iLGenerator2.Emit(OpCodes.Ldobj, type);
				iLGenerator2.Emit(OpCodes.Call, method3);
				iLGenerator2.Emit(OpCodes.Ret);
				method4 = dynamicMethodDefinition2.Generate();
			}
			GetDeclaringTypeOfMethodHandle = method4.CreateDelegate<GetDeclaringTypeOfMethodHandleD>();
			Type[] array = new Type[2]
			{
				typeof(IntPtr),
				typeof(object)
			};
			Type type2 = typeof(RuntimeMethodHandle).Assembly.GetType("System.RuntimeMethodInfoStub");
			ConstructorInfo constructor = type2.GetConstructor(array);
			MethodInfo method5;
			using (DynamicMethodDefinition dynamicMethodDefinition3 = new DynamicMethodDefinition("new RuntimeMethodInfoStub", type2, array))
			{
				ILGenerator iLGenerator3 = dynamicMethodDefinition3.GetILGenerator();
				iLGenerator3.Emit(OpCodes.Ldarg_0);
				iLGenerator3.Emit(OpCodes.Ldarg_1);
				iLGenerator3.Emit(OpCodes.Newobj, constructor);
				iLGenerator3.Emit(OpCodes.Ret);
				method5 = dynamicMethodDefinition3.Generate();
			}
			CreateRuntimeMethodInfoStub = method5.CreateDelegate<CreateRuntimeMethodInfoStubD>();
			ConstructorInfo con = typeof(RuntimeMethodHandle).GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).First();
			MethodInfo method6;
			using (DynamicMethodDefinition dynamicMethodDefinition4 = new DynamicMethodDefinition("new RuntimeMethodHandle", typeof(RuntimeMethodHandle), new Type[1] { typeof(object) }))
			{
				ILGenerator iLGenerator4 = dynamicMethodDefinition4.GetILGenerator();
				iLGenerator4.Emit(OpCodes.Ldarg_0);
				iLGenerator4.Emit(OpCodes.Newobj, con);
				iLGenerator4.Emit(OpCodes.Ret);
				method6 = dynamicMethodDefinition4.Generate();
			}
			CreateRuntimeMethodHandle = method6.CreateDelegate<CreateRuntimeMethodHandleD>();
		}

		private static MethodInfo GetOrCreateGetTypeFromHandleUnsafe(Core21Runtime runtime)
		{
			MethodInfo method = typeof(Type).GetMethod("GetTypeFromHandleUnsafe", (BindingFlags)(-1));
			if ((object)method != null)
			{
				return method;
			}
			Assembly assembly;
			using (ModuleDefinition moduleDefinition = ModuleDefinition.CreateModule("MonoMod.Core.Platforms.Runtimes.Core30Runtime+Helpers", new ModuleParameters
			{
				Kind = ModuleKind.Dll
			}))
			{
				TypeDefinition typeDefinition = new TypeDefinition("System", "Type", Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Abstract)
				{
					BaseType = moduleDefinition.TypeSystem.Object
				};
				moduleDefinition.Types.Add(typeDefinition);
				MethodDefinition methodDefinition = new MethodDefinition("GetTypeFromHandleUnsafe", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static, moduleDefinition.ImportReference(typeof(Type)))
				{
					IsInternalCall = true
				};
				methodDefinition.Parameters.Add(new ParameterDefinition(moduleDefinition.ImportReference(typeof(IntPtr))));
				typeDefinition.Methods.Add(methodDefinition);
				assembly = ReflectionHelper.Load(moduleDefinition);
			}
			runtime.MakeAssemblySystemAssembly(assembly);
			return assembly.GetType("System.Type").GetMethod("GetTypeFromHandleUnsafe", (BindingFlags)(-1));
		}
	}

	private readonly object sync = new object();

	private JitHookHelpersHolder? lazyJitHookHelpers;

	private static readonly Guid JitVersionGuid = new Guid(195102408u, 33184, 16511, 153, 161, 146, 132, 72, 193, 235, 98);

	private Delegate? ourCompileMethod;

	private IDisposable? n2mHookHelper;

	private IDisposable? m2nHookHelper;

	private static readonly FieldInfo RuntimeAssemblyPtrField = Type.GetType("System.Reflection.RuntimeAssembly").GetField("m_assembly", BindingFlags.Instance | BindingFlags.NonPublic);

	public override RuntimeFeature Features => base.Features | RuntimeFeature.CompileMethodHook;

	protected unsafe JitHookHelpersHolder JitHookHelpers => Helpers.GetOrInitWithLock(ref lazyJitHookHelpers, sync, (delegate*<Core21Runtime, JitHookHelpersHolder>)(&CreateJitHookHelpers), this);

	protected virtual Guid ExpectedJitVersion => JitVersionGuid;

	protected virtual int VtableIndexICorJitCompilerGetVersionGuid => 4;

	protected virtual int VtableIndexICorJitCompilerCompileMethod => 0;

	protected virtual CoreCLR.InvokeCompileMethodPtr InvokeCompileMethodPtr => CoreCLR.V21.InvokeCompileMethodPtr;

	public Core21Runtime(ISystem system)
		: base(system)
	{
	}

	private static JitHookHelpersHolder CreateJitHookHelpers(Core21Runtime self)
	{
		return new JitHookHelpersHolder(self);
	}

	protected virtual Delegate CastCompileHookToRealType(Delegate del)
	{
		return del.CastDelegate<CoreCLR.V21.CompileMethodDelegate>();
	}

	protected unsafe static IntPtr* GetVTableEntry(IntPtr @object, int index)
	{
		return (IntPtr*)((nint)(*(IntPtr*)(void*)@object) + (nint)index * (nint)sizeof(IntPtr));
	}

	protected unsafe static IntPtr ReadObjectVTable(IntPtr @object, int index)
	{
		return *GetVTableEntry(@object, index);
	}

	private unsafe void CheckVersionGuid(IntPtr jit)
	{
		Guid guid = default(Guid);
		((delegate* unmanaged[Thiscall]<IntPtr, Guid*, void>)(void*)ReadObjectVTable(jit, VtableIndexICorJitCompilerGetVersionGuid))(jit, &guid);
		bool flag = guid == ExpectedJitVersion;
		bool isEnabled;
		AssertionInterpolatedStringHandler message = new AssertionInterpolatedStringHandler(66, 2, flag, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("JIT version does not match expected JIT version! ");
			message.AppendLiteral("expected: ");
			message.AppendFormatted(ExpectedJitVersion);
			message.AppendLiteral(", got: ");
			message.AppendFormatted(guid);
		}
		Helpers.Assert(flag, ref message, "guid == ExpectedJitVersion");
	}

	protected unsafe override void InstallJitHook(IntPtr jit)
	{
		CheckVersionGuid(jit);
		IntPtr* vTableEntry = GetVTableEntry(jit, VtableIndexICorJitCompilerCompileMethod);
		IntPtr compileMethod = EHManagedToNative(*vTableEntry, out m2nHookHelper);
		IntPtr value = EHNativeToManaged(Marshal.GetFunctionPointerForDelegate(ourCompileMethod = CastCompileHookToRealType(CreateCompileMethodDelegate(compileMethod))), out n2mHookHelper);
		InvokeCompileMethodToPrepare(value);
		Span<byte> span = stackalloc byte[sizeof(IntPtr)];
		MemoryMarshal.Write(span, ref value);
		base.System.PatchData(PatchTargetKind.ReadOnly, (IntPtr)vTableEntry, span, default(Span<byte>));
	}

	protected unsafe virtual void InvokeCompileMethodToPrepare(IntPtr method)
	{
		delegate*<IntPtr, IntPtr, IntPtr, CoreCLR.V21.CORINFO_METHOD_INFO*, uint, byte**, uint*, CoreCLR.CorJitResult> invokeCompileMethod = InvokeCompileMethodPtr.InvokeCompileMethod;
		_ = IntPtr.Zero;
		_ = IntPtr.Zero;
		CoreCLR.V21.CORINFO_METHOD_INFO cORINFO_METHOD_INFO = default(CoreCLR.V21.CORINFO_METHOD_INFO);
		_ = &cORINFO_METHOD_INFO;
		_ = 0;
		byte* ptr = default(byte*);
		_ = &ptr;
		uint num = default(uint);
		_ = &num;
		/*Error near IL_0027: Handle with invalid row number.*/;
	}

	protected virtual Delegate CreateCompileMethodDelegate(IntPtr compileMethod)
	{
		return new _003C_003Ef__AnonymousDelegate0(new JitHookDelegateHolder(this, InvokeCompileMethodPtr, compileMethod).CompileMethodHook);
	}

	protected unsafe virtual void MakeAssemblySystemAssembly(Assembly assembly)
	{
		IntPtr intPtr = (IntPtr)RuntimeAssemblyPtrField.GetValue(assembly);
		int num = IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + 4 + IntPtr.Size + IntPtr.Size + 4 + 4 + IntPtr.Size + IntPtr.Size + 4 + 4 + IntPtr.Size;
		if (IntPtr.Size == 8)
		{
			num += 4;
		}
		IntPtr intPtr2 = *(IntPtr*)((byte*)(void*)intPtr + num);
		int num2 = IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size;
		IntPtr intPtr3 = *(IntPtr*)((byte*)(void*)intPtr2 + num2);
		int num3 = IntPtr.Size + (FxCoreBaseRuntime.IsDebugClr ? (IntPtr.Size + 4 + 4 + 4 + IntPtr.Size + 4) : 0) + IntPtr.Size + IntPtr.Size + 4 + 4 + IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + 4;
		if (FxCoreBaseRuntime.IsDebugClr && IntPtr.Size == 8)
		{
			num3 += 8;
		}
		int* ptr = (int*)((byte*)(void*)intPtr3 + num3);
		*ptr |= 1;
	}
}
