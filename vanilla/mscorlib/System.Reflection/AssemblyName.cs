using System.Configuration.Assemblies;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using Mono;
using Mono.Security;
using Mono.Security.Cryptography;

namespace System.Reflection;

/// <summary>Describes an assembly's unique identity in full.</summary>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(_AssemblyName))]
public sealed class AssemblyName : ICloneable, ISerializable, IDeserializationCallback, _AssemblyName
{
	private string name;

	private string codebase;

	private int major;

	private int minor;

	private int build;

	private int revision;

	private CultureInfo cultureinfo;

	private AssemblyNameFlags flags;

	private AssemblyHashAlgorithm hashalg;

	private StrongNameKeyPair keypair;

	private byte[] publicKey;

	private byte[] keyToken;

	private AssemblyVersionCompatibility versioncompat;

	private Version version;

	private ProcessorArchitecture processor_architecture;

	private AssemblyContentType contentType;

	/// <summary>Gets or sets a value that identifies the processor and bits-per-word of the platform targeted by an executable.</summary>
	/// <returns>One of the enumeration values that identifies the processor and bits-per-word of the platform targeted by an executable.</returns>
	[MonoLimitation("Not used, as the values are too limited;  Mono supports more")]
	public ProcessorArchitecture ProcessorArchitecture
	{
		get
		{
			return processor_architecture;
		}
		set
		{
			processor_architecture = value;
		}
	}

	/// <summary>Gets or sets the simple name of the assembly. This is usually, but not necessarily, the file name of the manifest file of the assembly, minus its extension.</summary>
	/// <returns>The simple name of the assembly.</returns>
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	/// <summary>Gets or sets the location of the assembly as a URL.</summary>
	/// <returns>A string that is the URL location of the assembly. </returns>
	public string CodeBase
	{
		get
		{
			return codebase;
		}
		set
		{
			codebase = value;
		}
	}

	/// <summary>Gets the URI, including escape characters, that represents the codebase.</summary>
	/// <returns>A URI with escape characters.</returns>
	public string EscapedCodeBase
	{
		get
		{
			if (codebase == null)
			{
				return null;
			}
			return Uri.EscapeString(codebase, escapeReserved: false, escapeHex: true, escapeBrackets: true);
		}
	}

	/// <summary>Gets or sets the culture supported by the assembly.</summary>
	/// <returns>An object that represents the culture supported by the assembly.</returns>
	public CultureInfo CultureInfo
	{
		get
		{
			return cultureinfo;
		}
		set
		{
			cultureinfo = value;
		}
	}

	/// <summary>Gets or sets the attributes of the assembly.</summary>
	/// <returns>A value that represents the attributes of the assembly.</returns>
	public AssemblyNameFlags Flags
	{
		get
		{
			return flags;
		}
		set
		{
			flags = value;
		}
	}

	/// <summary>Gets the full name of the assembly, also known as the display name.</summary>
	/// <returns>A string that is the full name of the assembly, also known as the display name.</returns>
	public string FullName
	{
		get
		{
			if (name == null)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (char.IsWhiteSpace(name[0]))
			{
				stringBuilder.Append("\"" + name + "\"");
			}
			else
			{
				stringBuilder.Append(name);
			}
			if (Version != null)
			{
				stringBuilder.Append(", Version=");
				stringBuilder.Append(Version.ToString());
			}
			if (cultureinfo != null)
			{
				stringBuilder.Append(", Culture=");
				if (cultureinfo.LCID == CultureInfo.InvariantCulture.LCID)
				{
					stringBuilder.Append("neutral");
				}
				else
				{
					stringBuilder.Append(cultureinfo.Name);
				}
			}
			byte[] array = InternalGetPublicKeyToken();
			if (array != null)
			{
				if (array.Length == 0)
				{
					stringBuilder.Append(", PublicKeyToken=null");
				}
				else
				{
					stringBuilder.Append(", PublicKeyToken=");
					for (int i = 0; i < array.Length; i++)
					{
						stringBuilder.Append(array[i].ToString("x2"));
					}
				}
			}
			if ((Flags & AssemblyNameFlags.Retargetable) != 0)
			{
				stringBuilder.Append(", Retargetable=Yes");
			}
			return stringBuilder.ToString();
		}
	}

