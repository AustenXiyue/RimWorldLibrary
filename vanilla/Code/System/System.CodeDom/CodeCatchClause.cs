namespace System.CodeDom;

/// <summary>Represents a catch exception block of a try/catch statement.</summary>
[Serializable]
public class CodeCatchClause
{
	private CodeStatementCollection _statements;

	private CodeTypeReference _catchExceptionType;

	private string _localName;

	/// <summary>Gets or sets the variable name of the exception that the catch clause handles.</summary>
	/// <returns>The name for the exception variable that the catch clause handles.</returns>
	public string LocalName
	{
		get
		{
			return _localName ?? string.Empty;
		}
		set
		{
			_localName = value;
		}
	}

	/// <summary>Gets or sets the type of the exception to handle with the catch block.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the type of the exception to handle.</returns>
	public CodeTypeReference CatchExceptionType
	{
		get
		{
			return _catchExceptionType ?? (_catchExceptionType = new CodeTypeReference(typeof(Exception)));
		}
		set
		{
			_catchExceptionType = value;
		}
	}

	/// <summary>Gets the statements within the catch block.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeStatementCollection" /> containing the statements within the catch block.</returns>
	public CodeStatementCollection Statements => _statements ?? (_statements = new CodeStatementCollection());

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCatchClause" /> class.</summary>
	public CodeCatchClause()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCatchClause" /> class using the specified local variable name for the exception.</summary>
	/// <param name="localName">The name of the local variable declared in the catch clause for the exception. This is optional. </param>
	public CodeCatchClause(string localName)
	{
		_localName = localName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCatchClause" /> class using the specified local variable name for the exception and exception type.</summary>
	/// <param name="localName">The name of the local variable declared in the catch clause for the exception. This is optional. </param>
	/// <param name="catchExceptionType">A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the type of exception to catch. </param>
	public CodeCatchClause(string localName, CodeTypeReference catchExceptionType)
	{
		_localName = localName;
		_catchExceptionType = catchExceptionType;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCatchClause" /> class using the specified local variable name for the exception, exception type and statement collection.</summary>
	/// <param name="localName">The name of the local variable declared in the catch clause for the exception. This is optional. </param>
	/// <param name="catchExceptionType">A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the type of exception to catch. </param>
	/// <param name="statements">An array of <see cref="T:System.CodeDom.CodeStatement" /> objects that represent the contents of the catch block. </param>
	public CodeCatchClause(string localName, CodeTypeReference catchExceptionType, params CodeStatement[] statements)
	{
		_localName = localName;
		_catchExceptionType = catchExceptionType;
		Statements.AddRange(statements);
	}
}
