namespace System.ComponentModel;

/// <summary>Provides the ability to retrieve the full nested name of a component.</summary>
public interface INestedSite : ISite, IServiceProvider
{
	/// <summary>Gets the full name of the component in this site.</summary>
	/// <returns>The full name of the component in this site.</returns>
	string FullName { get; }
}
