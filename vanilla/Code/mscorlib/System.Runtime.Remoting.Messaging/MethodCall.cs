using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;

namespace System.Runtime.Remoting.Messaging;

/// <summary>Implements the <see cref="T:System.Runtime.Remoting.Messaging.IMethodCallMessage" /> interface to create a request message that acts as a method call on a remote object.</summary>
[Serializable]
[ComVisible(true)]
[CLSCompliant(false)]
public class MethodCall : IMethodCallMessage, IMethodMessage, IMessage, ISerializable, IInternalMessage, ISerializationRootObject
{
	private string _uri;

	private string _typeName;

	private string _methodName;

	private object[] _args;

	private Type[] _methodSignature;

	private MethodBase _methodBase;

	private LogicalCallContext _callContext;

	private ArgInfo _inArgInfo;

	private Identity _targetIdentity;

	private Type[] _genericArguments;

	/// <summary>An <see cref="T:System.Collections.IDictionary" /> interface that represents a collection of the remoting message's properties. </summary>
	protected IDictionary ExternalProperties;

	/// <summary>An <see cref="T:System.Collections.IDictionary" /> interface that represents a collection of the remoting message's properties. </summary>
	protected IDictionary InternalProperties;

	/// <summary>Gets the number of arguments passed to a method. </summary>
	/// <returns>A <see cref="T:System.Int32" /> that represents the number of arguments passed to a method.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public int ArgCount
	{
		[SecurityCritical]
		get
		{
			return _args.Length;
		}
	}

	/// <summary>Gets an array of arguments passed to a method. </summary>
	/// <returns>An array of type <see cref="T:System.Object" /> that represents the arguments passed to a method.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public object[] Args
	{
		[SecurityCritical]
		get
		{
			return _args;
		}
	}

	/// <summary>Gets a value that indicates whether the method can accept a variable number of arguments. </summary>
	/// <returns>true if the method can accept a variable number of arguments; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public bool HasVarArgs
	{
		[SecurityCritical]
		get
		{
			return (MethodBase.CallingConvention | CallingConventions.VarArgs) != (CallingConventions)0;
		}
	}

	/// <summary>Gets the number of arguments in the method call that are not marked as out parameters.</summary>
	/// <returns>A <see cref="T:System.Int32" /> that represents the number of arguments in the method call that are not marked as out parameters.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public int InArgCount
	{
		[SecurityCritical]
		get
		{
			if (_inArgInfo == null)
			{
				_inArgInfo = new ArgInfo(_methodBase, ArgInfoType.In);
			}
			return _inArgInfo.GetInOutArgCount();
		}
	}

	/// <summary>Gets an array of arguments in the method call that are not marked as out parameters. </summary>
	/// <returns>An array of type <see cref="T:System.Object" /> that represents arguments in the method call that are not marked as out parameters.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public object[] InArgs
	{
		[SecurityCritical]
		get
		{
			if (_inArgInfo == null)
			{
				_inArgInfo = new ArgInfo(_methodBase, ArgInfoType.In);
			}
			return _inArgInfo.GetInOutArgs(_args);
		}
	}

	/// <summary>Gets the <see cref="T:System.Runtime.Remoting.Messaging.LogicalCallContext" /> for the current method call. </summary>
	/// <returns>The <see cref="T:System.Runtime.Remoting.Messaging.LogicalCallContext" /> for the current method call.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public LogicalCallContext LogicalCallContext
	{
		[SecurityCritical]
		get
		{
			if (_callContext == null)
			{
				_callContext = new LogicalCallContext();
			}
			return _callContext;
		}
	}

	/// <summary>Gets the <see cref="T:System.Reflection.MethodBase" /> of the called method. </summary>
	/// <returns>The <see cref="T:System.Reflection.MethodBase" /> of the called method.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public MethodBase MethodBase
	{
		[SecurityCritical]
		get
		{
			if (_methodBase == null)
			{
				ResolveMethod();
			}
			return _methodBase;
		}
	}

	/// <summary>Gets the name of the invoked method. </summary>
	/// <returns>A <see cref="T:System.String" /> that contains the name of the invoked method.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public string MethodName
	{
		[SecurityCritical]
		get
		{
			if (_methodName == null)
			{
				_methodName = _methodBase.Name;
			}
			return _methodName;
		}
	}

