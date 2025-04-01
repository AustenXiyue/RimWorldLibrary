using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Serialization.Formatters;

/// <summary>Holds the names and types of parameters required during serialization of a SOAP RPC (Remote Procedure Call).</summary>
[Serializable]
[ComVisible(true)]
public class SoapMessage : ISoapMessage
{
	internal string[] paramNames;

	internal object[] paramValues;

	internal Type[] paramTypes;

	internal string methodName;

	internal string xmlNameSpace;

	internal Header[] headers;

	/// <summary>Gets or sets the parameter names for the called method.</summary>
	/// <returns>The parameter names for the called method.</returns>
	public string[] ParamNames
	{
		get
		{
			return paramNames;
		}
		set
		{
			paramNames = value;
		}
	}

	/// <summary>Gets or sets the parameter values for the called method.</summary>
	/// <returns>Parameter values for the called method.</returns>
	public object[] ParamValues
	{
		get
		{
			return paramValues;
		}
		set
		{
			paramValues = value;
		}
	}

	/// <summary>This property is reserved. Use the <see cref="P:System.Runtime.Serialization.Formatters.SoapMessage.ParamNames" /> and/or <see cref="P:System.Runtime.Serialization.Formatters.SoapMessage.ParamValues" /> properties instead.</summary>
	/// <returns>Parameter types for the called method.</returns>
	public Type[] ParamTypes
	{
		get
		{
			return paramTypes;
		}
		set
		{
			paramTypes = value;
		}
	}

	/// <summary>Gets or sets the name of the called method.</summary>
	/// <returns>The name of the called method.</returns>
	public string MethodName
	{
		get
		{
			return methodName;
		}
		set
		{
			methodName = value;
		}
	}

	/// <summary>Gets or sets the XML namespace name where the object that contains the called method is located.</summary>
	/// <returns>The XML namespace name where the object that contains the called method is located.</returns>
	public string XmlNameSpace
	{
		get
		{
			return xmlNameSpace;
		}
		set
		{
			xmlNameSpace = value;
		}
	}

	/// <summary>Gets or sets the out-of-band data of the called method.</summary>
	/// <returns>The out-of-band data of the called method.</returns>
	public Header[] Headers
	{
		get
		{
			return headers;
		}
		set
		{
			headers = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Formatters.SoapMessage" /> class. </summary>
	public SoapMessage()
	{
	}
}
