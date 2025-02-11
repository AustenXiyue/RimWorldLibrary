using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Represents a collection of attributes.</summary>
[ComVisible(true)]
[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
public class AttributeCollection : ICollection, IEnumerable
{
	private struct AttributeEntry
	{
		public Type type;

		public int index;
	}

	/// <summary>Specifies an empty collection that you can use, rather than creating a new one. This field is read-only.</summary>
	public static readonly AttributeCollection Empty = new AttributeCollection((Attribute[])null);

	private static Hashtable _defaultAttributes;

	private Attribute[] _attributes;

	private static object internalSyncObject = new object();

	private const int FOUND_TYPES_LIMIT = 5;

	private AttributeEntry[] _foundAttributeTypes;

	private int _index;

	/// <summary>Gets the attribute collection.</summary>
	/// <returns>The attribute collection.</returns>
	protected virtual Attribute[] Attributes => _attributes;

	/// <summary>Gets the number of attributes.</summary>
	/// <returns>The number of attributes.</returns>
	public int Count => Attributes.Length;

	/// <summary>Gets the attribute with the specified index number.</summary>
	/// <returns>The <see cref="T:System.Attribute" /> with the specified index number.</returns>
	/// <param name="index">The zero-based index of <see cref="T:System.ComponentModel.AttributeCollection" />. </param>
	public virtual Attribute this[int index] => Attributes[index];

	/// <summary>Gets the attribute with the specified type.</summary>
	/// <returns>The <see cref="T:System.Attribute" /> with the specified type or, if the attribute does not exist, the default value for the attribute type.</returns>
	/// <param name="attributeType">The <see cref="T:System.Type" /> of the <see cref="T:System.Attribute" /> to get from the collection. </param>
	public virtual Attribute this[Type attributeType]
	{
		get
		{
			lock (internalSyncObject)
			{
				if (_foundAttributeTypes == null)
				{
					_foundAttributeTypes = new AttributeEntry[5];
				}
				int i;
				for (i = 0; i < 5; i++)
				{
					if (_foundAttributeTypes[i].type == attributeType)
					{
						int index = _foundAttributeTypes[i].index;
						if (index != -1)
						{
							return Attributes[index];
						}
						return GetDefaultAttribute(attributeType);
					}
					if (_foundAttributeTypes[i].type == null)
					{
						break;
					}
				}
				i = _index++;
				if (_index >= 5)
				{
					_index = 0;
				}
				_foundAttributeTypes[i].type = attributeType;
				int num = Attributes.Length;
				for (int j = 0; j < num; j++)
				{
					Attribute attribute = Attributes[j];
					if (attribute.GetType() == attributeType)
					{
						_foundAttributeTypes[i].index = j;
						return attribute;
					}
				}
				for (int k = 0; k < num; k++)
				{
					Attribute attribute2 = Attributes[k];
					Type type = attribute2.GetType();
					if (attributeType.IsAssignableFrom(type))
					{
						_foundAttributeTypes[i].index = k;
						return attribute2;
					}
				}
				_foundAttributeTypes[i].index = -1;
				return GetDefaultAttribute(attributeType);
			}
		}
	}

	/// <summary>Gets the number of elements contained in the collection.</summary>
	/// <returns>The number of elements contained in the collection.</returns>
	int ICollection.Count => Count;

	/// <summary>Gets a value indicating whether access to the collection is synchronized (thread-safe).</summary>
	/// <returns>true if access to the collection is synchronized (thread-safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
	/// <returns>An object that can be used to synchronize access to the collection.</returns>
	object ICollection.SyncRoot => null;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AttributeCollection" /> class.</summary>
	/// <param name="attributes">An array of type <see cref="T:System.Attribute" /> that provides the attributes for this collection. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="attributes" /> is null.</exception>
	public AttributeCollection(params Attribute[] attributes)
	{
		if (attributes == null)
		{
			attributes = new Attribute[0];
		}
		_attributes = attributes;
		for (int i = 0; i < attributes.Length; i++)
		{
			if (attributes[i] == null)
			{
				throw new ArgumentNullException("attributes");
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AttributeCollection" /> class. </summary>
	protected AttributeCollection()
	{
	}

	/// <summary>Creates a new <see cref="T:System.ComponentModel.AttributeCollection" /> from an existing <see cref="T:System.ComponentModel.AttributeCollection" />.</summary>
	/// <returns>A new <see cref="T:System.ComponentModel.AttributeCollection" /> that is a copy of <paramref name="existing" />.</returns>
	/// <param name="existing">An <see cref="T:System.ComponentModel.AttributeCollection" /> from which to create the copy.</param>
	/// <param name="newAttributes">An array of type <see cref="T:System.Attribute" /> that provides the attributes for this collection. Can be null.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="existing" /> is null.</exception>
	public static AttributeCollection FromExisting(AttributeCollection existing, params Attribute[] newAttributes)
	{
		if (existing == null)
		{
			throw new ArgumentNullException("existing");
		}
		if (newAttributes == null)
		{
			newAttributes = new Attribute[0];
		}
		Attribute[] array = new Attribute[existing.Count + newAttributes.Length];
		int count = existing.Count;
		existing.CopyTo(array, 0);
		for (int i = 0; i < newAttributes.Length; i++)
		{
			if (newAttributes[i] == null)
			{
				throw new ArgumentNullException("newAttributes");
			}
			bool flag = false;
			for (int j = 0; j < existing.Count; j++)
			{
				if (array[j].TypeId.Equals(newAttributes[i].TypeId))
				{
					flag = true;
					array[j] = newAttributes[i];
					break;
				}
			}
			if (!flag)
			{
				array[count++] = newAttributes[i];
			}
		}
		Attribute[] array2 = null;
		if (count < array.Length)
		{
			array2 = new Attribute[count];
			Array.Copy(array, 0, array2, 0, count);
		}
		else
		{
			array2 = array;
		}
		return new AttributeCollection(array2);
	}

	/// <summary>Determines whether this collection of attributes has the specified attribute.</summary>
	/// <returns>true if the collection contains the attribute or is the default attribute for the type of attribute; otherwise, false.</returns>
	/// <param name="attribute">An <see cref="T:System.Attribute" /> to find in the collection. </param>
	public bool Contains(Attribute attribute)
	{
		Attribute attribute2 = this[attribute.GetType()];
		if (attribute2 != null && attribute2.Equals(attribute))
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether this attribute collection contains all the specified attributes in the attribute array.</summary>
	/// <returns>true if the collection contains all the attributes; otherwise, false.</returns>
	/// <param name="attributes">An array of type <see cref="T:System.Attribute" /> to find in the collection. </param>
	public bool Contains(Attribute[] attributes)
	{
		if (attributes == null)
		{
			return true;
		}
		for (int i = 0; i < attributes.Length; i++)
		{
			if (!Contains(attributes[i]))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Returns the default <see cref="T:System.Attribute" /> of a given <see cref="T:System.Type" />.</summary>
	/// <returns>The default <see cref="T:System.Attribute" /> of a given <paramref name="attributeType" />.</returns>
	/// <param name="attributeType">The <see cref="T:System.Type" /> of the attribute to retrieve. </param>
	protected Attribute GetDefaultAttribute(Type attributeType)
	{
		lock (internalSyncObject)
		{
			if (_defaultAttributes == null)
			{
				_defaultAttributes = new Hashtable();
			}
			if (_defaultAttributes.ContainsKey(attributeType))
			{
				return (Attribute)_defaultAttributes[attributeType];
			}
			Attribute attribute = null;
			Type reflectionType = TypeDescriptor.GetReflectionType(attributeType);
			FieldInfo field = reflectionType.GetField("Default", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField);
			if (field != null && field.IsStatic)
			{
				attribute = (Attribute)field.GetValue(null);
			}
			else
			{
				ConstructorInfo constructor = reflectionType.UnderlyingSystemType.GetConstructor(new Type[0]);
				if (constructor != null)
				{
					attribute = (Attribute)constructor.Invoke(new object[0]);
					if (!attribute.IsDefaultAttribute())
					{
						attribute = null;
					}
				}
			}
			_defaultAttributes[attributeType] = attribute;
			return attribute;
		}
	}

	/// <summary>Gets an enumerator for this collection.</summary>
	/// <returns>An enumerator of type <see cref="T:System.Collections.IEnumerator" />.</returns>
	public IEnumerator GetEnumerator()
	{
		return Attributes.GetEnumerator();
	}

	/// <summary>Determines whether a specified attribute is the same as an attribute in the collection.</summary>
	/// <returns>true if the attribute is contained within the collection and has the same value as the attribute in the collection; otherwise, false.</returns>
	/// <param name="attribute">An instance of <see cref="T:System.Attribute" /> to compare with the attributes in this collection. </param>
	public bool Matches(Attribute attribute)
	{
		for (int i = 0; i < Attributes.Length; i++)
		{
			if (Attributes[i].Match(attribute))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Determines whether the attributes in the specified array are the same as the attributes in the collection.</summary>
	/// <returns>true if all the attributes in the array are contained in the collection and have the same values as the attributes in the collection; otherwise, false.</returns>
	/// <param name="attributes">An array of <see cref="T:System.CodeDom.MemberAttributes" /> to compare with the attributes in this collection. </param>
	public bool Matches(Attribute[] attributes)
	{
		for (int i = 0; i < attributes.Length; i++)
		{
			if (!Matches(attributes[i]))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Copies the collection to an array, starting at the specified index.</summary>
	/// <param name="array">The <see cref="T:System.Array" /> to copy the collection to. </param>
	/// <param name="index">The index to start from. </param>
	public void CopyTo(Array array, int index)
	{
		Array.Copy(Attributes, 0, array, index, Attributes.Length);
	}

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Collections.IDictionary" />. </summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Collections.IDictionary" />.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
