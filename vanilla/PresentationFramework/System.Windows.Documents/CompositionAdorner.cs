using System.Collections;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Documents;

internal class CompositionAdorner : Adorner
{
	private class AttributeRange
	{
		private ITextView _textView;

		private Rect _startRect;

		private Rect _endRect;

		private readonly int _startOffset;

		private readonly int _endOffset;

		private readonly TextServicesDisplayAttribute _textServicesDisplayAttribute;

		private readonly ArrayList _compositionLines;

		internal double Height => _startRect.Bottom - _startRect.Top;

		internal ArrayList CompositionLines => _compositionLines;

		internal TextServicesDisplayAttribute TextServicesDisplayAttribute => _textServicesDisplayAttribute;

		internal AttributeRange(ITextView textView, ITextPointer start, ITextPointer end, TextServicesDisplayAttribute textServicesDisplayAttribute)
		{
			_textView = textView;
			_startOffset = start.Offset;
			_endOffset = end.Offset;
			_textServicesDisplayAttribute = textServicesDisplayAttribute;
			_compositionLines = new ArrayList(1);
		}

		internal void AddCompositionLines()
		{
			_compositionLines.Clear();
			ITextPointer textPointer = _textView.TextContainer.Start.CreatePointer(_startOffset, LogicalDirection.Forward);
			ITextPointer textPointer2 = _textView.TextContainer.Start.CreatePointer(_endOffset, LogicalDirection.Backward);
			while (textPointer.CompareTo(textPointer2) < 0 && textPointer.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text)
			{
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
			}
			Invariant.Assert(textPointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text);
			if (textPointer2.HasValidLayout)
			{
				_startRect = _textView.GetRectangleFromTextPosition(textPointer);
				_endRect = _textView.GetRectangleFromTextPosition(textPointer2);
				if (_startRect.Top != _endRect.Top)
				{
					AddMultipleCompositionLines(textPointer, textPointer2);
					return;
				}
				Color lineColor = _textServicesDisplayAttribute.GetLineColor(textPointer);
				_compositionLines.Add(new CompositionLine(_startRect, _endRect, lineColor));
			}
		}

		private void AddMultipleCompositionLines(ITextPointer start, ITextPointer end)
		{
			ITextPointer textPointer = start;
			ITextPointer textPointer2 = textPointer;
			while (textPointer2.CompareTo(end) < 0)
			{
				TextSegment lineRange = _textView.GetLineRange(textPointer2);
				if (lineRange.IsNull)
				{
					textPointer = textPointer2;
				}
				else
				{
					if (textPointer.CompareTo(lineRange.Start) < 0)
					{
						textPointer = lineRange.Start;
					}
					if (textPointer2.CompareTo(lineRange.End) < 0)
					{
						textPointer2 = ((end.CompareTo(lineRange.End) >= 0) ? lineRange.End.CreatePointer(LogicalDirection.Backward) : end.CreatePointer());
					}
					Rect rectangleFromTextPosition = _textView.GetRectangleFromTextPosition(textPointer);
					Rect rectangleFromTextPosition2 = _textView.GetRectangleFromTextPosition(textPointer2);
					_compositionLines.Add(new CompositionLine(rectangleFromTextPosition, rectangleFromTextPosition2, _textServicesDisplayAttribute.GetLineColor(textPointer)));
					textPointer = lineRange.End.CreatePointer(LogicalDirection.Forward);
				}
				while (textPointer.GetPointerContext(LogicalDirection.Forward) != 0 && textPointer.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text)
				{
					textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				}
				textPointer2 = textPointer;
			}
		}
	}

	private class CompositionLine
	{
		private Rect _startRect;

		private Rect _endRect;

		private Color _color;

		internal Point StartPoint => _startRect.BottomLeft;

		internal Point EndPoint => _endRect.BottomRight;

		internal Rect StartRect => _startRect;

		internal Rect EndRect => _endRect;

		internal Color LineColor => _color;

		internal CompositionLine(Rect startRect, Rect endRect, Color lineColor)
		{
			_startRect = startRect;
			_endRect = endRect;
			_color = lineColor;
		}
	}

