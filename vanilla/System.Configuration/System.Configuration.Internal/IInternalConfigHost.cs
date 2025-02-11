using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Configuration.Internal;

/// <summary>Defines interfaces used by internal .NET structures to initialize application configuration properties.</summary>
[ComVisible(false)]
public interface IInternalConfigHost
{
	/// <summary>Returns a value indicating whether the configuration is remote.</summary>
	/// <returns>true if the configuration is remote; otherwise, false.</returns>
	bool IsRemote { get; }

	/// <summary>Returns a value indicating whether the host configuration supports change notification.</summary>
	/// <returns>true if the configuration supports change notification; otherwise, false.</returns>
	bool SupportsChangeNotifications { get; }

	/// <summary>Returns a value indicating whether the host configuration supports location tags.</summary>
	/// <returns>true if the configuration supports location tags; otherwise, false.</returns>
	bool SupportsLocation { get; }

	/// <summary>Returns a value indicating whether the host configuration supports path tags.</summary>
	/// <returns>true if the configuration supports path tags; otherwise, false.</returns>
	bool SupportsPath { get; }

	/// <summary>Returns a value indicating whether the host configuration supports configuration refresh.</summary>
	/// <returns>true if the configuration supports configuration refresh; otherwise, false.</returns>
	bool SupportsRefresh { get; }

	/// <summary>Creates and returns a context object for a <see cref="T:System.Configuration.ConfigurationElement" /> of an application configuration.</summary>
	/// <returns>A context object for a <see cref="T:System.Configuration.ConfigurationElement" /> object of an application configuration.</returns>
	/// <param name="configPath">A string representing the path of the application configuration file.</param>
	/// <param name="locationSubPath">A string representing a subpath location of the configuration element.</param>
	object CreateConfigurationContext(string configPath, string locationSubPath);

	/// <summary>Creates and returns a deprecated context object of the application configuration.</summary>
	/// <returns>A deprecated context object of the application configuration.</returns>
	/// <param name="configPath">A string representing a path to an application configuration file.</param>
	object CreateDeprecatedConfigContext(string configPath);

	/// <summary>Decrypts an encrypted configuration section and returns it as a string.</summary>
	/// <returns>A decrypted configuration section as a string.</returns>
	/// <param name="encryptedXml">An encrypted XML string representing a configuration section.</param>
	/// <param name="protectionProvider">The <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> object.</param>
	/// <param name="protectedConfigSection">The <see cref="T:System.Configuration.ProtectedConfigurationSection" /> object.</param>
	string DecryptSection(string encryptedXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection);

	/// <summary>Deletes the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the application configuration file.</summary>
	/// <param name="streamName">A string representing the name of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	void DeleteStream(string streamName);

	/// <summary>Encrypts a configuration section and returns it as a string.</summary>
	/// <returns>An encrypted configuration section represented as a string.</returns>
	/// <param name="clearTextXml">An XML string representing a configuration section to encrypt.</param>
	/// <param name="protectionProvider">The <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> object.</param>
	/// <param name="protectedConfigSection">The <see cref="T:System.Configuration.ProtectedConfigurationSection" /> object.</param>
	string EncryptSection(string clearTextXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection);

	/// <summary>Returns the complete path to an application configuration file based on the location subpath.</summary>
	/// <returns>A string representing the complete path to an application configuration file.</returns>
	/// <param name="configPath">A string representing the path of the application configuration file.</param>
	/// <param name="locationSubPath">The subpath location of the configuration file.</param>
	string GetConfigPathFromLocationSubPath(string configPath, string locationSubPath);

	/// <summary>Returns a <see cref="T:System.Type" /> object representing the type of the configuration object.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the type of the configuration object.</returns>
	/// <param name="typeName">The type name</param>
	/// <param name="throwOnError">true to throw an exception if an error occurs; otherwise, false</param>
	Type GetConfigType(string typeName, bool throwOnError);

	/// <summary>Returns a string representing a type name from the <see cref="T:System.Type" /> object representing the type of the configuration.</summary>
	/// <returns>A string representing the type name from a <see cref="T:System.Type" /> object representing the type of the configuration.</returns>
	/// <param name="t">A <see cref="T:System.Type" /> object.</param>
	string GetConfigTypeName(Type t);

