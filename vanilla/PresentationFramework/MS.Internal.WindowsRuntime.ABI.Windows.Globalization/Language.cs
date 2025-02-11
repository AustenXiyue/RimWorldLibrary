using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.Windows.Globalization;
using WinRT;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Globalization;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal struct Language
{
	public static IObjectReference CreateMarshaler(MS.Internal.WindowsRuntime.Windows.Globalization.Language obj)
	{
		if ((object)obj != null)
		{
			return MarshalInspectable.CreateMarshaler(obj).As<ILanguage.Vftbl>();
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

	public static MS.Internal.WindowsRuntime.Windows.Globalization.Language FromAbi(nint thisPtr)
	{
		return MS.Internal.WindowsRuntime.Windows.Globalization.Language.FromAbi(thisPtr);
	}

	public static nint FromManaged(MS.Internal.WindowsRuntime.Windows.Globalization.Language obj)
	{
		if ((object)obj != null)
		{
			return CreateMarshaler(obj).GetRef();
		}
		return IntPtr.Zero;
	}

	public static MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Globalization.Language>.MarshalerArray CreateMarshalerArray(MS.Internal.WindowsRuntime.Windows.Globalization.Language[] array)
	{
		return MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Globalization.Language>.CreateMarshalerArray(array, (MS.Internal.WindowsRuntime.Windows.Globalization.Language o) => CreateMarshaler(o));
	}

	public static (int length, nint data) GetAbiArray(object box)
	{
		return MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Globalization.Language>.GetAbiArray(box);
	}

	public static MS.Internal.WindowsRuntime.Windows.Globalization.Language[] FromAbiArray(object box)
	{
		return MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Globalization.Language>.FromAbiArray(box, FromAbi);
	}

	public static (int length, nint data) FromManagedArray(MS.Internal.WindowsRuntime.Windows.Globalization.Language[] array)
	{
		return MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Globalization.Language>.FromManagedArray(array, (MS.Internal.WindowsRuntime.Windows.Globalization.Language o) => FromManaged(o));
	}

	public static void DisposeMarshaler(IObjectReference value)
	{
		MarshalInspectable.DisposeMarshaler(value);
	}

	public static void DisposeMarshalerArray(MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Globalization.Language>.MarshalerArray array)
	{
		MarshalInterfaceHelper<MS.Internal.WindowsRuntime.Windows.Globalization.Language>.DisposeMarshalerArray(array);
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
