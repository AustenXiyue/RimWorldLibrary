namespace System.CodeDom;

/// <summary>Provides a base class for a member of a type. Type members include fields, methods, properties, constructors and nested types.</summary>
[Serializable]
public class CodeTypeMember : CodeObject
{
	private string _name;

	private CodeAttributeDeclarationCollection _customAttributes;

	private CodeDirectiveCollection _startDirectives;

	private CodeDirectiveCollection _endDirectives;

	/// <summary>Gets or sets the name of the member.</summary>
	/// <returns>The name of the member.</returns>
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

	/// <summary>Gets or sets the attributes of the member.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.CodeDom.MemberAttributes" /> values used to indicate the attributes of the member. The default value is <see cref="F:System.CodeDom.MemberAttributes.Private" /> | <see cref="F:System.CodeDom.MemberAttributes.Final" />. </returns>
	public MemberAttributes Attributes { get; set; } = (MemberAttributes)20482;

	/// <summary>Gets or sets the custom attributes of the member.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeAttributeDeclarationCollection" /> that indicates the custom attributes of the member.</returns>
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

	/// <summary>Gets or sets the line on which the type member statement occurs.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeLinePragma" /> object that indicates the location of the type member declaration.</returns>
	public CodeLinePragma LinePragma { get; set; }

	/// <summary>Gets the collection of comments for the type member.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeCommentStatementCollection" /> that indicates the comments for the member.</returns>
	public CodeCommentStatementCollection Comments { get; } = new CodeCommentStatementCollection();

	/// <summary>Gets the start directives for the member.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeDirectiveCollection" /> object containing start directives.</returns>
	public CodeDirectiveCollection StartDirectives => _startDirectives ?? (_startDirectives = new CodeDirectiveCollection());

	/// <summary>Gets the end directives for the member.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeDirectiveCollection" /> object containing end directives.</returns>
	public CodeDirectiveCollection EndDirectives => _endDirectives ?? (_endDirectives = new CodeDirectiveCollection());

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeMember" /> class. </summary>
	public CodeTypeMember()
	{
	}
}
