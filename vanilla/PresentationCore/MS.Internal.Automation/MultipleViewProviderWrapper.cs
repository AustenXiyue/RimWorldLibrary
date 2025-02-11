using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class MultipleViewProviderWrapper : MarshalByRefObject, IMultipleViewProvider
{
	private AutomationPeer _peer;

	private IMultipleViewProvider _iface;

	public int CurrentView => (int)ElementUtil.Invoke(_peer, GetCurrentView, null);

	private MultipleViewProviderWrapper(AutomationPeer peer, IMultipleViewProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public string GetViewName(int viewID)
	{
		return (string)ElementUtil.Invoke(_peer, GetViewName, viewID);
	}

	public void SetCurrentView(int viewID)
	{
		ElementUtil.Invoke(_peer, SetCurrentView, viewID);
	}

	public int[] GetSupportedViews()
	{
		return (int[])ElementUtil.Invoke(_peer, GetSupportedViews, null);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new MultipleViewProviderWrapper(peer, (IMultipleViewProvider)iface);
	}

	private object GetViewName(object arg)
	{
		return _iface.GetViewName((int)arg);
	}

	private object SetCurrentView(object arg)
	{
		_iface.SetCurrentView((int)arg);
		return null;
	}

	private object GetCurrentView(object unused)
	{
		return _iface.CurrentView;
	}

	private object GetSupportedViews(object unused)
	{
		return _iface.GetSupportedViews();
	}
}
