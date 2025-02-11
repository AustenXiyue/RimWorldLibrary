namespace System.CodeDom;

/// <summary>Represents a literal code fragment that can be compiled.</summary>
[Serializable]
public class CodeSnippetCompileUnit : CodeCompileUnit
{
	private string _value;

	/// <summary>Gets or sets the literal code fragment to represent.</summary>
	/// <returns>The literal code fragment.</returns>
	public string Value
	{
		get
		{
			return _value ?? string.Empty;
		}
		set
		{
			_value = value;
		}
	}

	/// <summary>Gets or sets the line and file information about where the code is located in a source code document.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeLinePragma" /> that indicates the position of the code fragment.</returns>
	public CodeLinePragma LinePragma { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeSnippetCompileUnit" /> class. </summary>
	public CodeSnippetCompileUnit()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeSnippetCompileUnit" /> class.</summary>
	/// <param name="value">The literal code fragment to represent. </param>
	public CodeSnippetCompileUnit(string value)
	{
		Value = value;
	}
}
