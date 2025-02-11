using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Automation.Peers;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using MS.Internal.Commands;
using MS.Internal.Controls;
using MS.Internal.Ink;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Defines an area that receives and displays ink strokes. </summary>
[ContentProperty("Children")]
public class InkCanvas : FrameworkElement, IAddChild
{
	private class RTIHighContrastCallback : HighContrastCallback
	{
		private InkCanvas _thisInkCanvas;

		internal override Dispatcher Dispatcher => _thisInkCanvas.Dispatcher;

		internal RTIHighContrastCallback(InkCanvas inkCanvas)
		{
			_thisInkCanvas = inkCanvas;
		}

		private RTIHighContrastCallback()
		{
		}

		internal override void TurnHighContrastOn(Color highContrastColor)
		{
			DrawingAttributes drawingAttributes = _thisInkCanvas.DefaultDrawingAttributes.Clone();
			drawingAttributes.Color = highContrastColor;
			_thisInkCanvas.UpdateDynamicRenderer(drawingAttributes);
		}

		internal override void TurnHighContrastOff()
		{
			_thisInkCanvas.UpdateDynamicRenderer(_thisInkCanvas.DefaultDrawingAttributes);
		}
	}

	private class ActiveEditingMode2VisibilityConverter : IValueConverter
	{
		public object Convert(object o, Type type, object parameter, CultureInfo culture)
		{
			if ((InkCanvasEditingMode)o != 0)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
		{
			return null;
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.InkCanvas.Background" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.InkCanvas.Background" /> dependency property.</returns>
	public static readonly DependencyProperty BackgroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.InkCanvas.Top" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.InkCanvas.Top" /> attached property.</returns>
	public static readonly DependencyProperty TopProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.InkCanvas.Bottom" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.InkCanvas.Bottom" /> attached property.</returns>
	public static readonly DependencyProperty BottomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.InkCanvas.Left" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.InkCanvas.Left" /> attached property.</returns>
	public static readonly DependencyProperty LeftProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.InkCanvas.Right" /> attached propertyy.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.InkCanvas.Right" /> attached property.</returns>
	public static readonly DependencyProperty RightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.InkCanvas.Strokes" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.InkCanvas.Strokes" /> dependency property.</returns>
	public static readonly DependencyProperty StrokesProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.InkCanvas.DefaultDrawingAttributes" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.InkCanvas.DefaultDrawingAttributes" /> dependency property.</returns>
	public static readonly DependencyProperty DefaultDrawingAttributesProperty;

	internal static readonly DependencyPropertyKey ActiveEditingModePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.InkCanvas.ActiveEditingMode" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.InkCanvas.ActiveEditingMode" /> dependency property.</returns>
	public static readonly DependencyProperty ActiveEditingModeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.InkCanvas.EditingMode" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.InkCanvas.EditingMode" /> dependency property.</returns>
	public static readonly DependencyProperty EditingModeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.InkCanvas.EditingModeInverted" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.InkCanvas.EditingModeInverted" /> dependency property.</returns>
	public static readonly DependencyProperty EditingModeInvertedProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.InkCanvas.StrokeCollected" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.InkCanvas.StrokeCollected" /> routed event.</returns>
	public static readonly RoutedEvent StrokeCollectedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.InkCanvas.Gesture" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.InkCanvas.Gesture" /> routed event.</returns>
	public static readonly RoutedEvent GestureEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.InkCanvas.ActiveEditingModeChanged" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.InkCanvas.ActiveEditingModeChanged" /> routed event.</returns>
	public static readonly RoutedEvent ActiveEditingModeChangedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.InkCanvas.EditingModeChanged" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.InkCanvas.EditingModeChanged" /> routed event.</returns>
	public static readonly RoutedEvent EditingModeChangedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.InkCanvas.EditingModeInvertedChanged" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.InkCanvas.EditingModeInvertedChanged" /> routed event.</returns>
	public static readonly RoutedEvent EditingModeInvertedChangedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.InkCanvas.StrokeErased" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.InkCanvas.StrokeErased" /> routed event.</returns>
	public static readonly RoutedEvent StrokeErasedEvent;

	internal static readonly RoutedCommand DeselectCommand;

	private InkCanvasSelection _selection;

	private InkCanvasSelectionAdorner _selectionAdorner;

	private InkCanvasFeedbackAdorner _feedbackAdorner;

	private InkCanvasInnerCanvas _innerCanvas;

	private AdornerDecorator _localAdornerDecorator;

	private StrokeCollection _dynamicallySelectedStrokes;

	private EditingCoordinator _editingCoordinator;

	private StylusPointDescription _defaultStylusPointDescription;

	private StylusShape _eraserShape;

	private bool _useCustomCursor;

	private InkPresenter _inkPresenter;

	private DynamicRenderer _dynamicRenderer;

	private ClipboardProcessor _clipboardProcessor;

	private GestureRecognizer _gestureRecognizer;

	private RTIHighContrastCallback _rtiHighContrastCallback;

	private const double c_pasteDefaultLocation = 0.0;

	private const string InkCanvasDeselectKey = "Esc";

	private const string KeyCtrlInsert = "Ctrl+Insert";

	private const string KeyShiftInsert = "Shift+Insert";

	private const string KeyShiftDelete = "Shift+Delete";

	protected override int VisualChildrenCount => (_localAdornerDecorator != null) ? 1 : 0;

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Brush" />. The brush is used to fill the border area surrounding a <see cref="T:System.Windows.Controls.InkCanvas" />.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Brush" /> used to fill the border area surrounding a <see cref="T:System.Windows.Controls.InkCanvas" />.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public Brush Background
	{
		get
		{
			return (Brush)GetValue(BackgroundProperty);
		}
		set
		{
			SetValue(BackgroundProperty, value);
		}
	}

	/// <summary>Gets or sets the collection of ink <see cref="T:System.Windows.Ink.Stroke" /> objects collected by the <see cref="T:System.Windows.Controls.InkCanvas" />.  </summary>
	/// <returns>The collection of <see cref="T:System.Windows.Ink.Stroke" /> objects contained within the <see cref="T:System.Windows.Controls.InkCanvas" />.</returns>
	public StrokeCollection Strokes
	{
		get
		{
			return (StrokeCollection)GetValue(StrokesProperty);
		}
		set
		{
			SetValue(StrokesProperty, value);
		}
	}

	internal InkCanvasSelectionAdorner SelectionAdorner
	{
		get
		{
			if (_selectionAdorner == null)
			{
				_selectionAdorner = new InkCanvasSelectionAdorner(InnerCanvas);
				Binding binding = new Binding();
				binding.Path = new PropertyPath(ActiveEditingModeProperty);
				binding.Mode = BindingMode.OneWay;
				binding.Source = this;
				binding.Converter = new ActiveEditingMode2VisibilityConverter();
				_selectionAdorner.SetBinding(UIElement.VisibilityProperty, binding);
			}
			return _selectionAdorner;
		}
	}

	internal InkCanvasFeedbackAdorner FeedbackAdorner
	{
		get
		{
			VerifyAccess();
			if (_feedbackAdorner == null)
			{
				_feedbackAdorner = new InkCanvasFeedbackAdorner(this);
			}
			return _feedbackAdorner;
		}
	}

	/// <summary>Gets (determines) whether the gesture recognition component is available on the user's system.</summary>
	/// <returns>true if the recognition component is available; otherwise, false. </returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsGestureRecognizerAvailable => GestureRecognizer.IsRecognizerAvailable;

	/// <summary>Retrieves child elements of the <see cref="T:System.Windows.Controls.InkCanvas" />. </summary>
	/// <returns>A collection of child elements located on the <see cref="T:System.Windows.Controls.InkCanvas" />.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public UIElementCollection Children => InnerCanvas.Children;

	/// <summary>Gets or sets the drawing attributes that are applied to new ink strokes made on the <see cref="T:System.Windows.Controls.InkCanvas" />.  </summary>
	/// <returns>The default drawing attributes for the <see cref="T:System.Windows.Controls.InkCanvas" />.</returns>
	public DrawingAttributes DefaultDrawingAttributes
	{
		get
		{
			return (DrawingAttributes)GetValue(DefaultDrawingAttributesProperty);
		}
		set
		{
			SetValue(DefaultDrawingAttributesProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Ink.StylusShape" /> used to point-erase ink from an <see cref="T:System.Windows.Controls.InkCanvas" />.</summary>
	/// <returns>The eraser shape associated with the <see cref="T:System.Windows.Controls.InkCanvas" />.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public StylusShape EraserShape
	{
		get
		{
			VerifyAccess();
			if (_eraserShape == null)
			{
				_eraserShape = new RectangleStylusShape(8.0, 8.0);
			}
			return _eraserShape;
		}
		set
		{
			VerifyAccess();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			StylusShape eraserShape = EraserShape;
			_eraserShape = value;
			if (eraserShape.Width != _eraserShape.Width || eraserShape.Height != _eraserShape.Height || eraserShape.Rotation != _eraserShape.Rotation || eraserShape.GetType() != _eraserShape.GetType())
			{
				EditingCoordinator.UpdatePointEraserCursor();
			}
		}
	}

	/// <summary>Gets the current editing mode of the <see cref="T:System.Windows.Controls.InkCanvas" />.  </summary>
	/// <returns>The current editing mode of the <see cref="T:System.Windows.Controls.InkCanvas" />.</returns>
	public InkCanvasEditingMode ActiveEditingMode => (InkCanvasEditingMode)GetValue(ActiveEditingModeProperty);

	/// <summary>Gets or sets the user editing mode used by an active pointing device.  </summary>
	/// <returns>The editing mode used when a pointing device (such as a tablet pen or mouse) is active.</returns>
	public InkCanvasEditingMode EditingMode
	{
		get
		{
			return (InkCanvasEditingMode)GetValue(EditingModeProperty);
		}
		set
		{
			SetValue(EditingModeProperty, value);
		}
	}

	/// <summary>Gets or sets the user editing mode if the stylus is inverted when it interacts with the <see cref="T:System.Windows.Controls.InkCanvas" />.  </summary>
	/// <returns>The inverted editing mode of the <see cref="T:System.Windows.Controls.InkCanvas" />.</returns>
	public InkCanvasEditingMode EditingModeInverted
	{
		get
		{
			return (InkCanvasEditingMode)GetValue(EditingModeInvertedProperty);
		}
		set
		{
			SetValue(EditingModeInvertedProperty, value);
		}
	}

	/// <summary>Gets or sets a Boolean value that indicates whether to override standard <see cref="T:System.Windows.Controls.InkCanvas" /> cursor functionality to support a custom cursor. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.InkCanvas" /> is using a custom cursor; otherwise, false.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool UseCustomCursor
	{
		get
		{
			VerifyAccess();
			return _useCustomCursor;
		}
		set
		{
			VerifyAccess();
			if (_useCustomCursor != value)
			{
				_useCustomCursor = value;
				UpdateCursor();
			}
		}
	}

	/// <summary>Gets or sets a Boolean value which indicates whether the user is enabled to move selected ink strokes and/or elements on the <see cref="T:System.Windows.Controls.InkCanvas" />. </summary>
	/// <returns>true if a user can move strokes and/or elements on the <see cref="T:System.Windows.Controls.InkCanvas" />; otherwise, false.</returns>
	public bool MoveEnabled
	{
		get
		{
			VerifyAccess();
			return _editingCoordinator.MoveEnabled;
		}
		set
		{
			VerifyAccess();
			if (_editingCoordinator.MoveEnabled != value)
			{
				_editingCoordinator.MoveEnabled = value;
			}
		}
	}

	/// <summary>Gets or sets a Boolean value that indicates whether the user can resize selected ink strokes and/or elements on the <see cref="T:System.Windows.Controls.InkCanvas" />. </summary>
	/// <returns>true if a user can resize strokes and/or elements on the <see cref="T:System.Windows.Controls.InkCanvas" />; otherwise, false.</returns>
	public bool ResizeEnabled
	{
		get
		{
			VerifyAccess();
			return _editingCoordinator.ResizeEnabled;
		}
		set
		{
			VerifyAccess();
			if (_editingCoordinator.ResizeEnabled != value)
			{
				_editingCoordinator.ResizeEnabled = value;
			}
		}
	}

	/// <summary>Gets or sets the stylus point description for an <see cref="T:System.Windows.Controls.InkCanvas" />. </summary>
	/// <returns>The stylus point description for an <see cref="T:System.Windows.Controls.InkCanvas" />.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public StylusPointDescription DefaultStylusPointDescription
	{
		get
		{
			VerifyAccess();
			return _defaultStylusPointDescription;
		}
		set
		{
			VerifyAccess();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_defaultStylusPointDescription = value;
		}
	}

	/// <summary>Gets or sets formats that can be pasted onto the <see cref="T:System.Windows.Controls.InkCanvas" />.</summary>
	/// <returns>A collection of enumeration values. The default is <see cref="F:System.Windows.Controls.InkCanvasClipboardFormat.InkSerializedFormat" />.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IEnumerable<InkCanvasClipboardFormat> PreferredPasteFormats
	{
		get
		{
			VerifyAccess();
			return ClipboardProcessor.PreferredFormats;
		}
		set
		{
			VerifyAccess();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			ClipboardProcessor.PreferredFormats = value;
		}
	}

	/// <summary>Returns enumerator to logical children. </summary>
	protected internal override IEnumerator LogicalChildren => InnerCanvas.PrivateLogicalChildren;

	/// <summary>Gets or sets the renderer that dynamically draws ink on the <see cref="T:System.Windows.Controls.InkCanvas" />.</summary>
	/// <returns>The renderer that dynamically draws ink on the <see cref="T:System.Windows.Controls.InkCanvas" />.</returns>
	protected DynamicRenderer DynamicRenderer
	{
		get
		{
			VerifyAccess();
			return InternalDynamicRenderer;
		}
		set
		{
			VerifyAccess();
			if (value == _dynamicRenderer)
			{
				return;
			}
			int num = -1;
			if (_dynamicRenderer != null)
			{
				num = base.StylusPlugIns.IndexOf(_dynamicRenderer);
				if (-1 != num)
				{
					base.StylusPlugIns.RemoveAt(num);
				}
				if (InkPresenter.ContainsAttachedVisual(_dynamicRenderer.RootVisual))
				{
					InkPresenter.DetachVisuals(_dynamicRenderer.RootVisual);
				}
			}
			_dynamicRenderer = value;
			if (_dynamicRenderer == null)
			{
				return;
			}
			if (!base.StylusPlugIns.Contains(_dynamicRenderer))
			{
				if (-1 != num)
				{
					base.StylusPlugIns.Insert(num, _dynamicRenderer);
				}
				else
				{
					base.StylusPlugIns.Add(_dynamicRenderer);
				}
			}
			_dynamicRenderer.DrawingAttributes = DefaultDrawingAttributes;
			if (!InkPresenter.ContainsAttachedVisual(_dynamicRenderer.RootVisual) && _dynamicRenderer.Enabled && _dynamicRenderer.RootVisual != null)
			{
				InkPresenter.AttachVisuals(_dynamicRenderer.RootVisual, DefaultDrawingAttributes);
			}
		}
	}

	/// <summary>Gets the ink presenter that displays ink on the <see cref="T:System.Windows.Controls.InkCanvas" />.</summary>
	/// <returns>The ink presenter that displays ink on the <see cref="T:System.Windows.Controls.InkCanvas" />.</returns>
	protected InkPresenter InkPresenter
	{
		get
		{
			VerifyAccess();
			if (_inkPresenter == null)
			{
				_inkPresenter = new InkPresenter();
				Binding binding = new Binding();
				binding.Path = new PropertyPath(StrokesProperty);
				binding.Mode = BindingMode.OneWay;
				binding.Source = this;
				_inkPresenter.SetBinding(InkPresenter.StrokesProperty, binding);
			}
			return _inkPresenter;
		}
	}

	internal EditingCoordinator EditingCoordinator => _editingCoordinator;

	internal DynamicRenderer InternalDynamicRenderer => _dynamicRenderer;

	internal InkCanvasInnerCanvas InnerCanvas
	{
		get
		{
			if (_innerCanvas == null)
			{
				_innerCanvas = new InkCanvasInnerCanvas(this);
				Binding binding = new Binding();
				binding.Path = new PropertyPath(BackgroundProperty);
				binding.Mode = BindingMode.OneWay;
				binding.Source = this;
				_innerCanvas.SetBinding(Panel.BackgroundProperty, binding);
			}
			return _innerCanvas;
		}
	}

	internal InkCanvasSelection InkCanvasSelection
	{
		get
		{
			if (_selection == null)
			{
				_selection = new InkCanvasSelection(this);
			}
			return _selection;
		}
	}

	private ClipboardProcessor ClipboardProcessor
	{
		get
		{
			if (_clipboardProcessor == null)
			{
				_clipboardProcessor = new ClipboardProcessor(this);
			}
			return _clipboardProcessor;
		}
	}

	private GestureRecognizer GestureRecognizer
	{
		get
		{
			if (_gestureRecognizer == null)
			{
				_gestureRecognizer = new GestureRecognizer();
			}
			return _gestureRecognizer;
		}
	}

	/// <summary>Occurs when a stroke drawn by the user is added to the <see cref="P:System.Windows.Controls.InkCanvas.Strokes" /> property. </summary>
	[Category("Behavior")]
	public event InkCanvasStrokeCollectedEventHandler StrokeCollected
	{
		add
		{
			AddHandler(StrokeCollectedEvent, value);
		}
		remove
		{
			RemoveHandler(StrokeCollectedEvent, value);
		}
	}

	/// <summary>Occurs when the <see cref="T:System.Windows.Controls.InkCanvas" /> detects a gesture.</summary>
	[Category("Behavior")]
	public event InkCanvasGestureEventHandler Gesture
	{
		add
		{
			AddHandler(GestureEvent, value);
		}
		remove
		{
			RemoveHandler(GestureEvent, value);
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.InkCanvas.Strokes" /> property is replaced.</summary>
	public event InkCanvasStrokesReplacedEventHandler StrokesReplaced;

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.InkCanvas.DefaultDrawingAttributes" /> property is replaced. </summary>
	public event DrawingAttributesReplacedEventHandler DefaultDrawingAttributesReplaced;

	/// <summary>Occurs when the current editing mode changes.</summary>
	[Category("Behavior")]
	public event RoutedEventHandler ActiveEditingModeChanged
	{
		add
		{
			AddHandler(ActiveEditingModeChangedEvent, value);
		}
		remove
		{
			RemoveHandler(ActiveEditingModeChangedEvent, value);
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.InkCanvas.EditingMode" /> property of an <see cref="T:System.Windows.Controls.InkCanvas" /> object has been changed. </summary>
	[Category("Behavior")]
	public event RoutedEventHandler EditingModeChanged
	{
		add
		{
			AddHandler(EditingModeChangedEvent, value);
		}
		remove
		{
			RemoveHandler(EditingModeChangedEvent, value);
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.InkCanvas.EditingModeInverted" /> property of an <see cref="T:System.Windows.Controls.InkCanvas" /> object has been changed. </summary>
	[Category("Behavior")]
	public event RoutedEventHandler EditingModeInvertedChanged
	{
		add
		{
			AddHandler(EditingModeInvertedChangedEvent, value);
		}
		remove
		{
			RemoveHandler(EditingModeInvertedChangedEvent, value);
		}
	}

	/// <summary>Occurs before selected strokes and elements are moved. </summary>
	public event InkCanvasSelectionEditingEventHandler SelectionMoving;

	/// <summary>Occurs after the user moves a selection of strokes and/or elements. </summary>
	public event EventHandler SelectionMoved;

	/// <summary>Occurs just before a user erases a stroke.</summary>
	public event InkCanvasStrokeErasingEventHandler StrokeErasing;

	/// <summary>Occurs when user erases a stroke. </summary>
	[Category("Behavior")]
	public event RoutedEventHandler StrokeErased
	{
		add
		{
			AddHandler(StrokeErasedEvent, value);
		}
		remove
		{
			RemoveHandler(StrokeErasedEvent, value);
		}
	}

	/// <summary>Occurs before selected strokes and elements are resized.</summary>
	public event InkCanvasSelectionEditingEventHandler SelectionResizing;

	/// <summary>Occurs when a selection of strokes and/or elements has been resized by the user. </summary>
	public event EventHandler SelectionResized;

	/// <summary>Occurs when a new set of ink strokes and/or elements is being selected. </summary>
	public event InkCanvasSelectionChangingEventHandler SelectionChanging;

	/// <summary>Occurs when the selection on the <see cref="T:System.Windows.Controls.InkCanvas" /> changes. </summary>
	public event EventHandler SelectionChanged;

	static InkCanvas()
	{
		BackgroundProperty = Panel.BackgroundProperty.AddOwner(typeof(InkCanvas), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		TopProperty = DependencyProperty.RegisterAttached("Top", typeof(double), typeof(InkCanvas), new FrameworkPropertyMetadata(double.NaN, OnPositioningChanged), Shape.IsDoubleFiniteOrNaN);
		BottomProperty = DependencyProperty.RegisterAttached("Bottom", typeof(double), typeof(InkCanvas), new FrameworkPropertyMetadata(double.NaN, OnPositioningChanged), Shape.IsDoubleFiniteOrNaN);
		LeftProperty = DependencyProperty.RegisterAttached("Left", typeof(double), typeof(InkCanvas), new FrameworkPropertyMetadata(double.NaN, OnPositioningChanged), Shape.IsDoubleFiniteOrNaN);
		RightProperty = DependencyProperty.RegisterAttached("Right", typeof(double), typeof(InkCanvas), new FrameworkPropertyMetadata(double.NaN, OnPositioningChanged), Shape.IsDoubleFiniteOrNaN);
		StrokesProperty = InkPresenter.StrokesProperty.AddOwner(typeof(InkCanvas), new FrameworkPropertyMetadata(new StrokeCollectionDefaultValueFactory(), OnStrokesChanged));
		DefaultDrawingAttributesProperty = DependencyProperty.Register("DefaultDrawingAttributes", typeof(DrawingAttributes), typeof(InkCanvas), new FrameworkPropertyMetadata(new DrawingAttributesDefaultValueFactory(), OnDefaultDrawingAttributesChanged), (object value) => value != null);
		ActiveEditingModePropertyKey = DependencyProperty.RegisterReadOnly("ActiveEditingMode", typeof(InkCanvasEditingMode), typeof(InkCanvas), new FrameworkPropertyMetadata(InkCanvasEditingMode.Ink));
		ActiveEditingModeProperty = ActiveEditingModePropertyKey.DependencyProperty;
		EditingModeProperty = DependencyProperty.Register("EditingMode", typeof(InkCanvasEditingMode), typeof(InkCanvas), new FrameworkPropertyMetadata(InkCanvasEditingMode.Ink, OnEditingModeChanged), ValidateEditingMode);
		EditingModeInvertedProperty = DependencyProperty.Register("EditingModeInverted", typeof(InkCanvasEditingMode), typeof(InkCanvas), new FrameworkPropertyMetadata(InkCanvasEditingMode.EraseByStroke, OnEditingModeInvertedChanged), ValidateEditingMode);
		StrokeCollectedEvent = EventManager.RegisterRoutedEvent("StrokeCollected", RoutingStrategy.Bubble, typeof(InkCanvasStrokeCollectedEventHandler), typeof(InkCanvas));
		GestureEvent = EventManager.RegisterRoutedEvent("Gesture", RoutingStrategy.Bubble, typeof(InkCanvasGestureEventHandler), typeof(InkCanvas));
		ActiveEditingModeChangedEvent = EventManager.RegisterRoutedEvent("ActiveEditingModeChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InkCanvas));
		EditingModeChangedEvent = EventManager.RegisterRoutedEvent("EditingModeChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InkCanvas));
		EditingModeInvertedChangedEvent = EventManager.RegisterRoutedEvent("EditingModeInvertedChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InkCanvas));
		StrokeErasedEvent = EventManager.RegisterRoutedEvent("StrokeErased", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InkCanvas));
		DeselectCommand = new RoutedCommand("Deselect", typeof(InkCanvas));
		Type typeFromHandle = typeof(InkCanvas);
		EventManager.RegisterClassHandler(typeFromHandle, Stylus.StylusDownEvent, new StylusDownEventHandler(_OnDeviceDown));
		EventManager.RegisterClassHandler(typeFromHandle, Mouse.MouseDownEvent, new MouseButtonEventHandler(_OnDeviceDown));
		EventManager.RegisterClassHandler(typeFromHandle, Stylus.StylusUpEvent, new StylusEventHandler(_OnDeviceUp));
		EventManager.RegisterClassHandler(typeFromHandle, Mouse.MouseUpEvent, new MouseButtonEventHandler(_OnDeviceUp));
		EventManager.RegisterClassHandler(typeFromHandle, Mouse.QueryCursorEvent, new QueryCursorEventHandler(_OnQueryCursor), handledEventsToo: true);
		_RegisterClipboardHandlers();
		CommandHelpers.RegisterCommandHandler(typeFromHandle, ApplicationCommands.Delete, _OnCommandExecuted, _OnQueryCommandEnabled);
		CommandHelpers.RegisterCommandHandler(typeFromHandle, ApplicationCommands.SelectAll, Key.A, ModifierKeys.Control, _OnCommandExecuted, _OnQueryCommandEnabled);
		CommandHelpers.RegisterCommandHandler(typeFromHandle, DeselectCommand, _OnCommandExecuted, _OnQueryCommandEnabled, KeyGesture.CreateFromResourceStrings("Esc", "InkCanvasDeselectKeyDisplayString"));
		UIElement.ClipToBoundsProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		UIElement.FocusableProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		Style style = new Style(typeFromHandle)
		{
			Setters = 
			{
				(SetterBase)new Setter(BackgroundProperty, new DynamicResourceExtension(SystemColors.WindowBrushKey)),
				(SetterBase)new Setter(Stylus.IsFlicksEnabledProperty, false),
				(SetterBase)new Setter(Stylus.IsTapFeedbackEnabledProperty, false),
				(SetterBase)new Setter(Stylus.IsTouchFeedbackEnabledProperty, false)
			}
		};
		Trigger trigger = new Trigger
		{
			Property = FrameworkElement.WidthProperty,
			Value = double.NaN
		};
		Setter item = new Setter
		{
			Property = FrameworkElement.MinWidthProperty,
			Value = 350.0
		};
		trigger.Setters.Add(item);
		style.Triggers.Add(trigger);
		trigger = new Trigger
		{
			Property = FrameworkElement.HeightProperty,
			Value = double.NaN
		};
		item = new Setter
		{
			Property = FrameworkElement.MinHeightProperty,
			Value = 250.0
		};
		trigger.Setters.Add(item);
		style.Triggers.Add(trigger);
		style.Seal();
		FrameworkElement.StyleProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(style));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(typeof(InkCanvas)));
		FrameworkElement.FocusVisualStyleProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata((object)null));
	}

	/// <summary>Initializes a new instance of the InkCanvas class.  </summary>
	public InkCanvas()
	{
		Initialize();
	}

	private void Initialize()
	{
		_dynamicRenderer = new DynamicRenderer();
		_dynamicRenderer.Enabled = false;
		base.StylusPlugIns.Add(_dynamicRenderer);
		_editingCoordinator = new EditingCoordinator(this);
		_editingCoordinator.UpdateActiveEditingState();
		DefaultDrawingAttributes.AttributeChanged += DefaultDrawingAttributes_Changed;
		InitializeInkObject();
		_rtiHighContrastCallback = new RTIHighContrastCallback(this);
		HighContrastHelper.RegisterHighContrastCallback(_rtiHighContrastCallback);
		if (SystemParameters.HighContrast)
		{
			_rtiHighContrastCallback.TurnHighContrastOn(SystemColors.WindowTextColor);
		}
	}

	private void InitializeInkObject()
	{
		UpdateDynamicRenderer();
		_defaultStylusPointDescription = new StylusPointDescription();
	}

	protected override Size MeasureOverride(Size availableSize)
	{
		if (_localAdornerDecorator == null)
		{
			ApplyTemplate();
		}
		_localAdornerDecorator.Measure(availableSize);
		return _localAdornerDecorator.DesiredSize;
	}

	/// <summary>ArrangeOverride </summary>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		if (_localAdornerDecorator == null)
		{
			ApplyTemplate();
		}
		_localAdornerDecorator.Arrange(new Rect(arrangeSize));
		return arrangeSize;
	}

	/// <summary>Determines whether a given point falls within the rendering bounds of an <see cref="T:System.Windows.Controls.InkCanvas" />. </summary>
	/// <returns>An object that represents the <see cref="T:System.Windows.Media.Visual" /> that is returned from a hit test.</returns>
	/// <param name="hitTestParams">An object that specifies the <see cref="T:System.Windows.Point" /> to hit test against.</param>
	protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParams)
	{
		VerifyAccess();
		Rect rect = new Rect(default(Point), base.RenderSize);
		if (rect.Contains(hitTestParams.HitPoint))
		{
			return new PointHitTestResult(this, hitTestParams.HitPoint);
		}
		return null;
	}

	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (!e.IsAValueChange && !e.IsASubPropertyChange)
		{
			return;
		}
		if (e.Property == UIElement.RenderTransformProperty || e.Property == FrameworkElement.LayoutTransformProperty)
		{
			EditingCoordinator.InvalidateTransform();
			if (e.NewValue is Transform { HasAnimatedProperties: false } transform)
			{
				if (transform is TransformGroup)
				{
					Stack<Transform> stack = new Stack<Transform>();
					stack.Push(transform);
					while (stack.Count > 0)
					{
						Transform transform2 = stack.Pop();
						if (transform2.HasAnimatedProperties)
						{
							return;
						}
						if (transform2 is TransformGroup transformGroup2)
						{
							for (int i = 0; i < transformGroup2.Children.Count; i++)
							{
								stack.Push(transformGroup2.Children[i]);
							}
						}
					}
				}
				_editingCoordinator.InvalidateBehaviorCursor(_editingCoordinator.InkCollectionBehavior);
				EditingCoordinator.UpdatePointEraserCursor();
			}
		}
		if (e.Property == FrameworkElement.FlowDirectionProperty)
		{
			_editingCoordinator.InvalidateBehaviorCursor(_editingCoordinator.InkCollectionBehavior);
		}
	}

	internal override void OnPreApplyTemplate()
	{
		base.OnPreApplyTemplate();
		if (_localAdornerDecorator == null)
		{
			_localAdornerDecorator = new AdornerDecorator();
			InkPresenter inkPresenter = InkPresenter;
			AddVisualChild(_localAdornerDecorator);
			_localAdornerDecorator.Child = inkPresenter;
			inkPresenter.Child = InnerCanvas;
			_localAdornerDecorator.AdornerLayer.Add(SelectionAdorner);
		}
	}

	protected override Visual GetVisualChild(int index)
	{
		if (_localAdornerDecorator == null || index != 0)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return _localAdornerDecorator;
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.InkCanvasAutomationPeer" /> implementation for this control, as part of the WPF infrastructure.</summary>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new InkCanvasAutomationPeer(this);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.InkCanvas.Top" /> attached property for a given dependency object. </summary>
	/// <returns>The top coordinate of the dependency object.</returns>
	/// <param name="element">The element of which to get the top property.</param>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	[AttachedPropertyBrowsableForChildren]
	public static double GetTop(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(TopProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.InkCanvas.Top" /> attached property for a given dependency object. </summary>
	/// <param name="element">The element on which to set the top property.</param>
	/// <param name="length">The top coordinate of <paramref name="element" />.</param>
	public static void SetTop(UIElement element, double length)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(TopProperty, length);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.InkCanvas.Bottom" /> attached property for a given dependency object. </summary>
	/// <returns>The bottom coordinate of the dependency object.</returns>
	/// <param name="element">The element of which to get the bottom property.</param>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	[AttachedPropertyBrowsableForChildren]
	public static double GetBottom(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(BottomProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.InkCanvas.Bottom" /> attached property for a given dependency object. </summary>
	/// <param name="element">The element on which to set the bottom property.</param>
	/// <param name="length">The bottom coordinate of <paramref name="element" />.</param>
	public static void SetBottom(UIElement element, double length)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(BottomProperty, length);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.InkCanvas.Left" /> attached property for a given dependency object. </summary>
	/// <returns>The left coordinate of the dependency object.</returns>
	/// <param name="element">The element of which to get the left property.</param>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	[AttachedPropertyBrowsableForChildren]
	public static double GetLeft(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(LeftProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.InkCanvas.Left" /> attached property for a given dependency object. </summary>
	/// <param name="element">The element on which to set the left property.</param>
	/// <param name="length">The left coordinate of <paramref name="element" />.</param>
	public static void SetLeft(UIElement element, double length)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(LeftProperty, length);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.InkCanvas.Right" /> attached property for a given dependency object. </summary>
	/// <returns>The right coordinate of the dependency object.</returns>
	/// <param name="element">The element of which to get the right property.</param>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	[AttachedPropertyBrowsableForChildren]
	public static double GetRight(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(RightProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.InkCanvas.Right" /> attached property for a given dependency object. </summary>
	/// <param name="element">The element on which to set the right property.</param>
	/// <param name="length">The right coordinate of <paramref name="element" />.</param>
	public static void SetRight(UIElement element, double length)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(RightProperty, length);
	}

	private static void OnPositioningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement reference && VisualTreeHelper.GetParent(reference) is InkCanvasInnerCanvas inkCanvasInnerCanvas)
		{
			if (e.Property == LeftProperty || e.Property == TopProperty)
			{
				inkCanvasInnerCanvas.InvalidateMeasure();
			}
			else
			{
				inkCanvasInnerCanvas.InvalidateArrange();
			}
		}
	}

	private static void OnStrokesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		InkCanvas inkCanvas = (InkCanvas)d;
		StrokeCollection strokeCollection = (StrokeCollection)e.OldValue;
		StrokeCollection strokeCollection2 = (StrokeCollection)e.NewValue;
		if (strokeCollection != strokeCollection2)
		{
			inkCanvas.CoreChangeSelection(new StrokeCollection(), inkCanvas.InkCanvasSelection.SelectedElements, raiseSelectionChanged: false);
			inkCanvas.InitializeInkObject();
			InkCanvasStrokesReplacedEventArgs e2 = new InkCanvasStrokesReplacedEventArgs(strokeCollection2, strokeCollection);
			inkCanvas.OnStrokesReplaced(e2);
		}
	}

	private static void OnDefaultDrawingAttributesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		InkCanvas inkCanvas = (InkCanvas)d;
		DrawingAttributes drawingAttributes = (DrawingAttributes)e.OldValue;
		DrawingAttributes drawingAttributes2 = (DrawingAttributes)e.NewValue;
		inkCanvas.UpdateDynamicRenderer(drawingAttributes2);
		if ((object)drawingAttributes != drawingAttributes2)
		{
			drawingAttributes.AttributeChanged -= inkCanvas.DefaultDrawingAttributes_Changed;
			DrawingAttributesReplacedEventArgs e2 = new DrawingAttributesReplacedEventArgs(drawingAttributes2, drawingAttributes);
			drawingAttributes2.AttributeChanged += inkCanvas.DefaultDrawingAttributes_Changed;
			inkCanvas.RaiseDefaultDrawingAttributeReplaced(e2);
		}
	}

	private static void OnEditingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((InkCanvas)d).RaiseEditingModeChanged(new RoutedEventArgs(EditingModeChangedEvent, d));
	}

	private static void OnEditingModeInvertedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((InkCanvas)d).RaiseEditingModeInvertedChanged(new RoutedEventArgs(EditingModeInvertedChangedEvent, d));
	}

	private static bool ValidateEditingMode(object value)
	{
		return EditingModeHelper.IsDefined((InkCanvasEditingMode)value);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.InkCanvas.StrokeCollected" /> event. </summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		RaiseEvent(e);
	}

	internal void RaiseGestureOrStrokeCollected(InkCanvasStrokeCollectedEventArgs e, bool userInitiated)
	{
		bool flag = true;
		try
		{
			if (userInitiated && (ActiveEditingMode == InkCanvasEditingMode.InkAndGesture || ActiveEditingMode == InkCanvasEditingMode.GestureOnly) && GestureRecognizer.IsRecognizerAvailable)
			{
				StrokeCollection strokeCollection = new StrokeCollection();
				strokeCollection.Add(e.Stroke);
				ReadOnlyCollection<GestureRecognitionResult> readOnlyCollection = GestureRecognizer.CriticalRecognize(strokeCollection);
				if (readOnlyCollection.Count > 0)
				{
					InkCanvasGestureEventArgs inkCanvasGestureEventArgs = new InkCanvasGestureEventArgs(strokeCollection, readOnlyCollection);
					if (readOnlyCollection[0].ApplicationGesture == ApplicationGesture.NoGesture)
					{
						inkCanvasGestureEventArgs.Cancel = true;
					}
					else
					{
						inkCanvasGestureEventArgs.Cancel = false;
					}
					OnGesture(inkCanvasGestureEventArgs);
					if (!inkCanvasGestureEventArgs.Cancel)
					{
						flag = false;
						return;
					}
				}
			}
			flag = false;
			if (ActiveEditingMode == InkCanvasEditingMode.Ink || ActiveEditingMode == InkCanvasEditingMode.InkAndGesture)
			{
				Strokes.Add(e.Stroke);
				OnStrokeCollected(e);
			}
		}
		finally
		{
			if (flag)
			{
				Strokes.Add(e.Stroke);
			}
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.InkCanvas.Gesture" /> event.</summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnGesture(InkCanvasGestureEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		RaiseEvent(e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.InkCanvas.StrokesReplaced" /> event. </summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnStrokesReplaced(InkCanvasStrokesReplacedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (this.StrokesReplaced != null)
		{
			this.StrokesReplaced(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.InkCanvas.DefaultDrawingAttributesReplaced" /> event. </summary>
	/// <param name="e">The event data. </param>
	protected virtual void OnDefaultDrawingAttributesReplaced(DrawingAttributesReplacedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (this.DefaultDrawingAttributesReplaced != null)
		{
			this.DefaultDrawingAttributesReplaced(this, e);
		}
	}

	private void RaiseDefaultDrawingAttributeReplaced(DrawingAttributesReplacedEventArgs e)
	{
		OnDefaultDrawingAttributesReplaced(e);
		_editingCoordinator.InvalidateBehaviorCursor(_editingCoordinator.InkCollectionBehavior);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.InkCanvas.ActiveEditingModeChanged" /> event.</summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnActiveEditingModeChanged(RoutedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		RaiseEvent(e);
	}

	internal void RaiseActiveEditingModeChanged(RoutedEventArgs e)
	{
		if (ActiveEditingMode != _editingCoordinator.ActiveEditingMode)
		{
			SetValue(ActiveEditingModePropertyKey, _editingCoordinator.ActiveEditingMode);
			OnActiveEditingModeChanged(e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.InkCanvas.EditingModeChanged" /> event. </summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnEditingModeChanged(RoutedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		RaiseEvent(e);
	}

	private void RaiseEditingModeChanged(RoutedEventArgs e)
	{
		_editingCoordinator.UpdateEditingState(inverted: false);
		OnEditingModeChanged(e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.InkCanvas.EditingModeInvertedChanged" /> event. </summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnEditingModeInvertedChanged(RoutedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		RaiseEvent(e);
	}

	private void RaiseEditingModeInvertedChanged(RoutedEventArgs e)
	{
		_editingCoordinator.UpdateEditingState(inverted: true);
		OnEditingModeInvertedChanged(e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.InkCanvas.SelectionMoving" /> event. </summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnSelectionMoving(InkCanvasSelectionEditingEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (this.SelectionMoving != null)
		{
			this.SelectionMoving(this, e);
		}
	}

	internal void RaiseSelectionMoving(InkCanvasSelectionEditingEventArgs e)
	{
		OnSelectionMoving(e);
	}

	/// <summary>An event announcing that the user selected and moved a selection of strokes and/or elements. </summary>
	/// <param name="e">Not used.</param>
	protected virtual void OnSelectionMoved(EventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (this.SelectionMoved != null)
		{
			this.SelectionMoved(this, e);
		}
	}

	internal void RaiseSelectionMoved(EventArgs e)
	{
		OnSelectionMoved(e);
		EditingCoordinator.SelectionEditor.OnInkCanvasSelectionChanged();
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.InkCanvas.StrokeErasing" /> event. </summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnStrokeErasing(InkCanvasStrokeErasingEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (this.StrokeErasing != null)
		{
			this.StrokeErasing(this, e);
		}
	}

	internal void RaiseStrokeErasing(InkCanvasStrokeErasingEventArgs e)
	{
		OnStrokeErasing(e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.InkCanvas.StrokeErased" /> event. </summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnStrokeErased(RoutedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		RaiseEvent(e);
	}

	internal void RaiseInkErased()
	{
		OnStrokeErased(new RoutedEventArgs(StrokeErasedEvent, this));
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.InkCanvas.SelectionResizing" /> event. </summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnSelectionResizing(InkCanvasSelectionEditingEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (this.SelectionResizing != null)
		{
			this.SelectionResizing(this, e);
		}
	}

	internal void RaiseSelectionResizing(InkCanvasSelectionEditingEventArgs e)
	{
		OnSelectionResizing(e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.InkCanvas.SelectionResized" /> event.</summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnSelectionResized(EventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (this.SelectionResized != null)
		{
			this.SelectionResized(this, e);
		}
	}

	internal void RaiseSelectionResized(EventArgs e)
	{
		OnSelectionResized(e);
		EditingCoordinator.SelectionEditor.OnInkCanvasSelectionChanged();
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.InkCanvas.SelectionChanging" /> event.</summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnSelectionChanging(InkCanvasSelectionChangingEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (this.SelectionChanging != null)
		{
			this.SelectionChanging(this, e);
		}
	}

	private void RaiseSelectionChanging(InkCanvasSelectionChangingEventArgs e)
	{
		OnSelectionChanging(e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.InkCanvas.SelectionChanged" /> event.</summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnSelectionChanged(EventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (this.SelectionChanged != null)
		{
			this.SelectionChanged(this, e);
		}
	}

	internal void RaiseSelectionChanged(EventArgs e)
	{
		OnSelectionChanged(e);
		EditingCoordinator.SelectionEditor.OnInkCanvasSelectionChanged();
	}

	internal void RaiseOnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
	{
		OnVisualChildrenChanged(visualAdded, visualRemoved);
	}

	/// <summary>Returns a collection of application gestures that are recognized by <see cref="T:System.Windows.Controls.InkCanvas" />. </summary>
	/// <returns>A collection of gestures that the <see cref="T:System.Windows.Controls.InkCanvas" /> recognizes. </returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.InkCanvas.IsGestureRecognizerAvailable" /> property is false.</exception>
	public ReadOnlyCollection<ApplicationGesture> GetEnabledGestures()
	{
		return new ReadOnlyCollection<ApplicationGesture>(GestureRecognizer.GetEnabledGestures());
	}

	/// <summary>Sets the application gestures that the <see cref="T:System.Windows.Controls.InkCanvas" /> will recognize.</summary>
	/// <param name="applicationGestures">A collection of application gestures that the <see cref="T:System.Windows.Controls.InkCanvas" /> will recognize.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.InkCanvas.IsGestureRecognizerAvailable" /> property is false.</exception>
	public void SetEnabledGestures(IEnumerable<ApplicationGesture> applicationGestures)
	{
		GestureRecognizer.SetEnabledGestures(applicationGestures);
	}

	/// <summary>Gets the bounds of the selected strokes and elements on the <see cref="T:System.Windows.Controls.InkCanvas" />. </summary>
	/// <returns>The smallest rectangle that encompasses all selected strokes and elements.</returns>
	public Rect GetSelectionBounds()
	{
		VerifyAccess();
		return InkCanvasSelection.SelectionBounds;
	}

	/// <summary>Retrieves the <see cref="T:System.Windows.FrameworkElement" /> objects that are selected in the <see cref="T:System.Windows.Controls.InkCanvas" />. </summary>
	/// <returns>Array of <see cref="T:System.Windows.FrameworkElement" /> objects.</returns>
	public ReadOnlyCollection<UIElement> GetSelectedElements()
	{
		VerifyAccess();
		return InkCanvasSelection.SelectedElements;
	}

	/// <summary>Retrieves a <see cref="T:System.Windows.Ink.StrokeCollection" /> that represents selected <see cref="T:System.Windows.Ink.Stroke" /> objects on the <see cref="T:System.Windows.Controls.InkCanvas" />. </summary>
	/// <returns>The collection of selected strokes.</returns>
	public StrokeCollection GetSelectedStrokes()
	{
		VerifyAccess();
		return new StrokeCollection { InkCanvasSelection.SelectedStrokes };
	}

	/// <summary>Selects a set of ink <see cref="T:System.Windows.Ink.Stroke" /> objects. </summary>
	/// <param name="selectedStrokes">A collection of <see cref="T:System.Windows.Ink.Stroke" /> objects to select.</param>
	/// <exception cref="T:System.ArgumentException">One or more strokes in <paramref name="selectedStrokes" /> is not in the <see cref="P:System.Windows.Controls.InkCanvas.Strokes" /> property.</exception>
	public void Select(StrokeCollection selectedStrokes)
	{
		Select(selectedStrokes, null);
	}

	/// <summary>Selects a set of <see cref="T:System.Windows.UIElement" /> objects. </summary>
	/// <param name="selectedElements">A collection of <see cref="T:System.Windows.UIElement" /> objects to select.</param>
	public void Select(IEnumerable<UIElement> selectedElements)
	{
		Select(null, selectedElements);
	}

	/// <summary>Selects a combination of <see cref="T:System.Windows.Ink.Stroke" /> and <see cref="T:System.Windows.UIElement" /> objects.</summary>
	/// <param name="selectedStrokes">A collection of <see cref="T:System.Windows.Ink.Stroke" /> objects to select.</param>
	/// <param name="selectedElements">A collection of <see cref="T:System.Windows.UIElement" /> objects to select.</param>
	/// <exception cref="T:System.ArgumentException">One or more strokes in <paramref name="selectedStrokes" /> is not included in the <see cref="P:System.Windows.Controls.InkCanvas.Strokes" /> property.</exception>
	public void Select(StrokeCollection selectedStrokes, IEnumerable<UIElement> selectedElements)
	{
		VerifyAccess();
		if (EnsureActiveEditingMode(InkCanvasEditingMode.Select))
		{
			UIElement[] elements = ValidateSelectedElements(selectedElements);
			StrokeCollection strokes = ValidateSelectedStrokes(selectedStrokes);
			ChangeInkCanvasSelection(strokes, elements);
		}
	}

	/// <summary>Returns a value that indicates which part of the selection adorner intersects or surrounds the specified point.</summary>
	/// <returns>A value that indicates which part of the selection adorner intersects or surrounds a specified point.</returns>
	/// <param name="point">The point to hit test.</param>
	public InkCanvasSelectionHitResult HitTestSelection(Point point)
	{
		VerifyAccess();
		if (_localAdornerDecorator == null)
		{
			ApplyTemplate();
		}
		return InkCanvasSelection.HitTestSelection(point);
	}

	/// <summary>Copies selected strokes and/or elements to the Clipboard. </summary>
	public void CopySelection()
	{
		VerifyAccess();
		PrivateCopySelection();
	}

	/// <summary>Deletes the selected strokes and elements, and copies them to the Clipboard.</summary>
	public void CutSelection()
	{
		VerifyAccess();
		InkCanvasClipboardDataFormats inkCanvasClipboardDataFormats = PrivateCopySelection();
		if (inkCanvasClipboardDataFormats != 0)
		{
			DeleteCurrentSelection((inkCanvasClipboardDataFormats & (InkCanvasClipboardDataFormats.XAML | InkCanvasClipboardDataFormats.ISF)) != 0, (inkCanvasClipboardDataFormats & InkCanvasClipboardDataFormats.XAML) != 0);
		}
	}

	/// <summary>Pastes the contents of the Clipboard to the top-left corner of the <see cref="T:System.Windows.Controls.InkCanvas" />. </summary>
	public void Paste()
	{
		Paste(new Point(0.0, 0.0));
	}

	/// <summary>Pastes the contents of the Clipboard to the <see cref="T:System.Windows.Controls.InkCanvas" /> at a given point. </summary>
	/// <param name="point">The point at which to paste the strokes.</param>
	public void Paste(Point point)
	{
		VerifyAccess();
		if (double.IsNaN(point.X) || double.IsNaN(point.Y) || double.IsInfinity(point.X) || double.IsInfinity(point.Y))
		{
			throw new ArgumentException(SR.InvalidPoint, "point");
		}
		if (!_editingCoordinator.UserIsEditing)
		{
			IDataObject dataObject = null;
			try
			{
				dataObject = Clipboard.GetDataObject();
			}
			catch (ExternalException)
			{
				return;
			}
			if (dataObject != null)
			{
				PasteFromDataObject(dataObject, point);
			}
		}
	}

	/// <summary>Indicates whether the contents of the Clipboard can be pasted into the <see cref="T:System.Windows.Controls.InkCanvas" />. </summary>
	/// <returns>true if the contents of the Clipboard can be pasted in; otherwise, false. </returns>
	public bool CanPaste()
	{
		VerifyAccess();
		if (_editingCoordinator.UserIsEditing)
		{
			return false;
		}
		return PrivateCanPaste();
	}

	/// <summary>Adds the specified object to the <see cref="T:System.Windows.Controls.InkCanvas" />. </summary>
	/// <param name="value">The child object to add.</param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		((IAddChild)InnerCanvas).AddChild(value);
	}

	/// <summary>Adds the text that within the tags in markup. Always throws an <see cref="T:System.ArgumentException" />.</summary>
	/// <param name="textData">Not used.</param>
	void IAddChild.AddText(string textData)
	{
		((IAddChild)InnerCanvas).AddText(textData);
	}

	private bool UserInitiatedCanPaste()
	{
		return PrivateCanPaste();
	}

	private bool PrivateCanPaste()
	{
		bool result = false;
		IDataObject dataObject = null;
		try
		{
			dataObject = Clipboard.GetDataObject();
		}
		catch (ExternalException)
		{
			return false;
		}
		if (dataObject != null)
		{
			result = ClipboardProcessor.CheckDataFormats(dataObject);
		}
		return result;
	}

	internal void PasteFromDataObject(IDataObject dataObj, Point point)
	{
		ClearSelection(raiseSelectionChangedEvent: false);
		StrokeCollection newStrokes = new StrokeCollection();
		List<UIElement> newElements = new List<UIElement>();
		if (!ClipboardProcessor.PasteData(dataObj, ref newStrokes, ref newElements) || (newStrokes.Count == 0 && newElements.Count == 0))
		{
			return;
		}
		UIElementCollection children = Children;
		foreach (UIElement item in newElements)
		{
			children.Add(item);
		}
		if (newStrokes != null)
		{
			Strokes.Add(newStrokes);
		}
		try
		{
			CoreChangeSelection(newStrokes, newElements.ToArray(), EditingMode == InkCanvasEditingMode.Select);
		}
		finally
		{
			Rect selectionBounds = GetSelectionBounds();
			InkCanvasSelection.CommitChanges(Rect.Offset(selectionBounds, 0.0 - selectionBounds.Left + point.X, 0.0 - selectionBounds.Top + point.Y), raiseEvent: false);
			if (EditingMode != InkCanvasEditingMode.Select)
			{
				ClearSelection(raiseSelectionChangedEvent: false);
			}
		}
	}

	private InkCanvasClipboardDataFormats CopyToDataObject()
	{
		DataObject dataObject = new DataObject();
		InkCanvasClipboardDataFormats num = ClipboardProcessor.CopySelectedData(dataObject);
		if (num != 0)
		{
			Clipboard.SetDataObject(dataObject, copy: true);
		}
		return num;
	}

	internal void BeginDynamicSelection(Visual visual)
	{
		_dynamicallySelectedStrokes = new StrokeCollection();
		InkPresenter.AttachVisuals(visual, new DrawingAttributes());
	}

	internal void UpdateDynamicSelection(StrokeCollection strokesToDynamicallySelect, StrokeCollection strokesToDynamicallyUnselect)
	{
		if (strokesToDynamicallySelect != null)
		{
			foreach (Stroke item in strokesToDynamicallySelect)
			{
				_dynamicallySelectedStrokes.Add(item);
				item.IsSelected = true;
			}
		}
		if (strokesToDynamicallyUnselect == null)
		{
			return;
		}
		foreach (Stroke item2 in strokesToDynamicallyUnselect)
		{
			_dynamicallySelectedStrokes.Remove(item2);
			item2.IsSelected = false;
		}
	}

	internal StrokeCollection EndDynamicSelection(Visual visual)
	{
		InkPresenter.DetachVisuals(visual);
		StrokeCollection dynamicallySelectedStrokes = _dynamicallySelectedStrokes;
		_dynamicallySelectedStrokes = null;
		return dynamicallySelectedStrokes;
	}

	internal bool ClearSelectionRaiseSelectionChanging()
	{
		if (!InkCanvasSelection.HasSelection)
		{
			return true;
		}
		ChangeInkCanvasSelection(new StrokeCollection(), new UIElement[0]);
		return !InkCanvasSelection.HasSelection;
	}

	internal void ClearSelection(bool raiseSelectionChangedEvent)
	{
		if (InkCanvasSelection.HasSelection)
		{
			CoreChangeSelection(new StrokeCollection(), new UIElement[0], raiseSelectionChangedEvent);
		}
	}

	internal void ChangeInkCanvasSelection(StrokeCollection strokes, UIElement[] elements)
	{
		InkCanvasSelection.SelectionIsDifferentThanCurrent(strokes, out var strokesAreDifferent, elements, out var elementsAreDifferent);
		if (!(strokesAreDifferent || elementsAreDifferent))
		{
			return;
		}
		InkCanvasSelectionChangingEventArgs inkCanvasSelectionChangingEventArgs = new InkCanvasSelectionChangingEventArgs(strokes, elements);
		StrokeCollection strokeCollection = strokes;
		UIElement[] validElements = elements;
		RaiseSelectionChanging(inkCanvasSelectionChangingEventArgs);
		if (!inkCanvasSelectionChangingEventArgs.Cancel)
		{
			if (inkCanvasSelectionChangingEventArgs.StrokesChanged)
			{
				strokeCollection = ValidateSelectedStrokes(inkCanvasSelectionChangingEventArgs.GetSelectedStrokes());
				int count = strokes.Count;
				for (int i = 0; i < count; i++)
				{
					if (!strokeCollection.Contains(strokes[i]))
					{
						strokes[i].IsSelected = false;
					}
				}
			}
			if (inkCanvasSelectionChangingEventArgs.ElementsChanged)
			{
				validElements = ValidateSelectedElements(inkCanvasSelectionChangingEventArgs.GetSelectedElements());
			}
			CoreChangeSelection(strokeCollection, validElements, raiseSelectionChanged: true);
			return;
		}
		StrokeCollection selectedStrokes = InkCanvasSelection.SelectedStrokes;
		int count2 = strokes.Count;
		for (int j = 0; j < count2; j++)
		{
			if (!selectedStrokes.Contains(strokes[j]))
			{
				strokes[j].IsSelected = false;
			}
		}
	}

	private void CoreChangeSelection(StrokeCollection validStrokes, IList<UIElement> validElements, bool raiseSelectionChanged)
	{
		InkCanvasSelection.Select(validStrokes, validElements, raiseSelectionChanged);
	}

	internal static StrokeCollection GetValidStrokes(StrokeCollection subset, StrokeCollection superset)
	{
		StrokeCollection strokeCollection = new StrokeCollection();
		int count = subset.Count;
		if (count == 0)
		{
			return strokeCollection;
		}
		for (int i = 0; i < count; i++)
		{
			Stroke item = subset[i];
			if (superset.Contains(item))
			{
				strokeCollection.Add(item);
			}
		}
		return strokeCollection;
	}

	private static void _RegisterClipboardHandlers()
	{
		Type? typeFromHandle = typeof(InkCanvas);
		CommandHelpers.RegisterCommandHandler(typeFromHandle, ApplicationCommands.Cut, _OnCommandExecuted, _OnQueryCommandEnabled, KeyGesture.CreateFromResourceStrings("Shift+Delete", "KeyShiftDeleteDisplayString"));
		CommandHelpers.RegisterCommandHandler(typeFromHandle, ApplicationCommands.Copy, _OnCommandExecuted, _OnQueryCommandEnabled, KeyGesture.CreateFromResourceStrings("Ctrl+Insert", "KeyCtrlInsertDisplayString"));
		CommandHelpers.RegisterCommandHandler(executedRoutedEventHandler: _OnCommandExecuted, canExecuteRoutedEventHandler: _OnQueryCommandEnabled, inputGesture: KeyGesture.CreateFromResourceStrings("Shift+Insert", SR.KeyShiftInsertDisplayString), controlType: typeFromHandle, command: ApplicationCommands.Paste);
	}

	private StrokeCollection ValidateSelectedStrokes(StrokeCollection strokes)
	{
		if (strokes == null)
		{
			return new StrokeCollection();
		}
		return GetValidStrokes(strokes, Strokes);
	}

	private UIElement[] ValidateSelectedElements(IEnumerable<UIElement> selectedElements)
	{
		if (selectedElements == null)
		{
			return new UIElement[0];
		}
		List<UIElement> list = new List<UIElement>();
		foreach (UIElement selectedElement in selectedElements)
		{
			if (!list.Contains(selectedElement) && InkCanvasIsAncestorOf(selectedElement))
			{
				list.Add(selectedElement);
			}
		}
		return list.ToArray();
	}

	private bool InkCanvasIsAncestorOf(UIElement element)
	{
		if (this != element && IsAncestorOf(element))
		{
			return true;
		}
		return false;
	}

	private void DefaultDrawingAttributes_Changed(object sender, PropertyDataChangedEventArgs args)
	{
		InvalidateSubProperty(DefaultDrawingAttributesProperty);
		UpdateDynamicRenderer();
		_editingCoordinator.InvalidateBehaviorCursor(_editingCoordinator.InkCollectionBehavior);
	}

	internal void UpdateDynamicRenderer()
	{
		UpdateDynamicRenderer(DefaultDrawingAttributes);
	}

	private void UpdateDynamicRenderer(DrawingAttributes newDrawingAttributes)
	{
		ApplyTemplate();
		if (DynamicRenderer == null)
		{
			return;
		}
		DynamicRenderer.DrawingAttributes = newDrawingAttributes;
		if (!InkPresenter.AttachedVisualIsPositionedCorrectly(DynamicRenderer.RootVisual, newDrawingAttributes))
		{
			if (InkPresenter.ContainsAttachedVisual(DynamicRenderer.RootVisual))
			{
				InkPresenter.DetachVisuals(DynamicRenderer.RootVisual);
			}
			if (DynamicRenderer.Enabled && DynamicRenderer.RootVisual != null)
			{
				InkPresenter.AttachVisuals(DynamicRenderer.RootVisual, newDrawingAttributes);
			}
		}
	}

	private bool EnsureActiveEditingMode(InkCanvasEditingMode newEditingMode)
	{
		bool result = true;
		if (ActiveEditingMode != newEditingMode)
		{
			if (EditingCoordinator.IsStylusInverted)
			{
				EditingModeInverted = newEditingMode;
			}
			else
			{
				EditingMode = newEditingMode;
			}
			result = ActiveEditingMode == newEditingMode;
		}
		return result;
	}

	private void DeleteCurrentSelection(bool removeSelectedStrokes, bool removeSelectedElements)
	{
		StrokeCollection selectedStrokes = GetSelectedStrokes();
		IList<UIElement> selectedElements = GetSelectedElements();
		StrokeCollection validStrokes = (removeSelectedStrokes ? new StrokeCollection() : selectedStrokes);
		IList<UIElement> validElements;
		if (!removeSelectedElements)
		{
			validElements = selectedElements;
		}
		else
		{
			IList<UIElement> list = new List<UIElement>();
			validElements = list;
		}
		CoreChangeSelection(validStrokes, validElements, raiseSelectionChanged: true);
		if (removeSelectedStrokes && selectedStrokes != null && selectedStrokes.Count != 0)
		{
			Strokes.Remove(selectedStrokes);
		}
		if (!removeSelectedElements)
		{
			return;
		}
		UIElementCollection children = Children;
		foreach (UIElement item in selectedElements)
		{
			children.Remove(item);
		}
	}

	private static void _OnCommandExecuted(object sender, ExecutedRoutedEventArgs args)
	{
		ICommand command = args.Command;
		InkCanvas inkCanvas = sender as InkCanvas;
		if (!inkCanvas.IsEnabled || inkCanvas.EditingCoordinator.UserIsEditing)
		{
			return;
		}
		if (command == ApplicationCommands.Delete)
		{
			inkCanvas.DeleteCurrentSelection(removeSelectedStrokes: true, removeSelectedElements: true);
			return;
		}
		if (command == ApplicationCommands.Cut)
		{
			inkCanvas.CutSelection();
			return;
		}
		if (command == ApplicationCommands.Copy)
		{
			inkCanvas.CopySelection();
			return;
		}
		if (command == ApplicationCommands.SelectAll)
		{
			if (inkCanvas.ActiveEditingMode != InkCanvasEditingMode.Select)
			{
				return;
			}
			IEnumerable<UIElement> selectedElements = null;
			UIElementCollection children = inkCanvas.Children;
			if (children.Count > 0)
			{
				UIElement[] array = new UIElement[children.Count];
				for (int i = 0; i < children.Count; i++)
				{
					array[i] = children[i];
				}
				selectedElements = array;
			}
			inkCanvas.Select(inkCanvas.Strokes, selectedElements);
			return;
		}
		if (command == ApplicationCommands.Paste)
		{
			try
			{
				inkCanvas.Paste();
				return;
			}
			catch (COMException)
			{
				return;
			}
			catch (XamlParseException)
			{
				return;
			}
			catch (ArgumentException)
			{
				return;
			}
		}
		if (command == DeselectCommand)
		{
			inkCanvas.ClearSelectionRaiseSelectionChanging();
		}
	}

	private static void _OnQueryCommandEnabled(object sender, CanExecuteRoutedEventArgs args)
	{
		RoutedCommand routedCommand = (RoutedCommand)args.Command;
		InkCanvas inkCanvas = sender as InkCanvas;
		if (inkCanvas.IsEnabled && !inkCanvas.EditingCoordinator.UserIsEditing)
		{
			if (routedCommand == ApplicationCommands.Delete || routedCommand == ApplicationCommands.Cut || routedCommand == ApplicationCommands.Copy || routedCommand == DeselectCommand)
			{
				args.CanExecute = inkCanvas.InkCanvasSelection.HasSelection;
			}
			else if (routedCommand == ApplicationCommands.Paste)
			{
				try
				{
					args.CanExecute = (args.UserInitiated ? inkCanvas.UserInitiatedCanPaste() : inkCanvas.CanPaste());
				}
				catch (COMException)
				{
					args.CanExecute = false;
				}
			}
			else if (routedCommand == ApplicationCommands.SelectAll)
			{
				args.CanExecute = inkCanvas.ActiveEditingMode == InkCanvasEditingMode.Select && (inkCanvas.Strokes.Count > 0 || inkCanvas.Children.Count > 0);
			}
		}
		else
		{
			args.CanExecute = false;
		}
		if (routedCommand == ApplicationCommands.Cut || routedCommand == ApplicationCommands.Copy || routedCommand == ApplicationCommands.Paste)
		{
			args.Handled = true;
		}
	}

	private InkCanvasClipboardDataFormats PrivateCopySelection()
	{
		InkCanvasClipboardDataFormats result = InkCanvasClipboardDataFormats.None;
		if (InkCanvasSelection.HasSelection && !_editingCoordinator.UserIsEditing)
		{
			result = CopyToDataObject();
		}
		return result;
	}

	private static void _OnDeviceDown<TEventArgs>(object sender, TEventArgs e) where TEventArgs : InputEventArgs
	{
		((InkCanvas)sender).EditingCoordinator.OnInkCanvasDeviceDown(sender, e);
	}

	private static void _OnDeviceUp<TEventArgs>(object sender, TEventArgs e) where TEventArgs : InputEventArgs
	{
		((InkCanvas)sender).EditingCoordinator.OnInkCanvasDeviceUp(sender, e);
	}

	private static void _OnQueryCursor(object sender, QueryCursorEventArgs e)
	{
		InkCanvas inkCanvas = (InkCanvas)sender;
		if (!inkCanvas.UseCustomCursor && (!e.Handled || inkCanvas.ForceCursor))
		{
			Cursor activeBehaviorCursor = inkCanvas.EditingCoordinator.GetActiveBehaviorCursor();
			if (activeBehaviorCursor != null)
			{
				e.Cursor = activeBehaviorCursor;
				e.Handled = true;
			}
		}
	}

	internal void UpdateCursor()
	{
		if (base.IsMouseOver)
		{
			Mouse.UpdateCursor();
		}
	}
}