	/// <summary>Gets or sets the hash algorithm used by the assembly manifest.</summary>
	/// <returns>The hash algorithm used by the assembly manifest.</returns>
	public AssemblyHashAlgorithm HashAlgorithm
	{
		get
		{
			return hashalg;
		}
		set
		{
			hashalg = value;
		}
	}

	/// <summary>Gets or sets the public and private cryptographic key pair that is used to create a strong name signature for the assembly.</summary>
	/// <returns>The public and private cryptographic key pair to be used to create a strong name for the assembly.</returns>
	public StrongNameKeyPair KeyPair
	{
		get
		{
			return keypair;
		}
		set
		{
			keypair = value;
		}
	}

	/// <summary>Gets or sets the major, minor, build, and revision numbers of the assembly.</summary>
	/// <returns>An object that represents the major, minor, build, and revision numbers of the assembly.</returns>
	public Version Version
	{
		get
		{
			return version;
		}
		set
		{
			version = value;
			if (value == null)
			{
				major = (minor = (build = (revision = 0)));
				return;
			}
			major = value.Major;
			minor = value.Minor;
			build = value.Build;
			revision = value.Revision;
		}
	}

	/// <summary>Gets or sets the information related to the assembly's compatibility with other assemblies.</summary>
	/// <returns>A value that represents information about the assembly's compatibility with other assemblies.</returns>
	public AssemblyVersionCompatibility VersionCompatibility
	{
		get
		{
			return versioncompat;
		}
		set
		{
			versioncompat = value;
		}
	}

	private bool IsPublicKeyValid
	{
		get
		{
			if (publicKey.Length == 16)
			{
				int num = 0;
				int num2 = 0;
				while (num < publicKey.Length)
				{
					num2 += publicKey[num++];
				}
				if (num2 == 4)
				{
					return true;
				}
			}
			switch (publicKey[0])
			{
			case 0:
				if (publicKey.Length > 12 && publicKey[12] == 6)
				{
					try
					{
						CryptoConvert.FromCapiPublicKeyBlob(publicKey, 12);
						return true;
					}
					catch (CryptographicException)
					{
					}
				}
				break;
			case 6:
				try
				{
					CryptoConvert.FromCapiPublicKeyBlob(publicKey);
					return true;
				}
				catch (CryptographicException)
				{
				}
				break;
			}
			return false;
		}
	}

