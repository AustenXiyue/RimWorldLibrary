using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Encodes a collection of <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> objects to an image stream. </summary>
public abstract class BitmapEncoder : DispatcherObject
{
	private enum EncodeState
	{
		None,
		EncoderInitialized,
		EncoderThumbnailSet,
		EncoderPaletteSet,
		EncoderCreatedNewFrame,
		FrameEncodeInitialized,
		FrameEncodeSizeSet,
		FrameEncodeResolutionSet,
		FrameEncodeThumbnailSet,
		FrameEncodeMetadataSet,
		FrameEncodeFormatSet,
		FrameEncodeSourceWritten,
		FrameEncodeCommitted,
		EncoderCommitted,
		FrameEncodeColorContextsSet
	}

	internal bool _supportsPreview = true;

	internal bool _supportsGlobalThumbnail = true;

	internal bool _supportsGlobalMetadata = true;

	internal bool _supportsFrameThumbnails = true;

	internal bool _supportsFrameMetadata = true;

	internal bool _supportsMultipleFrames;

	internal bool _supportsColorContext;

	private bool _isBuiltIn;

	private SafeMILHandle _encoderHandle;

	private BitmapMetadata _metadata;

	private SafeMILHandle _metadataHandle;

	private ReadOnlyCollection<ColorContext> _readOnlycolorContexts;

	private BitmapCodecInfoInternal _codecInfo;

	private BitmapSource _thumbnail;

	private BitmapSource _preview;

	private BitmapPalette _palette;

	private IList<BitmapFrame> _frames;

	private bool _hasSaved;

	private IList<SafeMILHandle> _frameHandles = new List<SafeMILHandle>(0);

	private IList<SafeMILHandle> _writeSourceHandles = new List<SafeMILHandle>(0);

	private EncodeState _encodeState;

	/// <summary>Gets or sets a value that represents the color profile that is associated with this encoder.</summary>
	/// <returns>The collection of <see cref="T:System.Windows.Media.ColorContext" /> objects that represents the color profiles that this encoder uses.</returns>
	/// <exception cref="T:System.InvalidOperationException">The encoder does not support color profiles.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Windows.Media.ColorContext" /> value that is passed to the encoder is null.</exception>
	public virtual ReadOnlyCollection<ColorContext> ColorContexts
	{
		get
		{
			VerifyAccess();
			EnsureBuiltIn();
			return _readOnlycolorContexts;
		}
		set
		{
			VerifyAccess();
			EnsureBuiltIn();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!_supportsColorContext)
			{
				throw new InvalidOperationException(SR.Image_EncoderNoColorContext);
			}
			_readOnlycolorContexts = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that represents the global embedded thumbnail.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that represents the thumbnail of the bitmap.</returns>
	/// <exception cref="T:System.InvalidOperationException">The bitmap does not support thumbnails.</exception>
	/// <exception cref="T:System.ArgumentNullException">The value is set to null.</exception>
	public virtual BitmapSource Thumbnail
	{
		get
		{
			VerifyAccess();
			EnsureBuiltIn();
			return _thumbnail;
		}
		set
		{
			VerifyAccess();
			EnsureBuiltIn();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!_supportsGlobalThumbnail)
			{
				throw new InvalidOperationException(SR.Image_EncoderNoGlobalThumbnail);
			}
			_thumbnail = value;
		}
	}

	/// <summary>Gets or sets the metadata that will be associated with this bitmap during encoding.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The encoder does not support global metadata.</exception>
	/// <exception cref="T:System.ArgumentNullException">The metadata value that is passed to the encoder is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">An attempt is made to write metadata in an incompatible format.</exception>
	public virtual BitmapMetadata Metadata
	{
		get
		{
			VerifyAccess();
			EnsureBuiltIn();
			EnsureMetadata(createBitmapMetadata: true);
			return _metadata;
		}
		set
		{
			VerifyAccess();
			EnsureBuiltIn();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.GuidFormat != ContainerFormat)
			{
				throw new InvalidOperationException(SR.Image_MetadataNotCompatible);
			}
			if (!_supportsGlobalMetadata)
			{
				throw new InvalidOperationException(SR.Image_EncoderNoGlobalMetadata);
			}
			_metadata = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that represents the global preview of a bitmap, if there is one. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that represents the preview of a bitmap.</returns>
	/// <exception cref="T:System.InvalidOperationException">The bitmap does not support preview.</exception>
	/// <exception cref="T:System.ArgumentNullException">The value is set to null.</exception>
	public virtual BitmapSource Preview
	{
		get
		{
			VerifyAccess();
			EnsureBuiltIn();
			return _preview;
		}
		set
		{
			VerifyAccess();
			EnsureBuiltIn();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!_supportsPreview)
			{
				throw new InvalidOperationException(SR.Image_EncoderNoPreview);
			}
			_preview = value;
		}
	}

