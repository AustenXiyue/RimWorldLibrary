using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MS.Internal.WindowsRuntime.Windows.UI.ViewManagement;

internal static class InputPaneRcw
{
	internal enum TrustLevel
	{
		BaseTrust,
		PartialTrust,
		FullTrust
	}

	[ComImport]
	[Guid("75CF2C57-9195-4931-8332-F0B409E916AF")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IInputPaneInterop
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		void GetIids(out uint iidCount, [MarshalAs(UnmanagedType.LPStruct)] out Guid iids);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void GetRuntimeClassName([MarshalAs(UnmanagedType.BStr)] out string className);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void GetTrustLevel(out TrustLevel TrustLevel);

		[MethodImpl(MethodImplOptions.InternalCall)]
		IInputPane2 GetForWindow([In] nint appWindow, [In] ref Guid riid);
	}

	[ComImport]
	[Guid("8A6B3F26-7090-4793-944C-C3F2CDE26276")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IInputPane2
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		void GetIids(out uint iidCount, [MarshalAs(UnmanagedType.LPStruct)] out Guid iids);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void GetRuntimeClassName([MarshalAs(UnmanagedType.BStr)] out string className);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void GetTrustLevel(out TrustLevel TrustLevel);

		[MethodImpl(MethodImplOptions.InternalCall)]
		bool TryShow();

		[MethodImpl(MethodImplOptions.InternalCall)]
		bool TryHide();
	}

	private static readonly Guid IID_IActivationFactory = Guid.Parse("00000035-0000-0000-C000-000000000046");

	public static object GetInputPaneActivationFactory()
	{
		nint hstring = IntPtr.Zero;
		Marshal.ThrowExceptionForHR(NativeMethods.WindowsCreateString("Windows.UI.ViewManagement.InputPane", "Windows.UI.ViewManagement.InputPane".Length, out hstring));
		try
		{
			Guid iid = IID_IActivationFactory;
			Marshal.ThrowExceptionForHR(NativeMethods.RoGetActivationFactory(hstring, ref iid, out var factory));
			return factory;
		}
		finally
		{
			Marshal.ThrowExceptionForHR(NativeMethods.WindowsDeleteString(hstring));
		}
	}
}
