using System.Collections.ObjectModel;
using System.Windows.Media;

namespace System.Windows.Documents;

internal interface ITextView
{
	UIElement RenderScope { get; }

	ITextContainer TextContainer { get; }

	bool IsValid { get; }

	bool RendersOwnSelection { get; }

	ReadOnlyCollection<TextSegment> TextSegments { get; }

	event BringPositionIntoViewCompletedEventHandler BringPositionIntoViewCompleted;

	event BringPointIntoViewCompletedEventHandler BringPointIntoViewCompleted;

	event BringLineIntoViewCompletedEventHandler BringLineIntoViewCompleted;

	event BringPageIntoViewCompletedEventHandler BringPageIntoViewCompleted;

	event EventHandler Updated;

	ITextPointer GetTextPositionFromPoint(Point point, bool snapToText);

	Rect GetRectangleFromTextPosition(ITextPointer position);

	Rect GetRawRectangleFromTextPosition(ITextPointer position, out Transform transform);

	Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition);

	ITextPointer GetPositionAtNextLine(ITextPointer position, double suggestedX, int count, out double newSuggestedX, out int linesMoved);

	ITextPointer GetPositionAtNextPage(ITextPointer position, Point suggestedOffset, int count, out Point newSuggestedOffset, out int pagesMoved);

	bool IsAtCaretUnitBoundary(ITextPointer position);

	ITextPointer GetNextCaretUnitPosition(ITextPointer position, LogicalDirection direction);

	ITextPointer GetBackspaceCaretUnitPosition(ITextPointer position);

	TextSegment GetLineRange(ITextPointer position);

	ReadOnlyCollection<GlyphRun> GetGlyphRuns(ITextPointer start, ITextPointer end);

	bool Contains(ITextPointer position);

	void BringPositionIntoViewAsync(ITextPointer position, object userState);

	void BringPointIntoViewAsync(Point point, object userState);

	void BringLineIntoViewAsync(ITextPointer position, double suggestedX, int count, object userState);

	void BringPageIntoViewAsync(ITextPointer position, Point suggestedOffset, int count, object userState);

	void CancelAsync(object userState);

	bool Validate();

	bool Validate(Point point);

	bool Validate(ITextPointer position);

	void ThrottleBackgroundTasksForUserInput();
}
