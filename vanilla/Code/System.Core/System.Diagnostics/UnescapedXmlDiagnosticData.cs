using System.Security.Permissions;
using Unity;

namespace System.Diagnostics;

/// <summary>Provides unescaped XML data for the logging of user-provided trace data.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class UnescapedXmlDiagnosticData
{
	/// <summary>Gets or sets the unescaped XML data string.</summary>
	/// <returns>An unescaped XML string.</returns>
	/// <filterpriority>2</filterpriority>
	public string UnescapedXml
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.UnescapedXmlDiagnosticData" /> class by using the specified XML data string.</summary>
	/// <param name="xmlPayload">The XML data to be logged in the UserData node of the event schema.  </param>
	/// <filterpriority>2</filterpriority>
	public UnescapedXmlDiagnosticData(string xmlPayload)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
