using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata;

/// <summary>Customizes SOAP generation and processing for a method. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Method)]
[ComVisible(true)]
public sealed class SoapMethodAttribute : SoapAttribute
{
	private string _responseElement;

	private string _responseNamespace;

	private string _returnElement;

	private string _soapAction;

	private bool _useAttribute;

	private string _namespace;

	/// <summary>Gets or sets the XML element name to use for the method response to the target method.</summary>
	/// <returns>The XML element name to use for the method response to the target method.</returns>
	public string ResponseXmlElementName
	{
		get
		{
			return _responseElement;
		}
		set
		{
			_responseElement = value;
		}
	}

	/// <summary>Gets or sets the XML element namesapce used for method response to the target method.</summary>
	/// <returns>The XML element namesapce used for method response to the target method.</returns>
	public string ResponseXmlNamespace
	{
		get
		{
			return _responseNamespace;
		}
		set
		{
			_responseNamespace = value;
		}
	}

	/// <summary>Gets or sets the XML element name used for the return value from the target method.</summary>
	/// <returns>The XML element name used for the return value from the target method.</returns>
	public string ReturnXmlElementName
	{
		get
		{
			return _returnElement;
		}
		set
		{
			_returnElement = value;
		}
	}

	/// <summary>Gets or sets the SOAPAction header field used with HTTP requests sent with this method. This property is currently not implemented.</summary>
	/// <returns>The SOAPAction header field used with HTTP requests sent with this method.</returns>
	public string SoapAction
	{
		get
		{
			return _soapAction;
		}
		set
		{
			_soapAction = value;
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

	/// <summary>Gets or sets the XML namespace that is used during serialization of remote method calls of the target method.</summary>
	/// <returns>The XML namespace that is used during serialization of remote method calls of the target method.</returns>
	public override string XmlNamespace
	{
		get
		{
			return _namespace;
		}
		set
		{
			_namespace = value;
		}
	}

	/// <summary>Creates an instance of <see cref="T:System.Runtime.Remoting.Metadata.SoapMethodAttribute" />.</summary>
	public SoapMethodAttribute()
	{
	}

	internal override void SetReflectionObject(object reflectionObject)
	{
		MethodBase methodBase = (MethodBase)reflectionObject;
		if (_responseElement == null)
		{
			_responseElement = methodBase.Name + "Response";
		}
		if (_responseNamespace == null)
		{
			_responseNamespace = SoapServices.GetXmlNamespaceForMethodResponse(methodBase);
		}
		if (_returnElement == null)
		{
			_returnElement = "return";
		}
		if (_soapAction == null)
		{
			_soapAction = SoapServices.GetXmlNamespaceForMethodCall(methodBase) + "#" + methodBase.Name;
		}
		if (_namespace == null)
		{
			_namespace = SoapServices.GetXmlNamespaceForMethodCall(methodBase);
		}
	}
}
