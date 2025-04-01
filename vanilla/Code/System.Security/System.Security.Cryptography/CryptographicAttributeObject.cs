namespace System.Security.Cryptography;

/// <summary>Contains a type and a collection of values associated with that type.</summary>
public sealed class CryptographicAttributeObject
{
	private Oid _oid;

	private AsnEncodedDataCollection _list;

	/// <summary>Gets the <see cref="T:System.Security.Cryptography.Oid" /> object that specifies the object identifier for the attribute.</summary>
	/// <returns>The object identifier for the attribute.</returns>
	public Oid Oid => _oid;

	/// <summary>Gets the <see cref="T:System.Security.Cryptography.AsnEncodedDataCollection" /> collection that contains the set of values that are associated with the attribute.</summary>
	/// <returns>The set of values that is associated with the attribute.</returns>
	public AsnEncodedDataCollection Values => _list;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> class using an attribute represented by the specified <see cref="T:System.Security.Cryptography.Oid" /> object.</summary>
	/// <param name="oid">The attribute to store in this <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object.</param>
	public CryptographicAttributeObject(Oid oid)
	{
		if (oid == null)
		{
			throw new ArgumentNullException("oid");
		}
		_oid = new Oid(oid);
		_list = new AsnEncodedDataCollection();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> class using an attribute represented by the specified <see cref="T:System.Security.Cryptography.Oid" /> object and the set of values associated with that attribute represented by the specified <see cref="T:System.Security.Cryptography.AsnEncodedDataCollection" /> collection.</summary>
	/// <param name="oid">The attribute to store in this <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object.</param>
	/// <param name="values">The set of values associated with the attribute represented by the <paramref name="oid" /> parameter.</param>
	/// <exception cref="T:System.InvalidOperationException">The collection contains duplicate items. </exception>
	public CryptographicAttributeObject(Oid oid, AsnEncodedDataCollection values)
	{
		if (oid == null)
		{
			throw new ArgumentNullException("oid");
		}
		_oid = new Oid(oid);
		if (values == null)
		{
			_list = new AsnEncodedDataCollection();
		}
		else
		{
			_list = values;
		}
	}
}
