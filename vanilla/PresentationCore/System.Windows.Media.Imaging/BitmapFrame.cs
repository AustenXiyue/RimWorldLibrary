using System.Collections.ObjectModel;
using System.IO;
using System.Net.Cache;
using System.Windows.Markup;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Represents image data returned by a decoder and accepted by encoders.</summary>
public abstract class BitmapFrame : BitmapSource, IUriContext
{
	internal BitmapSource _thumbnail;

	internal BitmapMetadata _metadata;

	internal ReadOnlyCollection<ColorContext> _readOnlycolorContexts;

	/// <summary>When overridden in a derived class, gets or sets a value that represents the base <see cref="T:System.Uri" /> of the current context.</summary>
	/// <returns>The <see cref="T:System.Uri" /> of the current context.</returns>
	public abstract Uri BaseUri { get; set; }

	/// <summary>When overridden in a derived class, gets the thumbnail image associated with this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that represents a thumbnail of the current <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</returns>
	public abstract BitmapSource Thumbnail { get; }

	/// <summary>When overridden in a derived class, gets the decoder associated with this instance of <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Imaging.BitmapDecoder" /> that is associated with this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</returns>
	public abstract BitmapDecoder Decoder { get; }

	/// <summary>When overridden in a derived class, gets a collection of <see cref="T:System.Windows.Media.ColorContext" /> objects that are associated with this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</summary>
	/// <returns>A read-only collection of <see cref="T:System.Windows.Media.ColorContext" /> objects that are associated with this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</returns>
	public abstract ReadOnlyCollection<ColorContext> ColorContexts { get; }

