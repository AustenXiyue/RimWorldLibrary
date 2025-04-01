using System.IO;
using System.Security;
using System.Xml;
using Unity;

namespace System.Configuration.Internal;

/// <summary>Delegates all members of the <see cref="T:System.Configuration.Internal.IInternalConfigHost" /> interface to another instance of a host.</summary>
public class DelegatingConfigHost : IInternalConfigHost, IInternalConfigurationBuilderHost
{
	private IInternalConfigHost host;

	/// <summary>Gets or sets the <see cref="T:System.Configuration.Internal.IInternalConfigHost" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.Internal.IInternalConfigHost" /> object.</returns>
	protected IInternalConfigHost Host
	{
		get
		{
			return host;
		}
		set
		{
			host = value;
		}
	}

	/// <summary>Gets a value indicating whether the configuration is remote.</summary>
	/// <returns>true if the configuration is remote; otherwise, false.</returns>
	public virtual bool IsRemote => host.IsRemote;

	/// <summary>Gets a value indicating whether the host configuration supports change notifications.</summary>
	/// <returns>true if the host supports change notifications; otherwise, false. </returns>
	public virtual bool SupportsChangeNotifications => host.SupportsChangeNotifications;

	/// <summary>Gets a value indicating whether the host configuration supports location tags.</summary>
	/// <returns>true if the host supports location tags; otherwise, false.</returns>
	public virtual bool SupportsLocation => host.SupportsLocation;

	/// <summary>Gets a value indicating whether the host configuration has path support.</summary>
	/// <returns>true if the host configuration has path support; otherwise, false.</returns>
	public virtual bool SupportsPath => host.SupportsPath;

	/// <summary>Gets a value indicating whether the host configuration supports refresh.</summary>
	/// <returns>true if the host configuration supports refresh; otherwise, false.</returns>
	public virtual bool SupportsRefresh => host.SupportsRefresh;

	protected IInternalConfigurationBuilderHost ConfigBuilderHost
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.Internal.DelegatingConfigHost" /> class.</summary>
	protected DelegatingConfigHost()
	{
	}

	/// <summary>Creates a new configuration context.</summary>
	/// <returns>A <see cref="T:System.Object" /> representing a new configuration context.</returns>
	/// <param name="configPath">A string representing the path to a configuration file.</param>
	/// <param name="locationSubPath">A string representing a location subpath.</param>
	public virtual object CreateConfigurationContext(string configPath, string locationSubPath)
	{
		return host.CreateConfigurationContext(configPath, locationSubPath);
	}

	/// <summary>Creates a deprecated configuration context.</summary>
	/// <returns>A <see cref="T:System.Object" /> representing a deprecated configuration context.</returns>
	/// <param name="configPath">A string representing the path to a configuration file.</param>
	public virtual object CreateDeprecatedConfigContext(string configPath)
	{
		return host.CreateDeprecatedConfigContext(configPath);
	}

	/// <summary>Decrypts an encrypted configuration section.</summary>
	/// <returns>A string representing a decrypted configuration section.</returns>
	/// <param name="encryptedXml">An encrypted section of a configuration file.</param>
	/// <param name="protectionProvider">A <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> object.</param>
	/// <param name="protectedConfigSection">A <see cref="T:System.Configuration.ProtectedConfigurationSection" /> object.</param>
	public virtual string DecryptSection(string encryptedXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection)
	{
		return host.DecryptSection(encryptedXml, protectionProvider, protectedConfigSection);
	}

	/// <summary>Deletes the <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</summary>
	/// <param name="streamName">The name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</param>
	public virtual void DeleteStream(string streamName)
	{
		host.DeleteStream(streamName);
	}

	/// <summary>Encrypts a section of a configuration object.</summary>
	/// <returns>A string representing an encrypted section of the configuration object.</returns>
	/// <param name="clearTextXml">A section of the configuration that is not encrypted.</param>
	/// <param name="protectionProvider">A <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> object.</param>
	/// <param name="protectedConfigSection">A <see cref="T:System.Configuration.ProtectedConfigurationSection" /> object.</param>
	public virtual string EncryptSection(string clearTextXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection)
	{
		return host.EncryptSection(clearTextXml, protectionProvider, protectedConfigSection);
	}

