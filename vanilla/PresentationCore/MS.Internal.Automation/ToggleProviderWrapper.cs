using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class ToggleProviderWrapper : MarshalByRefObject, IToggleProvider
{
	private AutomationPeer _peer;

	private IToggleProvider _iface;

	public ToggleState ToggleState => (ToggleState)ElementUtil.Invoke(_peer, GetToggleState, null);

	private ToggleProviderWrapper(AutomationPeer peer, IToggleProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public void Toggle()
	{
		ElementUtil.Invoke(_peer, ToggleInternal, null);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new ToggleProviderWrapper(peer, (IToggleProvider)iface);
	}

	private object ToggleInternal(object unused)
	{
		_iface.Toggle();
		return null;
	}

	private object GetToggleState(object unused)
	{
		return _iface.ToggleState;
	}
}
