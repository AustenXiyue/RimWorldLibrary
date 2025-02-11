using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using MS.Internal.Annotations.Anchoring;
using MS.Utility;

namespace MS.Internal.Annotations.Component;

internal sealed class MarkedHighlightComponent : Canvas, IAnnotationComponent
{
	private class ComponentsRegister
	{
		private List<MarkedHighlightComponent> _components;

		private EventHandler _selectionHandler;

		private MouseEventHandler _mouseMoveHandler;

		public List<MarkedHighlightComponent> Components => _components;

		public EventHandler SelectionHandler => _selectionHandler;

		public MouseEventHandler MouseMoveHandler => _mouseMoveHandler;

		public ComponentsRegister(EventHandler selectionHandler, MouseEventHandler mouseMoveHandler)
		{
			_components = new List<MarkedHighlightComponent>();
			_selectionHandler = selectionHandler;
			_mouseMoveHandler = mouseMoveHandler;
		}

		public void Add(MarkedHighlightComponent component)
		{
			if (_components.Count == 0)
			{
				UIElement host = component.PresentationContext.Host;
				if (host != null)
				{
					KeyboardNavigation.SetTabNavigation(host, KeyboardNavigationMode.Local);
					KeyboardNavigation.SetControlTabNavigation(host, KeyboardNavigationMode.Local);
				}
			}
			int i;
			for (i = 0; i < _components.Count && Compare(_components[i], component) <= 0; i++)
			{
			}
			_components.Insert(i, component);
			for (; i < _components.Count; i++)
			{
				_components[i].SetTabIndex(i);
			}
		}

		public void Remove(MarkedHighlightComponent component)
		{
			int i;
			for (i = 0; i < _components.Count && _components[i] != component; i++)
			{
			}
			if (i < _components.Count)
			{
				_components.RemoveAt(i);
				for (; i < _components.Count; i++)
				{
					_components[i].SetTabIndex(i);
				}
			}
		}

		private int Compare(IAnnotationComponent first, IAnnotationComponent second)
		{
			TextAnchor textAnchor = ((IAttachedAnnotation)first.AttachedAnnotations[0]).FullyAttachedAnchor as TextAnchor;
			TextAnchor textAnchor2 = ((IAttachedAnnotation)second.AttachedAnnotations[0]).FullyAttachedAnchor as TextAnchor;
			int num = textAnchor.Start.CompareTo(textAnchor2.Start);
			if (num == 0)
			{
				num = textAnchor.End.CompareTo(textAnchor2.End);
			}
			return num;
		}
	}

	public static DependencyProperty MarkerBrushProperty = DependencyProperty.Register("MarkerBrushProperty", typeof(Brush), typeof(MarkedHighlightComponent));

	public static DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThicknessProperty", typeof(double), typeof(MarkedHighlightComponent));

	internal static Color DefaultAnchorBackground = (Color)ColorConverter.ConvertFromString("#3380FF80");

	internal static Color DefaultMarkerColor = (Color)ColorConverter.ConvertFromString("#FF008000");

	internal static Color DefaultActiveAnchorBackground = (Color)ColorConverter.ConvertFromString("#3300FF00");

	internal static Color DefaultActiveMarkerColor = (Color)ColorConverter.ConvertFromString("#FF008000");

	internal static double MarkerStrokeThickness = 1.0;

	internal static double ActiveMarkerStrokeThickness = 2.0;

	internal static double MarkerVerticalSpace = 2.0;

	private static Hashtable _documentHandlers = new Hashtable();

	private byte _state;

	private HighlightComponent _highlightAnchor;

	private double _bodyHeight;

	private double _bottomTailHeight;

	private double _topTailHeight;

	private Path _leftMarker;

	private Path _rightMarker;

	private DependencyObject _DPHost;

	private const byte FocusFlag = 1;

	private const byte FocusFlagComplement = 126;

	private const byte SelectedFlag = 2;

	private const byte SelectedFlagComplement = 125;

	private IAttachedAnnotation _attachedAnnotation;

	private PresentationContext _presentationContext;

	private bool _isDirty = true;

	private ITextRange _selection;

	private UIElement _uiParent;

	public IList AttachedAnnotations
	{
		get
		{
			ArrayList arrayList = new ArrayList();
			if (_attachedAnnotation != null)
			{
				arrayList.Add(_attachedAnnotation);
			}
			return arrayList;
		}
	}

	public PresentationContext PresentationContext
	{
		get
		{
			return _presentationContext;
		}
		set
		{
			_presentationContext = value;
		}
	}

