using System.IO;
using System.IO.Packaging;
using MS.Internal;

namespace System.Windows.Documents;

internal sealed class XpsS0FixedPageSchema : XpsS0Schema
{
	private const string _printTicketRel = "http://schemas.microsoft.com/xps/2005/06/printticket";

	private const string _discardControlRel = "http://schemas.microsoft.com/xps/2005/06/discard-control";

	private const string _restrictedFontRel = "http://schemas.microsoft.com/xps/2005/06/restricted-font";

	private const string _thumbnailRel = "http://schemas.openxmlformats.org/package/2006/relationships/metadata/thumbnail";

	public XpsS0FixedPageSchema()
	{
		XpsSchema.RegisterSchema(this, new ContentType[3]
		{
			XpsS0Schema._fixedDocumentSequenceContentType,
			XpsS0Schema._fixedDocumentContentType,
			XpsS0Schema._fixedPageContentType
		});
		RegisterRequiredResourceMimeTypes(new ContentType[8]
		{
			XpsS0Schema._resourceDictionaryContentType,
			XpsS0Schema._fontContentType,
			XpsS0Schema._colorContextContentType,
			XpsS0Schema._obfuscatedContentType,
			XpsS0Schema._jpgContentType,
			XpsS0Schema._pngContentType,
			XpsS0Schema._tifContentType,
			XpsS0Schema._wmpContentType
		});
	}

	public override void ValidateRelationships(SecurityCriticalData<Package> package, Uri packageUri, Uri partUri, ContentType mimeType)
	{
		PackagePart part = package.Value.GetPart(partUri);
		PackageRelationshipCollection relationshipsByType = part.GetRelationshipsByType("http://schemas.microsoft.com/xps/2005/06/printticket");
		int num = 0;
		foreach (PackageRelationship item in relationshipsByType)
		{
			num++;
			if (num > 1)
			{
				throw new FileFormatException(SR.XpsValidatingLoaderMoreThanOnePrintTicketPart);
			}
			Uri partUri2 = PackUriHelper.ResolvePartUri(partUri, item.TargetUri);
			PackUriHelper.Create(packageUri, partUri2);
			PackagePart part2 = package.Value.GetPart(partUri2);
			if (!XpsS0Schema._printTicketContentType.AreTypeAndSubTypeEqual(new ContentType(part2.ContentType)))
			{
				throw new FileFormatException(SR.XpsValidatingLoaderPrintTicketHasIncorrectType);
			}
		}
		PackageRelationshipCollection relationshipsByType2 = part.GetRelationshipsByType("http://schemas.openxmlformats.org/package/2006/relationships/metadata/thumbnail");
		num = 0;
		foreach (PackageRelationship item2 in relationshipsByType2)
		{
			num++;
			if (num > 1)
			{
				throw new FileFormatException(SR.XpsValidatingLoaderMoreThanOneThumbnailPart);
			}
			Uri partUri3 = PackUriHelper.ResolvePartUri(partUri, item2.TargetUri);
			PackUriHelper.Create(packageUri, partUri3);
			PackagePart part3 = package.Value.GetPart(partUri3);
			if (!XpsS0Schema._jpgContentType.AreTypeAndSubTypeEqual(new ContentType(part3.ContentType)) && !XpsS0Schema._pngContentType.AreTypeAndSubTypeEqual(new ContentType(part3.ContentType)))
			{
				throw new FileFormatException(SR.XpsValidatingLoaderThumbnailHasIncorrectType);
			}
		}
		if (XpsS0Schema._fixedDocumentContentType.AreTypeAndSubTypeEqual(mimeType))
		{
			foreach (PackageRelationship item3 in part.GetRelationshipsByType("http://schemas.microsoft.com/xps/2005/06/restricted-font"))
			{
				Uri partUri4 = PackUriHelper.ResolvePartUri(partUri, item3.TargetUri);
				PackUriHelper.Create(packageUri, partUri4);
				PackagePart part4 = package.Value.GetPart(partUri4);
				if (!XpsS0Schema._fontContentType.AreTypeAndSubTypeEqual(new ContentType(part4.ContentType)) && !XpsS0Schema._obfuscatedContentType.AreTypeAndSubTypeEqual(new ContentType(part4.ContentType)))
				{
					throw new FileFormatException(SR.XpsValidatingLoaderRestrictedFontHasIncorrectType);
				}
			}
		}
		if (!XpsS0Schema._fixedDocumentSequenceContentType.AreTypeAndSubTypeEqual(mimeType))
		{
			return;
		}
		PackageRelationshipCollection relationshipsByType3 = package.Value.GetRelationshipsByType("http://schemas.microsoft.com/xps/2005/06/discard-control");
		num = 0;
		foreach (PackageRelationship item4 in relationshipsByType3)
		{
			num++;
			if (num > 1)
			{
				throw new FileFormatException(SR.XpsValidatingLoaderMoreThanOneDiscardControlInPackage);
			}
			Uri partUri5 = PackUriHelper.ResolvePartUri(partUri, item4.TargetUri);
			PackUriHelper.Create(packageUri, partUri5);
			PackagePart part5 = package.Value.GetPart(partUri5);
			if (!XpsS0Schema._discardControlContentType.AreTypeAndSubTypeEqual(new ContentType(part5.ContentType)))
			{
				throw new FileFormatException(SR.XpsValidatingLoaderDiscardControlHasIncorrectType);
			}
		}
		PackageRelationshipCollection relationshipsByType4 = package.Value.GetRelationshipsByType("http://schemas.openxmlformats.org/package/2006/relationships/metadata/thumbnail");
		num = 0;
		foreach (PackageRelationship item5 in relationshipsByType4)
		{
			num++;
			if (num > 1)
			{
				throw new FileFormatException(SR.XpsValidatingLoaderMoreThanOneThumbnailInPackage);
			}
			Uri partUri6 = PackUriHelper.ResolvePartUri(partUri, item5.TargetUri);
			PackUriHelper.Create(packageUri, partUri6);
			PackagePart part6 = package.Value.GetPart(partUri6);
			if (!XpsS0Schema._jpgContentType.AreTypeAndSubTypeEqual(new ContentType(part6.ContentType)) && !XpsS0Schema._pngContentType.AreTypeAndSubTypeEqual(new ContentType(part6.ContentType)))
			{
				throw new FileFormatException(SR.XpsValidatingLoaderThumbnailHasIncorrectType);
			}
		}
	}
}
