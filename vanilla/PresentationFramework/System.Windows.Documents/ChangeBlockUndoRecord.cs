using MS.Internal;
using MS.Internal.Documents;

namespace System.Windows.Documents;

internal class ChangeBlockUndoRecord
{
	private readonly UndoManager _undoManager;

	private readonly IParentUndoUnit _parentUndoUnit;

	internal ChangeBlockUndoRecord(ITextContainer textContainer, string actionDescription)
	{
		if (textContainer.UndoManager == null)
		{
			return;
		}
		_undoManager = textContainer.UndoManager;
		if (!_undoManager.IsEnabled)
		{
			return;
		}
		if (textContainer.TextView != null)
		{
			if (_undoManager.OpenedUnit == null)
			{
				if (textContainer.TextSelection != null)
				{
					_parentUndoUnit = new TextParentUndoUnit(textContainer.TextSelection);
				}
				else
				{
					_parentUndoUnit = new ParentUndoUnit(actionDescription);
				}
				_undoManager.Open(_parentUndoUnit);
			}
		}
		else
		{
			_undoManager.Clear();
		}
	}

	internal void OnEndChange()
	{
		if (_parentUndoUnit == null)
		{
			return;
		}
		IParentUndoUnit parentUndoUnit = ((!(_parentUndoUnit.Container is UndoManager)) ? ((IParentUndoUnit)_parentUndoUnit.Container).OpenedUnit : ((UndoManager)_parentUndoUnit.Container).OpenedUnit);
		if (parentUndoUnit == _parentUndoUnit)
		{
			if (_parentUndoUnit is TextParentUndoUnit)
			{
				((TextParentUndoUnit)_parentUndoUnit).RecordRedoSelectionState();
			}
			Invariant.Assert(_undoManager != null);
			_undoManager.Close(_parentUndoUnit, (_parentUndoUnit.LastUnit == null) ? UndoCloseAction.Discard : UndoCloseAction.Commit);
		}
	}
}
