namespace System.Windows.Documents.DocumentStructures;

/// <summary>Identifies an element within the hierarchy of elements under a <see cref="T:System.Windows.Documents.FixedPage" />.</summary>
public class NamedElement : BlockElement
{
	private string _reference;

	/// <summary>Gets or sets the name of the element in the <see cref="T:System.Windows.Documents.FixedPage" /> markup hierarchy that provides the content for the parent of the <see cref="T:System.Windows.Documents.DocumentStructures.NamedElement" />. </summary>
	/// <returns>The name of the element.</returns>
	public string NameReference
	{
		get
		{
			return _reference;
		}
		set
		{
			_reference = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.DocumentStructures.NamedElement" /> class.</summary>
	public NamedElement()
	{
	}
}
