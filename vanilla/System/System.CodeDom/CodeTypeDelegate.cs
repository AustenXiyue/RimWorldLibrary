using System.Reflection;

namespace System.CodeDom;

/// <summary>Represents a delegate declaration.</summary>
[Serializable]
public class CodeTypeDelegate : CodeTypeDeclaration
{
	private CodeTypeReference _returnType;

	/// <summary>Gets or sets the return type of the delegate.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the return type of the delegate.</returns>
	public CodeTypeReference ReturnType
	{
		get
		{
			return _returnType ?? (_returnType = new CodeTypeReference(""));
		}
		set
		{
			_returnType = value;
		}
	}

	/// <summary>Gets the parameters of the delegate.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeParameterDeclarationExpressionCollection" /> that indicates the parameters of the delegate.</returns>
	public CodeParameterDeclarationExpressionCollection Parameters { get; } = new CodeParameterDeclarationExpressionCollection();

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeDelegate" /> class.</summary>
	public CodeTypeDelegate()
	{
		base.TypeAttributes &= ~TypeAttributes.ClassSemanticsMask;
		base.TypeAttributes |= TypeAttributes.NotPublic;
		base.BaseTypes.Clear();
		base.BaseTypes.Add(new CodeTypeReference("System.Delegate"));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeTypeDelegate" /> class.</summary>
	/// <param name="name">The name of the delegate. </param>
	public CodeTypeDelegate(string name)
		: this()
	{
		base.Name = name;
	}
}
