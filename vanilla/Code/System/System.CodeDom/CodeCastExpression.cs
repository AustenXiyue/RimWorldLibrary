namespace System.CodeDom;

/// <summary>Represents an expression cast to a data type or interface.</summary>
[Serializable]
public class CodeCastExpression : CodeExpression
{
	private CodeTypeReference _targetType;

	/// <summary>Gets or sets the destination type of the cast.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the destination type to cast to.</returns>
	public CodeTypeReference TargetType
	{
		get
		{
			return _targetType ?? (_targetType = new CodeTypeReference(""));
		}
		set
		{
			_targetType = value;
		}
	}

	/// <summary>Gets or sets the expression to cast.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the code to cast.</returns>
	public CodeExpression Expression { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCastExpression" /> class.</summary>
	public CodeCastExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCastExpression" /> class using the specified destination type and expression.</summary>
	/// <param name="targetType">A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the destination type of the cast. </param>
	/// <param name="expression">The <see cref="T:System.CodeDom.CodeExpression" /> to cast. </param>
	public CodeCastExpression(CodeTypeReference targetType, CodeExpression expression)
	{
		TargetType = targetType;
		Expression = expression;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCastExpression" /> class using the specified destination type and expression.</summary>
	/// <param name="targetType">The name of the destination type of the cast. </param>
	/// <param name="expression">The <see cref="T:System.CodeDom.CodeExpression" /> to cast. </param>
	public CodeCastExpression(string targetType, CodeExpression expression)
	{
		TargetType = new CodeTypeReference(targetType);
		Expression = expression;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCastExpression" /> class using the specified destination type and expression.</summary>
	/// <param name="targetType">The destination data type of the cast. </param>
	/// <param name="expression">The <see cref="T:System.CodeDom.CodeExpression" /> to cast. </param>
	public CodeCastExpression(Type targetType, CodeExpression expression)
	{
		TargetType = new CodeTypeReference(targetType);
		Expression = expression;
	}
}
