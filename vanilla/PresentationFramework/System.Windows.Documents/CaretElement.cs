using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MS.Internal;
using MS.Internal.Documents;
using MS.Win32;

namespace System.Windows.Documents;

internal sealed class CaretElement : Adorner
{
	private class CaretSubElement : UIElement
	{
		internal CaretSubElement()
		{
		}

		protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
		{
			return null;
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			((CaretElement)_parent).OnRenderCaretSubElement(drawingContext);
		}
	}

	internal const double BidiCaretIndicatorWidth = 2.0;

	internal const double CaretPaddingWidth = 5.0;

	private readonly TextEditor _textEditor;

	private bool _showCaret;

	private bool _isSelectionActive;

	private AnimationClock _blinkAnimationClock;

	private double _left;

	private double _top;

	private double _systemCaretWidth;

	private double _interimWidth;

	private double _height;

	private double _win32Height;

	private bool _isBlinkEnabled;

	private Brush _caretBrush;

	private double _opacity;

	private AdornerLayer _adornerLayer;

	private bool _italic;

	private bool _win32Caret;

	private const double CaretOpacity = 0.5;

	private const double BidiIndicatorHeightRatio = 10.0;

	private const double DefaultNarrowCaretWidth = 1.0;

	private Geometry _selectionGeometry;

	internal const double c_geometryCombineTolerance = 0.0001;

	internal const double c_endOfParaMagicMultiplier = 0.5;

	internal const int ZOrderValue = 1073741823;

	private readonly CaretSubElement _caretElement;

	private bool _pendingGeometryUpdate;

	private bool _scrolledToCurrentPositionYet;

	protected override int VisualChildrenCount => (_caretElement != null) ? 1 : 0;

	private static CaretElement Debug_CaretElement
	{
		get
		{
			_ = TextEditor._ThreadLocalStore;
			return ((ITextSelection)TextEditor._ThreadLocalStore.FocusedTextSelection).CaretElement;
		}
	}

	private static FrameworkElement Debug_RenderScope => ((ITextSelection)TextEditor._ThreadLocalStore.FocusedTextSelection).TextView.RenderScope as FrameworkElement;

	internal Geometry SelectionGeometry => _selectionGeometry;

	internal bool IsSelectionActive
	{
		get
		{
			return _isSelectionActive;
		}
		set
		{
			_isSelectionActive = value;
		}
	}

	private bool IsInInterimState => _interimWidth != 0.0;

	internal CaretElement(TextEditor textEditor, bool isBlinkEnabled)
		: base(textEditor.TextView.RenderScope)
	{
		Invariant.Assert(textEditor.TextView != null && textEditor.TextView.RenderScope != null, "Assert: textView != null && RenderScope != null");
		_textEditor = textEditor;
		_isBlinkEnabled = isBlinkEnabled;
		_left = 0.0;
		_top = 0.0;
		_systemCaretWidth = SystemParameters.CaretWidth;
		_height = 0.0;
		base.AllowDrop = false;
		_caretElement = new CaretSubElement();
		_caretElement.ClipToBounds = false;
		AddVisualChild(_caretElement);
	}

	protected override Visual GetVisualChild(int index)
	{
		if (index != 0)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return _caretElement;
	}

	protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
	{
		return null;
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		if (_selectionGeometry != null)
		{
			FrameworkElement ownerElement = GetOwnerElement();
			Brush brush = (Brush)ownerElement.GetValue(TextBoxBase.SelectionBrushProperty);
			if (brush != null)
			{
				double opacity = (double)ownerElement.GetValue(TextBoxBase.SelectionOpacityProperty);
				drawingContext.PushOpacity(opacity);
				Pen pen = null;
				drawingContext.DrawGeometry(brush, pen, _selectionGeometry);
				drawingContext.Pop();
			}
		}
	}

	protected override Size MeasureOverride(Size availableSize)
	{
		base.MeasureOverride(availableSize);
		_caretElement.InvalidateVisual();
		return new Size(double.IsInfinity(availableSize.Width) ? 8.988465674311579E+307 : availableSize.Width, double.IsInfinity(availableSize.Height) ? 8.988465674311579E+307 : availableSize.Height);
	}

	protected override Size ArrangeOverride(Size availableSize)
	{
		if (_pendingGeometryUpdate)
		{
			((TextSelection)_textEditor.Selection).UpdateCaretState(CaretScrollMethod.None);
			_pendingGeometryUpdate = false;
		}
		Point location = new Point(_left, _top);
		_caretElement.Arrange(new Rect(location, availableSize));
		return availableSize;
	}

