using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading;
using Mono;

namespace System.Reflection;

/// <summary>Represents an assembly, which is a reusable, versionable, and self-describing building block of a common language runtime application.</summary>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
[ComDefaultInterface(typeof(_Assembly))]
[ClassInterface(ClassInterfaceType.None)]
[ComVisible(true)]
public abstract class Assembly : ICustomAttributeProvider, _Assembly, IEvidenceFactory, ISerializable
{
	internal class ResolveEventHolder
	{
		public event ModuleResolveEventHandler ModuleResolve;
	}

	internal class UnmanagedMemoryStreamForModule : UnmanagedMemoryStream
	{
		private Module module;

		public unsafe UnmanagedMemoryStreamForModule(byte* pointer, long length, Module module)
			: base(pointer, length)
		{
			this.module = module;
		}

		protected override void Dispose(bool disposing)
		{
			if (_isOpen)
			{
				module = null;
			}
			base.Dispose(disposing);
		}
	}

	internal IntPtr _mono_assembly;

	private ResolveEventHolder resolve_event_holder;

	private Evidence _evidence;

	internal PermissionSet _minimum;

	internal PermissionSet _optional;

	internal PermissionSet _refuse;

	private PermissionSet _granted;

	private PermissionSet _denied;

	private bool fromByteArray;

	private string assemblyName;

	/// <summary>Gets the location of the assembly as specified originally, for example, in an <see cref="T:System.Reflection.AssemblyName" /> object.</summary>
	/// <returns>The location of the assembly as specified originally.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual string CodeBase => GetCodeBase(escaped: false);

	/// <summary>Gets the URI, including escape characters, that represents the codebase.</summary>
	/// <returns>A URI with escape characters.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual string EscapedCodeBase
	{
		[SecuritySafeCritical]
		get
		{
			return GetCodeBase(escaped: true);
		}
	}

	/// <summary>Gets the display name of the assembly.</summary>
	/// <returns>The display name of the assembly.</returns>
	public virtual string FullName => ToString();

	/// <summary>Gets the entry point of this assembly.</summary>
	/// <returns>An object that represents the entry point of this assembly. If no entry point is found (for example, the assembly is a DLL), null is returned.</returns>
	public virtual extern MethodInfo EntryPoint
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	/// <summary>Gets the evidence for this assembly.</summary>
	/// <returns>The evidence for this assembly.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public virtual Evidence Evidence
	{
		[SecurityPermission(SecurityAction.Demand, ControlEvidence = true)]
		get
		{
			return UnprotectedGetEvidence();
		}
	}

	internal bool FromByteArray
	{
		set
		{
			fromByteArray = value;
		}
	}

	/// <summary>Gets the full path or UNC location of the loaded file that contains the manifest.</summary>
	/// <returns>The location of the loaded file that contains the manifest. If the loaded file was shadow-copied, the location is that of the file after being shadow-copied. If the assembly is loaded from a byte array, such as when using the <see cref="M:System.Reflection.Assembly.Load(System.Byte[])" /> method overload, the value returned is an empty string ("").</returns>
	/// <exception cref="T:System.NotSupportedException">The current assembly is a dynamic assembly, represented by an <see cref="T:System.Reflection.Emit.AssemblyBuilder" /> object. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual string Location
	{
		get
		{
			if (fromByteArray)
			{
				return string.Empty;
			}
			string location = get_location();
			if (location != string.Empty && SecurityManager.SecurityEnabled)
			{
				new FileIOPermission(FileIOPermissionAccess.PathDiscovery, location).Demand();
			}
			return location;
		}
	}

	/// <summary>Gets a string representing the version of the common language runtime (CLR) saved in the file containing the manifest.</summary>
	/// <returns>The CLR version folder name. This is not a full path.</returns>
	[ComVisible(false)]
	public virtual string ImageRuntimeVersion => InternalImageRuntimeVersion();

	/// <summary>Gets the host context with which the assembly was loaded.</summary>
	/// <returns>An <see cref="T:System.Int64" /> value that indicates the host context with which the assembly was loaded, if any.</returns>
	[ComVisible(false)]
	[MonoTODO("Currently it always returns zero")]
	public virtual long HostContext => 0L;

	/// <summary>Gets a <see cref="T:System.Boolean" /> value indicating whether this assembly was loaded into the reflection-only context.</summary>
	/// <returns>true if the assembly was loaded into the reflection-only context, rather than the execution context; otherwise, false.</returns>
	[ComVisible(false)]
	public virtual extern bool ReflectionOnly
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	internal PermissionSet GrantedPermissionSet
	{
		get
		{
			if (_granted == null)
			{
				if (SecurityManager.ResolvingPolicyLevel != null)
				{
					if (SecurityManager.ResolvingPolicyLevel.IsFullTrustAssembly(this))
					{
						return DefaultPolicies.FullTrust;
					}
					return null;
				}
				Resolve();
			}
			return _granted;
		}
	}

	internal PermissionSet DeniedPermissionSet
	{
		get
		{
			if (_granted == null)
			{
				if (SecurityManager.ResolvingPolicyLevel != null)
				{
					if (SecurityManager.ResolvingPolicyLevel.IsFullTrustAssembly(this))
					{
						return null;
					}
					return DefaultPolicies.FullTrust;
				}
				Resolve();
			}
			return _denied;
		}
	}

	/// <summary>Gets the grant set of the current assembly.</summary>
	/// <returns>The grant set of the current assembly.</returns>
	public virtual PermissionSet PermissionSet => GrantedPermissionSet;

	/// <summary>Gets a value that indicates which set of security rules the common language runtime (CLR) enforces for this assembly.</summary>
	/// <returns>The security rule set that the CLR enforces for this assembly.</returns>
	public virtual SecurityRuleSet SecurityRuleSet
	{
		get
		{
			throw CreateNIE();
		}
	}

	/// <summary>Gets a value that indicates whether the current assembly is loaded with full trust.</summary>
	/// <returns>true if the current assembly is loaded with full trust; otherwise, false.</returns>
	[MonoTODO]
	public bool IsFullyTrusted => true;

	/// <summary>Gets the module that contains the manifest for the current assembly. </summary>
	/// <returns>The module that contains the manifest for the assembly. </returns>
	public virtual Module ManifestModule
	{
		get
		{
			throw CreateNIE();
		}
	}

	/// <summary>Gets a value indicating whether the assembly was loaded from the global assembly cache.</summary>
	/// <returns>true if the assembly was loaded from the global assembly cache; otherwise, false.</returns>
	public virtual bool GlobalAssemblyCache
	{
		get
		{
			throw CreateNIE();
		}
	}

	/// <summary>Gets a value that indicates whether the current assembly was generated dynamically in the current process by using reflection emit.</summary>
	/// <returns>true if the current assembly was generated dynamically in the current process; otherwise, false.</returns>
	public virtual bool IsDynamic => false;

	/// <summary>Gets a collection of the types defined in this assembly.</summary>
	/// <returns>A collection of the types defined in this assembly.</returns>
	public virtual IEnumerable<TypeInfo> DefinedTypes
	{
		get
		{
			Type[] types = GetTypes();
			foreach (Type type in types)
			{
				yield return type.GetTypeInfo();
			}
		}
	}

