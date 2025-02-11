using System.ComponentModel;

namespace System.Windows.Documents;

/// <summary>An inline-level flow content element used to host a figure.  A figure is a portion of flow content with placement properties that can be customized independently from the primary content flow within a <see cref="T:System.Windows.Documents.FlowDocument" />.</summary>
public class Figure : AnchoredBlock
{
	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Figure.HorizontalAnchor" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Figure.HorizontalAnchor" /> dependency property.</returns>
	public static readonly DependencyProperty HorizontalAnchorProperty = DependencyProperty.Register("HorizontalAnchor", typeof(FigureHorizontalAnchor), typeof(Figure), new FrameworkPropertyMetadata(FigureHorizontalAnchor.ColumnRight, FrameworkPropertyMetadataOptions.AffectsParentMeasure), IsValidHorizontalAnchor);

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Figure.VerticalAnchor" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Figure.VerticalAnchor" /> dependency property.</returns>
	public static readonly DependencyProperty VerticalAnchorProperty = DependencyProperty.Register("VerticalAnchor", typeof(FigureVerticalAnchor), typeof(Figure), new FrameworkPropertyMetadata(FigureVerticalAnchor.ParagraphTop, FrameworkPropertyMetadataOptions.AffectsParentMeasure), IsValidVerticalAnchor);

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Figure.HorizontalOffset" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Figure.HorizontalOffset" /> dependency property.</returns>
	public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(Figure), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure), IsValidOffset);

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Figure.VerticalOffset" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Figure.VerticalOffset" /> dependency property.</returns>
	public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(double), typeof(Figure), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure), IsValidOffset);

	/// <summary> Identifies the <see cref="P:System.Windows.Documents.Figure.CanDelayPlacement" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Figure.CanDelayPlacement" /> dependency property.</returns>
	public static readonly DependencyProperty CanDelayPlacementProperty = DependencyProperty.Register("CanDelayPlacement", typeof(bool), typeof(Figure), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Figure.WrapDirection" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Figure.WrapDirection" /> dependency property.</returns>
	public static readonly DependencyProperty WrapDirectionProperty = DependencyProperty.Register("WrapDirection", typeof(WrapDirection), typeof(Figure), new FrameworkPropertyMetadata(WrapDirection.Both, FrameworkPropertyMetadataOptions.AffectsParentMeasure), IsValidWrapDirection);

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Figure.Width" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Figure.Width" /> dependency property.</returns>
	public static readonly DependencyProperty WidthProperty = DependencyProperty.Register("Width", typeof(FigureLength), typeof(Figure), new FrameworkPropertyMetadata(new FigureLength(1.0, FigureUnitType.Auto), FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Figure.Height" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Figure.Height" /> dependency property.</returns>
	public static readonly DependencyProperty HeightProperty = DependencyProperty.Register("Height", typeof(FigureLength), typeof(Figure), new FrameworkPropertyMetadata(new FigureLength(1.0, FigureUnitType.Auto), FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Gets or sets a value that indicates the position that content is anchored to in the horizontal direction.  </summary>
	/// <returns>A member of the <see cref="T:System.Windows.FigureHorizontalAnchor" /> enumeration specifying a horizontal anchor location for the <see cref="T:System.Windows.Documents.Figure" />.The default value is <see cref="F:System.Windows.FigureHorizontalAnchor.ColumnRight" />.</returns>
	public FigureHorizontalAnchor HorizontalAnchor
	{
		get
		{
			return (FigureHorizontalAnchor)GetValue(HorizontalAnchorProperty);
		}
		set
		{
			SetValue(HorizontalAnchorProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates the position that content is anchored to in the vertical direction.  </summary>
	/// <returns>A member of the <see cref="T:System.Windows.FigureVerticalAnchor" /> enumeration specifying a vertical anchor location for the <see cref="T:System.Windows.Documents.Figure" />.The default value is <see cref="F:System.Windows.FigureVerticalAnchor.ParagraphTop" />.</returns>
	public FigureVerticalAnchor VerticalAnchor
	{
		get
		{
			return (FigureVerticalAnchor)GetValue(VerticalAnchorProperty);
		}
		set
		{
			SetValue(VerticalAnchorProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates the distance that a <see cref="T:System.Windows.Documents.Figure" /> is offset from its baseline in the horizontal direction.  </summary>
	/// <returns>The distance that a <see cref="T:System.Windows.Documents.Figure" /> is offset from its baseline in the horizontal direction, in device independent pixels.The default value is 0.0.</returns>
	[TypeConverter(typeof(LengthConverter))]
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

	/// <summary>Gets or sets a value that indicates the distance that a <see cref="T:System.Windows.Documents.Figure" /> is offset from its baseline in the vertical direction.  </summary>
	/// <returns>The distance that a <see cref="T:System.Windows.Documents.Figure" /> is offset from its baseline in the vertical direction, in device independent pixels.The default value is 0.0.</returns>
	[TypeConverter(typeof(LengthConverter))]
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

	/// <summary>Gets or sets a value that indicates whether this figure can delay its placement in the flow of content.  </summary>
	/// <returns>true if this figure can delay placement; otherwise, false.The default value is true.</returns>
	public bool CanDelayPlacement
	{
		get
		{
			return (bool)GetValue(CanDelayPlacementProperty);
		}
		set
		{
			SetValue(CanDelayPlacementProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates the allowable ways in which content can flow around a <see cref="T:System.Windows.Documents.Figure" />.   </summary>
	/// <returns>A member of the <see cref="T:System.Windows.WrapDirection" /> enumeration specifying the allowable ways in which content can flow around a <see cref="T:System.Windows.Documents.Figure" />.The default value is <see cref="F:System.Windows.WrapDirection.Both" />.</returns>
	public WrapDirection WrapDirection
	{
		get
		{
			return (WrapDirection)GetValue(WrapDirectionProperty);
		}
		set
		{
			SetValue(WrapDirectionProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates the width of a <see cref="T:System.Windows.Documents.Figure" /> element.  </summary>
	/// <returns>A <see cref="T:System.Windows.FigureLength" /> structure specifying the width characteristics for the <see cref="T:System.Windows.Documents.Figure" />.The default value is <see cref="T:System.Windows.FigureLength" />.<see cref="P:System.Windows.FigureLength.Value" /> = 1.0 and <see cref="T:System.Windows.FigureLength" />.<see cref="P:System.Windows.FigureLength.FigureUnitType" /> = <see cref="F:System.Windows.FigureUnitType.Auto" />.</returns>
	public FigureLength Width
	{
		get
		{
			return (FigureLength)GetValue(WidthProperty);
		}
		set
		{
			SetValue(WidthProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates the height of a <see cref="T:System.Windows.Documents.Figure" /> element.  </summary>
	/// <returns>A <see cref="T:System.Windows.FigureLength" /> structure specifying the height characteristics for the <see cref="T:System.Windows.Documents.Figure" />.The default value is <see cref="T:System.Windows.FigureLength" />.<see cref="P:System.Windows.FigureLength.Value" /> = 1.0 and <see cref="T:System.Windows.FigureLength" />.<see cref="P:System.Windows.FigureLength.FigureUnitType" /> = <see cref="F:System.Windows.FigureUnitType.Auto" />.</returns>
	public FigureLength Height
	{
		get
		{
			return (FigureLength)GetValue(HeightProperty);
		}
		set
		{
			SetValue(HeightProperty, value);
		}
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Windows.Documents.Figure" /> class.</summary>
	public Figure()
		: this(null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Figure" /> class, taking a specified <see cref="T:System.Windows.Documents.Block" /> object as the initial contents of the new <see cref="T:System.Windows.Documents.Figure" />.</summary>
	/// <param name="childBlock">A <see cref="T:System.Windows.Documents.Block" /> object specifying the initial contents of the new <see cref="T:System.Windows.Documents.Figure" />.</param>
	public Figure(Block childBlock)
		: this(childBlock, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Figure" /> class, taking a specified <see cref="T:System.Windows.Documents.Block" /> object as the initial contents of the new <see cref="T:System.Windows.Documents.Figure" />, and a <see cref="T:System.Windows.Documents.TextPointer" /> specifying an insertion position for the new <see cref="T:System.Windows.Documents.Figure" /> element.</summary>
	/// <param name="childBlock">A <see cref="T:System.Windows.Documents.Block" /> object specifying the initial contents of the new <see cref="T:System.Windows.Documents.Figure" />.  This parameter may be null, in which case no <see cref="T:System.Windows.Documents.Block" /> is inserted.</param>
	/// <param name="insertionPosition">A <see cref="T:System.Windows.Documents.TextPointer" /> specifying an insertion position at which to insert the <see cref="T:System.Windows.Documents.Figure" /> element after it is created, or null for no automatic insertion.</param>
	public Figure(Block childBlock, TextPointer insertionPosition)
		: base(childBlock, insertionPosition)
	{
	}

	private static bool IsValidHorizontalAnchor(object o)
	{
		FigureHorizontalAnchor figureHorizontalAnchor = (FigureHorizontalAnchor)o;
		if (figureHorizontalAnchor != FigureHorizontalAnchor.ContentCenter && figureHorizontalAnchor != FigureHorizontalAnchor.ContentLeft && figureHorizontalAnchor != FigureHorizontalAnchor.ContentRight && figureHorizontalAnchor != FigureHorizontalAnchor.PageCenter && figureHorizontalAnchor != 0 && figureHorizontalAnchor != FigureHorizontalAnchor.PageRight && figureHorizontalAnchor != FigureHorizontalAnchor.ColumnCenter && figureHorizontalAnchor != FigureHorizontalAnchor.ColumnLeft)
		{
			return figureHorizontalAnchor == FigureHorizontalAnchor.ColumnRight;
		}
		return true;
	}

	private static bool IsValidVerticalAnchor(object o)
	{
		FigureVerticalAnchor figureVerticalAnchor = (FigureVerticalAnchor)o;
		if (figureVerticalAnchor != FigureVerticalAnchor.ContentBottom && figureVerticalAnchor != FigureVerticalAnchor.ContentCenter && figureVerticalAnchor != FigureVerticalAnchor.ContentTop && figureVerticalAnchor != FigureVerticalAnchor.PageBottom && figureVerticalAnchor != FigureVerticalAnchor.PageCenter && figureVerticalAnchor != 0)
		{
			return figureVerticalAnchor == FigureVerticalAnchor.ParagraphTop;
		}
		return true;
	}

	private static bool IsValidWrapDirection(object o)
	{
		WrapDirection wrapDirection = (WrapDirection)o;
		if (wrapDirection != WrapDirection.Both && wrapDirection != 0 && wrapDirection != WrapDirection.Left)
		{
			return wrapDirection == WrapDirection.Right;
		}
		return true;
	}

	private static bool IsValidOffset(object o)
	{
		double num = (double)o;
		double num2 = Math.Min(1000000, 3500000);
		double num3 = 0.0 - num2;
		if (double.IsNaN(num))
		{
			return false;
		}
		if (num < num3 || num > num2)
		{
			return false;
		}
		return true;
	}
}