	private AdornerLayer _adornerLayer;

	private ITextView _textView;

	private readonly ArrayList _attributeRanges;

	private const double DotLength = 1.2;

	private const double NormalLineHeightRatio = 0.06;

	private const double BoldLineHeightRatio = 0.08;

	private const double NormalDotLineHeightRatio = 0.08;

	private const double BoldDotLineHeightRatio = 0.1;

	private const double NormalDashRatio = 0.27;

	private const double BoldDashRatio = 0.39;

	private const double ClauseGapRatio = 0.09;

	private const double NormalDashGapRatio = 0.04;

	private const double BoldDashGapRatio = 0.06;

	private const string chinesePinyin = "zh-CN";

	static CompositionAdorner()
	{
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(CompositionAdorner), new FrameworkPropertyMetadata(false));
	}

	internal CompositionAdorner(ITextView textView)
		: this(textView, new ArrayList())
	{
	}

	internal CompositionAdorner(ITextView textView, ArrayList attributeRanges)
		: base(textView.RenderScope)
	{
		_textView = textView;
		_attributeRanges = attributeRanges;
	}

	public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
	{
		GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
		Transform transform2 = transform.AffineTransform;
		if (transform2 == null)
		{
			transform2 = Transform.Identity;
		}
		TranslateTransform value = new TranslateTransform(0.0 - transform2.Value.OffsetX, 0.0 - transform2.Value.OffsetY);
		generalTransformGroup.Children.Add(value);
		if (transform != null)
		{
			generalTransformGroup.Children.Add(transform);
		}
		return generalTransformGroup;
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		if (!(VisualTreeHelper.GetParent(base.AdornedElement) is Visual ancestor))
		{
			return;
		}
		GeneralTransform generalTransform = base.AdornedElement.TransformToAncestor(ancestor);
		if (generalTransform == null)
		{
			return;
		}
		bool flag = "zh-CN".Equals(InputLanguageManager.Current.CurrentInputLanguage.IetfLanguageTag);
		for (int i = 0; i < _attributeRanges.Count; i++)
		{
			AttributeRange attributeRange = (AttributeRange)_attributeRanges[i];
			if (attributeRange.CompositionLines.Count == 0)
			{
				continue;
			}
			bool flag2 = attributeRange.TextServicesDisplayAttribute.IsBoldLine;
			bool flag3 = false;
			bool flag4 = (attributeRange.TextServicesDisplayAttribute.AttrInfo & MS.Win32.UnsafeNativeMethods.TF_DA_ATTR_INFO.TF_ATTR_TARGET_CONVERTED) != 0;
			Brush brush = null;
			double opacity = -1.0;
			Pen pen = null;
			if (flag && flag4)
			{
				DependencyObject parent = _textView.TextContainer.Parent;
				brush = (Brush)parent.GetValue(TextBoxBase.SelectionBrushProperty);
				opacity = (double)parent.GetValue(TextBoxBase.SelectionOpacityProperty);
			}
			double height = attributeRange.Height;
			double num = height * (flag2 ? 0.08 : 0.06);
			double num2 = height * 0.09;
			Pen pen2 = new Pen(new SolidColorBrush(Colors.Black), num);
			switch (attributeRange.TextServicesDisplayAttribute.LineStyle)
			{
			case MS.Win32.UnsafeNativeMethods.TF_DA_LINESTYLE.TF_LS_DOT:
			{
				DoubleCollection doubleCollection = new DoubleCollection();
				doubleCollection.Add(1.2);
				doubleCollection.Add(1.2);
				pen2.DashStyle = new DashStyle(doubleCollection, 0.0);
				pen2.DashCap = PenLineCap.Round;
				pen2.StartLineCap = PenLineCap.Round;
				pen2.EndLineCap = PenLineCap.Round;
				num = height * (flag2 ? 0.1 : 0.08);
				break;
			}
			case MS.Win32.UnsafeNativeMethods.TF_DA_LINESTYLE.TF_LS_DASH:
			{
				double value = height * (flag2 ? 0.39 : 0.27);
				double value2 = height * (flag2 ? 0.06 : 0.04);
				DoubleCollection doubleCollection = new DoubleCollection();
				doubleCollection.Add(value);
				doubleCollection.Add(value2);
				pen2.DashStyle = new DashStyle(doubleCollection, 0.0);
				pen2.DashCap = PenLineCap.Round;
				pen2.StartLineCap = PenLineCap.Round;
				pen2.EndLineCap = PenLineCap.Round;
				break;
			}
			case MS.Win32.UnsafeNativeMethods.TF_DA_LINESTYLE.TF_LS_SOLID:
				pen2.StartLineCap = PenLineCap.Round;
				pen2.EndLineCap = PenLineCap.Round;
				break;
			case MS.Win32.UnsafeNativeMethods.TF_DA_LINESTYLE.TF_LS_SQUIGGLE:
				flag3 = true;
				break;
			}
			double num3 = num / 2.0;
			for (int j = 0; j < attributeRange.CompositionLines.Count; j++)
			{
				CompositionLine compositionLine = (CompositionLine)attributeRange.CompositionLines[j];
				Point result = new Point(compositionLine.StartPoint.X + num2, compositionLine.StartPoint.Y - num3);
				Point result2 = new Point(compositionLine.EndPoint.X - num2, compositionLine.EndPoint.Y - num3);
				pen2.Brush = new SolidColorBrush(compositionLine.LineColor);
				generalTransform.TryTransform(result, out result);
				generalTransform.TryTransform(result2, out result2);
				if (flag && flag4)
				{
					Rect rect = Rect.Union(compositionLine.StartRect, compositionLine.EndRect);
					rect = generalTransform.TransformBounds(rect);
					drawingContext.PushOpacity(opacity);
					drawingContext.DrawRectangle(brush, pen, rect);
					drawingContext.Pop();
				}
				if (flag3)
				{
					Point point = new Point(result.X, result.Y - num3);
					double num4 = num3;
					PathFigure pathFigure = new PathFigure();
					pathFigure.StartPoint = point;
					for (int k = 0; (double)k < (result2.X - result.X) / num4; k++)
					{
						if (k % 4 == 0 || k % 4 == 3)
						{
							point = new Point(point.X + num4, point.Y + num3);
							pathFigure.Segments.Add(new LineSegment(point, isStroked: true));
						}
						else if (k % 4 == 1 || k % 4 == 2)
						{
							point = new Point(point.X + num4, point.Y - num3);
							pathFigure.Segments.Add(new LineSegment(point, isStroked: true));
						}
					}
					PathGeometry pathGeometry = new PathGeometry();
					pathGeometry.Figures.Add(pathFigure);
					drawingContext.DrawGeometry(null, pen2, pathGeometry);
				}
				else
				{
					drawingContext.DrawLine(pen2, result, result2);
				}
			}
		}
	}

	internal void AddAttributeRange(ITextPointer start, ITextPointer end, TextServicesDisplayAttribute textServiceDisplayAttribute)
	{
		ITextPointer start2 = start.CreatePointer(LogicalDirection.Forward);
		ITextPointer end2 = end.CreatePointer(LogicalDirection.Backward);
		_attributeRanges.Add(new AttributeRange(_textView, start2, end2, textServiceDisplayAttribute));
	}

	internal void InvalidateAdorner()
	{
		for (int i = 0; i < _attributeRanges.Count; i++)
		{
			((AttributeRange)_attributeRanges[i]).AddCompositionLines();
		}
		if (VisualTreeHelper.GetParent(this) is AdornerLayer adornerLayer)
		{
			adornerLayer.Update(base.AdornedElement);
			adornerLayer.InvalidateArrange();
		}
	}

	internal void Initialize(ITextView textView)
	{
		_adornerLayer = AdornerLayer.GetAdornerLayer(textView.RenderScope);
		if (_adornerLayer != null)
		{
			_adornerLayer.Add(this);
		}
	}

	internal void Uninitialize()
	{
		if (_adornerLayer != null)
		{
			_adornerLayer.Remove(this);
			_adornerLayer = null;
		}
	}
}
