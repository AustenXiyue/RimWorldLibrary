using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class VirtualizedItemProviderWrapper : MarshalByRefObject, IVirtualizedItemProvider
{
	private AutomationPeer _peer;

	private IVirtualizedItemProvider _iface;

	private VirtualizedItemProviderWrapper(AutomationPeer peer, IVirtualizedItemProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public void Realize()
	{
		ElementUtil.Invoke(_peer, Realize, null);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new VirtualizedItemProviderWrapper(peer, (IVirtualizedItemProvider)iface);
	}

	private object Realize(object unused)
	{
		_iface.Realize();
		return null;
	}
}
