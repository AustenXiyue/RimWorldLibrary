using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using MonoMod.Core.Interop;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Runtimes;

internal class Core70Runtime : Core60Runtime
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	protected struct ICorJitInfoWrapper
	{
		public IntPtr Vtbl;

		public unsafe IntPtr** Wrapped;

		public const int HotCodeRW = 0;

		public const int ColdCodeRW = 1;

		private const int DataQWords = 4;

		private unsafe fixed ulong data[4];

		public unsafe ref IntPtr this[int index] => ref Unsafe.Add(ref Unsafe.As<ulong, IntPtr>(ref data[0]), index);
	}

	private sealed class JitHookDelegateHolder
	{
		public readonly Core70Runtime Runtime;

		public readonly INativeExceptionHelper? NativeExceptionHelper;

		public readonly GetExceptionSlot? GetNativeExceptionSlot;

		public readonly JitHookHelpersHolder JitHookHelpers;

		public readonly CoreCLR.InvokeCompileMethodPtr InvokeCompileMethodPtr;

		public readonly IntPtr CompileMethodPtr;

		public readonly ThreadLocal<IAllocatedMemory> iCorJitInfoWrapper = new ThreadLocal<IAllocatedMemory>();

		public readonly ReadOnlyMemory<IAllocatedMemory> iCorJitInfoWrapperAllocs;

		public readonly IntPtr iCorJitInfoWrapperVtbl;

		[ThreadStatic]
		private static int hookEntrancy;

		public unsafe JitHookDelegateHolder(Core70Runtime runtime, CoreCLR.InvokeCompileMethodPtr icmp, IntPtr compileMethod)
		{
			Runtime = runtime;
			NativeExceptionHelper = runtime.NativeExceptionHelper;
			JitHookHelpers = runtime.JitHookHelpers;
			InvokeCompileMethodPtr = icmp;
			CompileMethodPtr = compileMethod;
			iCorJitInfoWrapperVtbl = Marshal.AllocHGlobal(IntPtr.Size * runtime.ICorJitInfoFullVtableCount);
			iCorJitInfoWrapperAllocs = Runtime.arch.CreateNativeVtableProxyStubs(iCorJitInfoWrapperVtbl, runtime.ICorJitInfoFullVtableCount);
			Runtime.PatchWrapperVtable((IntPtr*)(void*)iCorJitInfoWrapperVtbl);
			bool isEnabled;
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(42, 1, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("Allocated ICorJitInfo wrapper vtable at 0x");
				message.AppendFormatted(iCorJitInfoWrapperVtbl, "x16");
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message);
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
			/*Error near IL_00e4: Handle with invalid row number.*/;
		}

		public unsafe CoreCLR.CorJitResult CompileMethodHook(IntPtr jit, IntPtr corJitInfo, CoreCLR.V21.CORINFO_METHOD_INFO* methodInfo, uint flags, byte** nativeEntry, uint* nativeSizeOfCode)
		{
			//Discarded unreachable code: IL_01b6, IL_0345, IL_0366
			*nativeEntry = null;
			*nativeSizeOfCode = 0u;
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
				if (hookEntrancy == 1)
				{
					try
					{
						IAllocatedMemory allocatedMemory = iCorJitInfoWrapper.Value;
						if (allocatedMemory == null)
						{
							AllocationRequest allocationRequest = new AllocationRequest(sizeof(ICorJitInfoWrapper));
							allocationRequest.Alignment = IntPtr.Size;
							allocationRequest.Executable = false;
							AllocationRequest request = allocationRequest;
							if (Runtime.System.MemoryAllocator.TryAllocate(request, out IAllocatedMemory allocated))
							{
								allocatedMemory = (iCorJitInfoWrapper.Value = allocated);
							}
						}
						if (allocatedMemory != null)
						{
							ICorJitInfoWrapper* ptr2 = (ICorJitInfoWrapper*)(void*)allocatedMemory.BaseAddress;
							ptr2->Vtbl = iCorJitInfoWrapperVtbl;
							ptr2->Wrapped = (IntPtr**)(void*)corJitInfo;
							(*ptr2)[0] = IntPtr.Zero;
							(*ptr2)[1] = IntPtr.Zero;
							corJitInfo = (IntPtr)ptr2;
						}
					}
					catch (Exception value)
					{
						try
						{
							bool isEnabled;
							_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(48, 1, out isEnabled);
							if (isEnabled)
							{
								message.AppendLiteral("Error while setting up the ICorJitInfo wrapper: ");
								message.AppendFormatted(value);
							}
							_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
						}
						catch
						{
						}
					}
				}
				delegate*<IntPtr, IntPtr, IntPtr, CoreCLR.V21.CORINFO_METHOD_INFO*, uint, byte**, uint*, CoreCLR.CorJitResult> invokeCompileMethod = InvokeCompileMethodPtr.InvokeCompileMethod;
				_ = CompileMethodPtr;
				/*Error near IL_0154: Handle with invalid row number.*/;
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

	private sealed class AllocMemDelegateHolder
	{
		public readonly Core70Runtime Runtime;

		public readonly INativeExceptionHelper? NativeExceptionHelper;

		public readonly GetExceptionSlot? GetNativeExceptionSlot;

		public readonly CoreCLR.InvokeAllocMemPtr InvokeAllocMemPtr;

		public readonly int ICorJitInfoAllocMemIdx;

		public readonly ConcurrentDictionary<IntPtr, (IntPtr M2N, IDisposable?)> AllocMemExceptionHelperCache = new ConcurrentDictionary<IntPtr, (IntPtr, IDisposable)>();

		public unsafe AllocMemDelegateHolder(Core70Runtime runtime, CoreCLR.InvokeAllocMemPtr iamp)
		{
			Runtime = runtime;
			NativeExceptionHelper = runtime.NativeExceptionHelper;
			GetNativeExceptionSlot = NativeExceptionHelper?.GetExceptionSlot;
			InvokeAllocMemPtr = iamp;
			ICorJitInfoAllocMemIdx = Runtime.VtableIndexICorJitInfoAllocMem;
			delegate*<IntPtr, IntPtr, CoreCLR.V70.AllocMemArgs*, void> invokeAllocMem = iamp.InvokeAllocMem;
			_ = IntPtr.Zero;
			_ = IntPtr.Zero;
			_ = 0u;
			/*Error near IL_0069: Handle with invalid row number.*/;
		}

		private IntPtr GetRealInvokePtr(IntPtr ptr)
		{
			if (NativeExceptionHelper == null)
			{
				return ptr;
			}
			IDisposable handle;
			return AllocMemExceptionHelperCache.GetOrAdd(ptr, (IntPtr p) => (M2N: Runtime.EHManagedToNative(p, out handle), handle)).M2N;
		}

		public unsafe void AllocMemHook(IntPtr thisPtr, CoreCLR.V70.AllocMemArgs* args)
		{
			ICorJitInfoWrapper* ptr = (ICorJitInfoWrapper*)(void*)thisPtr;
			IntPtr** wrapped = ptr->Wrapped;
			delegate*<IntPtr, IntPtr, CoreCLR.V70.AllocMemArgs*, void> invokeAllocMem = InvokeAllocMemPtr.InvokeAllocMem;
			GetRealInvokePtr((*wrapped)[ICorJitInfoAllocMemIdx]);
			_ = (IntPtr)wrapped;
			/*Error near IL_003a: Handle with invalid row number.*/;
		}
	}

	private readonly IArchitecture arch;

	private static readonly Guid JitVersionGuid = new Guid(1810136669u, 43307, 19734, 146, 128, 246, 61, 246, 70, 173, 164);

	private Delegate? allocMemDelegate;

	private IDisposable? n2mAllocMemHelper;

	protected override Guid ExpectedJitVersion => JitVersionGuid;

	protected virtual int VtableIndexICorJitInfoAllocMem => 159;

	protected virtual int ICorJitInfoFullVtableCount => 175;

	protected virtual CoreCLR.InvokeAllocMemPtr InvokeAllocMemPtr => CoreCLR.V70.InvokeAllocMemPtr;

	public Core70Runtime(ISystem system, IArchitecture arch)
		: base(system)
	{
		this.arch = arch;
	}

	protected override Delegate CreateCompileMethodDelegate(IntPtr compileMethod)
	{
		return new _003C_003Ef__AnonymousDelegate0(new JitHookDelegateHolder(this, InvokeCompileMethodPtr, compileMethod).CompileMethodHook);
	}

	protected unsafe virtual void PatchWrapperVtable(IntPtr* vtbl)
	{
		allocMemDelegate = CastAllocMemToRealType(CreateAllocMemDelegate());
		vtbl[VtableIndexICorJitInfoAllocMem] = EHNativeToManaged(Marshal.GetFunctionPointerForDelegate(allocMemDelegate), out n2mAllocMemHelper);
	}

	protected virtual Delegate CastAllocMemToRealType(Delegate del)
	{
		return del.CastDelegate<CoreCLR.V70.AllocMemDelegate>();
	}

	protected virtual Delegate CreateAllocMemDelegate()
	{
		return new _003C_003Ef__AnonymousDelegate1(new AllocMemDelegateHolder(this, InvokeAllocMemPtr).AllocMemHook);
	}
}
