using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace Microsoft.CSharp;

/// <summary>Provides access to instances of the C# code generator and code compiler.</summary>
public class CSharpCodeProvider : CodeDomProvider
{
	private readonly CSharpCodeGenerator _generator;

	/// <summary>Gets the file name extension to use when creating source code files.</summary>
	/// <returns>The file name extension to use for generated source code files.</returns>
	public override string FileExtension => "cs";

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.CSharp.CSharpCodeProvider" /> class. </summary>
	public CSharpCodeProvider()
	{
		_generator = new CSharpCodeGenerator();
	}

	/// <summary>Initializes a new instance of the <see cref="T:Microsoft.CSharp.CSharpCodeProvider" /> class by using the specified provider options. </summary>
	/// <param name="providerOptions">A <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains the provider options from the configuration file.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="providerOptions" /> is null.</exception>
	public CSharpCodeProvider(IDictionary<string, string> providerOptions)
	{
		if (providerOptions == null)
		{
			throw new ArgumentNullException("providerOptions");
		}
		_generator = new CSharpCodeGenerator(providerOptions);
	}

	/// <summary>Gets an instance of the C# code generator.</summary>
	/// <returns>An instance of the C# <see cref="T:System.CodeDom.Compiler.ICodeGenerator" /> implementation.</returns>
	[Obsolete("Callers should not use the ICodeGenerator interface and should instead use the methods directly on the CodeDomProvider class.")]
	public override ICodeGenerator CreateGenerator()
	{
		return _generator;
	}

	/// <summary>Gets an instance of the C# code compiler.</summary>
	/// <returns>An instance of the C# <see cref="T:System.CodeDom.Compiler.ICodeCompiler" /> implementation.</returns>
	[Obsolete("Callers should not use the ICodeCompiler interface and should instead use the methods directly on the CodeDomProvider class.")]
	public override ICodeCompiler CreateCompiler()
	{
		return _generator;
	}

	/// <summary>Gets a <see cref="T:System.ComponentModel.TypeConverter" /> for the specified type of object.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.TypeConverter" /> for the specified type.</returns>
	/// <param name="type">The type of object to retrieve a type converter for. </param>
	public override TypeConverter GetConverter(Type type)
	{
		if (!(type == typeof(MemberAttributes)))
		{
			if (!(type == typeof(TypeAttributes)))
			{
				return base.GetConverter(type);
			}
			return CSharpTypeAttributeConverter.Default;
		}
		return CSharpMemberAttributeConverter.Default;
	}

	/// <summary>Generates code for the specified class member using the specified text writer and code generator options.</summary>
	/// <param name="member">A <see cref="T:System.CodeDom.CodeTypeMember" /> to generate code for.</param>
	/// <param name="writer">The <see cref="T:System.IO.TextWriter" /> to write to.</param>
	/// <param name="options">The <see cref="T:System.CodeDom.Compiler.CodeGeneratorOptions" /> to use when generating the code.</param>
	public override void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
	{
		_generator.GenerateCodeFromMember(member, writer, options);
	}
}
