namespace System.CodeDom;

/// <summary>Represents a parameter declaration for a method, property, or constructor.</summary>
[Serializable]
public class CodeParameterDeclarationExpression : CodeExpression
{
	private CodeTypeReference _type;

	private string _name;

	private CodeAttributeDeclarationCollection _customAttributes;

	/// <summary>Gets or sets the custom attributes for the parameter declaration.</summary>
	/// <returns>An object that indicates the custom attributes.</returns>
	public CodeAttributeDeclarationCollection CustomAttributes
	{
		get
		{
			return _customAttributes ?? (_customAttributes = new CodeAttributeDeclarationCollection());
		}
		set
		{
			_customAttributes = value;
		}
	}

	/// <summary>Gets or sets the direction of the field.</summary>
	/// <returns>An object that indicates the direction of the field.</returns>
	public FieldDirection Direction { get; set; }

	/// <summary>Gets or sets the type of the parameter.</summary>
	/// <returns>The type of the parameter.</returns>
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

	/// <summary>Gets or sets the name of the parameter.</summary>
	/// <returns>The name of the parameter.</returns>
	public string Name
	{
		get
		{
			return _name ?? string.Empty;
		}
		set
		{
			_name = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeParameterDeclarationExpression" /> class.</summary>
	public CodeParameterDeclarationExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeParameterDeclarationExpression" /> class using the specified parameter type and name.</summary>
	/// <param name="type">An object that indicates the type of the parameter to declare. </param>
	/// <param name="name">The name of the parameter to declare. </param>
	public CodeParameterDeclarationExpression(CodeTypeReference type, string name)
	{
		Type = type;
		Name = name;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeParameterDeclarationExpression" /> class using the specified parameter type and name.</summary>
	/// <param name="type">The type of the parameter to declare. </param>
	/// <param name="name">The name of the parameter to declare. </param>
	public CodeParameterDeclarationExpression(string type, string name)
	{
		Type = new CodeTypeReference(type);
		Name = name;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeParameterDeclarationExpression" /> class using the specified parameter type and name.</summary>
	/// <param name="type">The type of the parameter to declare. </param>
	/// <param name="name">The name of the parameter to declare. </param>
	public CodeParameterDeclarationExpression(Type type, string name)
	{
		Type = new CodeTypeReference(type);
		Name = name;
	}
}
