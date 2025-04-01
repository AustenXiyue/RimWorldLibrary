using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Represents a container for the addresses of resources that bypass the proxy server. This class cannot be inherited.</summary>
[ConfigurationCollection(typeof(BypassElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
public sealed class BypassElementCollection : ConfigurationElementCollection
{
	/// <summary>Gets or sets the element at the specified position in the collection.</summary>
	/// <returns>The <see cref="T:System.Net.Configuration.BypassElement" /> at the specified location.</returns>
	/// <param name="index">The zero-based index of the element.</param>
	[System.MonoTODO]
	public BypassElement this[int index]
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the element with the specified key.</summary>
	/// <returns>The <see cref="T:System.Net.Configuration.BypassElement" /> with the specified key, or null if there is no element with the specified key.</returns>
	/// <param name="name">The key for an element in the collection. </param>
	public new BypassElement this[string name]
	{
		get
		{
			return (BypassElement)base[name];
		}
		set
		{
			base[name] = value;
		}
	}

	protected override bool ThrowOnDuplicate => false;

	/// <summary>Initializes an empty instance of the <see cref="T:System.Net.Configuration.BypassElementCollection" /> class. </summary>
	public BypassElementCollection()
	{
	}

	/// <summary>Adds an element to the collection.</summary>
	/// <param name="element">The <see cref="T:System.Net.Configuration.BypassElement" /> to add to the collection.</param>
	public void Add(BypassElement element)
	{
		BaseAdd(element);
	}

	/// <summary>Removes all elements from the collection.</summary>
	public void Clear()
	{
		BaseClear();
	}

	protected override ConfigurationElement CreateNewElement()
	{
		return new BypassElement();
	}

	[System.MonoTODO("argument exception?")]
	protected override object GetElementKey(ConfigurationElement element)
	{
		if (!(element is BypassElement))
		{
			throw new ArgumentException("element");
		}
		return ((BypassElement)element).Address;
	}

	/// <summary>Returns the index of the specified configuration element.</summary>
	/// <returns>The zero-based index of <paramref name="element" />.</returns>
	/// <param name="element">A <see cref="T:System.Net.Configuration.BypassElement" />.</param>
	public int IndexOf(BypassElement element)
	{
		return BaseIndexOf(element);
	}

	/// <summary>Removes the specified configuration element from the collection.</summary>
	/// <param name="element">The <see cref="T:System.Net.Configuration.BypassElement" /> to remove.</param>
	public void Remove(BypassElement element)
	{
		BaseRemove(element);
	}

	/// <summary>Removes the element with the specified key.</summary>
	/// <param name="name">The key of the element to remove.</param>
	public void Remove(string name)
	{
		BaseRemove(name);
	}

	/// <summary>Removes the element at the specified index.</summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	public void RemoveAt(int index)
	{
		BaseRemoveAt(index);
	}
}
