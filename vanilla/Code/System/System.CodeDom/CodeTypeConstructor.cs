namespace System.CodeDom;

/// <summary>Represents a static constructor for a class.</summary>
[Serializable]
public class CodeTypeConstructor : CodeMemberMethod
{
	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeConstructor" /> class.</summary>
	public CodeTypeConstructor()
	{
		base.Name = ".cctor";
	}
}
