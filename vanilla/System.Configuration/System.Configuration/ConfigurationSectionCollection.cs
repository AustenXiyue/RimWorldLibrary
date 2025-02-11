using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Unity;

namespace System.Configuration;

/// <summary>Represents a collection of related sections within a configuration file.</summary>
[Serializable]
public sealed class ConfigurationSectionCollection : NameObjectCollectionBase
{
	private SectionGroupInfo group;

	private Configuration config;

	private static readonly object lockObject = new object();

	/// <summary>Gets the keys to all <see cref="T:System.Configuration.ConfigurationSection" /> objects contained in this <see cref="T:System.Configuration.ConfigurationSectionCollection" /> object.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" /> object that contains the keys of all sections in this collection.</returns>
	public override KeysCollection Keys => group.Sections.Keys;

	/// <summary>Gets the number of sections in this <see cref="T:System.Configuration.ConfigurationSectionCollection" /> object.</summary>
	/// <returns>An integer that represents the number of sections in the collection.</returns>
	public override int Count => group.Sections.Count;

	/// <summary>Gets the specified <see cref="T:System.Configuration.ConfigurationSection" /> object.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationSection" /> object with the specified name.</returns>
	/// <param name="name">The name of the <see cref="T:System.Configuration.ConfigurationSection" /> object to be returned. </param>
	public ConfigurationSection this[string name]
	{
		get
		{
			ConfigurationSection configurationSection = BaseGet(name) as ConfigurationSection;
			if (configurationSection == null)
			{
				if (!(group.Sections[name] is SectionInfo sectionInfo))
				{
					return null;
				}
				configurationSection = config.GetSectionInstance(sectionInfo, createDefaultInstance: true);
				if (configurationSection == null)
				{
					return null;
				}
				lock (lockObject)
				{
					BaseSet(name, configurationSection);
				}
			}
			return configurationSection;
		}
	}

	/// <summary>Gets the specified <see cref="T:System.Configuration.ConfigurationSection" /> object.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationSection" /> object at the specified index.</returns>
	/// <param name="index">The index of the <see cref="T:System.Configuration.ConfigurationSection" /> object to be returned. </param>
	public ConfigurationSection this[int index] => this[GetKey(index)];

	internal ConfigurationSectionCollection(Configuration config, SectionGroupInfo group)
		: base(StringComparer.Ordinal)
	{
		this.config = config;
		this.group = group;
	}

	/// <summary>Adds a <see cref="T:System.Configuration.ConfigurationSection" /> object to the <see cref="T:System.Configuration.ConfigurationSectionCollection" /> object.</summary>
	/// <param name="name">The name of the section to be added.</param>
	/// <param name="section">The section to be added.</param>
	public void Add(string name, ConfigurationSection section)
	{
		config.CreateSection(group, name, section);
	}

	/// <summary>Clears this <see cref="T:System.Configuration.ConfigurationSectionCollection" /> object.</summary>
	public void Clear()
	{
		if (group.Sections == null)
		{
			return;
		}
		foreach (ConfigInfo section in group.Sections)
		{
			config.RemoveConfigInfo(section);
		}
	}

	/// <summary>Copies this <see cref="T:System.Configuration.ConfigurationSectionCollection" /> object to an array.</summary>
	/// <param name="array">The array to copy the <see cref="T:System.Configuration.ConfigurationSectionCollection" /> object to.</param>
	/// <param name="index">The index location at which to begin copying.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of <paramref name="array" /> is less than the value of <see cref="P:System.Configuration.ConfigurationSectionCollection.Count" /> plus <paramref name="index" />.</exception>
	public void CopyTo(ConfigurationSection[] array, int index)
	{
		for (int i = 0; i < group.Sections.Count; i++)
		{
			array[i + index] = this[i];
		}
	}

	/// <summary>Gets the specified <see cref="T:System.Configuration.ConfigurationSection" /> object contained in this <see cref="T:System.Configuration.ConfigurationSectionCollection" /> object.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationSection" /> object at the specified index.</returns>
	/// <param name="index">The index of the <see cref="T:System.Configuration.ConfigurationSection" /> object to be returned.</param>
	public ConfigurationSection Get(int index)
	{
		return this[index];
	}

	/// <summary>Gets the specified <see cref="T:System.Configuration.ConfigurationSection" /> object contained in this <see cref="T:System.Configuration.ConfigurationSectionCollection" /> object.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationSection" /> object with the specified name.</returns>
	/// <param name="name">The name of the <see cref="T:System.Configuration.ConfigurationSection" /> object to be returned.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is null or an empty string ("").</exception>
	public ConfigurationSection Get(string name)
	{
		return this[name];
	}

	/// <summary>Gets an enumerator that can iterate through this <see cref="T:System.Configuration.ConfigurationSectionCollection" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through this <see cref="T:System.Configuration.ConfigurationSectionCollection" /> object.</returns>
	public override IEnumerator GetEnumerator()
	{
		foreach (string allKey in group.Sections.AllKeys)
		{
			yield return this[allKey];
		}
	}

	/// <summary>Gets the key of the specified <see cref="T:System.Configuration.ConfigurationSection" /> object contained in this <see cref="T:System.Configuration.ConfigurationSectionCollection" /> object.</summary>
	/// <returns>The key of the <see cref="T:System.Configuration.ConfigurationSection" /> object at the specified index.</returns>
	/// <param name="index">The index of the <see cref="T:System.Configuration.ConfigurationSection" /> object whose key is to be returned. </param>
	public string GetKey(int index)
	{
		return group.Sections.GetKey(index);
	}

	/// <summary>Removes the specified <see cref="T:System.Configuration.ConfigurationSection" /> object from this <see cref="T:System.Configuration.ConfigurationSectionCollection" /> object.</summary>
	/// <param name="name">The name of the section to be removed. </param>
	public void Remove(string name)
	{
		if (group.Sections[name] is SectionInfo sectionInfo)
		{
			config.RemoveConfigInfo(sectionInfo);
		}
	}

	/// <summary>Removes the specified <see cref="T:System.Configuration.ConfigurationSection" /> object from this <see cref="T:System.Configuration.ConfigurationSectionCollection" /> object.</summary>
	/// <param name="index">The index of the section to be removed. </param>
	public void RemoveAt(int index)
	{
		SectionInfo sectionInfo = group.Sections[index] as SectionInfo;
		config.RemoveConfigInfo(sectionInfo);
	}

	/// <summary>Used by the system during serialization.</summary>
	/// <param name="info">The applicable <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object.</param>
	/// <param name="context">The applicable <see cref="T:System.Runtime.Serialization.StreamingContext" /> object.</param>
	[System.MonoTODO]
	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		throw new NotImplementedException();
	}

	internal ConfigurationSectionCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
