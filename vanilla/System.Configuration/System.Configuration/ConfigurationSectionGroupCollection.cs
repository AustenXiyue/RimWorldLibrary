using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Unity;

namespace System.Configuration;

/// <summary>Represents a collection of <see cref="T:System.Configuration.ConfigurationSectionGroup" /> objects.</summary>
[Serializable]
public sealed class ConfigurationSectionGroupCollection : NameObjectCollectionBase
{
	private SectionGroupInfo group;

	private Configuration config;

	/// <summary>Gets the keys to all <see cref="T:System.Configuration.ConfigurationSectionGroup" /> objects contained in this <see cref="T:System.Configuration.ConfigurationSectionGroupCollection" /> object.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.NameObjectCollectionBase.KeysCollection" /> object that contains the names of all section groups in this collection.</returns>
	public override KeysCollection Keys => group.Groups.Keys;

	/// <summary>Gets the number of section groups in the collection.</summary>
	/// <returns>An integer that represents the number of section groups in the collection.</returns>
	public override int Count => group.Groups.Count;

	/// <summary>Gets the <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object whose name is specified from the collection.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object with the specified name.In C#, this property is the indexer for the <see cref="T:System.Configuration.ConfigurationSectionCollection" /> class. </returns>
	/// <param name="name">The name of the <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object to be returned. </param>
	public ConfigurationSectionGroup this[string name]
	{
		get
		{
			ConfigurationSectionGroup configurationSectionGroup = BaseGet(name) as ConfigurationSectionGroup;
			if (configurationSectionGroup == null)
			{
				if (!(group.Groups[name] is SectionGroupInfo sectionGroupInfo))
				{
					return null;
				}
				configurationSectionGroup = config.GetSectionGroupInstance(sectionGroupInfo);
				BaseSet(name, configurationSectionGroup);
			}
			return configurationSectionGroup;
		}
	}

	/// <summary>Gets the <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object whose index is specified from the collection.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object at the specified index.In C#, this property is the indexer for the <see cref="T:System.Configuration.ConfigurationSectionCollection" /> class. </returns>
	/// <param name="index">The index of the <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object to be returned. </param>
	public ConfigurationSectionGroup this[int index] => this[GetKey(index)];

	internal ConfigurationSectionGroupCollection(Configuration config, SectionGroupInfo group)
		: base(StringComparer.Ordinal)
	{
		this.config = config;
		this.group = group;
	}

	/// <summary>Adds a <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object to this <see cref="T:System.Configuration.ConfigurationSectionGroupCollection" /> object.</summary>
	/// <param name="name">The name of the <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object to be added.</param>
	/// <param name="sectionGroup">The <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object to be added.</param>
	public void Add(string name, ConfigurationSectionGroup sectionGroup)
	{
		config.CreateSectionGroup(group, name, sectionGroup);
	}

	/// <summary>Clears the collection.</summary>
	public void Clear()
	{
		if (group.Groups == null)
		{
			return;
		}
		foreach (ConfigInfo group in group.Groups)
		{
			config.RemoveConfigInfo(group);
		}
	}

	/// <summary>Copies this <see cref="T:System.Configuration.ConfigurationSectionGroupCollection" /> object to an array.</summary>
	/// <param name="array">The array to copy the object to.</param>
	/// <param name="index">The index location at which to begin copying.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of <paramref name="array" /> is less than the value of <see cref="P:System.Configuration.ConfigurationSectionGroupCollection.Count" /> plus <paramref name="index" />.</exception>
	public void CopyTo(ConfigurationSectionGroup[] array, int index)
	{
		for (int i = 0; i < group.Groups.Count; i++)
		{
			array[i + index] = this[i];
		}
	}

	/// <summary>Gets the specified <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object contained in the collection.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object at the specified index.</returns>
	/// <param name="index">The index of the <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object to be returned. </param>
	public ConfigurationSectionGroup Get(int index)
	{
		return this[index];
	}

	/// <summary>Gets the specified <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object from the collection.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object with the specified name.</returns>
	/// <param name="name">The name of the <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object to be returned. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is null or an empty string ("").</exception>
	public ConfigurationSectionGroup Get(string name)
	{
		return this[name];
	}

	/// <summary>Gets an enumerator that can iterate through the <see cref="T:System.Configuration.ConfigurationSectionGroupCollection" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the <see cref="T:System.Configuration.ConfigurationSectionGroupCollection" /> object.</returns>
	public override IEnumerator GetEnumerator()
	{
		return group.Groups.AllKeys.GetEnumerator();
	}

	/// <summary>Gets the key of the specified <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object contained in this <see cref="T:System.Configuration.ConfigurationSectionGroupCollection" /> object.</summary>
	/// <returns>The key of the <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object at the specified index.</returns>
	/// <param name="index">The index of the section group whose key is to be returned. </param>
	public string GetKey(int index)
	{
		return group.Groups.GetKey(index);
	}

	/// <summary>Removes the <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object whose name is specified from this <see cref="T:System.Configuration.ConfigurationSectionGroupCollection" /> object.</summary>
	/// <param name="name">The name of the section group to be removed. </param>
	public void Remove(string name)
	{
		if (group.Groups[name] is SectionGroupInfo sectionGroupInfo)
		{
			config.RemoveConfigInfo(sectionGroupInfo);
		}
	}

	/// <summary>Removes the <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object whose index is specified from this <see cref="T:System.Configuration.ConfigurationSectionGroupCollection" /> object.</summary>
	/// <param name="index">The index of the section group to be removed. </param>
	public void RemoveAt(int index)
	{
		SectionGroupInfo sectionGroupInfo = group.Groups[index] as SectionGroupInfo;
		config.RemoveConfigInfo(sectionGroupInfo);
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

	internal ConfigurationSectionGroupCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
