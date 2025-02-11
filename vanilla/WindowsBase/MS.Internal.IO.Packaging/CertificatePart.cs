using System;
using System.IO;
using System.IO.Packaging;
using System.Security.Cryptography.X509Certificates;
using MS.Internal.IO.Packaging.Extensions;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

internal class CertificatePart
{
	private PackagePart _part;

	private X509Certificate2 _certificate;

	private static readonly ContentType _certificatePartContentType = new ContentType("application/vnd.openxmlformats-package.digital-signature-certificate");

	private static readonly string _certificatePartNamePrefix = "/package/services/digital-signature/certificate/";

	private static readonly string _certificatePartNameExtension = ".cer";

	private static readonly string _certificatePartRelationshipType = "http://schemas.openxmlformats.org/package/2006/relationships/digital-signature/certificate";

	private static long _maximumCertificateStreamLength = 262144L;

	internal static string RelationshipType => _certificatePartRelationshipType;

	internal static string PartNamePrefix => _certificatePartNamePrefix;

	internal static string PartNameExtension => _certificatePartNameExtension;

	internal static ContentType ContentType => _certificatePartContentType;

	internal Uri Uri => _part.Uri;

	internal X509Certificate2 GetCertificate()
	{
		if (_certificate == null)
		{
			using Stream stream = _part.GetStream();
			if (stream.Length > 0)
			{
				if (stream.Length > _maximumCertificateStreamLength)
				{
					throw new FileFormatException(SR.CorruptedData);
				}
				byte[] array = new byte[stream.Length];
				PackagingUtilities.ReliableRead(stream, array, 0, (int)stream.Length);
				_certificate = new X509Certificate2(array);
			}
		}
		return _certificate;
	}

	internal void SetCertificate(X509Certificate2 certificate)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		_certificate = certificate;
		byte[] rawCertData = _certificate.GetRawCertData();
		using Stream stream = _part.GetSeekableStream(FileMode.Create, FileAccess.Write);
		stream.Write(rawCertData, 0, rawCertData.Length);
	}

	internal CertificatePart(Package container, Uri partName)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		if (partName == null)
		{
			throw new ArgumentNullException("partName");
		}
		partName = PackUriHelper.ValidatePartUri(partName);
		if (container.PartExists(partName))
		{
			_part = container.GetPart(partName);
			if (!_part.ValidatedContentType().AreTypeAndSubTypeEqual(_certificatePartContentType))
			{
				throw new FileFormatException(SR.CertificatePartContentTypeMismatch);
			}
		}
		else
		{
			_part = container.CreatePart(partName, _certificatePartContentType.ToString());
		}
	}
}
