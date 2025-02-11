using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Serialization;
using System.Security;

namespace System.Runtime.Remoting.Messaging;

/// <summary>Implements the <see cref="T:System.Runtime.Remoting.Activation.IConstructionCallMessage" /> interface to create a request message that constitutes a constructor call on a remote object.</summary>
[Serializable]
[ComVisible(true)]
[CLSCompliant(false)]
public class ConstructionCall : MethodCall, IConstructionCallMessage, IMessage, IMethodCallMessage, IMethodMessage
{
	private IActivator _activator;

	private object[] _activationAttributes;

	private IList _contextProperties;

	private Type _activationType;

	private string _activationTypeName;

	private bool _isContextOk;

	[NonSerialized]
	private RemotingProxy _sourceProxy;

	internal bool IsContextOk
	{
		get
		{
			return _isContextOk;
		}
		set
		{
			_isContextOk = value;
		}
	}

	/// <summary>Gets the type of the remote object to activate. </summary>
	/// <returns>The <see cref="T:System.Type" /> of the remote object to activate.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public Type ActivationType
	{
		[SecurityCritical]
		get
		{
			if (_activationType == null)
			{
				_activationType = Type.GetType(_activationTypeName);
			}
			return _activationType;
		}
	}

	/// <summary>Gets the full type name of the remote object to activate. </summary>
	/// <returns>A <see cref="T:System.String" /> containing the full type name of the remote object to activate.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public string ActivationTypeName
	{
		[SecurityCritical]
		get
		{
			return _activationTypeName;
		}
	}

	/// <summary>Gets or sets the activator that activates the remote object. </summary>
	/// <returns>The <see cref="T:System.Runtime.Remoting.Activation.IActivator" /> that activates the remote object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public IActivator Activator
	{
		[SecurityCritical]
		get
		{
			return _activator;
		}
		[SecurityCritical]
		set
		{
			_activator = value;
		}
	}

	/// <summary>Gets the call site activation attributes for the remote object. </summary>
	/// <returns>An array of type <see cref="T:System.Object" /> containing the call site activation attributes for the remote object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public object[] CallSiteActivationAttributes
	{
		[SecurityCritical]
		get
		{
			return _activationAttributes;
		}
	}

	/// <summary>Gets a list of properties that define the context in which the remote object is to be created. </summary>
	/// <returns>A <see cref="T:System.Collections.IList" /> that contains a list of properties that define the context in which the remote object is to be created.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public IList ContextProperties
	{
		[SecurityCritical]
		get
		{
			if (_contextProperties == null)
			{
				_contextProperties = new ArrayList();
			}
			return _contextProperties;
		}
	}

	/// <summary>Gets an <see cref="T:System.Collections.IDictionary" /> interface that represents a collection of the remoting message's properties. </summary>
	/// <returns>An <see cref="T:System.Collections.IDictionary" /> interface that represents a collection of the remoting message's properties.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public override IDictionary Properties
	{
		[SecurityCritical]
		get
		{
			return base.Properties;
		}
	}

	internal RemotingProxy SourceProxy
	{
		get
		{
			return _sourceProxy;
		}
		set
		{
			_sourceProxy = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Messaging.ConstructionCall" /> class by copying an existing message. </summary>
	/// <param name="m">A remoting message.</param>
	public ConstructionCall(IMessage m)
		: base(m)
	{
		_activationTypeName = base.TypeName;
		_isContextOk = true;
	}

	internal ConstructionCall(Type type)
	{
		_activationType = type;
		_activationTypeName = type.AssemblyQualifiedName;
		_isContextOk = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Messaging.ConstructionCall" /> class from an array of remoting headers. </summary>
	/// <param name="headers">An array of remoting headers that contain key-value pairs. This array is used to initialize <see cref="T:System.Runtime.Remoting.Messaging.ConstructionCall" /> fields for those headers that belong to the namespace "http://schemas.microsoft.com/clr/soap/messageProperties".</param>
	public ConstructionCall(Header[] headers)
		: base(headers)
	{
	}

	internal ConstructionCall(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	internal override void InitDictionary()
	{
		InternalProperties = ((MessageDictionary)(ExternalProperties = new ConstructionCallDictionary(this))).GetInternalProperties();
	}

	internal void SetActivationAttributes(object[] attributes)
	{
		_activationAttributes = attributes;
	}

	internal override void InitMethodProperty(string key, object value)
	{
		switch (key)
		{
		case "__Activator":
			_activator = (IActivator)value;
			break;
		case "__CallSiteActivationAttributes":
			_activationAttributes = (object[])value;
			break;
		case "__ActivationType":
			_activationType = (Type)value;
			break;
		case "__ContextProperties":
			_contextProperties = (IList)value;
			break;
		case "__ActivationTypeName":
			_activationTypeName = (string)value;
			break;
		default:
			base.InitMethodProperty(key, value);
			break;
		}
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		IList list = _contextProperties;
		if (list != null && list.Count == 0)
		{
			list = null;
		}
		info.AddValue("__Activator", _activator);
		info.AddValue("__CallSiteActivationAttributes", _activationAttributes);
		info.AddValue("__ActivationType", null);
		info.AddValue("__ContextProperties", list);
		info.AddValue("__ActivationTypeName", _activationTypeName);
	}
}
