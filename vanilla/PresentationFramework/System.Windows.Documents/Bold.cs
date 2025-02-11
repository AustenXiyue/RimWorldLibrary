namespace System.Windows.Documents;

/// <summary>An inline-level flow content element which causes content to render with a bold font weight.</summary>
public class Bold : Span
{
	static Bold()
	{
		FrameworkContentElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Bold), new FrameworkPropertyMetadata(typeof(Bold)));
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Windows.Documents.Bold" /> class. </summary>
	public Bold()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Bold" /> class, taking a specified <see cref="T:System.Windows.Documents.Inline" /> object as the initial contents of the new <see cref="T:System.Windows.Documents.Bold" />.</summary>
	/// <param name="childInline">An <see cref="T:System.Windows.Documents.Inline" /> object specifying the initial contents of the new <see cref="T:System.Windows.Documents.Bold" />.</param>
	public Bold(Inline childInline)
		: base(childInline)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Bold" /> class, taking a specified <see cref="T:System.Windows.Documents.Inline" /> object as the initial contents of the new <see cref="T:System.Windows.Documents.Bold" />, and a <see cref="T:System.Windows.Documents.TextPointer" /> specifying an insertion position for the new <see cref="T:System.Windows.Documents.Inline" /> element.</summary>
	/// <param name="childInline">An <see cref="T:System.Windows.Documents.Inline" /> object specifying the initial contents of the new <see cref="T:System.Windows.Documents.Bold" />.  This parameter may be null, in which case no <see cref="T:System.Windows.Documents.Inline" /> is inserted.</param>
	/// <param name="insertionPosition">A <see cref="T:System.Windows.Documents.TextPointer" /> specifying an insertion position at which to insert the <see cref="T:System.Windows.Documents.Bold" /> element after it is created, or null for no automatic insertion.</param>
	public Bold(Inline childInline, TextPointer insertionPosition)
		: base(childInline, insertionPosition)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Bold" /> class, taking two <see cref="T:System.Windows.Documents.TextPointer" /> objects that indicate the beginning and end of a selection of content to be contained by the new <see cref="T:System.Windows.Documents.Bold" />.</summary>
	/// <param name="start">A <see cref="T:System.Windows.Documents.TextPointer" /> indicating the beginning of a selection of content to be contained by the new <see cref="T:System.Windows.Documents.Bold" />.</param>
	/// <param name="end">A <see cref="T:System.Windows.Documents.TextPointer" /> indicating the end of a selection of content to be contained by the new <see cref="T:System.Windows.Documents.Bold" />.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="start" /> or <paramref name="end" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">Raised when <paramref name="start" /> and <paramref name="end" /> do not resolve to a range of content suitable for enclosure by a <see cref="T:System.Windows.Documents.Bold" /> element, for example, if <paramref name="start" /> and <paramref name="end" /> indicate positions in different paragraphs.</exception>
	public Bold(TextPointer start, TextPointer end)
		: base(start, end)
	{
	}
}
