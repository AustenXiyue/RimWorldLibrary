namespace Standard;

internal delegate nint WndProcHook(nint hwnd, WM uMsg, nint wParam, nint lParam, ref bool handled);
