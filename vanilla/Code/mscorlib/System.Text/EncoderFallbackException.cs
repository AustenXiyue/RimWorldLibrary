using System.Runtime.Serialization;

namespace System.Text;

/// <summary>The exception that is thrown when an encoder fallback operation fails. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public sealed class EncoderFallbackException : ArgumentException
{
	private char charUnknown;

	private char charUnknownHigh;

	private char charUnknownLow;

	private int index;

	/// <summary>Gets the input character that caused the exception.</summary>
	/// <returns>The character that cannot be encoded.</returns>
	/// <filterpriority>2</filterpriority>
	public char CharUnknown => charUnknown;

	/// <summary>Gets the high component character of the surrogate pair that caused the exception.</summary>
	/// <returns>The high component character of the surrogate pair that cannot be encoded.</returns>
	/// <filterpriority>2</filterpriority>
	public char CharUnknownHigh => charUnknownHigh;

	/// <summary>Gets the low component character of the surrogate pair that caused the exception.</summary>
	/// <returns>The low component character of the surrogate pair that cannot be encoded.</returns>
	/// <filterpriority>2</filterpriority>
	public char CharUnknownLow => charUnknownLow;

	/// <summary>Gets the index position in the input buffer of the character that caused the exception.</summary>
	/// <returns>The index position in the input buffer of the character that cannot be encoded.</returns>
	/// <filterpriority>1</filterpriority>
	public int Index => index;

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.EncoderFallbackException" /> class.</summary>
	public EncoderFallbackException()
		: base(Environment.GetResourceString("Value does not fall within the expected range."))
	{
		SetErrorCode(-2147024809);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.EncoderFallbackException" /> class. A parameter specifies the error message.</summary>
	/// <param name="message">An error message.</param>
	public EncoderFallbackException(string message)
		: base(message)
	{
		SetErrorCode(-2147024809);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.EncoderFallbackException" /> class. Parameters specify the error message and the inner exception that is the cause of this exception.</summary>
	/// <param name="message">An error message.</param>
	/// <param name="innerException">The exception that caused this exception.</param>
	public EncoderFallbackException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2147024809);
	}

	internal EncoderFallbackException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	internal EncoderFallbackException(string message, char charUnknown, int index)
		: base(message)
	{
		this.charUnknown = charUnknown;
		this.index = index;
	}

	internal EncoderFallbackException(string message, char charUnknownHigh, char charUnknownLow, int index)
		: base(message)
	{
		if (!char.IsHighSurrogate(charUnknownHigh))
		{
			throw new ArgumentOutOfRangeException("charUnknownHigh", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 55296, 56319));
		}
		if (!char.IsLowSurrogate(charUnknownLow))
		{
			throw new ArgumentOutOfRangeException("charUnknownLow", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 56320, 57343));
		}
		this.charUnknownHigh = charUnknownHigh;
		this.charUnknownLow = charUnknownLow;
		this.index = index;
	}

	/// <summary>Indicates whether the input that caused the exception is a surrogate pair.</summary>
	/// <returns>true if the input was a surrogate pair; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsUnknownSurrogate()
	{
		return charUnknownHigh != '\0';
	}
}
