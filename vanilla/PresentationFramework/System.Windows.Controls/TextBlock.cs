using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.Documents;
using MS.Internal.PresentationFramework;
using MS.Internal.Telemetry.PresentationFramework;
using MS.Internal.Text;

namespace System.Windows.Controls;

/// <summary>Provides a lightweight control for displaying small amounts of flow content.</summary>
[ContentProperty("Inlines")]
[Localizability(LocalizationCategory.Text)]
public class TextBlock : FrameworkElement, IContentHost, IAddChildInternal, IAddChild, IServiceProvider
{
	[Flags]
	private enum Flags
	{
		FormattedOnce = 1,
		MeasureInProgress = 2,
		TreeInReadOnlyMode = 4,
		RequiresAlignment = 8,
		ContentChangeInProgress = 0x10,
		IsContentPresenterContainer = 0x20,
		HasParagraphEllipses = 0x40,
		PendingTextContainerEventInit = 0x80,
		ArrangeInProgress = 0x100,
		IsTypographySet = 0x200,
		TextContentChanging = 0x400,
		IsHyphenatorSet = 0x800,
		HasFirstLine = 0x1000
	}

	private class ComplexContent
	{
		internal VisualCollection VisualChildren;

		internal readonly ITextContainer TextContainer;

		internal readonly bool ForeignTextContainer;

		internal readonly TextParagraphView TextView;

		internal ArrayList InlineObjects;

		internal Highlights Highlights => TextContainer.Highlights;

		internal ComplexContent(TextBlock owner, ITextContainer textContainer, bool foreignTextContianer, string content)
		{
			VisualChildren = new VisualCollection(owner);
			TextContainer = textContainer;
			ForeignTextContainer = foreignTextContianer;
			if (content != null && content.Length > 0)
			{
				InsertTextRun(TextContainer.End, content, whitespacesIgnorable: false);
			}
			TextView = new TextParagraphView(owner, TextContainer);
			TextContainer.TextView = TextView;
		}

		internal void Detach(TextBlock owner)
		{
			Highlights.Changed -= owner.OnHighlightChanged;
			TextContainer.Changing -= owner.OnTextContainerChanging;
			TextContainer.Change -= owner.OnTextContainerChange;
		}
	}

	private class SimpleContentEnumerator : IEnumerator
	{
		private readonly string _content;

		private bool _initialized;

		private bool _invalidPosition;

		object IEnumerator.Current
		{
			get
			{
				if (!_initialized || _invalidPosition)
				{
					throw new InvalidOperationException();
				}
				return _content;
			}
		}

		internal SimpleContentEnumerator(string content)
		{
			_content = content;
			_initialized = false;
			_invalidPosition = false;
		}

		void IEnumerator.Reset()
		{
			_initialized = false;
			_invalidPosition = false;
		}

		bool IEnumerator.MoveNext()
		{
			if (!_initialized)
			{
				_initialized = true;
				return true;
			}
			_invalidPosition = true;
			return false;
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.BaselineOffset" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBlock.BaselineOffset" /> dependency property.</returns>
	public static readonly DependencyProperty BaselineOffsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.Text" /> dependency property.</summary>
	/// <returns>The identifier of the <see cref="P:System.Windows.Controls.TextBlock.Text" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty TextProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.FontFamily" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBlock.FontFamily" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FontFamilyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.FontStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBlock.FontStyle" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FontStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.FontWeight" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBlock.FontWeight" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FontWeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.FontStretch" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBlock.FontStretch" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FontStretchProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.FontSize" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBlock.FontSize" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FontSizeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.Foreground" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBlock.Foreground" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ForegroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.Background" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBlock.Background" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty BackgroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.TextDecorations" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBlock.TextDecorations" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty TextDecorationsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.TextEffects" /> dependency property.</summary>
	/// <returns>The identifier of the <see cref="P:System.Windows.Controls.TextBlock.TextEffects" /> dependency property.</returns>
	public static readonly DependencyProperty TextEffectsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.LineHeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBlock.LineHeight" /> dependency property.</returns>
	public static readonly DependencyProperty LineHeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.LineStackingStrategy" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBlock.LineStackingStrategy" /> dependency property.</returns>
	public static readonly DependencyProperty LineStackingStrategyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.Padding" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBlock.Padding" /> dependency property.</returns>
	public static readonly DependencyProperty PaddingProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.TextAlignment" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBlock.TextAlignment" /> dependency property.</returns>
	public static readonly DependencyProperty TextAlignmentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.TextTrimming" /> dependency property. </summary>
	/// <returns>The identifier of the <see cref="P:System.Windows.Controls.TextBlock.TextTrimming" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty TextTrimmingProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.TextWrapping" /> dependency property. </summary>
	/// <returns>The identifier of the <see cref="P:System.Windows.Controls.TextBlock.TextWrapping" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty TextWrappingProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.TextBlock.IsHyphenationEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.TextBlock.IsHyphenationEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsHyphenationEnabledProperty;

	private TextBlockCache _textBlockCache;

	private string _contentCache;

	private ComplexContent _complexContent;

	private TextFormatter _textFormatterIdeal;

	private TextFormatter _textFormatterDisplay;

	private Size _referenceSize;

	private Size _previousDesiredSize;

	private double _baselineOffset;

	private static readonly UncommonField<NaturalLanguageHyphenator> HyphenatorField;

	private LineMetrics _firstLine;

	private List<LineMetrics> _subsequentLines;

	private Flags _flags;

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.  Use the type-safe <see cref="P:System.Windows.Controls.TextBlock.HostedElementsCore" /> property instead.</summary>
	/// <returns>Elements hosted by the content host. </returns>
	IEnumerator<IInputElement> IContentHost.HostedElements => HostedElementsCore;

	/// <summary>Gets an enumerator that can iterate the logical children of the <see cref="T:System.Windows.Controls.TextBlock" />.</summary>
	/// <returns>An enumerator for the logical children.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			if (IsContentPresenterContainer)
			{
				return EmptyEnumerator.Instance;
			}
			if (_complexContent == null)
			{
				return new SimpleContentEnumerator(Text);
			}
			if (!_complexContent.ForeignTextContainer)
			{
				return new RangeContentEnumerator(ContentStart, ContentEnd);
			}
			return EmptyEnumerator.Instance;
		}
	}

	/// <summary>Gets an <see cref="T:System.Windows.Documents.InlineCollection" /> containing the top-level <see cref="T:System.Windows.Documents.Inline" /> elements that comprise the contents of the <see cref="T:System.Windows.Controls.TextBlock" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Documents.InlineCollection" /> containing the <see cref="T:System.Windows.Documents.Inline" /> elements that comprise the contents of the <see cref="T:System.Windows.Controls.TextBlock" />.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public InlineCollection Inlines => new InlineCollection(this, isOwnerParent: true);

