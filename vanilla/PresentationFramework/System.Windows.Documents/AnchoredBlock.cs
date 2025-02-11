using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>An abstract class that provides a base for <see cref="T:System.Windows.Documents.Inline" /> elements that are used to anchor <see cref="T:System.Windows.Documents.Block" /> elements to flow content.</summary>
[ContentProperty("Blocks")]
public abstract class AnchoredBlock : Inline
{
	/// <summary>Identifies the <see cref="P:System.Windows.Documents.AnchoredBlock.Margin" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.AnchoredBlock.Margin" /> dependency property.</returns>
	public static readonly DependencyProperty MarginProperty = Block.MarginProperty.AddOwner(typeof(AnchoredBlock), new FrameworkPropertyMetadata(new Thickness(double.NaN), FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.AnchoredBlock.Padding" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.AnchoredBlock.Padding" /> dependency property.</returns>
	public static readonly DependencyProperty PaddingProperty = Block.PaddingProperty.AddOwner(typeof(AnchoredBlock), new FrameworkPropertyMetadata(new Thickness(double.NaN), FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.AnchoredBlock.BorderThickness" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.AnchoredBlock.BorderThickness" /> dependency property.</returns>
	public static readonly DependencyProperty BorderThicknessProperty = Block.BorderThicknessProperty.AddOwner(typeof(AnchoredBlock), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.AnchoredBlock.BorderBrush" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.AnchoredBlock.BorderBrush" /> dependency property.</returns>
	public static readonly DependencyProperty BorderBrushProperty = Block.BorderBrushProperty.AddOwner(typeof(AnchoredBlock), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.AnchoredBlock.TextAlignment" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.AnchoredBlock.TextAlignment" /> dependency property.</returns>
	public static readonly DependencyProperty TextAlignmentProperty = Block.TextAlignmentProperty.AddOwner(typeof(AnchoredBlock));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.AnchoredBlock.LineHeight" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.AnchoredBlock.LineHeight" /> dependency property.</returns>
	public static readonly DependencyProperty LineHeightProperty = Block.LineHeightProperty.AddOwner(typeof(AnchoredBlock));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.AnchoredBlock.LineStackingStrategy" />  dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.AnchoredBlock.LineStackingStrategy" /> dependency property.</returns>
	public static readonly DependencyProperty LineStackingStrategyProperty = Block.LineStackingStrategyProperty.AddOwner(typeof(AnchoredBlock));

	/// <summary>Gets a <see cref="T:System.Windows.Documents.BlockCollection" /> containing the top-level <see cref="T:System.Windows.Documents.Block" /> elements that comprise the contents of the element.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.BlockCollection" /> containing the <see cref="T:System.Windows.Documents.Block" /> elements that comprise the contents of the element. This property has no default value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public BlockCollection Blocks => new BlockCollection(this, isOwnerParent: true);

	/// <summary>Gets or sets the margin thickness for the element. </summary>
	/// <returns>A <see cref="T:System.Windows.Thickness" /> structure specifying the amount of margin to apply, in device independent pixels.The default value is a uniform thickness of zero (0.0).</returns>
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

	/// <summary>Gets or sets the padding thickness for the element. </summary>
	/// <returns>A <see cref="T:System.Windows.Thickness" /> structure specifying the amount of padding to apply, in device independent pixels.The default value is a uniform thickness of zero (0.0).</returns>
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

	/// <summary>Gets or sets the border thickness for the element. </summary>
	/// <returns>A <see cref="T:System.Windows.Thickness" /> structure specifying the amount of border to apply, in device independent pixels.The default value is a uniform thickness of zero (0.0).</returns>
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

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Brush" /> to use when painting the element's border. </summary>
	/// <returns>The brush used to apply to the element's border.The default value is a null brush.</returns>
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

	/// <summary>Gets or sets a value that indicates the horizontal alignment of text content. </summary>
	/// <returns>A member of the <see cref="T:System.Windows.TextAlignment" /> enumerations specifying the desired alignment.The default value is <see cref="F:System.Windows.TextAlignment.Left" />.</returns>
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

	/// <summary>Gets or sets the height of each line of content. </summary>
	/// <returns>A double value specifying the height of line in device independent pixels.  <see cref="P:System.Windows.Documents.AnchoredBlock.LineHeight" /> must be equal to or greater than 0.0034 and equal to or less then 160000.A value of <see cref="F:System.Double.NaN" /> (equivalent to an attribute value of "Auto") causes the line height is determined automatically from the current font characteristics.  The default value is <see cref="F:System.Double.NaN" />.</returns>
	/// <exception cref="T:System.ArgumentException">Raised if an attempt is made to set <see cref="P:System.Windows.Controls.TextBlock.LineHeight" /> to a non-positive value.</exception>
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

	/// <summary>Gets or sets the mechanism by which a line box is determined for each line of text within the text element. </summary>
	/// <returns>The mechanism by which a line box is determined for each line of text within the text element. The default value is <see cref="F:System.Windows.LineStackingStrategy.MaxHeight" />.</returns>
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

	internal override bool IsIMEStructuralElement => true;

	/// <summary>Initializes base class values when called by a derived class, taking a specified <see cref="T:System.Windows.Documents.Block" /> object as the initial contents of the new descendant of <see cref="T:System.Windows.Documents.AnchoredBlock" />, and a <see cref="T:System.Windows.Documents.TextPointer" /> specifying an insertion position for the new <see cref="T:System.Windows.Documents.AnchoredBlock" /> descendant.</summary>
	/// <param name="block">A <see cref="T:System.Windows.Documents.Block" /> object specifying the initial contents of the new element.  This parameter may be null, in which case no <see cref="T:System.Windows.Documents.Block" /> is inserted.</param>
	/// <param name="insertionPosition">A <see cref="T:System.Windows.Documents.TextPointer" /> specifying an insertion position at which to insert the element after it is created, or null for no automatic insertion.</param>
	protected AnchoredBlock(Block block, TextPointer insertionPosition)
	{
		insertionPosition?.TextContainer.BeginChange();
		try
		{
			insertionPosition?.InsertInline(this);
			if (block != null)
			{
				Blocks.Add(block);
			}
		}
		finally
		{
			insertionPosition?.TextContainer.EndChange();
		}
	}

	/// <summary>Returns a value that indicates whether or not the effective value of the <see cref="P:System.Windows.Documents.AnchoredBlock.Blocks" /> property should be serialized during serialization of an object deriving from <see cref="T:System.Windows.Documents.AnchoredBlock" />.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Documents.AnchoredBlock.Blocks" /> property should be serialized; otherwise, false.</returns>
	/// <param name="manager">A serialization service manager object for this object.</param>
	/// <exception cref="T:System.NullReferenceException">Raised when <paramref name="manager" /> is null.</exception>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeBlocks(XamlDesignerSerializationManager manager)
	{
		if (manager != null)
		{
			return manager.XmlWriter == null;
		}
		return false;
	}
}
