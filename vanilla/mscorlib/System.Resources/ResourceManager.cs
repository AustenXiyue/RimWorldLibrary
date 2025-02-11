using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading;

namespace System.Resources;

/// <summary>Represents a resource manager that provides convenient access to culture-specific resources at run time.</summary>
[Serializable]
[ComVisible(true)]
public class ResourceManager
{
	internal class CultureNameResourceSetPair
	{
		public string lastCultureName;

		public ResourceSet lastResourceSet;
	}

	internal class ResourceManagerMediator
	{
		private ResourceManager _rm;

		internal string ModuleDir => _rm.moduleDir;

		internal Type LocationInfo => _rm._locationInfo;

		internal Type UserResourceSet => _rm._userResourceSet;

		internal string BaseNameField => _rm.BaseNameField;

		internal CultureInfo NeutralResourcesCulture
		{
			get
			{
				return _rm._neutralResourcesCulture;
			}
			set
			{
				_rm._neutralResourcesCulture = value;
			}
		}

		internal bool LookedForSatelliteContractVersion
		{
			get
			{
				return _rm._lookedForSatelliteContractVersion;
			}
			set
			{
				_rm._lookedForSatelliteContractVersion = value;
			}
		}

		internal Version SatelliteContractVersion
		{
			get
			{
				return _rm._satelliteContractVersion;
			}
			set
			{
				_rm._satelliteContractVersion = value;
			}
		}

		internal UltimateResourceFallbackLocation FallbackLoc
		{
			get
			{
				return _rm.FallbackLocation;
			}
			set
			{
				_rm._fallbackLoc = value;
			}
		}

		internal RuntimeAssembly CallingAssembly => _rm.m_callingAssembly;

		internal RuntimeAssembly MainAssembly => (RuntimeAssembly)_rm.MainAssembly;

		internal string BaseName => _rm.BaseName;

		internal ResourceManagerMediator(ResourceManager rm)
		{
			if (rm == null)
			{
				throw new ArgumentNullException("rm");
			}
			_rm = rm;
		}

		internal string GetResourceFileName(CultureInfo culture)
		{
			return _rm.GetResourceFileName(culture);
		}

		internal Version ObtainSatelliteContractVersion(Assembly a)
		{
			return GetSatelliteContractVersion(a);
		}
	}

	/// <summary>Specifies the root name of the resource files that the <see cref="T:System.Resources.ResourceManager" /> searches for resources.</summary>
	protected string BaseNameField;

	/// <summary>Contains a <see cref="T:System.Collections.Hashtable" /> that returns a mapping from cultures to <see cref="T:System.Resources.ResourceSet" /> objects. </summary>
	[Obsolete("call InternalGetResourceSet instead")]
	protected Hashtable ResourceSets;

	[NonSerialized]
	private Dictionary<string, ResourceSet> _resourceSets;

	private string moduleDir;

	/// <summary>Specifies the main assembly that contains the resources.</summary>
	protected Assembly MainAssembly;

	private Type _locationInfo;

	private Type _userResourceSet;

	private CultureInfo _neutralResourcesCulture;

	[NonSerialized]
	private CultureNameResourceSetPair _lastUsedResourceCache;

	private bool _ignoreCase;

	private bool UseManifest;

	[OptionalField(VersionAdded = 1)]
	private bool UseSatelliteAssem;

	[OptionalField]
	private UltimateResourceFallbackLocation _fallbackLoc;

	[OptionalField]
	private Version _satelliteContractVersion;

	[OptionalField]
	private bool _lookedForSatelliteContractVersion;

	[OptionalField(VersionAdded = 1)]
	private Assembly _callingAssembly;

	[OptionalField(VersionAdded = 4)]
	private RuntimeAssembly m_callingAssembly;

	[NonSerialized]
	private IResourceGroveler resourceGroveler;

	/// <summary>Holds the number used to identify resource files.</summary>
	public static readonly int MagicNumber = -1091581234;

	/// <summary> Specifies the version of resource file headers that the current implementation of <see cref="T:System.Resources.ResourceManager" /> can interpret and produce.</summary>
	public static readonly int HeaderVersionNumber = 1;

	private static readonly Type _minResourceSet = typeof(ResourceSet);

	internal static readonly string ResReaderTypeName = typeof(ResourceReader).FullName;

	internal static readonly string ResSetTypeName = typeof(RuntimeResourceSet).FullName;

	internal static readonly string MscorlibName = typeof(ResourceReader).Assembly.FullName;

	internal const string ResFileExtension = ".resources";

