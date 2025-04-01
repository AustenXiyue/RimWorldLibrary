using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;

namespace System.Runtime.Remoting.Messaging;

/// <summary>Implements the <see cref="T:System.Runtime.Remoting.Messaging.IMethodReturnMessage" /> interface to create a message that acts as a method response on a remote object.</summary>
[Serializable]
[ComVisible(true)]
[CLSCompliant(false)]
public class MethodResponse : IMethodReturnMessage, IMethodMessage, IMessage, ISerializable, IInternalMessage, ISerializationRootObject
{
	private string _methodName;

	private string _uri;

	private string _typeName;

	private MethodBase _methodBase;

	private object _returnValue;

	private Exception _exception;

	private Type[] _methodSignature;

	private ArgInfo _inArgInfo;

	private object[] _args;

	private object[] _outArgs;

	private IMethodCallMessage _callMsg;

	private LogicalCallContext _callContext;

	private Identity _targetIdentity;

	/// <summary>Specifies an <see cref="T:System.Collections.IDictionary" /> interface that represents a collection of the remoting message's properties. </summary>
	protected IDictionary ExternalProperties;

	/// <summary>Specifies an <see cref="T:System.Collections.IDictionary" /> interface that represents a collection of the remoting message's properties. </summary>
	protected IDictionary InternalProperties;

	/// <summary>Gets the number of arguments passed to the method. </summary>
	/// <returns>A <see cref="T:System.Int32" /> that represents the number of arguments passed to a method.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public int ArgCount
	{
		[SecurityCritical]
		get
		{
			if (_args == null)
			{
				return 0;
			}
			return _args.Length;
		}
	}

	/// <summary>Gets an array of arguments passed to the method. </summary>
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

	/// <summary>Gets the exception thrown during the method call, or null if the method did not throw an exception. </summary>
	/// <returns>The <see cref="T:System.Exception" /> thrown during the method call, or null if the method did not throw an exception.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public Exception Exception
	{
		[SecurityCritical]
		get
		{
			return _exception;
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
			if (null == _methodBase)
			{
				if (_callMsg != null)
				{
					_methodBase = _callMsg.MethodBase;
				}
				else if (MethodName != null && TypeName != null)
				{
					_methodBase = RemotingServices.GetMethodBaseFromMethodMessage(this);
				}
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
			if (_methodName == null && _callMsg != null)
			{
				_methodName = _callMsg.MethodName;
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
			if (_methodSignature == null && _callMsg != null)
			{
				_methodSignature = (Type[])_callMsg.MethodSignature;
			}
			return _methodSignature;
		}
	}

	/// <summary>Gets the number of arguments in the method call marked as ref or out parameters.</summary>
	/// <returns>A <see cref="T:System.Int32" /> that represents the number of arguments in the method call marked as ref or out parameters.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public int OutArgCount
	{
		[SecurityCritical]
		get
		{
			if (_args == null || _args.Length == 0)
			{
				return 0;
			}
			if (_inArgInfo == null)
			{
				_inArgInfo = new ArgInfo(MethodBase, ArgInfoType.Out);
			}
			return _inArgInfo.GetInOutArgCount();
		}
	}

	/// <summary>Gets an array of arguments in the method call that are marked as ref or out parameters.</summary>
	/// <returns>An array of type <see cref="T:System.Object" /> that represents the arguments in the method call that are marked as ref or out parameters.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public object[] OutArgs
	{
		[SecurityCritical]
		get
		{
			if (_outArgs == null && _args != null)
			{
				if (_inArgInfo == null)
				{
					_inArgInfo = new ArgInfo(MethodBase, ArgInfoType.Out);
				}
				_outArgs = _inArgInfo.GetInOutArgs(_args);
			}
			return _outArgs;
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
				InternalProperties = ((MessageDictionary)(ExternalProperties = new MethodReturnDictionary(this))).GetInternalProperties();
			}
			return ExternalProperties;
		}
	}

	/// <summary>Gets the return value of the method call. </summary>
	/// <returns>A <see cref="T:System.Object" /> that represents the return value of the method call.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public object ReturnValue
	{
		[SecurityCritical]
		get
		{
			return _returnValue;
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
			if (_typeName == null && _callMsg != null)
			{
				_typeName = _callMsg.TypeName;
			}
			return _typeName;
		}
	}

	/// <summary>Gets the Uniform Resource Identifier (URI) of the remote object on which the method call is being made. </summary>
	/// <returns>The URI of a remote object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public string Uri
	{
		[SecurityCritical]
		get
		{
			if (_uri == null && _callMsg != null)
			{
				_uri = _callMsg.Uri;
			}
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Messaging.MethodResponse" /> class from an array of remoting headers and a request message. </summary>
	/// <param name="h1">An array of remoting headers that contains key/value pairs. This array is used to initialize <see cref="T:System.Runtime.Remoting.Messaging.MethodResponse" /> fields for headers that belong to the namespace "http://schemas.microsoft.com/clr/soap/messageProperties".</param>
	/// <param name="mcm">A request message that acts as a method call on a remote object.</param>
	public MethodResponse(Header[] h1, IMethodCallMessage mcm)
	{
		if (mcm != null)
		{
			_methodName = mcm.MethodName;
			_uri = mcm.Uri;
			_typeName = mcm.TypeName;
			_methodBase = mcm.MethodBase;
			_methodSignature = (Type[])mcm.MethodSignature;
			_args = mcm.Args;
		}
		if (h1 != null)
		{
			foreach (Header header in h1)
			{
				InitMethodProperty(header.Name, header.Value);
			}
		}
	}

	internal MethodResponse(Exception e, IMethodCallMessage msg)
	{
		_callMsg = msg;
		if (msg != null)
		{
			_uri = msg.Uri;
		}
		else
		{
			_uri = string.Empty;
		}
		_exception = e;
		_returnValue = null;
		_outArgs = new object[0];
	}

	internal MethodResponse(object returnValue, object[] outArgs, LogicalCallContext callCtx, IMethodCallMessage msg)
	{
		_callMsg = msg;
		_uri = msg.Uri;
		_exception = null;
		_returnValue = returnValue;
		_args = outArgs;
	}

	internal MethodResponse(IMethodCallMessage msg, CADMethodReturnMessage retmsg)
	{
		_callMsg = msg;
		_methodBase = msg.MethodBase;
		_uri = msg.Uri;
		_methodName = msg.MethodName;
		ArrayList arguments = retmsg.GetArguments();
		_exception = retmsg.GetException(arguments);
		_returnValue = retmsg.GetReturnValue(arguments);
		_args = retmsg.GetArgs(arguments);
		_callContext = retmsg.GetLogicalCallContext(arguments);
		if (_callContext == null)
		{
			_callContext = new LogicalCallContext();
		}
		if (retmsg.PropertiesCount > 0)
		{
			CADMessageBase.UnmarshalProperties(Properties, retmsg.PropertiesCount, arguments);
		}
	}

	internal MethodResponse(IMethodCallMessage msg, object handlerObject, BinaryMethodReturnMessage smuggledMrm)
	{
		if (msg != null)
		{
			_methodBase = msg.MethodBase;
			_methodName = msg.MethodName;
			_uri = msg.Uri;
		}
		_returnValue = smuggledMrm.ReturnValue;
		_args = smuggledMrm.Args;
		_exception = smuggledMrm.Exception;
		_callContext = smuggledMrm.LogicalCallContext;
		if (smuggledMrm.HasProperties)
		{
			smuggledMrm.PopulateMessageProperties(Properties);
		}
	}

	internal MethodResponse(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			InitMethodProperty(current.Name, current.Value);
		}
	}

	internal void InitMethodProperty(string key, object value)
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
		case "__Uri":
			_uri = (string)value;
			break;
		case "__Return":
			_returnValue = value;
			break;
		case "__OutArgs":
			_args = (object[])value;
			break;
		case "__fault":
			_exception = (Exception)value;
			break;
		case "__CallContext":
			_callContext = (LogicalCallContext)value;
			break;
		default:
			Properties[key] = value;
			break;
		}
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
		if (_args == null)
		{
			return null;
		}
		return _args[argNum];
	}

