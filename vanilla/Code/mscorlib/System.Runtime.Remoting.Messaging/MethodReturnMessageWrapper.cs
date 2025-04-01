using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Runtime.Remoting.Messaging;

/// <summary>Implements the <see cref="T:System.Runtime.Remoting.Messaging.IMethodReturnMessage" /> interface to create a message that acts as a response to a method call on a remote object.</summary>
[ComVisible(true)]
public class MethodReturnMessageWrapper : InternalMessageWrapper, IMethodReturnMessage, IMethodMessage, IMessage
{
	private class DictionaryWrapper : MethodReturnDictionary
	{
		private IDictionary _wrappedDictionary;

		private static string[] _keys = new string[2] { "__Args", "__Return" };

		public DictionaryWrapper(IMethodReturnMessage message, IDictionary wrappedDictionary)
			: base(message)
		{
			_wrappedDictionary = wrappedDictionary;
			base.MethodKeys = _keys;
		}

		protected override IDictionary AllocInternalProperties()
		{
			return _wrappedDictionary;
		}

		protected override void SetMethodProperty(string key, object value)
		{
			if (key == "__Args")
			{
				((MethodReturnMessageWrapper)_message)._args = (object[])value;
			}
			else if (key == "__Return")
			{
				((MethodReturnMessageWrapper)_message)._return = value;
			}
			else
			{
				base.SetMethodProperty(key, value);
			}
		}

		protected override object GetMethodProperty(string key)
		{
			if (key == "__Args")
			{
				return ((MethodReturnMessageWrapper)_message)._args;
			}
			if (key == "__Return")
			{
				return ((MethodReturnMessageWrapper)_message)._return;
			}
			return base.GetMethodProperty(key);
		}
	}

	private object[] _args;

	private ArgInfo _outArgInfo;

	private DictionaryWrapper _properties;

	private Exception _exception;

	private object _return;

	/// <summary>Gets the number of arguments passed to the method. </summary>
	/// <returns>A <see cref="T:System.Int32" /> that represents the number of arguments passed to a method.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual int ArgCount
	{
		[SecurityCritical]
		get
		{
			return _args.Length;
		}
	}

	/// <summary>Gets an array of arguments passed to the method. </summary>
	/// <returns>An array of type <see cref="T:System.Object" /> that represents the arguments passed to a method.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual object[] Args
	{
		[SecurityCritical]
		get
		{
			return _args;
		}
		set
		{
			_args = value;
		}
	}

	/// <summary>Gets the exception thrown during the method call, or null if the method did not throw an exception. </summary>
	/// <returns>The <see cref="T:System.Exception" /> thrown during the method call, or null if the method did not throw an exception.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual Exception Exception
	{
		[SecurityCritical]
		get
		{
			return _exception;
		}
		set
		{
			_exception = value;
		}
	}

	/// <summary>Gets a flag that indicates whether the method can accept a variable number of arguments. </summary>
	/// <returns>true if the method can accept a variable number of arguments; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual bool HasVarArgs
	{
		[SecurityCritical]
		get
		{
			return ((IMethodReturnMessage)WrappedMessage).HasVarArgs;
		}
	}

	/// <summary>Gets the <see cref="T:System.Runtime.Remoting.Messaging.LogicalCallContext" /> for the current method call. </summary>
	/// <returns>The <see cref="T:System.Runtime.Remoting.Messaging.LogicalCallContext" /> for the current method call.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual LogicalCallContext LogicalCallContext
	{
		[SecurityCritical]
		get
		{
			return ((IMethodReturnMessage)WrappedMessage).LogicalCallContext;
		}
	}

	/// <summary>Gets the <see cref="T:System.Reflection.MethodBase" /> of the called method. </summary>
	/// <returns>The <see cref="T:System.Reflection.MethodBase" /> of the called method.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual MethodBase MethodBase
	{
		[SecurityCritical]
		get
		{
			return ((IMethodReturnMessage)WrappedMessage).MethodBase;
		}
	}

	/// <summary>Gets the name of the invoked method. </summary>
	/// <returns>A <see cref="T:System.String" /> that contains the name of the invoked method.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual string MethodName
	{
		[SecurityCritical]
		get
		{
			return ((IMethodReturnMessage)WrappedMessage).MethodName;
		}
	}