	internal void Update(bool visible, Rect caretRectangle, Brush caretBrush, double opacity, bool italic, CaretScrollMethod scrollMethod, double scrollToOriginPosition)
	{
		Invariant.Assert(caretBrush != null, "Assert: caretBrush != null");
		EnsureAttachedToView();
		bool num = visible && !_showCaret;
		if (_showCaret != visible)
		{
			InvalidateVisual();
			_showCaret = visible;
		}
		_caretBrush = caretBrush;
		_opacity = opacity;
		double num2;
		double num3;
		double num4;
		double num5;
		if (caretRectangle.IsEmpty || caretRectangle.Height <= 0.0)
		{
			num2 = 0.0;
			num3 = 0.0;
			num4 = 0.0;
			num5 = 0.0;
		}
		else
		{
			num2 = caretRectangle.X;
			num3 = caretRectangle.Y;
			num4 = caretRectangle.Height;
			num5 = SystemParameters.CaretWidth;
		}
		bool flag = num || italic != _italic;
		if (!DoubleUtil.AreClose(_left, num2))
		{
			_left = num2;
			flag = true;
		}
		if (!DoubleUtil.AreClose(_top, num3))
		{
			_top = num3;
			flag = true;
		}
		if (!caretRectangle.IsEmpty && _interimWidth != caretRectangle.Width)
		{
			_interimWidth = caretRectangle.Width;
			flag = true;
		}
		if (!DoubleUtil.AreClose(_systemCaretWidth, num5))
		{
			_systemCaretWidth = num5;
			flag = true;
		}
		if (!DoubleUtil.AreClose(_height, num4))
		{
			_height = num4;
			InvalidateMeasure();
		}
		if (flag || !double.IsNaN(scrollToOriginPosition))
		{
			_scrolledToCurrentPositionYet = false;
			RefreshCaret(italic);
		}
		if (scrollMethod != CaretScrollMethod.None && !_scrolledToCurrentPositionYet)
		{
			Rect rect = new Rect(_left - 5.0, _top, 10.0 + (IsInInterimState ? _interimWidth : _systemCaretWidth), _height);
			if (!double.IsNaN(scrollToOriginPosition) && scrollToOriginPosition > 0.0)
			{
				rect.X += rect.Width;
				rect.Width = 0.0;
			}
			switch (scrollMethod)
			{
			case CaretScrollMethod.Simple:
				DoSimpleScrollToView(scrollToOriginPosition, rect);
				break;
			case CaretScrollMethod.Navigation:
				DoNavigationalScrollToView(scrollToOriginPosition, rect);
				break;
			}
			_scrolledToCurrentPositionYet = true;
		}
		SetBlinkAnimation(visible, flag);
	}

	private void DoSimpleScrollToView(double scrollToOriginPosition, Rect scrollRectangle)
	{
		if (!double.IsNaN(scrollToOriginPosition))
		{
			TextViewBase.BringRectIntoViewMinimally(_textEditor.TextView, new Rect(scrollToOriginPosition, scrollRectangle.Y, scrollRectangle.Width, scrollRectangle.Height));
			scrollRectangle.X -= scrollToOriginPosition;
		}
		TextViewBase.BringRectIntoViewMinimally(_textEditor.TextView, scrollRectangle);
	}

	private void DoNavigationalScrollToView(double scrollToOriginPosition, Rect targetRect)
	{
		if (_textEditor._Scroller is ScrollViewer scrollViewer)
		{
			Point result = new Point(targetRect.Left, targetRect.Top);
			if (!_textEditor.TextView.RenderScope.TransformToAncestor(scrollViewer).TryTransform(result, out result))
			{
				return;
			}
			double viewportWidth = scrollViewer.ViewportWidth;
			double viewportHeight = scrollViewer.ViewportHeight;
			if (result.Y < 0.0 || result.Y + targetRect.Height > viewportHeight)
			{
				if (result.Y < 0.0)
				{
					double num = Math.Abs(result.Y);
					scrollViewer.ScrollToVerticalOffset(Math.Max(0.0, scrollViewer.VerticalOffset - num - viewportHeight / 4.0));
				}
				else
				{
					double num = result.Y + targetRect.Height - viewportHeight;
					scrollViewer.ScrollToVerticalOffset(Math.Min(scrollViewer.ExtentHeight, scrollViewer.VerticalOffset + num + viewportHeight / 4.0));
				}
			}
			if (result.X < 0.0 || result.X > viewportWidth)
			{
				if (result.X < 0.0)
				{
					double num = Math.Abs(result.X);
					scrollViewer.ScrollToHorizontalOffset(Math.Max(0.0, scrollViewer.HorizontalOffset - num - viewportWidth / 4.0));
				}
				else
				{
					double num = result.X - viewportWidth;
					scrollViewer.ScrollToHorizontalOffset(Math.Min(scrollViewer.ExtentWidth, scrollViewer.HorizontalOffset + num + viewportWidth / 4.0));
				}
			}
		}
		else if (!_textEditor.Selection.MovingPosition.HasValidLayout && _textEditor.TextView != null && _textEditor.TextView.IsValid)
		{
			DoSimpleScrollToView(scrollToOriginPosition, targetRect);
		}
	}

