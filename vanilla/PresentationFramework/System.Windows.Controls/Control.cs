using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;
using MS.Utility;

namespace System.Windows.Controls;

/// <summary>Represents the base class for user interface (UI) elements that use a <see cref="T:System.Windows.Controls.ControlTemplate" /> to define their appearance. </summary>
public class Control : FrameworkElement
{
	internal enum ControlBoolFlags : ushort
	{
		ContentIsNotLogical = 1,
		IsSpaceKeyDown = 2,
		HeaderIsNotLogical = 4,
		CommandDisabled = 8,
		ContentIsItem = 0x10,
		HeaderIsItem = 0x20,
		ScrollHostValid = 0x40,
		ContainsSelection = 0x80,
		VisualStateChangeSuspended = 0x100
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Control.BorderBrush" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Control.BorderBrush" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty BorderBrushProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Control.BorderThickness" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Control.BorderThickness" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty BorderThicknessProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Control.Background" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Control.Background" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty BackgroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Control.Foreground" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Control.Foreground" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ForegroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Control.FontFamily" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Control.FontFamily" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FontFamilyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Control.FontSize" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Control.FontSize" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FontSizeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Control.FontStretch" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Control.FontStretch" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FontStretchProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Control.FontStyle" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Control.FontStyle" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FontStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Control.FontWeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Control.FontWeight" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FontWeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Control.HorizontalContentAlignment" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Control.HorizontalContentAlignment" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty HorizontalContentAlignmentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Control.VerticalContentAlignment" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Control.VerticalContentAlignment" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty VerticalContentAlignmentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Control.TabIndex" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Control.TabIndex" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty TabIndexProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Control.IsTabStop" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Control.IsTabStop" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty IsTabStopProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Control.Padding" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Control.Padding" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty PaddingProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Control.Template" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Control.Template" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty TemplateProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Control.PreviewMouseDoubleClick" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Control.PreviewMouseDoubleClick" /> routed event.</returns>
	public static readonly RoutedEvent PreviewMouseDoubleClickEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Control.MouseDoubleClick" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Control.MouseDoubleClick" /> routed event.</returns>
	public static readonly RoutedEvent MouseDoubleClickEvent;

	private ControlTemplate _templateCache;

	internal ControlBoolFlags _controlBoolField;

	/// <summary>Gets or sets a brush that describes the border background of a control. </summary>
	/// <returns>The brush that is used to fill the control's border; the default is <see cref="P:System.Windows.Media.Brushes.Transparent" />.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public Brush BorderBrush
	{
		get
		{
			return (Brush)GetValue(BorderBrushProperty);
		}
		set
		{
			SetValue(BorderBrushProperty, value);
		}
	}

	/// <summary>Gets or sets the border thickness of a control.  </summary>
	/// <returns>A thickness value; the default is a thickness of 0 on all four sides.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public Thickness BorderThickness
	{
		get
		{
			return (Thickness)GetValue(BorderThicknessProperty);
		}
		set
		{
			SetValue(BorderThicknessProperty, value);
		}
	}

	/// <summary>Gets or sets a brush that describes the background of a control. </summary>
	/// <returns>The brush that is used to fill the background of the control. The default is <see cref="P:System.Windows.Media.Brushes.Transparent" />. </returns>
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

	/// <summary>Gets or sets a brush that describes the foreground color.    </summary>
	/// <returns>The brush that paints the foreground of the control. The default value is the system dialog font color.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public Brush Foreground
	{
		get
		{
			return (Brush)GetValue(ForegroundProperty);
		}
		set
		{
			SetValue(ForegroundProperty, value);
		}
	}

	/// <summary>Gets or sets the font family of the control.   </summary>
	/// <returns>A font family. The default is the system dialog font.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	[Localizability(LocalizationCategory.Font)]
	public FontFamily FontFamily
	{
		get
		{
			return (FontFamily)GetValue(FontFamilyProperty);
		}
		set
		{
			SetValue(FontFamilyProperty, value);
		}
	}

