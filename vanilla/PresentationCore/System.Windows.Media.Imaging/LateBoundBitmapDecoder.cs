using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Cache;

namespace System.Windows.Media.Imaging;

/// <summary>Defines a decoder that requires delayed bitmap creation such as asynchronous image downloads. </summary>
public sealed class LateBoundBitmapDecoder : BitmapDecoder
{
	private bool _isDownloading;

	private bool _failed;

	private BitmapDecoder _realDecoder;

	private RequestCachePolicy _requestCachePolicy;

	/// <summary>Gets the <see cref="T:System.Windows.Media.Imaging.BitmapPalette" /> that is associated with this decoder.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Imaging.BitmapPalette" /> that is associated with this decoder. If the bitmap has no palette, or if the <see cref="T:System.Windows.Media.Imaging.LateBoundBitmapDecoder" /> is still downloading content, this property returns null. This property has no default value.</returns>
	public override BitmapPalette Palette
	{
		get
		{
			VerifyAccess();
			if (_isDownloading)
			{
				return null;
			}
			return Decoder.Palette;
		}
	}

	/// <summary>Gets a value that represents the color profile that is associated with a bitmap, if one is defined.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.ColorContext" /> that represents the embedded color profile of the bitmap. If no color profile has been defined, or if the <see cref="T:System.Windows.Media.Imaging.LateBoundBitmapDecoder" /> is still downloading content, this property returns null. This property has no default value.</returns>
	public override ReadOnlyCollection<ColorContext> ColorContexts
	{
		get
		{
			VerifyAccess();
			if (_isDownloading)
			{
				return null;
			}
			return Decoder.ColorContexts;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that represents the thumbnail of the bitmap, if one is defined.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that represents a thumbnail of the bitmap. If no thumbnail is defined, or if the <see cref="T:System.Windows.Media.Imaging.LateBoundBitmapDecoder" /> is still downloading content, this property returns null. This property has no default value.</returns>
	public override BitmapSource Thumbnail
	{
		get
		{
			VerifyAccess();
			if (_isDownloading)
			{
				return null;
			}
			return Decoder.Thumbnail;
		}
	}

	/// <summary>Gets information that describes this codec.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapCodecInfo" />. This property has no default value. If the late bound decoder is still downloading, this property returns null.</returns>
	public override BitmapCodecInfo CodecInfo
	{
		get
		{
			VerifyAccess();
			if (_isDownloading)
			{
				return null;
			}
			return Decoder.CodecInfo;
		}
	}

	/// <summary>Gets the content of an individual frame within a bitmap.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />. This property has no default value.</returns>
	public override ReadOnlyCollection<BitmapFrame> Frames
	{
		get
		{
			VerifyAccess();
			if (_isDownloading)
			{
				if (_readOnlyFrames == null)
				{
					_frames = new List<BitmapFrame>(1);
					_frames.Add(new BitmapFrameDecode(0, _createOptions, _cacheOption, this));
					_readOnlyFrames = new ReadOnlyCollection<BitmapFrame>(_frames);
				}
				return _readOnlyFrames;
			}
			return Decoder.Frames;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that represents the global preview of this bitmap, if one is defined.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that represents the global preview of the bitmap. If no preview is defined, or if the <see cref="T:System.Windows.Media.Imaging.LateBoundBitmapDecoder" /> is still downloading content, this property returns null. This property has no default value.</returns>
	public override BitmapSource Preview
	{
		get
		{
			VerifyAccess();
			if (_isDownloading)
			{
				return null;
			}
			return Decoder.Preview;
		}
	}

	/// <summary>Gets the underlying decoder that is associated with this late-bound decoder.</summary>
	/// <returns>The underlying <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" />. If the <see cref="T:System.Windows.Media.Imaging.LateBoundBitmapDecoder" /> is still downloading a bitmap, the underlying decoder is null. Otherwise, the underlying decoder is created on first access.</returns>
	public BitmapDecoder Decoder
	{
		get
		{
			VerifyAccess();
			if (_isDownloading || _failed)
			{
				return null;
			}
			EnsureDecoder();
			return _realDecoder;
		}
	}

	/// <summary>Gets a value that indicates whether the decoder is currently downloading content.</summary>
	/// <returns>true if the decoder is downloading content; otherwise, false.</returns>
	public override bool IsDownloading
	{
		get
		{
			VerifyAccess();
			return _isDownloading;
		}
	}

	internal LateBoundBitmapDecoder(Uri baseUri, Uri uri, Stream stream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, RequestCachePolicy requestCachePolicy)
		: base(isBuiltIn: true)
	{
		_baseUri = baseUri;
		_uri = uri;
		_stream = stream;
		_createOptions = createOptions;
		_cacheOption = cacheOption;
		_requestCachePolicy = requestCachePolicy;
		Uri uri2 = ((_baseUri != null) ? new Uri(_baseUri, _uri) : _uri);
		if (uri2 != null && (uri2.Scheme == Uri.UriSchemeHttp || uri2.Scheme == Uri.UriSchemeHttps))
		{
			BitmapDownload.BeginDownload(this, uri2, _requestCachePolicy, _stream);
			_isDownloading = true;
		}
		if (_stream != null && !_stream.CanSeek)
		{
			BitmapDownload.BeginDownload(this, uri2, _requestCachePolicy, _stream);
			_isDownloading = true;
		}
	}

	private void EnsureDecoder()
	{
		if (_realDecoder == null)
		{
			_realDecoder = BitmapDecoder.CreateFromUriOrStream(_baseUri, _uri, _stream, _createOptions & ~BitmapCreateOptions.DelayCreation, _cacheOption, _requestCachePolicy, insertInDecoderCache: true);
			if (_readOnlyFrames != null)
			{
				_realDecoder.SetupFrames(null, _readOnlyFrames);
				_readOnlyFrames = null;
				_frames = null;
			}
		}
	}

	internal object DownloadCallback(object arg)
	{
		Stream stream = (Stream)arg;
		_stream = stream;
		if ((_createOptions & BitmapCreateOptions.DelayCreation) == 0)
		{
			try
			{
				EnsureDecoder();
			}
			catch (Exception arg2)
			{
				return ExceptionCallback(arg2);
			}
		}
		_isDownloading = false;
		_downloadEvent.InvokeEvents(this, null);
		return null;
	}

	internal object ProgressCallback(object arg)
	{
		int percentComplete = (int)arg;
		_progressEvent.InvokeEvents(this, new DownloadProgressEventArgs(percentComplete));
		return null;
	}

	internal object ExceptionCallback(object arg)
	{
		_isDownloading = false;
		_failed = true;
		_failedEvent.InvokeEvents(this, new ExceptionEventArgs((Exception)arg));
		return null;
	}

	internal override void SealObject()
	{
		throw new NotImplementedException();
	}
}
