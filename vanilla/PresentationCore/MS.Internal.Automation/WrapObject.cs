using System.Windows.Automation.Peers;

namespace MS.Internal.Automation;

internal delegate object WrapObject(AutomationPeer peer, object iface, nint hwnd);
