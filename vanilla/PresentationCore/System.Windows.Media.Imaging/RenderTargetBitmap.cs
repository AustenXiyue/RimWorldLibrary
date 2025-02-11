using System.Windows.Media.Composition;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Converts a <see cref="T:System.Windows.Media.Visual" /> object into a bitmap. </summary>
public sealed class RenderTargetBitmap : BitmapSource
{
	private SafeMILHandle _renderTargetBitmap;

	internal SafeMILHandle MILRenderTarget => _renderTargetBitmap;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.RenderTargetBitmap" /> class that has the specified parameters.  </summary>
	/// <param name="pixelWidth">The width of the bitmap.</param>
	/// <param name="pixelHeight">The height of the bitmap.</param>
	/// <param name="dpiX">The horizontal DPI of the bitmap.</param>
	/// <param name="dpiY">The vertical DPI of the bitmap.</param>
	/// <param name="pixelFormat">The format of the bitmap.</param>
	public RenderTargetBitmap(int pixelWidth, int pixelHeight, double dpiX, double dpiY, PixelFormat pixelFormat)
		: base(useVirtuals: true)
	{
		if (pixelFormat.Format == PixelFormatEnum.Default)
		{
			pixelFormat = PixelFormats.Pbgra32;
		}
		else if (pixelFormat.Format != PixelFormatEnum.Pbgra32)
		{
			throw new ArgumentException(SR.Format(SR.Effect_PixelFormat, pixelFormat), "pixelFormat");
		}
		if (pixelWidth <= 0)
		{
			throw new ArgumentOutOfRangeException("pixelWidth", SR.ParameterMustBeGreaterThanZero);
		}
		if (pixelHeight <= 0)
		{
			throw new ArgumentOutOfRangeException("pixelHeight", SR.ParameterMustBeGreaterThanZero);
		}
		if (dpiX < 2.220446049250313E-16)
		{
			dpiX = 96.0;
		}
		if (dpiY < 2.220446049250313E-16)
		{
			dpiY = 96.0;
		}
		_bitmapInit.BeginInit();
		_pixelWidth = pixelWidth;
		_pixelHeight = pixelHeight;
		_dpiX = dpiX;
		_dpiY = dpiY;
		_format = pixelFormat;
		_bitmapInit.EndInit();
		FinalizeCreation();
	}

	internal RenderTargetBitmap()
		: base(useVirtuals: true)
	{
	}

	protected override Freezable CreateInstanceCore()
	{
		return new RenderTargetBitmap();
	}

	private void CopyCommon(RenderTargetBitmap sourceBitmap)
	{
		_bitmapInit.BeginInit();
		_pixelWidth = sourceBitmap._pixelWidth;
		_pixelHeight = sourceBitmap._pixelHeight;
		_dpiX = sourceBitmap._dpiX;
		_dpiY = sourceBitmap._dpiY;
		_format = sourceBitmap._format;
		using (FactoryMaker factoryMaker = new FactoryMaker())
		{
			BitmapSourceSafeMILHandle bitmapSourceSafeMILHandle = BitmapSource.CreateCachedBitmap(null, base.WicSourceHandle, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad, null);
			lock (_syncObject)
			{
				base.WicSourceHandle = bitmapSourceSafeMILHandle;
			}
			HRESULT.Check(UnsafeNativeMethods.MILFactory2.CreateBitmapRenderTargetForBitmap(factoryMaker.FactoryPtr, bitmapSourceSafeMILHandle, out _renderTargetBitmap));
		}
		_bitmapInit.EndInit();
	}

	protected override void CloneCore(Freezable sourceFreezable)
	{
		RenderTargetBitmap sourceBitmap = (RenderTargetBitmap)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(sourceBitmap);
	}

	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		RenderTargetBitmap sourceBitmap = (RenderTargetBitmap)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(sourceBitmap);
	}

	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		RenderTargetBitmap sourceBitmap = (RenderTargetBitmap)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmap);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		RenderTargetBitmap sourceBitmap = (RenderTargetBitmap)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmap);
	}

	/// <summary>Renders the <see cref="T:System.Windows.Media.Visual" /> object.</summary>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> object to be used as a bitmap.</param>
	public void Render(Visual visual)
	{
		new BitmapVisualManager(this).Render(visual);
	}

	/// <summary>Clears the render target and sets all pixels to transparent black.</summary>
	public void Clear()
	{
		HRESULT.Check(MILRenderTargetBitmap.Clear(_renderTargetBitmap));
		RenderTargetContentsChanged();
	}

	internal void RenderTargetContentsChanged()
	{
		_isSourceCached = false;
		if (_convertedDUCEPtr != null)
		{
			_convertedDUCEPtr.Close();
			_convertedDUCEPtr = null;
		}
		RegisterForAsyncUpdateResource();
		FireChanged();
	}

	internal override void FinalizeCreation()
	{
		try
		{
			using (FactoryMaker factoryMaker = new FactoryMaker())
			{
				SafeMILHandle ppIRenderTargetBitmap = null;
				HRESULT.Check(UnsafeNativeMethods.MILFactory2.CreateBitmapRenderTarget(factoryMaker.FactoryPtr, (uint)_pixelWidth, (uint)_pixelHeight, _format.Format, (float)_dpiX, (float)_dpiY, MILRTInitializationFlags.MIL_RT_INITIALIZE_DEFAULT, out ppIRenderTargetBitmap));
				BitmapSourceSafeMILHandle ppIBitmap = null;
				HRESULT.Check(MILRenderTargetBitmap.GetBitmap(ppIRenderTargetBitmap, out ppIBitmap));
				lock (_syncObject)
				{
					_renderTargetBitmap = ppIRenderTargetBitmap;
					ppIBitmap.CalculateSize();
					base.WicSourceHandle = ppIBitmap;
					_isSourceCached = false;
				}
			}
			base.CreationCompleted = true;
			UpdateCachedSettings();
		}
		catch
		{
			_bitmapInit.Reset();
			throw;
		}
	}
}
