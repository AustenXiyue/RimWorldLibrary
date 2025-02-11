using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using MS.Internal.Annotations.Anchoring;

namespace MS.Internal.Annotations.Component;

internal class AnnotationHighlightLayer : HighlightLayer
{
	private class AnnotationHighlightChangedEventArgs : HighlightChangedEventArgs
	{
		private readonly ReadOnlyCollection<TextSegment> _ranges;

		internal override IList Ranges => _ranges;

		internal override Type OwnerType => typeof(HighlightComponent);

		internal AnnotationHighlightChangedEventArgs(ITextPointer start, ITextPointer end)
		{
			TextSegment[] list = new TextSegment[1]
			{
				new TextSegment(start, end)
			};
			_ranges = new ReadOnlyCollection<TextSegment>(list);
		}
	}

	internal sealed class HighlightSegment : Shape
	{
		private TextSegment _segment;

		private List<TextSegment> _contentSegments = new List<TextSegment>(1);

		private readonly List<IHighlightRange> _owners;

		private List<IHighlightRange> _activeOwners = new List<IHighlightRange>();

		private IHighlightRange _cachedTopOwner;

		private bool _isFixedContainer;

		protected override Geometry DefiningGeometry
		{
			get
			{
				if (_isFixedContainer)
				{
					return Geometry.Empty;
				}
				ITextView documentPageTextView = TextSelectionHelper.GetDocumentPageTextView(TopOwner.Range.Start.CreatePointer(LogicalDirection.Forward));
				GeometryGroup geometryGroup = new GeometryGroup();
				if (TopOwner.HighlightContent)
				{
					foreach (TextSegment contentSegment in _contentSegments)
					{
						GetSegmentGeometry(geometryGroup, contentSegment, documentPageTextView);
					}
				}
				else
				{
					GetSegmentGeometry(geometryGroup, _segment, documentPageTextView);
				}
				if (TopOwner is UIElement uIElement)
				{
					uIElement.RenderTransform = Transform.Identity;
				}
				return geometryGroup;
			}
		}

		internal TextSegment Segment => _segment;

		internal IHighlightRange TopOwner
		{
			get
			{
				if (_activeOwners.Count != 0)
				{
					return _activeOwners[0];
				}
				if (_owners.Count <= 0)
				{
					return null;
				}
				return _owners[0];
			}
		}

		private Brush OwnerColor
		{
			get
			{
				if (_activeOwners.Count != 0)
				{
					return new SolidColorBrush(_activeOwners[0].SelectedBackground);
				}
				if (_owners.Count <= 0)
				{
					return null;
				}
				return new SolidColorBrush(_owners[0].Background);
			}
		}

		internal HighlightSegment(ITextPointer start, ITextPointer end, IHighlightRange owner)
		{
			List<IHighlightRange> owners = new List<IHighlightRange>(1) { owner };
			Init(start, end, owners);
			_owners = owners;
			UpdateOwners();
		}

		internal HighlightSegment(ITextPointer start, ITextPointer end, IList<IHighlightRange> owners)
		{
			Init(start, end, owners);
			_owners = new List<IHighlightRange>(owners.Count);
			_owners.AddRange(owners);
			UpdateOwners();
		}

		private void Init(ITextPointer start, ITextPointer end, IList<IHighlightRange> owners)
		{
			for (int i = 0; i < owners.Count; i++)
			{
			}
			_segment = new TextSegment(start, end);
			base.IsHitTestVisible = false;
			object textContainer = start.TextContainer;
			_isFixedContainer = textContainer is FixedTextContainer || textContainer is DocumentSequenceTextContainer;
			GetContent();
		}

		internal void AddOwner(IHighlightRange owner)
		{
			for (int i = 0; i < _owners.Count; i++)
			{
				if (_owners[i].Priority < owner.Priority)
				{
					_owners.Insert(i, owner);
					UpdateOwners();
					return;
				}
			}
			_owners.Add(owner);
			UpdateOwners();
		}

		internal int RemoveOwner(IHighlightRange owner)
		{
			if (_owners.Contains(owner))
			{
				if (_activeOwners.Contains(owner))
				{
					_activeOwners.Remove(owner);
				}
				_owners.Remove(owner);
				UpdateOwners();
			}
			return _owners.Count;
		}

		internal void AddActiveOwner(IHighlightRange owner)
		{
			if (_owners.Contains(owner))
			{
				_activeOwners.Add(owner);
				UpdateOwners();
			}
		}

