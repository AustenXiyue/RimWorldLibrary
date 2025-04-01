namespace System.CodeDom;

/// <summary>Represents a try block with any number of catch clauses and, optionally, a finally block.</summary>
[Serializable]
public class CodeTryCatchFinallyStatement : CodeStatement
{
	/// <summary>Gets the statements to try.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeStatementCollection" /> that indicates the statements to try.</returns>
	public CodeStatementCollection TryStatements { get; } = new CodeStatementCollection();

	/// <summary>Gets the catch clauses to use.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeCatchClauseCollection" /> that indicates the catch clauses to use.</returns>
	public CodeCatchClauseCollection CatchClauses { get; } = new CodeCatchClauseCollection();

	/// <summary>Gets the finally statements to use.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeStatementCollection" /> that indicates the finally statements.</returns>
	public CodeStatementCollection FinallyStatements { get; } = new CodeStatementCollection();

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTryCatchFinallyStatement" /> class.</summary>
	public CodeTryCatchFinallyStatement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTryCatchFinallyStatement" /> class using the specified statements for try and catch clauses.</summary>
	/// <param name="tryStatements">An array of <see cref="T:System.CodeDom.CodeStatement" /> objects that indicate the statements to try. </param>
	/// <param name="catchClauses">An array of <see cref="T:System.CodeDom.CodeCatchClause" /> objects that indicate the clauses to catch. </param>
	public CodeTryCatchFinallyStatement(CodeStatement[] tryStatements, CodeCatchClause[] catchClauses)
	{
		TryStatements.AddRange(tryStatements);
		CatchClauses.AddRange(catchClauses);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTryCatchFinallyStatement" /> class using the specified statements for try, catch clauses, and finally statements.</summary>
	/// <param name="tryStatements">An array of <see cref="T:System.CodeDom.CodeStatement" /> objects that indicate the statements to try. </param>
	/// <param name="catchClauses">An array of <see cref="T:System.CodeDom.CodeCatchClause" /> objects that indicate the clauses to catch. </param>
	/// <param name="finallyStatements">An array of <see cref="T:System.CodeDom.CodeStatement" /> objects that indicate the finally statements to use. </param>
	public CodeTryCatchFinallyStatement(CodeStatement[] tryStatements, CodeCatchClause[] catchClauses, CodeStatement[] finallyStatements)
	{
		TryStatements.AddRange(tryStatements);
		CatchClauses.AddRange(catchClauses);
		FinallyStatements.AddRange(finallyStatements);
	}
}