	/// <summary>Gets information that describes this codec. </summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapCodecInfo" />.</returns>
	public virtual BitmapCodecInfo CodecInfo
	{
		get
		{
			VerifyAccess();
			EnsureBuiltIn();
			EnsureUnmanagedEncoder();
			if (_codecInfo == null)
			{
				SafeMILHandle ppIEncoderInfo = new SafeMILHandle();
				HRESULT.Check(UnsafeNativeMethods.WICBitmapEncoder.GetEncoderInfo(_encoderHandle, out ppIEncoderInfo));
				_codecInfo = new BitmapCodecInfoInternal(ppIEncoderInfo);
			}
			return _codecInfo;
		}
	}

	/// <summary>Gets or sets a value that represents the <see cref="T:System.Windows.Media.Imaging.BitmapPalette" /> of an encoded bitmap. </summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Windows.Media.Imaging.BitmapPalette" /> value that is passed to the encoder is null.</exception>
	public virtual BitmapPalette Palette
	{
		get
		{
			VerifyAccess();
			EnsureBuiltIn();
			return _palette;
		}
		set
		{
			VerifyAccess();
			EnsureBuiltIn();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_palette = value;
		}
	}

	/// <summary>Gets or sets the individual frames within an image.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> objects within the image.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> value that is passed to the encoder is null.</exception>
	public virtual IList<BitmapFrame> Frames
	{
		get
		{
			VerifyAccess();
			EnsureBuiltIn();
			if (_frames == null)
			{
				_frames = new List<BitmapFrame>(0);
			}
			return _frames;
		}
		set
		{
			VerifyAccess();
			EnsureBuiltIn();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_frames = value;
		}
	}

	internal virtual Guid ContainerFormat => Guid.Empty;

	internal virtual bool IsMetadataFixedSize => false;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.BitmapEncoder" /> class.</summary>
	protected BitmapEncoder()
	{
	}

	internal BitmapEncoder(bool isBuiltIn)
	{
		_isBuiltIn = isBuiltIn;
	}

	/// <summary>Creates a <see cref="T:System.Windows.Media.Imaging.BitmapEncoder" /> from a <see cref="T:System.Guid" /> that identifies the desired bitmap format. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapEncoder" /> that can encode to the specified <paramref name="containerFormat" />.</returns>
	/// <param name="containerFormat">Identifies the desired bitmap encoding format.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="containerFormat" /> is Empty.</exception>
	public static BitmapEncoder Create(Guid containerFormat)
	{
		if (containerFormat == Guid.Empty)
		{
			throw new ArgumentException(SR.Format(SR.Image_GuidEmpty, "containerFormat"), "containerFormat");
		}
		if (containerFormat == MILGuidData.GUID_ContainerFormatBmp)
		{
			return new BmpBitmapEncoder();
		}
		if (containerFormat == MILGuidData.GUID_ContainerFormatGif)
		{
			return new GifBitmapEncoder();
		}
		if (containerFormat == MILGuidData.GUID_ContainerFormatJpeg)
		{
			return new JpegBitmapEncoder();
		}
		if (containerFormat == MILGuidData.GUID_ContainerFormatPng)
		{
			return new PngBitmapEncoder();
		}
		if (containerFormat == MILGuidData.GUID_ContainerFormatTiff)
		{
			return new TiffBitmapEncoder();
		}
		if (containerFormat == MILGuidData.GUID_ContainerFormatWmp)
		{
			return new WmpBitmapEncoder();
		}
		return new UnknownBitmapEncoder(containerFormat);
	}

