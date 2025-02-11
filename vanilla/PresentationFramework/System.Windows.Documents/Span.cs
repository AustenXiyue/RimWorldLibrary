using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal;

namespace System.Windows.Documents;

/// <summary>Groups other <see cref="T:System.Windows.Documents.Inline" /> flow content elements.</summary>
[ContentProperty("Inlines")]
public class Span : Inline
{
	/// <summary>Gets an <see cref="T:System.Windows.Documents.InlineCollection" /> containing the top-level <see cref="T:System.Windows.Documents.Inline" /> elements that comprise the contents of the <see cref="T:System.Windows.Documents.Span" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Documents.InlineCollection" /> containing the <see cref="T:System.Windows.Documents.Inline" /> elements that comprise the contents of the <see cref="T:System.Windows.Documents.Span" />.This property has no default value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public InlineCollection Inlines => new InlineCollection(this, isOwnerParent: true);

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Windows.Documents.Span" /> class.</summary>
	public Span()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Span" /> class with the specified <see cref="T:System.Windows.Documents.Inline" /> object as the initial contents.</summary>
	/// <param name="childInline">The initial contents of the new <see cref="T:System.Windows.Documents.Span" />.</param>
	public Span(Inline childInline)
		: this(childInline, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Span" /> class, taking a specified <see cref="T:System.Windows.Documents.Inline" /> object as the initial contents of the new <see cref="T:System.Windows.Documents.Span" />, and a <see cref="T:System.Windows.Documents.TextPointer" /> that specifies an insertion position for the new <see cref="T:System.Windows.Documents.Inline" /> element.</summary>
	/// <param name="childInline">An <see cref="T:System.Windows.Documents.Inline" /> object that specifies the initial contents of the new <see cref="T:System.Windows.Documents.Span" />. This parameter may be null, in which case no <see cref="T:System.Windows.Documents.Inline" /> is inserted.</param>
	/// <param name="insertionPosition">A <see cref="T:System.Windows.Documents.TextPointer" /> that specifies the position at which to insert the <see cref="T:System.Windows.Documents.Span" /> element after it is created, or null for no automatic insertion.</param>
	public Span(Inline childInline, TextPointer insertionPosition)
	{
		insertionPosition?.TextContainer.BeginChange();
		try
		{
			insertionPosition?.InsertInline(this);
			if (childInline != null)
			{
				Inlines.Add(childInline);
			}
		}
		finally
		{
			insertionPosition?.TextContainer.EndChange();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Span" /> class, taking two <see cref="T:System.Windows.Documents.TextPointer" /> objects that indicate the beginning and end of a selection of content that the new <see cref="T:System.Windows.Documents.Span" /> will contain.</summary>
	/// <param name="start">A <see cref="T:System.Windows.Documents.TextPointer" /> that indicates the beginning of a selection of content that the new <see cref="T:System.Windows.Documents.Span" /> will contain.</param>
	/// <param name="end">A <see cref="T:System.Windows.Documents.TextPointer" /> that indicates the end of a selection of content that the new <see cref="T:System.Windows.Documents.Span" /> will contain.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="start" /> or <paramref name="end" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">Raised when <paramref name="start" /> and <paramref name="end" /> do not resolve to a range of content suitable for enclosure by a <see cref="T:System.Windows.Documents.Span" /> element; for example, if <paramref name="start" /> and <paramref name="end" /> indicate positions in different paragraphs.</exception>
	public Span(TextPointer start, TextPointer end)
	{
		if (start == null)
		{
			throw new ArgumentNullException("start");
		}
		if (end == null)
		{
			throw new ArgumentNullException("start");
		}
		if (start.TextContainer != end.TextContainer)
		{
			throw new ArgumentException(SR.Format(SR.InDifferentTextContainers, "start", "end"));
		}
		if (start.CompareTo(end) > 0)
		{
			throw new ArgumentException(SR.Format(SR.BadTextPositionOrder, "start", "end"));
		}
		start.TextContainer.BeginChange();
		try
		{
			start = TextRangeEditTables.EnsureInsertionPosition(start);
			Invariant.Assert(start.Parent is Run);
			end = TextRangeEditTables.EnsureInsertionPosition(end);
			Invariant.Assert(end.Parent is Run);
			if (start.Paragraph != end.Paragraph)
			{
				throw new ArgumentException(SR.Format(SR.InDifferentParagraphs, "start", "end"));
			}
			Inline nonMergeableInlineAncestor;
			if ((nonMergeableInlineAncestor = start.GetNonMergeableInlineAncestor()) != null)
			{
				throw new InvalidOperationException(SR.Format(SR.TextSchema_CannotSplitElement, nonMergeableInlineAncestor.GetType().Name));
			}
			if ((nonMergeableInlineAncestor = end.GetNonMergeableInlineAncestor()) != null)
			{
				throw new InvalidOperationException(SR.Format(SR.TextSchema_CannotSplitElement, nonMergeableInlineAncestor.GetType().Name));
			}
			TextElement commonAncestor = TextElement.GetCommonAncestor((TextElement)start.Parent, (TextElement)end.Parent);
			while (start.Parent != commonAncestor)
			{
				start = SplitElement(start);
			}
			while (end.Parent != commonAncestor)
			{
				end = SplitElement(end);
			}
			if (start.Parent is Run)
			{
				start = SplitElement(start);
			}
			if (end.Parent is Run)
			{
				end = SplitElement(end);
			}
			Invariant.Assert(start.Parent == end.Parent);
			Invariant.Assert(TextSchema.IsValidChild(start, typeof(Span)));
			Reposition(start, end);
		}
		finally
		{
			start.TextContainer.EndChange();
		}
	}

	/// <summary>Returns a value that indicates whether the content of a <see cref="T:System.Windows.Documents.Span" /> element should be serialized during serialization of a <see cref="T:System.Windows.Documents.Run" /> object.</summary>
	/// <returns>true if the content should be serialized; otherwise, false.</returns>
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

	private TextPointer SplitElement(TextPointer position)
	{
		position = ((position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart) ? position.GetNextContextPosition(LogicalDirection.Backward) : ((position.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.ElementEnd) ? TextRangeEdit.SplitElement(position) : position.GetNextContextPosition(LogicalDirection.Forward)));
		return position;
	}
}
