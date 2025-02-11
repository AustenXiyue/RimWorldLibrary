using System.Collections;
using System.Collections.Generic;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.AppModel;
using MS.Internal.Commands;
using MS.Internal.Controls;
using MS.Internal.Documents;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Provides a control for viewing flow content, with built-in support for multiple viewing modes.</summary>
[TemplatePart(Name = "PART_ContentHost", Type = typeof(Decorator))]
[TemplatePart(Name = "PART_FindToolBarHost", Type = typeof(Decorator))]
[ContentProperty("Document")]
public class FlowDocumentReader : Control, IAddChild, IJournalState
{
	[Serializable]
	private class JournalState : CustomJournalStateInternal
	{
		public int ContentPosition;

		public LogicalDirection ContentPositionDirection;

		public double Zoom;

		public FlowDocumentReaderViewingMode ViewingMode;

		public JournalState(int contentPosition, LogicalDirection contentPositionDirection, double zoom, FlowDocumentReaderViewingMode viewingMode)
		{
			ContentPosition = contentPosition;
			ContentPositionDirection = contentPositionDirection;
			Zoom = zoom;
			ViewingMode = viewingMode;
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.ViewingMode" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.ViewingMode" /> dependency property.</returns>
	public static readonly DependencyProperty ViewingModeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.IsPageViewEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.IsPageViewEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsPageViewEnabledProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.IsTwoPageViewEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.IsTwoPageViewEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsTwoPageViewEnabledProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.IsScrollViewEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.IsScrollViewEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsScrollViewEnabledProperty;

	private static readonly DependencyPropertyKey PageCountPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.PageCount" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.PageCount" /> dependency property.</returns>
	public static readonly DependencyProperty PageCountProperty;

	private static readonly DependencyPropertyKey PageNumberPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.PageNumber" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.PageNumber" /> dependency property.</returns>
	public static readonly DependencyProperty PageNumberProperty;

	private static readonly DependencyPropertyKey CanGoToPreviousPagePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.CanGoToPreviousPage" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.CanGoToPreviousPage" /> dependency property.</returns>
	public static readonly DependencyProperty CanGoToPreviousPageProperty;

	private static readonly DependencyPropertyKey CanGoToNextPagePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.CanGoToNextPage" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.CanGoToNextPage" /> dependency property.</returns>
	public static readonly DependencyProperty CanGoToNextPageProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.IsFindEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.IsFindEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsFindEnabledProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.IsPrintEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.IsPrintEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsPrintEnabledProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.Document" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.Document" /> dependency property.</returns>
	public static readonly DependencyProperty DocumentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.Zoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.Zoom" /> dependency property.</returns>
	public static readonly DependencyProperty ZoomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.MaxZoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.MaxZoom" /> dependency property.</returns>
	public static readonly DependencyProperty MaxZoomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.MinZoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.MinZoom" /> dependency property.</returns>
	public static readonly DependencyProperty MinZoomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.ZoomIncrement" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.ZoomIncrement" /> dependency property.</returns>
	public static readonly DependencyProperty ZoomIncrementProperty;

	private static readonly DependencyPropertyKey CanIncreaseZoomPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.CanIncreaseZoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.CanIncreaseZoom" /> dependency property.</returns>
	public static readonly DependencyProperty CanIncreaseZoomProperty;

	private static readonly DependencyPropertyKey CanDecreaseZoomPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.CanDecreaseZoom" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.CanDecreaseZoom" /> dependency property.</returns>
	public static readonly DependencyProperty CanDecreaseZoomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.SelectionBrush" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.SelectionBrush" /> dependency property.</returns>
	public static readonly DependencyProperty SelectionBrushProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.SelectionOpacity" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.SelectionOpacity" /> dependency property.</returns>
	public static readonly DependencyProperty SelectionOpacityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.IsSelectionActive" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.IsSelectionActive" /> dependency property.</returns>
	public static readonly DependencyProperty IsSelectionActiveProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.FlowDocumentReader.IsInactiveSelectionHighlightEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.FlowDocumentReader.IsInactiveSelectionHighlightEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsInactiveSelectionHighlightEnabledProperty;

	/// <summary>Gets the value that represents the Switch Viewing Mode command.</summary>
	/// <returns>The command.</returns>
	public static readonly RoutedUICommand SwitchViewingModeCommand;

	private Decorator _contentHost;

	private Decorator _findToolBarHost;

	private ToggleButton _findButton;

	private ReaderPageViewer _pageViewer;

	private ReaderTwoPageViewer _twoPageViewer;

	private ReaderScrollViewer _scrollViewer;

	private bool _documentAsLogicalChild;

	private bool _printInProgress;

	private const string _contentHostTemplateName = "PART_ContentHost";

	private const string _findToolBarHostTemplateName = "PART_FindToolBarHost";

	private const string _findButtonTemplateName = "FindButton";

	private const string KeySwitchViewingMode = "Ctrl+M";

	private const string Switch_ViewingMode = "_Switch ViewingMode";

	private static DependencyObjectType _dType;

	private static ComponentResourceKey _pageViewStyleKey;

	private static ComponentResourceKey _twoPageViewStyleKey;

	private static ComponentResourceKey _scrollViewStyleKey;

	/// <summary>Gets or sets the viewing mode for the <see cref="T:System.Windows.Controls.FlowDocumentReader" />. </summary>
	/// <returns>One of the <see cref="T:System.Windows.Controls.FlowDocumentReaderViewingMode" /> values that specifies the viewing mode. The default is <see cref="F:System.Windows.Controls.FlowDocumentReaderViewingMode.Page" />.</returns>
	public FlowDocumentReaderViewingMode ViewingMode
	{
		get
		{
			return (FlowDocumentReaderViewingMode)GetValue(ViewingModeProperty);
		}
		set
		{
			SetValue(ViewingModeProperty, value);
		}
	}

	/// <summary>Gets the selected content of the <see cref="T:System.Windows.Controls.FlowDocumentReader" />.</summary>
	/// <returns>The selected content of the <see cref="T:System.Windows.Controls.FlowDocumentReader" />.</returns>
	public TextSelection Selection
	{
		get
		{
			TextSelection result = null;
			if (_contentHost != null && _contentHost.Child is IFlowDocumentViewer flowDocumentViewer)
			{
				result = flowDocumentViewer.TextSelection as TextSelection;
			}
			return result;
		}
	}

	/// <summary>Gets or sets a value that indicates whether <see cref="F:System.Windows.Controls.FlowDocumentReaderViewingMode.Page" /> is available as a viewing mode. </summary>
	/// <returns>true to indicate that single-page viewing mode is available; otherwise, false. The default is true.</returns>
	/// <exception cref="T:System.ArgumentException">Setting this property to false while <see cref="P:System.Windows.Controls.FlowDocumentReader.IsScrollViewEnabled" /> and <see cref="P:System.Windows.Controls.FlowDocumentReader.IsTwoPageViewEnabled" /> are also false.</exception>
	public bool IsPageViewEnabled
	{
		get
		{
			return (bool)GetValue(IsPageViewEnabledProperty);
		}
		set
		{
			SetValue(IsPageViewEnabledProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a value that indicates whether <see cref="F:System.Windows.Controls.FlowDocumentReaderViewingMode.TwoPage" /> is available as a viewing mode. </summary>
	/// <returns>true to indicate that <see cref="F:System.Windows.Controls.FlowDocumentReaderViewingMode.TwoPage" /> is available as a viewing mode; otherwise, false. The default is true.</returns>
	/// <exception cref="T:System.ArgumentException">Setting this property to false while <see cref="P:System.Windows.Controls.FlowDocumentReader.IsPageViewEnabled" /> and <see cref="P:System.Windows.Controls.FlowDocumentReader.IsScrollViewEnabled" /> are also false.</exception>
	public bool IsTwoPageViewEnabled
	{
		get
		{
			return (bool)GetValue(IsTwoPageViewEnabledProperty);
		}
		set
		{
			SetValue(IsTwoPageViewEnabledProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a value that indicates whether <see cref="F:System.Windows.Controls.FlowDocumentReaderViewingMode.Scroll" /> is available as a viewing mode. </summary>
	/// <returns>true to indicate that <see cref="F:System.Windows.Controls.FlowDocumentReaderViewingMode.Scroll" /> is available as a viewing mode; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.ArgumentException">Setting this property to false while <see cref="P:System.Windows.Controls.FlowDocumentReader.IsPageViewEnabled" /> and <see cref="P:System.Windows.Controls.FlowDocumentReader.IsTwoPageViewEnabled" /> are also false.</exception>
	public bool IsScrollViewEnabled
	{
		get
		{
			return (bool)GetValue(IsScrollViewEnabledProperty);
		}
		set
		{
			SetValue(IsScrollViewEnabledProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets the current number of display pages for the content hosted by the <see cref="T:System.Windows.Controls.FlowDocumentReader" />. </summary>
	/// <returns>The current number of display pages for the content hosted by the <see cref="T:System.Windows.Controls.FlowDocumentReader" />.</returns>
	public int PageCount => (int)GetValue(PageCountProperty);

	/// <summary>Gets the page number for the currently displayed page. </summary>
	/// <returns>The page number for the currently displayed page.</returns>
	public int PageNumber => (int)GetValue(PageNumberProperty);

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> can execute the <see cref="P:System.Windows.Input.NavigationCommands.PreviousPage" /> routed command to jump to the previous page of content. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> can jump to the previous page of content; otherwise, false.</returns>
	public bool CanGoToPreviousPage => (bool)GetValue(CanGoToPreviousPageProperty);

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> can execute the <see cref="P:System.Windows.Input.NavigationCommands.NextPage" /> routed command to jump to the next page of content. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> can jump to the next page of content; otherwise, false.</returns>
	public bool CanGoToNextPage => (bool)GetValue(CanGoToNextPageProperty);

	/// <summary>Gets or sets a value that indicates whether the <see cref="P:System.Windows.Input.ApplicationCommands.Find" /> routed command is enabled. </summary>
	/// <returns>true to enable the <see cref="P:System.Windows.Input.ApplicationCommands.Find" /> routed command; otherwise, false. The default is true.</returns>
	public bool IsFindEnabled
	{
		get
		{
			return (bool)GetValue(IsFindEnabledProperty);
		}
		set
		{
			SetValue(IsFindEnabledProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="P:System.Windows.Input.ApplicationCommands.Print" /> routed command is enabled. </summary>
	/// <returns>true to enable the <see cref="P:System.Windows.Input.ApplicationCommands.Print" /> routed command; otherwise, false. The default is true.</returns>
	public bool IsPrintEnabled
	{
		get
		{
			return (bool)GetValue(IsPrintEnabledProperty);
		}
		set
		{
			SetValue(IsPrintEnabledProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Documents.FlowDocument" /> that hosts the content to be displayed by the <see cref="T:System.Windows.Controls.FlowDocumentReader" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Documents.FlowDocument" /> that hosts the content to be displayed by the <see cref="T:System.Windows.Controls.FlowDocumentReader" />. The default is null.</returns>
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

	/// <summary>Gets or sets the current zoom level. </summary>
	/// <returns>The current zoom level, interpreted as a percentage. The default value 100.0 (zoom level of 100%).</returns>
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

	/// <summary>Gets or sets the maximum allowable <see cref="P:System.Windows.Controls.FlowDocumentReader.Zoom" /> level for the <see cref="T:System.Windows.Controls.FlowDocumentReader" />. </summary>
	/// <returns>The maximum allowable <see cref="P:System.Windows.Controls.FlowDocumentReader.Zoom" /> level for the <see cref="T:System.Windows.Controls.FlowDocumentReader" />, interpreted as a percentage. The default is 200.0 (maximum zoom of 200%).</returns>
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

	/// <summary>Gets or sets the minimum allowable <see cref="P:System.Windows.Controls.FlowDocumentReader.Zoom" /> level for the <see cref="T:System.Windows.Controls.FlowDocumentReader" />. </summary>
	/// <returns>The minimum allowable <see cref="P:System.Windows.Controls.FlowDocumentReader.Zoom" /> level for the <see cref="T:System.Windows.Controls.FlowDocumentReader" />, interpreted as a percentage. The default is 80.0 (minimum zoom of 80%).</returns>
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
	/// <returns>The current zoom increment, interpreted as a percentage. The default is 10.0 (zoom increment of 10%).</returns>
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

	/// <summary>Gets a value that indicates whether the <see cref="P:System.Windows.Controls.FlowDocumentReader.Zoom" /> level can be increased. </summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.FlowDocumentReader.Zoom" /> level can be increased; otherwise, false.</returns>
	public bool CanIncreaseZoom => (bool)GetValue(CanIncreaseZoomProperty);

	/// <summary>Gets a value that indicates whether the <see cref="P:System.Windows.Controls.FlowDocumentReader.Zoom" /> level can be decreased. </summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.FlowDocumentReader.Zoom" /> level can be decreased; otherwise, false.</returns>
	public bool CanDecreaseZoom => (bool)GetValue(CanDecreaseZoomProperty);

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

	/// <summary>Gets or sets the opacity of the <see cref="P:System.Windows.Controls.FlowDocumentReader.SelectionBrush" />.</summary>
	/// <returns>The opacity of the <see cref="P:System.Windows.Controls.FlowDocumentReader.SelectionBrush" />. The default is 0.4.</returns>
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

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> has focus and selected text.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> displays selected text when the text box does not have focus; otherwise, false.The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public bool IsSelectionActive => (bool)GetValue(IsSelectionActiveProperty);

	/// <summary>Gets or sets a value that indicates whether <see cref="T:System.Windows.Controls.FlowDocumentReader" /> displays selected text when the control does not have focus.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> displays selected text when the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> does not have focus; otherwise, false. The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
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

	/// <summary>Gets an enumerator that can iterate the logical children of the <see cref="T:System.Windows.Controls.FlowDocumentReader" />.</summary>
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

	private bool CanShowFindToolBar
	{
		get
		{
			if (_findToolBarHost != null && IsFindEnabled)
			{
				return Document != null;
			}
			return false;
		}
	}

	private TextEditor TextEditor
	{
		get
		{
			TextEditor result = null;
			IFlowDocumentViewer currentViewer = CurrentViewer;
			if (currentViewer != null && currentViewer.TextSelection != null)
			{
				result = currentViewer.TextSelection.TextEditor;
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

	private IFlowDocumentViewer CurrentViewer
	{
		get
		{
			if (_contentHost != null)
			{
				return (IFlowDocumentViewer)_contentHost.Child;
			}
			return null;
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	private static ResourceKey PageViewStyleKey
	{
		get
		{
			if (_pageViewStyleKey == null)
			{
				_pageViewStyleKey = new ComponentResourceKey(typeof(PresentationUIStyleResources), "PUIPageViewStyleKey");
			}
			return _pageViewStyleKey;
		}
	}

	private static ResourceKey TwoPageViewStyleKey
	{
		get
		{
			if (_twoPageViewStyleKey == null)
			{
				_twoPageViewStyleKey = new ComponentResourceKey(typeof(PresentationUIStyleResources), "PUITwoPageViewStyleKey");
			}
			return _twoPageViewStyleKey;
		}
	}

	private static ResourceKey ScrollViewStyleKey
	{
		get
		{
			if (_scrollViewStyleKey == null)
			{
				_scrollViewStyleKey = new ComponentResourceKey(typeof(PresentationUIStyleResources), "PUIScrollViewStyleKey");
			}
			return _scrollViewStyleKey;
		}
	}

	static FlowDocumentReader()
	{
		ViewingModeProperty = DependencyProperty.Register("ViewingMode", typeof(FlowDocumentReaderViewingMode), typeof(FlowDocumentReader), new FrameworkPropertyMetadata(FlowDocumentReaderViewingMode.Page, FrameworkPropertyMetadataOptions.AffectsMeasure, ViewingModeChanged), IsValidViewingMode);
		IsPageViewEnabledProperty = DependencyProperty.Register("IsPageViewEnabled", typeof(bool), typeof(FlowDocumentReader), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, ViewingModeEnabledChanged));
		IsTwoPageViewEnabledProperty = DependencyProperty.Register("IsTwoPageViewEnabled", typeof(bool), typeof(FlowDocumentReader), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, ViewingModeEnabledChanged));
		IsScrollViewEnabledProperty = DependencyProperty.Register("IsScrollViewEnabled", typeof(bool), typeof(FlowDocumentReader), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, ViewingModeEnabledChanged));
		PageCountPropertyKey = DependencyProperty.RegisterReadOnly("PageCount", typeof(int), typeof(FlowDocumentReader), new FrameworkPropertyMetadata(0));
		PageCountProperty = PageCountPropertyKey.DependencyProperty;
		PageNumberPropertyKey = DependencyProperty.RegisterReadOnly("PageNumber", typeof(int), typeof(FlowDocumentReader), new FrameworkPropertyMetadata(0));
		PageNumberProperty = PageNumberPropertyKey.DependencyProperty;
		CanGoToPreviousPagePropertyKey = DependencyProperty.RegisterReadOnly("CanGoToPreviousPage", typeof(bool), typeof(FlowDocumentReader), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		CanGoToPreviousPageProperty = CanGoToPreviousPagePropertyKey.DependencyProperty;
		CanGoToNextPagePropertyKey = DependencyProperty.RegisterReadOnly("CanGoToNextPage", typeof(bool), typeof(FlowDocumentReader), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		CanGoToNextPageProperty = CanGoToNextPagePropertyKey.DependencyProperty;
		IsFindEnabledProperty = DependencyProperty.Register("IsFindEnabled", typeof(bool), typeof(FlowDocumentReader), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, IsFindEnabledChanged));
		IsPrintEnabledProperty = DependencyProperty.Register("IsPrintEnabled", typeof(bool), typeof(FlowDocumentReader), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, IsPrintEnabledChanged));
		DocumentProperty = DependencyProperty.Register("Document", typeof(FlowDocument), typeof(FlowDocumentReader), new FrameworkPropertyMetadata(null, DocumentChanged));
		ZoomProperty = FlowDocumentPageViewer.ZoomProperty.AddOwner(typeof(FlowDocumentReader), new FrameworkPropertyMetadata(100.0, ZoomChanged, CoerceZoom));
		MaxZoomProperty = FlowDocumentPageViewer.MaxZoomProperty.AddOwner(typeof(FlowDocumentReader), new FrameworkPropertyMetadata(200.0, MaxZoomChanged, CoerceMaxZoom));
		MinZoomProperty = FlowDocumentPageViewer.MinZoomProperty.AddOwner(typeof(FlowDocumentReader), new FrameworkPropertyMetadata(80.0, MinZoomChanged));
		ZoomIncrementProperty = FlowDocumentPageViewer.ZoomIncrementProperty.AddOwner(typeof(FlowDocumentReader));
		CanIncreaseZoomPropertyKey = DependencyProperty.RegisterReadOnly("CanIncreaseZoom", typeof(bool), typeof(FlowDocumentReader), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		CanIncreaseZoomProperty = CanIncreaseZoomPropertyKey.DependencyProperty;
		CanDecreaseZoomPropertyKey = DependencyProperty.RegisterReadOnly("CanDecreaseZoom", typeof(bool), typeof(FlowDocumentReader), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		CanDecreaseZoomProperty = CanDecreaseZoomPropertyKey.DependencyProperty;
		SelectionBrushProperty = TextBoxBase.SelectionBrushProperty.AddOwner(typeof(FlowDocumentReader));
		SelectionOpacityProperty = TextBoxBase.SelectionOpacityProperty.AddOwner(typeof(FlowDocumentReader));
		IsSelectionActiveProperty = TextBoxBase.IsSelectionActiveProperty.AddOwner(typeof(FlowDocumentReader));
		IsInactiveSelectionHighlightEnabledProperty = TextBoxBase.IsInactiveSelectionHighlightEnabledProperty.AddOwner(typeof(FlowDocumentReader));
		SwitchViewingModeCommand = new RoutedUICommand("_Switch ViewingMode", "SwitchViewingMode", typeof(FlowDocumentReader), null);
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FlowDocumentReader), new FrameworkPropertyMetadata(new ComponentResourceKey(typeof(PresentationUIStyleResources), "PUIFlowDocumentReader")));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(FlowDocumentReader));
		TextBoxBase.SelectionBrushProperty.OverrideMetadata(typeof(FlowDocumentReader), new FrameworkPropertyMetadata(UpdateCaretElement));
		TextBoxBase.SelectionOpacityProperty.OverrideMetadata(typeof(FlowDocumentReader), new FrameworkPropertyMetadata(0.4, UpdateCaretElement));
		CreateCommandBindings();
		EventManager.RegisterClassHandler(typeof(FlowDocumentReader), Keyboard.KeyDownEvent, new KeyEventHandler(KeyDownHandler), handledEventsToo: true);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> class.</summary>
	public FlowDocumentReader()
	{
	}

	/// <summary>Builds the visual tree for the <see cref="T:System.Windows.Controls.FlowDocumentReader" />.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		if (CurrentViewer != null)
		{
			DetachViewer(CurrentViewer);
			_contentHost.Child = null;
		}
		_contentHost = GetTemplateChild("PART_ContentHost") as Decorator;
		if (_contentHost != null)
		{
			if (_contentHost.Child != null)
			{
				throw new NotSupportedException(SR.FlowDocumentReaderDecoratorMarkedAsContentHostMustHaveNoContent);
			}
			SwitchViewingModeCore(ViewingMode);
		}
		if (FindToolBar != null)
		{
			ToggleFindToolBar(enable: false);
		}
		_findToolBarHost = GetTemplateChild("PART_FindToolBarHost") as Decorator;
		_findButton = GetTemplateChild("FindButton") as ToggleButton;
	}

	/// <summary>Returns a value that indicates whether or the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> is able to jump to the specified page number.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> is able to jump to the specified page number; otherwise, false.</returns>
	/// <param name="pageNumber">A page number to check for as a valid jump target.</param>
	public bool CanGoToPage(int pageNumber)
	{
		bool result = false;
		if (CurrentViewer != null)
		{
			result = CurrentViewer.CanGoToPage(pageNumber);
		}
		return result;
	}

	/// <summary>Toggles the Find dialog.</summary>
	public void Find()
	{
		OnFindCommand();
	}

	/// <summary>Invokes a standard Print dialog which can be used to print the contents of the <see cref="T:System.Windows.Controls.FlowDocumentReader" /> and configure printing preferences.</summary>
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

	/// <summary>Executes the <see cref="F:System.Windows.Controls.FlowDocumentReader.SwitchViewingModeCommand" /> command.</summary>
	/// <param name="viewingMode">One of the <see cref="T:System.Windows.Controls.FlowDocumentReaderViewingMode" /> values that specifies the desired viewing mode.</param>
	public void SwitchViewingMode(FlowDocumentReaderViewingMode viewingMode)
	{
		OnSwitchViewingModeCommand(viewingMode);
	}

	/// <summary>Called when a printing job has completed.</summary>
	protected virtual void OnPrintCompleted()
	{
		if (_printInProgress)
		{
			_printInProgress = false;
			CommandManager.InvalidateRequerySuggested();
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

	/// <summary>Handles the <see cref="P:System.Windows.Input.ApplicationCommands.Print" /> routed command.</summary>
	protected virtual void OnPrintCommand()
	{
		if (CurrentViewer != null)
		{
			CurrentViewer.Print();
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.ApplicationCommands.CancelPrint" /> routed command.</summary>
	protected virtual void OnCancelPrintCommand()
	{
		if (CurrentViewer != null)
		{
			CurrentViewer.CancelPrint();
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.NavigationCommands.IncreaseZoom" /> routed command.</summary>
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

	/// <summary>Handles the <see cref="M:System.Windows.Controls.FlowDocumentReader.SwitchViewingMode(System.Windows.Controls.FlowDocumentReaderViewingMode)" /> routed command.</summary>
	/// <param name="viewingMode">One of the <see cref="T:System.Windows.Controls.FlowDocumentReaderViewingMode" /> values that specifies the viewing mode to switch to.</param>
	protected virtual void OnSwitchViewingModeCommand(FlowDocumentReaderViewingMode viewingMode)
	{
		SwitchViewingModeCore(viewingMode);
	}

	/// <summary>Handles the <see cref="E:System.Windows.FrameworkElement.Initialized" /> routed event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> object containing the arguments associated with the <see cref="E:System.Windows.FrameworkElement.Initialized" /> routed event.</param>
	protected override void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);
		if (base.IsInitialized && !CanSwitchToViewingMode(ViewingMode))
		{
			throw new ArgumentException(SR.FlowDocumentReaderViewingModeEnabledConflict);
		}
	}

	protected override void OnDpiChanged(DpiScale oldDpiScaleInfo, DpiScale newDpiScaleInfo)
	{
		Document?.SetDpi(newDpiScaleInfo);
	}

	/// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Controls.FlowDocumentReader" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Controls.FlowDocumentReader" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new FlowDocumentReaderAutomationPeer(this);
	}

	/// <summary>Handles the <see cref="E:System.Windows.UIElement.IsKeyboardFocusWithinChanged" /> routed event.</summary>
	/// <param name="e">A <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> object containing the arguments associated with the <see cref="E:System.Windows.UIElement.IsKeyboardFocusWithinChanged" /> routed event.</param>
	protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnIsKeyboardFocusWithinChanged(e);
		if (base.IsKeyboardFocusWithin && CurrentViewer != null && !IsFocusWithinDocument())
		{
			((FrameworkElement)CurrentViewer).Focus();
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.Input.Keyboard.KeyDown" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Provides data about the event.</param>
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

	internal override bool BuildRouteCore(EventRoute route, RoutedEventArgs args)
	{
		return BuildRouteCoreHelper(route, args, shouldAddIntermediateElementsToRoute: false);
	}

	internal override bool InvalidateAutomationAncestorsCore(Stack<DependencyObject> branchNodeStack, out bool continuePastCoreTree)
	{
		bool shouldInvalidateIntermediateElements = false;
		return InvalidateAutomationAncestorsCoreHelper(branchNodeStack, out continuePastCoreTree, shouldInvalidateIntermediateElements);
	}

	/// <summary>Handles the <see cref="M:System.Windows.Controls.FlowDocumentReader.SwitchViewingMode(System.Windows.Controls.FlowDocumentReaderViewingMode)" /> command.</summary>
	/// <param name="viewingMode">One of the <see cref="T:System.Windows.Controls.FlowDocumentReaderViewingMode" /> values that specifies the desired viewing mode.</param>
	protected virtual void SwitchViewingModeCore(FlowDocumentReaderViewingMode viewingMode)
	{
		ITextSelection textSelection = null;
		ContentPosition contentPosition = null;
		DependencyObject dependencyObject = null;
		if (_contentHost == null)
		{
			return;
		}
		bool isKeyboardFocusWithin = base.IsKeyboardFocusWithin;
		if (_contentHost.Child is IFlowDocumentViewer flowDocumentViewer)
		{
			if (isKeyboardFocusWithin && IsFocusWithinDocument())
			{
				dependencyObject = Keyboard.FocusedElement as DependencyObject;
			}
			textSelection = flowDocumentViewer.TextSelection;
			contentPosition = flowDocumentViewer.ContentPosition;
			DetachViewer(flowDocumentViewer);
		}
		IFlowDocumentViewer viewerFromMode = GetViewerFromMode(viewingMode);
		FrameworkElement frameworkElement = (FrameworkElement)viewerFromMode;
		if (viewerFromMode != null)
		{
			_contentHost.Child = frameworkElement;
			AttachViewer(viewerFromMode);
			viewerFromMode.TextSelection = textSelection;
			viewerFromMode.ContentPosition = contentPosition;
			if (isKeyboardFocusWithin)
			{
				if (dependencyObject is UIElement)
				{
					((UIElement)dependencyObject).Focus();
				}
				else if (dependencyObject is ContentElement)
				{
					((ContentElement)dependencyObject).Focus();
				}
				else
				{
					frameworkElement.Focus();
				}
			}
		}
		UpdateReadOnlyProperties(pageCountChanged: true, pageNumberChanged: true);
	}

	private bool IsFocusWithinDocument()
	{
		DependencyObject dependencyObject = Keyboard.FocusedElement as DependencyObject;
		while (dependencyObject != null && dependencyObject != Document)
		{
			dependencyObject = ((!(dependencyObject is FrameworkElement { TemplatedParent: not null } frameworkElement)) ? LogicalTreeHelper.GetParent(dependencyObject) : frameworkElement.TemplatedParent);
		}
		return dependencyObject != null;
	}

	private void DocumentChanged(FlowDocument oldDocument, FlowDocument newDocument)
	{
		if (oldDocument != null && _documentAsLogicalChild)
		{
			RemoveLogicalChild(oldDocument);
		}
		if (base.TemplatedParent != null && newDocument != null && LogicalTreeHelper.GetParent(newDocument) != null)
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
			newDocument.SetDpi(GetDpi());
			if (_documentAsLogicalChild)
			{
				AddLogicalChild(newDocument);
			}
		}
		if (CurrentViewer != null)
		{
			CurrentViewer.SetDocument(newDocument);
		}
		UpdateReadOnlyProperties(pageCountChanged: true, pageNumberChanged: true);
		if (!CanShowFindToolBar && FindToolBar != null)
		{
			ToggleFindToolBar(enable: false);
		}
		if (UIElementAutomationPeer.FromElement(this) is FlowDocumentReaderAutomationPeer flowDocumentReaderAutomationPeer)
		{
			flowDocumentReaderAutomationPeer.InvalidatePeer();
		}
	}

	private void DetachViewer(IFlowDocumentViewer viewer)
	{
		Invariant.Assert(viewer != null && viewer is FrameworkElement);
		FrameworkElement target = (FrameworkElement)viewer;
		BindingOperations.ClearBinding(target, ZoomProperty);
		BindingOperations.ClearBinding(target, MaxZoomProperty);
		BindingOperations.ClearBinding(target, MinZoomProperty);
		BindingOperations.ClearBinding(target, ZoomIncrementProperty);
		viewer.PageCountChanged -= OnPageCountChanged;
		viewer.PageNumberChanged -= OnPageNumberChanged;
		viewer.PrintStarted -= OnViewerPrintStarted;
		viewer.PrintCompleted -= OnViewerPrintCompleted;
		viewer.SetDocument(null);
	}

	private void AttachViewer(IFlowDocumentViewer viewer)
	{
		Invariant.Assert(viewer != null && viewer is FrameworkElement);
		FrameworkElement fe = (FrameworkElement)viewer;
		viewer.SetDocument(Document);
		viewer.PageCountChanged += OnPageCountChanged;
		viewer.PageNumberChanged += OnPageNumberChanged;
		viewer.PrintStarted += OnViewerPrintStarted;
		viewer.PrintCompleted += OnViewerPrintCompleted;
		CreateTwoWayBinding(fe, ZoomProperty, "Zoom");
		CreateTwoWayBinding(fe, MaxZoomProperty, "MaxZoom");
		CreateTwoWayBinding(fe, MinZoomProperty, "MinZoom");
		CreateTwoWayBinding(fe, ZoomIncrementProperty, "ZoomIncrement");
	}

	private void CreateTwoWayBinding(FrameworkElement fe, DependencyProperty dp, string propertyPath)
	{
		Binding binding = new Binding(propertyPath);
		binding.Mode = BindingMode.TwoWay;
		binding.Source = this;
		fe.SetBinding(dp, binding);
	}

	private bool CanSwitchToViewingMode(FlowDocumentReaderViewingMode mode)
	{
		bool result = false;
		switch (mode)
		{
		case FlowDocumentReaderViewingMode.Page:
			result = IsPageViewEnabled;
			break;
		case FlowDocumentReaderViewingMode.TwoPage:
			result = IsTwoPageViewEnabled;
			break;
		case FlowDocumentReaderViewingMode.Scroll:
			result = IsScrollViewEnabled;
			break;
		}
		return result;
	}

	private IFlowDocumentViewer GetViewerFromMode(FlowDocumentReaderViewingMode mode)
	{
		IFlowDocumentViewer result = null;
		switch (mode)
		{
		case FlowDocumentReaderViewingMode.Page:
			if (_pageViewer == null)
			{
				_pageViewer = new ReaderPageViewer();
				_pageViewer.SetResourceReference(FrameworkElement.StyleProperty, PageViewStyleKey);
				_pageViewer.Name = "PageViewer";
				CommandManager.AddPreviewCanExecuteHandler(_pageViewer, PreviewCanExecuteRoutedEventHandler);
			}
			result = _pageViewer;
			break;
		case FlowDocumentReaderViewingMode.TwoPage:
			if (_twoPageViewer == null)
			{
				_twoPageViewer = new ReaderTwoPageViewer();
				_twoPageViewer.SetResourceReference(FrameworkElement.StyleProperty, TwoPageViewStyleKey);
				_twoPageViewer.Name = "TwoPageViewer";
				CommandManager.AddPreviewCanExecuteHandler(_twoPageViewer, PreviewCanExecuteRoutedEventHandler);
			}
			result = _twoPageViewer;
			break;
		case FlowDocumentReaderViewingMode.Scroll:
			if (_scrollViewer == null)
			{
				_scrollViewer = new ReaderScrollViewer();
				_scrollViewer.SetResourceReference(FrameworkElement.StyleProperty, ScrollViewStyleKey);
				_scrollViewer.Name = "ScrollViewer";
				CommandManager.AddPreviewCanExecuteHandler(_scrollViewer, PreviewCanExecuteRoutedEventHandler);
			}
			result = _scrollViewer;
			break;
		}
		return result;
	}

	private void UpdateReadOnlyProperties(bool pageCountChanged, bool pageNumberChanged)
	{
		if (pageCountChanged)
		{
			SetValue(PageCountPropertyKey, (CurrentViewer != null) ? CurrentViewer.PageCount : 0);
		}
		if (pageNumberChanged)
		{
			SetValue(PageNumberPropertyKey, (CurrentViewer != null) ? CurrentViewer.PageNumber : 0);
			SetValue(CanGoToPreviousPagePropertyKey, CurrentViewer != null && CurrentViewer.CanGoToPreviousPage);
		}
		if (pageCountChanged || pageNumberChanged)
		{
			SetValue(CanGoToNextPagePropertyKey, CurrentViewer != null && CurrentViewer.CanGoToNextPage);
		}
	}

	private void OnPageCountChanged(object sender, EventArgs e)
	{
		Invariant.Assert(CurrentViewer != null && sender == CurrentViewer);
		UpdateReadOnlyProperties(pageCountChanged: true, pageNumberChanged: false);
	}

	private void OnPageNumberChanged(object sender, EventArgs e)
	{
		Invariant.Assert(CurrentViewer != null && sender == CurrentViewer);
		UpdateReadOnlyProperties(pageCountChanged: false, pageNumberChanged: true);
	}

	private void OnViewerPrintStarted(object sender, EventArgs e)
	{
		Invariant.Assert(CurrentViewer != null && sender == CurrentViewer);
		_printInProgress = true;
		CommandManager.InvalidateRequerySuggested();
	}

	private void OnViewerPrintCompleted(object sender, EventArgs e)
	{
		Invariant.Assert(CurrentViewer != null && sender == CurrentViewer);
		OnPrintCompleted();
	}

	private bool ConvertToViewingMode(object value, out FlowDocumentReaderViewingMode mode)
	{
		if (value is FlowDocumentReaderViewingMode)
		{
			mode = (FlowDocumentReaderViewingMode)value;
			return true;
		}
		if (value is string)
		{
			string text = (string)value;
			if (text == FlowDocumentReaderViewingMode.Page.ToString())
			{
				mode = FlowDocumentReaderViewingMode.Page;
				return true;
			}
			if (text == FlowDocumentReaderViewingMode.TwoPage.ToString())
			{
				mode = FlowDocumentReaderViewingMode.TwoPage;
				return true;
			}
			if (text == FlowDocumentReaderViewingMode.Scroll.ToString())
			{
				mode = FlowDocumentReaderViewingMode.Scroll;
				return true;
			}
			mode = FlowDocumentReaderViewingMode.Page;
			return false;
		}
		mode = FlowDocumentReaderViewingMode.Page;
		return false;
	}

	private void ToggleFindToolBar(bool enable)
	{
		Invariant.Assert(enable == (FindToolBar == null));
		if (_findButton != null && _findButton.IsChecked.HasValue && _findButton.IsChecked.Value != enable)
		{
			_findButton.IsChecked = enable;
		}
		DocumentViewerHelper.ToggleFindToolBar(_findToolBarHost, OnFindInvoked, enable);
	}

	private static void CreateCommandBindings()
	{
		ExecutedRoutedEventHandler executedRoutedEventHandler = ExecutedRoutedEventHandler;
		CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = CanExecuteRoutedEventHandler;
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentReader), SwitchViewingModeCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+M", "KeySwitchViewingModeDisplayString"));
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentReader), ApplicationCommands.Find, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentReader), ApplicationCommands.Print, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentReader), ApplicationCommands.CancelPrint, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentReader), NavigationCommands.PreviousPage, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentReader), NavigationCommands.NextPage, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentReader), NavigationCommands.FirstPage, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentReader), NavigationCommands.LastPage, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentReader), NavigationCommands.IncreaseZoom, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.OemPlus, ModifierKeys.Control));
		CommandHelpers.RegisterCommandHandler(typeof(FlowDocumentReader), NavigationCommands.DecreaseZoom, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.OemMinus, ModifierKeys.Control));
	}

	private static void CanExecuteRoutedEventHandler(object target, CanExecuteRoutedEventArgs args)
	{
		FlowDocumentReader flowDocumentReader = target as FlowDocumentReader;
		Invariant.Assert(flowDocumentReader != null, "Target of CanExecuteRoutedEventHandler must be FlowDocumentReader.");
		Invariant.Assert(args != null, "args cannot be null.");
		if (!flowDocumentReader._printInProgress)
		{
			if (args.Command == SwitchViewingModeCommand)
			{
				if (flowDocumentReader.ConvertToViewingMode(args.Parameter, out var mode))
				{
					args.CanExecute = flowDocumentReader.CanSwitchToViewingMode(mode);
				}
				else
				{
					args.CanExecute = args.Parameter == null;
				}
			}
			else if (args.Command == ApplicationCommands.Find)
			{
				args.CanExecute = flowDocumentReader.CanShowFindToolBar;
			}
			else if (args.Command == ApplicationCommands.Print)
			{
				args.CanExecute = flowDocumentReader.Document != null && flowDocumentReader.IsPrintEnabled;
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
		FlowDocumentReader flowDocumentReader = target as FlowDocumentReader;
		Invariant.Assert(flowDocumentReader != null, "Target of ExecutedRoutedEventHandler must be FlowDocumentReader.");
		Invariant.Assert(args != null, "args cannot be null.");
		if (args.Command == SwitchViewingModeCommand)
		{
			flowDocumentReader.TrySwitchViewingMode(args.Parameter);
		}
		else if (args.Command == ApplicationCommands.Find)
		{
			flowDocumentReader.OnFindCommand();
		}
		else if (args.Command == ApplicationCommands.Print)
		{
			flowDocumentReader.OnPrintCommand();
		}
		else if (args.Command == ApplicationCommands.CancelPrint)
		{
			flowDocumentReader.OnCancelPrintCommand();
		}
		else if (args.Command == NavigationCommands.IncreaseZoom)
		{
			flowDocumentReader.OnIncreaseZoomCommand();
		}
		else if (args.Command == NavigationCommands.DecreaseZoom)
		{
			flowDocumentReader.OnDecreaseZoomCommand();
		}
		else if (args.Command == NavigationCommands.PreviousPage)
		{
			flowDocumentReader.OnPreviousPageCommand();
		}
		else if (args.Command == NavigationCommands.NextPage)
		{
			flowDocumentReader.OnNextPageCommand();
		}
		else if (args.Command == NavigationCommands.FirstPage)
		{
			flowDocumentReader.OnFirstPageCommand();
		}
		else if (args.Command == NavigationCommands.LastPage)
		{
			flowDocumentReader.OnLastPageCommand();
		}
		else
		{
			Invariant.Assert(condition: false, "Command not handled in ExecutedRoutedEventHandler.");
		}
	}

	private void TrySwitchViewingMode(object parameter)
	{
		if (!ConvertToViewingMode(parameter, out var mode))
		{
			if (parameter != null)
			{
				return;
			}
			mode = (FlowDocumentReaderViewingMode)((int)(ViewingMode + 1) % 3);
		}
		while (!CanSwitchToViewingMode(mode))
		{
			mode = (FlowDocumentReaderViewingMode)((int)(mode + 1) % 3);
		}
		SetCurrentValueInternal(ViewingModeProperty, mode);
	}

	private void OnPreviousPageCommand()
	{
		if (CurrentViewer != null)
		{
			CurrentViewer.PreviousPage();
		}
	}

	private void OnNextPageCommand()
	{
		if (CurrentViewer != null)
		{
			CurrentViewer.NextPage();
		}
	}

	private void OnFirstPageCommand()
	{
		if (CurrentViewer != null)
		{
			CurrentViewer.FirstPage();
		}
	}

	private void OnLastPageCommand()
	{
		if (CurrentViewer != null)
		{
			CurrentViewer.LastPage();
		}
	}

	private void OnFindInvoked(object sender, EventArgs e)
	{
		TextEditor textEditor = TextEditor;
		FindToolBar findToolBar = FindToolBar;
		if (findToolBar == null || textEditor == null)
		{
			return;
		}
		if (CurrentViewer != null && CurrentViewer is UIElement)
		{
			((UIElement)CurrentViewer).Focus();
		}
		ITextRange textRange = DocumentViewerHelper.Find(findToolBar, textEditor, textEditor.TextView, textEditor.TextView);
		if (textRange != null && !textRange.IsEmpty)
		{
			if (CurrentViewer != null)
			{
				CurrentViewer.ShowFindResult(textRange);
			}
		}
		else
		{
			DocumentViewerHelper.ShowFindUnsuccessfulMessage(findToolBar);
		}
	}

	private void PreviewCanExecuteRoutedEventHandler(object target, CanExecuteRoutedEventArgs args)
	{
		if (args.Command == ApplicationCommands.Find)
		{
			args.CanExecute = false;
			args.Handled = true;
		}
		else if (args.Command == ApplicationCommands.Print)
		{
			args.CanExecute = IsPrintEnabled;
			args.Handled = !IsPrintEnabled;
		}
	}

	private static void KeyDownHandler(object sender, KeyEventArgs e)
	{
		DocumentViewerHelper.KeyDownHelper(e, ((FlowDocumentReader)sender)._findToolBarHost);
	}

	private static void ViewingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentReader);
		FlowDocumentReader flowDocumentReader = (FlowDocumentReader)d;
		if (flowDocumentReader.CanSwitchToViewingMode((FlowDocumentReaderViewingMode)e.NewValue))
		{
			flowDocumentReader.SwitchViewingModeCore((FlowDocumentReaderViewingMode)e.NewValue);
		}
		else if (flowDocumentReader.IsInitialized)
		{
			throw new ArgumentException(SR.FlowDocumentReaderViewingModeEnabledConflict);
		}
		if (UIElementAutomationPeer.FromElement(flowDocumentReader) is FlowDocumentReaderAutomationPeer flowDocumentReaderAutomationPeer)
		{
			flowDocumentReaderAutomationPeer.RaiseCurrentViewChangedEvent((FlowDocumentReaderViewingMode)e.NewValue, (FlowDocumentReaderViewingMode)e.OldValue);
		}
	}

	private static bool IsValidViewingMode(object o)
	{
		FlowDocumentReaderViewingMode flowDocumentReaderViewingMode = (FlowDocumentReaderViewingMode)o;
		if (flowDocumentReaderViewingMode != 0 && flowDocumentReaderViewingMode != FlowDocumentReaderViewingMode.TwoPage)
		{
			return flowDocumentReaderViewingMode == FlowDocumentReaderViewingMode.Scroll;
		}
		return true;
	}

	private static void ViewingModeEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentReader);
		FlowDocumentReader flowDocumentReader = (FlowDocumentReader)d;
		if (!flowDocumentReader.IsPageViewEnabled && !flowDocumentReader.IsTwoPageViewEnabled && !flowDocumentReader.IsScrollViewEnabled)
		{
			throw new ArgumentException(SR.FlowDocumentReaderCannotDisableAllViewingModes);
		}
		if (flowDocumentReader.IsInitialized && !flowDocumentReader.CanSwitchToViewingMode(flowDocumentReader.ViewingMode))
		{
			throw new ArgumentException(SR.FlowDocumentReaderViewingModeEnabledConflict);
		}
		if (UIElementAutomationPeer.FromElement(flowDocumentReader) is FlowDocumentReaderAutomationPeer flowDocumentReaderAutomationPeer)
		{
			flowDocumentReaderAutomationPeer.RaiseSupportedViewsChangedEvent(e);
		}
	}

	private static void IsFindEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentReader);
		FlowDocumentReader flowDocumentReader = (FlowDocumentReader)d;
		if (!flowDocumentReader.CanShowFindToolBar && flowDocumentReader.FindToolBar != null)
		{
			flowDocumentReader.ToggleFindToolBar(enable: false);
		}
		CommandManager.InvalidateRequerySuggested();
	}

	private static void IsPrintEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentReader);
		_ = (FlowDocumentReader)d;
		CommandManager.InvalidateRequerySuggested();
	}

	private static void DocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentReader);
		((FlowDocumentReader)d).DocumentChanged((FlowDocument)e.OldValue, (FlowDocument)e.NewValue);
		CommandManager.InvalidateRequerySuggested();
	}

