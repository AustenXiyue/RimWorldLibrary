namespace System.Configuration;

/// <summary>Represents a collection of <see cref="T:System.Configuration.ProviderSettings" /> objects.</summary>
[ConfigurationCollection(typeof(ProviderSettings), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
public sealed class ProviderSettingsCollection : ConfigurationElementCollection
{
	private static ConfigurationPropertyCollection props = new ConfigurationPropertyCollection();

	/// <summary>Gets or sets a value at the specified index in the <see cref="T:System.Configuration.ProviderSettingsCollection" /> collection.</summary>
	/// <returns>The specified <see cref="T:System.Configuration.ProviderSettings" />.</returns>
	/// <param name="index">The index of the <see cref="T:System.Configuration.ProviderSettings" /> to return.</param>
	public ProviderSettings this[int index]
	{
		get
		{
			return (ProviderSettings)BaseGet(index);
		}
		set
		{
			BaseAdd(index, value);
		}
	}

	/// <summary>Gets an item from the collection. </summary>
	/// <returns>A <see cref="T:System.Configuration.ProviderSettings" /> object contained in the collection.</returns>
	/// <param name="key">A string reference to the <see cref="T:System.Configuration.ProviderSettings" /> object within the collection.</param>
	public new ProviderSettings this[string key] => (ProviderSettings)BaseGet(key);

	protected internal override ConfigurationPropertyCollection Properties => props;

	/// <summary>Adds a <see cref="T:System.Configuration.ProviderSettings" /> object to the collection.</summary>
	/// <param name="provider">The <see cref="T:System.Configuration.ProviderSettings" /> object to add.</param>
	public void Add(ProviderSettings provider)
	{
		BaseAdd(provider);
	}

	/// <summary>Clears the collection.</summary>
	public void Clear()
	{
		BaseClear();
	}

	protected override ConfigurationElement CreateNewElement()
	{
		return new ProviderSettings();
	}

	protected override object GetElementKey(ConfigurationElement element)
	{
		return ((ProviderSettings)element).Name;
	}

	/// <summary>Removes an element from the collection.</summary>
	/// <param name="name">The name of the <see cref="T:System.Configuration.ProviderSettings" /> object to remove.</param>
	public void Remove(string name)
	{
		BaseRemove(name);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ProviderSettingsCollection" /> class. </summary>
	public ProviderSettingsCollection()
	{
	}
}