	internal void UpdateSelection()
	{
		Geometry selectionGeometry = _selectionGeometry;
		_selectionGeometry = null;
		if (!_textEditor.Selection.IsEmpty)
		{
			EnsureAttachedToView();
			List<TextSegment> textSegments = _textEditor.Selection.TextSegments;
			for (int i = 0; i < textSegments.Count; i++)
			{
				TextSegment textSegment = textSegments[i];
				Geometry tightBoundingGeometryFromTextPositions = _textEditor.Selection.TextView.GetTightBoundingGeometryFromTextPositions(textSegment.Start, textSegment.End);
				AddGeometry(ref _selectionGeometry, tightBoundingGeometryFromTextPositions);
			}
		}
		if (_selectionGeometry != selectionGeometry)
		{
			RefreshCaret(_italic);
		}
	}

	internal static void AddGeometry(ref Geometry geometry, Geometry addedGeometry)
	{
		if (addedGeometry != null)
		{
			if (geometry == null)
			{
				geometry = addedGeometry;
			}
			else
			{
				geometry = Geometry.Combine(geometry, addedGeometry, GeometryCombineMode.Union, null, 0.0001, ToleranceType.Absolute);
			}
		}
	}

	internal static void ClipGeometryByViewport(ref Geometry geometry, Rect viewport)
	{
		if (geometry != null)
		{
			Geometry geometry2 = new RectangleGeometry(viewport);
			geometry = Geometry.Combine(geometry, geometry2, GeometryCombineMode.Intersect, null, 0.0001, ToleranceType.Absolute);
		}
	}

	internal static void AddTransformToGeometry(Geometry targetGeometry, Transform transformToAdd)
	{
		if (targetGeometry != null && transformToAdd != null)
		{
			targetGeometry.Transform = ((targetGeometry.Transform == null || targetGeometry.Transform.IsIdentity) ? transformToAdd : new MatrixTransform(targetGeometry.Transform.Value * transformToAdd.Value));
		}
	}

	internal void Hide()
	{
		if (_showCaret)
		{
			_showCaret = false;
			InvalidateVisual();
			SetBlinking(isBlinkEnabled: false);
			Win32DestroyCaret();
		}
	}

	internal void RefreshCaret(bool italic)
	{
		_italic = italic;
		AdornerLayer adornerLayer = _adornerLayer;
		if (adornerLayer == null)
		{
			return;
		}
		Adorner[] adorners = adornerLayer.GetAdorners(base.AdornedElement);
		if (adorners == null)
		{
			return;
		}
		for (int i = 0; i < adorners.Length; i++)
		{
			if (adorners[i] == this)
			{
				adornerLayer.Update(base.AdornedElement);
				adornerLayer.InvalidateVisual();
				break;
			}
		}
	}

	internal void DetachFromView()
	{
		SetBlinking(isBlinkEnabled: false);
		if (_adornerLayer != null)
		{
			_adornerLayer.Remove(this);
			_adornerLayer = null;
		}
	}

	internal void SetBlinking(bool isBlinkEnabled)
	{
		if (isBlinkEnabled != _isBlinkEnabled)
		{
			if (_isBlinkEnabled && _blinkAnimationClock != null && _blinkAnimationClock.CurrentState == ClockState.Active)
			{
				_blinkAnimationClock.Controller.Stop();
			}
			_isBlinkEnabled = isBlinkEnabled;
			if (isBlinkEnabled)
			{
				Win32CreateCaret();
			}
			else
			{
				Win32DestroyCaret();
			}
		}
	}

