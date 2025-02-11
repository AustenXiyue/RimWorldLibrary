using System.Collections.ObjectModel;

namespace System.ComponentModel;

/// <summary>Defines a property that provides information about an object's properties.</summary>
public interface IItemProperties
{
	/// <summary>Gets a collection that contains information about the properties that are available on the items in a collection.</summary>
	/// <returns>A collection that contains information about the properties that are available on the items in a collection.</returns>
	ReadOnlyCollection<ItemPropertyInfo> ItemProperties { get; }
}
