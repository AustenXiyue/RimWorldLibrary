using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.PtsHost;

namespace MS.Internal.Documents;

internal sealed class TextParagraphResult : ParagraphResult
{
	private ReadOnlyCollection<LineResult> _lines;

	private ReadOnlyCollection<ParagraphResult> _floaters;

	private ReadOnlyCollection<ParagraphResult> _figures;

	internal ReadOnlyCollection<LineResult> Lines
	{
		get
		{
			if (_lines == null)
			{
				_lines = ((TextParaClient)_paraClient).GetLineResults();
			}
			Invariant.Assert(_lines != null, "Lines collection is null");
			return _lines;
		}
	}

	internal ReadOnlyCollection<ParagraphResult> Floaters
	{
		get
		{
			if (_floaters == null)
			{
				_floaters = ((TextParaClient)_paraClient).GetFloaters();
			}
			return _floaters;
		}
	}

	internal ReadOnlyCollection<ParagraphResult> Figures
	{
		get
		{
			if (_figures == null)
			{
				_figures = ((TextParaClient)_paraClient).GetFigures();
			}
			return _figures;
		}
	}

	internal override bool HasTextContent
	{
		get
		{
			if (Lines.Count > 0)
			{
				return !ContainsOnlyFloatingElements;
			}
			return false;
		}
	}

	private bool ContainsOnlyFloatingElements
	{
		get
		{
			bool result = false;
			TextParagraph textParagraph = _paraClient.Paragraph as TextParagraph;
			Invariant.Assert(textParagraph != null);
			if (textParagraph.HasFiguresOrFloaters())
			{
				if (Lines.Count == 0)
				{
					result = true;
				}
				else if (Lines.Count == 1 && textParagraph.GetLastDcpAttachedObjectBeforeLine(0) + textParagraph.ParagraphStartCharacterPosition == textParagraph.ParagraphEndCharacterPosition)
				{
					result = true;
				}
			}
			return result;
		}
	}

	internal TextParagraphResult(TextParaClient paraClient)
		: base(paraClient)
	{
	}

	internal Rect GetRectangleFromTextPosition(ITextPointer position)
	{
		return ((TextParaClient)_paraClient).GetRectangleFromTextPosition(position);
	}

	internal Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition, double paragraphTopSpace, Rect visibleRect)
	{
		return ((TextParaClient)_paraClient).GetTightBoundingGeometryFromTextPositions(startPosition, endPosition, paragraphTopSpace, visibleRect);
	}

	internal bool IsAtCaretUnitBoundary(ITextPointer position)
	{
		return ((TextParaClient)_paraClient).IsAtCaretUnitBoundary(position);
	}

	internal ITextPointer GetNextCaretUnitPosition(ITextPointer position, LogicalDirection direction)
	{
		return ((TextParaClient)_paraClient).GetNextCaretUnitPosition(position, direction);
	}

	internal ITextPointer GetBackspaceCaretUnitPosition(ITextPointer position)
	{
		return ((TextParaClient)_paraClient).GetBackspaceCaretUnitPosition(position);
	}

	internal void GetGlyphRuns(List<GlyphRun> glyphRuns, ITextPointer start, ITextPointer end)
	{
		((TextParaClient)_paraClient).GetGlyphRuns(glyphRuns, start, end);
	}

	internal override bool Contains(ITextPointer position, bool strict)
	{
		bool flag = base.Contains(position, strict);
		if (!flag && strict)
		{
			flag = position.CompareTo(base.EndPosition) == 0;
		}
		return flag;
	}
}
