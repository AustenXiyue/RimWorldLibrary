using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.TextFormatting;

/// <summary>Describes a character buffer for a text run.</summary>
public struct CharacterBufferReference : IEquatable<CharacterBufferReference>
{
	private CharacterBuffer _charBuffer;

	private int _offsetToFirstChar;

	internal CharacterBuffer CharacterBuffer => _charBuffer;

	internal int OffsetToFirstChar => _offsetToFirstChar;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> structure using a specified character array.</summary>
	/// <param name="characterArray">The <see cref="T:System.Char" /> array.</param>
	/// <param name="offsetToFirstChar">The offset to the first character to use in <paramref name="characterArray" />.</param>
	public CharacterBufferReference(char[] characterArray, int offsetToFirstChar)
		: this(new CharArrayCharacterBuffer(characterArray), offsetToFirstChar)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> structure using a specified character string.</summary>
	/// <param name="characterString">The <see cref="T:System.String" /> containing the text characters.</param>
	/// <param name="offsetToFirstChar">The offset to the first character to use in <paramref name="characterString" />.</param>
	public CharacterBufferReference(string characterString, int offsetToFirstChar)
		: this(new StringCharacterBuffer(characterString), offsetToFirstChar)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> structure using a specified unsafe character string.</summary>
	/// <param name="unsafeCharacterString">Pointer to character string.</param>
	/// <param name="characterLength">The length of <paramref name="unsafeCharacterString" />.</param>
	[CLSCompliant(false)]
	public unsafe CharacterBufferReference(char* unsafeCharacterString, int characterLength)
		: this(new UnsafeStringCharacterBuffer(unsafeCharacterString, characterLength), 0)
	{
	}

	internal CharacterBufferReference(CharacterBuffer charBuffer, int offsetToFirstChar)
	{
		if (offsetToFirstChar < 0)
		{
			throw new ArgumentOutOfRangeException("offsetToFirstChar", SR.ParameterCannotBeNegative);
		}
		int num = ((charBuffer != null) ? Math.Max(0, charBuffer.Count - 1) : 0);
		if (offsetToFirstChar > num)
		{
			throw new ArgumentOutOfRangeException("offsetToFirstChar", SR.Format(SR.ParameterCannotBeGreaterThan, num));
		}
		_charBuffer = charBuffer;
		_offsetToFirstChar = offsetToFirstChar;
	}

	/// <summary>Serves as a hash function for <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" />. It is suitable for use in hashing algorithms and data structures such as a hash table.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the hash code for the current object.</returns>
	public override int GetHashCode()
	{
		if (_charBuffer == null)
		{
			return 0;
		}
		return _charBuffer.GetHashCode() ^ _offsetToFirstChar;
	}

	/// <summary>Determines whether the specified object is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object.</summary>
	/// <returns>true if <paramref name="obj" /> is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object; otherwise, false. If <paramref name="obj" /> is not a <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object, false is returned.</returns>
	/// <param name="obj">The object to compare with the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object.</param>
	public override bool Equals(object obj)
	{
		if (obj is CharacterBufferReference)
		{
			return Equals((CharacterBufferReference)obj);
		}
		return false;
	}

	/// <summary>Determines whether the <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object.</summary>
	/// <returns>true if <paramref name="value" /> is equal to the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> to compare with the current <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> object.</param>
	public bool Equals(CharacterBufferReference value)
	{
		if (_charBuffer == value._charBuffer)
		{
			return _offsetToFirstChar == value._offsetToFirstChar;
		}
		return false;
	}

	/// <summary>Compare two <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> strings for equality.</summary>
	/// <returns>true to show the specified <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> objects are equal; otherwise, false.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> to compare.</param>
	public static bool operator ==(CharacterBufferReference left, CharacterBufferReference right)
	{
		return left.Equals(right);
	}

	/// <summary>Compare two <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> strings for inequality.</summary>
	/// <returns>false to show <paramref name="left" /> is equal to <paramref name="right" />; otherwise, true.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> to compare.</param>
	public static bool operator !=(CharacterBufferReference left, CharacterBufferReference right)
	{
		return !(left == right);
	}
}
