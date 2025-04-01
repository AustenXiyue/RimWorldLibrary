using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System.Runtime.Remoting.Messaging;

/// <summary>Selects the remoting surrogate that can be used to serialize an object that derives from a <see cref="T:System.MarshalByRefObject" />.</summary>
[ComVisible(true)]
public class RemotingSurrogateSelector : ISurrogateSelector
{
	private static Type s_cachedTypeObjRef = typeof(ObjRef);

	private static ObjRefSurrogate _objRefSurrogate = new ObjRefSurrogate();

	private static RemotingSurrogate _objRemotingSurrogate = new RemotingSurrogate();

	private object _rootObj;

	private MessageSurrogateFilter _filter;

	private ISurrogateSelector _next;

	/// <summary>Gets or sets the <see cref="T:System.Runtime.Remoting.Messaging.MessageSurrogateFilter" /> delegate for the current instance of the <see cref="T:System.Runtime.Remoting.Messaging.RemotingSurrogateSelector" />.</summary>
	/// <returns>The <see cref="T:System.Runtime.Remoting.Messaging.MessageSurrogateFilter" /> delegate for the current instance of the <see cref="T:System.Runtime.Remoting.Messaging.RemotingSurrogateSelector" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public MessageSurrogateFilter Filter
	{
		get
		{
			return _filter;
		}
		set
		{
			_filter = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Messaging.RemotingSurrogateSelector" /> class.</summary>
	public RemotingSurrogateSelector()
	{
	}

	/// <summary>Adds the specified <see cref="T:System.Runtime.Serialization.ISurrogateSelector" /> to the surrogate selector chain.</summary>
	/// <param name="selector">The next <see cref="T:System.Runtime.Serialization.ISurrogateSelector" /> to examine. </param>
	[SecurityCritical]
	public virtual void ChainSelector(ISurrogateSelector selector)
	{
		if (_next != null)
		{
			selector.ChainSelector(_next);
		}
		_next = selector;
	}

	/// <summary>Returns the next <see cref="T:System.Runtime.Serialization.ISurrogateSelector" /> in the chain of surrogate selectors.</summary>
	/// <returns>The next <see cref="T:System.Runtime.Serialization.ISurrogateSelector" /> in the chain of surrogate selectors.</returns>
	[SecurityCritical]
	public virtual ISurrogateSelector GetNextSelector()
	{
		return _next;
	}

	/// <summary>Returns the object at the root of the object graph.</summary>
	/// <returns>The object at the root of the object graph.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public object GetRootObject()
	{
		return _rootObj;
	}

	/// <summary>Returns the appropriate surrogate for the given type in the given context.</summary>
	/// <returns>The appropriate surrogate for the given type in the given context.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> for which the surrogate is requested. </param>
	/// <param name="context">The source or destination of serialization. </param>
	/// <param name="ssout">When this method returns, contains an <see cref="T:System.Runtime.Serialization.ISurrogateSelector" /> that is appropriate for the specified object type. This parameter is passed uninitialized. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter, Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public virtual ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector ssout)
	{
		if (type.IsMarshalByRef)
		{
			ssout = this;
			return _objRemotingSurrogate;
		}
		if (s_cachedTypeObjRef.IsAssignableFrom(type))
		{
			ssout = this;
			return _objRefSurrogate;
		}
		if (_next != null)
		{
			return _next.GetSurrogate(type, context, out ssout);
		}
		ssout = null;
		return null;
	}

	/// <summary>Sets the object at the root of the object graph.</summary>
	/// <param name="obj">The object at the root of the object graph. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public void SetRootObject(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		_rootObj = obj;
	}

	/// <summary>Sets up the current surrogate selector to use the SOAP format.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[MonoTODO]
	public virtual void UseSoapFormat()
	{
		throw new NotImplementedException();
	}
}
