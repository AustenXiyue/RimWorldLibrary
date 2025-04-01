using System.Security;

namespace System.Text;

/// <summary>Represents a substitute output string that is emitted when the original input byte sequence cannot be decoded. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
public sealed class DecoderReplacementFallbackBuffer : DecoderFallbackBuffer
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.DecoderReplacementFallbackBuffer" /> class using the value of a <see cref="T:System.Text.DecoderReplacementFallback" /> object.</summary>
	/// <param name="fallback">A <see cref="T:System.Text.DecoderReplacementFallback" /> object that contains a replacement string. </param>
	public DecoderReplacementFallbackBuffer(DecoderReplacementFallback fallback)
	{
		strDefault = fallback.DefaultString;
	}

	/// <summary>Prepares the replacement fallback buffer to use the current replacement string.</summary>
	/// <returns>true if the replacement string is not empty; false if the replacement string is empty.</returns>
	/// <param name="bytesUnknown">An input byte sequence. This parameter is ignored unless an exception is thrown.</param>
	/// <param name="index">The index position of the byte in <paramref name="bytesUnknown" />. This parameter is ignored in this operation.</param>
	/// <exception cref="T:System.ArgumentException">This method is called again before the <see cref="M:System.Text.DecoderReplacementFallbackBuffer.GetNextChar" /> method has read all the characters in the replacement fallback buffer.  </exception>
	/// <filterpriority>1</filterpriority>
	public override bool Fallback(byte[] bytesUnknown, int index)
	{
		if (fallbackCount >= 1)
		{
			ThrowLastBytesRecursive(bytesUnknown);
		}
		if (strDefault.Length == 0)
		{
			return false;
		}
		fallbackCount = strDefault.Length;
		fallbackIndex = -1;
		return true;
	}

	/// <summary>Retrieves the next character in the replacement fallback buffer.</summary>
	/// <returns>The next character in the replacement fallback buffer.</returns>
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

	/// <summary>Causes the next call to <see cref="M:System.Text.DecoderReplacementFallbackBuffer.GetNextChar" /> to access the character position in the replacement fallback buffer prior to the current character position.</summary>
	/// <returns>true if the <see cref="M:System.Text.DecoderReplacementFallbackBuffer.MovePrevious" /> operation was successful; otherwise, false.</returns>
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

	/// <summary>Initializes all internal state information and data in the <see cref="T:System.Text.DecoderReplacementFallbackBuffer" /> object.</summary>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe override void Reset()
	{
		fallbackCount = -1;
		fallbackIndex = -1;
		byteStart = null;
	}

	[SecurityCritical]
	internal unsafe override int InternalFallback(byte[] bytes, byte* pBytes)
	{
		return strDefault.Length;
	}
}
