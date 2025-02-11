using System.Collections;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Documents;
using MS.Internal.FontCache;
using MS.Internal.PtsHost;
using MS.Internal.Telemetry.PresentationFramework;
using MS.Internal.Text;

namespace System.Windows.Documents;

/// <summary>Hosts and formats flow content with advanced document features, such as pagination and columns.</summary>
[Localizability(LocalizationCategory.Inherit, Readability = Readability.Unreadable)]
[ContentProperty("Blocks")]
public class FlowDocument : FrameworkContentElement, IDocumentPaginatorSource, IServiceProvider, IAddChild
{
	private static readonly Type _typeofThis;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.FontFamily" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.FontFamily" /> dependency property.</returns>
	public static readonly DependencyProperty FontFamilyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.FontStyle" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.FontStyle" /> dependency property.</returns>
	public static readonly DependencyProperty FontStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.FontWeight" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.FontWeight" /> dependency property.</returns>
	public static readonly DependencyProperty FontWeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.FontStretch" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.FontStretch" /> dependency property.</returns>
	public static readonly DependencyProperty FontStretchProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.FontSize" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.FontSize" /> dependency property.</returns>
	public static readonly DependencyProperty FontSizeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.Foreground" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.Foreground" /> dependency property.</returns>
	public static readonly DependencyProperty ForegroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.Background" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.Background" /> dependency property.</returns>
	public static readonly DependencyProperty BackgroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.TextEffects" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.TextEffects" /> dependency property.</returns>
	public static readonly DependencyProperty TextEffectsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.TextAlignment" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.TextAlignment" /> dependency property.</returns>
	public static readonly DependencyProperty TextAlignmentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.FlowDirection" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.FlowDirection" /> dependency property.</returns>
	public static readonly DependencyProperty FlowDirectionProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.LineHeight" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.LineHeight" /> dependency property.</returns>
	public static readonly DependencyProperty LineHeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.LineStackingStrategy" />  dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.LineStackingStrategy" /> dependency property.</returns>
	public static readonly DependencyProperty LineStackingStrategyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.ColumnWidth" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.ColumnWidth" /> dependency property.</returns>
	public static readonly DependencyProperty ColumnWidthProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.ColumnGap" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.ColumnGap" /> dependency property.</returns>
	public static readonly DependencyProperty ColumnGapProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.IsColumnWidthFlexible" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.IsColumnWidthFlexible" /> dependency property.</returns>
	public static readonly DependencyProperty IsColumnWidthFlexibleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.ColumnRuleWidth" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.ColumnRuleWidth" /> dependency property.</returns>
	public static readonly DependencyProperty ColumnRuleWidthProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.ColumnRuleBrush" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.ColumnRuleBrush" /> dependency property.</returns>
	public static readonly DependencyProperty ColumnRuleBrushProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.IsOptimalParagraphEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.IsOptimalParagraphEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsOptimalParagraphEnabledProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.PageWidth" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.PageWidth" /> dependency property.</returns>
	public static readonly DependencyProperty PageWidthProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.MinPageWidth" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.MinPageWidth" /> dependency property.</returns>
	public static readonly DependencyProperty MinPageWidthProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.MaxPageWidth" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.MaxPageWidth" /> dependency property.</returns>
	public static readonly DependencyProperty MaxPageWidthProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.PageHeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.PageHeight" /> dependency property.</returns>
	public static readonly DependencyProperty PageHeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.MinPageHeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.MinPageHeight" /> dependency property.</returns>
	public static readonly DependencyProperty MinPageHeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.MaxPageHeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.MaxPageHeight" /> dependency property.</returns>
	public static readonly DependencyProperty MaxPageHeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.PagePadding" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.PagePadding" /> dependency property.</returns>
	public static readonly DependencyProperty PagePaddingProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FlowDocument.IsHyphenationEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FlowDocument.IsHyphenationEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsHyphenationEnabledProperty;

	private StructuralCache _structuralCache;

	private TypographyProperties _typographyPropertiesGroup;

	private IFlowDocumentFormatter _formatter;

	private TextWrapping _textWrapping = TextWrapping.Wrap;

	private double _pixelsPerDip = Util.PixelsPerDip;

	/// <summary>Gets the top-level <see cref="T:System.Windows.Documents.Block" /> elements of the contents of the <see cref="T:System.Windows.Documents.FlowDocument" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.BlockCollection" /> containing the <see cref="T:System.Windows.Documents.Block" /> elements that make up the contents of the <see cref="T:System.Windows.Documents.FlowDocument" />.  </returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public BlockCollection Blocks => new BlockCollection(this, isOwnerParent: true);

	internal TextRange TextRange => new TextRange(ContentStart, ContentEnd);