	private static void ZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentReader);
		FlowDocumentReader flowDocumentReader = (FlowDocumentReader)d;
		if (!DoubleUtil.AreClose((double)e.OldValue, (double)e.NewValue))
		{
			flowDocumentReader.SetValue(CanIncreaseZoomPropertyKey, BooleanBoxes.Box(DoubleUtil.GreaterThan(flowDocumentReader.MaxZoom, flowDocumentReader.Zoom)));
			flowDocumentReader.SetValue(CanDecreaseZoomPropertyKey, BooleanBoxes.Box(DoubleUtil.LessThan(flowDocumentReader.MinZoom, flowDocumentReader.Zoom)));
		}
	}

	private static object CoerceZoom(DependencyObject d, object value)
	{
		Invariant.Assert(d != null && d is FlowDocumentReader);
		FlowDocumentReader flowDocumentReader = (FlowDocumentReader)d;
		double value2 = (double)value;
		double maxZoom = flowDocumentReader.MaxZoom;
		if (DoubleUtil.LessThan(maxZoom, value2))
		{
			return maxZoom;
		}
		double minZoom = flowDocumentReader.MinZoom;
		if (DoubleUtil.GreaterThan(minZoom, value2))
		{
			return minZoom;
		}
		return value;
	}

	private static void MaxZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentReader);
		FlowDocumentReader flowDocumentReader = (FlowDocumentReader)d;
		flowDocumentReader.CoerceValue(ZoomProperty);
		flowDocumentReader.SetValue(CanIncreaseZoomPropertyKey, BooleanBoxes.Box(DoubleUtil.GreaterThan(flowDocumentReader.MaxZoom, flowDocumentReader.Zoom)));
	}

	private static object CoerceMaxZoom(DependencyObject d, object value)
	{
		Invariant.Assert(d != null && d is FlowDocumentReader);
		double minZoom = ((FlowDocumentReader)d).MinZoom;
		if (!((double)value < minZoom))
		{
			return value;
		}
		return minZoom;
	}

	private static void MinZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is FlowDocumentReader);
		FlowDocumentReader flowDocumentReader = (FlowDocumentReader)d;
		flowDocumentReader.CoerceValue(MaxZoomProperty);
		flowDocumentReader.CoerceValue(ZoomProperty);
		flowDocumentReader.SetValue(CanDecreaseZoomPropertyKey, BooleanBoxes.Box(DoubleUtil.LessThan(flowDocumentReader.MinZoom, flowDocumentReader.Zoom)));
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
		FlowDocumentReader flowDocumentReader = (FlowDocumentReader)d;
		if (flowDocumentReader.Selection != null)
		{
			flowDocumentReader.Selection.CaretElement?.InvalidateVisual();
		}
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. Use the <see cref="P:System.Windows.Controls.FlowDocumentReader.Document" /> property to add a <see cref="T:System.Windows.Documents.FlowDocument" /> as the content child for the <see cref="T:System.Windows.Controls.FlowDocumentReader" />.</summary>
	/// <param name="value">An object to add as a child. </param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (Document != null)
		{
			throw new ArgumentException(SR.FlowDocumentReaderCanHaveOnlyOneChild);
		}
		if (!(value is FlowDocument))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(FlowDocument)), "value");
		}
		Document = value as FlowDocument;
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="text">A string to add to the object. </param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	CustomJournalStateInternal IJournalState.GetJournalState(JournalReason journalReason)
	{
		int contentPosition = -1;
		LogicalDirection contentPositionDirection = LogicalDirection.Forward;
		IFlowDocumentViewer currentViewer = CurrentViewer;
		if (currentViewer != null && currentViewer.ContentPosition is TextPointer textPointer)
		{
			contentPosition = textPointer.Offset;
			contentPositionDirection = textPointer.LogicalDirection;
		}
		return new JournalState(contentPosition, contentPositionDirection, Zoom, ViewingMode);
	}

	void IJournalState.RestoreJournalState(CustomJournalStateInternal state)
	{
		JournalState journalState = state as JournalState;
		if (state == null)
		{
			return;
		}
		SetCurrentValueInternal(ZoomProperty, journalState.Zoom);
		SetCurrentValueInternal(ViewingModeProperty, journalState.ViewingMode);
		if (journalState.ContentPosition == -1)
		{
			return;
		}
		IFlowDocumentViewer currentViewer = CurrentViewer;
		FlowDocument document = Document;
		if (currentViewer != null && document != null)
		{
			TextContainer textContainer = document.StructuralCache.TextContainer;
			if (journalState.ContentPosition <= textContainer.SymbolCount)
			{
				TextPointer contentPosition = textContainer.CreatePointerAtOffset(journalState.ContentPosition, journalState.ContentPositionDirection);
				currentViewer.ContentPosition = contentPosition;
			}
		}
	}
}