		private void AddActiveOwners(List<IHighlightRange> owners)
		{
			_activeOwners.AddRange(owners);
			UpdateOwners();
		}

		internal void RemoveActiveOwner(IHighlightRange owner)
		{
			if (_activeOwners.Contains(owner))
			{
				_activeOwners.Remove(owner);
				UpdateOwners();
			}
		}

		internal void ClearOwners()
		{
			_owners.Clear();
			_activeOwners.Clear();
			UpdateOwners();
		}

		internal IList<HighlightSegment> Split(ITextPointer ps, LogicalDirection side)
		{
			IList<HighlightSegment> list = null;
			if (ps.CompareTo(_segment.Start) == 0 || ps.CompareTo(_segment.End) == 0)
			{
				if ((ps.CompareTo(_segment.Start) == 0 && side == LogicalDirection.Forward) || (ps.CompareTo(_segment.End) == 0 && side == LogicalDirection.Backward))
				{
					list = new List<HighlightSegment>(1);
					list.Add(this);
				}
			}
			else if (_segment.Contains(ps))
			{
				list = new List<HighlightSegment>(2);
				list.Add(new HighlightSegment(_segment.Start, ps, _owners));
				list.Add(new HighlightSegment(ps, _segment.End, _owners));
				list[0].AddActiveOwners(_activeOwners);
				list[1].AddActiveOwners(_activeOwners);
			}
			return list;
		}

		internal IList<HighlightSegment> Split(ITextPointer ps1, ITextPointer ps2, IHighlightRange newOwner)
		{
			IList<HighlightSegment> list = new List<HighlightSegment>();
			if (ps1.CompareTo(ps2) == 0)
			{
				if (_segment.Start.CompareTo(ps1) > 0 || _segment.End.CompareTo(ps1) < 0)
				{
					return list;
				}
				if (_segment.Start.CompareTo(ps1) < 0)
				{
					list.Add(new HighlightSegment(_segment.Start, ps1, _owners));
				}
				list.Add(new HighlightSegment(ps1, ps1, _owners));
				if (_segment.End.CompareTo(ps1) > 0)
				{
					list.Add(new HighlightSegment(ps1, _segment.End, _owners));
				}
				foreach (HighlightSegment item in list)
				{
					item.AddActiveOwners(_activeOwners);
				}
			}
			else if (_segment.Contains(ps1))
			{
				IList<HighlightSegment> list2 = Split(ps1, LogicalDirection.Forward);
				for (int i = 0; i < list2.Count; i++)
				{
					if (list2[i].Segment.Contains(ps2))
					{
						IList<HighlightSegment> list3 = list2[i].Split(ps2, LogicalDirection.Backward);
						for (int j = 0; j < list3.Count; j++)
						{
							list.Add(list3[j]);
						}
						if (!list3.Contains(list2[i]))
						{
							list2[i].Discard();
						}
					}
					else
					{
						list.Add(list2[i]);
					}
				}
			}
			else
			{
				list = Split(ps2, LogicalDirection.Backward);
			}
			if (list != null && list.Count > 0 && newOwner != null)
			{
				if (list.Count == 3)
				{
					list[1].AddOwner(newOwner);
				}
				else if (list[0].Segment.Start.CompareTo(ps1) == 0 || list[0].Segment.End.CompareTo(ps2) == 0)
				{
					list[0].AddOwner(newOwner);
				}
				else
				{
					list[1].AddOwner(newOwner);
				}
			}
			return list;
		}

		internal void UpdateOwners()
		{
			if (_cachedTopOwner != TopOwner)
			{
				if (_cachedTopOwner != null)
				{
					_cachedTopOwner.RemoveChild(this);
				}
				_cachedTopOwner = TopOwner;
				if (_cachedTopOwner != null)
				{
					_cachedTopOwner.AddChild(this);
				}
			}
			base.Fill = OwnerColor;
		}

		internal void Discard()
		{
			if (TopOwner != null)
			{
				TopOwner.RemoveChild(this);
			}
			_activeOwners.Clear();
			_owners.Clear();
		}

		private void GetSegmentGeometry(GeometryGroup geometry, TextSegment segment, ITextView parentView)
		{
			foreach (ITextView documentPageTextView in TextSelectionHelper.GetDocumentPageTextViews(segment))
			{
				Geometry pageGeometry = GetPageGeometry(segment, documentPageTextView, parentView);
				if (pageGeometry != null)
				{
					geometry.Children.Add(pageGeometry);
				}
			}
		}