	/// <summary>Gets a <see cref="T:System.Windows.Documents.TextPointer" /> that represents the start of content within a <see cref="T:System.Windows.Documents.FlowDocument" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointerContext" /> representing the start of the contents in the <see cref="T:System.Windows.Documents.FlowDocument" />.</returns>
	public TextPointer ContentStart => _structuralCache.TextContainer.Start;

	/// <summary>Gets a <see cref="T:System.Windows.Documents.TextPointer" /> that represents the end of the content in a <see cref="T:System.Windows.Documents.FlowDocument" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TextPointer" /> representing the end of the contents in the <see cref="T:System.Windows.Documents.FlowDocument" />.</returns>
	public TextPointer ContentEnd => _structuralCache.TextContainer.End;

	/// <summary>Gets or sets the preferred top-level font family for the <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.FontFamily" /> object specifying the preferred font family, or a primary preferred font family with one or more fallback font families. The default is the font determined by the <see cref="P:System.Windows.SystemFonts.MessageFontFamily" /> value.</returns>
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

	/// <summary>Gets or sets the top-level font style for the <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>A member of the <see cref="T:System.Windows.FontStyles" /> class that specifies the desired font style. The default is determined by the <see cref="P:System.Windows.SystemFonts.MessageFontStyle" /> value.</returns>
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

	/// <summary>Gets or sets the top-level font weight for the <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>A member of the <see cref="T:System.Windows.FontWeights" /> class that specifies the desired font weight. The default is determined by the <see cref="P:System.Windows.SystemFonts.MessageFontWeight" /> value.</returns>
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

	/// <summary>Gets or sets the top-level font-stretching characteristics for the <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>A member of the <see cref="T:System.Windows.FontStretch" /> class that specifies the desired font-stretching characteristics to use. The default is <see cref="P:System.Windows.FontStretches.Normal" />.</returns>
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

	/// <summary>Gets or sets the top-level font size for the <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>The desired font size to use, in device independent pixels).   The default is determined by the <see cref="P:System.Windows.SystemFonts.MessageFontSize" /> value.</returns>
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

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> to apply to the text contents of the <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
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

	/// <summary>Gets or sets the effects to apply to the text of a <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextEffectCollection" /> containing one or more <see cref="T:System.Windows.Media.TextEffect" /> objects that define effects to apply to the text of the <see cref="T:System.Windows.Documents.FlowDocument" />. The default is null (no effects applied).</returns>
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

	/// <summary>Gets or sets the relative direction for flow of content in a <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.FlowDirection" /> values that specifies the relative flow direction. The default is <see cref="F:System.Windows.FlowDirection.LeftToRight" />.</returns>
	public FlowDirection FlowDirection
	{
		get
		{
			return (FlowDirection)GetValue(FlowDirectionProperty);
		}
		set
		{
			SetValue(FlowDirectionProperty, value);
		}
	}

	/// <summary>Gets or sets the height of each line of content.  </summary>
	/// <returns>The height of each line, in device independent pixels, in the range 0.0034 to 160000. A value of <see cref="F:System.Double.NaN" /> (equivalent to an attribute value of "Auto") causes the line height to be determined automatically from the current font characteristics. The default is <see cref="F:System.Double.NaN" />.</returns>
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

	/// <summary>Gets or sets the mechanism by which a line box is determined for each line of text within the <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.LineStackingStrategy" /> values that specifies the mechanism by which a line box is determined for each line of text in the <see cref="T:System.Windows.Documents.FlowDocument" />. The default is <see cref="F:System.Windows.LineStackingStrategy.MaxHeight" />.</returns>
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

	/// <summary>Gets or sets the minimum desired width of the columns in a <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>The minimum desired column width, in device independent pixels. A value of <see cref="F:System.Double.NaN" /> causes only one column to be displayed, regardless of the page width.  The default is <see cref="F:System.Double.NaN" />.</returns>
	[TypeConverter(typeof(LengthConverter))]
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
	public double ColumnWidth
	{
		get
		{
			return (double)GetValue(ColumnWidthProperty);
		}
		set
		{
			SetValue(ColumnWidthProperty, value);
		}
	}

