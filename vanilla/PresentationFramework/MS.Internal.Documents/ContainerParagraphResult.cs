using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.PtsHost;

namespace MS.Internal.Documents;

internal sealed class ContainerParagraphResult : ParagraphResult
{
	private ReadOnlyCollection<ParagraphResult> _paragraphs;

	internal ReadOnlyCollection<ParagraphResult> Paragraphs
	{
		get
		{
			if (_paragraphs == null)
			{
				_paragraphs = ((ContainerParaClient)_paraClient).GetChildrenParagraphResults(out _hasTextContent);
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

	internal ContainerParagraphResult(ContainerParaClient paraClient)
		: base(paraClient)
	{
	}

	internal Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition, Rect visibleRect)
	{
		return ((ContainerParaClient)_paraClient).GetTightBoundingGeometryFromTextPositions(startPosition, endPosition, visibleRect);
	}
}
