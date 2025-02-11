using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Packaging;
using System.Net.Cache;
using System.Text;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Win32.SafeHandles;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Represents a container for bitmap frames. Each bitmap frame is a <see cref="T:System.Windows.Media.Imaging.BitmapSource" />. This abstract class provides a base set of functionality for all derived decoder objects.</summary>
public abstract class BitmapDecoder : DispatcherObject
{
	private bool _isBuiltInDecoder;

	private SafeMILHandle _decoderHandle;

	private bool _shouldCacheDecoder = true;

	private bool _isOriginalWritable;

	private bool _isPaletteCached;

	private BitmapPalette _palette;

	private bool _isColorContextCached;

	internal ReadOnlyCollection<ColorContext> _readOnlycolorContexts;

	private bool _isThumbnailCached;

	private BitmapMetadata _metadata;

	private bool _isMetadataCached;

	private BitmapSource _thumbnail;

	private BitmapCodecInfo _codecInfo;

	private bool _isPreviewCached;

	private BitmapSource _preview;

	internal List<BitmapFrame> _frames;

	internal ReadOnlyCollection<BitmapFrame> _readOnlyFrames;

	internal Stream _stream;

	internal Uri _uri;

	internal Uri _baseUri;

	internal Stream _uriStream;

	internal BitmapCreateOptions _createOptions;

	internal BitmapCacheOption _cacheOption;

	internal UniqueEventHelper _downloadEvent = new UniqueEventHelper();

	internal UniqueEventHelper<DownloadProgressEventArgs> _progressEvent = new UniqueEventHelper<DownloadProgressEventArgs>();

	internal UniqueEventHelper<ExceptionEventArgs> _failedEvent = new UniqueEventHelper<ExceptionEventArgs>();

	private readonly object _syncObject = new object();

	private UnmanagedMemoryStream _unmanagedMemoryStream;

	private SafeFileHandle _safeFilehandle;

	private BitmapDecoder _cachedDecoder;

	/// <summary>Gets the <see cref="T:System.Windows.Media.Imaging.BitmapPalette" /> associated with this decoder. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Imaging.BitmapPalette" /> associated with this decoder. If the bitmap has no palette, null is returned. This property has no default value.</returns>
	public virtual BitmapPalette Palette
	{
		get
		{
			VerifyAccess();
			EnsureBuiltInDecoder();
			if (!_isPaletteCached)
			{
				SafeMILHandle safeMILHandle = BitmapPalette.CreateInternalPalette();
				lock (_syncObject)
				{
					int num = UnsafeNativeMethods.WICBitmapDecoder.CopyPalette(_decoderHandle, safeMILHandle);
					if (num != -2003292347)
					{
						HRESULT.Check(num);
						_palette = new BitmapPalette(safeMILHandle);
					}
				}
				_isPaletteCached = true;
			}
			return _palette;
		}
	}

