using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using MS.Internal.Documents;

namespace System.Windows.Documents;

internal sealed class FixedTextView : TextViewBase
{
	private readonly FixedDocumentPage _docPage;

	private FixedTextPointer _start;

	private FixedTextPointer _end;

	private ReadOnlyCollection<TextSegment> _textSegments;

	private static DependencyObjectType UIElementType = DependencyObjectType.FromSystemTypeInternal(typeof(UIElement));

	internal override UIElement RenderScope
	{
		get
		{
			Visual visual = _docPage.Visual;
			while (visual != null && !(visual is UIElement))
			{
				visual = VisualTreeHelper.GetParent(visual) as Visual;
			}
			return visual as UIElement;
		}
	}

	internal override ITextContainer TextContainer => Container;

	internal override bool IsValid => true;

	internal override bool RendersOwnSelection => true;

	internal override ReadOnlyCollection<TextSegment> TextSegments
	{
		get
		{
			if (_textSegments == null)
			{
				List<TextSegment> list = new List<TextSegment>(1);
				list.Add(new TextSegment(Start, End, preserveLogicalDirection: true));
				_textSegments = new ReadOnlyCollection<TextSegment>(list);
			}
			return _textSegments;
		}
	}

	internal FixedTextPointer Start
	{
		get
		{
			if (_start == null)
			{
				FlowPosition pageStartFlowPosition = Container.FixedTextBuilder.GetPageStartFlowPosition(PageIndex);
				_start = new FixedTextPointer(mutable: false, LogicalDirection.Forward, pageStartFlowPosition);
			}
			return _start;
		}
	}

	internal FixedTextPointer End
	{
		get
		{
			if (_end == null)
			{
				FlowPosition pageEndFlowPosition = Container.FixedTextBuilder.GetPageEndFlowPosition(PageIndex);
				_end = new FixedTextPointer(mutable: false, LogicalDirection.Backward, pageEndFlowPosition);
			}
			return _end;
		}
	}

	private FixedTextContainer Container => _docPage.TextContainer;

	private Visual VisualRoot => _docPage.Visual;

	private FixedPage FixedPage => _docPage.FixedPage;

	private int PageIndex => _docPage.PageIndex;

	private bool IsContainerStart => Start.CompareTo(TextContainer.Start) == 0;

	private bool IsContainerEnd => End.CompareTo(TextContainer.End) == 0;

	internal FixedTextView(FixedDocumentPage docPage)
	{
		_docPage = docPage;
	}

	internal override ITextPointer GetTextPositionFromPoint(Point point, bool snapToText)
	{
		if (point.Y == double.MaxValue && point.X == double.MaxValue)
		{
			ITextPointer textPointer = End;
			if (_GetFixedPosition(End, out var fixedp))
			{
				textPointer = _CreateTextPointer(fixedp, LogicalDirection.Backward);
				if (textPointer == null)
				{
					textPointer = End;
				}
			}
			return textPointer;
		}
		ITextPointer textPointer2 = null;
		if (_HitTest(point, out var e))
		{
			if (e is Glyphs g)
			{
				textPointer2 = _CreateTextPointerFromGlyphs(g, point);
			}
			else if (e is Image)
			{
				Image e2 = (Image)e;
				FixedPosition fixedPosition = new FixedPosition(FixedPage.CreateFixedNode(PageIndex, e2), 0);
				textPointer2 = _CreateTextPointer(fixedPosition, LogicalDirection.Forward);
			}
			else if (e is Path)
			{
				Path path = (Path)e;
				if (path.Fill is ImageBrush)
				{
					FixedPosition fixedPosition2 = new FixedPosition(FixedPage.CreateFixedNode(PageIndex, path), 0);
					textPointer2 = _CreateTextPointer(fixedPosition2, LogicalDirection.Forward);
				}
			}
		}
		if (snapToText && textPointer2 == null)
		{
			textPointer2 = _SnapToText(point);
		}
		return textPointer2;
	}

