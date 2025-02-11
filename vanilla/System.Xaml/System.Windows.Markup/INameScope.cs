using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Defines a contract for how names of elements should be accessed within a particular XAML namescope, and how to enforce uniqueness of names within that XAML namescope. </summary>
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public interface INameScope
{
	/// <summary>Registers the provided name into the current XAML namescope. </summary>
	/// <param name="name">The name to register.</param>
	/// <param name="scopedElement">The specific element that the provided <paramref name="name" /> refers to.</param>
	void RegisterName(string name, object scopedElement);

	/// <summary>Unregisters the provided name from the current XAML namescope. </summary>
	/// <param name="name">The name to unregister.</param>
	void UnregisterName(string name);

	/// <summary>Returns an object that has the provided identifying name. </summary>
	/// <returns>The object, if found. Returns null if no object of that name was found.</returns>
	/// <param name="name">The name identifier for the object being requested.</param>
	object FindName(string name);
}
