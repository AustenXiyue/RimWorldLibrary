namespace System.CodeDom;

/// <summary>Represents the abstract base class from which all code statements derive.</summary>
[Serializable]
public class CodeStatement : CodeObject
{
	private CodeDirectiveCollection _startDirectives;

	private CodeDirectiveCollection _endDirectives;

	/// <summary>Gets or sets the line on which the code statement occurs. </summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeLinePragma" /> object that indicates the context of the code statement.</returns>
	public CodeLinePragma LinePragma { get; set; }

	/// <summary>Gets a <see cref="T:System.CodeDom.CodeDirectiveCollection" /> object that contains start directives.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeDirectiveCollection" /> object containing start directives.</returns>
	public CodeDirectiveCollection StartDirectives => _startDirectives ?? (_startDirectives = new CodeDirectiveCollection());

	/// <summary>Gets a <see cref="T:System.CodeDom.CodeDirectiveCollection" /> object that contains end directives.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeDirectiveCollection" /> object containing end directives.</returns>
	public CodeDirectiveCollection EndDirectives => _endDirectives ?? (_endDirectives = new CodeDirectiveCollection());

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeStatement" /> class. </summary>
	public CodeStatement()
	{
	}
}