	internal virtual BitmapMetadata InternalMetadata
	{
		get
		{
			return null;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> class.</summary>
	protected BitmapFrame()
	{
	}

	internal BitmapFrame(bool useVirtuals)
		: base(useVirtuals)
	{
	}

	internal static BitmapFrame CreateFromUriOrStream(Uri baseUri, Uri uri, Stream stream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, RequestCachePolicy uriCachePolicy)
	{
		if (uri != null)
		{
			BitmapDecoder bitmapDecoder = BitmapDecoder.CreateFromUriOrStream(baseUri, uri, null, createOptions, cacheOption, uriCachePolicy, insertInDecoderCache: true);
			if (bitmapDecoder.Frames.Count == 0)
			{
				throw new ArgumentException(SR.Image_NoDecodeFrames, "uri");
			}
			return bitmapDecoder.Frames[0];
		}
		BitmapDecoder bitmapDecoder2 = BitmapDecoder.Create(stream, createOptions, cacheOption);
		if (bitmapDecoder2.Frames.Count == 0)
		{
			throw new ArgumentException(SR.Image_NoDecodeFrames, "stream");
		}
		return bitmapDecoder2.Frames[0];
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.Uri" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.Uri" />.</returns>
	/// <param name="bitmapUri">The <see cref="T:System.Uri" /> that identifies the source of the <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	public static BitmapFrame Create(Uri bitmapUri)
	{
		return Create(bitmapUri, null);
	}

	/// <summary>Creates a <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.Uri" /> with the specified <see cref="T:System.Net.Cache.RequestCachePolicy" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.Uri" /> with the specified <see cref="T:System.Net.Cache.RequestCachePolicy" />.</returns>
	/// <param name="bitmapUri">The location of the bitmap from which the <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> is created.</param>
	/// <param name="uriCachePolicy">The caching requirements for this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	public static BitmapFrame Create(Uri bitmapUri, RequestCachePolicy uriCachePolicy)
	{
		if (bitmapUri == null)
		{
			throw new ArgumentNullException("bitmapUri");
		}
		return CreateFromUriOrStream(null, bitmapUri, null, BitmapCreateOptions.None, BitmapCacheOption.Default, uriCachePolicy);
	}

	/// <summary>Creates a <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.Uri" /> with the specified <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" /> and <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.Uri" /> with the specified <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" />, and <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" />.</returns>
	/// <param name="bitmapUri">The location of the bitmap from which the <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> is created.</param>
	/// <param name="createOptions">The options that are used to create this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	/// <param name="cacheOption">The cache option that is used to create this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	public static BitmapFrame Create(Uri bitmapUri, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
	{
		return Create(bitmapUri, createOptions, cacheOption, null);
	}

	/// <summary>Creates a <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.Uri" /> with the specified <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" />, <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" />, and <see cref="T:System.Net.Cache.RequestCachePolicy" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.Uri" /> with the specified <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" />, <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" />, and <see cref="T:System.Net.Cache.RequestCachePolicy" />.</returns>
	/// <param name="bitmapUri">The location of the bitmap from which the <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> is created.</param>
	/// <param name="createOptions">The options that are used to create this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	/// <param name="cacheOption">The cache option that is used to create this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	/// <param name="uriCachePolicy">The caching requirements for this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	public static BitmapFrame Create(Uri bitmapUri, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, RequestCachePolicy uriCachePolicy)
	{
		if (bitmapUri == null)
		{
			throw new ArgumentNullException("bitmapUri");
		}
		return CreateFromUriOrStream(null, bitmapUri, null, createOptions, cacheOption, uriCachePolicy);
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.IO.Stream" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.IO.Stream" />.</returns>
	/// <param name="bitmapStream">The <see cref="T:System.IO.Stream" /> that is used to construct the <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	public static BitmapFrame Create(Stream bitmapStream)
	{
		if (bitmapStream == null)
		{
			throw new ArgumentNullException("bitmapStream");
		}
		return CreateFromUriOrStream(null, null, bitmapStream, BitmapCreateOptions.None, BitmapCacheOption.Default, null);
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.IO.Stream" /> with the specified <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" /> and <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.IO.Stream" /> with the specified <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" /> and <see cref="T:System.Windows.Media.Imaging.BitmapCacheOption" />.</returns>
	/// <param name="bitmapStream">The stream from which this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> is constructed.</param>
	/// <param name="createOptions">The options that are used to create this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	/// <param name="cacheOption">The cache option that is used to create this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	public static BitmapFrame Create(Stream bitmapStream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
	{
		if (bitmapStream == null)
		{
			throw new ArgumentNullException("bitmapStream");
		}
		return CreateFromUriOrStream(null, null, bitmapStream, createOptions, cacheOption, null);
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.Windows.Media.Imaging.BitmapSource" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.Windows.Media.Imaging.BitmapSource" />.</returns>
	/// <param name="source">The <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that is used to construct this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	public static BitmapFrame Create(BitmapSource source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		BitmapMetadata bitmapMetadata = null;
		try
		{
			bitmapMetadata = source.Metadata as BitmapMetadata;
		}
		catch (NotSupportedException)
		{
		}
		if (bitmapMetadata != null)
		{
			bitmapMetadata = bitmapMetadata.Clone();
		}
		return new BitmapFrameEncode(source, null, bitmapMetadata, null);
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> with the specified thumbnail.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> with the specified thumbnail.</returns>
	/// <param name="source">The source from which the <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> is constructed.</param>
	/// <param name="thumbnail">A thumbnail image of the resulting <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	public static BitmapFrame Create(BitmapSource source, BitmapSource thumbnail)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		BitmapMetadata bitmapMetadata = null;
		try
		{
			bitmapMetadata = source.Metadata as BitmapMetadata;
		}
		catch (NotSupportedException)
		{
		}
		if (bitmapMetadata != null)
		{
			bitmapMetadata = bitmapMetadata.Clone();
		}
		return Create(source, thumbnail, bitmapMetadata, null);
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> with the specified thumbnail, <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" />, and <see cref="T:System.Windows.Media.ColorContext" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapFrame" /> from a given <see cref="T:System.Windows.Media.Imaging.Bitmapsource" /> with the specified thumbnail, <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" />, and <see cref="T:System.Windows.Media.ColorContext" />.</returns>
	/// <param name="source">The <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that is used to construct this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	/// <param name="thumbnail">A thumbnail image of the resulting <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	/// <param name="metadata">The metadata to associate with this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	/// <param name="colorContexts">The <see cref="T:System.Windows.Media.ColorContext" /> objects that are associated with this <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</param>
	public static BitmapFrame Create(BitmapSource source, BitmapSource thumbnail, BitmapMetadata metadata, ReadOnlyCollection<ColorContext> colorContexts)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return new BitmapFrameEncode(source, thumbnail, metadata, colorContexts);
	}

	/// <summary>When overridden in a derived class, creates an instance of <see cref="T:System.Windows.Media.Imaging.InPlaceBitmapMetadataWriter" />, which can be used to associate metadata with a <see cref="T:System.Windows.Media.Imaging.BitmapFrame" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Media.Imaging.InPlaceBitmapMetadataWriter" />.</returns>
	public abstract InPlaceBitmapMetadataWriter CreateInPlaceBitmapMetadataWriter();
}
