namespace System.ComponentModel.Design;

/// <summary>Provides event notifications when root designers are added and removed, when a selected component changes, and when the current root designer changes.</summary>
public interface IDesignerEventService
{
	/// <summary>Gets the root designer for the currently active document.</summary>
	/// <returns>The currently active document, or null if there is no active document.</returns>
	IDesignerHost ActiveDesigner { get; }

	/// <summary>Gets a collection of root designers for design documents that are currently active in the development environment.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.Design.DesignerCollection" /> containing the root designers that have been created and not yet disposed.</returns>
	DesignerCollection Designers { get; }

	/// <summary>Occurs when the current root designer changes.</summary>
	event ActiveDesignerEventHandler ActiveDesignerChanged;

	/// <summary>Occurs when a root designer is created.</summary>
	event DesignerEventHandler DesignerCreated;

	/// <summary>Occurs when a root designer for a document is disposed.</summary>
	event DesignerEventHandler DesignerDisposed;

	/// <summary>Occurs when the current design-view selection changes.</summary>
	event EventHandler SelectionChanged;
}
