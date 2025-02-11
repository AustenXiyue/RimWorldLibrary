using System.Collections.Generic;
using System.Collections.ObjectModel;
using MS.Internal;
using MS.Internal.Security.RightsManagement;
using MS.Internal.WindowsBase;

namespace System.Security.RightsManagement;

/// <summary>Provides digital rights management services for encrypting and decrypting protected content.     </summary>
public class CryptoProvider : IDisposable
{
	private int _blockSize;

	private SafeRightsManagementHandle _decryptorHandle = SafeRightsManagementHandle.InvalidHandle;

	private bool _decryptorHandleCalculated;

	private SafeRightsManagementHandle _encryptorHandle = SafeRightsManagementHandle.InvalidHandle;

	private bool _encryptorHandleCalculated;

	private SafeRightsManagementHandle _boundLicenseOwnerViewRightsHandle = SafeRightsManagementHandle.InvalidHandle;

	private bool _boundLicenseOwnerViewRightsHandleCalculated;

	private List<SafeRightsManagementHandle> _boundLicenseHandleList;

	private List<RightNameExpirationInfoPair> _boundRightsInfoList;

	private ReadOnlyCollection<ContentGrant> _boundGrantReadOnlyCollection;

	private ContentUser _owner;

	private bool _disposed;

	/// <summary>Gets the cipher block size, in bytes. </summary>
	/// <returns>The cipher block size, in bytes.  The default block size for Advanced Encryption Standard (AES) is 8.</returns>
	public int BlockSize
	{
		get
		{
			CheckDisposed();
			if (_blockSize == 0)
			{
				_blockSize = QueryBlockSize();
			}
			return _blockSize;
		}
	}

	/// <summary>Gets a value that indicates whether <see cref="M:System.Security.RightsManagement.CryptoProvider.Encrypt(System.Byte[])" /> and <see cref="M:System.Security.RightsManagement.CryptoProvider.Decrypt(System.Byte[])" /> can accept buffers that are different block sizes in length.</summary>
	/// <returns>true if the buffer passed to <see cref="M:System.Security.RightsManagement.CryptoProvider.Encrypt(System.Byte[])" /> can be a different length from the buffer passed to <see cref="M:System.Security.RightsManagement.CryptoProvider.Decrypt(System.Byte[])" />; otherwise, false if the buffers passed to <see cref="M:System.Security.RightsManagement.CryptoProvider.Encrypt(System.Byte[])" /> and <see cref="M:System.Security.RightsManagement.CryptoProvider.Decrypt(System.Byte[])" /> must be the exact same length.  For Advanced Encryption Standard (AES) the default is true.</returns>
	public bool CanMergeBlocks
	{
		get
		{
			CheckDisposed();
			return BlockSize > 1;
		}
	}

	/// <summary>Gets a collection listing the rights that passed verification and that are granted to the user.</summary>
	/// <returns>A collection enumerating the rights that passed verification and that are granted to the user.</returns>
	public ReadOnlyCollection<ContentGrant> BoundGrants
	{
		get
		{
			CheckDisposed();
			if (_boundGrantReadOnlyCollection == null)
			{
				List<ContentGrant> list = new List<ContentGrant>(_boundRightsInfoList.Count);
				foreach (RightNameExpirationInfoPair boundRightsInfo in _boundRightsInfoList)
				{
					ContentRight? rightFromString = ClientSession.GetRightFromString(boundRightsInfo.RightName);
					if (rightFromString.HasValue)
					{
						list.Add(new ContentGrant(_owner, rightFromString.Value, boundRightsInfo.ValidFrom, boundRightsInfo.ValidUntil));
					}
				}
				_boundGrantReadOnlyCollection = new ReadOnlyCollection<ContentGrant>(list);
			}
			return _boundGrantReadOnlyCollection;
		}
	}

	/// <summary>Gets a value that indicates whether the user has rights to encrypt. </summary>
	/// <returns>true if the <see cref="T:System.Security.RightsManagement.UseLicense" /> grants the user rights to encrypt; otherwise, false.</returns>
	public bool CanEncrypt
	{
		get
		{
			CheckDisposed();
			return !EncryptorHandle.IsInvalid;
		}
	}

