using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.Windows.Data.Text;
using WinRT;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal struct WordSegment
{
	public static IObjectReference CreateMarshaler(MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment obj)
	{
		if ((object)obj != null)
		{
			return MarshalInspectable.CreateMarshaler(obj).As<IWordSegment.Vftbl>();
		}
		return null;
	}

	public static nint GetAbi(IObjectReference value)
	{
		if (value != null)
		{
			return MarshalInterfaceHelper<object>.GetAbi(value);
		}
		return IntPtr.Zero;
	}

	public static MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment FromAbi(nint thisPtr)
	{
		return MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment.FromAbi(thisPtr);
	}

	public static nint FromManaged(MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment obj)
	{
		if ((object)obj != null)
		{
			return CreateMarshaler(obj).GetRef();
		}
		return IntPtr.Zero;
	}

	public static MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.MarshalerArray CreateMarshalerArray(MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment[] array)
	{
		return MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.CreateMarshalerArray(array, (MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment o) => CreateMarshaler(o));
	}

	public static (int length, nint data) GetAbiArray(object box)
	{
		return MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.GetAbiArray(box);
	}

	public static MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment[] FromAbiArray(object box)
	{
		return MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.FromAbiArray(box, FromAbi);
	}

	public static (int length, nint data) FromManagedArray(MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment[] array)
	{
		return MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.FromManagedArray(array, (MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment o) => FromManaged(o));
	}

	public static void DisposeMarshaler(IObjectReference value)
	{
		MarshalInspectable.DisposeMarshaler(value);
	}

	public static void DisposeMarshalerArray(MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.MarshalerArray array)
	{
		MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment>.DisposeMarshalerArray(array);
	}

	public static void DisposeAbi(nint abi)
	{
		MarshalInspectable.DisposeAbi(abi);
	}

	public static void DisposeAbiArray(object box)
	{
		MarshalInspectable.DisposeAbiArray(box);
	}
}