	/// <summary>Associates the configuration with a <see cref="T:System.Security.PermissionSet" /> object.</summary>
	/// <param name="configRecord">An <see cref="T:System.Configuration.Internal.IInternalConfigRecord" /> object.</param>
	/// <param name="permissionSet">The <see cref="T:System.Security.PermissionSet" /> object to associate with the configuration.</param>
	/// <param name="isHostReady">true to indicate the configuration host is has completed building associated permissions; otherwise, false.</param>
	void GetRestrictedPermissions(IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady);

	/// <summary>Returns a string representing the configuration file name associated with the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</summary>
	/// <returns>A string representing the configuration file name associated with the <see cref="T:System.IO.Stream" /> I/O tasks on the configuration file.</returns>
	/// <param name="configPath">A string representing the path of the application configuration file.</param>
	string GetStreamName(string configPath);

	/// <summary>Returns a string representing the configuration file name associated with the <see cref="T:System.IO.Stream" /> object performing I/O tasks on a remote configuration file.</summary>
	/// <returns>A string representing the configuration file name associated with the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</returns>
	/// <param name="streamName">A string representing the configuration file name associated with the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	/// <param name="configSource">A string representing a path to a remote configuration file.</param>
	string GetStreamNameForConfigSource(string streamName, string configSource);

	/// <summary>Returns the version of the <see cref="T:System.IO.Stream" /> object associated with configuration file.</summary>
	/// <returns>The version of the <see cref="T:System.IO.Stream" /> object associated with configuration file.</returns>
	/// <param name="streamName">A string representing the name of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	object GetStreamVersion(string streamName);

	/// <summary>Instructs the host to impersonate and returns an <see cref="T:System.IDisposable" /> object required by the internal .NET structure.</summary>
	/// <returns>An <see cref="T:System.IDisposable" /> value.</returns>
	IDisposable Impersonate();

	/// <summary>Initializes a configuration host.</summary>
	/// <param name="configRoot">The configuration root object.</param>
	/// <param name="hostInitParams">The parameter object containing the values used for initializing the configuration host.</param>
	void Init(IInternalConfigRoot configRoot, params object[] hostInitParams);

	/// <summary>Initializes a configuration object.</summary>
	/// <param name="locationSubPath">The subpath location of the configuration file.</param>
	/// <param name="configPath">A string representing the path of the application configuration file.</param>
	/// <param name="locationConfigPath">A string representing the location of a configuration path.</param>
	/// <param name="configRoot">The <see cref="T:System.Configuration.Internal.IInternalConfigRoot" /> object.</param>
	/// <param name="hostInitConfigurationParams">The parameter object containing the values used for initializing the configuration host.</param>
	void InitForConfiguration(ref string locationSubPath, out string configPath, out string locationConfigPath, IInternalConfigRoot configRoot, params object[] hostInitConfigurationParams);

	/// <summary>Returns a value indicating whether the configuration file is located at a higher level in the configuration hierarchy than the application configuration.</summary>
	/// <returns>true the configuration file is located at a higher level in the configuration hierarchy than the application configuration; otherwise, false.</returns>
	/// <param name="configPath">A string representing the path of the application configuration file.</param>
	bool IsAboveApplication(string configPath);

	/// <summary>Returns a value indicating whether a child record is required for a child configuration path.</summary>
	/// <returns>true if child record is required for a child configuration path; otherwise, false.</returns>
	/// <param name="configPath">A string representing the path of the application configuration file.</param>
	bool IsConfigRecordRequired(string configPath);

