using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Enables in-place updates to existing blocks of <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" />. </summary>
public sealed class InPlaceBitmapMetadataWriter : BitmapMetadata
{
	private SafeMILHandle _fmeHandle;

	private InPlaceBitmapMetadataWriter()
	{
	}

	internal InPlaceBitmapMetadataWriter(SafeMILHandle fmeHandle, SafeMILHandle metadataHandle, object syncObject)
		: base(metadataHandle, readOnly: false, fixedSize: false, syncObject)
	{
		_fmeHandle = fmeHandle;
	}

	internal static InPlaceBitmapMetadataWriter CreateFromFrameDecode(BitmapSourceSafeMILHandle frameHandle, object syncObject)
	{
		Invariant.Assert(frameHandle != null);
		SafeMILHandle ppIBitmap = null;
		SafeMILHandle ppIQueryWriter = null;
		using (FactoryMaker factoryMaker = new FactoryMaker())
		{
			lock (syncObject)
			{
				HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateFastMetadataEncoderFromFrameDecode(factoryMaker.ImagingFactoryPtr, frameHandle, out ppIBitmap));
			}
		}
		HRESULT.Check(UnsafeNativeMethods.WICFastMetadataEncoder.GetMetadataQueryWriter(ppIBitmap, out ppIQueryWriter));
		return new InPlaceBitmapMetadataWriter(ppIBitmap, ppIQueryWriter, syncObject);
	}

	internal static InPlaceBitmapMetadataWriter CreateFromDecoder(SafeMILHandle decoderHandle, object syncObject)
	{
		Invariant.Assert(decoderHandle != null);
		SafeMILHandle ppIFME = null;
		SafeMILHandle ppIQueryWriter = null;
		using (FactoryMaker factoryMaker = new FactoryMaker())
		{
			lock (syncObject)
			{
				HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateFastMetadataEncoderFromDecoder(factoryMaker.ImagingFactoryPtr, decoderHandle, out ppIFME));
			}
		}
		HRESULT.Check(UnsafeNativeMethods.WICFastMetadataEncoder.GetMetadataQueryWriter(ppIFME, out ppIQueryWriter));
		return new InPlaceBitmapMetadataWriter(ppIFME, ppIQueryWriter, syncObject);
	}

	/// <summary>Gets a value that indicates whether image metadata can be saved successfully.</summary>
	/// <returns>true if bitmap metadata can be written successfully; otherwise, false.</returns>
	public bool TrySave()
	{
		Invariant.Assert(_fmeHandle != null);
		int hr;
		lock (base.SyncObject)
		{
			hr = UnsafeNativeMethods.WICFastMetadataEncoder.Commit(_fmeHandle);
		}
		return HRESULT.Succeeded(hr);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Imaging.InPlaceBitmapMetadataWriter" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new InPlaceBitmapMetadataWriter Clone()
	{
		return (InPlaceBitmapMetadataWriter)base.Clone();
	}

	protected override Freezable CreateInstanceCore()
	{
		throw new InvalidOperationException(SR.Image_InplaceMetadataNoCopy);
	}

	protected override void CloneCore(Freezable sourceFreezable)
	{
		throw new InvalidOperationException(SR.Image_InplaceMetadataNoCopy);
	}

	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		throw new InvalidOperationException(SR.Image_InplaceMetadataNoCopy);
	}

	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		throw new InvalidOperationException(SR.Image_InplaceMetadataNoCopy);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		throw new InvalidOperationException(SR.Image_InplaceMetadataNoCopy);
	}
}
