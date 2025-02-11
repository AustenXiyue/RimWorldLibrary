using System.Security;

namespace System.Text;

/// <summary>Provides a buffer that allows a fallback handler to return an alternate string to an encoder when it cannot encode an input character. </summary>
/// <filterpriority>2</filterpriority>
public abstract class EncoderFallbackBuffer
{
	[SecurityCritical]
	internal unsafe char* charStart;

	[SecurityCritical]
	internal unsafe char* charEnd;

	internal EncoderNLS encoder;

	internal bool setEncoder;

	internal bool bUsedEncoder;

	internal bool bFallingBack;

	internal int iRecursionCount;

	private const int iMaxRecursion = 250;

	/// <summary>When overridden in a derived class, gets the number of characters in the current <see cref="T:System.Text.EncoderFallbackBuffer" /> object that remain to be processed.</summary>
	/// <returns>The number of characters in the current fallback buffer that have not yet been processed.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract int Remaining { get; }

	/// <summary>When overridden in a derived class, prepares the fallback buffer to handle the specified input character. </summary>
	/// <returns>true if the fallback buffer can process <paramref name="charUnknown" />; false if the fallback buffer ignores <paramref name="charUnknown" />.</returns>
	/// <param name="charUnknown">An input character.</param>
	/// <param name="index">The index position of the character in the input buffer.</param>
	/// <filterpriority>1</filterpriority>
	public abstract bool Fallback(char charUnknown, int index);

	/// <summary>When overridden in a derived class, prepares the fallback buffer to handle the specified surrogate pair.</summary>
	/// <returns>true if the fallback buffer can process <paramref name="charUnknownHigh" /> and <paramref name="charUnknownLow" />; false if the fallback buffer ignores the surrogate pair.</returns>
	/// <param name="charUnknownHigh">The high surrogate of the input pair.</param>
	/// <param name="charUnknownLow">The low surrogate of the input pair.</param>
	/// <param name="index">The index position of the surrogate pair in the input buffer.</param>
	/// <filterpriority>1</filterpriority>
	public abstract bool Fallback(char charUnknownHigh, char charUnknownLow, int index);

	/// <summary>When overridden in a derived class, retrieves the next character in the fallback buffer.</summary>
	/// <returns>The next character in the fallback buffer.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract char GetNextChar();

	/// <summary>When overridden in a derived class, causes the next call to the <see cref="M:System.Text.EncoderFallbackBuffer.GetNextChar" /> method to access the data buffer character position that is prior to the current character position. </summary>
	/// <returns>true if the <see cref="M:System.Text.EncoderFallbackBuffer.MovePrevious" /> operation was successful; otherwise, false.</returns>
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
		charStart = null;
		bFallingBack = false;
		iRecursionCount = 0;
		Reset();
	}

	[SecurityCritical]
	internal unsafe void InternalInitialize(char* charStart, char* charEnd, EncoderNLS encoder, bool setEncoder)
	{
		this.charStart = charStart;
		this.charEnd = charEnd;
		this.encoder = encoder;
		this.setEncoder = setEncoder;
		bUsedEncoder = false;
		bFallingBack = false;
		iRecursionCount = 0;
	}

	internal char InternalGetNextChar()
	{
		char nextChar = GetNextChar();
		bFallingBack = nextChar != '\0';
		if (nextChar == '\0')
		{
			iRecursionCount = 0;
		}
		return nextChar;
	}

	[SecurityCritical]
	internal unsafe virtual bool InternalFallback(char ch, ref char* chars)
	{
		int index = (int)(chars - charStart) - 1;
		if (char.IsHighSurrogate(ch))
		{
			if (chars >= charEnd)
			{
				if (encoder != null && !encoder.MustFlush)
				{
					if (setEncoder)
					{
						bUsedEncoder = true;
						encoder.charLeftOver = ch;
					}
					bFallingBack = false;
					return false;
				}
			}
			else
			{
				char c = *chars;
				if (char.IsLowSurrogate(c))
				{
					if (bFallingBack && iRecursionCount++ > 250)
					{
						ThrowLastCharRecursive(char.ConvertToUtf32(ch, c));
					}
					chars++;
					bFallingBack = Fallback(ch, c, index);
					return bFallingBack;
				}
			}
		}
		if (bFallingBack && iRecursionCount++ > 250)
		{
			ThrowLastCharRecursive(ch);
		}
		bFallingBack = Fallback(ch, index);
		return bFallingBack;
	}

	internal void ThrowLastCharRecursive(int charRecursive)
	{
		throw new ArgumentException(Environment.GetResourceString("Recursive fallback not allowed for character \\\\u{0:X4}.", charRecursive), "chars");
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.EncoderFallbackBuffer" /> class.</summary>
	protected EncoderFallbackBuffer()
	{
	}
}
