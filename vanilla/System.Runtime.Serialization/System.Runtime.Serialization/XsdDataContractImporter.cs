using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization.Diagnostics;
using System.Security;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Schema;

namespace System.Runtime.Serialization;

/// <summary>Allows the transformation of a set of XML schema files (.xsd) into common language runtime (CLR) types. </summary>
public class XsdDataContractImporter
{
	private ImportOptions options;

	private CodeCompileUnit codeCompileUnit;

	private DataContractSet dataContractSet;

	private static readonly XmlQualifiedName[] emptyTypeNameArray = new XmlQualifiedName[0];

	private static readonly XmlSchemaElement[] emptyElementArray = new XmlSchemaElement[0];

	private XmlQualifiedName[] singleTypeNameArray;

	private XmlSchemaElement[] singleElementArray;

	/// <summary>Gets or sets an <see cref="T:System.Runtime.Serialization.ImportOptions" /> that contains settable options for the import operation. </summary>
	/// <returns>A <see cref="T:System.Runtime.Serialization.ImportOptions" /> that contains settable options. </returns>
	public ImportOptions Options
	{
		get
		{
			return options;
		}
		set
		{
			options = value;
		}
	}

	/// <summary>Gets a <see cref="T:System.CodeDom.CodeCompileUnit" /> used for storing the CLR types generated.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeCompileUnit" /> used to store the CLR types generated.</returns>
	public CodeCompileUnit CodeCompileUnit => GetCodeCompileUnit();

	private DataContractSet DataContractSet
	{
		get
		{
			if (dataContractSet == null)
			{
				dataContractSet = ((Options == null) ? new DataContractSet(null, null, null) : new DataContractSet(Options.DataContractSurrogate, Options.ReferencedTypes, Options.ReferencedCollectionTypes));
			}
			return dataContractSet;
		}
	}

	private XmlQualifiedName[] SingleTypeNameArray
	{
		get
		{
			if (singleTypeNameArray == null)
			{
				singleTypeNameArray = new XmlQualifiedName[1];
			}
			return singleTypeNameArray;
		}
	}

	private XmlSchemaElement[] SingleElementArray
	{
		get
		{
			if (singleElementArray == null)
			{
				singleElementArray = new XmlSchemaElement[1];
			}
			return singleElementArray;
		}
	}

