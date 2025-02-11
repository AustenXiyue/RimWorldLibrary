namespace System.Security.Cryptography.Xml;

/// <summary>Represents the &lt;X509IssuerSerial&gt; element of an XML digital signature.</summary>
public struct X509IssuerSerial
{
	private string _issuerName;

	private string _serialNumber;

	/// <summary>Gets or sets an X.509 certificate issuer's distinguished name.</summary>
	/// <returns>An X.509 certificate issuer's distinguished name.</returns>
	public string IssuerName
	{
		get
		{
			return _issuerName;
		}
		set
		{
			_issuerName = value;
		}
	}

	/// <summary>Gets or sets an X.509 certificate issuer's serial number.</summary>
	/// <returns>An X.509 certificate issuer's serial number.</returns>
	public string SerialNumber
	{
		get
		{
			return _serialNumber;
		}
		set
		{
			_serialNumber = value;
		}
	}

	internal X509IssuerSerial(string issuer, string serial)
	{
		_issuerName = issuer;
		_serialNumber = serial;
	}
}
