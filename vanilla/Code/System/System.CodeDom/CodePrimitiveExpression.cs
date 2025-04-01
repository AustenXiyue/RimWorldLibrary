namespace System.CodeDom;

/// <summary>Represents a primitive data type value.</summary>
[Serializable]
public class CodePrimitiveExpression : CodeExpression
{
	/// <summary>Gets or sets the primitive data type to represent.</summary>
	/// <returns>The primitive data type instance to represent the value of.</returns>
	public object Value { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodePrimitiveExpression" /> class.</summary>
	public CodePrimitiveExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodePrimitiveExpression" /> class using the specified object.</summary>
	/// <param name="value">The object to represent. </param>
	public CodePrimitiveExpression(object value)
	{
		Value = value;
	}
}