	/// <summary>Encodes a bitmap image to a specified <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="stream">Identifies the file stream that this bitmap is encoded to.</param>
	/// <exception cref="T:System.InvalidOperationException">The bitmap has already been encoded.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="P:System.Windows.Media.Imaging.BitmapEncoder.Frames" /> value that is passed to the encoder is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="P:System.Windows.Media.Imaging.BitmapEncoder.Frames" /> count is less than or equal to zero.</exception>
	public virtual void Save(Stream stream)
	{
		VerifyAccess();
		EnsureBuiltIn();
		EnsureUnmanagedEncoder();
		_ = _encodeState;
		if (_hasSaved)
		{
			throw new InvalidOperationException(SR.Image_OnlyOneSave);
		}
		if (_frames == null)
		{
			throw new NotSupportedException(SR.Format(SR.Image_NoFrames, null));
		}
		int count = _frames.Count;
		if (count <= 0)
		{
			throw new NotSupportedException(SR.Format(SR.Image_NoFrames, null));
		}
		nint ptr = IntPtr.Zero;
		SafeMILHandle encoderHandle = _encoderHandle;
		try
		{
			ptr = StreamAsIStream.IStreamFrom(stream);
			HRESULT.Check(UnsafeNativeMethods.WICBitmapEncoder.Initialize(encoderHandle, ptr, WICBitmapEncodeCacheOption.WICBitmapEncodeNoCache));
			_encodeState = EncodeState.EncoderInitialized;
			if (_thumbnail != null)
			{
				SafeMILHandle wicSourceHandle = _thumbnail.WicSourceHandle;
				lock (_thumbnail.SyncObject)
				{
					HRESULT.Check(UnsafeNativeMethods.WICBitmapEncoder.SetThumbnail(encoderHandle, wicSourceHandle));
					_encodeState = EncodeState.EncoderThumbnailSet;
				}
			}
			if (_palette != null && _palette.Colors.Count > 0)
			{
				SafeMILHandle internalPalette = _palette.InternalPalette;
				HRESULT.Check(UnsafeNativeMethods.WICBitmapEncoder.SetPalette(encoderHandle, internalPalette));
				_encodeState = EncodeState.EncoderPaletteSet;
			}
			if (_metadata != null && _metadata.GuidFormat == ContainerFormat)
			{
				EnsureMetadata(createBitmapMetadata: false);
				if (_metadata.InternalMetadataHandle != _metadataHandle)
				{
					PROPVARIANT propValue = default(PROPVARIANT);
					try
					{
						propValue.Init(_metadata);
						lock (_metadata.SyncObject)
						{
							HRESULT.Check(UnsafeNativeMethods.WICMetadataQueryWriter.SetMetadataByName(_metadataHandle, "/", ref propValue));
						}
					}
					finally
					{
						propValue.Clear();
					}
				}
			}
			for (int i = 0; i < count; i++)
			{
				SafeMILHandle ppIFramEncode = new SafeMILHandle();
				SafeMILHandle ppIEncoderOptions = new SafeMILHandle();
				HRESULT.Check(UnsafeNativeMethods.WICBitmapEncoder.CreateNewFrame(encoderHandle, out ppIFramEncode, out ppIEncoderOptions));
				_encodeState = EncodeState.EncoderCreatedNewFrame;
				_frameHandles.Add(ppIFramEncode);
				SaveFrame(ppIFramEncode, ppIEncoderOptions, _frames[i]);
				if (!_supportsMultipleFrames)
				{
					break;
				}
			}
			HRESULT.Check(UnsafeNativeMethods.WICBitmapEncoder.Commit(encoderHandle));
			_encodeState = EncodeState.EncoderCommitted;
		}
		finally
		{
			UnsafeNativeMethods.MILUnknown.ReleaseInterface(ref ptr);
		}
		_hasSaved = true;
	}

	internal virtual void SetupFrame(SafeMILHandle frameEncodeHandle, SafeMILHandle encoderOptions)
	{
		throw new NotImplementedException();
	}

	private void EnsureBuiltIn()
	{
		if (!_isBuiltIn)
		{
			throw new NotImplementedException();
		}
	}

	private void EnsureMetadata(bool createBitmapMetadata)
	{
		if (!_supportsGlobalMetadata)
		{
			return;
		}
		if (_metadataHandle == null)
		{
			SafeMILHandle ppIQueryWriter = new SafeMILHandle();
			int metadataQueryWriter = UnsafeNativeMethods.WICBitmapEncoder.GetMetadataQueryWriter(_encoderHandle, out ppIQueryWriter);
			if (metadataQueryWriter == -2003292287)
			{
				_supportsGlobalMetadata = false;
				return;
			}
			HRESULT.Check(metadataQueryWriter);
			_metadataHandle = ppIQueryWriter;
		}
		if (createBitmapMetadata && _metadata == null && _metadataHandle != null)
		{
			_metadata = new BitmapMetadata(_metadataHandle, readOnly: false, IsMetadataFixedSize, _metadataHandle);
		}
	}

