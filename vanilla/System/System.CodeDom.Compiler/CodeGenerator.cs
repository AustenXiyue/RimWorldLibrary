using System.Globalization;
using System.IO;
using System.Reflection;

namespace System.CodeDom.Compiler;

/// <summary>Provides an example implementation of the <see cref="T:System.CodeDom.Compiler.ICodeGenerator" /> interface. This class is abstract.</summary>
public abstract class CodeGenerator : ICodeGenerator
{
	private const int ParameterMultilineThreshold = 15;

	private ExposedTabStringIndentedTextWriter _output;

	private CodeGeneratorOptions _options;

	private CodeTypeDeclaration _currentClass;

	private CodeTypeMember _currentMember;

	private bool _inNestedBinary;

	/// <summary>Gets the code type declaration for the current class.</summary>
	/// <returns>The code type declaration for the current class.</returns>
	protected CodeTypeDeclaration CurrentClass => _currentClass;

	/// <summary>Gets the current class name.</summary>
	/// <returns>The current class name.</returns>
	protected string CurrentTypeName
	{
		get
		{
			if (_currentClass == null)
			{
				return "<% unknown %>";
			}
			return _currentClass.Name;
		}
	}

	/// <summary>Gets the current member of the class.</summary>
	/// <returns>The current member of the class.</returns>
	protected CodeTypeMember CurrentMember => _currentMember;

	/// <summary>Gets the current member name.</summary>
	/// <returns>The name of the current member.</returns>
	protected string CurrentMemberName
	{
		get
		{
			if (_currentMember == null)
			{
				return "<% unknown %>";
			}
			return _currentMember.Name;
		}
	}

	/// <summary>Gets a value indicating whether the current object being generated is an interface.</summary>
	/// <returns>true if the current object is an interface; otherwise, false.</returns>
	protected bool IsCurrentInterface
	{
		get
		{
			if (_currentClass == null || _currentClass is CodeTypeDelegate)
			{
				return false;
			}
			return _currentClass.IsInterface;
		}
	}

	/// <summary>Gets a value indicating whether the current object being generated is a class.</summary>
	/// <returns>true if the current object is a class; otherwise, false.</returns>
	protected bool IsCurrentClass
	{
		get
		{
			if (_currentClass == null || _currentClass is CodeTypeDelegate)
			{
				return false;
			}
			return _currentClass.IsClass;
		}
	}

	/// <summary>Gets a value indicating whether the current object being generated is a value type or struct.</summary>
	/// <returns>true if the current object is a value type or struct; otherwise, false.</returns>
	protected bool IsCurrentStruct
	{
		get
		{
			if (_currentClass == null || _currentClass is CodeTypeDelegate)
			{
				return false;
			}
			return _currentClass.IsStruct;
		}
	}

	/// <summary>Gets a value indicating whether the current object being generated is an enumeration.</summary>
	/// <returns>true if the current object is an enumeration; otherwise, false.</returns>
	protected bool IsCurrentEnum
	{
		get
		{
			if (_currentClass == null || _currentClass is CodeTypeDelegate)
			{
				return false;
			}
			return _currentClass.IsEnum;
		}
	}

	/// <summary>Gets a value indicating whether the current object being generated is a delegate.</summary>
	/// <returns>true if the current object is a delegate; otherwise, false.</returns>
	protected bool IsCurrentDelegate
	{
		get
		{
			if (_currentClass != null)
			{
				return _currentClass is CodeTypeDelegate;
			}
			return false;
		}
	}

	/// <summary>Gets or sets the amount of spaces to indent each indentation level.</summary>
	/// <returns>The number of spaces to indent for each indentation level.</returns>
	protected int Indent
	{
		get
		{
			return _output.Indent;
		}
		set
		{
			_output.Indent = value;
		}
	}

	/// <summary>Gets the token that represents null.</summary>
	/// <returns>The token that represents null.</returns>
	protected abstract string NullToken { get; }

	/// <summary>Gets the text writer to use for output.</summary>
	/// <returns>The text writer to use for output.</returns>
	protected TextWriter Output => _output;

	/// <summary>Gets the options to be used by the code generator.</summary>
	/// <returns>An object that indicates the options for the code generator to use.</returns>
	protected CodeGeneratorOptions Options => _options;

	private void GenerateType(CodeTypeDeclaration e)
	{
		_currentClass = e;
		if (e.StartDirectives.Count > 0)
		{
			GenerateDirectives(e.StartDirectives);
		}
		GenerateCommentStatements(e.Comments);
		if (e.LinePragma != null)
		{
			GenerateLinePragmaStart(e.LinePragma);
		}
		GenerateTypeStart(e);
		if (Options.VerbatimOrder)
		{
			foreach (CodeTypeMember member in e.Members)
			{
				GenerateTypeMember(member, e);
			}
		}
		else
		{
			GenerateFields(e);
			GenerateSnippetMembers(e);
			GenerateTypeConstructors(e);
			GenerateConstructors(e);
			GenerateProperties(e);
			GenerateEvents(e);
			GenerateMethods(e);
			GenerateNestedTypes(e);
		}
		_currentClass = e;
		GenerateTypeEnd(e);
		if (e.LinePragma != null)
		{
			GenerateLinePragmaEnd(e.LinePragma);
		}
		if (e.EndDirectives.Count > 0)
		{
			GenerateDirectives(e.EndDirectives);
		}
	}

	/// <summary>Generates code for the specified code directives.</summary>
	/// <param name="directives">The code directives to generate code for.</param>
	protected virtual void GenerateDirectives(CodeDirectiveCollection directives)
	{
	}

	private void GenerateTypeMember(CodeTypeMember member, CodeTypeDeclaration declaredType)
	{
		if (_options.BlankLinesBetweenMembers)
		{
			Output.WriteLine();
		}
		if (member is CodeTypeDeclaration)
		{
			((ICodeGenerator)this).GenerateCodeFromType((CodeTypeDeclaration)member, _output.InnerWriter, _options);
			_currentClass = declaredType;
			return;
		}
		if (member.StartDirectives.Count > 0)
		{
			GenerateDirectives(member.StartDirectives);
		}
		GenerateCommentStatements(member.Comments);
		if (member.LinePragma != null)
		{
			GenerateLinePragmaStart(member.LinePragma);
		}
		if (member is CodeMemberField)
		{
			GenerateField((CodeMemberField)member);
		}
		else if (member is CodeMemberProperty)
		{
			GenerateProperty((CodeMemberProperty)member, declaredType);
		}
		else if (member is CodeMemberMethod)
		{
			if (member is CodeConstructor)
			{
				GenerateConstructor((CodeConstructor)member, declaredType);
			}
			else if (member is CodeTypeConstructor)
			{
				GenerateTypeConstructor((CodeTypeConstructor)member);
			}
			else if (member is CodeEntryPointMethod)
			{
				GenerateEntryPointMethod((CodeEntryPointMethod)member, declaredType);
			}
			else
			{
				GenerateMethod((CodeMemberMethod)member, declaredType);
			}
		}
		else if (member is CodeMemberEvent)
		{
			GenerateEvent((CodeMemberEvent)member, declaredType);
		}
		else if (member is CodeSnippetTypeMember)
		{
			int indent = Indent;
			Indent = 0;
			GenerateSnippetMember((CodeSnippetTypeMember)member);
			Indent = indent;
			Output.WriteLine();
		}
		if (member.LinePragma != null)
		{
			GenerateLinePragmaEnd(member.LinePragma);
		}
		if (member.EndDirectives.Count > 0)
		{
			GenerateDirectives(member.EndDirectives);
		}
	}

