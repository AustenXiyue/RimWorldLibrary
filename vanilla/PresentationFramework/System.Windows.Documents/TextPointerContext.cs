namespace System.Windows.Documents;

/// <summary>Determines the category of content that is adjacent to a <see cref="T:System.Windows.Documents.TextPointer" /> in a specified <see cref="T:System.Windows.Documents.LogicalDirection" />.</summary>
public enum TextPointerContext
{
	/// <summary>The <see cref="T:System.Windows.Documents.TextPointer" /> is adjacent to the beginning or end of content.</summary>
	None,
	/// <summary>The <see cref="T:System.Windows.Documents.TextPointer" /> is adjacent to text.</summary>
	Text,
	/// <summary>The <see cref="T:System.Windows.Documents.TextPointer" /> is adjacent to an embedded <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />.</summary>
	EmbeddedElement,
	/// <summary>The <see cref="T:System.Windows.Documents.TextPointer" /> is adjacent to the opening tag of a <see cref="T:System.Windows.Documents.TextElement" />.</summary>
	ElementStart,
	/// <summary>The <see cref="T:System.Windows.Documents.TextPointer" /> is adjacent to the closing tag of a <see cref="T:System.Windows.Documents.TextElement" />.</summary>
	ElementEnd
}
