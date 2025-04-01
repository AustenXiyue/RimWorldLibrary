using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Channels;
using Mono.Xml;

namespace System.Runtime.Remoting;

/// <summary>Provides various static methods for configuring the remoting infrastructure.</summary>
[ComVisible(true)]
public static class RemotingConfiguration
{
	private static string applicationID = null;

	private static string applicationName = null;

	private static string processGuid = null;

	private static bool defaultConfigRead = false;

	private static bool defaultDelayedConfigRead = false;

	private static string _errorMode;

	private static Hashtable wellKnownClientEntries = new Hashtable();

	private static Hashtable activatedClientEntries = new Hashtable();

	private static Hashtable wellKnownServiceEntries = new Hashtable();

	private static Hashtable activatedServiceEntries = new Hashtable();

	private static Hashtable channelTemplates = new Hashtable();

	private static Hashtable clientProviderTemplates = new Hashtable();

	private static Hashtable serverProviderTemplates = new Hashtable();

	/// <summary>Gets the ID of the currently executing application.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the ID of the currently executing application.</returns>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static string ApplicationId
	{
		get
		{
			applicationID = ApplicationName;
			return applicationID;
		}
	}

	/// <summary>Gets or sets the name of a remoting application.</summary>
	/// <returns>The name of a remoting application.</returns>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. This exception is thrown only when setting the property value. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static string ApplicationName
	{
		get
		{
			return applicationName;
		}
		set
		{
			applicationName = value;
		}
	}

	/// <summary>Gets or sets value that indicates how custom errors are handled.</summary>
	/// <returns>A member of the <see cref="T:System.Runtime.Remoting.CustomErrorsModes" /> enumeration that indicates how custom errors are handled.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	[MonoTODO]
	public static CustomErrorsModes CustomErrorsMode
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the ID of the currently executing process.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the ID of the currently executing process.</returns>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static string ProcessId
	{
		get
		{
			if (processGuid == null)
			{
				processGuid = AppDomain.GetProcessGuid();
			}
			return processGuid;
		}
	}

	/// <summary>Reads the configuration file and configures the remoting infrastructure.</summary>
	/// <param name="filename">The name of the remoting configuration file. Can be null.</param>
	/// <param name="ensureSecurity">If set to true security is required. If set to false, security is not required but still may be used.</param>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	[MonoTODO("ensureSecurity support has not been implemented")]
	public static void Configure(string filename, bool ensureSecurity)
	{
		lock (channelTemplates)
		{
			if (!defaultConfigRead)
			{
				string bundledMachineConfig = Environment.GetBundledMachineConfig();
				if (bundledMachineConfig != null)
				{
					ReadConfigString(bundledMachineConfig);
				}
				if (File.Exists(Environment.GetMachineConfigPath()))
				{
					ReadConfigFile(Environment.GetMachineConfigPath());
				}
				defaultConfigRead = true;
			}
			if (filename != null)
			{
				ReadConfigFile(filename);
			}
		}
	}

	/// <summary>Reads the configuration file and configures the remoting infrastructure. <see cref="M:System.Runtime.Remoting.RemotingConfiguration.Configure(System.String)" /> is obsolete. Please use <see cref="M:System.Runtime.Remoting.RemotingConfiguration.Configure(System.String,System.Boolean)" /> instead.</summary>
	/// <param name="filename">The name of the remoting configuration file. Can be null. </param>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	[Obsolete("Use Configure(String,Boolean)")]
	public static void Configure(string filename)
	{
		Configure(filename, ensureSecurity: false);
	}

	private static void ReadConfigString(string filename)
	{
		try
		{
			SmallXmlParser smallXmlParser = new SmallXmlParser();
			using TextReader input = new StringReader(filename);
			ConfigHandler handler = new ConfigHandler(onlyDelayedChannels: false);
			smallXmlParser.Parse(input, handler);
		}
		catch (Exception ex)
		{
			throw new RemotingException("Configuration string could not be loaded: " + ex.Message, ex);
		}
	}

	private static void ReadConfigFile(string filename)
	{
		try
		{
			SmallXmlParser smallXmlParser = new SmallXmlParser();
			using TextReader input = new StreamReader(filename);
			ConfigHandler handler = new ConfigHandler(onlyDelayedChannels: false);
			smallXmlParser.Parse(input, handler);
		}
		catch (Exception ex)
		{
			throw new RemotingException("Configuration file '" + filename + "' could not be loaded: " + ex.Message, ex);
		}
	}

