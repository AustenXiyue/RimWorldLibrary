namespace System.CodeDom;

/// <summary>Represents an expression that consists of a binary operation between two expressions.</summary>
[Serializable]
public class CodeBinaryOperatorExpression : CodeExpression
{
	/// <summary>Gets or sets the code expression on the right of the operator.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the right operand.</returns>
	public CodeExpression Right { get; set; }

	/// <summary>Gets or sets the code expression on the left of the operator.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the left operand.</returns>
	public CodeExpression Left { get; set; }

	/// <summary>Gets or sets the operator in the binary operator expression.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeBinaryOperatorType" /> that indicates the type of operator in the expression.</returns>
	public CodeBinaryOperatorType Operator { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeBinaryOperatorExpression" /> class.</summary>
	public CodeBinaryOperatorExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeBinaryOperatorExpression" /> class using the specified parameters.</summary>
	/// <param name="left">The <see cref="T:System.CodeDom.CodeExpression" /> on the left of the operator. </param>
	/// <param name="op">A <see cref="T:System.CodeDom.CodeBinaryOperatorType" /> indicating the type of operator. </param>
	/// <param name="right">The <see cref="T:System.CodeDom.CodeExpression" /> on the right of the operator. </param>
	public CodeBinaryOperatorExpression(CodeExpression left, CodeBinaryOperatorType op, CodeExpression right)
	{
		Right = right;
		Operator = op;
		Left = left;
	}
}
