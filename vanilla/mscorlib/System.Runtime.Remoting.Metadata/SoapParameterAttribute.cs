using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata;

/// <summary>Customizes SOAP generation and processing for a parameter. This class cannot be inherited.</summary>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class SoapParameterAttribute : SoapAttribute
{
	/// <summary>Creates an instance of <see cref="T:System.Runtime.Remoting.Metadata.SoapParameterAttribute" />.</summary>
	public SoapParameterAttribute()
	{
	}
}
