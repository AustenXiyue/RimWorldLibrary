using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.PtsHost;
using MS.Internal.Text;

namespace MS.Internal.Documents;

internal sealed class FloaterParagraphResult : FloaterBaseParagraphResult
{
	private ReadOnlyCollection<ColumnResult> _columns;

	private ReadOnlyCollection<ParagraphResult> _floatingElements;

	internal ReadOnlyCollection<ColumnResult> Columns
	{
		get
		{
			if (_columns == null)
			{
				_columns = ((FloaterParaClient)_paraClient).GetColumnResults(out _hasTextContent);
				Invariant.Assert(_columns != null, "Columns collection is null");
			}
			return _columns;
		}
	}

	internal override bool HasTextContent
	{
		get
		{
			if (_columns == null)
			{
				_ = Columns;
			}
			return _hasTextContent;
		}
	}

	internal ReadOnlyCollection<ParagraphResult> FloatingElements
	{
		get
		{
			if (_floatingElements == null)
			{
				_floatingElements = ((FloaterParaClient)_paraClient).FloatingElementResults;
				Invariant.Assert(_floatingElements != null, "Floating elements collection is null");
			}
			return _floatingElements;
		}
	}

	internal Vector ContentOffset
	{
		get
		{
			MbpInfo mbpInfo = MbpInfo.FromElement(_paraClient.Paragraph.Element, _paraClient.Paragraph.StructuralCache.TextFormatterHost.PixelsPerDip);
			return new Vector(base.LayoutBox.X + TextDpi.FromTextDpi(mbpInfo.BPLeft), base.LayoutBox.Y + TextDpi.FromTextDpi(mbpInfo.BPTop));
		}
	}

	internal FloaterParagraphResult(BaseParaClient paraClient)
		: base(paraClient)
	{
	}

	internal Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition, Rect visibleRect, out bool success)
	{
		success = false;
		if (Contains(startPosition, strict: true))
		{
			success = true;
			ITextPointer endPosition2 = ((endPosition.CompareTo(base.EndPosition) < 0) ? endPosition : base.EndPosition);
			return ((FloaterParaClient)_paraClient).GetTightBoundingGeometryFromTextPositions(Columns, FloatingElements, startPosition, endPosition2, visibleRect);
		}
		return null;
	}
}
