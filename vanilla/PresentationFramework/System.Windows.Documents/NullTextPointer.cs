using MS.Internal;

namespace System.Windows.Documents;

internal sealed class NullTextPointer : ITextPointer
{
	private LogicalDirection _gravity;

	private NullTextContainer _container;

	private bool _isFrozen;

	Type ITextPointer.ParentType => typeof(FixedDocument);

	ITextContainer ITextPointer.TextContainer => _container;

	bool ITextPointer.HasValidLayout => false;

	bool ITextPointer.IsAtCaretUnitBoundary
	{
		get
		{
			Invariant.Assert(condition: false, "NullTextPointer never has valid layout!");
			return false;
		}
	}

	LogicalDirection ITextPointer.LogicalDirection => _gravity;

	bool ITextPointer.IsAtInsertionPosition => TextPointerBase.IsAtInsertionPosition(this);

	bool ITextPointer.IsFrozen => _isFrozen;

	int ITextPointer.Offset => 0;

	int ITextPointer.CharOffset
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal NullTextPointer(NullTextContainer container, LogicalDirection gravity)
	{
		_container = container;
		_gravity = gravity;
	}

	int ITextPointer.CompareTo(ITextPointer position)
	{
		return 0;
	}

	int ITextPointer.CompareTo(StaticTextPointer position)
	{
		return 0;
	}

	int ITextPointer.GetOffsetToPosition(ITextPointer position)
	{
		return 0;
	}

	TextPointerContext ITextPointer.GetPointerContext(LogicalDirection direction)
	{
		return TextPointerContext.None;
	}

	int ITextPointer.GetTextRunLength(LogicalDirection direction)
	{
		return 0;
	}

	string ITextPointer.GetTextInRun(LogicalDirection direction)
	{
		return TextPointerBase.GetTextInRun(this, direction);
	}

	int ITextPointer.GetTextInRun(LogicalDirection direction, char[] textBuffer, int startIndex, int count)
	{
		return 0;
	}

	object ITextPointer.GetAdjacentElement(LogicalDirection direction)
	{
		return null;
	}

	Type ITextPointer.GetElementType(LogicalDirection direction)
	{
		return null;
	}

	bool ITextPointer.HasEqualScope(ITextPointer position)
	{
		return true;
	}

	object ITextPointer.GetValue(DependencyProperty property)
	{
		return property.DefaultMetadata.DefaultValue;
	}

	object ITextPointer.ReadLocalValue(DependencyProperty property)
	{
		return DependencyProperty.UnsetValue;
	}

	LocalValueEnumerator ITextPointer.GetLocalValueEnumerator()
	{
		return new DependencyObject().GetLocalValueEnumerator();
	}

	ITextPointer ITextPointer.CreatePointer()
	{
		return ((ITextPointer)this).CreatePointer(0, _gravity);
	}

	StaticTextPointer ITextPointer.CreateStaticPointer()
	{
		return new StaticTextPointer(((ITextPointer)this).TextContainer, ((ITextPointer)this).CreatePointer());
	}

	ITextPointer ITextPointer.CreatePointer(int distance)
	{
		return ((ITextPointer)this).CreatePointer(distance, _gravity);
	}

	ITextPointer ITextPointer.CreatePointer(LogicalDirection gravity)
	{
		return ((ITextPointer)this).CreatePointer(0, gravity);
	}

	ITextPointer ITextPointer.CreatePointer(int distance, LogicalDirection gravity)
	{
		return new NullTextPointer(_container, gravity);
	}

	void ITextPointer.Freeze()
	{
		_isFrozen = true;
	}

	ITextPointer ITextPointer.GetFrozenPointer(LogicalDirection logicalDirection)
	{
		return TextPointerBase.GetFrozenPointer(this, logicalDirection);
	}

	void ITextPointer.SetLogicalDirection(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "gravity");
		_gravity = direction;
	}

	bool ITextPointer.MoveToNextContextPosition(LogicalDirection direction)
	{
		return false;
	}

	int ITextPointer.MoveByOffset(int distance)
	{
		return 0;
	}

	void ITextPointer.MoveToPosition(ITextPointer position)
	{
	}

	void ITextPointer.MoveToElementEdge(ElementEdge edge)
	{
	}

	int ITextPointer.MoveToLineBoundary(int count)
	{
		return 0;
	}

	Rect ITextPointer.GetCharacterRect(LogicalDirection direction)
	{
		return default(Rect);
	}

	bool ITextPointer.MoveToInsertionPosition(LogicalDirection direction)
	{
		return TextPointerBase.MoveToInsertionPosition(this, direction);
	}

	bool ITextPointer.MoveToNextInsertionPosition(LogicalDirection direction)
	{
		return TextPointerBase.MoveToNextInsertionPosition(this, direction);
	}

	void ITextPointer.InsertTextInRun(string textData)
	{
	}

	void ITextPointer.DeleteContentToPosition(ITextPointer limit)
	{
	}

	ITextPointer ITextPointer.GetNextContextPosition(LogicalDirection direction)
	{
		ITextPointer textPointer = ((ITextPointer)this).CreatePointer();
		if (textPointer.MoveToNextContextPosition(direction))
		{
			textPointer.Freeze();
		}
		else
		{
			textPointer = null;
		}
		return textPointer;
	}

	ITextPointer ITextPointer.GetInsertionPosition(LogicalDirection direction)
	{
		ITextPointer textPointer = ((ITextPointer)this).CreatePointer();
		textPointer.MoveToInsertionPosition(direction);
		textPointer.Freeze();
		return textPointer;
	}

	ITextPointer ITextPointer.GetFormatNormalizedPosition(LogicalDirection direction)
	{
		ITextPointer textPointer = ((ITextPointer)this).CreatePointer();
		TextPointerBase.MoveToFormatNormalizedPosition(textPointer, direction);
		textPointer.Freeze();
		return textPointer;
	}

	ITextPointer ITextPointer.GetNextInsertionPosition(LogicalDirection direction)
	{
		ITextPointer textPointer = ((ITextPointer)this).CreatePointer();
		if (textPointer.MoveToNextInsertionPosition(direction))
		{
			textPointer.Freeze();
		}
		else
		{
			textPointer = null;
		}
		return textPointer;
	}

	bool ITextPointer.ValidateLayout()
	{
		return false;
	}
}
