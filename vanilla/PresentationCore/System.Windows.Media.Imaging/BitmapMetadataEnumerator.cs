using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

internal struct BitmapMetadataEnumerator : IEnumerator<string>, IEnumerator, IDisposable
{
	private SafeMILHandle _enumeratorHandle;

	private string _current;

	private bool _fStarted;

	object IEnumerator.Current => Current;

	public string Current
	{
		get
		{
			if (_current == null)
			{
				if (!_fStarted)
				{
					throw new InvalidOperationException(SR.Enumerator_NotStarted);
				}
				throw new InvalidOperationException(SR.Enumerator_ReachedEnd);
			}
			return _current;
		}
	}

	public bool MoveNext()
	{
		if (_fStarted && _current == null)
		{
			return false;
		}
		_fStarted = true;
		nint rgElt = IntPtr.Zero;
		int pceltFetched = 0;
		try
		{
			int hr = UnsafeNativeMethods.EnumString.Next(_enumeratorHandle, 1, ref rgElt, ref pceltFetched);
			if (HRESULT.IsWindowsCodecError(hr))
			{
				_current = null;
				return false;
			}
			HRESULT.Check(hr);
			if (pceltFetched == 0)
			{
				_current = null;
				return false;
			}
			_current = Marshal.PtrToStringUni(rgElt);
		}
		finally
		{
			if (rgElt != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(rgElt);
				rgElt = IntPtr.Zero;
			}
		}
		return true;
	}

	public void Reset()
	{
		HRESULT.Check(UnsafeNativeMethods.EnumString.Reset(_enumeratorHandle));
		_current = null;
		_fStarted = false;
	}

	void IDisposable.Dispose()
	{
	}

	internal BitmapMetadataEnumerator(SafeMILHandle metadataHandle)
	{
		HRESULT.Check(UnsafeNativeMethods.WICMetadataQueryReader.GetEnumerator(metadataHandle, out _enumeratorHandle));
		_current = null;
		_fStarted = false;
	}
}
