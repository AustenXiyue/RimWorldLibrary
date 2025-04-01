using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

namespace System.CodeDom.Compiler;

/// <summary>Provides a base class for <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> implementations. This class is abstract. </summary>
public abstract class CodeDomProvider : Component
{
	private sealed class ConfigurationErrorsException : SystemException
	{
		public ConfigurationErrorsException(string message)
			: base(message)
		{
		}

		public ConfigurationErrorsException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			throw new PlatformNotSupportedException();
		}
	}

	private static readonly Dictionary<string, CompilerInfo> s_compilerLanguages;

	private static readonly Dictionary<string, CompilerInfo> s_compilerExtensions;

	private static readonly List<CompilerInfo> s_allCompilerInfo;

	/// <summary>Gets the default file name extension to use for source code files in the current language.</summary>
	/// <returns>A file name extension corresponding to the extension of the source files of the current language. The base implementation always returns <see cref="F:System.String.Empty" />.</returns>
	public virtual string FileExtension => string.Empty;

	/// <summary>Gets a language features identifier.</summary>
	/// <returns>A <see cref="T:System.CodeDom.Compiler.LanguageOptions" /> that indicates special features of the language.</returns>
	public virtual LanguageOptions LanguageOptions => LanguageOptions.None;

	static CodeDomProvider()
	{
		s_compilerLanguages = new Dictionary<string, CompilerInfo>(StringComparer.OrdinalIgnoreCase);
		s_compilerExtensions = new Dictionary<string, CompilerInfo>(StringComparer.OrdinalIgnoreCase);
		s_allCompilerInfo = new List<CompilerInfo>();
		AddCompilerInfo(new CompilerInfo(new CompilerParameters
		{
			WarningLevel = 4
		}, typeof(CSharpCodeProvider).FullName)
		{
			_compilerLanguages = new string[3] { "c#", "cs", "csharp" },
			_compilerExtensions = new string[2] { ".cs", "cs" }
		});
		AddCompilerInfo(new CompilerInfo(new CompilerParameters
		{
			WarningLevel = 4
		}, typeof(VBCodeProvider).FullName)
		{
			_compilerLanguages = new string[4] { "vb", "vbs", "visualbasic", "vbscript" },
			_compilerExtensions = new string[2] { ".vb", "vb" }
		});
	}

	private static void AddCompilerInfo(CompilerInfo compilerInfo)
	{
		string[] compilerLanguages = compilerInfo._compilerLanguages;
		foreach (string key in compilerLanguages)
		{
			s_compilerLanguages[key] = compilerInfo;
		}
		compilerLanguages = compilerInfo._compilerExtensions;
		foreach (string key2 in compilerLanguages)
		{
			s_compilerExtensions[key2] = compilerInfo;
		}
		s_allCompilerInfo.Add(compilerInfo);
	}

	/// <summary>Gets a <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> instance for the specified language and provider options.</summary>
	/// <returns>A CodeDOM provider that is implemented for the specified language name and options.</returns>
	/// <param name="language">The language name.</param>
	/// <param name="providerOptions">A collection of provider options from the configuration file.</param>
	public static CodeDomProvider CreateProvider(string language, IDictionary<string, string> providerOptions)
	{
		return GetCompilerInfo(language).CreateProvider(providerOptions);
	}

	/// <summary>Gets a <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> instance for the specified language.</summary>
	/// <returns>A CodeDOM provider that is implemented for the specified language name.</returns>
	/// <param name="language">The language name. </param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The <paramref name="language" /> does not have a configured provider on this computer. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="language" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public static CodeDomProvider CreateProvider(string language)
	{
		return GetCompilerInfo(language).CreateProvider();
	}

	/// <summary>Returns a language name associated with the specified file name extension, as configured in the <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> compiler configuration section.</summary>
	/// <returns>A language name associated with the file name extension, as configured in the <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> compiler configuration settings.</returns>
	/// <param name="extension">A file name extension. </param>
	/// <exception cref="T:System.Configuration.ConfigurationException">The <paramref name="extension" /> does not have a configured language provider on this computer. </exception>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The <paramref name="extension" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public static string GetLanguageFromExtension(string extension)
	{
		return (GetCompilerInfoForExtensionNoThrow(extension) ?? throw new ConfigurationErrorsException("There is no CodeDom provider defined for the language."))._compilerLanguages[0];
	}

	/// <summary>Tests whether a language has a <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> implementation configured on the computer.</summary>
	/// <returns>true if a <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> implementation is configured for the specified language; otherwise, false.</returns>
	/// <param name="language">The language name. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="language" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public static bool IsDefinedLanguage(string language)
	{
		return GetCompilerInfoForLanguageNoThrow(language) != null;
	}

	/// <summary>Tests whether a file name extension has an associated <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> implementation configured on the computer.</summary>
	/// <returns>true if a <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> implementation is configured for the specified file name extension; otherwise, false.</returns>
	/// <param name="extension">A file name extension. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="extension" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public static bool IsDefinedExtension(string extension)
	{
		return GetCompilerInfoForExtensionNoThrow(extension) != null;
	}

	/// <summary>Returns the language provider and compiler configuration settings for the specified language.</summary>
	/// <returns>A <see cref="T:System.CodeDom.Compiler.CompilerInfo" /> object populated with settings of the configured <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> implementation.</returns>
	/// <param name="language">A language name. </param>
	/// <exception cref="T:System.Configuration.ConfigurationException">The <paramref name="language" /> does not have a configured provider on this computer. </exception>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The <paramref name="language" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public static CompilerInfo GetCompilerInfo(string language)
	{
		return GetCompilerInfoForLanguageNoThrow(language) ?? throw new ConfigurationErrorsException("There is no CodeDom provider defined for the language.");
	}

	private static CompilerInfo GetCompilerInfoForLanguageNoThrow(string language)
	{
		if (language == null)
		{
			throw new ArgumentNullException("language");
		}
		s_compilerLanguages.TryGetValue(language.Trim(), out var value);
		return value;
	}

	private static CompilerInfo GetCompilerInfoForExtensionNoThrow(string extension)
	{
		if (extension == null)
		{
			throw new ArgumentNullException("extension");
		}
		s_compilerExtensions.TryGetValue(extension.Trim(), out var value);
		return value;
	}

	/// <summary>Returns the language provider and compiler configuration settings for this computer.</summary>
	/// <returns>An array of type <see cref="T:System.CodeDom.Compiler.CompilerInfo" /> representing the settings of all configured <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> implementations.</returns>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public static CompilerInfo[] GetAllCompilerInfo()
	{
		return s_allCompilerInfo.ToArray();
	}

	/// <summary>When overridden in a derived class, creates a new code generator.</summary>
	/// <returns>An <see cref="T:System.CodeDom.Compiler.ICodeGenerator" /> that can be used to generate <see cref="N:System.CodeDom" /> based source code representations.</returns>
	[Obsolete("Callers should not use the ICodeGenerator interface and should instead use the methods directly on the CodeDomProvider class. Those inheriting from CodeDomProvider must still implement this interface, and should exclude this warning or also obsolete this method.")]
	public abstract ICodeGenerator CreateGenerator();

	/// <summary>When overridden in a derived class, creates a new code generator using the specified <see cref="T:System.IO.TextWriter" /> for output.</summary>
	/// <returns>An <see cref="T:System.CodeDom.Compiler.ICodeGenerator" /> that can be used to generate <see cref="N:System.CodeDom" /> based source code representations.</returns>
	/// <param name="output">A <see cref="T:System.IO.TextWriter" /> to use to output. </param>
	public virtual ICodeGenerator CreateGenerator(TextWriter output)
	{
		return CreateGenerator();
	}

	/// <summary>When overridden in a derived class, creates a new code generator using the specified file name for output.</summary>
	/// <returns>An <see cref="T:System.CodeDom.Compiler.ICodeGenerator" /> that can be used to generate <see cref="N:System.CodeDom" /> based source code representations.</returns>
	/// <param name="fileName">The file name to output to. </param>
	public virtual ICodeGenerator CreateGenerator(string fileName)
	{
		return CreateGenerator();
	}

	/// <summary>When overridden in a derived class, creates a new code compiler. </summary>
	/// <returns>An <see cref="T:System.CodeDom.Compiler.ICodeCompiler" /> that can be used for compilation of <see cref="N:System.CodeDom" /> based source code representations. </returns>
	[Obsolete("Callers should not use the ICodeCompiler interface and should instead use the methods directly on the CodeDomProvider class. Those inheriting from CodeDomProvider must still implement this interface, and should exclude this warning or also obsolete this method.")]
	public abstract ICodeCompiler CreateCompiler();

	/// <summary>When overridden in a derived class, creates a new code parser.</summary>
	/// <returns>An <see cref="T:System.CodeDom.Compiler.ICodeParser" /> that can be used to parse source code. The base implementation always returns null.</returns>
	[Obsolete("Callers should not use the ICodeParser interface and should instead use the methods directly on the CodeDomProvider class. Those inheriting from CodeDomProvider must still implement this interface, and should exclude this warning or also obsolete this method.")]
	public virtual ICodeParser CreateParser()
	{
		return null;
	}

	/// <summary>Gets a <see cref="T:System.ComponentModel.TypeConverter" /> for the specified data type.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.TypeConverter" /> for the specified type, or null if a <see cref="T:System.ComponentModel.TypeConverter" /> for the specified type cannot be found.</returns>
	/// <param name="type">The type of object to retrieve a type converter for. </param>
	public virtual TypeConverter GetConverter(Type type)
	{
		return TypeDescriptor.GetConverter(type);
	}

	/// <summary>Compiles an assembly based on the <see cref="N:System.CodeDom" /> trees contained in the specified array of <see cref="T:System.CodeDom.CodeCompileUnit" /> objects, using the specified compiler settings.</summary>
	/// <returns>A <see cref="T:System.CodeDom.Compiler.CompilerResults" /> object that indicates the results of the compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the settings for the compilation.</param>
	/// <param name="compilationUnits">An array of type <see cref="T:System.CodeDom.CodeCompileUnit" /> that indicates the code to compile.</param>
	/// <exception cref="T:System.NotImplementedException">Neither this method nor the <see cref="M:System.CodeDom.Compiler.CodeDomProvider.CreateCompiler" /> method is overridden in a derived class.</exception>
	public virtual CompilerResults CompileAssemblyFromDom(CompilerParameters options, params CodeCompileUnit[] compilationUnits)
	{
		return CreateCompilerHelper().CompileAssemblyFromDomBatch(options, compilationUnits);
	}

	/// <summary>Compiles an assembly from the source code contained in the specified files, using the specified compiler settings.</summary>
	/// <returns>A <see cref="T:System.CodeDom.Compiler.CompilerResults" /> object that indicates the results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the settings for the compilation. </param>
	/// <param name="fileNames">An array of the names of the files to compile. </param>
	/// <exception cref="T:System.NotImplementedException">Neither this method nor the <see cref="M:System.CodeDom.Compiler.CodeDomProvider.CreateCompiler" /> method is overridden in a derived class.</exception>
	public virtual CompilerResults CompileAssemblyFromFile(CompilerParameters options, params string[] fileNames)
	{
		return CreateCompilerHelper().CompileAssemblyFromFileBatch(options, fileNames);
	}

	/// <summary>Compiles an assembly from the specified array of strings containing source code, using the specified compiler settings.</summary>
	/// <returns>A <see cref="T:System.CodeDom.Compiler.CompilerResults" /> object that indicates the results of compilation.</returns>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CompilerParameters" /> object that indicates the compiler settings for this compilation. </param>
	/// <param name="sources">An array of source code strings to compile. </param>
	/// <exception cref="T:System.NotImplementedException">Neither this method nor the <see cref="M:System.CodeDom.Compiler.CodeDomProvider.CreateCompiler" /> method is overridden in a derived class.</exception>
	public virtual CompilerResults CompileAssemblyFromSource(CompilerParameters options, params string[] sources)
	{
		return CreateCompilerHelper().CompileAssemblyFromSourceBatch(options, sources);
	}

	/// <summary>Returns a value that indicates whether the specified value is a valid identifier for the current language.</summary>
	/// <returns>true if the <paramref name="value" /> parameter is a valid identifier; otherwise, false.</returns>
	/// <param name="value">The value to verify as a valid identifier.</param>
	/// <exception cref="T:System.NotImplementedException">Neither this method nor the <see cref="M:System.CodeDom.Compiler.CodeDomProvider.CreateGenerator" /> method is overridden in a derived class.</exception>
	public virtual bool IsValidIdentifier(string value)
	{
		return CreateGeneratorHelper().IsValidIdentifier(value);
	}

	/// <summary>Creates an escaped identifier for the specified value.</summary>
	/// <returns>The escaped identifier for the value.</returns>
	/// <param name="value">The string for which to create an escaped identifier.</param>
	/// <exception cref="T:System.NotImplementedException">Neither this method nor the <see cref="M:System.CodeDom.Compiler.CodeDomProvider.CreateGenerator" /> method is overridden in a derived class.</exception>
	public virtual string CreateEscapedIdentifier(string value)
	{
		return CreateGeneratorHelper().CreateEscapedIdentifier(value);
	}

	/// <summary>Creates a valid identifier for the specified value.</summary>
	/// <returns>A valid identifier for the specified value.</returns>
	/// <param name="value">The string for which to generate a valid identifier.</param>
	/// <exception cref="T:System.NotImplementedException">Neither this method nor the <see cref="M:System.CodeDom.Compiler.CodeDomProvider.CreateGenerator" /> method is overridden in a derived class.</exception>
	public virtual string CreateValidIdentifier(string value)
	{
		return CreateGeneratorHelper().CreateValidIdentifier(value);
	}

	/// <summary>Gets the type indicated by the specified <see cref="T:System.CodeDom.CodeTypeReference" />.</summary>
	/// <returns>A text representation of the specified type, formatted for the language in which code is generated by this code generator. In Visual Basic, for example, passing in a <see cref="T:System.CodeDom.CodeTypeReference" /> for the <see cref="T:System.Int32" /> type will return "Integer".</returns>
	/// <param name="type">A <see cref="T:System.CodeDom.CodeTypeReference" /> that indicates the type to return.</param>
	/// <exception cref="T:System.NotImplementedException">Neither this method nor the <see cref="M:System.CodeDom.Compiler.CodeDomProvider.CreateGenerator" /> method is overridden in a derived class.</exception>
	public virtual string GetTypeOutput(CodeTypeReference type)
	{
		return CreateGeneratorHelper().GetTypeOutput(type);
	}

	/// <summary>Returns a value indicating whether the specified code generation support is provided.</summary>
	/// <returns>true if the specified code generation support is provided; otherwise, false.</returns>
	/// <param name="generatorSupport">A <see cref="T:System.CodeDom.Compiler.GeneratorSupport" /> object that indicates the type of code generation support to verify.</param>
	/// <exception cref="T:System.NotImplementedException">Neither this method nor the <see cref="M:System.CodeDom.Compiler.CodeDomProvider.CreateGenerator" /> method is overridden in a derived class.</exception>
	public virtual bool Supports(GeneratorSupport generatorSupport)
	{
		return CreateGeneratorHelper().Supports(generatorSupport);
	}

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) expression and sends it to the specified text writer, using the specified options.</summary>
	/// <param name="expression">A <see cref="T:System.CodeDom.CodeExpression" /> object that indicates the expression for which to generate code. </param>
	/// <param name="writer">The <see cref="T:System.IO.TextWriter" /> to which output code is sent. </param>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CodeGeneratorOptions" /> that indicates the options to use for generating code. </param>
	/// <exception cref="T:System.NotImplementedException">Neither this method nor the <see cref="M:System.CodeDom.Compiler.CodeDomProvider.CreateGenerator" /> method is overridden in a derived class.</exception>
	public virtual void GenerateCodeFromExpression(CodeExpression expression, TextWriter writer, CodeGeneratorOptions options)
	{
		CreateGeneratorHelper().GenerateCodeFromExpression(expression, writer, options);
	}

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) statement and sends it to the specified text writer, using the specified options.</summary>
	/// <param name="statement">A <see cref="T:System.CodeDom.CodeStatement" /> containing the CodeDOM elements for which to generate code. </param>
	/// <param name="writer">The <see cref="T:System.IO.TextWriter" /> to which output code is sent. </param>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CodeGeneratorOptions" /> that indicates the options to use for generating code. </param>
	/// <exception cref="T:System.NotImplementedException">Neither this method nor the <see cref="M:System.CodeDom.Compiler.CodeDomProvider.CreateGenerator" /> method is overridden in a derived class.</exception>
	public virtual void GenerateCodeFromStatement(CodeStatement statement, TextWriter writer, CodeGeneratorOptions options)
	{
		CreateGeneratorHelper().GenerateCodeFromStatement(statement, writer, options);
	}

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) namespace and sends it to the specified text writer, using the specified options.</summary>
	/// <param name="codeNamespace">A <see cref="T:System.CodeDom.CodeNamespace" /> object that indicates the namespace for which to generate code. </param>
	/// <param name="writer">The <see cref="T:System.IO.TextWriter" /> to which output code is sent. </param>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CodeGeneratorOptions" /> that indicates the options to use for generating code. </param>
	/// <exception cref="T:System.NotImplementedException">Neither this method nor the <see cref="M:System.CodeDom.Compiler.CodeDomProvider.CreateGenerator" /> method is overridden in a derived class.</exception>
	public virtual void GenerateCodeFromNamespace(CodeNamespace codeNamespace, TextWriter writer, CodeGeneratorOptions options)
	{
		CreateGeneratorHelper().GenerateCodeFromNamespace(codeNamespace, writer, options);
	}

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) compilation unit and sends it to the specified text writer, using the specified options.</summary>
	/// <param name="compileUnit">A <see cref="T:System.CodeDom.CodeCompileUnit" /> for which to generate code. </param>
	/// <param name="writer">The <see cref="T:System.IO.TextWriter" /> to which the output code is sent. </param>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CodeGeneratorOptions" /> that indicates the options to use for generating code. </param>
	/// <exception cref="T:System.NotImplementedException">Neither this method nor the <see cref="M:System.CodeDom.Compiler.CodeDomProvider.CreateGenerator" /> method is overridden in a derived class.</exception>
	public virtual void GenerateCodeFromCompileUnit(CodeCompileUnit compileUnit, TextWriter writer, CodeGeneratorOptions options)
	{
		CreateGeneratorHelper().GenerateCodeFromCompileUnit(compileUnit, writer, options);
	}

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) type declaration and sends it to the specified text writer, using the specified options.</summary>
	/// <param name="codeType">A <see cref="T:System.CodeDom.CodeTypeDeclaration" /> object that indicates the type for which to generate code. </param>
	/// <param name="writer">The <see cref="T:System.IO.TextWriter" /> to which output code is sent. </param>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CodeGeneratorOptions" /> that indicates the options to use for generating code. </param>
	/// <exception cref="T:System.NotImplementedException">Neither this method nor the <see cref="M:System.CodeDom.Compiler.CodeDomProvider.CreateGenerator" /> method is overridden in a derived class.</exception>
	public virtual void GenerateCodeFromType(CodeTypeDeclaration codeType, TextWriter writer, CodeGeneratorOptions options)
	{
		CreateGeneratorHelper().GenerateCodeFromType(codeType, writer, options);
	}

	/// <summary>Generates code for the specified Code Document Object Model (CodeDOM) member declaration and sends it to the specified text writer, using the specified options.</summary>
	/// <param name="member">A <see cref="T:System.CodeDom.CodeTypeMember" /> object that indicates the member for which to generate code. </param>
	/// <param name="writer">The <see cref="T:System.IO.TextWriter" /> to which output code is sent. </param>
	/// <param name="options">A <see cref="T:System.CodeDom.Compiler.CodeGeneratorOptions" /> that indicates the options to use for generating code. </param>
	/// <exception cref="T:System.NotImplementedException">This method is not overridden in a derived class.</exception>
	public virtual void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
	{
		throw new NotImplementedException("This CodeDomProvider does not support this method.");
	}

	/// <summary>Compiles the code read from the specified text stream into a <see cref="T:System.CodeDom.CodeCompileUnit" />.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeCompileUnit" /> that contains a representation of the parsed code.</returns>
	/// <param name="codeStream">A <see cref="T:System.IO.TextReader" /> object that is used to read the code to be parsed. </param>
	/// <exception cref="T:System.NotImplementedException">Neither this method nor the <see cref="M:System.CodeDom.Compiler.CodeDomProvider.CreateGenerator" /> method is overridden in a derived class.</exception>
	public virtual CodeCompileUnit Parse(TextReader codeStream)
	{
		return CreateParserHelper().Parse(codeStream);
	}

	private ICodeCompiler CreateCompilerHelper()
	{
		return CreateCompiler() ?? throw new NotImplementedException("This CodeDomProvider does not support this method.");
	}

	private ICodeGenerator CreateGeneratorHelper()
	{
		return CreateGenerator() ?? throw new NotImplementedException("This CodeDomProvider does not support this method.");
	}

	private ICodeParser CreateParserHelper()
	{
		return CreateParser() ?? throw new NotImplementedException("This CodeDomProvider does not support this method.");
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> class. </summary>
	protected CodeDomProvider()
	{
	}
}
