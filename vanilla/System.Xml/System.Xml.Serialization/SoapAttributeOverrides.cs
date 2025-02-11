using System.Collections;

namespace System.Xml.Serialization;

/// <summary>Allows you to override attributes applied to properties, fields, and classes when you use an <see cref="T:System.Xml.Serialization.XmlSerializer" /> to serialize or deserialize an object as encoded SOAP.</summary>
public class SoapAttributeOverrides
{
	private Hashtable types = new Hashtable();

	/// <summary>Gets the object associated with the specified (base class) type.</summary>
	/// <returns>A <see cref="T:System.Xml.Serialization.SoapAttributes" /> that represents the collection of overriding attributes.</returns>
	/// <param name="type">The base class <see cref="T:System.Type" /> that is associated with the collection of attributes you want to retrieve. </param>
	public SoapAttributes this[Type type] => this[type, string.Empty];

	/// <summary>Gets the object associated with the specified (base class) type. The <paramref name="member" /> parameter specifies the base class member that is overridden.</summary>
	/// <returns>A <see cref="T:System.Xml.Serialization.SoapAttributes" /> that represents the collection of overriding attributes.</returns>
	/// <param name="type">The base class <see cref="T:System.Type" /> that is associated with the collection of attributes you want to override. </param>
	/// <param name="member">The name of the overridden member that specifies the <see cref="T:System.Xml.Serialization.SoapAttributes" /> to return. </param>
	public SoapAttributes this[Type type, string member]
	{
		get
		{
			Hashtable hashtable = (Hashtable)types[type];
			if (hashtable == null)
			{
				return null;
			}
			return (SoapAttributes)hashtable[member];
		}
	}

	/// <summary>Adds a <see cref="T:System.Xml.Serialization.SoapAttributes" /> to a collection of <see cref="T:System.Xml.Serialization.SoapAttributes" /> objects. The <paramref name="type" /> parameter specifies an object to be overridden by the <see cref="T:System.Xml.Serialization.SoapAttributes" />.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> of the object that is overridden. </param>
	/// <param name="attributes">A <see cref="T:System.Xml.Serialization.SoapAttributes" /> that represents the overriding attributes. </param>
	public void Add(Type type, SoapAttributes attributes)
	{
		Add(type, string.Empty, attributes);
	}

	/// <summary>Adds a <see cref="T:System.Xml.Serialization.SoapAttributes" /> to the collection of <see cref="T:System.Xml.Serialization.SoapAttributes" /> objects contained by the <see cref="T:System.Xml.Serialization.SoapAttributeOverrides" />. The <paramref name="type" /> parameter specifies the object to be overridden by the <see cref="T:System.Xml.Serialization.SoapAttributes" />. The <paramref name="member" /> parameter specifies the name of a member that is overridden.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> of the object to override. </param>
	/// <param name="member">The name of the member to override. </param>
	/// <param name="attributes">A <see cref="T:System.Xml.Serialization.SoapAttributes" /> that represents the overriding attributes. </param>
	public void Add(Type type, string member, SoapAttributes attributes)
	{
		Hashtable hashtable = (Hashtable)types[type];
		if (hashtable == null)
		{
			hashtable = new Hashtable();
			types.Add(type, hashtable);
		}
		else if (hashtable[member] != null)
		{
			throw new InvalidOperationException(Res.GetString("{0}. {1} already has attributes.", type.FullName, member));
		}
		hashtable.Add(member, attributes);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.SoapAttributeOverrides" /> class. </summary>
	public SoapAttributeOverrides()
	{
	}
}