	internal const int ResFileExtensionLength = 10;

	internal static readonly int DEBUG = 0;

	/// <summary>Gets the root name of the resource files that the <see cref="T:System.Resources.ResourceManager" /> searches for resources.</summary>
	/// <returns>The root name of the resource files that the <see cref="T:System.Resources.ResourceManager" /> searches for resources.</returns>
	public virtual string BaseName => BaseNameField;

	/// <summary>Gets or sets a value that indicates whether the resource manager allows case-insensitive resource lookups in the <see cref="M:System.Resources.ResourceManager.GetString(System.String)" /> and <see cref="M:System.Resources.ResourceManager.GetObject(System.String)" /> methods.</summary>
	/// <returns>true to ignore case during resource lookup; otherwise, false.</returns>
	public virtual bool IgnoreCase
	{
		get
		{
			return _ignoreCase;
		}
		set
		{
			_ignoreCase = value;
		}
	}

	/// <summary>Gets the type of the resource set object that the resource manager uses to construct a <see cref="T:System.Resources.ResourceSet" /> object.</summary>
	/// <returns>The type of the resource set object that the resource manager uses to construct a <see cref="T:System.Resources.ResourceSet" /> object.</returns>
	public virtual Type ResourceSetType
	{
		get
		{
			if (!(_userResourceSet == null))
			{
				return _userResourceSet;
			}
			return typeof(RuntimeResourceSet);
		}
	}

