using System.Collections.Specialized;

namespace System.Configuration.Provider;

/// <summary>Provides a base implementation for the extensible provider model.</summary>
public abstract class ProviderBase
{
	private bool alreadyInitialized;

	private string _description;

	private string _name;

	/// <summary>Gets the friendly name used to refer to the provider during configuration.</summary>
	/// <returns>The friendly name used to refer to the provider during configuration.</returns>
	public virtual string Name => _name;

	/// <summary>Gets a brief, friendly description suitable for display in administrative tools or other user interfaces (UIs).</summary>
	/// <returns>A brief, friendly description suitable for display in administrative tools or other UIs.</returns>
	public virtual string Description => _description;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.Provider.ProviderBase" /> class. </summary>
	protected ProviderBase()
	{
	}

	/// <summary>Initializes the provider.</summary>
	/// <param name="name">The friendly name of the provider.</param>
	/// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
	/// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
	/// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)" /> on a provider after the provider has already been initialized.</exception>
	public virtual void Initialize(string name, NameValueCollection config)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("Provider name cannot be null or empty.", "name");
		}
		if (alreadyInitialized)
		{
			throw new InvalidOperationException("This provider instance has already been initialized.");
		}
		alreadyInitialized = true;
		_name = name;
		if (config != null)
		{
			_description = config["description"];
			config.Remove("description");
		}
		if (string.IsNullOrEmpty(_description))
		{
			_description = _name;
		}
	}
}
