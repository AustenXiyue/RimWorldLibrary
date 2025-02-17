using System;
using System.Reflection;
using MonoMod.Core.Interop;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Runtimes;

internal sealed class FxCLR4Runtime : FxBaseRuntime
{
	private ISystem system;

	public override RuntimeFeature Features => base.Features & ~RuntimeFeature.RequiresBodyThunkWalking;

	public FxCLR4Runtime(ISystem system)
	{
		this.system = system;
		if (PlatformDetection.Architecture == ArchitectureKind.x86_64 && (PlatformDetection.RuntimeVersion.Revision >= 17379 || PlatformDetection.RuntimeVersion.Minor >= 5))
		{
			Abi? defaultAbi = system.DefaultAbi;
			if (defaultAbi.HasValue)
			{
				Abi valueOrDefault = defaultAbi.GetValueOrDefault();
				AbiCore = FxCoreBaseRuntime.AbiForCoreFx45X64(valueOrDefault);
			}
		}
	}

	private unsafe IntPtr GetMethodBodyPtr(MethodBase method, RuntimeMethodHandle handle)
	{
		Fx.V48.MethodDesc* pMD = (Fx.V48.MethodDesc*)(void*)handle.Value;
		pMD = Fx.V48.MethodDesc.FindTightlyBoundWrappedMethodDesc(pMD);
		return (IntPtr)pMD->GetNativeCode();
	}

	public override IntPtr GetMethodEntryPoint(MethodBase method)
	{
		method = GetIdentifiable(method);
		RuntimeMethodHandle methodHandle = GetMethodHandle(method);
		bool flag = false;
		IntPtr methodBodyPtr;
		while (true)
		{
			Helpers.Assert(TryInvokeBclCompileMethod(methodHandle), null, "TryInvokeBclCompileMethod(handle)");
			methodHandle.GetFunctionPointer();
			methodBodyPtr = GetMethodBodyPtr(method, methodHandle);
			if (!(methodBodyPtr == IntPtr.Zero))
			{
				break;
			}
			if (!flag)
			{
				Helpers.Assert(TryInvokeBclCompileMethod(methodHandle), null, "TryInvokeBclCompileMethod(handle)");
				flag = true;
				continue;
			}
			methodBodyPtr = methodHandle.GetFunctionPointer();
			throw new InvalidOperationException($"Could not get entry point normally, GetFunctionPointer() = {methodBodyPtr:x16}");
		}
		return methodBodyPtr;
	}
}
