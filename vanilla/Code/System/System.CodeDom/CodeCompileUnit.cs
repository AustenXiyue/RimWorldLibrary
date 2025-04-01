using System.Collections.Specialized;

namespace System.CodeDom;

/// <summary>Provides a container for a CodeDOM program graph.</summary>
[Serializable]
public class CodeCompileUnit : CodeObject
{
	private StringCollection _assemblies;

	private CodeAttributeDeclarationCollection _attributes;

	private CodeDirectiveCollection _startDirectives;

	private CodeDirectiveCollection _endDirectives;

	/// <summary>Gets the collection of namespaces.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeNamespaceCollection" /> that indicates the namespaces that the compile unit uses.</returns>
	public CodeNamespaceCollection Namespaces { get; } = new CodeNamespaceCollection();

	/// <summary>Gets the referenced assemblies.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.StringCollection" /> that contains the file names of the referenced assemblies.</returns>
	public StringCollection ReferencedAssemblies => _assemblies ?? (_assemblies = new StringCollection());

	/// <summary>Gets a collection of custom attributes for the generated assembly.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeAttributeDeclarationCollection" /> that indicates the custom attributes for the generated assembly.</returns>
	public CodeAttributeDeclarationCollection AssemblyCustomAttributes => _attributes ?? (_attributes = new CodeAttributeDeclarationCollection());

	/// <summary>Gets a <see cref="T:System.CodeDom.CodeDirectiveCollection" /> object containing start directives.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeDirectiveCollection" /> object containing start directives.</returns>
	public CodeDirectiveCollection StartDirectives => _startDirectives ?? (_startDirectives = new CodeDirectiveCollection());

	/// <summary>Gets a <see cref="T:System.CodeDom.CodeDirectiveCollection" /> object containing end directives.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeDirectiveCollection" /> object containing end directives.</returns>
	public CodeDirectiveCollection EndDirectives => _endDirectives ?? (_endDirectives = new CodeDirectiveCollection());

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCompileUnit" /> class.</summary>
	public CodeCompileUnit()
	{
	}
}
