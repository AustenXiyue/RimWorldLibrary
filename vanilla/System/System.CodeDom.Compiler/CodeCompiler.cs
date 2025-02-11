using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace System.CodeDom.Compiler;

/// <summary>Provides an example implementation of the <see cref="T:System.CodeDom.Compiler.ICodeCompiler" /> interface.</summary>
public abstract class CodeCompiler : CodeGenerator, ICodeCompiler
{
	/// <summary>Gets the file name extension to use for source files.</summary>
	/// <returns>The file name extension to use for source files.</returns>
	protected abstract string FileExtension { get; }

	/// <summary>Gets the name of the compiler executable.</summary>
	/// <returns>The name of the compiler executable.</returns>
	protected abstract string CompilerName { get; }

	/// <summary>For a description of this member, see <see cref="M:System.CodeDom.Compiler.ICodeCompiler.CompileAssemblyFromDom(System.CodeDom.Compiler.CompilerParameters,System.CodeDom.CodeCompileUnit)" />. </summary>
	/// <returns>The results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the compiler options. </param>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeCompileUnit" /> that indicates the source to compile. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="options" /> is null.</exception>
	CompilerResults ICodeCompiler.CompileAssemblyFromDom(CompilerParameters options, CodeCompileUnit e)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		try
		{
			return FromDom(options, e);
		}
		finally
		{
			options.TempFiles.SafeDelete();
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.CodeDom.Compiler.ICodeCompiler.CompileAssemblyFromFile(System.CodeDom.Compiler.CompilerParameters,System.String)" />. </summary>
	/// <returns>The results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the compiler options. </param>
	/// <param name="fileName">The file name to compile. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="options" /> is null.</exception>
	CompilerResults ICodeCompiler.CompileAssemblyFromFile(CompilerParameters options, string fileName)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		try
		{
			return FromFile(options, fileName);
		}
		finally
		{
			options.TempFiles.SafeDelete();
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.CodeDom.Compiler.ICodeCompiler.CompileAssemblyFromSource(System.CodeDom.Compiler.CompilerParameters,System.String)" />.</summary>
	/// <returns>The results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the compiler options. </param>
	/// <param name="source">A string that indicates the source code to compile. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="options" /> is null.</exception>
	CompilerResults ICodeCompiler.CompileAssemblyFromSource(CompilerParameters options, string source)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		try
		{
			return FromSource(options, source);
		}
		finally
		{
			options.TempFiles.SafeDelete();
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.CodeDom.Compiler.ICodeCompiler.CompileAssemblyFromSourceBatch(System.CodeDom.Compiler.CompilerParameters,System.String[])" />. </summary>
	/// <returns>The results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the compiler options. </param>
	/// <param name="sources">An array of strings that indicates the source code to compile. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="options" /> is null.</exception>
	CompilerResults ICodeCompiler.CompileAssemblyFromSourceBatch(CompilerParameters options, string[] sources)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		try
		{
			return FromSourceBatch(options, sources);
		}
		finally
		{
			options.TempFiles.SafeDelete();
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.CodeDom.Compiler.ICodeCompiler.CompileAssemblyFromFileBatch(System.CodeDom.Compiler.CompilerParameters,System.String[])" />. </summary>
	/// <returns>The results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the compiler options. </param>
	/// <param name="fileNames">An array of strings that indicates the file names to compile. </param>
	CompilerResults ICodeCompiler.CompileAssemblyFromFileBatch(CompilerParameters options, string[] fileNames)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		if (fileNames == null)
		{
			throw new ArgumentNullException("fileNames");
		}
		try
		{
			for (int i = 0; i < fileNames.Length; i++)
			{
				File.OpenRead(fileNames[i]).Dispose();
			}
			return FromFileBatch(options, fileNames);
		}
		finally
		{
			options.TempFiles.SafeDelete();
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.CodeDom.Compiler.ICodeCompiler.CompileAssemblyFromDomBatch(System.CodeDom.Compiler.CompilerParameters,System.CodeDom.CodeCompileUnit[])" />. </summary>
	/// <returns>The results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the compiler options. </param>
	/// <param name="ea">An array of <see cref="T:System.CodeDom.CodeCompileUnit" /> objects that indicates the source to compile. </param>
	CompilerResults ICodeCompiler.CompileAssemblyFromDomBatch(CompilerParameters options, CodeCompileUnit[] ea)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		try
		{
			return FromDomBatch(options, ea);
		}
		finally
		{
			options.TempFiles.SafeDelete();
		}
	}

	/// <summary>Compiles the specified compile unit using the specified options, and returns the results from the compilation.</summary>
	/// <returns>The results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the compiler options. </param>
	/// <param name="e">A <see cref="T:System.CodeDom.CodeCompileUnit" /> object that indicates the source to compile. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="options" /> is null.</exception>
	protected virtual CompilerResults FromDom(CompilerParameters options, CodeCompileUnit e)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		return FromDomBatch(options, new CodeCompileUnit[1] { e });
	}

	/// <summary>Compiles the specified file using the specified options, and returns the results from the compilation.</summary>
	/// <returns>The results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the compiler options. </param>
	/// <param name="fileName">The file name to compile. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="options" /> is null. -or-<paramref name="fileName" /> is null.</exception>
	protected virtual CompilerResults FromFile(CompilerParameters options, string fileName)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		File.OpenRead(fileName).Dispose();
		return FromFileBatch(options, new string[1] { fileName });
	}

	/// <summary>Compiles the specified source code string using the specified options, and returns the results from the compilation.</summary>
	/// <returns>The results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the compiler options. </param>
	/// <param name="source">The source code string to compile. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="options" /> is null.</exception>
	protected virtual CompilerResults FromSource(CompilerParameters options, string source)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		return FromSourceBatch(options, new string[1] { source });
	}

	/// <summary>Compiles the specified compile units using the specified options, and returns the results from the compilation.</summary>
	/// <returns>The results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the compiler options. </param>
	/// <param name="ea">An array of <see cref="T:System.CodeDom.CodeCompileUnit" /> objects that indicates the source to compile. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="options" /> is null.-or-<paramref name="ea" /> is null.</exception>
	protected virtual CompilerResults FromDomBatch(CompilerParameters options, CodeCompileUnit[] ea)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		if (ea == null)
		{
			throw new ArgumentNullException("ea");
		}
		string[] array = new string[ea.Length];
		for (int i = 0; i < ea.Length; i++)
		{
			if (ea[i] == null)
			{
				continue;
			}
			ResolveReferencedAssemblies(options, ea[i]);
			array[i] = options.TempFiles.AddExtension(i + FileExtension);
			using FileStream stream = new FileStream(array[i], FileMode.Create, FileAccess.Write, FileShare.Read);
			using StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
			((ICodeGenerator)this).GenerateCodeFromCompileUnit(ea[i], (TextWriter)streamWriter, base.Options);
			streamWriter.Flush();
		}
		return FromFileBatch(options, array);
	}

