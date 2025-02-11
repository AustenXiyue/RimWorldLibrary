using System.ComponentModel;

namespace System.Windows.Documents;

/// <summary>Provides an inline-level flow content element used to host a floater. A floater displays images and other content parallel to the main content flow in a <see cref="T:System.Windows.Documents.FlowDocument" />.</summary>
public class Floater : AnchoredBlock
{
	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Floater.HorizontalAlignment" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Floater.HorizontalAlignment" /> dependency property.</returns>
	public static readonly DependencyProperty HorizontalAlignmentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Floater.Width" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Floater.Width" /> dependency property.</returns>
	public static readonly DependencyProperty WidthProperty;

	/// <summary>Gets or sets a value that indicates the horizontal alignment for a <see cref="T:System.Windows.Documents.Floater" /> object. </summary>
	/// <returns>A member of the <see cref="T:System.Windows.HorizontalAlignment" /> enumeration specifying the horizontal alignment for the <see cref="T:System.Windows.Documents.Floater" />. The default is <see cref="F:System.Windows.HorizontalAlignment.Stretch" />.</returns>
	public HorizontalAlignment HorizontalAlignment
	{
		get
		{
			return (HorizontalAlignment)GetValue(HorizontalAlignmentProperty);
		}
		set
		{
			SetValue(HorizontalAlignmentProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates the width of a <see cref="T:System.Windows.Documents.Floater" /> object. </summary>
	/// <returns>The width of the <see cref="T:System.Windows.Documents.Floater" />, in device independent pixels. The default value is <see cref="F:System.Double.NaN" /> (equivalent to an attribute value of Auto), which indicates that the line height is determined automatically. </returns>
	[TypeConverter(typeof(LengthConverter))]
	public double Width
	{
		get
		{
			return (double)GetValue(WidthProperty);
		}
		set
		{
			SetValue(WidthProperty, value);
		}
	}

	static Floater()
	{
		HorizontalAlignmentProperty = FrameworkElement.HorizontalAlignmentProperty.AddOwner(typeof(Floater), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsMeasure));
		WidthProperty = DependencyProperty.Register("Width", typeof(double), typeof(Floater), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), IsValidWidth);
		FrameworkContentElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Floater), new FrameworkPropertyMetadata(typeof(Floater)));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Floater" /> class.</summary>
	public Floater()
		: this(null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Floater" /> class with the specified <see cref="T:System.Windows.Documents.Block" /> object as its initial content.</summary>
	/// <param name="childBlock">The initial content of the new <see cref="T:System.Windows.Documents.Floater" />.</param>
	public Floater(Block childBlock)
		: this(childBlock, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Floater" /> class with the specified <see cref="T:System.Windows.Documents.Block" /> object as its initial content, and a <see cref="T:System.Windows.Documents.TextPointer" /> that specifies an insertion position for the new <see cref="T:System.Windows.Documents.Floater" />.</summary>
	/// <param name="childBlock">The initial content of the new <see cref="T:System.Windows.Documents.Floater" />. This parameter can be null, in which case no <see cref="T:System.Windows.Documents.Block" /> is inserted.</param>
	/// <param name="insertionPosition">The position at which to insert the <see cref="T:System.Windows.Documents.Floater" /> element after it is created.</param>
	public Floater(Block childBlock, TextPointer insertionPosition)
		: base(childBlock, insertionPosition)
	{
	}

	private static bool IsValidWidth(object o)
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
}
