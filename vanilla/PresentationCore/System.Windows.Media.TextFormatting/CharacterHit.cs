namespace System.Windows.Media.TextFormatting;

/// <summary>Represents information about a character hit within a glyph run.</summary>
public struct CharacterHit : IEquatable<CharacterHit>
{
	private int _firstCharacterIndex;

	private int _trailingLength;

	/// <summary>Gets the index of the first character that got hit.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the index.</returns>
	public int FirstCharacterIndex => _firstCharacterIndex;

	/// <summary>Gets the trailing length value for the character that got hit.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the trailing length.</returns>
	public int TrailingLength => _trailingLength;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.CharacterHit" /> structure.</summary>
	/// <param name="firstCharacterIndex">Index of the first character that got hit.</param>
	/// <param name="trailingLength">In the case of a leading edge, this value is 0. In the case of a trailing edge, this value is the number of code points until the next valid caret position.</param>
	public CharacterHit(int firstCharacterIndex, int trailingLength)
	{
		_firstCharacterIndex = firstCharacterIndex;
		_trailingLength = trailingLength;
	}

	/// <summary>Compare two <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> strings for equality.</summary>
	/// <returns>true when the values of <see cref="P:System.Windows.Media.TextFormatting.CharacterHit.FirstCharacterIndex" /> and <see cref="P:System.Windows.Media.TextFormatting.CharacterHit.TrailingLength" /> properties are equal for both objects; otherwise, false.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> to compare.</param>
	public static bool operator ==(CharacterHit left, CharacterHit right)
	{
		if (left._firstCharacterIndex == right._firstCharacterIndex)
		{
			return left._trailingLength == right._trailingLength;
		}
		return false;
	}

	/// <summary>Compare two <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> strings for inequality.</summary>
	/// <returns>false when the values of <see cref="P:System.Windows.Media.TextFormatting.CharacterHit.FirstCharacterIndex" /> and <see cref="P:System.Windows.Media.TextFormatting.CharacterHit.TrailingLength" /> properties are equal for both objects; otherwise, true.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> to compare.</param>
	public static bool operator !=(CharacterHit left, CharacterHit right)
	{
		return !(left == right);
	}

	/// <summary>Determines whether the <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object.</summary>
	/// <returns>true if <paramref name="obj" /> is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> to compare with the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object.</param>
	public bool Equals(CharacterHit obj)
	{
		return this == obj;
	}

	/// <summary>Determines whether the specified object is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object.</summary>
	/// <returns>true if <paramref name="obj" /> is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object; otherwise, false. If <paramref name="obj" /> is not a <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object, false is returned.</returns>
	/// <param name="obj">The object to compare with the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is CharacterHit))
		{
			return false;
		}
		return this == (CharacterHit)obj;
	}

	/// <summary>Serves as a hash function for <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" />. It is suitable for use in hashing algorithms and data structures such as a hash table.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return _firstCharacterIndex.GetHashCode() ^ _trailingLength.GetHashCode();
	}
}