	/// <summary>Gets or sets the column gap value, which indicates the spacing between columns in a <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>The column gap, in device independent pixels.  A value of <see cref="F:System.Double.NaN" /> (equivalent to an attribute value of "Auto") indicates that the column gap is equal to the <see cref="P:System.Windows.Documents.FlowDocument.LineHeight" /> property. The default is <see cref="F:System.Double.NaN" />.</returns>
	[TypeConverter(typeof(LengthConverter))]
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
	public double ColumnGap
	{
		get
		{
			return (double)GetValue(ColumnGapProperty);
		}
		set
		{
			SetValue(ColumnGapProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="P:System.Windows.Documents.FlowDocument.ColumnWidth" /> value is flexible or fixed.  </summary>
	/// <returns>true if the column width is flexible; false if the column width is fixed. The default is true.</returns>
	public bool IsColumnWidthFlexible
	{
		get
		{
			return (bool)GetValue(IsColumnWidthFlexibleProperty);
		}
		set
		{
			SetValue(IsColumnWidthFlexibleProperty, value);
		}
	}

	/// <summary>Gets or sets the column rule width.  </summary>
	/// <returns>The column rule width, in device independent pixels. The default is 0.0.</returns>
	[TypeConverter(typeof(LengthConverter))]
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
	public double ColumnRuleWidth
	{
		get
		{
			return (double)GetValue(ColumnRuleWidthProperty);
		}
		set
		{
			SetValue(ColumnRuleWidthProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> used to draw the rule between columns.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Brush" /> to use when drawing the rule line between columns, or null to not use a background brush. The default is null.</returns>
	public Brush ColumnRuleBrush
	{
		get
		{
			return (Brush)GetValue(ColumnRuleBrushProperty);
		}
		set
		{
			SetValue(ColumnRuleBrushProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether optimal paragraph layout is enabled or disabled.  </summary>
	/// <returns>true if optimal paragraph layout is enabled; otherwise, false. The default is false.</returns>
	public bool IsOptimalParagraphEnabled
	{
		get
		{
			return (bool)GetValue(IsOptimalParagraphEnabledProperty);
		}
		set
		{
			SetValue(IsOptimalParagraphEnabledProperty, value);
		}
	}

	/// <summary>Gets or sets the preferred width for pages in a <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>The preferred width, in device independent pixels, for pages in the <see cref="T:System.Windows.Documents.FlowDocument" />. A value of <see cref="F:System.Double.NaN" /> (equivalent to an attribute value of "Auto") causes the page width to be determined automatically. The default is <see cref="F:System.Double.NaN" />.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double PageWidth
	{
		get
		{
			return (double)GetValue(PageWidthProperty);
		}
		set
		{
			SetValue(PageWidthProperty, value);
		}
	}

	/// <summary>Gets or sets the minimum width for pages in a <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>The minimum width, in device independent pixels, for pages in the <see cref="T:System.Windows.Documents.FlowDocument" />. The default is 0.0.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double MinPageWidth
	{
		get
		{
			return (double)GetValue(MinPageWidthProperty);
		}
		set
		{
			SetValue(MinPageWidthProperty, value);
		}
	}

	/// <summary>Gets or sets the maximum width for pages in a <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>The maximum width, in device independent pixels, for pages in the <see cref="T:System.Windows.Documents.FlowDocument" />. The default is <see cref="F:System.Double.PositiveInfinity" /> (no maximum page width).</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double MaxPageWidth
	{
		get
		{
			return (double)GetValue(MaxPageWidthProperty);
		}
		set
		{
			SetValue(MaxPageWidthProperty, value);
		}
	}

	/// <summary>Gets or sets the preferred height for pages in a <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>The preferred height, in device independent pixels, for pages in the <see cref="T:System.Windows.Documents.FlowDocument" />. A value of <see cref="F:System.Double.NaN" /> (equivalent to an attribute value of "Auto") causes the page height to be determined automatically. The default is <see cref="F:System.Double.NaN" />.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double PageHeight
	{
		get
		{
			return (double)GetValue(PageHeightProperty);
		}
		set
		{
			SetValue(PageHeightProperty, value);
		}
	}

	/// <summary>Gets or sets the minimum height for pages in a <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>The minimum height, in device independent pixels, for pages in the <see cref="T:System.Windows.Documents.FlowDocument" />. The default is 0.0.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double MinPageHeight
	{
		get
		{
			return (double)GetValue(MinPageHeightProperty);
		}
		set
		{
			SetValue(MinPageHeightProperty, value);
		}
	}

	/// <summary>Gets or sets the maximum height for pages in a <see cref="T:System.Windows.Documents.FlowDocument" />.  </summary>
	/// <returns>The maximum height, in device independent pixels, for pages in the <see cref="T:System.Windows.Documents.FlowDocument" />. The default is <see cref="F:System.Double.PositiveInfinity" /> (no maximum page height).</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double MaxPageHeight
	{
		get
		{
			return (double)GetValue(MaxPageHeightProperty);
		}
		set
		{
			SetValue(MaxPageHeightProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates the thickness of padding space between the boundaries of a page and the page's content.  </summary>
	/// <returns>A <see cref="T:System.Windows.Thickness" /> structure that specifies the amount of padding to apply, in device independent pixels. The default is a uniform thickness of zero (0.0).</returns>
	public Thickness PagePadding
	{
		get
		{
			return (Thickness)GetValue(PagePaddingProperty);
		}
		set
		{
			SetValue(PagePaddingProperty, value);
		}
	}

	/// <summary>Gets the currently effective typography variations for the text contents of the <see cref="T:System.Windows.Documents.FlowDocument" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.Typography" /> object that specifies the currently effective typography variations. For a list of default typography values, see <see cref="T:System.Windows.Documents.Typography" />.</returns>
	public Typography Typography => new Typography(this);

	/// <summary>Gets or sets a value that indicates whether automatic hyphenation of words is enabled or disabled.  </summary>
	/// <returns>true if automatic breaking and hyphenation of words is enabled; otherwise, false. The default is false.</returns>
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

	/// <summary>Gets an enumerator that can iterate the logical children of the <see cref="T:System.Windows.Documents.FlowDocument" />. </summary>
	/// <returns>An enumerator for the logical children.</returns>
	protected internal override IEnumerator LogicalChildren => new RangeContentEnumerator(_structuralCache.TextContainer.Start, _structuralCache.TextContainer.End);

	/// <summary>Gets the value of the <see cref="P:System.Windows.ContentElement.IsEnabled" /> property for the <see cref="T:System.Windows.Documents.FlowDocument" />.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.ContentElement.IsEnabled" /> property for the <see cref="T:System.Windows.Documents.FlowDocument" />.</returns>
	protected override bool IsEnabledCore
	{
		get
		{
			if (!base.IsEnabledCore)
			{
				return false;
			}
			if (base.Parent is RichTextBox richTextBox)
			{
				return richTextBox.IsDocumentEnabled;
			}
			return true;
		}
	}

	internal FlowDocumentFormatter BottomlessFormatter
	{
		get
		{
			if (_formatter != null && !(_formatter is FlowDocumentFormatter))
			{
				_formatter.Suspend();
				_formatter = null;
			}
			if (_formatter == null)
			{
				_formatter = new FlowDocumentFormatter(this);
			}
			return (FlowDocumentFormatter)_formatter;
		}
	}

	internal StructuralCache StructuralCache => _structuralCache;

	internal TypographyProperties TypographyPropertiesGroup
	{
		get
		{
			if (_typographyPropertiesGroup == null)
			{
				_typographyPropertiesGroup = TextElement.GetTypographyProperties(this);
			}
			return _typographyPropertiesGroup;
		}
	}

	internal TextWrapping TextWrapping
	{
		get
		{
			return _textWrapping;
		}
		set
		{
			_textWrapping = value;
		}
	}

	internal IFlowDocumentFormatter Formatter => _formatter;

	internal bool IsLayoutDataValid
	{
		get
		{
			if (_formatter != null)
			{
				return _formatter.IsLayoutDataValid;
			}
			return false;
		}
	}

	internal TextContainer TextContainer => _structuralCache.TextContainer;

	internal double PixelsPerDip
	{
		get
		{
			return _pixelsPerDip;
		}
		set
		{
			_pixelsPerDip = value;
		}
	}

	/// <summary>Defines the source object that performs actual content pagination.</summary>
	/// <returns>The object that performs the actual content pagination.</returns>
	DocumentPaginator IDocumentPaginatorSource.DocumentPaginator
	{
		get
		{
			if (_formatter != null && !(_formatter is FlowDocumentPaginator))
			{
				_formatter.Suspend();
				_formatter = null;
			}
			if (_formatter == null)
			{
				_formatter = new FlowDocumentPaginator(this);
			}
			return (FlowDocumentPaginator)_formatter;
		}
	}

	internal event EventHandler PageSizeChanged;

	static FlowDocument()
	{
		_typeofThis = typeof(FlowDocument);
		FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(_typeofThis);
		FontStyleProperty = TextElement.FontStyleProperty.AddOwner(_typeofThis);
		FontWeightProperty = TextElement.FontWeightProperty.AddOwner(_typeofThis);
		FontStretchProperty = TextElement.FontStretchProperty.AddOwner(_typeofThis);
		FontSizeProperty = TextElement.FontSizeProperty.AddOwner(_typeofThis);
		ForegroundProperty = TextElement.ForegroundProperty.AddOwner(_typeofThis);
		BackgroundProperty = TextElement.BackgroundProperty.AddOwner(_typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		TextEffectsProperty = TextElement.TextEffectsProperty.AddOwner(_typeofThis, new FrameworkPropertyMetadata(new FreezableDefaultValueFactory(TextEffectCollection.Empty), FrameworkPropertyMetadataOptions.AffectsRender));
		TextAlignmentProperty = Block.TextAlignmentProperty.AddOwner(_typeofThis);
		FlowDirectionProperty = Block.FlowDirectionProperty.AddOwner(_typeofThis);
		LineHeightProperty = Block.LineHeightProperty.AddOwner(_typeofThis);
		LineStackingStrategyProperty = Block.LineStackingStrategyProperty.AddOwner(_typeofThis);
		ColumnWidthProperty = DependencyProperty.Register("ColumnWidth", typeof(double), _typeofThis, new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));
		ColumnGapProperty = DependencyProperty.Register("ColumnGap", typeof(double), _typeofThis, new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), IsValidColumnGap);
		IsColumnWidthFlexibleProperty = DependencyProperty.Register("IsColumnWidthFlexible", typeof(bool), _typeofThis, new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure));
		ColumnRuleWidthProperty = DependencyProperty.Register("ColumnRuleWidth", typeof(double), _typeofThis, new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure), IsValidColumnRuleWidth);
		ColumnRuleBrushProperty = DependencyProperty.Register("ColumnRuleBrush", typeof(Brush), _typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		IsOptimalParagraphEnabledProperty = DependencyProperty.Register("IsOptimalParagraphEnabled", typeof(bool), _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));
		PageWidthProperty = DependencyProperty.Register("PageWidth", typeof(double), _typeofThis, new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPageMetricsChanged, CoercePageWidth), IsValidPageSize);
		MinPageWidthProperty = DependencyProperty.Register("MinPageWidth", typeof(double), _typeofThis, new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnMinPageWidthChanged), IsValidMinPageSize);
		MaxPageWidthProperty = DependencyProperty.Register("MaxPageWidth", typeof(double), _typeofThis, new FrameworkPropertyMetadata(double.PositiveInfinity, FrameworkPropertyMetadataOptions.AffectsMeasure, OnMaxPageWidthChanged, CoerceMaxPageWidth), IsValidMaxPageSize);
		PageHeightProperty = DependencyProperty.Register("PageHeight", typeof(double), _typeofThis, new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPageMetricsChanged, CoercePageHeight), IsValidPageSize);
		MinPageHeightProperty = DependencyProperty.Register("MinPageHeight", typeof(double), _typeofThis, new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnMinPageHeightChanged), IsValidMinPageSize);
		MaxPageHeightProperty = DependencyProperty.Register("MaxPageHeight", typeof(double), _typeofThis, new FrameworkPropertyMetadata(double.PositiveInfinity, FrameworkPropertyMetadataOptions.AffectsMeasure, OnMaxPageHeightChanged, CoerceMaxPageHeight), IsValidMaxPageSize);
		PagePaddingProperty = DependencyProperty.Register("PagePadding", typeof(Thickness), _typeofThis, new FrameworkPropertyMetadata(new Thickness(double.NaN), FrameworkPropertyMetadataOptions.AffectsMeasure, OnPageMetricsChanged), IsValidPagePadding);
		IsHyphenationEnabledProperty = Block.IsHyphenationEnabledProperty.AddOwner(_typeofThis);
		PropertyChangedCallback propertyChangedCallback = OnTypographyChanged;
		DependencyProperty[] typographyPropertiesList = Typography.TypographyPropertiesList;
		for (int i = 0; i < typographyPropertiesList.Length; i++)
		{
			typographyPropertiesList[i].OverrideMetadata(_typeofThis, new FrameworkPropertyMetadata(propertyChangedCallback));
		}
		FrameworkContentElement.DefaultStyleKeyProperty.OverrideMetadata(_typeofThis, new FrameworkPropertyMetadata(_typeofThis));
		ContentElement.FocusableProperty.OverrideMetadata(_typeofThis, new FrameworkPropertyMetadata(true));
		ControlsTraceLogger.AddControl(TelemetryControls.FlowDocument);
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Windows.Documents.FlowDocument" /> class.</summary>
	public FlowDocument()
	{
		Initialize(null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.FlowDocument" /> class, adding a specified <see cref="T:System.Windows.Documents.Block" /> element as the initial content. </summary>
	/// <param name="block">An object deriving from the abstract <see cref="T:System.Windows.Documents.Block" /> class, to be added as the initial content.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="block" /> is null.</exception>
	public FlowDocument(Block block)
	{
		Initialize(null);
		if (block == null)
		{
			throw new ArgumentNullException("block");
		}
		Blocks.Add(block);
	}

	internal FlowDocument(TextContainer textContainer)
	{
		Initialize(textContainer);
	}

	public void SetDpi(DpiScale dpiInfo)
	{
		if (dpiInfo.PixelsPerDip != _pixelsPerDip)
		{
			_pixelsPerDip = dpiInfo.PixelsPerDip;
			if (StructuralCache.HasPtsContext())
			{
				StructuralCache.TextFormatterHost.PixelsPerDip = _pixelsPerDip;
			}
			_formatter?.OnContentInvalidated(affectsLayout: true);
		}
	}

	/// <summary>Called when one or more of the dependency properties that exist on the element have had their effective values changed.</summary>
	/// <param name="e">Arguments for the associated event.</param>
	protected sealed override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if ((!e.IsAValueChange && !e.IsASubPropertyChange) || _structuralCache == null || !_structuralCache.IsFormattedOnce || !(e.Metadata is FrameworkPropertyMetadata frameworkPropertyMetadata))
		{
			return;
		}
		bool flag = frameworkPropertyMetadata.AffectsRender && (e.IsAValueChange || !frameworkPropertyMetadata.SubPropertiesDoNotAffectRender);
		if (frameworkPropertyMetadata.AffectsMeasure || frameworkPropertyMetadata.AffectsArrange || flag || frameworkPropertyMetadata.AffectsParentMeasure || frameworkPropertyMetadata.AffectsParentArrange)
		{
			if (_structuralCache.IsFormattingInProgress)
			{
				_structuralCache.OnInvalidOperationDetected();
				throw new InvalidOperationException(SR.FlowDocumentInvalidContnetChange);
			}
			_structuralCache.InvalidateFormatCache(!flag);
			if (_formatter != null)
			{
				_formatter.OnContentInvalidated(!flag);
			}
		}
	}

	/// <summary>When overridden in a derived class, provides specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementations to the Windows Presentation Foundation (WPF) infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new DocumentAutomationPeer(this);
	}

	internal ContentPosition GetObjectPosition(object element)
	{
		ITextPointer textPointer = null;
		if (element == this)
		{
			textPointer = ContentStart;
		}
		else if (element is TextElement)
		{
			textPointer = ((TextElement)element).ContentStart;
		}
		else if (element is FrameworkElement)
		{
			DependencyObject dependencyObject = null;
			while (element is FrameworkElement)
			{
				dependencyObject = LogicalTreeHelper.GetParent((DependencyObject)element);
				if (dependencyObject == null)
				{
					dependencyObject = VisualTreeHelper.GetParent((Visual)element);
				}
				if (!(dependencyObject is FrameworkElement))
				{
					break;
				}
				element = dependencyObject;
			}
			if (dependencyObject is BlockUIContainer || dependencyObject is InlineUIContainer)
			{
				textPointer = TextContainerHelper.GetTextPointerForEmbeddedObject((FrameworkElement)element);
			}
		}
		if (textPointer != null && textPointer.TextContainer != _structuralCache.TextContainer)
		{
			textPointer = null;
		}
		if (!(textPointer is TextPointer result))
		{
			return ContentPosition.Missing;
		}
		return result;
	}

	internal void OnChildDesiredSizeChanged(UIElement child)
	{
		if (_structuralCache == null || !_structuralCache.IsFormattedOnce || _structuralCache.ForceReformat)
		{
			return;
		}
		if (_structuralCache.IsFormattingInProgress)
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(OnChildDesiredSizeChangedAsync), child);
			return;
		}
		int cPFromEmbeddedObject = TextContainerHelper.GetCPFromEmbeddedObject(child, ElementEdge.BeforeStart);
		if (cPFromEmbeddedObject >= 0)
		{
			TextPointer textPointer = new TextPointer(_structuralCache.TextContainer.Start);
			textPointer.MoveByOffset(cPFromEmbeddedObject);
			TextPointer textPointer2 = new TextPointer(textPointer);
			textPointer2.MoveByOffset(TextContainerHelper.EmbeddedObjectLength);
			DirtyTextRange dtr = new DirtyTextRange(cPFromEmbeddedObject, TextContainerHelper.EmbeddedObjectLength, TextContainerHelper.EmbeddedObjectLength);
			_structuralCache.AddDirtyTextRange(dtr);
			if (_formatter != null)
			{
				_formatter.OnContentInvalidated(affectsLayout: true, textPointer, textPointer2);
			}
		}
	}

	internal void InitializeForFirstFormatting()
	{
		_structuralCache.TextContainer.Changing += OnTextContainerChanging;
		_structuralCache.TextContainer.Change += OnTextContainerChange;
		_structuralCache.TextContainer.Highlights.Changed += OnHighlightChanged;
	}

	internal void Uninitialize()
	{
		_structuralCache.TextContainer.Changing -= OnTextContainerChanging;
		_structuralCache.TextContainer.Change -= OnTextContainerChange;
		_structuralCache.TextContainer.Highlights.Changed -= OnHighlightChanged;
		_structuralCache.IsFormattedOnce = false;
	}

	internal Thickness ComputePageMargin()
	{
		double lineHeightValue = DynamicPropertyReader.GetLineHeightValue(this);
		Thickness pagePadding = PagePadding;
		if (double.IsNaN(pagePadding.Left))
		{
			pagePadding.Left = lineHeightValue;
		}
		if (double.IsNaN(pagePadding.Top))
		{
			pagePadding.Top = lineHeightValue;
		}
		if (double.IsNaN(pagePadding.Right))
		{
			pagePadding.Right = lineHeightValue;
		}
		if (double.IsNaN(pagePadding.Bottom))
		{
			pagePadding.Bottom = lineHeightValue;
		}
		return pagePadding;
	}

	internal override void OnNewParent(DependencyObject newParent)
	{
		DependencyObject parent = base.Parent;
		base.OnNewParent(newParent);
		if (newParent is RichTextBox || parent is RichTextBox)
		{
			CoerceValue(ContentElement.IsEnabledProperty);
		}
	}

	private static void OnTypographyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((FlowDocument)d)._typographyPropertiesGroup = null;
	}

	private object OnChildDesiredSizeChangedAsync(object arg)
	{
		OnChildDesiredSizeChanged(arg as UIElement);
		return null;
	}

	private void Initialize(TextContainer textContainer)
	{
		if (textContainer == null)
		{
			textContainer = new TextContainer(this, plainTextOnly: false);
		}
		_structuralCache = new StructuralCache(this, textContainer);
		if (_formatter != null)
		{
			_formatter.Suspend();
			_formatter = null;
		}
	}

	private static void OnPageMetricsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		FlowDocument flowDocument = (FlowDocument)d;
		if (flowDocument._structuralCache != null && flowDocument._structuralCache.IsFormattedOnce)
		{
			if (flowDocument._formatter != null)
			{
				flowDocument._formatter.OnContentInvalidated(affectsLayout: true);
			}
			if (flowDocument.PageSizeChanged != null)
			{
				flowDocument.PageSizeChanged(flowDocument, EventArgs.Empty);
			}
		}
	}

	private static void OnMinPageWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		d.CoerceValue(MaxPageWidthProperty);
		d.CoerceValue(PageWidthProperty);
	}

	private static void OnMinPageHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		d.CoerceValue(MaxPageHeightProperty);
		d.CoerceValue(PageHeightProperty);
	}

