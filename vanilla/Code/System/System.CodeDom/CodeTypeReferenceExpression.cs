namespace System.CodeDom;

/// <summary>Represents a reference to a data type.</summary>
[Serializable]
public class CodeTypeReferenceExpression : CodeExpression
{
	private CodeTypeReference _type;

	/// <summary>Gets or sets the data type to reference.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the data type to reference.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeReferenceExpression" /> class.</summary>
	public CodeTypeReferenceExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeReferenceExpression" /> class using the specified type.</summary>
	/// <param name="type">A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the data type to reference. </param>
	public CodeTypeReferenceExpression(CodeTypeReference type)
	{
		Type = type;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeReferenceExpression" /> class using the specified data type name.</summary>
	/// <param name="type">The name of the data type to reference. </param>
	public CodeTypeReferenceExpression(string type)
	{
		Type = new CodeTypeReference(type);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeReferenceExpression" /> class using the specified data type.</summary>
	/// <param name="type">An instance of the data type to reference. </param>
	public CodeTypeReferenceExpression(Type type)
	{
		Type = new CodeTypeReference(type);
	}
}