	private void ResolveReferencedAssemblies(CompilerParameters options, CodeCompileUnit e)
	{
		if (e.ReferencedAssemblies.Count <= 0)
		{
			return;
		}
		StringEnumerator enumerator = e.ReferencedAssemblies.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				if (!options.ReferencedAssemblies.Contains(current))
				{
					options.ReferencedAssemblies.Add(current);
				}
			}
		}
		finally
		{
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}

	/// <summary>Compiles the specified files using the specified options, and returns the results from the compilation.</summary>
	/// <returns>The results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the compiler options. </param>
	/// <param name="fileNames">An array of strings that indicates the file names of the files to compile. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="options" /> is null.-or-<paramref name="fileNames" /> is null.</exception>
	protected virtual CompilerResults FromFileBatch(CompilerParameters options, string[] fileNames)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		if (fileNames == null)
		{
			throw new ArgumentNullException("fileNames");
		}
		throw new PlatformNotSupportedException();
	}

	/// <summary>Processes the specified line from the specified <see cref="T:System.CodeDom.Compiler.CompilerResults" />.</summary>
	/// <param name="results">A <see cref="T:System.CodeDom.Compiler.CompilerResults" /> that indicates the results of compilation. </param>
	/// <param name="line">The line to process. </param>
	protected abstract void ProcessCompilerOutputLine(CompilerResults results, string line);

	/// <summary>Gets the command arguments to be passed to the compiler from the specified <see cref="T:System.CodeDom.Compiler.CompilerParameters" />.</summary>
	/// <returns>The command arguments.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> that indicates the compiler options. </param>
	protected abstract string CmdArgsFromParameters(CompilerParameters options);

	/// <summary>Gets the command arguments to use when invoking the compiler to generate a response file.</summary>
	/// <returns>The command arguments to use to generate a response file, or null if there are no response file arguments.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the compiler options. </param>
	/// <param name="cmdArgs">A command arguments string. </param>
	protected virtual string GetResponseFileCmdArgs(CompilerParameters options, string cmdArgs)
	{
		string text = options.TempFiles.AddExtension("cmdline");
		using (FileStream stream = new FileStream(text, FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			using StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
			streamWriter.Write(cmdArgs);
			streamWriter.Flush();
		}
		return "@\"" + text + "\"";
	}

	/// <summary>Compiles the specified source code strings using the specified options, and returns the results from the compilation.</summary>
	/// <returns>The results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the compiler options. </param>
	/// <param name="sources">An array of strings containing the source code to compile. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="options" /> is null.-or-<paramref name="sources" /> is null.</exception>
	protected virtual CompilerResults FromSourceBatch(CompilerParameters options, string[] sources)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		string[] array = new string[sources.Length];
		for (int i = 0; i < sources.Length; i++)
		{
			string text = options.TempFiles.AddExtension(i + FileExtension);
			using (FileStream stream = new FileStream(text, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				using StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
				streamWriter.Write(sources[i]);
				streamWriter.Flush();
			}
			array[i] = text;
		}
		return FromFileBatch(options, array);
	}

	/// <summary>Joins the specified string arrays.</summary>
	/// <returns>The concatenated string.</returns>
	/// <param name="sa">The array of strings to join. </param>
	/// <param name="separator">The separator to use. </param>
	protected static string JoinStringArray(string[] sa, string separator)
	{
		if (sa == null || sa.Length == 0)
		{
			return string.Empty;
		}
		if (sa.Length == 1)
		{
			return "\"" + sa[0] + "\"";
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < sa.Length - 1; i++)
		{
			stringBuilder.Append('"');
			stringBuilder.Append(sa[i]);
			stringBuilder.Append('"');
			stringBuilder.Append(separator);
		}
		stringBuilder.Append('"');
		stringBuilder.Append(sa[sa.Length - 1]);
		stringBuilder.Append('"');
		return stringBuilder.ToString();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.Compiler.CodeCompiler" /> class. </summary>
	protected CodeCompiler()
	{
	}
}
