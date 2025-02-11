namespace System.CodeDom;

/// <summary>Represents a typeof expression, an expression that returns a <see cref="T:System.Type" /> for a specified type name.</summary>
[Serializable]
public class CodeTypeOfExpression : CodeExpression
{
	private CodeTypeReference _type;

	/// <summary>Gets or sets the data type referenced by the typeof expression.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the data type referenced by the typeof expression. This property will never return null, and defaults to the <see cref="T:System.Void" /> type.</returns>
	public CodeTypeReference Type
	{
		get
		{
			return _type ?? (_type = new CodeTypeReference(""));
		}
		set
		{
			_type = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeOfExpression" /> class.</summary>
	public CodeTypeOfExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeOfExpression" /> class.</summary>
	/// <param name="type">A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the data type for the typeof expression. </param>
	public CodeTypeOfExpression(CodeTypeReference type)
	{
		Type = type;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeOfExpression" /> class using the specified type.</summary>
	/// <param name="type">The name of the data type for the typeof expression. </param>
	public CodeTypeOfExpression(string type)
	{
		Type = new CodeTypeReference(type);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeOfExpression" /> class using the specified type.</summary>
	/// <param name="type">The data type of the data type of the typeof expression. </param>
	public CodeTypeOfExpression(Type type)
	{
		Type = new CodeTypeReference(type);
	}
}
