using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Services;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Threading;
using Mono.Interop;

namespace System.Runtime.Remoting;

/// <summary>Provides several methods for using and publishing remoted objects and proxies. This class cannot be inherited.</summary>
[ComVisible(true)]
public static class RemotingServices
{
	[Serializable]
	private class CACD
	{
		public object d;

		public object c;
	}

	private static Hashtable uri_hash;

	private static BinaryFormatter _serializationFormatter;

	private static BinaryFormatter _deserializationFormatter;

	private static string app_id;

	private static readonly object app_id_lock;

	private static int next_id;

	private const BindingFlags methodBindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly MethodInfo FieldSetterMethod;

	private static readonly MethodInfo FieldGetterMethod;

	static RemotingServices()
	{
		uri_hash = new Hashtable();
		app_id_lock = new object();
		next_id = 1;
		RemotingSurrogateSelector selector = new RemotingSurrogateSelector();
		StreamingContext context = new StreamingContext(StreamingContextStates.Remoting, null);
		_serializationFormatter = new BinaryFormatter(selector, context);
		_deserializationFormatter = new BinaryFormatter(null, context);
		_serializationFormatter.AssemblyFormat = FormatterAssemblyStyle.Full;
		_deserializationFormatter.AssemblyFormat = FormatterAssemblyStyle.Full;
		RegisterInternalChannels();
		CreateWellKnownServerIdentity(typeof(RemoteActivator), "RemoteActivationService.rem", WellKnownObjectMode.Singleton);
		FieldSetterMethod = typeof(object).GetMethod("FieldSetter", BindingFlags.Instance | BindingFlags.NonPublic);
		FieldGetterMethod = typeof(object).GetMethod("FieldGetter", BindingFlags.Instance | BindingFlags.NonPublic);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern object InternalExecute(MethodBase method, object obj, object[] parameters, out object[] out_args);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern MethodBase GetVirtualMethod(Type type, MethodBase method);

	/// <summary>Returns a Boolean value that indicates whether the given object is a transparent proxy or a real object.</summary>
	/// <returns>A Boolean value that indicates whether the object specified in the <paramref name="proxy" /> parameter is a transparent proxy or a real object.</returns>
	/// <param name="proxy">The reference to the object to check. </param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static extern bool IsTransparentProxy(object proxy);

	internal static bool ProxyCheckCast(RealProxy rp, RuntimeType castType)
	{
		return true;
	}

	internal static IMethodReturnMessage InternalExecuteMessage(MarshalByRefObject target, IMethodCallMessage reqMsg)
	{
		Type type = target.GetType();
		MethodBase methodBase;
		if (reqMsg.MethodBase.DeclaringType == type || reqMsg.MethodBase == FieldSetterMethod || reqMsg.MethodBase == FieldGetterMethod)
		{
			methodBase = reqMsg.MethodBase;
		}
		else
		{
			methodBase = GetVirtualMethod(type, reqMsg.MethodBase);
			if (methodBase == null)
			{
				throw new RemotingException($"Cannot resolve method {type}:{reqMsg.MethodName}");
			}
		}
		if (reqMsg.MethodBase.IsGenericMethod)
		{
			Type[] genericArguments = reqMsg.MethodBase.GetGenericArguments();
			methodBase = ((MethodInfo)methodBase).GetGenericMethodDefinition().MakeGenericMethod(genericArguments);
		}
		LogicalCallContext logicalCallContext = CallContext.SetLogicalCallContext(reqMsg.LogicalCallContext);
		ReturnMessage result;
		try
		{
			object[] out_args;
			object ret = InternalExecute(methodBase, target, reqMsg.Args, out out_args);
			ParameterInfo[] parameters = methodBase.GetParameters();
			object[] array = new object[parameters.Length];
			int outArgsCount = 0;
			int num = 0;
			ParameterInfo[] array2 = parameters;
			foreach (ParameterInfo parameterInfo in array2)
			{
				if (parameterInfo.IsOut && !parameterInfo.ParameterType.IsByRef)
				{
					array[outArgsCount++] = reqMsg.GetArg(parameterInfo.Position);
				}
				else if (parameterInfo.ParameterType.IsByRef)
				{
					array[outArgsCount++] = out_args[num++];
				}
				else
				{
					array[outArgsCount++] = null;
				}
			}
			LogicalCallContext logicalCallContext2 = Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext;
			result = new ReturnMessage(ret, array, outArgsCount, logicalCallContext2, reqMsg);
		}
		catch (Exception e)
		{
			result = new ReturnMessage(e, reqMsg);
		}
		CallContext.SetLogicalCallContext(logicalCallContext);
		return result;
	}

	/// <summary>Connects to the specified remote object, and executes the provided <see cref="T:System.Runtime.Remoting.Messaging.IMethodCallMessage" /> on it.</summary>
	/// <returns>The response of the remote method.</returns>
	/// <param name="target">The remote object whose method you want to call. </param>
	/// <param name="reqMsg">A method call message to the specified remote object's method. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">The method was called from a context other than the native context of the object.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static IMethodReturnMessage ExecuteMessage(MarshalByRefObject target, IMethodCallMessage reqMsg)
	{
		if (IsTransparentProxy(target))
		{
			return (IMethodReturnMessage)GetRealProxy(target).Invoke(reqMsg);
		}
		return InternalExecuteMessage(target, reqMsg);
	}

	/// <summary>Creates a proxy for a well-known object, given the <see cref="T:System.Type" /> and URL.</summary>
	/// <returns>A proxy to the remote object that points to an endpoint served by the specified well-known object.</returns>
	/// <param name="classToProxy">The <see cref="T:System.Type" /> of a well-known object on the server end to which you want to connect. </param>
	/// <param name="url">The URL of the server class. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, RemotingConfiguration" />
	/// </PermissionSet>
	[ComVisible(true)]
	public static object Connect(Type classToProxy, string url)
	{
		return GetRemoteObject(new ObjRef(classToProxy, url, null), classToProxy);
	}

	/// <summary>Creates a proxy for a well-known object, given the <see cref="T:System.Type" />, URL, and channel-specific data.</summary>
	/// <returns>A proxy that points to an endpoint that is served by the requested well-known object.</returns>
	/// <param name="classToProxy">The <see cref="T:System.Type" /> of the well-known object to which you want to connect. </param>
	/// <param name="url">The URL of the well-known object. </param>
	/// <param name="data">Channel specific data. Can be null. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, RemotingConfiguration" />
	/// </PermissionSet>
	[ComVisible(true)]
	public static object Connect(Type classToProxy, string url, object data)
	{
		return GetRemoteObject(new ObjRef(classToProxy, url, data), classToProxy);
	}

	/// <summary>Stops an object from receiving any further messages through the registered remoting channels.</summary>
	/// <returns>true if the object was disconnected from the registered remoting channels successfully; otherwise, false.</returns>
	/// <param name="obj">Object to disconnect from its channel. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="obj" /> parameter is a proxy. </exception>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static bool Disconnect(MarshalByRefObject obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		ServerIdentity serverIdentity;
		if (IsTransparentProxy(obj))
		{
			RealProxy realProxy = GetRealProxy(obj);
			if (!realProxy.GetProxiedType().IsContextful || !(realProxy.ObjectIdentity is ServerIdentity))
			{
				throw new ArgumentException("The obj parameter is a proxy.");
			}
			serverIdentity = realProxy.ObjectIdentity as ServerIdentity;
		}
		else
		{
			serverIdentity = obj.ObjectIdentity;
			obj.ObjectIdentity = null;
		}
		if (serverIdentity == null || !serverIdentity.IsConnected)
		{
			return false;
		}
		LifetimeServices.StopTrackingLifetime(serverIdentity);
		DisposeIdentity(serverIdentity);
		TrackingServices.NotifyDisconnectedObject(obj);
		return true;
	}

