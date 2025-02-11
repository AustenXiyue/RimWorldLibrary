using System.Text;
using Mono.Security;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentName" /> class defines the name of a CMS/PKCS #7 message.</summary>
public sealed class Pkcs9DocumentName : Pkcs9AttributeObject
{
	internal const string oid = "1.3.6.1.4.1.311.88.2.1";

	internal const string friendlyName = null;

	private string _name;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs9DocumentName.DocumentName" /> property retrieves the document name.</summary>
	/// <returns>A <see cref="T:System.String" /> object that contains the document name.</returns>
	public string DocumentName => _name;

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9DocumentName.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentName" /> class.</summary>
	public Pkcs9DocumentName()
	{
		((AsnEncodedData)this).Oid = new Oid("1.3.6.1.4.1.311.88.2.1", null);
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9DocumentName.#ctor(System.String)" /> constructor creates an instance of the  <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentName" /> class by using the specified name for the CMS/PKCS #7 message.</summary>
	/// <param name="documentName">A  <see cref="T:System.String" />   object that specifies the name for the CMS/PKCS #7 message.</param>
	public Pkcs9DocumentName(string documentName)
	{
		if (documentName == null)
		{
			throw new ArgumentNullException("documentName");
		}
		((AsnEncodedData)this).Oid = new Oid("1.3.6.1.4.1.311.88.2.1", null);
		_name = documentName;
		base.RawData = Encode();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9DocumentName.#ctor(System.Byte[])" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentName" /> class by using the specified array of byte values as the encoded name of the content of a CMS/PKCS #7 message.</summary>
	/// <param name="encodedDocumentName">An array of byte values that specifies the encoded name of the CMS/PKCS #7 message.</param>
	public Pkcs9DocumentName(byte[] encodedDocumentName)
	{
		if (encodedDocumentName == null)
		{
			throw new ArgumentNullException("encodedDocumentName");
		}
		((AsnEncodedData)this).Oid = new Oid("1.3.6.1.4.1.311.88.2.1", null);
		base.RawData = encodedDocumentName;
		Decode(encodedDocumentName);
	}

	/// <summary>Copies information from an <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object.</summary>
	/// <param name="asnEncodedData">The <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object from which to copy information.</param>
	public override void CopyFrom(AsnEncodedData asnEncodedData)
	{
		base.CopyFrom(asnEncodedData);
		Decode(base.RawData);
	}

	internal void Decode(byte[] attribute)
	{
		if (attribute[0] == 4)
		{
			byte[] value = new ASN1(attribute).Value;
			int num = value.Length;
			if (value[num - 2] == 0)
			{
				num -= 2;
			}
			_name = Encoding.Unicode.GetString(value, 0, num);
		}
	}

	internal byte[] Encode()
	{
		return new ASN1(4, Encoding.Unicode.GetBytes(_name + "\0")).GetBytes();
	}
}
