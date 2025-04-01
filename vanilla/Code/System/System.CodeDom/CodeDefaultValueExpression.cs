namespace System.CodeDom;

/// <summary>Represents a reference to a default value.</summary>
[Serializable]
public class CodeDefaultValueExpression : CodeExpression
{
	private CodeTypeReference _type;

	/// <summary>Gets or sets the data type reference for a default value.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReference" /> object representing a data type that has a default value.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeDefaultValueExpression" /> class. </summary>
	public CodeDefaultValueExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeDefaultValueExpression" /> class using the specified code type reference.</summary>
	/// <param name="type">A <see cref="T:System.CodeDom.CodeTypeReference" /> that specifies the reference to a value type.</param>
	public CodeDefaultValueExpression(CodeTypeReference type)
	{
		_type = type;
	}
}
