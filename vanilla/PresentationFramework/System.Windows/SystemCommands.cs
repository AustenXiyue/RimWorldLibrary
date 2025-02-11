using System.Windows.Input;
using System.Windows.Interop;
using Standard;

namespace System.Windows;

/// <summary>Defines routed commands that are common to window management.</summary>
public static class SystemCommands
{
	/// <summary>Gets a command that closes a window.</summary>
	/// <returns>A command that closes a window.</returns>
	public static RoutedCommand CloseWindowCommand { get; private set; }

	/// <summary>Gets a command that maximizes a window.</summary>
	/// <returns>A command that maximizes a window.</returns>
	public static RoutedCommand MaximizeWindowCommand { get; private set; }

	/// <summary>Gets a command that maximizes a window.</summary>
	/// <returns>A command that maximizes a window.</returns>
	public static RoutedCommand MinimizeWindowCommand { get; private set; }

	/// <summary>Gets a command that restores a window.</summary>
	/// <returns>A command that restores a window.</returns>
	public static RoutedCommand RestoreWindowCommand { get; private set; }

	/// <summary>Gets a command that displays the system menu.</summary>
	/// <returns>A command that displays the system menu.</returns>
	public static RoutedCommand ShowSystemMenuCommand { get; private set; }

	static SystemCommands()
	{
		CloseWindowCommand = new RoutedCommand("CloseWindow", typeof(SystemCommands));
		MaximizeWindowCommand = new RoutedCommand("MaximizeWindow", typeof(SystemCommands));
		MinimizeWindowCommand = new RoutedCommand("MinimizeWindow", typeof(SystemCommands));
		RestoreWindowCommand = new RoutedCommand("RestoreWindow", typeof(SystemCommands));
		ShowSystemMenuCommand = new RoutedCommand("ShowSystemMenu", typeof(SystemCommands));
	}

	private static void _PostSystemCommand(Window window, SC command)
	{
		nint handle = new WindowInteropHelper(window).Handle;
		if (handle != IntPtr.Zero && NativeMethods.IsWindow(handle))
		{
			NativeMethods.PostMessage(handle, WM.SYSCOMMAND, new IntPtr((int)command), IntPtr.Zero);
		}
	}

	/// <summary>Closes the specified window.</summary>
	/// <param name="window">The window to close.</param>
	public static void CloseWindow(Window window)
	{
		Verify.IsNotNull(window, "window");
		_PostSystemCommand(window, SC.CLOSE);
	}

	/// <summary>Maximizes the specified window.</summary>
	/// <param name="window">The window to maximize.</param>
	public static void MaximizeWindow(Window window)
	{
		Verify.IsNotNull(window, "window");
		_PostSystemCommand(window, SC.MAXIMIZE);
	}

	/// <summary>Minimizes the specified window.</summary>
	/// <param name="window">The window to minimize.</param>
	public static void MinimizeWindow(Window window)
	{
		Verify.IsNotNull(window, "window");
		_PostSystemCommand(window, SC.MINIMIZE);
	}

	/// <summary>Restores the specified widow.</summary>
	/// <param name="window">The window to restore.</param>
	public static void RestoreWindow(Window window)
	{
		Verify.IsNotNull(window, "window");
		_PostSystemCommand(window, SC.RESTORE);
	}

	/// <summary>Displays the system menu for the specified window.</summary>
	/// <param name="window">The window to have its system menu displayed.</param>
	/// <param name="screenLocation">The location of the system menu.</param>
	public static void ShowSystemMenu(Window window, Point screenLocation)
	{
		Verify.IsNotNull(window, "window");
		DpiScale dpi = window.GetDpi();
		ShowSystemMenuPhysicalCoordinates(window, DpiHelper.LogicalPixelsToDevice(screenLocation, dpi.DpiScaleX, dpi.DpiScaleY));
	}

	internal static void ShowSystemMenuPhysicalCoordinates(Window window, Point physicalScreenLocation)
	{
		Verify.IsNotNull(window, "window");
		nint handle = new WindowInteropHelper(window).Handle;
		if (handle != IntPtr.Zero && NativeMethods.IsWindow(handle))
		{
			uint num = NativeMethods.TrackPopupMenuEx(NativeMethods.GetSystemMenu(handle, bRevert: false), 258u, (int)physicalScreenLocation.X, (int)physicalScreenLocation.Y, handle, IntPtr.Zero);
			if (num != 0)
			{
				NativeMethods.PostMessage(handle, WM.SYSCOMMAND, new IntPtr(num), IntPtr.Zero);
			}
		}
	}
}
