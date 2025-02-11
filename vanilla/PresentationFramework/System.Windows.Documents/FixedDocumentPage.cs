using System.Windows.Media;

namespace System.Windows.Documents;

internal sealed class FixedDocumentPage : DocumentPage, IServiceProvider
{
	private readonly FixedDocument _panel;

	private readonly FixedPage _page;

	private readonly int _index;

	private bool _layedOut;

	private FixedTextView _textView;

	public override Visual Visual
	{
		get
		{
			if (!_layedOut)
			{
				_layedOut = true;
				if (base.Visual is UIElement uIElement)
				{
					uIElement.Measure(base.Size);
					uIElement.Arrange(new Rect(base.Size));
				}
			}
			return base.Visual;
		}
	}

	internal ContentPosition ContentPosition
	{
		get
		{
			FlowPosition pageStartFlowPosition = _panel.FixedContainer.FixedTextBuilder.GetPageStartFlowPosition(_index);
			return new FixedTextPointer(mutable: true, LogicalDirection.Forward, pageStartFlowPosition);
		}
	}

	internal FixedPage FixedPage => _page;

	internal int PageIndex => _panel.GetIndexOfPage(_page);

	internal FixedTextView TextView
	{
		get
		{
			if (_textView == null)
			{
				_textView = new FixedTextView(this);
			}
			return _textView;
		}
	}

	internal FixedDocument Owner => _panel;

	internal FixedTextContainer TextContainer => _panel.FixedContainer;

	internal FixedDocumentPage(FixedDocument panel, FixedPage page, Size fixedSize, int index)
		: base(page, fixedSize, new Rect(fixedSize), new Rect(fixedSize))
	{
		_panel = panel;
		_page = page;
		_index = index;
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (serviceType == typeof(ITextView))
		{
			return TextView;
		}
		return null;
	}
}
