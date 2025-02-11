using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class GridProviderWrapper : MarshalByRefObject, IGridProvider
{
	private AutomationPeer _peer;

	private IGridProvider _iface;

	public int RowCount => (int)ElementUtil.Invoke(_peer, GetRowCount, null);

	public int ColumnCount => (int)ElementUtil.Invoke(_peer, GetColumnCount, null);

	private GridProviderWrapper(AutomationPeer peer, IGridProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public IRawElementProviderSimple GetItem(int row, int column)
	{
		return (IRawElementProviderSimple)ElementUtil.Invoke(_peer, GetItem, new int[2] { row, column });
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new GridProviderWrapper(peer, (IGridProvider)iface);
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
}
