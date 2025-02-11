using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Media.TextFormatting;
using MS.Internal;

namespace System.Windows.Documents;

internal class NaturalLanguageHyphenator : TextLexicalService, IDisposable
{
	private class HyphenBreaks : TextLexicalBreaks
	{
		private byte[] _isHyphenPositions;

		private int _numPositions;

		private bool this[int index] => (_isHyphenPositions[index / 8] & (1 << index % 8)) != 0;

		public override int Length => _numPositions;

		internal HyphenBreaks(byte[] isHyphenPositions, int numPositions)
		{
			_isHyphenPositions = isHyphenPositions;
			_numPositions = numPositions;
		}

		public override int GetNextBreak(int currentIndex)
		{
			if (_isHyphenPositions != null && currentIndex >= 0)
			{
				int i;
				for (i = currentIndex + 1; i < _numPositions && !this[i]; i++)
				{
				}
				if (i < _numPositions)
				{
					return i;
				}
			}
			return -1;
		}

		public override int GetPreviousBreak(int currentIndex)
		{
			if (_isHyphenPositions != null && currentIndex < _numPositions)
			{
				int num = currentIndex;
				while (num > 0 && !this[num])
				{
					num--;
				}
				if (num > 0)
				{
					return num;
				}
			}
			return -1;
		}
	}

	private static class UnsafeNativeMethods
	{
		[DllImport("PresentationNative_cor3.dll", PreserveSig = false)]
		internal static extern nint NlCreateHyphenator();

		[DllImport("PresentationNative_cor3.dll")]
		internal static extern void NlDestroyHyphenator(ref nint hyphenator);

		[DllImport("PresentationNative_cor3.dll", PreserveSig = false)]
		internal static extern void NlHyphenate(nint hyphenator, [In][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeParamIndex = 2)] char[] inputText, int textLength, int localeID, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] byte[] hyphenBreaks, int numPositions);
	}

	private nint _hyphenatorResource;

	private bool _disposed;

	internal NaturalLanguageHyphenator()
	{
		try
		{
			_hyphenatorResource = UnsafeNativeMethods.NlCreateHyphenator();
		}
		catch (DllNotFoundException)
		{
		}
		catch (EntryPointNotFoundException)
		{
		}
	}

	~NaturalLanguageHyphenator()
	{
		CleanupInternal(finalizing: true);
	}

	void IDisposable.Dispose()
	{
		GC.SuppressFinalize(this);
		CleanupInternal(finalizing: false);
	}

	private void CleanupInternal(bool finalizing)
	{
		if (!_disposed && _hyphenatorResource != IntPtr.Zero)
		{
			UnsafeNativeMethods.NlDestroyHyphenator(ref _hyphenatorResource);
			_disposed = true;
		}
	}

	public override bool IsCultureSupported(CultureInfo culture)
	{
		return true;
	}

	public override TextLexicalBreaks AnalyzeText(char[] characterSource, int length, CultureInfo textCulture)
	{
		Invariant.Assert(characterSource != null && characterSource.Length != 0 && length > 0 && length <= characterSource.Length);
		if (_hyphenatorResource == IntPtr.Zero)
		{
			return null;
		}
		if (_disposed)
		{
			throw new ObjectDisposedException(SR.HyphenatorDisposed);
		}
		byte[] array = new byte[(length + 7) / 8];
		UnsafeNativeMethods.NlHyphenate(_hyphenatorResource, characterSource, length, (textCulture != null && textCulture != CultureInfo.InvariantCulture) ? textCulture.LCID : 0, array, array.Length);
		return new HyphenBreaks(array, length);
	}
}
