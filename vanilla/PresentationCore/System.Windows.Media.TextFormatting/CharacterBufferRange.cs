using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.TextFormatting;

/// <summary>Describes a string of characters.</summary>
public struct CharacterBufferRange : IEquatable<CharacterBufferRange>
{
	private CharacterBufferReference _charBufferRef;

	private int _length;

	/// <summary>Gets a reference to the character buffer of a string.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> value representing the character buffer of a string.</returns>
	public CharacterBufferReference CharacterBufferReference => _charBufferRef;

	/// <summary>Gets the number of characters in the text source character store.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the total number of characters.</returns>
	public int Length => _length;

	/// <summary>Gets an empty character string.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> object whose <see cref="P:System.Windows.Media.TextFormatting.CharacterBufferRange.Length" /> is equal to 0.</returns>
	public static CharacterBufferRange Empty => default(CharacterBufferRange);

	internal bool IsEmpty
	{
		get
		{
			if (_charBufferRef.CharacterBuffer != null)
			{
				return _length <= 0;
			}
			return true;
		}
	}

	internal CharacterBuffer CharacterBuffer => _charBufferRef.CharacterBuffer;

	internal int OffsetToFirstChar => _charBufferRef.OffsetToFirstChar;

	internal char this[int index]
	{
		get
		{
			Invariant.Assert(index >= 0 && index < _length);
			return _charBufferRef.CharacterBuffer[_charBufferRef.OffsetToFirstChar + index];
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> structure from a character array.</summary>
	/// <param name="characterArray">The character array.</param>
	/// <param name="offsetToFirstChar">The character buffer offset to the first character.</param>
	/// <param name="characterLength">The number of characters in <paramref name="characterArray" /> to use.</param>
	public CharacterBufferRange(char[] characterArray, int offsetToFirstChar, int characterLength)
		: this(new CharacterBufferReference(characterArray, offsetToFirstChar), characterLength)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> structure from a character string.</summary>
	/// <param name="characterString">The character string.</param>
	/// <param name="offsetToFirstChar">The character buffer offset to the first character.</param>
	/// <param name="characterLength">The number of characters in <paramref name="characterString" /> to use.</param>
	public CharacterBufferRange(string characterString, int offsetToFirstChar, int characterLength)
		: this(new CharacterBufferReference(characterString, offsetToFirstChar), characterLength)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> structure from a unmanaged character string.</summary>
	/// <param name="unsafeCharacterString">An unmanaged pointer reference to a character string.</param>
	/// <param name="characterLength">The number of characters in <paramref name="unsafecharacterString" /> to use.</param>
	[CLSCompliant(false)]
	public unsafe CharacterBufferRange(char* unsafeCharacterString, int characterLength)
		: this(new CharacterBufferReference(unsafeCharacterString, characterLength), characterLength)
	{
	}

	internal CharacterBufferRange(CharacterBufferReference characterBufferReference, int characterLength)
	{
		if (characterLength < 0)
		{
			throw new ArgumentOutOfRangeException("characterLength", SR.ParameterCannotBeNegative);
		}
		int num = ((characterBufferReference.CharacterBuffer != null) ? (characterBufferReference.CharacterBuffer.Count - characterBufferReference.OffsetToFirstChar) : 0);
		if (characterLength > num)
		{
			throw new ArgumentOutOfRangeException("characterLength", SR.Format(SR.ParameterCannotBeGreaterThan, num));
		}
		_charBufferRef = characterBufferReference;
		_length = characterLength;
	}

	internal CharacterBufferRange(CharacterBufferRange characterBufferRange, int offsetToFirstChar, int characterLength)
		: this(characterBufferRange.CharacterBuffer, characterBufferRange.OffsetToFirstChar + offsetToFirstChar, characterLength)
	{
	}

	internal CharacterBufferRange(string charString)
		: this(new StringCharacterBuffer(charString), 0, charString.Length)
	{
	}

	internal CharacterBufferRange(CharacterBuffer charBuffer, int offsetToFirstChar, int characterLength)
		: this(new CharacterBufferReference(charBuffer, offsetToFirstChar), characterLength)
	{
	}

	internal CharacterBufferRange(TextRun textRun)
	{
		_charBufferRef = textRun.CharacterBufferReference;
		_length = textRun.Length;
	}

	/// <summary>Serves as a hash function for <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" />. It is suitable for use in hashing algorithms and data structures such as a hash table.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return _charBufferRef.GetHashCode() ^ _length;
	}

	/// <summary>Determines whether the specified object is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> object.</summary>
	/// <returns>true if <paramref name="o" /> is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> object; otherwise, false. If <paramref name="o" /> is not a <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> object, false is returned.</returns>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> object.</param>
	public override bool Equals(object obj)
	{
		if (obj is CharacterBufferRange)
		{
			return Equals((CharacterBufferRange)obj);
		}
		return false;
	}

	/// <summary>Determines whether the <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> object is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> object.</summary>
	/// <returns>true if <paramref name="value" /> is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> object; otherwise, false. If <paramref name="value" /> is not a <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> object, false is returned.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> to compare with the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> object.</param>
	public bool Equals(CharacterBufferRange value)
	{
		if (_charBufferRef.Equals(value._charBufferRef))
		{
			return _length == value._length;
		}
		return false;
	}

	/// <summary>Compare two <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> strings for equality.</summary>
	/// <returns>true to show the specified <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> objects are equal; otherwise, false.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> to compare.</param>
	public static bool operator ==(CharacterBufferRange left, CharacterBufferRange right)
	{
		return left.Equals(right);
	}

	/// <summary>Compare two <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> strings for inequality.</summary>
	/// <returns>false to show <paramref name="left" /> is equal to <paramref name="right" />; otherwise, true.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferRange" /> to compare.</param>
	public static bool operator !=(CharacterBufferRange left, CharacterBufferRange right)
	{
		return !(left == right);
	}
}