	/// <summary>Gets or sets the name of the culture associated with the assembly.</summary>
	/// <returns>The culture name.</returns>
	public string CultureName
	{
		get
		{
			if (cultureinfo != null)
			{
				return cultureinfo.Name;
			}
			return null;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets a value that indicates what type of content the assembly contains.</summary>
	/// <returns>A value that indicates what type of content the assembly contains.</returns>
	[ComVisible(false)]
	public AssemblyContentType ContentType
	{
		get
		{
			return contentType;
		}
		set
		{
			contentType = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyName" /> class.</summary>
	public AssemblyName()
	{
		versioncompat = AssemblyVersionCompatibility.SameMachine;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool ParseAssemblyName(IntPtr name, out MonoAssemblyName aname, out bool is_version_definited, out bool is_token_defined);

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyName" /> class with the specified display name.</summary>
	/// <param name="assemblyName">The display name of the assembly, as returned by the <see cref="P:System.Reflection.AssemblyName.FullName" /> property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyName" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="assemblyName" /> is a zero length string. </exception>
	/// <exception cref="T:System.IO.FileLoadException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.IO.IOException" />, instead.The referenced assembly could not be found, or could not be loaded.</exception>
	public unsafe AssemblyName(string assemblyName)
	{
		if (assemblyName == null)
		{
			throw new ArgumentNullException("assemblyName");
		}
		if (assemblyName.Length < 1)
		{
			throw new ArgumentException("assemblyName cannot have zero length.");
		}
		using SafeStringMarshal safeStringMarshal = RuntimeMarshal.MarshalString(assemblyName);
		if (!ParseAssemblyName(safeStringMarshal.Value, out var aname, out var is_version_definited, out var is_token_defined))
		{
			throw new FileLoadException("The assembly name is invalid.");
		}
		try
		{
			FillName(&aname, null, is_version_definited, addPublickey: false, is_token_defined, assemblyRef: false);
		}
		finally
		{
			RuntimeMarshal.FreeAssemblyName(ref aname, freeStruct: false);
		}
	}

	internal AssemblyName(SerializationInfo si, StreamingContext sc)
	{
		name = si.GetString("_Name");
		codebase = si.GetString("_CodeBase");
		version = (Version)si.GetValue("_Version", typeof(Version));
		publicKey = (byte[])si.GetValue("_PublicKey", typeof(byte[]));
		keyToken = (byte[])si.GetValue("_PublicKeyToken", typeof(byte[]));
		hashalg = (AssemblyHashAlgorithm)si.GetValue("_HashAlgorithm", typeof(AssemblyHashAlgorithm));
		keypair = (StrongNameKeyPair)si.GetValue("_StrongNameKeyPair", typeof(StrongNameKeyPair));
		versioncompat = (AssemblyVersionCompatibility)si.GetValue("_VersionCompatibility", typeof(AssemblyVersionCompatibility));
		flags = (AssemblyNameFlags)si.GetValue("_Flags", typeof(AssemblyNameFlags));
		int @int = si.GetInt32("_CultureInfo");
		if (@int != -1)
		{
			cultureinfo = new CultureInfo(@int);
		}
	}

	/// <summary>Returns the full name of the assembly, also known as the display name.</summary>
	/// <returns>The full name of the assembly, or the class name if the full name cannot be determined.</returns>
	public override string ToString()
	{
		string fullName = FullName;
		if (fullName == null)
		{
			return base.ToString();
		}
		return fullName;
	}

	/// <summary>Gets the public key of the assembly.</summary>
	/// <returns>A byte array that contains the public key of the assembly.</returns>
	/// <exception cref="T:System.Security.SecurityException">A public key was provided (for example, by using the <see cref="M:System.Reflection.AssemblyName.SetPublicKey(System.Byte[])" /> method), but no public key token was provided. </exception>
	public byte[] GetPublicKey()
	{
		return publicKey;
	}

	/// <summary>Gets the public key token, which is the last 8 bytes of the SHA-1 hash of the public key under which the application or assembly is signed.</summary>
	/// <returns>A byte array that contains the public key token.</returns>
	public byte[] GetPublicKeyToken()
	{
		if (keyToken != null)
		{
			return keyToken;
		}
		if (publicKey == null)
		{
			return null;
		}
		if (publicKey.Length == 0)
		{
			return EmptyArray<byte>.Value;
		}
		if (!IsPublicKeyValid)
		{
			throw new SecurityException("The public key is not valid.");
		}
		keyToken = ComputePublicKeyToken();
		return keyToken;
	}

	private byte[] InternalGetPublicKeyToken()
	{
		if (keyToken != null)
		{
			return keyToken;
		}
		if (publicKey == null)
		{
			return null;
		}
		if (publicKey.Length == 0)
		{
			return EmptyArray<byte>.Value;
		}
		if (!IsPublicKeyValid)
		{
			throw new SecurityException("The public key is not valid.");
		}
		return ComputePublicKeyToken();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void get_public_token(byte* token, byte* pubkey, int len);

	private unsafe byte[] ComputePublicKeyToken()
	{
		byte[] array = new byte[8];
		fixed (byte* token = array)
		{
			fixed (byte* pubkey = publicKey)
			{
				get_public_token(token, pubkey, publicKey.Length);
			}
		}
		return array;
	}

	/// <summary>Returns a value indicating whether two assembly names are the same. The comparison is based on the simple assembly names.</summary>
	/// <returns>true if the simple assembly names are the same; otherwise, false.</returns>
	/// <param name="reference">The reference assembly name.</param>
	/// <param name="definition">The assembly name that is compared to the reference assembly.</param>
	public static bool ReferenceMatchesDefinition(AssemblyName reference, AssemblyName definition)
	{
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
		if (definition == null)
		{
			throw new ArgumentNullException("definition");
		}
		return string.Equals(reference.Name, definition.Name, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>Sets the public key identifying the assembly.</summary>
	/// <param name="publicKey">A byte array containing the public key of the assembly. </param>
	public void SetPublicKey(byte[] publicKey)
	{
		if (publicKey == null)
		{
			flags ^= AssemblyNameFlags.PublicKey;
		}
		else
		{
			flags |= AssemblyNameFlags.PublicKey;
		}
		this.publicKey = publicKey;
	}

	/// <summary>Sets the public key token, which is the last 8 bytes of the SHA-1 hash of the public key under which the application or assembly is signed.</summary>
	/// <param name="publicKeyToken">A byte array containing the public key token of the assembly. </param>
	public void SetPublicKeyToken(byte[] publicKeyToken)
	{
		keyToken = publicKeyToken;
	}

	/// <summary>Gets serialization information with all the data needed to recreate an instance of this AssemblyName.</summary>
	/// <param name="info">The object to be populated with serialization information. </param>
	/// <param name="context">The destination context of the serialization. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	[SecurityCritical]
	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("_Name", name);
		info.AddValue("_PublicKey", publicKey);
		info.AddValue("_PublicKeyToken", keyToken);
		info.AddValue("_CultureInfo", (cultureinfo != null) ? cultureinfo.LCID : (-1));
		info.AddValue("_CodeBase", codebase);
		info.AddValue("_Version", Version);
		info.AddValue("_HashAlgorithm", hashalg);
		info.AddValue("_HashAlgorithmForControl", AssemblyHashAlgorithm.None);
		info.AddValue("_StrongNameKeyPair", keypair);
		info.AddValue("_VersionCompatibility", versioncompat);
		info.AddValue("_Flags", flags);
		info.AddValue("_HashForControl", null);
	}

	/// <summary>Makes a copy of this <see cref="T:System.Reflection.AssemblyName" /> object.</summary>
	/// <returns>An object that is a copy of this <see cref="T:System.Reflection.AssemblyName" /> object.</returns>
	public object Clone()
	{
		return new AssemblyName
		{
			name = name,
			codebase = codebase,
			major = major,
			minor = minor,
			build = build,
			revision = revision,
			version = version,
			cultureinfo = cultureinfo,
			flags = flags,
			hashalg = hashalg,
			keypair = keypair,
			publicKey = publicKey,
			keyToken = keyToken,
			versioncompat = versioncompat,
			processor_architecture = processor_architecture
		};
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and is called back by the deserialization event when deserialization is complete.</summary>
	/// <param name="sender">The source of the deserialization event. </param>
	public void OnDeserialization(object sender)
	{
		Version = version;
	}

	/// <summary>Gets the <see cref="T:System.Reflection.AssemblyName" /> for a given file.</summary>
	/// <returns>An object that represents the given assembly file.</returns>
	/// <param name="assemblyFile">The path for the assembly whose <see cref="T:System.Reflection.AssemblyName" /> is to be returned. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyFile" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="assemblyFile" /> is invalid, such as an assembly with an invalid culture. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyFile" /> is not found. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have path discovery permission. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly. </exception>
	/// <exception cref="T:System.IO.FileLoadException">An assembly or module was loaded twice with two different sets of evidence. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public unsafe static AssemblyName GetAssemblyName(string assemblyFile)
	{
		if (assemblyFile == null)
		{
			throw new ArgumentNullException("assemblyFile");
		}
		AssemblyName assemblyName = new AssemblyName();
		Assembly.InternalGetAssemblyName(Path.GetFullPath(assemblyFile), out var aname, out var codeBase);
		try
		{
			assemblyName.FillName(&aname, codeBase, addVersion: true, addPublickey: false, defaultToken: true, assemblyRef: false);
			return assemblyName;
		}
		finally
		{
			RuntimeMarshal.FreeAssemblyName(ref aname, freeStruct: false);
		}
	}

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array that receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _AssemblyName.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the type information for an object, which can then be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">Receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _AssemblyName.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _AssemblyName.GetTypeInfoCount(out uint pcTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Provides access to properties and methods exposed by an object.</summary>
	/// <param name="dispIdMember">Identifies the member.</param>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="lcid">The locale context in which to interpret arguments.</param>
	/// <param name="wFlags">Flags describing the context of the call.</param>
	/// <param name="pDispParams">Pointer to a structure containing an array of arguments, an array of argument DispIDs for named arguments, and counts for the number of elements in the arrays.</param>
	/// <param name="pVarResult">Pointer to the location where the result is to be stored.</param>
	/// <param name="pExcepInfo">Pointer to a structure that contains exception information.</param>
	/// <param name="puArgErr">The index of the first argument that has an error.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _AssemblyName.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern MonoAssemblyName* GetNativeName(IntPtr assembly_ptr);

	internal unsafe void FillName(MonoAssemblyName* native, string codeBase, bool addVersion, bool addPublickey, bool defaultToken, bool assemblyRef)
	{
		name = RuntimeMarshal.PtrToUtf8String(native->name);
		major = native->major;
		minor = native->minor;
		build = native->build;
		revision = native->revision;
		flags = (AssemblyNameFlags)native->flags;
		hashalg = (AssemblyHashAlgorithm)native->hash_alg;
		versioncompat = AssemblyVersionCompatibility.SameMachine;
		processor_architecture = (ProcessorArchitecture)native->arch;
		if (addVersion)
		{
			version = new Version(major, minor, build, revision);
		}
		codebase = codeBase;
		if (native->culture != IntPtr.Zero)
		{
			cultureinfo = CultureInfo.CreateCulture(RuntimeMarshal.PtrToUtf8String(native->culture), assemblyRef);
		}
		if (native->public_key != IntPtr.Zero)
		{
			publicKey = RuntimeMarshal.DecodeBlobArray(native->public_key);
			flags |= AssemblyNameFlags.PublicKey;
		}
		else if (addPublickey)
		{
			publicKey = EmptyArray<byte>.Value;
			flags |= AssemblyNameFlags.PublicKey;
		}
		if (*native->public_key_token != 0)
		{
			byte[] array = new byte[8];
			int i = 0;
			int num = 0;
			for (; i < 8; i++)
			{
				array[i] = (byte)(RuntimeMarshal.AsciHexDigitValue(native->public_key_token[num++]) << 4);
				array[i] |= (byte)RuntimeMarshal.AsciHexDigitValue(native->public_key_token[num++]);
			}
			keyToken = array;
		}
		else if (defaultToken)
		{
			keyToken = EmptyArray<byte>.Value;
		}
	}

	internal unsafe static AssemblyName Create(Assembly assembly, bool fillCodebase)
	{
		AssemblyName assemblyName = new AssemblyName();
		MonoAssemblyName* nativeName = GetNativeName(assembly._mono_assembly);
		assemblyName.FillName(nativeName, fillCodebase ? assembly.CodeBase : null, addVersion: true, addPublickey: true, defaultToken: true, assemblyRef: false);
		return assemblyName;
	}
}