		private Geometry GetPageGeometry(TextSegment segment, ITextView view, ITextView parentView)
		{
			if (!view.IsValid || !parentView.IsValid)
			{
				return null;
			}
			if (view.RenderScope == null || parentView.RenderScope == null)
			{
				return null;
			}
			Geometry geometry = null;
			geometry = view.GetTightBoundingGeometryFromTextPositions(segment.Start, segment.End);
			if (geometry != null && parentView != null)
			{
				Transform transform = (Transform)view.RenderScope.TransformToVisual(parentView.RenderScope);
				if (geometry.Transform != null)
				{
					TransformGroup transformGroup = new TransformGroup();
					transformGroup.Children.Add(geometry.Transform);
					transformGroup.Children.Add(transform);
					geometry.Transform = transformGroup;
				}
				else
				{
					geometry.Transform = transform;
				}
			}
			return geometry;
		}

		private void GetContent()
		{
			_contentSegments.Clear();
			ITextPointer textPointer = _segment.Start.CreatePointer();
			ITextPointer segmentStart = null;
			while (textPointer.CompareTo(_segment.End) < 0)
			{
				switch (textPointer.GetPointerContext(LogicalDirection.Forward))
				{
				case TextPointerContext.ElementStart:
				{
					Type elementType = textPointer.GetElementType(LogicalDirection.Forward);
					if (typeof(Run).IsAssignableFrom(elementType) || typeof(BlockUIContainer).IsAssignableFrom(elementType))
					{
						OpenSegment(ref segmentStart, textPointer);
					}
					else if (typeof(Table).IsAssignableFrom(elementType) || typeof(Floater).IsAssignableFrom(elementType) || typeof(Figure).IsAssignableFrom(elementType))
					{
						CloseSegment(ref segmentStart, textPointer, _segment.End);
					}
					textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
					if (typeof(Run).IsAssignableFrom(elementType) || typeof(BlockUIContainer).IsAssignableFrom(elementType))
					{
						textPointer.MoveToElementEdge(ElementEdge.AfterEnd);
					}
					break;
				}
				case TextPointerContext.ElementEnd:
				{
					Type parentType = textPointer.ParentType;
					if (typeof(TableCell).IsAssignableFrom(parentType) || typeof(Floater).IsAssignableFrom(parentType) || typeof(Figure).IsAssignableFrom(parentType))
					{
						CloseSegment(ref segmentStart, textPointer, _segment.End);
					}
					textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
					break;
				}
				case TextPointerContext.Text:
				case TextPointerContext.EmbeddedElement:
					OpenSegment(ref segmentStart, textPointer);
					textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
					break;
				default:
					Invariant.Assert(condition: false, "unexpected TextPointerContext");
					break;
				}
			}
			CloseSegment(ref segmentStart, textPointer, _segment.End);
		}

		private void OpenSegment(ref ITextPointer segmentStart, ITextPointer cursor)
		{
			if (segmentStart == null)
			{
				segmentStart = cursor.GetInsertionPosition(LogicalDirection.Forward);
			}
		}

		private void CloseSegment(ref ITextPointer segmentStart, ITextPointer cursor, ITextPointer end)
		{
			if (segmentStart != null)
			{
				if (cursor.CompareTo(end) > 0)
				{
					cursor = end;
				}
				ITextPointer insertionPosition = cursor.GetInsertionPosition(LogicalDirection.Backward);
				if (segmentStart.CompareTo(insertionPosition) < 0)
				{
					_contentSegments.Add(new TextSegment(segmentStart, insertionPosition));
				}
				segmentStart = null;
			}
		}
	}

	private List<HighlightSegment> _segments;

	private bool _isFixedContainer;

	internal override Type OwnerType => typeof(HighlightComponent);

	private bool IsFixedContainer
	{
		get
		{
			return _isFixedContainer;
		}
		set
		{
			_isFixedContainer = value;
		}
	}

	internal override event HighlightChangedEventHandler Changed;

	internal AnnotationHighlightLayer()
	{
		_segments = new List<HighlightSegment>();
	}