	/// <summary>Gets an object that contains the method signature. </summary>
	/// <returns>A <see cref="T:System.Object" /> that contains the method signature.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public object MethodSignature
	{
		[SecurityCritical]
		get
		{
			if (_methodSignature == null && _methodBase != null)
			{
				ParameterInfo[] parameters = _methodBase.GetParameters();
				_methodSignature = new Type[parameters.Length];
				for (int i = 0; i < parameters.Length; i++)
				{
					_methodSignature[i] = parameters[i].ParameterType;
				}
			}
			return _methodSignature;
		}
	}

	/// <summary>Gets an <see cref="T:System.Collections.IDictionary" /> interface that represents a collection of the remoting message's properties. </summary>
	/// <returns>An <see cref="T:System.Collections.IDictionary" /> interface that represents a collection of the remoting message's properties.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual IDictionary Properties
	{
		[SecurityCritical]
		get
		{
			if (ExternalProperties == null)
			{
				InitDictionary();
			}
			return ExternalProperties;
		}
	}

	/// <summary>Gets the full type name of the remote object on which the method call is being made. </summary>
	/// <returns>A <see cref="T:System.String" /> that contains the full type name of the remote object on which the method call is being made.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public string TypeName
	{
		[SecurityCritical]
		get
		{
			if (_typeName == null)
			{
				_typeName = _methodBase.DeclaringType.AssemblyQualifiedName;
			}
			return _typeName;
		}
	}

	/// <summary>Gets or sets the Uniform Resource Identifier (URI) of the remote object on which the method call is being made.</summary>
	/// <returns>The URI of a remote object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public string Uri
	{
		[SecurityCritical]
		get
		{
			return _uri;
		}
		set
		{
			_uri = value;
		}
	}

	string IInternalMessage.Uri
	{
		get
		{
			return Uri;
		}
		set
		{
			Uri = value;
		}
	}

	Identity IInternalMessage.TargetIdentity
	{
		get
		{
			return _targetIdentity;
		}
		set
		{
			_targetIdentity = value;
		}
	}

