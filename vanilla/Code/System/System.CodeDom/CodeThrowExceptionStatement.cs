namespace System.CodeDom;

/// <summary>Represents a statement that throws an exception.</summary>
[Serializable]
public class CodeThrowExceptionStatement : CodeStatement
{
	/// <summary>Gets or sets the exception to throw.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> representing an instance of the exception to throw.</returns>
	public CodeExpression ToThrow { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeThrowExceptionStatement" /> class.</summary>
	public CodeThrowExceptionStatement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeThrowExceptionStatement" /> class with the specified exception type instance.</summary>
	/// <param name="toThrow">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the exception to throw. </param>
	public CodeThrowExceptionStatement(CodeExpression toThrow)
	{
		ToThrow = toThrow;
	}
}
