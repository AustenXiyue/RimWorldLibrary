namespace System.Windows.Documents;

/// <summary>Represents a collection of <see cref="T:System.Windows.Documents.ListItem" /> elements. <see cref="T:System.Windows.Documents.ListItemCollection" /> defines the allowable child content of a <see cref="T:System.Windows.Documents.List" /> element.</summary>
public class ListItemCollection : TextElementCollection<ListItem>
{
	/// <summary>Gets the first <see cref="T:System.Windows.Documents.ListItem" /> element within this instance of <see cref="T:System.Windows.Documents.ListItemCollection" />.</summary>
	/// <returns>The first <see cref="T:System.Windows.Documents.ListItem" /> element within this instance of <see cref="T:System.Windows.Documents.ListItemCollection" />.</returns>
	public ListItem FirstListItem => base.FirstChild;

	/// <summary>Gets the last <see cref="T:System.Windows.Documents.ListItem" /> element within this instance of <see cref="T:System.Windows.Documents.ListItemCollection" />.</summary>
	/// <returns>The last <see cref="T:System.Windows.Documents.ListItem" /> element within this instance of <see cref="T:System.Windows.Documents.ListItemCollection" />.</returns>
	public ListItem LastListItem => base.LastChild;

	internal ListItemCollection(DependencyObject owner, bool isOwnerParent)
		: base(owner, isOwnerParent)
	{
	}
}
