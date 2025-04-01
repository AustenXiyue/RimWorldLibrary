namespace System.CodeDom;

/// <summary>Represents a conditional branch statement, typically represented as an if statement.</summary>
[Serializable]
public class CodeConditionStatement : CodeStatement
{
	/// <summary>Gets or sets the expression to evaluate true or false.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> to evaluate true or false.</returns>
	public CodeExpression Condition { get; set; }

	/// <summary>Gets the collection of statements to execute if the conditional expression evaluates to true.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeStatementCollection" /> containing the statements to execute if the conditional expression evaluates to true.</returns>
	public CodeStatementCollection TrueStatements { get; } = new CodeStatementCollection();

	/// <summary>Gets the collection of statements to execute if the conditional expression evaluates to false.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeStatementCollection" /> containing the statements to execute if the conditional expression evaluates to false.</returns>
	public CodeStatementCollection FalseStatements { get; } = new CodeStatementCollection();

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeConditionStatement" /> class.</summary>
	public CodeConditionStatement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeConditionStatement" /> class using the specified condition and statements.</summary>
	/// <param name="condition">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the expression to evaluate. </param>
	/// <param name="trueStatements">An array of type <see cref="T:System.CodeDom.CodeStatement" /> containing the statements to execute if the condition is true. </param>
	public CodeConditionStatement(CodeExpression condition, params CodeStatement[] trueStatements)
	{
		Condition = condition;
		TrueStatements.AddRange(trueStatements);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeConditionStatement" /> class using the specified condition and statements.</summary>
	/// <param name="condition">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the condition to evaluate. </param>
	/// <param name="trueStatements">An array of type <see cref="T:System.CodeDom.CodeStatement" /> containing the statements to execute if the condition is true. </param>
	/// <param name="falseStatements">An array of type <see cref="T:System.CodeDom.CodeStatement" /> containing the statements to execute if the condition is false. </param>
	public CodeConditionStatement(CodeExpression condition, CodeStatement[] trueStatements, CodeStatement[] falseStatements)
	{
		Condition = condition;
		TrueStatements.AddRange(trueStatements);
		FalseStatements.AddRange(falseStatements);
	}
}
