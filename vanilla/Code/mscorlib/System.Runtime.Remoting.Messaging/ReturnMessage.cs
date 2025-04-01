using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Runtime.Remoting.Messaging;

/// <summary>Holds a message returned in response to a method call on a remote object.</summary>
[ComVisible(true)]
public class ReturnMessage : IMethodReturnMessage, IMethodMessage, IMessage, IInternalMessage
{
	private object[] _outArgs;

	private object[] _args;

	private LogicalCallContext _callCtx;

	private object _returnValue;

	private string _uri;

	private Exception _exception;

	private MethodBase _methodBase;

	private string _methodName;

	private Type[] _methodSignature;

	private string _typeName;

	private MethodReturnDictionary _properties;

	private Identity _targetIdentity;

	private ArgInfo _inArgInfo;

	/// <summary>Gets the number of arguments of the called method.</summary>
	/// <returns>The number of arguments of the called method.</returns>
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

	/// <summary>Gets a specified argument passed to the method called on the remote object.</summary>
	/// <returns>An argument passed to the method called on the remote object.</returns>
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

	/// <summary>Gets a value indicating whether the called method accepts a variable number of arguments.</summary>
	/// <returns>true if the called method accepts a variable number of arguments; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public bool HasVarArgs
	{
		[SecurityCritical]
		get
		{
			if (_methodBase == null)
			{
				return false;
			}
			return (_methodBase.CallingConvention | CallingConventions.VarArgs) != (CallingConventions)0;
		}
	}

	/// <summary>Gets the <see cref="T:System.Runtime.Remoting.Messaging.LogicalCallContext" /> of the called method.</summary>
	/// <returns>The <see cref="T:System.Runtime.Remoting.Messaging.LogicalCallContext" /> of the called method.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public LogicalCallContext LogicalCallContext
	{
		[SecurityCritical]
		get
		{
			if (_callCtx == null)
			{
				_callCtx = new LogicalCallContext();
			}
			return _callCtx;
		}
	}

	/// <summary>Gets the <see cref="T:System.Reflection.MethodBase" /> of the called method.</summary>
	/// <returns>The <see cref="T:System.Reflection.MethodBase" /> of the called method.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public MethodBase MethodBase
	{
		[SecurityCritical]
		get
		{
			return _methodBase;
		}
	}

	/// <summary>Gets the name of the called method.</summary>
	/// <returns>The name of the method that the current <see cref="T:System.Runtime.Remoting.Messaging.ReturnMessage" /> originated from.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public string MethodName
	{
		[SecurityCritical]
		get
		{
			if (_methodBase != null && _methodName == null)
			{
				_methodName = _methodBase.Name;
			}
			return _methodName;
		}
	}

