using System.ComponentModel;
using Unity;

namespace System.Net;

/// <summary>Provides data for the <see cref="E:System.Net.WebClient.UploadStringCompleted" /> event.</summary>
public class UploadStringCompletedEventArgs : AsyncCompletedEventArgs
{
	private string m_Result;

	/// <summary>Gets the server reply to a string upload operation that is started by calling an <see cref="Overload:System.Net.WebClient.UploadStringAsync" /> method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array that contains the server reply.</returns>
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

	internal UploadStringCompletedEventArgs(string result, Exception exception, bool cancelled, object userToken)
		: base(exception, cancelled, userToken)
	{
		m_Result = result;
	}

	internal UploadStringCompletedEventArgs()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
