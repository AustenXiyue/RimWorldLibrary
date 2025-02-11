using System.Collections.Generic;
using System.Windows;

namespace MS.Internal.Ink;

internal abstract class ElementsClipboardData : ClipboardData
{
	private List<UIElement> _elementList;

	internal List<UIElement> Elements
	{
		get
		{
			if (ElementList != null)
			{
				return _elementList;
			}
			return new List<UIElement>();
		}
	}

	protected List<UIElement> ElementList
	{
		get
		{
			return _elementList;
		}
		set
		{
			_elementList = value;
		}
	}

	internal ElementsClipboardData()
	{
	}

	internal ElementsClipboardData(UIElement[] elements)
	{
		if (elements != null)
		{
			ElementList = new List<UIElement>(elements);
		}
	}
}
