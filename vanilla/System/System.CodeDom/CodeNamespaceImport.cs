namespace System.CodeDom;

/// <summary>Represents a namespace import directive that indicates a namespace to use.</summary>
[Serializable]
public class CodeNamespaceImport : CodeObject
{
	private string _nameSpace;

	/// <summary>Gets or sets the line and file the statement occurs on.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeLinePragma" /> that indicates the context of the statement.</returns>
	public CodeLinePragma LinePragma { get; set; }

	/// <summary>Gets or sets the namespace to import.</summary>
	/// <returns>The name of the namespace to import.</returns>
	public string Namespace
	{
		get
		{
			return _nameSpace ?? string.Empty;
		}
		set
		{
			_nameSpace = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeNamespaceImport" /> class.</summary>
	public CodeNamespaceImport()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeNamespaceImport" /> class using the specified namespace to import.</summary>
	/// <param name="nameSpace">The name of the namespace to import. </param>
	public CodeNamespaceImport(string nameSpace)
	{
		Namespace = nameSpace;
	}
}
