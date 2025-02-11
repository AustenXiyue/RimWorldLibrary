using System.Collections.Generic;

namespace System.Xaml;

/// <summary>Describes a service that can return objects that are specified by XAML name, or alternatively, returns a token that defers name resolution. The service can also return an enumerable set of all named objects that are in the XAML namescope.</summary>
public interface IXamlNameResolver
{
	/// <summary>Gets a value that determines whether calling <see cref="M:System.Xaml.IXamlNameResolver.GetFixupToken(System.Collections.Generic.IEnumerable{System.String},System.Boolean)" /> is available in order to resolve a name into a token for forward resolution.</summary>
	/// <returns>true if <see cref="M:System.Xaml.IXamlNameResolver.GetFixupToken(System.Collections.Generic.IEnumerable{System.String},System.Boolean)" /> is available as an implementation that returns a useful token for forward resolution; otherwise, false.</returns>
	bool IsFixupTokenAvailable { get; }

	/// <summary>Occurs when a XAML processor has registered all the relevant names to the backing XAML namescope.</summary>
	event EventHandler OnNameScopeInitializationComplete;

	/// <summary>Resolves an object from a name reference.</summary>
	/// <returns>The resolved object; or null.</returns>
	/// <param name="name">The name reference to resolve.</param>
	object Resolve(string name);

	/// <summary>Resolves an object from a name reference, and provides a tracking value that reports whether the object is fully initialized for object graph purposes.</summary>
	/// <returns>An object that provides a token for lookup behavior to be evaluated later.</returns>
	/// <param name="name">The name reference to resolve.</param>
	/// <param name="isFullyInitialized">When this method returns, true if the returned object has any dependencies on unresolved references; otherwise, false.</param>
	object Resolve(string name, out bool isFullyInitialized);

	/// <summary>Returns an object that can correct for certain markup patterns that produce forward references.</summary>
	/// <returns>An object that provides a token for lookup behavior to be evaluated later.</returns>
	/// <param name="names">A collection of names that are possible forward references.</param>
	object GetFixupToken(IEnumerable<string> names);

	/// <summary>Returns an object that can correct for certain markup patterns that produce forward references.</summary>
	/// <returns>An object that provides a token for lookup behavior to be evaluated later.</returns>
	/// <param name="names">A collection of names that are possible forward references.</param>
	/// <param name="canAssignDirectly">true to immediately assign the resolved name reference to the target property. false to call the user code for a reparse. The default behavior is false.</param>
	object GetFixupToken(IEnumerable<string> names, bool canAssignDirectly);

	/// <summary>Returns an enumerable set of all named objects in the XAML namescope.</summary>
	/// <returns>An enumerable set of <see cref="T:System.Collections.Generic.KeyValuePair`2" /> objects. For each <see cref="T:System.Collections.Generic.KeyValuePair`2" />, the <see cref="P:System.Collections.Generic.KeyValuePair`2.Key" /> component is a string, and the <see cref="P:System.Collections.Generic.KeyValuePair`2.Value" /> component is the object that uses the <see cref="P:System.Collections.Generic.KeyValuePair`2.Key" /> name in the XAML namescope.</returns>
	IEnumerable<KeyValuePair<string, object>> GetAllNamesAndValuesInScope();
}
