namespace System.CodeDom;

/// <summary>Represents a return value statement.</summary>
[Serializable]
public class CodeMethodReturnStatement : CodeStatement
{
	/// <summary>Gets or sets the return value.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the value to return for the return statement, or null if the statement is part of a subroutine.</returns>
	public CodeExpression Expression { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeMethodReturnStatement" /> class.</summary>
	public CodeMethodReturnStatement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeMethodReturnStatement" /> class using the specified expression.</summary>
	/// <param name="expression">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the return value. </param>
	public CodeMethodReturnStatement(CodeExpression expression)
	{
		Expression = expression;
	}
}