	internal static void LoadDefaultDelayedChannels()
	{
		lock (channelTemplates)
		{
			if (!defaultDelayedConfigRead && !defaultConfigRead)
			{
				SmallXmlParser smallXmlParser = new SmallXmlParser();
				using (TextReader input = new StreamReader(Environment.GetMachineConfigPath()))
				{
					ConfigHandler handler = new ConfigHandler(onlyDelayedChannels: true);
					smallXmlParser.Parse(input, handler);
				}
				defaultDelayedConfigRead = true;
			}
		}
	}

	/// <summary>Retrieves an array of object types registered on the client as types that will be activated remotely.</summary>
	/// <returns>An array of object types registered on the client as types that will be activated remotely.</returns>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static ActivatedClientTypeEntry[] GetRegisteredActivatedClientTypes()
	{
		lock (channelTemplates)
		{
			ActivatedClientTypeEntry[] array = new ActivatedClientTypeEntry[activatedClientEntries.Count];
			activatedClientEntries.Values.CopyTo(array, 0);
			return array;
		}
	}

	/// <summary>Retrieves an array of object types registered on the service end that can be activated on request from a client.</summary>
	/// <returns>An array of object types registered on the service end that can be activated on request from a client.</returns>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static ActivatedServiceTypeEntry[] GetRegisteredActivatedServiceTypes()
	{
		lock (channelTemplates)
		{
			ActivatedServiceTypeEntry[] array = new ActivatedServiceTypeEntry[activatedServiceEntries.Count];
			activatedServiceEntries.Values.CopyTo(array, 0);
			return array;
		}
	}

	/// <summary>Retrieves an array of object types registered on the client end as well-known types.</summary>
	/// <returns>An array of object types registered on the client end as well-known types.</returns>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static WellKnownClientTypeEntry[] GetRegisteredWellKnownClientTypes()
	{
		lock (channelTemplates)
		{
			WellKnownClientTypeEntry[] array = new WellKnownClientTypeEntry[wellKnownClientEntries.Count];
			wellKnownClientEntries.Values.CopyTo(array, 0);
			return array;
		}
	}

	/// <summary>Retrieves an array of object types registered on the service end as well-known types.</summary>
	/// <returns>An array of object types registered on the service end as well-known types.</returns>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static WellKnownServiceTypeEntry[] GetRegisteredWellKnownServiceTypes()
	{
		lock (channelTemplates)
		{
			WellKnownServiceTypeEntry[] array = new WellKnownServiceTypeEntry[wellKnownServiceEntries.Count];
			wellKnownServiceEntries.Values.CopyTo(array, 0);
			return array;
		}
	}

	/// <summary>Returns a Boolean value that indicates whether the specified <see cref="T:System.Type" /> is allowed to be client activated.</summary>
	/// <returns>true if the specified <see cref="T:System.Type" /> is allowed to be client activated; otherwise, false.</returns>
	/// <param name="svrType">The object <see cref="T:System.Type" /> to check. </param>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static bool IsActivationAllowed(Type svrType)
	{
		lock (channelTemplates)
		{
			return activatedServiceEntries.ContainsKey(svrType);
		}
	}

	/// <summary>Checks whether the specified object <see cref="T:System.Type" /> is registered as a remotely activated client type.</summary>
	/// <returns>The <see cref="T:System.Runtime.Remoting.ActivatedClientTypeEntry" /> that corresponds to the specified object type.</returns>
	/// <param name="svrType">The object type to check. </param>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static ActivatedClientTypeEntry IsRemotelyActivatedClientType(Type svrType)
	{
		lock (channelTemplates)
		{
			return activatedClientEntries[svrType] as ActivatedClientTypeEntry;
		}
	}

	/// <summary>Checks whether the object specified by its type name and assembly name is registered as a remotely activated client type.</summary>
	/// <returns>The <see cref="T:System.Runtime.Remoting.ActivatedClientTypeEntry" /> that corresponds to the specified object type.</returns>
	/// <param name="typeName">The type name of the object to check. </param>
	/// <param name="assemblyName">The assembly name of the object to check. </param>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static ActivatedClientTypeEntry IsRemotelyActivatedClientType(string typeName, string assemblyName)
	{
		return IsRemotelyActivatedClientType(Assembly.Load(assemblyName).GetType(typeName));
	}

	/// <summary>Checks whether the specified object <see cref="T:System.Type" /> is registered as a well-known client type.</summary>
	/// <returns>The <see cref="T:System.Runtime.Remoting.WellKnownClientTypeEntry" /> that corresponds to the specified object type.</returns>
	/// <param name="svrType">The object <see cref="T:System.Type" /> to check. </param>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static WellKnownClientTypeEntry IsWellKnownClientType(Type svrType)
	{
		lock (channelTemplates)
		{
			return wellKnownClientEntries[svrType] as WellKnownClientTypeEntry;
		}
	}

