using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>A flow content element that represents a particular content item in an ordered or unordered <see cref="T:System.Windows.Documents.List" />.</summary>
[ContentProperty("Blocks")]
public class ListItem : TextElement
{
	/// <summary>Identifies the <see cref="P:System.Windows.Documents.ListItem.Margin" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.ListItem.Margin" /> dependency property.</returns>
	public static readonly DependencyProperty MarginProperty = Block.MarginProperty.AddOwner(typeof(ListItem), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.ListItem.Padding" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.ListItem.Padding" /> dependency property.</returns>
	public static readonly DependencyProperty PaddingProperty = Block.PaddingProperty.AddOwner(typeof(ListItem), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.ListItem.BorderThickness" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.ListItem.BorderThickness" /> dependency property.</returns>
	public static readonly DependencyProperty BorderThicknessProperty = Block.BorderThicknessProperty.AddOwner(typeof(ListItem), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.ListItem.BorderBrush" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.ListItem.BorderBrush" /> dependency property.</returns>
	public static readonly DependencyProperty BorderBrushProperty = Block.BorderBrushProperty.AddOwner(typeof(ListItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.ListItem.TextAlignment" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.ListItem.TextAlignment" /> dependency property.</returns>
	public static readonly DependencyProperty TextAlignmentProperty = Block.TextAlignmentProperty.AddOwner(typeof(ListItem));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.ListItem.FlowDirection" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.ListItem.FlowDirection" /> dependency property.</returns>
	public static readonly DependencyProperty FlowDirectionProperty = Block.FlowDirectionProperty.AddOwner(typeof(ListItem));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.ListItem.LineHeight" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.ListItem.LineHeight" /> dependency property.</returns>
	public static readonly DependencyProperty LineHeightProperty = Block.LineHeightProperty.AddOwner(typeof(ListItem));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.ListItem.LineStackingStrategy" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.ListItem.LineStackingStrategy" /> dependency property.</returns>
	public static readonly DependencyProperty LineStackingStrategyProperty = Block.LineStackingStrategyProperty.AddOwner(typeof(ListItem));

	/// <summary>Gets the <see cref="T:System.Windows.Documents.List" /> that contains the <see cref="T:System.Windows.Documents.ListItem" />.</summary>
	/// <returns>The list that contains the <see cref="T:System.Windows.Documents.ListItem" />.</returns>
	public List List => base.Parent as List;

	/// <summary>Gets a block collection that contains the top-level <see cref="T:System.Windows.Documents.Block" /> elements of the <see cref="T:System.Windows.Documents.ListItem" />.</summary>
	/// <returns>A block collection that contains the <see cref="T:System.Windows.Documents.Block" /> elements of the <see cref="T:System.Windows.Documents.ListItem" /></returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public BlockCollection Blocks => new BlockCollection(this, isOwnerParent: true);

	/// <summary>Gets a <see cref="T:System.Windows.Documents.ListItemCollection" /> that contains the <see cref="T:System.Windows.Documents.ListItem" /> elements that are siblings of the current <see cref="T:System.Windows.Documents.ListItem" /> element.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.ListItemCollection" /> that contains the child <see cref="T:System.Windows.Documents.ListItem" /> elements that are directly hosted by the parent of the current <see cref="T:System.Windows.Documents.ListItem" /> element, or null if the current <see cref="T:System.Windows.Documents.ListItem" /> element has no parent.</returns>
	public ListItemCollection SiblingListItems
	{
		get
		{
			if (base.Parent == null)
			{
				return null;
			}
			return new ListItemCollection(this, isOwnerParent: false);
		}
	}

	/// <summary>Gets the next <see cref="T:System.Windows.Documents.ListItem" /> in the containing <see cref="T:System.Windows.Documents.List" />.</summary>
	/// <returns>The next <see cref="T:System.Windows.Documents.ListItem" /> in the <see cref="T:System.Windows.Documents.List" />, or null if there is no next <see cref="T:System.Windows.Documents.ListItem" />.</returns>
	public ListItem NextListItem => base.NextElement as ListItem;

	/// <summary>Gets the previous <see cref="T:System.Windows.Documents.ListItem" /> in the containing <see cref="T:System.Windows.Documents.List" />.</summary>
	/// <returns>The previous <see cref="T:System.Windows.Documents.ListItem" /> in the <see cref="T:System.Windows.Documents.List" />, or null if there is no previous <see cref="T:System.Windows.Documents.ListItem" />.</returns>
	public ListItem PreviousListItem => base.PreviousElement as ListItem;

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
	/// <returns>A <see cref="T:System.Windows.Thickness" /> structure that specifies the amount of border to apply, in device independent pixels. The default is a uniform thickness of zero (0.0).</returns>
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

	/// <summary>Gets or sets the relative direction for flow of content within a <see cref="T:System.Windows.Documents.ListItem" /> element.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.FlowDirection" /> values that specifies the relative flow direction.  The default is <see cref="F:System.Windows.FlowDirection.LeftToRight" />.</returns>
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
	/// <returns>The height of each line in device independent pixels with a value range of 0.0034 to 160000.  A value of <see cref="F:System.Double.NaN" /> (equivalent to an attribute value of "Auto") causes the line height to be determined automatically from the current font characteristics.  The default is <see cref="F:System.Double.NaN" />.</returns>
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

	/// <summary>Gets or sets the mechanism by which a line box is determined for each line of text within the <see cref="T:System.Windows.Documents.ListItem" />.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.LineStackingStrategy" /> values that specifies the mechanism by which a line box is determined for each line of text within the <see cref="T:System.Windows.Documents.ListItem" />. The default is <see cref="F:System.Windows.LineStackingStrategy.MaxHeight" />.</returns>
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

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Windows.Documents.ListItem" /> class.</summary>
	public ListItem()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.ListItem" /> class, taking a specified <see cref="T:System.Windows.Documents.Paragraph" /> object as the initial contents of the new <see cref="T:System.Windows.Documents.ListItem" />.</summary>
	/// <param name="paragraph">A <see cref="T:System.Windows.Documents.Paragraph" /> object specifying the initial contents of the new <see cref="T:System.Windows.Documents.ListItem" />.</param>
	public ListItem(Paragraph paragraph)
	{
		if (paragraph == null)
		{
			throw new ArgumentNullException("paragraph");
		}
		Blocks.Add(paragraph);
	}

	/// <summary>Returns a value that indicates whether the effective value of the <see cref="P:System.Windows.Documents.ListItem.Blocks" /> property should be serialized during serialization of the <see cref="T:System.Windows.Documents.ListItem" /> object.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Documents.ListItem.Blocks" /> property should be serialized; otherwise, false.</returns>
	/// <param name="manager">A serialization service manager object for the object.</param>
	/// <exception cref="T:System.NullReferenceException">
	///   <paramref name="manager" /> is null.</exception>
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
