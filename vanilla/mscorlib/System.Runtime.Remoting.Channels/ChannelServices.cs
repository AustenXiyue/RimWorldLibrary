using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels;

/// <summary>Provides static methods to aid with remoting channel registration, resolution, and URL discovery. This class cannot be inherited.</summary>
[ComVisible(true)]
public sealed class ChannelServices
{
	private static ArrayList registeredChannels = new ArrayList();

	private static ArrayList delayedClientChannels = new ArrayList();

	private static CrossContextChannel _crossContextSink = new CrossContextChannel();

	internal static string CrossContextUrl = "__CrossContext";

	private static IList oldStartModeTypes = new string[2] { "Novell.Zenworks.Zmd.Public.UnixServerChannel", "Novell.Zenworks.Zmd.Public.UnixChannel" };

	internal static CrossContextChannel CrossContextChannel => _crossContextSink;

	/// <summary>Gets a list of currently registered channels.</summary>
	/// <returns>An array of all the currently registered channels.</returns>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static IChannel[] RegisteredChannels
	{
		get
		{
			lock (registeredChannels.SyncRoot)
			{
				List<IChannel> list = new List<IChannel>();
				for (int i = 0; i < registeredChannels.Count; i++)
				{
					IChannel channel = (IChannel)registeredChannels[i];
					if (!(channel is CrossAppDomainChannel))
					{
						list.Add(channel);
					}
				}
				return list.ToArray();
			}
		}
	}

	private ChannelServices()
	{
	}

	internal static IMessageSink CreateClientChannelSinkChain(string url, object remoteChannelData, out string objectUri)
	{
		object[] channelDataArray = (object[])remoteChannelData;
		lock (registeredChannels.SyncRoot)
		{
			foreach (IChannel registeredChannel in registeredChannels)
			{
				if (registeredChannel is IChannelSender sender)
				{
					IMessageSink messageSink = CreateClientChannelSinkChain(sender, url, channelDataArray, out objectUri);
					if (messageSink != null)
					{
						return messageSink;
					}
				}
			}
			RemotingConfiguration.LoadDefaultDelayedChannels();
			foreach (IChannelSender delayedClientChannel in delayedClientChannels)
			{
				IMessageSink messageSink2 = CreateClientChannelSinkChain(delayedClientChannel, url, channelDataArray, out objectUri);
				if (messageSink2 != null)
				{
					delayedClientChannels.Remove(delayedClientChannel);
					RegisterChannel(delayedClientChannel);
					return messageSink2;
				}
			}
		}
		objectUri = null;
		return null;
	}

	internal static IMessageSink CreateClientChannelSinkChain(IChannelSender sender, string url, object[] channelDataArray, out string objectUri)
	{
		objectUri = null;
		if (channelDataArray == null)
		{
			return sender.CreateMessageSink(url, null, out objectUri);
		}
		foreach (object obj in channelDataArray)
		{
			IMessageSink messageSink = ((!(obj is IChannelDataStore)) ? sender.CreateMessageSink(url, obj, out objectUri) : sender.CreateMessageSink(null, obj, out objectUri));
			if (messageSink != null)
			{
				return messageSink;
			}
		}
		return null;
	}

	/// <summary>Creates a channel sink chain for the specified channel.</summary>
	/// <returns>A new channel sink chain for the specified channel.</returns>
	/// <param name="provider">The first provider in the chain of sink providers that will create the channel sink chain. </param>
	/// <param name="channel">The <see cref="T:System.Runtime.Remoting.Channels.IChannelReceiver" /> for which to create the channel sink chain. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static IServerChannelSink CreateServerChannelSinkChain(IServerChannelSinkProvider provider, IChannelReceiver channel)
	{
		IServerChannelSinkProvider serverChannelSinkProvider = provider;
		while (serverChannelSinkProvider.Next != null)
		{
			serverChannelSinkProvider = serverChannelSinkProvider.Next;
		}
		serverChannelSinkProvider.Next = new ServerDispatchSinkProvider();
		return provider.CreateSink(channel);
	}

