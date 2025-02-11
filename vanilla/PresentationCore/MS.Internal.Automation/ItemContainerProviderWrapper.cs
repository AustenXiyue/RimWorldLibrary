using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class ItemContainerProviderWrapper : MarshalByRefObject, IItemContainerProvider
{
	private AutomationPeer _peer;

	private IItemContainerProvider _iface;

	private ItemContainerProviderWrapper(AutomationPeer peer, IItemContainerProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public IRawElementProviderSimple FindItemByProperty(IRawElementProviderSimple startAfter, int propertyId, object value)
	{
		object[] arg = new object[3] { startAfter, propertyId, value };
		return (IRawElementProviderSimple)ElementUtil.Invoke(_peer, FindItemByProperty, arg);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new ItemContainerProviderWrapper(peer, (IItemContainerProvider)iface);
	}

	private object FindItemByProperty(object arg)
	{
		object[] obj = (object[])arg;
		IRawElementProviderSimple startAfter = (IRawElementProviderSimple)obj[0];
		int propertyId = (int)obj[1];
		object value = obj[2];
		return _iface.FindItemByProperty(startAfter, propertyId, value);
	}
}
