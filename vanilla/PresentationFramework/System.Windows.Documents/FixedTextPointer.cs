using MS.Internal;

namespace System.Windows.Documents;

internal class FixedTextPointer : ContentPosition, ITextPointer
{
	private LogicalDirection _gravity;

	private FlowPosition _flowPosition;

	private bool _isFrozen;

	Type ITextPointer.ParentType
	{
		get
		{
			FixedElement scopingElement = _flowPosition.GetScopingElement();
			if (!scopingElement.IsTextElement)
			{
				return ((ITextContainer)_flowPosition.TextContainer).Parent.GetType();
			}
			return scopingElement.Type;
		}
	}

	ITextContainer ITextPointer.TextContainer => FixedTextContainer;

	bool ITextPointer.HasValidLayout
	{
		get
		{
			if (((ITextPointer)this).TextContainer.TextView != null && ((ITextPointer)this).TextContainer.TextView.IsValid)
			{
				return ((ITextPointer)this).TextContainer.TextView.Contains(this);
			}
			return false;
		}
	}

	bool ITextPointer.IsAtCaretUnitBoundary
	{
		get
		{
			Invariant.Assert(((ITextPointer)this).HasValidLayout);
			ITextView textView = ((ITextPointer)this).TextContainer.TextView;
			bool flag = textView.IsAtCaretUnitBoundary(this);
			if (!flag && LogicalDirection == LogicalDirection.Backward)
			{
				ITextPointer position = ((ITextPointer)this).CreatePointer(LogicalDirection.Forward);
				flag = textView.IsAtCaretUnitBoundary(position);
			}
			return flag;
		}
	}

	LogicalDirection ITextPointer.LogicalDirection => LogicalDirection;

	bool ITextPointer.IsAtInsertionPosition => TextPointerBase.IsAtInsertionPosition(this);

	bool ITextPointer.IsFrozen => _isFrozen;

	int ITextPointer.Offset => TextPointerBase.GetOffset(this);

	int ITextPointer.CharOffset
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal FlowPosition FlowPosition => _flowPosition;

	internal FixedTextContainer FixedTextContainer => _flowPosition.TextContainer;

	internal LogicalDirection LogicalDirection
	{
		get
		{
			return _gravity;
		}
		set
		{
			ValidationHelper.VerifyDirection(value, "value");
			_flowPosition = _flowPosition.GetClingPosition(value);
			_gravity = value;
		}
	}

	internal FixedTextPointer(bool mutable, LogicalDirection gravity, FlowPosition flow)
	{
		_isFrozen = !mutable;
		_gravity = gravity;
		_flowPosition = flow;
	}

	internal int CompareTo(ITextPointer position)
	{
		FixedTextPointer fixedTextPointer = FixedTextContainer.VerifyPosition(position);
		return _flowPosition.CompareTo(fixedTextPointer.FlowPosition);
	}

	int ITextPointer.CompareTo(StaticTextPointer position)
	{
		return ((ITextPointer)this).CompareTo((ITextPointer)position.Handle0);
	}

	int ITextPointer.CompareTo(ITextPointer position)
	{
		return CompareTo(position);
	}

	int ITextPointer.GetOffsetToPosition(ITextPointer position)
	{
		FixedTextPointer fixedTextPointer = FixedTextContainer.VerifyPosition(position);
		return _flowPosition.GetDistance(fixedTextPointer.FlowPosition);
	}

	TextPointerContext ITextPointer.GetPointerContext(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		return _flowPosition.GetPointerContext(direction);
	}

	int ITextPointer.GetTextRunLength(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		if (_flowPosition.GetPointerContext(direction) != TextPointerContext.Text)
		{
			return 0;
		}
		return _flowPosition.GetTextRunLength(direction);
	}

	string ITextPointer.GetTextInRun(LogicalDirection direction)
	{
		return TextPointerBase.GetTextInRun(this, direction);
	}

