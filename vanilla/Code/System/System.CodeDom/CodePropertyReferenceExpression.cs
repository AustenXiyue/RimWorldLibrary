namespace System.CodeDom;

/// <summary>Represents a reference to the value of a property.</summary>
[Serializable]
public class CodePropertyReferenceExpression : CodeExpression
{
	private string _propertyName;

	/// <summary>Gets or sets the object that contains the property to reference.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the object that contains the property to reference.</returns>
	public CodeExpression TargetObject { get; set; }

	/// <summary>Gets or sets the name of the property to reference.</summary>
	/// <returns>The name of the property to reference.</returns>
	public string PropertyName
	{
		get
		{
			return _propertyName ?? string.Empty;
		}
		set
		{
			_propertyName = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodePropertyReferenceExpression" /> class.</summary>
	public CodePropertyReferenceExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodePropertyReferenceExpression" /> class using the specified target object and property name.</summary>
	/// <param name="targetObject">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the object that contains the property to reference. </param>
	/// <param name="propertyName">The name of the property to reference. </param>
	public CodePropertyReferenceExpression(CodeExpression targetObject, string propertyName)
	{
		TargetObject = targetObject;
		PropertyName = propertyName;
	}
}
