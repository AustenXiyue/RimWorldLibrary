using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Commands;
using MS.Internal.Documents;
using MS.Internal.PresentationFramework;
using MS.Internal.Telemetry.PresentationFramework;
using MS.Utility;

namespace System.Windows.Controls;

/// <summary>Represents a document viewing control that can host paginated <see cref="T:System.Windows.Documents.FixedDocument" /> content such as an <see cref="T:System.Windows.Xps.Packaging.XpsDocument" />. </summary>
[TemplatePart(Name = "PART_FindToolBarHost", Type = typeof(ContentControl))]
[TemplatePart(Name = "PART_ContentHost", Type = typeof(ScrollViewer))]
public class DocumentViewer : DocumentViewerBase
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.HorizontalOffset" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.HorizontalOffset" /> dependency property. </returns>
	public static readonly DependencyProperty HorizontalOffsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.VerticalOffset" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.VerticalOffset" /> dependency property. </returns>
	public static readonly DependencyProperty VerticalOffsetProperty;

	private static readonly DependencyPropertyKey ExtentWidthPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.ExtentWidth" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.ExtentWidth" /> dependency property. </returns>
	public static readonly DependencyProperty ExtentWidthProperty;

	private static readonly DependencyPropertyKey ExtentHeightPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.ExtentHeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.ExtentHeight" /> dependency property. </returns>
	public static readonly DependencyProperty ExtentHeightProperty;

	private static readonly DependencyPropertyKey ViewportWidthPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.ViewportWidth" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.ViewportWidth" /> dependency property.</returns>
	public static readonly DependencyProperty ViewportWidthProperty;

	private static readonly DependencyPropertyKey ViewportHeightPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.ViewportHeight" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.ViewportHeight" /> dependency property.</returns>
	public static readonly DependencyProperty ViewportHeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.ShowPageBorders" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.ShowPageBorders" /> dependency property.</returns>
	public static readonly DependencyProperty ShowPageBordersProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.Zoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.Zoom" /> dependency property.</returns>
	public static readonly DependencyProperty ZoomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.MaxPagesAcross" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.MaxPagesAcross" /> dependency property. </returns>
	public static readonly DependencyProperty MaxPagesAcrossProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.VerticalPageSpacing" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.VerticalPageSpacing" /> dependency property.</returns>
	public static readonly DependencyProperty VerticalPageSpacingProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.HorizontalPageSpacing" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.HorizontalPageSpacing" /> dependency property.</returns>
	public static readonly DependencyProperty HorizontalPageSpacingProperty;

	private static readonly DependencyPropertyKey CanMoveUpPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.CanMoveUp" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.CanMoveUp" /> dependency property. </returns>
	public static readonly DependencyProperty CanMoveUpProperty;

	private static readonly DependencyPropertyKey CanMoveDownPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.CanMoveDown" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.CanMoveDown" /> dependency property.</returns>
	public static readonly DependencyProperty CanMoveDownProperty;

	private static readonly DependencyPropertyKey CanMoveLeftPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.CanMoveLeft" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.CanMoveLeft" /> dependency property. </returns>
	public static readonly DependencyProperty CanMoveLeftProperty;

	private static readonly DependencyPropertyKey CanMoveRightPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.CanMoveRight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.CanMoveRight" /> dependency property. </returns>
	public static readonly DependencyProperty CanMoveRightProperty;

	private static readonly DependencyPropertyKey CanIncreaseZoomPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.CanIncreaseZoom" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.CanIncreaseZoom" /> dependency property. </returns>
	public static readonly DependencyProperty CanIncreaseZoomProperty;

	private static readonly DependencyPropertyKey CanDecreaseZoomPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DocumentViewer.CanDecreaseZoom" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DocumentViewer.CanDecreaseZoom" /> dependency property. </returns>
	public static readonly DependencyProperty CanDecreaseZoomProperty;

	private IDocumentScrollInfo _documentScrollInfo;

	private ScrollViewer _scrollViewer;

	private ZoomPercentageConverter _zoomPercentageConverter;

	private FindToolBar _findToolbar;

	private const double _horizontalOffsetDefault = 0.0;

	private const double _verticalOffsetDefault = 0.0;

	private const double _extentWidthDefault = 0.0;

	private const double _extentHeightDefault = 0.0;

	private const double _viewportWidthDefault = 0.0;

	private const double _viewportHeightDefault = 0.0;

	private const bool _showPageBordersDefault = true;

	private const double _zoomPercentageDefault = 100.0;

	private const int _maxPagesAcrossDefault = 1;

	private const double _verticalPageSpacingDefault = 10.0;

	private const double _horizontalPageSpacingDefault = 10.0;

	private const bool _canMoveUpDefault = false;

	private const bool _canMoveDownDefault = false;

	private const bool _canMoveLeftDefault = false;

	private const bool _canMoveRightDefault = false;

	private const bool _canIncreaseZoomDefault = true;

	private const bool _canDecreaseZoomDefault = true;

	private static RoutedUICommand _viewThumbnailsCommand;

	private static RoutedUICommand _fitToWidthCommand;

	private static RoutedUICommand _fitToHeightCommand;

	private static RoutedUICommand _fitToMaxPagesAcrossCommand;

	private static double[] _zoomLevelCollection;

	private int _zoomLevelIndex;

	private bool _zoomLevelIndexValid;

	private bool _updatingInternalZoomLevel;

	private bool _internalIDSIChange;

	private bool _pageViewCollectionChanged;

	private bool _firstDocumentAssignment = true;

	private const string _findToolBarHostName = "PART_FindToolBarHost";

	private const string _contentHostName = "PART_ContentHost";

	private static DependencyObjectType _dType;

	/// <summary>Gets the <see cref="T:System.Windows.Input.RoutedUICommand" /> that performs the <see cref="M:System.Windows.Controls.DocumentViewer.ViewThumbnails" /> operation.</summary>
	/// <returns>The routed command that performs the <see cref="M:System.Windows.Controls.DocumentViewer.ViewThumbnails" /> operation.</returns>
	public static RoutedUICommand ViewThumbnailsCommand => _viewThumbnailsCommand;

	/// <summary>Gets the <see cref="T:System.Windows.Input.RoutedUICommand" /> that performs the <see cref="M:System.Windows.Controls.DocumentViewer.FitToWidth" /> operation.</summary>
	/// <returns>The routed command that performs the <see cref="M:System.Windows.Controls.DocumentViewer.FitToWidth" /> operation.</returns>
	public static RoutedUICommand FitToWidthCommand => _fitToWidthCommand;

	/// <summary>Gets the <see cref="T:System.Windows.Input.RoutedUICommand" /> that performs the <see cref="M:System.Windows.Controls.DocumentViewer.FitToHeight" /> operation.</summary>
	/// <returns>The routed command that performs the <see cref="M:System.Windows.Controls.DocumentViewer.FitToHeight" /> operation.</returns>
	public static RoutedUICommand FitToHeightCommand => _fitToHeightCommand;

	/// <summary>Gets the <see cref="T:System.Windows.Input.RoutedUICommand" /> that performs the <see cref="P:System.Windows.Controls.DocumentViewer.MaxPagesAcross" /> operation.</summary>
	/// <returns>The routed command that performs the <see cref="P:System.Windows.Controls.DocumentViewer.MaxPagesAcross" /> operation.</returns>
	public static RoutedUICommand FitToMaxPagesAcrossCommand => _fitToMaxPagesAcrossCommand;

	/// <summary>Gets or sets the horizontal scroll position. </summary>
	/// <returns>The current horizontal scroll position specified in device independent pixels.  The initial default is 0.0.</returns>
	/// <exception cref="T:System.ArgumentException">The value specified to set is negative.</exception>
	public double HorizontalOffset
	{
		get
		{
			return (double)GetValue(HorizontalOffsetProperty);
		}
		set
		{
			SetValue(HorizontalOffsetProperty, value);
		}
	}

	/// <summary>Gets or sets the vertical scroll position.  </summary>
	/// <returns>The current vertical scroll position offset in device independent pixels.  The default is 0.0.</returns>
	public double VerticalOffset
	{
		get
		{
			return (double)GetValue(VerticalOffsetProperty);
		}
		set
		{
			SetValue(VerticalOffsetProperty, value);
		}
	}

	/// <summary>Gets the overall horizontal width of the paginated document. </summary>
	/// <returns>The current horizontal width of the content layout area specified in device independent pixels.  The default is 0.0.</returns>
	public double ExtentWidth => (double)GetValue(ExtentWidthProperty);

	/// <summary>Gets the overall vertical height of the paginated document. </summary>
	/// <returns>The overall vertical height of the paginated content specified in device independent pixels.  The default is 0.0.</returns>
	public double ExtentHeight => (double)GetValue(ExtentHeightProperty);

	/// <summary>Gets the horizontal size of the scrollable content area. </summary>
	/// <returns>The horizontal size of the scrollable content area in device independent pixels.  The default is 0.0.</returns>
	public double ViewportWidth => (double)GetValue(ViewportWidthProperty);

	/// <summary>Gets the vertical size of the scrollable content area. </summary>
	/// <returns>The vertical size of the scrollable content area  in device independent pixels.  The default is 0.0.</returns>
	public double ViewportHeight => (double)GetValue(ViewportHeightProperty);

	/// <summary>Gets or sets a value that indicates whether drop-shadow page borders are displayed. </summary>
	/// <returns>true if drop-shadow borders are displayed; otherwise, false.  The default is true.</returns>
	public bool ShowPageBorders
	{
		get
		{
			return (bool)GetValue(ShowPageBordersProperty);
		}
		set
		{
			SetValue(ShowPageBordersProperty, value);
		}
	}

	/// <summary>Gets or sets the document zoom percentage. </summary>
	/// <returns>The zoom percentage expressed as a value, 5.0 to 5000.0.  The default is 100.0, which corresponds to 100.0%.</returns>
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

	/// <summary>Gets or sets a value defining the maximum number of page columns to display. </summary>
	/// <returns>The maximum number of page columns to be displayed.  The default is 1.</returns>
	public int MaxPagesAcross
	{
		get
		{
			return (int)GetValue(MaxPagesAcrossProperty);
		}
		set
		{
			SetValue(MaxPagesAcrossProperty, value);
		}
	}

	/// <summary>Gets or sets the vertical spacing between displayed pages. </summary>
	/// <returns>The vertical space between displayed pages in device independent pixels.  The default is 10.0.</returns>
	public double VerticalPageSpacing
	{
		get
		{
			return (double)GetValue(VerticalPageSpacingProperty);
		}
		set
		{
			SetValue(VerticalPageSpacingProperty, value);
		}
	}

	/// <summary>Gets or sets the horizontal space between pages. </summary>
	/// <returns>The horizontal space between displayed pages specified in device independent pixels.  The default is 10.0.</returns>
	/// <exception cref="T:System.ArgumentException">The value specified to set is negative.</exception>
	public double HorizontalPageSpacing
	{
		get
		{
			return (double)GetValue(HorizontalPageSpacingProperty);
		}
		set
		{
			SetValue(HorizontalPageSpacingProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.DocumentViewer" /> can move up more in the document. </summary>
	/// <returns>true if the control can move up more in the document; otherwise, false if the document is at the topmost edge.</returns>
	public bool CanMoveUp => (bool)GetValue(CanMoveUpProperty);

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.DocumentViewer" /> can move down more in the document. </summary>
	/// <returns>true if the control can move down more in the document; otherwise, false if the document is at the bottom.</returns>
	public bool CanMoveDown => (bool)GetValue(CanMoveDownProperty);

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.DocumentViewer" /> can move more to the left in the document. </summary>
	/// <returns>true if the control can move more left in the document; otherwise, false if the document is at the leftmost edge.</returns>
	public bool CanMoveLeft => (bool)GetValue(CanMoveLeftProperty);

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.DocumentViewer" /> can move more to the right in the document. </summary>
	/// <returns>true if the control can move more to the right in the document; otherwise, false if the document is at the rightmost edge.</returns>
	public bool CanMoveRight => (bool)GetValue(CanMoveRightProperty);

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.DocumentViewer" /> can zoom in more. </summary>
	/// <returns>true if the control can zoom in more; otherwise, false.</returns>
	public bool CanIncreaseZoom => (bool)GetValue(CanIncreaseZoomProperty);

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.DocumentViewer" /> can zoom out more. </summary>
	/// <returns>true if the control can zoom out more; otherwise, false.</returns>
	public bool CanDecreaseZoom => (bool)GetValue(CanDecreaseZoomProperty);

	internal ITextSelection TextSelection
	{
		get
		{
			if (base.TextEditor != null)
			{
				return base.TextEditor.Selection;
			}
			return null;
		}
	}

	internal IDocumentScrollInfo DocumentScrollInfo => _documentScrollInfo;

	internal ScrollViewer ScrollViewer => _scrollViewer;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static DocumentViewer()
	{
		HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(DocumentViewer), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHorizontalOffsetChanged), ValidateOffset);
		VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(double), typeof(DocumentViewer), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVerticalOffsetChanged), ValidateOffset);
		ExtentWidthPropertyKey = DependencyProperty.RegisterReadOnly("ExtentWidth", typeof(double), typeof(DocumentViewer), new FrameworkPropertyMetadata(0.0, OnExtentWidthChanged));
		ExtentWidthProperty = ExtentWidthPropertyKey.DependencyProperty;
		ExtentHeightPropertyKey = DependencyProperty.RegisterReadOnly("ExtentHeight", typeof(double), typeof(DocumentViewer), new FrameworkPropertyMetadata(0.0, OnExtentHeightChanged));
		ExtentHeightProperty = ExtentHeightPropertyKey.DependencyProperty;
		ViewportWidthPropertyKey = DependencyProperty.RegisterReadOnly("ViewportWidth", typeof(double), typeof(DocumentViewer), new FrameworkPropertyMetadata(0.0, OnViewportWidthChanged));
		ViewportWidthProperty = ViewportWidthPropertyKey.DependencyProperty;
		ViewportHeightPropertyKey = DependencyProperty.RegisterReadOnly("ViewportHeight", typeof(double), typeof(DocumentViewer), new FrameworkPropertyMetadata(0.0, OnViewportHeightChanged));
		ViewportHeightProperty = ViewportHeightPropertyKey.DependencyProperty;
		ShowPageBordersProperty = DependencyProperty.Register("ShowPageBorders", typeof(bool), typeof(DocumentViewer), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnShowPageBordersChanged));
		ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(DocumentViewer), new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnZoomChanged, CoerceZoom));
		MaxPagesAcrossProperty = DependencyProperty.Register("MaxPagesAcross", typeof(int), typeof(DocumentViewer), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMaxPagesAcrossChanged), ValidateMaxPagesAcross);
		VerticalPageSpacingProperty = DependencyProperty.Register("VerticalPageSpacing", typeof(double), typeof(DocumentViewer), new FrameworkPropertyMetadata(10.0, OnVerticalPageSpacingChanged), ValidatePageSpacing);
		HorizontalPageSpacingProperty = DependencyProperty.Register("HorizontalPageSpacing", typeof(double), typeof(DocumentViewer), new FrameworkPropertyMetadata(10.0, OnHorizontalPageSpacingChanged), ValidatePageSpacing);
		CanMoveUpPropertyKey = DependencyProperty.RegisterReadOnly("CanMoveUp", typeof(bool), typeof(DocumentViewer), new FrameworkPropertyMetadata(false));
		CanMoveUpProperty = CanMoveUpPropertyKey.DependencyProperty;
		CanMoveDownPropertyKey = DependencyProperty.RegisterReadOnly("CanMoveDown", typeof(bool), typeof(DocumentViewer), new FrameworkPropertyMetadata(false));
		CanMoveDownProperty = CanMoveDownPropertyKey.DependencyProperty;
		CanMoveLeftPropertyKey = DependencyProperty.RegisterReadOnly("CanMoveLeft", typeof(bool), typeof(DocumentViewer), new FrameworkPropertyMetadata(false));
		CanMoveLeftProperty = CanMoveLeftPropertyKey.DependencyProperty;
		CanMoveRightPropertyKey = DependencyProperty.RegisterReadOnly("CanMoveRight", typeof(bool), typeof(DocumentViewer), new FrameworkPropertyMetadata(false));
		CanMoveRightProperty = CanMoveRightPropertyKey.DependencyProperty;
		CanIncreaseZoomPropertyKey = DependencyProperty.RegisterReadOnly("CanIncreaseZoom", typeof(bool), typeof(DocumentViewer), new FrameworkPropertyMetadata(true));
		CanIncreaseZoomProperty = CanIncreaseZoomPropertyKey.DependencyProperty;
		CanDecreaseZoomPropertyKey = DependencyProperty.RegisterReadOnly("CanDecreaseZoom", typeof(bool), typeof(DocumentViewer), new FrameworkPropertyMetadata(true));
		CanDecreaseZoomProperty = CanDecreaseZoomPropertyKey.DependencyProperty;
		_zoomLevelCollection = new double[22]
		{
			5000.0, 4000.0, 3200.0, 2400.0, 2000.0, 1600.0, 1200.0, 800.0, 400.0, 300.0,
			200.0, 175.0, 150.0, 125.0, 100.0, 75.0, 66.0, 50.0, 33.0, 25.0,
			10.0, 5.0
		};
		CreateCommandBindings();
		RegisterMetadata();
		ControlsTraceLogger.AddControl(TelemetryControls.DocumentViewer);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DocumentViewer" /> class.</summary>
	public DocumentViewer()
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXInstantiated);
		SetUp();
	}

	/// <summary>Displays a thumbnail representation of the pages.</summary>
	public void ViewThumbnails()
	{
		OnViewThumbnailsCommand();
	}

	/// <summary>Fits a single page to the width of the current viewport. </summary>
	public void FitToWidth()
	{
		OnFitToWidthCommand();
	}

	/// <summary>Fits a single page to the height of the current viewport. </summary>
	public void FitToHeight()
	{
		OnFitToHeightCommand();
	}

	/// <summary>Fits the document to the current <see cref="P:System.Windows.Controls.DocumentViewer.MaxPagesAcross" /> property setting.</summary>
	public void FitToMaxPagesAcross()
	{
		OnFitToMaxPagesAcrossCommand();
	}

	/// <summary>Fits the document to a specified maximum number of page widths.</summary>
	/// <param name="pagesAcross">The maximum number of pages to fit in the current <see cref="P:System.Windows.Controls.DocumentViewer.ExtentWidth" />. </param>
	public void FitToMaxPagesAcross(int pagesAcross)
	{
		if (ValidateMaxPagesAcross(pagesAcross))
		{
			if (_documentScrollInfo != null)
			{
				_documentScrollInfo.FitColumns(pagesAcross);
			}
			return;
		}
		throw new ArgumentOutOfRangeException("pagesAcross");
	}

	/// <summary>Moves focus to the find toolbar to search the document content.</summary>
	public void Find()
	{
		OnFindCommand();
	}

	/// <summary>Scrolls up one viewport.</summary>
	public void ScrollPageUp()
	{
		OnScrollPageUpCommand();
	}

	/// <summary>Scrolls down one viewport.</summary>
	public void ScrollPageDown()
	{
		OnScrollPageDownCommand();
	}

	/// <summary>Scrolls left one viewport.</summary>
	public void ScrollPageLeft()
	{
		OnScrollPageLeftCommand();
	}

	/// <summary>Scrolls right one viewport.</summary>
	public void ScrollPageRight()
	{
		OnScrollPageRightCommand();
	}

	/// <summary>Scrolls the document content up 16 device independent pixels.</summary>
	public void MoveUp()
	{
		OnMoveUpCommand();
	}

	/// <summary>Scrolls the document content down 16 device independent pixels.</summary>
	public void MoveDown()
	{
		OnMoveDownCommand();
	}

	/// <summary>Scrolls the document content left 16 device independent pixels.</summary>
	public void MoveLeft()
	{
		OnMoveLeftCommand();
	}

	/// <summary>Scrolls the document content right 16 device independent pixels.</summary>
	public void MoveRight()
	{
		OnMoveRightCommand();
	}

	/// <summary>Zooms in on the document content by one zoom step. </summary>
	public void IncreaseZoom()
	{
		OnIncreaseZoomCommand();
	}

	/// <summary>Zooms out of the document content by one zoom step. </summary>
	public void DecreaseZoom()
	{
		OnDecreaseZoomCommand();
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" /> method.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		FindContentHost();
		InstantiateFindToolBar();
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXStyleCreated);
		if (base.ContextMenu == null)
		{
			base.ContextMenu = null;
		}
	}

	/// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for this <see cref="T:System.Windows.Controls.DocumentViewer" /> control.</summary>
	/// <returns>The new <see cref="T:System.Windows.Automation.Peers.DocumentViewerAutomationPeer" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new DocumentViewerAutomationPeer(this);
	}

	/// <summary>Responds to calls when the document to display is changed.</summary>
	protected override void OnDocumentChanged()
	{
		if (!(base.Document is FixedDocument) && !(base.Document is FixedDocumentSequence) && base.Document != null)
		{
			throw new NotSupportedException(SR.DocumentViewerOnlySupportsFixedDocumentSequence);
		}
		base.OnDocumentChanged();
		AttachContent();
		if (_findToolbar != null)
		{
			_findToolbar.DocumentLoaded = base.Document != null;
		}
		if (!_firstDocumentAssignment)
		{
			OnGoToPageCommand(1);
		}
		_firstDocumentAssignment = false;
	}

	/// <summary>Responds to the <see cref="M:System.Windows.Controls.Primitives.DocumentViewerBase.OnBringIntoView(System.Windows.DependencyObject,System.Windows.Rect,System.Int32)" /> method from the <see cref="T:System.Windows.Controls.Primitives.DocumentViewerBase" /> implementation.</summary>
	/// <param name="element">The object to make visible.</param>
	/// <param name="rect">The rectangular region of the <paramref name="element" /> to make visible.</param>
	/// <param name="pageNumber">The number of the page to be viewed.</param>
	protected override void OnBringIntoView(DependencyObject element, Rect rect, int pageNumber)
	{
		int num = pageNumber - 1;
		if (num >= 0 && num < base.PageCount)
		{
			_documentScrollInfo.MakeVisible(element, rect, num);
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.Primitives.DocumentViewerBase.PreviousPage" /> method.</summary>
	protected override void OnPreviousPageCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.ScrollToPreviousRow();
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.Primitives.DocumentViewerBase.NextPage" /> method.</summary>
	protected override void OnNextPageCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.ScrollToNextRow();
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.Primitives.DocumentViewerBase.FirstPage" /> method.</summary>
	protected override void OnFirstPageCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.MakePageVisible(0);
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.Primitives.DocumentViewerBase.LastPage" /> method.</summary>
	protected override void OnLastPageCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.MakePageVisible(base.PageCount - 1);
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.Primitives.DocumentViewerBase.GoToPage(System.Int32)" /> method.</summary>
	/// <param name="pageNumber">The page number to position to.</param>
	protected override void OnGoToPageCommand(int pageNumber)
	{
		if (CanGoToPage(pageNumber) && _documentScrollInfo != null)
		{
			Invariant.Assert(pageNumber > 0, "PageNumber must be positive.");
			_documentScrollInfo.MakePageVisible(pageNumber - 1);
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.ViewThumbnails" /> method.</summary>
	protected virtual void OnViewThumbnailsCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.ViewThumbnails();
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.FitToWidth" /> method.</summary>
	protected virtual void OnFitToWidthCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.FitToPageWidth();
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.FitToHeight" /> method.</summary>
	protected virtual void OnFitToHeightCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.FitToPageHeight();
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.FitToMaxPagesAcross" /> method.</summary>
	protected virtual void OnFitToMaxPagesAcrossCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.FitColumns(MaxPagesAcross);
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.FitToMaxPagesAcross(System.Int32)" /> method.</summary>
	/// <param name="pagesAcross">The number of pages to fit across the content area.</param>
	protected virtual void OnFitToMaxPagesAcrossCommand(int pagesAcross)
	{
		if (ValidateMaxPagesAcross(pagesAcross))
		{
			if (_documentScrollInfo != null)
			{
				_documentScrollInfo.FitColumns(pagesAcross);
			}
			return;
		}
		throw new ArgumentOutOfRangeException("pagesAcross");
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.Find" /> method.</summary>
	protected virtual void OnFindCommand()
	{
		GoToFind();
	}

	/// <summary>Responds to <see cref="E:System.Windows.UIElement.KeyDown" /> events.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		e = ProcessFindKeys(e);
		base.OnKeyDown(e);
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.ScrollPageUp" /> method.</summary>
	protected virtual void OnScrollPageUpCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.PageUp();
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.ScrollPageDown" /> method.</summary>
	protected virtual void OnScrollPageDownCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.PageDown();
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.ScrollPageLeft" /> method.</summary>
	protected virtual void OnScrollPageLeftCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.PageLeft();
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.ScrollPageRight" /> method.</summary>
	protected virtual void OnScrollPageRightCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.PageRight();
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.MoveUp" /> method.</summary>
	protected virtual void OnMoveUpCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.LineUp();
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.MoveDown" /> method.</summary>
	protected virtual void OnMoveDownCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.LineDown();
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.MoveLeft" /> method.</summary>
	protected virtual void OnMoveLeftCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.LineLeft();
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.MoveRight" /> method.</summary>
	protected virtual void OnMoveRightCommand()
	{
		if (_documentScrollInfo != null)
		{
			_documentScrollInfo.LineRight();
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.IncreaseZoom" /> method.</summary>
	protected virtual void OnIncreaseZoomCommand()
	{
		if (CanIncreaseZoom)
		{
			_ = Zoom;
			FindZoomLevelIndex();
			if (_zoomLevelIndex > 0)
			{
				_zoomLevelIndex--;
			}
			_updatingInternalZoomLevel = true;
			Zoom = _zoomLevelCollection[_zoomLevelIndex];
			_updatingInternalZoomLevel = false;
		}
	}

	/// <summary>Responds to calls to the <see cref="M:System.Windows.Controls.DocumentViewer.DecreaseZoom" /> method.</summary>
	protected virtual void OnDecreaseZoomCommand()
	{
		if (CanDecreaseZoom)
		{
			double zoom = Zoom;
			FindZoomLevelIndex();
			if (zoom == _zoomLevelCollection[_zoomLevelIndex] && _zoomLevelIndex < _zoomLevelCollection.Length - 1)
			{
				_zoomLevelIndex++;
			}
			_updatingInternalZoomLevel = true;
			Zoom = _zoomLevelCollection[_zoomLevelIndex];
			_updatingInternalZoomLevel = false;
		}
	}

	/// <summary>Returns a collection of <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> elements that are currently displayed.</summary>
	/// <returns>The collection of <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> elements that are currently displayed in the <see cref="T:System.Windows.Controls.DocumentViewer" /> control.</returns>
	/// <param name="changed">When this method returns, contains true if the collection of <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> elements changed after the last call to <see cref="M:System.Windows.Controls.DocumentViewer.GetPageViewsCollection(System.Boolean@)" />; otherwise, false.</param>
	protected override ReadOnlyCollection<DocumentPageView> GetPageViewsCollection(out bool changed)
	{
		ReadOnlyCollection<DocumentPageView> readOnlyCollection = null;
		changed = _pageViewCollectionChanged;
		_pageViewCollectionChanged = false;
		if (_documentScrollInfo != null && _documentScrollInfo.PageViews != null)
		{
			return _documentScrollInfo.PageViews;
		}
		return new ReadOnlyCollection<DocumentPageView>(new List<DocumentPageView>(0));
	}

	/// <summary>Responds to <see cref="E:System.Windows.UIElement.MouseLeftButtonDown" /> events.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonDown(e);
		if (!e.Handled)
		{
			Focus();
			e.Handled = true;
		}
	}

	/// <summary>Responds to <see cref="E:System.Windows.UIElement.PreviewMouseWheel" /> events.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
	{
		if (!e.Handled && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
		{
			e.Handled = true;
			if (e.Delta < 0)
			{
				DecreaseZoom();
			}
			else
			{
				IncreaseZoom();
			}
		}
	}

	internal void InvalidateDocumentScrollInfo()
	{
		_internalIDSIChange = true;
		SetValue(ExtentWidthPropertyKey, _documentScrollInfo.ExtentWidth);
		SetValue(ExtentHeightPropertyKey, _documentScrollInfo.ExtentHeight);
		SetValue(ViewportWidthPropertyKey, _documentScrollInfo.ViewportWidth);
		SetValue(ViewportHeightPropertyKey, _documentScrollInfo.ViewportHeight);
		if (HorizontalOffset != _documentScrollInfo.HorizontalOffset)
		{
			HorizontalOffset = _documentScrollInfo.HorizontalOffset;
		}
		if (VerticalOffset != _documentScrollInfo.VerticalOffset)
		{
			VerticalOffset = _documentScrollInfo.VerticalOffset;
		}
		SetValue(DocumentViewerBase.MasterPageNumberPropertyKey, _documentScrollInfo.FirstVisiblePageNumber + 1);
		double num = ScaleToZoom(_documentScrollInfo.Scale);
		if (Zoom != num)
		{
			Zoom = num;
		}
		if (MaxPagesAcross != _documentScrollInfo.MaxPagesAcross)
		{
			MaxPagesAcross = _documentScrollInfo.MaxPagesAcross;
		}
		_internalIDSIChange = false;
	}

	internal void InvalidatePageViewsInternal()
	{
		_pageViewCollectionChanged = true;
		InvalidatePageViews();
	}

	internal bool BringPointIntoView(Point point)
	{
		if (_documentScrollInfo is FrameworkElement frameworkElement)
		{
			Transform transform = TransformToDescendant(frameworkElement) as Transform;
			Rect rect = Rect.Transform(new Rect(frameworkElement.RenderSize), transform.Value);
			double num = VerticalOffset;
			double num2 = HorizontalOffset;
			if (point.Y > rect.Y + rect.Height)
			{
				num += point.Y - (rect.Y + rect.Height);
			}
			else if (point.Y < rect.Y)
			{
				num -= rect.Y - point.Y;
			}
			if (point.X < rect.X)
			{
				num2 -= rect.X - point.X;
			}
			else if (point.X > rect.X + rect.Width)
			{
				num2 += point.X - (rect.X + rect.Width);
			}
			VerticalOffset = Math.Max(num, 0.0);
			HorizontalOffset = Math.Max(num2, 0.0);
		}
		return false;
	}

	private static void CreateCommandBindings()
	{
		ExecutedRoutedEventHandler executedRoutedEventHandler = ExecutedRoutedEventHandler;
		CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = QueryEnabledHandler;
		_viewThumbnailsCommand = new RoutedUICommand(SR.DocumentViewerViewThumbnailsCommandText, "ViewThumbnailsCommand", typeof(DocumentViewer), null);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), _viewThumbnailsCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		_fitToWidthCommand = new RoutedUICommand(SR.DocumentViewerViewFitToWidthCommandText, "FitToWidthCommand", typeof(DocumentViewer), null);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), _fitToWidthCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.D2, ModifierKeys.Control));
		_fitToHeightCommand = new RoutedUICommand(SR.DocumentViewerViewFitToHeightCommandText, "FitToHeightCommand", typeof(DocumentViewer), null);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), _fitToHeightCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		_fitToMaxPagesAcrossCommand = new RoutedUICommand(SR.DocumentViewerViewFitToMaxPagesAcrossCommandText, "FitToMaxPagesAcrossCommand", typeof(DocumentViewer), null);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), _fitToMaxPagesAcrossCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), ApplicationCommands.Find, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), ComponentCommands.ScrollPageUp, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Prior);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), ComponentCommands.ScrollPageDown, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Next);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), ComponentCommands.ScrollPageLeft, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), ComponentCommands.ScrollPageRight, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), ComponentCommands.MoveUp, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Up);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), ComponentCommands.MoveDown, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Down);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), ComponentCommands.MoveLeft, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Left);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), ComponentCommands.MoveRight, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Right);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), NavigationCommands.Zoom, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), NavigationCommands.IncreaseZoom, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.Add, ModifierKeys.Control), new KeyGesture(Key.Add, ModifierKeys.Control | ModifierKeys.Shift), new KeyGesture(Key.OemPlus, ModifierKeys.Control), new KeyGesture(Key.OemPlus, ModifierKeys.Control | ModifierKeys.Shift));
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), NavigationCommands.DecreaseZoom, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.Subtract, ModifierKeys.Control), new KeyGesture(Key.Subtract, ModifierKeys.Control | ModifierKeys.Shift), new KeyGesture(Key.OemMinus, ModifierKeys.Control), new KeyGesture(Key.OemMinus, ModifierKeys.Control | ModifierKeys.Shift));
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), NavigationCommands.PreviousPage, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.Prior, ModifierKeys.Control));
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), NavigationCommands.NextPage, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.Next, ModifierKeys.Control));
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), NavigationCommands.FirstPage, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.Home, ModifierKeys.Control));
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewer), NavigationCommands.LastPage, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.End, ModifierKeys.Control));
		InputBinding inputBinding = new InputBinding(NavigationCommands.Zoom, new KeyGesture(Key.D1, ModifierKeys.Control));
		inputBinding.CommandParameter = 100.0;
		CommandManager.RegisterClassInputBinding(typeof(DocumentViewer), inputBinding);
		InputBinding inputBinding2 = new InputBinding(FitToMaxPagesAcrossCommand, new KeyGesture(Key.D3, ModifierKeys.Control));
		inputBinding2.CommandParameter = 1;
		CommandManager.RegisterClassInputBinding(typeof(DocumentViewer), inputBinding2);
		InputBinding inputBinding3 = new InputBinding(FitToMaxPagesAcrossCommand, new KeyGesture(Key.D4, ModifierKeys.Control));
		inputBinding3.CommandParameter = 2;
		CommandManager.RegisterClassInputBinding(typeof(DocumentViewer), inputBinding3);
	}

	private static void QueryEnabledHandler(object target, CanExecuteRoutedEventArgs args)
	{
		DocumentViewer documentViewer = target as DocumentViewer;
		Invariant.Assert(documentViewer != null, "Target of QueryEnabledEvent must be DocumentViewer.");
		Invariant.Assert(args != null, "args cannot be null.");
		if (documentViewer != null)
		{
			args.Handled = true;
			if (args.Command == ViewThumbnailsCommand || args.Command == FitToWidthCommand || args.Command == FitToHeightCommand || args.Command == FitToMaxPagesAcrossCommand || args.Command == NavigationCommands.Zoom)
			{
				args.CanExecute = true;
				return;
			}
			if (args.Command == ApplicationCommands.Find)
			{
				args.CanExecute = documentViewer.TextEditor != null;
				return;
			}
			if (args.Command == ComponentCommands.ScrollPageUp || args.Command == ComponentCommands.MoveUp)
			{
				args.CanExecute = documentViewer.CanMoveUp;
				return;
			}
			if (args.Command == ComponentCommands.ScrollPageDown || args.Command == ComponentCommands.MoveDown)
			{
				args.CanExecute = documentViewer.CanMoveDown;
				return;
			}
			if (args.Command == ComponentCommands.ScrollPageLeft || args.Command == ComponentCommands.MoveLeft)
			{
				args.CanExecute = documentViewer.CanMoveLeft;
				return;
			}
			if (args.Command == ComponentCommands.ScrollPageRight || args.Command == ComponentCommands.MoveRight)
			{
				args.CanExecute = documentViewer.CanMoveRight;
				return;
			}
			if (args.Command == NavigationCommands.IncreaseZoom)
			{
				args.CanExecute = documentViewer.CanIncreaseZoom;
				return;
			}
			if (args.Command == NavigationCommands.DecreaseZoom)
			{
				args.CanExecute = documentViewer.CanDecreaseZoom;
				return;
			}
			if (args.Command == NavigationCommands.PreviousPage || args.Command == NavigationCommands.FirstPage)
			{
				args.CanExecute = documentViewer.CanGoToPreviousPage;
				return;
			}
			if (args.Command == NavigationCommands.NextPage || args.Command == NavigationCommands.LastPage)
			{
				args.CanExecute = documentViewer.CanGoToNextPage;
				return;
			}
			if (args.Command == NavigationCommands.GoToPage)
			{
				args.CanExecute = documentViewer.Document != null;
				return;
			}
			args.Handled = false;
			Invariant.Assert(condition: false, "Command not handled in QueryEnabledHandler.");
		}
	}

	private static void ExecutedRoutedEventHandler(object target, ExecutedRoutedEventArgs args)
	{
		DocumentViewer documentViewer = target as DocumentViewer;
		Invariant.Assert(documentViewer != null, "Target of ExecuteEvent must be DocumentViewer.");
		Invariant.Assert(args != null, "args cannot be null.");
		if (documentViewer != null)
		{
			if (args.Command == ViewThumbnailsCommand)
			{
				documentViewer.OnViewThumbnailsCommand();
			}
			else if (args.Command == FitToWidthCommand)
			{
				documentViewer.OnFitToWidthCommand();
			}
			else if (args.Command == FitToHeightCommand)
			{
				documentViewer.OnFitToHeightCommand();
			}
			else if (args.Command == FitToMaxPagesAcrossCommand)
			{
				DoFitToMaxPagesAcross(documentViewer, args.Parameter);
			}
			else if (args.Command == ApplicationCommands.Find)
			{
				documentViewer.OnFindCommand();
			}
			else if (args.Command == ComponentCommands.ScrollPageUp)
			{
				documentViewer.OnScrollPageUpCommand();
			}
			else if (args.Command == ComponentCommands.ScrollPageDown)
			{
				documentViewer.OnScrollPageDownCommand();
			}
			else if (args.Command == ComponentCommands.ScrollPageLeft)
			{
				documentViewer.OnScrollPageLeftCommand();
			}
			else if (args.Command == ComponentCommands.ScrollPageRight)
			{
				documentViewer.OnScrollPageRightCommand();
			}
			else if (args.Command == ComponentCommands.MoveUp)
			{
				documentViewer.OnMoveUpCommand();
			}
			else if (args.Command == ComponentCommands.MoveDown)
			{
				documentViewer.OnMoveDownCommand();
			}
			else if (args.Command == ComponentCommands.MoveLeft)
			{
				documentViewer.OnMoveLeftCommand();
			}
			else if (args.Command == ComponentCommands.MoveRight)
			{
				documentViewer.OnMoveRightCommand();
			}
			else if (args.Command == NavigationCommands.Zoom)
			{
				DoZoom(documentViewer, args.Parameter);
			}
			else if (args.Command == NavigationCommands.DecreaseZoom)
			{
				documentViewer.DecreaseZoom();
			}
			else if (args.Command == NavigationCommands.IncreaseZoom)
			{
				documentViewer.IncreaseZoom();
			}
			else if (args.Command == NavigationCommands.PreviousPage)
			{
				documentViewer.PreviousPage();
			}
			else if (args.Command == NavigationCommands.NextPage)
			{
				documentViewer.NextPage();
			}
			else if (args.Command == NavigationCommands.FirstPage)
			{
				documentViewer.FirstPage();
			}
			else if (args.Command == NavigationCommands.LastPage)
			{
				documentViewer.LastPage();
			}
			else
			{
				Invariant.Assert(condition: false, "Command not handled in ExecutedRoutedEventHandler.");
			}
		}
	}

	private static void DoFitToMaxPagesAcross(DocumentViewer dv, object data)
	{
		if (data != null)
		{
			int pagesAcross = 0;
			bool flag = true;
			if (data is int)
			{
				pagesAcross = (int)data;
			}
			else if (data is string)
			{
				try
				{
					pagesAcross = Convert.ToInt32((string)data, CultureInfo.CurrentCulture);
				}
				catch (ArgumentNullException)
				{
					flag = false;
				}
				catch (FormatException)
				{
					flag = false;
				}
				catch (OverflowException)
				{
					flag = false;
				}
			}
			if (!flag)
			{
				throw new ArgumentException(SR.DocumentViewerArgumentMustBeInteger, "data");
			}
			dv.OnFitToMaxPagesAcrossCommand(pagesAcross);
			return;
		}
		throw new ArgumentNullException("data");
	}

	private static void DoZoom(DocumentViewer dv, object data)
	{
		if (data != null)
		{
			if (dv._zoomPercentageConverter == null)
			{
				dv._zoomPercentageConverter = new ZoomPercentageConverter();
			}
			object obj = dv._zoomPercentageConverter.ConvertBack(data, typeof(double), null, CultureInfo.InvariantCulture);
			if (obj == DependencyProperty.UnsetValue)
			{
				throw new ArgumentException(SR.DocumentViewerArgumentMustBePercentage, "data");
			}
			dv.Zoom = (double)obj;
			return;
		}
		throw new ArgumentNullException("data");
	}

	private static void RegisterMetadata()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DocumentViewer), new FrameworkPropertyMetadata(typeof(DocumentViewer)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(DocumentViewer));
	}

	private void SetUp()
	{
		base.IsSelectionEnabled = true;
		SetValue(TextBoxBase.AcceptsTabProperty, value: false);
		CreateIDocumentScrollInfo();
	}

	private void CreateIDocumentScrollInfo()
	{
		if (_documentScrollInfo == null)
		{
			_documentScrollInfo = new DocumentGrid();
			_documentScrollInfo.DocumentViewerOwner = this;
			if (_documentScrollInfo is FrameworkElement frameworkElement)
			{
				frameworkElement.Name = "DocumentGrid";
				frameworkElement.Focusable = false;
				frameworkElement.SetValue(KeyboardNavigation.IsTabStopProperty, value: false);
				base.TextEditorRenderScope = frameworkElement;
			}
		}
		AttachContent();
		_documentScrollInfo.VerticalPageSpacing = VerticalPageSpacing;
		_documentScrollInfo.HorizontalPageSpacing = HorizontalPageSpacing;
	}

	private void AttachContent()
	{
		_documentScrollInfo.Content = ((base.Document != null) ? (base.Document.DocumentPaginator as DynamicDocumentPaginator) : null);
		base.IsSelectionEnabled = true;
	}

	private void FindContentHost()
	{
		if (!(base.Template.FindName("PART_ContentHost", this) is ScrollViewer scrollViewer))
		{
			throw new NotSupportedException(SR.DocumentViewerStyleMustIncludeContentHost);
		}
		_scrollViewer = scrollViewer;
		_scrollViewer.Focusable = false;
		Invariant.Assert(_documentScrollInfo != null, "IDocumentScrollInfo cannot be null.");
		_scrollViewer.Content = _documentScrollInfo;
		_scrollViewer.ScrollInfo = _documentScrollInfo;
		if (_documentScrollInfo.Content != base.Document)
		{
			AttachContent();
		}
	}

	private void InstantiateFindToolBar()
	{
		if (base.Template.FindName("PART_FindToolBarHost", this) is ContentControl contentControl)
		{
			if (_findToolbar == null)
			{
				_findToolbar = new FindToolBar();
				_findToolbar.FindClicked += OnFindInvoked;
				_findToolbar.DocumentLoaded = base.Document != null;
			}
			if (!_findToolbar.IsAncestorOf(this))
			{
				((IAddChild)contentControl).AddChild((object)_findToolbar);
			}
		}
	}

	private void OnFindInvoked(object sender, EventArgs e)
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXFindBegin);
		try
		{
			if (_findToolbar == null || base.TextEditor == null)
			{
				return;
			}
			ITextRange textRange = Find(_findToolbar);
			if (textRange != null && !textRange.IsEmpty)
			{
				Focus();
				if (_documentScrollInfo != null)
				{
					_documentScrollInfo.MakeSelectionVisible();
				}
				_findToolbar.GoToTextBox();
				return;
			}
			string format = (_findToolbar.SearchUp ? SR.DocumentViewerSearchUpCompleteLabel : SR.DocumentViewerSearchDownCompleteLabel);
			format = string.Format(CultureInfo.CurrentCulture, format, _findToolbar.SearchText);
			Window parent = null;
			if (Application.Current != null && Application.Current.CheckAccess())
			{
				parent = Application.Current.MainWindow;
			}
			MS.Internal.PresentationFramework.SecurityHelper.ShowMessageBoxHelper(parent, format, SR.DocumentViewerSearchCompleteTitle, MessageBoxButton.OK, MessageBoxImage.Asterisk);
		}
		finally
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXFindEnd);
		}
	}

	private void GoToFind()
	{
		if (_findToolbar != null)
		{
			_findToolbar.GoToTextBox();
		}
	}

	private KeyEventArgs ProcessFindKeys(KeyEventArgs e)
	{
		if (_findToolbar == null || base.Document == null)
		{
			return e;
		}
		if (e.Key == Key.F3)
		{
			e.Handled = true;
			_findToolbar.SearchUp = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
			OnFindInvoked(this, EventArgs.Empty);
		}
		return e;
	}

	private void FindZoomLevelIndex()
	{
		if (_zoomLevelCollection == null)
		{
			return;
		}
		if (_zoomLevelIndex < 0 || _zoomLevelIndex >= _zoomLevelCollection.Length)
		{
			_zoomLevelIndex = 0;
			_zoomLevelIndexValid = false;
		}
		if (!_zoomLevelIndexValid)
		{
			double zoom = Zoom;
			int i;
			for (i = 0; i < _zoomLevelCollection.Length - 1 && !(zoom >= _zoomLevelCollection[i]); i++)
			{
			}
			_zoomLevelIndex = i;
			_zoomLevelIndexValid = true;
		}
	}

	private static bool DoubleValue_Validate(object value)
	{
		if (value is double d)
		{
			if (double.IsNaN(d) || double.IsInfinity(d))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private static double ScaleToZoom(double scale)
	{
		return scale * 100.0;
	}

	private static double ZoomToScale(double zoom)
	{
		return zoom / 100.0;
	}

	private static bool ValidateOffset(object value)
	{
		if (DoubleValue_Validate(value))
		{
			return (double)value >= 0.0;
		}
		return false;
	}

	private static void OnHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DocumentViewer documentViewer = (DocumentViewer)d;
		double num = (double)e.NewValue;
		if (!documentViewer._internalIDSIChange && documentViewer._documentScrollInfo != null)
		{
			documentViewer._documentScrollInfo.SetHorizontalOffset(num);
		}
		documentViewer.SetValue(CanMoveLeftPropertyKey, num > 0.0);
		documentViewer.SetValue(CanMoveRightPropertyKey, num < documentViewer.ExtentWidth - documentViewer.ViewportWidth);
	}

	private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DocumentViewer documentViewer = (DocumentViewer)d;
		double num = (double)e.NewValue;
		if (!documentViewer._internalIDSIChange && documentViewer._documentScrollInfo != null)
		{
			documentViewer._documentScrollInfo.SetVerticalOffset(num);
		}
		documentViewer.SetValue(CanMoveUpPropertyKey, num > 0.0);
		documentViewer.SetValue(CanMoveDownPropertyKey, num < documentViewer.ExtentHeight - documentViewer.ViewportHeight);
	}

	private static void OnExtentWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DocumentViewer documentViewer = (DocumentViewer)d;
		documentViewer.SetValue(CanMoveRightPropertyKey, documentViewer.HorizontalOffset < (double)e.NewValue - documentViewer.ViewportWidth);
	}

	private static void OnExtentHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DocumentViewer documentViewer = (DocumentViewer)d;
		documentViewer.SetValue(CanMoveDownPropertyKey, documentViewer.VerticalOffset < (double)e.NewValue - documentViewer.ViewportHeight);
	}

	private static void OnViewportWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DocumentViewer documentViewer = (DocumentViewer)d;
		_ = (double)e.NewValue;
		documentViewer.SetValue(CanMoveRightPropertyKey, documentViewer.HorizontalOffset < documentViewer.ExtentWidth - (double)e.NewValue);
	}

	private static void OnViewportHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DocumentViewer documentViewer = (DocumentViewer)d;
		double num = (double)e.NewValue;
		documentViewer.SetValue(CanMoveDownPropertyKey, documentViewer.VerticalOffset < documentViewer.ExtentHeight - num);
	}

	private static void OnShowPageBordersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DocumentViewer documentViewer = (DocumentViewer)d;
		if (documentViewer._documentScrollInfo != null)
		{
			documentViewer._documentScrollInfo.ShowPageBorders = (bool)e.NewValue;
		}
	}

	private static object CoerceZoom(DependencyObject d, object value)
	{
		double num = (double)value;
		if (num < DocumentViewerConstants.MinimumZoom)
		{
			return DocumentViewerConstants.MinimumZoom;
		}
		if (num > DocumentViewerConstants.MaximumZoom)
		{
			return DocumentViewerConstants.MaximumZoom;
		}
		return value;
	}

	private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DocumentViewer documentViewer = (DocumentViewer)d;
		if (documentViewer._documentScrollInfo != null)
		{
			double num = (double)e.NewValue;
			if (!documentViewer._internalIDSIChange)
			{
				EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXZoom, (int)num);
				documentViewer._documentScrollInfo.SetScale(ZoomToScale(num));
			}
			documentViewer.SetValue(CanIncreaseZoomPropertyKey, num < DocumentViewerConstants.MaximumZoom);
			documentViewer.SetValue(CanDecreaseZoomPropertyKey, num > DocumentViewerConstants.MinimumZoom);
			if (!documentViewer._updatingInternalZoomLevel)
			{
				documentViewer._zoomLevelIndexValid = false;
			}
		}
	}

	private static bool ValidateMaxPagesAcross(object value)
	{
		int num = (int)value;
		if (num > 0)
		{
			return num <= DocumentViewerConstants.MaximumMaxPagesAcross;
		}
		return false;
	}

	private static void OnMaxPagesAcrossChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DocumentViewer documentViewer = (DocumentViewer)d;
		if (!documentViewer._internalIDSIChange)
		{
			documentViewer._documentScrollInfo.SetColumns((int)e.NewValue);
		}
	}

	private static bool ValidatePageSpacing(object value)
	{
		if (DoubleValue_Validate(value))
		{
			return (double)value >= 0.0;
		}
		return false;
	}

	private static void OnVerticalPageSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DocumentViewer documentViewer = (DocumentViewer)d;
		if (documentViewer._documentScrollInfo != null)
		{
			documentViewer._documentScrollInfo.VerticalPageSpacing = (double)e.NewValue;
		}
	}

	private static void OnHorizontalPageSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DocumentViewer documentViewer = (DocumentViewer)d;
		if (documentViewer._documentScrollInfo != null)
		{
			documentViewer._documentScrollInfo.HorizontalPageSpacing = (double)e.NewValue;
		}
	}
}
