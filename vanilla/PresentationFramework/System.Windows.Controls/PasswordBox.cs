using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a control designed for entering and handling passwords.</summary>
[TemplatePart(Name = "PART_ContentHost", Type = typeof(FrameworkElement))]
public sealed class PasswordBox : Control, ITextBoxViewHost
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.PasswordBox.PasswordChar" />  dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.PasswordBox.PasswordChar" /> dependency property.</returns>
	public static readonly DependencyProperty PasswordCharProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.PasswordBox.MaxLength" />  dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.PasswordBox.MaxLength" /> dependency property.</returns>
	public static readonly DependencyProperty MaxLengthProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.PasswordBox.SelectionBrush" /> dependency property.</summary>
	public static readonly DependencyProperty SelectionBrushProperty;

	public static readonly DependencyProperty SelectionTextBrushProperty;

	/// <summary>Identifies the <see cref="F:System.Windows.Controls.PasswordBox.SelectionOpacityProperty" /> dependency property.</summary>
	public static readonly DependencyProperty SelectionOpacityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.PasswordBox.CaretBrush" /> dependency property.</summary>
	public static readonly DependencyProperty CaretBrushProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.PasswordBox.IsSelectionActive" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.PasswordBox.IsSelectionActive" /> dependency property.</returns>
	public static readonly DependencyProperty IsSelectionActiveProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.PasswordBox.IsInactiveSelectionHighlightEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.PasswordBox.IsInactiveSelectionHighlightEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsInactiveSelectionHighlightEnabledProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.PasswordBox.PasswordChanged" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.PasswordBox.PasswordChanged" /> routed event.</returns>
	public static readonly RoutedEvent PasswordChangedEvent;

	private TextEditor _textEditor;

	private PasswordTextContainer _textContainer;

	private TextBoxView _renderScope;

	private ScrollViewer _scrollViewer;

	private Border _border;

	private FrameworkElement _passwordBoxContentHost;

	private const int _defaultWidth = 100;

	private const int _defaultHeight = 20;

	private const string ContentHostTemplateName = "PART_ContentHost";

	private static DependencyObjectType _dType;

	private NavigationService _navigationService;

	/// <summary>Gets or sets the password currently held by the <see cref="T:System.Windows.Controls.PasswordBox" />.</summary>
	/// <returns>A string representing the password currently held by the <see cref="T:System.Windows.Controls.PasswordBox" />.The default value is <see cref="F:System.String.Empty" />.</returns>
	[DefaultValue("")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public unsafe string Password
	{
		get
		{
			using SecureString s = SecurePassword;
			nint num = Marshal.SecureStringToBSTR(s);
			try
			{
				return new string((char*)num);
			}
			finally
			{
				Marshal.ZeroFreeBSTR(num);
			}
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			using SecureString secureString = new SecureString();
			for (int i = 0; i < value.Length; i++)
			{
				secureString.AppendChar(value[i]);
			}
			SetSecurePassword(secureString);
		}
	}

	/// <summary>Gets the password currently held by the <see cref="T:System.Windows.Controls.PasswordBox" /> as a <see cref="T:System.Security.SecureString" />.</summary>
	/// <returns>A secure string representing the password currently held by the <see cref="T:System.Windows.Controls.PasswordBox" />.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public SecureString SecurePassword => TextContainer.GetPasswordCopy();

	/// <summary>Gets or sets the masking character for the <see cref="T:System.Windows.Controls.PasswordBox" />.  </summary>
	/// <returns>A masking character to echo when the user enters text into the <see cref="T:System.Windows.Controls.PasswordBox" />.The default value is a bullet character (●).</returns>
	public char PasswordChar
	{
		get
		{
			return (char)GetValue(PasswordCharProperty);
		}
		set
		{
			SetValue(PasswordCharProperty, value);
		}
	}

	/// <summary>Gets or sets the maximum length for passwords to be handled by this <see cref="T:System.Windows.Controls.PasswordBox" />.  </summary>
	/// <returns>An integer specifying the maximum length, in characters, for passwords to be handled by this <see cref="T:System.Windows.Controls.PasswordBox" />.A value of zero (0) means no limit.The default value is 0 (no length limit).</returns>
	[DefaultValue(0)]
	public int MaxLength
	{
		get
		{
			return (int)GetValue(MaxLengthProperty);
		}
		set
		{
			SetValue(MaxLengthProperty, value);
		}
	}

	/// <summary>Gets or sets the brush that highlights selected text.</summary>
	/// <returns>A brush that highlights selected text.</returns>
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

	/// <summary>Gets or sets the opacity of the <see cref="P:System.Windows.Controls.PasswordBox.SelectionBrush" />.</summary>
	/// <returns>The opacity of the <see cref="P:System.Windows.Controls.PasswordBox.SelectionBrush" />. The default is 0.4.</returns>
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

	/// <summary>Gets or sets the brush that specifies the color of the password box's caret.</summary>
	/// <returns>A brush that describes the color of the password box's caret.</returns>
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

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.PasswordBox" /> has focus and selected text.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.PasswordBox" /> has focus and selected text; otherwise, false.The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public bool IsSelectionActive => (bool)GetValue(IsSelectionActiveProperty);

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.PasswordBox" /> displays selected text when the <see cref="T:System.Windows.Controls.PasswordBox" /> does not have focus.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.PasswordBox" /> displays selected text when the <see cref="T:System.Windows.Controls.PasswordBox" /> does not have focus; otherwise, false.The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
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

	internal PasswordTextContainer TextContainer => _textContainer;

	internal FrameworkElement RenderScope => _renderScope;

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

	ITextContainer ITextBoxViewHost.TextContainer => TextContainer;

	bool ITextBoxViewHost.IsTypographyDefaultValue => true;

	private ITextSelection Selection => _textEditor.Selection;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.Controls.PasswordBox.Password" /> property changes.</summary>
	public event RoutedEventHandler PasswordChanged
	{
		add
		{
			AddHandler(PasswordChangedEvent, value);
		}
		remove
		{
			RemoveHandler(PasswordChangedEvent, value);
		}
	}

	static PasswordBox()
	{
		PasswordCharProperty = DependencyProperty.RegisterAttached("PasswordChar", typeof(char), typeof(PasswordBox), new FrameworkPropertyMetadata('*'));
		MaxLengthProperty = TextBox.MaxLengthProperty.AddOwner(typeof(PasswordBox));
		SelectionBrushProperty = TextBoxBase.SelectionBrushProperty.AddOwner(typeof(PasswordBox));
		SelectionTextBrushProperty = TextBoxBase.SelectionTextBrushProperty.AddOwner(typeof(PasswordBox));
		SelectionOpacityProperty = TextBoxBase.SelectionOpacityProperty.AddOwner(typeof(PasswordBox));
		CaretBrushProperty = TextBoxBase.CaretBrushProperty.AddOwner(typeof(PasswordBox));
		IsSelectionActiveProperty = TextBoxBase.IsSelectionActiveProperty.AddOwner(typeof(PasswordBox));
		IsInactiveSelectionHighlightEnabledProperty = TextBoxBase.IsInactiveSelectionHighlightEnabledProperty.AddOwner(typeof(PasswordBox));
		PasswordChangedEvent = EventManager.RegisterRoutedEvent("PasswordChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PasswordBox));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(PasswordBox), new FrameworkPropertyMetadata(typeof(PasswordBox)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(PasswordBox));
		PasswordCharProperty.OverrideMetadata(typeof(PasswordBox), new FrameworkPropertyMetadata(OnPasswordCharChanged));
		Control.PaddingProperty.OverrideMetadata(typeof(PasswordBox), new FrameworkPropertyMetadata(OnPaddingChanged));
		NavigationService.NavigationServiceProperty.OverrideMetadata(typeof(PasswordBox), new FrameworkPropertyMetadata(OnParentNavigationServiceChanged));
		InputMethod.IsInputMethodEnabledProperty.OverrideMetadata(typeof(PasswordBox), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits, null, ForceToFalse));
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(PasswordBox), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		UIElement.IsMouseOverPropertyKey.OverrideMetadata(typeof(PasswordBox), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		TextBoxBase.SelectionBrushProperty.OverrideMetadata(typeof(PasswordBox), new FrameworkPropertyMetadata(UpdateCaretElement));
		TextBoxBase.SelectionTextBrushProperty.OverrideMetadata(typeof(PasswordBox), new FrameworkPropertyMetadata(UpdateCaretElement));
		TextBoxBase.SelectionOpacityProperty.OverrideMetadata(typeof(PasswordBox), new FrameworkPropertyMetadata(UpdateCaretElement));
		TextBoxBase.CaretBrushProperty.OverrideMetadata(typeof(PasswordBox), new FrameworkPropertyMetadata(UpdateCaretElement));
		ControlsTraceLogger.AddControl(TelemetryControls.PasswordBox);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.PasswordBox" /> class.</summary>
	public PasswordBox()
	{
		Initialize();
	}

	/// <summary>Replaces the current selection in the <see cref="T:System.Windows.Controls.PasswordBox" /> with the contents of the Clipboard.</summary>
	public void Paste()
	{
		ApplicationCommands.Paste.Execute(null, this);
	}

	/// <summary>Selects the entire contents of the <see cref="T:System.Windows.Controls.PasswordBox" />.</summary>
	public void SelectAll()
	{
		Selection.Select(TextContainer.Start, TextContainer.End);
	}

	/// <summary>Clears the value of the <see cref="P:System.Windows.Controls.PasswordBox.Password" /> property.</summary>
	public void Clear()
	{
		Password = string.Empty;
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (!base.IsEnabled)
		{
			VisualStates.GoToState(this, useTransitions, "Disabled", "Normal");
		}
		else if (base.IsMouseOver)
		{
			VisualStates.GoToState(this, useTransitions, "MouseOver", "Normal");
		}
		else
		{
			VisualStateManager.GoToState(this, "Normal", useTransitions);
		}
		if (base.IsKeyboardFocused)
		{
			VisualStates.GoToState(this, useTransitions, "Focused", "Unfocused");
		}
		else
		{
			VisualStateManager.GoToState(this, "Unfocused", useTransitions);
		}
		base.ChangeVisualState(useTransitions);
	}

	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new PasswordBoxAutomationPeer(this);
	}

	/// <summary>Called when an internal process or application calls <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />, which is used to build the current template's visual tree. </summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		AttachToVisualTree();
	}

	protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
	{
		base.OnTemplateChanged(oldTemplate, newTemplate);
		if (oldTemplate != null && newTemplate != null && oldTemplate.VisualTree != newTemplate.VisualTree)
		{
			DetachFromVisualTree();
		}
	}

	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (RenderScope != null && e.Property.GetMetadata(typeof(PasswordBox)) is FrameworkPropertyMetadata frameworkPropertyMetadata && (e.IsAValueChange || e.IsASubPropertyChange))
		{
			if (frameworkPropertyMetadata.AffectsMeasure || frameworkPropertyMetadata.AffectsArrange || frameworkPropertyMetadata.AffectsParentMeasure || frameworkPropertyMetadata.AffectsParentArrange || e.Property == Control.HorizontalContentAlignmentProperty || e.Property == Control.VerticalContentAlignmentProperty)
			{
				((TextBoxView)RenderScope).Remeasure();
			}
			else if (frameworkPropertyMetadata.AffectsRender && (e.IsAValueChange || !frameworkPropertyMetadata.SubPropertiesDoNotAffectRender))
			{
				((TextBoxView)RenderScope).Rerender();
			}
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (!e.Handled)
		{
			_textEditor.OnKeyDown(e);
		}
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		base.OnKeyUp(e);
		if (!e.Handled)
		{
			_textEditor.OnKeyUp(e);
		}
	}

	protected override void OnTextInput(TextCompositionEventArgs e)
	{
		base.OnTextInput(e);
		if (!e.Handled)
		{
			_textEditor.OnTextInput(e);
		}
	}

	protected override void OnMouseDown(MouseButtonEventArgs e)
	{
		base.OnMouseDown(e);
		if (!e.Handled)
		{
			_textEditor.OnMouseDown(e);
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (!e.Handled)
		{
			_textEditor.OnMouseMove(e);
		}
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);
		if (!e.Handled)
		{
			_textEditor.OnMouseUp(e);
		}
	}

	protected override void OnQueryCursor(QueryCursorEventArgs e)
	{
		base.OnQueryCursor(e);
		if (!e.Handled)
		{
			_textEditor.OnQueryCursor(e);
		}
	}

	protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e)
	{
		base.OnQueryContinueDrag(e);
		if (!e.Handled)
		{
			_textEditor.OnQueryContinueDrag(e);
		}
	}

	protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
	{
		base.OnGiveFeedback(e);
		if (!e.Handled)
		{
			_textEditor.OnGiveFeedback(e);
		}
	}

	protected override void OnDragEnter(DragEventArgs e)
	{
		base.OnDragEnter(e);
		if (!e.Handled)
		{
			_textEditor.OnDragEnter(e);
		}
	}

	protected override void OnDragOver(DragEventArgs e)
	{
		base.OnDragOver(e);
		if (!e.Handled)
		{
			_textEditor.OnDragOver(e);
		}
	}

	protected override void OnDragLeave(DragEventArgs e)
	{
		base.OnDragLeave(e);
		if (!e.Handled)
		{
			_textEditor.OnDragLeave(e);
		}
	}

	protected override void OnDrop(DragEventArgs e)
	{
		base.OnDrop(e);
		if (!e.Handled)
		{
			_textEditor.OnDrop(e);
		}
	}

	protected override void OnContextMenuOpening(ContextMenuEventArgs e)
	{
		base.OnContextMenuOpening(e);
		if (!e.Handled)
		{
			_textEditor.OnContextMenuOpening(e);
		}
	}

	protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		base.OnGotKeyboardFocus(e);
		if (!e.Handled)
		{
			_textEditor.OnGotKeyboardFocus(e);
		}
	}

	protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		base.OnLostKeyboardFocus(e);
		if (!e.Handled)
		{
			_textEditor.OnLostKeyboardFocus(e);
		}
	}

	protected override void OnLostFocus(RoutedEventArgs e)
	{
		base.OnLostFocus(e);
		if (!e.Handled)
		{
			_textEditor.OnLostFocus(e);
		}
	}

	private void Initialize()
	{
		TextEditor.RegisterCommandHandlers(typeof(PasswordBox), acceptsRichContent: false, readOnly: false, registerEventListeners: false);
		InitializeTextContainer(new PasswordTextContainer(this));
		_textEditor.AcceptsRichContent = false;
		_textEditor.AcceptsTab = false;
	}

	private void InitializeTextContainer(PasswordTextContainer textContainer)
	{
		Invariant.Assert(textContainer != null);
		if (_textContainer != null)
		{
			Invariant.Assert(_textEditor != null);
			Invariant.Assert(_textEditor.TextContainer == _textContainer);
			DetachFromVisualTree();
			_textEditor.OnDetach();
		}
		_textContainer = textContainer;
		((ITextContainer)_textContainer).Changed += OnTextContainerChanged;
		_textEditor = new TextEditor(_textContainer, this, isUndoEnabled: true);
	}

	private static object ForceToFalse(DependencyObject d, object value)
	{
		return BooleanBoxes.FalseBox;
	}

	private static void OnPasswordCharChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		PasswordBox passwordBox = (PasswordBox)d;
		if (passwordBox._renderScope != null)
		{
			passwordBox._renderScope.InvalidateMeasure();
		}
	}

	private void OnTextContainerChanged(object sender, TextContainerChangedEventArgs e)
	{
		if (e.HasContentAddedOrRemoved)
		{
			RaiseEvent(new RoutedEventArgs(PasswordChangedEvent));
		}
	}

	private void SetRenderScopeToContentHost(TextBoxView renderScope)
	{
		ClearContentHost();
		_passwordBoxContentHost = GetTemplateChild("PART_ContentHost") as FrameworkElement;
		_renderScope = renderScope;
		if (_passwordBoxContentHost is ScrollViewer)
		{
			ScrollViewer scrollViewer = (ScrollViewer)_passwordBoxContentHost;
			if (scrollViewer.Content != null)
			{
				throw new NotSupportedException(SR.TextBoxScrollViewerMarkedAsTextBoxContentMustHaveNoContent);
			}
			scrollViewer.Content = _renderScope;
		}
		else if (_passwordBoxContentHost is Decorator)
		{
			Decorator decorator = (Decorator)_passwordBoxContentHost;
			if (decorator.Child != null)
			{
				throw new NotSupportedException(SR.TextBoxDecoratorMarkedAsTextBoxContentMustHaveNoContent);
			}
			decorator.Child = _renderScope;
		}
		else
		{
			_renderScope = null;
			if (_passwordBoxContentHost != null)
			{
				_passwordBoxContentHost = null;
				throw new NotSupportedException(SR.PasswordBoxInvalidTextContainer);
			}
		}
		InitializeRenderScope();
		FrameworkElement frameworkElement = _renderScope;
		while (frameworkElement != this && frameworkElement != null)
		{
			if (frameworkElement is Border)
			{
				_border = (Border)frameworkElement;
			}
			frameworkElement = frameworkElement.Parent as FrameworkElement;
		}
	}

	private void ClearContentHost()
	{
		UninitializeRenderScope();
		if (_passwordBoxContentHost is ScrollViewer)
		{
			((ScrollViewer)_passwordBoxContentHost).Content = null;
		}
		else if (_passwordBoxContentHost is Decorator)
		{
			((Decorator)_passwordBoxContentHost).Child = null;
		}
		else
		{
			Invariant.Assert(_passwordBoxContentHost == null, "_passwordBoxContentHost must be null here");
		}
		_passwordBoxContentHost = null;
	}

	private void InitializeRenderScope()
	{
		if (_renderScope != null)
		{
			ITextView textView = TextEditor.GetTextView(_renderScope);
			_textEditor.TextView = textView;
			TextContainer.TextView = textView;
			if (ScrollViewer != null)
			{
				ScrollViewer.CanContentScroll = true;
			}
		}
	}

	private void UninitializeRenderScope()
	{
		_textEditor.TextView = null;
	}

	private void ResetSelection()
	{
		Select(0, 0);
		if (ScrollViewer != null)
		{
			ScrollViewer.ScrollToHome();
		}
	}

	private void Select(int start, int length)
	{
		if (start < 0)
		{
			throw new ArgumentOutOfRangeException("start", SR.ParameterCannotBeNegative);
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", SR.ParameterCannotBeNegative);
		}
		ITextPointer textPointer = TextContainer.Start.CreatePointer();
		while (start-- > 0 && textPointer.MoveToNextInsertionPosition(LogicalDirection.Forward))
		{
		}
		ITextPointer textPointer2 = textPointer.CreatePointer();
		while (length-- > 0 && textPointer2.MoveToNextInsertionPosition(LogicalDirection.Forward))
		{
		}
		Selection.Select(textPointer, textPointer2);
	}

	private static void OnPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		PasswordBox passwordBox = (PasswordBox)d;
		if (passwordBox.ScrollViewer != null)
		{
			object value = passwordBox.GetValue(Control.PaddingProperty);
			if (value is Thickness)
			{
				passwordBox.ScrollViewer.Padding = (Thickness)value;
			}
			else
			{
				passwordBox.ScrollViewer.ClearValue(Control.PaddingProperty);
			}
		}
	}

	private static void OnParentNavigationServiceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
	{
		PasswordBox passwordBox = (PasswordBox)o;
		NavigationService navigationService = NavigationService.GetNavigationService(o);
		if (passwordBox._navigationService != null)
		{
			passwordBox._navigationService.Navigating -= passwordBox.OnNavigating;
		}
		if (navigationService != null)
		{
			navigationService.Navigating += passwordBox.OnNavigating;
			passwordBox._navigationService = navigationService;
		}
		else
		{
			passwordBox._navigationService = null;
		}
	}

	private void OnNavigating(object sender, NavigatingCancelEventArgs e)
	{
		Password = string.Empty;
	}

	private void AttachToVisualTree()
	{
		DetachFromVisualTree();
		SetRenderScopeToContentHost(new TextBoxView(this));
		if (ScrollViewer != null)
		{
			ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
			ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
			ScrollViewer.Focusable = false;
			if (ScrollViewer.Background == null)
			{
				ScrollViewer.Background = Brushes.Transparent;
			}
			OnPaddingChanged(this, default(DependencyPropertyChangedEventArgs));
		}
		if (_border != null)
		{
			_border.Style = null;
		}
	}

	private void DetachFromVisualTree()
	{
		if (_textEditor != null)
		{
			_textEditor.Selection.DetachFromVisualTree();
		}
		_scrollViewer = null;
		_border = null;
		ClearContentHost();
	}

	private void SetSecurePassword(SecureString value)
	{
		TextContainer.BeginChange();
		try
		{
			TextContainer.SetPassword(value);
			ResetSelection();
		}
		finally
		{
			TextContainer.EndChange();
		}
	}

	private static void UpdateCaretElement(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		PasswordBox passwordBox = (PasswordBox)d;
		if (passwordBox.Selection == null)
		{
			return;
		}
		CaretElement caretElement = passwordBox.Selection.CaretElement;
		if (caretElement != null)
		{
			if (e.Property == CaretBrushProperty)
			{
				caretElement.UpdateCaretBrush(TextSelection.GetCaretBrush(passwordBox.Selection.TextEditor));
			}
			caretElement.InvalidateVisual();
		}
		TextBoxView textBoxView = passwordBox?.RenderScope as TextBoxView;
		if (textBoxView != null && ((ITextView)textBoxView).RendersOwnSelection)
		{
			textBoxView.InvalidateArrange();
		}
	}
}
