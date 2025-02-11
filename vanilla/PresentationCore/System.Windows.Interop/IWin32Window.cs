namespace System.Windows.Interop;

/// <summary>Defines the contract for Win32 window handles.</summary>
public interface IWin32Window
{
	/// <summary>Gets the window handle.</summary>
	/// <returns>The window handle.</returns>
	nint Handle { get; }
}