	internal void UpdateCaretBrush(Brush caretBrush)
	{
		_caretBrush = caretBrush;
		_caretElement.InvalidateVisual();
	}

	internal void OnRenderCaretSubElement(DrawingContext context)
	{
		Win32SetCaretPos();
		if (_showCaret)
		{
			TextEditorThreadLocalStore threadLocalStore = TextEditor._ThreadLocalStore;
			Invariant.Assert(!_italic || !IsInInterimState, "Assert !(_italic && IsInInterimState)");
			int num = 0;
			context.PushOpacity(_opacity);
			num++;
			if (_italic && !threadLocalStore.Bidi)
			{
				FlowDirection flowDirection = (FlowDirection)base.AdornedElement.GetValue(FrameworkElement.FlowDirectionProperty);
				context.PushTransform(new RotateTransform((flowDirection == FlowDirection.RightToLeft) ? (-20) : 20, 0.0, _height));
				num++;
			}
			if (IsInInterimState || _systemCaretWidth > 1.0)
			{
				context.PushOpacity(0.5);
				num++;
			}
			if (IsInInterimState)
			{
				context.DrawRectangle(_caretBrush, null, new Rect(0.0, 0.0, _interimWidth, _height));
			}
			else
			{
				if (!_italic || threadLocalStore.Bidi)
				{
					GuidelineSet guidelines = new GuidelineSet(new double[2]
					{
						0.0 - _systemCaretWidth / 2.0,
						_systemCaretWidth / 2.0
					}, null);
					context.PushGuidelineSet(guidelines);
					num++;
				}
				context.DrawRectangle(_caretBrush, null, new Rect(0.0 - _systemCaretWidth / 2.0, 0.0, _systemCaretWidth, _height));
			}
			if (threadLocalStore.Bidi)
			{
				double num2 = 2.0;
				if ((FlowDirection)base.AdornedElement.GetValue(FrameworkElement.FlowDirectionProperty) == FlowDirection.RightToLeft)
				{
					num2 *= -1.0;
				}
				PathGeometry pathGeometry = new PathGeometry();
				PathFigure pathFigure = new PathFigure();
				pathFigure.StartPoint = new Point(0.0, 0.0);
				pathFigure.Segments.Add(new LineSegment(new Point(0.0 - num2, 0.0), isStroked: true));
				pathFigure.Segments.Add(new LineSegment(new Point(0.0, _height / 10.0), isStroked: true));
				pathFigure.IsClosed = true;
				pathGeometry.Figures.Add(pathFigure);
				context.DrawGeometry(_caretBrush, null, pathGeometry);
			}
			for (int i = 0; i < num; i++)
			{
				context.Pop();
			}
		}
		else
		{
			Win32DestroyCaret();
		}
	}

	internal void OnTextViewUpdated()
	{
		_pendingGeometryUpdate = true;
		InvalidateArrange();
	}

	private FrameworkElement GetOwnerElement()
	{
		return GetOwnerElement(_textEditor.UiScope);
	}

	internal static FrameworkElement GetOwnerElement(FrameworkElement uiScope)
	{
		if (uiScope is IFlowDocumentViewer)
		{
			for (DependencyObject dependencyObject = uiScope; dependencyObject != null; dependencyObject = VisualTreeHelper.GetParent(dependencyObject))
			{
				if (dependencyObject is FlowDocumentReader)
				{
					return (FrameworkElement)dependencyObject;
				}
			}
			return null;
		}
		return uiScope;
	}