	public int ZOrder
	{
		get
		{
			return -1;
		}
		set
		{
		}
	}

	public UIElement AnnotatedElement
	{
		get
		{
			if (_attachedAnnotation == null)
			{
				return null;
			}
			return _attachedAnnotation.Parent as UIElement;
		}
	}

	public bool IsDirty
	{
		get
		{
			return _isDirty;
		}
		set
		{
			_isDirty = value;
			if (value)
			{
				UpdateGeometry();
			}
		}
	}

	public bool Focused
	{
		set
		{
			byte state = _state;
			if (value)
			{
				_state |= 1;
			}
			else
			{
				_state &= 126;
			}
			if (_state == 0 != (state == 0))
			{
				SetState();
			}
		}
	}

	public Brush MarkerBrush
	{
		set
		{
			SetValue(MarkerBrushProperty, value);
		}
	}

	public double StrokeThickness
	{
		set
		{
			SetValue(StrokeThicknessProperty, value);
		}
	}

	internal HighlightComponent HighlightAnchor
	{
		get
		{
			return _highlightAnchor;
		}
		set
		{
			_highlightAnchor = value;
			if (_highlightAnchor != null)
			{
				_highlightAnchor.DefaultBackground = DefaultAnchorBackground;
				_highlightAnchor.DefaultActiveBackground = DefaultActiveAnchorBackground;
			}
		}
	}

