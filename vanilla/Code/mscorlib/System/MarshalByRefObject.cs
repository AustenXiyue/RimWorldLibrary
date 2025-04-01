using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Security.Permissions;

namespace System;

/// <summary>Enables access to objects across application domain boundaries in applications that support remoting.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
public abstract class MarshalByRefObject
{
	[NonSerialized]
	private ServerIdentity _identity;

	internal ServerIdentity ObjectIdentity
	{
		get
		{
			return _identity;
		}
		set
		{
			_identity = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.MarshalByRefObject" /> class. </summary>
	protected MarshalByRefObject()
	{
	}

	internal Identity GetObjectIdentity(MarshalByRefObject obj, out bool IsClient)
	{
		IsClient = false;
		Identity identity = null;
		if (RemotingServices.IsTransparentProxy(obj))
		{
			identity = RemotingServices.GetRealProxy(obj).ObjectIdentity;
			IsClient = true;
		}
		else
		{
			identity = obj.ObjectIdentity;
		}
		return identity;
	}

	/// <summary>Creates an object that contains all the relevant information required to generate a proxy used to communicate with a remote object.</summary>
	/// <returns>Information required to generate a proxy.</returns>
	/// <param name="requestedType">The <see cref="T:System.Type" /> of the object that the new <see cref="T:System.Runtime.Remoting.ObjRef" /> will reference. </param>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">This instance is not a valid remoting object. </exception>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.LinkDemand, Infrastructure = true)]
	public virtual ObjRef CreateObjRef(Type requestedType)
	{
		if (_identity == null)
		{
			throw new RemotingException(Locale.GetText("No remoting information was found for the object."));
		}
		return _identity.CreateObjRef(requestedType);
	}

	/// <summary>Retrieves the current lifetime service object that controls the lifetime policy for this instance.</summary>
	/// <returns>An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease" /> used to control the lifetime policy for this instance.</returns>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.LinkDemand, Infrastructure = true)]
	public object GetLifetimeService()
	{
		if (_identity == null)
		{
			return null;
		}
		return _identity.Lease;
	}

	/// <summary>Obtains a lifetime service object to control the lifetime policy for this instance.</summary>
	/// <returns>An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease" /> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime" /> property.</returns>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.LinkDemand, Infrastructure = true)]
	public virtual object InitializeLifetimeService()
	{
		if (_identity != null && _identity.Lease != null)
		{
			return _identity.Lease;
		}
		return new Lease();
	}

	/// <summary>Creates a shallow copy of the current <see cref="T:System.MarshalByRefObject" /> object.</summary>
	/// <returns>A shallow copy of the current <see cref="T:System.MarshalByRefObject" /> object.</returns>
	/// <param name="cloneIdentity">false to delete the current <see cref="T:System.MarshalByRefObject" /> object's identity, which will cause the object to be assigned a new identity when it is marshaled across a remoting boundary. A value of false is usually appropriate. true to copy the current <see cref="T:System.MarshalByRefObject" /> object's identity to its clone, which will cause remoting client calls to be routed to the remote server object. </param>
	protected MarshalByRefObject MemberwiseClone(bool cloneIdentity)
	{
		MarshalByRefObject marshalByRefObject = (MarshalByRefObject)MemberwiseClone();
		if (!cloneIdentity)
		{
			marshalByRefObject._identity = null;
		}
		return marshalByRefObject;
	}
}
