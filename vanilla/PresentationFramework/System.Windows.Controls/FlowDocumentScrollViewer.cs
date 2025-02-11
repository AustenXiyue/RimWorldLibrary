using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Printing;
using System.Windows.Annotations;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Xps;
using MS.Internal;
using MS.Internal.Annotations.Anchoring;
using MS.Internal.AppModel;
using MS.Internal.Commands;
using MS.Internal.Controls;
using MS.Internal.Documents;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Provides a control for viewing flow content in a continuous scrolling mode.</summary>
[TemplatePart(Name = "PART_ContentHost", Type = typeof(ScrollViewer))]
[TemplatePart(Name = "PART_FindToolBarHost", Type = typeof(Decorator))]
[TemplatePart(Name = "PART_ToolBarHost", Type = typeof(Decorator))]
[ContentProperty("Document")]
public class FlowDocumentScrollViewer : Control, IAddChild, IServiceProvider, IJournalState
{
	[Serializable]
	private class JournalState : CustomJournalStateInternal
	{
		public int ContentPosition;

		public LogicalDirection ContentPositionDirection;

		public double Zoom;

		public JournalState(int contentPosition, LogicalDirection contentPositionDirection, double zoom)
		{
			ContentPosition = contentPosition;
			ContentPositionDirection = contentPositionDirection;
			Zoom = zoom;
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.Document" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.Document" /> dependency property.</returns>
	public static readonly DependencyProperty DocumentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.Zoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.Zoom" /> dependency property.</returns>
	public static readonly DependencyProperty ZoomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.MaxZoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.MaxZoom" /> dependency property.</returns>
	public static readonly DependencyProperty MaxZoomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.MinZoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.MinZoom" /> dependency property.</returns>
	public static readonly DependencyProperty MinZoomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.ZoomIncrement" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.ZoomIncrement" /> dependency property.</returns>
	public static readonly DependencyProperty ZoomIncrementProperty;

	private static readonly DependencyPropertyKey CanIncreaseZoomPropertyKey;

	/// <summary>Identifies the <see cref="F:System.Windows.Controls.FlowDocumentScrollViewer.CanDecreaseZoomProperty" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.CanIncreaseZoom" /> dependency property.</returns>
	public static readonly DependencyProperty CanIncreaseZoomProperty;

	private static readonly DependencyPropertyKey CanDecreaseZoomPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.CanDecreaseZoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.CanDecreaseZoom" /> dependency property.</returns>
	public static readonly DependencyProperty CanDecreaseZoomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.IsSelectionEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.IsSelectionEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsSelectionEnabledProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.IsToolBarVisible" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.IsToolBarVisible" /> dependency property.</returns>
	public static readonly DependencyProperty IsToolBarVisibleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.HorizontalScrollBarVisibility" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.HorizontalScrollBarVisibility" /> dependency property.</returns>
	public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.VerticalScrollBarVisibility" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.VerticalScrollBarVisibility" /> dependency property.</returns>
	public static readonly DependencyProperty VerticalScrollBarVisibilityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.SelectionBrush" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.SelectionBrush" /> dependency property.</returns>
	public static readonly DependencyProperty SelectionBrushProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.SelectionOpacity" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.SelectionOpacity" /> dependency property.</returns>
	public static readonly DependencyProperty SelectionOpacityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.IsSelectionActive" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.IsSelectionActive" /> dependency property.</returns>
	public static readonly DependencyProperty IsSelectionActiveProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.IsInactiveSelectionHighlightEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.IsInactiveSelectionHighlightEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsInactiveSelectionHighlightEnabledProperty;

	private TextEditor _textEditor;

	private Decorator _findToolBarHost;

	private Decorator _toolBarHost;

	private ScrollViewer _contentHost;

	private bool _documentAsLogicalChild;

	private FlowDocumentPrintingState _printingState;

	private const string _contentHostTemplateName = "PART_ContentHost";

	private const string _findToolBarHostTemplateName = "PART_FindToolBarHost";

	private const string _toolBarHostTemplateName = "PART_ToolBarHost";

	private static bool IsEditingEnabled;

	private static RoutedUICommand _commandLineDown;

	private static RoutedUICommand _commandLineUp;

	private static RoutedUICommand _commandLineLeft;

	private static RoutedUICommand _commandLineRight;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets a <see cref="T:System.Windows.Documents.FlowDocument" /> that hosts the content to be displayed by the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" />.  </summary>
	/// <returns>A <see cref="T:System.Windows.Documents.FlowDocument" /> that hosts the content to be displayed by the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" />. The default is null.</returns>
	public FlowDocument Document
	{
		get
		{
			return (FlowDocument)GetValue(DocumentProperty);
		}
		set
		{
			SetValue(DocumentProperty, value);
		}
	}

	/// <summary>Gets the selected content of the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" />.</summary>
	/// <returns>The selected content of the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" />.</returns>
	public TextSelection Selection
	{
		get
		{
			ITextSelection textSelection = null;
			FlowDocument document = Document;
			if (document != null)
			{
				textSelection = document.StructuralCache.TextContainer.TextSelection;
			}
			return textSelection as TextSelection;
		}
	}

	/// <summary>Gets or sets the current zoom level.  </summary>
	/// <returns>The current zoom level, interpreted as a percentage. The default is 100.0 (a zoom level of 100%).</returns>
	public double Zoom
	{
		get
		{
			return (double)GetValue(ZoomProperty);
		}
		set
		{
			SetValue(ZoomProperty, value);
		}
	}

	/// <summary>Gets or sets the maximum allowable <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.Zoom" /> level for the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" />.  </summary>
	/// <returns>The maximum allowable <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.Zoom" /> level for the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" />, interpreted as a percentage. The default is 200.0 (a maximum zoom of 200%).</returns>
	public double MaxZoom
	{
		get
		{
			return (double)GetValue(MaxZoomProperty);
		}
		set
		{
			SetValue(MaxZoomProperty, value);
		}
	}

	/// <summary>Gets or sets the minimum allowable <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.Zoom" /> level for the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" />.  </summary>
	/// <returns>The minimum allowable <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.Zoom" /> level for the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" />, interpreted as a percentage. The default is 80.0 (a minimum zoom of 80%).</returns>
	public double MinZoom
	{
		get
		{
			return (double)GetValue(MinZoomProperty);
		}
		set
		{
			SetValue(MinZoomProperty, value);
		}
	}

	/// <summary>Gets or sets the zoom increment.  </summary>
	/// <returns>The current zoom increment, interpreted as a percentage. The default is 10.0 (zoom increments by 10%).</returns>
	public double ZoomIncrement
	{
		get
		{
			return (double)GetValue(ZoomIncrementProperty);
		}
		set
		{
			SetValue(ZoomIncrementProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.Zoom" /> level can be increased.  </summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.Zoom" /> level can be increased; otherwise, false.</returns>
	public bool CanIncreaseZoom => (bool)GetValue(CanIncreaseZoomProperty);

	/// <summary>Gets a value that indicates whether the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.Zoom" /> level can be decreased.  </summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.Zoom" /> level can be decreased; otherwise, false.</returns>
	public bool CanDecreaseZoom => (bool)GetValue(CanDecreaseZoomProperty);

	/// <summary>Gets or sets a value that indicates whether selection of content within the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> is enabled.  </summary>
	/// <returns>true to indicate that selection is enabled; otherwise, false. The default is true.</returns>
	public bool IsSelectionEnabled
	{
		get
		{
			return (bool)GetValue(IsSelectionEnabledProperty);
		}
		set
		{
			SetValue(IsSelectionEnabledProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> toolbar is visible.  </summary>
	/// <returns>true to indicate that the toolbar is visible; otherwise, false. The default is false.</returns>
	public bool IsToolBarVisible
	{
		get
		{
			return (bool)GetValue(IsToolBarVisibleProperty);
		}
		set
		{
			SetValue(IsToolBarVisibleProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a horizontal scroll bar is shown.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.Controls.ScrollBarVisibility" /> values. The default is <see cref="F:System.Windows.Controls.ScrollBarVisibility.Auto" /> .</returns>
	public ScrollBarVisibility HorizontalScrollBarVisibility
	{
		get
		{
			return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
		}
		set
		{
			SetValue(HorizontalScrollBarVisibilityProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a vertical scroll bar is shown.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.Controls.ScrollBarVisibility" /> values. The default is <see cref="F:System.Windows.Controls.ScrollBarVisibility.Visible" />.</returns>
	public ScrollBarVisibility VerticalScrollBarVisibility
	{
		get
		{
			return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
		}
		set
		{
			SetValue(VerticalScrollBarVisibilityProperty, value);
		}
	}

	/// <summary>Gets or sets the brush that highlights the selected text.</summary>
	/// <returns>A brush that highlights the selected text.</returns>
	public Brush SelectionBrush
	{
		get
		{
			return (Brush)GetValue(SelectionBrushProperty);
		}
		set
		{
			SetValue(SelectionBrushProperty, value);
		}
	}

	/// <summary>Gets or sets the opacity of the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.SelectionBrush" />.</summary>
	/// <returns>The opacity of the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.SelectionBrush" />. The default is 0.4.</returns>
	public double SelectionOpacity
	{
		get
		{
			return (double)GetValue(SelectionOpacityProperty);
		}
		set
		{
			SetValue(SelectionOpacityProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> has focus and selected text.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> displays selected text when the text box does not have focus; otherwise, false.The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public bool IsSelectionActive => (bool)GetValue(IsSelectionActiveProperty);

	/// <summary>Gets or sets a value that indicates whether <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> displays selected text when the control does not have focus.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> displays selected text when the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> does not have focus; otherwise, false.The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public bool IsInactiveSelectionHighlightEnabled
	{
		get
		{
			return (bool)GetValue(IsInactiveSelectionHighlightEnabledProperty);
		}
		set
		{
			SetValue(IsInactiveSelectionHighlightEnabledProperty, value);
		}
	}

	/// <summary>Gets an enumerator that can iterate the logical children of the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" />.</summary>
	/// <returns>An enumerator for the logical children.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			if (base.HasLogicalChildren && Document != null)
			{
				return new SingleChildEnumerator(Document);
			}
			return EmptyEnumerator.Instance;
		}
	}

	internal ScrollViewer ScrollViewer => _contentHost;

	internal bool CanShowFindToolBar
	{
		get
		{
			if (_findToolBarHost != null && Document != null)
			{
				return _textEditor != null;
			}
			return false;
		}
	}

	internal bool IsPrinting => _printingState != null;

	internal TextPointer ContentPosition
	{
		get
		{
			TextPointer result = null;
			ITextView textView = GetTextView();
			if (textView != null && textView.IsValid && textView.RenderScope is IScrollInfo)
			{
				result = textView.GetTextPositionFromPoint(default(Point), snapToText: true) as TextPointer;
			}
			return result;
		}
	}

	private FindToolBar FindToolBar
	{
		get
		{
			if (_findToolBarHost == null)
			{
				return null;
			}
			return _findToolBarHost.Child as FindToolBar;
		}
	}

	private FlowDocumentView RenderScope
	{
		get
		{
			if (_contentHost == null)
			{
				return null;
			}
			return _contentHost.Content as FlowDocumentView;
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static FlowDocumentScrollViewer()
	{
		DocumentProperty = DependencyProperty.Register("Document", typeof(FlowDocument), typeof(FlowDocumentScrollViewer), new FrameworkPropertyMetadata(null, DocumentChanged));
		ZoomProperty = FlowDocumentPageViewer.ZoomProperty.AddOwner(typeof(FlowDocumentScrollViewer), new FrameworkPropertyMetadata(100.0, ZoomChanged, CoerceZoom));
		MaxZoomProperty = FlowDocumentPageViewer.MaxZoomProperty.AddOwner(typeof(FlowDocumentScrollViewer), new FrameworkPropertyMetadata(200.0, MaxZoomChanged, CoerceMaxZoom));
		MinZoomProperty = FlowDocumentPageViewer.MinZoomProperty.AddOwner(typeof(FlowDocumentScrollViewer), new FrameworkPropertyMetadata(80.0, MinZoomChanged));
		ZoomIncrementProperty = FlowDocumentPageViewer.ZoomIncrementProperty.AddOwner(typeof(FlowDocumentScrollViewer));
		CanIncreaseZoomPropertyKey = DependencyProperty.RegisterReadOnly("CanIncreaseZoom", typeof(bool), typeof(FlowDocumentScrollViewer), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		CanIncreaseZoomProperty = CanIncreaseZoomPropertyKey.DependencyProperty;
		CanDecreaseZoomPropertyKey = DependencyProperty.RegisterReadOnly("CanDecreaseZoom", typeof(bool), typeof(FlowDocumentScrollViewer), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		CanDecreaseZoomProperty = CanDecreaseZoomPropertyKey.DependencyProperty;
		IsSelectionEnabledProperty = DependencyProperty.Register("IsSelectionEnabled", typeof(bool), typeof(FlowDocumentScrollViewer), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, IsSelectionEnabledChanged));
		IsToolBarVisibleProperty = DependencyProperty.Register("IsToolBarVisible", typeof(bool), typeof(FlowDocumentScrollViewer), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, IsToolBarVisibleChanged));
		HorizontalScrollBarVisibilityProperty = ScrollViewer.HorizontalScrollBarVisibilityProperty.AddOwner(typeof(FlowDocumentScrollViewer), new FrameworkPropertyMetadata(ScrollBarVisibility.Auto));
		VerticalScrollBarVisibilityProperty = ScrollViewer.VerticalScrollBarVisibilityProperty.AddOwner(typeof(FlowDocumentScrollViewer), new FrameworkPropertyMetadata(ScrollBarVisibility.Visible));
		SelectionBrushProperty = TextBoxBase.SelectionBrushProperty.AddOwner(typeof(FlowDocumentScrollViewer));
		SelectionOpacityProperty = TextBoxBase.SelectionOpacityProperty.AddOwner(typeof(FlowDocumentScrollViewer));
		IsSelectionActiveProperty = TextBoxBase.IsSelectionActiveProperty.AddOwner(typeof(FlowDocumentScrollViewer));
		IsInactiveSelectionHighlightEnabledProperty = TextBoxBase.IsInactiveSelectionHighlightEnabledProperty.AddOwner(typeof(FlowDocumentScrollViewer));
		IsEditingEnabled = false;
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FlowDocumentScrollViewer), new FrameworkPropertyMetadata(new ComponentResourceKey(typeof(PresentationUIStyleResources), "PUIFlowDocumentScrollViewer")));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(FlowDocumentScrollViewer));
		TextBoxBase.SelectionBrushProperty.OverrideMetadata(typeof(FlowDocumentScrollViewer), new FrameworkPropertyMetadata(UpdateCaretElement));
		TextBoxBase.SelectionOpacityProperty.OverrideMetadata(typeof(FlowDocumentScrollViewer), new FrameworkPropertyMetadata(0.4, UpdateCaretElement));
		CreateCommandBindings();
		EventManager.RegisterClassHandler(typeof(FlowDocumentScrollViewer), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(HandleRequestBringIntoView));
		EventManager.RegisterClassHandler(typeof(FlowDocumentScrollViewer), Keyboard.KeyDownEvent, new KeyEventHandler(KeyDownHandler), handledEventsToo: true);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> class.</summary>
	public FlowDocumentScrollViewer()
	{
		AnnotationService.SetDataId(this, "FlowDocument");
	}

	/// <summary>Builds the visual tree for the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" />.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		if (FindToolBar != null)
		{
			ToggleFindToolBar(enable: false);
		}
		_findToolBarHost = GetTemplateChild("PART_FindToolBarHost") as Decorator;
		_toolBarHost = GetTemplateChild("PART_ToolBarHost") as Decorator;
		if (_toolBarHost != null)
		{
			_toolBarHost.Visibility = ((!IsToolBarVisible) ? Visibility.Collapsed : Visibility.Visible);
		}
		if (_contentHost != null)
		{
			BindingOperations.ClearBinding(_contentHost, HorizontalScrollBarVisibilityProperty);
			BindingOperations.ClearBinding(_contentHost, VerticalScrollBarVisibilityProperty);
			_contentHost.ScrollChanged -= OnScrollChanged;
			RenderScope.Document = null;
			ClearValue(TextEditor.PageHeightProperty);
			_contentHost.Content = null;
		}
		_contentHost = GetTemplateChild("PART_ContentHost") as ScrollViewer;
		if (_contentHost != null)
		{
			if (_contentHost.Content != null)
			{
				throw new NotSupportedException(SR.FlowDocumentScrollViewerMarkedAsContentHostMustHaveNoContent);
			}
			_contentHost.ScrollChanged += OnScrollChanged;
			CreateTwoWayBinding(_contentHost, HorizontalScrollBarVisibilityProperty, "HorizontalScrollBarVisibility");
			CreateTwoWayBinding(_contentHost, VerticalScrollBarVisibilityProperty, "VerticalScrollBarVisibility");
			_contentHost.Focusable = false;
			_contentHost.Content = new FlowDocumentView();
			RenderScope.Document = Document;
		}
		AttachTextEditor();
		ApplyZoom();
	}

	/// <summary>Toggles the Find dialog.</summary>
	public void Find()
	{
		OnFindCommand();
	}

	/// <summary>Invokes a standard Print dialog which can be used to print the contents of the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> and configure printing preferences.</summary>
	public void Print()
	{
		OnPrintCommand();
	}

	/// <summary>Cancels any current printing job.</summary>
	public void CancelPrint()
	{
		OnCancelPrintCommand();
	}

	/// <summary>Executes the <see cref="P:System.Windows.Input.NavigationCommands.IncreaseZoom" /> routed command.</summary>
	public void IncreaseZoom()
	{
		OnIncreaseZoomCommand();
	}

	/// <summary>Executes the <see cref="P:System.Windows.Input.NavigationCommands.DecreaseZoom" /> routed command.</summary>
	public void DecreaseZoom()
	{
		OnDecreaseZoomCommand();
	}

	/// <summary>Called when a printing job has completed.</summary>
	protected virtual void OnPrintCompleted()
	{
		ClearPrintingState();
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.ApplicationCommands.Find" /> routed command.</summary>
	protected virtual void OnFindCommand()
	{
		if (CanShowFindToolBar)
		{
			ToggleFindToolBar(FindToolBar == null);
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.ApplicationCommands.Print" /> routed command.</summary>
	protected virtual void OnPrintCommand()
	{
		PrintDocumentImageableArea documentImageableArea = null;
		if (_printingState != null)
		{
			return;
		}
		if (Document != null)
		{
			XpsDocumentWriter xpsDocumentWriter = PrintQueue.CreateXpsDocumentWriter(ref documentImageableArea);
			if (xpsDocumentWriter != null && documentImageableArea != null)
			{
				if (RenderScope != null)
				{
					RenderScope.SuspendLayout();
				}
				FlowDocumentPaginator flowDocumentPaginator = ((IDocumentPaginatorSource)Document).DocumentPaginator as FlowDocumentPaginator;
				_printingState = new FlowDocumentPrintingState();
				_printingState.XpsDocumentWriter = xpsDocumentWriter;
				_printingState.PageSize = flowDocumentPaginator.PageSize;
				_printingState.PagePadding = Document.PagePadding;
				_printingState.IsSelectionEnabled = IsSelectionEnabled;
				_printingState.ColumnWidth = Document.ColumnWidth;
				CommandManager.InvalidateRequerySuggested();
				xpsDocumentWriter.WritingCompleted += HandlePrintCompleted;
				xpsDocumentWriter.WritingCancelled += HandlePrintCancelled;
				if (_contentHost != null)
				{
					CommandManager.AddPreviewCanExecuteHandler(_contentHost, PreviewCanExecuteRoutedEventHandler);
				}
				if (IsSelectionEnabled)
				{
					SetCurrentValueInternal(IsSelectionEnabledProperty, BooleanBoxes.FalseBox);
				}
				flowDocumentPaginator.PageSize = new Size(documentImageableArea.MediaSizeWidth, documentImageableArea.MediaSizeHeight);
				Thickness thickness = Document.ComputePageMargin();
				Document.PagePadding = new Thickness(Math.Max(documentImageableArea.OriginWidth, thickness.Left), Math.Max(documentImageableArea.OriginHeight, thickness.Top), Math.Max(documentImageableArea.MediaSizeWidth - (documentImageableArea.OriginWidth + documentImageableArea.ExtentWidth), thickness.Right), Math.Max(documentImageableArea.MediaSizeHeight - (documentImageableArea.OriginHeight + documentImageableArea.ExtentHeight), thickness.Bottom));
				Document.ColumnWidth = double.PositiveInfinity;
				xpsDocumentWriter.WriteAsync(flowDocumentPaginator);
			}
			else
			{
				OnPrintCompleted();
			}
		}
		else
		{
			OnPrintCompleted();
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.ApplicationCommands.CancelPrint" /> routed command.</summary>
	protected virtual void OnCancelPrintCommand()
	{
		if (_printingState != null)
		{
			_printingState.XpsDocumentWriter.CancelAsync();
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.NavigationCommands.IncreaseZoom" /> routed command.</summary>
	protected virtual void OnIncreaseZoomCommand()
	{
		if (CanIncreaseZoom)
		{
			Zoom = Math.Min(Zoom + ZoomIncrement, MaxZoom);
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.NavigationCommands.DecreaseZoom" /> routed command.</summary>
	protected virtual void OnDecreaseZoomCommand()
	{
		if (CanDecreaseZoom)
		{
			Zoom = Math.Max(Zoom - ZoomIncrement, MinZoom);
		}
	}

	/// <summary>Handles the <see cref="E:System.Windows.UIElement.KeyDown" />  routed event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.KeyEventArgs" /> object containing the arguments associated with the <see cref="E:System.Windows.UIElement.KeyDown" /> routed event.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.Handled)
		{
			return;
		}
		switch (e.Key)
		{
		case Key.Escape:
			if (FindToolBar != null)
			{
				ToggleFindToolBar(enable: false);
				e.Handled = true;
			}
			break;
		case Key.F3:
			if (CanShowFindToolBar)
			{
				if (FindToolBar != null)
				{
					FindToolBar.SearchUp = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
					OnFindInvoked(this, EventArgs.Empty);
				}
				else
				{
					ToggleFindToolBar(enable: true);
				}
				e.Handled = true;
			}
			break;
		}
		if (!e.Handled)
		{
			base.OnKeyDown(e);
		}
	}

	/// <summary>Handles the <see cref="E:System.Windows.UIElement.MouseWheel" />  routed event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.MouseWheelEventArgs" /> object containing arguments associated with the <see cref="E:System.Windows.UIElement.MouseWheel" /> routed event.</param>
	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		if (e.Handled)
		{
			return;
		}
		if (_contentHost != null)
		{
			if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
			{
				if (e.Delta > 0 && CanIncreaseZoom)
				{
					SetCurrentValueInternal(ZoomProperty, Math.Min(Zoom + ZoomIncrement, MaxZoom));
				}
				else if (e.Delta < 0 && CanDecreaseZoom)
				{
					SetCurrentValueInternal(ZoomProperty, Math.Max(Zoom - ZoomIncrement, MinZoom));
				}
			}
			else if (e.Delta < 0)
			{
				_contentHost.LineDown();
			}
			else
			{
				_contentHost.LineUp();
			}
			e.Handled = true;
		}
		if (!e.Handled)
		{
			base.OnMouseWheel(e);
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.FrameworkElement.ContextMenuOpening" /> routed event reaches this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Arguments of the event.</param>
	protected override void OnContextMenuOpening(ContextMenuEventArgs e)
	{
		base.OnContextMenuOpening(e);
		DocumentViewerHelper.OnContextMenuOpening(Document, this, e);
	}

	/// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new FlowDocumentScrollViewerAutomationPeer(this);
	}

	internal override bool BuildRouteCore(EventRoute route, RoutedEventArgs args)
	{
		DependencyObject document = Document;
		if (document != null && LogicalTreeHelper.GetParent(document) != this && route.PeekBranchNode() is DependencyObject dependencyObject && DocumentViewerHelper.IsLogicalDescendent(dependencyObject, document))
		{
			FrameworkElement.AddIntermediateElementsToRoute(LogicalTreeHelper.GetParent(document), route, args, LogicalTreeHelper.GetParent(dependencyObject));
		}
		return base.BuildRouteCore(route, args);
	}

	internal override bool InvalidateAutomationAncestorsCore(Stack<DependencyObject> branchNodeStack, out bool continuePastCoreTree)
	{
		bool flag = true;
		DependencyObject document = Document;
		if (document != null && LogicalTreeHelper.GetParent(document) != this)
		{
			DependencyObject dependencyObject = ((branchNodeStack.Count > 0) ? branchNodeStack.Peek() : null);
			if (dependencyObject != null && DocumentViewerHelper.IsLogicalDescendent(dependencyObject, document))
			{
				flag = FrameworkElement.InvalidateAutomationIntermediateElements(LogicalTreeHelper.GetParent(document), LogicalTreeHelper.GetParent(dependencyObject));
			}
		}
		return flag & base.InvalidateAutomationAncestorsCore(branchNodeStack, out continuePastCoreTree);
	}

	internal object BringContentPositionIntoView(object arg)
	{
		if (arg is ITextPointer textPointer)
		{
			ITextView textView = GetTextView();
			if (textView != null && textView.IsValid && textView.RenderScope is IScrollInfo && textPointer.TextContainer == textView.TextContainer)
			{
				if (textView.Contains(textPointer))
				{
					Rect rectangleFromTextPosition = textView.GetRectangleFromTextPosition(textPointer);
					if (rectangleFromTextPosition != Rect.Empty)
					{
						IScrollInfo scrollInfo = (IScrollInfo)textView.RenderScope;
						scrollInfo.SetVerticalOffset(rectangleFromTextPosition.Top + scrollInfo.VerticalOffset);
					}
				}
				else
				{
					base.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(BringContentPositionIntoView), textPointer);
				}
			}
		}
		return null;
	}

	private void ToggleFindToolBar(bool enable)
	{
		Invariant.Assert(enable == (FindToolBar == null));
		DocumentViewerHelper.ToggleFindToolBar(_findToolBarHost, OnFindInvoked, enable);
		if (!IsToolBarVisible && _toolBarHost != null)
		{
			_toolBarHost.Visibility = ((!enable) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private void ApplyZoom()
	{
		if (RenderScope != null)
		{
			RenderScope.LayoutTransform = new ScaleTransform(Zoom / 100.0, Zoom / 100.0);
		}
	}

	private void AttachTextEditor()
	{
		AnnotationService service = AnnotationService.GetService(this);
		bool flag = false;
		if (service != null && service.IsEnabled)
		{
			flag = true;
			service.Disable();
		}
		if (_textEditor != null)
		{
			_textEditor.TextContainer.TextView = null;
			_textEditor.OnDetach();
			_textEditor = null;
		}
		ITextView textView = null;
		if (Document != null)
		{
			textView = GetTextView();
			Document.StructuralCache.TextContainer.TextView = textView;
		}
		if (IsSelectionEnabled && Document != null && RenderScope != null && Document.StructuralCache.TextContainer.TextSelection == null)
		{
			_textEditor = new TextEditor(Document.StructuralCache.TextContainer, this, isUndoEnabled: false);
			_textEditor.IsReadOnly = !IsEditingEnabled;
			_textEditor.TextView = textView;
		}
		if (service != null && flag)
		{
			service.Enable(service.Store);
		}
		if (_textEditor == null && FindToolBar != null)
		{
			ToggleFindToolBar(enable: false);
		}
	}

	private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
	{
		if (e.OriginalSource == ScrollViewer && !DoubleUtil.IsZero(e.ViewportHeightChange))
		{
			SetValue(TextEditor.PageHeightProperty, e.ViewportHeight);
		}
	}

	private void HandlePrintCompleted(object sender, WritingCompletedEventArgs e)
	{
		OnPrintCompleted();
	}

	private void HandlePrintCancelled(object sender, WritingCancelledEventArgs e)
	{
		ClearPrintingState();
	}

	private void ClearPrintingState()
	{
		if (_printingState != null)
		{
			if (RenderScope != null)
			{
				RenderScope.ResumeLayout();
			}
			if (_printingState.IsSelectionEnabled)
			{
				SetCurrentValueInternal(IsSelectionEnabledProperty, BooleanBoxes.TrueBox);
			}
			if (_contentHost != null)
			{
				CommandManager.RemovePreviewCanExecuteHandler(_contentHost, PreviewCanExecuteRoutedEventHandler);
			}
			_printingState.XpsDocumentWriter.WritingCompleted -= HandlePrintCompleted;
			_printingState.XpsDocumentWriter.WritingCancelled -= HandlePrintCancelled;
			Document.PagePadding = _printingState.PagePadding;
			Document.ColumnWidth = _printingState.ColumnWidth;
			((IDocumentPaginatorSource)Document).DocumentPaginator.PageSize = _printingState.PageSize;
			_printingState = null;
			CommandManager.InvalidateRequerySuggested();
		}
	}

	private void HandleRequestBringIntoView(RequestBringIntoViewEventArgs args)
	{
		Rect rect = Rect.Empty;
		if (args == null || args.TargetObject == null || Document == null)
		{
			return;
		}
		DependencyObject document = Document;
		if (args.TargetObject == document)
		{
			if (_contentHost != null)
			{
				_contentHost.ScrollToHome();
			}
			args.Handled = true;
		}
		else if (args.TargetObject is UIElement)
		{
			UIElement uIElement = (UIElement)args.TargetObject;
			if (RenderScope != null && RenderScope.IsAncestorOf(uIElement))
			{
				rect = args.TargetRect;
				if (rect.IsEmpty)
				{
					rect = new Rect(uIElement.RenderSize);
				}
				rect = MakeVisible(RenderScope, uIElement, rect);
				if (!rect.IsEmpty)
				{
					rect = RenderScope.TransformToAncestor(this).TransformBounds(rect);
				}
				args.Handled = true;
			}
		}
		else if (args.TargetObject is ContentElement)
		{
			DependencyObject dependencyObject = args.TargetObject;
			while (dependencyObject != null && dependencyObject != document)
			{
				dependencyObject = LogicalTreeHelper.GetParent(dependencyObject);
			}
			if (dependencyObject != null)
			{
				IContentHost iContentHost = GetIContentHost();
				if (iContentHost != null)
				{
					ReadOnlyCollection<Rect> rectangles = iContentHost.GetRectangles((ContentElement)args.TargetObject);
					if (rectangles.Count > 0)
					{
						rect = MakeVisible(RenderScope, (Visual)iContentHost, rectangles[0]);
						if (!rect.IsEmpty)
						{
							rect = RenderScope.TransformToAncestor(this).TransformBounds(rect);
						}
					}
				}
				args.Handled = true;
			}
		}
		if (args.Handled)
		{
			if (rect.IsEmpty)
			{
				BringIntoView();
			}
			else
			{
				BringIntoView(rect);
			}
		}
	}

	private void DocumentChanged(FlowDocument oldDocument, FlowDocument newDocument)
	{
		if (newDocument != null && newDocument.StructuralCache.TextContainer != null && newDocument.StructuralCache.TextContainer.TextSelection != null)
		{
			throw new ArgumentException(SR.FlowDocumentScrollViewerDocumentBelongsToAnotherFlowDocumentScrollViewerAlready);
		}
		if (oldDocument != null)
		{
			if (_documentAsLogicalChild)
			{
				RemoveLogicalChild(oldDocument);
			}
			if (RenderScope != null)
			{
				RenderScope.Document = null;
			}
			oldDocument.ClearValue(PathNode.HiddenParentProperty);
			oldDocument.StructuralCache.ClearUpdateInfo(destroyStructureCache: true);
		}
		if (newDocument != null && LogicalTreeHelper.GetParent(newDocument) != null)
		{
			ContentOperations.SetParent(newDocument, this);
			_documentAsLogicalChild = false;
		}
		else
		{
			_documentAsLogicalChild = true;
		}
		if (newDocument != null)
		{
			if (RenderScope != null)
			{
				RenderScope.Document = newDocument;
			}
			if (_documentAsLogicalChild)
			{
				AddLogicalChild(newDocument);
			}
			newDocument.SetValue(PathNode.HiddenParentProperty, this);
			newDocument.StructuralCache.ClearUpdateInfo(destroyStructureCache: true);
		}
		AttachTextEditor();
		if (!CanShowFindToolBar && FindToolBar != null)
		{
			ToggleFindToolBar(enable: false);
		}
		if (UIElementAutomationPeer.FromElement(this) is FlowDocumentScrollViewerAutomationPeer flowDocumentScrollViewerAutomationPeer)
		{
			flowDocumentScrollViewerAutomationPeer.InvalidatePeer();
		}
	}

	private ITextView GetTextView()
	{
		ITextView result = null;
		if (RenderScope != null)
		{
			result = (ITextView)((IServiceProvider)RenderScope).GetService(typeof(ITextView));
		}
		return result;
	}

	private IContentHost GetIContentHost()
	{
		IContentHost result = null;
		if (RenderScope != null && VisualTreeHelper.GetChildrenCount(RenderScope) > 0)
		{
			result = VisualTreeHelper.GetChild(RenderScope, 0) as IContentHost;
		}
		return result;
	}

	private void CreateTwoWayBinding(FrameworkElement fe, DependencyProperty dp, string propertyPath)
	{
		Binding binding = new Binding(propertyPath);
		binding.Mode = BindingMode.TwoWay;
		binding.Source = this;
		fe.SetBinding(dp, binding);
	}

	private static void CreateCommandBindings()
	{
		ExecutedRoutedEventHandler executedRoutedEventHandler = ExecutedRoutedEventHandler;
		CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = CanExecuteRoutedEventHandler;
		_commandLineDown = new RoutedUICommand(string.Empty, "FDSV_LineDown", typeof(FlowDocumentScrollViewer));
		_commandLineUp = new RoutedUICommand(string.Empty, "FDSV_LineUp", typeof(FlowDocumentScrollViewer));
		_commandLineLeft = new RoutedUICommand(string.Empty, "FDSV_LineLeft", typeof(FlowDocumentScrollViewer));
		_commandLineRight = new RoutedUICommand(string.Empty, "FDSV_LineRight", typeof(FlowDocumentScrollViewer));
		TextEditor.RegisterCommandHandlers(typeof(FlowDocumentScrollViewer), acceptsRichContent: true, !IsEditingEnabled, registerEventListeners: true);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentScrollViewer), ApplicationCommands.Find, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentScrollViewer), ApplicationCommands.Print, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentScrollViewer), ApplicationCommands.CancelPrint, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentScrollViewer), NavigationCommands.PreviousPage, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Prior);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentScrollViewer), NavigationCommands.NextPage, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Next);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentScrollViewer), NavigationCommands.FirstPage, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.Home), new KeyGesture(Key.Home, ModifierKeys.Control));
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentScrollViewer), NavigationCommands.LastPage, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.End), new KeyGesture(Key.End, ModifierKeys.Control));
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentScrollViewer), NavigationCommands.IncreaseZoom, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.OemPlus, ModifierKeys.Control));
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentScrollViewer), NavigationCommands.DecreaseZoom, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.OemMinus, ModifierKeys.Control));
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentScrollViewer), _commandLineDown, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Down);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentScrollViewer), _commandLineUp, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Up);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentScrollViewer), _commandLineLeft, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Left);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentScrollViewer), _commandLineRight, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Right);
	}

	private static void CanExecuteRoutedEventHandler(object target, CanExecuteRoutedEventArgs args)
	{
		FlowDocumentScrollViewer flowDocumentScrollViewer = target as FlowDocumentScrollViewer;
		Invariant.Assert(flowDocumentScrollViewer != null, "Target of QueryEnabledEvent must be FlowDocumentScrollViewer.");
		Invariant.Assert(args != null, "args cannot be null.");
		if (flowDocumentScrollViewer._printingState == null)
		{
			if (args.Command == ApplicationCommands.Find)
			{
				args.CanExecute = flowDocumentScrollViewer.CanShowFindToolBar;
			}
			else if (args.Command == ApplicationCommands.Print)
			{
				args.CanExecute = flowDocumentScrollViewer.Document != null;
			}
			else if (args.Command == ApplicationCommands.CancelPrint)
			{
				args.CanExecute = false;
			}
			else
			{
				args.CanExecute = true;
			}
		}
		else
		{
			args.CanExecute = args.Command == ApplicationCommands.CancelPrint;
		}
	}

	private static void ExecutedRoutedEventHandler(object target, ExecutedRoutedEventArgs args)
	{
		FlowDocumentScrollViewer flowDocumentScrollViewer = target as FlowDocumentScrollViewer;
		Invariant.Assert(flowDocumentScrollViewer != null, "Target of ExecuteEvent must be FlowDocumentScrollViewer.");
		Invariant.Assert(args != null, "args cannot be null.");
		if (args.Command == ApplicationCommands.Find)
		{
			flowDocumentScrollViewer.OnFindCommand();
		}
		else if (args.Command == ApplicationCommands.Print)
		{
			flowDocumentScrollViewer.OnPrintCommand();
		}
		else if (args.Command == ApplicationCommands.CancelPrint)
		{
			flowDocumentScrollViewer.OnCancelPrintCommand();
		}
		else if (args.Command == NavigationCommands.IncreaseZoom)
		{
			flowDocumentScrollViewer.OnIncreaseZoomCommand();
		}
		else if (args.Command == NavigationCommands.DecreaseZoom)
		{
			flowDocumentScrollViewer.OnDecreaseZoomCommand();
		}
		else if (args.Command == _commandLineDown)
		{
			if (flowDocumentScrollViewer._contentHost != null)
			{
				flowDocumentScrollViewer._contentHost.LineDown();
			}
		}
		else if (args.Command == _commandLineUp)
		{
			if (flowDocumentScrollViewer._contentHost != null)
			{
				flowDocumentScrollViewer._contentHost.LineUp();
			}
		}
		else if (args.Command == _commandLineLeft)
		{
			if (flowDocumentScrollViewer._contentHost != null)
			{
				flowDocumentScrollViewer._contentHost.LineLeft();
			}
		}
		else if (args.Command == _commandLineRight)
		{
			if (flowDocumentScrollViewer._contentHost != null)
			{
				flowDocumentScrollViewer._contentHost.LineRight();
			}
		}
		else if (args.Command == NavigationCommands.NextPage)
		{
			if (flowDocumentScrollViewer._contentHost != null)
			{
				flowDocumentScrollViewer._contentHost.PageDown();
			}
		}
		else if (args.Command == NavigationCommands.PreviousPage)
		{
			if (flowDocumentScrollViewer._contentHost != null)
			{
				flowDocumentScrollViewer._contentHost.PageUp();
			}
		}
		else if (args.Command == NavigationCommands.FirstPage)
		{
			if (flowDocumentScrollViewer._contentHost != null)
			{
				flowDocumentScrollViewer._contentHost.ScrollToHome();
			}
		}
		else if (args.Command == NavigationCommands.LastPage)
		{
			if (flowDocumentScrollViewer._contentHost != null)
			{
				flowDocumentScrollViewer._contentHost.ScrollToEnd();
			}
		}
		else
		{
			Invariant.Assert(condition: false, "Command not handled in ExecutedRoutedEventHandler.");
		}
	}

	private void OnFindInvoked(object sender, EventArgs e)
	{
		FindToolBar findToolBar = FindToolBar;
		if (findToolBar != null && _textEditor != null)
		{
			Focus();
			ITextRange textRange = DocumentViewerHelper.Find(findToolBar, _textEditor, _textEditor.TextView, _textEditor.TextView);
			if (textRange == null || textRange.IsEmpty)
			{
				DocumentViewerHelper.ShowFindUnsuccessfulMessage(findToolBar);
			}
		}
	}

	private void PreviewCanExecuteRoutedEventHandler(object target, CanExecuteRoutedEventArgs args)
	{
		Invariant.Assert(target is ScrollViewer, "Target of PreviewCanExecuteRoutedEventHandler must be ScrollViewer.");
		Invariant.Assert(args != null, "args cannot be null.");
		if (_printingState != null)
		{
			args.CanExecute = false;
			args.Handled = true;
		}
	}

	private static void KeyDownHandler(object sender, KeyEventArgs e)
	{
		DocumentViewerHelper.KeyDownHelper(e, ((FlowDocumentScrollViewer)sender)._findToolBarHost);
	}

	private static Rect MakeVisible(IScrollInfo scrollInfo, Visual visual, Rect rectangle)
	{
		if (scrollInfo.GetType() == typeof(ScrollContentPresenter))
		{
			return ((ScrollContentPresenter)scrollInfo).MakeVisible(visual, rectangle, throwOnError: false);
		}
		return scrollInfo.MakeVisible(visual, rectangle);
	}

	private static void DocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentScrollViewer);
		((FlowDocumentScrollViewer)d).DocumentChanged((FlowDocument)e.OldValue, (FlowDocument)e.NewValue);
		CommandManager.InvalidateRequerySuggested();
	}

	private static void ZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentScrollViewer);
		FlowDocumentScrollViewer flowDocumentScrollViewer = (FlowDocumentScrollViewer)d;
		if (!DoubleUtil.AreClose((double)e.OldValue, (double)e.NewValue))
		{
			flowDocumentScrollViewer.SetValue(CanIncreaseZoomPropertyKey, BooleanBoxes.Box(DoubleUtil.GreaterThan(flowDocumentScrollViewer.MaxZoom, flowDocumentScrollViewer.Zoom)));
			flowDocumentScrollViewer.SetValue(CanDecreaseZoomPropertyKey, BooleanBoxes.Box(DoubleUtil.LessThan(flowDocumentScrollViewer.MinZoom, flowDocumentScrollViewer.Zoom)));
			flowDocumentScrollViewer.ApplyZoom();
		}
	}

	private static object CoerceZoom(DependencyObject d, object value)
	{
		Invariant.Assert(d != null && d is FlowDocumentScrollViewer);
		FlowDocumentScrollViewer flowDocumentScrollViewer = (FlowDocumentScrollViewer)d;
		double value2 = (double)value;
		double maxZoom = flowDocumentScrollViewer.MaxZoom;
		if (DoubleUtil.LessThan(maxZoom, value2))
		{
			return maxZoom;
		}
		double minZoom = flowDocumentScrollViewer.MinZoom;
		if (DoubleUtil.GreaterThan(minZoom, value2))
		{
			return minZoom;
		}
		return value;
	}

	private static void MaxZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentScrollViewer);
		FlowDocumentScrollViewer flowDocumentScrollViewer = (FlowDocumentScrollViewer)d;
		flowDocumentScrollViewer.CoerceValue(ZoomProperty);
		flowDocumentScrollViewer.SetValue(CanIncreaseZoomPropertyKey, BooleanBoxes.Box(DoubleUtil.GreaterThan(flowDocumentScrollViewer.MaxZoom, flowDocumentScrollViewer.Zoom)));
	}

	private static object CoerceMaxZoom(DependencyObject d, object value)
	{
		Invariant.Assert(d != null && d is FlowDocumentScrollViewer);
		double minZoom = ((FlowDocumentScrollViewer)d).MinZoom;
		if (!((double)value < minZoom))
		{
			return value;
		}
		return minZoom;
	}

	private static void MinZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentScrollViewer);
		FlowDocumentScrollViewer flowDocumentScrollViewer = (FlowDocumentScrollViewer)d;
		flowDocumentScrollViewer.CoerceValue(MaxZoomProperty);
		flowDocumentScrollViewer.CoerceValue(ZoomProperty);
		flowDocumentScrollViewer.SetValue(CanDecreaseZoomPropertyKey, BooleanBoxes.Box(DoubleUtil.LessThan(flowDocumentScrollViewer.MinZoom, flowDocumentScrollViewer.Zoom)));
	}

	private static bool ZoomValidateValue(object o)
	{
		double num = (double)o;
		if (!double.IsNaN(num) && !double.IsInfinity(num))
		{
			return DoubleUtil.GreaterThanZero(num);
		}
		return false;
	}

	private static void HandleRequestBringIntoView(object sender, RequestBringIntoViewEventArgs args)
	{
		if (sender != null && sender is FlowDocumentScrollViewer)
		{
			((FlowDocumentScrollViewer)sender).HandleRequestBringIntoView(args);
		}
	}

	private static void IsSelectionEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentScrollViewer);
		((FlowDocumentScrollViewer)d).AttachTextEditor();
	}

	private static void IsToolBarVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentScrollViewer);
		FlowDocumentScrollViewer flowDocumentScrollViewer = (FlowDocumentScrollViewer)d;
		if (flowDocumentScrollViewer._toolBarHost != null)
		{
			flowDocumentScrollViewer._toolBarHost.Visibility = ((!(bool)e.NewValue) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private static void UpdateCaretElement(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		FlowDocumentScrollViewer flowDocumentScrollViewer = (FlowDocumentScrollViewer)d;
		if (flowDocumentScrollViewer.Selection != null)
		{
			flowDocumentScrollViewer.Selection.CaretElement?.InvalidateVisual();
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. Use the <see cref="P:System.Windows.Controls.FlowDocumentScrollViewer.Document" /> property to add a <see cref="T:System.Windows.Documents.FlowDocument" /> as the content child for the <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" />.</summary>
	/// <param name="value">An object to add as a child.</param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (Document != null)
		{
			throw new ArgumentException(SR.FlowDocumentScrollViewerCanHaveOnlyOneChild);
		}
		if (!(value is FlowDocument))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(FlowDocument)), "value");
		}
		Document = value as FlowDocument;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="text">A string to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>A service object of type <paramref name="serviceType" />, or null if there is no service object of type <paramref name="serviceType" />.</returns>
	/// <param name="serviceType">An object that specifies the type of service object to get.</param>
	object IServiceProvider.GetService(Type serviceType)
	{
		object result = null;
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (serviceType == typeof(ITextView))
		{
			result = GetTextView();
		}
		else if ((serviceType == typeof(TextContainer) || serviceType == typeof(ITextContainer)) && Document != null)
		{
			result = ((IServiceProvider)Document).GetService(serviceType);
		}
		return result;
	}

	CustomJournalStateInternal IJournalState.GetJournalState(JournalReason journalReason)
	{
		int contentPosition = -1;
		LogicalDirection contentPositionDirection = LogicalDirection.Forward;
		TextPointer contentPosition2 = ContentPosition;
		if (contentPosition2 != null)
		{
			contentPosition = contentPosition2.Offset;
			contentPositionDirection = contentPosition2.LogicalDirection;
		}
		return new JournalState(contentPosition, contentPositionDirection, Zoom);
	}

	void IJournalState.RestoreJournalState(CustomJournalStateInternal state)
	{
		JournalState journalState = state as JournalState;
		if (state == null)
		{
			return;
		}
		Zoom = journalState.Zoom;
		if (journalState.ContentPosition == -1)
		{
			return;
		}
		FlowDocument document = Document;
		if (document != null)
		{
			TextContainer textContainer = document.StructuralCache.TextContainer;
			if (journalState.ContentPosition <= textContainer.SymbolCount)
			{
				TextPointer arg = textContainer.CreatePointerAtOffset(journalState.ContentPosition, journalState.ContentPositionDirection);
				base.Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(BringContentPositionIntoView), arg);
			}
		}
	}
}
