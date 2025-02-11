using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class ScrollProviderWrapper : MarshalByRefObject, IScrollProvider
{
	private AutomationPeer _peer;

	private IScrollProvider _iface;

	public double HorizontalScrollPercent => (double)ElementUtil.Invoke(_peer, GetHorizontalScrollPercent, null);

	public double VerticalScrollPercent => (double)ElementUtil.Invoke(_peer, GetVerticalScrollPercent, null);

	public double HorizontalViewSize => (double)ElementUtil.Invoke(_peer, GetHorizontalViewSize, null);

	public double VerticalViewSize => (double)ElementUtil.Invoke(_peer, GetVerticalViewSize, null);

	public bool HorizontallyScrollable => (bool)ElementUtil.Invoke(_peer, GetHorizontallyScrollable, null);

	public bool VerticallyScrollable => (bool)ElementUtil.Invoke(_peer, GetVerticallyScrollable, null);

	private ScrollProviderWrapper(AutomationPeer peer, IScrollProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public void Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount)
	{
		ElementUtil.Invoke(_peer, Scroll, new ScrollAmount[2] { horizontalAmount, verticalAmount });
	}

	public void SetScrollPercent(double horizontalPercent, double verticalPercent)
	{
		ElementUtil.Invoke(_peer, SetScrollPercent, new double[2] { horizontalPercent, verticalPercent });
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new ScrollProviderWrapper(peer, (IScrollProvider)iface);
	}

	private object Scroll(object arg)
	{
		ScrollAmount[] array = (ScrollAmount[])arg;
		_iface.Scroll(array[0], array[1]);
		return null;
	}

	private object SetScrollPercent(object arg)
	{
		double[] array = (double[])arg;
		_iface.SetScrollPercent(array[0], array[1]);
		return null;
	}

	private object GetHorizontalScrollPercent(object unused)
	{
		return _iface.HorizontalScrollPercent;
	}

	private object GetVerticalScrollPercent(object unused)
	{
		return _iface.VerticalScrollPercent;
	}

	private object GetHorizontalViewSize(object unused)
	{
		return _iface.HorizontalViewSize;
	}

	private object GetVerticalViewSize(object unused)
	{
		return _iface.VerticalViewSize;
	}

	private object GetHorizontallyScrollable(object unused)
	{
		return _iface.HorizontallyScrollable;
	}

	private object GetVerticallyScrollable(object unused)
	{
		return _iface.VerticallyScrollable;
	}
}