	/// <summary>Gets or sets the font size.   </summary>
	/// <returns>The size of the text in the <see cref="T:System.Windows.Controls.Control" />. The default is <see cref="P:System.Windows.SystemFonts.MessageFontSize" />. The font size must be a positive number.</returns>
	[TypeConverter(typeof(FontSizeConverter))]
	[Bindable(true)]
	[Category("Appearance")]
	[Localizability(LocalizationCategory.None)]
	public double FontSize
	{
		get
		{
			return (double)GetValue(FontSizeProperty);
		}
		set
		{
			SetValue(FontSizeProperty, value);
		}
	}

	/// <summary>Gets or sets the degree to which a font is condensed or expanded on the screen.   </summary>
	/// <returns>A <see cref="T:System.Windows.FontStretch" /> value. The default is <see cref="P:System.Windows.FontStretches.Normal" />.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public FontStretch FontStretch
	{
		get
		{
			return (FontStretch)GetValue(FontStretchProperty);
		}
		set
		{
			SetValue(FontStretchProperty, value);
		}
	}

	/// <summary>Gets or sets the font style.   </summary>
	/// <returns>A <see cref="T:System.Windows.FontStyle" /> value. The default is <see cref="P:System.Windows.FontStyles.Normal" />.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public FontStyle FontStyle
	{
		get
		{
			return (FontStyle)GetValue(FontStyleProperty);
		}
		set
		{
			SetValue(FontStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the weight or thickness of the specified font.   </summary>
	/// <returns>A <see cref="T:System.Windows.FontWeight" /> value. The default is <see cref="P:System.Windows.FontWeights.Normal" />.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public FontWeight FontWeight
	{
		get
		{
			return (FontWeight)GetValue(FontWeightProperty);
		}
		set
		{
			SetValue(FontWeightProperty, value);
		}
	}

	/// <summary>Gets or sets the horizontal alignment of the control's content.    </summary>
	/// <returns>One of the <see cref="T:System.Windows.HorizontalAlignment" /> values. The default is <see cref="F:System.Windows.HorizontalAlignment.Left" />.</returns>
	[Bindable(true)]
	[Category("Layout")]
	public HorizontalAlignment HorizontalContentAlignment
	{
		get
		{
			return (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty);
		}
		set
		{
			SetValue(HorizontalContentAlignmentProperty, value);
		}
	}

	/// <summary>Gets or sets the vertical alignment of the control's content.    </summary>
	/// <returns>One of the <see cref="T:System.Windows.VerticalAlignment" /> values. The default is <see cref="F:System.Windows.VerticalAlignment.Top" />.</returns>
	[Bindable(true)]
	[Category("Layout")]
	public VerticalAlignment VerticalContentAlignment
	{
		get
		{
			return (VerticalAlignment)GetValue(VerticalContentAlignmentProperty);
		}
		set
		{
			SetValue(VerticalContentAlignmentProperty, value);
		}
	}

	/// <summary>Gets or sets a value that determines the order in which elements receive focus when the user navigates through controls by using the TAB key.   </summary>
	/// <returns>A value that determines the order of logical navigation for a device. The default value is <see cref="F:System.Int32.MaxValue" />.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public int TabIndex
	{
		get
		{
			return (int)GetValue(TabIndexProperty);
		}
		set
		{
			SetValue(TabIndexProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a control is included in tab navigation.   </summary>
	/// <returns>true if the control is included in tab navigation; otherwise, false. The default is true.</returns>
	[Bindable(true)]
	[Category("Behavior")]
	public bool IsTabStop
	{
		get
		{
			return (bool)GetValue(IsTabStopProperty);
		}
		set
		{
			SetValue(IsTabStopProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets the padding inside a control.   </summary>
	/// <returns>The amount of space between the content of a <see cref="T:System.Windows.Controls.Control" /> and its <see cref="P:System.Windows.FrameworkElement.Margin" /> or <see cref="T:System.Windows.Controls.Border" />.  The default is a thickness of 0 on all four sides.</returns>
	[Bindable(true)]
	[Category("Layout")]
	public Thickness Padding
	{
		get
		{
			return (Thickness)GetValue(PaddingProperty);
		}
		set
		{
			SetValue(PaddingProperty, value);
		}
	}

	/// <summary>Gets or sets a control template.   </summary>
	/// <returns>The template that defines the appearance of the <see cref="T:System.Windows.Controls.Control" />.</returns>
	public ControlTemplate Template
	{
		get
		{
			return _templateCache;
		}
		set
		{
			SetValue(TemplateProperty, value);
		}
	}

	internal override FrameworkTemplate TemplateInternal => Template;

	internal override FrameworkTemplate TemplateCache
	{
		get
		{
			return _templateCache;
		}
		set
		{
			_templateCache = (ControlTemplate)value;
		}
	}

	/// <summary>Gets a value that indicates whether a control supports scrolling.</summary>
	/// <returns>true if the control has a <see cref="T:System.Windows.Controls.ScrollViewer" /> in its style and has a custom keyboard scrolling behavior; otherwise, false.</returns>
	protected internal virtual bool HandlesScrolling => false;

	internal bool VisualStateChangeSuspended
	{
		get
		{
			return ReadControlFlag(ControlBoolFlags.VisualStateChangeSuspended);
		}
		set
		{
			WriteControlFlag(ControlBoolFlags.VisualStateChangeSuspended, value);
		}
	}

	/// <summary>Occurs when a user clicks the mouse button two or more times. </summary>
	public event MouseButtonEventHandler PreviewMouseDoubleClick
	{
		add
		{
			AddHandler(PreviewMouseDoubleClickEvent, value);
		}
		remove
		{
			RemoveHandler(PreviewMouseDoubleClickEvent, value);
		}
	}

	/// <summary>Occurs when a mouse button is clicked two or more times. </summary>
	public event MouseButtonEventHandler MouseDoubleClick
	{
		add
		{
			AddHandler(MouseDoubleClickEvent, value);
		}
		remove
		{
			RemoveHandler(MouseDoubleClickEvent, value);
		}
	}

	static Control()
	{
		BorderBrushProperty = Border.BorderBrushProperty.AddOwner(typeof(Control), new FrameworkPropertyMetadata(Border.BorderBrushProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.None));
		BorderThicknessProperty = Border.BorderThicknessProperty.AddOwner(typeof(Control), new FrameworkPropertyMetadata(Border.BorderThicknessProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.None));
		BackgroundProperty = Panel.BackgroundProperty.AddOwner(typeof(Control), new FrameworkPropertyMetadata(Panel.BackgroundProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.None));
		ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(Control), new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.Inherits));
		FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(Control), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, FrameworkPropertyMetadataOptions.Inherits));
		FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(Control), new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, FrameworkPropertyMetadataOptions.Inherits));
		FontStretchProperty = TextElement.FontStretchProperty.AddOwner(typeof(Control), new FrameworkPropertyMetadata(TextElement.FontStretchProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.Inherits));
		FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(Control), new FrameworkPropertyMetadata(SystemFonts.MessageFontStyle, FrameworkPropertyMetadataOptions.Inherits));
		FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(Control), new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight, FrameworkPropertyMetadataOptions.Inherits));
		HorizontalContentAlignmentProperty = DependencyProperty.Register("HorizontalContentAlignment", typeof(HorizontalAlignment), typeof(Control), new FrameworkPropertyMetadata(HorizontalAlignment.Left), FrameworkElement.ValidateHorizontalAlignmentValue);
		VerticalContentAlignmentProperty = DependencyProperty.Register("VerticalContentAlignment", typeof(VerticalAlignment), typeof(Control), new FrameworkPropertyMetadata(VerticalAlignment.Top), FrameworkElement.ValidateVerticalAlignmentValue);
		TabIndexProperty = KeyboardNavigation.TabIndexProperty.AddOwner(typeof(Control));
		IsTabStopProperty = KeyboardNavigation.IsTabStopProperty.AddOwner(typeof(Control));
		PaddingProperty = DependencyProperty.Register("Padding", typeof(Thickness), typeof(Control), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsParentMeasure));
		TemplateProperty = DependencyProperty.Register("Template", typeof(ControlTemplate), typeof(Control), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnTemplateChanged));
		PreviewMouseDoubleClickEvent = EventManager.RegisterRoutedEvent("PreviewMouseDoubleClick", RoutingStrategy.Direct, typeof(MouseButtonEventHandler), typeof(Control));
		MouseDoubleClickEvent = EventManager.RegisterRoutedEvent("MouseDoubleClick", RoutingStrategy.Direct, typeof(MouseButtonEventHandler), typeof(Control));
		UIElement.FocusableProperty.OverrideMetadata(typeof(Control), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		EventManager.RegisterClassHandler(typeof(Control), UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(HandleDoubleClick), handledEventsToo: true);
		EventManager.RegisterClassHandler(typeof(Control), UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(HandleDoubleClick), handledEventsToo: true);
		EventManager.RegisterClassHandler(typeof(Control), UIElement.PreviewMouseRightButtonDownEvent, new MouseButtonEventHandler(HandleDoubleClick), handledEventsToo: true);
		EventManager.RegisterClassHandler(typeof(Control), UIElement.MouseRightButtonDownEvent, new MouseButtonEventHandler(HandleDoubleClick), handledEventsToo: true);
		UIElement.IsKeyboardFocusedPropertyKey.OverrideMetadata(typeof(Control), new PropertyMetadata(OnVisualStatePropertyChanged));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Control" /> class. </summary>
	public Control()
	{
		PropertyMetadata metadata = TemplateProperty.GetMetadata(base.DependencyObjectType);
		ControlTemplate controlTemplate = (ControlTemplate)metadata.DefaultValue;
		if (controlTemplate != null)
		{
			OnTemplateChanged(this, new DependencyPropertyChangedEventArgs(TemplateProperty, metadata, null, controlTemplate));
		}
	}

	private static bool IsMarginValid(object value)
	{
		Thickness thickness = (Thickness)value;
		if (thickness.Left >= 0.0 && thickness.Right >= 0.0 && thickness.Top >= 0.0)
		{
			return thickness.Bottom >= 0.0;
		}
		return false;
	}

	internal override void OnTemplateChangedInternal(FrameworkTemplate oldTemplate, FrameworkTemplate newTemplate)
	{
		OnTemplateChanged((ControlTemplate)oldTemplate, (ControlTemplate)newTemplate);
	}

	private static void OnTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		StyleHelper.UpdateTemplateCache((Control)d, (FrameworkTemplate)e.OldValue, (FrameworkTemplate)e.NewValue, TemplateProperty);
	}

