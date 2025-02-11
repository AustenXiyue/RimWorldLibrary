using System.Collections.Generic;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.TextFormatting;

/// <summary>Provides a set of properties, such as flow direction, alignment, or indentation, that can be applied to a paragraph. This is an abstract class.</summary>
public abstract class TextParagraphProperties
{
	private TextLexicalService _hyphenator;

	/// <summary>Gets a value that specifies whether the primary text advance direction shall be left-to-right, or right-to-left.</summary>
	/// <returns>An enumerated value of <see cref="T:System.Windows.FlowDirection" />.</returns>
	public abstract FlowDirection FlowDirection { get; }

	/// <summary>Gets a value that describes how an inline content of a block is aligned.</summary>
	/// <returns>An enumerated value of <see cref="T:System.Windows.TextAlignment" />.</returns>
	public abstract TextAlignment TextAlignment { get; }

	/// <summary>Gets the height of a line of text.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the height of a line of text.</returns>
	public abstract double LineHeight { get; }

	/// <summary>Gets a value that indicates whether the text run is the first line of the paragraph.</summary>
	/// <returns>true, if the text run is the first line of the paragraph; otherwise, false.</returns>
	public abstract bool FirstLineInParagraph { get; }

	/// <summary>Gets a value that indicates whether a formatted line can always be collapsed.</summary>
	/// <returns>true if the formatted line can always be collapsed; otherwise, false, which indicates that only formatted lines that overflow the paragraph width are collapsed. The default value is false.</returns>
	public virtual bool AlwaysCollapsible => false;

	/// <summary>Gets the default text run properties, such as typeface or foreground brush.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> value.</returns>
	public abstract TextRunProperties DefaultTextRunProperties { get; }

	/// <summary>Gets the collection of <see cref="T:System.Windows.TextDecoration" /> objects.</summary>
	/// <returns>A <see cref="T:System.Windows.TextDecorationCollection" /> value.</returns>
	public virtual TextDecorationCollection TextDecorations => null;

	/// <summary>Gets a value that controls whether text wraps when it reaches the flow edge of its containing block box.</summary>
	/// <returns>An enumerated value of <see cref="T:System.Windows.TextWrapping" />.</returns>
	public abstract TextWrapping TextWrapping { get; }

	/// <summary>Gets a value that specifies marker characteristics of the first line in the paragraph.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.TextMarkerProperties" /> value.</returns>
	public abstract TextMarkerProperties TextMarkerProperties { get; }

	/// <summary>Gets the amount of line indentation.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the amount of line indentation.</returns>
	public abstract double Indent { get; }

	/// <summary>Gets the amount of the paragraph indentation.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the amount of the paragraph indentation.</returns>
	public virtual double ParagraphIndent => 0.0;

	/// <summary>Gets the default incremental tab distance.</summary>
	/// <returns>A <see cref="T:System.Double" /> value that represents the default incremental tab distance.</returns>
	public virtual double DefaultIncrementalTab => 4.0 * DefaultTextRunProperties.FontRenderingEmSize;

	/// <summary>Gets a collection of tab definitions.</summary>
	/// <returns>A list of <see cref="T:System.Windows.Media.TextFormatting.TextTabProperties" /> objects.</returns>
	public virtual IList<TextTabProperties> Tabs => null;

	internal virtual TextLexicalService Hyphenator
	{
		[FriendAccessAllowed]
		get
		{
			return _hyphenator;
		}
		[FriendAccessAllowed]
		set
		{
			_hyphenator = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextParagraphProperties" /> class.</summary>
	protected TextParagraphProperties()
	{
	}
}
