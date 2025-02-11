using System.Configuration;

namespace System.Runtime.Serialization.Configuration;

/// <summary>Handles the XML elements used to configure serialization by the <see cref="T:System.Runtime.Serialization.DataContractSerializer" />.</summary>
[ConfigurationCollection(typeof(ParameterElement), AddItemName = "parameter", CollectionType = ConfigurationElementCollectionType.BasicMap)]
public sealed class ParameterElementCollection : ConfigurationElementCollection
{
	/// <summary>Gets or sets the element in the collection at the specified position. </summary>
	/// <returns>A <see cref="T:System.Runtime.Serialization.Configuration.ParameterElement" /> from the collection.</returns>
	/// <param name="index">The position of the element in the collection to get or set.</param>
	public ParameterElement this[int index]
	{
		get
		{
			return (ParameterElement)BaseGet(index);
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

	/// <summary>Gets the type of the parameters collection in configuration.</summary>
	/// <returns>A <see cref="T:System.Configuration.ConfigurationElementCollectionType" /> that contains the type of the parameters collection in configuration.</returns>
	public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;

	protected override string ElementName => "parameter";

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Configuration.ParameterElementCollection" /> class.  </summary>
	public ParameterElementCollection()
	{
		base.AddElementName = "parameter";
	}

	/// <summary>Adds an element to the collection of parameter elements.</summary>
	/// <param name="element">The <see cref="T:System.Runtime.Serialization.Configuration.ParameterElement" /> element to add to the collection.</param>
	public void Add(ParameterElement element)
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

	/// <summary>Gets or sets a value specifying whether the named type is found in the collection.</summary>
	/// <returns>true if the element is present; otherwise, false.</returns>
	/// <param name="typeName">The name of the type to find.</param>
	public bool Contains(string typeName)
	{
		if (string.IsNullOrEmpty(typeName))
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
		}
		return BaseGet(typeName) != null;
	}

	protected override ConfigurationElement CreateNewElement()
	{
		return new ParameterElement();
	}

	protected override object GetElementKey(ConfigurationElement element)
	{
		if (element == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
		}
		return ((ParameterElement)element).identity;
	}

	/// <summary>Gets the position of the specified element in the collection.</summary>
	/// <returns>The position of the specified element.</returns>
	/// <param name="element">The <see cref="T:System.Runtime.Serialization.Configuration.ParameterElement" /> element to find.</param>
	public int IndexOf(ParameterElement element)
	{
		if (element == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
		}
		return BaseIndexOf(element);
	}

	/// <summary>Removes the specified element from the collection.</summary>
	/// <param name="element">The <see cref="T:System.Runtime.Serialization.Configuration.ParameterElement" /> to remove.</param>
	public void Remove(ParameterElement element)
	{
		if (!IsReadOnly() && element == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
		}
		BaseRemove(GetElementKey(element));
	}

	/// <summary>Removes the element at the specified position.</summary>
	/// <param name="index">The position of the element to remove.</param>
	public void RemoveAt(int index)
	{
		BaseRemoveAt(index);
	}
}
