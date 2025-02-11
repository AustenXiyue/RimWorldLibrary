using System.ComponentModel;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Imaging;

internal class BitmapInitialize : ISupportInitialize
{
	private bool _inInit;

	private bool _isInitialized;

	public bool IsInInit => _inInit;

	public bool IsInitAtLeastOnce => _isInitialized;

	public void BeginInit()
	{
		if (IsInitAtLeastOnce)
		{
			throw new InvalidOperationException(SR.Format(SR.Image_OnlyOneInit, null));
		}
		if (IsInInit)
		{
			throw new InvalidOperationException(SR.Format(SR.Image_InInitialize, null));
		}
		_inInit = true;
	}

	public void EndInit()
	{
		if (!IsInInit)
		{
			throw new InvalidOperationException(SR.Format(SR.Image_EndInitWithoutBeginInit, null));
		}
		_inInit = false;
		_isInitialized = true;
	}

	public void SetPrologue()
	{
		if (!IsInInit)
		{
			throw new InvalidOperationException(SR.Format(SR.Image_SetPropertyOutsideBeginEndInit, null));
		}
	}

	public void EnsureInitializedComplete()
	{
		if (IsInInit)
		{
			throw new InvalidOperationException(SR.Format(SR.Image_InitializationIncomplete, null));
		}
		if (!IsInitAtLeastOnce)
		{
			throw new InvalidOperationException(SR.Format(SR.Image_NotInitialized, null));
		}
	}

	public void Reset()
	{
		_inInit = false;
		_isInitialized = false;
	}
}