	/// <summary>Gets a value that indicates whether the user has rights to decrypt. </summary>
	/// <returns>true if the <see cref="T:System.Security.RightsManagement.UseLicense" /> grants the user rights to decrypt; otherwise, false.</returns>
	public bool CanDecrypt
	{
		get
		{
			CheckDisposed();
			return !DecryptorHandle.IsInvalid;
		}
	}

	private SafeRightsManagementHandle DecryptorHandle
	{
		get
		{
			if (!_decryptorHandleCalculated)
			{
				for (int i = 0; i < _boundLicenseHandleList.Count; i++)
				{
					SafeRightsManagementHandle decryptorHandle = null;
					if (SafeNativeMethods.DRMCreateEnablingBitsDecryptor(_boundLicenseHandleList[i], _boundRightsInfoList[i].RightName, 0u, null, out decryptorHandle) == 0)
					{
						_decryptorHandle = decryptorHandle;
						_decryptorHandleCalculated = true;
						return _decryptorHandle;
					}
				}
				_decryptorHandleCalculated = true;
			}
			return _decryptorHandle;
		}
	}

	private SafeRightsManagementHandle EncryptorHandle
	{
		get
		{
			if (!_encryptorHandleCalculated)
			{
				for (int i = 0; i < _boundLicenseHandleList.Count; i++)
				{
					SafeRightsManagementHandle encryptorHandle = null;
					if (SafeNativeMethods.DRMCreateEnablingBitsEncryptor(_boundLicenseHandleList[i], _boundRightsInfoList[i].RightName, 0u, null, out encryptorHandle) == 0)
					{
						_encryptorHandle = encryptorHandle;
						_encryptorHandleCalculated = true;
						return _encryptorHandle;
					}
				}
				_encryptorHandleCalculated = true;
			}
			return _encryptorHandle;
		}
	}

	private SafeRightsManagementHandle BoundLicenseOwnerViewRightsHandle
	{
		get
		{
			if (!_boundLicenseOwnerViewRightsHandleCalculated)
			{
				for (int i = 0; i < _boundLicenseHandleList.Count; i++)
				{
					ContentRight? rightFromString = ClientSession.GetRightFromString(_boundRightsInfoList[i].RightName);
					if (rightFromString.HasValue && (rightFromString.Value == ContentRight.Owner || rightFromString.Value == ContentRight.ViewRightsData))
					{
						_boundLicenseOwnerViewRightsHandle = _boundLicenseHandleList[i];
						_boundLicenseOwnerViewRightsHandleCalculated = true;
						return _boundLicenseOwnerViewRightsHandle;
					}
				}
				_boundLicenseOwnerViewRightsHandleCalculated = true;
			}
			return _boundLicenseOwnerViewRightsHandle;
		}
	}

	/// <summary>Frees resources and performs internal cleanup before the instance is reclaimed by garbage collection.</summary>
	~CryptoProvider()
	{
		Dispose(disposing: false);
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Security.RightsManagement.CryptoProvider" />. </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Encrypts clear text to cipher text. </summary>
	/// <returns>Encrypted cipher text of the given <paramref name="clearText" />.</returns>
	/// <param name="clearText">The clear text content to encrypt.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="clearText" /> is null.</exception>
	/// <exception cref="T:System.Security.RightsManagement.RightsManagementException">Encryption is not permitted.</exception>
	public byte[] Encrypt(byte[] clearText)
	{
		CheckDisposed();
		if (clearText == null)
		{
			throw new ArgumentNullException("clearText");
		}
		if (!CanEncrypt)
		{
			throw new RightsManagementException(RightsManagementFailureCode.EncryptionNotPermitted);
		}
		uint num = 0u;
		byte[] array = null;
		num = (uint)clearText.Length;
		array = new byte[num];
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMEncrypt(EncryptorHandle, 0u, (uint)clearText.Length, clearText, ref num, array));
		return array;
	}