	private bool ImportXmlDataType
	{
		get
		{
			if (Options != null)
			{
				return Options.ImportXmlType;
			}
			return false;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.XsdDataContractImporter" /> class.  </summary>
	public XsdDataContractImporter()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.XsdDataContractImporter" /> class with the <see cref="T:System.CodeDom.CodeCompileUnit" /> that will be used to generate CLR code. </summary>
	/// <param name="codeCompileUnit">The <see cref="T:System.CodeDom.CodeCompileUnit" /> that will be used to store the code. </param>
	public XsdDataContractImporter(CodeCompileUnit codeCompileUnit)
	{
		this.codeCompileUnit = codeCompileUnit;
	}

	private CodeCompileUnit GetCodeCompileUnit()
	{
		if (codeCompileUnit == null)
		{
			codeCompileUnit = new CodeCompileUnit();
		}
		return codeCompileUnit;
	}

	/// <summary>Transforms the specified set of XML schemas contained in an <see cref="T:System.Xml.Schema.XmlSchemaSet" /> into a <see cref="T:System.CodeDom.CodeCompileUnit" />. </summary>
	/// <param name="schemas">A <see cref="T:System.Xml.Schema.XmlSchemaSet" /> that contains the schema representations to generate CLR types for.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="schemas" /> parameter is null.</exception>
	public void Import(XmlSchemaSet schemas)
	{
		if (schemas == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("schemas"));
		}
		InternalImport(schemas, null, null, null);
	}

	/// <summary>Transforms the specified set of schema types contained in an <see cref="T:System.Xml.Schema.XmlSchemaSet" /> into CLR types generated into a <see cref="T:System.CodeDom.CodeCompileUnit" />.</summary>
	/// <param name="schemas">A <see cref="T:System.Xml.Schema.XmlSchemaSet" /> that contains the schema representations.</param>
	/// <param name="typeNames">A <see cref="T:System.Collections.Generic.ICollection`1" />  (of <see cref="T:System.Xml.XmlQualifiedName" />) that represents the set of schema types to import.</param>
	public void Import(XmlSchemaSet schemas, ICollection<XmlQualifiedName> typeNames)
	{
		if (schemas == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("schemas"));
		}
		if (typeNames == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("typeNames"));
		}
		InternalImport(schemas, typeNames, emptyElementArray, emptyTypeNameArray);
	}

	/// <summary>Transforms the specified XML schema type contained in an <see cref="T:System.Xml.Schema.XmlSchemaSet" /> into a <see cref="T:System.CodeDom.CodeCompileUnit" />.</summary>
	/// <param name="schemas">A <see cref="T:System.Xml.Schema.XmlSchemaSet" /> that contains the schema representations. </param>
	/// <param name="typeName">A <see cref="T:System.Xml.XmlQualifiedName" /> that represents a specific schema type to import.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="schemas" /> or <paramref name="typeName" /> parameter is null.</exception>
	public void Import(XmlSchemaSet schemas, XmlQualifiedName typeName)
	{
		if (schemas == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("schemas"));
		}
		if (typeName == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("typeName"));
		}
		SingleTypeNameArray[0] = typeName;
		InternalImport(schemas, SingleTypeNameArray, emptyElementArray, emptyTypeNameArray);
	}

	/// <summary>Transforms the specified schema element in the set of specified XML schemas into a <see cref="T:System.CodeDom.CodeCompileUnit" /> and returns an <see cref="T:System.Xml.XmlQualifiedName" /> that represents the data contract name for the specified element.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlQualifiedName" /> that represents the specified element.</returns>
	/// <param name="schemas">An <see cref="T:System.Xml.Schema.XmlSchemaSet" /> that contains the schemas to transform.</param>
	/// <param name="element">An <see cref="T:System.Xml.Schema.XmlSchemaElement" /> that represents the specific schema element to transform. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="schemas" /> or <paramref name="element" /> parameter is null.</exception>
	public XmlQualifiedName Import(XmlSchemaSet schemas, XmlSchemaElement element)
	{
		if (schemas == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("schemas"));
		}
		if (element == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("element"));
		}
		SingleTypeNameArray[0] = null;
		SingleElementArray[0] = element;
		InternalImport(schemas, emptyTypeNameArray, SingleElementArray, SingleTypeNameArray);
		return SingleTypeNameArray[0];
	}

	/// <summary>Gets a value that indicates whether the schemas contained in an <see cref="T:System.Xml.Schema.XmlSchemaSet" /> can be transformed into a <see cref="T:System.CodeDom.CodeCompileUnit" />. </summary>
	/// <returns>true if the schemas can be transformed to data contract types; otherwise, false. </returns>
	/// <param name="schemas">A <see cref="T:System.Xml.Schema.XmlSchemaSet" /> that contains the schemas to transform. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="schemas" /> parameter is null.</exception>
	/// <exception cref="T:System.Runtime.Serialization.InvalidDataContractException">A data contract involved in the import is invalid.</exception>
	/// <filterpriority>2</filterpriority>
	public bool CanImport(XmlSchemaSet schemas)
	{
		if (schemas == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("schemas"));
		}
		return InternalCanImport(schemas, null, null, null);
	}

	/// <summary>Gets a value that indicates whether the specified set of types contained in an <see cref="T:System.Xml.Schema.XmlSchemaSet" /> can be transformed into CLR types generated into a <see cref="T:System.CodeDom.CodeCompileUnit" />.</summary>
	/// <returns>true if the schemas can be transformed; otherwise, false. </returns>
	/// <param name="schemas">A <see cref="T:System.Xml.Schema.XmlSchemaSet" /> that contains the schemas to transform.</param>
	/// <param name="typeNames">An <see cref="T:System.Collections.Generic.ICollection`1" /> of <see cref="T:System.Xml.XmlQualifiedName" /> that represents the set of schema types to import.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="schemas" /> or <paramref name="typeNames" /> parameter is null.</exception>
	/// <exception cref="T:System.Runtime.Serialization.InvalidDataContractException">A data contract involved in the import is invalid.</exception>
	/// <filterpriority>2</filterpriority>
	public bool CanImport(XmlSchemaSet schemas, ICollection<XmlQualifiedName> typeNames)
	{
		if (schemas == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("schemas"));
		}
		if (typeNames == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("typeNames"));
		}
		return InternalCanImport(schemas, typeNames, emptyElementArray, emptyTypeNameArray);
	}

	/// <summary>Gets a value that indicates whether the schemas contained in an <see cref="T:System.Xml.Schema.XmlSchemaSet" /> can be transformed into a <see cref="T:System.CodeDom.CodeCompileUnit" />. </summary>
	/// <returns>true if the schemas can be transformed to data contract types; otherwise, false.</returns>
	/// <param name="schemas">A <see cref="T:System.Xml.Schema.XmlSchemaSet" /> that contains the schema representations. </param>
	/// <param name="typeName">An <see cref="T:System.Collections.IList" /> of <see cref="T:System.Xml.XmlQualifiedName" /> that specifies the names of the schema types that need to be imported from the <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="schemas" /> or <paramref name="typeName" /> parameter is null.</exception>
	/// <exception cref="T:System.Runtime.Serialization.InvalidDataContractException">A data contract involved in the import is invalid.</exception>
	/// <filterpriority>2</filterpriority>
	public bool CanImport(XmlSchemaSet schemas, XmlQualifiedName typeName)
	{
		if (schemas == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("schemas"));
		}
		if (typeName == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("typeName"));
		}
		return InternalCanImport(schemas, new XmlQualifiedName[1] { typeName }, emptyElementArray, emptyTypeNameArray);
	}

	/// <summary>Gets a value that indicates whether a specific schema element contained in an <see cref="T:System.Xml.Schema.XmlSchemaSet" /> can be imported.</summary>
	/// <returns>true if the element can be imported; otherwise, false.</returns>
	/// <param name="schemas">An <see cref="T:System.Xml.Schema.XmlSchemaSet" /> to import.</param>
	/// <param name="element">A specific <see cref="T:System.Xml.Schema.XmlSchemaElement" /> to check in the set of schemas.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="schemas" /> or <paramref name="element" /> parameter is null.</exception>
	/// <exception cref="T:System.Runtime.Serialization.InvalidDataContractException">A data contract involved in the import is invalid.</exception>
	/// <filterpriority>2</filterpriority>
	public bool CanImport(XmlSchemaSet schemas, XmlSchemaElement element)
	{
		if (schemas == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("schemas"));
		}
		if (element == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("element"));
		}
		SingleTypeNameArray[0] = null;
		SingleElementArray[0] = element;
		return InternalCanImport(schemas, emptyTypeNameArray, SingleElementArray, SingleTypeNameArray);
	}

	/// <summary>Returns a <see cref="T:System.CodeDom.CodeTypeReference" /> to the CLR type generated for the schema type with the specified <see cref="T:System.Xml.XmlQualifiedName" />.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReference" /> reference to the CLR type generated for the schema type with the <paramref name="typeName" /> specified.</returns>
	/// <param name="typeName">The <see cref="T:System.Xml.XmlQualifiedName" /> that specifies the schema type to look up.</param>
	/// <filterpriority>2</filterpriority>
	public CodeTypeReference GetCodeTypeReference(XmlQualifiedName typeName)
	{
		DataContract dataContract = FindDataContract(typeName);
		return new CodeExporter(DataContractSet, Options, GetCodeCompileUnit()).GetCodeTypeReference(dataContract);
	}

	/// <summary>Returns a <see cref="T:System.CodeDom.CodeTypeReference" /> for the specified XML qualified element and schema element.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeTypeReference" /> that represents the type that was generated for the specified schema type.</returns>
	/// <param name="typeName">An <see cref="T:System.Xml.XmlQualifiedName" /> that specifies the XML qualified name of the schema type to look up.</param>
	/// <param name="element">An <see cref="T:System.Xml.Schema.XmlSchemaElement" /> that specifies an element in an XML schema.</param>
	/// <filterpriority>2</filterpriority>
	public CodeTypeReference GetCodeTypeReference(XmlQualifiedName typeName, XmlSchemaElement element)
	{
		if (element == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("element"));
		}
		if (typeName == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("typeName"));
		}
		DataContract dataContract = FindDataContract(typeName);
		return new CodeExporter(DataContractSet, Options, GetCodeCompileUnit()).GetElementTypeReference(dataContract, element.IsNillable);
	}

	internal DataContract FindDataContract(XmlQualifiedName typeName)
	{
		if (typeName == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("typeName"));
		}
		DataContract dataContract = DataContract.GetBuiltInDataContract(typeName.Name, typeName.Namespace);
		if (dataContract == null)
		{
			dataContract = DataContractSet[typeName];
			if (dataContract == null)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("Type '{0}' in '{1}' namespace has not been imported.", typeName.Name, typeName.Namespace)));
			}
		}
		return dataContract;
	}

	/// <summary>Returns a list of <see cref="T:System.CodeDom.CodeTypeReference" /> objects that represents the known types generated when generating code for the specified schema type.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.IList`1" /> of type <see cref="T:System.CodeDom.CodeTypeReference" />.</returns>
	/// <param name="typeName">An <see cref="T:System.Xml.XmlQualifiedName" /> that represents the schema type to look up known types for.</param>
	/// <filterpriority>2</filterpriority>
	public ICollection<CodeTypeReference> GetKnownTypeReferences(XmlQualifiedName typeName)
	{
		if (typeName == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("typeName"));
		}
		DataContract dataContract = DataContract.GetBuiltInDataContract(typeName.Name, typeName.Namespace);
		if (dataContract == null)
		{
			dataContract = DataContractSet[typeName];
			if (dataContract == null)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("Type '{0}' in '{1}' namespace has not been imported.", typeName.Name, typeName.Namespace)));
			}
		}
		return new CodeExporter(DataContractSet, Options, GetCodeCompileUnit()).GetKnownTypeReferences(dataContract);
	}

	[SecuritySafeCritical]
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	private void InternalImport(XmlSchemaSet schemas, ICollection<XmlQualifiedName> typeNames, ICollection<XmlSchemaElement> elements, XmlQualifiedName[] elementTypeNames)
	{
		if (DiagnosticUtility.ShouldTraceInformation)
		{
			TraceUtility.Trace(TraceEventType.Information, 196618, SR.GetString("XSD import begins"));
		}
		DataContractSet dataContractSet = ((this.dataContractSet == null) ? null : new DataContractSet(this.dataContractSet));
		try
		{
			new SchemaImporter(schemas, typeNames, elements, elementTypeNames, DataContractSet, ImportXmlDataType).Import();
			new CodeExporter(DataContractSet, Options, GetCodeCompileUnit()).Export();
		}
		catch (Exception exception)
		{
			if (Fx.IsFatal(exception))
			{
				throw;
			}
			this.dataContractSet = dataContractSet;
			TraceImportError(exception);
			throw;
		}
		if (DiagnosticUtility.ShouldTraceInformation)
		{
			TraceUtility.Trace(TraceEventType.Information, 196619, SR.GetString("XSD import ends"));
		}
	}

	private void TraceImportError(Exception exception)
	{
		if (DiagnosticUtility.ShouldTraceError)
		{
			TraceUtility.Trace(TraceEventType.Error, 196621, SR.GetString("XSD import error"), null, exception);
		}
	}

	private bool InternalCanImport(XmlSchemaSet schemas, ICollection<XmlQualifiedName> typeNames, ICollection<XmlSchemaElement> elements, XmlQualifiedName[] elementTypeNames)
	{
		DataContractSet dataContractSet = ((this.dataContractSet == null) ? null : new DataContractSet(this.dataContractSet));
		try
		{
			new SchemaImporter(schemas, typeNames, elements, elementTypeNames, DataContractSet, ImportXmlDataType).Import();
			return true;
		}
		catch (InvalidDataContractException)
		{
			this.dataContractSet = dataContractSet;
			return false;
		}
		catch (Exception exception)
		{
			if (Fx.IsFatal(exception))
			{
				throw;
			}
			this.dataContractSet = dataContractSet;
			TraceImportError(exception);
			throw;
		}
	}
}
