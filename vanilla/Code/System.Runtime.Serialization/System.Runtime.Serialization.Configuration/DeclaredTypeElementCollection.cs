using System.Configuration;

namespace System.Runtime.Serialization.Configuration;

/// <summary>Handles the XML elements used to configure XML serialization using the <see cref="T:System.Runtime.Serialization.DataContractSerializer" />.</summary>
[ConfigurationCollection(typeof(DeclaredTypeElement))]
public sealed class DeclaredTypeElementCollection : ConfigurationElementCollection
{
	/// <summary>Returns the configuration element specified by its index.</summary>
	/// <returns>The <see cref="T:System.Runtime.Serialization.Configuration.DeclaredTypeElement" /> specified by its index.</returns>
	/// <param name="index">The index of the element to access.</param>
	public DeclaredTypeElement this[int index]
	{
		get
		{
			return (DeclaredTypeElement)BaseGet(index);
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

	/// <summary>Returns the item specified by its key.</summary>
	/// <returns>The configuration element specified by its key.</returns>
	/// <param name="typeName">The name of the type (that functions as a key) to return.</param>
	public new DeclaredTypeElement this[string typeName]
	{
		get
		{
			if (string.IsNullOrEmpty(typeName))
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
			}
			return (DeclaredTypeElement)BaseGet(typeName);
		}
		set
		{
			if (!IsReadOnly())
			{
				if (string.IsNullOrEmpty(typeName))
				{
					throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
				}
				if (value == null)
				{
					throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
				}
				if (BaseGet(typeName) == null)
				{
					throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new IndexOutOfRangeException(SR.GetString("For type '{0}', configuration index is out of range.", typeName)));
				}
				BaseRemove(typeName);
			}
			Add(value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Configuration.DeclaredTypeElementCollection" /> class.  </summary>
	public DeclaredTypeElementCollection()
	{
	}

	/// <summary>Adds the specified configuration element.</summary>
	/// <param name="element">The <see cref="T:System.Runtime.Serialization.Configuration.DeclaredTypeElement" /> to add.</param>
	public void Add(DeclaredTypeElement element)
	{
		if (!IsReadOnly() && element == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
		}
		BaseAdd(element);
	}

	/// <summary>Clears the collection of all items.</summary>
	public void Clear()
	{
		BaseClear();
	}

	/// <summary>Returns a value if the collection contains the item specified by its type name.</summary>
	/// <returns>true if the collection contains the specified item; otherwise, false.</returns>
	/// <param name="typeName">The name of the configuration element to search for.</param>
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
		return new DeclaredTypeElement();
	}

	protected override object GetElementKey(ConfigurationElement element)
	{
		if (element == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
		}
		return ((DeclaredTypeElement)element).Type;
	}

	/// <summary>Returns the index of the specified configuration element.</summary>
	/// <returns>The index of the specified configuration element; otherwise, -1.</returns>
	/// <param name="element">The <see cref="T:System.Runtime.Serialization.Configuration.DeclaredTypeElement" /> to find.</param>
	public int IndexOf(DeclaredTypeElement element)
	{
		if (element == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
		}
		return BaseIndexOf(element);
	}

	/// <summary>Removes the specified configuration element from the collection.</summary>
	/// <param name="element">The <see cref="T:System.Runtime.Serialization.Configuration.DeclaredTypeElement" /> to remove.</param>
	public void Remove(DeclaredTypeElement element)
	{
		if (!IsReadOnly() && element == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
		}
		BaseRemove(GetElementKey(element));
	}

	/// <summary>Removes the configuration element specified by its key.</summary>
	/// <param name="typeName">The type name (that functions as a key) of the configuration element to remove.</param>
	public void Remove(string typeName)
	{
		if (!IsReadOnly() && string.IsNullOrEmpty(typeName))
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
		}
		BaseRemove(typeName);
	}

	/// <summary>Removes the configuration element at the specified index.</summary>
	/// <param name="index">The index of the configuration element to remove.</param>
	public void RemoveAt(int index)
	{
		BaseRemoveAt(index);
	}
}
