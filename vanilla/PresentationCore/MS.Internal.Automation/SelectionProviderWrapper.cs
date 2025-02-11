using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class SelectionProviderWrapper : MarshalByRefObject, ISelectionProvider
{
	private AutomationPeer _peer;

	private ISelectionProvider _iface;

	public bool CanSelectMultiple => (bool)ElementUtil.Invoke(_peer, GetCanSelectMultiple, null);

	public bool IsSelectionRequired => (bool)ElementUtil.Invoke(_peer, GetIsSelectionRequired, null);

	private SelectionProviderWrapper(AutomationPeer peer, ISelectionProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public IRawElementProviderSimple[] GetSelection()
	{
		return (IRawElementProviderSimple[])ElementUtil.Invoke(_peer, GetSelection, null);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new SelectionProviderWrapper(peer, (ISelectionProvider)iface);
	}

	private object GetSelection(object unused)
	{
		return _iface.GetSelection();
	}

	private object GetCanSelectMultiple(object unused)
	{
		return _iface.CanSelectMultiple;
	}

	private object GetIsSelectionRequired(object unused)
	{
		return _iface.IsSelectionRequired;
	}
}
