using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using MS.Internal.PtsHost;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.Documents;

internal sealed class ColumnResult
{
	private readonly FlowDocumentPage _page;

	private readonly BaseParaClient _subpage;

	private readonly nint _columnHandle;

	private readonly Rect _layoutBox;

	private readonly Vector _columnOffset;

	private TextContentRange _contentRange;

	private ReadOnlyCollection<ParagraphResult> _paragraphs;

	private bool _hasTextContent;

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

	internal ReadOnlyCollection<ParagraphResult> Paragraphs
	{
		get
		{
			if (_paragraphs == null)
			{
				_hasTextContent = false;
				if (_page != null)
				{
					_paragraphs = _page.GetParagraphResultsFromColumn(_columnHandle, _columnOffset, out _hasTextContent);
				}
				else if (_subpage is FigureParaClient)
				{
					_paragraphs = ((FigureParaClient)_subpage).GetParagraphResultsFromColumn(_columnHandle, _columnOffset, out _hasTextContent);
				}
				else if (_subpage is FloaterParaClient)
				{
					_paragraphs = ((FloaterParaClient)_subpage).GetParagraphResultsFromColumn(_columnHandle, _columnOffset, out _hasTextContent);
				}
				else if (_subpage is SubpageParaClient)
				{
					_paragraphs = ((SubpageParaClient)_subpage).GetParagraphResultsFromColumn(_columnHandle, _columnOffset, out _hasTextContent);
				}
				else
				{
					Invariant.Assert(condition: false, "Expecting Subpage, Figure or Floater ParaClient");
				}
			}
			return _paragraphs;
		}
	}

	internal bool HasTextContent
	{
		get
		{
			if (_paragraphs == null)
			{
				_ = Paragraphs;
			}
			return _hasTextContent;
		}
	}

	internal TextContentRange TextContentRange
	{
		get
		{
			EnsureTextContentRange();
			return _contentRange;
		}
	}

	internal ColumnResult(FlowDocumentPage page, ref PTS.FSTRACKDESCRIPTION trackDesc, Vector contentOffset)
	{
		_page = page;
		_columnHandle = trackDesc.pfstrack;
		_layoutBox = new Rect(TextDpi.FromTextDpi(trackDesc.fsrc.u), TextDpi.FromTextDpi(trackDesc.fsrc.v), TextDpi.FromTextDpi(trackDesc.fsrc.du), TextDpi.FromTextDpi(trackDesc.fsrc.dv));
		_layoutBox.X += contentOffset.X;
		_layoutBox.Y += contentOffset.Y;
		_columnOffset = new Vector(TextDpi.FromTextDpi(trackDesc.fsrc.u), TextDpi.FromTextDpi(trackDesc.fsrc.v));
		_hasTextContent = false;
	}

	internal ColumnResult(BaseParaClient subpage, ref PTS.FSTRACKDESCRIPTION trackDesc, Vector contentOffset)
	{
		Invariant.Assert(subpage is SubpageParaClient || subpage is FigureParaClient || subpage is FloaterParaClient);
		_subpage = subpage;
		_columnHandle = trackDesc.pfstrack;
		_layoutBox = new Rect(TextDpi.FromTextDpi(trackDesc.fsrc.u), TextDpi.FromTextDpi(trackDesc.fsrc.v), TextDpi.FromTextDpi(trackDesc.fsrc.du), TextDpi.FromTextDpi(trackDesc.fsrc.dv));
		_layoutBox.X += contentOffset.X;
		_layoutBox.Y += contentOffset.Y;
		_columnOffset = new Vector(TextDpi.FromTextDpi(trackDesc.fsrc.u), TextDpi.FromTextDpi(trackDesc.fsrc.v));
	}

	internal bool Contains(ITextPointer position, bool strict)
	{
		EnsureTextContentRange();
		return _contentRange.Contains(position, strict);
	}

	private void EnsureTextContentRange()
	{
		if (_contentRange == null)
		{
			if (_page != null)
			{
				_contentRange = _page.GetTextContentRangeFromColumn(_columnHandle);
			}
			else if (_subpage is FigureParaClient)
			{
				_contentRange = ((FigureParaClient)_subpage).GetTextContentRangeFromColumn(_columnHandle);
			}
			else if (_subpage is FloaterParaClient)
			{
				_contentRange = ((FloaterParaClient)_subpage).GetTextContentRangeFromColumn(_columnHandle);
			}
			else if (_subpage is SubpageParaClient)
			{
				_contentRange = ((SubpageParaClient)_subpage).GetTextContentRangeFromColumn(_columnHandle);
			}
			else
			{
				Invariant.Assert(condition: false, "Expecting Subpage, Figure or Floater ParaClient");
			}
			Invariant.Assert(_contentRange != null);
		}
	}
}
