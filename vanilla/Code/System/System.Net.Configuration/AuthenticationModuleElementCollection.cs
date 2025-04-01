using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Represents a container for authentication module configuration elements. This class cannot be inherited.</summary>
[ConfigurationCollection(typeof(AuthenticationModuleElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
public sealed class AuthenticationModuleElementCollection : ConfigurationElementCollection
{
	/// <summary>Gets or sets the element at the specified position in the collection.</summary>
	/// <returns>The <see cref="T:System.Net.Configuration.AuthenticationModuleElement" /> at the specified location.</returns>
	/// <param name="index">The zero-based index of the element.</param>
	[System.MonoTODO]
	public AuthenticationModuleElement this[int index]
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
	/// <returns>The <see cref="T:System.Net.Configuration.AuthenticationModuleElement" /> with the specified key or null if there is no element with the specified key.</returns>
	/// <param name="name">The key for an element in the collection. </param>
	[System.MonoTODO]
	public new AuthenticationModuleElement this[string name]
	{
		get
		{
			return (AuthenticationModuleElement)base[name];
		}
		set
		{
			base[name] = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.AuthenticationModuleElementCollection" /> class. </summary>
	[System.MonoTODO]
	public AuthenticationModuleElementCollection()
	{
	}

	/// <summary>Adds an element to the collection.</summary>
	/// <param name="element">The <see cref="T:System.Net.Configuration.AuthenticationModuleElement" /> to add to the collection.</param>
	public void Add(AuthenticationModuleElement element)
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
		return new AuthenticationModuleElement();
	}

	[System.MonoTODO("argument exception?")]
	protected override object GetElementKey(ConfigurationElement element)
	{
		if (!(element is AuthenticationModuleElement))
		{
			throw new ArgumentException("element");
		}
		return ((AuthenticationModuleElement)element).Type;
	}

	/// <summary>Returns the index of the specified configuration element.</summary>
	/// <returns>The zero-based index of <paramref name="element" />.</returns>
	/// <param name="element">A <see cref="T:System.Net.Configuration.AuthenticationModuleElement" />.</param>
	public int IndexOf(AuthenticationModuleElement element)
	{
		return BaseIndexOf(element);
	}

	/// <summary>Removes the specified configuration element from the collection.</summary>
	/// <param name="element">The <see cref="T:System.Net.Configuration.AuthenticationModuleElement" /> to remove.</param>
	public void Remove(AuthenticationModuleElement element)
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
