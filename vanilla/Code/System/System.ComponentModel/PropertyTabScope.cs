namespace System.ComponentModel;

/// <summary>Defines identifiers that indicate the persistence scope of a tab in the Properties window.</summary>
public enum PropertyTabScope
{
	/// <summary>This tab is added to the Properties window and cannot be removed.</summary>
	Static,
	/// <summary>This tab is added to the Properties window and can only be removed explicitly by a parent component.</summary>
	Global,
	/// <summary>This tab is specific to the current document. This tab is added to the Properties window and is removed when the currently selected document changes.</summary>
	Document,
	/// <summary>This tab is specific to the current component. This tab is added to the Properties window for the current component only and is removed when the component is no longer selected.</summary>
	Component
}
