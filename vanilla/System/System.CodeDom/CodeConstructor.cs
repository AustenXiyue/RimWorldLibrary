namespace System.CodeDom;

/// <summary>Represents a declaration for an instance constructor of a type.</summary>
[Serializable]
public class CodeConstructor : CodeMemberMethod
{
	/// <summary>Gets the collection of base constructor arguments.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpressionCollection" /> that contains the base constructor arguments.</returns>
	public CodeExpressionCollection BaseConstructorArgs { get; } = new CodeExpressionCollection();

	/// <summary>Gets the collection of chained constructor arguments.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpressionCollection" /> that contains the chained constructor arguments.</returns>
	public CodeExpressionCollection ChainedConstructorArgs { get; } = new CodeExpressionCollection();

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeConstructor" /> class.</summary>
	public CodeConstructor()
	{
		base.Name = ".ctor";
	}
}