	/// <summary>Returns the <see cref="T:System.Type" /> of the object with the specified URI.</summary>
	/// <returns>The <see cref="T:System.Type" /> of the object with the specified URI.</returns>
	/// <param name="URI">The URI of the object whose <see cref="T:System.Type" /> is requested. </param>
	/// <exception cref="T:System.Security.SecurityException">Either the immediate caller does not have infrastructure permission, or at least one of the callers higher in the callstack does not have permission to retrieve the type information of non-public members. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static Type GetServerTypeForUri(string URI)
	{
		if (!(GetIdentityForUri(URI) is ServerIdentity serverIdentity))
		{
			return null;
		}
		return serverIdentity.ObjectType;
	}

	/// <summary>Retrieves the URI for the specified object.</summary>
	/// <returns>The URI of the specified object if it has one, or null if the object has not yet been marshaled.</returns>
	/// <param name="obj">The <see cref="T:System.MarshalByRefObject" /> for which a URI is requested. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static string GetObjectUri(MarshalByRefObject obj)
	{
		Identity objectIdentity = GetObjectIdentity(obj);
		if (objectIdentity is ClientIdentity)
		{
			return ((ClientIdentity)objectIdentity).TargetUri;
		}
		return objectIdentity?.ObjectUri;
	}

	/// <summary>Takes a <see cref="T:System.Runtime.Remoting.ObjRef" /> and creates a proxy object out of it.</summary>
	/// <returns>A proxy to the object that the given <see cref="T:System.Runtime.Remoting.ObjRef" /> represents.</returns>
	/// <param name="objectRef">The <see cref="T:System.Runtime.Remoting.ObjRef" /> that represents the remote object for which the proxy is being created. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Runtime.Remoting.ObjRef" /> instance specified in the <paramref name="objectRef" /> parameter is not well-formed. </exception>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure" />
	/// </PermissionSet>
	public static object Unmarshal(ObjRef objectRef)
	{
		return Unmarshal(objectRef, fRefine: true);
	}

