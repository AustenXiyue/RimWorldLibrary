using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata;
using System.Security;

namespace System.Runtime.Serialization.Formatters;

/// <summary>Carries error and status information within a SOAP message. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
[SoapType(Embedded = true)]
public sealed class SoapFault : ISerializable
{
	private string faultCode;

	private string faultString;

	private string faultActor;

	[SoapField(Embedded = true)]
	private object detail;

	/// <summary>Gets or sets the fault code for the <see cref="T:System.Runtime.Serialization.Formatters.SoapFault" />.</summary>
	/// <returns>The fault code for this <see cref="T:System.Runtime.Serialization.Formatters.SoapFault" />.</returns>
	public string FaultCode
	{
		get
		{
			return faultCode;
		}
		set
		{
			faultCode = value;
		}
	}

	/// <summary>Gets or sets the fault message for the <see cref="T:System.Runtime.Serialization.Formatters.SoapFault" />.</summary>
	/// <returns>The fault message for the <see cref="T:System.Runtime.Serialization.Formatters.SoapFault" />.</returns>
	public string FaultString
	{
		get
		{
			return faultString;
		}
		set
		{
			faultString = value;
		}
	}

	/// <summary>Gets or sets the fault actor for the <see cref="T:System.Runtime.Serialization.Formatters.SoapFault" />.</summary>
	/// <returns>The fault actor for the <see cref="T:System.Runtime.Serialization.Formatters.SoapFault" />.</returns>
	public string FaultActor
	{
		get
		{
			return faultActor;
		}
		set
		{
			faultActor = value;
		}
	}

	/// <summary>Gets or sets additional information required for the <see cref="T:System.Runtime.Serialization.Formatters.SoapFault" />.</summary>
	/// <returns>Additional information required for the <see cref="T:System.Runtime.Serialization.Formatters.SoapFault" />.</returns>
	public object Detail
	{
		get
		{
			return detail;
		}
		set
		{
			detail = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Formatters.SoapFault" /> class with default values.</summary>
	public SoapFault()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Formatters.SoapFault" /> class, setting the properties to specified values.</summary>
	/// <param name="faultCode">The fault code for the new instance of <see cref="T:System.Runtime.Serialization.Formatters.SoapFault" />. The fault code identifies the type of the fault that occurred. </param>
	/// <param name="faultString">The fault string for the new instance of <see cref="T:System.Runtime.Serialization.Formatters.SoapFault" />. The fault string provides a human readable explanation of the fault. </param>
	/// <param name="faultActor">The URI of the object that generated the fault. </param>
	/// <param name="serverFault">The description of a common language runtime exception. This information is also present in the <see cref="P:System.Runtime.Serialization.Formatters.SoapFault.Detail" /> property. </param>
	public SoapFault(string faultCode, string faultString, string faultActor, ServerFault serverFault)
	{
		this.faultCode = faultCode;
		this.faultString = faultString;
		this.faultActor = faultActor;
		detail = serverFault;
	}

	internal SoapFault(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string name = enumerator.Name;
			object value = enumerator.Value;
			if (string.Compare(name, "faultCode", ignoreCase: true, CultureInfo.InvariantCulture) == 0)
			{
				int num = ((string)value).IndexOf(':');
				if (num > -1)
				{
					faultCode = ((string)value).Substring(++num);
				}
				else
				{
					faultCode = (string)value;
				}
			}
			else if (string.Compare(name, "faultString", ignoreCase: true, CultureInfo.InvariantCulture) == 0)
			{
				faultString = (string)value;
			}
			else if (string.Compare(name, "faultActor", ignoreCase: true, CultureInfo.InvariantCulture) == 0)
			{
				faultActor = (string)value;
			}
			else if (string.Compare(name, "detail", ignoreCase: true, CultureInfo.InvariantCulture) == 0)
			{
				detail = value;
			}
		}
	}

	/// <summary>Populates the specified <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data to serialize the <see cref="T:System.Runtime.Serialization.Formatters.SoapFault" /> object.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
	/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for the current serialization. </param>
	[SecurityCritical]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("faultcode", "SOAP-ENV:" + faultCode);
		info.AddValue("faultstring", faultString);
		if (faultActor != null)
		{
			info.AddValue("faultactor", faultActor);
		}
		info.AddValue("detail", detail, typeof(object));
	}
}
