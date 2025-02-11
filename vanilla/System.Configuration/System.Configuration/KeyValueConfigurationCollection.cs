using System.Collections;

namespace System.Configuration;

/// <summary>Contains a collection of <see cref="T:System.Configuration.KeyValueConfigurationElement" /> objects. </summary>
[ConfigurationCollection(typeof(KeyValueConfigurationElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
public class KeyValueConfigurationCollection : ConfigurationElementCollection
{
	private ConfigurationPropertyCollection properties;

	/// <summary>Gets the keys to all items contained in the <see cref="T:System.Configuration.KeyValueConfigurationCollection" /> collection.</summary>
	/// <returns>A string array.</returns>
	public string[] AllKeys
	{
		get
		{
			string[] array = new string[base.Count];
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					KeyValueConfigurationElement keyValueConfigurationElement = (KeyValueConfigurationElement)enumerator.Current;
					array[num++] = keyValueConfigurationElement.Key;
				}
				return array;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}
	}

	/// <summary>Gets the <see cref="T:System.Configuration.KeyValueConfigurationElement" /> object based on the supplied parameter.</summary>
	/// <returns>A configuration element, or null if the key does not exist in the collection.</returns>
	/// <param name="key">The key of the <see cref="T:System.Configuration.KeyValueConfigurationElement" /> contained in the collection.</param>
	public new KeyValueConfigurationElement this[string key] => (KeyValueConfigurationElement)BaseGet(key);

	/// <summary>Gets a collection of configuration properties.</summary>
	/// <returns>A collection of configuration properties.</returns>
	protected internal override ConfigurationPropertyCollection Properties
	{
		get
		{
			if (properties == null)
			{
				properties = new ConfigurationPropertyCollection();
			}
			return properties;
		}
	}

	/// <summary>Gets a value indicating whether an attempt to add a duplicate <see cref="T:System.Configuration.KeyValueConfigurationElement" /> object to the <see cref="T:System.Configuration.KeyValueConfigurationCollection" /> collection will cause an exception to be thrown.</summary>
	/// <returns>true if an attempt to add a duplicate <see cref="T:System.Configuration.KeyValueConfigurationElement" /> to the <see cref="T:System.Configuration.KeyValueConfigurationCollection" /> will cause an exception to be thrown; otherwise, false. </returns>
	protected override bool ThrowOnDuplicate => false;

	/// <summary>Adds a <see cref="T:System.Configuration.KeyValueConfigurationElement" /> object to the collection based on the supplied parameters.</summary>
	/// <param name="keyValue">A <see cref="T:System.Configuration.KeyValueConfigurationElement" />.</param>
	public void Add(KeyValueConfigurationElement keyValue)
	{
		keyValue.Init();
		BaseAdd(keyValue);
	}

	/// <summary>Adds a <see cref="T:System.Configuration.KeyValueConfigurationElement" /> object to the collection based on the supplied parameters.</summary>
	/// <param name="key">A string specifying the key.</param>
	/// <param name="value">A string specifying the value.</param>
	public void Add(string key, string value)
	{
		Add(new KeyValueConfigurationElement(key, value));
	}

	/// <summary>Clears the <see cref="T:System.Configuration.KeyValueConfigurationCollection" /> collection.</summary>
	public void Clear()
	{
		BaseClear();
	}

	/// <summary>Removes a <see cref="T:System.Configuration.KeyValueConfigurationElement" /> object from the collection.</summary>
	/// <param name="key">A string specifying the <paramref name="key" />.</param>
	public void Remove(string key)
	{
		BaseRemove(key);
	}

	/// <summary>When overridden in a derived class, the <see cref="M:System.Configuration.KeyValueConfigurationCollection.CreateNewElement" /> method creates a new <see cref="T:System.Configuration.KeyValueConfigurationElement" /> object.</summary>
	/// <returns>A newly created <see cref="T:System.Configuration.KeyValueConfigurationElement" />.</returns>
	protected override ConfigurationElement CreateNewElement()
	{
		return new KeyValueConfigurationElement();
	}

	/// <summary>Gets the element key for a specified configuration element when overridden in a derived class.</summary>
	/// <returns>An object that acts as the key for the specified <see cref="T:System.Configuration.KeyValueConfigurationElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Configuration.KeyValueConfigurationElement" /> to which the key should be returned.</param>
	protected override object GetElementKey(ConfigurationElement element)
	{
		return ((KeyValueConfigurationElement)element).Key;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.KeyValueConfigurationCollection" /> class.</summary>
	public KeyValueConfigurationCollection()
	{
	}
}
