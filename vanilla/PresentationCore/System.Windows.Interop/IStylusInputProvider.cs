using System.Windows.Input;
using MS.Internal.Interop;

namespace System.Windows.Interop;

internal interface IStylusInputProvider : IInputProvider, IDisposable
{
	nint FilterMessage(nint hwnd, WindowMessage msg, nint wParam, nint lParam, ref bool handled);
}
