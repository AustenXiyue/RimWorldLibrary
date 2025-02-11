using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Documents;
using MS.Internal;

namespace System.Windows.Annotations;

/// <summary>Represents a selection of content that an annotation is anchored to.</summary>
public sealed class TextAnchor
{
	private class TextSegmentComparer : IComparer<TextSegment>
	{
		public int Compare(TextSegment x, TextSegment y)
		{
			if (x.Equals(TextSegment.Null))
			{
				if (y.Equals(TextSegment.Null))
				{
					return 0;
				}
				return -1;
			}
			if (y.Equals(TextSegment.Null))
			{
				return 1;
			}
			int num = x.Start.CompareTo(y.Start);
			if (num != 0)
			{
				return num;
			}
			return x.End.CompareTo(y.End);
		}
	}

	private List<TextSegment> _segments = new List<TextSegment>(1);

	/// <summary>Gets the beginning position of the text anchor.</summary>
	/// <returns>The beginning position of the text anchor.</returns>
	public ContentPosition BoundingStart => Start as ContentPosition;

	/// <summary>Gets the end position of the text anchor.</summary>
	/// <returns>The end position of the text anchor.</returns>
	public ContentPosition BoundingEnd => End as ContentPosition;

	internal ITextPointer Start
	{
		get
		{
			if (_segments.Count <= 0)
			{
				return null;
			}
			return _segments[0].Start;
		}
	}

	internal ITextPointer End
	{
		get
		{
			if (_segments.Count <= 0)
			{
				return null;
			}
			return _segments[_segments.Count - 1].End;
		}
	}

	internal bool IsEmpty
	{
		get
		{
			if (_segments.Count == 1)
			{
				return _segments[0].Start == _segments[0].End;
			}
			return false;
		}
	}

