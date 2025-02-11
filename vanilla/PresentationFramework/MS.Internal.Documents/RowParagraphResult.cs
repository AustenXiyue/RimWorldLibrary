using System.Collections.ObjectModel;
using System.Windows;
using MS.Internal.PtsHost;

namespace MS.Internal.Documents;

internal sealed class RowParagraphResult : ParagraphResult
{
	private ReadOnlyCollection<ParagraphResult> _cells;

	private int _index;

	internal ReadOnlyCollection<ParagraphResult> CellParagraphs
	{
		get
		{
			if (_cells == null)
			{
				_cells = ((TableParaClient)_paraClient).GetChildrenParagraphResultsForRow(_index, out _hasTextContent);
			}
			Invariant.Assert(_cells != null, "Paragraph collection is empty");
			return _cells;
		}
	}

	internal override bool HasTextContent
	{
		get
		{
			if (_cells == null)
			{
				_ = CellParagraphs;
			}
			return _hasTextContent;
		}
	}

	internal RowParagraphResult(BaseParaClient paraClient, int index, Rect rowRect, RowParagraph rowParagraph)
		: base(paraClient, rowRect, rowParagraph.Element)
	{
		_index = index;
	}
}
