using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MS.Internal.Documents;

internal abstract class LineResult
{
	internal abstract ITextPointer StartPosition { get; }

	internal abstract ITextPointer EndPosition { get; }

	internal abstract int StartPositionCP { get; }

	internal abstract int EndPositionCP { get; }

	internal abstract Rect LayoutBox { get; }

	internal abstract double Baseline { get; }

	internal abstract ITextPointer GetTextPositionFromDistance(double distance);

	internal abstract bool IsAtCaretUnitBoundary(ITextPointer position);

	internal abstract ITextPointer GetNextCaretUnitPosition(ITextPointer position, LogicalDirection direction);

	internal abstract ITextPointer GetBackspaceCaretUnitPosition(ITextPointer position);

	internal abstract ReadOnlyCollection<GlyphRun> GetGlyphRuns(ITextPointer start, ITextPointer end);

	internal abstract ITextPointer GetContentEndPosition();

	internal abstract ITextPointer GetEllipsesPosition();

	internal abstract int GetContentEndPositionCP();

	internal abstract int GetEllipsesPositionCP();
}
