using System.ComponentModel;
using System.Windows.Media;
using MS.Internal.Text;

namespace System.Windows.Documents;

/// <summary>An abstract class that provides a base for all block-level flow content elements.</summary>
public abstract class Block : TextElement
{
	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Block.IsHyphenationEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Block.IsHyphenationEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsHyphenationEnabledProperty = DependencyProperty.RegisterAttached("IsHyphenationEnabled", typeof(bool), typeof(Block), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Block.Margin" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Block.Margin" /> dependency property.</returns>
	public static readonly DependencyProperty MarginProperty = DependencyProperty.Register("Margin", typeof(Thickness), typeof(Block), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure), IsValidMargin);

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Block.Padding" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Block.Padding" /> dependency property.</returns>
	public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register("Padding", typeof(Thickness), typeof(Block), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure), IsValidPadding);

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Block.BorderThickness" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Block.BorderThickness" /> dependency property.</returns>
	public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(Block), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure), IsValidBorderThickness);

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Block.BorderBrush" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Block.BorderBrush" /> dependency property.</returns>
	public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(Block), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Block.TextAlignment" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Block.TextAlignment" /> dependency property.</returns>
	public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.RegisterAttached("TextAlignment", typeof(TextAlignment), typeof(Block), new FrameworkPropertyMetadata(TextAlignment.Left, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits), IsValidTextAlignment);

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Block.FlowDirection" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Block.FlowDirection" /> dependency property.</returns>
	public static readonly DependencyProperty FlowDirectionProperty = FrameworkElement.FlowDirectionProperty.AddOwner(typeof(Block));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Block.LineHeight" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Block.LineHeight" /> dependency property.</returns>
	public static readonly DependencyProperty LineHeightProperty = DependencyProperty.RegisterAttached("LineHeight", typeof(double), typeof(Block), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits), IsValidLineHeight);

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Block.LineStackingStrategy" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Block.LineStackingStrategy" /> dependency property.</returns>
	public static readonly DependencyProperty LineStackingStrategyProperty = DependencyProperty.RegisterAttached("LineStackingStrategy", typeof(LineStackingStrategy), typeof(Block), new FrameworkPropertyMetadata(LineStackingStrategy.MaxHeight, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits), IsValidLineStackingStrategy);

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Block.BreakPageBefore" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Block.BreakPageBefore" /> dependency property.</returns>
	public static readonly DependencyProperty BreakPageBeforeProperty = DependencyProperty.Register("BreakPageBefore", typeof(bool), typeof(Block), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Block.BreakColumnBefore" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Block.BreakColumnBefore" /> dependency property.</returns>
	public static readonly DependencyProperty BreakColumnBeforeProperty = DependencyProperty.Register("BreakColumnBefore", typeof(bool), typeof(Block), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Block.ClearFloaters" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Block.ClearFloaters" /> dependency property.</returns>
	public static readonly DependencyProperty ClearFloatersProperty = DependencyProperty.Register("ClearFloaters", typeof(WrapDirection), typeof(Block), new FrameworkPropertyMetadata(WrapDirection.None, FrameworkPropertyMetadataOptions.AffectsParentMeasure), IsValidWrapDirection);

	/// <summary>Gets a collection of <see cref="T:System.Windows.Documents.Block" /> elements that are siblings to the current <see cref="T:System.Windows.Documents.Block" /> element.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.BlockCollection" /> that contains the child <see cref="T:System.Windows.Documents.Block" /> elements that are directly hosted by the parent of the current <see cref="T:System.Windows.Documents.Block" /> element, or null if the current <see cref="T:System.Windows.Documents.Block" /> element has no parent.</returns>
	public BlockCollection SiblingBlocks
	{
		get
		{
			if (base.Parent == null)
			{
				return null;
			}
			return new BlockCollection(this, isOwnerParent: false);
		}
	}

	/// <summary>Gets the sibling <see cref="T:System.Windows.Documents.Block" /> element that directly follows the current <see cref="T:System.Windows.Documents.Block" /> element.</summary>
	/// <returns>The sibling <see cref="T:System.Windows.Documents.Block" /> element that directly follows the current <see cref="T:System.Windows.Documents.Block" /> element, or null if no such element exists.</returns>
	public Block NextBlock => base.NextElement as Block;

	/// <summary>Gets the sibling <see cref="T:System.Windows.Documents.Block" /> element that directly precedes the current <see cref="T:System.Windows.Documents.Block" /> element.</summary>
	/// <returns>The sibling <see cref="T:System.Windows.Documents.Block" /> element that directly precedes the current <see cref="T:System.Windows.Documents.Block" /> element, or null if no such element exists.</returns>
	public Block PreviousBlock => base.PreviousElement as Block;

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

	/// <summary>Gets or sets the margin thickness for the element.  </summary>
	/// <returns>A <see cref="T:System.Windows.Thickness" /> structure that specifies the amount of margin to apply, in device independent pixels. The default is a uniform thickness of zero (0.0).</returns>
	public Thickness Margin
	{
		get
		{
			return (Thickness)GetValue(MarginProperty);
		}
		set
		{
			SetValue(MarginProperty, value);
		}
	}

	/// <summary>Gets or sets the padding thickness for the element.  </summary>
	/// <returns>A <see cref="T:System.Windows.Thickness" /> structure that specifies the amount of padding to apply, in device independent pixels. The default is a uniform thickness of zero (0.0).</returns>
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

	/// <summary>Gets or sets the border thickness for the element.  </summary>
	/// <returns>A <see cref="T:System.Windows.Thickness" /> structure specifying the amount of border to apply, in device independent pixels. The default is a uniform thickness of zero (0.0).</returns>
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

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Brush" /> to use when painting the element's border.  </summary>
	/// <returns>The brush used to apply to the element's border. The default is null.</returns>
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

	/// <summary>Gets or sets the horizontal alignment of text content.  </summary>
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

	/// <summary>Gets or sets the relative direction for flow of content within a <see cref="T:System.Windows.Documents.Block" /> element.  </summary>
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
	/// <returns>The height of each line in device independent pixels, in the range of 0.0034 to 160000, or <see cref="F:System.Double.NaN" /> to determine the height automatically. The default is <see cref="F:System.Double.NaN" />.</returns>
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

	/// <summary>Gets or sets how a line box is determined for each line of text within the block-level flow content element.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.LineStackingStrategy" /> values that specifies how a line box is determined for each line of text within the block-level flow content element. The default value is <see cref="F:System.Windows.LineStackingStrategy.MaxHeight" />.</returns>
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

	/// <summary>Gets or sets a value that indicates whether to automatically insert a page-break before this element.  </summary>
	/// <returns>true to automatically insert a page-break before this element; otherwise, false.</returns>
	public bool BreakPageBefore
	{
		get
		{
			return (bool)GetValue(BreakPageBeforeProperty);
		}
		set
		{
			SetValue(BreakPageBeforeProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether to automatically insert a column-break before this element in cases where the element participates in a column-based presentation.  </summary>
	/// <returns>true to automatically insert a column-break before this element; otherwise, false.</returns>
	public bool BreakColumnBefore
	{
		get
		{
			return (bool)GetValue(BreakColumnBeforeProperty);
		}
		set
		{
			SetValue(BreakColumnBeforeProperty, value);
		}
	}

	/// <summary>Gets or sets the direction in which any <see cref="T:System.Windows.Documents.Floater" /> elements contained by a <see cref="T:System.Windows.Documents.Block" /> element should be repositioned.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.WrapDirection" /> values that specifies the direction in which to separate any <see cref="T:System.Windows.Documents.Floater" /> elements from other content contained in the <see cref="T:System.Windows.Documents.Block" /> element. The default is <see cref="F:System.Windows.WrapDirection.None" />, which indicates that floaters should be rendered in place.</returns>
	public WrapDirection ClearFloaters
	{
		get
		{
			return (WrapDirection)GetValue(ClearFloatersProperty);
		}
		set
		{
			SetValue(ClearFloatersProperty, value);
		}
	}

	internal override bool IsIMEStructuralElement => true;

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Block.IsHyphenationEnabled" /> attached property on a specified dependency object.</summary>
	/// <param name="element">The dependency object on which to set the value of the <see cref="P:System.Windows.Documents.Block.IsHyphenationEnabled" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetIsHyphenationEnabled(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsHyphenationEnabledProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Block.IsHyphenationEnabled" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Block.IsHyphenationEnabled" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Documents.Block.IsHyphenationEnabled" /> property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static bool GetIsHyphenationEnabled(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsHyphenationEnabledProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Block.TextAlignment" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object on which to set the value of the <see cref="P:System.Windows.Documents.Block.TextAlignment" /> property.</param>
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

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Block.TextAlignment" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Block.TextAlignment" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Documents.Block.TextAlignment" /> property.</param>
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

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Block.LineHeight" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object on which to set the value of the <see cref="P:System.Windows.Documents.Block.LineHeight" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is negative.</exception>
	public static void SetLineHeight(DependencyObject element, double value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(LineHeightProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Block.LineHeight" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Block.LineHeight" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Documents.Block.LineHeight" /> property.</param>
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

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Block.LineStackingStrategy" /> attached property on a specified dependency object.</summary>
	/// <param name="element">The dependency object on which to set the value of the <see cref="P:System.Windows.Documents.Block.LineStackingStrategy" /> property.</param>
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

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Block.LineStackingStrategy" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Block.LineStackingStrategy" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Documents.Block.LineStackingStrategy" /> attached property.</param>
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

	internal static bool IsValidMargin(object o)
	{
		return IsValidThickness((Thickness)o, allowNaN: true);
	}

	internal static bool IsValidPadding(object o)
	{
		return IsValidThickness((Thickness)o, allowNaN: true);
	}

	internal static bool IsValidBorderThickness(object o)
	{
		return IsValidThickness((Thickness)o, allowNaN: false);
	}

	private static bool IsValidLineHeight(object o)
	{
		double num = (double)o;
		double minWidth = TextDpi.MinWidth;
		double num2 = Math.Min(1000000, 160000);
		if (double.IsNaN(num))
		{
			return true;
		}
		if (num < minWidth)
		{
			return false;
		}
		if (num > num2)
		{
			return false;
		}
		return true;
	}

	private static bool IsValidLineStackingStrategy(object o)
	{
		LineStackingStrategy lineStackingStrategy = (LineStackingStrategy)o;
		if (lineStackingStrategy != LineStackingStrategy.MaxHeight)
		{
			return lineStackingStrategy == LineStackingStrategy.BlockLineHeight;
		}
		return true;
	}

	private static bool IsValidTextAlignment(object o)
	{
		TextAlignment textAlignment = (TextAlignment)o;
		if (textAlignment != TextAlignment.Center && textAlignment != TextAlignment.Justify && textAlignment != 0)
		{
			return textAlignment == TextAlignment.Right;
		}
		return true;
	}

	private static bool IsValidWrapDirection(object o)
	{
		WrapDirection wrapDirection = (WrapDirection)o;
		if (wrapDirection != 0 && wrapDirection != WrapDirection.Left && wrapDirection != WrapDirection.Right)
		{
			return wrapDirection == WrapDirection.Both;
		}
		return true;
	}

	internal static bool IsValidThickness(Thickness t, bool allowNaN)
	{
		double num = Math.Min(1000000, 3500000);
		if (!allowNaN && (double.IsNaN(t.Left) || double.IsNaN(t.Right) || double.IsNaN(t.Top) || double.IsNaN(t.Bottom)))
		{
			return false;
		}
		if (!double.IsNaN(t.Left) && (t.Left < 0.0 || t.Left > num))
		{
			return false;
		}
		if (!double.IsNaN(t.Right) && (t.Right < 0.0 || t.Right > num))
		{
			return false;
		}
		if (!double.IsNaN(t.Top) && (t.Top < 0.0 || t.Top > num))
		{
			return false;
		}
		if (!double.IsNaN(t.Bottom) && (t.Bottom < 0.0 || t.Bottom > num))
		{
			return false;
		}
		return true;
	}

	/// <summary>Initializes <see cref="T:System.Windows.Documents.Block" /> base class values when called by a derived class.</summary>
	protected Block()
	{
	}
}
