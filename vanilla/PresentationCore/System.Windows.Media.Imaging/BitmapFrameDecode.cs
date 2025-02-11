using System.Collections.Generic;
using System.Collections.ObjectModel;
using MS.Internal;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

internal sealed class BitmapFrameDecode : BitmapFrame
{
	private class WeakBitmapFrameDecodeEventSink : WeakReference
	{
		private BitmapFrameDecode _original;

		public WeakBitmapFrameDecodeEventSink(BitmapFrameDecode cloned, BitmapFrameDecode original)
			: base(cloned)
		{
			_original = original;
			if (!_original.IsFrozen)
			{
				_original.DownloadCompleted += OnSourceDownloadCompleted;
				_original.DownloadFailed += OnSourceDownloadFailed;
				_original.DownloadProgress += OnSourceDownloadProgress;
			}
		}

		public void OnSourceDownloadCompleted(object sender, EventArgs e)
		{
			if (Target is BitmapFrameDecode bitmapFrameDecode)
			{
				bitmapFrameDecode.OnOriginalDownloadCompleted(_original, e);
			}
			else
			{
				DetachSourceDownloadHandlers();
			}
		}

		public void OnSourceDownloadFailed(object sender, ExceptionEventArgs e)
		{
			if (Target is BitmapFrameDecode bitmapFrameDecode)
			{
				bitmapFrameDecode.OnOriginalDownloadFailed(e);
			}
			else
			{
				DetachSourceDownloadHandlers();
			}
		}

		public void OnSourceDownloadProgress(object sender, DownloadProgressEventArgs e)
		{
			if (Target is BitmapFrameDecode bitmapFrameDecode)
			{
				bitmapFrameDecode.OnDownloadProgress(sender, e);
			}
			else
			{
				DetachSourceDownloadHandlers();
			}
		}

		public void DetachSourceDownloadHandlers()
		{
			if (!_original.IsFrozen)
			{
				_original.DownloadCompleted -= OnSourceDownloadCompleted;
				_original.DownloadFailed -= OnSourceDownloadFailed;
				_original.DownloadProgress -= OnSourceDownloadProgress;
			}
		}
	}

	private BitmapSourceSafeMILHandle _frameSource;

	private int _frameNumber;

	private bool _isThumbnailCached;

	private bool _isMetadataCached;

	private bool _isColorContextCached;

	private BitmapCreateOptions _createOptions;

	private BitmapCacheOption _cacheOption;

	private BitmapDecoder _decoder;

	private WeakBitmapFrameDecodeEventSink _weakBitmapFrameDecodeEventSink;

	public override Uri BaseUri
	{
		get
		{
			ReadPreamble();
			return _decoder._baseUri;
		}
		set
		{
			WritePreamble();
		}
	}

	public override BitmapSource Thumbnail
	{
		get
		{
			ReadPreamble();
			EnsureThumbnail();
			return _thumbnail;
		}
	}

	public override ImageMetadata Metadata
	{
		get
		{
			ReadPreamble();
			return InternalMetadata;
		}
	}

	public override BitmapDecoder Decoder
	{
		get
		{
			ReadPreamble();
			return _decoder;
		}
	}

	public override ReadOnlyCollection<ColorContext> ColorContexts
	{
		get
		{
			ReadPreamble();
			if (!_isColorContextCached && !IsDownloading)
			{
				EnsureSource();
				lock (_syncObject)
				{
					IList<ColorContext> colorContextsHelper = ColorContext.GetColorContextsHelper(GetColorContexts);
					if (colorContextsHelper != null)
					{
						_readOnlycolorContexts = new ReadOnlyCollection<ColorContext>(colorContextsHelper);
					}
					_isColorContextCached = true;
				}
			}
			return _readOnlycolorContexts;
		}
	}

	public override bool IsDownloading
	{
		get
		{
			ReadPreamble();
			return Decoder.IsDownloading;
		}
	}

