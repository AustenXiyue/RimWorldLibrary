namespace System.CodeDom;

/// <summary>Represents an expression that creates an array.</summary>
[Serializable]
public class CodeArrayCreateExpression : CodeExpression
{
	private readonly CodeExpressionCollection _initializers = new CodeExpressionCollection();

	private CodeTypeReference _createType;

	/// <summary>Gets or sets the type of array to create.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the type of the array.</returns>
	public CodeTypeReference CreateType
	{
		get
		{
			return _createType ?? (_createType = new CodeTypeReference(""));
		}
		set
		{
			_createType = value;
		}
	}

	/// <summary>Gets the initializers with which to initialize the array.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpressionCollection" /> that indicates the initialization values.</returns>
	public CodeExpressionCollection Initializers => _initializers;

	/// <summary>Gets or sets the number of indexes in the array.</summary>
	/// <returns>The number of indexes in the array.</returns>
	public int Size { get; set; }

	/// <summary>Gets or sets the expression that indicates the size of the array.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the size of the array.</returns>
	public CodeExpression SizeExpression { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeArrayCreateExpression" /> class.</summary>
	public CodeArrayCreateExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeArrayCreateExpression" /> class using the specified array data type and initialization expressions.</summary>
	/// <param name="createType">A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the data type of the array to create. </param>
	/// <param name="initializers">An array of expressions to use to initialize the array. </param>
	public CodeArrayCreateExpression(CodeTypeReference createType, params CodeExpression[] initializers)
	{
		_createType = createType;
		_initializers.AddRange(initializers);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeArrayCreateExpression" /> class using the specified array data type name and initializers.</summary>
	/// <param name="createType">The name of the data type of the array to create. </param>
	/// <param name="initializers">An array of expressions to use to initialize the array. </param>
	public CodeArrayCreateExpression(string createType, params CodeExpression[] initializers)
	{
		_createType = new CodeTypeReference(createType);
		_initializers.AddRange(initializers);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeArrayCreateExpression" /> class using the specified array data type and initializers.</summary>
	/// <param name="createType">The data type of the array to create. </param>
	/// <param name="initializers">An array of expressions to use to initialize the array. </param>
	public CodeArrayCreateExpression(Type createType, params CodeExpression[] initializers)
	{
		_createType = new CodeTypeReference(createType);
		_initializers.AddRange(initializers);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeArrayCreateExpression" /> class using the specified array data type and number of indexes for the array.</summary>
	/// <param name="createType">A <see cref="T:System.CodeDom.CodeTypeReference" /> indicating the data type of the array to create. </param>
	/// <param name="size">The number of indexes of the array to create. </param>
	public CodeArrayCreateExpression(CodeTypeReference createType, int size)
	{
		_createType = createType;
		Size = size;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeArrayCreateExpression" /> class using the specified array data type name and number of indexes for the array.</summary>
	/// <param name="createType">The name of the data type of the array to create. </param>
	/// <param name="size">The number of indexes of the array to create. </param>
	public CodeArrayCreateExpression(string createType, int size)
	{
		_createType = new CodeTypeReference(createType);
		Size = size;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeArrayCreateExpression" /> class using the specified array data type and number of indexes for the array.</summary>
	/// <param name="createType">The data type of the array to create. </param>
	/// <param name="size">The number of indexes of the array to create. </param>
	public CodeArrayCreateExpression(Type createType, int size)
	{
		_createType = new CodeTypeReference(createType);
		Size = size;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeArrayCreateExpression" /> class using the specified array data type and code expression indicating the number of indexes for the array.</summary>
	/// <param name="createType">A <see cref="T:System.CodeDom.CodeTypeReference" /> indicating the data type of the array to create. </param>
	/// <param name="size">An expression that indicates the number of indexes of the array to create. </param>
	public CodeArrayCreateExpression(CodeTypeReference createType, CodeExpression size)
	{
		_createType = createType;
		SizeExpression = size;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeArrayCreateExpression" /> class using the specified array data type name and code expression indicating the number of indexes for the array.</summary>
	/// <param name="createType">The name of the data type of the array to create. </param>
	/// <param name="size">An expression that indicates the number of indexes of the array to create. </param>
	public CodeArrayCreateExpression(string createType, CodeExpression size)
	{
		_createType = new CodeTypeReference(createType);
		SizeExpression = size;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeArrayCreateExpression" /> class using the specified array data type and code expression indicating the number of indexes for the array.</summary>
	/// <param name="createType">The data type of the array to create. </param>
	/// <param name="size">An expression that indicates the number of indexes of the array to create. </param>
	public CodeArrayCreateExpression(Type createType, CodeExpression size)
	{
		_createType = new CodeTypeReference(createType);
		SizeExpression = size;
	}
}
