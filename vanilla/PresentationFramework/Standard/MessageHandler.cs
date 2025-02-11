namespace Standard;

internal delegate nint MessageHandler(WM uMsg, nint wParam, nint lParam, out bool handled);
