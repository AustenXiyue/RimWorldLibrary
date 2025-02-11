using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MS.Internal;

namespace System.Windows.Documents;

internal class TextSelectionHighlightLayer : HighlightLayer
{
	private class TextSelectionHighlightChangedEventArgs : HighlightChangedEventArgs
	{
		private readonly ReadOnlyCollection<TextSegment> _ranges;

		internal override IList Ranges => _ranges;

		internal override Type OwnerType => typeof(TextSelection);

		internal TextSelectionHighlightChangedEventArgs(ITextPointer invalidRangeLeftStart, ITextPointer invalidRangeLeftEnd, ITextPointer invalidRangeRightStart, ITextPointer invalidRangeRightEnd)
		{
			Invariant.Assert(invalidRangeLeftStart != invalidRangeLeftEnd || invalidRangeRightStart != invalidRangeRightEnd, "Unexpected empty range!");
			_ranges = new ReadOnlyCollection<TextSegment>((invalidRangeLeftStart.CompareTo(invalidRangeLeftEnd) == 0) ? new List<TextSegment>(1)
			{
				new TextSegment(invalidRangeRightStart, invalidRangeRightEnd)
			} : ((invalidRangeRightStart.CompareTo(invalidRangeRightEnd) != 0) ? new List<TextSegment>(2)
			{
				new TextSegment(invalidRangeLeftStart, invalidRangeLeftEnd),
				new TextSegment(invalidRangeRightStart, invalidRangeRightEnd)
			} : new List<TextSegment>(1)
			{
				new TextSegment(invalidRangeLeftStart, invalidRangeLeftEnd)
			}));
		}
	}

	private readonly ITextSelection _selection;

	private ITextPointer _oldStart;

	private ITextPointer _oldEnd;

	private static readonly object _selectedValue;

	internal override Type OwnerType => typeof(TextSelection);

	internal override event HighlightChangedEventHandler Changed;

	static TextSelectionHighlightLayer()
	{
		_selectedValue = new object();
	}

	internal TextSelectionHighlightLayer(ITextSelection selection)
	{
		_selection = selection;
		_selection.Changed += OnSelectionChanged;
		_oldStart = _selection.Start;
		_oldEnd = _selection.End;
	}

	internal override object GetHighlightValue(StaticTextPointer textPosition, LogicalDirection direction)
	{
		if (IsContentHighlighted(textPosition, direction))
		{
			return _selectedValue;
		}
		return DependencyProperty.UnsetValue;
	}

	internal override bool IsContentHighlighted(StaticTextPointer textPosition, LogicalDirection direction)
	{
		if (_selection.IsInterimSelection)
		{
			return false;
		}
		List<TextSegment> textSegments = _selection.TextSegments;
		int count = textSegments.Count;
		for (int i = 0; i < count; i++)
		{
			TextSegment textSegment = textSegments[i];
			if ((direction == LogicalDirection.Forward && textSegment.Start.CompareTo(textPosition) <= 0 && textPosition.CompareTo(textSegment.End) < 0) || (direction == LogicalDirection.Backward && textSegment.Start.CompareTo(textPosition) < 0 && textPosition.CompareTo(textSegment.End) <= 0))
			{
				return true;
			}
		}
		return false;
	}

	internal override StaticTextPointer GetNextChangePosition(StaticTextPointer textPosition, LogicalDirection direction)
	{
		StaticTextPointer result = StaticTextPointer.Null;
		if (!IsTextRangeEmpty(_selection) && !_selection.IsInterimSelection)
		{
			List<TextSegment> textSegments = _selection.TextSegments;
			int count = textSegments.Count;
			if (direction == LogicalDirection.Forward)
			{
				for (int i = 0; i < count; i++)
				{
					TextSegment textSegment = textSegments[i];
					if (textSegment.Start.CompareTo(textSegment.End) != 0)
					{
						if (textPosition.CompareTo(textSegment.Start) < 0)
						{
							result = textSegment.Start.CreateStaticPointer();
							break;
						}
						if (textPosition.CompareTo(textSegment.End) < 0)
						{
							result = textSegment.End.CreateStaticPointer();
							break;
						}
					}
				}
			}
			else
			{
				for (int num = count - 1; num >= 0; num--)
				{
					TextSegment textSegment = textSegments[num];
					if (textSegment.Start.CompareTo(textSegment.End) != 0)
					{
						if (textPosition.CompareTo(textSegment.End) > 0)
						{
							result = textSegment.End.CreateStaticPointer();
							break;
						}
						if (textPosition.CompareTo(textSegment.Start) > 0)
						{
							result = textSegment.Start.CreateStaticPointer();
							break;
						}
					}
				}
			}
		}
		return result;
	}

	internal void InternalOnSelectionChanged()
	{
		ITextPointer textPointer = (_selection.IsInterimSelection ? _selection.End : _selection.Start);
		ITextPointer end = _selection.End;
		ITextPointer textPointer2;
		ITextPointer textPointer3;
		if (_oldStart.CompareTo(textPointer) < 0)
		{
			textPointer2 = _oldStart;
			textPointer3 = TextPointerBase.Min(textPointer, _oldEnd);
		}
		else
		{
			textPointer2 = textPointer;
			textPointer3 = TextPointerBase.Min(end, _oldStart);
		}
		ITextPointer textPointer4;
		ITextPointer textPointer5;
		if (_oldEnd.CompareTo(end) < 0)
		{
			textPointer4 = TextPointerBase.Max(textPointer, _oldEnd);
			textPointer5 = end;
		}
		else
		{
			textPointer4 = TextPointerBase.Max(end, _oldStart);
			textPointer5 = _oldEnd;
		}
		_oldStart = textPointer;
		_oldEnd = end;
		if (Changed != null && (textPointer2.CompareTo(textPointer3) != 0 || textPointer4.CompareTo(textPointer5) != 0))
		{
			TextSelectionHighlightChangedEventArgs args = new TextSelectionHighlightChangedEventArgs(textPointer2, textPointer3, textPointer4, textPointer5);
			Changed(this, args);
		}
	}

	private void OnSelectionChanged(object sender, EventArgs e)
	{
		Invariant.Assert(_selection == (ITextSelection)sender);
		InternalOnSelectionChanged();
	}

	private bool IsTextRangeEmpty(ITextRange textRange)
	{
		Invariant.Assert(textRange._TextSegments.Count > 0);
		return textRange._TextSegments[0].Start.CompareTo(textRange._TextSegments[textRange._TextSegments.Count - 1].End) == 0;
	}
}
