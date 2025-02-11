namespace System.Xaml;

/// <summary>Provides a service that represents indexed node control for a simple implementation of a node-based XAML reader.</summary>
public interface IXamlIndexingReader
{
	/// <summary>Gets the number of nodes in the current external node set.</summary>
	/// <returns>The number of nodes in the current external node set.</returns>
	int Count { get; }

	/// <summary>Gets or sets the index number of the current reader position for the indexed list view of XAML nodes.</summary>
	/// <returns>The index number of the current reader position.</returns>
	int CurrentIndex { get; set; }
}
