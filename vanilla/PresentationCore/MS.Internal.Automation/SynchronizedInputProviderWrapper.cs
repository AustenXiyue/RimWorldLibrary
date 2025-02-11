using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class SynchronizedInputProviderWrapper : MarshalByRefObject, ISynchronizedInputProvider
{
	private AutomationPeer _peer;

	private ISynchronizedInputProvider _iface;

	private SynchronizedInputProviderWrapper(AutomationPeer peer, ISynchronizedInputProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public void StartListening(SynchronizedInputType inputType)
	{
		ElementUtil.Invoke(_peer, StartListening, inputType);
	}

	public void Cancel()
	{
		ElementUtil.Invoke(_peer, Cancel, null);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new SynchronizedInputProviderWrapper(peer, (ISynchronizedInputProvider)iface);
	}

	private object StartListening(object arg)
	{
		_iface.StartListening((SynchronizedInputType)arg);
		return null;
	}

	private object Cancel(object unused)
	{
		_iface.Cancel();
		return null;
	}
}