	int ITextPointer.GetTextInRun(LogicalDirection direction, char[] textBuffer, int startIndex, int count)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		if (textBuffer == null)
		{
			throw new ArgumentNullException("textBuffer");
		}
		if (count < 0)
		{
			throw new ArgumentException(SR.Format(SR.NegativeValue, "count"));
		}
		if (_flowPosition.GetPointerContext(direction) != TextPointerContext.Text)
		{
			return 0;
		}
		return _flowPosition.GetTextInRun(direction, count, textBuffer, startIndex);
	}

	object ITextPointer.GetAdjacentElement(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		TextPointerContext pointerContext = _flowPosition.GetPointerContext(direction);
		if (pointerContext != TextPointerContext.EmbeddedElement && pointerContext != TextPointerContext.ElementStart && pointerContext != TextPointerContext.ElementEnd)
		{
			return null;
		}
		return _flowPosition.GetAdjacentElement(direction);
	}

	Type ITextPointer.GetElementType(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		TextPointerContext pointerContext = _flowPosition.GetPointerContext(direction);
		if (pointerContext == TextPointerContext.ElementStart || pointerContext == TextPointerContext.ElementEnd)
		{
			FixedElement element = _flowPosition.GetElement(direction);
			if (!element.IsTextElement)
			{
				return null;
			}
			return element.Type;
		}
		return null;
	}

	bool ITextPointer.HasEqualScope(ITextPointer position)
	{
		FixedTextPointer fixedTextPointer = FixedTextContainer.VerifyPosition(position);
		FixedElement scopingElement = _flowPosition.GetScopingElement();
		FixedElement scopingElement2 = fixedTextPointer.FlowPosition.GetScopingElement();
		return scopingElement == scopingElement2;
	}

	object ITextPointer.GetValue(DependencyProperty property)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		return _flowPosition.GetScopingElement().GetValue(property);
	}

	object ITextPointer.ReadLocalValue(DependencyProperty property)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		FixedElement scopingElement = _flowPosition.GetScopingElement();
		if (!scopingElement.IsTextElement)
		{
			throw new InvalidOperationException(SR.NoElementObject);
		}
		return scopingElement.ReadLocalValue(property);
	}

	LocalValueEnumerator ITextPointer.GetLocalValueEnumerator()
	{
		FixedElement scopingElement = _flowPosition.GetScopingElement();
		if (!scopingElement.IsTextElement)
		{
			return new DependencyObject().GetLocalValueEnumerator();
		}
		return scopingElement.GetLocalValueEnumerator();
	}

	ITextPointer ITextPointer.CreatePointer()
	{
		return ((ITextPointer)this).CreatePointer(0, ((ITextPointer)this).LogicalDirection);
	}

	StaticTextPointer ITextPointer.CreateStaticPointer()
	{
		return new StaticTextPointer(((ITextPointer)this).TextContainer, ((ITextPointer)this).CreatePointer());
	}

	ITextPointer ITextPointer.CreatePointer(int distance)
	{
		return ((ITextPointer)this).CreatePointer(distance, ((ITextPointer)this).LogicalDirection);
	}

	ITextPointer ITextPointer.CreatePointer(LogicalDirection gravity)
	{
		return ((ITextPointer)this).CreatePointer(0, gravity);
	}

	ITextPointer ITextPointer.CreatePointer(int distance, LogicalDirection gravity)
	{
		ValidationHelper.VerifyDirection(gravity, "gravity");
		FlowPosition flowPosition = (FlowPosition)_flowPosition.Clone();
		if (!flowPosition.Move(distance))
		{
			throw new ArgumentException(SR.BadDistance, "distance");
		}
		return new FixedTextPointer(mutable: true, gravity, flowPosition);
	}

	void ITextPointer.Freeze()
	{
		_isFrozen = true;
	}

	ITextPointer ITextPointer.GetFrozenPointer(LogicalDirection logicalDirection)
	{
		return TextPointerBase.GetFrozenPointer(this, logicalDirection);
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

	void ITextPointer.SetLogicalDirection(LogicalDirection direction)
	{
		LogicalDirection = direction;
	}

	bool ITextPointer.MoveToNextContextPosition(LogicalDirection direction)
	{
		ValidationHelper.VerifyDirection(direction, "direction");
		return _flowPosition.Move(direction);
	}

	int ITextPointer.MoveByOffset(int offset)
	{
		if (_isFrozen)
		{
			throw new InvalidOperationException(SR.TextPositionIsFrozen);
		}
		if (!_flowPosition.Move(offset))
		{
			throw new ArgumentException(SR.BadDistance, "offset");
		}
		return offset;
	}

	void ITextPointer.MoveToPosition(ITextPointer position)
	{
		FixedTextPointer fixedTextPointer = FixedTextContainer.VerifyPosition(position);
		_flowPosition.MoveTo(fixedTextPointer.FlowPosition);
	}

	void ITextPointer.MoveToElementEdge(ElementEdge edge)
	{
		ValidationHelper.VerifyElementEdge(edge, "edge");
		FixedElement scopingElement = _flowPosition.GetScopingElement();
		if (!scopingElement.IsTextElement)
		{
			throw new InvalidOperationException(SR.NoElementObject);
		}
		switch (edge)
		{
		case ElementEdge.BeforeStart:
			_flowPosition = (FlowPosition)scopingElement.Start.FlowPosition.Clone();
			_flowPosition.Move(-1);
			break;
		case ElementEdge.AfterStart:
			_flowPosition = (FlowPosition)scopingElement.Start.FlowPosition.Clone();
			break;
		case ElementEdge.BeforeEnd:
			_flowPosition = (FlowPosition)scopingElement.End.FlowPosition.Clone();
			break;
		case ElementEdge.AfterEnd:
			_flowPosition = (FlowPosition)scopingElement.End.FlowPosition.Clone();
			_flowPosition.Move(1);
			break;
		}
	}

	int ITextPointer.MoveToLineBoundary(int count)
	{
		return TextPointerBase.MoveToLineBoundary(this, ((ITextPointer)this).TextContainer.TextView, count, respectNonMeargeableInlineStart: true);
	}

	Rect ITextPointer.GetCharacterRect(LogicalDirection direction)
	{
		return TextPointerBase.GetCharacterRect(this, direction);
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
		if (textData == null)
		{
			throw new ArgumentNullException("textData");
		}
		throw new InvalidOperationException(SR.FixedDocumentReadonly);
	}

	void ITextPointer.DeleteContentToPosition(ITextPointer limit)
	{
		throw new InvalidOperationException(SR.FixedDocumentReadonly);
	}

	bool ITextPointer.ValidateLayout()
	{
		return TextPointerBase.ValidateLayout(this, ((ITextPointer)this).TextContainer.TextView);
	}
}
