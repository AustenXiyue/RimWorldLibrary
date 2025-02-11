namespace System.Windows.Documents;

/// <summary>Represents an element on a page that can be linked to from other documents or other places in the same document.</summary>
public sealed class LinkTarget
{
	private string _name;

	/// <summary>Gets or sets the name of the element that this <see cref="T:System.Windows.Documents.LinkTarget" /> identifies as a linkable element.</summary>
	/// <returns>A <see cref="T:System.String" /> that is identical to the <see cref="P:System.Windows.FrameworkElement.Name" /> property of the markup element that corresponds to this <see cref="T:System.Windows.Documents.LinkTarget" /> element.</returns>
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.LinkTarget" /> class. </summary>
	public LinkTarget()
	{
	}
}
