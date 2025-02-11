using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal;

namespace System.Windows.Documents;

/// <summary>A block-level flow content element used to group content into a paragraph.</summary>
[ContentProperty("Inlines")]
public class Paragraph : Block
{
	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Paragraph.TextDecorations" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Paragraph.TextDecorations" /> dependency property.</returns>
	public static readonly DependencyProperty TextDecorationsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Paragraph.TextIndent" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Paragraph.TextIndent" /> dependency property.</returns>
	public static readonly DependencyProperty TextIndentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Paragraph.MinOrphanLines" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Paragraph.MinOrphanLines" /> dependency property.</returns>
	public static readonly DependencyProperty MinOrphanLinesProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Paragraph.MinWidowLines" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Paragraph.MinWidowLines" /> dependency property.</returns>
	public static readonly DependencyProperty MinWidowLinesProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Paragraph.KeepWithNext" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Paragraph.KeepWithNext" /> dependency property.</returns>
	public static readonly DependencyProperty KeepWithNextProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Paragraph.KeepTogether" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Paragraph.KeepTogether" /> dependency property.</returns>
	public static readonly DependencyProperty KeepTogetherProperty;

	/// <summary>Gets an <see cref="T:System.Windows.Documents.InlineCollection" /> containing the top-level <see cref="T:System.Windows.Documents.Inline" /> elements that comprise the contents of the <see cref="T:System.Windows.Documents.Paragraph" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Documents.InlineCollection" /> containing the <see cref="T:System.Windows.Documents.Inline" /> elements that comprise the contents of the <see cref="T:System.Windows.Documents.Paragraph" />.This property has no default value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public InlineCollection Inlines => new InlineCollection(this, isOwnerParent: true);

	/// <summary>Gets or sets a <see cref="T:System.Windows.TextDecorationCollection" /> that contains text decorations to apply to this element.  </summary>
	/// <returns>A <see cref="T:System.Windows.TextDecorationCollection" /> collection that contains text decorations to apply to this element. A value of null means no text decorations will be applied. The default value is null.</returns>
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

