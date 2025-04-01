using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace System.Runtime.Remoting.Contexts;

/// <summary>Defines an environment for the objects that are resident inside it and for which a policy can be enforced.</summary>
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
public class Context
{
	private int domain_id;

	private int context_id;

	private UIntPtr static_data;

	private UIntPtr data;

	[ContextStatic]
	private static object[] local_slots;

	private static IMessageSink default_server_context_sink;

	private IMessageSink server_context_sink_chain;

	private IMessageSink client_context_sink_chain;

	private List<IContextProperty> context_properties;

	private static int global_count;

	private volatile LocalDataStoreHolder _localDataStore;

	private static LocalDataStoreMgr _localDataStoreMgr = new LocalDataStoreMgr();

	private static DynamicPropertyCollection global_dynamic_properties;

	private DynamicPropertyCollection context_dynamic_properties;

	private ContextCallbackObject callback_object;

	/// <summary>Gets the default context for the current application domain.</summary>
	/// <returns>The default context for the <see cref="T:System.AppDomain" /> namespace.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static Context DefaultContext => AppDomain.InternalGetDefaultContext();

	/// <summary>Gets the context ID for the current context.</summary>
	/// <returns>The context ID for the current context.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual int ContextID => context_id;

	/// <summary>Gets the array of the current context properties.</summary>
	/// <returns>The current context properties array; otherwise, null if the context does not have any properties attributed to it.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual IContextProperty[] ContextProperties
	{
		get
		{
			if (context_properties == null)
			{
				return new IContextProperty[0];
			}
			return context_properties.ToArray();
		}
	}

	internal bool IsDefaultContext => context_id == 0;

	internal bool NeedsContextSink
	{
		get
		{
			if (context_id == 0 && (global_dynamic_properties == null || !global_dynamic_properties.HasProperties))
			{
				if (context_dynamic_properties != null)
				{
					return context_dynamic_properties.HasProperties;
				}
				return false;
			}
			return true;
		}
	}

	internal static bool HasGlobalDynamicSinks
	{
		get
		{
			if (global_dynamic_properties != null)
			{
				return global_dynamic_properties.HasProperties;
			}
			return false;
		}
	}

	internal bool HasDynamicSinks
	{
		get
		{
			if (context_dynamic_properties != null)
			{
				return context_dynamic_properties.HasProperties;
			}
			return false;
		}
	}

	internal bool HasExitSinks
	{
		get
		{
			if (GetClientContextSinkChain() is ClientContextTerminatorSink && !HasDynamicSinks)
			{
				return HasGlobalDynamicSinks;
			}
			return true;
		}
	}

