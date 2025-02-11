using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.ABI.System.Collections.Generic;
using MS.Internal.WindowsRuntime.Windows.Data.Text;
using WinRT;
using WinRT.Interop;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;

[EditorBrowsable(EditorBrowsableState.Never)]
[Guid("A5DD6357-BF2A-4C4F-A31F-29E71C6F8B35")]
internal static class WordSegmentsTokenizingHandler
{
	private delegate int Abi_Invoke(nint thisPtr, nint precedingWords, nint words);

	[ObjectReferenceWrapper("_nativeDelegate")]
	private class NativeDelegateWrapper
	{
		private readonly ObjectReference<IDelegateVftbl> _nativeDelegate;

		public NativeDelegateWrapper(ObjectReference<IDelegateVftbl> nativeDelegate)
		{
			_nativeDelegate = nativeDelegate;
		}

		public void Invoke(global::System.Collections.Generic.IEnumerable<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment> precedingWords, global::System.Collections.Generic.IEnumerable<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment> words)
		{
			nint thisPtr = _nativeDelegate.ThisPtr;
			Abi_Invoke delegateForFunctionPointer = Marshal.GetDelegateForFunctionPointer<Abi_Invoke>(_nativeDelegate.Vftbl.Invoke);
			IObjectReference objRef = null;
			IObjectReference objRef2 = null;
			try
			{
				objRef = MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IEnumerable<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.CreateMarshaler(precedingWords);
				objRef2 = MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IEnumerable<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.CreateMarshaler(words);
				ExceptionHelpers.ThrowExceptionForHR(delegateForFunctionPointer(thisPtr, MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IEnumerable<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.GetAbi(objRef), MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IEnumerable<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.GetAbi(objRef2)));
			}
			finally
			{
				MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IEnumerable<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.DisposeMarshaler(objRef);
				MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IEnumerable<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.DisposeMarshaler(objRef2);
			}
		}
	}

	private static readonly IDelegateVftbl AbiToProjectionVftable;

	public static readonly nint AbiToProjectionVftablePtr;

	public static Delegate AbiInvokeDelegate { get; }

	static WordSegmentsTokenizingHandler()
	{
		AbiInvokeDelegate = new Abi_Invoke(Do_Abi_Invoke);
		AbiToProjectionVftable = new IDelegateVftbl
		{
			IUnknownVftbl = IUnknownVftbl.AbiToProjectionVftbl,
			Invoke = Marshal.GetFunctionPointerForDelegate(AbiInvokeDelegate)
		};
		nint num = ComWrappersSupport.AllocateVtableMemory(typeof(WordSegmentsTokenizingHandler), Marshal.SizeOf<IDelegateVftbl>());
		Marshal.StructureToPtr(AbiToProjectionVftable, num, fDeleteOld: false);
		AbiToProjectionVftablePtr = num;
	}

	public static IObjectReference CreateMarshaler(MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegmentsTokenizingHandler managedDelegate)
	{
		if (managedDelegate != null)
		{
			return ComWrappersSupport.CreateCCWForObject(managedDelegate).As<IDelegateVftbl>(GuidGenerator.GetIID(typeof(WordSegmentsTokenizingHandler)));
		}
		return null;
	}

	public static nint GetAbi(IObjectReference value)
	{
		return MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegmentsTokenizingHandler>.GetAbi(value);
	}

	public static MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegmentsTokenizingHandler FromAbi(nint nativeDelegate)
	{
		ObjectReference<IDelegateVftbl> objectReference = ObjectReference<IDelegateVftbl>.FromAbi(nativeDelegate);
		if (objectReference != null)
		{
			return (MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegmentsTokenizingHandler)ComWrappersSupport.TryRegisterObjectForInterface(new MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegmentsTokenizingHandler(new NativeDelegateWrapper(objectReference).Invoke), nativeDelegate);
		}
		return null;
	}

	public static nint FromManaged(MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegmentsTokenizingHandler managedDelegate)
	{
		return CreateMarshaler(managedDelegate)?.GetRef() ?? IntPtr.Zero;
	}

	public static void DisposeMarshaler(IObjectReference value)
	{
		MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegmentsTokenizingHandler>.DisposeMarshaler(value);
	}

	public static void DisposeAbi(nint abi)
	{
		MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegmentsTokenizingHandler>.DisposeAbi(abi);
	}

	private static int Do_Abi_Invoke(nint thisPtr, nint precedingWords, nint words)
	{
		try
		{
			ComWrappersSupport.MarshalDelegateInvoke(thisPtr, delegate(MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegmentsTokenizingHandler invoke)
			{
				invoke(MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IEnumerable<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.FromAbi(precedingWords), MS.Internal.WindowsRuntime.ABI.System.Collections.Generic.IEnumerable<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.FromAbi(words));
			});
		}
		catch (Exception ex)
		{
			ExceptionHelpers.SetErrorInfo(ex);
			return ExceptionHelpers.GetHRForException(ex);
		}
		return 0;
	}
}
