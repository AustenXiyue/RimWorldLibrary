namespace System.Windows.Documents;

/// <summary>Defines the source object that performs actual content pagination.</summary>
public interface IDocumentPaginatorSource
{
	/// <summary>When implemented in a derived class, gets the object that performs content pagination.</summary>
	/// <returns>The object that performs the actual content pagination.</returns>
	DocumentPaginator DocumentPaginator { get; }
}
