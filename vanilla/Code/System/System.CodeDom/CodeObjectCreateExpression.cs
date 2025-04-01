namespace System.CodeDom;

/// <summary>Represents an expression that creates a new instance of a type.</summary>
[Serializable]
public class CodeObjectCreateExpression : CodeExpression
{
	private CodeTypeReference _createType;

	/// <summary>Gets or sets the data type of the object to create.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReference" /> to the data type of the object to create.</returns>
	public CodeTypeReference CreateType
	{
		get
		{
			return _createType ?? (_createType = new CodeTypeReference(""));
		}
		set
		{
			_createType = value;
		}
	}

	/// <summary>Gets or sets the parameters to use in creating the object.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpressionCollection" /> that indicates the parameters to use when creating the object.</returns>
	public CodeExpressionCollection Parameters { get; } = new CodeExpressionCollection();

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeObjectCreateExpression" /> class.</summary>
	public CodeObjectCreateExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeObjectCreateExpression" /> class using the specified type and parameters.</summary>
	/// <param name="createType">A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the data type of the object to create. </param>
	/// <param name="parameters">An array of <see cref="T:System.CodeDom.CodeExpression" /> objects that indicates the parameters to use to create the object. </param>
	public CodeObjectCreateExpression(CodeTypeReference createType, params CodeExpression[] parameters)
	{
		CreateType = createType;
		Parameters.AddRange(parameters);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeObjectCreateExpression" /> class using the specified type and parameters.</summary>
	/// <param name="createType">The name of the data type of object to create. </param>
	/// <param name="parameters">An array of <see cref="T:System.CodeDom.CodeExpression" /> objects that indicates the parameters to use to create the object. </param>
	public CodeObjectCreateExpression(string createType, params CodeExpression[] parameters)
	{
		CreateType = new CodeTypeReference(createType);
		Parameters.AddRange(parameters);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeObjectCreateExpression" /> class using the specified type and parameters.</summary>
	/// <param name="createType">The data type of the object to create. </param>
	/// <param name="parameters">An array of <see cref="T:System.CodeDom.CodeExpression" /> objects that indicates the parameters to use to create the object. </param>
	public CodeObjectCreateExpression(Type createType, params CodeExpression[] parameters)
	{
		CreateType = new CodeTypeReference(createType);
		Parameters.AddRange(parameters);
	}
}
