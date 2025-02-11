using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst.LowLevel;

namespace Unity.Burst;

public static class BurstCompiler
{
	[BurstCompile]
	internal static class BurstCompilerHelper
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate bool IsBurstEnabledDelegate();

		private static readonly IsBurstEnabledDelegate IsBurstEnabledImpl = IsBurstEnabled;

		public static readonly bool IsBurstGenerated = IsCompiledByBurst(IsBurstEnabledImpl);

		[BurstCompile]
		[MonoPInvokeCallback(typeof(IsBurstEnabledDelegate))]
		private static bool IsBurstEnabled()
		{
			bool value = true;
			DiscardedMethod(ref value);
			return value;
		}

		[BurstDiscard]
		private static void DiscardedMethod(ref bool value)
		{
			value = false;
		}

		private unsafe static bool IsCompiledByBurst(Delegate del)
		{
			return BurstCompilerService.GetAsyncCompiledAsyncDelegateMethod(BurstCompilerService.CompileAsyncDelegateMethod(del, string.Empty)) != null;
		}
	}

	public static readonly BurstCompilerOptions Options = new BurstCompilerOptions(isGlobal: true);

	internal static bool _IsEnabled;

	public static bool IsEnabled
	{
		get
		{
			if (_IsEnabled)
			{
				return BurstCompilerHelper.IsBurstGenerated;
			}
			return false;
		}
	}

	public static void SetExecutionMode(BurstExecutionEnvironment mode)
	{
		BurstCompilerService.SetCurrentExecutionMode((uint)mode);
	}

	public static BurstExecutionEnvironment GetExecutionMode()
	{
		return (BurstExecutionEnvironment)BurstCompilerService.GetCurrentExecutionMode();
	}

	internal unsafe static T CompileDelegate<T>(T delegateMethod) where T : class
	{
		return (T)(object)Marshal.GetDelegateForFunctionPointer((IntPtr)Compile(delegateMethod, isFunctionPointer: false), delegateMethod.GetType());
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private static void VerifyDelegateIsNotMulticast<T>(T delegateMethod) where T : class
	{
		if ((delegateMethod as Delegate).GetInvocationList().Length > 1)
		{
			throw new InvalidOperationException($"Burst does not support multicast delegates, please use a regular delegate for `{delegateMethod}'");
		}
	}

	public unsafe static FunctionPointer<T> CompileFunctionPointer<T>(T delegateMethod) where T : class
	{
		return new FunctionPointer<T>(new IntPtr(Compile(delegateMethod, isFunctionPointer: true)));
	}

	private unsafe static void* Compile<T>(T delegateObj, bool isFunctionPointer) where T : class
	{
		if (delegateObj == null)
		{
			throw new ArgumentNullException("delegateObj");
		}
		if (!(delegateObj is Delegate))
		{
			throw new ArgumentException("object instance must be a System.Delegate", "delegateObj");
		}
		Delegate @delegate = (Delegate)(object)delegateObj;
		if (!@delegate.Method.IsStatic)
		{
			throw new InvalidOperationException($"The method `{@delegate.Method}` must be static. Instance methods are not supported");
		}
		if (@delegate.Method.IsGenericMethod)
		{
			throw new InvalidOperationException($"The method `{@delegate.Method}` must be a non-generic method");
		}
		if (BurstCompilerOptions.HasBurstCompileAttribute(@delegate.Method))
		{
			void* ptr;
			if (Options.EnableBurstCompilation && BurstCompilerHelper.IsBurstGenerated)
			{
				ptr = BurstCompilerService.GetAsyncCompiledAsyncDelegateMethod(BurstCompilerService.CompileAsyncDelegateMethod(delegateObj, string.Empty));
			}
			else
			{
				GCHandle.Alloc(@delegate);
				ptr = (void*)Marshal.GetFunctionPointerForDelegate(@delegate);
			}
			if (ptr == null)
			{
				throw new InvalidOperationException($"Burst failed to compile the function pointer `{@delegate.Method}`");
			}
			return ptr;
		}
		throw new InvalidOperationException($"Burst cannot compile the function pointer `{@delegate.Method}` because the `[BurstCompile]` attribute is missing");
	}

	internal static void Shutdown()
	{
	}

	internal static void Cancel()
	{
	}

	internal static void Enable()
	{
	}

	internal static void Disable()
	{
	}

	internal static void TriggerRecompilation()
	{
	}

	internal static void EagerCompileMethods(List<EagerCompilationRequest> requests)
	{
	}

	internal static void WaitUntilCompilationFinished()
	{
	}

	internal static void ClearEagerCompilationQueues()
	{
	}

	internal static void CancelEagerCompilation()
	{
	}

	internal static void SetProgressCallback()
	{
	}

	internal static void RequestClearJitCache()
	{
	}

	internal static void Reset()
	{
	}
}