	/// <summary>Checks whether the object specified by its type name and assembly name is registered as a well-known client type.</summary>
	/// <returns>The <see cref="T:System.Runtime.Remoting.WellKnownClientTypeEntry" /> that corresponds to the specified object type.</returns>
	/// <param name="typeName">The type name of the object to check. </param>
	/// <param name="assemblyName">The assembly name of the object to check. </param>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static WellKnownClientTypeEntry IsWellKnownClientType(string typeName, string assemblyName)
	{
		return IsWellKnownClientType(Assembly.Load(assemblyName).GetType(typeName));
	}

	/// <summary>Registers an object <see cref="T:System.Type" /> recorded in the provided <see cref="T:System.Runtime.Remoting.ActivatedClientTypeEntry" /> on the client end as a type that can be activated on the server.</summary>
	/// <param name="entry">Configuration settings for the client-activated type. </param>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static void RegisterActivatedClientType(ActivatedClientTypeEntry entry)
	{
		lock (channelTemplates)
		{
			if (wellKnownClientEntries.ContainsKey(entry.ObjectType) || activatedClientEntries.ContainsKey(entry.ObjectType))
			{
				throw new RemotingException("Attempt to redirect activation of type '" + entry.ObjectType.FullName + "' which is already redirected.");
			}
			activatedClientEntries[entry.ObjectType] = entry;
			ActivationServices.EnableProxyActivation(entry.ObjectType, enable: true);
		}
	}

