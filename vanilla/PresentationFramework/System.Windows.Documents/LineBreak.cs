using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>An inline flow content element that causes a line break to occur in flow content.</summary>
[TrimSurroundingWhitespace]
public class LineBreak : Inline
{
	/// <summary>Initializes a new, default instance of the <see cref="T:System.Windows.Documents.LineBreak" /> class.</summary>
	public LineBreak()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.LineBreak" /> class, and inserts the new <see cref="T:System.Windows.Documents.LineBreak" /> at a specified position.</summary>
	/// <param name="insertionPosition">A <see cref="T:System.Windows.Documents.TextPointer" /> specifying an insertion position at which to insert the <see cref="T:System.Windows.Documents.LineBreak" /> element after it is created, or null for no automatic insertion.</param>
	public LineBreak(TextPointer insertionPosition)
	{
		insertionPosition?.TextContainer.BeginChange();
		try
		{
			insertionPosition?.InsertInline(this);
		}
		finally
		{
			insertionPosition?.TextContainer.EndChange();
		}
	}
}
