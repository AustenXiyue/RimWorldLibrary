namespace System.CodeDom;

/// <summary>Represents an argument used in a metadata attribute declaration.</summary>
[Serializable]
public class CodeAttributeArgument
{
	private string _name;

	/// <summary>Gets or sets the name of the attribute.</summary>
	/// <returns>The name of the attribute property the argument is for.</returns>
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

	/// <summary>Gets or sets the value for the attribute argument.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the value for the attribute argument.</returns>
	public CodeExpression Value { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeAttributeArgument" /> class.</summary>
	public CodeAttributeArgument()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeAttributeArgument" /> class using the specified value.</summary>
	/// <param name="value">A <see cref="T:System.CodeDom.CodeExpression" /> that represents the value of the argument. </param>
	public CodeAttributeArgument(CodeExpression value)
	{
		Value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeAttributeArgument" /> class using the specified name and value.</summary>
	/// <param name="name">The name of the attribute property the argument applies to. </param>
	/// <param name="value">A <see cref="T:System.CodeDom.CodeExpression" /> that represents the value of the argument. </param>
	public CodeAttributeArgument(string name, CodeExpression value)
	{
		Name = name;
		Value = value;
	}
}
