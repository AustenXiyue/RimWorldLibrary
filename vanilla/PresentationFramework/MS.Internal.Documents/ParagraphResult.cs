using System.Windows;
using System.Windows.Documents;
using MS.Internal.PtsHost;

namespace MS.Internal.Documents;

internal abstract class ParagraphResult
{
	protected readonly BaseParaClient _paraClient;

	protected readonly Rect _layoutBox;

	protected readonly DependencyObject _element;

	private TextContentRange _contentRange;

	protected bool _hasTextContent;

	internal ITextPointer StartPosition
	{
		get
		{
			EnsureTextContentRange();
			return _contentRange.StartPosition;
		}
	}

	internal ITextPointer EndPosition
	{
		get
		{
			EnsureTextContentRange();
			return _contentRange.EndPosition;
		}
	}

	internal Rect LayoutBox => _layoutBox;

	internal DependencyObject Element => _element;

	internal virtual bool HasTextContent => false;

	internal ParagraphResult(BaseParaClient paraClient)
	{
		_paraClient = paraClient;
		_layoutBox = _paraClient.Rect.FromTextDpi();
		_element = paraClient.Paragraph.Element;
	}

	internal ParagraphResult(BaseParaClient paraClient, Rect layoutBox, DependencyObject element)
		: this(paraClient)
	{
		_layoutBox = layoutBox;
		_element = element;
	}

	internal virtual bool Contains(ITextPointer position, bool strict)
	{
		EnsureTextContentRange();
		return _contentRange.Contains(position, strict);
	}

	private void EnsureTextContentRange()
	{
		if (_contentRange == null)
		{
			_contentRange = _paraClient.GetTextContentRange();
			Invariant.Assert(_contentRange != null);
		}
	}
}
