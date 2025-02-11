using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Provides caching functionality for a <see cref="T:System.Windows.Media.Imaging.BitmapSource" />.</summary>
public sealed class CachedBitmap : BitmapSource
{
	private BitmapSource _source;

	private BitmapCreateOptions _createOptions;

	private BitmapCacheOption _cacheOption;

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.Imaging.CachedBitmap" /> that has the specified source, bitmap create options, and bitmap cache option.</summary>
	/// <param name="source">The source bitmap that is being cached.</param>
	/// <param name="createOptions">Initialization options for the bitmap image.</param>
	/// <param name="cacheOption">Specifies how the bitmap is cached to memory.</param>
	/// <exception cref="T:System.ArgumentNullException">Occurs when <paramref name="source" /> is null.</exception>
	public CachedBitmap(BitmapSource source, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(useVirtuals: true)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		BeginInit();
		_source = source;
		RegisterDownloadEventSource(_source);
		_createOptions = createOptions;
		_cacheOption = cacheOption;
		_syncObject = source.SyncObject;
		EndInit();
	}

	internal CachedBitmap(int pixelWidth, int pixelHeight, double dpiX, double dpiY, PixelFormat pixelFormat, BitmapPalette palette, nint buffer, int bufferSize, int stride)
		: base(useVirtuals: true)
	{
		InitFromMemoryPtr(pixelWidth, pixelHeight, dpiX, dpiY, pixelFormat, palette, buffer, bufferSize, stride);
	}

	internal CachedBitmap(BitmapSourceSafeMILHandle bitmap)
		: base(useVirtuals: true)
	{
		if (bitmap == null)
		{
			throw new ArgumentNullException("bitmap");
		}
		_bitmapInit.BeginInit();
		_source = null;
		_createOptions = BitmapCreateOptions.None;
		_cacheOption = BitmapCacheOption.OnLoad;
		bitmap.CalculateSize();
		base.WicSourceHandle = bitmap;
		_syncObject = base.WicSourceHandle;
		base.IsSourceCached = true;
		base.CreationCompleted = true;
		_bitmapInit.EndInit();
	}

	private CachedBitmap()
		: base(useVirtuals: true)
	{
	}

	internal unsafe CachedBitmap(int pixelWidth, int pixelHeight, double dpiX, double dpiY, PixelFormat pixelFormat, BitmapPalette palette, Array pixels, int stride)
		: base(useVirtuals: true)
	{
		if (pixels == null)
		{
			throw new ArgumentNullException("pixels");
		}
		if (pixels.Rank != 1)
		{
			throw new ArgumentException(SR.Collection_BadRank, "pixels");
		}
		int num = -1;
		if (pixels is byte[])
		{
			num = 1;
		}
		else if (pixels is short[] || pixels is ushort[])
		{
			num = 2;
		}
		else if (pixels is int[] || pixels is uint[] || pixels is float[])
		{
			num = 4;
		}
		else if (pixels is double[])
		{
			num = 8;
		}
		if (num == -1)
		{
			throw new ArgumentException(SR.Image_InvalidArrayForPixel);
		}
		int bufferSize = num * pixels.Length;
		if (pixels is byte[])
		{
			fixed (byte* ptr = (byte[])pixels)
			{
				void* buffer = ptr;
				InitFromMemoryPtr(pixelWidth, pixelHeight, dpiX, dpiY, pixelFormat, palette, (nint)buffer, bufferSize, stride);
			}
		}
		else if (pixels is short[])
		{
			fixed (short* ptr2 = (short[])pixels)
			{
				void* buffer2 = ptr2;
				InitFromMemoryPtr(pixelWidth, pixelHeight, dpiX, dpiY, pixelFormat, palette, (nint)buffer2, bufferSize, stride);
			}
		}
		else if (pixels is ushort[])
		{
			fixed (ushort* ptr3 = (ushort[])pixels)
			{
				void* buffer3 = ptr3;
				InitFromMemoryPtr(pixelWidth, pixelHeight, dpiX, dpiY, pixelFormat, palette, (nint)buffer3, bufferSize, stride);
			}
		}
		else if (pixels is int[])
		{
			fixed (int* ptr4 = (int[])pixels)
			{
				void* buffer4 = ptr4;
				InitFromMemoryPtr(pixelWidth, pixelHeight, dpiX, dpiY, pixelFormat, palette, (nint)buffer4, bufferSize, stride);
			}
		}
		else if (pixels is uint[])
		{
			fixed (uint* ptr5 = (uint[])pixels)
			{
				void* buffer5 = ptr5;
				InitFromMemoryPtr(pixelWidth, pixelHeight, dpiX, dpiY, pixelFormat, palette, (nint)buffer5, bufferSize, stride);
			}
		}
		else if (pixels is float[])
		{
			fixed (float* ptr6 = (float[])pixels)
			{
				void* buffer6 = ptr6;
				InitFromMemoryPtr(pixelWidth, pixelHeight, dpiX, dpiY, pixelFormat, palette, (nint)buffer6, bufferSize, stride);
			}
		}
		else if (pixels is double[])
		{
			fixed (double* ptr7 = (double[])pixels)
			{
				void* buffer7 = ptr7;
				InitFromMemoryPtr(pixelWidth, pixelHeight, dpiX, dpiY, pixelFormat, palette, (nint)buffer7, bufferSize, stride);
			}
		}
	}

