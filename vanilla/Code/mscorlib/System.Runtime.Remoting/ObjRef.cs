using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization;
using System.Security;

namespace System.Runtime.Remoting;

/// <summary>Stores all relevant information required to generate a proxy in order to communicate with a remote object.</summary>
[Serializable]
[ComVisible(true)]
public class ObjRef : IObjectReference, ISerializable
{
	private IChannelInfo channel_info;

	private string uri;

	private IRemotingTypeInfo typeInfo;

	private IEnvoyInfo envoyInfo;

	private int flags;

	private Type _serverType;

	private static int MarshalledObjectRef = 1;

	private static int WellKnowObjectRef = 2;

	internal bool IsReferenceToWellKnow => (flags & WellKnowObjectRef) > 0;

	/// <summary>Gets or sets the <see cref="T:System.Runtime.Remoting.IChannelInfo" /> for the <see cref="T:System.Runtime.Remoting.ObjRef" />.</summary>
	/// <returns>The <see cref="T:System.Runtime.Remoting.IChannelInfo" /> interface for the <see cref="T:System.Runtime.Remoting.ObjRef" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual IChannelInfo ChannelInfo
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		get
		{
			return channel_info;
		}
		set
		{
			channel_info = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Runtime.Remoting.IEnvoyInfo" /> for the <see cref="T:System.Runtime.Remoting.ObjRef" />.</summary>
	/// <returns>The <see cref="T:System.Runtime.Remoting.IEnvoyInfo" /> interface for the <see cref="T:System.Runtime.Remoting.ObjRef" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual IEnvoyInfo EnvoyInfo
	{
		get
		{
			return envoyInfo;
		}
		set
		{
			envoyInfo = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Runtime.Remoting.IRemotingTypeInfo" /> for the object that the <see cref="T:System.Runtime.Remoting.ObjRef" /> describes.</summary>
	/// <returns>The <see cref="T:System.Runtime.Remoting.IRemotingTypeInfo" /> for the object that the <see cref="T:System.Runtime.Remoting.ObjRef" /> describes.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual IRemotingTypeInfo TypeInfo
	{
		get
		{
			return typeInfo;
		}
		set
		{
			typeInfo = value;
		}
	}

	/// <summary>Gets or sets the URI of the specific object instance.</summary>
	/// <returns>The URI of the specific object instance.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual string URI
	{
		get
		{
			return uri;
		}
		set
		{
			uri = value;
		}
	}

	internal Type ServerType
	{
		get
		{
			if (_serverType == null)
			{
				_serverType = Type.GetType(typeInfo.TypeName);
			}
			return _serverType;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.ObjRef" /> class with default values.</summary>
	public ObjRef()
	{
		UpdateChannelInfo();
	}

	internal ObjRef(string uri, IChannelInfo cinfo)
	{
		this.uri = uri;
		channel_info = cinfo;
	}

	internal ObjRef DeserializeInTheCurrentDomain(int domainId, byte[] tInfo)
	{
		string text = string.Copy(uri);
		ChannelInfo cinfo = new ChannelInfo(new CrossAppDomainData(domainId));
		ObjRef objRef = new ObjRef(text, cinfo);
		IRemotingTypeInfo remotingTypeInfo = (IRemotingTypeInfo)CADSerializer.DeserializeObjectSafe(tInfo);
		objRef.typeInfo = remotingTypeInfo;
		return objRef;
	}

	internal byte[] SerializeType()
	{
		if (typeInfo == null)
		{
			throw new Exception("Attempt to serialize a null TypeInfo.");
		}
		return CADSerializer.SerializeObject(typeInfo).GetBuffer();
	}

	internal ObjRef(ObjRef o, bool unmarshalAsProxy)
	{
		channel_info = o.channel_info;
		uri = o.uri;
		typeInfo = o.typeInfo;
		envoyInfo = o.envoyInfo;
		flags = o.flags;
		if (unmarshalAsProxy)
		{
			flags |= MarshalledObjectRef;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.ObjRef" /> class to reference a specified <see cref="T:System.MarshalByRefObject" /> of a specified <see cref="T:System.Type" />.</summary>
	/// <param name="o">The object that the new <see cref="T:System.Runtime.Remoting.ObjRef" /> instance will reference. </param>
	/// <param name="requestedType">The <see cref="T:System.Type" /> of the object that the new <see cref="T:System.Runtime.Remoting.ObjRef" /> instance will reference. </param>
	public ObjRef(MarshalByRefObject o, Type requestedType)
	{
		if (o == null)
		{
			throw new ArgumentNullException("o");
		}
		if (requestedType == null)
		{
			throw new ArgumentNullException("requestedType");
		}
		uri = RemotingServices.GetObjectUri(o);
		typeInfo = new TypeInfo(requestedType);
		if (!requestedType.IsInstanceOfType(o))
		{
			throw new RemotingException("The server object type cannot be cast to the requested type " + requestedType.FullName);
		}
		UpdateChannelInfo();
	}

	internal ObjRef(Type type, string url, object remoteChannelData)
	{
		uri = url;
		typeInfo = new TypeInfo(type);
		if (remoteChannelData != null)
		{
			channel_info = new ChannelInfo(remoteChannelData);
		}
		flags |= WellKnowObjectRef;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.ObjRef" /> class from serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination of the exception. </param>
	protected ObjRef(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		bool flag = true;
		while (enumerator.MoveNext())
		{
			switch (enumerator.Name)
			{
			case "uri":
				uri = (string)enumerator.Value;
				break;
			case "typeInfo":
				typeInfo = (IRemotingTypeInfo)enumerator.Value;
				break;
			case "channelInfo":
				channel_info = (IChannelInfo)enumerator.Value;
				break;
			case "envoyInfo":
				envoyInfo = (IEnvoyInfo)enumerator.Value;
				break;
			case "fIsMarshalled":
			{
				object value = enumerator.Value;
				if (((!(value is string)) ? ((int)value) : ((IConvertible)value).ToInt32(null)) == 0)
				{
					flag = false;
				}
				break;
			}
			case "objrefFlags":
				flags = Convert.ToInt32(enumerator.Value);
				break;
			default:
				throw new NotSupportedException();
			}
		}
		if (flag)
		{
			flags |= MarshalledObjectRef;
		}
	}

	internal bool IsPossibleToCAD()
	{
		return false;
	}

	/// <summary>Populates a specified <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the current <see cref="T:System.Runtime.Remoting.ObjRef" /> instance.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
	/// <param name="context">The contextual information about the source or destination of the serialization. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have serialization formatter permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter, Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.SetType(GetType());
		info.AddValue("uri", uri);
		info.AddValue("typeInfo", typeInfo, typeof(IRemotingTypeInfo));
		info.AddValue("envoyInfo", envoyInfo, typeof(IEnvoyInfo));
		info.AddValue("channelInfo", channel_info, typeof(IChannelInfo));
		info.AddValue("objrefFlags", flags);
	}

	/// <summary>Returns a reference to the remote object that the <see cref="T:System.Runtime.Remoting.ObjRef" /> describes.</summary>
	/// <returns>A reference to the remote object that the <see cref="T:System.Runtime.Remoting.ObjRef" /> describes.</returns>
	/// <param name="context">The context where the current object resides. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have serialization formatter permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter, RemotingConfiguration, Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public virtual object GetRealObject(StreamingContext context)
	{
		if ((flags & MarshalledObjectRef) > 0)
		{
			return RemotingServices.Unmarshal(this);
		}
		return this;
	}

	/// <summary>Returns a Boolean value that indicates whether the current <see cref="T:System.Runtime.Remoting.ObjRef" /> instance references an object located in the current <see cref="T:System.AppDomain" />.</summary>
	/// <returns>A Boolean value that indicates whether the current <see cref="T:System.Runtime.Remoting.ObjRef" /> instance references an object located in the current <see cref="T:System.AppDomain" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public bool IsFromThisAppDomain()
	{
		return RemotingServices.GetIdentityForUri(uri)?.IsFromThisAppDomain ?? false;
	}

	/// <summary>Returns a Boolean value that indicates whether the current <see cref="T:System.Runtime.Remoting.ObjRef" /> instance references an object located in the current process.</summary>
	/// <returns>A Boolean value that indicates whether the current <see cref="T:System.Runtime.Remoting.ObjRef" /> instance references an object located in the current process.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public bool IsFromThisProcess()
	{
		object[] channelData = channel_info.ChannelData;
		foreach (object obj in channelData)
		{
			if (obj is CrossAppDomainData)
			{
				return ((CrossAppDomainData)obj).ProcessID == RemotingConfiguration.ProcessId;
			}
		}
		return true;
	}

	internal void UpdateChannelInfo()
	{
		channel_info = new ChannelInfo();
	}

	internal void SetDomainID(int id)
	{
	}
}
