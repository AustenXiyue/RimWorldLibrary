namespace System.CodeDom;

/// <summary>Represents a type parameter of a generic type or method.</summary>
[Serializable]
public class CodeTypeParameter : CodeObject
{
	private string _name;

	private CodeAttributeDeclarationCollection _customAttributes;

	private CodeTypeReferenceCollection _constraints;

	/// <summary>Gets or sets the name of the type parameter.</summary>
	/// <returns>The name of the type parameter. The default is an empty string ("").</returns>
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

	/// <summary>Gets the constraints for the type parameter.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReferenceCollection" /> object that contains the constraints for the type parameter.</returns>
	public CodeTypeReferenceCollection Constraints => _constraints ?? (_constraints = new CodeTypeReferenceCollection());

	/// <summary>Gets the custom attributes of the type parameter.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeAttributeDeclarationCollection" /> that indicates the custom attributes of the type parameter. The default is null.</returns>
	public CodeAttributeDeclarationCollection CustomAttributes => _customAttributes ?? (_customAttributes = new CodeAttributeDeclarationCollection());

	/// <summary>Gets or sets a value indicating whether the type parameter has a constructor constraint.</summary>
	/// <returns>true if the type parameter has a constructor constraint; otherwise, false. The default is false.</returns>
	public bool HasConstructorConstraint { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeParameter" /> class. </summary>
	public CodeTypeParameter()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeParameter" /> class with the specified type parameter name. </summary>
	/// <param name="name">The name of the type parameter.</param>
	public CodeTypeParameter(string name)
	{
		_name = name;
	}
}
