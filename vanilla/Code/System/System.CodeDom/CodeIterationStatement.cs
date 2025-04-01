namespace System.CodeDom;

/// <summary>Represents a for statement, or a loop through a block of statements, using a test expression as a condition for continuing to loop.</summary>
[Serializable]
public class CodeIterationStatement : CodeStatement
{
	/// <summary>Gets or sets the loop initialization statement.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeStatement" /> that indicates the loop initialization statement.</returns>
	public CodeStatement InitStatement { get; set; }

	/// <summary>Gets or sets the expression to test as the condition that continues the loop.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the expression to test.</returns>
	public CodeExpression TestExpression { get; set; }

	/// <summary>Gets or sets the statement that is called after each loop cycle.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeStatement" /> that indicates the per cycle increment statement.</returns>
	public CodeStatement IncrementStatement { get; set; }

	/// <summary>Gets the collection of statements to be executed within the loop.</summary>
	/// <returns>An array of type <see cref="T:System.CodeDom.CodeStatement" /> that indicates the statements within the loop.</returns>
	public CodeStatementCollection Statements { get; } = new CodeStatementCollection();

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeIterationStatement" /> class.</summary>
	public CodeIterationStatement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeIterationStatement" /> class using the specified parameters.</summary>
	/// <param name="initStatement">A <see cref="T:System.CodeDom.CodeStatement" /> containing the loop initialization statement. </param>
	/// <param name="testExpression">A <see cref="T:System.CodeDom.CodeExpression" /> containing the expression to test for exit condition. </param>
	/// <param name="incrementStatement">A <see cref="T:System.CodeDom.CodeStatement" /> containing the per-cycle increment statement. </param>
	/// <param name="statements">An array of type <see cref="T:System.CodeDom.CodeStatement" /> containing the statements within the loop. </param>
	public CodeIterationStatement(CodeStatement initStatement, CodeExpression testExpression, CodeStatement incrementStatement, params CodeStatement[] statements)
	{
		InitStatement = initStatement;
		TestExpression = testExpression;
		IncrementStatement = incrementStatement;
		Statements.AddRange(statements);
	}
}
