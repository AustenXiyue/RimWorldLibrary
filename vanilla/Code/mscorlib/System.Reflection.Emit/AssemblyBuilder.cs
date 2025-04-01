using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using Mono.Security;
using Unity;

namespace System.Reflection.Emit;

/// <summary>Defines and represents a dynamic assembly.</summary>
[StructLayout(LayoutKind.Sequential)]
[ComDefaultInterface(typeof(_AssemblyBuilder))]
[ClassInterface(ClassInterfaceType.None)]
[ComVisible(true)]
public sealed class AssemblyBuilder : Assembly, _AssemblyBuilder
{
	private UIntPtr dynamic_assembly;

	private MethodInfo entry_point;

	private ModuleBuilder[] modules;

	private string name;

	private string dir;

	private CustomAttributeBuilder[] cattrs;

	private MonoResource[] resources;

	private byte[] public_key;

	private string version;

	private string culture;

	private uint algid;

	private uint flags;

	private PEFileKinds pekind;

	private bool delay_sign;

	private uint access;

	private Module[] loaded_modules;

	private MonoWin32Resource[] win32_resources;

	private RefEmitPermissionSet[] permissions_minimum;

	private RefEmitPermissionSet[] permissions_optional;

	private RefEmitPermissionSet[] permissions_refused;

	private PortableExecutableKinds peKind;

	private ImageFileMachine machine;

	private bool corlib_internal;

	private Type[] type_forwarders;

	private byte[] pktoken;

	internal Type corlib_object_type;

	internal Type corlib_value_type;

	internal Type corlib_enum_type;

	internal Type corlib_void_type;

	private ArrayList resource_writers;

	private Win32VersionResource version_res;

	private bool created;

	private bool is_module_only;

	private StrongName sn;

	private NativeResourceType native_resource;

	private string versioninfo_culture;

	private const AssemblyBuilderAccess COMPILER_ACCESS = (AssemblyBuilderAccess)2048;

	private ModuleBuilder manifest_module;

	/// <summary>Gets the location of the assembly, as specified originally (such as in an <see cref="T:System.Reflection.AssemblyName" /> object).</summary>
	/// <returns>The location of the assembly, as specified originally.</returns>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public override string CodeBase
	{
		get
		{
			throw not_supported();
		}
	}

	/// <summary>Returns the entry point of this assembly.</summary>
	/// <returns>The entry point of this assembly.</returns>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public override MethodInfo EntryPoint => entry_point;

	/// <summary>Gets the location, in codebase format, of the loaded file that contains the manifest if it is not shadow-copied.</summary>
	/// <returns>The location of the loaded file that contains the manifest. If the loaded file has been shadow-copied, the Location is that of the file before being shadow-copied.</returns>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public override string Location
	{
		get
		{
			throw not_supported();
		}
	}

	/// <summary>Gets the version of the common language runtime that will be saved in the file containing the manifest.</summary>
	/// <returns>A string representing the common language runtime version.</returns>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override string ImageRuntimeVersion => base.ImageRuntimeVersion;

	/// <summary>Gets a value indicating whether the dynamic assembly is in the reflection-only context.</summary>
	/// <returns>true if the dynamic assembly is in the reflection-only context; otherwise, false.</returns>
	[MonoTODO]
	public override bool ReflectionOnly => base.ReflectionOnly;

	internal bool IsSave => access != 1;

	internal bool IsRun
	{
		get
		{
			if (access != 1 && access != 3)
			{
				return access == 9;
			}
			return true;
		}
	}

	internal string AssemblyDir => dir;

	internal bool IsModuleOnly
	{
		get
		{
			return is_module_only;
		}
		set
		{
			is_module_only = value;
		}
	}

	/// <summary>Gets the module in the current <see cref="T:System.Reflection.Emit.AssemblyBuilder" /> that contains the assembly manifest.</summary>
	/// <returns>The manifest module.</returns>
	public override Module ManifestModule => GetManifestModule();

	/// <summary>Gets a value that indicates whether the assembly was loaded from the global assembly cache.</summary>
	/// <returns>Always false.</returns>
	public override bool GlobalAssemblyCache => false;

	/// <summary>Gets a value that indicates that the current assembly is a dynamic assembly.</summary>
	/// <returns>Always true.</returns>
	public override bool IsDynamic => true;

	/// <summary>Gets the display name of the current dynamic assembly. </summary>
	/// <returns>The display name of the dynamic assembly.</returns>
	public override string FullName => base.FullName;

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void basic_init(AssemblyBuilder ab);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void UpdateNativeCustomAttributes(AssemblyBuilder ab);