	/// <summary>Gets or sets the location from which to retrieve default fallback resources.</summary>
	/// <returns>One of the enumeration values that specifies where the resource manager can look for fallback resources.</returns>
	protected UltimateResourceFallbackLocation FallbackLocation
	{
		get
		{
			return _fallbackLoc;
		}
		set
		{
			_fallbackLoc = value;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Init()
	{
		m_callingAssembly = (RuntimeAssembly)Assembly.GetCallingAssembly();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.ResourceManager" /> class with default values.</summary>
	protected ResourceManager()
	{
		Init();
		_lastUsedResourceCache = new CultureNameResourceSetPair();
		ResourceManagerMediator mediator = new ResourceManagerMediator(this);
		resourceGroveler = new ManifestBasedResourceGroveler(mediator);
	}

	private ResourceManager(string baseName, string resourceDir, Type usingResourceSet)
	{
		if (baseName == null)
		{
			throw new ArgumentNullException("baseName");
		}
		if (resourceDir == null)
		{
			throw new ArgumentNullException("resourceDir");
		}
		BaseNameField = baseName;
		moduleDir = resourceDir;
		_userResourceSet = usingResourceSet;
		ResourceSets = new Hashtable();
		_resourceSets = new Dictionary<string, ResourceSet>();
		_lastUsedResourceCache = new CultureNameResourceSetPair();
		UseManifest = false;
		ResourceManagerMediator mediator = new ResourceManagerMediator(this);
		resourceGroveler = new FileBasedResourceGroveler(mediator);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.ResourceManager" /> class that looks up resources contained in files with the specified root name in the given assembly.</summary>
	/// <param name="baseName">The root name of the resource file without its extension but including any fully qualified namespace name. For example, the root name for the resource file named MyApplication.MyResource.en-US.resources is MyApplication.MyResource. </param>
	/// <param name="assembly">The main assembly for the resources. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="baseName" /> or <paramref name="assembly" /> parameter is null. </exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public ResourceManager(string baseName, Assembly assembly)
	{
		if (baseName == null)
		{
			throw new ArgumentNullException("baseName");
		}
		if (null == assembly)
		{
			throw new ArgumentNullException("assembly");
		}
		if (!(assembly is RuntimeAssembly))
		{
			throw new ArgumentException(Environment.GetResourceString("Assembly must be a runtime Assembly object."));
		}
		MainAssembly = assembly;
		BaseNameField = baseName;
		SetAppXConfiguration();
		CommonAssemblyInit();
		m_callingAssembly = (RuntimeAssembly)Assembly.GetCallingAssembly();
		if (assembly == typeof(object).Assembly && m_callingAssembly != assembly)
		{
			m_callingAssembly = null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.ResourceManager" /> class that uses a specified <see cref="T:System.Resources.ResourceSet" /> class to look up resources contained in files with the specified root name in the given assembly.</summary>
	/// <param name="baseName">The root name of the resource file without its extension but including any fully qualified namespace name. For example, the root name for the resource file named MyApplication.MyResource.en-US.resources is MyApplication.MyResource. </param>
	/// <param name="assembly">The main assembly for the resources. </param>
	/// <param name="usingResourceSet">The type of the custom <see cref="T:System.Resources.ResourceSet" /> to use. If null, the default runtime <see cref="T:System.Resources.ResourceSet" /> object is used. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="usingResourceset" /> is not a derived class of <see cref="T:System.Resources.ResourceSet" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="baseName" /> or <paramref name="assembly" /> parameter is null. </exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public ResourceManager(string baseName, Assembly assembly, Type usingResourceSet)
	{
		if (baseName == null)
		{
			throw new ArgumentNullException("baseName");
		}
		if (null == assembly)
		{
			throw new ArgumentNullException("assembly");
		}
		if (!(assembly is RuntimeAssembly))
		{
			throw new ArgumentException(Environment.GetResourceString("Assembly must be a runtime Assembly object."));
		}
		MainAssembly = assembly;
		BaseNameField = baseName;
		if (usingResourceSet != null && usingResourceSet != _minResourceSet && !usingResourceSet.IsSubclassOf(_minResourceSet))
		{
			throw new ArgumentException(Environment.GetResourceString("Type parameter must refer to a subclass of ResourceSet."), "usingResourceSet");
		}
		_userResourceSet = usingResourceSet;
		CommonAssemblyInit();
		m_callingAssembly = (RuntimeAssembly)Assembly.GetCallingAssembly();
		if (assembly == typeof(object).Assembly && m_callingAssembly != assembly)
		{
			m_callingAssembly = null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.ResourceManager" /> class that looks up resources in satellite assemblies based on information from the specified type object.</summary>
	/// <param name="resourceSource">A type from which the resource manager derives all information for finding .resources files. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="resourceSource" /> parameter is null. </exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public ResourceManager(Type resourceSource)
	{
		if (null == resourceSource)
		{
			throw new ArgumentNullException("resourceSource");
		}
		if (!(resourceSource is RuntimeType))
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a runtime Type object."));
		}
		_locationInfo = resourceSource;
		MainAssembly = _locationInfo.Assembly;
		BaseNameField = resourceSource.Name;
		SetAppXConfiguration();
		CommonAssemblyInit();
		m_callingAssembly = (RuntimeAssembly)Assembly.GetCallingAssembly();
		if (MainAssembly == typeof(object).Assembly && m_callingAssembly != MainAssembly)
		{
			m_callingAssembly = null;
		}
	}

	[OnDeserializing]
	private void OnDeserializing(StreamingContext ctx)
	{
		_resourceSets = null;
		resourceGroveler = null;
		_lastUsedResourceCache = null;
	}

	[SecuritySafeCritical]
	[OnDeserialized]
	private void OnDeserialized(StreamingContext ctx)
	{
		_resourceSets = new Dictionary<string, ResourceSet>();
		_lastUsedResourceCache = new CultureNameResourceSetPair();
		ResourceManagerMediator mediator = new ResourceManagerMediator(this);
		if (UseManifest)
		{
			resourceGroveler = new ManifestBasedResourceGroveler(mediator);
		}
		else
		{
			resourceGroveler = new FileBasedResourceGroveler(mediator);
		}
		if (m_callingAssembly == null)
		{
			m_callingAssembly = (RuntimeAssembly)_callingAssembly;
		}
		if (UseManifest && _neutralResourcesCulture == null)
		{
			_neutralResourcesCulture = ManifestBasedResourceGroveler.GetNeutralResourcesLanguage(MainAssembly, ref _fallbackLoc);
		}
	}

	[OnSerializing]
	private void OnSerializing(StreamingContext ctx)
	{
		_callingAssembly = m_callingAssembly;
		UseSatelliteAssem = UseManifest;
		ResourceSets = new Hashtable();
	}

	[SecuritySafeCritical]
	private void CommonAssemblyInit()
	{
		UseManifest = true;
		_resourceSets = new Dictionary<string, ResourceSet>();
		_lastUsedResourceCache = new CultureNameResourceSetPair();
		_fallbackLoc = UltimateResourceFallbackLocation.MainAssembly;
		ResourceManagerMediator mediator = new ResourceManagerMediator(this);
		resourceGroveler = new ManifestBasedResourceGroveler(mediator);
		_neutralResourcesCulture = ManifestBasedResourceGroveler.GetNeutralResourcesLanguage(MainAssembly, ref _fallbackLoc);
		ResourceSets = new Hashtable();
	}

	/// <summary>Tells the resource manager to call the <see cref="M:System.Resources.ResourceSet.Close" /> method on all <see cref="T:System.Resources.ResourceSet" /> objects and release all resources.</summary>
	public virtual void ReleaseAllResources()
	{
		Dictionary<string, ResourceSet> resourceSets = _resourceSets;
		_resourceSets = new Dictionary<string, ResourceSet>();
		_lastUsedResourceCache = new CultureNameResourceSetPair();
		lock (resourceSets)
		{
			IDictionaryEnumerator dictionaryEnumerator = resourceSets.GetEnumerator();
			IDictionaryEnumerator dictionaryEnumerator2 = null;
			if (ResourceSets != null)
			{
				dictionaryEnumerator2 = ResourceSets.GetEnumerator();
			}
			ResourceSets = new Hashtable();
			while (dictionaryEnumerator.MoveNext())
			{
				((ResourceSet)dictionaryEnumerator.Value).Close();
			}
			if (dictionaryEnumerator2 != null)
			{
				while (dictionaryEnumerator2.MoveNext())
				{
					((ResourceSet)dictionaryEnumerator2.Value).Close();
				}
			}
		}
	}

	/// <summary>Returns a <see cref="T:System.Resources.ResourceManager" /> object that searches a specific directory instead of an assembly manifest for resources.</summary>
	/// <returns>A new instance of a resource manager that searches the specified directory instead of an assembly manifest for resources.</returns>
	/// <param name="baseName">The root name of the resources. For example, the root name for the resource file named "MyResource.en-US.resources" is "MyResource". </param>
	/// <param name="resourceDir">The name of the directory to search for the resources. <paramref name="resourceDir" /> can be an absolute path or a relative path from the application directory. </param>
	/// <param name="usingResourceSet">The type of the custom <see cref="T:System.Resources.ResourceSet" /> to use. If null, the default runtime <see cref="T:System.Resources.ResourceSet" /> object is used. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="baseName" /> or <paramref name="resourceDir" /> parameter is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static ResourceManager CreateFileBasedResourceManager(string baseName, string resourceDir, Type usingResourceSet)
	{
		return new ResourceManager(baseName, resourceDir, usingResourceSet);
	}

	/// <summary>Generates the name of the resource file for the given <see cref="T:System.Globalization.CultureInfo" /> object.</summary>
	/// <returns>The name that can be used for a resource file for the given <see cref="T:System.Globalization.CultureInfo" /> object.</returns>
	/// <param name="culture">The culture object for which a resource file name is constructed. </param>
	protected virtual string GetResourceFileName(CultureInfo culture)
	{
		StringBuilder stringBuilder = new StringBuilder(255);
		stringBuilder.Append(BaseNameField);
		if (!culture.HasInvariantCultureName)
		{
			CultureInfo.VerifyCultureName(culture.Name, throwException: true);
			stringBuilder.Append('.');
			stringBuilder.Append(culture.Name);
		}
		stringBuilder.Append(".resources");
		return stringBuilder.ToString();
	}

	internal ResourceSet GetFirstResourceSet(CultureInfo culture)
	{
		if (_neutralResourcesCulture != null && culture.Name == _neutralResourcesCulture.Name)
		{
			culture = CultureInfo.InvariantCulture;
		}
		if (_lastUsedResourceCache != null)
		{
			lock (_lastUsedResourceCache)
			{
				if (culture.Name == _lastUsedResourceCache.lastCultureName)
				{
					return _lastUsedResourceCache.lastResourceSet;
				}
			}
		}
		Dictionary<string, ResourceSet> resourceSets = _resourceSets;
		ResourceSet value = null;
		if (resourceSets != null)
		{
			lock (resourceSets)
			{
				resourceSets.TryGetValue(culture.Name, out value);
			}
		}
		if (value != null)
		{
			if (_lastUsedResourceCache != null)
			{
				lock (_lastUsedResourceCache)
				{
					_lastUsedResourceCache.lastCultureName = culture.Name;
					_lastUsedResourceCache.lastResourceSet = value;
				}
			}
			return value;
		}
		return null;
	}

	/// <summary>Retrieves the resource set for a particular culture.</summary>
	/// <returns>The resource set for the specified culture.</returns>
	/// <param name="culture">The culture whose resources are to be retrieved. </param>
	/// <param name="createIfNotExists">true to load the resource set, if it has not been loaded yet; otherwise, false. </param>
	/// <param name="tryParents">true to use resource fallback to load an appropriate resource if the resource set cannot be found; false to bypass the resource fallback process. (See the Remarks section.)</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="culture" /> parameter is null. </exception>
	/// <exception cref="T:System.Resources.MissingManifestResourceException">
	///   <paramref name="tryParents" /> is true, no usable set of resources has been found, and there are no default culture resources. </exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecuritySafeCritical]
	public virtual ResourceSet GetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		Dictionary<string, ResourceSet> resourceSets = _resourceSets;
		if (resourceSets != null)
		{
			lock (resourceSets)
			{
				if (resourceSets.TryGetValue(culture.Name, out var value))
				{
					return value;
				}
			}
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		if (UseManifest && culture.HasInvariantCultureName)
		{
			string resourceFileName = GetResourceFileName(culture);
			Stream manifestResourceStream = ((RuntimeAssembly)MainAssembly).GetManifestResourceStream(_locationInfo, resourceFileName, m_callingAssembly == MainAssembly, ref stackMark);
			if (createIfNotExists && manifestResourceStream != null)
			{
				ResourceSet value = ((ManifestBasedResourceGroveler)resourceGroveler).CreateResourceSet(manifestResourceStream, MainAssembly);
				AddResourceSet(resourceSets, culture.Name, ref value);
				return value;
			}
		}
		return InternalGetResourceSet(culture, createIfNotExists, tryParents);
	}

	/// <summary>Provides the implementation for finding a resource set.</summary>
	/// <returns>The specified resource set.</returns>
	/// <param name="culture">The culture object to look for. </param>
	/// <param name="createIfNotExists">true to load the resource set, if it has not been loaded yet; otherwise, false. </param>
	/// <param name="tryParents">true to check parent <see cref="T:System.Globalization.CultureInfo" /> objects if the resource set cannot be loaded; otherwise, false.</param>
	/// <exception cref="T:System.Resources.MissingManifestResourceException">The main assembly does not contain a .resources file, which is required to look up a resource. </exception>
	/// <exception cref="T:System.ExecutionEngineException">There was an internal error in the runtime.</exception>
	/// <exception cref="T:System.Resources.MissingSatelliteAssemblyException">The satellite assembly associated with <paramref name="culture" /> could not be located.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecuritySafeCritical]
	protected virtual ResourceSet InternalGetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return InternalGetResourceSet(culture, createIfNotExists, tryParents, ref stackMark);
	}

	[SecurityCritical]
	private ResourceSet InternalGetResourceSet(CultureInfo requestedCulture, bool createIfNotExists, bool tryParents, ref StackCrawlMark stackMark)
	{
		Dictionary<string, ResourceSet> resourceSets = _resourceSets;
		ResourceSet value = null;
		CultureInfo cultureInfo = null;
		lock (resourceSets)
		{
			if (resourceSets.TryGetValue(requestedCulture.Name, out value))
			{
				return value;
			}
		}
		ResourceFallbackManager resourceFallbackManager = new ResourceFallbackManager(requestedCulture, _neutralResourcesCulture, tryParents);
		foreach (CultureInfo item in resourceFallbackManager)
		{
			lock (resourceSets)
			{
				if (resourceSets.TryGetValue(item.Name, out value))
				{
					if (requestedCulture != item)
					{
						cultureInfo = item;
					}
					break;
				}
			}
			value = resourceGroveler.GrovelForResourceSet(item, resourceSets, tryParents, createIfNotExists, ref stackMark);
			if (value != null)
			{
				cultureInfo = item;
				break;
			}
		}
		if (value != null && cultureInfo != null)
		{
			foreach (CultureInfo item2 in resourceFallbackManager)
			{
				AddResourceSet(resourceSets, item2.Name, ref value);
				if (item2 == cultureInfo)
				{
					break;
				}
			}
		}
		return value;
	}

	private static void AddResourceSet(Dictionary<string, ResourceSet> localResourceSets, string cultureName, ref ResourceSet rs)
	{
		lock (localResourceSets)
		{
			if (localResourceSets.TryGetValue(cultureName, out var value))
			{
				if (value != rs)
				{
					if (!localResourceSets.ContainsValue(rs))
					{
						rs.Dispose();
					}
					rs = value;
				}
			}
			else
			{
				localResourceSets.Add(cultureName, rs);
			}
		}
	}

	/// <summary>Returns the version specified by the <see cref="T:System.Resources.SatelliteContractVersionAttribute" /> attribute in the given assembly.</summary>
	/// <returns>The satellite contract version of the given assembly, or null if no version was found.</returns>
	/// <param name="a">The assembly to check for the <see cref="T:System.Resources.SatelliteContractVersionAttribute" /> attribute. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Version" /> found in the assembly <paramref name="a" /> is invalid. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="a" /> is null. </exception>
	protected static Version GetSatelliteContractVersion(Assembly a)
	{
		if (a == null)
		{
			throw new ArgumentNullException("a", Environment.GetResourceString("Assembly cannot be null."));
		}
		string text = null;
		if (a.ReflectionOnly)
		{
			foreach (CustomAttributeData customAttribute in CustomAttributeData.GetCustomAttributes(a))
			{
				if (customAttribute.Constructor.DeclaringType == typeof(SatelliteContractVersionAttribute))
				{
					text = (string)customAttribute.ConstructorArguments[0].Value;
					break;
				}
			}
			if (text == null)
			{
				return null;
			}
		}
		else
		{
			object[] customAttributes = a.GetCustomAttributes(typeof(SatelliteContractVersionAttribute), inherit: false);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			text = ((SatelliteContractVersionAttribute)customAttributes[0]).Version;
		}
		try
		{
			return new Version(text);
		}
		catch (ArgumentOutOfRangeException innerException)
		{
			if (a == typeof(object).Assembly)
			{
				return null;
			}
			throw new ArgumentException(Environment.GetResourceString("Satellite contract version attribute on the assembly '{0}' specifies an invalid version: {1}.", a.ToString(), text), innerException);
		}
	}

	/// <summary>Returns culture-specific information for the main assembly's default resources by retrieving the value of the <see cref="T:System.Resources.NeutralResourcesLanguageAttribute" /> attribute on a specified assembly.</summary>
	/// <returns>The culture from the <see cref="T:System.Resources.NeutralResourcesLanguageAttribute" /> attribute, if found; otherwise, the invariant culture.</returns>
	/// <param name="a">The assembly for which to return culture-specific information. </param>
	[SecuritySafeCritical]
	protected static CultureInfo GetNeutralResourcesLanguage(Assembly a)
	{
		UltimateResourceFallbackLocation fallbackLocation = UltimateResourceFallbackLocation.MainAssembly;
		return ManifestBasedResourceGroveler.GetNeutralResourcesLanguage(a, ref fallbackLocation);
	}

	internal static bool CompareNames(string asmTypeName1, string typeName2, AssemblyName asmName2)
	{
		int num = asmTypeName1.IndexOf(',');
		if (((num == -1) ? asmTypeName1.Length : num) != typeName2.Length)
		{
			return false;
		}
		if (string.Compare(asmTypeName1, 0, typeName2, 0, typeName2.Length, StringComparison.Ordinal) != 0)
		{
			return false;
		}
		if (num == -1)
		{
			return true;
		}
		while (char.IsWhiteSpace(asmTypeName1[++num]))
		{
		}
		AssemblyName assemblyName = new AssemblyName(asmTypeName1.Substring(num));
		if (string.Compare(assemblyName.Name, asmName2.Name, StringComparison.OrdinalIgnoreCase) != 0)
		{
			return false;
		}
		if (string.Compare(assemblyName.Name, "mscorlib", StringComparison.OrdinalIgnoreCase) == 0)
		{
			return true;
		}
		if (assemblyName.CultureInfo != null && asmName2.CultureInfo != null && assemblyName.CultureInfo.LCID != asmName2.CultureInfo.LCID)
		{
			return false;
		}
		byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
		byte[] publicKeyToken2 = asmName2.GetPublicKeyToken();
		if (publicKeyToken != null && publicKeyToken2 != null)
		{
			if (publicKeyToken.Length != publicKeyToken2.Length)
			{
				return false;
			}
			for (int i = 0; i < publicKeyToken.Length; i++)
			{
				if (publicKeyToken[i] != publicKeyToken2[i])
				{
					return false;
				}
			}
		}
		return true;
	}

	private void SetAppXConfiguration()
	{
	}

	/// <summary>Returns the value of the specified string resource.</summary>
	/// <returns>The value of the resource localized for the caller's current UI culture, or null if <paramref name="name" /> cannot be found in a resource set.</returns>
	/// <param name="name">The name of the resource to retrieve. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The value of the specified resource is not a string. </exception>
	/// <exception cref="T:System.Resources.MissingManifestResourceException">No usable set of resources has been found, and there are no resources for the default culture. For information about how to handle this exception, see the "Handling MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions" section in the <see cref="T:System.Resources.ResourceManager" /> class topic. </exception>
	/// <exception cref="T:System.Resources.MissingSatelliteAssemblyException">The default culture's resources reside in a satellite assembly that could not be found. For information about how to handle this exception, see the "Handling MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions" section in the <see cref="T:System.Resources.ResourceManager" /> class topic.</exception>
	public virtual string GetString(string name)
	{
		return GetString(name, null);
	}

	/// <summary>Returns the value of the string resource localized for the specified culture.</summary>
	/// <returns>The value of the resource localized for the specified culture, or null if <paramref name="name" /> cannot be found in a resource set.</returns>
	/// <param name="name">The name of the resource to retrieve. </param>
	/// <param name="culture">An object that represents the culture for which the resource is localized. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The value of the specified resource is not a string. </exception>
	/// <exception cref="T:System.Resources.MissingManifestResourceException">No usable set of resources has been found, and there are no resources for a default culture. For information about how to handle this exception, see the "Handling MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions" section in the <see cref="T:System.Resources.ResourceManager" /> class topic. </exception>
	/// <exception cref="T:System.Resources.MissingSatelliteAssemblyException">The default culture's resources reside in a satellite assembly that could not be found. For information about how to handle this exception, see the "Handling MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions" section in the <see cref="T:System.Resources.ResourceManager" /> class topic.</exception>
	public virtual string GetString(string name, CultureInfo culture)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (culture == null)
		{
			culture = Thread.CurrentThread.GetCurrentUICultureNoAppX();
		}
		ResourceSet resourceSet = GetFirstResourceSet(culture);
		if (resourceSet != null)
		{
			string @string = resourceSet.GetString(name, _ignoreCase);
			if (@string != null)
			{
				return @string;
			}
		}
		foreach (CultureInfo item in new ResourceFallbackManager(culture, _neutralResourcesCulture, useParents: true))
		{
			ResourceSet resourceSet2 = InternalGetResourceSet(item, createIfNotExists: true, tryParents: true);
			if (resourceSet2 == null)
			{
				break;
			}
			if (resourceSet2 == resourceSet)
			{
				continue;
			}
			string string2 = resourceSet2.GetString(name, _ignoreCase);
			if (string2 != null)
			{
				if (_lastUsedResourceCache != null)
				{
					lock (_lastUsedResourceCache)
					{
						_lastUsedResourceCache.lastCultureName = item.Name;
						_lastUsedResourceCache.lastResourceSet = resourceSet2;
					}
				}
				return string2;
			}
			resourceSet = resourceSet2;
		}
		return null;
	}

	/// <summary>Returns the value of the specified non-string resource.</summary>
	/// <returns>The value of the resource localized for the caller's current culture settings. If an appropriate resource set exists but <paramref name="name" /> cannot be found, the method returns null. </returns>
	/// <param name="name">The name of the resource to get. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Resources.MissingManifestResourceException">No usable set of localized resources has been found, and there are no default culture resources. For information about how to handle this exception, see the "Handling MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions" section in the <see cref="T:System.Resources.ResourceManager" /> class topic.</exception>
	/// <exception cref="T:System.Resources.MissingSatelliteAssemblyException">The default culture's resources reside in a satellite assembly that could not be found. For information about how to handle this exception, see the "Handling MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions" section in the <see cref="T:System.Resources.ResourceManager" /> class topic.</exception>
	public virtual object GetObject(string name)
	{
		return GetObject(name, null, wrapUnmanagedMemStream: true);
	}

	/// <summary>Gets the value of the specified non-string resource localized for the specified culture.</summary>
	/// <returns>The value of the resource, localized for the specified culture. If an appropriate resource set exists but <paramref name="name" /> cannot be found, the method returns null.</returns>
	/// <param name="name">The name of the resource to get. </param>
	/// <param name="culture">The culture for which the resource is localized. If the resource is not localized for this culture, the resource manager uses fallback rules to locate an appropriate resource.If this value is null, the <see cref="T:System.Globalization.CultureInfo" /> object is obtained by using the <see cref="P:System.Globalization.CultureInfo.CurrentUICulture" /> property. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.Resources.MissingManifestResourceException">No usable set of resources have been found, and there are no default culture resources. For information about how to handle this exception, see the "Handling MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions" section in the <see cref="T:System.Resources.ResourceManager" /> class topic. </exception>
	/// <exception cref="T:System.Resources.MissingSatelliteAssemblyException">The default culture's resources reside in a satellite assembly that could not be found. For information about how to handle this exception, see the "Handling MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions" section in the <see cref="T:System.Resources.ResourceManager" /> class topic. </exception>
	public virtual object GetObject(string name, CultureInfo culture)
	{
		return GetObject(name, culture, wrapUnmanagedMemStream: true);
	}

	private object GetObject(string name, CultureInfo culture, bool wrapUnmanagedMemStream)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (culture == null)
		{
			culture = Thread.CurrentThread.GetCurrentUICultureNoAppX();
		}
		ResourceSet resourceSet = GetFirstResourceSet(culture);
		if (resourceSet != null)
		{
			object @object = resourceSet.GetObject(name, _ignoreCase);
			if (@object != null)
			{
				UnmanagedMemoryStream unmanagedMemoryStream = @object as UnmanagedMemoryStream;
				if (unmanagedMemoryStream != null && wrapUnmanagedMemStream)
				{
					return new UnmanagedMemoryStreamWrapper(unmanagedMemoryStream);
				}
				return @object;
			}
		}
		foreach (CultureInfo item in new ResourceFallbackManager(culture, _neutralResourcesCulture, useParents: true))
		{
			ResourceSet resourceSet2 = InternalGetResourceSet(item, createIfNotExists: true, tryParents: true);
			if (resourceSet2 == null)
			{
				break;
			}
			if (resourceSet2 == resourceSet)
			{
				continue;
			}
			object object2 = resourceSet2.GetObject(name, _ignoreCase);
			if (object2 != null)
			{
				if (_lastUsedResourceCache != null)
				{
					lock (_lastUsedResourceCache)
					{
						_lastUsedResourceCache.lastCultureName = item.Name;
						_lastUsedResourceCache.lastResourceSet = resourceSet2;
					}
				}
				UnmanagedMemoryStream unmanagedMemoryStream2 = object2 as UnmanagedMemoryStream;
				if (unmanagedMemoryStream2 != null && wrapUnmanagedMemStream)
				{
					return new UnmanagedMemoryStreamWrapper(unmanagedMemoryStream2);
				}
				return object2;
			}
			resourceSet = resourceSet2;
		}
		return null;
	}

	/// <summary>Returns an unmanaged memory stream object from the specified resource.</summary>
	/// <returns>An unmanaged memory stream object that represents a resource .</returns>
	/// <param name="name">The name of a resource.</param>
	/// <exception cref="T:System.InvalidOperationException">The value of the specified resource is not a <see cref="T:System.IO.MemoryStream" /> object.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.Resources.MissingManifestResourceException">No usable set of resources is found, and there are no default resources. For information about how to handle this exception, see the "Handling MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions" section in the <see cref="T:System.Resources.ResourceManager" /> class topic. </exception>
	/// <exception cref="T:System.Resources.MissingSatelliteAssemblyException">The default culture's resources reside in a satellite assembly that could not be found. For information about how to handle this exception, see the "Handling MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions" section in the <see cref="T:System.Resources.ResourceManager" /> class topic.</exception>
	[ComVisible(false)]
	public UnmanagedMemoryStream GetStream(string name)
	{
		return GetStream(name, null);
	}

	/// <summary>Returns an unmanaged memory stream object from the specified resource, using the specified culture.</summary>
	/// <returns>An unmanaged memory stream object that represents a resource.</returns>
	/// <param name="name">The name of a resource.</param>
	/// <param name="culture">An  object that specifies the culture to use for the resource lookup. If <paramref name="culture" /> is null, the culture for the current thread is used.</param>
	/// <exception cref="T:System.InvalidOperationException">The value of the specified resource is not a <see cref="T:System.IO.MemoryStream" /> object.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.Resources.MissingManifestResourceException">No usable set of resources is found, and there are no default resources. For information about how to handle this exception, see the "Handling MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions" section in the <see cref="T:System.Resources.ResourceManager" /> class topic. </exception>
	/// <exception cref="T:System.Resources.MissingSatelliteAssemblyException">The default culture's resources reside in a satellite assembly that could not be found. For information about how to handle this exception, see the "Handling MissingManifestResourceException and MissingSatelliteAssemblyException Exceptions" section in the <see cref="T:System.Resources.ResourceManager" /> class topic.</exception>
	[ComVisible(false)]
	public UnmanagedMemoryStream GetStream(string name, CultureInfo culture)
	{
		object @object = GetObject(name, culture, wrapUnmanagedMemStream: false);
		UnmanagedMemoryStream unmanagedMemoryStream = @object as UnmanagedMemoryStream;
		if (unmanagedMemoryStream == null && @object != null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Resource '{0}' was not a Stream - call GetObject instead.", name));
		}
		return unmanagedMemoryStream;
	}
}
