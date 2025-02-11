using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class ScrollItemProviderWrapper : MarshalByRefObject, IScrollItemProvider
{
	private AutomationPeer _peer;

	private IScrollItemProvider _iface;

	private ScrollItemProviderWrapper(AutomationPeer peer, IScrollItemProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public void ScrollIntoView()
	{
		ElementUtil.Invoke(_peer, ScrollIntoView, null);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new ScrollItemProviderWrapper(peer, (IScrollItemProvider)iface);
	}

	private object ScrollIntoView(object unused)
	{
		_iface.ScrollIntoView();
		return null;
	}
}