	/// <summary>Gets an object that contains the method signature. </summary>
	/// <returns>A <see cref="T:System.Object" /> that contains the method signature.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual object MethodSignature
	{
		[SecurityCritical]
		get
		{
			return ((IMethodReturnMessage)WrappedMessage).MethodSignature;
		}
	}

	/// <summary>Gets the number of arguments in the method call that are marked as ref parameters or out parameters. </summary>
	/// <returns>A <see cref="T:System.Int32" /> that represents the number of arguments in the method call that are marked as ref parameters or out parameters.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual int OutArgCount
	{
		[SecurityCritical]
		get
		{
			if (_outArgInfo == null)
			{
				return 0;
			}
			return _outArgInfo.GetInOutArgCount();
		}
	}

	/// <summary>Gets an array of arguments in the method call that are marked as ref parameters or out parameters. </summary>
	/// <returns>An array of type <see cref="T:System.Object" /> that represents the arguments in the method call that are marked as ref parameters or out parameters.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual object[] OutArgs
	{
		[SecurityCritical]
		get
		{
			if (_outArgInfo == null)
			{
				return _args;
			}
			return _outArgInfo.GetInOutArgs(_args);
		}
	}

	/// <summary>An <see cref="T:System.Collections.IDictionary" /> interface that represents a collection of the remoting message's properties. </summary>
	/// <returns>An <see cref="T:System.Collections.IDictionary" /> interface that represents a collection of the remoting message's properties.</returns>
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
				_properties = new DictionaryWrapper(this, WrappedMessage.Properties);
			}
			return _properties;
		}
	}

	/// <summary>Gets the return value of the method call. </summary>
	/// <returns>A <see cref="T:System.Object" /> that represents the return value of the method call.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual object ReturnValue
	{
		[SecurityCritical]
		get
		{
			return _return;
		}
		set
		{
			_return = value;
		}
	}

	/// <summary>Gets the full type name of the remote object on which the method call is being made. </summary>
	/// <returns>A <see cref="T:System.String" /> that contains the full type name of the remote object on which the method call is being made.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual string TypeName
	{
		[SecurityCritical]
		get
		{
			return ((IMethodReturnMessage)WrappedMessage).TypeName;
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
			return ((IMethodReturnMessage)WrappedMessage).Uri;
		}
		set
		{
			Properties["__Uri"] = value;
		}
	}

	/// <summary>Wraps an <see cref="T:System.Runtime.Remoting.Messaging.IMethodReturnMessage" /> to create a <see cref="T:System.Runtime.Remoting.Messaging.MethodReturnMessageWrapper" />. </summary>
	/// <param name="msg">A message that acts as an outgoing method call on a remote object.</param>
	public MethodReturnMessageWrapper(IMethodReturnMessage msg)
		: base(msg)
	{
		if (msg.Exception != null)
		{
			_exception = msg.Exception;
			_args = new object[0];
			return;
		}
		_args = msg.Args;
		_return = msg.ReturnValue;
		if (msg.MethodBase != null)
		{
			_outArgInfo = new ArgInfo(msg.MethodBase, ArgInfoType.Out);
		}
	}

	/// <summary>Gets a method argument, as an object, at a specified index. </summary>
	/// <returns>The method argument as an object.</returns>
	/// <param name="argNum">The index of the requested argument.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public virtual object GetArg(int argNum)
	{
		return _args[argNum];
	}

	/// <summary>Gets the name of a method argument at a specified index. </summary>
	/// <returns>The name of the method argument.</returns>
	/// <param name="index">The index of the requested argument.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public virtual string GetArgName(int index)
	{
		return ((IMethodReturnMessage)WrappedMessage).GetArgName(index);
	}

	/// <summary>Returns the specified argument marked as a ref parameter or an out parameter. </summary>
	/// <returns>The specified argument marked as a ref parameter or an out parameter.</returns>
	/// <param name="argNum">The index of the requested argument.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public virtual object GetOutArg(int argNum)
	{
		return _args[_outArgInfo.GetInOutArgIndex(argNum)];
	}

	/// <summary>Returns the name of the specified argument marked as a ref parameter or an out parameter. </summary>
	/// <returns>The argument name, or null if the current method is not implemented.</returns>
	/// <param name="index">The index of the requested argument.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public virtual string GetOutArgName(int index)
	{
		return _outArgInfo.GetInOutArgName(index);
	}
}
