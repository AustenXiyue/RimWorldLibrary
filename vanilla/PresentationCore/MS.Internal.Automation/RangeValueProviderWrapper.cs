using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class RangeValueProviderWrapper : MarshalByRefObject, IRangeValueProvider
{
	private AutomationPeer _peer;

	private IRangeValueProvider _iface;

	public double Value => (double)ElementUtil.Invoke(_peer, GetValue, null);

	public bool IsReadOnly => (bool)ElementUtil.Invoke(_peer, GetIsReadOnly, null);

	public double Maximum => (double)ElementUtil.Invoke(_peer, GetMaximum, null);

	public double Minimum => (double)ElementUtil.Invoke(_peer, GetMinimum, null);

	public double LargeChange => (double)ElementUtil.Invoke(_peer, GetLargeChange, null);

	public double SmallChange => (double)ElementUtil.Invoke(_peer, GetSmallChange, null);

	private RangeValueProviderWrapper(AutomationPeer peer, IRangeValueProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public void SetValue(double val)
	{
		ElementUtil.Invoke(_peer, SetValueInternal, val);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new RangeValueProviderWrapper(peer, (IRangeValueProvider)iface);
	}

	private object SetValueInternal(object arg)
	{
		_iface.SetValue((double)arg);
		return null;
	}

	private object GetValue(object unused)
	{
		return _iface.Value;
	}

	private object GetIsReadOnly(object unused)
	{
		return _iface.IsReadOnly;
	}

	private object GetMaximum(object unused)
	{
		return _iface.Maximum;
	}

	private object GetMinimum(object unused)
	{
		return _iface.Minimum;
	}

	private object GetLargeChange(object unused)
	{
		return _iface.LargeChange;
	}

	private object GetSmallChange(object unused)
	{
		return _iface.SmallChange;
	}
}
