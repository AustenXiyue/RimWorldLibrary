using System.Runtime.InteropServices;
using System.Windows.Input;

namespace System.Windows.Interop;

/// <summary>Provides a static helper class for WPF/Win32 interoperation with one method, which is used to obtain a Windows Presentation Foundation (WPF) <see cref="T:System.Windows.Input.Cursor" /> object based on a provided Win32 cursor handle.</summary>
public static class CursorInteropHelper
{
	/// <summary>Returns a Windows Presentation Foundation (WPF) <see cref="T:System.Windows.Input.Cursor" /> object based on a provided Win32 cursor handle.</summary>
	/// <returns>The Windows Presentation Foundation (WPF) cursor object based on the provided Win32 cursor handle.</returns>
	/// <param name="cursorHandle">Cursor reference to use for interoperation.</param>
	public static Cursor Create(SafeHandle cursorHandle)
	{
		return CriticalCreate(cursorHandle);
	}

	internal static Cursor CriticalCreate(SafeHandle cursorHandle)
	{
		return new Cursor(cursorHandle);
	}
}
