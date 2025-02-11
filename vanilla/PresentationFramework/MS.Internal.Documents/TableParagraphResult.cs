using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.PtsHost;

namespace MS.Internal.Documents;

internal sealed class TableParagraphResult : ParagraphResult
{
	private ReadOnlyCollection<ParagraphResult> _paragraphs;

	internal ReadOnlyCollection<ParagraphResult> Paragraphs
	{
		get
		{
			if (_paragraphs == null)
			{
				_paragraphs = ((TableParaClient)_paraClient).GetChildrenParagraphResults(out _hasTextContent);
			}
			Invariant.Assert(_paragraphs != null, "Paragraph collection is empty");
			return _paragraphs;
		}
	}

	internal override bool HasTextContent
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

	internal TableParagraphResult(BaseParaClient paraClient)
		: base(paraClient)
	{
	}

	internal ReadOnlyCollection<ParagraphResult> GetParagraphsFromPoint(Point point, bool snapToText)
	{
		return ((TableParaClient)_paraClient).GetParagraphsFromPoint(point, snapToText);
	}

	internal ReadOnlyCollection<ParagraphResult> GetParagraphsFromPosition(ITextPointer position)
	{
		return ((TableParaClient)_paraClient).GetParagraphsFromPosition(position);
	}

	internal Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition, Rect visibleRect)
	{
		return ((TableParaClient)_paraClient).GetTightBoundingGeometryFromTextPositions(startPosition, endPosition, visibleRect);
	}

	internal CellParaClient GetCellParaClientFromPosition(ITextPointer position)
	{
		return ((TableParaClient)_paraClient).GetCellParaClientFromPosition(position);
	}

	internal CellParaClient GetCellAbove(double suggestedX, int rowGroupIndex, int rowIndex)
	{
		return ((TableParaClient)_paraClient).GetCellAbove(suggestedX, rowGroupIndex, rowIndex);
	}

	internal CellParaClient GetCellBelow(double suggestedX, int rowGroupIndex, int rowIndex)
	{
		return ((TableParaClient)_paraClient).GetCellBelow(suggestedX, rowGroupIndex, rowIndex);
	}

	internal CellInfo GetCellInfoFromPoint(Point point)
	{
		return ((TableParaClient)_paraClient).GetCellInfoFromPoint(point);
	}

	internal Rect GetRectangleFromRowEndPosition(ITextPointer position)
	{
		return ((TableParaClient)_paraClient).GetRectangleFromRowEndPosition(position);
	}
}