	/// <summary>Called whenever the control's template changes. </summary>
	/// <param name="oldTemplate">The old template.</param>
	/// <param name="newTemplate">The new template.</param>
	protected virtual void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
	{
	}

	/// <summary>Returns the string representation of a <see cref="T:System.Windows.Controls.Control" /> object. </summary>
	/// <returns>A string that represents the control.</returns>
	public override string ToString()
	{
		string text = null;
		text = ((!CheckAccess()) ? ((string)base.Dispatcher.Invoke(DispatcherPriority.Send, new TimeSpan(0, 0, 0, 0, 20), (DispatcherOperationCallback)((object o) => GetPlainText()), null)) : GetPlainText());
		if (!string.IsNullOrEmpty(text))
		{
			return SR.Format(SR.ToStringFormatString_Control, base.ToString(), text);
		}
		return base.ToString();
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.Control.PreviewMouseDoubleClick" /> routed event. </summary>
	/// <param name="e">The event data. </param>
	protected virtual void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.Control.MouseDoubleClick" /> routed event. </summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnMouseDoubleClick(MouseButtonEventArgs e)
	{
		RaiseEvent(e);
	}

	private static void HandleDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (e.ClickCount == 2)
		{
			Control control = (Control)sender;
			MouseButtonEventArgs mouseButtonEventArgs = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton, e.StylusDevice);
			if (e.RoutedEvent == UIElement.PreviewMouseLeftButtonDownEvent || e.RoutedEvent == UIElement.PreviewMouseRightButtonDownEvent)
			{
				mouseButtonEventArgs.RoutedEvent = PreviewMouseDoubleClickEvent;
				mouseButtonEventArgs.Source = e.OriginalSource;
				mouseButtonEventArgs.OverrideSource(e.Source);
				control.OnPreviewMouseDoubleClick(mouseButtonEventArgs);
			}
			else
			{
				mouseButtonEventArgs.RoutedEvent = MouseDoubleClickEvent;
				mouseButtonEventArgs.Source = e.OriginalSource;
				mouseButtonEventArgs.OverrideSource(e.Source);
				control.OnMouseDoubleClick(mouseButtonEventArgs);
			}
			if (mouseButtonEventArgs.Handled)
			{
				e.Handled = true;
			}
		}
	}

	internal override void OnPreApplyTemplate()
	{
		VisualStateChangeSuspended = true;
		base.OnPreApplyTemplate();
	}

	internal override void OnPostApplyTemplate()
	{
		base.OnPostApplyTemplate();
		VisualStateChangeSuspended = false;
		UpdateVisualState(useTransitions: false);
	}

	internal void UpdateVisualState()
	{
		UpdateVisualState(useTransitions: true);
	}

	internal void UpdateVisualState(bool useTransitions)
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordGeneral | EventTrace.Keyword.KeywordPerf, EventTrace.Level.Info, EventTrace.Event.UpdateVisualStateStart);
		if (!VisualStateChangeSuspended)
		{
			ChangeVisualState(useTransitions);
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordGeneral | EventTrace.Keyword.KeywordPerf, EventTrace.Level.Info, EventTrace.Event.UpdateVisualStateEnd);
	}

	internal virtual void ChangeVisualState(bool useTransitions)
	{
		ChangeValidationVisualState(useTransitions);
	}

	internal void ChangeValidationVisualState(bool useTransitions)
	{
		if (Validation.GetHasError(this))
		{
			if (base.IsKeyboardFocused)
			{
				VisualStateManager.GoToState(this, "InvalidFocused", useTransitions);
			}
			else
			{
				VisualStateManager.GoToState(this, "InvalidUnfocused", useTransitions);
			}
		}
		else
		{
			VisualStateManager.GoToState(this, "Valid", useTransitions);
		}
	}

	internal static void OnVisualStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is Control control)
		{
			control.UpdateVisualState();
		}
	}

	/// <summary>Called to remeasure a control. </summary>
	/// <returns>The size of the control, up to the maximum specified by <paramref name="constraint" />.</returns>
	/// <param name="constraint">The maximum size that the method can return.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		if (VisualChildrenCount > 0)
		{
			UIElement uIElement = (UIElement)GetVisualChild(0);
			if (uIElement != null)
			{
				uIElement.Measure(constraint);
				return uIElement.DesiredSize;
			}
		}
		return new Size(0.0, 0.0);
	}

	/// <summary>Called to arrange and size the content of a <see cref="T:System.Windows.Controls.Control" /> object. </summary>
	/// <returns>The size of the control.</returns>
	/// <param name="arrangeBounds">The computed size that is used to arrange the content.</param>
	protected override Size ArrangeOverride(Size arrangeBounds)
	{
		if (VisualChildrenCount > 0)
		{
			((UIElement)GetVisualChild(0))?.Arrange(new Rect(arrangeBounds));
		}
		return arrangeBounds;
	}

	internal bool ReadControlFlag(ControlBoolFlags reqFlag)
	{
		return (_controlBoolField & reqFlag) != 0;
	}

	internal void WriteControlFlag(ControlBoolFlags reqFlag, bool set)
	{
		if (set)
		{
			_controlBoolField |= reqFlag;
		}
		else
		{
			_controlBoolField &= (ControlBoolFlags)(ushort)(~(int)reqFlag);
		}
	}
}
