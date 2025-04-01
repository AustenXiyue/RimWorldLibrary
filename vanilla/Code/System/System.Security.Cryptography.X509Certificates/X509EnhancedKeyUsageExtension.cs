using System.Text;
using Mono.Security;

namespace System.Security.Cryptography.X509Certificates;

/// <summary>Defines the collection of object identifiers (OIDs) that indicates the applications that use the key. This class cannot be inherited.</summary>
public sealed class X509EnhancedKeyUsageExtension : X509Extension
{
	internal const string oid = "2.5.29.37";

	internal const string friendlyName = "Enhanced Key Usage";

	private OidCollection _enhKeyUsage;

	private AsnDecodeStatus _status;

	/// <summary>Gets the collection of object identifiers (OIDs) that indicate the applications that use the key.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.OidCollection" /> object indicating the applications that use the key.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public OidCollection EnhancedKeyUsages
	{
		get
		{
			AsnDecodeStatus status = _status;
			if (status == AsnDecodeStatus.Ok || status == AsnDecodeStatus.InformationNotAvailable)
			{
				OidCollection oidCollection = new OidCollection();
				if (_enhKeyUsage != null)
				{
					OidEnumerator enumerator = _enhKeyUsage.GetEnumerator();
					while (enumerator.MoveNext())
					{
						Oid current = enumerator.Current;
						oidCollection.Add(current);
					}
				}
				return oidCollection;
			}
			throw new CryptographicException("Badly encoded extension.");
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension" /> class.</summary>
	public X509EnhancedKeyUsageExtension()
	{
		_oid = new Oid("2.5.29.37", "Enhanced Key Usage");
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension" /> class using an <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object and a value that identifies whether the extension is critical.</summary>
	/// <param name="encodedEnhancedKeyUsages">The encoded data to use to create the extension.</param>
	/// <param name="critical">true if the extension is critical; otherwise, false.</param>
	public X509EnhancedKeyUsageExtension(AsnEncodedData encodedEnhancedKeyUsages, bool critical)
	{
		_oid = new Oid("2.5.29.37", "Enhanced Key Usage");
		_raw = encodedEnhancedKeyUsages.RawData;
		base.Critical = critical;
		_status = Decode(base.RawData);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension" /> class using an <see cref="T:System.Security.Cryptography.OidCollection" /> and a value that identifies whether the extension is critical. </summary>
	/// <param name="enhancedKeyUsages">An <see cref="T:System.Security.Cryptography.OidCollection" /> collection. </param>
	/// <param name="critical">true if the extension is critical; otherwise, false.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The specified <see cref="T:System.Security.Cryptography.OidCollection" />  contains one or more corrupt values.</exception>
	public X509EnhancedKeyUsageExtension(OidCollection enhancedKeyUsages, bool critical)
	{
		if (enhancedKeyUsages == null)
		{
			throw new ArgumentNullException("enhancedKeyUsages");
		}
		_oid = new Oid("2.5.29.37", "Enhanced Key Usage");
		base.Critical = critical;
		_enhKeyUsage = new OidCollection();
		OidEnumerator enumerator = enhancedKeyUsages.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Oid current = enumerator.Current;
			_enhKeyUsage.Add(current);
		}
		base.RawData = Encode();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension" /> class using an <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object.</summary>
	/// <param name="asnEncodedData">The encoded data to use to create the extension.</param>
	public override void CopyFrom(AsnEncodedData asnEncodedData)
	{
		if (asnEncodedData == null)
		{
			throw new ArgumentNullException("encodedData");
		}
		if (!(asnEncodedData is X509Extension x509Extension))
		{
			throw new ArgumentException(global::Locale.GetText("Wrong type."), "asnEncodedData");
		}
		if (x509Extension._oid == null)
		{
			_oid = new Oid("2.5.29.37", "Enhanced Key Usage");
		}
		else
		{
			_oid = new Oid(x509Extension._oid);
		}
		base.RawData = x509Extension.RawData;
		base.Critical = x509Extension.Critical;
		_status = Decode(base.RawData);
	}

	internal AsnDecodeStatus Decode(byte[] extension)
	{
		if (extension == null || extension.Length == 0)
		{
			return AsnDecodeStatus.BadAsn;
		}
		if (extension[0] != 48)
		{
			return AsnDecodeStatus.BadTag;
		}
		if (_enhKeyUsage == null)
		{
			_enhKeyUsage = new OidCollection();
		}
		try
		{
			ASN1 aSN = new ASN1(extension);
			if (aSN.Tag != 48)
			{
				throw new CryptographicException(global::Locale.GetText("Invalid ASN.1 Tag"));
			}
			for (int i = 0; i < aSN.Count; i++)
			{
				_enhKeyUsage.Add(new Oid(ASN1Convert.ToOid(aSN[i])));
			}
		}
		catch
		{
			return AsnDecodeStatus.BadAsn;
		}
		return AsnDecodeStatus.Ok;
	}

	internal byte[] Encode()
	{
		ASN1 aSN = new ASN1(48);
		OidEnumerator enumerator = _enhKeyUsage.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Oid current = enumerator.Current;
			aSN.Add(ASN1Convert.FromOid(current.Value));
		}
		return aSN.GetBytes();
	}

	internal override string ToString(bool multiLine)
	{
		switch (_status)
		{
		case AsnDecodeStatus.BadAsn:
			return string.Empty;
		case AsnDecodeStatus.BadTag:
		case AsnDecodeStatus.BadLength:
			return FormatUnkownData(_raw);
		case AsnDecodeStatus.InformationNotAvailable:
			return "Information Not Available";
		default:
		{
			if (_oid.Value != "2.5.29.37")
			{
				return $"Unknown Key Usage ({_oid.Value})";
			}
			if (_enhKeyUsage.Count == 0)
			{
				return "Information Not Available";
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < _enhKeyUsage.Count; i++)
			{
				Oid oid = _enhKeyUsage[i];
				string value = oid.Value;
				if (value == "1.3.6.1.5.5.7.3.1")
				{
					stringBuilder.Append("Server Authentication (");
				}
				else
				{
					stringBuilder.Append("Unknown Key Usage (");
				}
				stringBuilder.Append(oid.Value);
				stringBuilder.Append(")");
				if (multiLine)
				{
					stringBuilder.Append(Environment.NewLine);
				}
				else if (i != _enhKeyUsage.Count - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			return stringBuilder.ToString();
		}
		}
	}
}
