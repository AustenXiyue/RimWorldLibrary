namespace System.CodeDom;

/// <summary>Represents a reference to a method.</summary>
[Serializable]
public class CodeMethodReferenceExpression : CodeExpression
{
	private string _methodName;

	private CodeTypeReferenceCollection _typeArguments;

	/// <summary>Gets or sets the expression that indicates the method to reference.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that represents the method to reference.</returns>
	public CodeExpression TargetObject { get; set; }

	/// <summary>Gets or sets the name of the method to reference.</summary>
	/// <returns>The name of the method to reference.</returns>
	public string MethodName
	{
		get
		{
			return _methodName ?? string.Empty;
		}
		set
		{
			_methodName = value;
		}
	}

	/// <summary>Gets the type arguments for the current generic method reference expression.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReferenceCollection" /> containing the type arguments for the current code <see cref="T:System.CodeDom.CodeMethodReferenceExpression" />.</returns>
	public CodeTypeReferenceCollection TypeArguments => _typeArguments ?? (_typeArguments = new CodeTypeReferenceCollection());

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeMethodReferenceExpression" /> class.</summary>
	public CodeMethodReferenceExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeMethodReferenceExpression" /> class using the specified target object and method name.</summary>
	/// <param name="targetObject">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the object to target. </param>
	/// <param name="methodName">The name of the method to call. </param>
	public CodeMethodReferenceExpression(CodeExpression targetObject, string methodName)
	{
		TargetObject = targetObject;
		MethodName = methodName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeMethodReferenceExpression" /> class using the specified target object, method name, and generic type arguments.</summary>
	/// <param name="targetObject">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the object to target. </param>
	/// <param name="methodName">The name of the method to call. </param>
	/// <param name="typeParameters">An array of <see cref="T:System.CodeDom.CodeTypeReference" /> values that specify the <see cref="P:System.CodeDom.CodeMethodReferenceExpression.TypeArguments" /> for this <see cref="T:System.CodeDom.CodeMethodReferenceExpression" />.</param>
	public CodeMethodReferenceExpression(CodeExpression targetObject, string methodName, params CodeTypeReference[] typeParameters)
	{
		TargetObject = targetObject;
		MethodName = methodName;
		if (typeParameters != null && typeParameters.Length != 0)
		{
			TypeArguments.AddRange(typeParameters);
		}
	}
}