	private void GenerateTypeConstructors(CodeTypeDeclaration e)
	{
		foreach (CodeTypeMember member in e.Members)
		{
			if (member is CodeTypeConstructor)
			{
				_currentMember = member;
				if (_options.BlankLinesBetweenMembers)
				{
					Output.WriteLine();
				}
				if (_currentMember.StartDirectives.Count > 0)
				{
					GenerateDirectives(_currentMember.StartDirectives);
				}
				GenerateCommentStatements(_currentMember.Comments);
				CodeTypeConstructor codeTypeConstructor = (CodeTypeConstructor)member;
				if (codeTypeConstructor.LinePragma != null)
				{
					GenerateLinePragmaStart(codeTypeConstructor.LinePragma);
				}
				GenerateTypeConstructor(codeTypeConstructor);
				if (codeTypeConstructor.LinePragma != null)
				{
					GenerateLinePragmaEnd(codeTypeConstructor.LinePragma);
				}
				if (_currentMember.EndDirectives.Count > 0)
				{
					GenerateDirectives(_currentMember.EndDirectives);
				}
			}
		}
	}

	/// <summary>Generates code for the namespaces in the specified compile unit.</summary>
	/// <param name="e">The compile unit to generate namespaces for. </param>
	protected void GenerateNamespaces(CodeCompileUnit e)
	{
		foreach (CodeNamespace @namespace in e.Namespaces)
		{
			((ICodeGenerator)this).GenerateCodeFromNamespace(@namespace, _output.InnerWriter, _options);
		}
	}

	/// <summary>Generates code for the specified namespace and the classes it contains.</summary>
	/// <param name="e">The namespace to generate classes for. </param>
	protected void GenerateTypes(CodeNamespace e)
	{
		foreach (CodeTypeDeclaration type in e.Types)
		{
			if (_options.BlankLinesBetweenMembers)
			{
				Output.WriteLine();
			}
			((ICodeGenerator)this).GenerateCodeFromType(type, _output.InnerWriter, _options);
		}
	}