	/// <summary>Returns a configuration path based on a location subpath.</summary>
	/// <returns>A string representing a configuration path.</returns>
	/// <param name="configPath">A string representing the path to a configuration file.</param>
	/// <param name="locationSubPath">A string representing a location subpath.</param>
	public virtual string GetConfigPathFromLocationSubPath(string configPath, string locationSubPath)
	{
		return host.GetConfigPathFromLocationSubPath(configPath, locationSubPath);
	}

	/// <summary>Returns a <see cref="T:System.Type" /> representing the type of the configuration.</summary>
	/// <returns>A <see cref="T:System.Type" /> representing the type of the configuration.</returns>
	/// <param name="typeName">A string representing the configuration type.</param>
	/// <param name="throwOnError">true if an exception should be thrown if an error is encountered; false if an exception should not be thrown if an error is encountered.</param>
	public virtual Type GetConfigType(string typeName, bool throwOnError)
	{
		return host.GetConfigType(typeName, throwOnError);
	}

	/// <summary>Returns a string representing the type name of the configuration object.</summary>
	/// <returns>A string representing the type name of the configuration object.</returns>
	/// <param name="t">A <see cref="T:System.Type" /> object.</param>
	public virtual string GetConfigTypeName(Type t)
	{
		return host.GetConfigTypeName(t);
	}

	/// <summary>Sets the specified permission set if available within the host object.</summary>
	/// <param name="configRecord">An <see cref="T:System.Configuration.Internal.IInternalConfigRecord" /> object.</param>
	/// <param name="permissionSet">A <see cref="T:System.Security.PermissionSet" /> object.</param>
	/// <param name="isHostReady">true if the host has finished initialization; otherwise, false.</param>
	public virtual void GetRestrictedPermissions(IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady)
	{
		host.GetRestrictedPermissions(configRecord, out permissionSet, out isHostReady);
	}

	/// <summary>Returns the name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</summary>
	/// <returns>A string representing the name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</returns>
	/// <param name="configPath">A string representing the path to a configuration file.</param>
	public virtual string GetStreamName(string configPath)
	{
		return host.GetStreamName(configPath);
	}

	/// <summary>Returns the name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration source.</summary>
	/// <returns>A string representing the name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration source.</returns>
	/// <param name="streamName">The name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</param>
	/// <param name="configSource">A string representing the configuration source.</param>
	public virtual string GetStreamNameForConfigSource(string streamName, string configSource)
	{
		return host.GetStreamNameForConfigSource(streamName, configSource);
	}

	/// <summary>Returns a <see cref="P:System.Diagnostics.FileVersionInfo.FileVersion" /> object representing the version of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</summary>
	/// <returns>A <see cref="P:System.Diagnostics.FileVersionInfo.FileVersion" /> object representing the version of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</returns>
	/// <param name="streamName">The name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</param>
	public virtual object GetStreamVersion(string streamName)
	{
		return host.GetStreamVersion(streamName);
	}

	/// <summary>Instructs the host to impersonate and returns an <see cref="T:System.IDisposable" /> object required internally by the .NET Framework.</summary>
	/// <returns>An <see cref="T:System.IDisposable" /> value.</returns>
	public virtual IDisposable Impersonate()
	{
		return host.Impersonate();
	}

	/// <summary>Initializes the configuration host.</summary>
	/// <param name="configRoot">An <see cref="T:System.Configuration.Internal.IInternalConfigRoot" /> object.</param>
	/// <param name="hostInitParams">A parameter object containing the values used for initializing the configuration host.</param>
	public virtual void Init(IInternalConfigRoot configRoot, params object[] hostInitParams)
	{
		host.Init(configRoot, hostInitParams);
	}

