using MS.Internal.Documents;

namespace System.Windows.Documents;

internal class TextParentUndoUnit : ParentUndoUnit
{
	private readonly ITextSelection _selection;

	private readonly int _undoAnchorPositionOffset;

	private readonly LogicalDirection _undoAnchorPositionDirection;

	private readonly int _undoMovingPositionOffset;

	private readonly LogicalDirection _undoMovingPositionDirection;

	private int _redoAnchorPositionOffset;

	private LogicalDirection _redoAnchorPositionDirection;

	private int _redoMovingPositionOffset;

	private LogicalDirection _redoMovingPositionDirection;

	private TextParentUndoUnit _redoUnit;

	internal TextParentUndoUnit(ITextSelection selection)
		: this(selection, selection.AnchorPosition, selection.MovingPosition)
	{
	}

	internal TextParentUndoUnit(ITextSelection selection, ITextPointer anchorPosition, ITextPointer movingPosition)
		: base(string.Empty)
	{
		_selection = selection;
		_undoAnchorPositionOffset = anchorPosition.Offset;
		_undoAnchorPositionDirection = anchorPosition.LogicalDirection;
		_undoMovingPositionOffset = movingPosition.Offset;
		_undoMovingPositionDirection = movingPosition.LogicalDirection;
		_redoAnchorPositionOffset = 0;
		_redoMovingPositionOffset = 0;
	}

	protected TextParentUndoUnit(TextParentUndoUnit undoUnit)
		: base(string.Empty)
	{
		_selection = undoUnit._selection;
		_undoAnchorPositionOffset = undoUnit._redoAnchorPositionOffset;
		_undoAnchorPositionDirection = undoUnit._redoAnchorPositionDirection;
		_undoMovingPositionOffset = undoUnit._redoMovingPositionOffset;
		_undoMovingPositionDirection = undoUnit._redoMovingPositionDirection;
		_redoAnchorPositionOffset = 0;
		_redoMovingPositionOffset = 0;
	}

	public override void Do()
	{
		base.Do();
		ITextContainer textContainer = _selection.Start.TextContainer;
		ITextPointer position = textContainer.CreatePointerAtOffset(_undoAnchorPositionOffset, _undoAnchorPositionDirection);
		ITextPointer position2 = textContainer.CreatePointerAtOffset(_undoMovingPositionOffset, _undoMovingPositionDirection);
		_selection.Select(position, position2);
		_redoUnit.RecordRedoSelectionState();
	}

	protected override IParentUndoUnit CreateParentUndoUnitForSelf()
	{
		_redoUnit = CreateRedoUnit();
		return _redoUnit;
	}

	protected virtual TextParentUndoUnit CreateRedoUnit()
	{
		return new TextParentUndoUnit(this);
	}

	protected void MergeRedoSelectionState(TextParentUndoUnit undoUnit)
	{
		_redoAnchorPositionOffset = undoUnit._redoAnchorPositionOffset;
		_redoAnchorPositionDirection = undoUnit._redoAnchorPositionDirection;
		_redoMovingPositionOffset = undoUnit._redoMovingPositionOffset;
		_redoMovingPositionDirection = undoUnit._redoMovingPositionDirection;
	}

	internal void RecordRedoSelectionState()
	{
		RecordRedoSelectionState(_selection.AnchorPosition, _selection.MovingPosition);
	}

	internal void RecordRedoSelectionState(ITextPointer anchorPosition, ITextPointer movingPosition)
	{
		_redoAnchorPositionOffset = anchorPosition.Offset;
		_redoAnchorPositionDirection = anchorPosition.LogicalDirection;
		_redoMovingPositionOffset = movingPosition.Offset;
		_redoMovingPositionDirection = movingPosition.LogicalDirection;
	}
}
