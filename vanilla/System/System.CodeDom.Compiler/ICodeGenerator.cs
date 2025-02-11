using System.IO;

namespace System.CodeDom.Compiler;

/// <summary>Defines an interface for generating code.</summary>
public interface ICodeGenerator
{
	/// <summary>Gets a value that indicates whether the specified value is a valid identifier for the current language.</summary>
	/// <returns>true if the <paramref name="value" /> parameter is a valid identifier; otherwise, false.</returns>
	/// <param name="value">The value to test for being a valid identifier. </param>
	bool IsValidIdentifier(string value);

	/// <summary>Throws an exception if the specified value is not a valid identifier.</summary>
	/// <param name="value">The identifier to validate. </param>
	/// <exception cref="T:System.ArgumentException">The identifier is not valid. </exception>
	void ValidateIdentifier(string value);

	/// <summary>Creates an escaped identifier for the specified value.</summary>
	/// <returns>The escaped identifier for the value.</returns>
	/// <param name="value">The string to create an escaped identifier for. </param>
	string CreateEscapedIdentifier(string value);

	/// <summary>Creates a valid identifier for the specified value.</summary>
	/// <returns>A valid identifier for the specified value.</returns>
	/// <param name="value">The string to generate a valid identifier for. </param>
	string CreateValidIdentifier(string value);

	/// <summary>Gets the type indicated by the specified <see cref="T:System.CodeDom.CodeTypeReference" />.</summary>
	/// <returns>A text representation of the specified type for the language this code generator is designed to generate code in. For example, in Visual Basic, passing in type System.Int32 will return "Integer".</returns>
	/// <param name="type">A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the type to return. </param>
	string GetTypeOutput(CodeTypeReference type);

	/// <summary>Gets a value indicating whether the generator provides support for the language features represented by the specified <see cref="T:System.CodeDom.Compiler.GeneratorSupport" /> object.</summary>
	/// <returns>true if the specified capabilities are supported; otherwise, false.</returns>
	/// <param name="supports">The capabilities to test the generator for. </param>
	bool Supports(GeneratorSupport supports);

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) expression and outputs it to the specified text writer.</summary>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the expression to generate code for. </param>
	/// <param name="w">The <see cref="T:System.IO.TextWriter" /> to output code to. </param>
	/// <param name="o">A <see cref="T:System.CodeDom.Compiler.CodeGeneratorOptions" /> that indicates the options to use for generating code. </param>
	void GenerateCodeFromExpression(CodeExpression e, TextWriter w, CodeGeneratorOptions o);

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) statement and outputs it to the specified text writer using the specified options.</summary>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeStatement" /> containing the CodeDOM elements to translate. </param>
	/// <param name="w">The <see cref="T:System.IO.TextWriter" /> to output code to. </param>
	/// <param name="o">A <see cref="T:System.CodeDom.Compiler.CodeGeneratorOptions" /> that indicates the options to use for generating code. </param>
	void GenerateCodeFromStatement(CodeStatement e, TextWriter w, CodeGeneratorOptions o);

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) namespace and outputs it to the specified text writer using the specified options.</summary>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeNamespace" /> that indicates the namespace to generate code for. </param>
	/// <param name="w">The <see cref="T:System.IO.TextWriter" /> to output code to. </param>
	/// <param name="o">A <see cref="T:System.CodeDom.Compiler.CodeGeneratorOptions" /> that indicates the options to use for generating code. </param>
	void GenerateCodeFromNamespace(CodeNamespace e, TextWriter w, CodeGeneratorOptions o);

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) compilation unit and outputs it to the specified text writer using the specified options.</summary>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeCompileUnit" /> to generate code for. </param>
	/// <param name="w">The <see cref="T:System.IO.TextWriter" /> to output code to. </param>
	/// <param name="o">A <see cref="T:System.CodeDom.Compiler.CodeGeneratorOptions" /> that indicates the options to use for generating code. </param>
	void GenerateCodeFromCompileUnit(CodeCompileUnit e, TextWriter w, CodeGeneratorOptions o);

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) type declaration and outputs it to the specified text writer using the specified options.</summary>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeTypeDeclaration" /> that indicates the type to generate code for. </param>
	/// <param name="w">The <see cref="T:System.IO.TextWriter" /> to output code to. </param>
	/// <param name="o">A <see cref="T:System.CodeDom.Compiler.CodeGeneratorOptions" /> that indicates the options to use for generating code. </param>
	void GenerateCodeFromType(CodeTypeDeclaration e, TextWriter w, CodeGeneratorOptions o);
}