	/// <summary>Registers an object <see cref="T:System.Type" /> on the client end as a type that can be activated on the server, using the given parameters to initialize a new instance of the <see cref="T:System.Runtime.Remoting.ActivatedClientTypeEntry" /> class.</summary>
	/// <param name="type">The object <see cref="T:System.Type" />. </param>
	/// <param name="appUrl">URL of the application where this type is activated. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="typeName" /> or <paramref name="URI" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static void RegisterActivatedClientType(Type type, string appUrl)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (appUrl == null)
		{
			throw new ArgumentNullException("appUrl");
		}
		RegisterActivatedClientType(new ActivatedClientTypeEntry(type, appUrl));
	}

	/// <summary>Registers an object type recorded in the provided <see cref="T:System.Runtime.Remoting.ActivatedServiceTypeEntry" /> on the service end as one that can be activated on request from a client.</summary>
	/// <param name="entry">Configuration settings for the client-activated type. </param>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static void RegisterActivatedServiceType(ActivatedServiceTypeEntry entry)
	{
		lock (channelTemplates)
		{
			activatedServiceEntries.Add(entry.ObjectType, entry);
		}
	}

	/// <summary>Registers a specified object type on the service end as a type that can be activated on request from a client.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> of object to register. </param>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static void RegisterActivatedServiceType(Type type)
	{
		RegisterActivatedServiceType(new ActivatedServiceTypeEntry(type));
	}

	/// <summary>Registers an object <see cref="T:System.Type" /> on the client end as a well-known type that can be activated on the server, using the given parameters to initialize a new instance of the <see cref="T:System.Runtime.Remoting.WellKnownClientTypeEntry" /> class.</summary>
	/// <param name="type">The object <see cref="T:System.Type" />. </param>
	/// <param name="objectUrl">URL of a well-known client object. </param>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static void RegisterWellKnownClientType(Type type, string objectUrl)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (objectUrl == null)
		{
			throw new ArgumentNullException("objectUrl");
		}
		RegisterWellKnownClientType(new WellKnownClientTypeEntry(type, objectUrl));
	}

	/// <summary>Registers an object <see cref="T:System.Type" /> recorded in the provided <see cref="T:System.Runtime.Remoting.WellKnownClientTypeEntry" /> on the client end as a well-known type that can be activated on the server.</summary>
	/// <param name="entry">Configuration settings for the well-known type. </param>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static void RegisterWellKnownClientType(WellKnownClientTypeEntry entry)
	{
		lock (channelTemplates)
		{
			if (wellKnownClientEntries.ContainsKey(entry.ObjectType) || activatedClientEntries.ContainsKey(entry.ObjectType))
			{
				throw new RemotingException("Attempt to redirect activation of type '" + entry.ObjectType.FullName + "' which is already redirected.");
			}
			wellKnownClientEntries[entry.ObjectType] = entry;
			ActivationServices.EnableProxyActivation(entry.ObjectType, enable: true);
		}
	}

	/// <summary>Registers an object <see cref="T:System.Type" /> on the service end as a well-known type, using the given parameters to initialize a new instance of <see cref="T:System.Runtime.Remoting.WellKnownServiceTypeEntry" />.</summary>
	/// <param name="type">The object <see cref="T:System.Type" />. </param>
	/// <param name="objectUri">The object URI. </param>
	/// <param name="mode">The activation mode of the well-known object type being registered. (See <see cref="T:System.Runtime.Remoting.WellKnownObjectMode" />.) </param>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static void RegisterWellKnownServiceType(Type type, string objectUri, WellKnownObjectMode mode)
	{
		RegisterWellKnownServiceType(new WellKnownServiceTypeEntry(type, objectUri, mode));
	}

	/// <summary>Registers an object <see cref="T:System.Type" /> recorded in the provided <see cref="T:System.Runtime.Remoting.WellKnownServiceTypeEntry" /> on the service end as a well-known type.</summary>
	/// <param name="entry">Configuration settings for the well-known type. </param>
	/// <exception cref="T:System.Security.SecurityException">At least one of the callers higher in the callstack does not have permission to configure remoting types and channels. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	public static void RegisterWellKnownServiceType(WellKnownServiceTypeEntry entry)
	{
		lock (channelTemplates)
		{
			wellKnownServiceEntries[entry.ObjectUri] = entry;
			RemotingServices.CreateWellKnownServerIdentity(entry.ObjectType, entry.ObjectUri, entry.Mode);
		}
	}

	internal static void RegisterChannelTemplate(ChannelData channel)
	{
		channelTemplates[channel.Id] = channel;
	}

	internal static void RegisterClientProviderTemplate(ProviderData prov)
	{
		clientProviderTemplates[prov.Id] = prov;
	}

	internal static void RegisterServerProviderTemplate(ProviderData prov)
	{
		serverProviderTemplates[prov.Id] = prov;
	}

	internal static void RegisterChannels(ArrayList channels, bool onlyDelayed)
	{
		foreach (ChannelData channel in channels)
		{
			if ((onlyDelayed && channel.DelayLoadAsClientChannel != "true") || (defaultDelayedConfigRead && channel.DelayLoadAsClientChannel == "true"))
			{
				continue;
			}
			if (channel.Ref != null)
			{
				ChannelData channelData2 = (ChannelData)channelTemplates[channel.Ref];
				if (channelData2 == null)
				{
					throw new RemotingException("Channel template '" + channel.Ref + "' not found");
				}
				channel.CopyFrom(channelData2);
			}
			foreach (ProviderData serverProvider in channel.ServerProviders)
			{
				if (serverProvider.Ref != null)
				{
					ProviderData providerData2 = (ProviderData)serverProviderTemplates[serverProvider.Ref];
					if (providerData2 == null)
					{
						throw new RemotingException("Provider template '" + serverProvider.Ref + "' not found");
					}
					serverProvider.CopyFrom(providerData2);
				}
			}
			foreach (ProviderData clientProvider in channel.ClientProviders)
			{
				if (clientProvider.Ref != null)
				{
					ProviderData providerData4 = (ProviderData)clientProviderTemplates[clientProvider.Ref];
					if (providerData4 == null)
					{
						throw new RemotingException("Provider template '" + clientProvider.Ref + "' not found");
					}
					clientProvider.CopyFrom(providerData4);
				}
			}
			ChannelServices.RegisterChannelConfig(channel);
		}
	}

	internal static void RegisterTypes(ArrayList types)
	{
		foreach (TypeEntry type in types)
		{
			if (type is ActivatedClientTypeEntry)
			{
				RegisterActivatedClientType((ActivatedClientTypeEntry)type);
			}
			else if (type is ActivatedServiceTypeEntry)
			{
				RegisterActivatedServiceType((ActivatedServiceTypeEntry)type);
			}
			else if (type is WellKnownClientTypeEntry)
			{
				RegisterWellKnownClientType((WellKnownClientTypeEntry)type);
			}
			else if (type is WellKnownServiceTypeEntry)
			{
				RegisterWellKnownServiceType((WellKnownServiceTypeEntry)type);
			}
		}
	}

	/// <summary>Indicates whether the server channels in this application domain return filtered or complete exception information to local or remote callers.</summary>
	/// <returns>true if only filtered exception information is returned to local or remote callers, as specified by the <paramref name="isLocalRequest" /> parameter; false if complete exception information is returned.</returns>
	/// <param name="isLocalRequest">true to specify local callers; false to specify remote callers. </param>
	public static bool CustomErrorsEnabled(bool isLocalRequest)
	{
		if (_errorMode == "off")
		{
			return false;
		}
		if (_errorMode == "on")
		{
			return true;
		}
		return !isLocalRequest;
	}

	internal static void SetCustomErrorsMode(string mode)
	{
		if (mode == null)
		{
			throw new RemotingException("mode attribute is required");
		}
		string text = mode.ToLower();
		if (text != "on" && text != "off" && text != "remoteonly")
		{
			throw new RemotingException("Invalid custom error mode: " + mode);
		}
		_errorMode = text;
	}
}
