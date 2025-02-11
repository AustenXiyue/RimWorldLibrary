using System.Collections.Generic;
using MS.Internal.FontCache;
using MS.Internal.PresentationCore;
using MS.Internal.TextFormatting;

namespace System.Windows.Media.TextFormatting;

/// <summary>Provides an abstract class for supporting formatting services to a line of text.</summary>
public abstract class TextLine : ITextMetrics, IDisposable
{
	private double _pixelsPerDip = Util.PixelsPerDip;

	public double PixelsPerDip
	{
		get
		{
			return _pixelsPerDip;
		}
		set
		{
			_pixelsPerDip = value;
		}
	}

	/// <summary>Gets a value that indicates whether content of the line overflows the specified paragraph width.</summary>
	/// <returns>true, it the line overflows the specified paragraph width; otherwise, false.</returns>
	public abstract bool HasOverflowed { get; }

	/// <summary>Gets a value that indicates whether the line is collapsed.</summary>
	/// <returns>true, if the line is collapsed; otherwise, false.</returns>
	public abstract bool HasCollapsed { get; }

	/// <summary>Determines whether the text line is truncated in the middle of a word.</summary>
	/// <returns>true if the text line is truncated in the middle of a word; otherwise, false.</returns>
	public virtual bool IsTruncated => false;

	/// <summary>Gets the total number of <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> positions of the current line.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the total number of <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> positions of the current line.</returns>
	public abstract int Length { get; }

	/// <summary>Gets the number of whitespace code points beyond the last non-blank character in a line.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the number of whitespace code points beyond the last non-blank character in a line.</returns>
	public abstract int TrailingWhitespaceLength { get; }

	/// <summary>Gets the number of characters following the last character of the line that may trigger reformatting of the current line.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the number of characters.</returns>
	public abstract int DependentLength { get; }

	/// <summary>Gets the number of newline characters at the end of a line.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the number of newline characters.</returns>
	public abstract int NewlineLength { get; }

	/// <summary>Gets the distance from the start of a paragraph to the starting point of a line.</summary>
	/// <returns>A <see cref="T:System.Double" /> value that represents the distance from the start of a paragraph to the starting point of a line.</returns>
	public abstract double Start { get; }

	/// <summary>Gets the width of a line of text, excluding trailing whitespace characters.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the text line width, excluding trailing whitespace characters.</returns>
	public abstract double Width { get; }

	/// <summary>Gets the width of a line of text, including trailing whitespace characters.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the text line width, including trailing whitespace characters.</returns>
	public abstract double WidthIncludingTrailingWhitespace { get; }

	/// <summary>Gets the height of a line of text.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the text line height.</returns>
	public abstract double Height { get; }

	/// <summary>Gets the height of the text and any other content in the line.</summary>
	/// <returns>A <see cref="T:System.Double" /> value.</returns>
	public abstract double TextHeight { get; }

	/// <summary>Gets the distance from the top-most to bottom-most black pixel in a line.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the extent distance.</returns>
	public abstract double Extent { get; }

	/// <summary>Gets the distance from the top to the baseline of the current <see cref="T:System.Windows.Media.TextFormatting.TextLine" /> object.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the baseline distance.</returns>
	public abstract double Baseline { get; }

	/// <summary>Gets the distance from the top to the baseline of the line of text.</summary>
	/// <returns>A <see cref="T:System.Double" /> value that represents the text baseline distance.</returns>
	public abstract double TextBaseline { get; }

	/// <summary>Gets the distance from the edge of the line's highest point to the baseline marker of the line.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the baseline distance of the marker.</returns>
	public abstract double MarkerBaseline { get; }

	/// <summary>Gets the height of a marker for a list item.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the height of the marker.</returns>
	public abstract double MarkerHeight { get; }

	/// <summary>Gets the distance that black pixels extend prior to the left leading alignment edge of the line. </summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the overhang leading distance.</returns>
	public abstract double OverhangLeading { get; }

	/// <summary>Gets the distance that black pixels extend following the right trailing alignment edge of the line.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the overhang trailing distance.</returns>
	public abstract double OverhangTrailing { get; }

	/// <summary>Gets the distance that black pixels extend beyond the bottom alignment edge of a line.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the overhang after distance.</returns>
	public abstract double OverhangAfter { get; }

