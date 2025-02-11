namespace System.Text;

/// <summary>Provides a failure handling mechanism, called a fallback, for an input character that cannot be converted to an output byte sequence. The fallback uses a user-specified replacement string instead of the original input character. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public sealed class EncoderReplacementFallback : EncoderFallback
{
	private string strDefault;

	/// <summary>Gets the replacement string that is the value of the <see cref="T:System.Text.EncoderReplacementFallback" /> object.</summary>
	/// <returns>A substitute string that is used in place of an input character that cannot be encoded.</returns>
	/// <filterpriority>2</filterpriority>
	public string DefaultString => strDefault;

	/// <summary>Gets the number of characters in the replacement string for the <see cref="T:System.Text.EncoderReplacementFallback" /> object.</summary>
	/// <returns>The number of characters in the string used in place of an input character that cannot be encoded.</returns>
	/// <filterpriority>2</filterpriority>
	public override int MaxCharCount => strDefault.Length;

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.EncoderReplacementFallback" /> class.</summary>
	public EncoderReplacementFallback()
		: this("?")
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.EncoderReplacementFallback" /> class using a specified replacement string.</summary>
	/// <param name="replacement">A string that is converted in an encoding operation in place of an input character that cannot be encoded.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="replacement" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="replacement" /> contains an invalid surrogate pair. In other words, the surrogate does not consist of one high surrogate component followed by one low surrogate component.</exception>
	public EncoderReplacementFallback(string replacement)
	{
		if (replacement == null)
		{
			throw new ArgumentNullException("replacement");
		}
		bool flag = false;
		for (int i = 0; i < replacement.Length; i++)
		{
			if (char.IsSurrogate(replacement, i))
			{
				if (char.IsHighSurrogate(replacement, i))
				{
					if (flag)
					{
						break;
					}
					flag = true;
					continue;
				}
				if (!flag)
				{
					flag = true;
					break;
				}
				flag = false;
			}
			else if (flag)
			{
				break;
			}
		}
		if (flag)
		{
			throw new ArgumentException(Environment.GetResourceString("String contains invalid Unicode code points.", "replacement"));
		}
		strDefault = replacement;
	}

	/// <summary>Creates a <see cref="T:System.Text.EncoderFallbackBuffer" /> object that is initialized with the replacement string of this <see cref="T:System.Text.EncoderReplacementFallback" /> object.</summary>
	/// <returns>A <see cref="T:System.Text.EncoderFallbackBuffer" /> object equal to this <see cref="T:System.Text.EncoderReplacementFallback" /> object. </returns>
	/// <filterpriority>2</filterpriority>
	public override EncoderFallbackBuffer CreateFallbackBuffer()
	{
		return new EncoderReplacementFallbackBuffer(this);
	}

	/// <summary>Indicates whether the value of a specified object is equal to the <see cref="T:System.Text.EncoderReplacementFallback" /> object.</summary>
	/// <returns>true if the <paramref name="value" /> parameter specifies an <see cref="T:System.Text.EncoderReplacementFallback" /> object and the replacement string of that object is equal to the replacement string of this <see cref="T:System.Text.EncoderReplacementFallback" /> object; otherwise, false. </returns>
	/// <param name="value">A <see cref="T:System.Text.EncoderReplacementFallback" /> object.</param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object value)
	{
		if (value is EncoderReplacementFallback encoderReplacementFallback)
		{
			return strDefault == encoderReplacementFallback.strDefault;
		}
		return false;
	}

	/// <summary>Retrieves the hash code for the value of the <see cref="T:System.Text.EncoderReplacementFallback" /> object.</summary>
	/// <returns>The hash code of the value of the object.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return strDefault.GetHashCode();
	}
}
