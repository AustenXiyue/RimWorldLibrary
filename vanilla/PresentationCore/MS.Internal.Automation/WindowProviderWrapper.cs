using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class WindowProviderWrapper : MarshalByRefObject, IWindowProvider
{
	private AutomationPeer _peer;

	private IWindowProvider _iface;

	public bool Maximizable => (bool)ElementUtil.Invoke(_peer, GetMaximizable, null);

	public bool Minimizable => (bool)ElementUtil.Invoke(_peer, GetMinimizable, null);

	public bool IsModal => (bool)ElementUtil.Invoke(_peer, GetIsModal, null);

	public WindowVisualState VisualState => (WindowVisualState)ElementUtil.Invoke(_peer, GetVisualState, null);

	public WindowInteractionState InteractionState => (WindowInteractionState)ElementUtil.Invoke(_peer, GetInteractionState, null);

	public bool IsTopmost => (bool)ElementUtil.Invoke(_peer, GetIsTopmost, null);

	private WindowProviderWrapper(AutomationPeer peer, IWindowProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public void SetVisualState(WindowVisualState state)
	{
		ElementUtil.Invoke(_peer, SetVisualState, state);
	}

	public void Close()
	{
		ElementUtil.Invoke(_peer, Close, null);
	}

	public bool WaitForInputIdle(int milliseconds)
	{
		return (bool)ElementUtil.Invoke(_peer, WaitForInputIdle, milliseconds);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new WindowProviderWrapper(peer, (IWindowProvider)iface);
	}

	private object SetVisualState(object arg)
	{
		_iface.SetVisualState((WindowVisualState)arg);
		return null;
	}

	private object WaitForInputIdle(object arg)
	{
		return _iface.WaitForInputIdle((int)arg);
	}

	private object Close(object unused)
	{
		_iface.Close();
		return null;
	}

	private object GetMaximizable(object unused)
	{
		return _iface.Maximizable;
	}

	private object GetMinimizable(object unused)
	{
		return _iface.Minimizable;
	}

	private object GetIsModal(object unused)
	{
		return _iface.IsModal;
	}

	private object GetVisualState(object unused)
	{
		return _iface.VisualState;
	}

	private object GetInteractionState(object unused)
	{
		return _iface.InteractionState;
	}

	private object GetIsTopmost(object unused)
	{
		return _iface.IsTopmost;
	}
}
