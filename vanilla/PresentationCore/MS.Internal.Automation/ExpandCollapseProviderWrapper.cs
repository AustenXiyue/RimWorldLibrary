using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class ExpandCollapseProviderWrapper : MarshalByRefObject, IExpandCollapseProvider
{
	private AutomationPeer _peer;

	private IExpandCollapseProvider _iface;

	public ExpandCollapseState ExpandCollapseState => (ExpandCollapseState)ElementUtil.Invoke(_peer, GetExpandCollapseState, null);

	private ExpandCollapseProviderWrapper(AutomationPeer peer, IExpandCollapseProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public void Expand()
	{
		ElementUtil.Invoke(_peer, Expand, null);
	}

	public void Collapse()
	{
		ElementUtil.Invoke(_peer, Collapse, null);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new ExpandCollapseProviderWrapper(peer, (IExpandCollapseProvider)iface);
	}

	private object Expand(object unused)
	{
		_iface.Expand();
		return null;
	}

	private object Collapse(object unused)
	{
		_iface.Collapse();
		return null;
	}

	private object GetExpandCollapseState(object unused)
	{
		return _iface.ExpandCollapseState;
	}
}