	/// <summary>Gets a <see cref="T:System.Windows.Documents.TextPointer" /> to the beginning of content in the <see cref="T:System.Windows.Controls.TextBlock" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> to the beginning of content in the <see cref="T:System.Windows.Controls.TextBlock" />.</returns>
	public TextPointer ContentStart
	{
		get
		{
			EnsureComplexContent();
			return (TextPointer)_complexContent.TextContainer.Start;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Documents.TextPointer" /> to the end of content in the <see cref="T:System.Windows.Controls.TextBlock" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> to the end of content in the <see cref="T:System.Windows.Controls.TextBlock" />.</returns>
	public TextPointer ContentEnd
	{
		get
		{
			EnsureComplexContent();
			return (TextPointer)_complexContent.TextContainer.End;
		}
	}

	internal TextRange TextRange => new TextRange(ContentStart, ContentEnd);

	/// <summary>Gets a <see cref="T:System.Windows.LineBreakCondition" /> that indicates how content should break before the current element. </summary>
	/// <returns>The conditions for breaking content after the current element.</returns>
	public LineBreakCondition BreakBefore => LineBreakCondition.BreakDesired;

	/// <summary>Gets a <see cref="T:System.Windows.LineBreakCondition" /> that indicates how content should break after the current element.</summary>
	/// <returns>The conditions for breaking content after the current element.</returns>
	public LineBreakCondition BreakAfter => LineBreakCondition.BreakDesired;

	/// <summary>Gets the currently effective typography variations for the contents of this element.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.Typography" /> object that specifies the currently effective typography variations. For a list of default typography values, see <see cref="T:System.Windows.Documents.Typography" />.</returns>
	public Typography Typography => new Typography(this);

	/// <summary>Gets or sets the amount by which each line of text is offset from the baseline.  </summary>
	/// <returns>The amount by which each line of text is offset from the baseline, in device independent pixels. <see cref="F:System.Double.NaN" /> indicates that an optimal baseline offset is automatically calculated from the current font characteristics. The default is <see cref="F:System.Double.NaN" />.</returns>
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

	/// <summary>Gets or sets the text contents of a <see cref="T:System.Windows.Controls.TextBlock" />.  </summary>
	/// <returns>The text contents of this <see cref="T:System.Windows.Controls.TextBlock" />. Note that all non-text content is stripped out, resulting in a plain text representation of the <see cref="T:System.Windows.Controls.TextBlock" /> contents. The default is <see cref="F:System.String.Empty" />.</returns>
	[Localizability(LocalizationCategory.Text)]
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

	/// <summary>Gets or sets the preferred top-level font family for the <see cref="T:System.Windows.Controls.TextBlock" />.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.FontFamily" /> object specifying the preferred font family, or a primary preferred font family with one or more fallback font families. The default is the font determined by the <see cref="P:System.Windows.SystemFonts.MessageFontFamily" /> value.</returns>
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

	/// <summary>Gets or sets the top-level font style for the <see cref="T:System.Windows.Controls.TextBlock" />.  </summary>
	/// <returns>A member of the <see cref="T:System.Windows.FontStyles" /> class specifying the desired font style. The default is determined by the <see cref="P:System.Windows.SystemFonts.MessageFontStyle" /> value.</returns>
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

	/// <summary>Gets or sets the top-level font weight for the <see cref="T:System.Windows.Controls.TextBlock" />.  </summary>
	/// <returns>A member of the <see cref="T:System.Windows.FontWeights" /> class specifying the desired font weight. The default is determined by the <see cref="P:System.Windows.SystemFonts.MessageFontWeight" /> value.</returns>
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

	/// <summary>Gets or sets the top-level font-stretching characteristics for the <see cref="T:System.Windows.Controls.TextBlock" />.  </summary>
	/// <returns>A member of the <see cref="T:System.Windows.FontStretch" /> class specifying the desired font-stretching characteristics to use. The default is <see cref="P:System.Windows.FontStretches.Normal" />.</returns>
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

	/// <summary>Gets or sets the top-level font size for the <see cref="T:System.Windows.Controls.TextBlock" />.   </summary>
	/// <returns>The desired font size to use in device independent pixels). The default is determined by the <see cref="P:System.Windows.SystemFonts.MessageFontSize" /> value.</returns>
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

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> to apply to the text contents of the <see cref="T:System.Windows.Controls.TextBlock" />.  </summary>
	/// <returns>The brush used to apply to the text contents. The default is <see cref="P:System.Windows.Media.Brushes.Black" />.</returns>
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

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> used to fill the background of content area.  </summary>
	/// <returns>The brush used to fill the background of the content area, or null to not use a background brush. The default is null.</returns>
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

	/// <summary>Gets or sets a <see cref="T:System.Windows.TextDecorationCollection" /> that contains the effects to apply to the text of a <see cref="T:System.Windows.Controls.TextBlock" />.  </summary>
	/// <returns>A <see cref="T:System.Windows.TextDecorationCollection" /> collection that contains text decorations to apply to this element. The default is null (no text decorations applied).</returns>
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

	/// <summary>Gets or sets the effects to apply to the text content in this element.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextEffectCollection" /> containing one or more <see cref="T:System.Windows.Media.TextEffect" /> objects that define effects to apply to the text of the <see cref="T:System.Windows.Controls.TextBlock" />. The default is null (no effects applied).</returns>
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

	/// <summary>Gets or sets the height of each line of content.  </summary>
	/// <returns>The height of line, in device independent pixels, in the range of 0.0034 to 160000. A value of <see cref="F:System.Double.NaN" /> (equivalent to an attribute value of "Auto") indicates that the line height is determined automatically from the current font characteristics. The default is <see cref="F:System.Double.NaN" />.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Windows.Controls.TextBlock.LineHeight" /> is set to a non-positive value.</exception>
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

	/// <summary>Gets or sets the mechanism by which a line box is determined for each line of text within the <see cref="T:System.Windows.Controls.TextBlock" />.  </summary>
	/// <returns>The mechanism by which a line box is determined for each line of text within the <see cref="T:System.Windows.Controls.TextBlock" />. The default is <see cref="F:System.Windows.LineStackingStrategy.MaxHeight" />.</returns>
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

	/// <summary>Gets or sets a value that indicates the thickness of padding space between the boundaries of the content area, and the content displayed by a <see cref="T:System.Windows.Controls.TextBlock" />.  </summary>
	/// <returns>A <see cref="T:System.Windows.Thickness" /> structure specifying the amount of padding to apply, in device independent pixels. The default is <see cref="F:System.Double.NaN" />.</returns>
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

	/// <summary>Gets or sets a value that indicates the horizontal alignment of text content.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.TextAlignment" /> values that specifies the desired alignment. The default is <see cref="F:System.Windows.TextAlignment.Left" />.</returns>
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

	/// <summary>Gets or sets the text trimming behavior to employ when content overflows the content area.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.TextTrimming" /> values that specifies the text trimming behavior to employ. The default is <see cref="F:System.Windows.TextTrimming.None" />.</returns>
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

	/// <summary>Gets or sets how the <see cref="T:System.Windows.Controls.TextBlock" /> should wrap text.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.TextWrapping" /> values. The default is <see cref="F:System.Windows.TextWrapping.NoWrap" />.</returns>
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

	/// <summary>Gets or sets a value that indicates whether automatic hyphenation of words is enabled or disabled.  </summary>
	/// <returns>true to indicate that automatic breaking and hyphenation of words is enabled; otherwise, false. The default is false.</returns>
	public bool IsHyphenationEnabled
	{
		get
		{
			return (bool)GetValue(IsHyphenationEnabledProperty);
		}
		set
		{
			SetValue(IsHyphenationEnabledProperty, value);
		}
	}

	/// <summary>Gets the number of <see cref="T:System.Windows.Media.Visual" /> children hosted by the <see cref="T:System.Windows.Controls.TextBlock" />.</summary>
	/// <returns>The number of visual children hosted by the <see cref="T:System.Windows.Controls.TextBlock" />.</returns>
	protected override int VisualChildrenCount
	{
		get
		{
			if (_complexContent != null)
			{
				return _complexContent.VisualChildren.Count;
			}
			return 0;
		}
	}

	/// <summary>Gets an enumerator that can be used iterate the elements hosted by this <see cref="T:System.Windows.Controls.TextBlock" />.</summary>
	/// <returns>An enumerator that can iterate elements hosted by this <see cref="T:System.Windows.Controls.TextBlock" />.</returns>
	protected virtual IEnumerator<IInputElement> HostedElementsCore
	{
		get
		{
			if (CheckFlags(Flags.ContentChangeInProgress))
			{
				throw new InvalidOperationException(SR.TextContainerChangingReentrancyInvalid);
			}
			if (_complexContent == null || !(_complexContent.TextContainer is TextContainer))
			{
				return new HostedElements(new ReadOnlyCollection<TextSegment>(new List<TextSegment>(0)));
			}
			List<TextSegment> list = new List<TextSegment>(1);
			TextSegment item = new TextSegment(_complexContent.TextContainer.Start, _complexContent.TextContainer.End);
			list.Insert(0, item);
			return new HostedElements(new ReadOnlyCollection<TextSegment>(list));
		}
	}

	internal TextFormatter TextFormatter
	{
		get
		{
			TextFormattingMode textFormattingMode = TextOptions.GetTextFormattingMode(this);
			if (TextFormattingMode.Display == textFormattingMode)
			{
				if (_textFormatterDisplay == null)
				{
					_textFormatterDisplay = TextFormatter.FromCurrentDispatcher(textFormattingMode);
				}
				return _textFormatterDisplay;
			}
			if (_textFormatterIdeal == null)
			{
				_textFormatterIdeal = TextFormatter.FromCurrentDispatcher(textFormattingMode);
			}
			return _textFormatterIdeal;
		}
	}

	internal ITextContainer TextContainer
	{
		get
		{
			EnsureComplexContent();
			return _complexContent.TextContainer;
		}
	}

	internal ITextView TextView
	{
		get
		{
			EnsureComplexContent();
			return _complexContent.TextView;
		}
	}

	internal Highlights Highlights
	{
		get
		{
			EnsureComplexContent();
			return _complexContent.Highlights;
		}
	}

	internal LineProperties ParagraphProperties => GetLineProperties();

	internal bool IsLayoutDataValid
	{
		get
		{
			if (base.IsMeasureValid && base.IsArrangeValid && CheckFlags(Flags.HasFirstLine) && !CheckFlags(Flags.ContentChangeInProgress) && !CheckFlags(Flags.MeasureInProgress))
			{
				return !CheckFlags(Flags.ArrangeInProgress);
			}
			return false;
		}
	}

	internal bool HasComplexContent => _complexContent != null;

	internal bool IsTypographyDefaultValue => !CheckFlags(Flags.IsTypographySet);

	private ArrayList InlineObjects
	{
		get
		{
			if (_complexContent != null)
			{
				return _complexContent.InlineObjects;
			}
			return null;
		}
		set
		{
			if (_complexContent != null)
			{
				_complexContent.InlineObjects = value;
			}
		}
	}

	internal bool IsContentPresenterContainer
	{
		get
		{
			return CheckFlags(Flags.IsContentPresenterContainer);
		}
		set
		{
			SetFlags(value, Flags.IsContentPresenterContainer);
		}
	}

	private int LineCount
	{
		get
		{
			if (CheckFlags(Flags.HasFirstLine))
			{
				if (_subsequentLines != null)
				{
					return _subsequentLines.Count + 1;
				}
				return 1;
			}
			return 0;
		}
	}

	internal override int EffectiveValuesInitialSize => 28;

	/// <summary>This method supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.  Use the type-safe <see cref="M:System.Windows.Controls.TextBlock.InputHitTestCore(System.Windows.Point)" /> method instead.</summary>
	/// <returns>The element that has been hit. </returns>
	/// <param name="point">Mouse coordinates relative to the content host. </param>
	IInputElement IContentHost.InputHitTest(Point point)
	{
		return InputHitTestCore(point);
	}

	/// <summary>This method supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.  Use the type-safe <see cref="M:System.Windows.Controls.TextBlock.GetRectanglesCore(System.Windows.ContentElement)" /> method instead.</summary>
	/// <returns> A read-only collection of bounding rectangles for the specified <see cref="T:System.Windows.ContentElement" />.</returns>
	/// <param name="child"> A <see cref="T:System.Windows.ContentElement" /> for which to generate and return a collection of bounding rectangles.</param>
	ReadOnlyCollection<Rect> IContentHost.GetRectangles(ContentElement child)
	{
		return GetRectanglesCore(child);
	}

	/// <summary>This method supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.  Use the type-safe <see cref="M:System.Windows.Controls.TextBlock.OnChildDesiredSizeChangedCore(System.Windows.UIElement)" /> method instead.</summary>
	/// <param name="child"> The child <see cref="T:System.Windows.UIElement" /> element whose <see cref="P:System.Windows.UIElement.DesiredSize" /> has changed.</param>
	void IContentHost.OnChildDesiredSizeChanged(UIElement child)
	{
		OnChildDesiredSizeChangedCore(child);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="value">An object to add as a child. </param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		EnsureComplexContent();
		if (!(_complexContent.TextContainer is TextContainer))
		{
			throw new ArgumentException(SR.Format(SR.TextPanelIllegalParaTypeForIAddChild, "value", value.GetType()));
		}
		Type type = _complexContent.TextContainer.Parent.GetType();
		Type type2 = value.GetType();
		if (!TextSchema.IsValidChildOfContainer(type, type2))
		{
			if (!(value is UIElement))
			{
				throw new ArgumentException(SR.Format(SR.TextSchema_ChildTypeIsInvalid, type.Name, type2.Name));
			}
			value = new InlineUIContainer((UIElement)value);
		}
		Invariant.Assert(value is Inline, "Schema validation helper must guarantee that invalid element is not passed here");
		TextContainer textContainer = (TextContainer)_complexContent.TextContainer;
		textContainer.BeginChange();
		try
		{
			TextPointer end = textContainer.End;
			textContainer.InsertElementInternal(end, end, (Inline)value);
		}
		finally
		{
			textContainer.EndChange();
		}
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="text">A string to add to the object. </param>
	void IAddChild.AddText(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (_complexContent == null)
		{
			Text += text;
			return;
		}
		TextContainer textContainer = (TextContainer)_complexContent.TextContainer;
		textContainer.BeginChange();
		try
		{
			TextPointer end = textContainer.End;
			Run run = Inline.CreateImplicitRun(this);
			textContainer.InsertElementInternal(end, end, run);
			run.Text = text;
		}
		finally
		{
			textContainer.EndChange();
		}
	}

	/// <summary>This method supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>A service object of type <paramref name="serviceType" />, or null if there is no service object of type <paramref name="serviceType" />. </returns>
	/// <param name="serviceType">An object that specifies the type of service object to get. </param>
	object IServiceProvider.GetService(Type serviceType)
	{
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (serviceType == typeof(ITextView))
		{
			EnsureComplexContent();
			return _complexContent.TextView;
		}
		if (serviceType == typeof(ITextContainer))
		{
			EnsureComplexContent();
			return _complexContent.TextContainer;
		}
		if (serviceType == typeof(TextContainer))
		{
			EnsureComplexContent();
			return _complexContent.TextContainer as TextContainer;
		}
		return null;
	}

	static TextBlock()
	{
		BaselineOffsetProperty = DependencyProperty.RegisterAttached("BaselineOffset", typeof(double), typeof(TextBlock), new FrameworkPropertyMetadata(double.NaN, OnBaselineOffsetChanged));
		TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextBlock), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnTextChanged, CoerceText));
		FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(TextBlock));
		FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(TextBlock));
		FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(TextBlock));
		FontStretchProperty = TextElement.FontStretchProperty.AddOwner(typeof(TextBlock));
		FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(TextBlock));
		ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(TextBlock));
		BackgroundProperty = TextElement.BackgroundProperty.AddOwner(typeof(TextBlock), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		TextDecorationsProperty = Inline.TextDecorationsProperty.AddOwner(typeof(TextBlock), new FrameworkPropertyMetadata(new FreezableDefaultValueFactory(TextDecorationCollection.Empty), FrameworkPropertyMetadataOptions.AffectsRender));
		TextEffectsProperty = TextElement.TextEffectsProperty.AddOwner(typeof(TextBlock), new FrameworkPropertyMetadata(new FreezableDefaultValueFactory(TextEffectCollection.Empty), FrameworkPropertyMetadataOptions.AffectsRender));
		LineHeightProperty = Block.LineHeightProperty.AddOwner(typeof(TextBlock));
		LineStackingStrategyProperty = Block.LineStackingStrategyProperty.AddOwner(typeof(TextBlock));
		PaddingProperty = Block.PaddingProperty.AddOwner(typeof(TextBlock), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure));
		TextAlignmentProperty = Block.TextAlignmentProperty.AddOwner(typeof(TextBlock));
		TextTrimmingProperty = DependencyProperty.Register("TextTrimming", typeof(TextTrimming), typeof(TextBlock), new FrameworkPropertyMetadata(TextTrimming.None, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), IsValidTextTrimming);
		TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(TextBlock), new FrameworkPropertyMetadata(TextWrapping.NoWrap, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), IsValidTextWrap);
		IsHyphenationEnabledProperty = Block.IsHyphenationEnabledProperty.AddOwner(typeof(TextBlock));
		HyphenatorField = new UncommonField<NaturalLanguageHyphenator>();
		BaselineOffsetProperty.OverrideMetadata(typeof(TextBlock), new FrameworkPropertyMetadata(null, CoerceBaselineOffset));
		PropertyChangedCallback propertyChangedCallback = OnTypographyChanged;
		DependencyProperty[] typographyPropertiesList = Typography.TypographyPropertiesList;
		for (int i = 0; i < typographyPropertiesList.Length; i++)
		{
			typographyPropertiesList[i].OverrideMetadata(typeof(TextBlock), new FrameworkPropertyMetadata(propertyChangedCallback));
		}
		EventManager.RegisterClassHandler(typeof(TextBlock), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(OnRequestBringIntoView));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBlock), new FrameworkPropertyMetadata(typeof(TextBlock)));
		ControlsTraceLogger.AddControl(TelemetryControls.TextBlock);
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Controls.TextBlock" /> class.</summary>
	public TextBlock()
	{
		Initialize();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.TextBlock" /> class, adding a specified <see cref="T:System.Windows.Documents.Inline" /> element as the initial display content.</summary>
	/// <param name="inline">An object deriving from the abstract <see cref="T:System.Windows.Documents.Inline" /> class, to be added as the initial content.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inline" /> is null.</exception>
	public TextBlock(Inline inline)
	{
		Initialize();
		if (inline == null)
		{
			throw new ArgumentNullException("inline");
		}
		Inlines.Add(inline);
	}

	private void Initialize()
	{
	}

	/// <summary>Returns a <see cref="T:System.Windows.Documents.TextPointer" /> to the position closest to a specified <see cref="T:System.Windows.Point" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> to the specified point, or null if <paramref name="snapToText" /> is false and the specified point does not fall within a character bounding box in the <see cref="T:System.Windows.Controls.TextBlock" /> content area.</returns>
	/// <param name="point">A <see cref="T:System.Windows.Point" /> in the coordinate space of the <see cref="T:System.Windows.Controls.TextBlock" /> for which to return a <see cref="T:System.Windows.Documents.TextPointer" />.</param>
	/// <param name="snapToText">true to return a <see cref="T:System.Windows.Documents.TextPointer" /> to the insertion point closest to <paramref name="point" />, whether or not <paramref name="point" /> is inside a character's bounding box; false to return null if <paramref name="point" /> is not inside a character's bounding box.</param>
	/// <exception cref="T:System.InvalidOperationException">Current, valid layout information for the control is unavailable.</exception>
	public TextPointer GetPositionFromPoint(Point point, bool snapToText)
	{
		if (CheckFlags(Flags.ContentChangeInProgress))
		{
			throw new InvalidOperationException(SR.TextContainerChangingReentrancyInvalid);
		}
		EnsureComplexContent();
		if (((ITextView)_complexContent.TextView).Validate(point))
		{
			return (TextPointer)_complexContent.TextView.GetTextPositionFromPoint(point, snapToText);
		}
		return snapToText ? new TextPointer((TextPointer)_complexContent.TextContainer.Start) : null;
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.TextBlock.BaselineOffset" /> attached property on a specified dependency object.</summary>
	/// <param name="element">The dependency object on which to set the value of the <see cref="P:System.Windows.Controls.TextBlock.BaselineOffset" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetBaselineOffset(DependencyObject element, double value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(BaselineOffsetProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Controls.TextBlock.BaselineOffset" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Controls.TextBlock.BaselineOffset" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Controls.TextBlock.BaselineOffset" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static double GetBaselineOffset(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(BaselineOffsetProperty);
	}

	private static object CoerceText(DependencyObject d, object value)
	{
		TextBlock textBlock = (TextBlock)d;
		if (value == null)
		{
			value = string.Empty;
		}
		if (textBlock._complexContent != null && !textBlock.CheckFlags(Flags.TextContentChanging) && (string)value == (string)textBlock.GetValue(TextProperty))
		{
			OnTextChanged(d, (string)value);
		}
		return value;
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.TextBlock.FontFamily" /> attached property on a specified dependency object.</summary>
	/// <param name="element">The dependency object on which to set the value of the <see cref="P:System.Windows.Controls.TextBlock.FontFamily" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetFontFamily(DependencyObject element, FontFamily value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(FontFamilyProperty, value);
	}

	/// <summary>Returns the value of the <see cref="F:System.Windows.Controls.TextBlock.FontFamilyProperty" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Controls.TextBlock.FontFamily" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Controls.TextBlock.FontFamily" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static FontFamily GetFontFamily(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FontFamily)element.GetValue(FontFamilyProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.TextBlock.FontStyle" /> attached property on a specified dependency object.</summary>
	/// <param name="element">The dependency object on which to set the value of the <see cref="P:System.Windows.Controls.TextBlock.FontStyle" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetFontStyle(DependencyObject element, FontStyle value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(FontStyleProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Controls.TextBlock.FontStyle" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Controls.TextBlock.FontStyle" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Controls.TextBlock.FontStyle" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static FontStyle GetFontStyle(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FontStyle)element.GetValue(FontStyleProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.TextBlock.FontWeight" /> attached property on a specified dependency object.</summary>
	/// <param name="element">The dependency object on which to set the value of the <see cref="P:System.Windows.Controls.TextBlock.FontWeight" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetFontWeight(DependencyObject element, FontWeight value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(FontWeightProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Controls.TextBlock.FontWeight" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Controls.TextBlock.FontWeight" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Controls.TextBlock.FontWeight" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static FontWeight GetFontWeight(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FontWeight)element.GetValue(FontWeightProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.TextBlock.FontStretch" /> attached property on a specified dependency object.</summary>
	/// <param name="element">The dependency object on which to set the value of the <see cref="P:System.Windows.Controls.TextBlock.FontStretch" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetFontStretch(DependencyObject element, FontStretch value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(FontStretchProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Controls.TextBlock.FontStretch" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Controls.TextBlock.FontStretch" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Controls.TextBlock.FontStretch" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static FontStretch GetFontStretch(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FontStretch)element.GetValue(FontStretchProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.TextBlock.FontSize" /> attached property on a specified dependency object.</summary>
	/// <param name="element">The dependency object on which to set the value of the <see cref="P:System.Windows.Controls.TextBlock.FontSize" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetFontSize(DependencyObject element, double value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(FontSizeProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Controls.TextBlock.FontSize" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Controls.TextBlock.FontSize" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Controls.TextBlock.FontSize" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	[TypeConverter(typeof(FontSizeConverter))]
	public static double GetFontSize(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(FontSizeProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.TextBlock.Foreground" /> attached property on a specified dependency object.</summary>
	/// <param name="element">The dependency object on which to set the value of the <see cref="P:System.Windows.Controls.TextBlock.Foreground" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetForeground(DependencyObject element, Brush value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ForegroundProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Controls.TextBlock.Foreground" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Controls.TextBlock.Foreground" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Controls.TextBlock.Foreground" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static Brush GetForeground(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (Brush)element.GetValue(ForegroundProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.TextBlock.LineHeight" /> attached property on a specified dependency object.</summary>
	/// <param name="element">The dependency object on which to set the value of the <see cref="P:System.Windows.Controls.TextBlock.LineHeight" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Windows.Controls.TextBlock.LineHeight" />is set to a non-positive value.</exception>
	public static void SetLineHeight(DependencyObject element, double value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(LineHeightProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Controls.TextBlock.LineHeight" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Controls.TextBlock.LineHeight" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Controls.TextBlock.LineHeight" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	[TypeConverter(typeof(LengthConverter))]
	public static double GetLineHeight(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(LineHeightProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.TextBlock.LineStackingStrategy" /> attached property on a specified dependency object.</summary>
	/// <param name="element">The dependency object on which to set the value of the <see cref="P:System.Windows.Controls.TextBlock.LineStackingStrategy" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetLineStackingStrategy(DependencyObject element, LineStackingStrategy value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(LineStackingStrategyProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Controls.TextBlock.LineStackingStrategy" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Controls.TextBlock.LineStackingStrategy" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Controls.TextBlock.LineStackingStrategy" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static LineStackingStrategy GetLineStackingStrategy(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (LineStackingStrategy)element.GetValue(LineStackingStrategyProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.TextBlock.TextAlignment" /> attached property on a specified dependency object.</summary>
	/// <param name="element">The dependency object on which to set the value of the <see cref="P:System.Windows.Controls.TextBlock.TextAlignment" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetTextAlignment(DependencyObject element, TextAlignment value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(TextAlignmentProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Controls.TextBlock.TextAlignment" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Controls.TextBlock.TextAlignment" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Controls.TextBlock.TextAlignment" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static TextAlignment GetTextAlignment(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (TextAlignment)element.GetValue(TextAlignmentProperty);
	}

	/// <summary>Returns the <see cref="T:System.Windows.Media.Visual" /> child at a specified index.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Visual" /> child at the specified index.</returns>
	/// <param name="index">A zero-based index specifying the <see cref="T:System.Windows.Media.Visual" /> child to return.  This value must be between 0 and (<see cref="P:System.Windows.Controls.TextBlock.VisualChildrenCount" /> minus 1)</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not between 0 and (<see cref="P:System.Windows.Controls.TextBlock.VisualChildrenCount" /> minus 1)</exception>
	protected override Visual GetVisualChild(int index)
	{
		if (_complexContent == null)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return _complexContent.VisualChildren[index];
	}

	/// <summary>Called to re-measure the <see cref="T:System.Windows.Controls.TextBlock" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> structure indicating the new size of the <see cref="T:System.Windows.Controls.TextBlock" />.</returns>
	/// <param name="constraint">A <see cref="T:System.Windows.Size" /> structure specifying any constraints on the size of the <see cref="T:System.Windows.Controls.TextBlock" />.</param>
	protected sealed override Size MeasureOverride(Size constraint)
	{
		VerifyReentrancy();
		_textBlockCache = null;
		EnsureTextBlockCache();
		LineProperties lineProperties = _textBlockCache._lineProperties;
		if (CheckFlags(Flags.PendingTextContainerEventInit))
		{
			Invariant.Assert(_complexContent != null);
			InitializeTextContainerListeners();
			SetFlags(value: false, Flags.PendingTextContainerEventInit);
		}
		int lineCount = LineCount;
		if (lineCount > 0 && base.IsMeasureValid && InlineObjects == null && ((lineProperties.TextTrimming != 0) ? (DoubleUtil.AreClose(constraint.Width, _referenceSize.Width) && lineProperties.TextWrapping == TextWrapping.NoWrap && (DoubleUtil.AreClose(constraint.Height, _referenceSize.Height) || lineCount == 1)) : (DoubleUtil.AreClose(constraint.Width, _referenceSize.Width) || lineProperties.TextWrapping == TextWrapping.NoWrap)))
		{
			_referenceSize = constraint;
			return _previousDesiredSize;
		}
		_referenceSize = constraint;
		CheckFlags(Flags.FormattedOnce);
		double baselineOffset = _baselineOffset;
		InlineObjects = null;
		int capacity = ((_subsequentLines == null) ? 1 : _subsequentLines.Count);
		ClearLineMetrics();
		if (_complexContent != null)
		{
			_complexContent.TextView.Invalidate();
		}
		lineProperties.IgnoreTextAlignment = true;
		SetFlags(value: true, Flags.RequiresAlignment);
		SetFlags(value: true, Flags.FormattedOnce);
		SetFlags(value: false, Flags.HasParagraphEllipses);
		SetFlags(value: true, Flags.MeasureInProgress | Flags.TreeInReadOnlyMode);
		Size size = default(Size);
		bool flag = true;
		try
		{
			Line line = CreateLine(lineProperties);
			bool flag2 = false;
			int num = 0;
			TextLineBreak textLineBreak = null;
			Thickness padding = Padding;
			Size size2 = new Size(Math.Max(0.0, constraint.Width - (padding.Left + padding.Right)), Math.Max(0.0, constraint.Height - (padding.Top + padding.Bottom)));
			TextDpi.EnsureValidLineWidth(ref size2);
			while (!flag2)
			{
				using (line)
				{
					line.Format(num, size2.Width, GetLineProperties(num == 0, lineProperties), textLineBreak, _textBlockCache._textRunCache, showParagraphEllipsis: false);
					double num2 = CalcLineAdvance(line.Height, lineProperties);
					LineMetrics lineMetrics = new LineMetrics(line.Length, line.Width, num2, line.BaselineOffset, line.HasInlineObjects(), textLineBreak);
					if (!CheckFlags(Flags.HasFirstLine))
					{
						SetFlags(value: true, Flags.HasFirstLine);
						_firstLine = lineMetrics;
					}
					else
					{
						if (_subsequentLines == null)
						{
							_subsequentLines = new List<LineMetrics>(capacity);
						}
						_subsequentLines.Add(lineMetrics);
					}
					size.Width = Math.Max(size.Width, line.GetCollapsedWidth());
					if (lineProperties.TextTrimming == TextTrimming.None || size2.Height >= size.Height + num2 || num == 0)
					{
						_baselineOffset = size.Height + line.BaselineOffset;
						size.Height += num2;
					}
					else
					{
						SetFlags(value: true, Flags.HasParagraphEllipses);
					}
					textLineBreak = line.GetTextLineBreak();
					flag2 = line.EndOfParagraph;
					num += line.Length;
					if (!flag2 && lineProperties.TextWrapping == TextWrapping.NoWrap && line.Length == 9600)
					{
						flag2 = true;
					}
				}
			}
			size.Width += padding.Left + padding.Right;
			size.Height += padding.Top + padding.Bottom;
			Invariant.Assert(textLineBreak == null);
			flag = false;
		}
		finally
		{
			lineProperties.IgnoreTextAlignment = false;
			SetFlags(value: false, Flags.MeasureInProgress | Flags.TreeInReadOnlyMode);
			if (flag)
			{
				_textBlockCache._textRunCache = null;
				ClearLineMetrics();
			}
		}
		if (!DoubleUtil.AreClose(baselineOffset, _baselineOffset))
		{
			CoerceValue(BaselineOffsetProperty);
		}
		_previousDesiredSize = size;
		return size;
	}

	/// <summary>Positions child elements and determines a size for the <see cref="T:System.Windows.Controls.TextBlock" />.</summary>
	/// <returns>The actual <see cref="T:System.Windows.Size" /> used to arrange the element.</returns>
	/// <param name="arrangeSize">A <see cref="T:System.Windows.Size" /> within the hosting parent element that the <see cref="T:System.Windows.Controls.TextBlock" /> should use to arrange itself and its child elements. Sizing constraints may affect this requested size.</param>
	protected sealed override Size ArrangeOverride(Size arrangeSize)
	{
		VerifyReentrancy();
		if (_complexContent != null)
		{
			_complexContent.VisualChildren.Clear();
		}
		ArrayList inlineObjects = InlineObjects;
		int lineCount = LineCount;
		if (inlineObjects != null && lineCount > 0)
		{
			bool flag = true;
			SetFlags(value: true, Flags.TreeInReadOnlyMode);
			SetFlags(value: true, Flags.ArrangeInProgress);
			try
			{
				EnsureTextBlockCache();
				LineProperties lineProperties = _textBlockCache._lineProperties;
				double wrappingWidth = CalcWrappingWidth(arrangeSize.Width);
				Vector vector = CalcContentOffset(arrangeSize, wrappingWidth);
				Line line = CreateLine(lineProperties);
				int num = 0;
				Vector lineOffset = vector;
				for (int i = 0; i < lineCount; i++)
				{
					LineMetrics line2 = GetLine(i);
					if (line2.HasInlineObjects)
					{
						using (line)
						{
							bool ellipsis = ParagraphEllipsisShownOnLine(i, lineOffset.Y - vector.Y);
							Format(line, line2.Length, num, wrappingWidth, GetLineProperties(num == 0, lineProperties), line2.TextLineBreak, _textBlockCache._textRunCache, ellipsis);
							line.Arrange(_complexContent.VisualChildren, lineOffset);
						}
					}
					lineOffset.Y += line2.Height;
					num += line2.Length;
				}
				flag = false;
			}
			finally
			{
				SetFlags(value: false, Flags.TreeInReadOnlyMode);
				SetFlags(value: false, Flags.ArrangeInProgress);
				if (flag)
				{
					_textBlockCache._textRunCache = null;
					ClearLineMetrics();
				}
			}
		}
		if (_complexContent != null)
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(OnValidateTextView), EventArgs.Empty);
		}
		InvalidateVisual();
		return arrangeSize;
	}

	/// <summary>Renders the contents of a <see cref="T:System.Windows.Controls.TextBlock" />.</summary>
	/// <param name="ctx">The <see cref="T:System.Windows.Media.DrawingContext" /> to render the control on.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="ctx" /> is null.</exception>
	protected sealed override void OnRender(DrawingContext ctx)
	{
		VerifyReentrancy();
		if (ctx == null)
		{
			throw new ArgumentNullException("ctx");
		}
		if (!IsLayoutDataValid)
		{
			return;
		}
		Brush background = Background;
		if (background != null)
		{
			ctx.DrawRectangle(background, null, new Rect(0.0, 0.0, base.RenderSize.Width, base.RenderSize.Height));
		}
		SetFlags(value: false, Flags.RequiresAlignment);
		SetFlags(value: true, Flags.TreeInReadOnlyMode);
		try
		{
			EnsureTextBlockCache();
			LineProperties lineProperties = _textBlockCache._lineProperties;
			double wrappingWidth = CalcWrappingWidth(base.RenderSize.Width);
			Vector vector = CalcContentOffset(base.RenderSize, wrappingWidth);
			Point lineOffset = new Point(vector.X, vector.Y);
			Line line = CreateLine(lineProperties);
			int num = 0;
			bool flag = false;
			SetFlags(CheckFlags(Flags.HasParagraphEllipses), Flags.RequiresAlignment);
			int lineCount = LineCount;
			for (int i = 0; i < lineCount; i++)
			{
				LineMetrics metrics = GetLine(i);
				double value = Math.Max(0.0, base.RenderSize.Height - Padding.Bottom);
				if (CheckFlags(Flags.HasParagraphEllipses) && i + 1 < lineCount)
				{
					double value2 = GetLine(i + 1).Height + metrics.Height + lineOffset.Y;
					flag = DoubleUtil.GreaterThan(value2, value) && !DoubleUtil.AreClose(value2, value);
				}
				if (CheckFlags(Flags.HasParagraphEllipses) && !DoubleUtil.LessThanOrClose(metrics.Height + lineOffset.Y, value) && i != 0)
				{
					continue;
				}
				using (line)
				{
					Format(line, metrics.Length, num, wrappingWidth, GetLineProperties(num == 0, flag, lineProperties), metrics.TextLineBreak, _textBlockCache._textRunCache, flag);
					if (!CheckFlags(Flags.HasParagraphEllipses))
					{
						metrics = UpdateLine(i, metrics, line.Start, line.Width);
					}
					line.Render(ctx, lineOffset);
					lineOffset.Y += metrics.Height;
					num += metrics.Length;
				}
			}
		}
		finally
		{
			SetFlags(value: false, Flags.TreeInReadOnlyMode);
			_textBlockCache = null;
		}
	}

	/// <summary>Called when the value one or more hosted dependency properties changes.</summary>
	/// <param name="e">Arguments for the associated event.</param>
	protected sealed override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if ((e.IsAValueChange || e.IsASubPropertyChange) && CheckFlags(Flags.FormattedOnce) && e.Metadata is FrameworkPropertyMetadata frameworkPropertyMetadata)
		{
			bool flag = frameworkPropertyMetadata.AffectsRender && (e.IsAValueChange || !frameworkPropertyMetadata.SubPropertiesDoNotAffectRender);
			if (frameworkPropertyMetadata.AffectsMeasure || frameworkPropertyMetadata.AffectsArrange || flag)
			{
				VerifyTreeIsUnlocked();
				_textBlockCache = null;
			}
		}
	}

	/// <summary>Returns a <see cref="T:System.Windows.Media.PointHitTestResult" /> for specified <see cref="T:System.Windows.Media.PointHitTestParameters" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.PointHitTestResult" /> for the specified hit test parameters.</returns>
	/// <param name="hitTestParameters">A <see cref="T:System.Windows.Media.PointHitTestParameters" /> object specifying the parameters to hit test for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="hitTestParameters" /> is null.</exception>
	protected sealed override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
	{
		VerifyReentrancy();
		if (hitTestParameters == null)
		{
			throw new ArgumentNullException("hitTestParameters");
		}
		Rect rect = new Rect(default(Point), base.RenderSize);
		if (rect.Contains(hitTestParameters.HitPoint))
		{
			return new PointHitTestResult(this, hitTestParameters.HitPoint);
		}
		return null;
	}

	/// <summary>Returns the <see cref="T:System.Windows.IInputElement" /> at a specified <see cref="T:System.Windows.Point" /> within the <see cref="T:System.Windows.Controls.TextBlock" />.</summary>
	/// <returns>The <see cref="T:System.Windows.IInputElement" /> found at the specified Point, or null if no such <see cref="T:System.Windows.IInputElement" /> can be found.</returns>
	/// <param name="point">A <see cref="T:System.Windows.Point" />, in the coordinate space of the <see cref="T:System.Windows.Controls.TextBlock" />, for which to return the corresponding <see cref="T:System.Windows.IInputElement" />.</param>
	protected virtual IInputElement InputHitTestCore(Point point)
	{
		if (!IsLayoutDataValid)
		{
			return this;
		}
		LineProperties lineProperties = GetLineProperties();
		IInputElement inputElement = null;
		double wrappingWidth = CalcWrappingWidth(base.RenderSize.Width);
		Vector vector = CalcContentOffset(base.RenderSize, wrappingWidth);
		point -= vector;
		if (point.X < 0.0 || point.Y < 0.0)
		{
			return this;
		}
		inputElement = null;
		int num = 0;
		double num2 = 0.0;
		TextRunCache textRunCache = new TextRunCache();
		int lineCount = LineCount;
		for (int i = 0; i < lineCount; i++)
		{
			LineMetrics line = GetLine(i);
			if (num2 + line.Height > point.Y)
			{
				Line line2 = CreateLine(lineProperties);
				using (line2)
				{
					bool ellipsis = ParagraphEllipsisShownOnLine(i, num2);
					Format(line2, line.Length, num, wrappingWidth, GetLineProperties(num == 0, lineProperties), line.TextLineBreak, textRunCache, ellipsis);
					if (line2.Start <= point.X && line2.Start + line2.Width >= point.X)
					{
						inputElement = line2.InputHitTest(point.X);
					}
				}
				break;
			}
			num += line.Length;
			num2 += line.Height;
		}
		if (inputElement == null)
		{
			return this;
		}
		return inputElement;
	}

	/// <summary>Returns a read-only collection of bounding rectangles for a specified <see cref="T:System.Windows.ContentElement" />.</summary>
	/// <returns>A read-only collection of bounding rectangles for the specified <see cref="T:System.Windows.ContentElement" />.</returns>
	/// <param name="child">A <see cref="T:System.Windows.ContentElement" /> for which to generate and return a collection of bounding rectangles.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="child" /> is null.</exception>
	protected virtual ReadOnlyCollection<Rect> GetRectanglesCore(ContentElement child)
	{
		if (child == null)
		{
			throw new ArgumentNullException("child");
		}
		if (!IsLayoutDataValid)
		{
			return new ReadOnlyCollection<Rect>(new List<Rect>(0));
		}
		LineProperties lineProperties = GetLineProperties();
		if (_complexContent == null || !(_complexContent.TextContainer is TextContainer))
		{
			return new ReadOnlyCollection<Rect>(new List<Rect>(0));
		}
		TextPointer textPointer = FindElementPosition(child);
		if (textPointer == null)
		{
			return new ReadOnlyCollection<Rect>(new List<Rect>(0));
		}
		TextPointer textPointer2 = null;
		if (child is TextElement)
		{
			textPointer2 = new TextPointer(((TextElement)child).ElementEnd);
		}
		else if (child is FrameworkContentElement)
		{
			textPointer2 = new TextPointer(textPointer);
			textPointer2.MoveByOffset(1);
		}
		if (textPointer2 == null)
		{
			return new ReadOnlyCollection<Rect>(new List<Rect>(0));
		}
		int offsetToPosition = _complexContent.TextContainer.Start.GetOffsetToPosition(textPointer);
		int offsetToPosition2 = _complexContent.TextContainer.Start.GetOffsetToPosition(textPointer2);
		int num = 0;
		int num2 = 0;
		double num3 = 0.0;
		int lineCount = LineCount;
		while (offsetToPosition >= num2 + GetLine(num).Length && num < lineCount)
		{
			num2 += GetLine(num).Length;
			num++;
			num3 += GetLine(num).Height;
		}
		int num4 = num2;
		List<Rect> list = new List<Rect>();
		double wrappingWidth = CalcWrappingWidth(base.RenderSize.Width);
		TextRunCache textRunCache = new TextRunCache();
		Vector vector = CalcContentOffset(base.RenderSize, wrappingWidth);
		do
		{
			LineMetrics line = GetLine(num);
			Line line2 = CreateLine(lineProperties);
			using (line2)
			{
				bool ellipsis = ParagraphEllipsisShownOnLine(num, num2);
				Format(line2, line.Length, num4, wrappingWidth, GetLineProperties(num == 0, lineProperties), line.TextLineBreak, textRunCache, ellipsis);
				if (line.Length == line2.Length)
				{
					int num5 = ((offsetToPosition >= num4) ? offsetToPosition : num4);
					int num6 = ((offsetToPosition2 < num4 + line.Length) ? offsetToPosition2 : (num4 + line.Length));
					double x = vector.X;
					double yOffset = vector.Y + num3;
					List<Rect> rangeBounds = line2.GetRangeBounds(num5, num6 - num5, x, yOffset);
					list.AddRange(rangeBounds);
				}
			}
			num4 += line.Length;
			num3 += line.Height;
			num++;
		}
		while (offsetToPosition2 > num4);
		Invariant.Assert(list != null);
		return new ReadOnlyCollection<Rect>(list);
	}

	/// <summary>Called when a child element deriving from <see cref="T:System.Windows.UIElement" /> changes its <see cref="P:System.Windows.UIElement.DesiredSize" />.</summary>
	/// <param name="child">The child <see cref="T:System.Windows.UIElement" /> element whose <see cref="P:System.Windows.UIElement.DesiredSize" /> has changed.</param>
	protected virtual void OnChildDesiredSizeChangedCore(UIElement child)
	{
		OnChildDesiredSizeChanged(child);
	}

	/// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Controls.TextBlock" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Controls.TextBlock" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new TextBlockAutomationPeer(this);
	}

	internal void RemoveChild(Visual child)
	{
		if (_complexContent != null)
		{
			_complexContent.VisualChildren.Remove(child);
		}
	}

	internal void SetTextContainer(ITextContainer textContainer)
	{
		if (_complexContent != null)
		{
			_complexContent.Detach(this);
			_complexContent = null;
			SetFlags(value: false, Flags.PendingTextContainerEventInit);
		}
		if (textContainer != null)
		{
			_complexContent = null;
			EnsureComplexContent(textContainer);
		}
		SetFlags(value: false, Flags.ContentChangeInProgress);
		InvalidateMeasure();
		InvalidateVisual();
	}

	internal Size MeasureChild(InlineObject inlineObject)
	{
		Size desiredSize;
		if (CheckFlags(Flags.MeasureInProgress))
		{
			Thickness padding = Padding;
			Size availableSize = new Size(Math.Max(0.0, _referenceSize.Width - (padding.Left + padding.Right)), Math.Max(0.0, _referenceSize.Height - (padding.Top + padding.Bottom)));
			inlineObject.Element.Measure(availableSize);
			desiredSize = inlineObject.Element.DesiredSize;
			ArrayList arrayList = InlineObjects;
			bool flag = false;
			if (arrayList == null)
			{
				arrayList = (InlineObjects = new ArrayList(1));
			}
			else
			{
				for (int i = 0; i < arrayList.Count; i++)
				{
					if (((InlineObject)arrayList[i]).Dcp == inlineObject.Dcp)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				arrayList.Add(inlineObject);
			}
		}
		else
		{
			desiredSize = inlineObject.Element.DesiredSize;
		}
		return desiredSize;
	}

	internal override string GetPlainText()
	{
		if (_complexContent != null)
		{
			return TextRangeBase.GetTextInternal(_complexContent.TextContainer.Start, _complexContent.TextContainer.End);
		}
		if (_contentCache != null)
		{
			return _contentCache;
		}
		return string.Empty;
	}

	internal ReadOnlyCollection<LineResult> GetLineResults()
	{
		Invariant.Assert(IsLayoutDataValid);
		if (CheckFlags(Flags.RequiresAlignment))
		{
			AlignContent();
		}
		double wrappingWidth = CalcWrappingWidth(base.RenderSize.Width);
		Vector vector = CalcContentOffset(base.RenderSize, wrappingWidth);
		int lineCount = LineCount;
		List<LineResult> list = new List<LineResult>(lineCount);
		int num = 0;
		double num2 = 0.0;
		for (int i = 0; i < lineCount; i++)
		{
			LineMetrics line = GetLine(i);
			list.Add(new TextLineResult(layoutBox: new Rect(vector.X + line.Start, vector.Y + num2, line.Width, line.Height), owner: this, dcp: num, cch: line.Length, baseline: line.Baseline, index: i));
			num2 += line.Height;
			num += line.Length;
		}
		return new ReadOnlyCollection<LineResult>(list);
	}

	internal void GetLineDetails(int dcp, int index, double lineVOffset, out int cchContent, out int cchEllipses)
	{
		Invariant.Assert(IsLayoutDataValid);
		Invariant.Assert(index >= 0 && index < LineCount);
		LineProperties lineProperties = GetLineProperties();
		double wrappingWidth = CalcWrappingWidth(base.RenderSize.Width);
		TextRunCache textRunCache = new TextRunCache();
		LineMetrics line = GetLine(index);
		using Line line2 = CreateLine(lineProperties);
		TextLineBreak textLineBreak = GetLine(index).TextLineBreak;
		bool ellipsis = ParagraphEllipsisShownOnLine(index, lineVOffset);
		Format(line2, line.Length, dcp, wrappingWidth, GetLineProperties(dcp == 0, lineProperties), textLineBreak, textRunCache, ellipsis);
		Invariant.Assert(line.Length == line2.Length, "Line length is out of sync");
		cchContent = line2.ContentLength;
		cchEllipses = line2.GetEllipsesLength();
	}

	internal ITextPointer GetTextPositionFromDistance(int dcp, double distance, double lineVOffset, int index)
	{
		Invariant.Assert(IsLayoutDataValid);
		LineProperties lineProperties = GetLineProperties();
		EnsureComplexContent();
		double wrappingWidth = CalcWrappingWidth(base.RenderSize.Width);
		Vector vector = CalcContentOffset(base.RenderSize, wrappingWidth);
		distance -= vector.X;
		lineVOffset -= vector.Y;
		TextRunCache textRunCache = new TextRunCache();
		LineMetrics line = GetLine(index);
		using Line line2 = CreateLine(lineProperties);
		Invariant.Assert(index >= 0 && index < LineCount);
		TextLineBreak textLineBreak = GetLine(index).TextLineBreak;
		bool ellipsis = ParagraphEllipsisShownOnLine(index, lineVOffset);
		Format(line2, line.Length, dcp, wrappingWidth, GetLineProperties(dcp == 0, lineProperties), textLineBreak, textRunCache, ellipsis);
		Invariant.Assert(line.Length == line2.Length, "Line length is out of sync");
		CharacterHit textPositionFromDistance = line2.GetTextPositionFromDistance(distance);
		LogicalDirection gravity = ((textPositionFromDistance.TrailingLength <= 0) ? LogicalDirection.Forward : LogicalDirection.Backward);
		return _complexContent.TextContainer.Start.CreatePointer(textPositionFromDistance.FirstCharacterIndex + textPositionFromDistance.TrailingLength, gravity);
	}

	internal Rect GetRectangleFromTextPosition(ITextPointer orientedPosition)
	{
		Invariant.Assert(IsLayoutDataValid);
		Invariant.Assert(orientedPosition != null);
		LineProperties lineProperties = GetLineProperties();
		EnsureComplexContent();
		int num = _complexContent.TextContainer.Start.GetOffsetToPosition(orientedPosition);
		int num2 = num;
		if (orientedPosition.LogicalDirection == LogicalDirection.Backward && num > 0)
		{
			num--;
		}
		double wrappingWidth = CalcWrappingWidth(base.RenderSize.Width);
		Vector vector = CalcContentOffset(base.RenderSize, wrappingWidth);
		double num3 = 0.0;
		int num4 = 0;
		TextRunCache textRunCache = new TextRunCache();
		Rect result = Rect.Empty;
		FlowDirection flowDirection = FlowDirection.LeftToRight;
		int lineCount = LineCount;
		for (int i = 0; i < lineCount; i++)
		{
			LineMetrics line = GetLine(i);
			if (num4 + line.Length > num || (num4 + line.Length == num && i == lineCount - 1))
			{
				using (Line line2 = CreateLine(lineProperties))
				{
					bool ellipsis = ParagraphEllipsisShownOnLine(i, num3);
					Format(line2, line.Length, num4, wrappingWidth, GetLineProperties(num4 == 0, lineProperties), line.TextLineBreak, textRunCache, ellipsis);
					Invariant.Assert(line.Length == line2.Length, "Line length is out of sync");
					result = line2.GetBoundsFromTextPosition(num, out flowDirection);
				}
				break;
			}
			num4 += line.Length;
			num3 += line.Height;
		}
		if (!result.IsEmpty)
		{
			result.X += vector.X;
			result.Y += vector.Y + num3;
			if (lineProperties.FlowDirection != flowDirection)
			{
				if (orientedPosition.LogicalDirection == LogicalDirection.Forward || num2 == 0)
				{
					result.X = result.Right;
				}
			}
			else if (orientedPosition.LogicalDirection == LogicalDirection.Backward && num2 > 0)
			{
				result.X = result.Right;
			}
			result.Width = 0.0;
		}
		return result;
	}

	internal Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition)
	{
		Invariant.Assert(IsLayoutDataValid);
		Invariant.Assert(startPosition != null);
		Invariant.Assert(endPosition != null);
		Invariant.Assert(startPosition.CompareTo(endPosition) <= 0);
		Geometry geometry = null;
		LineProperties lineProperties = GetLineProperties();
		EnsureComplexContent();
		int offsetToPosition = _complexContent.TextContainer.Start.GetOffsetToPosition(startPosition);
		int offsetToPosition2 = _complexContent.TextContainer.Start.GetOffsetToPosition(endPosition);
		double wrappingWidth = CalcWrappingWidth(base.RenderSize.Width);
		Vector vector = CalcContentOffset(base.RenderSize, wrappingWidth);
		TextRunCache textRunCache = new TextRunCache();
		Line line = CreateLine(lineProperties);
		int num = 0;
		ITextPointer textPointer = _complexContent.TextContainer.Start.CreatePointer(0);
		double num2 = 0.0;
		int lineCount = LineCount;
		int i = 0;
		for (int num3 = lineCount; i < num3; i++)
		{
			LineMetrics line2 = GetLine(i);
			if (offsetToPosition2 <= num)
			{
				break;
			}
			int num4 = num + line2.Length;
			textPointer.MoveByOffset(line2.Length);
			if (offsetToPosition < num4)
			{
				using (line)
				{
					bool ellipsis = ParagraphEllipsisShownOnLine(i, num2);
					Format(line, line2.Length, num, wrappingWidth, GetLineProperties(num == 0, lineProperties), line2.TextLineBreak, textRunCache, ellipsis);
					if (Invariant.Strict)
					{
						Invariant.Assert(GetLine(i).Length == line.Length, "Line length is out of sync");
					}
					int num5 = Math.Max(num, offsetToPosition);
					int num6 = Math.Min(num4, offsetToPosition2);
					if (num5 != num6)
					{
						IList<Rect> rangeBounds = line.GetRangeBounds(num5, num6 - num5, vector.X, vector.Y + num2);
						if (rangeBounds.Count > 0)
						{
							int num7 = 0;
							int count = rangeBounds.Count;
							do
							{
								Rect rect = rangeBounds[num7];
								if (num7 == count - 1 && offsetToPosition2 >= num4 && TextPointerBase.IsNextToAnyBreak(textPointer, LogicalDirection.Backward))
								{
									double num8 = FontSize * 0.5;
									rect.Width += num8;
								}
								RectangleGeometry addedGeometry = new RectangleGeometry(rect);
								CaretElement.AddGeometry(ref geometry, addedGeometry);
							}
							while (++num7 < count);
						}
					}
				}
			}
			num += line2.Length;
			num2 += line2.Height;
		}
		return geometry;
	}

	internal bool IsAtCaretUnitBoundary(ITextPointer position, int dcp, int lineIndex)
	{
		Invariant.Assert(IsLayoutDataValid);
		LineProperties lineProperties = GetLineProperties();
		EnsureComplexContent();
		TextRunCache textRunCache = new TextRunCache();
		bool flag = false;
		int offsetToPosition = _complexContent.TextContainer.Start.GetOffsetToPosition(position);
		CharacterHit charHit = default(CharacterHit);
		if (position.LogicalDirection == LogicalDirection.Backward)
		{
			if (offsetToPosition <= dcp)
			{
				return false;
			}
			charHit = new CharacterHit(offsetToPosition - 1, 1);
		}
		else if (position.LogicalDirection == LogicalDirection.Forward)
		{
			charHit = new CharacterHit(offsetToPosition, 0);
		}
		LineMetrics line = GetLine(lineIndex);
		double wrappingWidth = CalcWrappingWidth(base.RenderSize.Width);
		using Line line2 = CreateLine(lineProperties);
		Format(line2, line.Length, dcp, wrappingWidth, GetLineProperties(lineIndex == 0, lineProperties), line.TextLineBreak, textRunCache, ellipsis: false);
		Invariant.Assert(line.Length == line2.Length, "Line length is out of sync");
		return line2.IsAtCaretCharacterHit(charHit);
	}

	internal ITextPointer GetNextCaretUnitPosition(ITextPointer position, LogicalDirection direction, int dcp, int lineIndex)
	{
		Invariant.Assert(IsLayoutDataValid);
		LineProperties lineProperties = GetLineProperties();
		EnsureComplexContent();
		int offsetToPosition = _complexContent.TextContainer.Start.GetOffsetToPosition(position);
		if (offsetToPosition == dcp && direction == LogicalDirection.Backward)
		{
			if (lineIndex == 0)
			{
				return position;
			}
			lineIndex--;
			dcp -= GetLine(lineIndex).Length;
		}
		else if (offsetToPosition == dcp + GetLine(lineIndex).Length && direction == LogicalDirection.Forward)
		{
			int lineCount = LineCount;
			if (lineIndex == lineCount - 1)
			{
				return position;
			}
			dcp += GetLine(lineIndex).Length;
			lineIndex++;
		}
		TextRunCache textRunCache = new TextRunCache();
		double wrappingWidth = CalcWrappingWidth(base.RenderSize.Width);
		CharacterHit index = new CharacterHit(offsetToPosition, 0);
		LineMetrics line = GetLine(lineIndex);
		CharacterHit characterHit;
		using (Line line2 = CreateLine(lineProperties))
		{
			Format(line2, line.Length, dcp, wrappingWidth, GetLineProperties(lineIndex == 0, lineProperties), line.TextLineBreak, textRunCache, ellipsis: false);
			Invariant.Assert(line.Length == line2.Length, "Line length is out of sync");
			characterHit = ((direction != LogicalDirection.Forward) ? line2.GetPreviousCaretCharacterHit(index) : line2.GetNextCaretCharacterHit(index));
		}
		LogicalDirection gravity = ((characterHit.FirstCharacterIndex + characterHit.TrailingLength == dcp + GetLine(lineIndex).Length && direction == LogicalDirection.Forward) ? ((lineIndex != LineCount - 1) ? LogicalDirection.Forward : LogicalDirection.Backward) : (((characterHit.FirstCharacterIndex + characterHit.TrailingLength != dcp || direction != 0) ? (characterHit.TrailingLength <= 0) : (dcp == 0)) ? LogicalDirection.Forward : LogicalDirection.Backward));
		return _complexContent.TextContainer.Start.CreatePointer(characterHit.FirstCharacterIndex + characterHit.TrailingLength, gravity);
	}

	internal ITextPointer GetBackspaceCaretUnitPosition(ITextPointer position, int dcp, int lineIndex)
	{
		Invariant.Assert(IsLayoutDataValid);
		LineProperties lineProperties = GetLineProperties();
		EnsureComplexContent();
		int offsetToPosition = _complexContent.TextContainer.Start.GetOffsetToPosition(position);
		if (offsetToPosition == dcp)
		{
			if (lineIndex == 0)
			{
				return position;
			}
			lineIndex--;
			dcp -= GetLine(lineIndex).Length;
		}
		double wrappingWidth = CalcWrappingWidth(base.RenderSize.Width);
		CharacterHit index = new CharacterHit(offsetToPosition, 0);
		LineMetrics line = GetLine(lineIndex);
		TextRunCache textRunCache = new TextRunCache();
		CharacterHit backspaceCaretCharacterHit;
		using (Line line2 = CreateLine(lineProperties))
		{
			Format(line2, line.Length, dcp, wrappingWidth, GetLineProperties(lineIndex == 0, lineProperties), line.TextLineBreak, textRunCache, ellipsis: false);
			Invariant.Assert(line.Length == line2.Length, "Line length is out of sync");
			backspaceCaretCharacterHit = line2.GetBackspaceCaretCharacterHit(index);
		}
		LogicalDirection gravity = (((backspaceCaretCharacterHit.FirstCharacterIndex + backspaceCaretCharacterHit.TrailingLength != dcp) ? (backspaceCaretCharacterHit.TrailingLength <= 0) : (dcp == 0)) ? LogicalDirection.Forward : LogicalDirection.Backward);
		return _complexContent.TextContainer.Start.CreatePointer(backspaceCaretCharacterHit.FirstCharacterIndex + backspaceCaretCharacterHit.TrailingLength, gravity);
	}

	private static void OnTypographyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TextBlock)d).SetFlags(value: true, Flags.IsTypographySet);
	}

	private object OnValidateTextView(object arg)
	{
		if (IsLayoutDataValid && _complexContent != null)
		{
			_complexContent.TextView.OnUpdated();
		}
		return null;
	}

	private static void InsertTextRun(ITextPointer position, string text, bool whitespacesIgnorable)
	{
		if (!(position is TextPointer) || ((TextPointer)position).Parent == null || ((TextPointer)position).Parent is TextBox)
		{
			position.InsertTextInRun(text);
		}
		else if (!whitespacesIgnorable || text.Trim().Length > 0)
		{
			Run run = Inline.CreateImplicitRun(((TextPointer)position).Parent);
			((TextPointer)position).InsertTextElement(run);
			run.Text = text;
		}
	}

	private Line CreateLine(LineProperties lineProperties)
	{
		if (_complexContent == null)
		{
			return new SimpleLine(this, Text, lineProperties.DefaultTextRunProperties);
		}
		return new ComplexLine(this);
	}

	private void EnsureComplexContent()
	{
		EnsureComplexContent(null);
	}

	private void EnsureComplexContent(ITextContainer textContainer)
	{
		if (_complexContent != null)
		{
			return;
		}
		if (textContainer == null)
		{
			textContainer = new TextContainer(IsContentPresenterContainer ? null : this, plainTextOnly: false);
		}
		_complexContent = new ComplexContent(this, textContainer, foreignTextContianer: false, Text);
		_contentCache = null;
		if (CheckFlags(Flags.FormattedOnce))
		{
			Invariant.Assert(!CheckFlags(Flags.PendingTextContainerEventInit));
			InitializeTextContainerListeners();
			bool num = base.IsMeasureValid && base.IsArrangeValid;
			InvalidateMeasure();
			InvalidateVisual();
			if (num)
			{
				UpdateLayout();
			}
		}
		else
		{
			SetFlags(value: true, Flags.PendingTextContainerEventInit);
		}
	}

	private void ClearComplexContent()
	{
		if (_complexContent != null)
		{
			_complexContent.Detach(this);
			_complexContent = null;
			Invariant.Assert(_contentCache == null, "Content cache should be null when complex content exists.");
		}
	}

	private void OnHighlightChanged(object sender, HighlightChangedEventArgs args)
	{
		Invariant.Assert(args != null);
		Invariant.Assert(args.Ranges != null);
		Invariant.Assert(CheckFlags(Flags.FormattedOnce), "Unexpected Highlights.Changed callback before first format!");
		if (!(args.OwnerType != typeof(SpellerHighlightLayer)))
		{
			InvalidateVisual();
		}
	}

	private void OnTextContainerChanging(object sender, EventArgs args)
	{
		if (CheckFlags(Flags.FormattedOnce))
		{
			VerifyTreeIsUnlocked();
			SetFlags(value: true, Flags.ContentChangeInProgress);
		}
	}

	private void OnTextContainerChange(object sender, TextContainerChangeEventArgs args)
	{
		Invariant.Assert(args != null);
		if (_complexContent == null)
		{
			return;
		}
		Invariant.Assert(sender == _complexContent.TextContainer, "Received text change for foreign TextContainer.");
		if (args.Count == 0)
		{
			return;
		}
		if (CheckFlags(Flags.FormattedOnce))
		{
			VerifyTreeIsUnlocked();
			SetFlags(value: false, Flags.ContentChangeInProgress);
			InvalidateMeasure();
		}
		if (CheckFlags(Flags.TextContentChanging) || args.TextChange == TextChangeType.PropertyModified)
		{
			return;
		}
		SetFlags(value: true, Flags.TextContentChanging);
		try
		{
			SetDeferredValue(TextProperty, new DeferredTextReference(TextContainer));
		}
		finally
		{
			SetFlags(value: false, Flags.TextContentChanging);
		}
	}

	private void EnsureTextBlockCache()
	{
		if (_textBlockCache == null)
		{
			_textBlockCache = new TextBlockCache();
			_textBlockCache._lineProperties = GetLineProperties();
			_textBlockCache._textRunCache = new TextRunCache();
		}
	}

	private LineProperties GetLineProperties()
	{
		TextProperties defaultTextProperties = new TextProperties(this, IsTypographyDefaultValue);
		LineProperties lineProperties = new LineProperties(this, this, defaultTextProperties, null);
		if ((bool)GetValue(IsHyphenationEnabledProperty))
		{
			lineProperties.Hyphenator = EnsureHyphenator();
		}
		return lineProperties;
	}

	private TextParagraphProperties GetLineProperties(bool firstLine, LineProperties lineProperties)
	{
		return GetLineProperties(firstLine, showParagraphEllipsis: false, lineProperties);
	}

	private TextParagraphProperties GetLineProperties(bool firstLine, bool showParagraphEllipsis, LineProperties lineProperties)
	{
		GetLineProperties();
		firstLine = firstLine && lineProperties.HasFirstLineProperties;
		if (!showParagraphEllipsis)
		{
			if (!firstLine)
			{
				return lineProperties;
			}
			return lineProperties.FirstLineProps;
		}
		return lineProperties.GetParaEllipsisLineProps(firstLine);
	}

	private double CalcLineAdvance(double lineHeight, LineProperties lineProperties)
	{
		return lineProperties.CalcLineAdvance(lineHeight);
	}

	private Vector CalcContentOffset(Size computedSize, double wrappingWidth)
	{
		Vector result = default(Vector);
		Thickness padding = Padding;
		Size size = new Size(Math.Max(0.0, computedSize.Width - (padding.Left + padding.Right)), Math.Max(0.0, computedSize.Height - (padding.Top + padding.Bottom)));
		switch (TextAlignment)
		{
		case TextAlignment.Right:
			result.X = size.Width - wrappingWidth;
			break;
		case TextAlignment.Center:
			result.X = (size.Width - wrappingWidth) / 2.0;
			break;
		}
		result.X += padding.Left;
		result.Y += padding.Top;
		return result;
	}

	private bool ParagraphEllipsisShownOnLine(int lineIndex, double lineVOffset)
	{
		if (lineIndex >= LineCount - 1)
		{
			return false;
		}
		if (!CheckFlags(Flags.HasParagraphEllipses))
		{
			return false;
		}
		double value = GetLine(lineIndex + 1).Height + GetLine(lineIndex).Height + lineVOffset;
		double value2 = Math.Max(0.0, base.RenderSize.Height - Padding.Bottom);
		if (DoubleUtil.GreaterThan(value, value2) && !DoubleUtil.AreClose(value, value2))
		{
			return true;
		}
		return false;
	}

	private double CalcWrappingWidth(double width)
	{
		if (width < _previousDesiredSize.Width)
		{
			width = _previousDesiredSize.Width;
		}
		if (width > _referenceSize.Width)
		{
			width = _referenceSize.Width;
		}
		bool num = DoubleUtil.AreClose(width, _referenceSize.Width);
		double num2 = Padding.Left + Padding.Right;
		width = Math.Max(0.0, width - num2);
		if (!num && width != 0.0)
		{
			if (TextOptions.GetTextFormattingMode(this) == TextFormattingMode.Display)
			{
				width += 0.5 / GetDpi().DpiScaleY;
			}
			if (num2 != 0.0)
			{
				width += 1E-11;
			}
		}
		TextDpi.EnsureValidLineWidth(ref width);
		return width;
	}

	private void Format(Line line, int length, int dcp, double wrappingWidth, TextParagraphProperties paragraphProperties, TextLineBreak textLineBreak, TextRunCache textRunCache, bool ellipsis)
	{
		line.Format(dcp, wrappingWidth, paragraphProperties, textLineBreak, textRunCache, ellipsis);
		if (line.Length >= length)
		{
			return;
		}
		double width = _referenceSize.Width;
		double num = wrappingWidth;
		TextDpi.EnsureValidLineWidth(ref width);
		double num2 = 0.01;
		while (true)
		{
			double num3 = num + num2;
			if (num3 > width)
			{
				break;
			}
			line.Format(dcp, num3, paragraphProperties, textLineBreak, textRunCache, ellipsis);
			if (line.Length < length)
			{
				num = num3;
				num2 *= 2.0;
				continue;
			}
			width = num3;
			break;
		}
		for (double num4 = (width - num) / 2.0; num4 > 0.01; num4 /= 2.0)
		{
			double num5 = num + num4;
			line.Format(dcp, num5, paragraphProperties, textLineBreak, textRunCache, ellipsis);
			if (line.Length < length)
			{
				num = num5;
			}
			else
			{
				width = num5;
			}
		}
		line.Format(dcp, width, paragraphProperties, textLineBreak, textRunCache, ellipsis);
	}

	private void VerifyTreeIsUnlocked()
	{
		if (CheckFlags(Flags.TreeInReadOnlyMode))
		{
			throw new InvalidOperationException(SR.IllegalTreeChangeDetected);
		}
	}

	/// <summary>Returns a value that indicates whether the effective value of the <see cref="P:System.Windows.Controls.TextBlock.Text" /> property should be serialized during serialization of a <see cref="T:System.Windows.Controls.TextBlock" /> object.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.TextBlock.Text" /> property should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeText()
	{
		bool result = false;
		if (_complexContent == null)
		{
			object obj = ReadLocalValue(TextProperty);
			if (obj != null && obj != DependencyProperty.UnsetValue && obj as string != string.Empty)
			{
				result = true;
			}
		}
		return result;
	}

	/// <summary>Returns a value that indicates whether the effective value of the <see cref="P:System.Windows.Controls.TextBlock.Inlines" /> property should be serialized during serialization of a <see cref="T:System.Windows.Controls.TextBlock" /> object.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.TextBlock.Inlines" /> property should be serialized; otherwise, false.</returns>
	/// <param name="manager">A serialization service manager object for this object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="manager" /> is null.</exception>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeInlines(XamlDesignerSerializationManager manager)
	{
		if (_complexContent != null && manager != null)
		{
			return manager.XmlWriter == null;
		}
		return false;
	}

	private void AlignContent()
	{
		LineProperties lineProperties = GetLineProperties();
		double wrappingWidth = CalcWrappingWidth(base.RenderSize.Width);
		CalcContentOffset(base.RenderSize, wrappingWidth);
		Line line = CreateLine(lineProperties);
		TextRunCache textRunCache = new TextRunCache();
		int num = 0;
		double num2 = 0.0;
		int lineCount = LineCount;
		for (int i = 0; i < lineCount; i++)
		{
			LineMetrics line2 = GetLine(i);
			using (line)
			{
				bool ellipsis = ParagraphEllipsisShownOnLine(i, num2);
				Format(line, line2.Length, num, wrappingWidth, GetLineProperties(num == 0, lineProperties), line2.TextLineBreak, textRunCache, ellipsis);
				double num3 = CalcLineAdvance(line.Height, lineProperties);
				Invariant.Assert(line2.Length == line.Length, "Line length is out of sync");
				num += UpdateLine(i, line2, line.Start, line.Width).Length;
				num2 += num3;
			}
		}
		SetFlags(value: false, Flags.RequiresAlignment);
	}

	private static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs args)
	{
		TextBlock textBlock = sender as TextBlock;
		ContentElement contentElement = args.TargetObject as ContentElement;
		if (textBlock != null && contentElement != null && ContainsContentElement(textBlock, contentElement))
		{
			args.Handled = true;
			ReadOnlyCollection<Rect> rectanglesCore = textBlock.GetRectanglesCore(contentElement);
			Invariant.Assert(rectanglesCore != null, "Rect collection cannot be null.");
			if (rectanglesCore.Count > 0)
			{
				textBlock.BringIntoView(rectanglesCore[0]);
			}
			else
			{
				textBlock.BringIntoView();
			}
		}
	}

	private static bool ContainsContentElement(TextBlock textBlock, ContentElement element)
	{
		if (textBlock._complexContent == null || !(textBlock._complexContent.TextContainer is TextContainer))
		{
			return false;
		}
		if (element is TextElement)
		{
			if (textBlock._complexContent.TextContainer != ((TextElement)element).TextContainer)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private LineMetrics GetLine(int index)
	{
		if (index != 0)
		{
			return _subsequentLines[index - 1];
		}
		return _firstLine;
	}

	private LineMetrics UpdateLine(int index, LineMetrics metrics, double start, double width)
	{
		metrics = new LineMetrics(metrics, start, width);
		if (index == 0)
		{
			_firstLine = metrics;
		}
		else
		{
			_subsequentLines[index - 1] = metrics;
		}
		return metrics;
	}

	private void SetFlags(bool value, Flags flags)
	{
		_flags = (value ? (_flags | flags) : (_flags & ~flags));
	}

	private bool CheckFlags(Flags flags)
	{
		return (_flags & flags) == flags;
	}

	private void VerifyReentrancy()
	{
		if (CheckFlags(Flags.MeasureInProgress))
		{
			throw new InvalidOperationException(SR.MeasureReentrancyInvalid);
		}
		if (CheckFlags(Flags.ArrangeInProgress))
		{
			throw new InvalidOperationException(SR.ArrangeReentrancyInvalid);
		}
		if (CheckFlags(Flags.ContentChangeInProgress))
		{
			throw new InvalidOperationException(SR.TextContainerChangingReentrancyInvalid);
		}
	}

	private int GetLineIndexFromDcp(int dcpLine)
	{
		Invariant.Assert(dcpLine >= 0);
		int i = 0;
		int num = 0;
		for (int lineCount = LineCount; i < lineCount; i++)
		{
			if (num == dcpLine)
			{
				return i;
			}
			num += GetLine(i).Length;
		}
		Invariant.Assert(condition: false, "Dcp passed is not at start of any line in TextBlock");
		return -1;
	}

	private TextPointer FindElementPosition(IInputElement e)
	{
		if (e is TextElement && (e as TextElement).TextContainer == _complexContent.TextContainer)
		{
			return new TextPointer((e as TextElement).ElementStart);
		}
		TextPointer textPointer = new TextPointer((TextPointer)_complexContent.TextContainer.Start);
		while (textPointer.CompareTo((TextPointer)_complexContent.TextContainer.End) < 0)
		{
			if (textPointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.EmbeddedElement)
			{
				DependencyObject adjacentElement = textPointer.GetAdjacentElement(LogicalDirection.Forward);
				if ((adjacentElement is ContentElement || adjacentElement is UIElement) && (adjacentElement == e as ContentElement || adjacentElement == e as UIElement))
				{
					return textPointer;
				}
			}
			textPointer.MoveByOffset(1);
		}
		return null;
	}

	internal void OnChildBaselineOffsetChanged(DependencyObject source)
	{
		if (!CheckFlags(Flags.MeasureInProgress))
		{
			InvalidateMeasure();
			InvalidateVisual();
		}
	}

	private static void OnBaselineOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TextElement value = TextElement.ContainerTextElementField.GetValue(d);
		if (value != null)
		{
			DependencyObject parent = value.TextContainer.Parent;
			if (parent is TextBlock textBlock)
			{
				textBlock.OnChildBaselineOffsetChanged(d);
			}
			else if (parent is FlowDocument flowDocument && d is UIElement)
			{
				flowDocument.OnChildDesiredSizeChanged((UIElement)d);
			}
		}
	}

	private void InitializeTextContainerListeners()
	{
		_complexContent.TextContainer.Changing += OnTextContainerChanging;
		_complexContent.TextContainer.Change += OnTextContainerChange;
		_complexContent.Highlights.Changed += OnHighlightChanged;
	}

	private void ClearLineMetrics()
	{
		if (!CheckFlags(Flags.HasFirstLine))
		{
			return;
		}
		if (_subsequentLines != null)
		{
			int count = _subsequentLines.Count;
			for (int i = 0; i < count; i++)
			{
				_subsequentLines[i].Dispose(returnUpdatedMetrics: false);
			}
			_subsequentLines = null;
		}
		_firstLine = _firstLine.Dispose(returnUpdatedMetrics: true);
		SetFlags(value: false, Flags.HasFirstLine);
	}

	private NaturalLanguageHyphenator EnsureHyphenator()
	{
		if (CheckFlags(Flags.IsHyphenatorSet))
		{
			return HyphenatorField.GetValue(this);
		}
		NaturalLanguageHyphenator naturalLanguageHyphenator = new NaturalLanguageHyphenator();
		HyphenatorField.SetValue(this, naturalLanguageHyphenator);
		SetFlags(value: true, Flags.IsHyphenatorSet);
		return naturalLanguageHyphenator;
	}

	private static bool IsValidTextTrimming(object o)
	{
		TextTrimming textTrimming = (TextTrimming)o;
		if (textTrimming != TextTrimming.CharacterEllipsis && textTrimming != 0)
		{
			return textTrimming == TextTrimming.WordEllipsis;
		}
		return true;
	}

	private static bool IsValidTextWrap(object o)
	{
		TextWrapping textWrapping = (TextWrapping)o;
		if (textWrapping != TextWrapping.Wrap && textWrapping != TextWrapping.NoWrap)
		{
			return textWrapping == TextWrapping.WrapWithOverflow;
		}
		return true;
	}

	private static object CoerceBaselineOffset(DependencyObject d, object value)
	{
		TextBlock textBlock = (TextBlock)d;
		if (double.IsNaN((double)value))
		{
			return textBlock._baselineOffset;
		}
		return value;
	}

	/// <summary>Returns a value that indicates whether the effective value of the <see cref="P:System.Windows.Controls.TextBlock.BaselineOffset" /> property should be serialized during serialization of a <see cref="T:System.Windows.Controls.TextBlock" /> object.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.TextBlock.BaselineOffset" /> property should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeBaselineOffset()
	{
		object obj = ReadLocalValue(BaselineOffsetProperty);
		if (obj != DependencyProperty.UnsetValue)
		{
			return !double.IsNaN((double)obj);
		}
		return false;
	}

	private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		OnTextChanged(d, (string)e.NewValue);
	}

	private static void OnTextChanged(DependencyObject d, string newText)
	{
		TextBlock textBlock = (TextBlock)d;
		if (textBlock.CheckFlags(Flags.TextContentChanging))
		{
			return;
		}
		if (textBlock._complexContent == null)
		{
			textBlock._contentCache = ((newText != null) ? newText : string.Empty);
			return;
		}
		textBlock.SetFlags(value: true, Flags.TextContentChanging);
		try
		{
			bool flag = true;
			Invariant.Assert(textBlock._contentCache == null, "Content cache should be null when complex content exists.");
			textBlock._complexContent.TextContainer.BeginChange();
			try
			{
				((TextContainer)textBlock._complexContent.TextContainer).DeleteContentInternal((TextPointer)textBlock._complexContent.TextContainer.Start, (TextPointer)textBlock._complexContent.TextContainer.End);
				InsertTextRun(textBlock._complexContent.TextContainer.End, newText, whitespacesIgnorable: true);
				flag = false;
			}
			finally
			{
				textBlock._complexContent.TextContainer.EndChange();
				if (flag)
				{
					textBlock.ClearLineMetrics();
				}
			}
		}
		finally
		{
			textBlock.SetFlags(value: false, Flags.TextContentChanging);
		}
	}
}
