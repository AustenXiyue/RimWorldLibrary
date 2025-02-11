using System.IO;

namespace System.CodeDom.Compiler;

/// <summary>Provides an empty implementation of the <see cref="T:System.CodeDom.Compiler.ICodeParser" /> interface.</summary>
public abstract class CodeParser : ICodeParser
{
	/// <summary>Compiles the specified text stream into a <see cref="T:System.CodeDom.CodeCompileUnit" />.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeCompileUnit" /> containing the code model produced from parsing the code.</returns>
	/// <param name="codeStream">A <see cref="T:System.IO.TextReader" /> that is used to read the code to be parsed. </param>
	public abstract CodeCompileUnit Parse(TextReader codeStream);

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.Compiler.CodeParser" /> class. </summary>
	protected CodeParser()
	{
	}
}
