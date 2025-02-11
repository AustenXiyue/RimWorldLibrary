using System.Configuration;

namespace System.Xml.Serialization.Configuration;

/// <summary>Handles the XML elements used to configure the operation of the <see cref="T:System.Xml.Serialization.XmlSchemaImporter" />. This class cannot be inherited.</summary>
[ConfigurationCollection(typeof(SchemaImporterExtensionElement))]
public sealed class SchemaImporterExtensionElementCollection : ConfigurationElementCollection
{
	/// <summary>Gets or sets the object that represents the XML element at the specified index.</summary>
	/// <returns>The <see cref="T:System.Xml.Serialization.Configuration.SchemaImporterExtensionElement" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the XML element to get or set.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="index" /> is equal to or greater than Count.</exception>
	public SchemaImporterExtensionElement this[int index]
	{
		get
		{
			return (SchemaImporterExtensionElement)BaseGet(index);
		}
		set
		{
			if (BaseGet(index) != null)
			{
				BaseRemoveAt(index);
			}
			BaseAdd(index, value);
		}
	}

	/// <summary>Gets or sets the item with the specified name.</summary>
	/// <returns>The <see cref="T:System.Xml.Serialization.Configuration.SchemaImporterExtensionElement" /> with the specified name.</returns>
	/// <param name="name">The name of the item to get or set.</param>
	public new SchemaImporterExtensionElement this[string name]
	{
		get
		{
			return (SchemaImporterExtensionElement)BaseGet(name);
		}
		set
		{
			if (BaseGet(name) != null)
			{
				BaseRemove(name);
			}
			BaseAdd(value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.Configuration.SchemaImporterExtensionElementCollection" /> class.</summary>
	public SchemaImporterExtensionElementCollection()
	{
	}

	/// <summary>Adds an item to the end of the collection.</summary>
	/// <param name="element">The <see cref="T:System.Xml.Serialization.Configuration.SchemaImporterExtensionElement" /> to add to the collection.</param>
	public void Add(SchemaImporterExtensionElement element)
	{
		BaseAdd(element);
	}

	/// <summary>Removes all items from the collection.</summary>
	public void Clear()
	{
		BaseClear();
	}

	protected override ConfigurationElement CreateNewElement()
	{
		return new SchemaImporterExtensionElement();
	}

	protected override object GetElementKey(ConfigurationElement element)
	{
		return ((SchemaImporterExtensionElement)element).Key;
	}

	/// <summary>Returns the zero-based index of the first element in the collection with the specified value.</summary>
	/// <returns>The index of the found element.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Serialization.Configuration.SchemaImporterExtensionElement" /> to find.</param>
	public int IndexOf(SchemaImporterExtensionElement element)
	{
		return BaseIndexOf(element);
	}

	/// <summary>Removes the first occurrence of a specific item from the collection.</summary>
	/// <param name="element">The <see cref="T:System.Xml.Serialization.Configuration.SchemaImporterExtensionElement" /> to remove.</param>
	public void Remove(SchemaImporterExtensionElement element)
	{
		BaseRemove(element.Key);
	}

	/// <summary>Removes the item with the specified name from the collection.</summary>
	/// <param name="name">The name of the item to remove.</param>
	public void Remove(string name)
	{
		BaseRemove(name);
	}

	/// <summary>Removes the item at the specified index from the collection.</summary>
	/// <param name="index">The index of the object to remove.</param>
	public void RemoveAt(int index)
	{
		BaseRemoveAt(index);
	}
}
