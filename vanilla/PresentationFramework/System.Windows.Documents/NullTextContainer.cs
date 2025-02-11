using MS.Internal;
using MS.Internal.Documents;

namespace System.Windows.Documents;

internal sealed class NullTextContainer : ITextContainer
{
	private NullTextPointer _start;

	private NullTextPointer _end;

	bool ITextContainer.IsReadOnly => true;

	ITextPointer ITextContainer.Start => _start;

	ITextPointer ITextContainer.End => _end;

	uint ITextContainer.Generation => 0u;

	Highlights ITextContainer.Highlights => null;

	DependencyObject ITextContainer.Parent => null;

	ITextSelection ITextContainer.TextSelection
	{
		get
		{
			return null;
		}
		set
		{
			Invariant.Assert(condition: false, "NullTextContainer is never associated with a TextEditor/TextSelection!");
		}
	}

	UndoManager ITextContainer.UndoManager => null;

	ITextView ITextContainer.TextView
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	int ITextContainer.SymbolCount => 0;

	int ITextContainer.IMECharCount
	{
		get
		{
			Invariant.Assert(condition: false);
			return 0;
		}
	}

	public event EventHandler Changing
	{
		add
		{
		}
		remove
		{
		}
	}

	public event TextContainerChangeEventHandler Change
	{
		add
		{
		}
		remove
		{
		}
	}

	public event TextContainerChangedEventHandler Changed
	{
		add
		{
		}
		remove
		{
		}
	}

	internal NullTextContainer()
	{
		_start = new NullTextPointer(this, LogicalDirection.Backward);
		_end = new NullTextPointer(this, LogicalDirection.Forward);
	}

	void ITextContainer.BeginChange()
	{
	}

	void ITextContainer.BeginChangeNoUndo()
	{
		((ITextContainer)this).BeginChange();
	}

	void ITextContainer.EndChange()
	{
		((ITextContainer)this).EndChange(skipEvents: false);
	}

	void ITextContainer.EndChange(bool skipEvents)
	{
	}

	ITextPointer ITextContainer.CreatePointerAtOffset(int offset, LogicalDirection direction)
	{
		return ((ITextContainer)this).Start.CreatePointer(offset, direction);
	}

	ITextPointer ITextContainer.CreatePointerAtCharOffset(int charOffset, LogicalDirection direction)
	{
		throw new NotImplementedException();
	}

	ITextPointer ITextContainer.CreateDynamicTextPointer(StaticTextPointer position, LogicalDirection direction)
	{
		return ((ITextPointer)position.Handle0).CreatePointer(direction);
	}

	StaticTextPointer ITextContainer.CreateStaticPointerAtOffset(int offset)
	{
		return new StaticTextPointer(this, ((ITextContainer)this).CreatePointerAtOffset(offset, LogicalDirection.Forward));
	}

	TextPointerContext ITextContainer.GetPointerContext(StaticTextPointer pointer, LogicalDirection direction)
	{
		return ((ITextPointer)pointer.Handle0).GetPointerContext(direction);
	}

	int ITextContainer.GetOffsetToPosition(StaticTextPointer position1, StaticTextPointer position2)
	{
		return ((ITextPointer)position1.Handle0).GetOffsetToPosition((ITextPointer)position2.Handle0);
	}

	int ITextContainer.GetTextInRun(StaticTextPointer position, LogicalDirection direction, char[] textBuffer, int startIndex, int count)
	{
		return ((ITextPointer)position.Handle0).GetTextInRun(direction, textBuffer, startIndex, count);
	}

	object ITextContainer.GetAdjacentElement(StaticTextPointer position, LogicalDirection direction)
	{
		return ((ITextPointer)position.Handle0).GetAdjacentElement(direction);
	}

	DependencyObject ITextContainer.GetParent(StaticTextPointer position)
	{
		return null;
	}

	StaticTextPointer ITextContainer.CreatePointer(StaticTextPointer position, int offset)
	{
		return new StaticTextPointer(this, ((ITextPointer)position.Handle0).CreatePointer(offset));
	}

	StaticTextPointer ITextContainer.GetNextContextPosition(StaticTextPointer position, LogicalDirection direction)
	{
		return new StaticTextPointer(this, ((ITextPointer)position.Handle0).GetNextContextPosition(direction));
	}

	int ITextContainer.CompareTo(StaticTextPointer position1, StaticTextPointer position2)
	{
		return ((ITextPointer)position1.Handle0).CompareTo((ITextPointer)position2.Handle0);
	}

	int ITextContainer.CompareTo(StaticTextPointer position1, ITextPointer position2)
	{
		return ((ITextPointer)position1.Handle0).CompareTo(position2);
	}

	object ITextContainer.GetValue(StaticTextPointer position, DependencyProperty formattingProperty)
	{
		return ((ITextPointer)position.Handle0).GetValue(formattingProperty);
	}
}
