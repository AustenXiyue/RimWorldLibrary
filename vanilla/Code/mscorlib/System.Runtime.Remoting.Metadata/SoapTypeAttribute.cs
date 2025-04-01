using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata;

/// <summary>Customizes SOAP generation and processing for target types. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface)]
[ComVisible(true)]
public sealed class SoapTypeAttribute : SoapAttribute
{
	private SoapOption _soapOption;

	private bool _useAttribute;

	private string _xmlElementName;

	private XmlFieldOrderOption _xmlFieldOrder;

	private string _xmlNamespace;

	private string _xmlTypeName;

	private string _xmlTypeNamespace;

	private bool _isType;

	private bool _isElement;

	/// <summary>Gets or sets a <see cref="T:System.Runtime.Remoting.Metadata.SoapOption" /> configuration value.</summary>
	/// <returns>A <see cref="T:System.Runtime.Remoting.Metadata.SoapOption" /> configuration value.</returns>
	public SoapOption SoapOptions
	{
		get
		{
			return _soapOption;
		}
		set
		{
			_soapOption = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the target of the current attribute will be serialized as an XML attribute instead of an XML field.</summary>
	/// <returns>The current implementation always returns false.</returns>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">An attempt was made to set the current property. </exception>
	public override bool UseAttribute
	{
		get
		{
			return _useAttribute;
		}
		set
		{
			_useAttribute = value;
		}
	}

	/// <summary>Gets or sets the XML element name.</summary>
	/// <returns>The XML element name.</returns>
	public string XmlElementName
	{
		get
		{
			return _xmlElementName;
		}
		set
		{
			_isElement = value != null;
			_xmlElementName = value;
		}
	}

	/// <summary>Gets or sets the XML field order for the target object type.</summary>
	/// <returns>The XML field order for the target object type.</returns>
	public XmlFieldOrderOption XmlFieldOrder
	{
		get
		{
			return _xmlFieldOrder;
		}
		set
		{
			_xmlFieldOrder = value;
		}
	}

	/// <summary>Gets or sets the XML namespace that is used during serialization of the target object type.</summary>
	/// <returns>The XML namespace that is used during serialization of the target object type.</returns>
	public override string XmlNamespace
	{
		get
		{
			return _xmlNamespace;
		}
		set
		{
			_isElement = value != null;
			_xmlNamespace = value;
		}
	}

	/// <summary>Gets or sets the XML type name for the target object type.</summary>
	/// <returns>The XML type name for the target object type.</returns>
	public string XmlTypeName
	{
		get
		{
			return _xmlTypeName;
		}
		set
		{
			_isType = value != null;
			_xmlTypeName = value;
		}
	}

	/// <summary>Gets or sets the XML type namespace for the current object type.</summary>
	/// <returns>The XML type namespace for the current object type.</returns>
	public string XmlTypeNamespace
	{
		get
		{
			return _xmlTypeNamespace;
		}
		set
		{
			_isType = value != null;
			_xmlTypeNamespace = value;
		}
	}

	internal bool IsInteropXmlElement => _isElement;

	internal bool IsInteropXmlType => _isType;

	/// <summary>Creates an instance of <see cref="T:System.Runtime.Remoting.Metadata.SoapTypeAttribute" />.</summary>
	public SoapTypeAttribute()
	{
	}

	internal override void SetReflectionObject(object reflectionObject)
	{
		Type type = (Type)reflectionObject;
		if (_xmlElementName == null)
		{
			_xmlElementName = type.Name;
		}
		if (_xmlTypeName == null)
		{
			_xmlTypeName = type.Name;
		}
		if (_xmlTypeNamespace == null)
		{
			_xmlTypeNamespace = SoapServices.CodeXmlNamespaceForClrTypeNamespace(assemblyName: (!(type.Assembly == typeof(object).Assembly)) ? type.Assembly.GetName().Name : string.Empty, typeNamespace: type.Namespace);
		}
		if (_xmlNamespace == null)
		{
			_xmlNamespace = _xmlTypeNamespace;
		}
	}
}
