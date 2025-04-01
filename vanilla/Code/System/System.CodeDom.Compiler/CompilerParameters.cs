using System.Collections.Specialized;
using System.Security.Policy;

namespace System.CodeDom.Compiler;

/// <summary>Represents the parameters used to invoke a compiler.</summary>
[Serializable]
public class CompilerParameters
{
	private Evidence _evidence;

	private readonly StringCollection _assemblyNames = new StringCollection();

	private readonly StringCollection _embeddedResources = new StringCollection();

	private readonly StringCollection _linkedResources = new StringCollection();

	private TempFileCollection _tempFiles;

	/// <summary>Specifies an evidence object that represents the security policy permissions to grant the compiled assembly.</summary>
	/// <returns>An  object that represents the security policy permissions to grant the compiled assembly.</returns>
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

	/// <summary>Gets or sets the name of the core or standard assembly that contains basic types such as <see cref="T:System.Object" />, <see cref="T:System.String" />, or <see cref="T:System.Int32" />.</summary>
	/// <returns>The name of the core assembly that contains basic types.</returns>
	public string CoreAssemblyFileName { get; set; } = string.Empty;

	/// <summary>Gets or sets a value indicating whether to generate an executable.</summary>
	/// <returns>true if an executable should be generated; otherwise, false.</returns>
	public bool GenerateExecutable { get; set; }

	/// <summary>Gets or sets a value indicating whether to generate the output in memory.</summary>
	/// <returns>true if the compiler should generate the output in memory; otherwise, false.</returns>
	public bool GenerateInMemory { get; set; }

	/// <summary>Gets the assemblies referenced by the current project.</summary>
	/// <returns>A collection that contains the assembly names that are referenced by the source to compile.</returns>
	public StringCollection ReferencedAssemblies => _assemblyNames;

	/// <summary>Gets or sets the name of the main class.</summary>
	/// <returns>The name of the main class.</returns>
	public string MainClass { get; set; }

	/// <summary>Gets or sets the name of the output assembly.</summary>
	/// <returns>The name of the output assembly.</returns>
	public string OutputAssembly { get; set; }

	/// <summary>Gets or sets the collection that contains the temporary files.</summary>
	/// <returns>A collection that contains the temporary files.</returns>
	public TempFileCollection TempFiles
	{
		get
		{
			return _tempFiles ?? (_tempFiles = new TempFileCollection());
		}
		set
		{
			_tempFiles = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to include debug information in the compiled executable.</summary>
	/// <returns>true if debug information should be generated; otherwise, false.</returns>
	public bool IncludeDebugInformation { get; set; }

	/// <summary>Gets or sets a value indicating whether to treat warnings as errors.</summary>
	/// <returns>true if warnings should be treated as errors; otherwise, false.</returns>
	public bool TreatWarningsAsErrors { get; set; }

	/// <summary>Gets or sets the warning level at which the compiler aborts compilation.</summary>
	/// <returns>The warning level at which the compiler aborts compilation.</returns>
	public int WarningLevel { get; set; } = -1;

	/// <summary>Gets or sets optional command-line arguments to use when invoking the compiler.</summary>
	/// <returns>Any additional command-line arguments for the compiler.</returns>
	public string CompilerOptions { get; set; }

	/// <summary>Gets or sets the file name of a Win32 resource file to link into the compiled assembly.</summary>
	/// <returns>A Win32 resource file that will be linked into the compiled assembly.</returns>
	public string Win32Resource { get; set; }

	/// <summary>Gets the .NET Framework resource files to include when compiling the assembly output.</summary>
	/// <returns>A collection that contains the file paths of .NET Framework resources to include in the generated assembly.</returns>
	public StringCollection EmbeddedResources => _embeddedResources;

	/// <summary>Gets the .NET Framework resource files that are referenced in the current source.</summary>
	/// <returns>A collection that contains the file paths of .NET Framework resources that are referenced by the source.</returns>
	public StringCollection LinkedResources => _linkedResources;

	/// <summary>Gets or sets the user token to use when creating the compiler process.</summary>
	/// <returns>The user token to use.</returns>
	public IntPtr UserToken { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> class.</summary>
	public CompilerParameters()
		: this(null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> class using the specified assembly names.</summary>
	/// <param name="assemblyNames">The names of the assemblies to reference. </param>
	public CompilerParameters(string[] assemblyNames)
		: this(assemblyNames, null, includeDebugInformation: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> class using the specified assembly names and output file name.</summary>
	/// <param name="assemblyNames">The names of the assemblies to reference. </param>
	/// <param name="outputName">The output file name. </param>
	public CompilerParameters(string[] assemblyNames, string outputName)
		: this(assemblyNames, outputName, includeDebugInformation: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> class using the specified assembly names, output name, and a value indicating whether to include debug information.</summary>
	/// <param name="assemblyNames">The names of the assemblies to reference. </param>
	/// <param name="outputName">The output file name. </param>
	/// <param name="includeDebugInformation">true to include debug information; false to exclude debug information. </param>
	public CompilerParameters(string[] assemblyNames, string outputName, bool includeDebugInformation)
	{
		if (assemblyNames != null)
		{
			ReferencedAssemblies.AddRange(assemblyNames);
		}
		OutputAssembly = outputName;
		IncludeDebugInformation = includeDebugInformation;
	}
}
