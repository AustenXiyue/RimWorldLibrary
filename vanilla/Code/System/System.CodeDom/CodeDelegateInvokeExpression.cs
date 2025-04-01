namespace System.CodeDom;

/// <summary>Represents an expression that raises an event.</summary>
[Serializable]
public class CodeDelegateInvokeExpression : CodeExpression
{
	/// <summary>Gets or sets the event to invoke.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the event to invoke.</returns>
	public CodeExpression TargetObject { get; set; }

	/// <summary>Gets or sets the parameters to pass to the event handling methods attached to the event.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the parameters to pass to the event handling methods attached to the event.</returns>
	public CodeExpressionCollection Parameters { get; } = new CodeExpressionCollection();

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeDelegateInvokeExpression" /> class.</summary>
	public CodeDelegateInvokeExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeDelegateInvokeExpression" /> class using the specified target object.</summary>
	/// <param name="targetObject">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the target object. </param>
	public CodeDelegateInvokeExpression(CodeExpression targetObject)
	{
		TargetObject = targetObject;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeDelegateInvokeExpression" /> class using the specified target object and parameters.</summary>
	/// <param name="targetObject">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the target object. </param>
	/// <param name="parameters">An array of <see cref="T:System.CodeDom.CodeExpression" /> objects that indicate the parameters. </param>
	public CodeDelegateInvokeExpression(CodeExpression targetObject, params CodeExpression[] parameters)
	{
		TargetObject = targetObject;
		Parameters.AddRange(parameters);
	}
}
