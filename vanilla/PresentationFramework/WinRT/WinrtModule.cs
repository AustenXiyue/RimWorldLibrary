using System;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace WinRT;

internal class WinrtModule
{
	private readonly nint _mtaCookie;

	private static Lazy<WinrtModule> _instance = new Lazy<WinrtModule>();

	public static WinrtModule Instance => _instance.Value;

	public unsafe WinrtModule()
	{
		nint mtaCookie = default(nint);
		Marshal.ThrowExceptionForHR(Platform.CoIncrementMTAUsage(&mtaCookie));
		_mtaCookie = mtaCookie;
	}

	public unsafe static (ObjectReference<IActivationFactoryVftbl> obj, int hr) GetActivationFactory(string runtimeClassId)
	{
		_ = Instance;
		Guid iid = typeof(IActivationFactoryVftbl).GUID;
		nint thisPtr = default(nint);
		int num = Platform.RoGetActivationFactory(MarshalString.GetAbi(MarshalString.CreateMarshaler(runtimeClassId)), ref iid, &thisPtr);
		return (obj: (num == 0) ? ObjectReference<IActivationFactoryVftbl>.Attach(ref thisPtr) : null, hr: num);
	}

	~WinrtModule()
	{
		Marshal.ThrowExceptionForHR(Platform.CoDecrementMTAUsage(_mtaCookie));
	}
}
