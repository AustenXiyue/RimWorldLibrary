using System.Configuration.Provider;

namespace System.Configuration;

/// <summary>Provides a collection of <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> objects.</summary>
public class ProtectedConfigurationProviderCollection : ProviderCollection
{
	/// <summary>Gets a <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> object in the collection with the specified name.</summary>
	/// <returns>The <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> object with the specified name, or null if there is no object with that name.</returns>
	/// <param name="name">The name of a <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> object in the collection.</param>
	[System.MonoTODO]
	public new ProtectedConfigurationProvider this[string name] => (ProtectedConfigurationProvider)base[name];

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ProtectedConfigurationProviderCollection" /> class using default settings.</summary>
	public ProtectedConfigurationProviderCollection()
	{
	}

	/// <summary>Adds a <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> object to the collection.</summary>
	/// <param name="provider">A <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> object to add to the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="provider" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="provider" /> is not a <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> object.</exception>
	/// <exception cref="T:System.Configuration.ConfigurationException">The <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> object to add already exists in the collection.- or -The collection is read-only.</exception>
	[System.MonoTODO]
	public override void Add(ProviderBase provider)
	{
		base.Add(provider);
	}
}