	/// <summary>Dispatches incoming remote calls.</summary>
	/// <returns>A <see cref="T:System.Runtime.Remoting.Channels.ServerProcessing" /> that gives the status of the server message processing.</returns>
	/// <param name="sinkStack">The stack of server channel sinks that the message already traversed. </param>
	/// <param name="msg">The message to dispatch. </param>
	/// <param name="replyMsg">When this method returns, contains a <see cref="T:System.Runtime.Remoting.Messaging.IMessage" /> that holds the reply from the server to the message that is contained in the <paramref name="msg" /> parameter. This parameter is passed uninitialized. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="msg" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static ServerProcessing DispatchMessage(IServerChannelSinkStack sinkStack, IMessage msg, out IMessage replyMsg)
	{
		if (msg == null)
		{
			throw new ArgumentNullException("msg");
		}
		replyMsg = SyncDispatchMessage(msg);
		if (RemotingServices.IsOneWay(((IMethodMessage)msg).MethodBase))
		{
			return ServerProcessing.OneWay;
		}
		return ServerProcessing.Complete;
	}

	/// <summary>Returns a registered channel with the specified name.</summary>
	/// <returns>An interface to a registered channel, or null if the channel is not registered.</returns>
	/// <param name="name">The channel name. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static IChannel GetChannel(string name)
	{
		lock (registeredChannels.SyncRoot)
		{
			foreach (IChannel registeredChannel in registeredChannels)
			{
				if (registeredChannel.ChannelName == name && !(registeredChannel is CrossAppDomainChannel))
				{
					return registeredChannel;
				}
			}
			return null;
		}
	}

	/// <summary>Returns a <see cref="T:System.Collections.IDictionary" /> of properties for a given proxy.</summary>
	/// <returns>An interface to the dictionary of properties, or null if no properties were found.</returns>
	/// <param name="obj">The proxy to retrieve properties for. </param>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers that is higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static IDictionary GetChannelSinkProperties(object obj)
	{
		if (!RemotingServices.IsTransparentProxy(obj))
		{
			throw new ArgumentException("obj must be a proxy", "obj");
		}
		IMessageSink messageSink = ((ClientIdentity)RemotingServices.GetRealProxy(obj).ObjectIdentity).ChannelSink;
		List<IDictionary> list = new List<IDictionary>();
		while (messageSink != null && !(messageSink is IClientChannelSink))
		{
			messageSink = messageSink.NextSink;
		}
		if (messageSink == null)
		{
			return new Hashtable();
		}
		for (IClientChannelSink clientChannelSink = messageSink as IClientChannelSink; clientChannelSink != null; clientChannelSink = clientChannelSink.NextChannelSink)
		{
			list.Add(clientChannelSink.Properties);
		}
		return new AggregateDictionary(list.ToArray());
	}

	/// <summary>Returns an array of all the URLs that can be used to reach the specified object.</summary>
	/// <returns>An array of strings that contains the URLs that can be used to remotely identify the object, or null if none were found.</returns>
	/// <param name="obj">The object to retrieve the URL array for. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static string[] GetUrlsForObject(MarshalByRefObject obj)
	{
		string objectUri = RemotingServices.GetObjectUri(obj);
		if (objectUri == null)
		{
			return new string[0];
		}
		List<string> list = new List<string>();
		lock (registeredChannels.SyncRoot)
		{
			foreach (object registeredChannel in registeredChannels)
			{
				if (!(registeredChannel is CrossAppDomainChannel) && registeredChannel is IChannelReceiver channelReceiver)
				{
					list.AddRange(channelReceiver.GetUrlsForUri(objectUri));
				}
			}
		}
		return list.ToArray();
	}

	/// <summary>Registers a channel with the channel services. <see cref="M:System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(System.Runtime.Remoting.Channels.IChannel)" /> is obsolete. Please use <see cref="M:System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(System.Runtime.Remoting.Channels.IChannel,System.Boolean)" /> instead.</summary>
	/// <param name="chnl">The channel to register. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="chnl" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">The channel has already been registered. </exception>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	[Obsolete("Use RegisterChannel(IChannel,Boolean)")]
	public static void RegisterChannel(IChannel chnl)
	{
		RegisterChannel(chnl, ensureSecurity: false);
	}