	private LocalDataStore MyLocalStore
	{
		get
		{
			if (_localDataStore == null)
			{
				lock (_localDataStoreMgr)
				{
					if (_localDataStore == null)
					{
						_localDataStore = _localDataStoreMgr.CreateLocalDataStore();
					}
				}
			}
			return _localDataStore.Store;
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void RegisterContext(Context ctx);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void ReleaseContext(Context ctx);

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Contexts.Context" /> class.</summary>
	public Context()
	{
		domain_id = Thread.GetDomainID();
		context_id = Interlocked.Increment(ref global_count);
		RegisterContext(this);
	}

	/// <summary>Cleans up the backing objects for the nondefault contexts.</summary>
	~Context()
	{
		ReleaseContext(this);
	}

	/// <summary>Registers a dynamic property implementing the <see cref="T:System.Runtime.Remoting.Contexts.IDynamicProperty" /> interface with the remoting service.</summary>
	/// <returns>true if the property was successfully registered; otherwise, false.</returns>
	/// <param name="prop">The dynamic property to register. </param>
	/// <param name="obj">The object/proxy for which the <paramref name="property" /> is registered. </param>
	/// <param name="ctx">The context for which the <paramref name="property" /> is registered. </param>
	/// <exception cref="T:System.ArgumentNullException">Either <paramref name="prop" /> or its name is null, or it is not dynamic (it does not implement <see cref="T:System.Runtime.Remoting.Contexts.IDynamicProperty" />). </exception>
	/// <exception cref="T:System.ArgumentException">Both an object as well as a context are specified (both <paramref name="obj" /> and <paramref name="ctx" /> are not null). </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static bool RegisterDynamicProperty(IDynamicProperty prop, ContextBoundObject obj, Context ctx)
	{
		return GetDynamicPropertyCollection(obj, ctx).RegisterDynamicProperty(prop);
	}

	/// <summary>Unregisters a dynamic property implementing the <see cref="T:System.Runtime.Remoting.Contexts.IDynamicProperty" /> interface.</summary>
	/// <returns>true if the object was successfully unregistered; otherwise, false.</returns>
	/// <param name="name">The name of the dynamic property to unregister. </param>
	/// <param name="obj">The object/proxy for which the <paramref name="property" /> is registered. </param>
	/// <param name="ctx">The context for which the <paramref name="property" /> is registered. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">Both an object as well as a context are specified (both <paramref name="obj" /> and <paramref name="ctx" /> are not null). </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static bool UnregisterDynamicProperty(string name, ContextBoundObject obj, Context ctx)
	{
		return GetDynamicPropertyCollection(obj, ctx).UnregisterDynamicProperty(name);
	}

	private static DynamicPropertyCollection GetDynamicPropertyCollection(ContextBoundObject obj, Context ctx)
	{
		if (ctx == null && obj != null)
		{
			if (RemotingServices.IsTransparentProxy(obj))
			{
				return RemotingServices.GetRealProxy(obj).ObjectIdentity.ClientDynamicProperties;
			}
			return obj.ObjectIdentity.ServerDynamicProperties;
		}
		if (ctx != null && obj == null)
		{
			if (ctx.context_dynamic_properties == null)
			{
				ctx.context_dynamic_properties = new DynamicPropertyCollection();
			}
			return ctx.context_dynamic_properties;
		}
		if (ctx == null && obj == null)
		{
			if (global_dynamic_properties == null)
			{
				global_dynamic_properties = new DynamicPropertyCollection();
			}
			return global_dynamic_properties;
		}
		throw new ArgumentException("Either obj or ctx must be null");
	}

	internal static void NotifyGlobalDynamicSinks(bool start, IMessage req_msg, bool client_site, bool async)
	{
		if (global_dynamic_properties != null && global_dynamic_properties.HasProperties)
		{
			global_dynamic_properties.NotifyMessage(start, req_msg, client_site, async);
		}
	}

	internal void NotifyDynamicSinks(bool start, IMessage req_msg, bool client_site, bool async)
	{
		if (context_dynamic_properties != null && context_dynamic_properties.HasProperties)
		{
			context_dynamic_properties.NotifyMessage(start, req_msg, client_site, async);
		}
	}

	/// <summary>Returns a specific context property, specified by name.</summary>
	/// <returns>The specified context property.</returns>
	/// <param name="name">The name of the property. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual IContextProperty GetProperty(string name)
	{
		if (context_properties == null)
		{
			return null;
		}
		foreach (IContextProperty context_property in context_properties)
		{
			if (context_property.Name == name)
			{
				return context_property;
			}
		}
		return null;
	}

	/// <summary>Sets a specific context property by name.</summary>
	/// <param name="prop">The actual context property. </param>
	/// <exception cref="T:System.InvalidOperationException">There is an attempt to add properties to the default context. </exception>
	/// <exception cref="T:System.InvalidOperationException">The context is frozen. </exception>
	/// <exception cref="T:System.ArgumentNullException">The property or the property name is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual void SetProperty(IContextProperty prop)
	{
		if (prop == null)
		{
			throw new ArgumentNullException("IContextProperty");
		}
		if (this == DefaultContext)
		{
			throw new InvalidOperationException("Can not add properties to default context");
		}
		if (context_properties == null)
		{
			context_properties = new List<IContextProperty>();
		}
		context_properties.Add(prop);
	}

	/// <summary>Freezes the context, making it impossible to add or remove context properties from the current context.</summary>
	/// <exception cref="T:System.InvalidOperationException">The context is already frozen. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual void Freeze()
	{
		if (context_properties == null)
		{
			return;
		}
		foreach (IContextProperty context_property in context_properties)
		{
			context_property.Freeze(this);
		}
	}

	/// <summary>Returns a <see cref="T:System.String" /> class representation of the current context.</summary>
	/// <returns>A <see cref="T:System.String" /> class representation of the current context.</returns>
	public override string ToString()
	{
		return "ContextID: " + context_id;
	}

	internal IMessageSink GetServerContextSinkChain()
	{
		if (server_context_sink_chain == null)
		{
			if (default_server_context_sink == null)
			{
				default_server_context_sink = new ServerContextTerminatorSink();
			}
			server_context_sink_chain = default_server_context_sink;
			if (context_properties != null)
			{
				for (int num = context_properties.Count - 1; num >= 0; num--)
				{
					if (context_properties[num] is IContributeServerContextSink contributeServerContextSink)
					{
						server_context_sink_chain = contributeServerContextSink.GetServerContextSink(server_context_sink_chain);
					}
				}
			}
		}
		return server_context_sink_chain;
	}

	internal IMessageSink GetClientContextSinkChain()
	{
		if (client_context_sink_chain == null)
		{
			client_context_sink_chain = new ClientContextTerminatorSink(this);
			if (context_properties != null)
			{
				foreach (IContextProperty context_property in context_properties)
				{
					if (context_property is IContributeClientContextSink contributeClientContextSink)
					{
						client_context_sink_chain = contributeClientContextSink.GetClientContextSink(client_context_sink_chain);
					}
				}
			}
		}
		return client_context_sink_chain;
	}

	internal IMessageSink CreateServerObjectSinkChain(MarshalByRefObject obj, bool forceInternalExecute)
	{
		IMessageSink nextSink = new StackBuilderSink(obj, forceInternalExecute);
		nextSink = new ServerObjectTerminatorSink(nextSink);
		nextSink = new LeaseSink(nextSink);
		if (context_properties != null)
		{
			for (int num = context_properties.Count - 1; num >= 0; num--)
			{
				if (context_properties[num] is IContributeObjectSink contributeObjectSink)
				{
					nextSink = contributeObjectSink.GetObjectSink(obj, nextSink);
				}
			}
		}
		return nextSink;
	}

	internal IMessageSink CreateEnvoySink(MarshalByRefObject serverObject)
	{
		IMessageSink messageSink = EnvoyTerminatorSink.Instance;
		if (context_properties != null)
		{
			foreach (IContextProperty context_property in context_properties)
			{
				if (context_property is IContributeEnvoySink contributeEnvoySink)
				{
					messageSink = contributeEnvoySink.GetEnvoySink(serverObject, messageSink);
				}
			}
		}
		return messageSink;
	}

	internal static Context SwitchToContext(Context newContext)
	{
		return AppDomain.InternalSetContext(newContext);
	}

	internal static Context CreateNewContext(IConstructionCallMessage msg)
	{
		Context context = new Context();
		foreach (IContextProperty contextProperty in msg.ContextProperties)
		{
			if (context.GetProperty(contextProperty.Name) == null)
			{
				context.SetProperty(contextProperty);
			}
		}
		context.Freeze();
		foreach (IContextProperty contextProperty2 in msg.ContextProperties)
		{
			if (!contextProperty2.IsNewContextOK(context))
			{
				throw new RemotingException("A context property did not approve the candidate context for activating the object");
			}
		}
		return context;
	}

	/// <summary>Executes code in another context.</summary>
	/// <param name="deleg">The delegate used to request the callback. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public void DoCallBack(CrossContextDelegate deleg)
	{
		lock (this)
		{
			if (callback_object == null)
			{
				Context newContext = SwitchToContext(this);
				callback_object = new ContextCallbackObject();
				SwitchToContext(newContext);
			}
		}
		callback_object.DoCallBack(deleg);
	}

	/// <summary>Allocates an unnamed data slot.</summary>
	/// <returns>A local data slot.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static LocalDataStoreSlot AllocateDataSlot()
	{
		return _localDataStoreMgr.AllocateDataSlot();
	}

	/// <summary>Allocates a named data slot.</summary>
	/// <returns>A local data slot object.</returns>
	/// <param name="name">The required name for the data slot. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static LocalDataStoreSlot AllocateNamedDataSlot(string name)
	{
		return _localDataStoreMgr.AllocateNamedDataSlot(name);
	}

	/// <summary>Frees a named data slot on all the contexts.</summary>
	/// <param name="name">The name of the data slot to free. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static void FreeNamedDataSlot(string name)
	{
		_localDataStoreMgr.FreeNamedDataSlot(name);
	}

	/// <summary>Looks up a named data slot.</summary>
	/// <returns>Returns a local data slot.</returns>
	/// <param name="name">The data slot name. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static LocalDataStoreSlot GetNamedDataSlot(string name)
	{
		return _localDataStoreMgr.GetNamedDataSlot(name);
	}

	/// <summary>Retrieves the value from the specified slot on the current context.</summary>
	/// <returns>Returns the data associated with <paramref name="slot" />. </returns>
	/// <param name="slot">The data slot that contains the data. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static object GetData(LocalDataStoreSlot slot)
	{
		return Thread.CurrentContext.MyLocalStore.GetData(slot);
	}

	/// <summary>Sets the data in the specified slot on the current context.</summary>
	/// <param name="slot">The data slot where the data is to be added. </param>
	/// <param name="data">The data that is to be added. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static void SetData(LocalDataStoreSlot slot, object data)
	{
		Thread.CurrentContext.MyLocalStore.SetData(slot, data);
	}
}
