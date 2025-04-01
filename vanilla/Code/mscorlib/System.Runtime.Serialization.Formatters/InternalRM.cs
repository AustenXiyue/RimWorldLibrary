using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Runtime.Serialization.Formatters;

/// <summary>Logs tracing messages when the .NET Framework serialization infrastructure is compiled.</summary>
[ComVisible(true)]
[SecurityCritical]
public sealed class InternalRM
{
	/// <summary>Prints SOAP trace messages.</summary>
	/// <param name="messages">An array of trace messages to print.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.StrongNameIdentityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PublicKeyBlob="00000000000000000400000000000000" Name="System.Runtime.Remoting" />
	/// </PermissionSet>
	[Conditional("_LOGGING")]
	public static void InfoSoap(params object[] messages)
	{
	}

	/// <summary>Checks if SOAP tracing is enabled.</summary>
	/// <returns>true, if tracing is enabled; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.StrongNameIdentityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PublicKeyBlob="00000000000000000400000000000000" Name="System.Runtime.Remoting" />
	/// </PermissionSet>
	public static bool SoapCheckEnabled()
	{
		return BCLDebug.CheckEnabled("SOAP");
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Formatters.InternalRM" /> class. </summary>
	public InternalRM()
	{
	}
}
