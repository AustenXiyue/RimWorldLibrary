namespace System.CodeDom;

/// <summary>Represents a literal expression.</summary>
[Serializable]
public class CodeSnippetExpression : CodeExpression
{
	private string _value;

	/// <summary>Gets or sets the literal string of code.</summary>
	/// <returns>The literal string.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeSnippetExpression" /> class.</summary>
	public CodeSnippetExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeSnippetExpression" /> class using the specified literal expression.</summary>
	/// <param name="value">The literal expression to represent. </param>
	public CodeSnippetExpression(string value)
	{
		Value = value;
	}
}
