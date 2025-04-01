using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata;

/// <summary>Customizes SOAP generation and processing for a field. This class cannot be inherited.</summary>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Field)]
public sealed class SoapFieldAttribute : SoapAttribute
{
	private int _order;

	private string _elementName;

	private bool _isElement;

	/// <summary>Gets or sets the order of the current field attribute.</summary>
	/// <returns>The order of the current field attribute.</returns>
	public int Order
	{
		get
		{
			return _order;
		}
		set
		{
			_order = value;
		}
	}

	/// <summary>Gets or sets the XML element name of the field contained in the <see cref="T:System.Runtime.Remoting.Metadata.SoapFieldAttribute" /> attribute.</summary>
	/// <returns>The XML element name of the field contained in this attribute.</returns>
	public string XmlElementName
	{
		get
		{
			return _elementName;
		}
		set
		{
			_isElement = value != null;
			_elementName = value;
		}
	}

	/// <summary>Creates an instance of <see cref="T:System.Runtime.Remoting.Metadata.SoapFieldAttribute" /> class.</summary>
	public SoapFieldAttribute()
	{
	}

	/// <summary>Returns a value indicating whether the current attribute contains interop XML element values.</summary>
	/// <returns>true if the current attribute contains interop XML element values; otherwise, false.</returns>
	public bool IsInteropXmlElement()
	{
		return _isElement;
	}

	internal override void SetReflectionObject(object reflectionObject)
	{
		FieldInfo fieldInfo = (FieldInfo)reflectionObject;
		if (_elementName == null)
		{
			_elementName = fieldInfo.Name;
		}
	}
}
