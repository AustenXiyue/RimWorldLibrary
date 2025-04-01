namespace System.CodeDom;

/// <summary>Represents a namespace declaration.</summary>
[Serializable]
public class CodeNamespace : CodeObject
{
	private string _name;

	private readonly CodeNamespaceImportCollection _imports = new CodeNamespaceImportCollection();

	private readonly CodeCommentStatementCollection _comments = new CodeCommentStatementCollection();

	private readonly CodeTypeDeclarationCollection _classes = new CodeTypeDeclarationCollection();

	private int _populated;

	private const int ImportsCollection = 1;

	private const int CommentsCollection = 2;

	private const int TypesCollection = 4;

	/// <summary>Gets the collection of types that the namespace contains.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeDeclarationCollection" /> that indicates the types contained in the namespace.</returns>
	public CodeTypeDeclarationCollection Types
	{
		get
		{
			if ((_populated & 4) == 0)
			{
				_populated |= 4;
				this.PopulateTypes?.Invoke(this, EventArgs.Empty);
			}
			return _classes;
		}
	}

	/// <summary>Gets the collection of namespace import directives used by the namespace.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeNamespaceImportCollection" /> that indicates the namespace import directives used by the namespace.</returns>
	public CodeNamespaceImportCollection Imports
	{
		get
		{
			if ((_populated & 1) == 0)
			{
				_populated |= 1;
				this.PopulateImports?.Invoke(this, EventArgs.Empty);
			}
			return _imports;
		}
	}

	/// <summary>Gets or sets the name of the namespace.</summary>
	/// <returns>The name of the namespace.</returns>
	public string Name
	{
		get
		{
			return _name ?? string.Empty;
		}
		set
		{
			_name = value;
		}
	}

	/// <summary>Gets the comments for the namespace.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeCommentStatementCollection" /> that indicates the comments for the namespace.</returns>
	public CodeCommentStatementCollection Comments
	{
		get
		{
			if ((_populated & 2) == 0)
			{
				_populated |= 2;
				this.PopulateComments?.Invoke(this, EventArgs.Empty);
			}
			return _comments;
		}
	}

	/// <summary>An event that will be raised the first time the <see cref="P:System.CodeDom.CodeNamespace.Comments" /> collection is accessed.</summary>
	public event EventHandler PopulateComments;

	/// <summary>An event that will be raised the first time the <see cref="P:System.CodeDom.CodeNamespace.Imports" /> collection is accessed.</summary>
	public event EventHandler PopulateImports;

	/// <summary>An event that will be raised the first time the <see cref="P:System.CodeDom.CodeNamespace.Types" /> collection is accessed.</summary>
	public event EventHandler PopulateTypes;

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeNamespace" /> class.</summary>
	public CodeNamespace()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeNamespace" /> class using the specified name.</summary>
	/// <param name="name">The name of the namespace being declared. </param>
	public CodeNamespace(string name)
	{
		Name = name;
	}
}
