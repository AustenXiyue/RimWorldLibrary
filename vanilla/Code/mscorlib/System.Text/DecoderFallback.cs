using System.Threading;

namespace System.Text;

/// <summary>Provides a failure-handling mechanism, called a fallback, for an encoded input byte sequence that cannot be converted to an output character. </summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public abstract class DecoderFallback
{
	internal bool bIsMicrosoftBestFitFallback;

	private static volatile DecoderFallback replacementFallback;

	private static volatile DecoderFallback exceptionFallback;

	private static object s_InternalSyncObject;

	private static object InternalSyncObject
	{
		get
		{
			if (s_InternalSyncObject == null)
			{
				object value = new object();
				Interlocked.CompareExchange<object>(ref s_InternalSyncObject, value, (object)null);
			}
			return s_InternalSyncObject;
		}
	}

	/// <summary>Gets an object that outputs a substitute string in place of an input byte sequence that cannot be decoded.</summary>
	/// <returns>A type derived from the <see cref="T:System.Text.DecoderFallback" /> class. The default value is a <see cref="T:System.Text.DecoderReplacementFallback" /> object that emits the QUESTION MARK character ("?", U+003F) in place of unknown byte sequences. </returns>
	/// <filterpriority>1</filterpriority>
	public static DecoderFallback ReplacementFallback
	{
		get
		{
			if (replacementFallback == null)
			{
				lock (InternalSyncObject)
				{
					if (replacementFallback == null)
					{
						replacementFallback = new DecoderReplacementFallback();
					}
				}
			}
			return replacementFallback;
		}
	}

	/// <summary>Gets an object that throws an exception when an input byte sequence cannot be decoded.</summary>
	/// <returns>A type derived from the <see cref="T:System.Text.DecoderFallback" /> class. The default value is a <see cref="T:System.Text.DecoderExceptionFallback" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	public static DecoderFallback ExceptionFallback
	{
		get
		{
			if (exceptionFallback == null)
			{
				lock (InternalSyncObject)
				{
					if (exceptionFallback == null)
					{
						exceptionFallback = new DecoderExceptionFallback();
					}
				}
			}
			return exceptionFallback;
		}
	}

	/// <summary>When overridden in a derived class, gets the maximum number of characters the current <see cref="T:System.Text.DecoderFallback" /> object can return.</summary>
	/// <returns>The maximum number of characters the current <see cref="T:System.Text.DecoderFallback" /> object can return.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract int MaxCharCount { get; }

	internal bool IsMicrosoftBestFitFallback => bIsMicrosoftBestFitFallback;

	/// <summary>When overridden in a derived class, initializes a new instance of the <see cref="T:System.Text.DecoderFallbackBuffer" /> class. </summary>
	/// <returns>An object that provides a fallback buffer for a decoder.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract DecoderFallbackBuffer CreateFallbackBuffer();

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.DecoderFallback" /> class. </summary>
	protected DecoderFallback()
	{
	}
}
