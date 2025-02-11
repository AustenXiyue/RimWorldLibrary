using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class ValueProviderWrapper : MarshalByRefObject, IValueProvider
{
	private AutomationPeer _peer;

	private IValueProvider _iface;

	public string Value => (string)ElementUtil.Invoke(_peer, GetValue, null);

	public bool IsReadOnly => (bool)ElementUtil.Invoke(_peer, GetIsReadOnly, null);

	private ValueProviderWrapper(AutomationPeer peer, IValueProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public void SetValue(string val)
	{
		ElementUtil.Invoke(_peer, SetValueInternal, val);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new ValueProviderWrapper(peer, (IValueProvider)iface);
	}

	private object SetValueInternal(object arg)
	{
		_iface.SetValue((string)arg);
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
}
