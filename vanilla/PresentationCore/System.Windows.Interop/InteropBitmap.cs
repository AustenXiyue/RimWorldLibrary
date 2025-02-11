using System.Windows.Media;
using System.Windows.Media.Composition;
using System.Windows.Media.Imaging;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Interop;

/// <summary>
///   <see cref="T:System.Windows.Interop.InteropBitmap" /> enables developers to improve rendering performance of non-WPF UIs that are hosted by WPF in interoperability scenarios.</summary>
public sealed class InteropBitmap : BitmapSource
{
	private BitmapSourceSafeMILHandle _unmanagedSource;

	private Int32Rect _sourceRect = Int32Rect.Empty;

	private BitmapSizeOptions _sizeOptions;

	private InteropBitmap()
		: base(useVirtuals: true)
	{
	}

	internal InteropBitmap(nint hbitmap, nint hpalette, Int32Rect sourceRect, BitmapSizeOptions sizeOptions, WICBitmapAlphaChannelOption alphaOptions)
		: base(useVirtuals: true)
	{
		_bitmapInit.BeginInit();
		using (FactoryMaker factoryMaker = new FactoryMaker())
		{
			HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateBitmapFromHBITMAP(factoryMaker.ImagingFactoryPtr, hbitmap, hpalette, alphaOptions, out _unmanagedSource));
		}
		_unmanagedSource.CalculateSize();
		_sizeOptions = sizeOptions;
		_sourceRect = sourceRect;
		_syncObject = _unmanagedSource;
		_bitmapInit.EndInit();
		FinalizeCreation();
	}

	internal InteropBitmap(nint hicon, Int32Rect sourceRect, BitmapSizeOptions sizeOptions)
		: base(useVirtuals: true)
	{
		_bitmapInit.BeginInit();
		using (FactoryMaker factoryMaker = new FactoryMaker())
		{
			HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateBitmapFromHICON(factoryMaker.ImagingFactoryPtr, hicon, out _unmanagedSource));
		}
		_unmanagedSource.CalculateSize();
		_sourceRect = sourceRect;
		_sizeOptions = sizeOptions;
		_syncObject = _unmanagedSource;
		_bitmapInit.EndInit();
		FinalizeCreation();
	}

	internal InteropBitmap(nint section, int pixelWidth, int pixelHeight, PixelFormat format, int stride, int offset)
		: base(useVirtuals: true)
	{
		_bitmapInit.BeginInit();
		if (pixelWidth <= 0)
		{
			throw new ArgumentOutOfRangeException("pixelWidth", SR.ParameterMustBeGreaterThanZero);
		}
		if (pixelHeight <= 0)
		{
			throw new ArgumentOutOfRangeException("pixelHeight", SR.ParameterMustBeGreaterThanZero);
		}
		Guid pixelFormatGuid = format.Guid;
		HRESULT.Check(UnsafeNativeMethods.WindowsCodecApi.CreateBitmapFromSection((uint)pixelWidth, (uint)pixelHeight, ref pixelFormatGuid, section, (uint)stride, (uint)offset, out _unmanagedSource));
		_unmanagedSource.CalculateSize();
		_sourceRect = Int32Rect.Empty;
		_sizeOptions = null;
		_syncObject = _unmanagedSource;
		_bitmapInit.EndInit();
		FinalizeCreation();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new InteropBitmap();
	}

	private void CopyCommon(InteropBitmap sourceBitmapSource)
	{
		base.Animatable_IsResourceInvalidationNecessary = false;
		_unmanagedSource = sourceBitmapSource._unmanagedSource;
		_sourceRect = sourceBitmapSource._sourceRect;
		_sizeOptions = sourceBitmapSource._sizeOptions;
		InitFromWICSource(sourceBitmapSource.WicSourceHandle);
		base.Animatable_IsResourceInvalidationNecessary = true;
	}

	protected override void CloneCore(Freezable sourceFreezable)
	{
		InteropBitmap sourceBitmapSource = (InteropBitmap)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(sourceBitmapSource);
	}

	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		InteropBitmap sourceBitmapSource = (InteropBitmap)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(sourceBitmapSource);
	}

	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		InteropBitmap sourceBitmapSource = (InteropBitmap)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmapSource);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		InteropBitmap sourceBitmapSource = (InteropBitmap)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmapSource);
	}

	private void InitFromWICSource(SafeMILHandle wicSource)
	{
		_bitmapInit.BeginInit();
		BitmapSourceSafeMILHandle ppIBitmap = null;
		lock (_syncObject)
		{
			using (FactoryMaker factoryMaker = new FactoryMaker())
			{
				HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateBitmapFromSource(factoryMaker.ImagingFactoryPtr, wicSource, WICBitmapCreateCacheOptions.WICBitmapCacheOnLoad, out ppIBitmap));
			}
			ppIBitmap.CalculateSize();
		}
		base.WicSourceHandle = ppIBitmap;
		_isSourceCached = true;
		_bitmapInit.EndInit();
		UpdateCachedSettings();
	}

	/// <summary>Forces the hosted non-WPF UI to be rendered.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Interop.InteropBitmap" /> instance is frozen and cannot have its members written to.</exception>
	public void Invalidate()
	{
		Invalidate(null);
	}

	/// <summary>Forces the specified area of the hosted non-WPF UI to be rendered.</summary>
	/// <param name="dirtyRect">The area of the hosted non-WPF UI to be rendered. If this parameter is null, the entire non-WPF UI is rendered.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Interop.InteropBitmap" /> instance is frozen and cannot have its members written to.</exception>
	public unsafe void Invalidate(Int32Rect? dirtyRect)
	{
		if (dirtyRect.HasValue)
		{
			dirtyRect.Value.ValidateForDirtyRect("dirtyRect", _pixelWidth, _pixelHeight);
			if (!dirtyRect.Value.HasArea)
			{
				return;
			}
		}
		WritePreamble();
		if (_unmanagedSource != null)
		{
			if (base.UsableWithoutCache)
			{
				int i = 0;
				DUCE.MILCMD_BITMAP_INVALIDATE mILCMD_BITMAP_INVALIDATE = default(DUCE.MILCMD_BITMAP_INVALIDATE);
				for (int channelCount = _duceResource.GetChannelCount(); i < channelCount; i++)
				{
					DUCE.Channel channel = _duceResource.GetChannel(i);
					mILCMD_BITMAP_INVALIDATE.Type = MILCMD.MilCmdBitmapInvalidate;
					mILCMD_BITMAP_INVALIDATE.Handle = _duceResource.GetHandle(channel);
					bool hasValue = dirtyRect.HasValue;
					if (hasValue)
					{
						mILCMD_BITMAP_INVALIDATE.DirtyRect.left = dirtyRect.Value.X;
						mILCMD_BITMAP_INVALIDATE.DirtyRect.top = dirtyRect.Value.Y;
						mILCMD_BITMAP_INVALIDATE.DirtyRect.right = dirtyRect.Value.X + dirtyRect.Value.Width;
						mILCMD_BITMAP_INVALIDATE.DirtyRect.bottom = dirtyRect.Value.Y + dirtyRect.Value.Height;
					}
					mILCMD_BITMAP_INVALIDATE.UseDirtyRect = (hasValue ? 1u : 0u);
					channel.SendCommand((byte*)(&mILCMD_BITMAP_INVALIDATE), sizeof(DUCE.MILCMD_BITMAP_INVALIDATE));
				}
			}
			else
			{
				_needsUpdate = true;
				RegisterForAsyncUpdateResource();
			}
		}
		WritePostscript();
	}

	internal override void FinalizeCreation()
	{
		BitmapSourceSafeMILHandle ppBitmapClipper = null;
		BitmapSourceSafeMILHandle ppBitmapScaler = null;
		BitmapSourceSafeMILHandle bitmapSourceSafeMILHandle = _unmanagedSource;
		HRESULT.Check(UnsafeNativeMethods.WICBitmap.SetResolution(_unmanagedSource, 96.0, 96.0));
		using (FactoryMaker factoryMaker = new FactoryMaker())
		{
			nint imagingFactoryPtr = factoryMaker.ImagingFactoryPtr;
			if (!_sourceRect.IsEmpty)
			{
				HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateBitmapClipper(imagingFactoryPtr, out ppBitmapClipper));
				lock (_syncObject)
				{
					HRESULT.Check(UnsafeNativeMethods.WICBitmapClipper.Initialize(ppBitmapClipper, bitmapSourceSafeMILHandle, ref _sourceRect));
				}
				bitmapSourceSafeMILHandle = ppBitmapClipper;
			}
			if (_sizeOptions != null)
			{
				if (_sizeOptions.DoesScale)
				{
					_sizeOptions.GetScaledWidthAndHeight((uint)_sizeOptions.PixelWidth, (uint)_sizeOptions.PixelHeight, out var newWidth, out var newHeight);
					HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateBitmapScaler(imagingFactoryPtr, out ppBitmapScaler));
					lock (_syncObject)
					{
						HRESULT.Check(UnsafeNativeMethods.WICBitmapScaler.Initialize(ppBitmapScaler, bitmapSourceSafeMILHandle, newWidth, newHeight, WICInterpolationMode.Fant));
					}
				}
				else if (_sizeOptions.Rotation != 0)
				{
					HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateBitmapFlipRotator(imagingFactoryPtr, out ppBitmapScaler));
					lock (_syncObject)
					{
						HRESULT.Check(UnsafeNativeMethods.WICBitmapFlipRotator.Initialize(ppBitmapScaler, bitmapSourceSafeMILHandle, _sizeOptions.WICTransformOptions));
					}
				}
				if (ppBitmapScaler != null)
				{
					bitmapSourceSafeMILHandle = ppBitmapScaler;
				}
			}
			base.WicSourceHandle = bitmapSourceSafeMILHandle;
			_isSourceCached = true;
		}
		base.CreationCompleted = true;
		UpdateCachedSettings();
	}
}