	internal void AddRange(IHighlightRange highlightRange)
	{
		Invariant.Assert(highlightRange != null, "the owner is null");
		ITextPointer start = highlightRange.Range.Start;
		ITextPointer end = highlightRange.Range.End;
		if (start.CompareTo(end) != 0)
		{
			if (_segments.Count == 0)
			{
				object textContainer = start.TextContainer;
				IsFixedContainer = textContainer is FixedTextContainer || textContainer is DocumentSequenceTextContainer;
			}
			ProcessOverlapingSegments(highlightRange, out var invalidateStart, out var invalidateEnd);
			if (Changed != null && IsFixedContainer)
			{
				Changed(this, new AnnotationHighlightChangedEventArgs(invalidateStart, invalidateEnd));
			}
		}
	}

	internal void RemoveRange(IHighlightRange highlightRange)
	{
		if (highlightRange.Range.Start.CompareTo(highlightRange.Range.End) == 0)
		{
			return;
		}
		GetSpannedSegments(highlightRange.Range.Start, highlightRange.Range.End, out var startSeg, out var endSeg);
		ITextPointer start = _segments[startSeg].Segment.Start;
		ITextPointer end = _segments[endSeg].Segment.End;
		int num = startSeg;
		while (num <= endSeg)
		{
			HighlightSegment highlightSegment = _segments[num];
			if (highlightSegment.RemoveOwner(highlightRange) == 0)
			{
				_segments.Remove(highlightSegment);
				endSeg--;
			}
			else
			{
				num++;
			}
		}
		if (Changed != null && IsFixedContainer)
		{
			Changed(this, new AnnotationHighlightChangedEventArgs(start, end));
		}
	}

	internal void ModifiedRange(IHighlightRange highlightRange)
	{
		Invariant.Assert(highlightRange != null, "null range data");
		if (highlightRange.Range.Start.CompareTo(highlightRange.Range.End) != 0)
		{
			GetSpannedSegments(highlightRange.Range.Start, highlightRange.Range.End, out var startSeg, out var endSeg);
			for (int i = startSeg; i < endSeg; i++)
			{
				_segments[i].UpdateOwners();
			}
			ITextPointer start = _segments[startSeg].Segment.Start;
			ITextPointer end = _segments[endSeg].Segment.End;
			if (Changed != null && IsFixedContainer)
			{
				Changed(this, new AnnotationHighlightChangedEventArgs(start, end));
			}
		}
	}

	internal void ActivateRange(IHighlightRange highlightRange, bool activate)
	{
		Invariant.Assert(highlightRange != null, "null range data");
		if (highlightRange.Range.Start.CompareTo(highlightRange.Range.End) == 0)
		{
			return;
		}
		GetSpannedSegments(highlightRange.Range.Start, highlightRange.Range.End, out var startSeg, out var endSeg);
		ITextPointer start = _segments[startSeg].Segment.Start;
		ITextPointer end = _segments[endSeg].Segment.End;
		for (int i = startSeg; i <= endSeg; i++)
		{
			if (activate)
			{
				_segments[i].AddActiveOwner(highlightRange);
			}
			else
			{
				_segments[i].RemoveActiveOwner(highlightRange);
			}
		}
		if (Changed != null && IsFixedContainer)
		{
			Changed(this, new AnnotationHighlightChangedEventArgs(start, end));
		}
	}

	internal override object GetHighlightValue(StaticTextPointer textPosition, LogicalDirection direction)
	{
		object result = DependencyProperty.UnsetValue;
		HighlightSegment highlightSegment = null;
		for (int i = 0; i < _segments.Count; i++)
		{
			highlightSegment = _segments[i];
			if (highlightSegment.Segment.Start.CompareTo(textPosition) > 0 || (highlightSegment.Segment.Start.CompareTo(textPosition) == 0 && direction == LogicalDirection.Backward))
			{
				break;
			}
			if (highlightSegment.Segment.End.CompareTo(textPosition) > 0 || (highlightSegment.Segment.End.CompareTo(textPosition) == 0 && direction == LogicalDirection.Backward))
			{
				result = highlightSegment;
				break;
			}
		}
		return result;
	}

	internal override bool IsContentHighlighted(StaticTextPointer staticTextPosition, LogicalDirection direction)
	{
		return GetHighlightValue(staticTextPosition, direction) != DependencyProperty.UnsetValue;
	}

	internal override StaticTextPointer GetNextChangePosition(StaticTextPointer textPosition, LogicalDirection direction)
	{
		return ((direction != LogicalDirection.Forward) ? GetNextBackwardPosition(textPosition) : GetNextForwardPosition(textPosition))?.CreateStaticPointer() ?? StaticTextPointer.Null;
	}

