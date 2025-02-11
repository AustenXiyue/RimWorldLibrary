using MS.Internal;

namespace System.Windows.Interop;

/// <summary>Assists interoperation between Windows Presentation Foundation (WPF) and Win32 code. </summary>
public sealed class WindowInteropHelper
{
	private Window _window;

	/// <summary>Gets the window handle for a Windows Presentation Foundation (WPF) window that is used to create this <see cref="T:System.Windows.Interop.WindowInteropHelper" />. </summary>
	/// <returns>The Windows Presentation Foundation (WPF) window handle (HWND).</returns>
	public nint Handle => CriticalHandle;

	internal nint CriticalHandle
	{
		get
		{
			Invariant.Assert(_window != null, "Cannot be null since we verify in the constructor");
			return _window.CriticalHandle;
		}
	}

	/// <summary>Gets or sets the handle of the Windows Presentation Foundation (WPF) owner window. </summary>
	/// <returns>The owner window handle (HWND).</returns>
	public nint Owner
	{
		get
		{
			return _window.OwnerHandle;
		}
		set
		{
			_window.OwnerHandle = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Interop.WindowInteropHelper" /> class for a specified Windows Presentation Foundation (WPF) window. </summary>
	/// <param name="window">A WPF window object.</param>
	public WindowInteropHelper(Window window)
	{
		if (window == null)
		{
			throw new ArgumentNullException("window");
		}
		_window = window;
	}

	/// <summary>Creates the HWND of the window if the HWND has not been created yet.</summary>
	/// <returns>An <see cref="T:System.IntPtr" /> that represents the HWND.</returns>
	public nint EnsureHandle()
	{
		if (CriticalHandle == IntPtr.Zero)
		{
			_window.CreateSourceWindow(duringShow: false);
		}
		return CriticalHandle;
	}
}
