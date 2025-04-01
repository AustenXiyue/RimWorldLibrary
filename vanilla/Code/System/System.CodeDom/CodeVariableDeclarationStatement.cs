namespace System.CodeDom;

/// <summary>Represents a variable declaration.</summary>
[Serializable]
public class CodeVariableDeclarationStatement : CodeStatement
{
	private CodeTypeReference _type;

	private string _name;

	/// <summary>Gets or sets the initialization expression for the variable.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the initialization expression for the variable.</returns>
	public CodeExpression InitExpression { get; set; }

	/// <summary>Gets or sets the name of the variable.</summary>
	/// <returns>The name of the variable.</returns>
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

	/// <summary>Gets or sets the data type of the variable.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the data type of the variable.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeVariableDeclarationStatement" /> class.</summary>
	public CodeVariableDeclarationStatement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeVariableDeclarationStatement" /> class using the specified type and name.</summary>
	/// <param name="type">A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the data type of the variable. </param>
	/// <param name="name">The name of the variable. </param>
	public CodeVariableDeclarationStatement(CodeTypeReference type, string name)
	{
		Type = type;
		Name = name;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeVariableDeclarationStatement" /> class using the specified data type name and variable name.</summary>
	/// <param name="type">The name of the data type of the variable. </param>
	/// <param name="name">The name of the variable. </param>
	public CodeVariableDeclarationStatement(string type, string name)
	{
		Type = new CodeTypeReference(type);
		Name = name;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeVariableDeclarationStatement" /> class using the specified data type and variable name.</summary>
	/// <param name="type">The data type for the variable. </param>
	/// <param name="name">The name of the variable. </param>
	public CodeVariableDeclarationStatement(Type type, string name)
	{
		Type = new CodeTypeReference(type);
		Name = name;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeVariableDeclarationStatement" /> class using the specified data type, variable name, and initialization expression.</summary>
	/// <param name="type">A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the type of the variable. </param>
	/// <param name="name">The name of the variable. </param>
	/// <param name="initExpression">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the initialization expression for the variable. </param>
	public CodeVariableDeclarationStatement(CodeTypeReference type, string name, CodeExpression initExpression)
	{
		Type = type;
		Name = name;
		InitExpression = initExpression;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeVariableDeclarationStatement" /> class using the specified data type, variable name, and initialization expression.</summary>
	/// <param name="type">The name of the data type of the variable. </param>
	/// <param name="name">The name of the variable. </param>
	/// <param name="initExpression">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the initialization expression for the variable. </param>
	public CodeVariableDeclarationStatement(string type, string name, CodeExpression initExpression)
	{
		Type = new CodeTypeReference(type);
		Name = name;
		InitExpression = initExpression;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeVariableDeclarationStatement" /> class using the specified data type, variable name, and initialization expression.</summary>
	/// <param name="type">The data type of the variable. </param>
	/// <param name="name">The name of the variable. </param>
	/// <param name="initExpression">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the initialization expression for the variable. </param>
	public CodeVariableDeclarationStatement(Type type, string name, CodeExpression initExpression)
	{
		Type = new CodeTypeReference(type);
		Name = name;
		InitExpression = initExpression;
	}
}
