using MS.Internal;

namespace System.Windows.Documents;

internal struct StaticTextPointer
{
	internal static StaticTextPointer Null = new StaticTextPointer(null, null, 0);

	private readonly ITextContainer _textContainer;

	private readonly uint _generation;

	private readonly object _handle0;

	private readonly int _handle1;

	internal ITextContainer TextContainer => _textContainer;

	internal DependencyObject Parent => _textContainer.GetParent(this);

	internal bool IsNull => _textContainer == null;

	internal object Handle0 => _handle0;

	internal int Handle1 => _handle1;

	internal StaticTextPointer(ITextContainer textContainer, object handle0)
		: this(textContainer, handle0, 0)
	{
	}

	internal StaticTextPointer(ITextContainer textContainer, object handle0, int handle1)
	{
		_textContainer = textContainer;
		_generation = textContainer?.Generation ?? 0;
		_handle0 = handle0;
		_handle1 = handle1;
	}

	internal ITextPointer CreateDynamicTextPointer(LogicalDirection direction)
	{
		AssertGeneration();
		return _textContainer.CreateDynamicTextPointer(this, direction);
	}

	internal TextPointerContext GetPointerContext(LogicalDirection direction)
	{
		AssertGeneration();
		return _textContainer.GetPointerContext(this, direction);
	}

	internal int GetOffsetToPosition(StaticTextPointer position)
	{
		AssertGeneration();
		return _textContainer.GetOffsetToPosition(this, position);
	}

	internal int GetTextInRun(LogicalDirection direction, char[] textBuffer, int startIndex, int count)
	{
		AssertGeneration();
		return _textContainer.GetTextInRun(this, direction, textBuffer, startIndex, count);
	}

	internal object GetAdjacentElement(LogicalDirection direction)
	{
		AssertGeneration();
		return _textContainer.GetAdjacentElement(this, direction);
	}

	internal StaticTextPointer CreatePointer(int offset)
	{
		AssertGeneration();
		return _textContainer.CreatePointer(this, offset);
	}

	internal StaticTextPointer GetNextContextPosition(LogicalDirection direction)
	{
		AssertGeneration();
		return _textContainer.GetNextContextPosition(this, direction);
	}

	internal int CompareTo(StaticTextPointer position)
	{
		AssertGeneration();
		return _textContainer.CompareTo(this, position);
	}

	internal int CompareTo(ITextPointer position)
	{
		AssertGeneration();
		return _textContainer.CompareTo(this, position);
	}

	internal object GetValue(DependencyProperty formattingProperty)
	{
		AssertGeneration();
		return _textContainer.GetValue(this, formattingProperty);
	}

	internal static StaticTextPointer Min(StaticTextPointer position1, StaticTextPointer position2)
	{
		position2.AssertGeneration();
		if (position1.CompareTo(position2) > 0)
		{
			return position2;
		}
		return position1;
	}

	internal static StaticTextPointer Max(StaticTextPointer position1, StaticTextPointer position2)
	{
		position2.AssertGeneration();
		if (position1.CompareTo(position2) < 0)
		{
			return position2;
		}
		return position1;
	}

	internal void AssertGeneration()
	{
		if (_textContainer != null)
		{
			Invariant.Assert(_generation == _textContainer.Generation, "StaticTextPointer not synchronized to tree generation!");
		}
	}
}
