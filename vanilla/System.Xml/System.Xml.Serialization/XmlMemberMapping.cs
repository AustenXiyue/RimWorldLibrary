using System.CodeDom.Compiler;
using Unity;

namespace System.Xml.Serialization;

/// <summary>Maps a code entity in a .NET Framework Web service method to an element in a Web Services Description Language (WSDL) message.</summary>
public class XmlMemberMapping
{
	private MemberMapping mapping;

	internal MemberMapping Mapping => mapping;

	internal Accessor Accessor => mapping.Accessor;

	/// <summary>Gets or sets a value that indicates whether the .NET Framework type maps to an XML element or attribute of any type. </summary>
	/// <returns>true, if the type maps to an XML any element or attribute; otherwise, false.</returns>
	public bool Any => Accessor.Any;

	/// <summary>Gets the unqualified name of the XML element declaration that applies to this mapping. </summary>
	/// <returns>The unqualified name of the XML element declaration that applies to this mapping.</returns>
	public string ElementName => Accessor.UnescapeName(Accessor.Name);

	/// <summary>Gets the XML element name as it appears in the service description document.</summary>
	/// <returns>The XML element name.</returns>
	public string XsdElementName => Accessor.Name;

	/// <summary>Gets the XML namespace that applies to this mapping. </summary>
	/// <returns>The XML namespace that applies to this mapping.</returns>
	public string Namespace => Accessor.Namespace;

	/// <summary>Gets the name of the Web service method member that is represented by this mapping. </summary>
	/// <returns>The name of the Web service method member represented by this mapping.</returns>
	public string MemberName => mapping.Name;

	/// <summary>Gets the type name of the .NET Framework type for this mapping. </summary>
	/// <returns>The type name of the .NET Framework type for this mapping.</returns>
	public string TypeName
	{
		get
		{
			if (Accessor.Mapping == null)
			{
				return string.Empty;
			}
			return Accessor.Mapping.TypeName;
		}
	}

	/// <summary>Gets the namespace of the .NET Framework type for this mapping.</summary>
	/// <returns>The namespace of the .NET Framework type for this mapping.</returns>
	public string TypeNamespace
	{
		get
		{
			if (Accessor.Mapping == null)
			{
				return null;
			}
			return Accessor.Mapping.Namespace;
		}
	}

	/// <summary>Gets the fully qualified type name of the .NET Framework type for this mapping. </summary>
	/// <returns>The fully qualified type name of the .NET Framework type for this mapping.</returns>
	public string TypeFullName => mapping.TypeDesc.FullName;

	/// <summary>Gets a value that indicates whether the accompanying field in the .NET Framework type has a value specified.</summary>
	/// <returns>true, if the accompanying field has a value specified; otherwise, false.</returns>
	public bool CheckSpecified => mapping.CheckSpecified != SpecifiedAccessor.None;

	internal bool IsNullable => mapping.IsNeedNullable;

	internal XmlMemberMapping(MemberMapping mapping)
	{
		this.mapping = mapping;
	}

	/// <summary>Returns the name of the type associated with the specified <see cref="T:System.CodeDom.Compiler.CodeDomProvider" />.</summary>
	/// <returns>The name of the type.</returns>
	/// <param name="codeProvider">A <see cref="T:System.CodeDom.Compiler.CodeDomProvider" />  that contains the name of the type.</param>
	public string GenerateTypeName(CodeDomProvider codeProvider)
	{
		return mapping.GetTypeName(codeProvider);
	}

	internal XmlMemberMapping()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
