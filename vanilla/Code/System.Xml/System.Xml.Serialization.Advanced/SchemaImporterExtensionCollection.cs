using System.Collections;

namespace System.Xml.Serialization.Advanced;

/// <summary>Represents a collection of <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtension" /> objects.</summary>
public class SchemaImporterExtensionCollection : CollectionBase
{
	private Hashtable exNames;

	internal Hashtable Names
	{
		get
		{
			if (exNames == null)
			{
				exNames = new Hashtable();
			}
			return exNames;
		}
	}

	/// <summary>Gets the <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtensionCollection" /> at the specified index.</summary>
	/// <returns>The <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtensionCollection" /> at the specified index.</returns>
	/// <param name="index">The index of the item to find.</param>
	public SchemaImporterExtension this[int index]
	{
		get
		{
			return (SchemaImporterExtension)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	/// <summary>Adds the specified importer extension to the collection.</summary>
	/// <returns>The index of the added extension.</returns>
	/// <param name="extension">The <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtensionCollection" /> to add.</param>
	public int Add(SchemaImporterExtension extension)
	{
		return Add(extension.GetType().FullName, extension);
	}

	/// <summary>Adds the specified importer extension to the collection. The name parameter allows you to supply a custom name for the extension.</summary>
	/// <returns>The index of the newly added item.</returns>
	/// <param name="name">A custom name for the extension.</param>
	/// <param name="type">The <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtensionCollection" /> to add.</param>
	/// <exception cref="T:System.ArgumentException">The value of type does not inherit from <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtensionCollection" />.</exception>
	public int Add(string name, Type type)
	{
		if (type.IsSubclassOf(typeof(SchemaImporterExtension)))
		{
			return Add(name, (SchemaImporterExtension)Activator.CreateInstance(type));
		}
		throw new ArgumentException(Res.GetString("'{0}' is not a valid SchemaExtensionType.", type));
	}

	/// <summary>Removes the <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtension" />, specified by name, from the collection.</summary>
	/// <param name="name">The name of the <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtension" /> to remove. The name is set using the <see cref="M:System.Xml.Serialization.Advanced.SchemaImporterExtensionCollection.Add(System.String,System.Type)" /> method.</param>
	public void Remove(string name)
	{
		if (Names[name] != null)
		{
			base.List.Remove(Names[name]);
			Names[name] = null;
		}
	}

	/// <summary>Clears the collection of importer extensions.</summary>
	public new void Clear()
	{
		Names.Clear();
		base.List.Clear();
	}

	internal SchemaImporterExtensionCollection Clone()
	{
		SchemaImporterExtensionCollection schemaImporterExtensionCollection = new SchemaImporterExtensionCollection();
		schemaImporterExtensionCollection.exNames = (Hashtable)Names.Clone();
		foreach (object item in base.List)
		{
			schemaImporterExtensionCollection.List.Add(item);
		}
		return schemaImporterExtensionCollection;
	}

	internal int Add(string name, SchemaImporterExtension extension)
	{
		if (Names[name] != null)
		{
			if (Names[name].GetType() != extension.GetType())
			{
				throw new InvalidOperationException(Res.GetString("Duplicate extension name.  schemaImporterExtension with name '{0}' already been added.", name));
			}
			return -1;
		}
		Names[name] = extension;
		return base.List.Add(extension);
	}

	/// <summary>Inserts the specified <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtension" /> into the collection at the specified index.</summary>
	/// <param name="index">The zero-base index at which the <paramref name="extension" /> should be inserted.</param>
	/// <param name="extension">The <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtension" /> to insert.</param>
	public void Insert(int index, SchemaImporterExtension extension)
	{
		base.List.Insert(index, extension);
	}

	/// <summary>Searches for the specified item and returns the zero-based index of the first occurrence within the collection.</summary>
	/// <returns>The index of the found item.</returns>
	/// <param name="extension">The <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtension" /> to search for.</param>
	public int IndexOf(SchemaImporterExtension extension)
	{
		return base.List.IndexOf(extension);
	}

	/// <summary>Gets a value that indicates whether the specified importer extension exists in the collection.</summary>
	/// <returns>true if the extension is found; otherwise, false.</returns>
	/// <param name="extension">The <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtensionCollection" /> to search for.</param>
	public bool Contains(SchemaImporterExtension extension)
	{
		return base.List.Contains(extension);
	}

	/// <summary>Removes the specified <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtension" /> from the collection.</summary>
	/// <param name="extension">The <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtension" /> to remove. </param>
	public void Remove(SchemaImporterExtension extension)
	{
		base.List.Remove(extension);
	}

	/// <summary>Copies all the elements of the current <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtensionCollection" /> to the specified array of <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtension" /> objects at the specified index. </summary>
	/// <param name="array">The <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtension" /> to copy the current collection to.</param>
	/// <param name="index">The zero-based index at which the collection is added.</param>
	public void CopyTo(SchemaImporterExtension[] array, int index)
	{
		base.List.CopyTo(array, index);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.Advanced.SchemaImporterExtensionCollection" /> class. </summary>
	public SchemaImporterExtensionCollection()
	{
	}
}
