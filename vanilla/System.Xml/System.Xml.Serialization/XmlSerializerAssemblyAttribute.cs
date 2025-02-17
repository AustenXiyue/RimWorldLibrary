namespace System.Xml.Serialization;

/// <summary>Applied to a Web service client proxy, enables you to specify an assembly that contains custom-made serializers. </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, AllowMultiple = false)]
public sealed class XmlSerializerAssemblyAttribute : Attribute
{
	private string assemblyName;

	private string codeBase;

	/// <summary>Gets or sets the location of the assembly that contains the serializers.</summary>
	/// <returns>A location, such as a path or URI, that points to the assembly.</returns>
	public string CodeBase
	{
		get
		{
			return codeBase;
		}
		set
		{
			codeBase = value;
		}
	}

	/// <summary>Gets or sets the name of the assembly that contains serializers for a specific set of types.</summary>
	/// <returns>The simple, unencrypted name of the assembly. </returns>
	public string AssemblyName
	{
		get
		{
			return assemblyName;
		}
		set
		{
			assemblyName = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializerAssemblyAttribute" /> class. </summary>
	public XmlSerializerAssemblyAttribute()
		: this(null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializerAssemblyAttribute" /> class with the specified assembly name.</summary>
	/// <param name="assemblyName">The simple, unencrypted name of the assembly. </param>
	public XmlSerializerAssemblyAttribute(string assemblyName)
		: this(assemblyName, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlSerializerAssemblyAttribute" /> class with the specified assembly name and location of the assembly.</summary>
	/// <param name="assemblyName">The simple, unencrypted name of the assembly. </param>
	/// <param name="codeBase">A string that is the URL location of the assembly.</param>
	public XmlSerializerAssemblyAttribute(string assemblyName, string codeBase)
	{
		this.assemblyName = assemblyName;
		this.codeBase = codeBase;
	}
}