	public MarkedHighlightComponent(XmlQualifiedName type, DependencyObject host)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		_DPHost = ((host == null) ? this : host);
		base.ClipToBounds = false;
		HighlightAnchor = new HighlightComponent(1, highlightContent: true, type);
		base.Children.Add(HighlightAnchor);
		_leftMarker = null;
		_rightMarker = null;
		_state = 0;
		SetState();
	}

	public GeneralTransform GetDesiredTransform(GeneralTransform transform)
	{
		if (_attachedAnnotation == null)
		{
			throw new InvalidOperationException(SR.InvalidAttachedAnnotation);
		}
		HighlightAnchor.GetDesiredTransform(transform);
		return transform;
	}

	public void AddAttachedAnnotation(IAttachedAnnotation attachedAnnotation)
	{
		if (_attachedAnnotation != null)
		{
			throw new ArgumentException(SR.MoreThanOneAttachedAnnotation);
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.AddAttachedMHBegin);
		_attachedAnnotation = attachedAnnotation;
		if ((attachedAnnotation.AttachmentLevel & AttachmentLevel.StartPortion) != 0)
		{
			_leftMarker = CreateMarker(GetMarkerGeometry());
		}
		if ((attachedAnnotation.AttachmentLevel & AttachmentLevel.EndPortion) != 0)
		{
			_rightMarker = CreateMarker(GetMarkerGeometry());
		}
		RegisterAnchor();
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.AddAttachedMHEnd);
	}

	public void RemoveAttachedAnnotation(IAttachedAnnotation attachedAnnotation)
	{
		if (attachedAnnotation == null)
		{
			throw new ArgumentNullException("attachedAnnotation");
		}
		if (attachedAnnotation != _attachedAnnotation)
		{
			throw new ArgumentException(SR.InvalidAttachedAnnotation, "attachedAnnotation");
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.RemoveAttachedMHBegin);
		CleanUpAnchor();
		_attachedAnnotation = null;
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.RemoveAttachedMHEnd);
	}

	public void ModifyAttachedAnnotation(IAttachedAnnotation attachedAnnotation, object previousAttachedAnchor, AttachmentLevel previousAttachmentLevel)
	{
		throw new NotSupportedException(SR.NotSupported);
	}

	internal void SetTabIndex(int index)
	{
		if (_DPHost != null)
		{
			KeyboardNavigation.SetTabIndex(_DPHost, index);
		}
	}

	private void SetMarkerTransform(Path marker, ITextPointer anchor, ITextPointer baseAnchor, int xScaleFactor)
	{
		if (marker == null)
		{
			return;
		}
		GeometryGroup geometryGroup = marker.Data as GeometryGroup;
		Rect anchorRectangle = TextSelectionHelper.GetAnchorRectangle(anchor);
		if (anchorRectangle == Rect.Empty)
		{
			return;
		}
		double num = anchorRectangle.Height - MarkerVerticalSpace - _bottomTailHeight - _topTailHeight;
		double scaleY = 0.0;
		double scaleY2 = 0.0;
		if (num > 0.0)
		{
			scaleY = num / _bodyHeight;
			scaleY2 = 1.0;
		}
		ScaleTransform value = new ScaleTransform(1.0, scaleY);
		TranslateTransform value2 = new TranslateTransform(anchorRectangle.X, anchorRectangle.Y + _topTailHeight + MarkerVerticalSpace);
		TransformGroup transformGroup = new TransformGroup();
		transformGroup.Children.Add(value);
		transformGroup.Children.Add(value2);
		ScaleTransform value3 = new ScaleTransform(xScaleFactor, scaleY2);
		TranslateTransform value4 = new TranslateTransform(anchorRectangle.X, anchorRectangle.Bottom - _bottomTailHeight);
		TranslateTransform value5 = new TranslateTransform(anchorRectangle.X, anchorRectangle.Top + MarkerVerticalSpace);
		TransformGroup transformGroup2 = new TransformGroup();
		transformGroup2.Children.Add(value3);
		transformGroup2.Children.Add(value4);
		TransformGroup transformGroup3 = new TransformGroup();
		transformGroup3.Children.Add(value3);
		transformGroup3.Children.Add(value5);
		if (geometryGroup.Children[0] != null)
		{
			geometryGroup.Children[0].Transform = transformGroup3;
		}
		if (geometryGroup.Children[1] != null)
		{
			geometryGroup.Children[1].Transform = transformGroup;
		}
		if (geometryGroup.Children[2] != null)
		{
			geometryGroup.Children[2].Transform = transformGroup2;
		}
		if (baseAnchor != null)
		{
			ITextView documentPageTextView = TextSelectionHelper.GetDocumentPageTextView(baseAnchor);
			ITextView documentPageTextView2 = TextSelectionHelper.GetDocumentPageTextView(anchor);
			if (documentPageTextView != documentPageTextView2 && documentPageTextView.RenderScope != null && documentPageTextView2.RenderScope != null)
			{
				geometryGroup.Transform = (Transform)documentPageTextView2.RenderScope.TransformToVisual(documentPageTextView.RenderScope);
			}
		}
	}

	private void SetSelected(bool selected)
	{
		byte state = _state;
		if (selected && _uiParent.IsFocused)
		{
			_state |= 2;
		}
		else
		{
			_state &= 125;
		}
		if (_state == 0 != (state == 0))
		{
			SetState();
		}
	}

	private void RemoveHighlightMarkers()
	{
		if (_leftMarker != null)
		{
			base.Children.Remove(_leftMarker);
		}
		if (_rightMarker != null)
		{
			base.Children.Remove(_rightMarker);
		}
		_leftMarker = null;
		_rightMarker = null;
	}

	private void RegisterAnchor()
	{
		ITextContainer textContainer = ((_attachedAnnotation.AttachedAnchor as TextAnchor) ?? throw new ArgumentException(SR.InvalidAttachedAnchor)).Start.TextContainer;
		HighlightAnchor.AddAttachedAnnotation(_attachedAnnotation);
		UpdateGeometry();
		AdornerPresentationContext.HostComponent(AdornerLayer.GetAdornerLayer(AnnotatedElement) ?? throw new InvalidOperationException(SR.Format(SR.NoPresentationContextForGivenElement, AnnotatedElement)), this, AnnotatedElement, reorder: false);
		_selection = textContainer.TextSelection;
		if (_selection == null)
		{
			return;
		}
		_uiParent = PathNode.GetParent(textContainer.Parent) as UIElement;
		RegisterComponent();
		if (_uiParent != null)
		{
			_uiParent.GotKeyboardFocus += OnContainerGotFocus;
			_uiParent.LostKeyboardFocus += OnContainerLostFocus;
			if (HighlightAnchor.IsSelected(_selection))
			{
				SetSelected(selected: true);
			}
		}
	}

	private void CleanUpAnchor()
	{
		if (_selection != null)
		{
			UnregisterComponent();
			if (_uiParent != null)
			{
				_uiParent.GotKeyboardFocus -= OnContainerGotFocus;
				_uiParent.LostKeyboardFocus -= OnContainerLostFocus;
			}
		}
		_presentationContext.RemoveFromHost(this, reorder: false);
		if (HighlightAnchor != null)
		{
			HighlightAnchor.RemoveAttachedAnnotation(_attachedAnnotation);
			base.Children.Remove(HighlightAnchor);
			HighlightAnchor = null;
			RemoveHighlightMarkers();
		}
		_attachedAnnotation = null;
	}

	private void SetState()
	{
		if (_state == 0)
		{
			if (_highlightAnchor != null)
			{
				_highlightAnchor.Activate(active: false);
			}
			MarkerBrush = new SolidColorBrush(DefaultMarkerColor);
			StrokeThickness = MarkerStrokeThickness;
			_DPHost.SetValue(StickyNoteControl.IsActiveProperty, value: false);
		}
		else
		{
			if (_highlightAnchor != null)
			{
				_highlightAnchor.Activate(active: true);
			}
			MarkerBrush = new SolidColorBrush(DefaultActiveMarkerColor);
			StrokeThickness = ActiveMarkerStrokeThickness;
			_DPHost.SetValue(StickyNoteControl.IsActiveProperty, value: true);
		}
	}

	private Path CreateMarker(Geometry geometry)
	{
		Path path = new Path();
		path.Data = geometry;
		Binding binding = new Binding("MarkerBrushProperty");
		binding.Source = this;
		path.SetBinding(Shape.StrokeProperty, binding);
		Binding binding2 = new Binding("StrokeThicknessProperty");
		binding2.Source = this;
		path.SetBinding(Shape.StrokeThicknessProperty, binding2);
		path.StrokeEndLineCap = PenLineCap.Round;
		path.StrokeStartLineCap = PenLineCap.Round;
		base.Children.Add(path);
		return path;
	}

	private void RegisterComponent()
	{
		ComponentsRegister componentsRegister = (ComponentsRegister)_documentHandlers[_selection];
		if (componentsRegister == null)
		{
			componentsRegister = new ComponentsRegister(OnSelectionChanged, OnMouseMove);
			_documentHandlers.Add(_selection, componentsRegister);
			_selection.Changed += componentsRegister.SelectionHandler;
			if (_uiParent != null)
			{
				_uiParent.MouseMove += componentsRegister.MouseMoveHandler;
			}
		}
		componentsRegister.Add(this);
	}

	private void UnregisterComponent()
	{
		ComponentsRegister componentsRegister = (ComponentsRegister)_documentHandlers[_selection];
		componentsRegister.Remove(this);
		if (componentsRegister.Components.Count == 0)
		{
			_documentHandlers.Remove(_selection);
			_selection.Changed -= componentsRegister.SelectionHandler;
			if (_uiParent != null)
			{
				_uiParent.MouseMove -= componentsRegister.MouseMoveHandler;
			}
		}
	}

	private void UpdateGeometry()
	{
		if (HighlightAnchor == null || HighlightAnchor == null)
		{
			throw new Exception(SR.UndefinedHighlightAnchor);
		}
		TextAnchor range = ((IHighlightRange)HighlightAnchor).Range;
		ITextPointer textPointer = range.Start.CreatePointer(LogicalDirection.Forward);
		ITextPointer textPointer2 = range.End.CreatePointer(LogicalDirection.Backward);
		FlowDirection textFlowDirection = GetTextFlowDirection(textPointer);
		FlowDirection textFlowDirection2 = GetTextFlowDirection(textPointer2);
		SetMarkerTransform(_leftMarker, textPointer, null, (textFlowDirection == FlowDirection.LeftToRight) ? 1 : (-1));
		SetMarkerTransform(_rightMarker, textPointer2, textPointer, (textFlowDirection2 != 0) ? 1 : (-1));
		HighlightAnchor.IsDirty = true;
		IsDirty = false;
	}

	private Geometry GetMarkerGeometry()
	{
		GeometryGroup geometryGroup = new GeometryGroup();
		geometryGroup.Children.Add(new LineGeometry(new Point(0.0, 1.0), new Point(1.0, 0.0)));
		geometryGroup.Children.Add(new LineGeometry(new Point(0.0, 0.0), new Point(0.0, 50.0)));
		geometryGroup.Children.Add(new LineGeometry(new Point(0.0, 0.0), new Point(1.0, 1.0)));
		_bodyHeight = geometryGroup.Children[1].Bounds.Height;
		_topTailHeight = geometryGroup.Children[0].Bounds.Height;
		_bottomTailHeight = geometryGroup.Children[2].Bounds.Height;
		return geometryGroup;
	}

	private void CheckPosition(ITextPointer position)
	{
		bool flag = ((IHighlightRange)_highlightAnchor).Range.Contains(position);
		bool flag2 = (bool)_DPHost.GetValue(StickyNoteControl.IsMouseOverAnchorProperty);
		if (flag != flag2)
		{
			_DPHost.SetValue(StickyNoteControl.IsMouseOverAnchorPropertyKey, flag);
		}
	}

	private void OnContainerGotFocus(object sender, KeyboardFocusChangedEventArgs args)
	{
		if (HighlightAnchor != null && HighlightAnchor.IsSelected(_selection))
		{
			SetSelected(selected: true);
		}
	}

	private void OnContainerLostFocus(object sender, KeyboardFocusChangedEventArgs args)
	{
		SetSelected(selected: false);
	}

	private static FlowDirection GetTextFlowDirection(ITextPointer pointer)
	{
		Invariant.Assert(pointer != null, "Null pointer passed.");
		Invariant.Assert(pointer.IsAtInsertionPosition, "Pointer is not an insertion position");
		int num = 0;
		LogicalDirection logicalDirection = pointer.LogicalDirection;
		TextPointerContext pointerContext = pointer.GetPointerContext(logicalDirection);
		if ((pointerContext == TextPointerContext.ElementEnd || pointerContext == TextPointerContext.ElementStart) && !TextSchema.IsFormattingType(pointer.ParentType))
		{
			return (FlowDirection)pointer.GetValue(FrameworkElement.FlowDirectionProperty);
		}
		Rect anchorRectangle = TextSelectionHelper.GetAnchorRectangle(pointer);
		ITextPointer nextInsertionPosition = pointer.GetNextInsertionPosition(logicalDirection);
		if (nextInsertionPosition != null)
		{
			nextInsertionPosition = nextInsertionPosition.CreatePointer((logicalDirection == LogicalDirection.Backward) ? LogicalDirection.Forward : LogicalDirection.Backward);
			if (logicalDirection == LogicalDirection.Forward)
			{
				if (pointerContext == TextPointerContext.ElementEnd && nextInsertionPosition.GetPointerContext(nextInsertionPosition.LogicalDirection) == TextPointerContext.ElementStart)
				{
					return (FlowDirection)pointer.GetValue(FrameworkElement.FlowDirectionProperty);
				}
			}
			else if (pointerContext == TextPointerContext.ElementStart && nextInsertionPosition.GetPointerContext(nextInsertionPosition.LogicalDirection) == TextPointerContext.ElementEnd)
			{
				return (FlowDirection)pointer.GetValue(FrameworkElement.FlowDirectionProperty);
			}
			Rect anchorRectangle2 = TextSelectionHelper.GetAnchorRectangle(nextInsertionPosition);
			if (anchorRectangle2 != Rect.Empty && anchorRectangle != Rect.Empty)
			{
				num = Math.Sign(anchorRectangle2.Left - anchorRectangle.Left);
				if (logicalDirection == LogicalDirection.Backward)
				{
					num = -num;
				}
			}
		}
		if (num == 0)
		{
			return (FlowDirection)pointer.GetValue(FrameworkElement.FlowDirectionProperty);
		}
		return (num <= 0) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
	}

	private static void OnSelectionChanged(object sender, EventArgs args)
	{
		ITextRange textRange = sender as ITextRange;
		ComponentsRegister componentsRegister = (ComponentsRegister)_documentHandlers[textRange];
		if (componentsRegister == null)
		{
			return;
		}
		List<MarkedHighlightComponent> components = componentsRegister.Components;
		bool[] array = new bool[components.Count];
		for (int i = 0; i < components.Count; i++)
		{
			array[i] = components[i].HighlightAnchor.IsSelected(textRange);
			if (!array[i])
			{
				components[i].SetSelected(selected: false);
			}
		}
		for (int j = 0; j < components.Count; j++)
		{
			if (array[j])
			{
				components[j].SetSelected(selected: true);
			}
		}
	}

	private static void OnMouseMove(object sender, MouseEventArgs args)
	{
		if (!(sender is IServiceProvider serviceProvider))
		{
			return;
		}
		ITextView textView = (ITextView)serviceProvider.GetService(typeof(ITextView));
		if (textView != null && textView.IsValid)
		{
			Point position = Mouse.PrimaryDevice.GetPosition(textView.RenderScope);
			ITextPointer textPositionFromPoint = textView.GetTextPositionFromPoint(position, snapToText: false);
			if (textPositionFromPoint != null)
			{
				CheckAllHighlightRanges(textPositionFromPoint);
			}
		}
	}

	private static void CheckAllHighlightRanges(ITextPointer pos)
	{
		ITextRange textSelection = pos.TextContainer.TextSelection;
		if (textSelection == null)
		{
			return;
		}
		ComponentsRegister componentsRegister = (ComponentsRegister)_documentHandlers[textSelection];
		if (componentsRegister != null)
		{
			List<MarkedHighlightComponent> components = componentsRegister.Components;
			for (int i = 0; i < components.Count; i++)
			{
				components[i].CheckPosition(pos);
			}
		}
	}
}