	private Type[] GenericArguments
	{
		get
		{
			if (_genericArguments != null)
			{
				return _genericArguments;
			}
			return _genericArguments = MethodBase.GetGenericArguments();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Messaging.MethodCall" /> class from an array of remoting headers.</summary>
	/// <param name="h1">An array of remoting headers that contains key/value pairs. This array is used to initialize <see cref="T:System.Runtime.Remoting.Messaging.MethodCall" /> fields for headers that belong to the namespace "http://schemas.microsoft.com/clr/soap/messageProperties".</param>
	public MethodCall(Header[] h1)
	{
		Init();
		if (h1 != null && h1.Length != 0)
		{
			foreach (Header header in h1)
			{
				InitMethodProperty(header.Name, header.Value);
			}
			ResolveMethod();
		}
	}

	internal MethodCall(SerializationInfo info, StreamingContext context)
	{
		Init();
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			InitMethodProperty(current.Name, current.Value);
		}
	}

	internal MethodCall(CADMethodCallMessage msg)
	{
		_uri = string.Copy(msg.Uri);
		ArrayList arguments = msg.GetArguments();
		_args = msg.GetArgs(arguments);
		_callContext = msg.GetLogicalCallContext(arguments);
		if (_callContext == null)
		{
			_callContext = new LogicalCallContext();
		}
		_methodBase = msg.GetMethod();
		Init();
		if (msg.PropertiesCount > 0)
		{
			CADMessageBase.UnmarshalProperties(Properties, msg.PropertiesCount, arguments);
		}
	}

	/// <summary>Initializes  a new instance of the <see cref="T:System.Runtime.Remoting.Messaging.MethodCall" /> class by copying an existing message.</summary>
	/// <param name="msg">A remoting message.</param>
	public MethodCall(IMessage msg)
	{
		if (msg is IMethodMessage)
		{
			CopyFrom((IMethodMessage)msg);
			return;
		}
		foreach (DictionaryEntry property in msg.Properties)
		{
			InitMethodProperty((string)property.Key, property.Value);
		}
		Init();
	}

	internal MethodCall(string uri, string typeName, string methodName, object[] args)
	{
		_uri = uri;
		_typeName = typeName;
		_methodName = methodName;
		_args = args;
		Init();
		ResolveMethod();
	}

	internal MethodCall(object handlerObject, BinaryMethodCallMessage smuggledMsg)
	{
		if (handlerObject != null)
		{
			_uri = handlerObject as string;
			if (_uri == null && handlerObject is MarshalByRefObject)
			{
				throw new NotImplementedException("MarshalByRefObject.GetIdentity");
			}
		}
		_typeName = smuggledMsg.TypeName;
		_methodName = smuggledMsg.MethodName;
		_methodSignature = (Type[])smuggledMsg.MethodSignature;
		_args = smuggledMsg.Args;
		_genericArguments = smuggledMsg.InstantiationArgs;
		_callContext = smuggledMsg.LogicalCallContext;
		ResolveMethod();
		if (smuggledMsg.HasProperties)
		{
			smuggledMsg.PopulateMessageProperties(Properties);
		}
	}

	internal MethodCall()
	{
	}

	internal void CopyFrom(IMethodMessage call)
	{
		_uri = call.Uri;
		_typeName = call.TypeName;
		_methodName = call.MethodName;
		_args = call.Args;
		_methodSignature = (Type[])call.MethodSignature;
		_methodBase = call.MethodBase;
		_callContext = call.LogicalCallContext;
		Init();
	}

	internal virtual void InitMethodProperty(string key, object value)
	{
		switch (key)
		{
		case "__TypeName":
			_typeName = (string)value;
			break;
		case "__MethodName":
			_methodName = (string)value;
			break;
		case "__MethodSignature":
			_methodSignature = (Type[])value;
			break;
		case "__Args":
			_args = (object[])value;
			break;
		case "__CallContext":
			_callContext = (LogicalCallContext)value;
			break;
		case "__Uri":
			_uri = (string)value;
			break;
		case "__GenericArguments":
			_genericArguments = (Type[])value;
			break;
		default:
			Properties[key] = value;
			break;
		}
	}

	/// <summary>The <see cref="M:System.Runtime.Remoting.Messaging.MethodCall.GetObjectData(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)" /> method is not implemented. </summary>
	/// <param name="info">The data for serializing or deserializing the remote object.</param>
	/// <param name="context">The context of a certain serialized stream.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter, Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("__TypeName", _typeName);
		info.AddValue("__MethodName", _methodName);
		info.AddValue("__MethodSignature", _methodSignature);
		info.AddValue("__Args", _args);
		info.AddValue("__CallContext", _callContext);
		info.AddValue("__Uri", _uri);
		info.AddValue("__GenericArguments", _genericArguments);
		if (InternalProperties == null)
		{
			return;
		}
		foreach (DictionaryEntry internalProperty in InternalProperties)
		{
			info.AddValue((string)internalProperty.Key, internalProperty.Value);
		}
	}

	internal virtual void InitDictionary()
	{
		InternalProperties = ((MessageDictionary)(ExternalProperties = new MCMDictionary(this))).GetInternalProperties();
	}

	/// <summary>Gets a method argument, as an object, at a specified index. </summary>
	/// <returns>The method argument as an object.</returns>
	/// <param name="argNum">The index of the requested argument.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public object GetArg(int argNum)
	{
		return _args[argNum];
	}

	/// <summary>Gets the name of a method argument at a specified index. </summary>
	/// <returns>The name of the method argument.</returns>
	/// <param name="index">The index of the requested argument.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public string GetArgName(int index)
	{
		return _methodBase.GetParameters()[index].Name;
	}

	/// <summary>Gets a method argument at a specified index that is not marked as an out parameter. </summary>
	/// <returns>The method argument that is not marked as an out parameter.</returns>
	/// <param name="argNum">The index of the requested argument.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public object GetInArg(int argNum)
	{
		if (_inArgInfo == null)
		{
			_inArgInfo = new ArgInfo(_methodBase, ArgInfoType.In);
		}
		return _args[_inArgInfo.GetInOutArgIndex(argNum)];
	}