	/// <summary>Gets an array of <see cref="T:System.Type" /> objects containing the method signature.</summary>
	/// <returns>An array of <see cref="T:System.Type" /> objects containing the method signature.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public object MethodSignature
	{
		[SecurityCritical]
		get
		{
			if (_methodBase != null && _methodSignature == null)
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

	/// <summary>Gets an <see cref="T:System.Collections.IDictionary" /> of properties contained in the current <see cref="T:System.Runtime.Remoting.Messaging.ReturnMessage" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionary" /> of properties contained in the current <see cref="T:System.Runtime.Remoting.Messaging.ReturnMessage" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual IDictionary Properties
	{
		[SecurityCritical]
		get
		{
			if (_properties == null)
			{
				_properties = new MethodReturnDictionary(this);
			}
			return _properties;
		}
	}

	/// <summary>Gets the name of the type on which the remote method was called.</summary>
	/// <returns>The type name of the remote object on which the remote method was called.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public string TypeName
	{
		[SecurityCritical]
		get
		{
			if (_methodBase != null && _typeName == null)
			{
				_typeName = _methodBase.DeclaringType.AssemblyQualifiedName;
			}
			return _typeName;
		}
	}

	/// <summary>Gets or sets the URI of the remote object on which the remote method was called.</summary>
	/// <returns>The URI of the remote object on which the remote method was called.</returns>
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

	/// <summary>Gets the exception that was thrown during the remote method call.</summary>
	/// <returns>The exception thrown during the method call, or null if an exception did not occur during the call.</returns>
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

	/// <summary>Gets the number of out or ref arguments on the called method.</summary>
	/// <returns>The number of out or ref arguments on the called method.</returns>
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

	/// <summary>Gets a specified object passed as an out or ref parameter to the called method.</summary>
	/// <returns>An object passed as an out or ref parameter to the called method.</returns>
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

	/// <summary>Gets the object returned by the called method.</summary>
	/// <returns>The object returned by the called method.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual object ReturnValue
	{
		[SecurityCritical]
		get
		{
			return _returnValue;
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Messaging.ReturnMessage" /> class with all the information returning to the caller after the method call.</summary>
	/// <param name="ret">The object returned by the invoked method from which the current <see cref="T:System.Runtime.Remoting.Messaging.ReturnMessage" /> instance originated. </param>
	/// <param name="outArgs">The objects returned from the invoked method as out parameters. </param>
	/// <param name="outArgsCount">The number of out parameters returned from the invoked method. </param>
	/// <param name="callCtx">The <see cref="T:System.Runtime.Remoting.Messaging.LogicalCallContext" /> of the method call. </param>
	/// <param name="mcm">The original method call to the invoked method. </param>
	public ReturnMessage(object ret, object[] outArgs, int outArgsCount, LogicalCallContext callCtx, IMethodCallMessage mcm)
	{
		_returnValue = ret;
		_args = outArgs;
		_callCtx = callCtx;
		if (mcm != null)
		{
			_uri = mcm.Uri;
			_methodBase = mcm.MethodBase;
		}
		if (_args == null)
		{
			_args = new object[outArgsCount];
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Messaging.ReturnMessage" /> class.</summary>
	/// <param name="e">The exception that was thrown during execution of the remotely called method. </param>
	/// <param name="mcm">An <see cref="T:System.Runtime.Remoting.Messaging.IMethodCallMessage" /> with which to create an instance of the <see cref="T:System.Runtime.Remoting.Messaging.ReturnMessage" /> class. </param>
	public ReturnMessage(Exception e, IMethodCallMessage mcm)
	{
		_exception = e;
		if (mcm != null)
		{
			_methodBase = mcm.MethodBase;
			_callCtx = mcm.LogicalCallContext;
		}
		_args = new object[0];
	}

	/// <summary>Returns a specified argument passed to the remote method during the method call.</summary>
	/// <returns>An argument passed to the remote method during the method call.</returns>
	/// <param name="argNum">The zero-based index of the requested argument. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public object GetArg(int argNum)
	{
		return _args[argNum];
	}

	/// <summary>Returns the name of a specified method argument.</summary>
	/// <returns>The name of a specified method argument.</returns>
	/// <param name="index">The zero-based index of the requested argument name. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public string GetArgName(int index)
	{
		return _methodBase.GetParameters()[index].Name;
	}

	/// <summary>Returns the object passed as an out or ref parameter during the remote method call.</summary>
	/// <returns>The object passed as an out or ref parameter during the remote method call.</returns>
	/// <param name="argNum">The zero-based index of the requested out or ref parameter. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public object GetOutArg(int argNum)
	{
		if (_inArgInfo == null)
		{
			_inArgInfo = new ArgInfo(MethodBase, ArgInfoType.Out);
		}
		return _args[_inArgInfo.GetInOutArgIndex(argNum)];
	}

	/// <summary>Returns the name of a specified out or ref parameter passed to the remote method.</summary>
	/// <returns>A string representing the name of the specified out or ref parameter, or null if the current method is not implemented.</returns>
	/// <param name="index">The zero-based index of the requested argument. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public string GetOutArgName(int index)
	{
		if (_inArgInfo == null)
		{
			_inArgInfo = new ArgInfo(MethodBase, ArgInfoType.Out);
		}
		return _inArgInfo.GetInOutArgName(index);
	}

	bool IInternalMessage.HasProperties()
	{
		return _properties != null;
	}

	internal bool HasProperties()
	{
		return _properties != null;
	}
}