	internal string Text
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < _segments.Count; i++)
			{
				stringBuilder.Append(TextRangeBase.GetTextInternal(_segments[i].Start, _segments[i].End));
			}
			return stringBuilder.ToString();
		}
	}

	internal ReadOnlyCollection<TextSegment> TextSegments => _segments.AsReadOnly();

	internal TextAnchor()
	{
	}

	internal TextAnchor(TextAnchor anchor)
	{
		Invariant.Assert(anchor != null, "Anchor to clone is null.");
		foreach (TextSegment textSegment in anchor.TextSegments)
		{
			_segments.Add(new TextSegment(textSegment.Start, textSegment.End));
		}
	}

	internal bool Contains(ITextPointer textPointer)
	{
		if (textPointer == null)
		{
			throw new ArgumentNullException("textPointer");
		}
		if (textPointer.TextContainer != Start.TextContainer)
		{
			throw new ArgumentException(SR.Format(SR.NotInAssociatedTree, "textPointer"));
		}
		if (textPointer.CompareTo(Start) < 0)
		{
			textPointer = textPointer.GetInsertionPosition(LogicalDirection.Forward);
		}
		else if (textPointer.CompareTo(End) > 0)
		{
			textPointer = textPointer.GetInsertionPosition(LogicalDirection.Backward);
		}
		for (int i = 0; i < _segments.Count; i++)
		{
			if (_segments[i].Contains(textPointer))
			{
				return true;
			}
		}
		return false;
	}

	internal void AddTextSegment(ITextPointer start, ITextPointer end)
	{
		Invariant.Assert(start != null, "Non-null start required to create segment.");
		Invariant.Assert(end != null, "Non-null end required to create segment.");
		TextSegment newSegment = CreateNormalizedSegment(start, end);
		InsertSegment(newSegment);
	}

	/// <summary>Returns the hash code of the text anchor instance.</summary>
	/// <returns>The hash code of the text anchor instance.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Returns a value that indicates whether the text anchor is equal to the specified object. </summary>
	/// <returns>true if the two instances are equal; otherwise, false.</returns>
	/// <param name="obj">The object to compare to.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is TextAnchor textAnchor))
		{
			return false;
		}
		if (textAnchor._segments.Count != _segments.Count)
		{
			return false;
		}
		for (int i = 0; i < _segments.Count; i++)
		{
			if (_segments[i].Start.CompareTo(textAnchor._segments[i].Start) != 0 || _segments[i].End.CompareTo(textAnchor._segments[i].End) != 0)
			{
				return false;
			}
		}
		return true;
	}

	internal bool IsOverlapping(ICollection<TextSegment> textSegments)
	{
		Invariant.Assert(textSegments != null, "TextSegments must not be null.");
		textSegments = SortTextSegments(textSegments, excludeZeroLength: false);
		IEnumerator<TextSegment> enumerator = _segments.GetEnumerator();
		IEnumerator<TextSegment> enumerator2 = textSegments.GetEnumerator();
		bool flag = enumerator.MoveNext();
		bool flag2 = enumerator2.MoveNext();
		while (flag && flag2)
		{
			TextSegment current = enumerator.Current;
			TextSegment current2 = enumerator2.Current;
			if (current2.Start.CompareTo(current2.End) == 0)
			{
				if (current.Start.CompareTo(current2.Start) == 0 && current2.Start.LogicalDirection == LogicalDirection.Forward)
				{
					return true;
				}
				if (current.End.CompareTo(current2.End) == 0 && current2.End.LogicalDirection == LogicalDirection.Backward)
				{
					return true;
				}
			}
			if (current.Start.CompareTo(current2.End) >= 0)
			{
				flag2 = enumerator2.MoveNext();
				continue;
			}
			if (current.End.CompareTo(current2.Start) <= 0)
			{
				flag = enumerator.MoveNext();
				continue;
			}
			return true;
		}
		return false;
	}

	internal static TextAnchor ExclusiveUnion(TextAnchor anchor, TextAnchor otherAnchor)
	{
		Invariant.Assert(anchor != null, "anchor must not be null.");
		Invariant.Assert(otherAnchor != null, "otherAnchor must not be null.");
		foreach (TextSegment textSegment in otherAnchor.TextSegments)
		{
			anchor.InsertSegment(textSegment);
		}
		return anchor;
	}

	internal static TextAnchor TrimToRelativeComplement(TextAnchor anchor, ICollection<TextSegment> textSegments)
	{
		Invariant.Assert(anchor != null, "Anchor must not be null.");
		Invariant.Assert(textSegments != null, "TextSegments must not be null.");
		textSegments = SortTextSegments(textSegments, excludeZeroLength: true);
		IEnumerator<TextSegment> enumerator = textSegments.GetEnumerator();
		bool flag = enumerator.MoveNext();
		int num = 0;
		TextSegment textSegment = TextSegment.Null;
		while (num < anchor._segments.Count && flag)
		{
			Invariant.Assert(textSegment.Equals(TextSegment.Null) || textSegment.Equals(enumerator.Current) || textSegment.End.CompareTo(enumerator.Current.Start) <= 0, "TextSegments are overlapping or not ordered.");
			TextSegment textSegment2 = anchor._segments[num];
			textSegment = enumerator.Current;
			if (textSegment2.Start.CompareTo(textSegment.End) >= 0)
			{
				flag = enumerator.MoveNext();
				continue;
			}
			if (textSegment2.Start.CompareTo(textSegment.Start) >= 0)
			{
				if (textSegment2.End.CompareTo(textSegment.End) <= 0)
				{
					anchor._segments.RemoveAt(num);
					continue;
				}
				anchor._segments[num] = CreateNormalizedSegment(textSegment.End, textSegment2.End);
				flag = enumerator.MoveNext();
				continue;
			}
			if (textSegment2.End.CompareTo(textSegment.Start) > 0)
			{
				anchor._segments[num] = CreateNormalizedSegment(textSegment2.Start, textSegment.Start);
				if (textSegment2.End.CompareTo(textSegment.End) > 0)
				{
					anchor._segments.Insert(num + 1, CreateNormalizedSegment(textSegment.End, textSegment2.End));
					flag = enumerator.MoveNext();
				}
			}
			num++;
		}
		if (anchor._segments.Count > 0)
		{
			return anchor;
		}
		return null;
	}

	internal static TextAnchor TrimToIntersectionWith(TextAnchor anchor, ICollection<TextSegment> textSegments)
	{
		Invariant.Assert(anchor != null, "Anchor must not be null.");
		Invariant.Assert(textSegments != null, "TextSegments must not be null.");
		textSegments = SortTextSegments(textSegments, excludeZeroLength: true);
		TextSegment textSegment = TextSegment.Null;
		int num = 0;
		IEnumerator<TextSegment> enumerator = textSegments.GetEnumerator();
		bool flag = enumerator.MoveNext();
		while (num < anchor._segments.Count && flag)
		{
			Invariant.Assert(textSegment.Equals(TextSegment.Null) || textSegment.Equals(enumerator.Current) || textSegment.End.CompareTo(enumerator.Current.Start) <= 0, "TextSegments are overlapping or not ordered.");
			TextSegment textSegment2 = anchor._segments[num];
			textSegment = enumerator.Current;
			if (textSegment2.Start.CompareTo(textSegment.End) >= 0)
			{
				flag = enumerator.MoveNext();
				continue;
			}
			if (textSegment2.End.CompareTo(textSegment.Start) <= 0)
			{
				anchor._segments.RemoveAt(num);
				continue;
			}
			if (textSegment2.Start.CompareTo(textSegment.Start) < 0)
			{
				anchor._segments[num] = CreateNormalizedSegment(textSegment.Start, textSegment2.End);
				continue;
			}
			if (textSegment2.End.CompareTo(textSegment.End) > 0)
			{
				anchor._segments[num] = CreateNormalizedSegment(textSegment2.Start, textSegment.End);
				anchor._segments.Insert(num + 1, CreateNormalizedSegment(textSegment.End, textSegment2.End));
				flag = enumerator.MoveNext();
			}
			else if (textSegment2.End.CompareTo(textSegment.End) == 0)
			{
				flag = enumerator.MoveNext();
			}
			num++;
		}
		if (!flag && num < anchor._segments.Count)
		{
			anchor._segments.RemoveRange(num, anchor._segments.Count - num);
		}
		if (anchor._segments.Count == 0)
		{
			return null;
		}
		return anchor;
	}

	private static ICollection<TextSegment> SortTextSegments(ICollection<TextSegment> textSegments, bool excludeZeroLength)
	{
		Invariant.Assert(textSegments != null, "TextSegments must not be null.");
		List<TextSegment> list = new List<TextSegment>(textSegments.Count);
		list.AddRange(textSegments);
		if (excludeZeroLength)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				TextSegment item = list[num];
				if (item.Start.CompareTo(item.End) >= 0)
				{
					list.Remove(item);
				}
			}
		}
		if (list.Count > 1)
		{
			list.Sort(new TextSegmentComparer());
		}
		return list;
	}

	private void InsertSegment(TextSegment newSegment)
	{
		int i;
		for (i = 0; i < _segments.Count && newSegment.Start.CompareTo(_segments[i].Start) >= 0; i++)
		{
		}
		if (i > 0 && newSegment.Start.CompareTo(_segments[i - 1].End) < 0)
		{
			throw new InvalidOperationException(SR.TextSegmentsMustNotOverlap);
		}
		if (i < _segments.Count && newSegment.End.CompareTo(_segments[i].Start) > 0)
		{
			throw new InvalidOperationException(SR.TextSegmentsMustNotOverlap);
		}
		_segments.Insert(i, newSegment);
	}

	private static TextSegment CreateNormalizedSegment(ITextPointer start, ITextPointer end)
	{
		if (start.CompareTo(end) == 0)
		{
			if (!TextPointerBase.IsAtInsertionPosition(start, start.LogicalDirection))
			{
				start = start.GetInsertionPosition(start.LogicalDirection);
				end = start;
			}
		}
		else
		{
			if (!TextPointerBase.IsAtInsertionPosition(start, start.LogicalDirection))
			{
				start = start.GetInsertionPosition(LogicalDirection.Forward);
			}
			if (!TextPointerBase.IsAtInsertionPosition(end, start.LogicalDirection))
			{
				end = end.GetInsertionPosition(LogicalDirection.Backward);
			}
			if (start.CompareTo(end) >= 0)
			{
				if (start.LogicalDirection == LogicalDirection.Backward)
				{
					start = end.GetFrozenPointer(LogicalDirection.Backward);
				}
				end = start;
			}
		}
		return new TextSegment(start, end);
	}
}