	/// <summary>Gets the name of a method argument at a specified index. </summary>
	/// <returns>The name of the method argument.</returns>
	/// <param name="index">The index of the requested argument.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public string GetArgName(int index)
	{
		return MethodBase.GetParameters()[index].Name;
	}

	/// <summary>The <see cref="M:System.Runtime.Remoting.Messaging.MethodResponse.GetObjectData(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)" /> method is not implemented. </summary>
	/// <param name="info">Data for serializing or deserializing the remote object.</param>
	/// <param name="context">Context of a certain serialized stream.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter, Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (_exception == null)
		{
			info.AddValue("__TypeName", _typeName);
			info.AddValue("__MethodName", _methodName);
			info.AddValue("__MethodSignature", _methodSignature);
			info.AddValue("__Uri", _uri);
			info.AddValue("__Return", _returnValue);
			info.AddValue("__OutArgs", _args);
		}
		else
		{
			info.AddValue("__fault", _exception);
		}
		info.AddValue("__CallContext", _callContext);
		if (InternalProperties == null)
		{
			return;
		}
		foreach (DictionaryEntry internalProperty in InternalProperties)
		{
			info.AddValue((string)internalProperty.Key, internalProperty.Value);
		}
	}

	/// <summary>Returns the specified argument marked as a ref parameter or an out parameter. </summary>
	/// <returns>The specified argument marked as a ref parameter or an out parameter.</returns>
	/// <param name="argNum">The index of the requested argument.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public object GetOutArg(int argNum)
	{
		if (_args == null)
		{
			return null;
		}
		if (_inArgInfo == null)
		{
			_inArgInfo = new ArgInfo(MethodBase, ArgInfoType.Out);
		}
		return _args[_inArgInfo.GetInOutArgIndex(argNum)];
	}

	/// <summary>Returns the name of the specified argument marked as a ref parameter or an out parameter.</summary>
	/// <returns>The argument name, or null if the current method is not implemented.</returns>
	/// <param name="index">The index of the requested argument.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public string GetOutArgName(int index)
	{
		if (null == _methodBase)
		{
			return "__method_" + index;
		}
		if (_inArgInfo == null)
		{
			_inArgInfo = new ArgInfo(MethodBase, ArgInfoType.Out);
		}
		return _inArgInfo.GetInOutArgName(index);
	}

	/// <summary>Initializes an internal serialization handler from an array of remoting headers that are applied to a method. </summary>
	/// <returns>An internal serialization handler.</returns>
	/// <param name="h">An array of remoting headers that contain key/value pairs. This array is used to initialize <see cref="T:System.Runtime.Remoting.Messaging.MethodResponse" /> fields for headers that belong to the namespace "http://schemas.microsoft.com/clr/soap/messageProperties".</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[MonoTODO]
	public virtual object HeaderHandler(Header[] h)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sets method information from serialization settings. </summary>
	/// <param name="info">The data for serializing or deserializing the remote object.</param>
	/// <param name="ctx">The context of a certain serialized stream.</param>
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