	/// <summary>Registers a channel with the channel services.</summary>
	/// <param name="chnl">The channel to register.</param>
	/// <param name="ensureSecurity">true ensures that security is enabled; otherwise false. Setting the value to false does not effect the security setting on the TCP or IPC channel. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="chnl" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">The channel has already been registered. </exception>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the call stack does not have permission to configure remoting types and channels. </exception>
	/// <exception cref="T:System.NotSupportedException">Not supported in Windows 98 for <see cref="T:System.Runtime.Remoting.Channels.Tcp.TcpServerChannel" /> and on all platforms for <see cref="T:System.Runtime.Remoting.Channels.Http.HttpServerChannel" />. Host the service using Internet Information Services (IIS) if you require a secure HTTP channel.</exception>
	public static void RegisterChannel(IChannel chnl, bool ensureSecurity)
	{
		if (chnl == null)
		{
			throw new ArgumentNullException("chnl");
		}
		if (ensureSecurity)
		{
			((chnl as ISecurableChannel) ?? throw new RemotingException($"Channel {chnl.ChannelName} is not securable while ensureSecurity is specified as true")).IsSecured = true;
		}
		lock (registeredChannels.SyncRoot)
		{
			int num = -1;
			for (int i = 0; i < registeredChannels.Count; i++)
			{
				IChannel channel = (IChannel)registeredChannels[i];
				if (channel.ChannelName == chnl.ChannelName && chnl.ChannelName != "")
				{
					throw new RemotingException("Channel " + channel.ChannelName + " already registered");
				}
				if (channel.ChannelPriority < chnl.ChannelPriority && num == -1)
				{
					num = i;
				}
			}
			if (num != -1)
			{
				registeredChannels.Insert(num, chnl);
			}
			else
			{
				registeredChannels.Add(chnl);
			}
			if (chnl is IChannelReceiver channelReceiver && oldStartModeTypes.Contains(chnl.GetType().ToString()))
			{
				channelReceiver.StartListening(null);
			}
		}
	}

	internal static void RegisterChannelConfig(ChannelData channel)
	{
		IServerChannelSinkProvider serverChannelSinkProvider = null;
		IClientChannelSinkProvider clientChannelSinkProvider = null;
		for (int num = channel.ServerProviders.Count - 1; num >= 0; num--)
		{
			IServerChannelSinkProvider obj = (IServerChannelSinkProvider)CreateProvider(channel.ServerProviders[num] as ProviderData);
			obj.Next = serverChannelSinkProvider;
			serverChannelSinkProvider = obj;
		}
		for (int num2 = channel.ClientProviders.Count - 1; num2 >= 0; num2--)
		{
			IClientChannelSinkProvider obj2 = (IClientChannelSinkProvider)CreateProvider(channel.ClientProviders[num2] as ProviderData);
			obj2.Next = clientChannelSinkProvider;
			clientChannelSinkProvider = obj2;
		}
		Type type = Type.GetType(channel.Type);
		if (type == null)
		{
			throw new RemotingException("Type '" + channel.Type + "' not found");
		}
		bool flag = typeof(IChannelSender).IsAssignableFrom(type);
		bool flag2 = typeof(IChannelReceiver).IsAssignableFrom(type);
		Type[] types;
		object[] parameters;
		if (flag && flag2)
		{
			types = new Type[3]
			{
				typeof(IDictionary),
				typeof(IClientChannelSinkProvider),
				typeof(IServerChannelSinkProvider)
			};
			parameters = new object[3] { channel.CustomProperties, clientChannelSinkProvider, serverChannelSinkProvider };
		}
		else if (flag)
		{
			types = new Type[2]
			{
				typeof(IDictionary),
				typeof(IClientChannelSinkProvider)
			};
			parameters = new object[2] { channel.CustomProperties, clientChannelSinkProvider };
		}
		else
		{
			if (!flag2)
			{
				throw new RemotingException(string.Concat(type, " is not a valid channel type"));
			}
			types = new Type[2]
			{
				typeof(IDictionary),
				typeof(IServerChannelSinkProvider)
			};
			parameters = new object[2] { channel.CustomProperties, serverChannelSinkProvider };
		}
		ConstructorInfo constructor = type.GetConstructor(types);
		if (constructor == null)
		{
			throw new RemotingException(string.Concat(type, " does not have a valid constructor"));
		}
		IChannel channel2;
		try
		{
			channel2 = (IChannel)constructor.Invoke(parameters);
		}
		catch (TargetInvocationException ex)
		{
			throw ex.InnerException;
		}
		lock (registeredChannels.SyncRoot)
		{
			if (channel.DelayLoadAsClientChannel == "true" && !(channel2 is IChannelReceiver))
			{
				delayedClientChannels.Add(channel2);
			}
			else
			{
				RegisterChannel(channel2);
			}
		}
	}

