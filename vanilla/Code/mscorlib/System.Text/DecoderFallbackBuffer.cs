using System.Globalization;
using System.Security;

namespace System.Text;

/// <summary>Provides a buffer that allows a fallback handler to return an alternate string to a decoder when it cannot decode an input byte sequence. </summary>
/// <filterpriority>2</filterpriority>
public abstract class DecoderFallbackBuffer
{
	[SecurityCritical]
	internal unsafe byte* byteStart;

	[SecurityCritical]
	internal unsafe char* charEnd;

	/// <summary>When overridden in a derived class, gets the number of characters in the current <see cref="T:System.Text.DecoderFallbackBuffer" /> object that remain to be processed.</summary>
	/// <returns>The number of characters in the current fallback buffer that have not yet been processed.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract int Remaining { get; }

	/// <summary>When overridden in a derived class, prepares the fallback buffer to handle the specified input byte sequence.</summary>
	/// <returns>true if the fallback buffer can process <paramref name="bytesUnknown" />; false if the fallback buffer ignores <paramref name="bytesUnknown" />.</returns>
	/// <param name="bytesUnknown">An input array of bytes.</param>
	/// <param name="index">The index position of a byte in <paramref name="bytesUnknown" />.</param>
	/// <filterpriority>1</filterpriority>
	public abstract bool Fallback(byte[] bytesUnknown, int index);

	/// <summary>When overridden in a derived class, retrieves the next character in the fallback buffer.</summary>
	/// <returns>The next character in the fallback buffer.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract char GetNextChar();

	/// <summary>When overridden in a derived class, causes the next call to the <see cref="M:System.Text.DecoderFallbackBuffer.GetNextChar" /> method to access the data buffer character position that is prior to the current character position. </summary>
	/// <returns>true if the <see cref="M:System.Text.DecoderFallbackBuffer.MovePrevious" /> operation was successful; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract bool MovePrevious();

	/// <summary>Initializes all data and state information pertaining to this fallback buffer.</summary>
	/// <filterpriority>1</filterpriority>
	public virtual void Reset()
	{
		while (GetNextChar() != 0)
		{
		}
	}

	[SecurityCritical]
	internal unsafe void InternalReset()
	{
		byteStart = null;
		Reset();
	}

	[SecurityCritical]
	internal unsafe void InternalInitialize(byte* byteStart, char* charEnd)
	{
		this.byteStart = byteStart;
		this.charEnd = charEnd;
	}

	[SecurityCritical]
	internal unsafe virtual bool InternalFallback(byte[] bytes, byte* pBytes, ref char* chars)
	{
		if (Fallback(bytes, (int)(pBytes - byteStart - bytes.Length)))
		{
			char* ptr = chars;
			bool flag = false;
			char nextChar;
			while ((nextChar = GetNextChar()) != 0)
			{
				if (char.IsSurrogate(nextChar))
				{
					if (char.IsHighSurrogate(nextChar))
					{
						if (flag)
						{
							throw new ArgumentException(Environment.GetResourceString("String contains invalid Unicode code points."));
						}
						flag = true;
					}
					else
					{
						if (!flag)
						{
							throw new ArgumentException(Environment.GetResourceString("String contains invalid Unicode code points."));
						}
						flag = false;
					}
				}
				if (ptr >= charEnd)
				{
					return false;
				}
				*(ptr++) = nextChar;
			}
			if (flag)
			{
				throw new ArgumentException(Environment.GetResourceString("String contains invalid Unicode code points."));
			}
			chars = ptr;
		}
		return true;
	}

	[SecurityCritical]
	internal unsafe virtual int InternalFallback(byte[] bytes, byte* pBytes)
	{
		if (Fallback(bytes, (int)(pBytes - byteStart - bytes.Length)))
		{
			int num = 0;
			bool flag = false;
			char nextChar;
			while ((nextChar = GetNextChar()) != 0)
			{
				if (char.IsSurrogate(nextChar))
				{
					if (char.IsHighSurrogate(nextChar))
					{
						if (flag)
						{
							throw new ArgumentException(Environment.GetResourceString("String contains invalid Unicode code points."));
						}
						flag = true;
					}
					else
					{
						if (!flag)
						{
							throw new ArgumentException(Environment.GetResourceString("String contains invalid Unicode code points."));
						}
						flag = false;
					}
				}
				num++;
			}
			if (flag)
			{
				throw new ArgumentException(Environment.GetResourceString("String contains invalid Unicode code points."));
			}
			return num;
		}
		return 0;
	}

	internal void ThrowLastBytesRecursive(byte[] bytesUnknown)
	{
		StringBuilder stringBuilder = new StringBuilder(bytesUnknown.Length * 3);
		int i;
		for (i = 0; i < bytesUnknown.Length && i < 20; i++)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "\\x{0:X2}", bytesUnknown[i]));
		}
		if (i == 20)
		{
			stringBuilder.Append(" ...");
		}
		throw new ArgumentException(Environment.GetResourceString("Recursive fallback not allowed for bytes {0}.", stringBuilder.ToString()), "bytesUnknown");
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.DecoderFallbackBuffer" /> class. </summary>
	protected DecoderFallbackBuffer()
	{
	}
}
