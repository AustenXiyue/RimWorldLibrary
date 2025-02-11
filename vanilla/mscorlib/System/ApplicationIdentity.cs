using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System;

/// <summary>Provides the ability to uniquely identify a manifest-activated application. This class cannot be inherited. </summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(false)]
public sealed class ApplicationIdentity : ISerializable
{
	private string _fullName;

	private string _codeBase;

	/// <summary>Gets the location of the deployment manifest as a URL.</summary>
	/// <returns>The URL of the deployment manifest.</returns>
	/// <filterpriority>1</filterpriority>
	public string CodeBase => _codeBase;

	/// <summary>Gets the full name of the application.</summary>
	/// <returns>The full name of the application, also known as the display name.</returns>
	/// <filterpriority>1</filterpriority>
	public string FullName => _fullName;

	/// <summary>Initializes a new instance of the <see cref="T:System.ApplicationIdentity" /> class. </summary>
	/// <param name="applicationIdentityFullName">The full name of the application.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="applicationIdentityFullName" /> is null.</exception>
	public ApplicationIdentity(string applicationIdentityFullName)
	{
		if (applicationIdentityFullName == null)
		{
			throw new ArgumentNullException("applicationIdentityFullName");
		}
		if (applicationIdentityFullName.IndexOf(", Culture=") == -1)
		{
			_fullName = applicationIdentityFullName + ", Culture=neutral";
		}
		else
		{
			_fullName = applicationIdentityFullName;
		}
	}

	/// <summary>Returns the full name of the manifest-activated application.</summary>
	/// <returns>The full name of the manifest-activated application.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override string ToString()
	{
		return _fullName;
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the data needed to serialize the target object.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.</param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" />) structure for the serialization.</param>
	[MonoTODO("Missing serialization")]
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
	}
}
