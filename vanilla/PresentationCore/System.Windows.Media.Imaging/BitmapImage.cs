using System.ComponentModel;
using System.IO;
using System.Net.Cache;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Provides a specialized <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that is optimized for loading images using Extensible Application Markup Language (XAML).</summary>
public sealed class BitmapImage : BitmapSource, ISupportInitialize, IUriContext
{
	private Uri _baseUri;

	private bool _isDownloading;

	private BitmapDecoder _decoder;

	private RequestCachePolicy _uriCachePolicy;

	private Uri _uriSource;

	private Stream _streamSource;

	private int _decodePixelWidth;

	private int _decodePixelHeight;

	private Rotation _rotation;

	private Int32Rect _sourceRect;

	private BitmapCreateOptions _createOptions;

	private BitmapCacheOption _cacheOption;

	private BitmapSource _finalSource;

	private BitmapImage _cachedBitmapImage;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.BitmapImage.UriCachePolicy" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.BitmapImage.UriCachePolicy" /> dependency property.</returns>
	public static readonly DependencyProperty UriCachePolicyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.BitmapImage.UriSource" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.BitmapImage.UriSource" /> dependency property.</returns>
	public static readonly DependencyProperty UriSourceProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.BitmapImage.StreamSource" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.BitmapImage.StreamSource" /> dependency property.</returns>
	public static readonly DependencyProperty StreamSourceProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.BitmapImage.DecodePixelWidth" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.BitmapImage.DecodePixelWidth" /> dependency property.</returns>
	public static readonly DependencyProperty DecodePixelWidthProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.BitmapImage.DecodePixelHeight" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.BitmapImage.DecodePixelHeight" /> dependency property.</returns>
	public static readonly DependencyProperty DecodePixelHeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.BitmapImage.Rotation" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.BitmapImage.Rotation" /> dependency property.</returns>
	public static readonly DependencyProperty RotationProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.BitmapImage.SourceRect" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.BitmapImage.SourceRect" /> dependency property.</returns>
	public static readonly DependencyProperty SourceRectProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.BitmapImage.CreateOptions" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.BitmapImage.CreateOptions" /> dependency property.</returns>
	public static readonly DependencyProperty CreateOptionsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.BitmapImage.CacheOption" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.BitmapImage.CacheOption" /> dependency property.</returns>
	public static readonly DependencyProperty CacheOptionProperty;

	internal static RequestCachePolicy s_UriCachePolicy;

	internal static Uri s_UriSource;

	internal static Stream s_StreamSource;

	internal const int c_DecodePixelWidth = 0;

	internal const int c_DecodePixelHeight = 0;

	internal const Rotation c_Rotation = Rotation.Rotate0;

	internal static Int32Rect s_SourceRect;

	internal static BitmapCreateOptions s_CreateOptions;

	internal static BitmapCacheOption s_CacheOption;

