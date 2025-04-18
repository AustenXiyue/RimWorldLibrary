using System.Globalization;

namespace System.Text;

/// <summary>Throws <see cref="T:System.Text.DecoderFallbackException" /> when an encoded input byte sequence cannot be converted to a decoded output character. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
public sealed class DecoderExceptionFallbackBuffer : DecoderFallbackBuffer
{
	/// <summary>Gets the number of characters in the current <see cref="T:System.Text.DecoderExceptionFallbackBuffer" /> object that remain to be processed.</summary>
	/// <returns>The return value is always zero. A return value is defined, although it is unchanging, because this method implements an abstract method.</returns>
	/// <filterpriority>1</filterpriority>
	public override int Remaining => 0;

	/// <summary>Throws <see cref="T:System.Text.DecoderFallbackException" /> when the input byte sequence cannot be decoded. The nominal return value is not used. </summary>
	/// <returns>None. No value is returned because the <see cref="M:System.Text.DecoderExceptionFallbackBuffer.Fallback(System.Byte[],System.Int32)" /> method always throws an exception. The nominal return value is true. A return value is defined, although it is unchanging, because this method implements an abstract method.</returns>
	/// <param name="bytesUnknown">An input array of bytes.</param>
	/// <param name="index">The index position of a byte in the input.</param>
	/// <exception cref="T:System.Text.DecoderFallbackException">This method always throws an exception that reports the value and index position of the input byte that cannot be decoded. </exception>
	/// <filterpriority>1</filterpriority>
	public override bool Fallback(byte[] bytesUnknown, int index)
	{
		Throw(bytesUnknown, index);
		return true;
	}

	/// <summary>Retrieves the next character in the exception data buffer.</summary>
	/// <returns>The return value is always the Unicode character NULL (U+0000). A return value is defined, although it is unchanging, because this method implements an abstract method.</returns>
	/// <filterpriority>2</filterpriority>
	public override char GetNextChar()
	{
		return '\0';
	}

	/// <summary>Causes the next call to <see cref="M:System.Text.DecoderExceptionFallbackBuffer.GetNextChar" /> to access the exception data buffer character position that is prior to the current position.</summary>
	/// <returns>The return value is always false. A return value is defined, although it is unchanging, because this method implements an abstract method.</returns>
	/// <filterpriority>1</filterpriority>
	public override bool MovePrevious()
	{
		return false;
	}

	private void Throw(byte[] bytesUnknown, int index)
	{
		StringBuilder stringBuilder = new StringBuilder(bytesUnknown.Length * 3);
		int i;
		for (i = 0; i < bytesUnknown.Length && i < 20; i++)
		{
			stringBuilder.Append("[");
			stringBuilder.Append(bytesUnknown[i].ToString("X2", CultureInfo.InvariantCulture));
			stringBuilder.Append("]");
		}
		if (i == 20)
		{
			stringBuilder.Append(" ...");
		}
		throw new DecoderFallbackException(Environment.GetResourceString("Unable to translate bytes {0} at index {1} from specified code page to Unicode.", stringBuilder, index), bytesUnknown, index);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.DecoderExceptionFallbackBuffer" /> class. </summary>
	public DecoderExceptionFallbackBuffer()
	{
	}
}