	/// <summary>Takes a <see cref="T:System.Runtime.Remoting.ObjRef" /> and creates a proxy object out of it, refining it to the type on the server.</summary>
	/// <returns>A proxy to the object that the given <see cref="T:System.Runtime.Remoting.ObjRef" /> represents.</returns>
	/// <param name="objectRef">The <see cref="T:System.Runtime.Remoting.ObjRef" /> that represents the remote object for which the proxy is being created. </param>
	/// <param name="fRefine">true to refine the proxy to the type on the server; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Runtime.Remoting.ObjRef" /> instance specified in the <paramref name="objectRef" /> parameter is not well-formed. </exception>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure" />
	/// </PermissionSet>
	public static object Unmarshal(ObjRef objectRef, bool fRefine)
	{
		Type type = (fRefine ? objectRef.ServerType : typeof(MarshalByRefObject));
		if (type == null)
		{
			type = typeof(MarshalByRefObject);
		}
		if (objectRef.IsReferenceToWellKnow)
		{
			object remoteObject = GetRemoteObject(objectRef, type);
			TrackingServices.NotifyUnmarshaledObject(remoteObject, objectRef);
			return remoteObject;
		}
		if (type.IsContextful)
		{
			ProxyAttribute proxyAttribute = (ProxyAttribute)Attribute.GetCustomAttribute(type, typeof(ProxyAttribute), inherit: true);
			if (proxyAttribute != null)
			{
				object transparentProxy = proxyAttribute.CreateProxy(objectRef, type, null, null).GetTransparentProxy();
				TrackingServices.NotifyUnmarshaledObject(transparentProxy, objectRef);
				return transparentProxy;
			}
		}
		object proxyForRemoteObject = GetProxyForRemoteObject(objectRef, type);
		TrackingServices.NotifyUnmarshaledObject(proxyForRemoteObject, objectRef);
		return proxyForRemoteObject;
	}

	/// <summary>Takes a <see cref="T:System.MarshalByRefObject" />, registers it with the remoting infrastructure, and converts it into an instance of the <see cref="T:System.Runtime.Remoting.ObjRef" /> class.</summary>
	/// <returns>An instance of the <see cref="T:System.Runtime.Remoting.ObjRef" /> class that represents the object specified in the <paramref name="Obj" /> parameter.</returns>
	/// <param name="Obj">The object to convert. </param>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">The <paramref name="Obj" /> parameter is an object proxy. </exception>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static ObjRef Marshal(MarshalByRefObject Obj)
	{
		return Marshal(Obj, null, null);
	}

	/// <summary>Converts the given <see cref="T:System.MarshalByRefObject" /> into an instance of the <see cref="T:System.Runtime.Remoting.ObjRef" /> class with the specified URI.</summary>
	/// <returns>An instance of the <see cref="T:System.Runtime.Remoting.ObjRef" /> class that represents the object specified in the <paramref name="Obj" /> parameter.</returns>
	/// <param name="Obj">The object to convert. </param>
	/// <param name="URI">The specified URI with which to initialize the new <see cref="T:System.Runtime.Remoting.ObjRef" />. Can be null. </param>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">
	///   <paramref name="Obj" /> is an object proxy, and the <paramref name="URI" /> parameter is not null. </exception>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static ObjRef Marshal(MarshalByRefObject Obj, string URI)
	{
		return Marshal(Obj, URI, null);
	}

