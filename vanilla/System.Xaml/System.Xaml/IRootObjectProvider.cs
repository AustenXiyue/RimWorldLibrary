namespace System.Xaml;

/// <summary>Describes a service that can return the root object of markup being parsed.</summary>
public interface IRootObjectProvider
{
	/// <summary>Gets the root object from markup or from an object graph.</summary>
	/// <returns>The root object.</returns>
	object RootObject { get; }
}