	private static object CreateProvider(ProviderData prov)
	{
		Type type = Type.GetType(prov.Type);
		if (type == null)
		{
			throw new RemotingException("Type '" + prov.Type + "' not found");
		}
		object[] args = new object[2] { prov.CustomProperties, prov.CustomData };
		try
		{
			return Activator.CreateInstance(type, args);
		}
		catch (Exception innerException)
		{
			if (innerException is TargetInvocationException)
			{
				innerException = ((TargetInvocationException)innerException).InnerException;
			}
			throw new RemotingException(string.Concat("An instance of provider '", type, "' could not be created: ", innerException.Message));
		}
	}

	/// <summary>Synchronously dispatches the incoming message to the server-side chain(s) based on the URI embedded in the message.</summary>
	/// <returns>A reply message is returned by the call to the server-side chain.</returns>
	/// <param name="msg">The message to dispatch. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="msg" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static IMessage SyncDispatchMessage(IMessage msg)
	{
		IMessage message = CheckIncomingMessage(msg);
		if (message != null)
		{
			return CheckReturnMessage(msg, message);
		}
		message = _crossContextSink.SyncProcessMessage(msg);
		return CheckReturnMessage(msg, message);
	}

	/// <summary>Asynchronously dispatches the given message to the server-side chain(s) based on the URI embedded in the message.</summary>
	/// <returns>A <see cref="T:System.Runtime.Remoting.Messaging.IMessageCtrl" /> object used to control the asynchronously dispatched message.</returns>
	/// <param name="msg">The message to dispatch. </param>
	/// <param name="replySink">The sink that will process the return message if it is not null. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="msg" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static IMessageCtrl AsyncDispatchMessage(IMessage msg, IMessageSink replySink)
	{
		IMessage message = CheckIncomingMessage(msg);
		if (message != null)
		{
			replySink.SyncProcessMessage(CheckReturnMessage(msg, message));
			return null;
		}
		if (RemotingConfiguration.CustomErrorsEnabled(IsLocalCall(msg)))
		{
			replySink = new ExceptionFilterSink(msg, replySink);
		}
		return _crossContextSink.AsyncProcessMessage(msg, replySink);
	}

	private static ReturnMessage CheckIncomingMessage(IMessage msg)
	{
		IMethodMessage methodMessage = (IMethodMessage)msg;
		if (!(RemotingServices.GetIdentityForUri(methodMessage.Uri) is ServerIdentity ident))
		{
			return new ReturnMessage(new RemotingException("No receiver for uri " + methodMessage.Uri), (IMethodCallMessage)msg);
		}
		RemotingServices.SetMessageTargetIdentity(msg, ident);
		return null;
	}

	internal static IMessage CheckReturnMessage(IMessage callMsg, IMessage retMsg)
	{
		if (retMsg is IMethodReturnMessage { Exception: not null } && RemotingConfiguration.CustomErrorsEnabled(IsLocalCall(callMsg)))
		{
			retMsg = new MethodResponse(new Exception("Server encountered an internal error. For more information, turn off customErrors in the server's .config file."), (IMethodCallMessage)callMsg);
		}
		return retMsg;
	}

	private static bool IsLocalCall(IMessage callMsg)
	{
		return true;
	}

	/// <summary>Unregisters a particular channel from the registered channels list.</summary>
	/// <param name="chnl">The channel to unregister. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="chnl" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The channel is not registered. </exception>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static void UnregisterChannel(IChannel chnl)
	{
		if (chnl == null)
		{
			throw new ArgumentNullException();
		}
		lock (registeredChannels.SyncRoot)
		{
			for (int i = 0; i < registeredChannels.Count; i++)
			{
				if (registeredChannels[i] == chnl)
				{
					registeredChannels.RemoveAt(i);
					if (chnl is IChannelReceiver channelReceiver)
					{
						channelReceiver.StopListening(null);
					}
					return;
				}
			}
			throw new RemotingException("Channel not registered");
		}
	}

	internal static object[] GetCurrentChannelInfo()
	{
		List<object> list = new List<object>();
		lock (registeredChannels.SyncRoot)
		{
			foreach (object registeredChannel in registeredChannels)
			{
				if (registeredChannel is IChannelReceiver { ChannelData: { } channelData })
				{
					list.Add(channelData);
				}
			}
		}
		return list.ToArray();
	}
}
