using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class GridItemProviderWrapper : MarshalByRefObject, IGridItemProvider
{
	private AutomationPeer _peer;

	private IGridItemProvider _iface;

	public int Row => (int)ElementUtil.Invoke(_peer, GetRow, null);

	public int Column => (int)ElementUtil.Invoke(_peer, GetColumn, null);

	public int RowSpan => (int)ElementUtil.Invoke(_peer, GetRowSpan, null);

	public int ColumnSpan => (int)ElementUtil.Invoke(_peer, GetColumnSpan, null);

	public IRawElementProviderSimple ContainingGrid => (IRawElementProviderSimple)ElementUtil.Invoke(_peer, GetContainingGrid, null);

	private GridItemProviderWrapper(AutomationPeer peer, IGridItemProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new GridItemProviderWrapper(peer, (IGridItemProvider)iface);
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
}
