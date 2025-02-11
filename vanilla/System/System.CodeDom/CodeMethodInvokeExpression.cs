namespace System.CodeDom;

/// <summary>Represents an expression that invokes a method.</summary>
[Serializable]
public class CodeMethodInvokeExpression : CodeExpression
{
	private CodeMethodReferenceExpression _method;

	/// <summary>Gets or sets the method to invoke.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeMethodReferenceExpression" /> that indicates the method to invoke.</returns>
	public CodeMethodReferenceExpression Method
	{
		get
		{
			return _method ?? (_method = new CodeMethodReferenceExpression());
		}
		set
		{
			_method = value;
		}
	}

	/// <summary>Gets the parameters to invoke the method with.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpressionCollection" /> that indicates the parameters to invoke the method with.</returns>
	public CodeExpressionCollection Parameters { get; } = new CodeExpressionCollection();

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeMethodInvokeExpression" /> class.</summary>
	public CodeMethodInvokeExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeMethodInvokeExpression" /> class using the specified method and parameters.</summary>
	/// <param name="method">A <see cref="T:System.CodeDom.CodeMethodReferenceExpression" /> that indicates the method to invoke. </param>
	/// <param name="parameters">An array of <see cref="T:System.CodeDom.CodeExpression" /> objects that indicate the parameters with which to invoke the method. </param>
	public CodeMethodInvokeExpression(CodeMethodReferenceExpression method, params CodeExpression[] parameters)
	{
		_method = method;
		Parameters.AddRange(parameters);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeMethodInvokeExpression" /> class using the specified target object, method name, and parameters.</summary>
	/// <param name="targetObject">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the target object with the method to invoke. </param>
	/// <param name="methodName">The name of the method to invoke. </param>
	/// <param name="parameters">An array of <see cref="T:System.CodeDom.CodeExpression" /> objects that indicate the parameters to call the method with. </param>
	public CodeMethodInvokeExpression(CodeExpression targetObject, string methodName, params CodeExpression[] parameters)
	{
		_method = new CodeMethodReferenceExpression(targetObject, methodName);
		Parameters.AddRange(parameters);
	}
}
