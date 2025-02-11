using System.Collections.ObjectModel;

namespace System.Windows.Media.Imaging;

internal sealed class BitmapFrameEncode : BitmapFrame
{
	private BitmapSource _source;

	public override Uri BaseUri
	{
		get
		{
			ReadPreamble();
			return null;
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
			return null;
		}
	}

	public override ReadOnlyCollection<ColorContext> ColorContexts
	{
		get
		{
			ReadPreamble();
			return _readOnlycolorContexts;
		}
	}

	internal override BitmapMetadata InternalMetadata
	{
		get
		{
			CheckIfSiteOfOrigin();
			return _metadata;
		}
		set
		{
			CheckIfSiteOfOrigin();
			_metadata = value;
		}
	}

	internal BitmapFrameEncode(BitmapSource source, BitmapSource thumbnail, BitmapMetadata metadata, ReadOnlyCollection<ColorContext> colorContexts)
		: base(useVirtuals: true)
	{
		_bitmapInit.BeginInit();
		_source = source;
		base.WicSourceHandle = _source.WicSourceHandle;
		base.IsSourceCached = _source.IsSourceCached;
		_isColorCorrected = _source._isColorCorrected;
		_thumbnail = thumbnail;
		_readOnlycolorContexts = colorContexts;
		InternalMetadata = metadata;
		_syncObject = source.SyncObject;
		_bitmapInit.EndInit();
		FinalizeCreation();
	}

	private BitmapFrameEncode()
		: base(useVirtuals: true)
	{
	}

	public override InPlaceBitmapMetadataWriter CreateInPlaceBitmapMetadataWriter()
	{
		ReadPreamble();
		return null;
	}

	protected override Freezable CreateInstanceCore()
	{
		return new BitmapFrameEncode();
	}

	private void CopyCommon(BitmapFrameEncode sourceBitmapFrameEncode)
	{
		_bitmapInit.BeginInit();
		_source = sourceBitmapFrameEncode._source;
		_thumbnail = sourceBitmapFrameEncode._thumbnail;
		_readOnlycolorContexts = sourceBitmapFrameEncode.ColorContexts;
		if (sourceBitmapFrameEncode.InternalMetadata != null)
		{
			InternalMetadata = sourceBitmapFrameEncode.InternalMetadata.Clone();
		}
		_bitmapInit.EndInit();
	}

	protected override void CloneCore(Freezable sourceFreezable)
	{
		BitmapFrameEncode sourceBitmapFrameEncode = (BitmapFrameEncode)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(sourceBitmapFrameEncode);
	}

	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		BitmapFrameEncode sourceBitmapFrameEncode = (BitmapFrameEncode)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(sourceBitmapFrameEncode);
	}

	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		BitmapFrameEncode sourceBitmapFrameEncode = (BitmapFrameEncode)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmapFrameEncode);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		BitmapFrameEncode sourceBitmapFrameEncode = (BitmapFrameEncode)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmapFrameEncode);
	}

	internal override void FinalizeCreation()
	{
		base.CreationCompleted = true;
		UpdateCachedSettings();
	}
}