	/// <summary>Gets a value indicating whether the generator provides support for the language features represented by the specified <see cref="T:System.CodeDom.Compiler.GeneratorSupport" />  object.</summary>
	/// <returns>true if the specified capabilities are supported; otherwise, false.</returns>
	/// <param name="support">The capabilities to test the generator for.</param>
	bool ICodeGenerator.Supports(GeneratorSupport support)
	{
		return Supports(support);
	}

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) type declaration and outputs it to the specified text writer using the specified options.</summary>
	/// <param name="e">The type to generate code for.</param>
	/// <param name="w">The text writer to output code to.</param>
	/// <param name="o">The options to use for generating code.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="w" /> is not available. <paramref name="w" /> may have been closed before the method call was made.</exception>
	void ICodeGenerator.GenerateCodeFromType(CodeTypeDeclaration e, TextWriter w, CodeGeneratorOptions o)
	{
		bool flag = false;
		if (_output != null && w != _output.InnerWriter)
		{
			throw new InvalidOperationException("The output writer for code generation and the writer supplied don't match and cannot be used. This is generally caused by a bad implementation of a CodeGenerator derived class.");
		}
		if (_output == null)
		{
			flag = true;
			_options = o ?? new CodeGeneratorOptions();
			_output = new ExposedTabStringIndentedTextWriter(w, _options.IndentString);
		}
		try
		{
			GenerateType(e);
		}
		finally
		{
			if (flag)
			{
				_output = null;
				_options = null;
			}
		}
	}

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) expression and outputs it to the specified text writer.</summary>
	/// <param name="e">The expression to generate code for.</param>
	/// <param name="w">The text writer to output code to.</param>
	/// <param name="o">The options to use for generating code.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="w" /> is not available. <paramref name="w" /> may have been closed before the method call was made.</exception>
	void ICodeGenerator.GenerateCodeFromExpression(CodeExpression e, TextWriter w, CodeGeneratorOptions o)
	{
		bool flag = false;
		if (_output != null && w != _output.InnerWriter)
		{
			throw new InvalidOperationException("The output writer for code generation and the writer supplied don't match and cannot be used. This is generally caused by a bad implementation of a CodeGenerator derived class.");
		}
		if (_output == null)
		{
			flag = true;
			_options = o ?? new CodeGeneratorOptions();
			_output = new ExposedTabStringIndentedTextWriter(w, _options.IndentString);
		}
		try
		{
			GenerateExpression(e);
		}
		finally
		{
			if (flag)
			{
				_output = null;
				_options = null;
			}
		}
	}

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) compilation unit and outputs it to the specified text writer using the specified options.</summary>
	/// <param name="e">The CodeDOM compilation unit to generate code for.</param>
	/// <param name="w">The text writer to output code to.</param>
	/// <param name="o">The options to use for generating code.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="w" /> is not available. <paramref name="w" /> may have been closed before the method call was made.</exception>
	void ICodeGenerator.GenerateCodeFromCompileUnit(CodeCompileUnit e, TextWriter w, CodeGeneratorOptions o)
	{
		bool flag = false;
		if (_output != null && w != _output.InnerWriter)
		{
			throw new InvalidOperationException("The output writer for code generation and the writer supplied don't match and cannot be used. This is generally caused by a bad implementation of a CodeGenerator derived class.");
		}
		if (_output == null)
		{
			flag = true;
			_options = o ?? new CodeGeneratorOptions();
			_output = new ExposedTabStringIndentedTextWriter(w, _options.IndentString);
		}
		try
		{
			if (e is CodeSnippetCompileUnit)
			{
				GenerateSnippetCompileUnit((CodeSnippetCompileUnit)e);
			}
			else
			{
				GenerateCompileUnit(e);
			}
		}
		finally
		{
			if (flag)
			{
				_output = null;
				_options = null;
			}
		}
	}

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) namespace and outputs it to the specified text writer using the specified options.</summary>
	/// <param name="e">The namespace to generate code for.</param>
	/// <param name="w">The text writer to output code to.</param>
	/// <param name="o">The options to use for generating code.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="w" /> is not available. <paramref name="w" /> may have been closed before the method call was made.</exception>
	void ICodeGenerator.GenerateCodeFromNamespace(CodeNamespace e, TextWriter w, CodeGeneratorOptions o)
	{
		bool flag = false;
		if (_output != null && w != _output.InnerWriter)
		{
			throw new InvalidOperationException("The output writer for code generation and the writer supplied don't match and cannot be used. This is generally caused by a bad implementation of a CodeGenerator derived class.");
		}
		if (_output == null)
		{
			flag = true;
			_options = o ?? new CodeGeneratorOptions();
			_output = new ExposedTabStringIndentedTextWriter(w, _options.IndentString);
		}
		try
		{
			GenerateNamespace(e);
		}
		finally
		{
			if (flag)
			{
				_output = null;
				_options = null;
			}
		}
	}

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) statement and outputs it to the specified text writer using the specified options.</summary>
	/// <param name="e">The statement that contains the CodeDOM elements to translate.</param>
	/// <param name="w">The text writer to output code to.</param>
	/// <param name="o">The options to use for generating code.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="w" /> is not available. <paramref name="w" /> may have been closed before the method call was made.</exception>
	void ICodeGenerator.GenerateCodeFromStatement(CodeStatement e, TextWriter w, CodeGeneratorOptions o)
	{
		bool flag = false;
		if (_output != null && w != _output.InnerWriter)
		{
			throw new InvalidOperationException("The output writer for code generation and the writer supplied don't match and cannot be used. This is generally caused by a bad implementation of a CodeGenerator derived class.");
		}
		if (_output == null)
		{
			flag = true;
			_options = o ?? new CodeGeneratorOptions();
			_output = new ExposedTabStringIndentedTextWriter(w, _options.IndentString);
		}
		try
		{
			GenerateStatement(e);
		}
		finally
		{
			if (flag)
			{
				_output = null;
				_options = null;
			}
		}
	}

	/// <summary>Generates code for the specified class member using the specified text writer and code generator options.</summary>
	/// <param name="member">The class member to generate code for.</param>
	/// <param name="writer">The text writer to output code to.</param>
	/// <param name="options">The options to use when generating the code. </param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.CodeDom.Compiler.CodeGenerator.Output" /> property is not null.</exception>
	public virtual void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
	{
		if (_output != null)
		{
			throw new InvalidOperationException("This code generation API cannot be called while the generator is being used to generate something else.");
		}
		_options = options ?? new CodeGeneratorOptions();
		_output = new ExposedTabStringIndentedTextWriter(writer, _options.IndentString);
		try
		{
			GenerateTypeMember(member, _currentClass = new CodeTypeDeclaration());
		}
		finally
		{
			_currentClass = null;
			_output = null;
			_options = null;
		}
	}

	/// <summary>Gets a value that indicates whether the specified value is a valid identifier for the current language.</summary>
	/// <returns>true if the <paramref name="value" /> parameter is a valid identifier; otherwise, false.</returns>
	/// <param name="value">The value to test.</param>
	bool ICodeGenerator.IsValidIdentifier(string value)
	{
		return IsValidIdentifier(value);
	}

	/// <summary>Throws an exception if the specified value is not a valid identifier.</summary>
	/// <param name="value">The identifier to validate.</param>
	void ICodeGenerator.ValidateIdentifier(string value)
	{
		ValidateIdentifier(value);
	}

	/// <summary>Creates an escaped identifier for the specified value.</summary>
	/// <returns>The escaped identifier for the value.</returns>
	/// <param name="value">The string to create an escaped identifier for.</param>
	string ICodeGenerator.CreateEscapedIdentifier(string value)
	{
		return CreateEscapedIdentifier(value);
	}

	/// <summary>Creates a valid identifier for the specified value.</summary>
	/// <returns>A valid identifier for the specified value.</returns>
	/// <param name="value">The string to generate a valid identifier for.</param>
	string ICodeGenerator.CreateValidIdentifier(string value)
	{
		return CreateValidIdentifier(value);
	}

	/// <summary>Gets the type indicated by the specified <see cref="T:System.CodeDom.CodeTypeReference" />.</summary>
	/// <returns>The name of the data type reference.</returns>
	/// <param name="type">The type to return.</param>
	string ICodeGenerator.GetTypeOutput(CodeTypeReference type)
	{
		return GetTypeOutput(type);
	}

	private void GenerateConstructors(CodeTypeDeclaration e)
	{
		foreach (CodeTypeMember member in e.Members)
		{
			if (member is CodeConstructor)
			{
				_currentMember = member;
				if (_options.BlankLinesBetweenMembers)
				{
					Output.WriteLine();
				}
				if (_currentMember.StartDirectives.Count > 0)
				{
					GenerateDirectives(_currentMember.StartDirectives);
				}
				GenerateCommentStatements(_currentMember.Comments);
				CodeConstructor codeConstructor = (CodeConstructor)member;
				if (codeConstructor.LinePragma != null)
				{
					GenerateLinePragmaStart(codeConstructor.LinePragma);
				}
				GenerateConstructor(codeConstructor, e);
				if (codeConstructor.LinePragma != null)
				{
					GenerateLinePragmaEnd(codeConstructor.LinePragma);
				}
				if (_currentMember.EndDirectives.Count > 0)
				{
					GenerateDirectives(_currentMember.EndDirectives);
				}
			}
		}
	}

	private void GenerateEvents(CodeTypeDeclaration e)
	{
		foreach (CodeTypeMember member in e.Members)
		{
			if (member is CodeMemberEvent)
			{
				_currentMember = member;
				if (_options.BlankLinesBetweenMembers)
				{
					Output.WriteLine();
				}
				if (_currentMember.StartDirectives.Count > 0)
				{
					GenerateDirectives(_currentMember.StartDirectives);
				}
				GenerateCommentStatements(_currentMember.Comments);
				CodeMemberEvent codeMemberEvent = (CodeMemberEvent)member;
				if (codeMemberEvent.LinePragma != null)
				{
					GenerateLinePragmaStart(codeMemberEvent.LinePragma);
				}
				GenerateEvent(codeMemberEvent, e);
				if (codeMemberEvent.LinePragma != null)
				{
					GenerateLinePragmaEnd(codeMemberEvent.LinePragma);
				}
				if (_currentMember.EndDirectives.Count > 0)
				{
					GenerateDirectives(_currentMember.EndDirectives);
				}
			}
		}
	}

	/// <summary>Generates code for the specified code expression.</summary>
	/// <param name="e">The code expression to generate code for. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="e" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="e" /> is not a valid <see cref="T:System.CodeDom.CodeStatement" />. </exception>
	protected void GenerateExpression(CodeExpression e)
	{
		if (e is CodeArrayCreateExpression)
		{
			GenerateArrayCreateExpression((CodeArrayCreateExpression)e);
			return;
		}
		if (e is CodeBaseReferenceExpression)
		{
			GenerateBaseReferenceExpression((CodeBaseReferenceExpression)e);
			return;
		}
		if (e is CodeBinaryOperatorExpression)
		{
			GenerateBinaryOperatorExpression((CodeBinaryOperatorExpression)e);
			return;
		}
		if (e is CodeCastExpression)
		{
			GenerateCastExpression((CodeCastExpression)e);
			return;
		}
		if (e is CodeDelegateCreateExpression)
		{
			GenerateDelegateCreateExpression((CodeDelegateCreateExpression)e);
			return;
		}
		if (e is CodeFieldReferenceExpression)
		{
			GenerateFieldReferenceExpression((CodeFieldReferenceExpression)e);
			return;
		}
		if (e is CodeArgumentReferenceExpression)
		{
			GenerateArgumentReferenceExpression((CodeArgumentReferenceExpression)e);
			return;
		}
		if (e is CodeVariableReferenceExpression)
		{
			GenerateVariableReferenceExpression((CodeVariableReferenceExpression)e);
			return;
		}
		if (e is CodeIndexerExpression)
		{
			GenerateIndexerExpression((CodeIndexerExpression)e);
			return;
		}
		if (e is CodeArrayIndexerExpression)
		{
			GenerateArrayIndexerExpression((CodeArrayIndexerExpression)e);
			return;
		}
		if (e is CodeSnippetExpression)
		{
			GenerateSnippetExpression((CodeSnippetExpression)e);
			return;
		}
		if (e is CodeMethodInvokeExpression)
		{
			GenerateMethodInvokeExpression((CodeMethodInvokeExpression)e);
			return;
		}
		if (e is CodeMethodReferenceExpression)
		{
			GenerateMethodReferenceExpression((CodeMethodReferenceExpression)e);
			return;
		}
		if (e is CodeEventReferenceExpression)
		{
			GenerateEventReferenceExpression((CodeEventReferenceExpression)e);
			return;
		}
		if (e is CodeDelegateInvokeExpression)
		{
			GenerateDelegateInvokeExpression((CodeDelegateInvokeExpression)e);
			return;
		}
		if (e is CodeObjectCreateExpression)
		{
			GenerateObjectCreateExpression((CodeObjectCreateExpression)e);
			return;
		}
		if (e is CodeParameterDeclarationExpression)
		{
			GenerateParameterDeclarationExpression((CodeParameterDeclarationExpression)e);
			return;
		}
		if (e is CodeDirectionExpression)
		{
			GenerateDirectionExpression((CodeDirectionExpression)e);
			return;
		}
		if (e is CodePrimitiveExpression)
		{
			GeneratePrimitiveExpression((CodePrimitiveExpression)e);
			return;
		}
		if (e is CodePropertyReferenceExpression)
		{
			GeneratePropertyReferenceExpression((CodePropertyReferenceExpression)e);
			return;
		}
		if (e is CodePropertySetValueReferenceExpression)
		{
			GeneratePropertySetValueReferenceExpression((CodePropertySetValueReferenceExpression)e);
			return;
		}
		if (e is CodeThisReferenceExpression)
		{
			GenerateThisReferenceExpression((CodeThisReferenceExpression)e);
			return;
		}
		if (e is CodeTypeReferenceExpression)
		{
			GenerateTypeReferenceExpression((CodeTypeReferenceExpression)e);
			return;
		}
		if (e is CodeTypeOfExpression)
		{
			GenerateTypeOfExpression((CodeTypeOfExpression)e);
			return;
		}
		if (e is CodeDefaultValueExpression)
		{
			GenerateDefaultValueExpression((CodeDefaultValueExpression)e);
			return;
		}
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		throw new ArgumentException(global::SR.Format("Element type {0} is not supported.", e.GetType().FullName), "e");
	}

	private void GenerateFields(CodeTypeDeclaration e)
	{
		foreach (CodeTypeMember member in e.Members)
		{
			if (member is CodeMemberField)
			{
				_currentMember = member;
				if (_options.BlankLinesBetweenMembers)
				{
					Output.WriteLine();
				}
				if (_currentMember.StartDirectives.Count > 0)
				{
					GenerateDirectives(_currentMember.StartDirectives);
				}
				GenerateCommentStatements(_currentMember.Comments);
				CodeMemberField codeMemberField = (CodeMemberField)member;
				if (codeMemberField.LinePragma != null)
				{
					GenerateLinePragmaStart(codeMemberField.LinePragma);
				}
				GenerateField(codeMemberField);
				if (codeMemberField.LinePragma != null)
				{
					GenerateLinePragmaEnd(codeMemberField.LinePragma);
				}
				if (_currentMember.EndDirectives.Count > 0)
				{
					GenerateDirectives(_currentMember.EndDirectives);
				}
			}
		}
	}

	private void GenerateSnippetMembers(CodeTypeDeclaration e)
	{
		bool flag = false;
		foreach (CodeTypeMember member in e.Members)
		{
			if (member is CodeSnippetTypeMember)
			{
				flag = true;
				_currentMember = member;
				if (_options.BlankLinesBetweenMembers)
				{
					Output.WriteLine();
				}
				if (_currentMember.StartDirectives.Count > 0)
				{
					GenerateDirectives(_currentMember.StartDirectives);
				}
				GenerateCommentStatements(_currentMember.Comments);
				CodeSnippetTypeMember codeSnippetTypeMember = (CodeSnippetTypeMember)member;
				if (codeSnippetTypeMember.LinePragma != null)
				{
					GenerateLinePragmaStart(codeSnippetTypeMember.LinePragma);
				}
				int indent = Indent;
				Indent = 0;
				GenerateSnippetMember(codeSnippetTypeMember);
				Indent = indent;
				if (codeSnippetTypeMember.LinePragma != null)
				{
					GenerateLinePragmaEnd(codeSnippetTypeMember.LinePragma);
				}
				if (_currentMember.EndDirectives.Count > 0)
				{
					GenerateDirectives(_currentMember.EndDirectives);
				}
			}
		}
		if (flag)
		{
			Output.WriteLine();
		}
	}

	/// <summary>Outputs the code of the specified literal code fragment compile unit.</summary>
	/// <param name="e">The literal code fragment compile unit to generate code for. </param>
	protected virtual void GenerateSnippetCompileUnit(CodeSnippetCompileUnit e)
	{
		GenerateDirectives(e.StartDirectives);
		if (e.LinePragma != null)
		{
			GenerateLinePragmaStart(e.LinePragma);
		}
		Output.WriteLine(e.Value);
		if (e.LinePragma != null)
		{
			GenerateLinePragmaEnd(e.LinePragma);
		}
		if (e.EndDirectives.Count > 0)
		{
			GenerateDirectives(e.EndDirectives);
		}
	}

	private void GenerateMethods(CodeTypeDeclaration e)
	{
		foreach (CodeTypeMember member in e.Members)
		{
			if (member is CodeMemberMethod && !(member is CodeTypeConstructor) && !(member is CodeConstructor))
			{
				_currentMember = member;
				if (_options.BlankLinesBetweenMembers)
				{
					Output.WriteLine();
				}
				if (_currentMember.StartDirectives.Count > 0)
				{
					GenerateDirectives(_currentMember.StartDirectives);
				}
				GenerateCommentStatements(_currentMember.Comments);
				CodeMemberMethod codeMemberMethod = (CodeMemberMethod)member;
				if (codeMemberMethod.LinePragma != null)
				{
					GenerateLinePragmaStart(codeMemberMethod.LinePragma);
				}
				if (member is CodeEntryPointMethod)
				{
					GenerateEntryPointMethod((CodeEntryPointMethod)member, e);
				}
				else
				{
					GenerateMethod(codeMemberMethod, e);
				}
				if (codeMemberMethod.LinePragma != null)
				{
					GenerateLinePragmaEnd(codeMemberMethod.LinePragma);
				}
				if (_currentMember.EndDirectives.Count > 0)
				{
					GenerateDirectives(_currentMember.EndDirectives);
				}
			}
		}
	}

	private void GenerateNestedTypes(CodeTypeDeclaration e)
	{
		foreach (CodeTypeMember member in e.Members)
		{
			if (member is CodeTypeDeclaration)
			{
				if (_options.BlankLinesBetweenMembers)
				{
					Output.WriteLine();
				}
				CodeTypeDeclaration e2 = (CodeTypeDeclaration)member;
				((ICodeGenerator)this).GenerateCodeFromType(e2, _output.InnerWriter, _options);
			}
		}
	}

	/// <summary>Generates code for the specified compile unit.</summary>
	/// <param name="e">The compile unit to generate code for. </param>
	protected virtual void GenerateCompileUnit(CodeCompileUnit e)
	{
		GenerateCompileUnitStart(e);
		GenerateNamespaces(e);
		GenerateCompileUnitEnd(e);
	}

	/// <summary>Generates code for the specified namespace.</summary>
	/// <param name="e">The namespace to generate code for. </param>
	protected virtual void GenerateNamespace(CodeNamespace e)
	{
		GenerateCommentStatements(e.Comments);
		GenerateNamespaceStart(e);
		GenerateNamespaceImports(e);
		Output.WriteLine();
		GenerateTypes(e);
		GenerateNamespaceEnd(e);
	}

	/// <summary>Generates code for the specified namespace import.</summary>
	/// <param name="e">The namespace import to generate code for. </param>
	protected void GenerateNamespaceImports(CodeNamespace e)
	{
		foreach (CodeNamespaceImport import in e.Imports)
		{
			if (import.LinePragma != null)
			{
				GenerateLinePragmaStart(import.LinePragma);
			}
			GenerateNamespaceImport(import);
			if (import.LinePragma != null)
			{
				GenerateLinePragmaEnd(import.LinePragma);
			}
		}
	}

	private void GenerateProperties(CodeTypeDeclaration e)
	{
		foreach (CodeTypeMember member in e.Members)
		{
			if (member is CodeMemberProperty)
			{
				_currentMember = member;
				if (_options.BlankLinesBetweenMembers)
				{
					Output.WriteLine();
				}
				if (_currentMember.StartDirectives.Count > 0)
				{
					GenerateDirectives(_currentMember.StartDirectives);
				}
				GenerateCommentStatements(_currentMember.Comments);
				CodeMemberProperty codeMemberProperty = (CodeMemberProperty)member;
				if (codeMemberProperty.LinePragma != null)
				{
					GenerateLinePragmaStart(codeMemberProperty.LinePragma);
				}
				GenerateProperty(codeMemberProperty, e);
				if (codeMemberProperty.LinePragma != null)
				{
					GenerateLinePragmaEnd(codeMemberProperty.LinePragma);
				}
				if (_currentMember.EndDirectives.Count > 0)
				{
					GenerateDirectives(_currentMember.EndDirectives);
				}
			}
		}
	}

	/// <summary>Generates code for the specified statement.</summary>
	/// <param name="e">The statement to generate code for. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="e" /> is not a valid <see cref="T:System.CodeDom.CodeStatement" />.</exception>
	protected void GenerateStatement(CodeStatement e)
	{
		if (e.StartDirectives.Count > 0)
		{
			GenerateDirectives(e.StartDirectives);
		}
		if (e.LinePragma != null)
		{
			GenerateLinePragmaStart(e.LinePragma);
		}
		if (e is CodeCommentStatement)
		{
			GenerateCommentStatement((CodeCommentStatement)e);
		}
		else if (e is CodeMethodReturnStatement)
		{
			GenerateMethodReturnStatement((CodeMethodReturnStatement)e);
		}
		else if (e is CodeConditionStatement)
		{
			GenerateConditionStatement((CodeConditionStatement)e);
		}
		else if (e is CodeTryCatchFinallyStatement)
		{
			GenerateTryCatchFinallyStatement((CodeTryCatchFinallyStatement)e);
		}
		else if (e is CodeAssignStatement)
		{
			GenerateAssignStatement((CodeAssignStatement)e);
		}
		else if (e is CodeExpressionStatement)
		{
			GenerateExpressionStatement((CodeExpressionStatement)e);
		}
		else if (e is CodeIterationStatement)
		{
			GenerateIterationStatement((CodeIterationStatement)e);
		}
		else if (e is CodeThrowExceptionStatement)
		{
			GenerateThrowExceptionStatement((CodeThrowExceptionStatement)e);
		}
		else if (e is CodeSnippetStatement)
		{
			int indent = Indent;
			Indent = 0;
			GenerateSnippetStatement((CodeSnippetStatement)e);
			Indent = indent;
		}
		else if (e is CodeVariableDeclarationStatement)
		{
			GenerateVariableDeclarationStatement((CodeVariableDeclarationStatement)e);
		}
		else if (e is CodeAttachEventStatement)
		{
			GenerateAttachEventStatement((CodeAttachEventStatement)e);
		}
		else if (e is CodeRemoveEventStatement)
		{
			GenerateRemoveEventStatement((CodeRemoveEventStatement)e);
		}
		else if (e is CodeGotoStatement)
		{
			GenerateGotoStatement((CodeGotoStatement)e);
		}
		else
		{
			if (!(e is CodeLabeledStatement))
			{
				throw new ArgumentException(global::SR.Format("Element type {0} is not supported.", e.GetType().FullName), "e");
			}
			GenerateLabeledStatement((CodeLabeledStatement)e);
		}
		if (e.LinePragma != null)
		{
			GenerateLinePragmaEnd(e.LinePragma);
		}
		if (e.EndDirectives.Count > 0)
		{
			GenerateDirectives(e.EndDirectives);
		}
	}

	/// <summary>Generates code for the specified statement collection.</summary>
	/// <param name="stms">The statements to generate code for. </param>
	protected void GenerateStatements(CodeStatementCollection stmts)
	{
		foreach (CodeStatement stmt in stmts)
		{
			((ICodeGenerator)this).GenerateCodeFromStatement(stmt, _output.InnerWriter, _options);
		}
	}

	/// <summary>Generates code for the specified attribute declaration collection.</summary>
	/// <param name="attributes">The attributes to generate code for. </param>
	protected virtual void OutputAttributeDeclarations(CodeAttributeDeclarationCollection attributes)
	{
		if (attributes.Count == 0)
		{
			return;
		}
		GenerateAttributeDeclarationsStart(attributes);
		bool flag = true;
		foreach (CodeAttributeDeclaration attribute in attributes)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				ContinueOnNewLine(", ");
			}
			Output.Write(attribute.Name);
			Output.Write('(');
			bool flag2 = true;
			foreach (CodeAttributeArgument argument in attribute.Arguments)
			{
				if (flag2)
				{
					flag2 = false;
				}
				else
				{
					Output.Write(", ");
				}
				OutputAttributeArgument(argument);
			}
			Output.Write(')');
		}
		GenerateAttributeDeclarationsEnd(attributes);
	}

	/// <summary>Outputs an argument in an attribute block.</summary>
	/// <param name="arg">The attribute argument to generate code for. </param>
	protected virtual void OutputAttributeArgument(CodeAttributeArgument arg)
	{
		if (!string.IsNullOrEmpty(arg.Name))
		{
			OutputIdentifier(arg.Name);
			Output.Write('=');
		}
		((ICodeGenerator)this).GenerateCodeFromExpression(arg.Value, _output.InnerWriter, _options);
	}

	/// <summary>Generates code for the specified <see cref="T:System.CodeDom.FieldDirection" />.</summary>
	/// <param name="dir">One of the enumeration values that indicates the attribute of the field. </param>
	protected virtual void OutputDirection(FieldDirection dir)
	{
		switch (dir)
		{
		case FieldDirection.Out:
			Output.Write("out ");
			break;
		case FieldDirection.Ref:
			Output.Write("ref ");
			break;
		case FieldDirection.In:
			break;
		}
	}

	/// <summary>Outputs a field scope modifier that corresponds to the specified attributes.</summary>
	/// <param name="attributes">One of the enumeration values that specifies the attributes. </param>
	protected virtual void OutputFieldScopeModifier(MemberAttributes attributes)
	{
		MemberAttributes memberAttributes = attributes & MemberAttributes.VTableMask;
		if (memberAttributes == MemberAttributes.New)
		{
			Output.Write("new ");
		}
		switch (attributes & MemberAttributes.ScopeMask)
		{
		case MemberAttributes.Static:
			Output.Write("static ");
			break;
		case MemberAttributes.Const:
			Output.Write("const ");
			break;
		case MemberAttributes.Final:
		case MemberAttributes.Override:
			break;
		}
	}

	/// <summary>Generates code for the specified member access modifier.</summary>
	/// <param name="attributes">One of the enumeration values that indicates the member access modifier to generate code for. </param>
	protected virtual void OutputMemberAccessModifier(MemberAttributes attributes)
	{
		switch (attributes & MemberAttributes.AccessMask)
		{
		case MemberAttributes.Assembly:
			Output.Write("internal ");
			break;
		case MemberAttributes.FamilyAndAssembly:
			Output.Write("internal ");
			break;
		case MemberAttributes.Family:
			Output.Write("protected ");
			break;
		case MemberAttributes.FamilyOrAssembly:
			Output.Write("protected internal ");
			break;
		case MemberAttributes.Private:
			Output.Write("private ");
			break;
		case MemberAttributes.Public:
			Output.Write("public ");
			break;
		}
	}

	/// <summary>Generates code for the specified member scope modifier.</summary>
	/// <param name="attributes">One of the enumeration values that indicates the member scope modifier to generate code for. </param>
	protected virtual void OutputMemberScopeModifier(MemberAttributes attributes)
	{
		MemberAttributes memberAttributes = attributes & MemberAttributes.VTableMask;
		if (memberAttributes == MemberAttributes.New)
		{
			Output.Write("new ");
		}
		switch (attributes & MemberAttributes.ScopeMask)
		{
		case MemberAttributes.Abstract:
			Output.Write("abstract ");
			return;
		case MemberAttributes.Final:
			Output.Write("");
			return;
		case MemberAttributes.Static:
			Output.Write("static ");
			return;
		case MemberAttributes.Override:
			Output.Write("override ");
			return;
		}
		memberAttributes = attributes & MemberAttributes.AccessMask;
		if (memberAttributes == MemberAttributes.Family || memberAttributes == MemberAttributes.Public)
		{
			Output.Write("virtual ");
		}
	}

	/// <summary>Generates code for the specified type.</summary>
	/// <param name="typeRef">The type to generate code for. </param>
	protected abstract void OutputType(CodeTypeReference typeRef);

	/// <summary>Generates code for the specified type attributes.</summary>
	/// <param name="attributes">One of the enumeration values that indicates the type attributes to generate code for. </param>
	/// <param name="isStruct">true if the type is a struct; otherwise, false. </param>
	/// <param name="isEnum">true if the type is an enum; otherwise, false. </param>
	protected virtual void OutputTypeAttributes(TypeAttributes attributes, bool isStruct, bool isEnum)
	{
		switch (attributes & TypeAttributes.VisibilityMask)
		{
		case TypeAttributes.Public:
		case TypeAttributes.NestedPublic:
			Output.Write("public ");
			break;
		case TypeAttributes.NestedPrivate:
			Output.Write("private ");
			break;
		}
		if (isStruct)
		{
			Output.Write("struct ");
			return;
		}
		if (isEnum)
		{
			Output.Write("enum ");
			return;
		}
		switch (attributes & TypeAttributes.ClassSemanticsMask)
		{
		case TypeAttributes.NotPublic:
			if ((attributes & TypeAttributes.Sealed) == TypeAttributes.Sealed)
			{
				Output.Write("sealed ");
			}
			if ((attributes & TypeAttributes.Abstract) == TypeAttributes.Abstract)
			{
				Output.Write("abstract ");
			}
			Output.Write("class ");
			break;
		case TypeAttributes.ClassSemanticsMask:
			Output.Write("interface ");
			break;
		}
	}

	/// <summary>Generates code for the specified object type and name pair.</summary>
	/// <param name="typeRef">The type. </param>
	/// <param name="name">The name for the object. </param>
	protected virtual void OutputTypeNamePair(CodeTypeReference typeRef, string name)
	{
		OutputType(typeRef);
		Output.Write(' ');
		OutputIdentifier(name);
	}

	/// <summary>Outputs the specified identifier.</summary>
	/// <param name="ident">The identifier to output. </param>
	protected virtual void OutputIdentifier(string ident)
	{
		Output.Write(ident);
	}

	/// <summary>Generates code for the specified expression list.</summary>
	/// <param name="expressions">The expressions to generate code for. </param>
	protected virtual void OutputExpressionList(CodeExpressionCollection expressions)
	{
		OutputExpressionList(expressions, newlineBetweenItems: false);
	}

	/// <summary>Generates code for the specified expression list.</summary>
	/// <param name="expressions">The expressions to generate code for. </param>
	/// <param name="newlineBetweenItems">true to insert a new line after each item; otherwise, false. </param>
	protected virtual void OutputExpressionList(CodeExpressionCollection expressions, bool newlineBetweenItems)
	{
		bool flag = true;
		Indent++;
		foreach (CodeExpression expression in expressions)
		{
			if (flag)
			{
				flag = false;
			}
			else if (newlineBetweenItems)
			{
				ContinueOnNewLine(",");
			}
			else
			{
				Output.Write(", ");
			}
			((ICodeGenerator)this).GenerateCodeFromExpression(expression, _output.InnerWriter, _options);
		}
		Indent--;
	}

	/// <summary>Generates code for the specified operator.</summary>
	/// <param name="op">The operator to generate code for. </param>
	protected virtual void OutputOperator(CodeBinaryOperatorType op)
	{
		switch (op)
		{
		case CodeBinaryOperatorType.Add:
			Output.Write('+');
			break;
		case CodeBinaryOperatorType.Subtract:
			Output.Write('-');
			break;
		case CodeBinaryOperatorType.Multiply:
			Output.Write('*');
			break;
		case CodeBinaryOperatorType.Divide:
			Output.Write('/');
			break;
		case CodeBinaryOperatorType.Modulus:
			Output.Write('%');
			break;
		case CodeBinaryOperatorType.Assign:
			Output.Write('=');
			break;
		case CodeBinaryOperatorType.IdentityInequality:
			Output.Write("!=");
			break;
		case CodeBinaryOperatorType.IdentityEquality:
			Output.Write("==");
			break;
		case CodeBinaryOperatorType.ValueEquality:
			Output.Write("==");
			break;
		case CodeBinaryOperatorType.BitwiseOr:
			Output.Write('|');
			break;
		case CodeBinaryOperatorType.BitwiseAnd:
			Output.Write('&');
			break;
		case CodeBinaryOperatorType.BooleanOr:
			Output.Write("||");
			break;
		case CodeBinaryOperatorType.BooleanAnd:
			Output.Write("&&");
			break;
		case CodeBinaryOperatorType.LessThan:
			Output.Write('<');
			break;
		case CodeBinaryOperatorType.LessThanOrEqual:
			Output.Write("<=");
			break;
		case CodeBinaryOperatorType.GreaterThan:
			Output.Write('>');
			break;
		case CodeBinaryOperatorType.GreaterThanOrEqual:
			Output.Write(">=");
			break;
		}
	}

	/// <summary>Generates code for the specified parameters.</summary>
	/// <param name="parameters">The parameter declaration expressions to generate code for. </param>
	protected virtual void OutputParameters(CodeParameterDeclarationExpressionCollection parameters)
	{
		bool flag = true;
		bool flag2 = parameters.Count > 15;
		if (flag2)
		{
			Indent += 3;
		}
		foreach (CodeParameterDeclarationExpression parameter in parameters)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				Output.Write(", ");
			}
			if (flag2)
			{
				ContinueOnNewLine("");
			}
			GenerateExpression(parameter);
		}
		if (flag2)
		{
			Indent -= 3;
		}
	}

	/// <summary>Generates code for the specified array creation expression.</summary>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeArrayCreateExpression" /> that indicates the expression to generate code for. </param>
	protected abstract void GenerateArrayCreateExpression(CodeArrayCreateExpression e);

	/// <summary>Generates code for the specified base reference expression.</summary>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeBaseReferenceExpression" /> that indicates the expression to generate code for. </param>
	protected abstract void GenerateBaseReferenceExpression(CodeBaseReferenceExpression e);

	/// <summary>Generates code for the specified binary operator expression.</summary>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeBinaryOperatorExpression" /> that indicates the expression to generate code for. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="e" /> is null.</exception>
	protected virtual void GenerateBinaryOperatorExpression(CodeBinaryOperatorExpression e)
	{
		bool flag = false;
		Output.Write('(');
		GenerateExpression(e.Left);
		Output.Write(' ');
		if (e.Left is CodeBinaryOperatorExpression || e.Right is CodeBinaryOperatorExpression)
		{
			if (!_inNestedBinary)
			{
				flag = true;
				_inNestedBinary = true;
				Indent += 3;
			}
			ContinueOnNewLine("");
		}
		OutputOperator(e.Operator);
		Output.Write(' ');
		GenerateExpression(e.Right);
		Output.Write(')');
		if (flag)
		{
			Indent -= 3;
			_inNestedBinary = false;
		}
	}

	/// <summary>Generates a line-continuation character and outputs the specified string on a new line.</summary>
	/// <param name="st">The string to write on the new line. </param>
	protected virtual void ContinueOnNewLine(string st)
	{
		Output.WriteLine(st);
	}

	/// <summary>Generates code for the specified cast expression.</summary>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeCastExpression" /> that indicates the expression to generate code for. </param>
	protected abstract void GenerateCastExpression(CodeCastExpression e);

	/// <summary>Generates code for the specified delegate creation expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected abstract void GenerateDelegateCreateExpression(CodeDelegateCreateExpression e);

	/// <summary>Generates code for the specified field reference expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected abstract void GenerateFieldReferenceExpression(CodeFieldReferenceExpression e);

	/// <summary>Generates code for the specified argument reference expression.</summary>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeArgumentReferenceExpression" /> that indicates the expression to generate code for. </param>
	protected abstract void GenerateArgumentReferenceExpression(CodeArgumentReferenceExpression e);

	/// <summary>Generates code for the specified variable reference expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected abstract void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e);

	/// <summary>Generates code for the specified indexer expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected abstract void GenerateIndexerExpression(CodeIndexerExpression e);

	/// <summary>Generates code for the specified array indexer expression.</summary>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeArrayIndexerExpression" /> that indicates the expression to generate code for. </param>
	protected abstract void GenerateArrayIndexerExpression(CodeArrayIndexerExpression e);

	/// <summary>Outputs the code of the specified literal code fragment expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected abstract void GenerateSnippetExpression(CodeSnippetExpression e);

	/// <summary>Generates code for the specified method invoke expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected abstract void GenerateMethodInvokeExpression(CodeMethodInvokeExpression e);

	/// <summary>Generates code for the specified method reference expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected abstract void GenerateMethodReferenceExpression(CodeMethodReferenceExpression e);

	/// <summary>Generates code for the specified event reference expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected abstract void GenerateEventReferenceExpression(CodeEventReferenceExpression e);

	/// <summary>Generates code for the specified delegate invoke expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected abstract void GenerateDelegateInvokeExpression(CodeDelegateInvokeExpression e);

	/// <summary>Generates code for the specified object creation expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected abstract void GenerateObjectCreateExpression(CodeObjectCreateExpression e);

	/// <summary>Generates code for the specified parameter declaration expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected virtual void GenerateParameterDeclarationExpression(CodeParameterDeclarationExpression e)
	{
		if (e.CustomAttributes.Count > 0)
		{
			OutputAttributeDeclarations(e.CustomAttributes);
			Output.Write(' ');
		}
		OutputDirection(e.Direction);
		OutputTypeNamePair(e.Type, e.Name);
	}

	/// <summary>Generates code for the specified direction expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected virtual void GenerateDirectionExpression(CodeDirectionExpression e)
	{
		OutputDirection(e.Direction);
		GenerateExpression(e.Expression);
	}

	/// <summary>Generates code for the specified primitive expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="e" /> uses an invalid data type. Only the following data types are valid:stringcharbyteInt16Int32Int64SingleDoubleDecimal</exception>
	protected virtual void GeneratePrimitiveExpression(CodePrimitiveExpression e)
	{
		if (e.Value == null)
		{
			Output.Write(NullToken);
			return;
		}
		if (e.Value is string)
		{
			Output.Write(QuoteSnippetString((string)e.Value));
			return;
		}
		if (e.Value is char)
		{
			Output.Write("'" + e.Value.ToString() + "'");
			return;
		}
		if (e.Value is byte)
		{
			Output.Write(((byte)e.Value).ToString(CultureInfo.InvariantCulture));
			return;
		}
		if (e.Value is short)
		{
			Output.Write(((short)e.Value).ToString(CultureInfo.InvariantCulture));
			return;
		}
		if (e.Value is int)
		{
			Output.Write(((int)e.Value).ToString(CultureInfo.InvariantCulture));
			return;
		}
		if (e.Value is long)
		{
			Output.Write(((long)e.Value).ToString(CultureInfo.InvariantCulture));
			return;
		}
		if (e.Value is float)
		{
			GenerateSingleFloatValue((float)e.Value);
			return;
		}
		if (e.Value is double)
		{
			GenerateDoubleValue((double)e.Value);
			return;
		}
		if (e.Value is decimal)
		{
			GenerateDecimalValue((decimal)e.Value);
			return;
		}
		if (e.Value is bool)
		{
			if ((bool)e.Value)
			{
				Output.Write("true");
			}
			else
			{
				Output.Write("false");
			}
			return;
		}
		throw new ArgumentException(global::SR.Format("Invalid Primitive Type: {0}. Consider using CodeObjectCreateExpression.", e.Value.GetType().ToString()));
	}

	/// <summary>Generates code for a single-precision floating point number.</summary>
	/// <param name="s">The value to generate code for. </param>
	protected virtual void GenerateSingleFloatValue(float s)
	{
		Output.Write(s.ToString("R", CultureInfo.InvariantCulture));
	}

	/// <summary>Generates code for a double-precision floating point number.</summary>
	/// <param name="d">The value to generate code for. </param>
	protected virtual void GenerateDoubleValue(double d)
	{
		Output.Write(d.ToString("R", CultureInfo.InvariantCulture));
	}

	/// <summary>Generates code for the specified decimal value.</summary>
	/// <param name="d">The decimal value to generate code for. </param>
	protected virtual void GenerateDecimalValue(decimal d)
	{
		Output.Write(d.ToString(CultureInfo.InvariantCulture));
	}

	/// <summary>Generates code for the specified reference to a default value.</summary>
	/// <param name="e">The reference to generate code for.</param>
	protected virtual void GenerateDefaultValueExpression(CodeDefaultValueExpression e)
	{
	}

	/// <summary>Generates code for the specified property reference expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected abstract void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression e);

	/// <summary>Generates code for the specified property set value reference expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected abstract void GeneratePropertySetValueReferenceExpression(CodePropertySetValueReferenceExpression e);

	/// <summary>Generates code for the specified this reference expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected abstract void GenerateThisReferenceExpression(CodeThisReferenceExpression e);

	/// <summary>Generates code for the specified type reference expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected virtual void GenerateTypeReferenceExpression(CodeTypeReferenceExpression e)
	{
		OutputType(e.Type);
	}

	/// <summary>Generates code for the specified type of expression.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected virtual void GenerateTypeOfExpression(CodeTypeOfExpression e)
	{
		Output.Write("typeof(");
		OutputType(e.Type);
		Output.Write(')');
	}

	/// <summary>Generates code for the specified expression statement.</summary>
	/// <param name="e">The statement to generate code for. </param>
	protected abstract void GenerateExpressionStatement(CodeExpressionStatement e);

	/// <summary>Generates code for the specified iteration statement.</summary>
	/// <param name="e">The statement to generate code for. </param>
	protected abstract void GenerateIterationStatement(CodeIterationStatement e);

	/// <summary>Generates code for the specified throw exception statement.</summary>
	/// <param name="e">The statement to generate code for. </param>
	protected abstract void GenerateThrowExceptionStatement(CodeThrowExceptionStatement e);

	/// <summary>Generates code for the specified comment statement.</summary>
	/// <param name="e">The statement to generate code for.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.CodeDom.CodeCommentStatement.Comment" /> property of <paramref name="e " />is not set.</exception>
	protected virtual void GenerateCommentStatement(CodeCommentStatement e)
	{
		if (e.Comment == null)
		{
			throw new ArgumentException(global::SR.Format("The 'Comment' property of the CodeCommentStatement '{0}' cannot be null.", "e"), "e");
		}
		GenerateComment(e.Comment);
	}

	/// <summary>Generates code for the specified comment statements.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected virtual void GenerateCommentStatements(CodeCommentStatementCollection e)
	{
		foreach (CodeCommentStatement item in e)
		{
			GenerateCommentStatement(item);
		}
	}

	/// <summary>Generates code for the specified comment.</summary>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeComment" /> to generate code for. </param>
	protected abstract void GenerateComment(CodeComment e);

	/// <summary>Generates code for the specified method return statement.</summary>
	/// <param name="e">The statement to generate code for. </param>
	protected abstract void GenerateMethodReturnStatement(CodeMethodReturnStatement e);

	/// <summary>Generates code for the specified conditional statement.</summary>
	/// <param name="e">The statement to generate code for. </param>
	protected abstract void GenerateConditionStatement(CodeConditionStatement e);

	/// <summary>Generates code for the specified try...catch...finally statement.</summary>
	/// <param name="e">The statement to generate code for. </param>
	protected abstract void GenerateTryCatchFinallyStatement(CodeTryCatchFinallyStatement e);

	/// <summary>Generates code for the specified assignment statement.</summary>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeAssignStatement" /> that indicates the statement to generate code for. </param>
	protected abstract void GenerateAssignStatement(CodeAssignStatement e);

	/// <summary>Generates code for the specified attach event statement.</summary>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeAttachEventStatement" /> that indicates the statement to generate code for. </param>
	protected abstract void GenerateAttachEventStatement(CodeAttachEventStatement e);

	/// <summary>Generates code for the specified remove event statement.</summary>
	/// <param name="e">The statement to generate code for. </param>
	protected abstract void GenerateRemoveEventStatement(CodeRemoveEventStatement e);

	/// <summary>Generates code for the specified goto statement.</summary>
	/// <param name="e">The expression to generate code for. </param>
	protected abstract void GenerateGotoStatement(CodeGotoStatement e);

	/// <summary>Generates code for the specified labeled statement.</summary>
	/// <param name="e">The statement to generate code for. </param>
	protected abstract void GenerateLabeledStatement(CodeLabeledStatement e);

	/// <summary>Outputs the code of the specified literal code fragment statement.</summary>
	/// <param name="e">The statement to generate code for. </param>
	protected virtual void GenerateSnippetStatement(CodeSnippetStatement e)
	{
		Output.WriteLine(e.Value);
	}

	/// <summary>Generates code for the specified variable declaration statement.</summary>
	/// <param name="e">The statement to generate code for. </param>
	protected abstract void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement e);

	/// <summary>Generates code for the specified line pragma start.</summary>
	/// <param name="e">The start of the line pragma to generate code for. </param>
	protected abstract void GenerateLinePragmaStart(CodeLinePragma e);

	/// <summary>Generates code for the specified line pragma end.</summary>
	/// <param name="e">The end of the line pragma to generate code for. </param>
	protected abstract void GenerateLinePragmaEnd(CodeLinePragma e);

	/// <summary>Generates code for the specified event.</summary>
	/// <param name="e">The member event to generate code for. </param>
	/// <param name="c">The type of the object that this event occurs on. </param>
	protected abstract void GenerateEvent(CodeMemberEvent e, CodeTypeDeclaration c);

	/// <summary>Generates code for the specified member field.</summary>
	/// <param name="e">The field to generate code for. </param>
	protected abstract void GenerateField(CodeMemberField e);

	/// <summary>Outputs the code of the specified literal code fragment class member.</summary>
	/// <param name="e">The member to generate code for. </param>
	protected abstract void GenerateSnippetMember(CodeSnippetTypeMember e);

	/// <summary>Generates code for the specified entry point method.</summary>
	/// <param name="e">The entry point for the code. </param>
	/// <param name="c">The code that declares the type. </param>
	protected abstract void GenerateEntryPointMethod(CodeEntryPointMethod e, CodeTypeDeclaration c);

	/// <summary>Generates code for the specified method.</summary>
	/// <param name="e">The member method to generate code for. </param>
	/// <param name="c">The type of the object that this method occurs on. </param>
	protected abstract void GenerateMethod(CodeMemberMethod e, CodeTypeDeclaration c);

	/// <summary>Generates code for the specified property.</summary>
	/// <param name="e">The property to generate code for. </param>
	/// <param name="c">The type of the object that this property occurs on. </param>
	protected abstract void GenerateProperty(CodeMemberProperty e, CodeTypeDeclaration c);

	/// <summary>Generates code for the specified constructor.</summary>
	/// <param name="e">The constructor to generate code for. </param>
	/// <param name="c">The type of the object that this constructor constructs. </param>
	protected abstract void GenerateConstructor(CodeConstructor e, CodeTypeDeclaration c);

	/// <summary>Generates code for the specified class constructor.</summary>
	/// <param name="e">The class constructor to generate code for. </param>
	protected abstract void GenerateTypeConstructor(CodeTypeConstructor e);

	/// <summary>Generates code for the specified start of the class.</summary>
	/// <param name="e">The start of the class to generate code for. </param>
	protected abstract void GenerateTypeStart(CodeTypeDeclaration e);

	/// <summary>Generates code for the specified end of the class.</summary>
	/// <param name="e">The end of the class to generate code for. </param>
	protected abstract void GenerateTypeEnd(CodeTypeDeclaration e);

	/// <summary>Generates code for the start of a compile unit.</summary>
	/// <param name="e">The compile unit to generate code for. </param>
	protected virtual void GenerateCompileUnitStart(CodeCompileUnit e)
	{
		if (e.StartDirectives.Count > 0)
		{
			GenerateDirectives(e.StartDirectives);
		}
	}

	/// <summary>Generates code for the end of a compile unit.</summary>
	/// <param name="e">The compile unit to generate code for. </param>
	protected virtual void GenerateCompileUnitEnd(CodeCompileUnit e)
	{
		if (e.EndDirectives.Count > 0)
		{
			GenerateDirectives(e.EndDirectives);
		}
	}

	/// <summary>Generates code for the start of a namespace.</summary>
	/// <param name="e">The namespace to generate code for. </param>
	protected abstract void GenerateNamespaceStart(CodeNamespace e);

	/// <summary>Generates code for the end of a namespace.</summary>
	/// <param name="e">The namespace to generate code for. </param>
	protected abstract void GenerateNamespaceEnd(CodeNamespace e);

	/// <summary>Generates code for the specified namespace import.</summary>
	/// <param name="e">The namespace import to generate code for. </param>
	protected abstract void GenerateNamespaceImport(CodeNamespaceImport e);

	/// <summary>Generates code for the specified attribute block start.</summary>
	/// <param name="attributes">A <see cref="T:System.CodeDom.CodeAttributeDeclarationCollection" /> that indicates the start of the attribute block to generate code for. </param>
	protected abstract void GenerateAttributeDeclarationsStart(CodeAttributeDeclarationCollection attributes);

	/// <summary>Generates code for the specified attribute block end.</summary>
	/// <param name="attributes">A <see cref="T:System.CodeDom.CodeAttributeDeclarationCollection" /> that indicates the end of the attribute block to generate code for. </param>
	protected abstract void GenerateAttributeDeclarationsEnd(CodeAttributeDeclarationCollection attributes);

	/// <summary>Gets a value indicating whether the specified code generation support is provided.</summary>
	/// <returns>true if the specified code generation support is provided; otherwise, false.</returns>
	/// <param name="support">The type of code generation support to test for. </param>
	protected abstract bool Supports(GeneratorSupport support);

	/// <summary>Gets a value indicating whether the specified value is a valid identifier.</summary>
	/// <returns>true if the value is a valid identifier; otherwise, false.</returns>
	/// <param name="value">The value to test for conflicts with valid identifiers. </param>
	protected abstract bool IsValidIdentifier(string value);

	/// <summary>Throws an exception if the specified string is not a valid identifier.</summary>
	/// <param name="value">The identifier to test for validity as an identifier. </param>
	/// <exception cref="T:System.ArgumentException">If the specified identifier is invalid or conflicts with reserved or language keywords. </exception>
	protected virtual void ValidateIdentifier(string value)
	{
		if (!IsValidIdentifier(value))
		{
			throw new ArgumentException(global::SR.Format("Identifier '{0}' is not valid.", value));
		}
	}

	/// <summary>Creates an escaped identifier for the specified value.</summary>
	/// <returns>The escaped identifier for the value.</returns>
	/// <param name="value">The string to create an escaped identifier for. </param>
	protected abstract string CreateEscapedIdentifier(string value);

	/// <summary>Creates a valid identifier for the specified value.</summary>
	/// <returns>A valid identifier for the value.</returns>
	/// <param name="value">A string to create a valid identifier for. </param>
	protected abstract string CreateValidIdentifier(string value);

	/// <summary>Gets the name of the specified data type.</summary>
	/// <returns>The name of the data type reference.</returns>
	/// <param name="value">The type whose name will be returned. </param>
	protected abstract string GetTypeOutput(CodeTypeReference value);

	/// <summary>Converts the specified string by formatting it with escape codes.</summary>
	/// <returns>The converted string.</returns>
	/// <param name="value">The string to convert. </param>
	protected abstract string QuoteSnippetString(string value);

	/// <summary>Gets a value indicating whether the specified string is a valid identifier.</summary>
	/// <returns>true if the specified string is a valid identifier; otherwise, false.</returns>
	/// <param name="value">The string to test for validity. </param>
	public static bool IsValidLanguageIndependentIdentifier(string value)
	{
		return CSharpHelpers.IsValidTypeNameOrIdentifier(value, isTypeName: false);
	}

	internal static bool IsValidLanguageIndependentTypeName(string value)
	{
		return CSharpHelpers.IsValidTypeNameOrIdentifier(value, isTypeName: true);
	}

	/// <summary>Attempts to validate each identifier field contained in the specified <see cref="T:System.CodeDom.CodeObject" /> or <see cref="N:System.CodeDom" /> tree.</summary>
	/// <param name="e">An object to test for invalid identifiers. </param>
	/// <exception cref="T:System.ArgumentException">The specified <see cref="T:System.CodeDom.CodeObject" /> contains an invalid identifier. </exception>
	public static void ValidateIdentifiers(CodeObject e)
	{
		new CodeValidator().ValidateIdentifiers(e);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.Compiler.CodeGenerator" /> class. </summary>
	protected CodeGenerator()
	{
	}
}