	protected TextLine(double pixelsPerDip)
	{
		_pixelsPerDip = pixelsPerDip;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextLine" /> class.</summary>
	protected TextLine()
	{
	}

	/// <summary>Releases all managed and unmanaged resources used by the <see cref="T:System.Windows.Media.TextFormatting.TextFormatter" /> object.</summary>
	public abstract void Dispose();

	/// <summary>Renders the <see cref="T:System.Windows.Media.TextFormatting.TextLine" /> object based upon the specified <see cref="T:System.Windows.Media.DrawingContext" />.</summary>
	/// <param name="drawingContext">The <see cref="T:System.Windows.Media.DrawingContext" /> object onto which the <see cref="T:System.Windows.Media.TextFormatting.TextLine" /> is rendered.</param>
	/// <param name="origin">A <see cref="T:System.Windows.Point" /> value that represents the drawing origin.</param>
	/// <param name="inversion">An enumerated <see cref="T:System.Windows.Media.TextFormatting.InvertAxes" /> value that indicates the inversion of the horizontal and vertical axes of the drawing surface.</param>
	public abstract void Draw(DrawingContext drawingContext, Point origin, InvertAxes inversion);

	/// <summary>Create a collapsed line based on collapsed text properties.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.TextLine" /> value that represents a collapsed line that can be displayed.</returns>
	/// <param name="collapsingPropertiesList">A list of <see cref="T:System.Windows.Media.TextFormatting.TextCollapsingProperties" /> objects that represent the collapsed text properties.</param>
	public abstract TextLine Collapse(params TextCollapsingProperties[] collapsingPropertiesList);

	/// <summary>Gets a collection of collapsed text ranges after a line has been collapsed.</summary>
	/// <returns>A list of <see cref="T:System.Windows.Media.TextFormatting.TextCollapsedRange" /> objects that represent the collapsed text.</returns>
	public abstract IList<TextCollapsedRange> GetTextCollapsedRanges();

	/// <summary>Gets the character hit corresponding to the specified distance from the beginning of the line.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> object at the specified <paramref name="distance" /> from the beginning of the line.</returns>
	/// <param name="distance">A <see cref="T:System.Double" /> value that represents the distance from the beginning of the line.</param>
	public abstract CharacterHit GetCharacterHitFromDistance(double distance);

	/// <summary>Gets the distance from the beginning of the line to the specified character hit.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the distance from the beginning of the line.</returns>
	/// <param name="characterHit">The <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> object whose distance you want to query.</param>
	public abstract double GetDistanceFromCharacterHit(CharacterHit characterHit);

	/// <summary>Gets the next character hit for caret navigation.</summary>
	/// <returns>The next <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> object based on caret navigation.</returns>
	/// <param name="characterHit">The current <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> object.</param>
	public abstract CharacterHit GetNextCaretCharacterHit(CharacterHit characterHit);

	/// <summary>Gets the previous character hit for caret navigation.</summary>
	/// <returns>The previous <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> object based on caret navigation.</returns>
	/// <param name="characterHit">The current <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> object.</param>
	public abstract CharacterHit GetPreviousCaretCharacterHit(CharacterHit characterHit);

	/// <summary>Gets the previous character hit after backspacing.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> object after backspacing.</returns>
	/// <param name="characterHit">The current <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> object.</param>
	public abstract CharacterHit GetBackspaceCaretCharacterHit(CharacterHit characterHit);

	[FriendAccessAllowed]
	internal bool IsAtCaretCharacterHit(CharacterHit characterHit, int cpFirst)
	{
		if (characterHit.TrailingLength == 0)
		{
			CharacterHit characterHit2 = GetNextCaretCharacterHit(characterHit);
			if (characterHit2 == characterHit)
			{
				characterHit2 = new CharacterHit(cpFirst + Length - 1, 1);
			}
			return GetPreviousCaretCharacterHit(characterHit2) == characterHit;
		}
		CharacterHit previousCaretCharacterHit = GetPreviousCaretCharacterHit(characterHit);
		return GetNextCaretCharacterHit(previousCaretCharacterHit) == characterHit;
	}

	/// <summary>Gets an array of bounding rectangles that represent the range of characters within a text line.</summary>
	/// <returns>A list of <see cref="T:System.Windows.Media.TextFormatting.TextBounds" /> objects representing the bounding rectangle.</returns>
	/// <param name="firstTextSourceCharacterIndex">An <see cref="T:System.Int32" /> value that represents the index of first character of specified range.</param>
	/// <param name="textLength">An <see cref="T:System.Int32" /> value that represents the number of characters of the specified range.</param>
	public abstract IList<TextBounds> GetTextBounds(int firstTextSourceCharacterIndex, int textLength);

	/// <summary>Gets a collection of <see cref="T:System.Windows.Media.TextFormatting.TextRun" /> objects in a text span that are contained within a line.</summary>
	/// <returns>Gets a list of <see cref="T:System.Windows.Media.TextFormatting.TextRun" /> objects contained within a text span.</returns>
	public abstract IList<TextSpan<TextRun>> GetTextRunSpans();

	/// <summary>Gets an enumerator for enumerating <see cref="T:System.Windows.Media.TextFormatting.IndexedGlyphRun" /> objects in the <see cref="T:System.Windows.Media.TextFormatting.TextLine" />.</summary>
	/// <returns>An enumerator that allows you to enumerate each <see cref="T:System.Windows.Media.TextFormatting.IndexedGlyphRun" /> object in the <see cref="T:System.Windows.Media.TextFormatting.TextLine" />.</returns>
	public abstract IEnumerable<IndexedGlyphRun> GetIndexedGlyphRuns();

	/// <summary>Gets the state of the line when broken by line breaking process.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.TextLineBreak" /> value that represents the line break.</returns>
	public abstract TextLineBreak GetTextLineBreak();
}
