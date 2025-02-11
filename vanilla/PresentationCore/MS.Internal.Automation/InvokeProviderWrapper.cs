using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class InvokeProviderWrapper : MarshalByRefObject, IInvokeProvider
{
	private AutomationPeer _peer;

	private IInvokeProvider _iface;

	private InvokeProviderWrapper(AutomationPeer peer, IInvokeProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public void Invoke()
	{
		ElementUtil.Invoke(_peer, Invoke, null);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new InvokeProviderWrapper(peer, (IInvokeProvider)iface);
	}

	private object Invoke(object unused)
	{
		_iface.Invoke();
		return null;
	}
}
