using System.Windows.Documents;

namespace System.Windows.Controls;

internal sealed class PasswordTextPointer : ITextPointer
{
	private PasswordTextContainer _container;

	private LogicalDirection _gravity;

	private int _offset;

	private bool _isFrozen;

	Type ITextPointer.ParentType => null;

	ITextContainer ITextPointer.TextContainer => _container;

	bool ITextPointer.HasValidLayout
	{
		get
		{
			if (_container.TextView != null && _container.TextView.IsValid)
			{
				return _container.TextView.Contains(this);
			}
			return false;
		}
	}

	bool ITextPointer.IsAtCaretUnitBoundary => true;

	LogicalDirection ITextPointer.LogicalDirection => _gravity;

	bool ITextPointer.IsAtInsertionPosition => TextPointerBase.IsAtInsertionPosition(this);

	bool ITextPointer.IsFrozen => _isFrozen;

	int ITextPointer.Offset => TextPointerBase.GetOffset(this);

	int ITextPointer.CharOffset => Offset;

	internal PasswordTextContainer Container => _container;

	internal LogicalDirection LogicalDirection => _gravity;

	internal int Offset
	{
		get
		{
			return _offset;
		}
		set
		{
			_offset = value;
		}
	}

	internal PasswordTextPointer(PasswordTextContainer container, LogicalDirection gravity, int offset)
	{
		_container = container;
		_gravity = gravity;
		_offset = offset;
		container.AddPosition(this);
	}

	void ITextPointer.SetLogicalDirection(LogicalDirection direction)
	{
		if (direction != _gravity)
		{
			Container.RemovePosition(this);
			_gravity = direction;
			Container.AddPosition(this);
		}
	}

	int ITextPointer.CompareTo(ITextPointer position)
	{
		int offset = ((PasswordTextPointer)position)._offset;
		if (_offset < offset)
		{
			return -1;
		}
		if (_offset > offset)
		{
			return 1;
		}
		return 0;
	}

	int ITextPointer.CompareTo(StaticTextPointer position)
	{
		return ((ITextPointer)this).CompareTo((ITextPointer)position.Handle0);
	}

	int ITextPointer.GetOffsetToPosition(ITextPointer position)
	{
		return ((PasswordTextPointer)position)._offset - _offset;
	}

	TextPointerContext ITextPointer.GetPointerContext(LogicalDirection direction)
	{
		if ((direction == LogicalDirection.Backward && _offset == 0) || (direction == LogicalDirection.Forward && _offset == _container.SymbolCount))
		{
			return TextPointerContext.None;
		}
		return TextPointerContext.Text;
	}

	int ITextPointer.GetTextRunLength(LogicalDirection direction)
	{
		if (direction == LogicalDirection.Forward)
		{
			return _container.SymbolCount - _offset;
		}
		return _offset;
	}

	string ITextPointer.GetTextInRun(LogicalDirection direction)
	{
		return TextPointerBase.GetTextInRun(this, direction);
	}

	int ITextPointer.GetTextInRun(LogicalDirection direction, char[] textBuffer, int startIndex, int count)
	{
		int num = ((direction != LogicalDirection.Forward) ? Math.Min(count, _offset) : Math.Min(count, _container.SymbolCount - _offset));
		char passwordChar = _container.PasswordChar;
		for (int i = 0; i < num; i++)
		{
			textBuffer[startIndex + i] = passwordChar;
		}
		return num;
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

	object ITextPointer.GetValue(DependencyProperty formattingProperty)
	{
		return _container.PasswordBox.GetValue(formattingProperty);
	}

	object ITextPointer.ReadLocalValue(DependencyProperty formattingProperty)
	{
		return DependencyProperty.UnsetValue;
	}

	LocalValueEnumerator ITextPointer.GetLocalValueEnumerator()
	{
		return new DependencyObject().GetLocalValueEnumerator();
	}

	ITextPointer ITextPointer.CreatePointer()
	{
		return new PasswordTextPointer(_container, _gravity, _offset);
	}

	StaticTextPointer ITextPointer.CreateStaticPointer()
	{
		return new StaticTextPointer(((ITextPointer)this).TextContainer, ((ITextPointer)this).CreatePointer());
	}

	ITextPointer ITextPointer.CreatePointer(int distance)
	{
		return new PasswordTextPointer(_container, _gravity, _offset + distance);
	}

	ITextPointer ITextPointer.CreatePointer(LogicalDirection gravity)
	{
		return new PasswordTextPointer(_container, gravity, _offset);
	}

	ITextPointer ITextPointer.CreatePointer(int distance, LogicalDirection gravity)
	{
		return new PasswordTextPointer(_container, gravity, _offset + distance);
	}

	void ITextPointer.Freeze()
	{
		_isFrozen = true;
	}

	ITextPointer ITextPointer.GetFrozenPointer(LogicalDirection logicalDirection)
	{
		return TextPointerBase.GetFrozenPointer(this, logicalDirection);
	}

	void ITextPointer.InsertTextInRun(string textData)
	{
		_container.InsertText(this, textData);
	}

	void ITextPointer.DeleteContentToPosition(ITextPointer limit)
	{
		_container.DeleteContent(this, limit);
	}

	bool ITextPointer.MoveToNextContextPosition(LogicalDirection direction)
	{
		int offset;
		if (direction == LogicalDirection.Backward)
		{
			if (Offset == 0)
			{
				return false;
			}
			offset = 0;
		}
		else
		{
			if (Offset == Container.SymbolCount)
			{
				return false;
			}
			offset = Container.SymbolCount;
		}
		Container.RemovePosition(this);
		Offset = offset;
		Container.AddPosition(this);
		return true;
	}

	int ITextPointer.MoveByOffset(int distance)
	{
		int num = Offset + distance;
		if (num >= 0)
		{
			_ = Container.SymbolCount;
		}
		Container.RemovePosition(this);
		Offset = num;
		Container.AddPosition(this);
		return distance;
	}

	void ITextPointer.MoveToPosition(ITextPointer position)
	{
		Container.RemovePosition(this);
		Offset = ((PasswordTextPointer)position).Offset;
		Container.AddPosition(this);
	}

	void ITextPointer.MoveToElementEdge(ElementEdge edge)
	{
	}

	int ITextPointer.MoveToLineBoundary(int count)
	{
		return TextPointerBase.MoveToLineBoundary(this, _container.TextView, count);
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

	ITextPointer ITextPointer.GetFormatNormalizedPosition(LogicalDirection direction)
	{
		ITextPointer textPointer = ((ITextPointer)this).CreatePointer();
		TextPointerBase.MoveToFormatNormalizedPosition(textPointer, direction);
		textPointer.Freeze();
		return textPointer;
	}

	bool ITextPointer.ValidateLayout()
	{
		return TextPointerBase.ValidateLayout(this, _container.TextView);
	}
}
