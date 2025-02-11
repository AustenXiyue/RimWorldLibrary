using System;
using ABI.WinRT.Interop;
using WinRT.Interop;

namespace WinRT;

internal class ObjectReferenceWithContext<T> : ObjectReference<T>
{
	private static readonly Guid IID_ICallbackWithNoReentrancyToApplicationSTA = Guid.Parse("0A299774-3E4E-FC42-1D9D-72CEE105CA57");

	private readonly nint _contextCallbackPtr;

	public ObjectReferenceWithContext(nint thisPtr, nint contextCallbackPtr)
		: base(thisPtr)
	{
		_contextCallbackPtr = contextCallbackPtr;
	}

	protected unsafe override void Release()
	{
		ComCallData comCallData = default(ComCallData);
		nint thisPtr = _contextCallbackPtr;
		new ABI.WinRT.Interop.IContextCallback(ObjectReference<ABI.WinRT.Interop.IContextCallback.Vftbl>.Attach(ref thisPtr)).ContextCallback(delegate
		{
			base.Release();
			return 0;
		}, &comCallData, IID_ICallbackWithNoReentrancyToApplicationSTA, 5);
	}

	public override int TryAs<U>(Guid iid, out ObjectReference<U> objRef)
	{
		objRef = null;
		ThrowIfDisposed();
		nint vftbl;
		int num = base.VftblIUnknown.QueryInterface(base.ThisPtr, ref iid, out vftbl);
		if (num >= 0)
		{
			using ObjectReference<ABI.WinRT.Interop.IContextCallback.Vftbl> objectReference = ObjectReference<ABI.WinRT.Interop.IContextCallback.Vftbl>.FromAbi(_contextCallbackPtr);
			objRef = new ObjectReferenceWithContext<U>(vftbl, objectReference.GetRef());
		}
		return num;
	}
}