	/// <summary>Decrypts cipher text to clear text. </summary>
	/// <returns>The decrypted clear text of <paramref name="cryptoText" />.</returns>
	/// <param name="cryptoText">The cipher text to decrypt.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="cipherText" /> is null.</exception>
	/// <exception cref="T:System.Security.RightsManagement.RightsManagementException">Decryption right not granted.</exception>
	public byte[] Decrypt(byte[] cryptoText)
	{
		CheckDisposed();
		if (cryptoText == null)
		{
			throw new ArgumentNullException("cryptoText");
		}
		if (!CanDecrypt)
		{
			throw new RightsManagementException(RightsManagementFailureCode.RightNotGranted);
		}
		uint num = 0u;
		byte[] array = null;
		num = (uint)cryptoText.Length;
		array = new byte[num];
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMDecrypt(DecryptorHandle, 0u, (uint)cryptoText.Length, cryptoText, ref num, array));
		return array;
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.RightsManagement.CryptoProvider" /> and optionally releases the managed resources.  </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		try
		{
			if (!disposing)
			{
				return;
			}
			if (_decryptorHandle != null && !_decryptorHandle.IsInvalid)
			{
				_decryptorHandle.Close();
			}
			if (_encryptorHandle != null && !_encryptorHandle.IsInvalid)
			{
				_encryptorHandle.Close();
			}
			if (_boundLicenseOwnerViewRightsHandle != null && !_boundLicenseOwnerViewRightsHandle.IsInvalid)
			{
				_boundLicenseOwnerViewRightsHandle.Close();
			}
			if (_boundLicenseHandleList == null)
			{
				return;
			}
			foreach (SafeRightsManagementHandle boundLicenseHandle in _boundLicenseHandleList)
			{
				if (boundLicenseHandle != null && !boundLicenseHandle.IsInvalid)
				{
					boundLicenseHandle.Close();
				}
			}
		}
		finally
		{
			_disposed = true;
			_boundLicenseHandleList = null;
			_boundLicenseOwnerViewRightsHandle = null;
			_decryptorHandle = null;
			_encryptorHandle = null;
		}
	}

	internal CryptoProvider(List<SafeRightsManagementHandle> boundLicenseHandleList, List<RightNameExpirationInfoPair> rightsInfoList, ContentUser owner)
	{
		Invariant.Assert(boundLicenseHandleList != null);
		Invariant.Assert(boundLicenseHandleList.Count > 0);
		Invariant.Assert(rightsInfoList != null);
		Invariant.Assert(rightsInfoList.Count > 0);
		Invariant.Assert(rightsInfoList.Count == boundLicenseHandleList.Count);
		Invariant.Assert(owner != null);
		_boundLicenseHandleList = boundLicenseHandleList;
		_boundRightsInfoList = rightsInfoList;
		_owner = owner;
	}

	internal UnsignedPublishLicense DecryptPublishLicense(string serializedPublishLicense)
	{
		Invariant.Assert(serializedPublishLicense != null);
		if (BoundLicenseOwnerViewRightsHandle == null || BoundLicenseOwnerViewRightsHandle.IsInvalid)
		{
			throw new RightsManagementException(RightsManagementFailureCode.RightNotGranted);
		}
		return new UnsignedPublishLicense(BoundLicenseOwnerViewRightsHandle, serializedPublishLicense);
	}

	private void CheckDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(null, SR.CryptoProviderDisposed);
		}
	}

	private int QueryBlockSize()
	{
		uint outputByteCount = 0u;
		byte[] array = null;
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetInfo(DecryptorHandle, "block-size", out var encodingType, ref outputByteCount, null));
		Invariant.Assert(outputByteCount == 4);
		array = new byte[outputByteCount];
		Errors.ThrowOnErrorCode(SafeNativeMethods.DRMGetInfo(DecryptorHandle, "block-size", out encodingType, ref outputByteCount, array));
		return BitConverter.ToInt32(array, 0);
	}
}
