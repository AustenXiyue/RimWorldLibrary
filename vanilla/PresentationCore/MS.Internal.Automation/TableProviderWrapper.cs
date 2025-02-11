using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class TableProviderWrapper : MarshalByRefObject, ITableProvider, IGridProvider
{
	private AutomationPeer _peer;

	private ITableProvider _iface;

	public int RowCount => (int)ElementUtil.Invoke(_peer, GetRowCount, null);

	public int ColumnCount => (int)ElementUtil.Invoke(_peer, GetColumnCount, null);

	public RowOrColumnMajor RowOrColumnMajor => (RowOrColumnMajor)ElementUtil.Invoke(_peer, GetRowOrColumnMajor, null);

	private TableProviderWrapper(AutomationPeer peer, ITableProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public IRawElementProviderSimple GetItem(int row, int column)
	{
		return (IRawElementProviderSimple)ElementUtil.Invoke(_peer, GetItem, new int[2] { row, column });
	}

	public IRawElementProviderSimple[] GetRowHeaders()
	{
		return (IRawElementProviderSimple[])ElementUtil.Invoke(_peer, GetRowHeaders, null);
	}

	public IRawElementProviderSimple[] GetColumnHeaders()
	{
		return (IRawElementProviderSimple[])ElementUtil.Invoke(_peer, GetColumnHeaders, null);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new TableProviderWrapper(peer, (ITableProvider)iface);
	}

	private object GetItem(object arg)
	{
		int[] array = (int[])arg;
		return _iface.GetItem(array[0], array[1]);
	}

	private object GetRowCount(object unused)
	{
		return _iface.RowCount;
	}

	private object GetColumnCount(object unused)
	{
		return _iface.ColumnCount;
	}

	private object GetRowHeaders(object unused)
	{
		return _iface.GetRowHeaders();
	}

	private object GetColumnHeaders(object unused)
	{
		return _iface.GetColumnHeaders();
	}

	private object GetRowOrColumnMajor(object unused)
	{
		return _iface.RowOrColumnMajor;
	}
}