	/// <summary>Determines if a different <see cref="T:System.Type" /> definition is allowable for an application configuration object.</summary>
	/// <returns>true if a different <see cref="T:System.Type" /> definition is allowable for an application configuration object; otherwise, false.</returns>
	/// <param name="configPath">A string representing the path of the application configuration file.</param>
	/// <param name="allowDefinition">A <see cref="T:System.Configuration.ConfigurationAllowDefinition" /> object.</param>
	/// <param name="allowExeDefinition">A <see cref="T:System.Configuration.ConfigurationAllowExeDefinition" /> object.</param>
	bool IsDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition);

	/// <summary>Returns a value indicating whether the file path used by a <see cref="T:System.IO.Stream" /> object to read a configuration file is a valid path.</summary>
	/// <returns>true if the path used by a <see cref="T:System.IO.Stream" /> object to read a configuration file is a valid path; otherwise, false.</returns>
	/// <param name="streamName">A string representing the name of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	bool IsFile(string streamName);

	/// <summary>Returns a value indicating whether a configuration section requires a fully trusted code access security level and does not allow the <see cref="T:System.Security.AllowPartiallyTrustedCallersAttribute" /> attribute to disable implicit link demands.</summary>
	/// <returns>true if the configuration section requires a fully trusted code access security level and does not allow the <see cref="T:System.Security.AllowPartiallyTrustedCallersAttribute" /> attribute to disable implicit link demands; otherwise, false.</returns>
	/// <param name="configRecord">The <see cref="T:System.Configuration.Internal.IInternalConfigRecord" /> object.</param>
	bool IsFullTrustSectionWithoutAptcaAllowed(IInternalConfigRecord configRecord);

	/// <summary>Returns a value indicating whether the initialization of a configuration object is considered delayed.</summary>
	/// <returns>true if the initialization of a configuration object is considered delayed; otherwise, false.</returns>
	/// <param name="configRecord">The <see cref="T:System.Configuration.Internal.IInternalConfigRecord" /> object.</param>
	bool IsInitDelayed(IInternalConfigRecord configRecord);

	/// <summary>Returns a value indicating whether the configuration object supports a location tag.</summary>
	/// <returns>true if the configuration object supports a location tag; otherwise, false.</returns>
	/// <param name="configPath">A string representing the path of the application configuration file.</param>
	bool IsLocationApplicable(string configPath);

	/// <summary>Returns a value indicating whether a configuration path is to a configuration node whose contents should be treated as a root.</summary>
	/// <returns>true if the configuration path is to a configuration node whose contents should be treated as a root; otherwise, false.</returns>
	/// <param name="configPath">A string representing the path of the application configuration file.</param>
	bool IsSecondaryRoot(string configPath);

	/// <summary>Returns a value indicating whether the configuration path is trusted.</summary>
	/// <returns>true if the configuration path is trusted; otherwise, false.</returns>
	/// <param name="configPath">A string representing the path of the application configuration file.</param>
	bool IsTrustedConfigPath(string configPath);

	/// <summary>Opens a <see cref="T:System.IO.Stream" /> to read a configuration file.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> object.</returns>
	/// <param name="streamName">A string representing the name of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	Stream OpenStreamForRead(string streamName);

	/// <summary>Opens a <see cref="T:System.IO.Stream" /> object to read a configuration file.</summary>
	/// <returns>Returns the <see cref="T:System.IO.Stream" /> object specified by <paramref name="streamName" />.</returns>
	/// <param name="streamName">A string representing the name of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	/// <param name="assertPermissions">true to assert permissions; otherwise, false.</param>
	Stream OpenStreamForRead(string streamName, bool assertPermissions);

	/// <summary>Opens a <see cref="T:System.IO.Stream" /> object for writing to a configuration file or for writing to a temporary file used to build a configuration file. Allows a <see cref="T:System.IO.Stream" /> object to be designated as a template for copying file attributes.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> object.</returns>
	/// <param name="streamName">A string representing the name of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	/// <param name="templateStreamName">The name of a <see cref="T:System.IO.Stream" /> object from which file attributes are to be copied as a template.</param>
	/// <param name="writeContext">The write context of the <see cref="T:System.IO.Stream" /> object.</param>
	Stream OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext);

	/// <summary>Opens a <see cref="T:System.IO.Stream" /> object for writing to a configuration file. Allows a <see cref="T:System.IO.Stream" /> object to be designated as a template for copying file attributes.</summary>
	/// <returns>Returns the <see cref="T:System.IO.Stream" /> object specified by <paramref name="streamName" />.</returns>
	/// <param name="streamName">A string representing the name of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	/// <param name="templateStreamName">The name of a <see cref="T:System.IO.Stream" /> from which file attributes are to be copied as a template.</param>
	/// <param name="writeContext">The write context of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	/// <param name="assertPermissions">true to assert permissions; otherwise, false.</param>
	Stream OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext, bool assertPermissions);

	/// <summary>Returns a value indicating whether the entire configuration file could be read by a designated <see cref="T:System.IO.Stream" /> object.</summary>
	/// <returns>true if the entire configuration file could be read by the <see cref="T:System.IO.Stream" /> object designated by <paramref name="streamName" />; otherwise, false.</returns>
	/// <param name="configPath">A string representing the path of the application configuration file.</param>
	/// <param name="streamName">A string representing the name of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	bool PrefetchAll(string configPath, string streamName);

	/// <summary>Instructs the <see cref="T:System.Configuration.Internal.IInternalConfigHost" /> object to read a designated section of its associated configuration file.</summary>
	/// <returns>true if a section of the configuration file designated by <paramref name="sectionGroupName" /> and <paramref name="sectionName" /> could be read by a <see cref="T:System.IO.Stream" /> object; otherwise, false.</returns>
	/// <param name="sectionGroupName">A string representing the identifying name of a configuration file section group.</param>
	/// <param name="sectionName">A string representing the identifying name of a configuration file section.</param>
	bool PrefetchSection(string sectionGroupName, string sectionName);

	/// <summary>Indicates a new configuration record requires a complete initialization.</summary>
	/// <param name="configRecord">An <see cref="T:System.Configuration.Internal.IInternalConfigRecord" /> object.</param>
	void RequireCompleteInit(IInternalConfigRecord configRecord);

	/// <summary>Instructs the <see cref="T:System.Configuration.Internal.IInternalConfigHost" /> object to monitor an associated <see cref="T:System.IO.Stream" /> object for changes in a configuration file.</summary>
	/// <returns>An <see cref="T:System.Object" /> containing changed configuration settings.</returns>
	/// <param name="streamName">A string representing the name of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	/// <param name="callback">A <see cref="T:System.Configuration.Internal.StreamChangeCallback" /> object to receive the returned data representing the changes in the configuration file.</param>
	object StartMonitoringStreamForChanges(string streamName, StreamChangeCallback callback);

	/// <summary>Instructs the  <see cref="T:System.Configuration.Internal.IInternalConfigHost" /> object to stop monitoring an associated <see cref="T:System.IO.Stream" /> object for changes in a configuration file.</summary>
	/// <param name="streamName">A string representing the name of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	/// <param name="callback">A <see cref="T:System.Configuration.Internal.StreamChangeCallback" /> object.</param>
	void StopMonitoringStreamForChanges(string streamName, StreamChangeCallback callback);

	/// <summary>Verifies that a configuration definition is allowed for a configuration record.</summary>
	/// <param name="configPath">A string representing the path of the application configuration file.</param>
	/// <param name="allowDefinition">A <see cref="P:System.Configuration.SectionInformation.AllowDefinition" /> object.</param>
	/// <param name="allowExeDefinition">A <see cref="T:System.Configuration.ConfigurationAllowExeDefinition" /> object</param>
	/// <param name="errorInfo">An <see cref="T:System.Configuration.Internal.IConfigErrorInfo" /> object.</param>
	void VerifyDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition, IConfigErrorInfo errorInfo);

	/// <summary>Indicates that all writing to the configuration file has completed.</summary>
	/// <param name="streamName">A string representing the name of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	/// <param name="success">true if the write to the configuration file was completed successfully; otherwise, false.</param>
	/// <param name="writeContext">The write context of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	void WriteCompleted(string streamName, bool success, object writeContext);

	/// <summary>Indicates that all writing to the configuration file has completed and specifies whether permissions should be asserted.</summary>
	/// <param name="streamName">A string representing the name of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	/// <param name="success">true to indicate the write was completed successfully; otherwise, false.</param>
	/// <param name="writeContext">The write context of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
	/// <param name="assertPermissions">true to assert permissions; otherwise, false.</param>
	void WriteCompleted(string streamName, bool success, object writeContext, bool assertPermissions);
}