	private void ProcessOverlapingSegments(IHighlightRange highlightRange, out ITextPointer invalidateStart, out ITextPointer invalidateEnd)
	{
		ReadOnlyCollection<TextSegment> textSegments = highlightRange.Range.TextSegments;
		invalidateStart = null;
		invalidateEnd = null;
		int num = 0;
		IEnumerator<TextSegment> enumerator = textSegments.GetEnumerator();
		TextSegment textSegment = (enumerator.MoveNext() ? enumerator.Current : TextSegment.Null);
		while (num < _segments.Count && !textSegment.IsNull)
		{
			HighlightSegment highlightSegment = _segments[num];
			if (highlightSegment.Segment.Start.CompareTo(textSegment.Start) <= 0)
			{
				if (highlightSegment.Segment.End.CompareTo(textSegment.Start) > 0)
				{
					IList<HighlightSegment> list = highlightSegment.Split(textSegment.Start, textSegment.End, highlightRange);
					if (!list.Contains(highlightSegment))
					{
						highlightSegment.ClearOwners();
					}
					_segments.Remove(highlightSegment);
					_segments.InsertRange(num, list);
					num = num + list.Count - 1;
					textSegment = ((textSegment.End.CompareTo(highlightSegment.Segment.End) > 0) ? new TextSegment(highlightSegment.Segment.End, textSegment.End) : (enumerator.MoveNext() ? enumerator.Current : TextSegment.Null));
					if (invalidateStart == null)
					{
						invalidateStart = highlightSegment.Segment.Start;
					}
				}
				else
				{
					num++;
				}
			}
			else
			{
				if (invalidateStart == null)
				{
					invalidateStart = textSegment.Start;
				}
				if (textSegment.End.CompareTo(highlightSegment.Segment.Start) > 0)
				{
					HighlightSegment item = new HighlightSegment(textSegment.Start, highlightSegment.Segment.Start, highlightRange);
					_segments.Insert(num++, item);
					textSegment = new TextSegment(highlightSegment.Segment.Start, textSegment.End);
				}
				else
				{
					_segments.Insert(num++, new HighlightSegment(textSegment.Start, textSegment.End, highlightRange));
					textSegment = (enumerator.MoveNext() ? enumerator.Current : TextSegment.Null);
				}
			}
		}
		if (!textSegment.IsNull)
		{
			if (invalidateStart == null)
			{
				invalidateStart = textSegment.Start;
			}
			_segments.Insert(num++, new HighlightSegment(textSegment.Start, textSegment.End, highlightRange));
		}
		while (enumerator.MoveNext())
		{
			_segments.Insert(num++, new HighlightSegment(enumerator.Current.Start, enumerator.Current.End, highlightRange));
		}
		if (invalidateStart != null)
		{
			if (num == _segments.Count)
			{
				num--;
			}
			invalidateEnd = _segments[num].Segment.End;
		}
	}

	private ITextPointer GetNextForwardPosition(StaticTextPointer pos)
	{
		for (int i = 0; i < _segments.Count; i++)
		{
			HighlightSegment highlightSegment = _segments[i];
			if (pos.CompareTo(highlightSegment.Segment.Start) >= 0)
			{
				if (pos.CompareTo(highlightSegment.Segment.End) < 0)
				{
					return highlightSegment.Segment.End;
				}
				continue;
			}
			return highlightSegment.Segment.Start;
		}
		return null;
	}

	private ITextPointer GetNextBackwardPosition(StaticTextPointer pos)
	{
		int num = _segments.Count - 1;
		while (num >= 0)
		{
			HighlightSegment highlightSegment = _segments[num];
			if (pos.CompareTo(highlightSegment.Segment.End) <= 0)
			{
				if (pos.CompareTo(highlightSegment.Segment.Start) > 0)
				{
					return highlightSegment.Segment.Start;
				}
				num--;
				continue;
			}
			return highlightSegment.Segment.End;
		}
		return null;
	}

	private void GetSpannedSegments(ITextPointer start, ITextPointer end, out int startSeg, out int endSeg)
	{
		startSeg = -1;
		endSeg = -1;
		for (int i = 0; i < _segments.Count; i++)
		{
			HighlightSegment highlightSegment = _segments[i];
			if (highlightSegment.Segment.Start.CompareTo(start) == 0)
			{
				startSeg = i;
			}
			if (highlightSegment.Segment.End.CompareTo(end) == 0)
			{
				endSeg = i;
				break;
			}
		}
		if (startSeg >= 0 && endSeg >= 0)
		{
			_ = startSeg;
			_ = endSeg;
		}
	}
}
