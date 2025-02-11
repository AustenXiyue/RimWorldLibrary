using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal;

namespace System.Windows.Documents;

/// <summary>A block-level flow content element that provides facilities for presenting content in an ordered or unordered list.</summary>
[ContentProperty("ListItems")]
public class List : Block
{
	/// <summary>Identifies the <see cref="P:System.Windows.Documents.List.MarkerStyle" />  dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.List.MarkerStyle" /> dependency property.</returns>
	public static readonly DependencyProperty MarkerStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.List.MarkerOffset" />  dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.List.MarkerOffset" /> dependency property.</returns>
	public static readonly DependencyProperty MarkerOffsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.List.StartIndex" />  dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.List.StartIndex" /> dependency property.</returns>
	public static readonly DependencyProperty StartIndexProperty;

	/// <summary>Gets a <see cref="T:System.Windows.Documents.ListItemCollection" /> containing the <see cref="T:System.Windows.Documents.ListItem" /> elements that comprise the contents of the <see cref="T:System.Windows.Documents.List" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.ListItemCollection" /> containing the <see cref="T:System.Windows.Documents.ListItem" /> elements that comprise the contents of the <see cref="T:System.Windows.Documents.List" />.This property has no default value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public ListItemCollection ListItems => new ListItemCollection(this, isOwnerParent: true);

