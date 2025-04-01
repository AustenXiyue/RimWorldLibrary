using System.Security;

namespace System.Text;

/// <summary>Represents a substitute input string that is used when the original input character cannot be encoded. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
public sealed class EncoderReplacementFallbackBuffer : EncoderFallbackBuffer
{
	private string strDefault;

	private int fallbackCount = -1;

	private int fallbackIndex = -1;

	/// <summary>Gets the number of characters in the replacement fallback buffer that remain to be processed.</summary>
	/// <returns>The number of characters in the replacement fallback buffer that have not yet been processed.</returns>
	/// <filterpriority>1</filterpriority>
	public override int Remaining
	{
		get
		{
			if (fallbackCount >= 0)
			{
				return fallbackCount;
			}
			return 0;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.EncoderReplacementFallbackBuffer" /> class using the value of a <see cref="T:System.Text.EncoderReplacementFallback" /> object.</summary>
	/// <param name="fallback">A <see cref="T:System.Text.EncoderReplacementFallback" /> object. </param>
	public EncoderReplacementFallbackBuffer(EncoderReplacementFallback fallback)
	{
		strDefault = fallback.DefaultString + fallback.DefaultString;
	}

	/// <summary>Prepares the replacement fallback buffer to use the current replacement string.</summary>
	/// <returns>true if the replacement string is not empty; false if the replacement string is empty.</returns>
	/// <param name="charUnknown">An input character. This parameter is ignored in this operation unless an exception is thrown.</param>
	/// <param name="index">The index position of the character in the input buffer. This parameter is ignored in this operation.</param>
	/// <exception cref="T:System.ArgumentException">This method is called again before the <see cref="M:System.Text.EncoderReplacementFallbackBuffer.GetNextChar" /> method has read all the characters in the replacement fallback buffer.  </exception>
	/// <filterpriority>1</filterpriority>
	public override bool Fallback(char charUnknown, int index)
	{
		if (fallbackCount >= 1)
		{
			if (char.IsHighSurrogate(charUnknown) && fallbackCount >= 0 && char.IsLowSurrogate(strDefault[fallbackIndex + 1]))
			{
				ThrowLastCharRecursive(char.ConvertToUtf32(charUnknown, strDefault[fallbackIndex + 1]));
			}
			ThrowLastCharRecursive(charUnknown);
		}
		fallbackCount = strDefault.Length / 2;
		fallbackIndex = -1;
		return fallbackCount != 0;
	}

	/// <summary>Indicates whether a replacement string can be used when an input surrogate pair cannot be encoded, or whether the surrogate pair can be ignored. Parameters specify the surrogate pair and the index position of the pair in the input.</summary>
	/// <returns>true if the replacement string is not empty; false if the replacement string is empty.</returns>
	/// <param name="charUnknownHigh">The high surrogate of the input pair.</param>
	/// <param name="charUnknownLow">The low surrogate of the input pair.</param>
	/// <param name="index">The index position of the surrogate pair in the input buffer.</param>
	/// <exception cref="T:System.ArgumentException">This method is called again before the <see cref="M:System.Text.EncoderReplacementFallbackBuffer.GetNextChar" /> method has read all the replacement string characters. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="charUnknownHigh" /> is less than U+D800 or greater than U+D8FF.-or-The value of <paramref name="charUnknownLow" /> is less than U+DC00 or greater than U+DFFF.</exception>
	/// <filterpriority>1</filterpriority>
	public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index)
	{
		if (!char.IsHighSurrogate(charUnknownHigh))
		{
			throw new ArgumentOutOfRangeException("charUnknownHigh", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 55296, 56319));
		}
		if (!char.IsLowSurrogate(charUnknownLow))
		{
			throw new ArgumentOutOfRangeException("charUnknownLow", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 56320, 57343));
		}
		if (fallbackCount >= 1)
		{
			ThrowLastCharRecursive(char.ConvertToUtf32(charUnknownHigh, charUnknownLow));
		}
		fallbackCount = strDefault.Length;
		fallbackIndex = -1;
		return fallbackCount != 0;
	}

	/// <summary>Retrieves the next character in the replacement fallback buffer.</summary>
	/// <returns>The next Unicode character in the replacement fallback buffer that the application can encode.</returns>
	/// <filterpriority>2</filterpriority>
	public override char GetNextChar()
	{
		fallbackCount--;
		fallbackIndex++;
		if (fallbackCount < 0)
		{
			return '\0';
		}
		if (fallbackCount == int.MaxValue)
		{
			fallbackCount = -1;
			return '\0';
		}
		return strDefault[fallbackIndex];
	}

	/// <summary>Causes the next call to the <see cref="M:System.Text.EncoderReplacementFallbackBuffer.GetNextChar" /> method to access the character position in the replacement fallback buffer prior to the current character position. </summary>
	/// <returns>true if the <see cref="M:System.Text.EncoderReplacementFallbackBuffer.MovePrevious" /> operation was successful; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public override bool MovePrevious()
	{
		if (fallbackCount >= -1 && fallbackIndex >= 0)
		{
			fallbackIndex--;
			fallbackCount++;
			return true;
		}
		return false;
	}

	/// <summary>Initializes all internal state information and data in this instance of <see cref="T:System.Text.EncoderReplacementFallbackBuffer" />.</summary>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe override void Reset()
	{
		fallbackCount = -1;
		fallbackIndex = 0;
		charStart = null;
		bFallingBack = false;
	}
}
