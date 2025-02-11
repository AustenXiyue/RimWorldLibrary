using System.Collections;
using System.Windows.Controls;

namespace MS.Internal.Controls;

internal class HeaderedItemsModelTreeEnumerator : ModelTreeEnumerator
{
	private HeaderedItemsControl _owner;

	private IEnumerator _items;

	protected override object Current
	{
		get
		{
			if (base.Index > 0)
			{
				return _items.Current;
			}
			return base.Current;
		}
	}

	protected override bool IsUnchanged => base.Content == _owner.Header;

	internal HeaderedItemsModelTreeEnumerator(HeaderedItemsControl headeredItemsControl, IEnumerator items, object header)
		: base(header)
	{
		_owner = headeredItemsControl;
		_items = items;
	}

	protected override bool MoveNext()
	{
		if (base.Index >= 0)
		{
			base.Index++;
			return _items.MoveNext();
		}
		return base.MoveNext();
	}

	protected override void Reset()
	{
		base.Reset();
		_items.Reset();
	}
}