	/// <summary>Gets a collection of the public types defined in this assembly that are visible outside the assembly.</summary>
	/// <returns>A collection of the public types defined in this assembly that are visible outside the assembly.</returns>
	public virtual IEnumerable<Type> ExportedTypes => GetExportedTypes();

	/// <summary>Gets a collection that contains the modules in this assembly.</summary>
	/// <returns>A collection that contains the modules in this assembly.</returns>
	public virtual IEnumerable<Module> Modules => GetModules();

	/// <summary>Gets a collection that contains this assembly's custom attributes.</summary>
	/// <returns>A collection that contains this assembly's custom attributes.</returns>
	public virtual IEnumerable<CustomAttributeData> CustomAttributes => GetCustomAttributesData();

	/// <summary>Occurs when the common language runtime class loader cannot resolve a reference to an internal module of an assembly through normal means.</summary>
	public virtual event ModuleResolveEventHandler ModuleResolve
	{
		[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		add
		{
			resolve_event_holder.ModuleResolve += value;
		}
		[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		remove
		{
			resolve_event_holder.ModuleResolve -= value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.Assembly" /> class.</summary>
	protected Assembly()
	{
		resolve_event_holder = new ResolveEventHolder();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern string get_code_base(bool escaped);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern string get_fullname();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern string get_location();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern string InternalImageRuntimeVersion();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern string GetAotId();

	private string GetCodeBase(bool escaped)
	{
		string text = get_code_base(escaped);
		if (SecurityManager.SecurityEnabled && string.Compare("FILE://", 0, text, 0, 7, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
		{
			string path = text.Substring(7);
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Demand();
		}
		return text;
	}

	internal Evidence UnprotectedGetEvidence()
	{
		if (_evidence == null)
		{
			lock (this)
			{
				_evidence = Evidence.GetDefaultHostEvidence(this);
			}
		}
		return _evidence;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern bool get_global_assembly_cache();

	/// <summary>Gets serialization information with all of the data needed to reinstantiate this assembly.</summary>
	/// <param name="info">The object to be populated with serialization information. </param>
	/// <param name="context">The destination context of the serialization. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
	/// </PermissionSet>
	[SecurityCritical]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		throw new NotImplementedException();
	}

	/// <summary>Indicates whether or not a specified attribute has been applied to the assembly.</summary>
	/// <returns>true if the attribute has been applied to the assembly; otherwise, false.</returns>
	/// <param name="attributeType">The type of the attribute to be checked for this assembly. </param>
	/// <param name="inherit">This argument is ignored for objects of this type. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> uses an invalid type.</exception>
	public virtual bool IsDefined(Type attributeType, bool inherit)
	{
		return MonoCustomAttrs.IsDefined(this, attributeType, inherit);
	}

	/// <summary>Gets all the custom attributes for this assembly.</summary>
	/// <returns>An array that contains the custom attributes for this assembly.</returns>
	/// <param name="inherit">This argument is ignored for objects of type <see cref="T:System.Reflection.Assembly" />. </param>
	public virtual object[] GetCustomAttributes(bool inherit)
	{
		return MonoCustomAttrs.GetCustomAttributes(this, inherit);
	}

	/// <summary>Gets the custom attributes for this assembly as specified by type.</summary>
	/// <returns>An array that contains the custom attributes for this assembly as specified by <paramref name="attributeType" />.</returns>
	/// <param name="attributeType">The type for which the custom attributes are to be returned. </param>
	/// <param name="inherit">This argument is ignored for objects of type <see cref="T:System.Reflection.Assembly" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not a runtime type. </exception>
	public virtual object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		return MonoCustomAttrs.GetCustomAttributes(this, attributeType, inherit);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern object GetFilesInternal(string name, bool getResourceModules);

	/// <summary>Gets the files in the file table of an assembly manifest.</summary>
	/// <returns>An array of streams that contain the files.</returns>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">A file was not found. </exception>
	/// <exception cref="T:System.BadImageFormatException">A file was not a valid assembly. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual FileStream[] GetFiles()
	{
		return GetFiles(getResourceModules: false);
	}

	/// <summary>Gets the files in the file table of an assembly manifest, specifying whether to include resource modules.</summary>
	/// <returns>An array of streams that contain the files.</returns>
	/// <param name="getResourceModules">true to include resource modules; otherwise, false. </param>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">A file was not found. </exception>
	/// <exception cref="T:System.BadImageFormatException">A file was not a valid assembly. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual FileStream[] GetFiles(bool getResourceModules)
	{
		string[] array = (string[])GetFilesInternal(null, getResourceModules);
		if (array == null)
		{
			return EmptyArray<FileStream>.Value;
		}
		string location = Location;
		FileStream[] array2;
		if (location != string.Empty)
		{
			array2 = new FileStream[array.Length + 1];
			array2[0] = new FileStream(location, FileMode.Open, FileAccess.Read);
			for (int i = 0; i < array.Length; i++)
			{
				array2[i + 1] = new FileStream(array[i], FileMode.Open, FileAccess.Read);
			}
		}
		else
		{
			array2 = new FileStream[array.Length];
			for (int j = 0; j < array.Length; j++)
			{
				array2[j] = new FileStream(array[j], FileMode.Open, FileAccess.Read);
			}
		}
		return array2;
	}

	/// <summary>Gets a <see cref="T:System.IO.FileStream" /> for the specified file in the file table of the manifest of this assembly.</summary>
	/// <returns>A stream that contains the specified file, or null if the file is not found.</returns>
	/// <param name="name">The name of the specified file. Do not include the path to the file.</param>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="name" /> parameter is an empty string (""). </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="name" /> was not found. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="name" /> is not a valid assembly. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual FileStream GetFile(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException(null, "Name cannot be null.");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("Empty name is not valid");
		}
		string text = (string)GetFilesInternal(name, getResourceModules: true);
		if (text != null)
		{
			return new FileStream(text, FileMode.Open, FileAccess.Read);
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern IntPtr GetManifestResourceInternal(string name, out int size, out Module module);

	/// <summary>Loads the specified manifest resource from this assembly.</summary>
	/// <returns>The manifest resource; or null if no resources were specified during compilation or if the resource is not visible to the caller.</returns>
	/// <param name="name">The case-sensitive name of the manifest resource being requested. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="name" /> parameter is an empty string (""). </exception>
	/// <exception cref="T:System.IO.FileLoadException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.IO.IOException" />, instead.A file that was found could not be loaded. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="name" /> was not found. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="name" /> is not a valid assembly. </exception>
	/// <exception cref="T:System.NotImplementedException">Resource length is greater than <see cref="F:System.Int64.MaxValue" />.</exception>
	public unsafe virtual Stream GetManifestResourceStream(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("String cannot have zero length.", "name");
		}
		ManifestResourceInfo manifestResourceInfo = GetManifestResourceInfo(name);
		if (manifestResourceInfo == null)
		{
			Assembly assembly = AppDomain.CurrentDomain.DoResourceResolve(name, this);
			if (assembly != null && assembly != this)
			{
				return assembly.GetManifestResourceStream(name);
			}
			return null;
		}
		if (manifestResourceInfo.ReferencedAssembly != null)
		{
			return manifestResourceInfo.ReferencedAssembly.GetManifestResourceStream(name);
		}
		if (manifestResourceInfo.FileName != null && manifestResourceInfo.ResourceLocation == (ResourceLocation)0)
		{
			if (fromByteArray)
			{
				throw new FileNotFoundException(manifestResourceInfo.FileName);
			}
			return new FileStream(Path.Combine(Path.GetDirectoryName(Location), manifestResourceInfo.FileName), FileMode.Open, FileAccess.Read);
		}
		int size;
		Module module;
		IntPtr manifestResourceInternal = GetManifestResourceInternal(name, out size, out module);
		if (manifestResourceInternal == (IntPtr)0)
		{
			return null;
		}
		return new UnmanagedMemoryStreamForModule((byte*)(void*)manifestResourceInternal, size, module);
	}

	/// <summary>Loads the specified manifest resource, scoped by the namespace of the specified type, from this assembly.</summary>
	/// <returns>The manifest resource; or null if no resources were specified during compilation or if the resource is not visible to the caller.</returns>
	/// <param name="type">The type whose namespace is used to scope the manifest resource name. </param>
	/// <param name="name">The case-sensitive name of the manifest resource being requested. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="name" /> parameter is an empty string (""). </exception>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="name" /> was not found. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="name" /> is not a valid assembly. </exception>
	/// <exception cref="T:System.NotImplementedException">Resource length is greater than <see cref="F:System.Int64.MaxValue" />.</exception>
	public virtual Stream GetManifestResourceStream(Type type, string name)
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return GetManifestResourceStream(type, name, skipSecurityCheck: false, ref stackMark);
	}

	internal Stream GetManifestResourceStream(Type type, string name, bool skipSecurityCheck, ref StackCrawlMark stackMark)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (type == null)
		{
			if (name == null)
			{
				throw new ArgumentNullException("type");
			}
		}
		else
		{
			string @namespace = type.Namespace;
			if (@namespace != null)
			{
				stringBuilder.Append(@namespace);
				if (name != null)
				{
					stringBuilder.Append(Type.Delimiter);
				}
			}
		}
		if (name != null)
		{
			stringBuilder.Append(name);
		}
		return GetManifestResourceStream(stringBuilder.ToString());
	}

	internal Stream GetManifestResourceStream(string name, ref StackCrawlMark stackMark, bool skipSecurityCheck)
	{
		return GetManifestResourceStream(null, name, skipSecurityCheck, ref stackMark);
	}

	internal string GetSimpleName()
	{
		return GetName(copiedName: true).Name;
	}

	internal byte[] GetPublicKey()
	{
		return GetName(copiedName: true).GetPublicKey();
	}

	internal Version GetVersion()
	{
		return GetName(copiedName: true).Version;
	}

	private AssemblyNameFlags GetFlags()
	{
		return GetName(copiedName: true).Flags;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal virtual extern Type[] GetTypes(bool exportedOnly);

	/// <summary>Gets the types defined in this assembly.</summary>
	/// <returns>An array that contains all the types that are defined in this assembly.</returns>
	/// <exception cref="T:System.Reflection.ReflectionTypeLoadException">The assembly contains one or more types that cannot be loaded. The array returned by the <see cref="P:System.Reflection.ReflectionTypeLoadException.Types" /> property of this exception contains a <see cref="T:System.Type" /> object for each type that was loaded and null for each type that could not be loaded, while the <see cref="P:System.Reflection.ReflectionTypeLoadException.LoaderExceptions" /> property contains an exception for each type that could not be loaded.</exception>
	public virtual Type[] GetTypes()
	{
		return GetTypes(exportedOnly: false);
	}

	/// <summary>Gets the public types defined in this assembly that are visible outside the assembly.</summary>
	/// <returns>An array that represents the types defined in this assembly that are visible outside the assembly.</returns>
	/// <exception cref="T:System.NotSupportedException">The assembly is a dynamic assembly.</exception>
	public virtual Type[] GetExportedTypes()
	{
		return GetTypes(exportedOnly: true);
	}

	/// <summary>Gets the <see cref="T:System.Type" /> object with the specified name in the assembly instance and optionally throws an exception if the type is not found.</summary>
	/// <returns>An object that represents the specified class.</returns>
	/// <param name="name">The full name of the type. </param>
	/// <param name="throwOnError">true to throw an exception if the type is not found; false to return null. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is invalid.-or- The length of <paramref name="name" /> exceeds 1024 characters. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="throwOnError" /> is true, and the type cannot be found.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="name" /> requires a dependent assembly that could not be found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="name" /> requires a dependent assembly that was found but could not be loaded.-or-The current assembly was loaded into the reflection-only context, and <paramref name="name" /> requires a dependent assembly that was not preloaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="name" /> requires a dependent assembly, but the file is not a valid assembly. -or-<paramref name="name" /> requires a dependent assembly which was compiled for a version of the runtime later than the currently loaded version.</exception>
	public virtual Type GetType(string name, bool throwOnError)
	{
		return GetType(name, throwOnError, ignoreCase: false);
	}

	/// <summary>Gets the <see cref="T:System.Type" /> object with the specified name in the assembly instance.</summary>
	/// <returns>An object that represents the specified class, or null if the class is not found.</returns>
	/// <param name="name">The full name of the type. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is invalid. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="name" /> requires a dependent assembly that could not be found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.IO.IOException" />, instead.<paramref name="name" /> requires a dependent assembly that was found but could not be loaded.-or-The current assembly was loaded into the reflection-only context, and <paramref name="name" /> requires a dependent assembly that was not preloaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="name" /> requires a dependent assembly, but the file is not a valid assembly. -or-<paramref name="name" /> requires a dependent assembly which was compiled for a version of the runtime later than the currently loaded version. </exception>
	public virtual Type GetType(string name)
	{
		return GetType(name, throwOnError: false, ignoreCase: false);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern Type InternalGetType(Module module, string name, bool throwOnError, bool ignoreCase);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern void InternalGetAssemblyName(string assemblyFile, out MonoAssemblyName aname, out string codebase);

	/// <summary>Gets an <see cref="T:System.Reflection.AssemblyName" /> for this assembly, setting the codebase as specified by <paramref name="copiedName" />.</summary>
	/// <returns>An object that contains the fully parsed display name for this assembly.</returns>
	/// <param name="copiedName">true to set the <see cref="P:System.Reflection.Assembly.CodeBase" /> to the location of the assembly after it was shadow copied; false to set <see cref="P:System.Reflection.Assembly.CodeBase" /> to the original location. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual AssemblyName GetName(bool copiedName)
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets an <see cref="T:System.Reflection.AssemblyName" /> for this assembly.</summary>
	/// <returns>An object that contains the fully parsed display name for this assembly.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual AssemblyName GetName()
	{
		return GetName(copiedName: false);
	}

	/// <summary>Returns the full name of the assembly, also known as the display name.</summary>
	/// <returns>The full name of the assembly, or the class name if the full name of the assembly cannot be determined.</returns>
	public override string ToString()
	{
		if (assemblyName != null)
		{
			return assemblyName;
		}
		assemblyName = get_fullname();
		return assemblyName;
	}

	/// <summary>Creates the name of a type qualified by the display name of its assembly.</summary>
	/// <returns>The full name of the type qualified by the display name of the assembly.</returns>
	/// <param name="assemblyName">The display name of an assembly. </param>
	/// <param name="typeName">The full name of a type. </param>
	public static string CreateQualifiedName(string assemblyName, string typeName)
	{
		return typeName + ", " + assemblyName;
	}

	/// <summary>Gets the currently loaded assembly in which the specified class is defined.</summary>
	/// <returns>The assembly in which the specified class is defined.</returns>
	/// <param name="type">An object representing a class in the assembly that will be returned. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null. </exception>
	public static Assembly GetAssembly(Type type)
	{
		if (type != null)
		{
			return type.Assembly;
		}
		throw new ArgumentNullException("type");
	}

	/// <summary>Gets the process executable in the default application domain. In other application domains, this is the first executable that was executed by <see cref="M:System.AppDomain.ExecuteAssembly(System.String)" />.</summary>
	/// <returns>The assembly that is the process executable in the default application domain, or the first executable that was executed by <see cref="M:System.AppDomain.ExecuteAssembly(System.String)" />. Can return null when called from unmanaged code.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern Assembly GetEntryAssembly();

	internal Assembly GetSatelliteAssembly(CultureInfo culture, Version version, bool throwOnError)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		string name = GetSimpleName() + ".resources";
		return InternalGetSatelliteAssembly(name, culture, version, throwOnFileNotFound: true, ref stackMark);
	}

	internal RuntimeAssembly InternalGetSatelliteAssembly(string name, CultureInfo culture, Version version, bool throwOnFileNotFound, ref StackCrawlMark stackMark)
	{
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.SetPublicKey(GetPublicKey());
		assemblyName.Flags = GetFlags() | AssemblyNameFlags.PublicKey;
		if (version == null)
		{
			assemblyName.Version = GetVersion();
		}
		else
		{
			assemblyName.Version = version;
		}
		assemblyName.CultureInfo = culture;
		assemblyName.Name = name;
		try
		{
			Assembly assembly = AppDomain.CurrentDomain.LoadSatellite(assemblyName, throwOnError: false);
			if (assembly != null)
			{
				return (RuntimeAssembly)assembly;
			}
		}
		catch (FileNotFoundException)
		{
			Assembly assembly = null;
		}
		if (string.IsNullOrEmpty(Location))
		{
			return null;
		}
		string text = Path.Combine(Path.GetDirectoryName(Location), Path.Combine(culture.Name, assemblyName.Name + ".dll"));
		try
		{
			return (RuntimeAssembly)LoadFrom(text);
		}
		catch
		{
			if (!throwOnFileNotFound && !File.Exists(text))
			{
				return null;
			}
			throw;
		}
	}

	/// <summary>Returns the type of the current instance.</summary>
	/// <returns>An object that represents the <see cref="T:System.Reflection.Assembly" /> type.</returns>
	Type _Assembly.GetType()
	{
		return GetType();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern Assembly LoadFrom(string assemblyFile, bool refonly);

	/// <summary>Loads an assembly given its file name or path.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyFile" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyFile" /> is not found, or the module you are trying to load does not specify a filename extension. </exception>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly; for example, a 32-bit assembly in a 64-bit process. See the exception topic for more information. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="assemblyFile" /> was compiled with a later version.</exception>
	/// <exception cref="T:System.Security.SecurityException">A codebase that does not start with "file://" was specified without the required <see cref="T:System.Net.WebPermission" />. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="assemblyFile" /> parameter is an empty string (""). </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The assembly name is longer than MAX_PATH characters.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public static Assembly LoadFrom(string assemblyFile)
	{
		return LoadFrom(assemblyFile, refonly: false);
	}

	/// <summary>Loads an assembly given its file name or path and supplying security evidence.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly. </param>
	/// <param name="securityEvidence">Evidence for loading the assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyFile" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyFile" /> is not found, or the module you are trying to load does not specify a filename extension. </exception>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded.-or-The <paramref name="securityEvidence" /> is not ambiguous and is determined to be invalid.</exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly; for example, a 32-bit assembly in a 64-bit process. See the exception topic for more information. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="assemblyFile" /> was compiled with a later version.</exception>
	/// <exception cref="T:System.Security.SecurityException">A codebase that does not start with "file://" was specified without the required <see cref="T:System.Net.WebPermission" />. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="assemblyFile" /> parameter is an empty string (""). </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The assembly name is longer than MAX_PATH characters.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[Obsolete]
	public static Assembly LoadFrom(string assemblyFile, Evidence securityEvidence)
	{
		Assembly assembly = LoadFrom(assemblyFile, refonly: false);
		if (assembly != null && securityEvidence != null)
		{
			assembly.Evidence.Merge(securityEvidence);
		}
		return assembly;
	}

	/// <summary>Loads an assembly given its file name or path, security evidence, hash value, and hash algorithm.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly. </param>
	/// <param name="securityEvidence">Evidence for loading the assembly. </param>
	/// <param name="hashValue">The value of the computed hash code. </param>
	/// <param name="hashAlgorithm">The hash algorithm used for hashing files and for generating the strong name. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyFile" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyFile" /> is not found, or the module you are trying to load does not specify a filename extension. </exception>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded.-or-The <paramref name="securityEvidence" /> is not ambiguous and is determined to be invalid. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly; for example, a 32-bit assembly in a 64-bit process. See the exception topic for more information. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="assemblyFile" /> was compiled with a later version.</exception>
	/// <exception cref="T:System.Security.SecurityException">A codebase that does not start with "file://" was specified without the required <see cref="T:System.Net.WebPermission" />. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="assemblyFile" /> parameter is an empty string (""). </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The assembly name is longer than MAX_PATH characters.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[MonoTODO("This overload is not currently implemented")]
	[Obsolete]
	public static Assembly LoadFrom(string assemblyFile, Evidence securityEvidence, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
	{
		throw new NotImplementedException();
	}

	/// <summary>Loads an assembly given its file name or path, hash value, and hash algorithm.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly. </param>
	/// <param name="hashValue">The value of the computed hash code. </param>
	/// <param name="hashAlgorithm">The hash algorithm used for hashing files and for generating the strong name. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyFile" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyFile" /> is not found, or the module you are trying to load does not specify a file name extension. </exception>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded.</exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly; for example, a 32-bit assembly in a 64-bit process. See the exception topic for more information. -or-<paramref name="assemblyFile" /> was compiled with a later version of the common language runtime than the version that is currently loaded.</exception>
	/// <exception cref="T:System.Security.SecurityException">A codebase that does not start with "file://" was specified without the required <see cref="T:System.Net.WebPermission" />. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="assemblyFile" /> parameter is an empty string (""). </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The assembly name is longer than MAX_PATH characters.</exception>
	[MonoTODO]
	public static Assembly LoadFrom(string assemblyFile, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
	{
		throw new NotImplementedException();
	}

	/// <summary>Loads an assembly into the load-from context, bypassing some security checks.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyFile" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyFile" /> is not found, or the module you are trying to load does not specify a filename extension. </exception>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly. -or-<paramref name="assemblyFile" /> was compiled with a later version of the common language runtime than the version that is currently loaded.</exception>
	/// <exception cref="T:System.Security.SecurityException">A codebase that does not start with "file://" was specified without the required <see cref="T:System.Net.WebPermission" />. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="assemblyFile" /> parameter is an empty string (""). </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The assembly name is longer than MAX_PATH characters.</exception>
	public static Assembly UnsafeLoadFrom(string assemblyFile)
	{
		return LoadFrom(assemblyFile);
	}

	/// <summary>Loads an assembly given its path, loading the assembly into the domain of the caller using the supplied evidence.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="path">The path of the assembly file. </param>
	/// <param name="securityEvidence">Evidence for loading the assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> parameter is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The <paramref name="path" /> parameter is an empty string ("") or does not exist. </exception>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="path" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="path" /> was compiled with a later version.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="securityEvidence" /> is not null. By default, legacy CAS policy is not enabled in the .NET Framework 4; when it is not enabled, <paramref name="securityEvidence" /> must be null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Obsolete]
	public static Assembly LoadFile(string path, Evidence securityEvidence)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path == string.Empty)
		{
			throw new ArgumentException("Path can't be empty", "path");
		}
		return LoadFrom(path, securityEvidence);
	}

	/// <summary>Loads the contents of an assembly file on the specified path.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="path">The path of the file to load. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> parameter is null. </exception>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The <paramref name="path" /> parameter is an empty string ("") or does not exist. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="path" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="path" /> was compiled with a later version.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public static Assembly LoadFile(string path)
	{
		return LoadFile(path, null);
	}

	/// <summary>Loads an assembly given the long form of its name.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="assemblyString">The long form of the assembly name. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyString" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="assemblyString" /> is a zero-length string. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyString" /> is not found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyString" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="assemblyString" /> was compiled with a later version.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public static Assembly Load(string assemblyString)
	{
		return AppDomain.CurrentDomain.Load(assemblyString);
	}

	/// <summary>Loads an assembly given its display name, loading the assembly into the domain of the caller using the supplied evidence.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="assemblyString">The display name of the assembly. </param>
	/// <param name="assemblySecurity">Evidence for loading the assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyString" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyString" /> is not found. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyString" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="assemblyString" /> was compiled with a later version.</exception>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded.-or-An assembly or module was loaded twice with two different evidences. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[Obsolete]
	public static Assembly Load(string assemblyString, Evidence assemblySecurity)
	{
		return AppDomain.CurrentDomain.Load(assemblyString, assemblySecurity);
	}

	/// <summary>Loads an assembly given its <see cref="T:System.Reflection.AssemblyName" />.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="assemblyRef">The object that describes the assembly to be loaded. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyRef" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyRef" /> is not found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.IO.IOException" />, instead.A file that was found could not be loaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyRef" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="assemblyRef" /> was compiled with a later version.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public static Assembly Load(AssemblyName assemblyRef)
	{
		return AppDomain.CurrentDomain.Load(assemblyRef);
	}

	/// <summary>Loads an assembly given its <see cref="T:System.Reflection.AssemblyName" />. The assembly is loaded into the domain of the caller using the supplied evidence.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="assemblyRef">The object that describes the assembly to be loaded. </param>
	/// <param name="assemblySecurity">Evidence for loading the assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyRef" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyRef" /> is not found. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyRef" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="assemblyRef" /> was compiled with a later version.</exception>
	/// <exception cref="T:System.IO.FileLoadException">An assembly or module was loaded twice with two different evidences. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[Obsolete]
	public static Assembly Load(AssemblyName assemblyRef, Evidence assemblySecurity)
	{
		return AppDomain.CurrentDomain.Load(assemblyRef, assemblySecurity);
	}

	/// <summary>Loads the assembly with a common object file format (COFF)-based image containing an emitted assembly. The assembly is loaded into the application domain of the caller.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="rawAssembly">A byte array that is a COFF-based image containing an emitted assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rawAssembly" /> is null. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="rawAssembly" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="rawAssembly" /> was compiled with a later version.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public static Assembly Load(byte[] rawAssembly)
	{
		return AppDomain.CurrentDomain.Load(rawAssembly);
	}

	/// <summary>Loads the assembly with a common object file format (COFF)-based image containing an emitted assembly, optionally including symbols for the assembly. The assembly is loaded into the application domain of the caller.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="rawAssembly">A byte array that is a COFF-based image containing an emitted assembly. </param>
	/// <param name="rawSymbolStore">A byte array that contains the raw bytes representing the symbols for the assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rawAssembly" /> is null. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="rawAssembly" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="rawAssembly" /> was compiled with a later version.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public static Assembly Load(byte[] rawAssembly, byte[] rawSymbolStore)
	{
		return AppDomain.CurrentDomain.Load(rawAssembly, rawSymbolStore);
	}

	/// <summary>Loads the assembly with a common object file format (COFF)-based image containing an emitted assembly, optionally including symbols and evidence for the assembly. The assembly is loaded into the application domain of the caller.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="rawAssembly">A byte array that is a COFF-based image containing an emitted assembly. </param>
	/// <param name="rawSymbolStore">A byte array that contains the raw bytes representing the symbols for the assembly. </param>
	/// <param name="securityEvidence">Evidence for loading the assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rawAssembly" /> is null. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="rawAssembly" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="rawAssembly" /> was compiled with a later version.</exception>
	/// <exception cref="T:System.IO.FileLoadException">An assembly or module was loaded twice with two different evidences. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="securityEvidence" /> is not null.  By default, legacy CAS policy is not enabled in the .NET Framework 4; when it is not enabled, <paramref name="securityEvidence" /> must be null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Obsolete]
	public static Assembly Load(byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence)
	{
		return AppDomain.CurrentDomain.Load(rawAssembly, rawSymbolStore, securityEvidence);
	}

	/// <summary>Loads the assembly with a common object file format (COFF)-based image containing an emitted assembly, optionally including symbols and specifying the source for the security context. The assembly is loaded into the application domain of the caller.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="rawAssembly">A byte array that is a COFF-based image containing an emitted assembly. </param>
	/// <param name="rawSymbolStore">A byte array that contains the raw bytes representing the symbols for the assembly. </param>
	/// <param name="securityContextSource">The source of the security context. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rawAssembly" /> is null. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="rawAssembly" /> is not a valid assembly. -or-<paramref name="rawAssembly" /> was compiled with a later version of the common language runtime than the version that is currently loaded.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="securityContextSource" /> is not one of the enumeration values.</exception>
	[MonoLimitation("Argument securityContextSource is ignored")]
	public static Assembly Load(byte[] rawAssembly, byte[] rawSymbolStore, SecurityContextSource securityContextSource)
	{
		return AppDomain.CurrentDomain.Load(rawAssembly, rawSymbolStore);
	}

	/// <summary>Loads the assembly from a common object file format (COFF)-based image containing an emitted assembly. The assembly is loaded into the reflection-only context of the caller's application domain.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="rawAssembly">A byte array that is a COFF-based image containing an emitted assembly.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rawAssembly" /> is null. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="rawAssembly" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="rawAssembly" /> was compiled with a later version.</exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="rawAssembly" /> cannot be loaded. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public static Assembly ReflectionOnlyLoad(byte[] rawAssembly)
	{
		return AppDomain.CurrentDomain.Load(rawAssembly, null, null, refonly: true);
	}

	/// <summary>Loads an assembly into the reflection-only context, given its display name.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="assemblyString">The display name of the assembly, as returned by the <see cref="P:System.Reflection.AssemblyName.FullName" /> property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyString" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="assemblyString" /> is an empty string (""). </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyString" /> is not found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="assemblyString" /> is found, but cannot be loaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyString" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="assemblyString" /> was compiled with a later version.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public static Assembly ReflectionOnlyLoad(string assemblyString)
	{
		return AppDomain.CurrentDomain.Load(assemblyString, null, refonly: true);
	}

	/// <summary>Loads an assembly into the reflection-only context, given its path.</summary>
	/// <returns>The loaded assembly.</returns>
	/// <param name="assemblyFile">The path of the file that contains the manifest of the assembly.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyFile" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyFile" /> is not found, or the module you are trying to load does not specify a file name extension. </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="assemblyFile" /> is found, but could not be loaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="assemblyFile" /> was compiled with a later version.</exception>
	/// <exception cref="T:System.Security.SecurityException">A codebase that does not start with "file://" was specified without the required <see cref="T:System.Net.WebPermission" />. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The assembly name is longer than MAX_PATH characters. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="assemblyFile" /> is an empty string (""). </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public static Assembly ReflectionOnlyLoadFrom(string assemblyFile)
	{
		if (assemblyFile == null)
		{
			throw new ArgumentNullException("assemblyFile");
		}
		return LoadFrom(assemblyFile, refonly: true);
	}

	/// <summary>Loads an assembly from the application directory or from the global assembly cache using a partial name.</summary>
	/// <returns>The loaded assembly. If <paramref name="partialName" /> is not found, this method returns null.</returns>
	/// <param name="partialName">The display name of the assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="partialName" /> parameter is null. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="partialName" /> was compiled with a later version.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Obsolete("This method has been deprecated. Please use Assembly.Load() instead. http://go.microsoft.com/fwlink/?linkid=14202")]
	public static Assembly LoadWithPartialName(string partialName)
	{
		return LoadWithPartialName(partialName, null);
	}

	/// <summary>Loads the module, internal to this assembly, with a common object file format (COFF)-based image containing an emitted module, or a resource file.</summary>
	/// <returns>The loaded module.</returns>
	/// <param name="moduleName">The name of the module. This string must correspond to a file name in this assembly's manifest. </param>
	/// <param name="rawModule">A byte array that is a COFF-based image containing an emitted module, or a resource. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="moduleName" /> or <paramref name="rawModule" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="moduleName" /> does not match a file entry in this assembly's manifest. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="rawModule" /> is not a valid module. </exception>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[MonoTODO("Not implemented")]
	public Module LoadModule(string moduleName, byte[] rawModule)
	{
		throw new NotImplementedException();
	}

	/// <summary>Loads the module, internal to this assembly, with a common object file format (COFF)-based image containing an emitted module, or a resource file. The raw bytes representing the symbols for the module are also loaded.</summary>
	/// <returns>The loaded module.</returns>
	/// <param name="moduleName">The name of the module. This string must correspond to a file name in this assembly's manifest. </param>
	/// <param name="rawModule">A byte array that is a COFF-based image containing an emitted module, or a resource. </param>
	/// <param name="rawSymbolStore">A byte array containing the raw bytes representing the symbols for the module. Must be null if this is a resource file. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="moduleName" /> or <paramref name="rawModule" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="moduleName" /> does not match a file entry in this assembly's manifest. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="rawModule" /> is not a valid module. </exception>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[MonoTODO("Not implemented")]
	public virtual Module LoadModule(string moduleName, byte[] rawModule, byte[] rawSymbolStore)
	{
		throw new NotImplementedException();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern Assembly load_with_partial_name(string name, Evidence e);

	/// <summary>Loads an assembly from the application directory or from the global assembly cache using a partial name. The assembly is loaded into the domain of the caller using the supplied evidence.</summary>
	/// <returns>The loaded assembly. If <paramref name="partialName" /> is not found, this method returns null.</returns>
	/// <param name="partialName">The display name of the assembly. </param>
	/// <param name="securityEvidence">Evidence for loading the assembly. </param>
	/// <exception cref="T:System.IO.FileLoadException">An assembly or module was loaded twice with two different sets of evidence. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="partialName" /> parameter is null. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="partialName" /> was compiled with a later version.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Obsolete("This method has been deprecated. Please use Assembly.Load() instead. http://go.microsoft.com/fwlink/?linkid=14202")]
	public static Assembly LoadWithPartialName(string partialName, Evidence securityEvidence)
	{
		return LoadWithPartialName(partialName, securityEvidence, oldBehavior: true);
	}

	internal static Assembly LoadWithPartialName(string partialName, Evidence securityEvidence, bool oldBehavior)
	{
		if (!oldBehavior)
		{
			throw new NotImplementedException();
		}
		if (partialName == null)
		{
			throw new NullReferenceException();
		}
		return load_with_partial_name(partialName, securityEvidence);
	}

	/// <summary>Locates the specified type from this assembly and creates an instance of it using the system activator, using case-sensitive search.</summary>
	/// <returns>An instance of the specified type created with the default constructor; or null if <paramref name="typeName" /> is not found. The type is resolved using the default binder, without specifying culture or activation attributes, and with <see cref="T:System.Reflection.BindingFlags" /> set to Public or Instance. </returns>
	/// <param name="typeName">The <see cref="P:System.Type.FullName" /> of the type to locate. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="typeName" /> is an empty string ("") or a string beginning with a null character.-or-The current assembly was loaded into the reflection-only context.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="typeName" /> requires a dependent assembly that could not be found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="typeName" /> requires a dependent assembly that was found but could not be loaded.-or-The current assembly was loaded into the reflection-only context, and <paramref name="typeName" /> requires a dependent assembly that was not preloaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="typeName" /> requires a dependent assembly, but the file is not a valid assembly. -or-<paramref name="typeName" /> requires a dependent assembly which was compiled for a version of the runtime later than the currently loaded version.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public object CreateInstance(string typeName)
	{
		return CreateInstance(typeName, ignoreCase: false);
	}

	/// <summary>Locates the specified type from this assembly and creates an instance of it using the system activator, with optional case-sensitive search.</summary>
	/// <returns>An instance of the specified type created with the default constructor; or null if <paramref name="typeName" /> is not found. The type is resolved using the default binder, without specifying culture or activation attributes, and with <see cref="T:System.Reflection.BindingFlags" /> set to Public or Instance.</returns>
	/// <param name="typeName">The <see cref="P:System.Type.FullName" /> of the type to locate. </param>
	/// <param name="ignoreCase">true to ignore the case of the type name; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="typeName" /> is an empty string ("") or a string beginning with a null character. -or-The current assembly was loaded into the reflection-only context.</exception>
	/// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="typeName" /> requires a dependent assembly that could not be found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="typeName" /> requires a dependent assembly that was found but could not be loaded.-or-The current assembly was loaded into the reflection-only context, and <paramref name="typeName" /> requires a dependent assembly that was not preloaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="typeName" /> requires a dependent assembly, but the file is not a valid assembly. -or-<paramref name="typeName" /> requires a dependent assembly which was compiled for a version of the runtime later than the currently loaded version.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public object CreateInstance(string typeName, bool ignoreCase)
	{
		Type type = GetType(typeName, throwOnError: false, ignoreCase);
		if (type == null)
		{
			return null;
		}
		try
		{
			return Activator.CreateInstance(type);
		}
		catch (InvalidOperationException)
		{
			throw new ArgumentException("It is illegal to invoke a method on a Type loaded via ReflectionOnly methods.");
		}
	}

	/// <summary>Locates the specified type from this assembly and creates an instance of it using the system activator, with optional case-sensitive search and having the specified culture, arguments, and binding and activation attributes.</summary>
	/// <returns>An instance of the specified type, or null if <paramref name="typeName" /> is not found. The supplied arguments are used to resolve the type, and to bind the constructor that is used to create the instance.</returns>
	/// <param name="typeName">The <see cref="P:System.Type.FullName" /> of the type to locate. </param>
	/// <param name="ignoreCase">true to ignore the case of the type name; otherwise, false. </param>
	/// <param name="bindingAttr">A bitmask that affects the way in which the search is conducted. The value is a combination of bit flags from <see cref="T:System.Reflection.BindingFlags" />. </param>
	/// <param name="binder">An object that enables the binding, coercion of argument types, invocation of members, and retrieval of MemberInfo objects via reflection. If <paramref name="binder" /> is null, the default binder is used. </param>
	/// <param name="args">An array that contains the arguments to be passed to the constructor. This array of arguments must match in number, order, and type the parameters of the constructor to be invoked. If the default constructor is desired, <paramref name="args" /> must be an empty array or null. </param>
	/// <param name="culture">An instance of CultureInfo used to govern the coercion of types. If this is null, the CultureInfo for the current thread is used. (This is necessary to convert a String that represents 1000 to a Double value, for example, since 1000 is represented differently by different cultures.) </param>
	/// <param name="activationAttributes">An array of one or more attributes that can participate in activation. Typically, an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object.  </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="typeName" /> is an empty string ("") or a string beginning with a null character. -or-The current assembly was loaded into the reflection-only context.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
	/// <exception cref="T:System.NotSupportedException">A non-empty activation attributes array is passed to a type that does not inherit from <see cref="T:System.MarshalByRefObject" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="typeName" /> requires a dependent assembly that could not be found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="typeName" /> requires a dependent assembly that was found but could not be loaded.-or-The current assembly was loaded into the reflection-only context, and <paramref name="typeName" /> requires a dependent assembly that was not preloaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="typeName" /> requires a dependent assembly, but the file is not a valid assembly. -or-<paramref name="typeName" /> requires a dependent assembly which was compiled for a version of the runtime later than the currently loaded version.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public virtual object CreateInstance(string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
	{
		Type type = GetType(typeName, throwOnError: false, ignoreCase);
		if (type == null)
		{
			return null;
		}
		try
		{
			return Activator.CreateInstance(type, bindingAttr, binder, args, culture, activationAttributes);
		}
		catch (InvalidOperationException)
		{
			throw new ArgumentException("It is illegal to invoke a method on a Type loaded via ReflectionOnly methods.");
		}
	}

	/// <summary>Gets all the loaded modules that are part of this assembly.</summary>
	/// <returns>An array of modules.</returns>
	public Module[] GetLoadedModules()
	{
		return GetLoadedModules(getResourceModules: false);
	}

	/// <summary>Gets all the modules that are part of this assembly.</summary>
	/// <returns>An array of modules.</returns>
	/// <exception cref="T:System.IO.FileNotFoundException">The module to be loaded does not specify a file name extension. </exception>
	public Module[] GetModules()
	{
		return GetModules(getResourceModules: false);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal virtual extern Module[] GetModulesInternal();

	/// <summary>Returns the names of all the resources in this assembly.</summary>
	/// <returns>An array that contains the names of all the resources.</returns>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public virtual extern string[] GetManifestResourceNames();

	/// <summary>Gets the assembly that contains the code that is currently executing.</summary>
	/// <returns>The assembly that contains the code that is currently executing. </returns>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern Assembly GetExecutingAssembly();

	/// <summary>Returns the <see cref="T:System.Reflection.Assembly" /> of the method that invoked the currently executing method.</summary>
	/// <returns>The Assembly object of the method that invoked the currently executing method.</returns>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern Assembly GetCallingAssembly();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern IntPtr InternalGetReferencedAssemblies(Assembly module);

	internal unsafe static AssemblyName[] GetReferencedAssemblies(Assembly module)
	{
		using SafeGPtrArrayHandle safeGPtrArrayHandle = new SafeGPtrArrayHandle(InternalGetReferencedAssemblies(module));
		int length = safeGPtrArrayHandle.Length;
		try
		{
			AssemblyName[] array = new AssemblyName[length];
			for (int i = 0; i < length; i++)
			{
				AssemblyName assemblyName = new AssemblyName();
				MonoAssemblyName* native = (MonoAssemblyName*)(void*)safeGPtrArrayHandle[i];
				assemblyName.FillName(native, null, addVersion: true, addPublickey: false, defaultToken: true, assemblyRef: true);
				array[i] = assemblyName;
			}
			return array;
		}
		finally
		{
			for (int j = 0; j < length; j++)
			{
				MonoAssemblyName* ptr = (MonoAssemblyName*)(void*)safeGPtrArrayHandle[j];
				RuntimeMarshal.FreeAssemblyName(ref *ptr, freeStruct: true);
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern bool GetManifestResourceInfoInternal(string name, ManifestResourceInfo info);

	/// <summary>Returns information about how the given resource has been persisted.</summary>
	/// <returns>An object that is populated with information about the resource's topology, or null if the resource is not found.</returns>
	/// <param name="resourceName">The case-sensitive name of the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="resourceName" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="resourceName" /> parameter is an empty string (""). </exception>
	public virtual ManifestResourceInfo GetManifestResourceInfo(string resourceName)
	{
		if (resourceName == null)
		{
			throw new ArgumentNullException("resourceName");
		}
		if (resourceName.Length == 0)
		{
			throw new ArgumentException("String cannot have zero length.");
		}
		ManifestResourceInfo manifestResourceInfo = new ManifestResourceInfo(null, null, (ResourceLocation)0);
		if (GetManifestResourceInfoInternal(resourceName, manifestResourceInfo))
		{
			return manifestResourceInfo;
		}
		return null;
	}

	internal virtual Module GetManifestModule()
	{
		return GetManifestModuleInternal();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern Module GetManifestModuleInternal();

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Determines whether this assembly and the specified object are equal.</summary>
	/// <returns>true if <paramref name="o" /> is equal to this instance; otherwise, false.</returns>
	/// <param name="o">The object to compare with this instance. </param>
	public override bool Equals(object o)
	{
		if (this == o)
		{
			return true;
		}
		if (o == null)
		{
			return false;
		}
		return ((Assembly)o)._mono_assembly == _mono_assembly;
	}

	internal void Resolve()
	{
		lock (this)
		{
			LoadAssemblyPermissions();
			Evidence evidence = new Evidence(UnprotectedGetEvidence());
			evidence.AddHost(new PermissionRequestEvidence(_minimum, _optional, _refuse));
			_granted = SecurityManager.ResolvePolicy(evidence, _minimum, _optional, _refuse, out _denied);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool LoadPermissions(Assembly a, ref IntPtr minimum, ref int minLength, ref IntPtr optional, ref int optLength, ref IntPtr refused, ref int refLength);

	private void LoadAssemblyPermissions()
	{
		IntPtr minimum = IntPtr.Zero;
		IntPtr optional = IntPtr.Zero;
		IntPtr refused = IntPtr.Zero;
		int minLength = 0;
		int optLength = 0;
		int refLength = 0;
		if (LoadPermissions(this, ref minimum, ref minLength, ref optional, ref optLength, ref refused, ref refLength))
		{
			if (minLength > 0)
			{
				byte[] array = new byte[minLength];
				Marshal.Copy(minimum, array, 0, minLength);
				_minimum = SecurityManager.Decode(array);
			}
			if (optLength > 0)
			{
				byte[] array2 = new byte[optLength];
				Marshal.Copy(optional, array2, 0, optLength);
				_optional = SecurityManager.Decode(array2);
			}
			if (refLength > 0)
			{
				byte[] array3 = new byte[refLength];
				Marshal.Copy(refused, array3, 0, refLength);
				_refuse = SecurityManager.Decode(array3);
			}
		}
	}

	private static Exception CreateNIE()
	{
		return new NotImplementedException("Derived classes must implement it");
	}

	/// <summary>Returns information about the attributes that have been applied to the current <see cref="T:System.Reflection.Assembly" />, expressed as <see cref="T:System.Reflection.CustomAttributeData" /> objects.</summary>
	/// <returns>A generic list of <see cref="T:System.Reflection.CustomAttributeData" /> objects representing data about the attributes that have been applied to the current assembly.</returns>
	public virtual IList<CustomAttributeData> GetCustomAttributesData()
	{
		return CustomAttributeData.GetCustomAttributes(this);
	}

	/// <summary>Gets the <see cref="T:System.Type" /> object with the specified name in the assembly instance, with the options of ignoring the case, and of throwing an exception if the type is not found.</summary>
	/// <returns>An object that represents the specified class.</returns>
	/// <param name="name">The full name of the type. </param>
	/// <param name="throwOnError">true to throw an exception if the type is not found; false to return null. </param>
	/// <param name="ignoreCase">true to ignore the case of the type name; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is invalid.-or- The length of <paramref name="name" /> exceeds 1024 characters. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="throwOnError" /> is true, and the type cannot be found.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="name" /> requires a dependent assembly that could not be found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="name" /> requires a dependent assembly that was found but could not be loaded.-or-The current assembly was loaded into the reflection-only context, and <paramref name="name" /> requires a dependent assembly that was not preloaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="name" /> requires a dependent assembly, but the file is not a valid assembly. -or-<paramref name="name" /> requires a dependent assembly which was compiled for a version of the runtime later than the currently loaded version.</exception>
	public virtual Type GetType(string name, bool throwOnError, bool ignoreCase)
	{
		throw CreateNIE();
	}

	/// <summary>Gets the specified module in this assembly.</summary>
	/// <returns>The module being requested, or null if the module is not found.</returns>
	/// <param name="name">The name of the module being requested. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="name" /> parameter is an empty string (""). </exception>
	/// <exception cref="T:System.IO.FileLoadException">A file that was found could not be loaded. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="name" /> was not found. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="name" /> is not a valid assembly. </exception>
	public virtual Module GetModule(string name)
	{
		throw CreateNIE();
	}

	/// <summary>Gets the <see cref="T:System.Reflection.AssemblyName" /> objects for all the assemblies referenced by this assembly.</summary>
	/// <returns>An array that contains the fully parsed display names of all the assemblies referenced by this assembly.</returns>
	public virtual AssemblyName[] GetReferencedAssemblies()
	{
		throw CreateNIE();
	}

	/// <summary>Gets all the modules that are part of this assembly, specifying whether to include resource modules.</summary>
	/// <returns>An array of modules.</returns>
	/// <param name="getResourceModules">true to include resource modules; otherwise, false. </param>
	public virtual Module[] GetModules(bool getResourceModules)
	{
		throw CreateNIE();
	}

	/// <summary>Gets all the loaded modules that are part of this assembly, specifying whether to include resource modules.</summary>
	/// <returns>An array of modules.</returns>
	/// <param name="getResourceModules">true to include resource modules; otherwise, false. </param>
	[MonoTODO("Always returns the same as GetModules")]
	public virtual Module[] GetLoadedModules(bool getResourceModules)
	{
		throw CreateNIE();
	}

	/// <summary>Gets the satellite assembly for the specified culture.</summary>
	/// <returns>The specified satellite assembly.</returns>
	/// <param name="culture">The specified culture. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The assembly cannot be found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">The satellite assembly with a matching file name was found, but the CultureInfo did not match the one specified. </exception>
	/// <exception cref="T:System.BadImageFormatException">The satellite assembly is not a valid assembly. </exception>
	public virtual Assembly GetSatelliteAssembly(CultureInfo culture)
	{
		throw CreateNIE();
	}

	/// <summary>Gets the specified version of the satellite assembly for the specified culture.</summary>
	/// <returns>The specified satellite assembly.</returns>
	/// <param name="culture">The specified culture. </param>
	/// <param name="version">The version of the satellite assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null. </exception>
	/// <exception cref="T:System.IO.FileLoadException">The satellite assembly with a matching file name was found, but the CultureInfo or the version did not match the one specified. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The assembly cannot be found. </exception>
	/// <exception cref="T:System.BadImageFormatException">The satellite assembly is not a valid assembly. </exception>
	public virtual Assembly GetSatelliteAssembly(CultureInfo culture, Version version)
	{
		throw CreateNIE();
	}

	/// <summary>Indicates whether two <see cref="T:System.Reflection.Assembly" /> objects are equal.</summary>
	/// <returns>true if <paramref name="left" /> is equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The assembly to compare to <paramref name="right" />. </param>
	/// <param name="right">The assembly to compare to <paramref name="left" />.</param>
	public static bool operator ==(Assembly left, Assembly right)
	{
		if ((object)left == right)
		{
			return true;
		}
		if (((object)left == null) ^ ((object)right == null))
		{
			return false;
		}
		return left.Equals(right);
	}

	/// <summary>Indicates whether two <see cref="T:System.Reflection.Assembly" /> objects are not equal.</summary>
	/// <returns>true if <paramref name="left" /> is not equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The assembly to compare to <paramref name="right" />.</param>
	/// <param name="right">The assembly to compare to <paramref name="left" />.</param>
	public static bool operator !=(Assembly left, Assembly right)
	{
		if ((object)left == right)
		{
			return false;
		}
		if (((object)left == null) ^ ((object)right == null))
		{
			return true;
		}
		return !left.Equals(right);
	}
}