	/// <summary>Gets or sets a value that represents the base <see cref="T:System.Uri" /> of the current <see cref="T:System.Windows.Media.Imaging.BitmapImage" /> context.</summary>
	/// <returns>The base <see cref="T:System.Uri" /> of the current context.</returns>
	public Uri BaseUri
	{
		get
		{
			ReadPreamble();
			return _baseUri;
		}
		set
		{
			WritePreamble();
			if (!base.CreationCompleted && _baseUri != value)
			{
				_baseUri = value;
				WritePostscript();
			}
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Media.Imaging.BitmapImage" /> is currently downloading content.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.Imaging.BitmapImage" /> is downloading content; otherwise, false.</returns>
	public override bool IsDownloading
	{
		get
		{
			ReadPreamble();
			return _isDownloading;
		}
	}

	/// <summary>Not supported. <see cref="T:System.Windows.Media.Imaging.BitmapImage" /> does not support the <see cref="P:System.Windows.Media.Imaging.BitmapImage.Metadata" /> property and will throw a <see cref="T:System.NotSupportedException" />.</summary>
	/// <returns>Not supported.</returns>
	/// <exception cref="T:System.NotSupportedException">An attempt to read the <see cref="P:System.Windows.Media.Imaging.BitmapImage.Metadata" /> occurs.</exception>
	public override ImageMetadata Metadata
	{
		get
		{
			throw new NotSupportedException(SR.Image_MetadataNotSupported);
		}
	}

	/// <summary>Gets or sets a value that represents the caching policy for images that come from an HTTP source. </summary>
	/// <returns>The base <see cref="T:System.Net.Cache.RequestCachePolicy" /> of the current context. The default is null.</returns>
	[TypeConverter(typeof(RequestCachePolicyConverter))]
	public RequestCachePolicy UriCachePolicy
	{
		get
		{
			return (RequestCachePolicy)GetValue(UriCachePolicyProperty);
		}
		set
		{
			SetValueInternal(UriCachePolicyProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Uri" /> source of the <see cref="T:System.Windows.Media.Imaging.BitmapImage" />.  </summary>
	/// <returns>The <see cref="T:System.Uri" /> source of the <see cref="T:System.Windows.Media.Imaging.BitmapImage" />. The default is null.</returns>
	public Uri UriSource
	{
		get
		{
			return (Uri)GetValue(UriSourceProperty);
		}
		set
		{
			SetValueInternal(UriSourceProperty, value);
		}
	}

	/// <summary>Gets or sets the stream source of the <see cref="T:System.Windows.Media.Imaging.BitmapImage" />.  </summary>
	/// <returns>The stream source of the <see cref="T:System.Windows.Media.Imaging.BitmapImage" />. The default is null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Stream StreamSource
	{
		get
		{
			return (Stream)GetValue(StreamSourceProperty);
		}
		set
		{
			SetValueInternal(StreamSourceProperty, value);
		}
	}

	/// <summary>Gets or sets the width, in pixels, that the image is decoded to.  </summary>
	/// <returns>The width, in pixels, that the image is decoded to. The default value is 0.</returns>
	public int DecodePixelWidth
	{
		get
		{
			return (int)GetValue(DecodePixelWidthProperty);
		}
		set
		{
			SetValueInternal(DecodePixelWidthProperty, value);
		}
	}

	/// <summary>Gets or sets the height, in pixels, that the image is decoded to.  </summary>
	/// <returns>The height, in pixels, that the image is decoded to. The default value is 0.</returns>
	public int DecodePixelHeight
	{
		get
		{
			return (int)GetValue(DecodePixelHeightProperty);
		}
		set
		{
			SetValueInternal(DecodePixelHeightProperty, value);
		}
	}

	/// <summary>Gets or sets the angle that this <see cref="T:System.Windows.Media.Imaging.BitmapImage" /> is rotated to.  </summary>
	/// <returns>The rotation that is used for the <see cref="T:System.Windows.Media.Imaging.BitmapImage" />. The default is <see cref="F:System.Windows.Media.Imaging.Rotation.Rotate0" />.</returns>
	public Rotation Rotation
	{
		get
		{
			return (Rotation)GetValue(RotationProperty);
		}
		set
		{
			SetValueInternal(RotationProperty, value);
		}
	}

	/// <summary>Gets or sets the rectangle that is used as the source of the <see cref="T:System.Windows.Media.Imaging.BitmapImage" />.  </summary>
	/// <returns>The rectangle that is used as the source of the <see cref="T:System.Windows.Media.Imaging.BitmapImage" />. The default is <see cref="P:System.Windows.Int32Rect.Empty" />.</returns>
	public Int32Rect SourceRect
	{
		get
		{
			return (Int32Rect)GetValue(SourceRectProperty);
		}
		set
		{
			SetValueInternal(SourceRectProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" /> for a <see cref="T:System.Windows.Media.Imaging.BitmapImage" />.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" /> used for this <see cref="T:System.Windows.Media.Imaging.BitmapImage" />. The default is <see cref="F:System.Windows.Media.Imaging.BitmapCreateOptions.None" />.</returns>
	public BitmapCreateOptions CreateOptions
	{
		get
		{
			return (BitmapCreateOptions)GetValue(CreateOptionsProperty);
		}
		set
		{
			SetValueInternal(CreateOptionsProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" /> to use for this instance of <see cref="T:System.Windows.Media.Imaging.BitmapImage" />.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" /> being used for the <see cref="T:System.Windows.Media.Imaging.BitmapImage" />. The default is <see cref="F:System.Windows.Media.Imaging.BitmapCacheOption.Default" />.</returns>
	public BitmapCacheOption CacheOption
	{
		get
		{
			return (BitmapCacheOption)GetValue(CacheOptionProperty);
		}
		set
		{
			SetValueInternal(CacheOptionProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.BitmapImage" /> class.</summary>
	public BitmapImage()
		: base(useVirtuals: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.BitmapImage" /> class by using the supplied <see cref="T:System.Uri" />.</summary>
	/// <param name="uriSource">The <see cref="T:System.Uri" /> to use as the source of the <see cref="T:System.Windows.Media.Imaging.BitmapImage" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="uriSource" /> parameter is null.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by the <paramref name="uriSource" /> parameter is not found.</exception>
	public BitmapImage(Uri uriSource)
		: this(uriSource, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.BitmapImage" /> class with an image whose source is a <see cref="T:System.Uri" />, and is cached according to the provided <see cref="T:System.Net.Cache.RequestCachePolicy" />.</summary>
	/// <param name="uriSource">The <see cref="T:System.Uri" /> to use as the source of the <see cref="T:System.Windows.Media.Imaging.BitmapImage" />.</param>
	/// <param name="uriCachePolicy">The <see cref="T:System.Net.Cache.RequestCachePolicy" /> that specifies the caching requirements for images that are obtained using HTTP.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="uriSource" /> parameter is null.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by the <paramref name="uriSource" /> parameter is not found.</exception>
	public BitmapImage(Uri uriSource, RequestCachePolicy uriCachePolicy)
		: base(useVirtuals: true)
	{
		if (uriSource == null)
		{
			throw new ArgumentNullException("uriSource");
		}
		BeginInit();
		UriSource = uriSource;
		UriCachePolicy = uriCachePolicy;
		EndInit();
	}

	/// <summary>Signals the start of the <see cref="T:System.Windows.Media.Imaging.BitmapImage" /> initialization.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.Imaging.BitmapImage" /> is currently being initialized. <see cref="M:System.Windows.Media.Imaging.BitmapImage.BeginInit" /> has already been called.-or-The <see cref="T:System.Windows.Media.Imaging.BitmapImage" /> has already been initialized.</exception>
	public void BeginInit()
	{
		WritePreamble();
		_bitmapInit.BeginInit();
	}

	/// <summary>Signals the end of the <see cref="T:System.Windows.Media.Imaging.BitmapImage" /> initialization.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Media.Imaging.BitmapImage.UriSource" /> or <see cref="P:System.Windows.Media.Imaging.BitmapImage.StreamSource" /> properties are null.-or-The <see cref="M:System.Windows.Media.Imaging.BitmapImage.EndInit" /> method is called without first calling <see cref="M:System.Windows.Media.Imaging.BitmapImage.BeginInit" />.</exception>
	public void EndInit()
	{
		WritePreamble();
		_bitmapInit.EndInit();
		if (UriSource == null && StreamSource == null)
		{
			throw new InvalidOperationException(SR.Format(SR.Image_NeitherArgument, "UriSource", "StreamSource"));
		}
		if (UriSource != null && !UriSource.IsAbsoluteUri && CacheOption != BitmapCacheOption.OnLoad)
		{
			base.DelayCreation = true;
		}
		if (!base.DelayCreation && !base.CreationCompleted)
		{
			FinalizeCreation();
		}
	}

	internal override bool CanSerializeToString()
	{
		ReadPreamble();
		if (UriSource != null && StreamSource == null && SourceRect.IsEmpty && DecodePixelWidth == 0 && DecodePixelHeight == 0 && Rotation == Rotation.Rotate0 && CreateOptions == BitmapCreateOptions.None && CacheOption == BitmapCacheOption.Default)
		{
			return UriCachePolicy == null;
		}
		return false;
	}

	internal override string ConvertToString(string format, IFormatProvider provider)
	{
		ReadPreamble();
		if (UriSource != null)
		{
			if (_baseUri != null)
			{
				return BindUriHelper.UriToString(new Uri(_baseUri, UriSource));
			}
			return BindUriHelper.UriToString(UriSource);
		}
		return base.ConvertToString(format, provider);
	}

	private void ClonePrequel(BitmapImage otherBitmapImage)
	{
		BeginInit();
		_isDownloading = otherBitmapImage._isDownloading;
		_decoder = otherBitmapImage._decoder;
		_baseUri = otherBitmapImage._baseUri;
	}

	private void ClonePostscript(BitmapImage otherBitmapImage)
	{
		if (_isDownloading)
		{
			_decoder.DownloadProgress += OnDownloadProgress;
			_decoder.DownloadCompleted += OnDownloadCompleted;
			_decoder.DownloadFailed += OnDownloadFailed;
		}
		EndInit();
	}

	private BitmapImage CheckCache(Uri uri)
	{
		if (uri != null && ImagingCache.CheckImageCache(uri) is WeakReference weakReference)
		{
			if (weakReference.Target is BitmapImage result)
			{
				return result;
			}
			ImagingCache.RemoveFromImageCache(uri);
		}
		return null;
	}

	private void InsertInCache(Uri uri)
	{
		if (uri != null)
		{
			ImagingCache.AddToImageCache(uri, new WeakReference(this));
		}
	}

	internal override void FinalizeCreation()
	{
		_bitmapInit.EnsureInitializedComplete();
		Uri uri = UriSource;
		if (_baseUri != null)
		{
			uri = new Uri(_baseUri, UriSource);
		}
		if ((CreateOptions & BitmapCreateOptions.IgnoreImageCache) != 0)
		{
			ImagingCache.RemoveFromImageCache(uri);
		}
		BitmapImage bitmapImage = CheckCache(uri);
		if (bitmapImage != null && bitmapImage.CheckAccess() && bitmapImage.SourceRect.Equals(SourceRect) && bitmapImage.DecodePixelWidth == DecodePixelWidth && bitmapImage.DecodePixelHeight == DecodePixelHeight && bitmapImage.Rotation == Rotation && (bitmapImage.CreateOptions & BitmapCreateOptions.IgnoreColorProfile) == (CreateOptions & BitmapCreateOptions.IgnoreColorProfile))
		{
			_syncObject = bitmapImage.SyncObject;
			lock (_syncObject)
			{
				base.WicSourceHandle = bitmapImage.WicSourceHandle;
				base.IsSourceCached = bitmapImage.IsSourceCached;
				_convertedDUCEPtr = bitmapImage._convertedDUCEPtr;
				_cachedBitmapImage = bitmapImage;
			}
			UpdateCachedSettings();
			return;
		}
		BitmapDecoder bitmapDecoder = null;
		if (_decoder == null)
		{
			bitmapDecoder = BitmapDecoder.CreateFromUriOrStream(_baseUri, UriSource, StreamSource, CreateOptions & ~BitmapCreateOptions.DelayCreation, BitmapCacheOption.None, _uriCachePolicy, insertInDecoderCache: false);
			if (bitmapDecoder.IsDownloading)
			{
				_isDownloading = true;
				_decoder = bitmapDecoder;
				bitmapDecoder.DownloadProgress += OnDownloadProgress;
				bitmapDecoder.DownloadCompleted += OnDownloadCompleted;
				bitmapDecoder.DownloadFailed += OnDownloadFailed;
			}
		}
		else
		{
			bitmapDecoder = _decoder;
			_decoder = null;
		}
		if (bitmapDecoder.Frames.Count == 0)
		{
			throw new ArgumentException(SR.Image_NoDecodeFrames);
		}
		BitmapFrame bitmapFrame = bitmapDecoder.Frames[0];
		BitmapSource bitmapSource = bitmapFrame;
		Int32Rect sourceRect = SourceRect;
		if (sourceRect.X == 0 && sourceRect.Y == 0 && sourceRect.Width == bitmapSource.PixelWidth && sourceRect.Height == bitmapSource.PixelHeight)
		{
			sourceRect = Int32Rect.Empty;
		}
		if (!sourceRect.IsEmpty)
		{
			CroppedBitmap croppedBitmap = new CroppedBitmap();
			croppedBitmap.BeginInit();
			croppedBitmap.Source = bitmapSource;
			croppedBitmap.SourceRect = sourceRect;
			croppedBitmap.EndInit();
			bitmapSource = croppedBitmap;
			if (_isDownloading)
			{
				bitmapSource.UnregisterDownloadEventSource();
			}
		}
		int num = DecodePixelWidth;
		int num2 = DecodePixelHeight;
		if (num == 0 && num2 == 0)
		{
			num = bitmapSource.PixelWidth;
			num2 = bitmapSource.PixelHeight;
		}
		else if (num == 0)
		{
			num = bitmapSource.PixelWidth * num2 / bitmapSource.PixelHeight;
		}
		else if (num2 == 0)
		{
			num2 = bitmapSource.PixelHeight * num / bitmapSource.PixelWidth;
		}
		if (num != bitmapSource.PixelWidth || num2 != bitmapSource.PixelHeight || Rotation != 0)
		{
			TransformedBitmap transformedBitmap = new TransformedBitmap();
			transformedBitmap.BeginInit();
			transformedBitmap.Source = bitmapSource;
			TransformGroup transformGroup = new TransformGroup();
			if (num != bitmapSource.PixelWidth || num2 != bitmapSource.PixelHeight)
			{
				int pixelWidth = bitmapSource.PixelWidth;
				int pixelHeight = bitmapSource.PixelHeight;
				transformGroup.Children.Add(new ScaleTransform(1.0 * (double)num / (double)pixelWidth, 1.0 * (double)num2 / (double)pixelHeight));
			}
			if (Rotation != 0)
			{
				double angle = 0.0;
				switch (Rotation)
				{
				case Rotation.Rotate0:
					angle = 0.0;
					break;
				case Rotation.Rotate90:
					angle = 90.0;
					break;
				case Rotation.Rotate180:
					angle = 180.0;
					break;
				case Rotation.Rotate270:
					angle = 270.0;
					break;
				}
				transformGroup.Children.Add(new RotateTransform(angle));
			}
			transformedBitmap.Transform = transformGroup;
			transformedBitmap.EndInit();
			bitmapSource = transformedBitmap;
			if (_isDownloading)
			{
				bitmapSource.UnregisterDownloadEventSource();
			}
		}
		if ((CreateOptions & BitmapCreateOptions.IgnoreColorProfile) == 0 && bitmapFrame.ColorContexts != null && bitmapFrame.ColorContexts[0] != null && bitmapFrame.ColorContexts[0].IsValid && bitmapSource.Format.Format != 0)
		{
			PixelFormat closestDUCEFormat = BitmapSource.GetClosestDUCEFormat(bitmapSource.Format, bitmapSource.Palette);
			bool flag = bitmapSource.Format != closestDUCEFormat;
			ColorContext colorContext;
			try
			{
				colorContext = new ColorContext(closestDUCEFormat);
			}
			catch (NotSupportedException)
			{
				colorContext = null;
			}
			if (colorContext != null)
			{
				bool flag2 = false;
				bool flag3 = false;
				try
				{
					bitmapSource = new ColorConvertedBitmap(bitmapSource, bitmapFrame.ColorContexts[0], colorContext, closestDUCEFormat);
					if (_isDownloading)
					{
						bitmapSource.UnregisterDownloadEventSource();
					}
					flag2 = true;
				}
				catch (NotSupportedException)
				{
				}
				catch (FileFormatException)
				{
					flag3 = true;
				}
				if (!flag2 && !flag3 && flag)
				{
					bitmapSource = new ColorConvertedBitmap(new FormatConvertedBitmap(bitmapSource, closestDUCEFormat, bitmapSource.Palette, 0.0), bitmapFrame.ColorContexts[0], colorContext, closestDUCEFormat);
					if (_isDownloading)
					{
						bitmapSource.UnregisterDownloadEventSource();
					}
				}
			}
		}
		if (CacheOption != BitmapCacheOption.None)
		{
			try
			{
				bitmapSource = new CachedBitmap(bitmapSource, CreateOptions & ~BitmapCreateOptions.DelayCreation, CacheOption);
				if (_isDownloading)
				{
					bitmapSource.UnregisterDownloadEventSource();
				}
			}
			catch (Exception e)
			{
				RecoverFromDecodeFailure(e);
				base.CreationCompleted = true;
				return;
			}
		}
		if (bitmapDecoder != null && CacheOption == BitmapCacheOption.OnLoad)
		{
			bitmapDecoder.CloseStream();
		}
		else if (CacheOption != BitmapCacheOption.OnLoad)
		{
			_finalSource = bitmapSource;
		}
		base.WicSourceHandle = bitmapSource.WicSourceHandle;
		base.IsSourceCached = bitmapSource.IsSourceCached;
		base.CreationCompleted = true;
		UpdateCachedSettings();
		if (!IsDownloading)
		{
			InsertInCache(uri);
		}
	}

	private void UriCachePolicyPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_uriCachePolicy = e.NewValue as RequestCachePolicy;
		}
	}

	private void UriSourcePropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_uriSource = e.NewValue as Uri;
		}
	}

	private void StreamSourcePropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_streamSource = e.NewValue as Stream;
		}
	}

	private void DecodePixelWidthPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_decodePixelWidth = (int)e.NewValue;
		}
	}

	private void DecodePixelHeightPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_decodePixelHeight = (int)e.NewValue;
		}
	}

	private void RotationPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_rotation = (Rotation)e.NewValue;
		}
	}

	private void SourceRectPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_sourceRect = (Int32Rect)e.NewValue;
		}
	}

	private void CreateOptionsPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		base.DelayCreation = ((_createOptions = (BitmapCreateOptions)e.NewValue) & BitmapCreateOptions.DelayCreation) != 0;
	}

	private void CacheOptionPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_cacheOption = (BitmapCacheOption)e.NewValue;
		}
	}

	private static object CoerceUriCachePolicy(DependencyObject d, object value)
	{
		BitmapImage bitmapImage = (BitmapImage)d;
		if (!bitmapImage._bitmapInit.IsInInit)
		{
			return bitmapImage._uriCachePolicy;
		}
		return value;
	}

	private static object CoerceUriSource(DependencyObject d, object value)
	{
		BitmapImage bitmapImage = (BitmapImage)d;
		if (!bitmapImage._bitmapInit.IsInInit)
		{
			return bitmapImage._uriSource;
		}
		return value;
	}

	private static object CoerceStreamSource(DependencyObject d, object value)
	{
		BitmapImage bitmapImage = (BitmapImage)d;
		if (!bitmapImage._bitmapInit.IsInInit)
		{
			return bitmapImage._streamSource;
		}
		return value;
	}

	private static object CoerceDecodePixelWidth(DependencyObject d, object value)
	{
		BitmapImage bitmapImage = (BitmapImage)d;
		if (!bitmapImage._bitmapInit.IsInInit)
		{
			return bitmapImage._decodePixelWidth;
		}
		return value;
	}

	private static object CoerceDecodePixelHeight(DependencyObject d, object value)
	{
		BitmapImage bitmapImage = (BitmapImage)d;
		if (!bitmapImage._bitmapInit.IsInInit)
		{
			return bitmapImage._decodePixelHeight;
		}
		return value;
	}

	private static object CoerceRotation(DependencyObject d, object value)
	{
		BitmapImage bitmapImage = (BitmapImage)d;
		if (!bitmapImage._bitmapInit.IsInInit)
		{
			return bitmapImage._rotation;
		}
		return value;
	}

	private static object CoerceSourceRect(DependencyObject d, object value)
	{
		BitmapImage bitmapImage = (BitmapImage)d;
		if (!bitmapImage._bitmapInit.IsInInit)
		{
			return bitmapImage._sourceRect;
		}
		return value;
	}

	private static object CoerceCreateOptions(DependencyObject d, object value)
	{
		BitmapImage bitmapImage = (BitmapImage)d;
		if (!bitmapImage._bitmapInit.IsInInit)
		{
			return bitmapImage._createOptions;
		}
		return value;
	}

	private static object CoerceCacheOption(DependencyObject d, object value)
	{
		BitmapImage bitmapImage = (BitmapImage)d;
		if (!bitmapImage._bitmapInit.IsInInit)
		{
			return bitmapImage._cacheOption;
		}
		return value;
	}

	private void OnDownloadCompleted(object sender, EventArgs e)
	{
		_isDownloading = false;
		_decoder.DownloadProgress -= OnDownloadProgress;
		_decoder.DownloadCompleted -= OnDownloadCompleted;
		_decoder.DownloadFailed -= OnDownloadFailed;
		if ((CreateOptions & BitmapCreateOptions.DelayCreation) != 0)
		{
			base.DelayCreation = true;
		}
		else
		{
			FinalizeCreation();
			_needsUpdate = true;
			RegisterForAsyncUpdateResource();
			FireChanged();
		}
		_downloadEvent.InvokeEvents(this, null);
	}

	private void OnDownloadProgress(object sender, DownloadProgressEventArgs e)
	{
		_progressEvent.InvokeEvents(this, e);
	}

	private void OnDownloadFailed(object sender, ExceptionEventArgs e)
	{
		_isDownloading = false;
		_decoder.DownloadProgress -= OnDownloadProgress;
		_decoder.DownloadCompleted -= OnDownloadCompleted;
		_decoder.DownloadFailed -= OnDownloadFailed;
		_failedEvent.InvokeEvents(this, e);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Imaging.BitmapImage" />, making deep copies of this object's values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new BitmapImage Clone()
	{
		return (BitmapImage)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Imaging.BitmapImage" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new BitmapImage CloneCurrentValue()
	{
		return (BitmapImage)base.CloneCurrentValue();
	}

	private static void UriCachePolicyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		BitmapImage obj = (BitmapImage)d;
		obj.UriCachePolicyPropertyChangedHook(e);
		obj.PropertyChanged(UriCachePolicyProperty);
	}

	private static void UriSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		BitmapImage obj = (BitmapImage)d;
		obj.UriSourcePropertyChangedHook(e);
		obj.PropertyChanged(UriSourceProperty);
	}

	private static void StreamSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		BitmapImage obj = (BitmapImage)d;
		obj.StreamSourcePropertyChangedHook(e);
		obj.PropertyChanged(StreamSourceProperty);
	}

	private static void DecodePixelWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		BitmapImage obj = (BitmapImage)d;
		obj.DecodePixelWidthPropertyChangedHook(e);
		obj.PropertyChanged(DecodePixelWidthProperty);
	}

	private static void DecodePixelHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		BitmapImage obj = (BitmapImage)d;
		obj.DecodePixelHeightPropertyChangedHook(e);
		obj.PropertyChanged(DecodePixelHeightProperty);
	}

	private static void RotationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		BitmapImage obj = (BitmapImage)d;
		obj.RotationPropertyChangedHook(e);
		obj.PropertyChanged(RotationProperty);
	}

	private static void SourceRectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		BitmapImage obj = (BitmapImage)d;
		obj.SourceRectPropertyChangedHook(e);
		obj.PropertyChanged(SourceRectProperty);
	}

	private static void CreateOptionsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		BitmapImage obj = (BitmapImage)d;
		obj.CreateOptionsPropertyChangedHook(e);
		obj.PropertyChanged(CreateOptionsProperty);
	}

	private static void CacheOptionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		BitmapImage obj = (BitmapImage)d;
		obj.CacheOptionPropertyChangedHook(e);
		obj.PropertyChanged(CacheOptionProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new BitmapImage();
	}

	protected override void CloneCore(Freezable source)
	{
		BitmapImage otherBitmapImage = (BitmapImage)source;
		ClonePrequel(otherBitmapImage);
		base.CloneCore(source);
		ClonePostscript(otherBitmapImage);
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		BitmapImage otherBitmapImage = (BitmapImage)source;
		ClonePrequel(otherBitmapImage);
		base.CloneCurrentValueCore(source);
		ClonePostscript(otherBitmapImage);
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		BitmapImage otherBitmapImage = (BitmapImage)source;
		ClonePrequel(otherBitmapImage);
		base.GetAsFrozenCore(source);
		ClonePostscript(otherBitmapImage);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		BitmapImage otherBitmapImage = (BitmapImage)source;
		ClonePrequel(otherBitmapImage);
		base.GetCurrentValueAsFrozenCore(source);
		ClonePostscript(otherBitmapImage);
	}

	static BitmapImage()
	{
		s_UriCachePolicy = null;
		s_UriSource = null;
		s_StreamSource = null;
		s_SourceRect = Int32Rect.Empty;
		s_CreateOptions = BitmapCreateOptions.None;
		s_CacheOption = BitmapCacheOption.Default;
		Type typeFromHandle = typeof(BitmapImage);
		UriCachePolicyProperty = Animatable.RegisterProperty("UriCachePolicy", typeof(RequestCachePolicy), typeFromHandle, null, UriCachePolicyPropertyChanged, null, isIndependentlyAnimated: false, CoerceUriCachePolicy);
		UriSourceProperty = Animatable.RegisterProperty("UriSource", typeof(Uri), typeFromHandle, null, UriSourcePropertyChanged, null, isIndependentlyAnimated: false, CoerceUriSource);
		StreamSourceProperty = Animatable.RegisterProperty("StreamSource", typeof(Stream), typeFromHandle, null, StreamSourcePropertyChanged, null, isIndependentlyAnimated: false, CoerceStreamSource);
		DecodePixelWidthProperty = Animatable.RegisterProperty("DecodePixelWidth", typeof(int), typeFromHandle, 0, DecodePixelWidthPropertyChanged, null, isIndependentlyAnimated: false, CoerceDecodePixelWidth);
		DecodePixelHeightProperty = Animatable.RegisterProperty("DecodePixelHeight", typeof(int), typeFromHandle, 0, DecodePixelHeightPropertyChanged, null, isIndependentlyAnimated: false, CoerceDecodePixelHeight);
		RotationProperty = Animatable.RegisterProperty("Rotation", typeof(Rotation), typeFromHandle, Rotation.Rotate0, RotationPropertyChanged, ValidateEnums.IsRotationValid, isIndependentlyAnimated: false, CoerceRotation);
		SourceRectProperty = Animatable.RegisterProperty("SourceRect", typeof(Int32Rect), typeFromHandle, Int32Rect.Empty, SourceRectPropertyChanged, null, isIndependentlyAnimated: false, CoerceSourceRect);
		CreateOptionsProperty = Animatable.RegisterProperty("CreateOptions", typeof(BitmapCreateOptions), typeFromHandle, BitmapCreateOptions.None, CreateOptionsPropertyChanged, null, isIndependentlyAnimated: false, CoerceCreateOptions);
		CacheOptionProperty = Animatable.RegisterProperty("CacheOption", typeof(BitmapCacheOption), typeFromHandle, BitmapCacheOption.Default, CacheOptionPropertyChanged, null, isIndependentlyAnimated: false, CoerceCacheOption);
	}
}