	/// <summary>Initializes the host for configuration.</summary>
	/// <param name="locationSubPath">A string representing a location subpath (passed by reference).</param>
	/// <param name="configPath">A string representing the path to a configuration file.</param>
	/// <param name="locationConfigPath">The location configuration path.</param>
	/// <param name="configRoot">The configuration root element.</param>
	/// <param name="hostInitConfigurationParams">A parameter object representing the parameters used to initialize the host.</param>
	public virtual void InitForConfiguration(ref string locationSubPath, out string configPath, out string locationConfigPath, IInternalConfigRoot configRoot, params object[] hostInitConfigurationParams)
	{
		host.InitForConfiguration(ref locationSubPath, out configPath, out locationConfigPath, configRoot, hostInitConfigurationParams);
	}

	/// <summary>Returns a value indicating whether the configuration is above the application configuration in the configuration hierarchy.</summary>
	/// <returns>true if the configuration is above the application configuration in the configuration hierarchy; otherwise, false.</returns>
	/// <param name="configPath">A string representing the path to a configuration file.</param>
	public virtual bool IsAboveApplication(string configPath)
	{
		return host.IsAboveApplication(configPath);
	}

	/// <summary>Returns a value indicating whether a configuration record is required for the host configuration initialization.</summary>
	/// <returns>true if a configuration record is required for the host configuration initialization; otherwise, false.</returns>
	/// <param name="configPath">A string representing the path to a configuration file.</param>
	public virtual bool IsConfigRecordRequired(string configPath)
	{
		return host.IsConfigRecordRequired(configPath);
	}