	private void CopyCommon(CachedBitmap sourceBitmap)
	{
		base.Animatable_IsResourceInvalidationNecessary = false;
		if (sourceBitmap._source != null)
		{
			BeginInit();
			_syncObject = sourceBitmap._syncObject;
			_source = sourceBitmap._source;
			RegisterDownloadEventSource(_source);
			_createOptions = sourceBitmap._createOptions;
			_cacheOption = sourceBitmap._cacheOption;
			if (_cacheOption == BitmapCacheOption.Default)
			{
				_cacheOption = BitmapCacheOption.OnLoad;
			}
			EndInit();
		}
		else
		{
			InitFromWICSource(sourceBitmap.WicSourceHandle);
		}
		base.Animatable_IsResourceInvalidationNecessary = true;
	}

	private void BeginInit()
	{
		_bitmapInit.BeginInit();
	}

	private void EndInit()
	{
		_bitmapInit.EndInit();
		if (!base.DelayCreation)
		{
			FinalizeCreation();
		}
	}

	internal override void FinalizeCreation()
	{
		lock (_syncObject)
		{
			base.WicSourceHandle = BitmapSource.CreateCachedBitmap(_source as BitmapFrame, _source.WicSourceHandle, _createOptions, _cacheOption, _source.Palette);
		}
		base.IsSourceCached = _cacheOption != BitmapCacheOption.None;
		base.CreationCompleted = true;
		UpdateCachedSettings();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Imaging.CachedBitmap" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new CachedBitmap Clone()
	{
		return (CachedBitmap)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Imaging.CachedBitmap" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new CachedBitmap CloneCurrentValue()
	{
		return (CachedBitmap)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new CachedBitmap();
	}

	protected override void CloneCore(Freezable sourceFreezable)
	{
		CachedBitmap sourceBitmap = (CachedBitmap)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(sourceBitmap);
	}

	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		CachedBitmap sourceBitmap = (CachedBitmap)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(sourceBitmap);
	}

	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		CachedBitmap sourceBitmap = (CachedBitmap)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmap);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		CachedBitmap sourceBitmap = (CachedBitmap)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmap);
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

	private void InitFromMemoryPtr(int pixelWidth, int pixelHeight, double dpiX, double dpiY, PixelFormat pixelFormat, BitmapPalette palette, nint buffer, int bufferSize, int stride)
	{
		if (pixelFormat.Palettized && palette == null)
		{
			throw new InvalidOperationException(SR.Image_IndexedPixelFormatRequiresPalette);
		}
		if (pixelFormat.Format == PixelFormatEnum.Default && pixelFormat.Guid == WICPixelFormatGUIDs.WICPixelFormatDontCare)
		{
			throw new ArgumentException(SR.Format(SR.Effect_PixelFormat, pixelFormat), "pixelFormat");
		}
		_bitmapInit.BeginInit();
		try
		{
			Guid pixelFormatGuid = pixelFormat.Guid;
			BitmapSourceSafeMILHandle ppIBitmap;
			using (FactoryMaker factoryMaker = new FactoryMaker())
			{
				HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateBitmapFromMemory(factoryMaker.ImagingFactoryPtr, (uint)pixelWidth, (uint)pixelHeight, ref pixelFormatGuid, (uint)stride, (uint)bufferSize, buffer, out ppIBitmap));
				ppIBitmap.CalculateSize();
			}
			HRESULT.Check(UnsafeNativeMethods.WICBitmap.SetResolution(ppIBitmap, dpiX, dpiY));
			if (pixelFormat.Palettized)
			{
				HRESULT.Check(UnsafeNativeMethods.WICBitmap.SetPalette(ppIBitmap, palette.InternalPalette));
			}
			base.WicSourceHandle = ppIBitmap;
			_isSourceCached = true;
		}
		catch
		{
			_bitmapInit.Reset();
			throw;
		}
		_createOptions = BitmapCreateOptions.PreservePixelFormat;
		_cacheOption = BitmapCacheOption.OnLoad;
		_syncObject = base.WicSourceHandle;
		_bitmapInit.EndInit();
		UpdateCachedSettings();
	}
}
