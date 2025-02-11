using System.Collections.Generic;
using System.IO;
using System.Runtime.Hosting;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Policy;
using Unity;

namespace System;

/// <summary>Represents assembly binding information that can be added to an instance of <see cref="T:System.AppDomain" />.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
[ClassInterface(ClassInterfaceType.None)]
[ComVisible(true)]
public sealed class AppDomainSetup : IAppDomainSetup
{
	private string application_base;

	private string application_name;

	private string cache_path;

	private string configuration_file;

	private string dynamic_base;

	private string license_file;

	private string private_bin_path;

	private string private_bin_path_probe;

	private string shadow_copy_directories;

	private string shadow_copy_files;

	private bool publisher_policy;

	private bool path_changed;

	private LoaderOptimization loader_optimization;

	private bool disallow_binding_redirects;

	private bool disallow_code_downloads;

	private ActivationArguments _activationArguments;

	private AppDomainInitializer domain_initializer;

	[NonSerialized]
	private ApplicationTrust application_trust;

	private string[] domain_initializer_args;

	private bool disallow_appbase_probe;

	private byte[] configuration_bytes;

	private byte[] serialized_non_primitives;

	/// <summary>Gets or sets the name of the directory containing the application.</summary>
	/// <returns>The name of the application base directory.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string ApplicationBase
	{
		[SecuritySafeCritical]
		get
		{
			return GetAppBase(application_base);
		}
		set
		{
			application_base = value;
		}
	}

	/// <summary>Gets or sets the name of the application.</summary>
	/// <returns>The name of the application.</returns>
	/// <filterpriority>2</filterpriority>
	public string ApplicationName
	{
		get
		{
			return application_name;
		}
		set
		{
			application_name = value;
		}
	}

	/// <summary>Gets or sets the name of an area specific to the application where files are shadow copied. </summary>
	/// <returns>The fully qualified name of the directory path and file name where files are shadow copied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string CachePath
	{
		[SecuritySafeCritical]
		get
		{
			return cache_path;
		}
		set
		{
			cache_path = value;
		}
	}

