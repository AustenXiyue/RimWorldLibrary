using System.Configuration;

namespace System.Runtime.Serialization.Configuration;

/// <summary>Handles the XML elements used to configure the known types used for serialization by the <see cref="T:System.Runtime.Serialization.DataContractSerializer" />.</summary>
[ConfigurationCollection(typeof(TypeElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
public sealed class TypeElementCollection : ConfigurationElementCollection
{
	private const string KnownTypeConfig = "knownType";

	/// <summary>Returns a specific member of the collection by its position.</summary>
	/// <returns>The element at the specified position.</returns>
	/// <param name="index">The position of the item to return.</param>
	public TypeElement this[int index]
	{
		get
		{
			return (TypeElement)BaseGet(index);
		}
		set
		{
			if (!IsReadOnly())
			{
				if (value == null)
				{
					throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
				}
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
			}
			BaseAdd(index, value);
		}
	}

	/// <summary>Gets the collection of elements that represents the types using known types.</summary>
	/// <returns>A <see cref="T:System.Configuration.ConfigurationElementCollectionType" /> that contains the element objects.</returns>
	public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;

	protected override string ElementName => "knownType";

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Configuration.TypeElementCollection" /> class.  </summary>
	public TypeElementCollection()
	{
	}

	/// <summary>Adds the specified element to the collection.</summary>
	/// <param name="element">A <see cref="T:System.Runtime.Serialization.Configuration.TypeElement" /> that represents the known type to add. </param>
	public void Add(TypeElement element)
	{
		if (!IsReadOnly() && element == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
		}
		BaseAdd(element);
	}

	/// <summary>Removes all members of the collection.</summary>
	public void Clear()
	{
		BaseClear();
	}

	protected override ConfigurationElement CreateNewElement()
	{
		return new TypeElement();
	}

	protected override object GetElementKey(ConfigurationElement element)
	{
		if (element == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
		}
		return ((TypeElement)element).Key;
	}

	/// <summary>Returns the position of the specified element.</summary>
	/// <returns>The position of the specified element.</returns>
	/// <param name="element">The <see cref="T:System.Runtime.Serialization.Configuration.TypeElement" /> to find in the collection.</param>
	public int IndexOf(TypeElement element)
	{
		if (element == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
		}
		return BaseIndexOf(element);
	}

	/// <summary>Removes the specified element from the collection.</summary>
	/// <param name="element">The <see cref="T:System.Runtime.Serialization.Configuration.TypeElement" /> to remove.</param>
	public void Remove(TypeElement element)
	{
		if (!IsReadOnly() && element == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
		}
		BaseRemove(GetElementKey(element));
	}

	/// <summary>Removes the element at the specified position.</summary>
	/// <param name="index">The position in the collection from which to remove the element.</param>
	public void RemoveAt(int index)
	{
		BaseRemoveAt(index);
	}
}
