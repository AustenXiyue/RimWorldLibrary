using System.Collections.Specialized;
using System.Reflection;
using System.Security.Policy;

namespace System.CodeDom.Compiler;

/// <summary>Represents the results of compilation that are returned from a compiler.</summary>
[Serializable]
public class CompilerResults
{
	private Evidence _evidence;

	private readonly CompilerErrorCollection _errors = new CompilerErrorCollection();

	private readonly StringCollection _output = new StringCollection();

	private Assembly _compiledAssembly;

	private TempFileCollection _tempFiles;

	/// <summary>Indicates the evidence object that represents the security policy permissions of the compiled assembly.</summary>
	/// <returns>An <see cref="T:System.Security.Policy.Evidence" /> object that represents the security policy permissions of the compiled assembly.</returns>
	[Obsolete("CAS policy is obsolete and will be removed in a future release of the .NET Framework. Please see http://go2.microsoft.com/fwlink/?LinkId=131738 for more information.")]
	public Evidence Evidence
	{
		get
		{
			return _evidence?.Clone();
		}
		set
		{
			_evidence = value?.Clone();
		}
	}

	/// <summary>Gets or sets the temporary file collection to use.</summary>
	/// <returns>A <see cref="T:System.CodeDom.Compiler.TempFileCollection" /> with which to manage and store references to intermediate files generated during compilation.</returns>
	public TempFileCollection TempFiles
	{
		get
		{
			return _tempFiles;
		}
		set
		{
			_tempFiles = value;
		}
	}

	/// <summary>Gets or sets the compiled assembly.</summary>
	/// <returns>An <see cref="T:System.Reflection.Assembly" /> that indicates the compiled assembly.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Assembly CompiledAssembly
	{
		get
		{
			if (_compiledAssembly == null && PathToAssembly != null)
			{
				_compiledAssembly = Assembly.Load(new AssemblyName
				{
					CodeBase = PathToAssembly
				});
			}
			return _compiledAssembly;
		}
		set
		{
			_compiledAssembly = value;
		}
	}

	/// <summary>Gets the collection of compiler errors and warnings.</summary>
	/// <returns>A <see cref="T:System.CodeDom.Compiler.CompilerErrorCollection" /> that indicates the errors and warnings resulting from compilation, if any.</returns>
	public CompilerErrorCollection Errors => _errors;

	/// <summary>Gets the compiler output messages.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.StringCollection" /> that contains the output messages.</returns>
	public StringCollection Output => _output;

	/// <summary>Gets or sets the path of the compiled assembly.</summary>
	/// <returns>The path of the assembly, or null if the assembly was generated in memory.</returns>
	public string PathToAssembly { get; set; }

	/// <summary>Gets or sets the compiler's return value.</summary>
	/// <returns>The compiler's return value.</returns>
	public int NativeCompilerReturnValue { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.Compiler.CompilerResults" /> class that uses the specified temporary files.</summary>
	/// <param name="tempFiles">A <see cref="T:System.CodeDom.Compiler.TempFileCollection" /> with which to manage and store references to intermediate files generated during compilation. </param>
	public CompilerResults(TempFileCollection tempFiles)
	{
		_tempFiles = tempFiles;
	}
}