	/// <summary>Gets the name of a method argument at a specified index that is not marked as an out parameter.</summary>
	/// <returns>The name of the method argument that is not marked as an out parameter.</returns>
	/// <param name="index">The index of the requested argument. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public string GetInArgName(int index)
	{
		if (_inArgInfo == null)
		{
			_inArgInfo = new ArgInfo(_methodBase, ArgInfoType.In);
		}
		return _inArgInfo.GetInOutArgName(index);
	}

	/// <summary>Initializes an internal serialization handler from an array of remoting headers that are applied to a method. </summary>
	/// <returns>An internal serialization handler.</returns>
	/// <param name="h">An array of remoting headers that contain key/value pairs. This array is used to initialize <see cref="T:System.Runtime.Remoting.Messaging.MethodCall" /> fields for headers that belong to the namespace "http://schemas.microsoft.com/clr/soap/messageProperties".</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[MonoTODO]
	public virtual object HeaderHandler(Header[] h)
	{
		throw new NotImplementedException();
	}

	/// <summary>Initializes a <see cref="T:System.Runtime.Remoting.Messaging.MethodCall" />. </summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual void Init()
	{
	}

	/// <summary>Sets method information from previously initialized remoting message properties. </summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public void ResolveMethod()
	{
		if (_uri != null)
		{
			Type serverTypeForUri = RemotingServices.GetServerTypeForUri(_uri);
			if (serverTypeForUri == null)
			{
				string text = ((_typeName != null) ? (" (" + _typeName + ")") : "");
				throw new RemotingException("Requested service not found" + text + ". No receiver for uri " + _uri);
			}
			Type type = CastTo(_typeName, serverTypeForUri);
			if (type == null)
			{
				throw new RemotingException("Cannot cast from client type '" + _typeName + "' to server type '" + serverTypeForUri.FullName + "'");
			}
			_methodBase = RemotingServices.GetMethodBaseFromName(type, _methodName, _methodSignature);
			if (_methodBase == null)
			{
				throw new RemotingException("Method " + _methodName + " not found in " + type);
			}
			if (type != serverTypeForUri && type.IsInterface && !serverTypeForUri.IsInterface)
			{
				_methodBase = RemotingServices.GetVirtualMethod(serverTypeForUri, _methodBase);
				if (_methodBase == null)
				{
					throw new RemotingException("Method " + _methodName + " not found in " + serverTypeForUri);
				}
			}
		}
		else
		{
			_methodBase = RemotingServices.GetMethodBaseFromMethodMessage(this);
			if (_methodBase == null)
			{
				throw new RemotingException("Method " + _methodName + " not found in " + TypeName);
			}
		}
		if (_methodBase.IsGenericMethod && _methodBase.ContainsGenericParameters)
		{
			if (GenericArguments == null)
			{
				throw new RemotingException("The remoting infrastructure does not support open generic methods.");
			}
			_methodBase = ((MethodInfo)_methodBase).MakeGenericMethod(GenericArguments);
		}
	}

	private Type CastTo(string clientType, Type serverType)
	{
		clientType = GetTypeNameFromAssemblyQualifiedName(clientType);
		if (clientType == serverType.FullName)
		{
			return serverType;
		}
		Type baseType = serverType.BaseType;
		while (baseType != null)
		{
			if (clientType == baseType.FullName)
			{
				return baseType;
			}
			baseType = baseType.BaseType;
		}
		Type[] interfaces = serverType.GetInterfaces();
		foreach (Type type in interfaces)
		{
			if (clientType == type.FullName)
			{
				return type;
			}
		}
		return null;
	}

	private static string GetTypeNameFromAssemblyQualifiedName(string aqname)
	{
		int num = aqname.IndexOf("]]");
		int num2 = aqname.IndexOf(',', (num != -1) ? (num + 2) : 0);
		if (num2 != -1)
		{
			aqname = aqname.Substring(0, num2).Trim();
		}
		return aqname;
	}

	/// <summary>Sets method information from serialization settings. </summary>
	/// <param name="info">The data for serializing or deserializing the remote object.</param>
	/// <param name="ctx">The context of a given serialized stream.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[MonoTODO]
	public void RootSetObjectData(SerializationInfo info, StreamingContext ctx)
	{
		throw new NotImplementedException();
	}

	bool IInternalMessage.HasProperties()
	{
		if (ExternalProperties == null)
		{
			return InternalProperties != null;
		}
		return true;
	}
}
