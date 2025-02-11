using System.ComponentModel;
using Unity;

namespace System.Net;

/// <summary>Provides data for the <see cref="E:System.Net.WebClient.DownloadStringCompleted" /> event.</summary>
public class DownloadStringCompletedEventArgs : AsyncCompletedEventArgs
{
	private string m_Result;

	/// <summary>Gets the data that is downloaded by a <see cref="Overload:System.Net.WebClient.DownloadStringAsync" /> method.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the downloaded data.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public string Result
	{
		get
		{
			RaiseExceptionIfNecessary();
			return m_Result;
		}
	}

	internal DownloadStringCompletedEventArgs(string result, Exception exception, bool cancelled, object userToken)
		: base(exception, cancelled, userToken)
	{
		m_Result = result;
	}

	internal DownloadStringCompletedEventArgs()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
