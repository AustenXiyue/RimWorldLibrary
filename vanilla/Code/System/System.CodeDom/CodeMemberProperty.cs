namespace System.CodeDom;

/// <summary>Represents a declaration for a property of a type.</summary>
[Serializable]
public class CodeMemberProperty : CodeTypeMember
{
	private CodeTypeReference _type;

	private bool _hasGet;

	private bool _hasSet;

	private CodeTypeReferenceCollection _implementationTypes;

	/// <summary>Gets or sets the data type of the interface, if any, this property, if private, implements.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the data type of the interface, if any, the property, if private, implements.</returns>
	public CodeTypeReference PrivateImplementationType { get; set; }

	/// <summary>Gets the data types of any interfaces that the property implements.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReferenceCollection" /> that indicates the data types the property implements.</returns>
	public CodeTypeReferenceCollection ImplementationTypes => _implementationTypes ?? (_implementationTypes = new CodeTypeReferenceCollection());

	/// <summary>Gets or sets the data type of the property.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the data type of the property.</returns>
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

	/// <summary>Gets or sets a value indicating whether the property has a get method accessor.</summary>
	/// <returns>true if the Count property of the <see cref="P:System.CodeDom.CodeMemberProperty.GetStatements" /> collection is non-zero, or if the value of this property has been set to true; otherwise, false.</returns>
	public bool HasGet
	{
		get
		{
			if (!_hasGet)
			{
				return GetStatements.Count > 0;
			}
			return true;
		}
		set
		{
			_hasGet = value;
			if (!value)
			{
				GetStatements.Clear();
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether the property has a set method accessor.</summary>
	/// <returns>true if the <see cref="P:System.Collections.CollectionBase.Count" /> property of the <see cref="P:System.CodeDom.CodeMemberProperty.SetStatements" /> collection is non-zero; otherwise, false.</returns>
	public bool HasSet
	{
		get
		{
			if (!_hasSet)
			{
				return SetStatements.Count > 0;
			}
			return true;
		}
		set
		{
			_hasSet = value;
			if (!value)
			{
				SetStatements.Clear();
			}
		}
	}

	/// <summary>Gets the collection of get statements for the property.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeStatementCollection" /> that contains the get statements for the member property.</returns>
	public CodeStatementCollection GetStatements { get; } = new CodeStatementCollection();

	/// <summary>Gets the collection of set statements for the property.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeStatementCollection" /> that contains the set statements for the member property.</returns>
	public CodeStatementCollection SetStatements { get; } = new CodeStatementCollection();

	/// <summary>Gets the collection of declaration expressions for the property.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeParameterDeclarationExpressionCollection" /> that indicates the declaration expressions for the property.</returns>
	public CodeParameterDeclarationExpressionCollection Parameters { get; } = new CodeParameterDeclarationExpressionCollection();

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeMemberProperty" /> class.</summary>
	public CodeMemberProperty()
	{
	}
}
