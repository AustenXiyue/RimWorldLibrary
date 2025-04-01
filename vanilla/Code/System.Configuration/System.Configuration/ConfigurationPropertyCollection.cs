using System.Collections;
using System.Collections.Generic;

namespace System.Configuration;

/// <summary>Represents a collection of configuration-element properties.</summary>
public class ConfigurationPropertyCollection : ICollection, IEnumerable
{
	private List<ConfigurationProperty> collection;

	/// <summary>Gets the number of properties in the collection.</summary>
	/// <returns>The number of properties in the collection.</returns>
	public int Count => collection.Count;

	/// <summary>Gets the collection item with the specified name.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationProperty" /> with the specified <paramref name="name" />.</returns>
	/// <param name="name">The <see cref="T:System.Configuration.ConfigurationProperty" /> to return. </param>
	public ConfigurationProperty this[string name]
	{
		get
		{
			foreach (ConfigurationProperty item in collection)
			{
				if (item.Name == name)
				{
					return item;
				}
			}
			return null;
		}
	}

	/// <summary>Gets a value indicating whether access to the collection is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Configuration.ConfigurationPropertyCollection" /> is synchronized; otherwise, false.</returns>
	public bool IsSynchronized => false;

	/// <summary>Gets the object to synchronize access to the collection.</summary>
	/// <returns>The object to synchronize access to the collection.</returns>
	public object SyncRoot => collection;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationPropertyCollection" /> class. </summary>
	public ConfigurationPropertyCollection()
	{
		collection = new List<ConfigurationProperty>();
	}

	/// <summary>Adds a configuration property to the collection.</summary>
	/// <param name="property">The <see cref="T:System.Configuration.ConfigurationProperty" />  to add. </param>
	public void Add(ConfigurationProperty property)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		collection.Add(property);
	}

	/// <summary>Specifies whether the configuration property is contained in this collection.</summary>
	/// <returns>true if the specified <see cref="T:System.Configuration.ConfigurationProperty" /> is contained in the collection; otherwise, false.</returns>
	/// <param name="name">An identifier for the <see cref="T:System.Configuration.ConfigurationProperty" /> to verify. </param>
	public bool Contains(string name)
	{
		ConfigurationProperty configurationProperty = this[name];
		if (configurationProperty == null)
		{
			return false;
		}
		return collection.Contains(configurationProperty);
	}

	/// <summary>Copies this ConfigurationPropertyCollection to an array.</summary>
	/// <param name="array">Array to which to copy.</param>
	/// <param name="index">Index at which to begin copying.</param>
	public void CopyTo(ConfigurationProperty[] array, int index)
	{
		collection.CopyTo(array, index);
	}

	/// <summary>Copies this collection to an array.</summary>
	/// <param name="array">The array to which to copy.</param>
	/// <param name="index">The index location at which to begin copying.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)collection).CopyTo(array, index);
	}

	/// <summary>Gets the <see cref="T:System.Collections.IEnumerator" /> object as it applies to the collection.</summary>
	/// <returns>The <see cref="T:System.Collections.IEnumerator" /> object as it applies to the collection</returns>
	public IEnumerator GetEnumerator()
	{
		return collection.GetEnumerator();
	}

	/// <summary>Removes a configuration property from the collection.</summary>
	/// <returns>true if the specified <see cref="T:System.Configuration.ConfigurationProperty" /> was removed; otherwise, false.</returns>
	/// <param name="name">The <see cref="T:System.Configuration.ConfigurationProperty" /> to remove. </param>
	public bool Remove(string name)
	{
		return collection.Remove(this[name]);
	}

	/// <summary>Removes all configuration property objects from the collection.</summary>
	public void Clear()
	{
		collection.Clear();
	}
}
