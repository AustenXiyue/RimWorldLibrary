namespace System.Windows.Documents;

/// <summary>Represents a collection of <see cref="T:System.Windows.Documents.Block" /> elements. <see cref="T:System.Windows.Documents.BlockCollection" /> defines the allowable child content of the <see cref="T:System.Windows.Documents.FlowDocument" />, <see cref="T:System.Windows.Documents.Section" />, <see cref="T:System.Windows.Documents.ListItem" />, <see cref="T:System.Windows.Documents.TableCell" />, <see cref="T:System.Windows.Documents.Floater" />, and <see cref="T:System.Windows.Documents.Figure" /> elements.</summary>
public class BlockCollection : TextElementCollection<Block>
{
	/// <summary>Gets the first <see cref="T:System.Windows.Documents.Block" /> element within this instance of <see cref="T:System.Windows.Documents.BlockCollection" />.</summary>
	/// <returns>The first <see cref="T:System.Windows.Documents.Block" /> element in the <see cref="T:System.Windows.Documents.BlockCollection" />.</returns>
	public Block FirstBlock => base.FirstChild;

	/// <summary>Gets the last <see cref="T:System.Windows.Documents.Block" /> element within this instance of <see cref="T:System.Windows.Documents.BlockCollection" />.</summary>
	/// <returns>The last <see cref="T:System.Windows.Documents.Block" /> element in the <see cref="T:System.Windows.Documents.BlockCollection" />.</returns>
	public Block LastBlock => base.LastChild;

	internal BlockCollection(DependencyObject owner, bool isOwnerParent)
		: base(owner, isOwnerParent)
	{
	}
}