	/// <summary>Gets or sets a value that indicates how far to indent the first line of a <see cref="T:System.Windows.Documents.Paragraph" />.  </summary>
	/// <returns>A double value specifying the amount to indent the first line of the paragraph, in device independent pixels. The default value is 0.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double TextIndent
	{
		get
		{
			return (double)GetValue(TextIndentProperty);
		}
		set
		{
			SetValue(TextIndentProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the minimum number of lines that can be left before the break when a <see cref="T:System.Windows.Documents.Paragraph" /> is broken by a page break or column break.  </summary>
	/// <returns>An integer specifying the minimum number of lines that can be left before the break when a <see cref="T:System.Windows.Documents.Paragraph" /> is broken by a page break or column break. A value of 0 indicates no minimum.The default value is 0.</returns>
	public int MinOrphanLines
	{
		get
		{
			return (int)GetValue(MinOrphanLinesProperty);
		}
		set
		{
			SetValue(MinOrphanLinesProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the minimum number of lines that can be placed after the break when a <see cref="T:System.Windows.Documents.Paragraph" /> is broken by a page break or column break.  </summary>
	/// <returns>An integer specifying the minimum number of lines that can be placed after the break when a <see cref="T:System.Windows.Documents.Paragraph" /> is broken by a page break or column break.  A value of 0 indicates no minimum.The default value is 0.</returns>
	public int MinWidowLines
	{
		get
		{
			return (int)GetValue(MinWidowLinesProperty);
		}
		set
		{
			SetValue(MinWidowLinesProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a break may occur between this paragraph and the next paragraph.  </summary>
	/// <returns>true to prevent a break from occurring between this paragraph and the next paragraph; otherwise, false. The default value is false.</returns>
	public bool KeepWithNext
	{
		get
		{
			return (bool)GetValue(KeepWithNextProperty);
		}
		set
		{
			SetValue(KeepWithNextProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the text of the paragraph may be broken by a page break or column break.  </summary>
	/// <returns>true to prevent the text of the paragraph from being broken; otherwise, false. The default value is false.</returns>
	public bool KeepTogether
	{
		get
		{
			return (bool)GetValue(KeepTogetherProperty);
		}
		set
		{
			SetValue(KeepTogetherProperty, value);
		}
	}

	static Paragraph()
	{
		TextDecorationsProperty = Inline.TextDecorationsProperty.AddOwner(typeof(Paragraph), new FrameworkPropertyMetadata(new FreezableDefaultValueFactory(TextDecorationCollection.Empty), FrameworkPropertyMetadataOptions.AffectsRender));
		TextIndentProperty = DependencyProperty.Register("TextIndent", typeof(double), typeof(Paragraph), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), IsValidTextIndent);
		MinOrphanLinesProperty = DependencyProperty.Register("MinOrphanLines", typeof(int), typeof(Paragraph), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsParentMeasure), IsValidMinOrphanLines);
		MinWidowLinesProperty = DependencyProperty.Register("MinWidowLines", typeof(int), typeof(Paragraph), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsParentMeasure), IsValidMinWidowLines);
		KeepWithNextProperty = DependencyProperty.Register("KeepWithNext", typeof(bool), typeof(Paragraph), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
		KeepTogetherProperty = DependencyProperty.Register("KeepTogether", typeof(bool), typeof(Paragraph), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
		FrameworkContentElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Paragraph), new FrameworkPropertyMetadata(typeof(Paragraph)));
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Windows.Documents.Paragraph" /> class.</summary>
	public Paragraph()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Paragraph" /> class, taking a specified <see cref="T:System.Windows.Documents.Inline" /> object as its initial contents.</summary>
	/// <param name="inline">An <see cref="T:System.Windows.Documents.Inline" /> object specifying the initial contents of the new <see cref="T:System.Windows.Documents.Paragraph" />.</param>
	public Paragraph(Inline inline)
	{
		if (inline == null)
		{
			throw new ArgumentNullException("inline");
		}
		Inlines.Add(inline);
	}

	internal void GetDefaultMarginValue(ref Thickness margin)
	{
		double num = base.LineHeight;
		if (IsLineHeightAuto(num))
		{
			num = base.FontFamily.LineSpacing * base.FontSize;
		}
		margin = new Thickness(0.0, num, 0.0, num);
	}

	internal static bool IsMarginAuto(Thickness margin)
	{
		if (double.IsNaN(margin.Left) && double.IsNaN(margin.Right) && double.IsNaN(margin.Top))
		{
			return double.IsNaN(margin.Bottom);
		}
		return false;
	}

	internal static bool IsLineHeightAuto(double lineHeight)
	{
		return double.IsNaN(lineHeight);
	}

	internal static bool HasNoTextContent(Paragraph paragraph)
	{
		ITextPointer textPointer = paragraph.ContentStart.CreatePointer();
		ITextPointer contentEnd = paragraph.ContentEnd;
		while (textPointer.CompareTo(contentEnd) < 0)
		{
			TextPointerContext pointerContext = textPointer.GetPointerContext(LogicalDirection.Forward);
			if (pointerContext == TextPointerContext.Text || pointerContext == TextPointerContext.EmbeddedElement || typeof(LineBreak).IsAssignableFrom(textPointer.ParentType) || typeof(AnchoredBlock).IsAssignableFrom(textPointer.ParentType))
			{
				return false;
			}
			textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
		}
		return true;
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Documents.Paragraph.Inlines" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	/// <param name="manager">A serialization service manager object for this object.</param>
	/// <exception cref="T:System.NullReferenceException">
	///   <paramref name="manager" /> is null.</exception>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeInlines(XamlDesignerSerializationManager manager)
	{
		if (manager != null)
		{
			return manager.XmlWriter == null;
		}
		return false;
	}

	private static bool IsValidMinOrphanLines(object o)
	{
		int num = (int)o;
		if (num >= 0)
		{
			return num <= 1000000;
		}
		return false;
	}

	private static bool IsValidMinWidowLines(object o)
	{
		int num = (int)o;
		if (num >= 0)
		{
			return num <= 1000000;
		}
		return false;
	}

	private static bool IsValidTextIndent(object o)
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
