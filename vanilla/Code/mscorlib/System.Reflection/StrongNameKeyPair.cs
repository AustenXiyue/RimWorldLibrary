using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Permissions;
using Mono.Security;
using Mono.Security.Cryptography;

namespace System.Reflection;

/// <summary>Encapsulates access to a public or private key pair used to sign strong name assemblies.</summary>
[Serializable]
[ComVisible(true)]
public class StrongNameKeyPair : ISerializable, IDeserializationCallback
{
	private byte[] _publicKey;

	private string _keyPairContainer;

	private bool _keyPairExported;

	private byte[] _keyPairArray;

	[NonSerialized]
	private RSA _rsa;

	/// <summary>Gets the public part of the public key or public key token of the key pair.</summary>
	/// <returns>An array of type byte containing the public key or public key token of the key pair.</returns>
	public byte[] PublicKey
	{
		get
		{
			if (_publicKey == null)
			{
				byte[] array = CryptoConvert.ToCapiKeyBlob(GetRSA() ?? throw new ArgumentException("invalid keypair"), includePrivateKey: false);
				_publicKey = new byte[array.Length + 12];
				_publicKey[0] = 0;
				_publicKey[1] = 36;
				_publicKey[2] = 0;
				_publicKey[3] = 0;
				_publicKey[4] = 4;
				_publicKey[5] = 128;
				_publicKey[6] = 0;
				_publicKey[7] = 0;
				int num = array.Length;
				_publicKey[8] = (byte)(num % 256);
				_publicKey[9] = (byte)(num / 256);
				_publicKey[10] = 0;
				_publicKey[11] = 0;
				Buffer.BlockCopy(array, 0, _publicKey, 12, array.Length);
			}
			return _publicKey;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.StrongNameKeyPair" /> class, building the key pair from a byte array.</summary>
	/// <param name="keyPairArray">An array of type byte containing the key pair. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyPairArray" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	public StrongNameKeyPair(byte[] keyPairArray)
	{
		if (keyPairArray == null)
		{
			throw new ArgumentNullException("keyPairArray");
		}
		LoadKey(keyPairArray);
		GetRSA();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.StrongNameKeyPair" /> class, building the key pair from a FileStream.</summary>
	/// <param name="keyPairFile">A FileStream containing the key pair. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyPairFile" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	public StrongNameKeyPair(FileStream keyPairFile)
	{
		if (keyPairFile == null)
		{
			throw new ArgumentNullException("keyPairFile");
		}
		byte[] array = new byte[keyPairFile.Length];
		keyPairFile.Read(array, 0, array.Length);
		LoadKey(array);
		GetRSA();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.StrongNameKeyPair" /> class, building the key pair from a String.</summary>
	/// <param name="keyPairContainer">A string containing the key pair. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyPairContainer" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	public StrongNameKeyPair(string keyPairContainer)
	{
		if (keyPairContainer == null)
		{
			throw new ArgumentNullException("keyPairContainer");
		}
		_keyPairContainer = keyPairContainer;
		GetRSA();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.StrongNameKeyPair" /> class, building the key pair from serialized data.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that holds the serialized object data.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that contains contextual information about the source or destination.</param>
	protected StrongNameKeyPair(SerializationInfo info, StreamingContext context)
	{
		_publicKey = (byte[])info.GetValue("_publicKey", typeof(byte[]));
		_keyPairContainer = info.GetString("_keyPairContainer");
		_keyPairExported = info.GetBoolean("_keyPairExported");
		_keyPairArray = (byte[])info.GetValue("_keyPairArray", typeof(byte[]));
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with all the data required to reinstantiate the current <see cref="T:System.Reflection.StrongNameKeyPair" /> object.</summary>
	/// <param name="info">The object to be populated with serialization information.</param>
	/// <param name="context">The destination context of the serialization.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="info" /> is null.</exception>
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("_publicKey", _publicKey, typeof(byte[]));
		info.AddValue("_keyPairContainer", _keyPairContainer);
		info.AddValue("_keyPairExported", _keyPairExported);
		info.AddValue("_keyPairArray", _keyPairArray, typeof(byte[]));
	}

	/// <summary>Runs when the entire object graph has been deserialized.</summary>
	/// <param name="sender">The object that initiated the callback.</param>
	void IDeserializationCallback.OnDeserialization(object sender)
	{
	}

	private RSA GetRSA()
	{
		if (_rsa != null)
		{
			return _rsa;
		}
		if (_keyPairArray != null)
		{
			try
			{
				_rsa = CryptoConvert.FromCapiKeyBlob(_keyPairArray);
			}
			catch
			{
				_keyPairArray = null;
			}
		}
		else if (_keyPairContainer != null)
		{
			CspParameters cspParameters = new CspParameters();
			cspParameters.KeyContainerName = _keyPairContainer;
			_rsa = new RSACryptoServiceProvider(cspParameters);
		}
		return _rsa;
	}

	private void LoadKey(byte[] key)
	{
		try
		{
			if (key.Length == 16)
			{
				int num = 0;
				int num2 = 0;
				while (num < key.Length)
				{
					num2 += key[num++];
				}
				if (num2 == 4)
				{
					_publicKey = (byte[])key.Clone();
				}
			}
			else
			{
				_keyPairArray = key;
			}
		}
		catch
		{
		}
	}

	internal StrongName StrongName()
	{
		RSA rSA = GetRSA();
		if (rSA != null)
		{
			return new StrongName(rSA);
		}
		if (_publicKey != null)
		{
			return new StrongName(_publicKey);
		}
		return null;
	}
}
