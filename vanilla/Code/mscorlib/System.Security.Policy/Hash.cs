using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using Unity;

namespace System.Security.Policy;

/// <summary>Provides evidence about the hash value for an assembly. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class Hash : EvidenceBase, ISerializable, IBuiltInEvidence
{
	private Assembly assembly;

	private byte[] data;

	internal byte[] _md5;

	internal byte[] _sha1;

	/// <summary>Gets the <see cref="T:System.Security.Cryptography.MD5" /> hash value for the assembly.</summary>
	/// <returns>A byte array that represents the <see cref="T:System.Security.Cryptography.MD5" /> hash value for the assembly.</returns>
	public byte[] MD5
	{
		get
		{
			if (_md5 != null)
			{
				return _md5;
			}
			if (assembly == null && _sha1 != null)
			{
				throw new SecurityException(Locale.GetText("No assembly data. This instance was initialized with an MSHA1 digest value."));
			}
			HashAlgorithm hashAlg = System.Security.Cryptography.MD5.Create();
			_md5 = GenerateHash(hashAlg);
			return _md5;
		}
	}

	/// <summary>Gets the <see cref="T:System.Security.Cryptography.SHA1" /> hash value for the assembly.</summary>
	/// <returns>A byte array that represents the <see cref="T:System.Security.Cryptography.SHA1" /> hash value for the assembly.</returns>
	public byte[] SHA1
	{
		get
		{
			if (_sha1 != null)
			{
				return _sha1;
			}
			if (assembly == null && _md5 != null)
			{
				throw new SecurityException(Locale.GetText("No assembly data. This instance was initialized with an MD5 digest value."));
			}
			HashAlgorithm hashAlg = System.Security.Cryptography.SHA1.Create();
			_sha1 = GenerateHash(hashAlg);
			return _sha1;
		}
	}

	/// <summary>Gets the <see cref="T:System.Security.Cryptography.SHA256" /> hash value for the assembly.</summary>
	/// <returns>A byte array that represents the <see cref="T:System.Security.Cryptography.SHA256" /> hash value for the assembly.</returns>
	public byte[] SHA256
	{
		get
		{
			ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.Hash" /> class.</summary>
	/// <param name="assembly">The assembly for which to compute the hash value. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="assembly" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The hash cannot be generated.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="assembly" /> is not a run-time <see cref="T:System.Reflection.Assembly" /> object.</exception>
	public Hash(Assembly assembly)
	{
		if (assembly == null)
		{
			throw new ArgumentNullException("assembly");
		}
		this.assembly = assembly;
	}

	internal Hash()
	{
	}

	internal Hash(SerializationInfo info, StreamingContext context)
	{
		data = (byte[])info.GetValue("RawData", typeof(byte[]));
	}

	/// <summary>Computes the hash value for the assembly using the specified hash algorithm.</summary>
	/// <returns>A byte array that represents the hash value for the assembly.</returns>
	/// <param name="hashAlg">The hash algorithm to use to compute the hash value for the assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="hashAlg" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The hash value for the assembly cannot be generated.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public byte[] GenerateHash(HashAlgorithm hashAlg)
	{
		if (hashAlg == null)
		{
			throw new ArgumentNullException("hashAlg");
		}
		return hashAlg.ComputeHash(GetData());
	}

	/// <summary>Gets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the parameter name and additional exception information.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecurityCritical]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("RawData", GetData());
	}

	/// <summary>Returns a string representation of the current <see cref="T:System.Security.Policy.Hash" />.</summary>
	/// <returns>A representation of the current <see cref="T:System.Security.Policy.Hash" />.</returns>
	public override string ToString()
	{
		SecurityElement securityElement = new SecurityElement(GetType().FullName);
		securityElement.AddAttribute("version", "1");
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array = GetData();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("X2"));
		}
		securityElement.AddChild(new SecurityElement("RawData", stringBuilder.ToString()));
		return securityElement.ToString();
	}

	[FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
	private byte[] GetData()
	{
		if (assembly == null && data == null)
		{
			throw new SecurityException(Locale.GetText("No assembly data."));
		}
		if (data == null)
		{
			FileStream fileStream = new FileStream(assembly.Location, FileMode.Open, FileAccess.Read);
			data = new byte[fileStream.Length];
			fileStream.Read(data, 0, (int)fileStream.Length);
		}
		return data;
	}

	int IBuiltInEvidence.GetRequiredSize(bool verbose)
	{
		if (!verbose)
		{
			return 0;
		}
		return 5;
	}

	[MonoTODO("IBuiltInEvidence")]
	int IBuiltInEvidence.InitFromBuffer(char[] buffer, int position)
	{
		return 0;
	}

	[MonoTODO("IBuiltInEvidence")]
	int IBuiltInEvidence.OutputToBuffer(char[] buffer, int position, bool verbose)
	{
		return 0;
	}

	/// <summary>Creates a <see cref="T:System.Security.Policy.Hash" /> object that contains an <see cref="T:System.Security.Cryptography.MD5" /> hash value.</summary>
	/// <returns>An object that contains the hash value provided by the <paramref name="md5" /> parameter.</returns>
	/// <param name="md5">A byte array that contains an <see cref="T:System.Security.Cryptography.MD5" /> hash value.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="md5" /> parameter is null. </exception>
	public static Hash CreateMD5(byte[] md5)
	{
		if (md5 == null)
		{
			throw new ArgumentNullException("md5");
		}
		return new Hash
		{
			_md5 = md5
		};
	}

	/// <summary>Creates a <see cref="T:System.Security.Policy.Hash" /> object that contains a <see cref="T:System.Security.Cryptography.SHA1" /> hash value.</summary>
	/// <returns>An object that contains the hash value provided by the <paramref name="sha1" /> parameter.</returns>
	/// <param name="sha1">A byte array that contains a <see cref="T:System.Security.Cryptography.SHA1" /> hash value.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="sha1" /> parameter is null. </exception>
	public static Hash CreateSHA1(byte[] sha1)
	{
		if (sha1 == null)
		{
			throw new ArgumentNullException("sha1");
		}
		return new Hash
		{
			_sha1 = sha1
		};
	}

	/// <summary>Creates a <see cref="T:System.Security.Policy.Hash" /> object that contains a <see cref="T:System.Security.Cryptography.SHA256" /> hash value.</summary>
	/// <returns>A hash object that contains the hash value provided by the <paramref name="sha256" /> parameter.</returns>
	/// <param name="sha256">A byte array that contains a <see cref="T:System.Security.Cryptography.SHA256" /> hash value.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="sha256" /> parameter is null. </exception>
	public static Hash CreateSHA256(byte[] sha256)
	{
		ThrowStub.ThrowNotSupportedException();
		return null;
	}
}
