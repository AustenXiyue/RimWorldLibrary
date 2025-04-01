using System.Collections;

namespace System.ComponentModel.Design;

/// <summary>Provides a set of recommended default values during component creation.</summary>
public interface IComponentInitializer
{
	/// <summary>Restores an instance of a component to its default state.</summary>
	/// <param name="defaultValues">A dictionary of default property values, which are name/value pairs, with which to reset the component's state.</param>
	void InitializeExistingComponent(IDictionary defaultValues);

	/// <summary>Initializes a new component using a set of recommended values.</summary>
	/// <param name="defaultValues">A dictionary of default property values, which are name/value pairs, with which to initialize the component's state.</param>
	void InitializeNewComponent(IDictionary defaultValues);
}
