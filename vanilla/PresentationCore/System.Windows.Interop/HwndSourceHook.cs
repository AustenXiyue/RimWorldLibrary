namespace System.Windows.Interop;

/// <summary> Represents the method that handles Win32 window messages. </summary>
/// <returns>The appropriate return value depends on the particular message. See the message documentation details for the Win32 message being handled.</returns>
/// <param name="hwnd">The window handle.</param>
/// <param name="msg">The message ID.</param>
/// <param name="wParam">The message's wParam value.</param>
/// <param name="lParam">The message's lParam value.</param>
/// <param name="handled">A value that indicates whether the message was handled. Set the value to true if the message was handled; otherwise, false.</param>
public delegate nint HwndSourceHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled);