	/// <summary>Gets or sets the name of the configuration file for an application domain.</summary>
	/// <returns>The name of the configuration file.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string ConfigurationFile
	{
		[SecuritySafeCritical]
		get
		{
			if (configuration_file == null)
			{
				return null;
			}
			if (Path.IsPathRooted(configuration_file))
			{
				return configuration_file;
			}
			if (ApplicationBase == null)
			{
				throw new MemberAccessException("The ApplicationBase must be set before retrieving this property.");
			}
			return Path.Combine(ApplicationBase, configuration_file);
		}
		set
		{
			configuration_file = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the &lt;publisherPolicy&gt; section of the configuration file is applied to an application domain.</summary>
	/// <returns>true if the &lt;publisherPolicy&gt; section of the configuration file for an application domain is ignored; false if the declared publisher policy is honored.</returns>
	/// <filterpriority>2</filterpriority>
	public bool DisallowPublisherPolicy
	{
		get
		{
			return publisher_policy;
		}
		set
		{
			publisher_policy = value;
		}
	}

	/// <summary>Gets or sets the base directory where the directory for dynamically generated files is located.</summary>
	/// <returns>The directory where the <see cref="P:System.AppDomain.DynamicDirectory" /> is located.NoteThe return value of this property is different from the value assigned. See the Remarks section.</returns>
	/// <exception cref="T:System.MemberAccessException">This property cannot be set because the application name on the application domain is null.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string DynamicBase
	{
		[SecuritySafeCritical]
		get
		{
			if (dynamic_base == null)
			{
				return null;
			}
			if (Path.IsPathRooted(dynamic_base))
			{
				return dynamic_base;
			}
			if (ApplicationBase == null)
			{
				throw new MemberAccessException("The ApplicationBase must be set before retrieving this property.");
			}
			return Path.Combine(ApplicationBase, dynamic_base);
		}
		[SecuritySafeCritical]
		set
		{
			if (application_name == null)
			{
				throw new MemberAccessException("ApplicationName must be set before the DynamicBase can be set.");
			}
			dynamic_base = Path.Combine(value, ((uint)application_name.GetHashCode()).ToString("x"));
		}
	}

	/// <summary>Gets or sets the location of the license file associated with this domain.</summary>
	/// <returns>The location and name of the license file.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string LicenseFile
	{
		[SecuritySafeCritical]
		get
		{
			return license_file;
		}
		set
		{
			license_file = value;
		}
	}

	/// <summary>Specifies the optimization policy used to load an executable.</summary>
	/// <returns>An enumerated constant that is used with the <see cref="T:System.LoaderOptimizationAttribute" />.</returns>
	/// <filterpriority>2</filterpriority>
	[MonoLimitation("In Mono this is controlled by the --share-code flag")]
	public LoaderOptimization LoaderOptimization
	{
		get
		{
			return loader_optimization;
		}
		set
		{
			loader_optimization = value;
		}
	}

	/// <summary>Gets or sets the list of directories under the application base directory that are probed for private assemblies.</summary>
	/// <returns>A list of directory names separated by semicolons.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string PrivateBinPath
	{
		[SecuritySafeCritical]
		get
		{
			return private_bin_path;
		}
		set
		{
			private_bin_path = value;
			path_changed = true;
		}
	}

	/// <summary>Gets or sets a string value that includes or excludes <see cref="P:System.AppDomainSetup.ApplicationBase" /> from the search path for the application, and searches only <see cref="P:System.AppDomainSetup.PrivateBinPath" />.</summary>
	/// <returns>A null reference (Nothing in Visual Basic) to include the application base path when searching for assemblies; any non-null string value to exclude the path. The default value is null.</returns>
	/// <filterpriority>2</filterpriority>
	public string PrivateBinPathProbe
	{
		get
		{
			return private_bin_path_probe;
		}
		set
		{
			private_bin_path_probe = value;
			path_changed = true;
		}
	}

	/// <summary>Gets or sets the names of the directories containing assemblies to be shadow copied.</summary>
	/// <returns>A list of directory names separated by semicolons.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string ShadowCopyDirectories
	{
		[SecuritySafeCritical]
		get
		{
			return shadow_copy_directories;
		}
		set
		{
			shadow_copy_directories = value;
		}
	}

	/// <summary>Gets or sets a string that indicates whether shadow copying is turned on or off.</summary>
	/// <returns>The string value "true" to indicate that shadow copying is turned on; or "false" to indicate that shadow copying is turned off.</returns>
	/// <filterpriority>2</filterpriority>
	public string ShadowCopyFiles
	{
		get
		{
			return shadow_copy_files;
		}
		set
		{
			shadow_copy_files = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether an application domain allows assembly binding redirection.</summary>
	/// <returns>true if redirection of assemblies is not allowed; false if it is allowed.</returns>
	/// <filterpriority>2</filterpriority>
	public bool DisallowBindingRedirects
	{
		get
		{
			return disallow_binding_redirects;
		}
		set
		{
			disallow_binding_redirects = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether HTTP download of assemblies is allowed for an application domain.</summary>
	/// <returns>true if HTTP download of assemblies is not allowed; false if it is allowed.</returns>
	/// <filterpriority>2</filterpriority>
	public bool DisallowCodeDownload
	{
		get
		{
			return disallow_code_downloads;
		}
		set
		{
			disallow_code_downloads = value;
		}
	}

	/// <summary>Gets or sets a string that specifies the target version and profile of the .NET Framework for the application domain, in a format that can be parsed by the <see cref="M:System.Runtime.Versioning.FrameworkName.#ctor(System.String)" /> constructor. </summary>
	/// <returns>The target version and profile of the .NET Framework. </returns>
	public string TargetFrameworkName { get; set; }

	/// <summary>Gets or sets data about the activation of an application domain.</summary>
	/// <returns>An object that contains data about the activation of an application domain.</returns>
	/// <exception cref="T:System.InvalidOperationException">The property is set to an <see cref="T:System.Runtime.Hosting.ActivationArguments" /> object whose application identity does not match the application identity of the <see cref="T:System.Security.Policy.ApplicationTrust" /> object returned by the <see cref="P:System.AppDomainSetup.ApplicationTrust" /> property. No exception is thrown if the <see cref="P:System.AppDomainSetup.ApplicationTrust" /> property is null.</exception>
	/// <filterpriority>1</filterpriority>
	public ActivationArguments ActivationArguments
	{
		get
		{
			if (_activationArguments != null)
			{
				return _activationArguments;
			}
			DeserializeNonPrimitives();
			return _activationArguments;
		}
		set
		{
			_activationArguments = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.AppDomainInitializer" /> delegate, which represents a callback method that is invoked when the application domain is initialized.</summary>
	/// <returns>A delegate that represents a callback method that is invoked when the application domain is initialized.</returns>
	/// <filterpriority>2</filterpriority>
	[MonoLimitation("it needs to be invoked within the created domain")]
	public AppDomainInitializer AppDomainInitializer
	{
		get
		{
			if (domain_initializer != null)
			{
				return domain_initializer;
			}
			DeserializeNonPrimitives();
			return domain_initializer;
		}
		set
		{
			domain_initializer = value;
		}
	}

	/// <summary>Gets or sets the arguments passed to the callback method represented by the <see cref="T:System.AppDomainInitializer" /> delegate. The callback method is invoked when the application domain is initialized.</summary>
	/// <returns>An array of strings that is passed to the callback method represented by the <see cref="T:System.AppDomainInitializer" /> delegate, when the callback method is invoked during <see cref="T:System.AppDomain" /> initialization.</returns>
	/// <filterpriority>2</filterpriority>
	[MonoLimitation("it needs to be used to invoke the initializer within the created domain")]
	public string[] AppDomainInitializerArguments
	{
		get
		{
			return domain_initializer_args;
		}
		set
		{
			domain_initializer_args = value;
		}
	}

	/// <summary>Gets or sets an object containing security and trust information.</summary>
	/// <returns>An object that contains security and trust information. </returns>
	/// <exception cref="T:System.InvalidOperationException">The property is set to an <see cref="T:System.Security.Policy.ApplicationTrust" /> object whose application identity does not match the application identity of the <see cref="T:System.Runtime.Hosting.ActivationArguments" /> object returned by the <see cref="P:System.AppDomainSetup.ActivationArguments" /> property. No exception is thrown if the <see cref="P:System.AppDomainSetup.ActivationArguments" /> property is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The property is set to null.</exception>
	[MonoNotSupported("This property exists but not considered.")]
	public ApplicationTrust ApplicationTrust
	{
		get
		{
			if (application_trust != null)
			{
				return application_trust;
			}
			DeserializeNonPrimitives();
			if (application_trust == null)
			{
				application_trust = new ApplicationTrust();
			}
			return application_trust;
		}
		set
		{
			application_trust = value;
		}
	}

	/// <summary>Specifies whether the application base path and private binary path are probed when searching for assemblies to load.</summary>
	/// <returns>true if probing is not allowed; otherwise, false. The default is false.</returns>
	/// <filterpriority>1</filterpriority>
	[MonoNotSupported("This property exists but not considered.")]
	public bool DisallowApplicationBaseProbing
	{
		get
		{
			return disallow_appbase_probe;
		}
		set
		{
			disallow_appbase_probe = value;
		}
	}

	/// <summary>Gets or sets the display name of the assembly that provides the type of the application domain manager for application domains created using this <see cref="T:System.AppDomainSetup" /> object.</summary>
	/// <returns>The display name of the assembly that provides the <see cref="T:System.Type" /> of the application domain manager.</returns>
	public string AppDomainManagerAssembly
	{
		get
		{
			ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets or sets the full name of the type that provides the application domain manager for application domains created using this <see cref="T:System.AppDomainSetup" /> object.</summary>
	/// <returns>The full name of the type, including the namespace.</returns>
	public string AppDomainManagerType
	{
		get
		{
			ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets or sets a list of assemblies marked with the <see cref="F:System.Security.PartialTrustVisibilityLevel.NotVisibleByDefault" /> flag that are made visible to partial-trust code running in a sandboxed application domain. </summary>
	/// <returns>An array of partial assembly names, where each partial name consists of the simple assembly name and the public key.</returns>
	public string[] PartialTrustVisibleAssemblies
	{
		get
		{
			ThrowStub.ThrowNotSupportedException();
			return null;
		}
		set
		{
			ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets or sets a value that indicates whether interface caching is disabled for interop calls in the application domain, so that a QueryInterface is performed on each call.</summary>
	/// <returns>true if interface caching is disabled for interop calls in application domains created with the current <see cref="T:System.AppDomainSetup" /> object; otherwise, false.</returns>
	public bool SandboxInterop
	{
		get
		{
			ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
		set
		{
			ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.AppDomainSetup" /> class.</summary>
	public AppDomainSetup()
	{
	}

	internal AppDomainSetup(AppDomainSetup setup)
	{
		application_base = setup.application_base;
		application_name = setup.application_name;
		cache_path = setup.cache_path;
		configuration_file = setup.configuration_file;
		dynamic_base = setup.dynamic_base;
		license_file = setup.license_file;
		private_bin_path = setup.private_bin_path;
		private_bin_path_probe = setup.private_bin_path_probe;
		shadow_copy_directories = setup.shadow_copy_directories;
		shadow_copy_files = setup.shadow_copy_files;
		publisher_policy = setup.publisher_policy;
		path_changed = setup.path_changed;
		loader_optimization = setup.loader_optimization;
		disallow_binding_redirects = setup.disallow_binding_redirects;
		disallow_code_downloads = setup.disallow_code_downloads;
		_activationArguments = setup._activationArguments;
		domain_initializer = setup.domain_initializer;
		application_trust = setup.application_trust;
		domain_initializer_args = setup.domain_initializer_args;
		disallow_appbase_probe = setup.disallow_appbase_probe;
		configuration_bytes = setup.configuration_bytes;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.AppDomainSetup" /> class with the specified activation arguments required for manifest-based activation of an application domain.</summary>
	/// <param name="activationArguments">An object that specifies information required for the manifest-based activation of a new application domain.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="activationArguments" /> is null.</exception>
	public AppDomainSetup(ActivationArguments activationArguments)
	{
		_activationArguments = activationArguments;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.AppDomainSetup" /> class with the specified activation context to use for manifest-based activation of an application domain.</summary>
	/// <param name="activationContext">The activation context to be used for an application domain.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="activationContext" /> is null.</exception>
	public AppDomainSetup(ActivationContext activationContext)
	{
		_activationArguments = new ActivationArguments(activationContext);
	}

	private static string GetAppBase(string appBase)
	{
		if (appBase == null)
		{
			return null;
		}
		if (appBase.Length >= 8 && appBase.ToLower().StartsWith("file://"))
		{
			appBase = appBase.Substring(7);
			if (Path.DirectorySeparatorChar != '/')
			{
				appBase = appBase.Replace('/', Path.DirectorySeparatorChar);
			}
		}
		appBase = Path.GetFullPath(appBase);
		if (Path.DirectorySeparatorChar != '/')
		{
			bool flag = appBase.StartsWith("\\\\?\\", StringComparison.Ordinal);
			if (appBase.IndexOf(':', flag ? 6 : 2) != -1)
			{
				throw new NotSupportedException("The given path's format is not supported.");
			}
		}
		string directoryName = Path.GetDirectoryName(appBase);
		if (directoryName != null && directoryName.LastIndexOfAny(Path.GetInvalidPathChars()) >= 0)
		{
			throw new ArgumentException(string.Format(Locale.GetText("Invalid path characters in path: '{0}'"), appBase), "appBase");
		}
		string fileName = Path.GetFileName(appBase);
		if (fileName != null && fileName.LastIndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
		{
			throw new ArgumentException(string.Format(Locale.GetText("Invalid filename characters in path: '{0}'"), appBase), "appBase");
		}
		return appBase;
	}

	/// <summary>Returns the XML configuration information set by the <see cref="M:System.AppDomainSetup.SetConfigurationBytes(System.Byte[])" /> method, which overrides the application's XML configuration information.</summary>
	/// <returns>An array that contains the XML configuration information that was set by the <see cref="M:System.AppDomainSetup.SetConfigurationBytes(System.Byte[])" /> method, or null if the <see cref="M:System.AppDomainSetup.SetConfigurationBytes(System.Byte[])" /> method has not been called.</returns>
	/// <filterpriority>1</filterpriority>
	[MonoNotSupported("This method exists but not considered.")]
	public byte[] GetConfigurationBytes()
	{
		if (configuration_bytes == null)
		{
			return null;
		}
		return configuration_bytes.Clone() as byte[];
	}

	/// <summary>Provides XML configuration information for the application domain, replacing the application's XML configuration information.</summary>
	/// <param name="value">An array that contains the XML configuration information to be used for the application domain.</param>
	/// <filterpriority>1</filterpriority>
	[MonoNotSupported("This method exists but not considered.")]
	public void SetConfigurationBytes(byte[] value)
	{
		configuration_bytes = value;
	}

	private void DeserializeNonPrimitives()
	{
		lock (this)
		{
			if (serialized_non_primitives != null)
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				MemoryStream serializationStream = new MemoryStream(serialized_non_primitives);
				object[] array = (object[])binaryFormatter.Deserialize(serializationStream);
				_activationArguments = (ActivationArguments)array[0];
				domain_initializer = (AppDomainInitializer)array[1];
				application_trust = (ApplicationTrust)array[2];
				serialized_non_primitives = null;
			}
		}
	}

	internal void SerializeNonPrimitives()
	{
		object[] graph = new object[3] { _activationArguments, domain_initializer, application_trust };
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, graph);
		serialized_non_primitives = memoryStream.ToArray();
	}

	/// <summary>Sets the specified switches, making the application domain compatible with previous versions of the .NET Framework for the specified issues.</summary>
	/// <param name="switches">An enumerable set of string values that specify compatibility switches, or null to erase the existing compatibility switches.</param>
	[MonoTODO("not implemented, does not throw because it's used in testing moonlight")]
	public void SetCompatibilitySwitches(IEnumerable<string> switches)
	{
	}

	/// <summary>Provides the common language runtime with an alternate implementation of a string comparison function. </summary>
	/// <param name="functionName">The name of the string comparison function to override.</param>
	/// <param name="functionVersion">The function version. For .NET Framework 4.5, its value must be 1 or greater.</param>
	/// <param name="functionPointer">A pointer to the function that overrides <paramref name="functionName" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="functionName" /> is null. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="functionVersion" /> is not 1 or greater.-or-<paramref name="functionPointer" /> is <see cref="F:System.IntPtr.Zero" />. </exception>
	[SecurityCritical]
	public void SetNativeFunction(string functionName, int functionVersion, IntPtr functionPointer)
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
