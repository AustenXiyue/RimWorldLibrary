using System.Collections;

namespace System.Windows.Navigation;

internal class LimitedJournalEntryStackEnumerator : IEnumerator
{
	private uint _itemsReturned;

	private uint _viewLimit;

	private IEnumerator _ienum;

	public object Current => _ienum.Current;

	internal LimitedJournalEntryStackEnumerator(IEnumerable ieble, uint viewLimit)
	{
		_ienum = ieble.GetEnumerator();
		_viewLimit = viewLimit;
	}

	public void Reset()
	{
		_itemsReturned = 0u;
		_ienum.Reset();
	}

	public bool MoveNext()
	{
		bool flag;
		if (_itemsReturned == _viewLimit)
		{
			flag = false;
		}
		else
		{
			flag = _ienum.MoveNext();
			if (flag)
			{
				_itemsReturned++;
			}
		}
		return flag;
	}
}
