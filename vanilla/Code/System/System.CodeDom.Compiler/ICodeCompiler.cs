namespace System.CodeDom.Compiler;

/// <summary>Defines an interface for invoking compilation of source code or a CodeDOM tree using a specific compiler.</summary>
public interface ICodeCompiler
{
	/// <summary>Compiles an assembly from the <see cref="N:System.CodeDom" /> tree contained in the specified <see cref="T:System.CodeDom.CodeCompileUnit" />, using the specified compiler settings.</summary>
	/// <returns>A <see cref="T:System.CodeDom.Compiler.CompilerResults" /> object that indicates the results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the settings for compilation. </param>
	/// <param name="compilationUnit">A <see cref="T:System.CodeDom.CodeCompileUnit" /> that indicates the code to compile. </param>
	CompilerResults CompileAssemblyFromDom(CompilerParameters options, CodeCompileUnit compilationUnit);

	/// <summary>Compiles an assembly from the source code contained within the specified file, using the specified compiler settings.</summary>
	/// <returns>A <see cref="T:System.CodeDom.Compiler.CompilerResults" /> object that indicates the results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the settings for compilation. </param>
	/// <param name="fileName">The file name of the file that contains the source code to compile. </param>
	CompilerResults CompileAssemblyFromFile(CompilerParameters options, string fileName);

	/// <summary>Compiles an assembly from the specified string containing source code, using the specified compiler settings.</summary>
	/// <returns>A <see cref="T:System.CodeDom.Compiler.CompilerResults" /> object that indicates the results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the settings for compilation. </param>
	/// <param name="source">The source code to compile. </param>
	CompilerResults CompileAssemblyFromSource(CompilerParameters options, string source);

	/// <summary>Compiles an assembly based on the <see cref="N:System.CodeDom" /> trees contained in the specified array of <see cref="T:System.CodeDom.CodeCompileUnit" /> objects, using the specified compiler settings.</summary>
	/// <returns>A <see cref="T:System.CodeDom.Compiler.CompilerResults" /> object that indicates the results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the settings for compilation. </param>
	/// <param name="compilationUnits">An array of type <see cref="T:System.CodeDom.CodeCompileUnit" /> that indicates the code to compile. </param>
	CompilerResults CompileAssemblyFromDomBatch(CompilerParameters options, CodeCompileUnit[] compilationUnits);

	/// <summary>Compiles an assembly from the source code contained within the specified files, using the specified compiler settings.</summary>
	/// <returns>A <see cref="T:System.CodeDom.Compiler.CompilerResults" /> object that indicates the results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the settings for compilation. </param>
	/// <param name="fileNames">The file names of the files to compile. </param>
	CompilerResults CompileAssemblyFromFileBatch(CompilerParameters options, string[] fileNames);

	/// <summary>Compiles an assembly from the specified array of strings containing source code, using the specified compiler settings.</summary>
	/// <returns>A <see cref="T:System.CodeDom.Compiler.CompilerResults" /> object that indicates the results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the settings for compilation. </param>
	/// <param name="sources">The source code strings to compile. </param>
	CompilerResults CompileAssemblyFromSourceBatch(CompilerParameters options, string[] sources);
}
