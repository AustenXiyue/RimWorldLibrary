using System.ComponentModel;
using Unity;

namespace System.Net;

/// <summary>Provides data for the <see cref="E:System.Net.WebClient.UploadDataCompleted" /> event.</summary>
public class UploadDataCompletedEventArgs : AsyncCompletedEventArgs
{
	private byte[] m_Result;

	/// <summary>Gets the server reply to a data upload operation started by calling an <see cref="Overload:System.Net.WebClient.UploadDataAsync" /> method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the server reply.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] Result
	{
		get
		{
			RaiseExceptionIfNecessary();
			return m_Result;
		}
	}

	internal UploadDataCompletedEventArgs(byte[] result, Exception exception, bool cancelled, object userToken)
		: base(exception, cancelled, userToken)
	{
		m_Result = result;
	}

	internal UploadDataCompletedEventArgs()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