	/// <summary>Takes a <see cref="T:System.MarshalByRefObject" /> and converts it into an instance of the <see cref="T:System.Runtime.Remoting.ObjRef" /> class with the specified URI, and the provided <see cref="T:System.Type" />.</summary>
	/// <returns>An instance of the <see cref="T:System.Runtime.Remoting.ObjRef" /> class that represents the object specified in the <paramref name="Obj" /> parameter.</returns>
	/// <param name="Obj">The object to convert into a <see cref="T:System.Runtime.Remoting.ObjRef" />. </param>
	/// <param name="ObjURI">The URI the object specified in the <paramref name="Obj" /> parameter is marshaled with. Can be null. </param>
	/// <param name="RequestedType">The <see cref="T:System.Type" /><paramref name="Obj" /> is marshaled as. Can be null. </param>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">
	///   <paramref name="Obj" /> is a proxy of a remote object, and the <paramref name="ObjUri" /> parameter is not null. </exception>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static ObjRef Marshal(MarshalByRefObject Obj, string ObjURI, Type RequestedType)
	{
		if (IsTransparentProxy(Obj))
		{
			RealProxy realProxy = GetRealProxy(Obj);
			Identity objectIdentity = realProxy.ObjectIdentity;
			if (objectIdentity != null)
			{
				if (realProxy.GetProxiedType().IsContextful && !objectIdentity.IsConnected)
				{
					ClientActivatedIdentity obj = (ClientActivatedIdentity)objectIdentity;
					if (ObjURI == null)
					{
						ObjURI = NewUri();
					}
					obj.ObjectUri = ObjURI;
					RegisterServerIdentity(obj);
					obj.StartTrackingLifetime((ILease)Obj.InitializeLifetimeService());
					return obj.CreateObjRef(RequestedType);
				}
				if (ObjURI != null)
				{
					throw new RemotingException("It is not possible marshal a proxy of a remote object.");
				}
				ObjRef objRef = realProxy.ObjectIdentity.CreateObjRef(RequestedType);
				TrackingServices.NotifyMarshaledObject(Obj, objRef);
				return objRef;
			}
		}
		if (RequestedType == null)
		{
			RequestedType = Obj.GetType();
		}
		if (ObjURI == null)
		{
			if (Obj.ObjectIdentity == null)
			{
				ObjURI = NewUri();
				CreateClientActivatedServerIdentity(Obj, RequestedType, ObjURI);
			}
		}
		else if (!(GetIdentityForUri("/" + ObjURI) is ClientActivatedIdentity clientActivatedIdentity) || Obj != clientActivatedIdentity.GetServerObject())
		{
			CreateClientActivatedServerIdentity(Obj, RequestedType, ObjURI);
		}
		ObjRef objRef2 = ((!IsTransparentProxy(Obj)) ? Obj.CreateObjRef(RequestedType) : GetRealProxy(Obj).ObjectIdentity.CreateObjRef(RequestedType));
		TrackingServices.NotifyMarshaledObject(Obj, objRef2);
		return objRef2;
	}

	private static string NewUri()
	{
		if (app_id == null)
		{
			lock (app_id_lock)
			{
				if (app_id == null)
				{
					app_id = Guid.NewGuid().ToString().Replace('-', '_') + "/";
				}
			}
		}
		int num = Interlocked.Increment(ref next_id);
		return app_id + Environment.TickCount.ToString("x") + "_" + num + ".rem";
	}

	/// <summary>Returns the real proxy backing the specified transparent proxy.</summary>
	/// <returns>The real proxy instance backing the transparent proxy.</returns>
	/// <param name="proxy">A transparent proxy. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static RealProxy GetRealProxy(object proxy)
	{
		if (!IsTransparentProxy(proxy))
		{
			throw new RemotingException("Cannot get the real proxy from an object that is not a transparent proxy.");
		}
		return ((TransparentProxy)proxy)._rp;
	}

	/// <summary>Returns the method base from the given <see cref="T:System.Runtime.Remoting.Messaging.IMethodMessage" />.</summary>
	/// <returns>The method base extracted from the <paramref name="msg" /> parameter.</returns>
	/// <param name="msg">The method message to extract the method base from. </param>
	/// <exception cref="T:System.Security.SecurityException">Either the immediate caller does not have infrastructure permission, or at least one of the callers higher in the callstack does not have permission to retrieve the type information of non-public members. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static MethodBase GetMethodBaseFromMethodMessage(IMethodMessage msg)
	{
		Type type = Type.GetType(msg.TypeName);
		if (type == null)
		{
			throw new RemotingException("Type '" + msg.TypeName + "' not found.");
		}
		return GetMethodBaseFromName(type, msg.MethodName, (Type[])msg.MethodSignature);
	}

