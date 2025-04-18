using System.Runtime.Serialization;

namespace System.Text;

/// <summary>The exception that is thrown when a decoder fallback operation fails. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public sealed class DecoderFallbackException : ArgumentException
{
	private byte[] bytesUnknown;

	private int index;

	/// <summary>Gets the input byte sequence that caused the exception.</summary>
	/// <returns>The input byte array that cannot be decoded. </returns>
	/// <filterpriority>2</filterpriority>
	public byte[] BytesUnknown => bytesUnknown;

	/// <summary>Gets the index position in the input byte sequence of the byte that caused the exception.</summary>
	/// <returns>The index position in the input byte array of the byte that cannot be decoded. The index position is zero-based. </returns>
	/// <filterpriority>1</filterpriority>
	public int Index => index;

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.DecoderFallbackException" /> class. </summary>
	public DecoderFallbackException()
		: base(Environment.GetResourceString("Value does not fall within the expected range."))
	{
		SetErrorCode(-2147024809);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.DecoderFallbackException" /> class. A parameter specifies the error message.</summary>
	/// <param name="message">An error message.</param>
	public DecoderFallbackException(string message)
		: base(message)
	{
		SetErrorCode(-2147024809);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.DecoderFallbackException" /> class. Parameters specify the error message and the inner exception that is the cause of this exception.</summary>
	/// <param name="message">An error message.</param>
	/// <param name="innerException">The exception that caused this exception.</param>
	public DecoderFallbackException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2147024809);
	}

	internal DecoderFallbackException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.DecoderFallbackException" /> class. Parameters specify the error message, the array of bytes being decoded, and the index of the byte that cannot be decoded.</summary>
	/// <param name="message">An error message.</param>
	/// <param name="bytesUnknown">The input byte array.</param>
	/// <param name="index">The index position in <paramref name="bytesUnknown" /> of the byte that cannot be decoded.</param>
	public DecoderFallbackException(string message, byte[] bytesUnknown, int index)
		: base(message)
	{
		this.bytesUnknown = bytesUnknown;
		this.index = index;
	}
}