	/// <summary>Gets or sets the marker style for the <see cref="T:System.Windows.Documents.List" />.  </summary>
	/// <returns>A member of the <see cref="T:System.Windows.TextMarkerStyle" /> enumeration specifying the marker style to use.The default value is <see cref="F:System.Windows.TextMarkerStyle.Disc" />.</returns>
	public TextMarkerStyle MarkerStyle
	{
		get
		{
			return (TextMarkerStyle)GetValue(MarkerStyleProperty);
		}
		set
		{
			SetValue(MarkerStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the desired distance between the contents of each <see cref="T:System.Windows.Documents.ListItem" /> element, and the near edge of the list marker.  </summary>
	/// <returns>A double value specifying the desired distance between list content and the near edge of list markers, in device independent pixels.  A value of <see cref="F:System.Double.NaN" /> (equivalent to an attribute value of "Auto") causes the marker offset to be determined automatically.The default value is <see cref="F:System.Double.NaN" />.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double MarkerOffset
	{
		get
		{
			return (double)GetValue(MarkerOffsetProperty);
		}
		set
		{
			SetValue(MarkerOffsetProperty, value);
		}
	}

	/// <summary>Gets or sets the starting index for labeling the items in an ordered list.  </summary>
	/// <returns>The starting index for labeling items in an ordered list.The default value is 1.</returns>
	public int StartIndex
	{
		get
		{
			return (int)GetValue(StartIndexProperty);
		}
		set
		{
			SetValue(StartIndexProperty, value);
		}
	}

	static List()
	{
		MarkerStyleProperty = DependencyProperty.Register("MarkerStyle", typeof(TextMarkerStyle), typeof(List), new FrameworkPropertyMetadata(TextMarkerStyle.Disc, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), IsValidMarkerStyle);
		MarkerOffsetProperty = DependencyProperty.Register("MarkerOffset", typeof(double), typeof(List), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), IsValidMarkerOffset);
		StartIndexProperty = DependencyProperty.Register("StartIndex", typeof(int), typeof(List), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), IsValidStartIndex);
		FrameworkContentElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(List), new FrameworkPropertyMetadata(typeof(List)));
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Windows.Documents.List" /> class. </summary>
	public List()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.List" /> class, taking a specified <see cref="T:System.Windows.Documents.ListItem" /> object as the initial contents of the new <see cref="T:System.Windows.Documents.List" />.</summary>
	/// <param name="listItem">A <see cref="T:System.Windows.Documents.ListItem" /> object specifying the initial contents of the new <see cref="T:System.Windows.Documents.List" />.</param>
	public List(ListItem listItem)
	{
		if (listItem == null)
		{
			throw new ArgumentNullException("listItem");
		}
		ListItems.Add(listItem);
	}

	internal int GetListItemIndex(ListItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (item.Parent != this)
		{
			throw new InvalidOperationException(SR.ListElementItemNotAChildOfList);
		}
		int num = StartIndex;
		TextPointer textPointer = new TextPointer(base.ContentStart);
		while (textPointer.CompareTo(base.ContentEnd) != 0)
		{
			if (textPointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart)
			{
				DependencyObject adjacentElementFromOuterPosition = textPointer.GetAdjacentElementFromOuterPosition(LogicalDirection.Forward);
				if (adjacentElementFromOuterPosition is ListItem)
				{
					if (adjacentElementFromOuterPosition == item)
					{
						break;
					}
					if (num < int.MaxValue)
					{
						num++;
					}
				}
				textPointer.MoveToPosition(((TextElement)adjacentElementFromOuterPosition).ElementEnd);
			}
			else
			{
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
			}
		}
		return num;
	}

	internal void Apply(Block firstBlock, Block lastBlock)
	{
		Invariant.Assert(base.Parent == null, "Cannot Apply List Because It Is Inserted In The Tree Already.");
		Invariant.Assert(base.IsEmpty, "Cannot Apply List Because It Is Not Empty.");
		Invariant.Assert(firstBlock.Parent == lastBlock.Parent, "Cannot Apply List Because Block Are Not Siblings.");
		TextContainer textContainer = base.TextContainer;
		textContainer.BeginChange();
		try
		{
			Reposition(firstBlock.ElementStart, lastBlock.ElementEnd);
			Block block = firstBlock;
			while (block != null)
			{
				ListItem listItem;
				if (block is List)
				{
					listItem = block.ElementStart.GetAdjacentElement(LogicalDirection.Backward) as ListItem;
					if (listItem != null)
					{
						listItem.Reposition(listItem.ContentStart, block.ElementEnd);
					}
					else
					{
						listItem = new ListItem();
						listItem.Reposition(block.ElementStart, block.ElementEnd);
					}
				}
				else
				{
					listItem = new ListItem();
					listItem.Reposition(block.ElementStart, block.ElementEnd);
					block.ClearValue(Block.MarginProperty);
					block.ClearValue(Block.PaddingProperty);
					block.ClearValue(Paragraph.TextIndentProperty);
				}
				block = ((block == lastBlock) ? null : ((Block)listItem.ElementEnd.GetAdjacentElement(LogicalDirection.Forward)));
			}
			TextRangeEdit.SetParagraphProperty(base.ElementStart, base.ElementEnd, Block.FlowDirectionProperty, firstBlock.GetValue(Block.FlowDirectionProperty));
		}
		finally
		{
			textContainer.EndChange();
		}
	}

	private static bool IsValidMarkerStyle(object o)
	{
		TextMarkerStyle textMarkerStyle = (TextMarkerStyle)o;
		if (textMarkerStyle != 0 && textMarkerStyle != TextMarkerStyle.Disc && textMarkerStyle != TextMarkerStyle.Circle && textMarkerStyle != TextMarkerStyle.Square && textMarkerStyle != TextMarkerStyle.Box && textMarkerStyle != TextMarkerStyle.LowerRoman && textMarkerStyle != TextMarkerStyle.UpperRoman && textMarkerStyle != TextMarkerStyle.LowerLatin && textMarkerStyle != TextMarkerStyle.UpperLatin)
		{
			return textMarkerStyle == TextMarkerStyle.Decimal;
		}
		return true;
	}

	private static bool IsValidStartIndex(object o)
	{
		return (int)o > 0;
	}

	private static bool IsValidMarkerOffset(object o)
	{
		double num = (double)o;
		double num2 = Math.Min(1000000, 3500000);
		double num3 = 0.0 - num2;
		if (double.IsNaN(num))
		{
			return true;
		}
		if (num < num3 || num > num2)
		{
			return false;
		}
		return true;
	}
}