	internal override Rect GetRawRectangleFromTextPosition(ITextPointer position, out Transform transform)
	{
		FixedTextPointer fixedTextPointer = Container.VerifyPosition(position);
		Rect result = new Rect(0.0, 0.0, 0.0, 10.0);
		transform = Transform.Identity;
		FixedPosition fixedp;
		if (fixedTextPointer.FlowPosition.IsBoundary)
		{
			if (!_GetFirstFixedPosition(fixedTextPointer, out fixedp))
			{
				return result;
			}
		}
		else if (!_GetFixedPosition(fixedTextPointer, out fixedp))
		{
			if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.None)
			{
				return result;
			}
			ITextPointer position2 = position.CreatePointer(1);
			FixedTextPointer ftp = Container.VerifyPosition(position2);
			if (!_GetFixedPosition(ftp, out fixedp))
			{
				return result;
			}
		}
		if (fixedp.Page != PageIndex)
		{
			return result;
		}
		DependencyObject element = FixedPage.GetElement(fixedp.Node);
		if (element is Glyphs)
		{
			Glyphs obj = (Glyphs)element;
			result = _GetGlyphRunDesignRect(obj, fixedp.Offset, fixedp.Offset);
			GeneralTransform transform2 = obj.TransformToAncestor(FixedPage);
			return _GetTransformedCaretRect(transform2, result.TopLeft, result.Height);
		}
		if (element is Image)
		{
			Image image = (Image)element;
			GeneralTransform transform3 = image.TransformToAncestor(FixedPage);
			Point origin = new Point(0.0, 0.0);
			if (fixedp.Offset > 0)
			{
				origin.X += image.ActualWidth;
			}
			return _GetTransformedCaretRect(transform3, origin, image.ActualHeight);
		}
		if (element is Path)
		{
			Path obj2 = (Path)element;
			GeneralTransform transform4 = obj2.TransformToAncestor(FixedPage);
			Rect bounds = obj2.Data.Bounds;
			Point topLeft = bounds.TopLeft;
			if (fixedp.Offset > 0)
			{
				topLeft.X += bounds.Width;
			}
			return _GetTransformedCaretRect(transform4, topLeft, bounds.Height);
		}
		return result;
	}

	internal override Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition)
	{
		PathGeometry pathGeometry = new PathGeometry();
		Dictionary<FixedPage, ArrayList> dictionary = new Dictionary<FixedPage, ArrayList>();
		FixedTextPointer start = Container.VerifyPosition(startPosition);
		FixedTextPointer end = Container.VerifyPosition(endPosition);
		Container.GetMultiHighlights(start, end, dictionary, FixedHighlightType.TextSelection, null, null);
		dictionary.TryGetValue(FixedPage, out var value);
		if (value != null)
		{
			foreach (FixedHighlight item in value)
			{
				if (item.HighlightType == FixedHighlightType.None)
				{
					continue;
				}
				Rect rect = item.ComputeDesignRect();
				if (!(rect == Rect.Empty))
				{
					GeneralTransform generalTransform = item.Element.TransformToAncestor(FixedPage);
					Transform transform = generalTransform.AffineTransform;
					if (transform == null)
					{
						transform = Transform.Identity;
					}
					_ = item.Glyphs;
					if (item.Element.Clip != null)
					{
						Rect bounds = item.Element.Clip.Bounds;
						rect.Intersect(bounds);
					}
					Geometry geometry = new RectangleGeometry(rect)
					{
						Transform = transform
					};
					rect = generalTransform.TransformBounds(rect);
					pathGeometry.AddGeometry(geometry);
				}
			}
		}
		return pathGeometry;
	}

	internal override ITextPointer GetPositionAtNextLine(ITextPointer position, double suggestedX, int count, out double newSuggestedX, out int linesMoved)
	{
		newSuggestedX = suggestedX;
		linesMoved = 0;
		LogicalDirection edge = position.LogicalDirection;
		LogicalDirection logicalDirection = LogicalDirection.Forward;
		FixedTextPointer fixedTextPointer = Container.VerifyPosition(position);
		FixedTextPointer fixedTextPointer2 = new FixedTextPointer(mutable: true, edge, (FlowPosition)fixedTextPointer.FlowPosition.Clone());
		_SkipFormattingTags(fixedTextPointer2);
		bool flag = false;
		if (count == 0 || ((flag = _GetFixedPosition(fixedTextPointer2, out var fixedp)) && fixedp.Page != PageIndex))
		{
			return position;
		}
		if (count < 0)
		{
			count = -count;
			logicalDirection = LogicalDirection.Backward;
		}
		if (!flag)
		{
			if (Contains(position))
			{
				fixedTextPointer2 = new FixedTextPointer(mutable: true, logicalDirection, (FlowPosition)fixedTextPointer.FlowPosition.Clone());
				((ITextPointer)fixedTextPointer2).MoveToInsertionPosition(logicalDirection);
				((ITextPointer)fixedTextPointer2).MoveToNextInsertionPosition(logicalDirection);
				if (Contains(fixedTextPointer2))
				{
					linesMoved = ((logicalDirection == LogicalDirection.Forward) ? 1 : (-1));
					return fixedTextPointer2;
				}
			}
			return position;
		}
		if (double.IsNaN(suggestedX))
		{
			suggestedX = 0.0;
		}
		while (count > linesMoved && _GetNextLineGlyphs(ref fixedp, ref edge, suggestedX, logicalDirection))
		{
			linesMoved++;
		}
		if (linesMoved == 0)
		{
			return position.CreatePointer();
		}
		if (logicalDirection == LogicalDirection.Backward)
		{
			linesMoved = -linesMoved;
		}
		ITextPointer textPointer = _CreateTextPointer(fixedp, edge);
		if (textPointer.CompareTo(position) == 0)
		{
			linesMoved = 0;
		}
		return textPointer;
	}

	internal override bool IsAtCaretUnitBoundary(ITextPointer position)
	{
		FixedTextPointer ftp = Container.VerifyPosition(position);
		if (_GetFixedPosition(ftp, out var fixedp))
		{
			DependencyObject element = FixedPage.GetElement(fixedp.Node);
			if (element is Glyphs)
			{
				Glyphs glyphs = (Glyphs)element;
				int num = ((glyphs.UnicodeString != null) ? glyphs.UnicodeString.Length : 0);
				if (fixedp.Offset == num)
				{
					return true;
				}
				GlyphRun measurementGlyphRun = glyphs.MeasurementGlyphRun;
				if (measurementGlyphRun.CaretStops != null)
				{
					return measurementGlyphRun.CaretStops[fixedp.Offset];
				}
				return true;
			}
			if (element is Image || element is Path)
			{
				return true;
			}
		}
		return false;
	}

	internal override ITextPointer GetNextCaretUnitPosition(ITextPointer position, LogicalDirection direction)
	{
		FixedTextPointer ftp = Container.VerifyPosition(position);
		if (_GetFixedPosition(ftp, out var fixedp))
		{
			DependencyObject element = FixedPage.GetElement(fixedp.Node);
			if (element is Glyphs)
			{
				GlyphRun glyphRun = ((Glyphs)element).ToGlyphRun();
				int num = ((glyphRun.Characters != null) ? glyphRun.Characters.Count : 0);
				CharacterHit characterHit = ((fixedp.Offset == num) ? new CharacterHit(fixedp.Offset - 1, 1) : new CharacterHit(fixedp.Offset, 0));
				CharacterHit obj = ((direction == LogicalDirection.Forward) ? glyphRun.GetNextCaretCharacterHit(characterHit) : glyphRun.GetPreviousCaretCharacterHit(characterHit));
				if (!characterHit.Equals(obj))
				{
					LogicalDirection edge = LogicalDirection.Forward;
					if (obj.TrailingLength > 0)
					{
						edge = LogicalDirection.Backward;
					}
					int offset = obj.FirstCharacterIndex + obj.TrailingLength;
					return _CreateTextPointer(new FixedPosition(fixedp.Node, offset), edge);
				}
			}
		}
		ITextPointer textPointer = position.CreatePointer();
		textPointer.MoveToNextInsertionPosition(direction);
		return textPointer;
	}

	internal override ITextPointer GetBackspaceCaretUnitPosition(ITextPointer position)
	{
		throw new NotImplementedException();
	}

	internal override TextSegment GetLineRange(ITextPointer position)
	{
		FixedTextPointer ftp = Container.VerifyPosition(position);
		if (!_GetFixedPosition(ftp, out var fixedp))
		{
			return new TextSegment(position, position, preserveLogicalDirection: true);
		}
		int count = 0;
		FixedNode[] array = Container.FixedTextBuilder.GetNextLine(fixedp.Node, forward: true, ref count);
		if (array == null)
		{
			array = new FixedNode[1] { fixedp.Node };
		}
		FixedNode fixedNode = array[^1];
		DependencyObject element = FixedPage.GetElement(fixedNode);
		int offset = 1;
		if (element is Glyphs)
		{
			offset = ((Glyphs)element).UnicodeString.Length;
		}
		ITextPointer textPointer = _CreateTextPointer(new FixedPosition(array[0], 0), LogicalDirection.Forward);
		ITextPointer textPointer2 = _CreateTextPointer(new FixedPosition(fixedNode, offset), LogicalDirection.Backward);
		if (textPointer.CompareTo(textPointer2) > 0)
		{
			ITextPointer textPointer3 = textPointer;
			textPointer = textPointer2;
			textPointer2 = textPointer3;
		}
		return new TextSegment(textPointer, textPointer2, preserveLogicalDirection: true);
	}

	internal override bool Contains(ITextPointer position)
	{
		FixedTextPointer fixedTextPointer = Container.VerifyPosition(position);
		if ((fixedTextPointer.CompareTo(Start) <= 0 || fixedTextPointer.CompareTo(End) >= 0) && (fixedTextPointer.CompareTo(Start) != 0 || (fixedTextPointer.LogicalDirection != LogicalDirection.Forward && !IsContainerStart)))
		{
			if (fixedTextPointer.CompareTo(End) == 0)
			{
				if (fixedTextPointer.LogicalDirection != 0)
				{
					return IsContainerEnd;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	internal override bool Validate()
	{
		return true;
	}

	private bool _HitTest(Point pt, out UIElement e)
	{
		e = null;
		for (DependencyObject dependencyObject = VisualTreeHelper.HitTest(FixedPage, pt)?.VisualHit; dependencyObject != null; dependencyObject = VisualTreeHelper.GetParent(dependencyObject))
		{
			DependencyObjectType dependencyObjectType = dependencyObject.DependencyObjectType;
			if (dependencyObjectType == UIElementType || dependencyObjectType.IsSubclassOf(UIElementType))
			{
				e = (UIElement)dependencyObject;
				return true;
			}
		}
		return false;
	}

	private void _GlyphRunHitTest(Glyphs g, double xoffset, out int charIndex, out LogicalDirection edge)
	{
		charIndex = 0;
		edge = LogicalDirection.Forward;
		GlyphRun glyphRun = g.ToGlyphRun();
		double distance = (((glyphRun.BidiLevel & 1) == 0) ? (xoffset - glyphRun.BaselineOrigin.X) : (glyphRun.BaselineOrigin.X - xoffset));
		bool isInside;
		CharacterHit caretCharacterHitFromDistance = glyphRun.GetCaretCharacterHitFromDistance(distance, out isInside);
		charIndex = caretCharacterHitFromDistance.FirstCharacterIndex + caretCharacterHitFromDistance.TrailingLength;
		edge = ((caretCharacterHitFromDistance.TrailingLength <= 0) ? LogicalDirection.Forward : LogicalDirection.Backward);
	}

	private ITextPointer _SnapToText(Point point)
	{
		ITextPointer textPointer = null;
		FixedNode[] line = Container.FixedTextBuilder.GetLine(PageIndex, point);
		if (line != null && line.Length != 0)
		{
			double num = double.MaxValue;
			double xoffset = 0.0;
			Glyphs glyphs = null;
			FixedNode fixedNode = line[0];
			FixedNode[] array = line;
			foreach (FixedNode fixedNode2 in array)
			{
				Glyphs glyphsElement = FixedPage.GetGlyphsElement(fixedNode2);
				GeneralTransform generalTransform = FixedPage.TransformToDescendant(glyphsElement);
				Point result = point;
				generalTransform?.TryTransform(result, out result);
				Rect rect = glyphsElement.ToGlyphRun().ComputeAlignmentBox();
				rect.Offset(glyphsElement.OriginX, glyphsElement.OriginY);
				double num2 = Math.Max(0.0, (result.X > rect.X) ? (result.X - rect.Right) : (rect.X - result.X));
				double num3 = Math.Max(0.0, (result.Y > rect.Y) ? (result.Y - rect.Bottom) : (rect.Y - result.Y));
				double num4 = num2 + num3;
				if (glyphs == null || num4 < num)
				{
					num = num4;
					glyphs = glyphsElement;
					fixedNode = fixedNode2;
					xoffset = result.X;
				}
			}
			_GlyphRunHitTest(glyphs, xoffset, out var charIndex, out var edge);
			FixedPosition fixedPosition = new FixedPosition(fixedNode, charIndex);
			textPointer = _CreateTextPointer(fixedPosition, edge);
		}
		else if (point.Y < FixedPage.Height / 2.0)
		{
			textPointer = ((ITextPointer)Start).CreatePointer(LogicalDirection.Forward);
			textPointer.MoveToInsertionPosition(LogicalDirection.Forward);
		}
		else
		{
			textPointer = ((ITextPointer)End).CreatePointer(LogicalDirection.Backward);
			textPointer.MoveToInsertionPosition(LogicalDirection.Backward);
		}
		return textPointer;
	}

	private bool _GetNextLineGlyphs(ref FixedPosition fixedp, ref LogicalDirection edge, double suggestedX, LogicalDirection scanDir)
	{
		int count = 1;
		int page = fixedp.Page;
		bool result = false;
		FixedNode[] nextLine = Container.FixedTextBuilder.GetNextLine(fixedp.Node, scanDir == LogicalDirection.Forward, ref count);
		if (nextLine != null && nextLine.Length != 0)
		{
			FixedPage fixedPage = Container.FixedDocument.SyncGetPage(page, forceReload: false);
			if (double.IsInfinity(suggestedX))
			{
				suggestedX = 0.0;
			}
			Point point = new Point(suggestedX, 0.0);
			Point point2 = new Point(suggestedX, 1000.0);
			FixedNode fixedNode = nextLine[0];
			Glyphs g = null;
			double num = double.MaxValue;
			double xoffset = 0.0;
			for (int num2 = nextLine.Length - 1; num2 >= 0; num2--)
			{
				FixedNode fixedNode2 = nextLine[num2];
				Glyphs glyphsElement = fixedPage.GetGlyphsElement(fixedNode2);
				if (glyphsElement != null)
				{
					GeneralTransform generalTransform = fixedPage.TransformToDescendant(glyphsElement);
					Point result2 = point;
					Point result3 = point2;
					if (generalTransform != null)
					{
						generalTransform.TryTransform(result2, out result2);
						generalTransform.TryTransform(result3, out result3);
					}
					double num3 = (result3.X - result2.X) / (result3.Y - result2.Y);
					Rect rect = glyphsElement.ToGlyphRun().ComputeAlignmentBox();
					rect.Offset(glyphsElement.OriginX, glyphsElement.OriginY);
					double num4;
					double num5;
					if (num3 > 1000.0 || num3 < -1000.0)
					{
						num4 = 0.0;
						num5 = ((result2.Y > rect.Y) ? (result2.Y - rect.Bottom) : (rect.Y - result2.Y));
					}
					else
					{
						double num6 = (rect.Top + rect.Bottom) / 2.0;
						num4 = result2.X + num3 * (num6 - result2.Y);
						num5 = ((num4 > rect.X) ? (num4 - rect.Right) : (rect.X - num4));
					}
					if (num5 < num)
					{
						num = num5;
						xoffset = num4;
						fixedNode = fixedNode2;
						g = glyphsElement;
						if (num5 <= 0.0)
						{
							break;
						}
					}
				}
			}
			_GlyphRunHitTest(g, xoffset, out var charIndex, out edge);
			fixedp = new FixedPosition(fixedNode, charIndex);
			result = true;
		}
		return result;
	}

	private static double _GetDistanceToCharacter(GlyphRun run, int charOffset)
	{
		int num = charOffset;
		int trailingLength = 0;
		int num2 = ((run.Characters != null) ? run.Characters.Count : 0);
		if (num == num2)
		{
			num--;
			trailingLength = 1;
		}
		return run.GetDistanceFromCaretCharacterHit(new CharacterHit(num, trailingLength));
	}

	internal static Rect _GetGlyphRunDesignRect(Glyphs g, int charStart, int charEnd)
	{
		GlyphRun glyphRun = g.ToGlyphRun();
		if (glyphRun == null)
		{
			return Rect.Empty;
		}
		Rect result = glyphRun.ComputeAlignmentBox();
		result.Offset(glyphRun.BaselineOrigin.X, glyphRun.BaselineOrigin.Y);
		int num = 0;
		if (glyphRun.Characters != null)
		{
			num = glyphRun.Characters.Count;
		}
		else if (g.UnicodeString != null)
		{
			num = g.UnicodeString.Length;
		}
		if (charStart > num)
		{
			charStart = num;
		}
		else if (charStart < 0)
		{
			charStart = 0;
		}
		if (charEnd > num)
		{
			charEnd = num;
		}
		else if (charEnd < 0)
		{
			charEnd = 0;
		}
		double num2 = _GetDistanceToCharacter(glyphRun, charStart);
		double num3 = _GetDistanceToCharacter(glyphRun, charEnd);
		double width = num3 - num2;
		if ((glyphRun.BidiLevel & 1) != 0)
		{
			result.X = glyphRun.BaselineOrigin.X - num3;
		}
		else
		{
			result.X = glyphRun.BaselineOrigin.X + num2;
		}
		result.Width = width;
		return result;
	}

	private Rect _GetTransformedCaretRect(GeneralTransform transform, Point origin, double height)
	{
		Point result = origin;
		result.Y += height;
		transform.TryTransform(origin, out origin);
		transform.TryTransform(result, out result);
		Rect result2 = new Rect(origin, result);
		if (result2.Width > 0.0)
		{
			result2.X += result2.Width / 2.0;
			result2.Width = 0.0;
		}
		if (result2.Height < 1.0)
		{
			result2.Height = 1.0;
		}
		return result2;
	}

	private bool _GetFixedPosition(FixedTextPointer ftp, out FixedPosition fixedp)
	{
		LogicalDirection logicalDirection = ftp.LogicalDirection;
		TextPointerContext pointerContext = ((ITextPointer)ftp).GetPointerContext(logicalDirection);
		if (ftp.FlowPosition.IsBoundary || pointerContext == TextPointerContext.None)
		{
			return _GetFirstFixedPosition(ftp, out fixedp);
		}
		if (pointerContext == TextPointerContext.ElementStart || pointerContext == TextPointerContext.ElementEnd)
		{
			switch (pointerContext)
			{
			case TextPointerContext.ElementStart:
				logicalDirection = LogicalDirection.Forward;
				break;
			case TextPointerContext.ElementEnd:
				logicalDirection = LogicalDirection.Backward;
				break;
			}
			FixedTextPointer fixedTextPointer = new FixedTextPointer(mutable: true, logicalDirection, (FlowPosition)ftp.FlowPosition.Clone());
			_SkipFormattingTags(fixedTextPointer);
			pointerContext = ((ITextPointer)fixedTextPointer).GetPointerContext(logicalDirection);
			if (pointerContext != TextPointerContext.Text && pointerContext != TextPointerContext.EmbeddedElement)
			{
				if (((ITextPointer)fixedTextPointer).MoveToNextInsertionPosition(logicalDirection) && Container.GetPageNumber(fixedTextPointer) == PageIndex)
				{
					return Container.FixedTextBuilder.GetFixedPosition(fixedTextPointer.FlowPosition, logicalDirection, out fixedp);
				}
				fixedp = new FixedPosition(Container.FixedTextBuilder.FixedFlowMap.FixedStartEdge, 0);
				return false;
			}
			ftp = fixedTextPointer;
		}
		return Container.FixedTextBuilder.GetFixedPosition(ftp.FlowPosition, logicalDirection, out fixedp);
	}

	private bool _GetFirstFixedPosition(FixedTextPointer ftp, out FixedPosition fixedP)
	{
		LogicalDirection logicalDirection = LogicalDirection.Forward;
		if (ftp.FlowPosition.FlowNode.Fp != 0)
		{
			logicalDirection = LogicalDirection.Backward;
		}
		FlowPosition flowPosition = (FlowPosition)ftp.FlowPosition.Clone();
		flowPosition.Move(logicalDirection);
		FixedTextPointer fixedTextPointer = new FixedTextPointer(mutable: true, logicalDirection, flowPosition);
		if (flowPosition.IsStart || flowPosition.IsEnd)
		{
			((ITextPointer)fixedTextPointer).MoveToNextInsertionPosition(logicalDirection);
		}
		if (Container.GetPageNumber(fixedTextPointer) == PageIndex)
		{
			return Container.FixedTextBuilder.GetFixedPosition(fixedTextPointer.FlowPosition, logicalDirection, out fixedP);
		}
		fixedP = new FixedPosition(Container.FixedTextBuilder.FixedFlowMap.FixedStartEdge, 0);
		return false;
	}

	private ITextPointer _CreateTextPointer(FixedPosition fixedPosition, LogicalDirection edge)
	{
		FlowPosition flowPosition = Container.FixedTextBuilder.CreateFlowPosition(fixedPosition);
		if (flowPosition != null)
		{
			return new FixedTextPointer(mutable: true, edge, flowPosition);
		}
		return null;
	}

	private ITextPointer _CreateTextPointerFromGlyphs(Glyphs g, Point point)
	{
		VisualRoot.TransformToDescendant(g)?.TryTransform(point, out point);
		_GlyphRunHitTest(g, point.X, out var charIndex, out var edge);
		FixedPosition fixedPosition = new FixedPosition(FixedPage.CreateFixedNode(PageIndex, g), charIndex);
		return _CreateTextPointer(fixedPosition, edge);
	}

	private void _SkipFormattingTags(ITextPointer textPointer)
	{
		LogicalDirection logicalDirection = textPointer.LogicalDirection;
		int offset = ((logicalDirection == LogicalDirection.Forward) ? 1 : (-1));
		while (TextSchema.IsFormattingType(textPointer.GetElementType(logicalDirection)))
		{
			textPointer.MoveByOffset(offset);
		}
	}
}