	internal AssemblyBuilder(AssemblyName n, string directory, AssemblyBuilderAccess access, bool corlib_internal)
	{
		pekind = PEFileKinds.Dll;
		corlib_object_type = typeof(object);
		corlib_value_type = typeof(ValueType);
		corlib_enum_type = typeof(Enum);
		corlib_void_type = typeof(void);
		base._002Ector();
		if ((access & (AssemblyBuilderAccess)2048) != 0)
		{
			throw new NotImplementedException("COMPILER_ACCESS is no longer supperted, use a newer mcs.");
		}
		if (!Enum.IsDefined(typeof(AssemblyBuilderAccess), access))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Argument value {0} is not valid.", (int)access), "access");
		}
		name = n.Name;
		this.access = (uint)access;
		flags = (uint)n.Flags;
		if (IsSave && (directory == null || directory.Length == 0))
		{
			dir = Directory.GetCurrentDirectory();
		}
		else
		{
			dir = directory;
		}
		if (n.CultureInfo != null)
		{
			culture = n.CultureInfo.Name;
			versioninfo_culture = n.CultureInfo.Name;
		}
		Version version = n.Version;
		if (version != null)
		{
			this.version = version.ToString();
		}
		if (n.KeyPair != null)
		{
			sn = n.KeyPair.StrongName();
		}
		else
		{
			byte[] publicKey = n.GetPublicKey();
			if (publicKey != null && publicKey.Length != 0)
			{
				sn = new StrongName(publicKey);
			}
		}
		if (sn != null)
		{
			flags |= 1u;
		}
		this.corlib_internal = corlib_internal;
		if (sn != null)
		{
			pktoken = new byte[sn.PublicKeyToken.Length * 2];
			int num = 0;
			byte[] publicKeyToken = sn.PublicKeyToken;
			foreach (byte b in publicKeyToken)
			{
				string text = b.ToString("x2");
				pktoken[num++] = (byte)text[0];
				pktoken[num++] = (byte)text[1];
			}
		}
		basic_init(this);
	}

	/// <summary>Adds an existing resource file to this assembly.</summary>
	/// <param name="name">The logical name of the resource. </param>
	/// <param name="fileName">The physical file name (.resources file) to which the logical name is mapped. This should not include a path; the file must be in the same directory as the assembly to which it is added. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> has been previously defined.-or- There is another file in the assembly named <paramref name="fileName" />.-or- The length of <paramref name="name" /> is zero.-or- The length of <paramref name="fileName" /> is zero, or if <paramref name="fileName" /> includes a path. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="fileName" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file <paramref name="fileName" /> is not found. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void AddResourceFile(string name, string fileName)
	{
		AddResourceFile(name, fileName, ResourceAttributes.Public);
	}

	/// <summary>Adds an existing resource file to this assembly.</summary>
	/// <param name="name">The logical name of the resource. </param>
	/// <param name="fileName">The physical file name (.resources file) to which the logical name is mapped. This should not include a path; the file must be in the same directory as the assembly to which it is added. </param>
	/// <param name="attribute">The resource attributes. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> has been previously defined.-or- There is another file in the assembly named <paramref name="fileName" />.-or- The length of <paramref name="name" /> is zero or if the length of <paramref name="fileName" /> is zero.-or- <paramref name="fileName" /> includes a path. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="fileName" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">If the file <paramref name="fileName" /> is not found. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void AddResourceFile(string name, string fileName, ResourceAttributes attribute)
	{
		AddResourceFile(name, fileName, attribute, fileNeedsToExists: true);
	}

	private void AddResourceFile(string name, string fileName, ResourceAttributes attribute, bool fileNeedsToExists)
	{
		check_name_and_filename(name, fileName, fileNeedsToExists);
		if (dir != null)
		{
			fileName = Path.Combine(dir, fileName);
		}
		if (resources != null)
		{
			MonoResource[] destinationArray = new MonoResource[resources.Length + 1];
			Array.Copy(resources, destinationArray, resources.Length);
			resources = destinationArray;
		}
		else
		{
			resources = new MonoResource[1];
		}
		int num = resources.Length - 1;
		resources[num].name = name;
		resources[num].filename = fileName;
		resources[num].attrs = attribute;
	}

	internal void AddPermissionRequests(PermissionSet required, PermissionSet optional, PermissionSet refused)
	{
		if (created)
		{
			throw new InvalidOperationException("Assembly was already saved.");
		}
		_minimum = required;
		_optional = optional;
		_refuse = refused;
		if (required != null)
		{
			permissions_minimum = new RefEmitPermissionSet[1];
			permissions_minimum[0] = new RefEmitPermissionSet(SecurityAction.RequestMinimum, required.ToXml().ToString());
		}
		if (optional != null)
		{
			permissions_optional = new RefEmitPermissionSet[1];
			permissions_optional[0] = new RefEmitPermissionSet(SecurityAction.RequestOptional, optional.ToXml().ToString());
		}
		if (refused != null)
		{
			permissions_refused = new RefEmitPermissionSet[1];
			permissions_refused[0] = new RefEmitPermissionSet(SecurityAction.RequestRefuse, refused.ToXml().ToString());
		}
	}

	internal void EmbedResourceFile(string name, string fileName)
	{
		EmbedResourceFile(name, fileName, ResourceAttributes.Public);
	}

	private void EmbedResourceFile(string name, string fileName, ResourceAttributes attribute)
	{
		if (resources != null)
		{
			MonoResource[] destinationArray = new MonoResource[resources.Length + 1];
			Array.Copy(resources, destinationArray, resources.Length);
			resources = destinationArray;
		}
		else
		{
			resources = new MonoResource[1];
		}
		int num = resources.Length - 1;
		resources[num].name = name;
		resources[num].attrs = attribute;
		try
		{
			FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			long length = fileStream.Length;
			resources[num].data = new byte[length];
			fileStream.Read(resources[num].data, 0, (int)length);
			fileStream.Close();
		}
		catch
		{
		}
	}

	/// <summary>Defines a dynamic assembly that has the specified name and access rights.</summary>
	/// <returns>An object that represents the new assembly.</returns>
	/// <param name="name">The name of the assembly.</param>
	/// <param name="access">The access rights of the assembly.</param>
	public static AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return new AssemblyBuilder(name, null, access, corlib_internal: false);
	}

	/// <summary>Defines a new assembly that has the specified name, access rights, and attributes.</summary>
	/// <returns>An object that represents the new assembly.</returns>
	/// <param name="name">The name of the assembly.</param>
	/// <param name="access">The access rights of the assembly.</param>
	/// <param name="assemblyAttributes">A collection that contains the attributes of the assembly.</param>
	public static AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
	{
		AssemblyBuilder assemblyBuilder = DefineDynamicAssembly(name, access);
		foreach (CustomAttributeBuilder assemblyAttribute in assemblyAttributes)
		{
			assemblyBuilder.SetCustomAttribute(assemblyAttribute);
		}
		return assemblyBuilder;
	}

	/// <summary>Defines a named transient dynamic module in this assembly.</summary>
	/// <returns>A <see cref="T:System.Reflection.Emit.ModuleBuilder" /> representing the defined dynamic module.</returns>
	/// <param name="name">The name of the dynamic module. Must be less than 260 characters in length. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> begins with white space.-or- The length of <paramref name="name" /> is zero.-or- The length of <paramref name="name" /> is greater than or equal to 260. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ExecutionEngineException">The assembly for default symbol writer cannot be loaded.-or- The type that implements the default symbol writer interface cannot be found. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public ModuleBuilder DefineDynamicModule(string name)
	{
		return DefineDynamicModule(name, name, emitSymbolInfo: false, transient: true);
	}

	/// <summary>Defines a named transient dynamic module in this assembly and specifies whether symbol information should be emitted.</summary>
	/// <returns>A <see cref="T:System.Reflection.Emit.ModuleBuilder" /> representing the defined dynamic module.</returns>
	/// <param name="name">The name of the dynamic module. Must be less than 260 characters in length. </param>
	/// <param name="emitSymbolInfo">true if symbol information is to be emitted; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> begins with white space.-or- The length of <paramref name="name" /> is zero.-or- The length of <paramref name="name" /> is greater than or equal to 260. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.ExecutionEngineException">The assembly for default symbol writer cannot be loaded.-or- The type that implements the default symbol writer interface cannot be found. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public ModuleBuilder DefineDynamicModule(string name, bool emitSymbolInfo)
	{
		return DefineDynamicModule(name, name, emitSymbolInfo, transient: true);
	}

	/// <summary>Defines a persistable dynamic module with the given name that will be saved to the specified file. No symbol information is emitted.</summary>
	/// <returns>A <see cref="T:System.Reflection.Emit.ModuleBuilder" /> object representing the defined dynamic module.</returns>
	/// <param name="name">The name of the dynamic module. Must be less than 260 characters in length. </param>
	/// <param name="fileName">The name of the file to which the dynamic module should be saved. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="fileName" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="name" /> or <paramref name="fileName" /> is zero.-or- The length of <paramref name="name" /> is greater than or equal to 260.-or- <paramref name="fileName" /> contains a path specification (a directory component, for example).-or- There is a conflict with the name of another file that belongs to this assembly. </exception>
	/// <exception cref="T:System.InvalidOperationException">This assembly has been previously saved. </exception>
	/// <exception cref="T:System.NotSupportedException">This assembly was called on a dynamic assembly with <see cref="F:System.Reflection.Emit.AssemblyBuilderAccess.Run" /> attribute. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ExecutionEngineException">The assembly for default symbol writer cannot be loaded.-or- The type that implements the default symbol writer interface cannot be found. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public ModuleBuilder DefineDynamicModule(string name, string fileName)
	{
		return DefineDynamicModule(name, fileName, emitSymbolInfo: false, transient: false);
	}

	/// <summary>Defines a persistable dynamic module, specifying the module name, the name of the file to which the module will be saved, and whether symbol information should be emitted using the default symbol writer.</summary>
	/// <returns>A <see cref="T:System.Reflection.Emit.ModuleBuilder" /> object representing the defined dynamic module.</returns>
	/// <param name="name">The name of the dynamic module. Must be less than 260 characters in length. </param>
	/// <param name="fileName">The name of the file to which the dynamic module should be saved. </param>
	/// <param name="emitSymbolInfo">If true, symbolic information is written using the default symbol writer. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="fileName" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="name" /> or <paramref name="fileName" /> is zero.-or- The length of <paramref name="name" /> is greater than or equal to 260.-or- <paramref name="fileName" /> contains a path specification (a directory component, for example).-or- There is a conflict with the name of another file that belongs to this assembly. </exception>
	/// <exception cref="T:System.InvalidOperationException">This assembly has been previously saved. </exception>
	/// <exception cref="T:System.NotSupportedException">This assembly was called on a dynamic assembly with the <see cref="F:System.Reflection.Emit.AssemblyBuilderAccess.Run" /> attribute. </exception>
	/// <exception cref="T:System.ExecutionEngineException">The assembly for default symbol writer cannot be loaded.-or- The type that implements the default symbol writer interface cannot be found. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public ModuleBuilder DefineDynamicModule(string name, string fileName, bool emitSymbolInfo)
	{
		return DefineDynamicModule(name, fileName, emitSymbolInfo, transient: false);
	}

	private ModuleBuilder DefineDynamicModule(string name, string fileName, bool emitSymbolInfo, bool transient)
	{
		check_name_and_filename(name, fileName, fileNeedsToExists: false);
		if (!transient)
		{
			if (Path.GetExtension(fileName) == string.Empty)
			{
				throw new ArgumentException("Module file name '" + fileName + "' must have file extension.");
			}
			if (!IsSave)
			{
				throw new NotSupportedException("Persistable modules are not supported in a dynamic assembly created with AssemblyBuilderAccess.Run");
			}
			if (created)
			{
				throw new InvalidOperationException("Assembly was already saved.");
			}
		}
		ModuleBuilder moduleBuilder = new ModuleBuilder(this, name, fileName, emitSymbolInfo, transient);
		if (modules != null && is_module_only)
		{
			throw new InvalidOperationException("A module-only assembly can only contain one module.");
		}
		if (modules != null)
		{
			ModuleBuilder[] destinationArray = new ModuleBuilder[modules.Length + 1];
			Array.Copy(modules, destinationArray, modules.Length);
			modules = destinationArray;
		}
		else
		{
			modules = new ModuleBuilder[1];
		}
		modules[modules.Length - 1] = moduleBuilder;
		return moduleBuilder;
	}

	/// <summary>Defines a standalone managed resource for this assembly with the default public resource attribute.</summary>
	/// <returns>A <see cref="T:System.Resources.ResourceWriter" /> object for the specified resource.</returns>
	/// <param name="name">The logical name of the resource. </param>
	/// <param name="description">A textual description of the resource. </param>
	/// <param name="fileName">The physical file name (.resources file) to which the logical name is mapped. This should not include a path. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> has been previously defined.-or- There is another file in the assembly named <paramref name="fileName" />.-or- The length of <paramref name="name" /> is zero.-or- The length of <paramref name="fileName" /> is zero.-or- <paramref name="fileName" /> includes a path. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="fileName" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public IResourceWriter DefineResource(string name, string description, string fileName)
	{
		return DefineResource(name, description, fileName, ResourceAttributes.Public);
	}

	/// <summary>Defines a standalone managed resource for this assembly. Attributes can be specified for the managed resource.</summary>
	/// <returns>A <see cref="T:System.Resources.ResourceWriter" /> object for the specified resource.</returns>
	/// <param name="name">The logical name of the resource. </param>
	/// <param name="description">A textual description of the resource. </param>
	/// <param name="fileName">The physical file name (.resources file) to which the logical name is mapped. This should not include a path. </param>
	/// <param name="attribute">The resource attributes. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> has been previously defined or if there is another file in the assembly named <paramref name="fileName" />.-or- The length of <paramref name="name" /> is zero.-or- The length of <paramref name="fileName" /> is zero.-or- <paramref name="fileName" /> includes a path. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="fileName" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public IResourceWriter DefineResource(string name, string description, string fileName, ResourceAttributes attribute)
	{
		AddResourceFile(name, fileName, attribute, fileNeedsToExists: false);
		IResourceWriter resourceWriter = new ResourceWriter(fileName);
		if (resource_writers == null)
		{
			resource_writers = new ArrayList();
		}
		resource_writers.Add(resourceWriter);
		return resourceWriter;
	}

	private void AddUnmanagedResource(Win32Resource res)
	{
		MemoryStream memoryStream = new MemoryStream();
		res.WriteTo(memoryStream);
		if (win32_resources != null)
		{
			MonoWin32Resource[] destinationArray = new MonoWin32Resource[win32_resources.Length + 1];
			Array.Copy(win32_resources, destinationArray, win32_resources.Length);
			win32_resources = destinationArray;
		}
		else
		{
			win32_resources = new MonoWin32Resource[1];
		}
		win32_resources[win32_resources.Length - 1] = new MonoWin32Resource(res.Type.Id, res.Name.Id, res.Language, memoryStream.ToArray());
	}

	/// <summary>Defines an unmanaged resource for this assembly as an opaque blob of bytes.</summary>
	/// <param name="resource">The opaque blob of bytes representing the unmanaged resource. </param>
	/// <exception cref="T:System.ArgumentException">An unmanaged resource was previously defined. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="resource" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[MonoTODO("Not currently implemenented")]
	public void DefineUnmanagedResource(byte[] resource)
	{
		if (resource == null)
		{
			throw new ArgumentNullException("resource");
		}
		if (native_resource != 0)
		{
			throw new ArgumentException("Native resource has already been defined.");
		}
		native_resource = NativeResourceType.Unmanaged;
		throw new NotImplementedException();
	}

	/// <summary>Defines an unmanaged resource file for this assembly given the name of the resource file.</summary>
	/// <param name="resourceFileName">The name of the resource file. </param>
	/// <exception cref="T:System.ArgumentException">An unmanaged resource was previously defined.-or- The file <paramref name="resourceFileName" /> is not readable.-or- <paramref name="resourceFileName" /> is the empty string (""). </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="resourceFileName" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="resourceFileName" /> is not found.-or- <paramref name="resourceFileName" /> is a directory. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void DefineUnmanagedResource(string resourceFileName)
	{
		if (resourceFileName == null)
		{
			throw new ArgumentNullException("resourceFileName");
		}
		if (resourceFileName.Length == 0)
		{
			throw new ArgumentException("resourceFileName");
		}
		if (!File.Exists(resourceFileName) || Directory.Exists(resourceFileName))
		{
			throw new FileNotFoundException("File '" + resourceFileName + "' does not exist or is a directory.");
		}
		if (native_resource != 0)
		{
			throw new ArgumentException("Native resource has already been defined.");
		}
		native_resource = NativeResourceType.Unmanaged;
		using FileStream s = new FileStream(resourceFileName, FileMode.Open, FileAccess.Read);
		foreach (Win32EncodedResource item in new Win32ResFileReader(s).ReadResources())
		{
			if (item.Name.IsName || item.Type.IsName)
			{
				throw new InvalidOperationException("resource files with named resources or non-default resource types are not supported.");
			}
			AddUnmanagedResource(item);
		}
	}

	/// <summary>Defines an unmanaged version information resource using the information specified in the assembly's AssemblyName object and the assembly's custom attributes.</summary>
	/// <exception cref="T:System.ArgumentException">An unmanaged version information resource was previously defined.-or- The unmanaged version information is too large to persist. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public void DefineVersionInfoResource()
	{
		if (native_resource != 0)
		{
			throw new ArgumentException("Native resource has already been defined.");
		}
		native_resource = NativeResourceType.Assembly;
		version_res = new Win32VersionResource(1, 0, compilercontext: false);
	}

	/// <summary>Defines an unmanaged version information resource for this assembly with the given specifications.</summary>
	/// <param name="product">The name of the product with which this assembly is distributed. </param>
	/// <param name="productVersion">The version of the product with which this assembly is distributed. </param>
	/// <param name="company">The name of the company that produced this assembly. </param>
	/// <param name="copyright">Describes all copyright notices, trademarks, and registered trademarks that apply to this assembly. This should include the full text of all notices, legal symbols, copyright dates, trademark numbers, and so on. In English, this string should be in the format "Copyright Microsoft Corp. 1990-2001". </param>
	/// <param name="trademark">Describes all trademarks and registered trademarks that apply to this assembly. This should include the full text of all notices, legal symbols, trademark numbers, and so on. In English, this string should be in the format "Windows is a trademark of Microsoft Corporation". </param>
	/// <exception cref="T:System.ArgumentException">An unmanaged version information resource was previously defined.-or- The unmanaged version information is too large to persist. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public void DefineVersionInfoResource(string product, string productVersion, string company, string copyright, string trademark)
	{
		if (native_resource != 0)
		{
			throw new ArgumentException("Native resource has already been defined.");
		}
		native_resource = NativeResourceType.Explicit;
		version_res = new Win32VersionResource(1, 0, compilercontext: false);
		version_res.ProductName = ((product != null) ? product : " ");
		version_res.ProductVersion = ((productVersion != null) ? productVersion : " ");
		version_res.CompanyName = ((company != null) ? company : " ");
		version_res.LegalCopyright = ((copyright != null) ? copyright : " ");
		version_res.LegalTrademarks = ((trademark != null) ? trademark : " ");
	}

	private void DefineVersionInfoResourceImpl(string fileName)
	{
		if (versioninfo_culture != null)
		{
			version_res.FileLanguage = new CultureInfo(versioninfo_culture).LCID;
		}
		version_res.Version = ((version == null) ? "0.0.0.0" : version);
		if (cattrs != null)
		{
			switch (native_resource)
			{
			case NativeResourceType.Assembly:
			{
				CustomAttributeBuilder[] array = cattrs;
				foreach (CustomAttributeBuilder customAttributeBuilder2 in array)
				{
					switch (customAttributeBuilder2.Ctor.ReflectedType.FullName)
					{
					case "System.Reflection.AssemblyProductAttribute":
						version_res.ProductName = customAttributeBuilder2.string_arg();
						break;
					case "System.Reflection.AssemblyCompanyAttribute":
						version_res.CompanyName = customAttributeBuilder2.string_arg();
						break;
					case "System.Reflection.AssemblyCopyrightAttribute":
						version_res.LegalCopyright = customAttributeBuilder2.string_arg();
						break;
					case "System.Reflection.AssemblyTrademarkAttribute":
						version_res.LegalTrademarks = customAttributeBuilder2.string_arg();
						break;
					case "System.Reflection.AssemblyCultureAttribute":
						version_res.FileLanguage = new CultureInfo(customAttributeBuilder2.string_arg()).LCID;
						break;
					case "System.Reflection.AssemblyFileVersionAttribute":
						version_res.FileVersion = customAttributeBuilder2.string_arg();
						break;
					case "System.Reflection.AssemblyInformationalVersionAttribute":
						version_res.ProductVersion = customAttributeBuilder2.string_arg();
						break;
					case "System.Reflection.AssemblyTitleAttribute":
						version_res.FileDescription = customAttributeBuilder2.string_arg();
						break;
					case "System.Reflection.AssemblyDescriptionAttribute":
						version_res.Comments = customAttributeBuilder2.string_arg();
						break;
					}
				}
				break;
			}
			case NativeResourceType.Explicit:
			{
				CustomAttributeBuilder[] array = cattrs;
				foreach (CustomAttributeBuilder customAttributeBuilder in array)
				{
					string fullName = customAttributeBuilder.Ctor.ReflectedType.FullName;
					if (fullName == "System.Reflection.AssemblyCultureAttribute")
					{
						version_res.FileLanguage = new CultureInfo(customAttributeBuilder.string_arg()).LCID;
					}
					else if (fullName == "System.Reflection.AssemblyDescriptionAttribute")
					{
						version_res.Comments = customAttributeBuilder.string_arg();
					}
				}
				break;
			}
			}
		}
		version_res.OriginalFilename = fileName;
		version_res.InternalName = Path.GetFileNameWithoutExtension(fileName);
		AddUnmanagedResource(version_res);
	}

	/// <summary>Returns the dynamic module with the specified name.</summary>
	/// <returns>A ModuleBuilder object representing the requested dynamic module.</returns>
	/// <param name="name">The name of the requested dynamic module. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="name" /> is zero. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public ModuleBuilder GetDynamicModule(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("Empty name is not legal.", "name");
		}
		if (modules != null)
		{
			for (int i = 0; i < modules.Length; i++)
			{
				if (modules[i].name == name)
				{
					return modules[i];
				}
			}
		}
		return null;
	}

	/// <summary>Gets the exported types defined in this assembly.</summary>
	/// <returns>An array of <see cref="T:System.Type" /> containing the exported types defined in this assembly.</returns>
	/// <exception cref="T:System.NotSupportedException">This method is not implemented. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public override Type[] GetExportedTypes()
	{
		throw not_supported();
	}

	/// <summary>Gets a <see cref="T:System.IO.FileStream" /> for the specified file in the file table of the manifest of this assembly.</summary>
	/// <returns>A <see cref="T:System.IO.FileStream" /> for the specified file, or null, if the file is not found.</returns>
	/// <param name="name">The name of the specified file. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public override FileStream GetFile(string name)
	{
		throw not_supported();
	}

	/// <summary>Gets the files in the file table of an assembly manifest, specifying whether to include resource modules.</summary>
	/// <returns>An array of <see cref="T:System.IO.FileStream" /> objects.</returns>
	/// <param name="getResourceModules">true to include resource modules; otherwise, false. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public override FileStream[] GetFiles(bool getResourceModules)
	{
		throw not_supported();
	}

	internal override Module[] GetModulesInternal()
	{
		if (modules == null)
		{
			return new Module[0];
		}
		return (Module[])modules.Clone();
	}

	internal override Type[] GetTypes(bool exportedOnly)
	{
		Type[] array = null;
		if (modules != null)
		{
			for (int i = 0; i < modules.Length; i++)
			{
				Type[] types = modules[i].GetTypes();
				if (array == null)
				{
					array = types;
					continue;
				}
				Type[] destinationArray = new Type[array.Length + types.Length];
				Array.Copy(array, 0, destinationArray, 0, array.Length);
				Array.Copy(types, 0, destinationArray, array.Length, types.Length);
			}
		}
		if (loaded_modules != null)
		{
			for (int j = 0; j < loaded_modules.Length; j++)
			{
				Type[] types2 = loaded_modules[j].GetTypes();
				if (array == null)
				{
					array = types2;
					continue;
				}
				Type[] destinationArray2 = new Type[array.Length + types2.Length];
				Array.Copy(array, 0, destinationArray2, 0, array.Length);
				Array.Copy(types2, 0, destinationArray2, array.Length, types2.Length);
			}
		}
		if (array != null)
		{
			List<Exception> list = null;
			Type[] array2 = array;
			foreach (Type type in array2)
			{
				if (type is TypeBuilder)
				{
					if (list == null)
					{
						list = new List<Exception>();
					}
					list.Add(new TypeLoadException($"Type '{type.FullName}' is not finished"));
				}
			}
			if (list != null)
			{
				throw new ReflectionTypeLoadException(new Type[list.Count], list.ToArray());
			}
		}
		if (array != null)
		{
			return array;
		}
		return Type.EmptyTypes;
	}

	/// <summary>Returns information about how the given resource has been persisted.</summary>
	/// <returns>
	///   <see cref="T:System.Reflection.ManifestResourceInfo" /> populated with information about the resource's topology, or null if the resource is not found.</returns>
	/// <param name="resourceName">The name of the resource. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
	{
		throw not_supported();
	}

	/// <summary>Loads the specified manifest resource from this assembly.</summary>
	/// <returns>An array of type String containing the names of all the resources.</returns>
	/// <exception cref="T:System.NotSupportedException">This method is not supported on a dynamic assembly. To get the manifest resource names, use <see cref="M:System.Reflection.Assembly.GetManifestResourceNames" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public override string[] GetManifestResourceNames()
	{
		throw not_supported();
	}

	/// <summary>Loads the specified manifest resource from this assembly.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> representing this manifest resource.</returns>
	/// <param name="name">The name of the manifest resource being requested. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public override Stream GetManifestResourceStream(string name)
	{
		throw not_supported();
	}

	/// <summary>Loads the specified manifest resource, scoped by the namespace of the specified type, from this assembly.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> representing this manifest resource.</returns>
	/// <param name="type">The type whose namespace is used to scope the manifest resource name. </param>
	/// <param name="name">The name of the manifest resource being requested. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public override Stream GetManifestResourceStream(Type type, string name)
	{
		throw not_supported();
	}

	internal override Module GetManifestModule()
	{
		if (manifest_module == null)
		{
			manifest_module = DefineDynamicModule("Default Dynamic Module");
		}
		return manifest_module;
	}

	/// <summary>Saves this dynamic assembly to disk, specifying the nature of code in the assembly's executables and the target platform.</summary>
	/// <param name="assemblyFileName">The file name of the assembly.</param>
	/// <param name="portableExecutableKind">A bitwise combination of the <see cref="T:System.Reflection.PortableExecutableKinds" /> values that specifies the nature of the code.</param>
	/// <param name="imageFileMachine">One of the <see cref="T:System.Reflection.ImageFileMachine" /> values that specifies the target platform.</param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="assemblyFileName" /> is 0.-or- There are two or more modules resource files in the assembly with the same name.-or- The target directory of the assembly is invalid.-or- <paramref name="assemblyFileName" /> is not a simple file name (for example, has a directory or drive component), or more than one unmanaged resource, including a version information resources, was defined in this assembly.-or- The CultureInfo string in <see cref="T:System.Reflection.AssemblyCultureAttribute" /> is not a valid string and <see cref="M:System.Reflection.Emit.AssemblyBuilder.DefineVersionInfoResource(System.String,System.String,System.String,System.String,System.String)" /> was called prior to calling this method. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyFileName" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">This assembly has been saved before.-or- This assembly has access Run<see cref="T:System.Reflection.Emit.AssemblyBuilderAccess" /></exception>
	/// <exception cref="T:System.IO.IOException">An output error occurs during the save. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" /> has not been called for any of the types in the modules of the assembly to be written to disk. </exception>
	[MonoLimitation("No support for PE32+ assemblies for AMD64 and IA64")]
	public void Save(string assemblyFileName, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
	{
		peKind = portableExecutableKind;
		machine = imageFileMachine;
		if ((peKind & PortableExecutableKinds.PE32Plus) != 0 || (peKind & PortableExecutableKinds.Unmanaged32Bit) != 0)
		{
			throw new NotImplementedException(peKind.ToString());
		}
		if (machine == ImageFileMachine.IA64 || machine == ImageFileMachine.AMD64)
		{
			throw new NotImplementedException(machine.ToString());
		}
		if (resource_writers != null)
		{
			foreach (IResourceWriter resource_writer in resource_writers)
			{
				resource_writer.Generate();
				resource_writer.Close();
			}
		}
		ModuleBuilder moduleBuilder = null;
		ModuleBuilder[] array;
		if (modules != null)
		{
			array = modules;
			foreach (ModuleBuilder moduleBuilder2 in array)
			{
				if (moduleBuilder2.FullyQualifiedName == assemblyFileName)
				{
					moduleBuilder = moduleBuilder2;
				}
			}
		}
		if (moduleBuilder == null)
		{
			moduleBuilder = DefineDynamicModule("RefEmit_OnDiskManifestModule", assemblyFileName);
		}
		if (!is_module_only)
		{
			moduleBuilder.IsMain = true;
		}
		if (entry_point != null && entry_point.DeclaringType.Module != moduleBuilder)
		{
			Type[] array2 = ((entry_point.GetParametersCount() != 1) ? Type.EmptyTypes : new Type[1] { typeof(string) });
			MethodBuilder methodBuilder = moduleBuilder.DefineGlobalMethod("__EntryPoint$", MethodAttributes.Static, entry_point.ReturnType, array2);
			ILGenerator iLGenerator = methodBuilder.GetILGenerator();
			if (array2.Length == 1)
			{
				iLGenerator.Emit(OpCodes.Ldarg_0);
			}
			iLGenerator.Emit(OpCodes.Tailcall);
			iLGenerator.Emit(OpCodes.Call, entry_point);
			iLGenerator.Emit(OpCodes.Ret);
			entry_point = methodBuilder;
		}
		if (version_res != null)
		{
			DefineVersionInfoResourceImpl(assemblyFileName);
		}
		if (sn != null)
		{
			public_key = sn.PublicKey;
		}
		array = modules;
		foreach (ModuleBuilder moduleBuilder3 in array)
		{
			if (moduleBuilder3 != moduleBuilder)
			{
				moduleBuilder3.Save();
			}
		}
		moduleBuilder.Save();
		if (sn != null && sn.CanSign)
		{
			sn.Sign(Path.Combine(AssemblyDir, assemblyFileName));
		}
		created = true;
	}

	/// <summary>Saves this dynamic assembly to disk.</summary>
	/// <param name="assemblyFileName">The file name of the assembly. </param>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="assemblyFileName" /> is 0.-or- There are two or more modules resource files in the assembly with the same name.-or- The target directory of the assembly is invalid.-or- <paramref name="assemblyFileName" /> is not a simple file name (for example, has a directory or drive component), or more than one unmanaged resource, including a version information resource, was defined in this assembly.-or- The CultureInfo string in <see cref="T:System.Reflection.AssemblyCultureAttribute" /> is not a valid string and <see cref="M:System.Reflection.Emit.AssemblyBuilder.DefineVersionInfoResource(System.String,System.String,System.String,System.String,System.String)" /> was called prior to calling this method. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyFileName" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">This assembly has been saved before.-or- This assembly has access Run<see cref="T:System.Reflection.Emit.AssemblyBuilderAccess" /></exception>
	/// <exception cref="T:System.IO.IOException">An output error occurs during the save. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" /> has not been called for any of the types in the modules of the assembly to be written to disk. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Save(string assemblyFileName)
	{
		Save(assemblyFileName, PortableExecutableKinds.ILOnly, ImageFileMachine.I386);
	}

	/// <summary>Sets the entry point for this dynamic assembly, assuming that a console application is being built.</summary>
	/// <param name="entryMethod">A reference to the method that represents the entry point for this dynamic assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="entryMethod" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="entryMethod" /> is not contained within this assembly. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public void SetEntryPoint(MethodInfo entryMethod)
	{
		SetEntryPoint(entryMethod, PEFileKinds.ConsoleApplication);
	}

	/// <summary>Sets the entry point for this assembly and defines the type of the portable executable (PE file) being built.</summary>
	/// <param name="entryMethod">A reference to the method that represents the entry point for this dynamic assembly. </param>
	/// <param name="fileKind">The type of the assembly executable being built. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="entryMethod" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="entryMethod" /> is not contained within this assembly. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public void SetEntryPoint(MethodInfo entryMethod, PEFileKinds fileKind)
	{
		if (entryMethod == null)
		{
			throw new ArgumentNullException("entryMethod");
		}
		if (entryMethod.DeclaringType.Assembly != this)
		{
			throw new InvalidOperationException("Entry method is not defined in the same assembly.");
		}
		entry_point = entryMethod;
		pekind = fileKind;
	}

	/// <summary>Set a custom attribute on this assembly using a custom attribute builder.</summary>
	/// <param name="customBuilder">An instance of a helper class to define the custom attribute. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="con" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
	{
		if (customBuilder == null)
		{
			throw new ArgumentNullException("customBuilder");
		}
		if (cattrs != null)
		{
			CustomAttributeBuilder[] array = new CustomAttributeBuilder[cattrs.Length + 1];
			cattrs.CopyTo(array, 0);
			array[cattrs.Length] = customBuilder;
			cattrs = array;
		}
		else
		{
			cattrs = new CustomAttributeBuilder[1];
			cattrs[0] = customBuilder;
		}
		if (customBuilder.Ctor != null && customBuilder.Ctor.DeclaringType == typeof(RuntimeCompatibilityAttribute))
		{
			UpdateNativeCustomAttributes(this);
		}
	}

	/// <summary>Set a custom attribute on this assembly using a specified custom attribute blob.</summary>
	/// <param name="con">The constructor for the custom attribute. </param>
	/// <param name="binaryAttribute">A byte blob representing the attributes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="con" /> or <paramref name="binaryAttribute" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="con" /> is not a RuntimeConstructorInfo.</exception>
	[ComVisible(true)]
	public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
	{
		if (con == null)
		{
			throw new ArgumentNullException("con");
		}
		if (binaryAttribute == null)
		{
			throw new ArgumentNullException("binaryAttribute");
		}
		SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
	}

	private Exception not_supported()
	{
		return new NotSupportedException("The invoked member is not supported in a dynamic module.");
	}

	private void check_name_and_filename(string name, string fileName, bool fileNeedsToExists)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("Empty name is not legal.", "name");
		}
		if (fileName.Length == 0)
		{
			throw new ArgumentException("Empty file name is not legal.", "fileName");
		}
		if (Path.GetFileName(fileName) != fileName)
		{
			throw new ArgumentException("fileName '" + fileName + "' must not include a path.", "fileName");
		}
		string text = fileName;
		if (dir != null)
		{
			text = Path.Combine(dir, fileName);
		}
		if (fileNeedsToExists && !File.Exists(text))
		{
			throw new FileNotFoundException("Could not find file '" + fileName + "'");
		}
		if (resources != null)
		{
			for (int i = 0; i < resources.Length; i++)
			{
				if (resources[i].filename == text)
				{
					throw new ArgumentException("Duplicate file name '" + fileName + "'");
				}
				if (resources[i].name == name)
				{
					throw new ArgumentException("Duplicate name '" + name + "'");
				}
			}
		}
		if (modules == null)
		{
			return;
		}
		for (int j = 0; j < modules.Length; j++)
		{
			if (!modules[j].IsTransient() && modules[j].FileName == fileName)
			{
				throw new ArgumentException("Duplicate file name '" + fileName + "'");
			}
			if (modules[j].Name == name)
			{
				throw new ArgumentException("Duplicate name '" + name + "'");
			}
		}
	}

	private string create_assembly_version(string version)
	{
		string[] array = version.Split('.');
		int[] array2 = new int[4];
		if (array.Length < 0 || array.Length > 4)
		{
			throw new ArgumentException("The version specified '" + version + "' is invalid");
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == "*")
			{
				DateTime now = DateTime.Now;
				switch (i)
				{
				case 2:
					array2[2] = (now - new DateTime(2000, 1, 1)).Days;
					if (array.Length == 3)
					{
						array2[3] = (now.Second + now.Minute * 60 + now.Hour * 3600) / 2;
					}
					break;
				case 3:
					array2[3] = (now.Second + now.Minute * 60 + now.Hour * 3600) / 2;
					break;
				default:
					throw new ArgumentException("The version specified '" + version + "' is invalid");
				}
			}
			else
			{
				try
				{
					array2[i] = int.Parse(array[i]);
				}
				catch (FormatException)
				{
					throw new ArgumentException("The version specified '" + version + "' is invalid");
				}
			}
		}
		return array2[0] + "." + array2[1] + "." + array2[2] + "." + array2[3];
	}

	private string GetCultureString(string str)
	{
		if (!(str == "neutral"))
		{
			return str;
		}
		return string.Empty;
	}

	internal Type MakeGenericType(Type gtd, Type[] typeArguments)
	{
		return new TypeBuilderInstantiation(gtd, typeArguments);
	}

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array which receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _AssemblyBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the type information for an object, which can then be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">Receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _AssemblyBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _AssemblyBuilder.GetTypeInfoCount(out uint pcTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Provides access to properties and methods exposed by an object.</summary>
	/// <param name="dispIdMember">Identifies the member.</param>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="lcid">The locale context in which to interpret arguments.</param>
	/// <param name="wFlags">Flags describing the context of the call.</param>
	/// <param name="pDispParams">Pointer to a structure containing an array of arguments, an array of argument DISPIDs for named arguments, and counts for the number of elements in the arrays.</param>
	/// <param name="pVarResult">Pointer to the location where the result is to be stored.</param>
	/// <param name="pExcepInfo">Pointer to a structure that contains exception information.</param>
	/// <param name="puArgErr">The index of the first argument that has an error.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _AssemblyBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets the specified type from the types that have been defined and created in the current <see cref="T:System.Reflection.Emit.AssemblyBuilder" />.</summary>
	/// <returns>The specified type, or null if the type is not found or has not been created yet. </returns>
	/// <param name="name">The name of the type to search for.</param>
	/// <param name="throwOnError">true to throw an exception if the type is not found; otherwise, false.</param>
	/// <param name="ignoreCase">true to ignore the case of the type name when searching; otherwise, false.</param>
	public override Type GetType(string name, bool throwOnError, bool ignoreCase)
	{
		if (name == null)
		{
			throw new ArgumentNullException(name);
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("name", "Name cannot be empty");
		}
		Type type = InternalGetType(null, name, throwOnError, ignoreCase);
		if (type is TypeBuilder)
		{
			if (throwOnError)
			{
				throw new TypeLoadException($"Could not load type '{name}' from assembly '{this.name}'");
			}
			return null;
		}
		return type;
	}

	/// <summary>Gets the specified module in this assembly.</summary>
	/// <returns>The module being requested, or null if the module is not found.</returns>
	/// <param name="name">The name of the requested module.</param>
	public override Module GetModule(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("Name can't be empty");
		}
		if (modules == null)
		{
			return null;
		}
		ModuleBuilder[] array = modules;
		foreach (Module module in array)
		{
			if (module.ScopeName == name)
			{
				return module;
			}
		}
		return null;
	}

	/// <summary>Gets all the modules that are part of this assembly, and optionally includes resource modules.</summary>
	/// <returns>The modules that are part of this assembly.</returns>
	/// <param name="getResourceModules">true to include resource modules; otherwise, false.</param>
	public override Module[] GetModules(bool getResourceModules)
	{
		Module[] modulesInternal = GetModulesInternal();
		if (!getResourceModules)
		{
			List<Module> list = new List<Module>(modulesInternal.Length);
			Module[] array = modulesInternal;
			foreach (Module module in array)
			{
				if (!module.IsResource())
				{
					list.Add(module);
				}
			}
			return list.ToArray();
		}
		return modulesInternal;
	}

	/// <summary>Gets the <see cref="T:System.Reflection.AssemblyName" /> that was specified when the current dynamic assembly was created, and sets the code base as specified.</summary>
	/// <returns>The name of the dynamic assembly.</returns>
	/// <param name="copiedName">true to set the code base to the location of the assembly after it is shadow-copied; false to set the code base to the original location.</param>
	public override AssemblyName GetName(bool copiedName)
	{
		AssemblyName assemblyName = AssemblyName.Create(this, fillCodebase: false);
		if (sn != null)
		{
			assemblyName.SetPublicKey(sn.PublicKey);
			assemblyName.SetPublicKeyToken(sn.PublicKeyToken);
		}
		return assemblyName;
	}

	/// <summary>Gets an incomplete list of <see cref="T:System.Reflection.AssemblyName" /> objects for the assemblies that are referenced by this <see cref="T:System.Reflection.Emit.AssemblyBuilder" />. </summary>
	/// <returns>An array of assembly names for the referenced assemblies. This array is not a complete list.</returns>
	[MonoTODO("This always returns an empty array")]
	public override AssemblyName[] GetReferencedAssemblies()
	{
		return Assembly.GetReferencedAssemblies(this);
	}

	/// <summary>Returns all the loaded modules that are part of this assembly, and optionally includes resource modules. </summary>
	/// <returns>The loaded modules that are part of this assembly.</returns>
	/// <param name="getResourceModules">true to include resource modules; otherwise, false.</param>
	public override Module[] GetLoadedModules(bool getResourceModules)
	{
		return GetModules(getResourceModules);
	}

	/// <summary>Gets the satellite assembly for the specified culture.</summary>
	/// <returns>The specified satellite assembly.</returns>
	/// <param name="culture">The specified culture. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The assembly cannot be found. </exception>
	/// <exception cref="T:System.IO.FileLoadException">The satellite assembly with a matching file name was found, but the CultureInfo did not match the one specified. </exception>
	/// <exception cref="T:System.BadImageFormatException">The satellite assembly is not a valid assembly. </exception>
	public override Assembly GetSatelliteAssembly(CultureInfo culture)
	{
		return GetSatelliteAssembly(culture, null, throwOnError: true);
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
	public override Assembly GetSatelliteAssembly(CultureInfo culture, Version version)
	{
		return GetSatelliteAssembly(culture, version, throwOnError: true);
	}

	/// <summary>Returns a value that indicates whether this instance is equal to the specified object.</summary>
	/// <returns>true if <paramref name="obj" /> equals the type and value of this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance, or null.</param>
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Returns a value that indicates whether one or more instances of the specified attribute type is applied to this member.</summary>
	/// <returns>true if one or more instances of <paramref name="attributeType" /> is applied to this dynamic assembly; otherwise, false.</returns>
	/// <param name="attributeType">The type of attribute to test for.</param>
	/// <param name="inherit">This argument is ignored for objects of this type.</param>
	public override bool IsDefined(Type attributeType, bool inherit)
	{
		return base.IsDefined(attributeType, inherit);
	}

	/// <summary>Returns all the custom attributes that have been applied to the current <see cref="T:System.Reflection.Emit.AssemblyBuilder" />.</summary>
	/// <returns>An array that contains the custom attributes; the array is empty if there are no attributes.</returns>
	/// <param name="inherit">This argument is ignored for objects of this type.</param>
	public override object[] GetCustomAttributes(bool inherit)
	{
		return base.GetCustomAttributes(inherit);
	}

	/// <summary>Returns all the custom attributes that have been applied to the current <see cref="T:System.Reflection.Emit.AssemblyBuilder" />, and that derive from a specified attribute type.</summary>
	/// <returns>An array that contains the custom attributes that are derived at any level from <paramref name="attributeType" />; the array is empty if there are no such attributes.</returns>
	/// <param name="attributeType">The base type from which attributes derive.</param>
	/// <param name="inherit">This argument is ignored for objects of this type.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not a <see cref="T:System.Type" /> object supplied by the runtime. For example, <paramref name="attributeType" /> is a <see cref="T:System.Reflection.Emit.TypeBuilder" /> object.</exception>
	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		return base.GetCustomAttributes(attributeType, inherit);
	}

	internal AssemblyBuilder()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