	private static void OnMaxPageWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		d.CoerceValue(PageWidthProperty);
	}

	private static void OnMaxPageHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		d.CoerceValue(PageHeightProperty);
	}

	private static object CoerceMaxPageWidth(DependencyObject d, object value)
	{
		FlowDocument flowDocument = (FlowDocument)d;
		double num = (double)value;
		double minPageWidth = flowDocument.MinPageWidth;
		if (num < minPageWidth)
		{
			return minPageWidth;
		}
		return value;
	}

	private static object CoerceMaxPageHeight(DependencyObject d, object value)
	{
		FlowDocument flowDocument = (FlowDocument)d;
		double num = (double)value;
		double minPageHeight = flowDocument.MinPageHeight;
		if (num < minPageHeight)
		{
			return minPageHeight;
		}
		return value;
	}

	private static object CoercePageWidth(DependencyObject d, object value)
	{
		FlowDocument flowDocument = (FlowDocument)d;
		double num = (double)value;
		if (!double.IsNaN(num))
		{
			double maxPageWidth = flowDocument.MaxPageWidth;
			if (num > maxPageWidth)
			{
				num = maxPageWidth;
			}
			double minPageWidth = flowDocument.MinPageWidth;
			if (num < minPageWidth)
			{
				num = minPageWidth;
			}
		}
		return value;
	}

	private static object CoercePageHeight(DependencyObject d, object value)
	{
		FlowDocument flowDocument = (FlowDocument)d;
		double num = (double)value;
		if (!double.IsNaN(num))
		{
			double maxPageHeight = flowDocument.MaxPageHeight;
			if (num > maxPageHeight)
			{
				num = maxPageHeight;
			}
			double minPageHeight = flowDocument.MinPageHeight;
			if (num < minPageHeight)
			{
				num = minPageHeight;
			}
		}
		return value;
	}

	private void OnHighlightChanged(object sender, HighlightChangedEventArgs args)
	{
		Invariant.Assert(args != null);
		Invariant.Assert(args.Ranges != null);
		Invariant.Assert(_structuralCache != null && _structuralCache.IsFormattedOnce, "Unexpected Highlights.Changed callback before first format!");
		if (_structuralCache.IsFormattingInProgress)
		{
			_structuralCache.OnInvalidOperationDetected();
			throw new InvalidOperationException(SR.FlowDocumentInvalidContnetChange);
		}
		if (args.OwnerType != typeof(SpellerHighlightLayer) || args.Ranges.Count <= 0)
		{
			return;
		}
		if (_formatter == null || !(_formatter is FlowDocumentFormatter))
		{
			_structuralCache.InvalidateFormatCache(destroyStructure: false);
		}
		if (_formatter == null)
		{
			return;
		}
		for (int i = 0; i < args.Ranges.Count; i++)
		{
			TextSegment textSegment = (TextSegment)args.Ranges[i];
			_formatter.OnContentInvalidated(affectsLayout: false, textSegment.Start, textSegment.End);
			if (_formatter is FlowDocumentFormatter)
			{
				DirtyTextRange dtr = new DirtyTextRange(textSegment.Start.Offset, textSegment.Start.GetOffsetToPosition(textSegment.End), textSegment.Start.GetOffsetToPosition(textSegment.End));
				_structuralCache.AddDirtyTextRange(dtr);
			}
		}
	}

	private void OnTextContainerChanging(object sender, EventArgs args)
	{
		Invariant.Assert(sender == _structuralCache.TextContainer, "Received text change for foreign TextContainer.");
		Invariant.Assert(_structuralCache != null && _structuralCache.IsFormattedOnce, "Unexpected TextContainer.Changing callback before first format!");
		if (_structuralCache.IsFormattingInProgress)
		{
			_structuralCache.OnInvalidOperationDetected();
			throw new InvalidOperationException(SR.FlowDocumentInvalidContnetChange);
		}
		_structuralCache.IsContentChangeInProgress = true;
	}

	private void OnTextContainerChange(object sender, TextContainerChangeEventArgs args)
	{
		Invariant.Assert(args != null);
		Invariant.Assert(sender == _structuralCache.TextContainer);
		Invariant.Assert(_structuralCache != null && _structuralCache.IsFormattedOnce, "Unexpected TextContainer.Change callback before first format!");
		if (args.Count == 0)
		{
			return;
		}
		try
		{
			if (_structuralCache.IsFormattingInProgress)
			{
				_structuralCache.OnInvalidOperationDetected();
				throw new InvalidOperationException(SR.FlowDocumentInvalidContnetChange);
			}
			ITextPointer end = ((args.TextChange == TextChangeType.ContentRemoved) ? args.ITextPosition : args.ITextPosition.CreatePointer(args.Count, LogicalDirection.Forward));
			if (!args.AffectsRenderOnly || (_formatter != null && _formatter is FlowDocumentFormatter))
			{
				DirtyTextRange dtr = new DirtyTextRange(args);
				_structuralCache.AddDirtyTextRange(dtr);
			}
			else
			{
				_structuralCache.InvalidateFormatCache(destroyStructure: false);
			}
			if (_formatter != null)
			{
				_formatter.OnContentInvalidated(!args.AffectsRenderOnly, args.ITextPosition, end);
			}
		}
		finally
		{
			_structuralCache.IsContentChangeInProgress = false;
		}
	}

	private static bool IsValidPageSize(object o)
	{
		double num = (double)o;
		double num2 = Math.Min(1000000, 3500000);
		if (double.IsNaN(num))
		{
			return true;
		}
		if (num < 0.0 || num > num2)
		{
			return false;
		}
		return true;
	}

	private static bool IsValidMinPageSize(object o)
	{
		double num = (double)o;
		double num2 = Math.Min(1000000, 3500000);
		if (double.IsNaN(num))
		{
			return false;
		}
		if (!double.IsNegativeInfinity(num) && (num < 0.0 || num > num2))
		{
			return false;
		}
		return true;
	}

	private static bool IsValidMaxPageSize(object o)
	{
		double num = (double)o;
		double num2 = Math.Min(1000000, 3500000);
		if (double.IsNaN(num))
		{
			return false;
		}
		if (!double.IsPositiveInfinity(num) && (num < 0.0 || num > num2))
		{
			return false;
		}
		return true;
	}

	private static bool IsValidPagePadding(object o)
	{
		return Block.IsValidThickness((Thickness)o, allowNaN: true);
	}

	private static bool IsValidColumnRuleWidth(object o)
	{
		double num = (double)o;
		double num2 = Math.Min(1000000, 3500000);
		if (double.IsNaN(num) || num < 0.0 || num > num2)
		{
			return false;
		}
		return true;
	}

	private static bool IsValidColumnGap(object o)
	{
		double num = (double)o;
		double num2 = Math.Min(1000000, 3500000);
		if (double.IsNaN(num))
		{
			return true;
		}
		if (num < 0.0 || num > num2)
		{
			return false;
		}
		return true;
	}

	/// <summary>Adds a child object. </summary>
	/// <param name="value">The child object to add.</param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!TextSchema.IsValidChildOfContainer(_typeofThis, value.GetType()))
		{
			throw new ArgumentException(SR.Format(SR.TextSchema_ChildTypeIsInvalid, _typeofThis.Name, value.GetType().Name));
		}
		if (value is TextElement && ((TextElement)value).Parent != null)
		{
			throw new ArgumentException(SR.Format(SR.TextSchema_TheChildElementBelongsToAnotherTreeAlready, value.GetType().Name));
		}
		if (value is Block)
		{
			TextContainer textContainer = _structuralCache.TextContainer;
			((Block)value).RepositionWithContent(textContainer.End);
		}
		else
		{
			Invariant.Assert(condition: false);
		}
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	/// <summary>Gets the service object of the specified type.</summary>
	/// <returns>null</returns>
	/// <param name="serviceType">An object that specifies the type of service object to get. </param>
	object IServiceProvider.GetService(Type serviceType)
	{
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (serviceType == typeof(ITextContainer))
		{
			return _structuralCache.TextContainer;
		}
		if (serviceType == typeof(TextContainer))
		{
			return _structuralCache.TextContainer;
		}
		return null;
	}
}