	internal override bool ShouldCloneEventDelegates => false;

	internal override BitmapMetadata InternalMetadata
	{
		get
		{
			CheckIfSiteOfOrigin();
			if (!_isMetadataCached && !IsDownloading)
			{
				EnsureSource();
				nint ppIQueryReader = IntPtr.Zero;
				lock (_syncObject)
				{
					int metadataQueryReader = UnsafeNativeMethods.WICBitmapFrameDecode.GetMetadataQueryReader(_frameSource, out ppIQueryReader);
					if (metadataQueryReader != -2003292287)
					{
						HRESULT.Check(metadataQueryReader);
					}
				}
				if (ppIQueryReader != IntPtr.Zero)
				{
					SafeMILHandle metadataHandle = new SafeMILHandle(ppIQueryReader);
					_metadata = new BitmapMetadata(metadataHandle, readOnly: true, _decoder != null && _decoder.IsMetadataFixedSize, _syncObject);
					_metadata.Freeze();
				}
				_isMetadataCached = true;
			}
			return _metadata;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	internal BitmapFrameDecode(int frameNumber, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, BitmapDecoder decoder)
		: base(useVirtuals: true)
	{
		_bitmapInit.BeginInit();
		_frameNumber = frameNumber;
		_isThumbnailCached = false;
		_isMetadataCached = false;
		_frameSource = null;
		_decoder = decoder;
		_syncObject = decoder.SyncObject;
		_createOptions = createOptions;
		_cacheOption = cacheOption;
		_bitmapInit.EndInit();
		if ((createOptions & BitmapCreateOptions.DelayCreation) != 0)
		{
			base.DelayCreation = true;
		}
		else
		{
			FinalizeCreation();
		}
	}

	internal BitmapFrameDecode(int frameNumber, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, BitmapFrameDecode frameDecode)
		: base(useVirtuals: true)
	{
		_bitmapInit.BeginInit();
		_frameNumber = frameNumber;
		base.WicSourceHandle = frameDecode.WicSourceHandle;
		base.IsSourceCached = frameDecode.IsSourceCached;
		base.CreationCompleted = frameDecode.CreationCompleted;
		_frameSource = frameDecode._frameSource;
		_decoder = frameDecode.Decoder;
		_syncObject = _decoder.SyncObject;
		_createOptions = createOptions;
		_cacheOption = cacheOption;
		_thumbnail = frameDecode._thumbnail;
		_isThumbnailCached = frameDecode._isThumbnailCached;
		_metadata = frameDecode._metadata;
		_isMetadataCached = frameDecode._isMetadataCached;
		_readOnlycolorContexts = frameDecode._readOnlycolorContexts;
		_isColorContextCached = frameDecode._isColorContextCached;
		_bitmapInit.EndInit();
		if ((createOptions & BitmapCreateOptions.DelayCreation) != 0)
		{
			base.DelayCreation = true;
		}
		else if (!base.CreationCompleted)
		{
			FinalizeCreation();
		}
		else
		{
			UpdateCachedSettings();
		}
	}

	internal BitmapFrameDecode(int frameNumber, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, LateBoundBitmapDecoder decoder)
		: base(useVirtuals: true)
	{
		_bitmapInit.BeginInit();
		_frameNumber = frameNumber;
		byte[] pixels = new byte[4];
		BitmapSource bitmapSource = BitmapSource.Create(1, 1, 96.0, 96.0, PixelFormats.Pbgra32, null, pixels, 4);
		base.WicSourceHandle = bitmapSource.WicSourceHandle;
		_decoder = decoder;
		_createOptions = createOptions;
		_cacheOption = cacheOption;
		_decoder.DownloadCompleted += OnDownloadCompleted;
		_decoder.DownloadProgress += OnDownloadProgress;
		_decoder.DownloadFailed += OnDownloadFailed;
		_bitmapInit.EndInit();
	}

	private BitmapFrameDecode()
		: base(useVirtuals: true)
	{
	}

	private int GetColorContexts(ref uint numContexts, nint[] colorContextPtrs)
	{
		Invariant.Assert(colorContextPtrs == null || numContexts <= colorContextPtrs.Length);
		return UnsafeNativeMethods.WICBitmapFrameDecode.GetColorContexts(_frameSource, numContexts, colorContextPtrs, out numContexts);
	}

	public override InPlaceBitmapMetadataWriter CreateInPlaceBitmapMetadataWriter()
	{
		ReadPreamble();
		if (_decoder != null)
		{
			_decoder.CheckOriginalWritable();
		}
		CheckIfSiteOfOrigin();
		EnsureSource();
		return InPlaceBitmapMetadataWriter.CreateFromFrameDecode(_frameSource, _syncObject);
	}

	internal override bool CanSerializeToString()
	{
		ReadPreamble();
		return _decoder.CanConvertToString();
	}

	internal override string ConvertToString(string format, IFormatProvider provider)
	{
		ReadPreamble();
		if (_decoder != null)
		{
			return _decoder.ToString();
		}
		return base.ConvertToString(format, provider);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new BitmapFrameDecode();
	}

	private void CopyCommon(BitmapFrameDecode sourceBitmapFrameDecode)
	{
		_bitmapInit.BeginInit();
		_frameNumber = sourceBitmapFrameDecode._frameNumber;
		_isThumbnailCached = sourceBitmapFrameDecode._isThumbnailCached;
		_isMetadataCached = sourceBitmapFrameDecode._isMetadataCached;
		_isColorContextCached = sourceBitmapFrameDecode._isColorContextCached;
		_frameSource = sourceBitmapFrameDecode._frameSource;
		_thumbnail = sourceBitmapFrameDecode._thumbnail;
		_metadata = sourceBitmapFrameDecode.InternalMetadata;
		_readOnlycolorContexts = sourceBitmapFrameDecode._readOnlycolorContexts;
		_decoder = sourceBitmapFrameDecode._decoder;
		if (_decoder != null && _decoder.IsDownloading)
		{
			_weakBitmapFrameDecodeEventSink = new WeakBitmapFrameDecodeEventSink(this, sourceBitmapFrameDecode);
		}
		_syncObject = _decoder.SyncObject;
		_createOptions = sourceBitmapFrameDecode._createOptions;
		_cacheOption = sourceBitmapFrameDecode._cacheOption;
		_bitmapInit.EndInit();
	}

	protected override void CloneCore(Freezable sourceFreezable)
	{
		BitmapFrameDecode sourceBitmapFrameDecode = (BitmapFrameDecode)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(sourceBitmapFrameDecode);
	}

	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		BitmapFrameDecode sourceBitmapFrameDecode = (BitmapFrameDecode)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(sourceBitmapFrameDecode);
	}

	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		BitmapFrameDecode sourceBitmapFrameDecode = (BitmapFrameDecode)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmapFrameDecode);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		BitmapFrameDecode sourceBitmapFrameDecode = (BitmapFrameDecode)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmapFrameDecode);
	}

	internal void UpdateDecoder(BitmapDecoder decoder)
	{
		_decoder = decoder;
		_syncObject = decoder.SyncObject;
		base.WicSourceHandle = null;
		_needsUpdate = true;
		FinalizeCreation();
		RegisterForAsyncUpdateResource();
	}

	internal override void FinalizeCreation()
	{
		EnsureSource();
		base.WicSourceHandle = _frameSource;
		UpdateCachedSettings();
		lock (_syncObject)
		{
			base.WicSourceHandle = BitmapSource.CreateCachedBitmap(this, _frameSource, _createOptions, _cacheOption, Palette);
		}
		base.IsSourceCached = _cacheOption != BitmapCacheOption.None;
		base.CreationCompleted = true;
		UpdateCachedSettings();
		EnsureThumbnail();
	}

	private void OnDownloadCompleted(object sender, EventArgs e)
	{
		LateBoundBitmapDecoder obj = (LateBoundBitmapDecoder)sender;
		obj.DownloadCompleted -= OnDownloadCompleted;
		obj.DownloadProgress -= OnDownloadProgress;
		obj.DownloadFailed -= OnDownloadFailed;
		FireChanged();
		_downloadEvent.InvokeEvents(this, null);
	}

	private void OnDownloadProgress(object sender, DownloadProgressEventArgs e)
	{
		_progressEvent.InvokeEvents(this, e);
	}

	private void OnDownloadFailed(object sender, ExceptionEventArgs e)
	{
		LateBoundBitmapDecoder obj = (LateBoundBitmapDecoder)sender;
		obj.DownloadCompleted -= OnDownloadCompleted;
		obj.DownloadProgress -= OnDownloadProgress;
		obj.DownloadFailed -= OnDownloadFailed;
		_failedEvent.InvokeEvents(this, e);
	}

	private void OnOriginalDownloadCompleted(BitmapFrameDecode original, EventArgs e)
	{
		CleanUpWeakEventSink();
		UpdateDecoder(original.Decoder);
		FireChanged();
		_downloadEvent.InvokeEvents(this, e);
	}

	private void OnOriginalDownloadFailed(ExceptionEventArgs e)
	{
		CleanUpWeakEventSink();
		_failedEvent.InvokeEvents(this, e);
	}

	private void CleanUpWeakEventSink()
	{
		_weakBitmapFrameDecodeEventSink.DetachSourceDownloadHandlers();
		_weakBitmapFrameDecodeEventSink = null;
	}

	private void EnsureThumbnail()
	{
		if (_isThumbnailCached || IsDownloading)
		{
			return;
		}
		EnsureSource();
		nint ppIThumbnail = IntPtr.Zero;
		lock (_syncObject)
		{
			int thumbnail = UnsafeNativeMethods.WICBitmapFrameDecode.GetThumbnail(_frameSource, out ppIThumbnail);
			if (thumbnail != -2003292348)
			{
				HRESULT.Check(thumbnail);
			}
		}
		_isThumbnailCached = true;
		if (ppIThumbnail != IntPtr.Zero)
		{
			BitmapSourceSafeMILHandle bitmapSourceSafeMILHandle = new BitmapSourceSafeMILHandle(ppIThumbnail);
			SafeMILHandle safeMILHandle = BitmapPalette.CreateInternalPalette();
			BitmapPalette palette = null;
			if (UnsafeNativeMethods.WICBitmapSource.CopyPalette(bitmapSourceSafeMILHandle, safeMILHandle) == 0)
			{
				palette = new BitmapPalette(safeMILHandle);
			}
			_thumbnail = new UnmanagedBitmapWrapper(BitmapSource.CreateCachedBitmap(null, bitmapSourceSafeMILHandle, BitmapCreateOptions.PreservePixelFormat, _cacheOption, palette));
			_thumbnail.Freeze();
		}
	}

	private void EnsureSource()
	{
		if (_frameSource == null)
		{
			if (_decoder == null)
			{
				HRESULT.Check(-2003292404);
			}
			if (_decoder.InternalDecoder == null)
			{
				_decoder = ((LateBoundBitmapDecoder)_decoder).Decoder;
				_syncObject = _decoder.SyncObject;
			}
			nint ppIFrameDecode = IntPtr.Zero;
			lock (_syncObject)
			{
				HRESULT.Check(UnsafeNativeMethods.WICBitmapDecoder.GetFrame(_decoder.InternalDecoder, (uint)_frameNumber, out ppIFrameDecode));
				_frameSource = new BitmapSourceSafeMILHandle(ppIFrameDecode);
				_frameSource.CalculateSize();
			}
		}
	}
}
