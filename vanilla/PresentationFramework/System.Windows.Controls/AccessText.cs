using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Specifies with an underscore the character that is used as the access key.</summary>
[ContentProperty("Text")]
public class AccessText : FrameworkElement, IAddChild
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.Text" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.Text" /> dependency property.</returns>
	public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(AccessText), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnTextChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.FontFamily" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.FontFamily" /> dependency property.</returns>
	public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(AccessText));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.FontStyle" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.FontStyle" /> dependency property.</returns>
	public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(AccessText));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.FontWeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.FontWeight" /> dependency property.</returns>
	public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(AccessText));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.FontStretch" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.FontStretch" /> dependency property.</returns>
	public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(typeof(AccessText));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.FontSize" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.FontSize" /> dependency property.</returns>
	public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(AccessText));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.Foreground" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.Foreground" /> dependency property.</returns>
	public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(AccessText));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.Background" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.Background" /> dependency property.</returns>
	public static readonly DependencyProperty BackgroundProperty = TextElement.BackgroundProperty.AddOwner(typeof(AccessText), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.TextDecorations" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.TextDecorations" /> dependency property.</returns>
	public static readonly DependencyProperty TextDecorationsProperty = Inline.TextDecorationsProperty.AddOwner(typeof(AccessText), new FrameworkPropertyMetadata(new FreezableDefaultValueFactory(TextDecorationCollection.Empty), FrameworkPropertyMetadataOptions.AffectsRender, OnPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.TextEffects" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.TextEffects" /> dependency property.</returns>
	public static readonly DependencyProperty TextEffectsProperty = TextElement.TextEffectsProperty.AddOwner(typeof(AccessText), new FrameworkPropertyMetadata(new FreezableDefaultValueFactory(TextEffectCollection.Empty), FrameworkPropertyMetadataOptions.AffectsRender, OnPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.LineHeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.LineHeight" /> dependency property.</returns>
	public static readonly DependencyProperty LineHeightProperty = Block.LineHeightProperty.AddOwner(typeof(AccessText));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.LineStackingStrategy" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.LineStackingStrategy" /> dependency property.</returns>
	public static readonly DependencyProperty LineStackingStrategyProperty = Block.LineStackingStrategyProperty.AddOwner(typeof(AccessText));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.TextAlignment" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.TextAlignment" /> dependency property.</returns>
	public static readonly DependencyProperty TextAlignmentProperty = Block.TextAlignmentProperty.AddOwner(typeof(AccessText));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.TextTrimming" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.TextTrimming" /> dependency property.</returns>
	public static readonly DependencyProperty TextTrimmingProperty = TextBlock.TextTrimmingProperty.AddOwner(typeof(AccessText), new FrameworkPropertyMetadata(TextTrimming.None, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.TextWrapping" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.TextWrapping" /> dependency property.</returns>
	public static readonly DependencyProperty TextWrappingProperty = TextBlock.TextWrappingProperty.AddOwner(typeof(AccessText), new FrameworkPropertyMetadata(TextWrapping.NoWrap, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.AccessText.BaselineOffset" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.AccessText.BaselineOffset" /> dependency property.</returns>
	public static readonly DependencyProperty BaselineOffsetProperty = TextBlock.BaselineOffsetProperty.AddOwner(typeof(AccessText), new FrameworkPropertyMetadata(OnPropertyChanged));

	private TextContainer _textContainer;

	private TextBlock _textBlock;

	private Run _accessKey;

	private bool _accessKeyLocated;

	private const char _accessKeyMarker = '_';

	private static Style _accessKeyStyle;

	private string _currentlyRegistered;

	private static readonly UncommonField<bool> HasCustomSerializationStorage = new UncommonField<bool>();

	/// <summary>Gets an enumerator that iterates the logical child elements of the <see cref="T:System.Windows.Controls.AccessText" /> element. </summary>
	/// <returns>An enumerator that iterates the logical child elements of this element.</returns>
	protected internal override IEnumerator LogicalChildren => new RangeContentEnumerator(TextContainer.Start, TextContainer.End);

	/// <summary>Provides read-only access to the character that follows the first underline character.</summary>
	/// <returns>The character to return.</returns>
	public char AccessKey
	{
		get
		{
			if (_accessKey == null || _accessKey.Text.Length <= 0)
			{
				return '\0';
			}
			return _accessKey.Text[0];
		}
	}

	/// <summary>Gets or sets the text that is displayed by the <see cref="T:System.Windows.Controls.AccessText" /> element. </summary>
	/// <returns>The text without the first underscore character. The default is an empty string.</returns>
	[DefaultValue("")]
	public string Text
	{
		get
		{
			return (string)GetValue(TextProperty);
		}
		set
		{
			SetValue(TextProperty, value);
		}
	}

	/// <summary>Gets or sets the font family to use with the <see cref="T:System.Windows.Controls.AccessText" /> element.  </summary>
	/// <returns>The font family to use. The default is the font that is determined by the <see cref="P:System.Windows.SystemFonts.MessageFontFamily" /> metric.</returns>
	[Localizability(LocalizationCategory.Font, Modifiability = Modifiability.Unmodifiable)]
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

	/// <summary>Gets or sets the font style to use with the <see cref="T:System.Windows.Controls.AccessText" /> element. </summary>
	/// <returns>The font style to use; for example, normal, italic, or oblique. The default is determined by the <see cref="P:System.Windows.SystemFonts.MessageFontStyle" /> metric.</returns>
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

	/// <summary>Gets or sets the font weight to use with the <see cref="T:System.Windows.Controls.AccessText" /> element. </summary>
	/// <returns>The font weight to use. The default is determined by the <see cref="P:System.Windows.SystemFonts.MessageFontWeight" /> metric.</returns>
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

	/// <summary>Gets or sets a <see cref="T:System.Windows.FontStretch" /> property that selects a normal, condensed, or expanded font from a <see cref="T:System.Windows.Media.FontFamily" />. </summary>
	/// <returns>The relative degree that the font is stretched. The default is <see cref="P:System.Windows.FontStretches.Normal" />.</returns>
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

	/// <summary>Gets or sets the font size to use with the <see cref="T:System.Windows.Controls.AccessText" /> element.  </summary>
	/// <returns>The font size to use. The default is the font size that is determined by the <see cref="P:System.Windows.SystemFonts.MessageFontSize" /> metric.</returns>
	[TypeConverter(typeof(FontSizeConverter))]
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

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> that draws the text content of the element. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Brush" /> that draws the text. The default is <see cref="P:System.Windows.Media.Brushes.Black" />.</returns>
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

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> that fills the content area.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Brush" /> that fills the content area. The default is null.</returns>
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

	/// <summary>Gets or sets the decorations that are added to the text of an <see cref="T:System.Windows.Controls.AccessText" /> element. </summary>
	/// <returns>The <see cref="T:System.Windows.TextDecorations" /> applied to the text of an <see cref="T:System.Windows.Controls.AccessText" />. The default is null.</returns>
	public TextDecorationCollection TextDecorations
	{
		get
		{
			return (TextDecorationCollection)GetValue(TextDecorationsProperty);
		}
		set
		{
			SetValue(TextDecorationsProperty, value);
		}
	}

	/// <summary>Gets or sets the effects that are added to the text of an <see cref="T:System.Windows.Controls.AccessText" /> element. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.TextEffectCollection" />. The default is null.</returns>
	public TextEffectCollection TextEffects
	{
		get
		{
			return (TextEffectCollection)GetValue(TextEffectsProperty);
		}
		set
		{
			SetValue(TextEffectsProperty, value);
		}
	}

	/// <summary>Gets or sets the height of each line box. </summary>
	/// <returns>A double that specifies the height of each line box. This value must be equal to or greater than 0.0034 and equal to or less then 160000. A value of <see cref="F:System.Double.NaN" /> (equivalent to an attribute value of Auto) causes the line height to be determined automatically from the current font characteristics. The default is <see cref="F:System.Double.NaN" />.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double LineHeight
	{
		get
		{
			return (double)GetValue(LineHeightProperty);
		}
		set
		{
			SetValue(LineHeightProperty, value);
		}
	}

	/// <summary>Gets or sets how the <see cref="P:System.Windows.Controls.AccessText.LineHeight" /> property is enforced. </summary>
	/// <returns>A <see cref="T:System.Windows.LineStackingStrategy" /> value that determines the behavior of the <see cref="P:System.Windows.Controls.AccessText.LineHeight" /> property.</returns>
	public LineStackingStrategy LineStackingStrategy
	{
		get
		{
			return (LineStackingStrategy)GetValue(LineStackingStrategyProperty);
		}
		set
		{
			SetValue(LineStackingStrategyProperty, value);
		}
	}

	/// <summary>Gets or sets the horizontal alignment of the content.  </summary>
	/// <returns>The horizontal alignment of the text.</returns>
	public TextAlignment TextAlignment
	{
		get
		{
			return (TextAlignment)GetValue(TextAlignmentProperty);
		}
		set
		{
			SetValue(TextAlignmentProperty, value);
		}
	}

	/// <summary>Gets or sets how the textual content of an <see cref="T:System.Windows.Controls.AccessText" /> element is clipped if it overflows the line box. </summary>
	/// <returns>The trimming behavior to use. The default is <see cref="F:System.Windows.TextTrimming.None" /></returns>
	public TextTrimming TextTrimming
	{
		get
		{
			return (TextTrimming)GetValue(TextTrimmingProperty);
		}
		set
		{
			SetValue(TextTrimmingProperty, value);
		}
	}

	/// <summary>Gets or sets whether the textual content of an <see cref="T:System.Windows.Controls.AccessText" /> element is wrapped if it overflows the line box. </summary>
	/// <returns>The wrapping behavior to use. The default is <see cref="F:System.Windows.TextWrapping.NoWrap" />.</returns>
	public TextWrapping TextWrapping
	{
		get
		{
			return (TextWrapping)GetValue(TextWrappingProperty);
		}
		set
		{
			SetValue(TextWrappingProperty, value);
		}
	}

	/// <summary>Gets or sets a value that adjusts the baseline offset position of text in an <see cref="T:System.Windows.Controls.AccessText" /> element.  </summary>
	/// <returns>The amount to adjust the baseline offset position.</returns>
	public double BaselineOffset
	{
		get
		{
			return (double)GetValue(BaselineOffsetProperty);
		}
		set
		{
			SetValue(BaselineOffsetProperty, value);
		}
	}

	internal TextBlock TextBlock
	{
		get
		{
			if (_textBlock == null)
			{
				CreateTextBlock();
			}
			return _textBlock;
		}
	}

	internal static char AccessKeyMarker => '_';

	private TextContainer TextContainer
	{
		get
		{
			if (_textContainer == null)
			{
				CreateTextBlock();
			}
			return _textContainer;
		}
	}

	/// <summary>Gets the number of child elements that are visual.</summary>
	/// <returns>Returns an integer that represents the number of child elements that are visible.</returns>
	protected override int VisualChildrenCount => 1;

	private static Style AccessKeyStyle
	{
		get
		{
			if (_accessKeyStyle == null)
			{
				Style style = new Style(typeof(Run));
				Trigger item = new Trigger
				{
					Property = KeyboardNavigation.ShowKeyboardCuesProperty,
					Value = true,
					Setters = { (SetterBase)new Setter(TextDecorationsProperty, System.Windows.TextDecorations.Underline) }
				};
				style.Triggers.Add(item);
				style.Seal();
				_accessKeyStyle = style;
			}
			return _accessKeyStyle;
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Markup.IAddChild.AddChild(System.Object)" />.</summary>
	/// <param name="value">The object to add to the <see cref="T:System.Windows.Controls.AccessText" />.</param>
	void IAddChild.AddChild(object value)
	{
		((IAddChild)TextBlock).AddChild(value);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Markup.IAddChild.AddText(System.String)" />.</summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		((IAddChild)TextBlock).AddText(text);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.AccessText" /> class. </summary>
	public AccessText()
	{
	}

	/// <summary>Remeasures the control. </summary>
	/// <returns>The size of the control. Cannot exceed the maximum size limit for the control.</returns>
	/// <param name="constraint">The maximum size limit for the control. The return value cannot exceed this size.</param>
	protected sealed override Size MeasureOverride(Size constraint)
	{
		TextBlock.Measure(constraint);
		return TextBlock.DesiredSize;
	}

	/// <summary>Arranges and sizes the content of an <see cref="T:System.Windows.Controls.AccessText" /> object. </summary>
	/// <returns>The size of the content.</returns>
	/// <param name="arrangeSize">The computed size that is used to arrange the content.</param>
	protected sealed override Size ArrangeOverride(Size arrangeSize)
	{
		TextBlock.Arrange(new Rect(arrangeSize));
		return arrangeSize;
	}

	internal static bool HasCustomSerialization(object o)
	{
		if (o is Run instance)
		{
			return HasCustomSerializationStorage.GetValue(instance);
		}
		return false;
	}

	private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((AccessText)d).TextBlock.SetValue(e.Property, e.NewValue);
	}

	private void CreateTextBlock()
	{
		_textContainer = new TextContainer(this, plainTextOnly: false);
		_textBlock = new TextBlock();
		AddVisualChild(_textBlock);
		_textBlock.IsContentPresenterContainer = true;
		_textBlock.SetTextContainer(_textContainer);
		InitializeTextContainerListener();
	}

	/// <summary>Gets the index of a visual child element.</summary>
	/// <returns>Returns an integer that represents the index of a visual child element.</returns>
	/// <param name="index">The index of the visual child element to return.</param>
	protected override Visual GetVisualChild(int index)
	{
		if (index != 0)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return TextBlock;
	}

	internal static void SerializeCustom(XmlWriter xmlWriter, object o)
	{
		if (o is Run run)
		{
			xmlWriter.WriteString(AccessKeyMarker + run.Text);
		}
	}

	private void UpdateAccessKey()
	{
		TextPointer textPointer = new TextPointer(TextContainer.Start);
		while (!_accessKeyLocated && textPointer.CompareTo(TextContainer.End) < 0)
		{
			if (textPointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
			{
				string textInRun = textPointer.GetTextInRun(LogicalDirection.Forward);
				int num = FindAccessKeyMarker(textInRun);
				if (num != -1 && num < textInRun.Length - 1)
				{
					string nextTextElement = StringInfo.GetNextTextElement(textInRun, num + 1);
					TextPointer positionAtOffset = textPointer.GetPositionAtOffset(num + 1 + nextTextElement.Length);
					_accessKey = new Run(nextTextElement);
					_accessKey.Style = AccessKeyStyle;
					RegisterAccessKey();
					HasCustomSerializationStorage.SetValue(_accessKey, value: true);
					_accessKeyLocated = true;
					UninitializeTextContainerListener();
					TextContainer.BeginChange();
					try
					{
						TextPointer textPointer2 = new TextPointer(textPointer, num);
						TextRangeEdit.DeleteInlineContent(textPointer2, positionAtOffset);
						_accessKey.RepositionWithContent(textPointer2);
					}
					finally
					{
						TextContainer.EndChange();
						InitializeTextContainerListener();
					}
				}
			}
			textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
		}
		textPointer = new TextPointer(TextContainer.Start);
		string text = AccessKeyMarker.ToString();
		string oldValue = text + text;
		while (textPointer.CompareTo(TextContainer.End) < 0)
		{
			if (textPointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
			{
				string textInRun2 = textPointer.GetTextInRun(LogicalDirection.Forward);
				string text2 = textInRun2.Replace(oldValue, text);
				if (textInRun2 != text2)
				{
					TextPointer start = new TextPointer(textPointer, 0);
					TextPointer textPointer3 = new TextPointer(textPointer, textInRun2.Length);
					UninitializeTextContainerListener();
					TextContainer.BeginChange();
					try
					{
						textPointer3.InsertTextInRun(text2);
						TextRangeEdit.DeleteInlineContent(start, textPointer3);
					}
					finally
					{
						TextContainer.EndChange();
						InitializeTextContainerListener();
					}
				}
			}
			textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
		}
	}

	private static int FindAccessKeyMarker(string text)
	{
		int length = text.Length;
		int num = 0;
		while (num < length)
		{
			int num2 = text.IndexOf(AccessKeyMarker, num);
			if (num2 == -1)
			{
				return -1;
			}
			if (num2 + 1 < length && text[num2 + 1] != AccessKeyMarker)
			{
				return num2;
			}
			num = num2 + 2;
		}
		return -1;
	}

	internal static string RemoveAccessKeyMarker(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			string text2 = AccessKeyMarker.ToString();
			string oldValue = text2 + text2;
			int num = FindAccessKeyMarker(text);
			if (num >= 0 && num < text.Length - 1)
			{
				text = text.Remove(num, 1);
			}
			text = text.Replace(oldValue, text2);
		}
		return text;
	}

	private void RegisterAccessKey()
	{
		if (_currentlyRegistered != null)
		{
			AccessKeyManager.Unregister(_currentlyRegistered, this);
			_currentlyRegistered = null;
		}
		string text = _accessKey.Text;
		if (!string.IsNullOrEmpty(text))
		{
			AccessKeyManager.Register(text, this);
			_currentlyRegistered = text;
		}
	}

	private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((AccessText)d).UpdateText((string)e.NewValue);
	}

	private void UpdateText(string text)
	{
		if (text == null)
		{
			text = string.Empty;
		}
		_accessKeyLocated = false;
		_accessKey = null;
		TextContainer.BeginChange();
		try
		{
			TextContainer.DeleteContentInternal(TextContainer.Start, TextContainer.End);
			Run run = Inline.CreateImplicitRun(this);
			TextContainer.End.InsertTextElement(run);
			run.Text = text;
		}
		finally
		{
			TextContainer.EndChange();
		}
	}

	private void InitializeTextContainerListener()
	{
		TextContainer.Changed += OnTextContainerChanged;
	}

	private void UninitializeTextContainerListener()
	{
		TextContainer.Changed -= OnTextContainerChanged;
	}

	private void OnTextContainerChanged(object sender, TextContainerChangedEventArgs args)
	{
		if (args.HasContentAddedOrRemoved)
		{
			UpdateAccessKey();
		}
	}
}
