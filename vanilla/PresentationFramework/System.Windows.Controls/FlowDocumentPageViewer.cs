using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Printing;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Xps;
using MS.Internal;
using MS.Internal.AppModel;
using MS.Internal.Commands;
using MS.Internal.Documents;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Represents a control for viewing flow content in a fixed viewing mode that shows content one page at a time.</summary>
[TemplatePart(Name = "PART_FindToolBarHost", Type = typeof(Decorator))]
public class FlowDocumentPageViewer : DocumentViewerBase, IJournalState
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

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.Zoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.Zoom" /> dependency property.</returns>
	public static readonly DependencyProperty ZoomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.MaxZoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.MaxZoom" /> dependency property.</returns>
	public static readonly DependencyProperty MaxZoomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.MinZoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.MinZoom" /> dependency property.</returns>
	public static readonly DependencyProperty MinZoomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.ZoomIncrement" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.ZoomIncrement" /> dependency property.</returns>
	public static readonly DependencyProperty ZoomIncrementProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.CanIncreaseZoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.CanIncreaseZoom" /> dependency property.</returns>
	protected static readonly DependencyPropertyKey CanIncreaseZoomPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.CanIncreaseZoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.CanIncreaseZoom" /> dependency property.</returns>
	public static readonly DependencyProperty CanIncreaseZoomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.CanDecreaseZoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.CanDecreaseZoom" /> dependency property.</returns>
	protected static readonly DependencyPropertyKey CanDecreaseZoomPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.CanDecreaseZoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.CanDecreaseZoom" /> dependency property.</returns>
	public static readonly DependencyProperty CanDecreaseZoomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.SelectionBrush" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.SelectionBrush" /> dependency property.</returns>
	public static readonly DependencyProperty SelectionBrushProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.SelectionOpacity" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.SelectionOpacity" /> dependency property.</returns>
	public static readonly DependencyProperty SelectionOpacityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.IsSelectionActive" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.IsSelectionActive" /> dependency property.</returns>
	public static readonly DependencyProperty IsSelectionActiveProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.IsInactiveSelectionHighlightEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.IsInactiveSelectionHighlightEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsInactiveSelectionHighlightEnabledProperty;

	private Decorator _findToolBarHost;

	private ContentPosition _contentPosition;

	private FlowDocumentPrintingState _printingState;

	private IDocumentPaginatorSource _oldDocument;

	private object _bringContentPositionIntoViewToken = new object();

	private const string _findToolBarHostTemplateName = "PART_FindToolBarHost";

	private static DependencyObjectType _dType;

	/// <summary>Gets the selected content of the <see cref="T:System.Windows.Controls.FlowDocumentPageViewer" />.</summary>
	/// <returns>The selected content of the <see cref="T:System.Windows.Controls.FlowDocumentPageViewer" />.</returns>
	public TextSelection Selection
	{
		get
		{
			ITextSelection textSelection = null;
			if (base.Document is FlowDocument flowDocument)
			{
				textSelection = flowDocument.StructuralCache.TextContainer.TextSelection;
			}
			return textSelection as TextSelection;
		}
	}

	/// <summary>Gets or sets the current zoom level for the <see cref="T:System.Windows.Controls.FlowDocumentPageViewer" />. </summary>
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

	/// <summary>Gets or sets the maximum allowable <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.Zoom" /> level for the <see cref="T:System.Windows.Controls.FlowDocumentPageViewer" />. </summary>
	/// <returns>The maximum allowable zoom level for the <see cref="T:System.Windows.Controls.FlowDocumentPageViewer" />, interpreted as a percentage. The default is 200.0 (a maximum zoom of 200%).</returns>
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

	/// <summary>Gets or sets the minimum allowable <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.Zoom" /> level for the <see cref="T:System.Windows.Controls.FlowDocumentPageViewer" />. </summary>
	/// <returns>The minimum allowable zoom level for the <see cref="T:System.Windows.Controls.FlowDocumentPageViewer" />, interpreted as a percentage. The default is 80.0 (a minimum zoom of 80%).</returns>
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

	/// <summary>Gets or sets the zoom increment. </summary>
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

	/// <summary>Gets a value that indicates whether the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.Zoom" /> level can be increased. </summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.Zoom" /> level can be increased; otherwise, false.</returns>
	public virtual bool CanIncreaseZoom => (bool)GetValue(CanIncreaseZoomProperty);

	/// <summary>Gets a value that indicates whether the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.Zoom" /> level can be decreased. </summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.Zoom" /> level can be decreased; otherwise, false.</returns>
	public virtual bool CanDecreaseZoom => (bool)GetValue(CanDecreaseZoomProperty);

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

	/// <summary>Gets or sets the opacity of the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.SelectionBrush" />.</summary>
	/// <returns>The opacity of the <see cref="P:System.Windows.Controls.FlowDocumentPageViewer.SelectionBrush" />. The default is 0.4.</returns>
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

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.FlowDocumentPageViewer" /> has focus and selected text.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.FlowDocumentPageViewer" /> displays selected text when the text box does not have focus; otherwise, false.The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public bool IsSelectionActive => (bool)GetValue(IsSelectionActiveProperty);

	/// <summary>Gets or sets a value that indicates whether <see cref="T:System.Windows.Controls.FlowDocumentPageViewer" /> displays selected text when the control does not have focus.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.FlowDocumentPageViewer" /> displays selected text when the <see cref="T:System.Windows.Controls.FlowDocumentPageViewer" /> does not have focus; otherwise, false.The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
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

	internal ContentPosition ContentPosition => _contentPosition;

	internal bool CanShowFindToolBar
	{
		get
		{
			if (_findToolBarHost != null && base.Document != null)
			{
				return base.TextEditor != null;
			}
			return false;
		}
	}

	internal bool IsPrinting => _printingState != null;

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

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static FlowDocumentPageViewer()
	{
		ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(FlowDocumentPageViewer), new FrameworkPropertyMetadata(100.0, ZoomChanged, CoerceZoom), ZoomValidateValue);
		MaxZoomProperty = DependencyProperty.Register("MaxZoom", typeof(double), typeof(FlowDocumentPageViewer), new FrameworkPropertyMetadata(200.0, MaxZoomChanged, CoerceMaxZoom), ZoomValidateValue);
		MinZoomProperty = DependencyProperty.Register("MinZoom", typeof(double), typeof(FlowDocumentPageViewer), new FrameworkPropertyMetadata(80.0, MinZoomChanged), ZoomValidateValue);
		ZoomIncrementProperty = DependencyProperty.Register("ZoomIncrement", typeof(double), typeof(FlowDocumentPageViewer), new FrameworkPropertyMetadata(10.0), ZoomValidateValue);
		CanIncreaseZoomPropertyKey = DependencyProperty.RegisterReadOnly("CanIncreaseZoom", typeof(bool), typeof(FlowDocumentPageViewer), new FrameworkPropertyMetadata(true));
		CanIncreaseZoomProperty = CanIncreaseZoomPropertyKey.DependencyProperty;
		CanDecreaseZoomPropertyKey = DependencyProperty.RegisterReadOnly("CanDecreaseZoom", typeof(bool), typeof(FlowDocumentPageViewer), new FrameworkPropertyMetadata(true));
		CanDecreaseZoomProperty = CanDecreaseZoomPropertyKey.DependencyProperty;
		SelectionBrushProperty = TextBoxBase.SelectionBrushProperty.AddOwner(typeof(FlowDocumentPageViewer));
		SelectionOpacityProperty = TextBoxBase.SelectionOpacityProperty.AddOwner(typeof(FlowDocumentPageViewer));
		IsSelectionActiveProperty = TextBoxBase.IsSelectionActiveProperty.AddOwner(typeof(FlowDocumentPageViewer));
		IsInactiveSelectionHighlightEnabledProperty = TextBoxBase.IsInactiveSelectionHighlightEnabledProperty.AddOwner(typeof(FlowDocumentPageViewer));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FlowDocumentPageViewer), new FrameworkPropertyMetadata(new ComponentResourceKey(typeof(PresentationUIStyleResources), "PUIFlowDocumentPageViewer")));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(FlowDocumentPageViewer));
		TextBoxBase.SelectionBrushProperty.OverrideMetadata(typeof(FlowDocumentPageViewer), new FrameworkPropertyMetadata(UpdateCaretElement));
		TextBoxBase.SelectionOpacityProperty.OverrideMetadata(typeof(FlowDocumentPageViewer), new FrameworkPropertyMetadata(0.4, UpdateCaretElement));
		CreateCommandBindings();
		EventManager.RegisterClassHandler(typeof(FlowDocumentPageViewer), Keyboard.KeyDownEvent, new KeyEventHandler(KeyDownHandler), handledEventsToo: true);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.FlowDocumentPageViewer" /> class.</summary>
	public FlowDocumentPageViewer()
	{
		base.LayoutUpdated += HandleLayoutUpdated;
	}

	/// <summary>Builds the visual tree for the <see cref="T:System.Windows.Controls.FlowDocumentPageViewer" />.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		if (FindToolBar != null)
		{
			ToggleFindToolBar(enable: false);
		}
		_findToolBarHost = GetTemplateChild("PART_FindToolBarHost") as Decorator;
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

	/// <summary>Toggles the Find dialog.</summary>
	public void Find()
	{
		OnFindCommand();
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation for this control, as part of the WPF automation infrastructure.</summary>
	/// <returns>The appropriate <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation for this control.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new FlowDocumentPageViewerAutomationPeer(this);
	}

	/// <summary>Handles the <see cref="E:System.Windows.UIElement.KeyDown" />  routed event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.KeyEventArgs" /> object that contains the arguments associated with the <see cref="E:System.Windows.UIElement.KeyDown" /> routed event.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.Key == Key.Escape && FindToolBar != null)
		{
			ToggleFindToolBar(enable: false);
			e.Handled = true;
		}
		if (e.Key == Key.F3 && CanShowFindToolBar)
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
		if (!e.Handled)
		{
			base.OnKeyDown(e);
		}
	}

	/// <summary>Handles the <see cref="E:System.Windows.UIElement.MouseWheel" /> routed event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.MouseWheelEventArgs" /> object containing arguments associated with the <see cref="E:System.Windows.UIElement.MouseWheel" /> routed event.</param>
	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		if (e.Delta != 0)
		{
			if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
			{
				if (e.Delta > 0)
				{
					IncreaseZoom();
				}
				else
				{
					DecreaseZoom();
				}
			}
			else if (e.Delta > 0)
			{
				PreviousPage();
			}
			else
			{
				NextPage();
			}
			e.Handled = false;
		}
		if (!e.Handled)
		{
			base.OnMouseWheel(e);
		}
	}

	/// <summary>Called whenever an unhandled <see cref="E:System.Windows.FrameworkElement.ContextMenuOpening" /> routed event reaches this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Arguments of the event.</param>
	protected override void OnContextMenuOpening(ContextMenuEventArgs e)
	{
		base.OnContextMenuOpening(e);
		DocumentViewerHelper.OnContextMenuOpening(base.Document as FlowDocument, this, e);
	}

	/// <summary>Handles the <see cref="E:System.Windows.Controls.Primitives.DocumentViewerBase.PageViewsChanged" />  routed event.</summary>
	protected override void OnPageViewsChanged()
	{
		_contentPosition = null;
		ApplyZoom();
		base.OnPageViewsChanged();
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.Document" /> property is changed.</summary>
	protected override void OnDocumentChanged()
	{
		_contentPosition = null;
		if (_oldDocument != null)
		{
			if (_oldDocument.DocumentPaginator is DynamicDocumentPaginator dynamicDocumentPaginator)
			{
				dynamicDocumentPaginator.GetPageNumberCompleted -= HandleGetPageNumberCompleted;
			}
			if (_oldDocument.DocumentPaginator is FlowDocumentPaginator flowDocumentPaginator)
			{
				flowDocumentPaginator.BreakRecordTableInvalidated -= HandleAllBreakRecordsInvalidated;
			}
		}
		base.OnDocumentChanged();
		_oldDocument = base.Document;
		if (base.Document != null && !(base.Document is FlowDocument))
		{
			base.Document = null;
			throw new NotSupportedException(SR.FlowDocumentPageViewerOnlySupportsFlowDocument);
		}
		if (base.Document != null)
		{
			if (base.Document.DocumentPaginator is DynamicDocumentPaginator dynamicDocumentPaginator2)
			{
				dynamicDocumentPaginator2.GetPageNumberCompleted += HandleGetPageNumberCompleted;
			}
			if (base.Document.DocumentPaginator is FlowDocumentPaginator flowDocumentPaginator2)
			{
				flowDocumentPaginator2.BreakRecordTableInvalidated += HandleAllBreakRecordsInvalidated;
			}
		}
		if (!CanShowFindToolBar && FindToolBar != null)
		{
			ToggleFindToolBar(enable: false);
		}
		OnGoToPageCommand(1);
	}

	/// <summary>Called when a printing job has completed.</summary>
	protected virtual void OnPrintCompleted()
	{
		ClearPrintingState();
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.NavigationCommands.PreviousPage" /> routed command.</summary>
	protected override void OnPreviousPageCommand()
	{
		if (CanGoToPreviousPage)
		{
			_contentPosition = null;
			base.OnPreviousPageCommand();
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.NavigationCommands.NextPage" /> routed command.</summary>
	protected override void OnNextPageCommand()
	{
		if (CanGoToNextPage)
		{
			_contentPosition = null;
			base.OnNextPageCommand();
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.NavigationCommands.FirstPage" /> routed command.</summary>
	protected override void OnFirstPageCommand()
	{
		if (CanGoToPreviousPage)
		{
			_contentPosition = null;
			base.OnFirstPageCommand();
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.NavigationCommands.LastPage" /> routed command.</summary>
	protected override void OnLastPageCommand()
	{
		if (CanGoToNextPage)
		{
			_contentPosition = null;
			base.OnLastPageCommand();
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.NavigationCommands.GoToPage" /> routed command.</summary>
	/// <param name="pageNumber">The page number to go to.</param>
	protected override void OnGoToPageCommand(int pageNumber)
	{
		if (CanGoToPage(pageNumber) && MasterPageNumber != pageNumber)
		{
			_contentPosition = null;
			base.OnGoToPageCommand(pageNumber);
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.ApplicationCommands.Find" /> routed command.</summary>
	protected virtual void OnFindCommand()
	{
		if (CanShowFindToolBar)
		{
			ToggleFindToolBar(FindToolBar == null);
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.ApplicationCommands.Print" /> routed command.</summary>
	protected override void OnPrintCommand()
	{
		PrintDocumentImageableArea documentImageableArea = null;
		FlowDocument flowDocument = base.Document as FlowDocument;
		if (_printingState != null)
		{
			return;
		}
		if (flowDocument != null)
		{
			XpsDocumentWriter xpsDocumentWriter = PrintQueue.CreateXpsDocumentWriter(ref documentImageableArea);
			if (xpsDocumentWriter != null && documentImageableArea != null)
			{
				FlowDocumentPaginator flowDocumentPaginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator as FlowDocumentPaginator;
				_printingState = new FlowDocumentPrintingState();
				_printingState.XpsDocumentWriter = xpsDocumentWriter;
				_printingState.PageSize = flowDocumentPaginator.PageSize;
				_printingState.PagePadding = flowDocument.PagePadding;
				_printingState.IsSelectionEnabled = base.IsSelectionEnabled;
				CommandManager.InvalidateRequerySuggested();
				xpsDocumentWriter.WritingCompleted += HandlePrintCompleted;
				xpsDocumentWriter.WritingCancelled += HandlePrintCancelled;
				CommandManager.AddPreviewCanExecuteHandler(this, PreviewCanExecuteRoutedEventHandler);
				ReadOnlyCollection<DocumentPageView> pageViews = base.PageViews;
				for (int i = 0; i < pageViews.Count; i++)
				{
					pageViews[i].SuspendLayout();
				}
				if (base.IsSelectionEnabled)
				{
					base.IsSelectionEnabled = false;
				}
				flowDocumentPaginator.PageSize = new Size(documentImageableArea.MediaSizeWidth, documentImageableArea.MediaSizeHeight);
				Thickness thickness = flowDocument.ComputePageMargin();
				flowDocument.PagePadding = new Thickness(Math.Max(documentImageableArea.OriginWidth, thickness.Left), Math.Max(documentImageableArea.OriginHeight, thickness.Top), Math.Max(documentImageableArea.MediaSizeWidth - (documentImageableArea.OriginWidth + documentImageableArea.ExtentWidth), thickness.Right), Math.Max(documentImageableArea.MediaSizeHeight - (documentImageableArea.OriginHeight + documentImageableArea.ExtentHeight), thickness.Bottom));
				xpsDocumentWriter.WriteAsync(flowDocumentPaginator);
			}
		}
		else
		{
			base.OnPrintCommand();
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.ApplicationCommands.CancelPrint" /> routed command.</summary>
	protected override void OnCancelPrintCommand()
	{
		if (_printingState != null)
		{
			_printingState.XpsDocumentWriter.CancelAsync();
		}
		else
		{
			base.OnCancelPrintCommand();
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.NavigationCommands.IncreaseZoom" /> routed command.</summary>
	protected virtual void OnIncreaseZoomCommand()
	{
		if (CanIncreaseZoom)
		{
			SetCurrentValueInternal(ZoomProperty, Math.Min(Zoom + ZoomIncrement, MaxZoom));
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.NavigationCommands.DecreaseZoom" /> routed command.</summary>
	protected virtual void OnDecreaseZoomCommand()
	{
		if (CanDecreaseZoom)
		{
			SetCurrentValueInternal(ZoomProperty, Math.Max(Zoom - ZoomIncrement, MinZoom));
		}
	}

	internal override bool BuildRouteCore(EventRoute route, RoutedEventArgs args)
	{
		if (base.Document is DependencyObject dependencyObject && LogicalTreeHelper.GetParent(dependencyObject) != this && route.PeekBranchNode() is DependencyObject dependencyObject2 && DocumentViewerHelper.IsLogicalDescendent(dependencyObject2, dependencyObject))
		{
			FrameworkElement.AddIntermediateElementsToRoute(LogicalTreeHelper.GetParent(dependencyObject), route, args, LogicalTreeHelper.GetParent(dependencyObject2));
		}
		return base.BuildRouteCore(route, args);
	}

	internal override bool InvalidateAutomationAncestorsCore(Stack<DependencyObject> branchNodeStack, out bool continuePastCoreTree)
	{
		bool flag = true;
		if (base.Document is DependencyObject dependencyObject && LogicalTreeHelper.GetParent(dependencyObject) != this)
		{
			DependencyObject dependencyObject2 = ((branchNodeStack.Count > 0) ? branchNodeStack.Peek() : null);
			if (dependencyObject2 != null && DocumentViewerHelper.IsLogicalDescendent(dependencyObject2, dependencyObject))
			{
				flag = FrameworkElement.InvalidateAutomationIntermediateElements(LogicalTreeHelper.GetParent(dependencyObject), LogicalTreeHelper.GetParent(dependencyObject2));
			}
		}
		return flag & base.InvalidateAutomationAncestorsCore(branchNodeStack, out continuePastCoreTree);
	}

	internal bool BringPointIntoView(Point point)
	{
		ReadOnlyCollection<DocumentPageView> pageViews = base.PageViews;
		bool result = false;
		if (pageViews.Count > 0)
		{
			Rect[] array = new Rect[pageViews.Count];
			int i;
			for (i = 0; i < pageViews.Count; i++)
			{
				Rect rect = new Rect(pageViews[i].RenderSize);
				rect = pageViews[i].TransformToAncestor(this).TransformBounds(rect);
				array[i] = rect;
			}
			for (i = 0; i < array.Length && !array[i].Contains(point); i++)
			{
			}
			if (i >= array.Length)
			{
				Rect rect = array[0];
				for (i = 1; i < array.Length; i++)
				{
					rect.Union(array[i]);
				}
				if (DoubleUtil.LessThan(point.X, rect.Left))
				{
					if (CanGoToPreviousPage)
					{
						OnPreviousPageCommand();
						result = true;
					}
				}
				else if (DoubleUtil.GreaterThan(point.X, rect.Right))
				{
					if (CanGoToNextPage)
					{
						OnNextPageCommand();
						result = true;
					}
				}
				else if (DoubleUtil.LessThan(point.Y, rect.Top))
				{
					if (CanGoToPreviousPage)
					{
						OnPreviousPageCommand();
						result = true;
					}
				}
				else if (DoubleUtil.GreaterThan(point.Y, rect.Bottom) && CanGoToNextPage)
				{
					OnNextPageCommand();
					result = true;
				}
			}
		}
		return result;
	}

	internal object BringContentPositionIntoView(object arg)
	{
		PrivateBringContentPositionIntoView(arg, isAsyncRequest: false);
		return null;
	}

	private void HandleLayoutUpdated(object sender, EventArgs e)
	{
		if (base.Document == null || _printingState != null || !(base.Document.DocumentPaginator is DynamicDocumentPaginator dynamicDocumentPaginator))
		{
			return;
		}
		if (_contentPosition == null)
		{
			DocumentPageView masterPageView = GetMasterPageView();
			if (masterPageView != null && masterPageView.DocumentPage != null)
			{
				_contentPosition = dynamicDocumentPaginator.GetPagePosition(masterPageView.DocumentPage);
			}
			if (_contentPosition == ContentPosition.Missing)
			{
				_contentPosition = null;
			}
		}
		else
		{
			PrivateBringContentPositionIntoView(_contentPosition, isAsyncRequest: true);
		}
	}

	private void HandleGetPageNumberCompleted(object sender, GetPageNumberCompletedEventArgs e)
	{
		if (base.Document != null && sender == base.Document.DocumentPaginator && e != null && !e.Cancelled && e.Error == null && e.UserState == _bringContentPositionIntoViewToken)
		{
			int pageNumber = e.PageNumber + 1;
			OnGoToPageCommand(pageNumber);
		}
	}

	private void HandleAllBreakRecordsInvalidated(object sender, EventArgs e)
	{
		ReadOnlyCollection<DocumentPageView> pageViews = base.PageViews;
		for (int i = 0; i < pageViews.Count; i++)
		{
			pageViews[i].DuplicateVisual();
		}
	}

	private bool IsValidContentPositionForDocument(IDocumentPaginatorSource document, ContentPosition contentPosition)
	{
		FlowDocument flowDocument = document as FlowDocument;
		TextPointer textPointer = contentPosition as TextPointer;
		if (flowDocument != null && textPointer != null)
		{
			return flowDocument.ContentStart.TextContainer == textPointer.TextContainer;
		}
		return true;
	}

	private void PrivateBringContentPositionIntoView(object arg, bool isAsyncRequest)
	{
		if (arg is ContentPosition contentPosition && base.Document != null && base.Document.DocumentPaginator is DynamicDocumentPaginator dynamicDocumentPaginator && IsValidContentPositionForDocument(base.Document, contentPosition))
		{
			dynamicDocumentPaginator.CancelAsync(_bringContentPositionIntoViewToken);
			if (isAsyncRequest)
			{
				dynamicDocumentPaginator.GetPageNumberAsync(contentPosition, _bringContentPositionIntoViewToken);
			}
			else
			{
				int pageNumber = dynamicDocumentPaginator.GetPageNumber(contentPosition) + 1;
				OnGoToPageCommand(pageNumber);
			}
			_contentPosition = contentPosition;
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
			ReadOnlyCollection<DocumentPageView> pageViews = base.PageViews;
			for (int i = 0; i < pageViews.Count; i++)
			{
				pageViews[i].ResumeLayout();
			}
			if (_printingState.IsSelectionEnabled)
			{
				base.IsSelectionEnabled = true;
			}
			CommandManager.RemovePreviewCanExecuteHandler(this, PreviewCanExecuteRoutedEventHandler);
			_printingState.XpsDocumentWriter.WritingCompleted -= HandlePrintCompleted;
			_printingState.XpsDocumentWriter.WritingCancelled -= HandlePrintCancelled;
			((FlowDocument)base.Document).PagePadding = _printingState.PagePadding;
			base.Document.DocumentPaginator.PageSize = _printingState.PageSize;
			_printingState = null;
			CommandManager.InvalidateRequerySuggested();
		}
	}

	private void ApplyZoom()
	{
		ReadOnlyCollection<DocumentPageView> pageViews = base.PageViews;
		for (int i = 0; i < pageViews.Count; i++)
		{
			pageViews[i].SetPageZoom(Zoom / 100.0);
		}
	}

	private void ToggleFindToolBar(bool enable)
	{
		Invariant.Assert(enable == (FindToolBar == null));
		DocumentViewerHelper.ToggleFindToolBar(_findToolBarHost, OnFindInvoked, enable);
	}

	private void OnFindInvoked(object sender, EventArgs e)
	{
		FindToolBar findToolBar = FindToolBar;
		if (findToolBar == null || base.TextEditor == null)
		{
			return;
		}
		Focus();
		ITextRange textRange = Find(findToolBar);
		if (textRange != null && !textRange.IsEmpty)
		{
			if (textRange.Start is ContentPosition)
			{
				_contentPosition = (ContentPosition)textRange.Start;
				int pageNumber = ((DynamicDocumentPaginator)base.Document.DocumentPaginator).GetPageNumber(_contentPosition) + 1;
				OnBringIntoView(this, Rect.Empty, pageNumber);
			}
		}
		else
		{
			DocumentViewerHelper.ShowFindUnsuccessfulMessage(findToolBar);
		}
	}

	private void ZoomChanged(double oldValue, double newValue)
	{
		if (!DoubleUtil.AreClose(oldValue, newValue))
		{
			UpdateCanIncreaseZoom();
			UpdateCanDecreaseZoom();
			ApplyZoom();
		}
	}

	private void UpdateCanIncreaseZoom()
	{
		SetValue(CanIncreaseZoomPropertyKey, BooleanBoxes.Box(DoubleUtil.GreaterThan(MaxZoom, Zoom)));
	}

	private void UpdateCanDecreaseZoom()
	{
		SetValue(CanDecreaseZoomPropertyKey, BooleanBoxes.Box(DoubleUtil.LessThan(MinZoom, Zoom)));
	}

	private void MaxZoomChanged(double oldValue, double newValue)
	{
		CoerceValue(ZoomProperty);
		UpdateCanIncreaseZoom();
	}

	private void MinZoomChanged(double oldValue, double newValue)
	{
		CoerceValue(MaxZoomProperty);
		CoerceValue(ZoomProperty);
		UpdateCanIncreaseZoom();
		UpdateCanDecreaseZoom();
	}

	private static void CreateCommandBindings()
	{
		ExecutedRoutedEventHandler executedRoutedEventHandler = ExecutedRoutedEventHandler;
		CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = CanExecuteRoutedEventHandler;
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentPageViewer), ApplicationCommands.Find, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentPageViewer), NavigationCommands.IncreaseZoom, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.OemPlus, ModifierKeys.Control));
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentPageViewer), NavigationCommands.DecreaseZoom, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.OemMinus, ModifierKeys.Control));
		CommandManager.RegisterClassInputBinding(typeof(FlowDocumentPageViewer), new InputBinding(NavigationCommands.PreviousPage, new KeyGesture(Key.Left)));
		CommandManager.RegisterClassInputBinding(typeof(FlowDocumentPageViewer), new InputBinding(NavigationCommands.PreviousPage, new KeyGesture(Key.Up)));
		CommandManager.RegisterClassInputBinding(typeof(FlowDocumentPageViewer), new InputBinding(NavigationCommands.PreviousPage, new KeyGesture(Key.Prior)));
		CommandManager.RegisterClassInputBinding(typeof(FlowDocumentPageViewer), new InputBinding(NavigationCommands.NextPage, new KeyGesture(Key.Right)));
		CommandManager.RegisterClassInputBinding(typeof(FlowDocumentPageViewer), new InputBinding(NavigationCommands.NextPage, new KeyGesture(Key.Down)));
		CommandManager.RegisterClassInputBinding(typeof(FlowDocumentPageViewer), new InputBinding(NavigationCommands.NextPage, new KeyGesture(Key.Next)));
		CommandManager.RegisterClassInputBinding(typeof(FlowDocumentPageViewer), new InputBinding(NavigationCommands.FirstPage, new KeyGesture(Key.Home)));
		CommandManager.RegisterClassInputBinding(typeof(FlowDocumentPageViewer), new InputBinding(NavigationCommands.FirstPage, new KeyGesture(Key.Home, ModifierKeys.Control)));
		CommandManager.RegisterClassInputBinding(typeof(FlowDocumentPageViewer), new InputBinding(NavigationCommands.LastPage, new KeyGesture(Key.End)));
		CommandManager.RegisterClassInputBinding(typeof(FlowDocumentPageViewer), new InputBinding(NavigationCommands.LastPage, new KeyGesture(Key.End, ModifierKeys.Control)));
	}

	private static void CanExecuteRoutedEventHandler(object target, CanExecuteRoutedEventArgs args)
	{
		FlowDocumentPageViewer flowDocumentPageViewer = target as FlowDocumentPageViewer;
		Invariant.Assert(flowDocumentPageViewer != null, "Target of QueryEnabledEvent must be FlowDocumentPageViewer.");
		Invariant.Assert(args != null, "args cannot be null.");
		if (args.Command == ApplicationCommands.Find)
		{
			args.CanExecute = flowDocumentPageViewer.CanShowFindToolBar;
		}
		else
		{
			args.CanExecute = true;
		}
	}

	private static void ExecutedRoutedEventHandler(object target, ExecutedRoutedEventArgs args)
	{
		FlowDocumentPageViewer flowDocumentPageViewer = target as FlowDocumentPageViewer;
		Invariant.Assert(flowDocumentPageViewer != null, "Target of ExecuteEvent must be FlowDocumentPageViewer.");
		Invariant.Assert(args != null, "args cannot be null.");
		if (args.Command == NavigationCommands.IncreaseZoom)
		{
			flowDocumentPageViewer.OnIncreaseZoomCommand();
		}
		else if (args.Command == NavigationCommands.DecreaseZoom)
		{
			flowDocumentPageViewer.OnDecreaseZoomCommand();
		}
		else if (args.Command == ApplicationCommands.Find)
		{
			flowDocumentPageViewer.OnFindCommand();
		}
		else
		{
			Invariant.Assert(condition: false, "Command not handled in ExecutedRoutedEventHandler.");
		}
	}

	private void PreviewCanExecuteRoutedEventHandler(object target, CanExecuteRoutedEventArgs args)
	{
		FlowDocumentPageViewer obj = target as FlowDocumentPageViewer;
		Invariant.Assert(obj != null, "Target of PreviewCanExecuteRoutedEventHandler must be FlowDocumentPageViewer.");
		Invariant.Assert(args != null, "args cannot be null.");
		if (obj._printingState != null && args.Command != ApplicationCommands.CancelPrint)
		{
			args.CanExecute = false;
			args.Handled = true;
		}
	}

	private static void KeyDownHandler(object sender, KeyEventArgs e)
	{
		DocumentViewerHelper.KeyDownHelper(e, ((FlowDocumentPageViewer)sender)._findToolBarHost);
	}

	private static object CoerceZoom(DependencyObject d, object value)
	{
		Invariant.Assert(d != null && d is FlowDocumentPageViewer);
		FlowDocumentPageViewer flowDocumentPageViewer = (FlowDocumentPageViewer)d;
		double value2 = (double)value;
		double maxZoom = flowDocumentPageViewer.MaxZoom;
		if (DoubleUtil.LessThan(maxZoom, value2))
		{
			return maxZoom;
		}
		double minZoom = flowDocumentPageViewer.MinZoom;
		if (DoubleUtil.GreaterThan(minZoom, value2))
		{
			return minZoom;
		}
		return value;
	}

	private static object CoerceMaxZoom(DependencyObject d, object value)
	{
		Invariant.Assert(d != null && d is FlowDocumentPageViewer);
		double minZoom = ((FlowDocumentPageViewer)d).MinZoom;
		if (DoubleUtil.LessThan((double)value, minZoom))
		{
			return minZoom;
		}
		return value;
	}

	private static void ZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentPageViewer);
		((FlowDocumentPageViewer)d).ZoomChanged((double)e.OldValue, (double)e.NewValue);
	}

	private static void MaxZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentPageViewer);
		((FlowDocumentPageViewer)d).MaxZoomChanged((double)e.OldValue, (double)e.NewValue);
	}

	private static void MinZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentPageViewer);
		((FlowDocumentPageViewer)d).MinZoomChanged((double)e.OldValue, (double)e.NewValue);
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

	private static void UpdateCaretElement(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		FlowDocumentPageViewer flowDocumentPageViewer = (FlowDocumentPageViewer)d;
		if (flowDocumentPageViewer.Selection != null)
		{
			flowDocumentPageViewer.Selection.CaretElement?.InvalidateVisual();
		}
	}

	CustomJournalStateInternal IJournalState.GetJournalState(JournalReason journalReason)
	{
		int contentPosition = -1;
		LogicalDirection contentPositionDirection = LogicalDirection.Forward;
		if (ContentPosition is TextPointer textPointer)
		{
			contentPosition = textPointer.Offset;
			contentPositionDirection = textPointer.LogicalDirection;
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
		SetCurrentValueInternal(ZoomProperty, journalState.Zoom);
		if (journalState.ContentPosition != -1 && base.Document is FlowDocument flowDocument)
		{
			TextContainer textContainer = flowDocument.StructuralCache.TextContainer;
			if (journalState.ContentPosition <= textContainer.SymbolCount)
			{
				TextPointer arg = textContainer.CreatePointerAtOffset(journalState.ContentPosition, journalState.ContentPositionDirection);
				base.Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(BringContentPositionIntoView), arg);
			}
		}
	}
}
