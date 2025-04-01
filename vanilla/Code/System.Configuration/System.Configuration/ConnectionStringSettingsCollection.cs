using System.Collections;
using System.Globalization;

namespace System.Configuration;

/// <summary>Contains a collection of <see cref="T:System.Configuration.ConnectionStringSettings" /> objects.</summary>
[ConfigurationCollection(typeof(ConnectionStringSettings), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
public sealed class ConnectionStringSettingsCollection : ConfigurationElementCollection
{
	/// <summary>Gets or sets the <see cref="T:System.Configuration.ConnectionStringSettings" /> object with the specified name in the collection.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConnectionStringSettings" /> object with the specified name; otherwise, null.</returns>
	/// <param name="name">The name of a <see cref="T:System.Configuration.ConnectionStringSettings" /> object in the collection.</param>
	public new ConnectionStringSettings this[string name]
	{
		get
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ConfigurationElement configurationElement = (ConfigurationElement)enumerator.Current;
					if (configurationElement is ConnectionStringSettings && string.Compare(((ConnectionStringSettings)configurationElement).Name, name, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
					{
						return configurationElement as ConnectionStringSettings;
					}
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return null;
		}
	}

	/// <summary>Gets or sets the connection string at the specified index in the collection.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConnectionStringSettings" /> object at the specified index.</returns>
	/// <param name="index">The index of a <see cref="T:System.Configuration.ConnectionStringSettings" /> object in the collection.</param>
	public ConnectionStringSettings this[int index]
	{
		get
		{
			return (ConnectionStringSettings)BaseGet(index);
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

	[System.MonoTODO]
	protected internal override ConfigurationPropertyCollection Properties => base.Properties;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConnectionStringSettingsCollection" /> class. </summary>
	public ConnectionStringSettingsCollection()
	{
	}

	protected override ConfigurationElement CreateNewElement()
	{
		return new ConnectionStringSettings();
	}

	protected override object GetElementKey(ConfigurationElement element)
	{
		return ((ConnectionStringSettings)element).Name;
	}

	/// <summary>Adds a <see cref="T:System.Configuration.ConnectionStringSettings" /> object to the collection.</summary>
	/// <param name="settings">A <see cref="T:System.Configuration.ConnectionStringSettings" /> object to add to the collection.</param>
	public void Add(ConnectionStringSettings settings)
	{
		BaseAdd(settings);
	}

	/// <summary>Removes all the <see cref="T:System.Configuration.ConnectionStringSettings" /> objects from the collection.</summary>
	public void Clear()
	{
		BaseClear();
	}

	/// <summary>Returns the collection index of the passed <see cref="T:System.Configuration.ConnectionStringSettings" /> object.</summary>
	/// <returns>The collection index of the specified <see cref="T:System.Configuration.ConnectionStringSettingsCollection" /> object.</returns>
	/// <param name="settings">A <see cref="T:System.Configuration.ConnectionStringSettings" /> object in the collection.</param>
	public int IndexOf(ConnectionStringSettings settings)
	{
		return BaseIndexOf(settings);
	}

	/// <summary>Removes the specified <see cref="T:System.Configuration.ConnectionStringSettings" /> object from the collection.</summary>
	/// <param name="settings">A <see cref="T:System.Configuration.ConnectionStringSettings" /> object in the collection.</param>
	public void Remove(ConnectionStringSettings settings)
	{
		BaseRemove(settings.Name);
	}

	/// <summary>Removes the specified <see cref="T:System.Configuration.ConnectionStringSettings" /> object from the collection.</summary>
	/// <param name="name">The name of a <see cref="T:System.Configuration.ConnectionStringSettings" /> object in the collection.</param>
	public void Remove(string name)
	{
		BaseRemove(name);
	}

	/// <summary>Removes the <see cref="T:System.Configuration.ConnectionStringSettings" /> object at the specified index in the collection.</summary>
	/// <param name="index">The index of a <see cref="T:System.Configuration.ConnectionStringSettings" /> object in the collection.</param>
	public void RemoveAt(int index)
	{
		BaseRemoveAt(index);
	}

	protected override void BaseAdd(int index, ConfigurationElement element)
	{
		if (!(element is ConnectionStringSettings))
		{
			base.BaseAdd(element);
		}
		if (IndexOf((ConnectionStringSettings)element) >= 0)
		{
			throw new ConfigurationErrorsException($"The element {((ConnectionStringSettings)element).Name} already exist!");
		}
		this[index] = (ConnectionStringSettings)element;
	}
}
