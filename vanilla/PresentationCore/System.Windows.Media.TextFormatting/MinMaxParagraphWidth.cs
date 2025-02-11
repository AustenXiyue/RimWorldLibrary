namespace System.Windows.Media.TextFormatting;

/// <summary>Represents the smallest and largest possible paragraph width that can fully contain the specified text content.</summary>
public struct MinMaxParagraphWidth : IEquatable<MinMaxParagraphWidth>
{
	private double _minWidth;

	private double _maxWidth;

	/// <summary>Gets the smallest paragraph width possible that can fully contain the specified text content.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the smallest paragraph width possible.</returns>
	public double MinWidth => _minWidth;

	/// <summary>Gets the largest paragraph width possible that can fully contain the specified text content.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the largest paragraph width possible.</returns>
	public double MaxWidth => _maxWidth;

	internal MinMaxParagraphWidth(double minWidth, double maxWidth)
	{
		_minWidth = minWidth;
		_maxWidth = maxWidth;
	}

	/// <summary>Serves as a hash function for <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" />. It is suitable for use in hashing algorithms and data structures such as a hash table.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return _minWidth.GetHashCode() ^ _maxWidth.GetHashCode();
	}

	/// <summary>Determines whether the <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object.</summary>
	/// <returns>true if <paramref name="value" /> is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> to compare with the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object.</param>
	public bool Equals(MinMaxParagraphWidth value)
	{
		return this == value;
	}

	/// <summary>Determines whether the specified object is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object.</summary>
	/// <returns>true if <paramref name="obj" /> is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object; otherwise, false. If <paramref name="obj" /> is not a <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object, false is returned.</returns>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is MinMaxParagraphWidth))
		{
			return false;
		}
		return this == (MinMaxParagraphWidth)obj;
	}

	/// <summary>Compare two <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> strings for equality.</summary>
	/// <returns>true to show the specified <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> objects are equal; otherwise, false.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> to compare.</param>
	public static bool operator ==(MinMaxParagraphWidth left, MinMaxParagraphWidth right)
	{
		if (left._minWidth == right._minWidth)
		{
			return left._maxWidth == right._maxWidth;
		}
		return false;
	}

	/// <summary>Compare two <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> objects for inequality.</summary>
	/// <returns>false to show <paramref name="left" /> is equal to <paramref name="right" />; otherwise, true.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> to compare.</param>
	public static bool operator !=(MinMaxParagraphWidth left, MinMaxParagraphWidth right)
	{
		return !(left == right);
	}
}