	/// <summary>Gets a value that represents the color profile associated with a bitmap, if one is defined.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.ColorContext" /> that represents the embedded color profile of the bitmap. If no color profile has been defined, this property returns null. This property has no default value.</returns>
	public virtual ReadOnlyCollection<ColorContext> ColorContexts
	{
		get
		{
			VerifyAccess();
			return InternalColorContexts;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that represents the thumbnail of the bitmap, if one is defined. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that represents a thumbnail of the bitmap. If no thumbnail is defined, this property returns null. This property has no default value.</returns>
	public virtual BitmapSource Thumbnail
	{
		get
		{
			VerifyAccess();
			EnsureBuiltInDecoder();
			if (!_isThumbnailCached)
			{
				nint ppIThumbnail = IntPtr.Zero;
				lock (_syncObject)
				{
					int thumbnail = UnsafeNativeMethods.WICBitmapDecoder.GetThumbnail(_decoderHandle, out ppIThumbnail);
					if (thumbnail != -2003292348)
					{
						HRESULT.Check(thumbnail);
					}
				}
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
				_isThumbnailCached = true;
			}
			return _thumbnail;
		}
	}

	/// <summary>Gets an instance of <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" /> that represents the global metadata associated with this bitmap, if metadata is defined.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" /> that represents global metadata associated with a bitmap. If no metadata is defined, this property returns null.</returns>
	public virtual BitmapMetadata Metadata
	{
		get
		{
			VerifyAccess();
			EnsureBuiltInDecoder();
			if (!_isMetadataCached)
			{
				nint ppIQueryReader = IntPtr.Zero;
				lock (_syncObject)
				{
					int metadataQueryReader = UnsafeNativeMethods.WICBitmapDecoder.GetMetadataQueryReader(_decoderHandle, out ppIQueryReader);
					if (metadataQueryReader != -2003292287)
					{
						HRESULT.Check(metadataQueryReader);
					}
				}
				if (ppIQueryReader != IntPtr.Zero)
				{
					SafeMILHandle metadataHandle = new SafeMILHandle(ppIQueryReader);
					_metadata = new BitmapMetadata(metadataHandle, readOnly: true, IsMetadataFixedSize, _syncObject);
					_metadata.Freeze();
				}
				_isMetadataCached = true;
			}
			return _metadata;
		}
	}

	/// <summary>Gets information that describes this codec. </summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapCodecInfo" />. This property has no default value.</returns>
	public virtual BitmapCodecInfo CodecInfo
	{
		get
		{
			VerifyAccess();
			EnsureBuiltInDecoder();
			if (_codecInfo == null)
			{
				SafeMILHandle ppIDecoderInfo = new SafeMILHandle();
				HRESULT.Check(UnsafeNativeMethods.WICBitmapDecoder.GetDecoderInfo(_decoderHandle, out ppIDecoderInfo));
				_codecInfo = new BitmapCodecInfoInternal(ppIDecoderInfo);
			}
			return _codecInfo;
		}
	}

	/// <summary>Gets the content of an individual frame within a bitmap.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />. This property has no default value.</returns>
	public virtual ReadOnlyCollection<BitmapFrame> Frames
	{
		get
		{
			VerifyAccess();
			EnsureBuiltInDecoder();
			if (_frames == null)
			{
				SetupFrames(null, null);
			}
			if (_readOnlyFrames == null)
			{
				_readOnlyFrames = new ReadOnlyCollection<BitmapFrame>(_frames);
			}
			return _readOnlyFrames;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that represents the global preview of this bitmap, if one is defined.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that represents the global preview of the bitmap. If no preview is defined, this property returns null. This property has no default value.</returns>
	public virtual BitmapSource Preview
	{
		get
		{
			VerifyAccess();
			EnsureBuiltInDecoder();
			if (!_isPreviewCached)
			{
				nint ppIBitmapSource = IntPtr.Zero;
				lock (_syncObject)
				{
					int preview = UnsafeNativeMethods.WICBitmapDecoder.GetPreview(_decoderHandle, out ppIBitmapSource);
					if (preview != -2003292287)
					{
						HRESULT.Check(preview);
					}
				}
				if (ppIBitmapSource != IntPtr.Zero)
				{
					BitmapSourceSafeMILHandle bitmapSourceSafeMILHandle = new BitmapSourceSafeMILHandle(ppIBitmapSource);
					SafeMILHandle safeMILHandle = BitmapPalette.CreateInternalPalette();
					BitmapPalette palette = null;
					if (UnsafeNativeMethods.WICBitmapSource.CopyPalette(bitmapSourceSafeMILHandle, safeMILHandle) == 0)
					{
						palette = new BitmapPalette(safeMILHandle);
					}
					_preview = new UnmanagedBitmapWrapper(BitmapSource.CreateCachedBitmap(null, bitmapSourceSafeMILHandle, BitmapCreateOptions.PreservePixelFormat, _cacheOption, palette));
					_preview.Freeze();
				}
				_isPreviewCached = true;
			}
			return _preview;
		}
	}

	/// <summary>Gets a value that indicates if the decoder is currently downloading content.</summary>
	/// <returns>true if the decoder is downloading content; otherwise, false.</returns>
	public virtual bool IsDownloading
	{
		get
		{
			VerifyAccess();
			EnsureBuiltInDecoder();
			return false;
		}
	}

	internal SafeMILHandle InternalDecoder => _decoderHandle;

	internal virtual bool IsMetadataFixedSize => false;

	internal object SyncObject => _syncObject;

	internal ReadOnlyCollection<ColorContext> InternalColorContexts
	{
		get
		{
			EnsureBuiltInDecoder();
			if (!_isColorContextCached)
			{
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

	/// <summary>Occurs when a <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" /> has finished downloading bitmap content.</summary>
	public virtual event EventHandler DownloadCompleted
	{
		add
		{
			VerifyAccess();
			EnsureBuiltInDecoder();
			_downloadEvent.AddEvent(value);
		}
		remove
		{
			VerifyAccess();
			EnsureBuiltInDecoder();
			_downloadEvent.RemoveEvent(value);
		}
	}

	/// <summary>Occurs when a <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" /> has made progress downloading bitmap content.</summary>
	public virtual event EventHandler<DownloadProgressEventArgs> DownloadProgress
	{
		add
		{
			VerifyAccess();
			EnsureBuiltInDecoder();
			_progressEvent.AddEvent(value);
		}
		remove
		{
			VerifyAccess();
			EnsureBuiltInDecoder();
			_progressEvent.RemoveEvent(value);
		}
	}

	/// <summary>Occurs when bitmap content failed to download.</summary>
	public virtual event EventHandler<ExceptionEventArgs> DownloadFailed
	{
		add
		{
			VerifyAccess();
			EnsureBuiltInDecoder();
			_failedEvent.AddEvent(value);
		}
		remove
		{
			VerifyAccess();
			EnsureBuiltInDecoder();
			_failedEvent.RemoveEvent(value);
		}
	}

	static BitmapDecoder()
	{
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" />.</summary>
	protected BitmapDecoder()
	{
	}

	internal BitmapDecoder(bool isBuiltIn)
	{
		_isBuiltInDecoder = isBuiltIn;
	}

	internal BitmapDecoder(Uri bitmapUri, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, Guid expectedClsId)
	{
		Guid clsId = Guid.Empty;
		bool isOriginalWritable = false;
		if (bitmapUri == null)
		{
			throw new ArgumentNullException("bitmapUri");
		}
		if ((createOptions & BitmapCreateOptions.IgnoreImageCache) != 0)
		{
			ImagingCache.RemoveFromDecoderCache(bitmapUri);
		}
		BitmapDecoder bitmapDecoder = CheckCache(bitmapUri, out clsId);
		if (bitmapDecoder != null)
		{
			_decoderHandle = bitmapDecoder.InternalDecoder;
		}
		else
		{
			_decoderHandle = SetupDecoderFromUriOrStream(bitmapUri, null, cacheOption, out clsId, out isOriginalWritable, out _uriStream, out _unmanagedMemoryStream, out _safeFilehandle);
			if (_uriStream == null)
			{
				GC.SuppressFinalize(this);
			}
		}
		if (clsId != expectedClsId)
		{
			throw new FileFormatException(bitmapUri, SR.Image_CantDealWithUri);
		}
		_uri = bitmapUri;
		_createOptions = createOptions;
		_cacheOption = cacheOption;
		_syncObject = _decoderHandle;
		_isOriginalWritable = isOriginalWritable;
		Initialize(bitmapDecoder);
	}

	internal BitmapDecoder(Stream bitmapStream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, Guid expectedClsId)
	{
		Guid clsId = Guid.Empty;
		bool isOriginalWritable = false;
		if (bitmapStream == null)
		{
			throw new ArgumentNullException("bitmapStream");
		}
		_decoderHandle = SetupDecoderFromUriOrStream(null, bitmapStream, cacheOption, out clsId, out isOriginalWritable, out _uriStream, out _unmanagedMemoryStream, out _safeFilehandle);
		if (_uriStream == null)
		{
			GC.SuppressFinalize(this);
		}
		if (clsId != Guid.Empty && clsId != expectedClsId)
		{
			throw new FileFormatException(null, SR.Image_CantDealWithStream);
		}
		_stream = bitmapStream;
		_createOptions = createOptions;
		_cacheOption = cacheOption;
		_syncObject = _decoderHandle;
		_isOriginalWritable = isOriginalWritable;
		Initialize(null);
	}

	internal BitmapDecoder(SafeMILHandle decoderHandle, BitmapDecoder decoder, Uri baseUri, Uri uri, Stream stream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, bool insertInDecoderCache, bool isOriginalWritable, Stream uriStream, UnmanagedMemoryStream unmanagedMemoryStream, SafeFileHandle safeFilehandle)
	{
		_decoderHandle = decoderHandle;
		_baseUri = baseUri;
		_uri = uri;
		_stream = stream;
		_createOptions = createOptions;
		_cacheOption = cacheOption;
		_syncObject = decoderHandle;
		_shouldCacheDecoder = insertInDecoderCache;
		_isOriginalWritable = isOriginalWritable;
		_uriStream = uriStream;
		_unmanagedMemoryStream = unmanagedMemoryStream;
		_safeFilehandle = safeFilehandle;
		if (_uriStream == null)
		{
			GC.SuppressFinalize(this);
		}
		Initialize(decoder);
	}

	/// <summary>Frees resources and performs other cleanup operations before the <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" /> is reclaimed by garbage collection. </summary>
	~BitmapDecoder()
	{
		if (_uriStream != null)
		{
			_uriStream.Close();
		}
	}

	internal static BitmapDecoder CreateFromUriOrStream(Uri baseUri, Uri uri, Stream stream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, RequestCachePolicy uriCachePolicy, bool insertInDecoderCache)
	{
		Guid clsId = Guid.Empty;
		bool isOriginalWritable = false;
		SafeMILHandle safeMILHandle = null;
		BitmapDecoder bitmapDecoder = null;
		Uri uri2 = null;
		Stream uriStream = null;
		UnmanagedMemoryStream unmanagedMemoryStream = null;
		SafeFileHandle safeFilehandle = null;
		if (uri != null)
		{
			uri2 = ((baseUri != null) ? BaseUriHelper.GetResolvedUri(baseUri, uri) : uri);
			if (insertInDecoderCache)
			{
				if ((createOptions & BitmapCreateOptions.IgnoreImageCache) != 0)
				{
					ImagingCache.RemoveFromDecoderCache(uri2);
				}
				bitmapDecoder = CheckCache(uri2, out clsId);
			}
		}
		if (bitmapDecoder != null)
		{
			safeMILHandle = bitmapDecoder.InternalDecoder;
		}
		else
		{
			if (uri2 != null && uri2.IsAbsoluteUri && stream == null && (uri2.Scheme == Uri.UriSchemeHttp || uri2.Scheme == Uri.UriSchemeHttps))
			{
				return new LateBoundBitmapDecoder(baseUri, uri, stream, createOptions, cacheOption, uriCachePolicy);
			}
			if (stream != null && !stream.CanSeek)
			{
				return new LateBoundBitmapDecoder(baseUri, uri, stream, createOptions, cacheOption, uriCachePolicy);
			}
			safeMILHandle = SetupDecoderFromUriOrStream(uri2, stream, cacheOption, out clsId, out isOriginalWritable, out uriStream, out unmanagedMemoryStream, out safeFilehandle);
		}
		BitmapDecoder bitmapDecoder2 = null;
		if (MILGuidData.GUID_ContainerFormatBmp == clsId)
		{
			return new BmpBitmapDecoder(safeMILHandle, bitmapDecoder, baseUri, uri, stream, createOptions, cacheOption, insertInDecoderCache, isOriginalWritable, uriStream, unmanagedMemoryStream, safeFilehandle);
		}
		if (MILGuidData.GUID_ContainerFormatGif == clsId)
		{
			return new GifBitmapDecoder(safeMILHandle, bitmapDecoder, baseUri, uri, stream, createOptions, cacheOption, insertInDecoderCache, isOriginalWritable, uriStream, unmanagedMemoryStream, safeFilehandle);
		}
		if (MILGuidData.GUID_ContainerFormatIco == clsId)
		{
			return new IconBitmapDecoder(safeMILHandle, bitmapDecoder, baseUri, uri, stream, createOptions, cacheOption, insertInDecoderCache, isOriginalWritable, uriStream, unmanagedMemoryStream, safeFilehandle);
		}
		if (MILGuidData.GUID_ContainerFormatJpeg == clsId)
		{
			return new JpegBitmapDecoder(safeMILHandle, bitmapDecoder, baseUri, uri, stream, createOptions, cacheOption, insertInDecoderCache, isOriginalWritable, uriStream, unmanagedMemoryStream, safeFilehandle);
		}
		if (MILGuidData.GUID_ContainerFormatPng == clsId)
		{
			return new PngBitmapDecoder(safeMILHandle, bitmapDecoder, baseUri, uri, stream, createOptions, cacheOption, insertInDecoderCache, isOriginalWritable, uriStream, unmanagedMemoryStream, safeFilehandle);
		}
		if (MILGuidData.GUID_ContainerFormatTiff == clsId)
		{
			return new TiffBitmapDecoder(safeMILHandle, bitmapDecoder, baseUri, uri, stream, createOptions, cacheOption, insertInDecoderCache, isOriginalWritable, uriStream, unmanagedMemoryStream, safeFilehandle);
		}
		if (MILGuidData.GUID_ContainerFormatWmp == clsId)
		{
			return new WmpBitmapDecoder(safeMILHandle, bitmapDecoder, baseUri, uri, stream, createOptions, cacheOption, insertInDecoderCache, isOriginalWritable, uriStream, unmanagedMemoryStream, safeFilehandle);
		}
		return new UnknownBitmapDecoder(safeMILHandle, bitmapDecoder, baseUri, uri, stream, createOptions, cacheOption, insertInDecoderCache, isOriginalWritable, uriStream, unmanagedMemoryStream, safeFilehandle);
	}

	/// <summary>Creates a <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" /> from a <see cref="T:System.Uri" /> by using the specified <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" /> and <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" /> from a <see cref="T:System.Uri" /> by using the specified <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" /> and <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" />.</returns>
	/// <param name="bitmapUri">The <see cref="T:System.Uri" /> of the bitmap to decode.</param>
	/// <param name="createOptions">Identifies the <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" /> for this decoder.</param>
	/// <param name="cacheOption">Identifies the <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" /> for this decoder.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="bitmapUri" /> is null.</exception>
	/// <exception cref="T:System.IO.FileFormatException">The <paramref name="bitmapUri" /> specifies a class ID of an unsupported format type.</exception>
	public static BitmapDecoder Create(Uri bitmapUri, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
	{
		return Create(bitmapUri, createOptions, cacheOption, null);
	}

	/// <summary>Creates a <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" /> from a <see cref="T:System.Uri" /> by using the specified <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" />, <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" /> and <see cref="T:System.Net.Cache.RequestCachePolicy" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" /> from a <see cref="T:System.Uri" /> by using the specified <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" />, <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" /> and <see cref="T:System.Net.Cache.RequestCachePolicy" />.</returns>
	/// <param name="bitmapUri">The location of the bitmap from which the <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" /> is created.</param>
	/// <param name="createOptions">The options that are used to create this <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" />.</param>
	/// <param name="cacheOption">The cache option that is used to create this <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" />.</param>
	/// <param name="uriCachePolicy">The caching requirements for this <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" />.</param>
	public static BitmapDecoder Create(Uri bitmapUri, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, RequestCachePolicy uriCachePolicy)
	{
		if (bitmapUri == null)
		{
			throw new ArgumentNullException("bitmapUri");
		}
		return CreateFromUriOrStream(null, bitmapUri, null, createOptions, cacheOption, uriCachePolicy, insertInDecoderCache: true);
	}

	/// <summary>Creates a <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" /> from a <see cref="T:System.IO.Stream" /> by using the specified <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" /> and <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" /> from a <see cref="T:System.IO.Stream" /> by using the specified <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" /> and <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" />.</returns>
	/// <param name="bitmapStream">The file stream that identifies the bitmap to decode.</param>
	/// <param name="createOptions">Identifies the <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" /> for this decoder.</param>
	/// <param name="cacheOption">Identifies the <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" /> for this decoder.</param>
	public static BitmapDecoder Create(Stream bitmapStream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
	{
		if (bitmapStream == null)
		{
			throw new ArgumentNullException("bitmapStream");
		}
		return CreateFromUriOrStream(null, null, bitmapStream, createOptions, cacheOption, null, insertInDecoderCache: true);
	}

	/// <summary>Creates an instance of <see cref="T:System.Windows.Media.Imaging.InPlaceBitmapMetadataWriter" />, which can be used to update the metadata of a bitmap.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.InPlaceBitmapMetadataWriter" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The file original image stream is read only.</exception>
	/// <exception cref="T:System.NotImplementedException">The decoder is not a built in decoder.</exception>
	public virtual InPlaceBitmapMetadataWriter CreateInPlaceBitmapMetadataWriter()
	{
		VerifyAccess();
		EnsureBuiltInDecoder();
		CheckOriginalWritable();
		return InPlaceBitmapMetadataWriter.CreateFromDecoder(_decoderHandle, _syncObject);
	}

	/// <summary>Converts the current value of a <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>A string representation of the <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" />.</returns>
	public override string ToString()
	{
		VerifyAccess();
		if (!_isBuiltInDecoder)
		{
			return base.ToString();
		}
		if (_uri != null)
		{
			if (_baseUri != null)
			{
				return BindUriHelper.UriToString(new Uri(_baseUri, _uri));
			}
			return BindUriHelper.UriToString(_uri);
		}
		return "image";
	}

	private int GetColorContexts(ref uint numContexts, nint[] colorContextPtrs)
	{
		Invariant.Assert(colorContextPtrs == null || numContexts <= colorContextPtrs.Length);
		return UnsafeNativeMethods.WICBitmapDecoder.GetColorContexts(_decoderHandle, numContexts, colorContextPtrs, out numContexts);
	}

	internal void CheckOriginalWritable()
	{
		if (!_isOriginalWritable)
		{
			throw new InvalidOperationException(SR.Image_OriginalStreamReadOnly);
		}
	}

	internal static SafeMILHandle SetupDecoderFromUriOrStream(Uri uri, Stream stream, BitmapCacheOption cacheOption, out Guid clsId, out bool isOriginalWritable, out Stream uriStream, out UnmanagedMemoryStream unmanagedMemoryStream, out SafeFileHandle safeFilehandle)
	{
		nint ppIDecode = IntPtr.Zero;
		Stream stream2 = null;
		unmanagedMemoryStream = null;
		safeFilehandle = null;
		isOriginalWritable = false;
		uriStream = null;
		if (uri != null)
		{
		}
		if (uri != null)
		{
			if (uri.IsAbsoluteUri && string.Equals(uri.Scheme, PackUriHelper.UriSchemePack, StringComparison.OrdinalIgnoreCase))
			{
				stream2 = (uriStream = WpfWebRequestHelper.CreateRequestAndGetResponse(uri).GetResponseStream());
			}
			if (stream2 == null || stream2 == Stream.Null)
			{
				if (uri.IsAbsoluteUri)
				{
					if (SecurityHelper.MapUrlToZoneWrapper(uri) != 0)
					{
						stream2 = ((uri.IsFile && uri.IsUnc) ? ProcessUncFiles(uri) : ((uri.Scheme == Uri.UriSchemeHttp) ? ProcessHttpFiles(uri, stream) : ((!(uri.Scheme == Uri.UriSchemeHttps)) ? WpfWebRequestHelper.CreateRequestAndGetResponseStream(uri) : ProcessHttpsFiles(uri, stream))));
					}
					else if (uri.IsFile)
					{
						stream2 = new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
					}
				}
				else
				{
					stream2 = new FileStream(uri.OriginalString, FileMode.Open, FileAccess.Read, FileShare.Read);
				}
				uriStream = stream2;
			}
		}
		if (stream2 != null)
		{
			stream = stream2;
		}
		else
		{
			isOriginalWritable = stream.CanSeek && stream.CanWrite;
		}
		stream = GetSeekableStream(stream);
		if (stream is UnmanagedMemoryStream)
		{
			unmanagedMemoryStream = stream as UnmanagedMemoryStream;
		}
		nint ptr = IntPtr.Zero;
		if (stream is FileStream)
		{
			FileStream fileStream = stream as FileStream;
			try
			{
				if (!fileStream.IsAsync)
				{
					safeFilehandle = fileStream.SafeFileHandle;
				}
				else
				{
					safeFilehandle = null;
				}
			}
			catch
			{
				safeFilehandle = null;
			}
		}
		SafeMILHandle safeMILHandle;
		try
		{
			Guid guidVendor = new Guid(MILGuidData.GUID_VendorMicrosoft);
			uint metadataFlags = 0u;
			if (cacheOption == BitmapCacheOption.OnLoad)
			{
				metadataFlags = 1u;
			}
			if (safeFilehandle != null)
			{
				using FactoryMaker factoryMaker = new FactoryMaker();
				HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateDecoderFromFileHandle(factoryMaker.ImagingFactoryPtr, safeFilehandle, ref guidVendor, metadataFlags, out ppIDecode));
			}
			else
			{
				ptr = GetIStreamFromStream(ref stream);
				using FactoryMaker factoryMaker2 = new FactoryMaker();
				HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateDecoderFromStream(factoryMaker2.ImagingFactoryPtr, ptr, ref guidVendor, metadataFlags, out ppIDecode));
			}
			safeMILHandle = new SafeMILHandle(ppIDecode);
		}
		catch
		{
			safeMILHandle = null;
			throw;
		}
		finally
		{
			UnsafeNativeMethods.MILUnknown.ReleaseInterface(ref ptr);
		}
		clsId = GetCLSIDFromDecoder(safeMILHandle, out var _);
		return safeMILHandle;
	}

	private static Stream ProcessHttpsFiles(Uri uri, Stream stream)
	{
		Stream stream2 = stream;
		if (stream2 == null || !stream2.CanSeek)
		{
			stream2 = WpfWebRequestHelper.GetResponseStream(WpfWebRequestHelper.CreateRequest(uri));
		}
		return stream2;
	}

	private static Stream ProcessHttpFiles(Uri uri, Stream stream)
	{
		Stream stream2 = stream;
		if (stream2 == null || !stream2.CanSeek)
		{
			stream2 = WpfWebRequestHelper.GetResponseStream(WpfWebRequestHelper.CreateRequest(uri));
		}
		return stream2;
	}

	private static Stream ProcessUncFiles(Uri uri)
	{
		return new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
	}

	private static Guid GetCLSIDFromDecoder(SafeMILHandle decoderHandle, out string decoderMimeTypes)
	{
		SafeMILHandle ppIDecoderInfo = new SafeMILHandle();
		HRESULT.Check(UnsafeNativeMethods.WICBitmapDecoder.GetDecoderInfo(decoderHandle, out ppIDecoderInfo));
		HRESULT.Check(UnsafeNativeMethods.WICBitmapCodecInfo.GetContainerFormat(ppIDecoderInfo, out var pguidContainerFormat));
		StringBuilder stringBuilder = null;
		uint pcchActual = 0u;
		HRESULT.Check(UnsafeNativeMethods.WICBitmapCodecInfo.GetMimeTypes(ppIDecoderInfo, 0u, stringBuilder, out pcchActual));
		if (pcchActual != 0)
		{
			stringBuilder = new StringBuilder((int)pcchActual);
			HRESULT.Check(UnsafeNativeMethods.WICBitmapCodecInfo.GetMimeTypes(ppIDecoderInfo, pcchActual, stringBuilder, out pcchActual));
		}
		if (stringBuilder != null)
		{
			decoderMimeTypes = stringBuilder.ToString();
		}
		else
		{
			decoderMimeTypes = string.Empty;
		}
		return pguidContainerFormat;
	}

	private static Stream GetSeekableStream(Stream bitmapStream)
	{
		if (bitmapStream.CanSeek)
		{
			return bitmapStream;
		}
		MemoryStream memoryStream = new MemoryStream();
		byte[] buffer = new byte[1024];
		while (true)
		{
			int num = bitmapStream.Read(buffer, 0, 1024);
			if (num <= 0)
			{
				break;
			}
			memoryStream.Write(buffer, 0, num);
		}
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return memoryStream;
	}

	private static BitmapDecoder CheckCache(Uri uri, out Guid clsId)
	{
		clsId = Guid.Empty;
		if (uri != null && ImagingCache.CheckDecoderCache(uri) is WeakReference weakReference)
		{
			BitmapDecoder bitmapDecoder = weakReference.Target as BitmapDecoder;
			if (bitmapDecoder != null && bitmapDecoder.CheckAccess())
			{
				lock (bitmapDecoder.SyncObject)
				{
					clsId = GetCLSIDFromDecoder(bitmapDecoder.InternalDecoder, out var _);
					return bitmapDecoder;
				}
			}
			if (bitmapDecoder == null)
			{
				ImagingCache.RemoveFromDecoderCache(uri);
			}
		}
		return null;
	}

	private void Initialize(BitmapDecoder decoder)
	{
		_isBuiltInDecoder = true;
		if (decoder != null)
		{
			SetupFrames(decoder, null);
			_cachedDecoder = decoder;
		}
		else if ((_createOptions & BitmapCreateOptions.DelayCreation) == 0 && _cacheOption == BitmapCacheOption.OnLoad)
		{
			SetupFrames(null, null);
			CloseStream();
		}
		if (_uri != null && decoder == null && _shouldCacheDecoder)
		{
			ImagingCache.AddToDecoderCache((_baseUri == null) ? _uri : new Uri(_baseUri, _uri), new WeakReference(this));
		}
	}

	internal void CloseStream()
	{
		if (_uriStream != null)
		{
			_uriStream.Close();
			_uriStream = null;
			GC.SuppressFinalize(this);
		}
	}

	internal void SetupFrames(BitmapDecoder decoder, ReadOnlyCollection<BitmapFrame> frames)
	{
		uint pFrameCount = 1u;
		HRESULT.Check(UnsafeNativeMethods.WICBitmapDecoder.GetFrameCount(_decoderHandle, out pFrameCount));
		_frames = new List<BitmapFrame>((int)pFrameCount);
		for (int i = 0; i < (int)pFrameCount; i++)
		{
			if (i > 0 && _cacheOption != BitmapCacheOption.OnLoad)
			{
				_createOptions |= BitmapCreateOptions.DelayCreation;
			}
			BitmapFrameDecode bitmapFrameDecode = null;
			if (frames != null && frames.Count == i + 1)
			{
				bitmapFrameDecode = frames[i] as BitmapFrameDecode;
				bitmapFrameDecode.UpdateDecoder(this);
			}
			else if (decoder == null)
			{
				bitmapFrameDecode = new BitmapFrameDecode(i, _createOptions, _cacheOption, this);
				bitmapFrameDecode.Freeze();
			}
			else
			{
				bitmapFrameDecode = new BitmapFrameDecode(i, _createOptions, _cacheOption, decoder.Frames[i] as BitmapFrameDecode);
				bitmapFrameDecode.Freeze();
			}
			_frames.Add(bitmapFrameDecode);
		}
	}

	private void EnsureBuiltInDecoder()
	{
		if (!_isBuiltInDecoder)
		{
			throw new NotImplementedException();
		}
	}

	private unsafe static nint GetIStreamFromStream(ref Stream bitmapStream)
	{
		nint ptr = IntPtr.Zero;
		bool canSeek = bitmapStream.CanSeek;
		if (bitmapStream is UnmanagedMemoryStream)
		{
			UnmanagedMemoryStream obj = bitmapStream as UnmanagedMemoryStream;
			nint zero = IntPtr.Zero;
			int num = 0;
			zero = (nint)obj.PositionPointer;
			num = (int)obj.Length;
			if (zero != IntPtr.Zero)
			{
				ptr = StreamAsIStream.IStreamFrom(zero, num);
			}
		}
		else
		{
			ptr = StreamAsIStream.IStreamFrom(bitmapStream);
			if (ptr == IntPtr.Zero)
			{
				throw new InvalidOperationException(SR.Image_CantDealWithStream);
			}
			if (!canSeek || (!bitmapStream.CanWrite && bitmapStream.Length <= 1048576))
			{
				nint num2 = StreamAsIStream.IStreamMemoryFrom(ptr);
				if (num2 != IntPtr.Zero)
				{
					UnsafeNativeMethods.MILUnknown.ReleaseInterface(ref ptr);
					bitmapStream = Stream.Null;
					return num2;
				}
				if (!canSeek)
				{
					throw new InvalidOperationException(SR.Image_CantDealWithStream);
				}
			}
		}
		if (ptr == IntPtr.Zero)
		{
			throw new InvalidOperationException(SR.Image_CantDealWithStream);
		}
		return ptr;
	}

	internal bool CanConvertToString()
	{
		return _uri != null;
	}

	internal abstract void SealObject();
}
