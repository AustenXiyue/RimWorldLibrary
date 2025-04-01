namespace System.CodeDom;

/// <summary>Represents a simple assignment statement.</summary>
[Serializable]
public class CodeAssignStatement : CodeStatement
{
	/// <summary>Gets or sets the expression representing the object or reference to assign to.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the object or reference to assign to.</returns>
	public CodeExpression Left { get; set; }

	/// <summary>Gets or sets the expression representing the object or reference to assign.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the object or reference to assign.</returns>
	public CodeExpression Right { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeAssignStatement" /> class.</summary>
	public CodeAssignStatement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeAssignStatement" /> class using the specified expressions.</summary>
	/// <param name="left">The variable to assign to. </param>
	/// <param name="right">The value to assign. </param>
	public CodeAssignStatement(CodeExpression left, CodeExpression right)
	{
		Left = left;
		Right = right;
	}
}
