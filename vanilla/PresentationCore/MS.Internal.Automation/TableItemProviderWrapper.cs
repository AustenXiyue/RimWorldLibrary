using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class TableItemProviderWrapper : MarshalByRefObject, ITableItemProvider, IGridItemProvider
{
	private AutomationPeer _peer;

	private ITableItemProvider _iface;

	public int Row => (int)ElementUtil.Invoke(_peer, GetRow, null);

	public int Column => (int)ElementUtil.Invoke(_peer, GetColumn, null);

	public int RowSpan => (int)ElementUtil.Invoke(_peer, GetRowSpan, null);

	public int ColumnSpan => (int)ElementUtil.Invoke(_peer, GetColumnSpan, null);

	public IRawElementProviderSimple ContainingGrid => (IRawElementProviderSimple)ElementUtil.Invoke(_peer, GetContainingGrid, null);

	private TableItemProviderWrapper(AutomationPeer peer, ITableItemProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public IRawElementProviderSimple[] GetRowHeaderItems()
	{
		return (IRawElementProviderSimple[])ElementUtil.Invoke(_peer, GetRowHeaderItems, null);
	}

	public IRawElementProviderSimple[] GetColumnHeaderItems()
	{
		return (IRawElementProviderSimple[])ElementUtil.Invoke(_peer, GetColumnHeaderItems, null);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new TableItemProviderWrapper(peer, (ITableItemProvider)iface);
	}

	private object GetRow(object unused)
	{
		return _iface.Row;
	}

	private object GetColumn(object unused)
	{
		return _iface.Column;
	}

	private object GetRowSpan(object unused)
	{
		return _iface.RowSpan;
	}

	private object GetColumnSpan(object unused)
	{
		return _iface.ColumnSpan;
	}

	private object GetContainingGrid(object unused)
	{
		return _iface.ContainingGrid;
	}

	private object GetRowHeaderItems(object unused)
	{
		return _iface.GetRowHeaderItems();
	}

	private object GetColumnHeaderItems(object unused)
	{
		return _iface.GetColumnHeaderItems();
	}
}