	private void EnsureUnmanagedEncoder()
	{
		if (_encoderHandle == null)
		{
			using (FactoryMaker factoryMaker = new FactoryMaker())
			{
				SafeMILHandle ppICodec = null;
				Guid guidVendor = new Guid(MILGuidData.GUID_VendorMicrosoft);
				Guid guidContainerFormat = ContainerFormat;
				HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateEncoder(factoryMaker.ImagingFactoryPtr, ref guidContainerFormat, ref guidVendor, out ppICodec));
				_encoderHandle = ppICodec;
			}
		}
	}

	private void SaveFrame(SafeMILHandle frameEncodeHandle, SafeMILHandle encoderOptions, BitmapFrame frame)
	{
		SetupFrame(frameEncodeHandle, encoderOptions);
		_encodeState = EncodeState.FrameEncodeInitialized;
		HRESULT.Check(UnsafeNativeMethods.WICBitmapFrameEncode.SetSize(frameEncodeHandle, frame.PixelWidth, frame.PixelHeight));
		_encodeState = EncodeState.FrameEncodeSizeSet;
		double num = frame.DpiX;
		double num2 = frame.DpiY;
		if (num <= 0.0)
		{
			num = 96.0;
		}
		if (num2 <= 0.0)
		{
			num2 = 96.0;
		}
		HRESULT.Check(UnsafeNativeMethods.WICBitmapFrameEncode.SetResolution(frameEncodeHandle, num, num2));
		_encodeState = EncodeState.FrameEncodeResolutionSet;
		if (_supportsFrameThumbnails)
		{
			BitmapSource thumbnail = frame.Thumbnail;
			if (thumbnail != null)
			{
				SafeMILHandle wicSourceHandle = thumbnail.WicSourceHandle;
				lock (thumbnail.SyncObject)
				{
					HRESULT.Check(UnsafeNativeMethods.WICBitmapFrameEncode.SetThumbnail(frameEncodeHandle, wicSourceHandle));
					_encodeState = EncodeState.FrameEncodeThumbnailSet;
				}
			}
		}
		if (frame._isColorCorrected)
		{
			ColorContext colorContext = new ColorContext(frame.Format);
			nint[] ppIColorContext = new nint[1] { colorContext.ColorContextHandle.DangerousGetHandle() };
			if (UnsafeNativeMethods.WICBitmapFrameEncode.SetColorContexts(frameEncodeHandle, 1u, ppIColorContext) == 0)
			{
				_encodeState = EncodeState.FrameEncodeColorContextsSet;
			}
		}
		else
		{
			IList<ColorContext> colorContexts = frame.ColorContexts;
			if (colorContexts != null && colorContexts.Count > 0)
			{
				int count = colorContexts.Count;
				nint[] array = new nint[count];
				for (int i = 0; i < count; i++)
				{
					array[i] = colorContexts[i].ColorContextHandle.DangerousGetHandle();
				}
				if (UnsafeNativeMethods.WICBitmapFrameEncode.SetColorContexts(frameEncodeHandle, (uint)count, array) == 0)
				{
					_encodeState = EncodeState.FrameEncodeColorContextsSet;
				}
			}
		}
		lock (frame.SyncObject)
		{
			SafeMILHandle ppSourceOut = new SafeMILHandle();
			BitmapSourceSafeMILHandle wicSourceHandle2 = frame.WicSourceHandle;
			SafeMILHandle pIPalette = new SafeMILHandle();
			HRESULT.Check(UnsafeNativeMethods.WICCodec.WICSetEncoderFormat(wicSourceHandle2, pIPalette, frameEncodeHandle, out ppSourceOut));
			_encodeState = EncodeState.FrameEncodeFormatSet;
			_writeSourceHandles.Add(ppSourceOut);
			if (_supportsFrameMetadata && frame.Metadata is BitmapMetadata bitmapMetadata && bitmapMetadata.GuidFormat == ContainerFormat)
			{
				SafeMILHandle ppIQueryWriter = new SafeMILHandle();
				HRESULT.Check(UnsafeNativeMethods.WICBitmapFrameEncode.GetMetadataQueryWriter(frameEncodeHandle, out ppIQueryWriter));
				PROPVARIANT propValue = default(PROPVARIANT);
				try
				{
					propValue.Init(bitmapMetadata);
					lock (bitmapMetadata.SyncObject)
					{
						HRESULT.Check(UnsafeNativeMethods.WICMetadataQueryWriter.SetMetadataByName(ppIQueryWriter, "/", ref propValue));
						_encodeState = EncodeState.FrameEncodeMetadataSet;
					}
				}
				finally
				{
					propValue.Clear();
				}
			}
			Int32Rect r = default(Int32Rect);
			HRESULT.Check(UnsafeNativeMethods.WICBitmapFrameEncode.WriteSource(frameEncodeHandle, ppSourceOut, ref r));
			_encodeState = EncodeState.FrameEncodeSourceWritten;
			HRESULT.Check(UnsafeNativeMethods.WICBitmapFrameEncode.Commit(frameEncodeHandle));
			_encodeState = EncodeState.FrameEncodeCommitted;
		}
	}

	internal abstract void SealObject();
}
