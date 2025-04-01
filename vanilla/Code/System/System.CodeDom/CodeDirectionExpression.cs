namespace System.CodeDom;

/// <summary>Represents an expression used as a method invoke parameter along with a reference direction indicator.</summary>
[Serializable]
public class CodeDirectionExpression : CodeExpression
{
	/// <summary>Gets or sets the code expression to represent.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the expression to represent.</returns>
	public CodeExpression Expression { get; set; }

	/// <summary>Gets or sets the field direction for this direction expression.</summary>
	/// <returns>A <see cref="T:System.CodeDom.FieldDirection" /> that indicates the field direction for this direction expression.</returns>
	public FieldDirection Direction { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeDirectionExpression" /> class.</summary>
	public CodeDirectionExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeDirectionExpression" /> class using the specified field direction and expression.</summary>
	/// <param name="direction">A <see cref="T:System.CodeDom.FieldDirection" /> that indicates the field direction of the expression. </param>
	/// <param name="expression">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the code expression to represent. </param>
	public CodeDirectionExpression(FieldDirection direction, CodeExpression expression)
	{
		Expression = expression;
		Direction = direction;
	}
}