	private void EnsureAttachedToView()
	{
		AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_textEditor.TextView.RenderScope);
		if (adornerLayer == null)
		{
			if (_adornerLayer != null)
			{
				_adornerLayer.Remove(this);
			}
			_adornerLayer = null;
		}
		else if (_adornerLayer != adornerLayer)
		{
			if (_adornerLayer != null)
			{
				_adornerLayer.Remove(this);
			}
			_adornerLayer = adornerLayer;
			_adornerLayer.Add(this, 1073741823);
		}
	}

	private void SetBlinkAnimation(bool visible, bool positionChanged)
	{
		if (!_isBlinkEnabled)
		{
			return;
		}
		int num = Win32GetCaretBlinkTime();
		if (num > 0)
		{
			Duration duration = new Duration(TimeSpan.FromMilliseconds(num * 2));
			if (_blinkAnimationClock == null || _blinkAnimationClock.Timeline.Duration != duration)
			{
				DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
				doubleAnimationUsingKeyFrames.BeginTime = null;
				doubleAnimationUsingKeyFrames.RepeatBehavior = RepeatBehavior.Forever;
				doubleAnimationUsingKeyFrames.KeyFrames.Add(new DiscreteDoubleKeyFrame(1.0, KeyTime.FromPercent(0.0)));
				doubleAnimationUsingKeyFrames.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.0, KeyTime.FromPercent(0.5)));
				doubleAnimationUsingKeyFrames.Duration = duration;
				Timeline.SetDesiredFrameRate(doubleAnimationUsingKeyFrames, 10);
				_blinkAnimationClock = doubleAnimationUsingKeyFrames.CreateClock();
				_blinkAnimationClock.Controller.Begin();
				_caretElement.ApplyAnimationClock(UIElement.OpacityProperty, _blinkAnimationClock);
			}
		}
		else if (_blinkAnimationClock != null)
		{
			_caretElement.ApplyAnimationClock(UIElement.OpacityProperty, null);
			_blinkAnimationClock = null;
		}
		if (_blinkAnimationClock != null)
		{
			if (visible && (_blinkAnimationClock.CurrentState != ClockState.Active || positionChanged))
			{
				_blinkAnimationClock.Controller.Begin();
			}
			else if (!visible)
			{
				_blinkAnimationClock.Controller.Stop();
			}
		}
	}

	private void Win32CreateCaret()
	{
		if (!_isSelectionActive || (_win32Caret && _win32Height == _height))
		{
			return;
		}
		nint num = IntPtr.Zero;
		PresentationSource presentationSource = PresentationSource.CriticalFromVisual(this);
		if (presentationSource != null)
		{
			num = (presentationSource as IWin32Window).Handle;
		}
		if (num != IntPtr.Zero)
		{
			double y = presentationSource.CompositionTarget.TransformToDevice.Transform(new Point(0.0, _height)).Y;
			MS.Win32.NativeMethods.BitmapHandle hbitmap = MS.Win32.UnsafeNativeMethods.CreateBitmap(1, ConvertToInt32(y), 1, 1, null);
			bool num2 = MS.Win32.UnsafeNativeMethods.CreateCaret(new HandleRef(null, num), hbitmap, 0, 0);
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (!num2)
			{
				_win32Caret = false;
				throw new Win32Exception(lastWin32Error);
			}
			_win32Caret = true;
			_win32Height = _height;
		}
	}

	private void Win32DestroyCaret()
	{
		if (_isSelectionActive && _win32Caret)
		{
			SafeNativeMethods.DestroyCaret();
			Marshal.GetLastWin32Error();
			_win32Caret = false;
			_win32Height = 0.0;
		}
	}

	private void Win32SetCaretPos()
	{
		if (!_isSelectionActive)
		{
			return;
		}
		if (!_win32Caret)
		{
			Win32CreateCaret();
		}
		PresentationSource presentationSource = null;
		presentationSource = PresentationSource.CriticalFromVisual(this);
		if (presentationSource == null)
		{
			return;
		}
		Point result = new Point(0.0, 0.0);
		if (!_caretElement.TransformToAncestor(presentationSource.RootVisual).TryTransform(result, out result))
		{
			result = new Point(0.0, 0.0);
		}
		result = presentationSource.CompositionTarget.TransformToDevice.Transform(result);
		bool num = SafeNativeMethods.SetCaretPos(ConvertToInt32(result.X), ConvertToInt32(result.Y));
		int lastWin32Error = Marshal.GetLastWin32Error();
		if (!num)
		{
			_win32Caret = false;
			Win32CreateCaret();
			bool num2 = SafeNativeMethods.SetCaretPos(ConvertToInt32(result.X), ConvertToInt32(result.Y));
			lastWin32Error = Marshal.GetLastWin32Error();
			if (!num2)
			{
				throw new Win32Exception(lastWin32Error);
			}
		}
	}

	private int ConvertToInt32(double value)
	{
		if (double.IsNaN(value))
		{
			return 0;
		}
		if (value < -2147483648.0)
		{
			return int.MinValue;
		}
		if (value > 2147483647.0)
		{
			return int.MaxValue;
		}
		return Convert.ToInt32(value);
	}

	private int Win32GetCaretBlinkTime()
	{
		Invariant.Assert(_isSelectionActive, "Blink animation should only be required for an owner with active selection.");
		int caretBlinkTime = SafeNativeMethods.GetCaretBlinkTime();
		if (caretBlinkTime == 0)
		{
			return -1;
		}
		return caretBlinkTime;
	}
}
