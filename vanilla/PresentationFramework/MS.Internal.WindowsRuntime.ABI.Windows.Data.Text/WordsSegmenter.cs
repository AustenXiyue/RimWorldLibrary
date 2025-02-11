using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.Windows.Data.Text;
using WinRT;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal struct WordsSegmenter
{
	public static IObjectReference CreateMarshaler(MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter obj)
	{
		if ((object)obj != null)
		{
			return MarshalInspectable.CreateMarshaler(obj).As<IWordsSegmenter.Vftbl>();
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

	public static MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter FromAbi(nint thisPtr)
	{
		return MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter.FromAbi(thisPtr);
	}

	public static nint FromManaged(MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter obj)
	{
		if ((object)obj != null)
		{
			return CreateMarshaler(obj).GetRef();
		}
		return IntPtr.Zero;
	}

	public static MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter>.MarshalerArray CreateMarshalerArray(MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter[] array)
	{
		return MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter>.CreateMarshalerArray(array, (MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter o) => CreateMarshaler(o));
	}

	public static (int length, nint data) GetAbiArray(object box)
	{
		return MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter>.GetAbiArray(box);
	}

	public static MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter[] FromAbiArray(object box)
	{
		return MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter>.FromAbiArray(box, FromAbi);
	}

	public static (int length, nint data) FromManagedArray(MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter[] array)
	{
		return MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter>.FromManagedArray(array, (MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter o) => FromManaged(o));
	}

	public static void DisposeMarshaler(IObjectReference value)
	{
		MarshalInspectable.DisposeMarshaler(value);
	}

	public static void DisposeMarshalerArray(MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter>.MarshalerArray array)
	{
		MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Data.Text.WordsSegmenter>.DisposeMarshalerArray(array);
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
