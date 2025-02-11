using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class SelectionItemProviderWrapper : MarshalByRefObject, ISelectionItemProvider
{
	private AutomationPeer _peer;

	private ISelectionItemProvider _iface;

	public bool IsSelected => (bool)ElementUtil.Invoke(_peer, GetIsSelected, null);

	public IRawElementProviderSimple SelectionContainer => (IRawElementProviderSimple)ElementUtil.Invoke(_peer, GetSelectionContainer, null);

	private SelectionItemProviderWrapper(AutomationPeer peer, ISelectionItemProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public void Select()
	{
		ElementUtil.Invoke(_peer, Select, null);
	}

	public void AddToSelection()
	{
		ElementUtil.Invoke(_peer, AddToSelection, null);
	}

	public void RemoveFromSelection()
	{
		ElementUtil.Invoke(_peer, RemoveFromSelection, null);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new SelectionItemProviderWrapper(peer, (ISelectionItemProvider)iface);
	}

	private object Select(object unused)
	{
		_iface.Select();
		return null;
	}

	private object AddToSelection(object unused)
	{
		_iface.AddToSelection();
		return null;
	}

	private object RemoveFromSelection(object unused)
	{
		_iface.RemoveFromSelection();
		return null;
	}

	private object GetIsSelected(object unused)
	{
		return _iface.IsSelected;
	}

	private object GetSelectionContainer(object unused)
	{
		return _iface.SelectionContainer;
	}
}
