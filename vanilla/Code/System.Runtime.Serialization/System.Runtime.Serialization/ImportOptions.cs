using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace System.Runtime.Serialization;

/// <summary>Represents the options that can be set on an <see cref="T:System.Runtime.Serialization.XsdDataContractImporter" />. </summary>
public class ImportOptions
{
	private bool generateSerializable;

	private bool generateInternal;

	private bool enableDataBinding;

	private CodeDomProvider codeProvider;

	private ICollection<Type> referencedTypes;

	private ICollection<Type> referencedCollectionTypes;

	private IDictionary<string, string> namespaces;

	private bool importXmlType;

	private IDataContractSurrogate dataContractSurrogate;

	/// <summary>Gets or sets a value that specifies whether generated data contract classes will be marked with the <see cref="T:System.SerializableAttribute" /> attribute in addition to the <see cref="T:System.Runtime.Serialization.DataContractAttribute" /> attribute.</summary>
	/// <returns>true to generate classes with the <see cref="T:System.SerializableAttribute" /> applied; otherwise, false. The default is false.</returns>
	public bool GenerateSerializable
	{
		get
		{
			return generateSerializable;
		}
		set
		{
			generateSerializable = value;
		}
	}

	/// <summary>Gets or sets a value that specifies whether generated code will be marked internal or public.</summary>
	/// <returns>true if the code will be marked internal; otherwise, false. The default is false.</returns>
	public bool GenerateInternal
	{
		get
		{
			return generateInternal;
		}
		set
		{
			generateInternal = value;
		}
	}

	/// <summary>Gets or sets a value that specifies whether types in generated code should implement the <see cref="T:System.ComponentModel.INotifyPropertyChanged" /> interface.</summary>
	/// <returns>true if the generated code should implement the <see cref="T:System.ComponentModel.INotifyPropertyChanged" /> interface; otherwise, false. The default is false.</returns>
	public bool EnableDataBinding
	{
		get
		{
			return enableDataBinding;
		}
		set
		{
			enableDataBinding = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> instance that provides the means to check whether particular options for a target language are supported.</summary>
	/// <returns>A <see cref="T:System.CodeDom.Compiler.CodeDomProvider" /> that provides the means to check whether particular options for a target language are supported.</returns>
	/// <filterpriority>2</filterpriority>
	public CodeDomProvider CodeProvider
	{
		get
		{
			return codeProvider;
		}
		set
		{
			codeProvider = value;
		}
	}

	/// <summary>Gets a <see cref="T:System.Collections.Generic.IList`1" /> containing types referenced in generated code. </summary>
	/// <returns>A <see cref="T:System.Collections.Generic.IList`1" /> that contains the referenced types. </returns>
	public ICollection<Type> ReferencedTypes
	{
		get
		{
			if (referencedTypes == null)
			{
				referencedTypes = new List<Type>();
			}
			return referencedTypes;
		}
	}

	/// <summary>Gets a collection of types that represents data contract collections that should be referenced when generating code for collections, such as lists or dictionaries of items.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> that contains the referenced collection types.</returns>
	public ICollection<Type> ReferencedCollectionTypes
	{
		get
		{
			if (referencedCollectionTypes == null)
			{
				referencedCollectionTypes = new List<Type>();
			}
			return referencedCollectionTypes;
		}
	}

	/// <summary>Gets a dictionary that contains the mapping of data contract namespaces to the CLR namespaces that must be used to generate code during an import operation.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.IDictionary`2" /> that contains the namespace mappings. </returns>
	public IDictionary<string, string> Namespaces
	{
		get
		{
			if (namespaces == null)
			{
				namespaces = new Dictionary<string, string>();
			}
			return namespaces;
		}
	}

	/// <summary>Gets or sets a value that determines whether all XML schema types, even those that do not conform to a data contract schema, will be imported.</summary>
	/// <returns>true to import all schema types; otherwise, false. The default is false.</returns>
	public bool ImportXmlType
	{
		get
		{
			return importXmlType;
		}
		set
		{
			importXmlType = value;
		}
	}

	/// <summary>Gets or sets a data contract surrogate that can be used to modify the code generated during an import operation. </summary>
	/// <returns>An implementation of the <see cref="T:System.Runtime.Serialization.IDataContractSurrogate" /> interface that handles schema import. </returns>
	public IDataContractSurrogate DataContractSurrogate
	{
		get
		{
			return dataContractSurrogate;
		}
		set
		{
			dataContractSurrogate = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.ImportOptions" /> class.</summary>
	public ImportOptions()
	{
	}
}
