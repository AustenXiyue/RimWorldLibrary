using System.Text;
using Mono.Security;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription" /> class defines the description of the content of a CMS/PKCS #7 message.</summary>
public sealed class Pkcs9DocumentDescription : Pkcs9AttributeObject
{
	internal const string oid = "1.3.6.1.4.1.311.88.2.2";

	internal const string friendlyName = null;

	private string _desc;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription.DocumentDescription" /> property retrieves the document description.</summary>
	/// <returns>A <see cref="T:System.String" /> object that contains the document description.</returns>
	public string DocumentDescription => _desc;

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription" /> class.</summary>
	public Pkcs9DocumentDescription()
	{
		((AsnEncodedData)this).Oid = new Oid("1.3.6.1.4.1.311.88.2.2", null);
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription.#ctor(System.String)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription" /> class by using the specified description of the content of a CMS/PKCS #7 message.</summary>
	/// <param name="documentDescription">An instance of the <see cref="T:System.String" />  class that specifies the description for the CMS/PKCS #7 message.</param>
	public Pkcs9DocumentDescription(string documentDescription)
	{
		if (documentDescription == null)
		{
			throw new ArgumentNullException("documentName");
		}
		((AsnEncodedData)this).Oid = new Oid("1.3.6.1.4.1.311.88.2.2", null);
		_desc = documentDescription;
		base.RawData = Encode();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription.#ctor(System.Byte[])" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription" /> class by using the specified array of byte values as the encoded description of the content of a CMS/PKCS #7 message.</summary>
	/// <param name="encodedDocumentDescription">An array of byte values that specifies the encoded description of the CMS/PKCS #7 message.</param>
	public Pkcs9DocumentDescription(byte[] encodedDocumentDescription)
	{
		if (encodedDocumentDescription == null)
		{
			throw new ArgumentNullException("encodedDocumentDescription");
		}
		((AsnEncodedData)this).Oid = new Oid("1.3.6.1.4.1.311.88.2.2", null);
		base.RawData = encodedDocumentDescription;
		Decode(encodedDocumentDescription);
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
			_desc = Encoding.Unicode.GetString(value, 0, num);
		}
	}

	internal byte[] Encode()
	{
		return new ASN1(4, Encoding.Unicode.GetBytes(_desc + "\0")).GetBytes();
	}
}