	/// <summary>Restricts or allows definitions in the host configuration. </summary>
	/// <returns>true if the grant or restriction of definitions in the host configuration was successful; otherwise, false.</returns>
	/// <param name="configPath">A string representing the path to a configuration file.</param>
	/// <param name="allowDefinition">The <see cref="T:System.Configuration.ConfigurationAllowDefinition" /> object.</param>
	/// <param name="allowExeDefinition">The <see cref="T:System.Configuration.ConfigurationAllowExeDefinition" /> object.</param>
	public virtual bool IsDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition)
	{
		return host.IsDefinitionAllowed(configPath, allowDefinition, allowExeDefinition);
	}

	/// <summary>Returns a value indicating whether the initialization of a configuration object is considered delayed.</summary>
	/// <returns>true if the initialization of a configuration object is considered delayed; otherwise, false.</returns>
	/// <param name="configRecord">The <see cref="T:System.Configuration.Internal.IInternalConfigRecord" /> object.</param>
	public virtual bool IsInitDelayed(IInternalConfigRecord configRecord)
	{
		return host.IsInitDelayed(configRecord);
	}

	/// <summary>Returns a value indicating whether the file path used by a <see cref="T:System.IO.Stream" /> object to read a configuration file is a valid path.</summary>
	/// <returns>true if the path used by a <see cref="T:System.IO.Stream" /> object to read a configuration file is a valid path; otherwise, false.</returns>
	/// <param name="streamName">The name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</param>
	public virtual bool IsFile(string streamName)
	{
		return host.IsFile(streamName);
	}

	/// <summary>Returns a value indicating whether a configuration section requires a fully trusted code access security level and does not allow the <see cref="T:System.Security.AllowPartiallyTrustedCallersAttribute" /> attribute to disable implicit link demands.</summary>
	/// <returns>true if the configuration section requires a fully trusted code access security level and does not allow the <see cref="T:System.Security.AllowPartiallyTrustedCallersAttribute" /> attribute to disable implicit link demands; otherwise, false.</returns>
	/// <param name="configRecord">The <see cref="T:System.Configuration.Internal.IInternalConfigRecord" /> object.</param>
	public virtual bool IsFullTrustSectionWithoutAptcaAllowed(IInternalConfigRecord configRecord)
	{
		return host.IsFullTrustSectionWithoutAptcaAllowed(configRecord);
	}

	/// <summary>Returns a value indicating whether the configuration object supports a location tag.</summary>
	/// <returns>true if the configuration object supports a location tag; otherwise, false.</returns>
	/// <param name="configPath">A string representing the path to a configuration file.</param>
	public virtual bool IsLocationApplicable(string configPath)
	{
		return host.IsLocationApplicable(configPath);
	}

	/// <summary>Returns a value indicating whether a configuration path is to a configuration node whose contents should be treated as a root.</summary>
	/// <returns>true if the configuration path is to a configuration node whose contents should be treated as a root; otherwise, false.</returns>
	/// <param name="configPath">A string representing the path to a configuration file.</param>
	public virtual bool IsSecondaryRoot(string configPath)
	{
		return host.IsSecondaryRoot(configPath);
	}

	/// <summary>Returns a value indicating whether the configuration path is trusted.</summary>
	/// <returns>true if the configuration path is trusted; otherwise, false.</returns>
	/// <param name="configPath">A string representing the path to a configuration file.</param>
	public virtual bool IsTrustedConfigPath(string configPath)
	{
		return host.IsTrustedConfigPath(configPath);
	}

	/// <summary>Opens a <see cref="T:System.IO.Stream" /> object to read a configuration file.</summary>
	/// <returns>Returns the <see cref="T:System.IO.Stream" /> object specified by <paramref name="streamName" />.</returns>
	/// <param name="streamName">The name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</param>
	public virtual Stream OpenStreamForRead(string streamName)
	{
		return host.OpenStreamForRead(streamName);
	}

	/// <summary>Opens a <see cref="T:System.IO.Stream" /> object to read a configuration file.</summary>
	/// <returns>Returns the <see cref="T:System.IO.Stream" /> object specified by <paramref name="streamName" />.</returns>
	/// <param name="streamName">The name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</param>
	/// <param name="assertPermissions">true to assert permissions; otherwise, false.</param>
	public virtual Stream OpenStreamForRead(string streamName, bool assertPermissions)
	{
		return host.OpenStreamForRead(streamName, assertPermissions);
	}

	/// <summary>Opens a <see cref="T:System.IO.Stream" /> object for writing to a configuration file or for writing to a temporary file used to build a configuration file. Allows a <see cref="T:System.IO.Stream" /> object to be designated as a template for copying file attributes.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> object.</returns>
	/// <param name="streamName">The name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</param>
	/// <param name="templateStreamName">The name of a <see cref="T:System.IO.Stream" /> object from which file attributes are to be copied as a template.</param>
	/// <param name="writeContext">The write context of the <see cref="T:System.IO.Stream" /> object (passed by reference).</param>
	public virtual Stream OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext)
	{
		return host.OpenStreamForWrite(streamName, templateStreamName, ref writeContext);
	}

	/// <summary>Opens a <see cref="T:System.IO.Stream" /> object for writing to a configuration file. Allows a <see cref="T:System.IO.Stream" /> object to be designated as a template for copying file attributes.</summary>
	/// <returns>Returns the <see cref="T:System.IO.Stream" /> object specified by the <paramref name="streamName" /> parameter.</returns>
	/// <param name="streamName">The name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</param>
	/// <param name="templateStreamName">The name of a <see cref="T:System.IO.Stream" /> object from which file attributes are to be copied as a template.</param>
	/// <param name="writeContext">The write context of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file (passed by reference).</param>
	/// <param name="assertPermissions">true to assert permissions; otherwise, false.</param>
	public virtual Stream OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext, bool assertPermissions)
	{
		return host.OpenStreamForWrite(streamName, templateStreamName, ref writeContext, assertPermissions);
	}

	/// <summary>Returns a value indicating whether the entire configuration file could be read by a designated <see cref="T:System.IO.Stream" /> object.</summary>
	/// <returns>true if the entire configuration file could be read by the <see cref="T:System.IO.Stream" /> object designated by <paramref name="streamName" />; otherwise, false.</returns>
	/// <param name="configPath">A string representing the path to a configuration file.</param>
	/// <param name="streamName">The name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</param>
	public virtual bool PrefetchAll(string configPath, string streamName)
	{
		return host.PrefetchAll(configPath, streamName);
	}

	/// <summary>Instructs the <see cref="T:System.Configuration.Internal.IInternalConfigHost" /> object to read a designated section of its associated configuration file.</summary>
	/// <returns>true if a section of the configuration file designated by the <paramref name="sectionGroupName" /> and <paramref name="sectionName" /> parameters can be read by a <see cref="T:System.IO.Stream" /> object; otherwise, false.</returns>
	/// <param name="sectionGroupName">A string representing the name of a section group in the configuration file.</param>
	/// <param name="sectionName">A string representing the name of a section in the configuration file.</param>
	public virtual bool PrefetchSection(string sectionGroupName, string sectionName)
	{
		return host.PrefetchSection(sectionGroupName, sectionName);
	}

	/// <summary>Indicates that a new configuration record requires a complete initialization.</summary>
	/// <param name="configRecord">An <see cref="T:System.Configuration.Internal.IInternalConfigRecord" /> object.</param>
	public virtual void RequireCompleteInit(IInternalConfigRecord configRecord)
	{
		host.RequireCompleteInit(configRecord);
	}

	/// <summary>Instructs the host to monitor an associated <see cref="T:System.IO.Stream" /> object for changes in a configuration file.</summary>
	/// <returns>An <see cref="T:System.Object" /> instance containing changed configuration settings.</returns>
	/// <param name="streamName">The name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</param>
	/// <param name="callback">A <see cref="T:System.Configuration.Internal.StreamChangeCallback" /> object to receive the returned data representing the changes in the configuration file.</param>
	public virtual object StartMonitoringStreamForChanges(string streamName, StreamChangeCallback callback)
	{
		return host.StartMonitoringStreamForChanges(streamName, callback);
	}

	/// <summary>Instructs the host object to stop monitoring an associated <see cref="T:System.IO.Stream" /> object for changes in a configuration file.</summary>
	/// <param name="streamName">The name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</param>
	/// <param name="callback">A <see cref="T:System.Configuration.Internal.StreamChangeCallback" /> object.</param>
	public virtual void StopMonitoringStreamForChanges(string streamName, StreamChangeCallback callback)
	{
		host.StopMonitoringStreamForChanges(streamName, callback);
	}

	/// <summary>Verifies that a configuration definition is allowed for a configuration record.</summary>
	/// <param name="configPath">A string representing the path to a configuration file.</param>
	/// <param name="allowDefinition">An <see cref="P:System.Configuration.SectionInformation.AllowDefinition" /> object.</param>
	/// <param name="allowExeDefinition">A <see cref="T:System.Configuration.ConfigurationAllowExeDefinition" /> object</param>
	/// <param name="errorInfo">An <see cref="T:System.Configuration.Internal.IConfigErrorInfo" /> object.</param>
	public virtual void VerifyDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition, IConfigErrorInfo errorInfo)
	{
		host.VerifyDefinitionAllowed(configPath, allowDefinition, allowExeDefinition, errorInfo);
	}

	/// <summary>Indicates that all writing to the configuration file has completed.</summary>
	/// <param name="streamName">The name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</param>
	/// <param name="success">true if writing to the configuration file completed successfully; otherwise, false.</param>
	/// <param name="writeContext">The write context of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	public virtual void WriteCompleted(string streamName, bool success, object writeContext)
	{
		host.WriteCompleted(streamName, success, writeContext);
	}

	/// <summary>Indicates that all writing to the configuration file has completed and specifies whether permissions should be asserted.</summary>
	/// <param name="streamName">The name of a <see cref="T:System.IO.Stream" /> object performing I/O tasks on a configuration file.</param>
	/// <param name="success">true to indicate that writing was completed successfully; otherwise, false.</param>
	/// <param name="writeContext">The write context of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	/// <param name="assertPermissions">true to assert permissions; otherwise, false.</param>
	public virtual void WriteCompleted(string streamName, bool success, object writeContext, bool assertPermissions)
	{
		host.WriteCompleted(streamName, success, writeContext, assertPermissions);
	}

	public virtual ConfigurationSection ProcessConfigurationSection(ConfigurationSection configSection, ConfigurationBuilder builder)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	public virtual XmlNode ProcessRawXml(XmlNode rawXml, ConfigurationBuilder builder)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}
}
