namespace System.Configuration.Internal;

/// <summary>Defines an interface used by the .NET Framework to support the initialization of configuration properties.</summary>
public interface IConfigSystem
{
	/// <summary>Gets the configuration host.</summary>
	/// <returns>An <see cref="T:System.Configuration.Internal.IInternalConfigHost" /> object that is used by the .NET Framework to initialize application configuration properties.</returns>
	IInternalConfigHost Host { get; }

	/// <summary>Gets the root of the configuration hierarchy.</summary>
	/// <returns>An <see cref="T:System.Configuration.Internal.IInternalConfigRoot" /> object.</returns>
	IInternalConfigRoot Root { get; }

	/// <summary>Initializes a configuration object.</summary>
	/// <param name="typeConfigHost">The type of configuration host.</param>
	/// <param name="hostInitParams">An array of configuration host parameters.</param>
	void Init(Type typeConfigHost, params object[] hostInitParams);
}
