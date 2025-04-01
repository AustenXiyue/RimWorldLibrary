namespace System.CodeDom;

/// <summary>Represents a statement that consists of a single expression.</summary>
[Serializable]
public class CodeExpressionStatement : CodeStatement
{
	/// <summary>Gets or sets the expression for the statement.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the expression for the statement.</returns>
	public CodeExpression Expression { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeExpressionStatement" /> class.</summary>
	public CodeExpressionStatement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeExpressionStatement" /> class by using the specified expression.</summary>
	/// <param name="expression">A <see cref="T:System.CodeDom.CodeExpression" /> for the statement. </param>
	public CodeExpressionStatement(CodeExpression expression)
	{
		Expression = expression;
	}
}
