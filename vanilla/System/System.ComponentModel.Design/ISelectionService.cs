using System.Collections;
using System.Runtime.InteropServices;

namespace System.ComponentModel.Design;

/// <summary>Provides an interface for a designer to select components.</summary>
[ComVisible(true)]
public interface ISelectionService
{
	/// <summary>Gets the object that is currently the primary selected object.</summary>
	/// <returns>The object that is currently the primary selected object.</returns>
	object PrimarySelection { get; }

	/// <summary>Gets the count of selected objects.</summary>
	/// <returns>The number of selected objects.</returns>
	int SelectionCount { get; }

	/// <summary>Occurs when the current selection changes.</summary>
	event EventHandler SelectionChanged;

	/// <summary>Occurs when the current selection is about to change.</summary>
	event EventHandler SelectionChanging;

	/// <summary>Gets a value indicating whether the specified component is currently selected.</summary>
	/// <returns>true if the component is part of the user's current selection; otherwise, false.</returns>
	/// <param name="component">The component to test. </param>
	bool GetComponentSelected(object component);

	/// <summary>Gets a collection of components that are currently selected.</summary>
	/// <returns>A collection that represents the current set of components that are selected.</returns>
	ICollection GetSelectedComponents();

	/// <summary>Selects the specified collection of components.</summary>
	/// <param name="components">The collection of components to select. </param>
	void SetSelectedComponents(ICollection components);

	/// <summary>Selects the components from within the specified collection of components that match the specified selection type.</summary>
	/// <param name="components">The collection of components to select. </param>
	/// <param name="selectionType">A value from the <see cref="T:System.ComponentModel.Design.SelectionTypes" /> enumeration. The default is <see cref="F:System.ComponentModel.Design.SelectionTypes.Normal" />. </param>
	void SetSelectedComponents(ICollection components, SelectionTypes selectionType);
}