	internal static MethodBase GetMethodBaseFromName(Type type, string methodName, Type[] signature)
	{
		if (type.IsInterface)
		{
			return FindInterfaceMethod(type, methodName, signature);
		}
		MethodBase methodBase = null;
		methodBase = ((signature != null) ? type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, signature, null) : type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
		if (methodBase != null)
		{
			return methodBase;
		}
		if (methodName == "FieldSetter")
		{
			return FieldSetterMethod;
		}
		if (methodName == "FieldGetter")
		{
			return FieldGetterMethod;
		}
		if (signature == null)
		{
			return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
		}
		return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, signature, null);
	}

	private static MethodBase FindInterfaceMethod(Type type, string methodName, Type[] signature)
	{
		MethodBase methodBase = null;
		methodBase = ((signature != null) ? type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, signature, null) : type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
		if (methodBase != null)
		{
			return methodBase;
		}
		Type[] interfaces = type.GetInterfaces();
		for (int i = 0; i < interfaces.Length; i++)
		{
			methodBase = FindInterfaceMethod(interfaces[i], methodName, signature);
			if (methodBase != null)
			{
				return methodBase;
			}
		}
		return null;
	}

	/// <summary>Serializes the specified marshal by reference object into the provided <see cref="T:System.Runtime.Serialization.SerializationInfo" />.</summary>
	/// <param name="obj">The object to serialize. </param>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> into which the object is serialized. </param>
	/// <param name="context">The source and destination of the serialization. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> or <paramref name="info" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		Marshal((MarshalByRefObject)obj).GetObjectData(info, context);
	}

	/// <summary>Returns the <see cref="T:System.Runtime.Remoting.ObjRef" /> that represents the remote object from the specified proxy.</summary>
	/// <returns>A <see cref="T:System.Runtime.Remoting.ObjRef" /> that represents the remote object the specified proxy is connected to, or null if the object or proxy have not been marshaled.</returns>
	/// <param name="obj">A proxy connected to the object you want to create a <see cref="T:System.Runtime.Remoting.ObjRef" /> for. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static ObjRef GetObjRefForProxy(MarshalByRefObject obj)
	{
		return GetObjectIdentity(obj)?.CreateObjRef(null);
	}

	/// <summary>Returns a lifetime service object that controls the lifetime policy of the specified object.</summary>
	/// <returns>The object that controls the lifetime of <paramref name="obj" />.</returns>
	/// <param name="obj">The object to obtain lifetime service for. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static object GetLifetimeService(MarshalByRefObject obj)
	{
		return obj?.GetLifetimeService();
	}

	/// <summary>Returns a chain of envoy sinks that should be used when sending messages to the remote object represented by the specified proxy.</summary>
	/// <returns>A chain of envoy sinks associated with the specified proxy.</returns>
	/// <param name="obj">The proxy of the remote object that requested envoy sinks are associated with. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static IMessageSink GetEnvoyChainForProxy(MarshalByRefObject obj)
	{
		if (IsTransparentProxy(obj))
		{
			return ((ClientIdentity)GetRealProxy(obj).ObjectIdentity).EnvoySink;
		}
		throw new ArgumentException("obj must be a proxy.", "obj");
	}

	/// <summary>Logs the stage in a remoting exchange to an external debugger.</summary>
	/// <param name="stage">An internally defined constant that identifies the stage in a remoting exchange.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[Obsolete("It existed for only internal use in .NET and unimplemented in mono")]
	[Conditional("REMOTING_PERF")]
	[MonoTODO]
	public static void LogRemotingStage(int stage)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves a session ID for a message.</summary>
	/// <returns>A session ID string that uniquely identifies the current session.</returns>
	/// <param name="msg">The <see cref="T:System.Runtime.Remoting.Messaging.IMethodMessage" /> for which a session ID is requested. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static string GetSessionIdForMethodMessage(IMethodMessage msg)
	{
		return msg.Uri;
	}

	/// <summary>Returns a Boolean value that indicates whether the method in the given message is overloaded.</summary>
	/// <returns>true if the method called in <paramref name="msg" /> is overloaded; otherwise, false.</returns>
	/// <param name="msg">The message that contains a call to the method in question. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static bool IsMethodOverloaded(IMethodMessage msg)
	{
		RuntimeType runtimeType = (RuntimeType)msg.MethodBase.DeclaringType;
		return runtimeType.GetMethodsByName(msg.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, ignoreCase: false, runtimeType).Length > 1;
	}

	/// <summary>Returns a Boolean value that indicates whether the object specified by the given transparent proxy is contained in a different application domain than the object that called the current method.</summary>
	/// <returns>true if the object is out of the current application domain; otherwise, false.</returns>
	/// <param name="tp">The object to check. </param>
	public static bool IsObjectOutOfAppDomain(object tp)
	{
		if (!(tp is MarshalByRefObject obj))
		{
			return false;
		}
		return GetObjectIdentity(obj) is ClientIdentity;
	}

	/// <summary>Returns a Boolean value that indicates whether the object represented by the given proxy is contained in a different context than the object that called the current method.</summary>
	/// <returns>true if the object is out of the current context; otherwise, false.</returns>
	/// <param name="tp">The object to check. </param>
	public static bool IsObjectOutOfContext(object tp)
	{
		if (!(tp is MarshalByRefObject obj))
		{
			return false;
		}
		Identity objectIdentity = GetObjectIdentity(obj);
		if (objectIdentity == null)
		{
			return false;
		}
		if (objectIdentity is ServerIdentity serverIdentity)
		{
			return serverIdentity.Context != Thread.CurrentContext;
		}
		return true;
	}

	/// <summary>Returns a Boolean value that indicates whether the client that called the method specified in the given message is waiting for the server to finish processing the method before continuing execution.</summary>
	/// <returns>true if the method is one way; otherwise, false.</returns>
	/// <param name="method">The method in question. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static bool IsOneWay(MethodBase method)
	{
		return method.IsDefined(typeof(OneWayAttribute), inherit: false);
	}

	internal static bool IsAsyncMessage(IMessage msg)
	{
		if (!(msg is MonoMethodMessage))
		{
			return false;
		}
		if (((MonoMethodMessage)msg).IsAsync)
		{
			return true;
		}
		if (IsOneWay(((MonoMethodMessage)msg).MethodBase))
		{
			return true;
		}
		return false;
	}

	/// <summary>Sets the URI for the subsequent call to the <see cref="M:System.Runtime.Remoting.RemotingServices.Marshal(System.MarshalByRefObject)" /> method.</summary>
	/// <param name="obj">The object to set a URI for. </param>
	/// <param name="uri">The URI to assign to the specified object. </param>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">
	///   <paramref name="obj" /> is not a local object, has already been marshaled, or the current method has already been called on. </exception>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static void SetObjectUriForMarshal(MarshalByRefObject obj, string uri)
	{
		if (IsTransparentProxy(obj))
		{
			RealProxy realProxy = GetRealProxy(obj);
			Identity objectIdentity = realProxy.ObjectIdentity;
			if (objectIdentity != null && !(objectIdentity is ServerIdentity) && !realProxy.GetProxiedType().IsContextful)
			{
				throw new RemotingException("SetObjectUriForMarshal method should only be called for MarshalByRefObjects that exist in the current AppDomain.");
			}
		}
		Marshal(obj, uri);
	}

	internal static object CreateClientProxy(ActivatedClientTypeEntry entry, object[] activationAttributes)
	{
		if (entry.ContextAttributes != null || activationAttributes != null)
		{
			ArrayList arrayList = new ArrayList();
			if (entry.ContextAttributes != null)
			{
				arrayList.AddRange(entry.ContextAttributes);
			}
			if (activationAttributes != null)
			{
				arrayList.AddRange(activationAttributes);
			}
			return CreateClientProxy(entry.ObjectType, entry.ApplicationUrl, arrayList.ToArray());
		}
		return CreateClientProxy(entry.ObjectType, entry.ApplicationUrl, null);
	}

	internal static object CreateClientProxy(Type objectType, string url, object[] activationAttributes)
	{
		string text = url;
		if (!text.EndsWith("/"))
		{
			text += "/";
		}
		text += "RemoteActivationService.rem";
		GetClientChannelSinkChain(text, null, out var _);
		return new RemotingProxy(objectType, text, activationAttributes).GetTransparentProxy();
	}

	internal static object CreateClientProxy(WellKnownClientTypeEntry entry)
	{
		return Connect(entry.ObjectType, entry.ObjectUrl, null);
	}

	internal static object CreateClientProxyForContextBound(Type type, object[] activationAttributes)
	{
		if (type.IsContextful)
		{
			ProxyAttribute proxyAttribute = (ProxyAttribute)Attribute.GetCustomAttribute(type, typeof(ProxyAttribute), inherit: true);
			if (proxyAttribute != null)
			{
				return proxyAttribute.CreateInstance(type);
			}
		}
		return new RemotingProxy(type, ChannelServices.CrossContextUrl, activationAttributes).GetTransparentProxy();
	}

	internal static object CreateClientProxyForComInterop(Type type)
	{
		return ComInteropProxy.CreateProxy(type).GetTransparentProxy();
	}

	internal static Identity GetIdentityForUri(string uri)
	{
		string normalizedUri = GetNormalizedUri(uri);
		lock (uri_hash)
		{
			Identity identity = (Identity)uri_hash[normalizedUri];
			if (identity == null)
			{
				normalizedUri = RemoveAppNameFromUri(uri);
				if (normalizedUri != null)
				{
					identity = (Identity)uri_hash[normalizedUri];
				}
			}
			return identity;
		}
	}

	private static string RemoveAppNameFromUri(string uri)
	{
		string applicationName = RemotingConfiguration.ApplicationName;
		if (applicationName == null)
		{
			return null;
		}
		applicationName = "/" + applicationName + "/";
		if (uri.StartsWith(applicationName))
		{
			return uri.Substring(applicationName.Length);
		}
		return null;
	}

	internal static Identity GetObjectIdentity(MarshalByRefObject obj)
	{
		if (IsTransparentProxy(obj))
		{
			return GetRealProxy(obj).ObjectIdentity;
		}
		return obj.ObjectIdentity;
	}

	internal static ClientIdentity GetOrCreateClientIdentity(ObjRef objRef, Type proxyType, out object clientProxy)
	{
		object channelData = ((objRef.ChannelInfo != null) ? objRef.ChannelInfo.ChannelData : null);
		string objectUri;
		IMessageSink clientChannelSinkChain = GetClientChannelSinkChain(objRef.URI, channelData, out objectUri);
		if (objectUri == null)
		{
			objectUri = objRef.URI;
		}
		lock (uri_hash)
		{
			clientProxy = null;
			string normalizedUri = GetNormalizedUri(objRef.URI);
			if (uri_hash[normalizedUri] is ClientIdentity clientIdentity)
			{
				clientProxy = clientIdentity.ClientProxy;
				if (clientProxy != null)
				{
					return clientIdentity;
				}
				DisposeIdentity(clientIdentity);
			}
			ClientIdentity clientIdentity2 = new ClientIdentity(objectUri, objRef);
			clientIdentity2.ChannelSink = clientChannelSinkChain;
			uri_hash[normalizedUri] = clientIdentity2;
			if (proxyType != null)
			{
				RemotingProxy remotingProxy = new RemotingProxy(proxyType, clientIdentity2);
				if (clientChannelSinkChain is CrossAppDomainSink crossAppDomainSink)
				{
					remotingProxy.SetTargetDomain(crossAppDomainSink.TargetDomainId);
				}
				clientProxy = remotingProxy.GetTransparentProxy();
				clientIdentity2.ClientProxy = (MarshalByRefObject)clientProxy;
			}
			return clientIdentity2;
		}
	}

	private static IMessageSink GetClientChannelSinkChain(string url, object channelData, out string objectUri)
	{
		IMessageSink messageSink = ChannelServices.CreateClientChannelSinkChain(url, channelData, out objectUri);
		if (messageSink == null)
		{
			if (url != null)
			{
				throw new RemotingException($"Cannot create channel sink to connect to URL {url}. An appropriate channel has probably not been registered.");
			}
			throw new RemotingException(string.Format("Cannot create channel sink to connect to the remote object. An appropriate channel has probably not been registered.", url));
		}
		return messageSink;
	}

	internal static ClientActivatedIdentity CreateContextBoundObjectIdentity(Type objectType)
	{
		return new ClientActivatedIdentity(null, objectType)
		{
			ChannelSink = ChannelServices.CrossContextChannel
		};
	}

	internal static ClientActivatedIdentity CreateClientActivatedServerIdentity(MarshalByRefObject realObject, Type objectType, string objectUri)
	{
		ClientActivatedIdentity clientActivatedIdentity = new ClientActivatedIdentity(objectUri, objectType);
		clientActivatedIdentity.AttachServerObject(realObject, Context.DefaultContext);
		RegisterServerIdentity(clientActivatedIdentity);
		clientActivatedIdentity.StartTrackingLifetime((ILease)realObject.InitializeLifetimeService());
		return clientActivatedIdentity;
	}

	internal static ServerIdentity CreateWellKnownServerIdentity(Type objectType, string objectUri, WellKnownObjectMode mode)
	{
		ServerIdentity serverIdentity = ((mode != WellKnownObjectMode.SingleCall) ? ((ServerIdentity)new SingletonIdentity(objectUri, Context.DefaultContext, objectType)) : ((ServerIdentity)new SingleCallIdentity(objectUri, Context.DefaultContext, objectType)));
		RegisterServerIdentity(serverIdentity);
		return serverIdentity;
	}

	private static void RegisterServerIdentity(ServerIdentity identity)
	{
		lock (uri_hash)
		{
			if (uri_hash.ContainsKey(identity.ObjectUri))
			{
				throw new RemotingException("Uri already in use: " + identity.ObjectUri + ".");
			}
			uri_hash[identity.ObjectUri] = identity;
		}
	}

	internal static object GetProxyForRemoteObject(ObjRef objref, Type classToProxy)
	{
		if (GetIdentityForUri(objref.URI) is ClientActivatedIdentity clientActivatedIdentity)
		{
			return clientActivatedIdentity.GetServerObject();
		}
		return GetRemoteObject(objref, classToProxy);
	}

	internal static object GetRemoteObject(ObjRef objRef, Type proxyType)
	{
		GetOrCreateClientIdentity(objRef, proxyType, out var clientProxy);
		return clientProxy;
	}

	internal static object GetServerObject(string uri)
	{
		return ((GetIdentityForUri(uri) as ClientActivatedIdentity) ?? throw new RemotingException("Server for uri '" + uri + "' not found")).GetServerObject();
	}

	[SecurityPermission(SecurityAction.Assert, SerializationFormatter = true)]
	internal static byte[] SerializeCallData(object obj)
	{
		LogicalCallContext.Reader logicalCallContext = Thread.CurrentThread.GetExecutionContextReader().LogicalCallContext;
		if (!logicalCallContext.IsNull)
		{
			obj = new CACD
			{
				d = obj,
				c = logicalCallContext.Clone()
			};
		}
		if (obj == null)
		{
			return null;
		}
		MemoryStream memoryStream = new MemoryStream();
		_serializationFormatter.Serialize(memoryStream, obj);
		return memoryStream.ToArray();
	}

	[SecurityPermission(SecurityAction.Assert, SerializationFormatter = true)]
	internal static object DeserializeCallData(byte[] array)
	{
		if (array == null)
		{
			return null;
		}
		MemoryStream serializationStream = new MemoryStream(array);
		object obj = _deserializationFormatter.Deserialize(serializationStream);
		if (obj is CACD)
		{
			CACD obj2 = (CACD)obj;
			obj = obj2.d;
			LogicalCallContext logicalCallContext = (LogicalCallContext)obj2.c;
			if (logicalCallContext.HasInfo)
			{
				Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext.Merge(logicalCallContext);
			}
		}
		return obj;
	}

	[SecurityPermission(SecurityAction.Assert, SerializationFormatter = true)]
	internal static byte[] SerializeExceptionData(Exception ex)
	{
		byte[] array = null;
		try
		{
		}
		finally
		{
			MemoryStream memoryStream = new MemoryStream();
			_serializationFormatter.Serialize(memoryStream, ex);
			array = memoryStream.ToArray();
		}
		return array;
	}

	internal static object GetDomainProxy(AppDomain domain)
	{
		byte[] array = null;
		Context currentContext = Thread.CurrentContext;
		try
		{
			array = (byte[])AppDomain.InvokeInDomain(domain, typeof(AppDomain).GetMethod("GetMarshalledDomainObjRef", BindingFlags.Instance | BindingFlags.NonPublic), domain, null);
		}
		finally
		{
			AppDomain.InternalSetContext(currentContext);
		}
		byte[] array2 = new byte[array.Length];
		array.CopyTo(array2, 0);
		return (AppDomain)Unmarshal((ObjRef)CADSerializer.DeserializeObject(new MemoryStream(array2)));
	}

	private static void RegisterInternalChannels()
	{
		CrossAppDomainChannel.RegisterCrossAppDomainChannel();
	}

	internal static void DisposeIdentity(Identity ident)
	{
		lock (uri_hash)
		{
			if (!ident.Disposed)
			{
				if (ident is ClientIdentity clientIdentity)
				{
					uri_hash.Remove(GetNormalizedUri(clientIdentity.TargetUri));
				}
				else
				{
					uri_hash.Remove(ident.ObjectUri);
				}
				ident.Disposed = true;
			}
		}
	}

	internal static Identity GetMessageTargetIdentity(IMessage msg)
	{
		if (msg is IInternalMessage)
		{
			return ((IInternalMessage)msg).TargetIdentity;
		}
		lock (uri_hash)
		{
			string normalizedUri = GetNormalizedUri(((IMethodMessage)msg).Uri);
			return uri_hash[normalizedUri] as ServerIdentity;
		}
	}

	internal static void SetMessageTargetIdentity(IMessage msg, Identity ident)
	{
		if (msg is IInternalMessage)
		{
			((IInternalMessage)msg).TargetIdentity = ident;
		}
	}

	internal static bool UpdateOutArgObject(ParameterInfo pi, object local, object remote)
	{
		if (pi.ParameterType.IsArray && ((Array)local).Rank == 1)
		{
			Array array = (Array)local;
			if (array.Rank == 1)
			{
				Array.Copy((Array)remote, array, array.Length);
				return true;
			}
		}
		return false;
	}

	private static string GetNormalizedUri(string uri)
	{
		if (uri.StartsWith("/"))
		{
			return uri.Substring(1);
		}
		return uri;
	}
}
