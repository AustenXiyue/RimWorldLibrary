namespace System.Windows.Media.TextFormatting;

/// <summary>Represents the range of characters and its width measurement for collapsed text within a line.</summary>
public sealed class TextCollapsedRange
{
	private int _cp;

	private int _length;

	private double _width;

	/// <summary>Gets the index to the first character in the <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> that represents collapsed text characters.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the index of the first collapsed text character.</returns>
	public int TextSourceCharacterIndex => _cp;

	/// <summary>Gets the number of characters for the collapsed text.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the number of collapsed text characters.</returns>
	public int Length => _length;

	/// <summary>The total width of collapsed text characters.</summary>
	/// <returns>A <see cref="T:System.Double" /> value that represents the width of the collapsed text characters.</returns>
	public double Width => _width;

	internal TextCollapsedRange(int cp, int length, double width)
	{
		_cp = cp;
		_length = length;
		_width = width;
	}
}
