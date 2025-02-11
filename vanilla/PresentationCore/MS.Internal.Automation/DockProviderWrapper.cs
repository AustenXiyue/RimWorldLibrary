using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class DockProviderWrapper : MarshalByRefObject, IDockProvider
{
	private AutomationPeer _peer;

	private IDockProvider _iface;

	public DockPosition DockPosition => (DockPosition)ElementUtil.Invoke(_peer, GetDockPosition, null);

	private DockProviderWrapper(AutomationPeer peer, IDockProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public void SetDockPosition(DockPosition dockPosition)
	{
		ElementUtil.Invoke(_peer, SetDockPosition, dockPosition);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new DockProviderWrapper(peer, (IDockProvider)iface);
	}

	private object SetDockPosition(object arg)
	{
		_iface.SetDockPosition((DockPosition)arg);
		return null;
	}

	private object GetDockPosition(object unused)
	{
		return _iface.DockPosition;
	}
}
