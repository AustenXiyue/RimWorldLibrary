using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Documents;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls.Primitives;

/// <summary>An abstract base class that provides functionality for text editing controls, including <see cref="T:System.Windows.Controls.TextBox" /> and <see cref="T:System.Windows.Controls.RichTextBox" />.</summary>
[Localizability(LocalizationCategory.Text)]
[TemplatePart(Name = "PART_ContentHost", Type = typeof(FrameworkElement))]
public abstract class TextBoxBase : Control
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.IsReadOnly" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.IsReadOnly" /> dependency property.</returns>
	public static readonly DependencyProperty IsReadOnlyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.IsReadOnlyCaretVisible" /> dependency property.</summary>
	public static readonly DependencyProperty IsReadOnlyCaretVisibleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.AcceptsReturn" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.AcceptsReturn" /> dependency property.</returns>
	public static readonly DependencyProperty AcceptsReturnProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.AcceptsTab" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.AcceptsTab" /> dependency property.</returns>
	public static readonly DependencyProperty AcceptsTabProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.HorizontalScrollBarVisibility" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.HorizontalScrollBarVisibility" /> dependency property.</returns>
	public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.VerticalScrollBarVisibility" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.VerticalScrollBarVisibility" /> dependency property.</returns>
	public static readonly DependencyProperty VerticalScrollBarVisibilityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.IsUndoEnabled" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.IsUndoEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsUndoEnabledProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.UndoLimit" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.UndoLimit" /> dependency property.</returns>
	public static readonly DependencyProperty UndoLimitProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.AutoWordSelection" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.AutoWordSelection" /> dependency property.</returns>
	public static readonly DependencyProperty AutoWordSelectionProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.SelectionBrush" /> dependency property.</summary>
	public static readonly DependencyProperty SelectionBrushProperty;

	public static readonly DependencyProperty SelectionTextBrushProperty;

	internal const double AdornerSelectionOpacityDefaultValue = 0.4;

	internal const double NonAdornerSelectionOpacityDefaultValue = 1.0;

	private static double SelectionOpacityDefaultValue;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.SelectionOpacity" /> dependency property.</summary>
	public static readonly DependencyProperty SelectionOpacityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.CaretBrush" /> dependency property.</summary>
	public static readonly DependencyProperty CaretBrushProperty;

	internal static readonly DependencyPropertyKey IsSelectionActivePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.IsSelectionActive" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.IsSelectionActive" /> dependency property.</returns>
	public static readonly DependencyProperty IsSelectionActiveProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.IsInactiveSelectionHighlightEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.IsInactiveSelectionHighlightEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsInactiveSelectionHighlightEnabledProperty;

	/// <summary> Identifies the <see cref="E:System.Windows.Controls.Primitives.TextBoxBase.TextChanged" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Primitives.TextBoxBase.TextChanged" /> routed event.</returns>
	public static readonly RoutedEvent TextChangedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Primitives.TextBoxBase.SelectionChanged" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Primitives.TextBoxBase.SelectionChanged" /> routed event.</returns>
	public static readonly RoutedEvent SelectionChangedEvent;

	private static DependencyObjectType _dType;

	private TextContainer _textContainer;

	private TextEditor _textEditor;

	private FrameworkElement _textBoxContentHost;

	private FrameworkElement _renderScope;

	private ScrollViewer _scrollViewer;

	private UndoAction _pendingUndoAction;

	internal const string ContentHostTemplateName = "PART_ContentHost";

	/// <summary>Gets or sets a value that indicates whether the text editing control is read-only to a user interacting with the control. </summary>
	/// <returns>true if the contents of the text editing control are read-only to a user; otherwise, the contents of the text editing control can be modified by the user. The default value is false.</returns>
	public bool IsReadOnly
	{
		get
		{
			return (bool)GetValue(TextEditor.IsReadOnlyProperty);
		}
		set
		{
			SetValue(TextEditor.IsReadOnlyProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a read-only text box displays a caret.</summary>
	/// <returns>true if a read-only text box displays a caret; otherwise, false. The default is false.</returns>
	public bool IsReadOnlyCaretVisible
	{
		get
		{
			return (bool)GetValue(IsReadOnlyCaretVisibleProperty);
		}
		set
		{
			SetValue(IsReadOnlyCaretVisibleProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates how the text editing control responds when the user presses the ENTER key.</summary>
	/// <returns>true if pressing the ENTER key inserts a new line at the current cursor position; otherwise, the ENTER key is ignored. The default value is false for <see cref="T:System.Windows.Controls.TextBox" /> and true for <see cref="T:System.Windows.Controls.RichTextBox" />.</returns>
	public bool AcceptsReturn
	{
		get
		{
			return (bool)GetValue(AcceptsReturnProperty);
		}
		set
		{
			SetValue(AcceptsReturnProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates how the text editing control responds when the user presses the TAB key.  </summary>
	/// <returns>true if pressing the TAB key inserts a tab character at the current cursor position; false if pressing the TAB key moves the focus to the next control that is marked as a tab stop and does not insert a tab character.The default value is false.</returns>
	public bool AcceptsTab
	{
		get
		{
			return (bool)GetValue(AcceptsTabProperty);
		}
		set
		{
			SetValue(AcceptsTabProperty, value);
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Controls.SpellCheck" /> object that provides access to spelling errors in the text contents of a <see cref="T:System.Windows.Controls.Primitives.TextBoxBase" /> or <see cref="T:System.Windows.Controls.RichTextBox" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.SpellCheck" /> object that provides access to spelling errors in the text contents of a <see cref="T:System.Windows.Controls.Primitives.TextBoxBase" /> or <see cref="T:System.Windows.Controls.RichTextBox" />.This property has no default value.</returns>
	public SpellCheck SpellCheck => new SpellCheck(this);

	/// <summary>Gets or sets a value that indicates whether a horizontal scroll bar is shown. </summary>
	/// <returns>A value that is defined by the <see cref="T:System.Windows.Controls.ScrollBarVisibility" /> enumeration.The default value is <see cref="F:System.Windows.Visibility.Hidden" />.</returns>
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

	/// <summary>Gets or sets a value that indicates whether a vertical scroll bar is shown. </summary>
	/// <returns>A value that is defined by the <see cref="T:System.Windows.Controls.ScrollBarVisibility" /> enumeration. The default value is <see cref="F:System.Windows.Visibility.Hidden" />.</returns>
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

	/// <summary>Gets the horizontal size of the visible content area.</summary>
	/// <returns>A floating-point value that specifies the horizontal size of the visible content area, in device-independent units (1/96th inch per unit).The value of this property is 0.0 if the text editing control is not configured to support scrolling.This property has no default value.</returns>
	public double ExtentWidth
	{
		get
		{
			if (ScrollViewer == null)
			{
				return 0.0;
			}
			return ScrollViewer.ExtentWidth;
		}
	}

	/// <summary>Gets the vertical size of the visible content area.</summary>
	/// <returns>A floating-point value that specifies the vertical size of the visible content area, in device-independent units (1/96th inch per unit).The value of this property is 0.0 if the text-editing control is not configured to support scrolling.This property has no default value.</returns>
	public double ExtentHeight
	{
		get
		{
			if (ScrollViewer == null)
			{
				return 0.0;
			}
			return ScrollViewer.ExtentHeight;
		}
	}

	/// <summary>Gets the horizontal size of the scrollable content area.</summary>
	/// <returns>A floating-point value that specifies the horizontal size of the scrollable content area, in device-independent units (1/96th inch per unit).The value of this property is 0.0 if the text editing control is not configured to support scrolling.This property has no default value.</returns>
	public double ViewportWidth
	{
		get
		{
			if (ScrollViewer == null)
			{
				return 0.0;
			}
			return ScrollViewer.ViewportWidth;
		}
	}

	/// <summary>Gets the vertical size of the scrollable content area.</summary>
	/// <returns>A floating-point value that specifies the vertical size of the scrollable content area, in device-independent units (1/96th inch per unit).The value of this property is 0.0 if the text editing control is not configured to support scrolling.This property has no default value.</returns>
	public double ViewportHeight
	{
		get
		{
			if (ScrollViewer == null)
			{
				return 0.0;
			}
			return ScrollViewer.ViewportHeight;
		}
	}

	/// <summary>Gets or sets the horizontal scroll position.</summary>
	/// <returns>A floating-point value that specifies the horizontal scroll position, in device-independent units (1/96th inch per unit). Setting this property causes the text editing control to scroll to the specified horizontal offset. Reading this property returns the current horizontal offset.The value of this property is 0.0 if the text editing control is not configured to support scrolling.This property has no default value.</returns>
	/// <exception cref="T:System.ArgumentException">An attempt is made to set this property to a negative value.</exception>
	public double HorizontalOffset
	{
		get
		{
			if (ScrollViewer == null)
			{
				return 0.0;
			}
			return ScrollViewer.HorizontalOffset;
		}
	}

	/// <summary>Gets or sets the vertical scroll position.</summary>
	/// <returns>A floating-point value that specifies the vertical scroll position, in device-independent units (1/96th inch per unit).Setting this property causes the text editing control to scroll to the specified vertical offset. Reading this property returns the current vertical offset.The value of this property is 0.0 if the text editing control is not configured to support scrolling.This property has no default value.</returns>
	/// <exception cref="T:System.ArgumentException">An attempt is made to set this property to a negative value.</exception>
	public double VerticalOffset
	{
		get
		{
			if (ScrollViewer == null)
			{
				return 0.0;
			}
			return ScrollViewer.VerticalOffset;
		}
	}

	/// <summary>Gets a value that indicates whether the most recent action can be undone.</summary>
	/// <returns>true if the most recent action can be undone; otherwise, false.This property has no default value.</returns>
	public bool CanUndo
	{
		get
		{
			UndoManager undoManager = UndoManager.GetUndoManager(this);
			if (undoManager != null && _pendingUndoAction != UndoAction.Clear && (undoManager.UndoCount > undoManager.MinUndoStackCount || (undoManager.State != UndoState.Undo && _pendingUndoAction == UndoAction.Create)))
			{
				return true;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether the most recent undo action can be redone.</summary>
	/// <returns>true if the most recent undo action can be redone; otherwise, false.This property has no default value.</returns>
	public bool CanRedo
	{
		get
		{
			UndoManager undoManager = UndoManager.GetUndoManager(this);
			if (undoManager != null && _pendingUndoAction != UndoAction.Clear && (undoManager.RedoCount > 0 || (undoManager.State == UndoState.Undo && _pendingUndoAction == UndoAction.Create)))
			{
				return true;
			}
			return false;
		}
	}

	/// <summary>Gets or sets a value that indicates whether undo support is enabled for the text-editing control.  </summary>
	/// <returns>true if undo support is enabled; otherwise, false. The default value is true.</returns>
	public bool IsUndoEnabled
	{
		get
		{
			return (bool)GetValue(IsUndoEnabledProperty);
		}
		set
		{
			SetValue(IsUndoEnabledProperty, value);
		}
	}

	/// <summary>Gets or sets the number of actions stored in the undo queue.</summary>
	/// <returns>The number of actions stored in the undo queue. The default is –1, which means the undo queue is limited to the memory that is available.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.UndoLimit" /> is set after calling <see cref="M:System.Windows.Controls.Primitives.TextBoxBase.BeginChange" /> and before calling <see cref="M:System.Windows.Controls.Primitives.TextBoxBase.EndChange" />.</exception>
	public int UndoLimit
	{
		get
		{
			return (int)GetValue(UndoLimitProperty);
		}
		set
		{
			SetValue(UndoLimitProperty, value);
		}
	}

	/// <summary>Gets or sets a value that determines whether when a user selects part of a word by dragging across it with the mouse, the rest of the word is selected.</summary>
	/// <returns>true if automatic word selection is enabled; otherwise, false.The default value is false.</returns>
	public bool AutoWordSelection
	{
		get
		{
			return (bool)GetValue(AutoWordSelectionProperty);
		}
		set
		{
			SetValue(AutoWordSelectionProperty, value);
		}
	}

	/// <summary>Gets or sets the brush that highlights selected text.</summary>
	/// <returns>The brush that highlights selected text.</returns>
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

	public Brush SelectionTextBrush
	{
		get
		{
			return (Brush)GetValue(SelectionTextBrushProperty);
		}
		set
		{
			SetValue(SelectionTextBrushProperty, value);
		}
	}

	/// <summary>Gets or sets the opacity of the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.SelectionBrush" />.</summary>
	/// <returns>The opacity of the <see cref="P:System.Windows.Controls.Primitives.TextBoxBase.SelectionBrush" />. The default is 0.4.</returns>
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

	/// <summary>Gets or sets the brush that is used to paint the caret of the text box.</summary>
	/// <returns>The brush that is used to paint the caret of the text box.</returns>
	public Brush CaretBrush
	{
		get
		{
			return (Brush)GetValue(CaretBrushProperty);
		}
		set
		{
			SetValue(CaretBrushProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether the text box has focus and selected text.</summary>
	/// <returns>true if the text box has focus and selected text; otherwise, false. The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public bool IsSelectionActive => (bool)GetValue(IsSelectionActiveProperty);

	/// <summary>Gets or sets a value that indicates whether the text box displays selected text when the text box does not have focus.</summary>
	/// <returns>true if the text box displays selected text when the text box does not have focus; otherwise, false. The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
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

	internal ScrollViewer ScrollViewer
	{
		get
		{
			if (_scrollViewer == null && _textEditor != null)
			{
				_scrollViewer = _textEditor._Scroller as ScrollViewer;
			}
			return _scrollViewer;
		}
	}

	internal TextSelection TextSelectionInternal => (TextSelection)_textEditor.Selection;

	internal TextContainer TextContainer => _textContainer;

	internal FrameworkElement RenderScope => _renderScope;

	internal UndoAction PendingUndoAction
	{
		get
		{
			return _pendingUndoAction;
		}
		set
		{
			_pendingUndoAction = value;
		}
	}

	internal TextEditor TextEditor => _textEditor;

	internal bool IsContentHostAvailable => _textBoxContentHost != null;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Occurs when content changes in the text element.</summary>
	public event TextChangedEventHandler TextChanged
	{
		add
		{
			AddHandler(TextChangedEvent, value);
		}
		remove
		{
			RemoveHandler(TextChangedEvent, value);
		}
	}

	/// <summary>Occurs when the text selection has changed.</summary>
	public event RoutedEventHandler SelectionChanged
	{
		add
		{
			AddHandler(SelectionChangedEvent, value);
		}
		remove
		{
			RemoveHandler(SelectionChangedEvent, value);
		}
	}

	static TextBoxBase()
	{
		IsReadOnlyProperty = TextEditor.IsReadOnlyProperty.AddOwner(typeof(TextBoxBase), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, Control.OnVisualStatePropertyChanged));
		IsReadOnlyCaretVisibleProperty = DependencyProperty.Register("IsReadOnlyCaretVisible", typeof(bool), typeof(TextBoxBase), new FrameworkPropertyMetadata(false, OnIsReadOnlyCaretVisiblePropertyChanged));
		AcceptsReturnProperty = KeyboardNavigation.AcceptsReturnProperty.AddOwner(typeof(TextBoxBase));
		AcceptsTabProperty = DependencyProperty.Register("AcceptsTab", typeof(bool), typeof(TextBoxBase), new FrameworkPropertyMetadata(false));
		HorizontalScrollBarVisibilityProperty = ScrollViewer.HorizontalScrollBarVisibilityProperty.AddOwner(typeof(TextBoxBase), new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden, OnScrollViewerPropertyChanged));
		VerticalScrollBarVisibilityProperty = ScrollViewer.VerticalScrollBarVisibilityProperty.AddOwner(typeof(TextBoxBase), new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden, OnScrollViewerPropertyChanged));
		IsUndoEnabledProperty = DependencyProperty.Register("IsUndoEnabled", typeof(bool), typeof(TextBoxBase), new FrameworkPropertyMetadata(true, OnIsUndoEnabledChanged));
		UndoLimitProperty = DependencyProperty.Register("UndoLimit", typeof(int), typeof(TextBoxBase), new FrameworkPropertyMetadata(UndoManager.UndoLimitDefaultValue, OnUndoLimitChanged), UndoLimitValidateValue);
		AutoWordSelectionProperty = DependencyProperty.Register("AutoWordSelection", typeof(bool), typeof(TextBoxBase), new FrameworkPropertyMetadata(false));
		SelectionBrushProperty = DependencyProperty.Register("SelectionBrush", typeof(Brush), typeof(TextBoxBase), new FrameworkPropertyMetadata(GetDefaultSelectionBrush(), UpdateCaretElement));
		SelectionTextBrushProperty = DependencyProperty.Register("SelectionTextBrush", typeof(Brush), typeof(TextBoxBase), new FrameworkPropertyMetadata(GetDefaultSelectionTextBrush(), UpdateCaretElement));
		SelectionOpacityDefaultValue = (FrameworkAppContextSwitches.UseAdornerForTextboxSelectionRendering ? 0.4 : 1.0);
		SelectionOpacityProperty = DependencyProperty.Register("SelectionOpacity", typeof(double), typeof(TextBoxBase), new FrameworkPropertyMetadata(SelectionOpacityDefaultValue, UpdateCaretElement));
		CaretBrushProperty = DependencyProperty.Register("CaretBrush", typeof(Brush), typeof(TextBoxBase), new FrameworkPropertyMetadata(null, UpdateCaretElement));
		IsSelectionActivePropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsSelectionActive", typeof(bool), typeof(TextBoxBase), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsSelectionActiveProperty = IsSelectionActivePropertyKey.DependencyProperty;
		IsInactiveSelectionHighlightEnabledProperty = DependencyProperty.Register("IsInactiveSelectionHighlightEnabled", typeof(bool), typeof(TextBoxBase));
		TextChangedEvent = EventManager.RegisterRoutedEvent("TextChanged", RoutingStrategy.Bubble, typeof(TextChangedEventHandler), typeof(TextBoxBase));
		SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TextBoxBase));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxBase), new FrameworkPropertyMetadata(typeof(TextBoxBase)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(TextBoxBase));
		Control.PaddingProperty.OverrideMetadata(typeof(TextBoxBase), new FrameworkPropertyMetadata(OnScrollViewerPropertyChanged));
		InputMethod.IsInputMethodEnabledProperty.OverrideMetadata(typeof(TextBoxBase), new FrameworkPropertyMetadata(OnInputMethodEnabledPropertyChanged));
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(TextBoxBase), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		UIElement.IsMouseOverPropertyKey.OverrideMetadata(typeof(TextBoxBase), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
	}

	internal TextBoxBase()
	{
		CoerceValue(HorizontalScrollBarVisibilityProperty);
	}

	/// <summary>Appends a string to the contents of a text control.</summary>
	/// <param name="textData">A string that specifies the text to append to the current contents of the text control.</param>
	public void AppendText(string textData)
	{
		if (textData != null)
		{
			new TextRange(_textContainer.End, _textContainer.End).Text = textData;
		}
	}

	/// <summary>Is called when a control template is applied.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		AttachToVisualTree();
	}

	/// <summary>Copies the current selection of the text editing control to the <see cref="T:System.Windows.Clipboard" />.</summary>
	public void Copy()
	{
		TextEditorCopyPaste.Copy(TextEditor, userInitiated: false);
	}

	/// <summary>Removes the current selection from the text editing control and copies it to the <see cref="T:System.Windows.Clipboard" />.</summary>
	public void Cut()
	{
		TextEditorCopyPaste.Cut(TextEditor, userInitiated: false);
	}

	/// <summary>Pastes the contents of the Clipboard over the current selection in the text editing control.</summary>
	public void Paste()
	{
		TextEditorCopyPaste.Paste(TextEditor);
	}

	/// <summary>Selects all the contents of the text editing control.</summary>
	public void SelectAll()
	{
		using (TextSelectionInternal.DeclareChangeBlock())
		{
			TextSelectionInternal.Select(_textContainer.Start, _textContainer.End);
		}
	}

	/// <summary>Scrolls the contents of the control to the left by one line.</summary>
	public void LineLeft()
	{
		if (ScrollViewer != null)
		{
			UpdateLayout();
			ScrollViewer.LineLeft();
		}
	}

	/// <summary>Scrolls the contents of the control to the right by one line.</summary>
	public void LineRight()
	{
		if (ScrollViewer != null)
		{
			UpdateLayout();
			ScrollViewer.LineRight();
		}
	}

	/// <summary>Scrolls the contents of the control to the left by one page.</summary>
	public void PageLeft()
	{
		if (ScrollViewer != null)
		{
			UpdateLayout();
			ScrollViewer.PageLeft();
		}
	}

	/// <summary>Scrolls the contents of the control to the right by one page.</summary>
	public void PageRight()
	{
		if (ScrollViewer != null)
		{
			UpdateLayout();
			ScrollViewer.PageRight();
		}
	}

	/// <summary>Scrolls the contents of the control upward by one line. </summary>
	public void LineUp()
	{
		UpdateLayout();
		DoLineUp();
	}

	/// <summary>Scrolls the contents of the control down by one line.</summary>
	public void LineDown()
	{
		UpdateLayout();
		DoLineDown();
	}

	/// <summary>Scrolls the contents of the control up by one page.</summary>
	public void PageUp()
	{
		if (ScrollViewer != null)
		{
			UpdateLayout();
			ScrollViewer.PageUp();
		}
	}

	/// <summary>Scrolls the contents of the control down by one page.</summary>
	public void PageDown()
	{
		if (ScrollViewer != null)
		{
			UpdateLayout();
			ScrollViewer.PageDown();
		}
	}

	/// <summary>Scrolls the view of the editing control to the beginning of the viewport.</summary>
	public void ScrollToHome()
	{
		if (ScrollViewer != null)
		{
			UpdateLayout();
			ScrollViewer.ScrollToHome();
		}
	}

	/// <summary>Scrolls the view of the editing control to the end of the content.</summary>
	public void ScrollToEnd()
	{
		if (ScrollViewer != null)
		{
			UpdateLayout();
			ScrollViewer.ScrollToEnd();
		}
	}

	/// <summary>Scrolls the contents of the editing control to the specified horizontal offset.</summary>
	/// <param name="offset">A double value that specifies the horizontal offset to scroll to.</param>
	public void ScrollToHorizontalOffset(double offset)
	{
		if (double.IsNaN(offset))
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (ScrollViewer != null)
		{
			UpdateLayout();
			ScrollViewer.ScrollToHorizontalOffset(offset);
		}
	}

	/// <summary>Scrolls the contents of the editing control to the specified vertical offset.</summary>
	/// <param name="offset">A double value that specifies the vertical offset to scroll to.</param>
	public void ScrollToVerticalOffset(double offset)
	{
		if (double.IsNaN(offset))
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (ScrollViewer != null)
		{
			UpdateLayout();
			ScrollViewer.ScrollToVerticalOffset(offset);
		}
	}

	/// <summary>Undoes the most recent undo command. In other words, undoes the most recent undo unit on the undo stack.</summary>
	/// <returns>true if the undo operation was successful; otherwise, false. This method returns false if the undo stack is empty.</returns>
	public bool Undo()
	{
		UndoManager undoManager = UndoManager.GetUndoManager(this);
		if (undoManager != null && undoManager.UndoCount > undoManager.MinUndoStackCount)
		{
			TextEditor.Undo();
			return true;
		}
		return false;
	}

	/// <summary>Undoes the most recent undo command. In other words, redoes the most recent undo unit on the undo stack.</summary>
	/// <returns>true if the redo operation was successful; otherwise, false. This method returns false if there is no undo command available (the undo stack is empty).</returns>
	public bool Redo()
	{
		UndoManager undoManager = UndoManager.GetUndoManager(this);
		if (undoManager != null && undoManager.RedoCount > 0)
		{
			TextEditor.Redo();
			return true;
		}
		return false;
	}

	/// <summary>Locks the most recent undo unit of the undo stack of the application. This prevents the locked unit from being merged with undo units that are added subsequently.</summary>
	public void LockCurrentUndoUnit()
	{
		UndoManager undoManager = UndoManager.GetUndoManager(this);
		if (undoManager == null)
		{
			return;
		}
		IParentUndoUnit openedUnit = undoManager.OpenedUnit;
		if (openedUnit != null)
		{
			while (openedUnit.OpenedUnit != null)
			{
				openedUnit = openedUnit.OpenedUnit;
			}
			if (openedUnit.LastUnit is IParentUndoUnit)
			{
				openedUnit.OnNextAdd();
			}
		}
		else if (undoManager.LastUnit is IParentUndoUnit)
		{
			((IParentUndoUnit)undoManager.LastUnit).OnNextAdd();
		}
	}

	/// <summary>Begins a change block.</summary>
	public void BeginChange()
	{
		TextEditor.Selection.BeginChange();
	}

	/// <summary>Ends a change block.</summary>
	public void EndChange()
	{
		if (TextEditor.Selection.ChangeBlockLevel == 0)
		{
			throw new InvalidOperationException(SR.TextBoxBase_UnmatchedEndChange);
		}
		TextEditor.Selection.EndChange();
	}

	/// <summary>Creates a change block.</summary>
	/// <returns>An <see cref="T:System.IDisposable" /> object that refers to a new change block.</returns>
	public IDisposable DeclareChangeBlock()
	{
		return TextEditor.Selection.DeclareChangeBlock();
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (!base.IsEnabled)
		{
			VisualStateManager.GoToState(this, "Disabled", useTransitions);
		}
		else if (IsReadOnly)
		{
			VisualStateManager.GoToState(this, "ReadOnly", useTransitions);
		}
		else if (base.IsMouseOver)
		{
			VisualStateManager.GoToState(this, "MouseOver", useTransitions);
		}
		else
		{
			VisualStateManager.GoToState(this, "Normal", useTransitions);
		}
		if (base.IsKeyboardFocused)
		{
			VisualStateManager.GoToState(this, "Focused", useTransitions);
		}
		else
		{
			VisualStateManager.GoToState(this, "Unfocused", useTransitions);
		}
		base.ChangeVisualState(useTransitions);
	}

	/// <summary>Is called when content in this editing control changes.</summary>
	/// <param name="e">The arguments that are associated with the <see cref="E:System.Windows.Controls.Primitives.TextBoxBase.TextChanged" /> event.</param>
	protected virtual void OnTextChanged(TextChangedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Is called when the caret or current selection changes position.</summary>
	/// <param name="e">The arguments that are associated with the <see cref="E:System.Windows.Controls.Primitives.TextBoxBase.SelectionChanged" /> event.</param>
	protected virtual void OnSelectionChanged(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Is called when the control template changes.</summary>
	/// <param name="oldTemplate">A <see cref="T:System.Windows.Controls.ControlTemplate" /> object that specifies the control template that is currently active.</param>
	/// <param name="newTemplate">A <see cref="T:System.Windows.Controls.ControlTemplate" /> object that specifies a new control template to use.</param>
	protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
	{
		base.OnTemplateChanged(oldTemplate, newTemplate);
		if (oldTemplate != null && newTemplate != null && oldTemplate.VisualTree != newTemplate.VisualTree)
		{
			DetachFromVisualTree();
		}
	}

	/// <summary>Is called when a <see cref="E:System.Windows.UIElement.MouseWheel" /> event is routed to this class (or to a class that inherits from this class).</summary>
	/// <param name="e">The mouse wheel arguments that are associated with this event.</param>
	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		if (ScrollViewer != null && ((e.Delta > 0 && VerticalOffset != 0.0) || (e.Delta < 0 && VerticalOffset < ScrollViewer.ScrollableHeight)))
		{
			Invariant.Assert(RenderScope is IScrollInfo);
			if (e.Delta > 0)
			{
				((IScrollInfo)RenderScope).MouseWheelUp();
			}
			else
			{
				((IScrollInfo)RenderScope).MouseWheelDown();
			}
			e.Handled = true;
		}
		base.OnMouseWheel(e);
	}

	/// <summary>Called when the <see cref="E:System.Windows.UIElement.KeyDown" /> occurs.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnPreviewKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnPreviewKeyDown(e);
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.Input.Keyboard.KeyDown" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Provides data about the event.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnKeyDown(e);
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.Input.Keyboard.KeyUp" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Provides data about the event.</param>
	protected override void OnKeyUp(KeyEventArgs e)
	{
		base.OnKeyUp(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnKeyUp(e);
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.Input.TextCompositionManager.TextInput" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Provides data about the event.</param>
	protected override void OnTextInput(TextCompositionEventArgs e)
	{
		base.OnTextInput(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnTextInput(e);
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.Input.Mouse.MouseDown" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Provides data about the event.</param>
	protected override void OnMouseDown(MouseButtonEventArgs e)
	{
		base.OnMouseDown(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnMouseDown(e);
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.Input.Mouse.MouseMove" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Provides data about the event.</param>
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnMouseMove(e);
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.Input.Mouse.MouseUp" /> event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Arguments of the event. These arguments will include details about which mouse button was depressed, and the handled state.</param>
	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnMouseUp(e);
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.Input.Mouse.QueryCursor" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Provides data about the event.</param>
	protected override void OnQueryCursor(QueryCursorEventArgs e)
	{
		base.OnQueryCursor(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnQueryCursor(e);
		}
	}

	/// <summary>  Invoked whenever an unhandled <see cref="E:System.Windows.DragDrop.QueryContinueDrag" /> attached  routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Provides data about the event.</param>
	protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e)
	{
		base.OnQueryContinueDrag(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnQueryContinueDrag(e);
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.DragDrop.GiveFeedback" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Provides data about the event.</param>
	protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
	{
		base.OnGiveFeedback(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnGiveFeedback(e);
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.DragDrop.DragEnter" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Provides data about the event.</param>
	protected override void OnDragEnter(DragEventArgs e)
	{
		base.OnDragEnter(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnDragEnter(e);
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.DragDrop.DragOver" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Provides data about the event.</param>
	protected override void OnDragOver(DragEventArgs e)
	{
		base.OnDragOver(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnDragOver(e);
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.DragDrop.DragLeave" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Provides data about the event.</param>
	protected override void OnDragLeave(DragEventArgs e)
	{
		base.OnDragLeave(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnDragLeave(e);
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.DragDrop.DragEnter" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Provides data about the event.</param>
	protected override void OnDrop(DragEventArgs e)
	{
		base.OnDrop(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnDrop(e);
		}
	}

	/// <summary>Called whenever an unhandled <see cref="E:System.Windows.FrameworkElement.ContextMenuOpening" /> routed event reaches this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Arguments of the event.</param>
	protected override void OnContextMenuOpening(ContextMenuEventArgs e)
	{
		base.OnContextMenuOpening(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnContextMenuOpening(e);
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.Input.Keyboard.GotKeyboardFocus" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Provides data about the event.</param>
	protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		base.OnGotKeyboardFocus(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnGotKeyboardFocus(e);
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.Input.Keyboard.LostKeyboardFocus" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.</summary>
	/// <param name="e">Provides data about the event.</param>
	protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		base.OnLostKeyboardFocus(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnLostKeyboardFocus(e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.UIElement.LostFocus" /> event (using the provided arguments).</summary>
	/// <param name="e">Provides data about the event.</param>
	protected override void OnLostFocus(RoutedEventArgs e)
	{
		base.OnLostFocus(e);
		if (!e.Handled && _textEditor != null)
		{
			_textEditor.OnLostFocus(e);
		}
	}

	internal abstract FrameworkElement CreateRenderScope();

	internal virtual void OnTextContainerChanged(object sender, TextContainerChangedEventArgs e)
	{
		if (!e.HasContentAddedOrRemoved && !e.HasLocalPropertyValueChange)
		{
			return;
		}
		UndoManager undoManager = UndoManager.GetUndoManager(this);
		UndoAction action = (_pendingUndoAction = ((undoManager == null) ? UndoAction.Create : ((_textEditor.UndoState == UndoState.Redo) ? UndoAction.Redo : ((_textEditor.UndoState == UndoState.Undo) ? UndoAction.Undo : ((undoManager.OpenedUnit == null) ? UndoAction.Clear : ((undoManager.LastReopenedUnit == undoManager.OpenedUnit) ? UndoAction.Merge : UndoAction.Create))))));
		try
		{
			OnTextChanged(new TextChangedEventArgs(TextChangedEvent, action, new ReadOnlyCollection<TextChange>(e.Changes.Values)));
		}
		finally
		{
			_pendingUndoAction = UndoAction.None;
		}
	}

	internal void InitializeTextContainer(TextContainer textContainer)
	{
		Invariant.Assert(textContainer != null);
		Invariant.Assert(textContainer.TextSelection == null);
		if (_textContainer != null)
		{
			Invariant.Assert(_textEditor != null);
			Invariant.Assert(_textEditor.TextContainer == _textContainer);
			Invariant.Assert(_textEditor.TextContainer.TextSelection == _textEditor.Selection);
			DetachFromVisualTree();
			_textEditor.OnDetach();
		}
		_textContainer = textContainer;
		_textContainer.Changed += OnTextContainerChanged;
		_textEditor = new TextEditor(_textContainer, this, isUndoEnabled: true);
		_textEditor.Selection.Changed += OnSelectionChangedInternal;
		UndoManager undoManager = UndoManager.GetUndoManager(this);
		if (undoManager != null)
		{
			undoManager.UndoLimit = UndoLimit;
		}
	}

	internal TextPointer GetTextPositionFromPointInternal(Point point, bool snapToText)
	{
		TransformToDescendant(RenderScope)?.TryTransform(point, out point);
		if (TextEditor.GetTextView(RenderScope).Validate(point))
		{
			return (TextPointer)TextEditor.GetTextView(RenderScope).GetTextPositionFromPoint(point, snapToText);
		}
		return snapToText ? TextContainer.Start : null;
	}

	internal bool GetRectangleFromTextPosition(TextPointer position, out Rect rect)
	{
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}
		if (TextEditor.GetTextView(RenderScope).Validate(position))
		{
			rect = TextEditor.GetTextView(RenderScope).GetRectangleFromTextPosition(position);
			Point result = new Point(0.0, 0.0);
			TransformToDescendant(RenderScope)?.TryTransform(result, out result);
			rect.X -= result.X;
			rect.Y -= result.Y;
		}
		else
		{
			rect = Rect.Empty;
		}
		return rect != Rect.Empty;
	}

	internal virtual void AttachToVisualTree()
	{
		DetachFromVisualTree();
		SetRenderScopeToContentHost();
		if (ScrollViewer != null)
		{
			ScrollViewer.ScrollChanged += OnScrollChanged;
			SetValue(TextEditor.PageHeightProperty, ScrollViewer.ViewportHeight);
			ScrollViewer.Focusable = false;
			ScrollViewer.HandlesMouseWheelScrolling = false;
			if (ScrollViewer.Background == null)
			{
				ScrollViewer.Background = Brushes.Transparent;
			}
			OnScrollViewerPropertyChanged(this, new DependencyPropertyChangedEventArgs(ScrollViewer.HorizontalScrollBarVisibilityProperty, null, GetValue(HorizontalScrollBarVisibilityProperty)));
			OnScrollViewerPropertyChanged(this, new DependencyPropertyChangedEventArgs(ScrollViewer.VerticalScrollBarVisibilityProperty, null, GetValue(VerticalScrollBarVisibilityProperty)));
			OnScrollViewerPropertyChanged(this, new DependencyPropertyChangedEventArgs(Control.PaddingProperty, null, GetValue(Control.PaddingProperty)));
		}
		else
		{
			ClearValue(TextEditor.PageHeightProperty);
		}
	}

	internal virtual void DoLineUp()
	{
		if (ScrollViewer != null)
		{
			ScrollViewer.LineUp();
		}
	}

	internal virtual void DoLineDown()
	{
		if (ScrollViewer != null)
		{
			ScrollViewer.LineDown();
		}
	}

	internal override void AddToEventRouteCore(EventRoute route, RoutedEventArgs args)
	{
		base.AddToEventRouteCore(route, args);
		Visual visual = RenderScope;
		while (visual != this && visual != null)
		{
			if (visual is UIElement)
			{
				((UIElement)visual).AddToEventRoute(route, args);
			}
			visual = VisualTreeHelper.GetParent(visual) as Visual;
		}
	}

	internal void ChangeUndoEnabled(bool value)
	{
		if (TextSelectionInternal.ChangeBlockLevel > 0)
		{
			throw new InvalidOperationException(SR.TextBoxBase_CantSetIsUndoEnabledInsideChangeBlock);
		}
		UndoManager undoManager = UndoManager.GetUndoManager(this);
		if (undoManager != null)
		{
			if (!value && undoManager.IsEnabled)
			{
				undoManager.Clear();
			}
			undoManager.IsEnabled = value;
		}
	}

	internal void ChangeUndoLimit(object value)
	{
		UndoManager undoManager = UndoManager.GetUndoManager(this);
		if (undoManager != null)
		{
			if (undoManager.OpenedUnit != null)
			{
				throw new InvalidOperationException(SR.TextBoxBase_CantSetIsUndoEnabledInsideChangeBlock);
			}
			int undoLimit = ((value != DependencyProperty.UnsetValue) ? ((int)value) : UndoManager.UndoLimitDefaultValue);
			undoManager.UndoLimit = undoLimit;
		}
	}

	private void DetachFromVisualTree()
	{
		if (_textEditor != null)
		{
			_textEditor.Selection.DetachFromVisualTree();
		}
		if (ScrollViewer != null)
		{
			ScrollViewer.ScrollChanged -= OnScrollChanged;
		}
		_scrollViewer = null;
		ClearContentHost();
	}

	private void InitializeRenderScope()
	{
		if (_renderScope != null)
		{
			ITextView textView = (ITextView)((IServiceProvider)_renderScope).GetService(typeof(ITextView));
			TextContainer.TextView = textView;
			_textEditor.TextView = textView;
			if (ScrollViewer != null)
			{
				ScrollViewer.CanContentScroll = true;
			}
		}
	}

	private void UninitializeRenderScope()
	{
		_textEditor.TextView = null;
		if (_renderScope is TextBoxView textBoxView)
		{
			textBoxView.RemoveTextContainerListeners();
		}
		else if (_renderScope is FlowDocumentView flowDocumentView)
		{
			if (flowDocumentView.Document != null)
			{
				flowDocumentView.Document.Uninitialize();
				flowDocumentView.Document = null;
			}
		}
		else
		{
			Invariant.Assert(_renderScope == null, "_renderScope must be null here");
		}
	}

	private static Brush GetDefaultSelectionBrush()
	{
		SolidColorBrush solidColorBrush = new SolidColorBrush(SystemColors.HighlightColor);
		solidColorBrush.Freeze();
		return solidColorBrush;
	}

	private static Brush GetDefaultSelectionTextBrush()
	{
		SolidColorBrush solidColorBrush = new SolidColorBrush(SystemColors.HighlightTextColor);
		solidColorBrush.Freeze();
		return solidColorBrush;
	}

	private static object OnPageHeightGetValue(DependencyObject d)
	{
		return ((TextBoxBase)d).ViewportHeight;
	}

	private void SetRenderScopeToContentHost()
	{
		FrameworkElement renderScope = CreateRenderScope();
		ClearContentHost();
		_textBoxContentHost = GetTemplateChild("PART_ContentHost") as FrameworkElement;
		_renderScope = renderScope;
		if (_textBoxContentHost is ScrollViewer)
		{
			ScrollViewer scrollViewer = (ScrollViewer)_textBoxContentHost;
			if (scrollViewer.Content != null)
			{
				_renderScope = null;
				_textBoxContentHost = null;
				throw new NotSupportedException(SR.TextBoxScrollViewerMarkedAsTextBoxContentMustHaveNoContent);
			}
			scrollViewer.Content = _renderScope;
		}
		else if (_textBoxContentHost is Decorator)
		{
			Decorator decorator = (Decorator)_textBoxContentHost;
			if (decorator.Child != null)
			{
				_renderScope = null;
				_textBoxContentHost = null;
				throw new NotSupportedException(SR.TextBoxDecoratorMarkedAsTextBoxContentMustHaveNoContent);
			}
			decorator.Child = _renderScope;
		}
		else
		{
			_renderScope = null;
			if (_textBoxContentHost != null)
			{
				_textBoxContentHost = null;
				throw new NotSupportedException(SR.TextBoxInvalidTextContainer);
			}
		}
		InitializeRenderScope();
	}

	private void ClearContentHost()
	{
		UninitializeRenderScope();
		if (_textBoxContentHost is ScrollViewer)
		{
			((ScrollViewer)_textBoxContentHost).Content = null;
		}
		else if (_textBoxContentHost is Decorator)
		{
			((Decorator)_textBoxContentHost).Child = null;
		}
		else
		{
			Invariant.Assert(_textBoxContentHost == null, "_textBoxContentHost must be null here");
		}
		_textBoxContentHost = null;
	}

	private static void OnIsReadOnlyCaretVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TextBoxBase obj = (TextBoxBase)d;
		obj.TextSelectionInternal.UpdateCaretState(CaretScrollMethod.None);
		((ITextSelection)obj.TextSelectionInternal).RefreshCaret();
	}

	internal virtual void OnScrollChanged(object sender, ScrollChangedEventArgs e)
	{
		if (e.ViewportHeightChange != 0.0)
		{
			SetValue(TextEditor.PageHeightProperty, e.ViewportHeight);
		}
	}

	private void OnSelectionChangedInternal(object sender, EventArgs e)
	{
		OnSelectionChanged(new RoutedEventArgs(SelectionChangedEvent));
	}

	internal static void OnScrollViewerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is TextBoxBase { ScrollViewer: not null } textBoxBase)
		{
			object newValue = e.NewValue;
			if (newValue == DependencyProperty.UnsetValue)
			{
				textBoxBase.ScrollViewer.ClearValue(e.Property);
			}
			else
			{
				textBoxBase.ScrollViewer.SetValue(e.Property, newValue);
			}
		}
	}

	private static void OnIsUndoEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TextBoxBase)d).ChangeUndoEnabled((bool)e.NewValue);
	}

	private static bool UndoLimitValidateValue(object value)
	{
		return (int)value >= -1;
	}

	private static void OnUndoLimitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TextBoxBase)d).ChangeUndoLimit(e.NewValue);
	}

	private static void OnInputMethodEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TextBoxBase textBoxBase = (TextBoxBase)d;
		if (textBoxBase.TextEditor != null && textBoxBase.TextEditor.TextStore != null && (bool)e.NewValue && Keyboard.FocusedElement == textBoxBase)
		{
			textBoxBase.TextEditor.TextStore.OnGotFocus();
		}
	}

	private static void UpdateCaretElement(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TextBoxBase textBoxBase = (TextBoxBase)d;
		if (textBoxBase.TextSelectionInternal == null)
		{
			return;
		}
		CaretElement caretElement = textBoxBase.TextSelectionInternal.CaretElement;
		if (caretElement != null)
		{
			if (e.Property == CaretBrushProperty)
			{
				caretElement.UpdateCaretBrush(TextSelection.GetCaretBrush(textBoxBase.TextEditor));
			}
			caretElement.InvalidateVisual();
		}
		TextBoxView textBoxView = textBoxBase?.RenderScope as TextBoxView;
		if (textBoxView != null && ((ITextView)textBoxView).RendersOwnSelection)
		{
			textBoxView.InvalidateArrange();
		}
	}
}
