using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using MS.Internal;
using MS.Internal.IO.Packaging;
using MS.Internal.IO.Packaging.CompoundFile;
using MS.Internal.WindowsBase;

namespace System.IO.Packaging;

/// <summary>Represents an OLE compound file that contains an encrypted package.</summary>
public class EncryptedPackageEnvelope : IDisposable
{
	private bool _disposed;

	private bool _handedOutPackage;

	private bool _handedOutPackageStream;

	private StorageRoot _root;

	private Package _package;

	private string _dataSpaceName;

	private Stream _packageStream;

	private StorageBasedPackageProperties _packageProperties;

	private RightsManagementInformation _rmi;

	private const string _encryptionTransformName = "EncryptionTransform";

	private const string _packageStreamName = "EncryptedPackage";

	private const string _dataspaceLabelRMEncryptionNoCompression = "RMEncryptionNoCompression";

	private const int STG_E_FILEALREADYEXISTS = -2147286960;

	private const FileMode _defaultFileModeForCreate = FileMode.Create;

	private const FileAccess _defaultFileAccess = FileAccess.ReadWrite;

	private const FileShare _defaultFileShare = FileShare.None;

	private const FileMode _defaultFileModeForOpen = FileMode.Open;

	/// <summary>Gets the rights management information stored in the <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" />; specifically, the <see cref="T:System.Security.RightsManagement.PublishLicense" /> and the <see cref="T:System.Security.RightsManagement.UseLicense" />s stored in the OLE compound file that embodies the rights-management protected package</summary>
	/// <returns>The <see cref="T:System.IO.Packaging.RightsManagementInformation" /> for the package.</returns>
	public RightsManagementInformation RightsManagementInformation
	{
		get
		{
			CheckDisposed();
			return _rmi;
		}
	}

	/// <summary>Gets the core package properties (such as Title and Subject) of the rights managed document.</summary>
	/// <returns>The <see cref="T:System.IO.Packaging.PackageProperties" /> for the package.</returns>
	public PackageProperties PackageProperties
	{
		get
		{
			CheckDisposed();
			if (_packageProperties == null)
			{
				_packageProperties = new StorageBasedPackageProperties(_root);
			}
			return _packageProperties;
		}
	}

	/// <summary>Gets a value that specifies whether the file was opened with access to read, write, or both.</summary>
	/// <returns>The <see cref="T:System.IO.FileAccess" /> value that was used to open the file. </returns>
	public FileAccess FileOpenAccess
	{
		get
		{
			CheckDisposed();
			return _root.OpenAccess;
		}
	}

	/// <summary>Gets an object that provides access to the compound file streams outside the encrypted package.</summary>
	/// <returns>A <see cref="T:System.IO.Packaging.StorageInfo" /> representing streams in the OLE compound file that are outside the encrypted package.</returns>
	public StorageInfo StorageInfo
	{
		get
		{
			CheckDisposed();
			return _root;
		}
	}

	internal static string EncryptionTransformName => "EncryptionTransform";

	internal static string PackageStreamName => "EncryptedPackage";

	internal static string DataspaceLabelRMEncryptionNoCompression => "RMEncryptionNoCompression";

	internal EncryptedPackageEnvelope(string envelopeFileName, PublishLicense publishLicense, CryptoProvider cryptoProvider)
	{
		if (envelopeFileName == null)
		{
			throw new ArgumentNullException("envelopeFileName");
		}
		ThrowIfRMEncryptionInfoInvalid(publishLicense, cryptoProvider);
		_root = StorageRoot.Open(envelopeFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		InitializeRMForCreate(publishLicense, cryptoProvider);
		EmbedPackage(null);
	}

	internal EncryptedPackageEnvelope(Stream envelopeStream, PublishLicense publishLicense, CryptoProvider cryptoProvider)
	{
		if (envelopeStream == null)
		{
			throw new ArgumentNullException("envelopeStream");
		}
		ThrowIfRMEncryptionInfoInvalid(publishLicense, cryptoProvider);
		_root = StorageRoot.CreateOnStream(envelopeStream, FileMode.Create);
		if (_root.OpenAccess != FileAccess.ReadWrite)
		{
			throw new NotSupportedException(SR.StreamNeedsReadWriteAccess);
		}
		InitializeRMForCreate(publishLicense, cryptoProvider);
		EmbedPackage(null);
	}

	internal EncryptedPackageEnvelope(string envelopeFileName, Stream packageStream, PublishLicense publishLicense, CryptoProvider cryptoProvider)
	{
		if (envelopeFileName == null)
		{
			throw new ArgumentNullException("envelopeFileName");
		}
		if (packageStream == null)
		{
			throw new ArgumentNullException("packageStream");
		}
		ThrowIfRMEncryptionInfoInvalid(publishLicense, cryptoProvider);
		_root = StorageRoot.Open(envelopeFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		InitializeRMForCreate(publishLicense, cryptoProvider);
		EmbedPackage(packageStream);
	}

	internal EncryptedPackageEnvelope(Stream envelopeStream, Stream packageStream, PublishLicense publishLicense, CryptoProvider cryptoProvider)
	{
		if (envelopeStream == null)
		{
			throw new ArgumentNullException("envelopeStream");
		}
		if (packageStream == null)
		{
			throw new ArgumentNullException("packageStream");
		}
		ThrowIfRMEncryptionInfoInvalid(publishLicense, cryptoProvider);
		_root = StorageRoot.CreateOnStream(envelopeStream, FileMode.Create);
		if (_root.OpenAccess != FileAccess.ReadWrite)
		{
			throw new NotSupportedException(SR.StreamNeedsReadWriteAccess);
		}
		InitializeRMForCreate(publishLicense, cryptoProvider);
		EmbedPackage(packageStream);
	}

	internal EncryptedPackageEnvelope(string envelopeFileName, FileAccess access, FileShare sharing)
	{
		if (envelopeFileName == null)
		{
			throw new ArgumentNullException("envelopeFileName");
		}
		_root = StorageRoot.Open(envelopeFileName, FileMode.Open, access, sharing);
		InitForOpen();
	}

	internal EncryptedPackageEnvelope(Stream envelopeStream)
	{
		if (envelopeStream == null)
		{
			throw new ArgumentNullException("envelopeStream");
		}
		_root = StorageRoot.CreateOnStream(envelopeStream, FileMode.Open);
		InitForOpen();
	}

	/// <summary>Creates and returns an <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" /> and gives it the specified file name.</summary>
	/// <returns>The newly created <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" />. </returns>
	/// <param name="envelopeFileName">The name of the OLE compound file.</param>
	/// <param name="publishLicense">The publish license that is embedded in the compound file.</param>
	/// <param name="cryptoProvider">An object that determines what operations the current user is allowed to perform on the encrypted content.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="envelopeFileName" /> is null.</exception>
	public static EncryptedPackageEnvelope Create(string envelopeFileName, PublishLicense publishLicense, CryptoProvider cryptoProvider)
	{
		return new EncryptedPackageEnvelope(envelopeFileName, publishLicense, cryptoProvider);
	}

	/// <summary>Creates and returns an <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" /> on the specified stream.</summary>
	/// <returns>The newly created <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" />. </returns>
	/// <param name="envelopeStream">The stream on which to create the file.</param>
	/// <param name="publishLicense">The publish license that is embedded in the compound file.</param>
	/// <param name="cryptoProvider">An object that determines what operations the current user is allowed to perform on the encrypted content.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="envelopeStream" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="envelopeStream" /> does not provide read/write access.</exception>
	public static EncryptedPackageEnvelope Create(Stream envelopeStream, PublishLicense publishLicense, CryptoProvider cryptoProvider)
	{
		return new EncryptedPackageEnvelope(envelopeStream, publishLicense, cryptoProvider);
	}

	/// <summary>Creates and returns an <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" /> that uses the specified unencrypted package as its content, and gives it the specified file name.</summary>
	/// <returns>The newly created <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" />.</returns>
	/// <param name="envelopeFileName">The name of the OLE compound file.</param>
	/// <param name="packageStream">The stream representing the existing unencrypted package.</param>
	/// <param name="publishLicense">The publish license that is embedded in the compound file.</param>
	/// <param name="cryptoProvider">An object that determines what operations the current user is allowed to perform on the encrypted content.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="envelopeFileName" /> or <paramref name="packageStream" /> is null.</exception>
	public static EncryptedPackageEnvelope CreateFromPackage(string envelopeFileName, Stream packageStream, PublishLicense publishLicense, CryptoProvider cryptoProvider)
	{
		return new EncryptedPackageEnvelope(envelopeFileName, packageStream, publishLicense, cryptoProvider);
	}

	/// <summary>Creates and returns an <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" /> (on the specified stream) that uses the specified unencrypted package as its content.</summary>
	/// <returns>The newly created <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" />. </returns>
	/// <param name="envelopeStream">The stream on which to create the file.</param>
	/// <param name="packageStream">The stream representing the existing unencrypted package.</param>
	/// <param name="publishLicense">The publish license that is embedded in the compound file.</param>
	/// <param name="cryptoProvider">An object that determines what operations the current user is allowed to perform on the encrypted content.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="envelopeStream" /> or <paramref name="packageStream" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="envelopeStream" /> does not provide read/write access.</exception>
	public static EncryptedPackageEnvelope CreateFromPackage(Stream envelopeStream, Stream packageStream, PublishLicense publishLicense, CryptoProvider cryptoProvider)
	{
		return new EncryptedPackageEnvelope(envelopeStream, packageStream, publishLicense, cryptoProvider);
	}

	/// <summary>Open the encrypted package in the specified file as read only and unshared.</summary>
	/// <returns>The <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" /> that is opened.</returns>
	/// <param name="envelopeFileName">The file containing the encrypted package.</param>
	public static EncryptedPackageEnvelope Open(string envelopeFileName)
	{
		return Open(envelopeFileName, FileAccess.ReadWrite, FileShare.None);
	}

	/// <summary>Open the encrypted package in the specified file as unshared and with the specified access.</summary>
	/// <returns>The <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" /> that is opened.</returns>
	/// <param name="envelopeFileName">The file containing the encrypted package.</param>
	/// <param name="access">The type of access.</param>
	public static EncryptedPackageEnvelope Open(string envelopeFileName, FileAccess access)
	{
		return Open(envelopeFileName, access, FileShare.None);
	}

	/// <summary>Open the encrypted package in the specified file and gives it the specified access and sharing.</summary>
	/// <returns>The <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" /> that is opened.</returns>
	/// <param name="envelopeFileName">The file containing the encrypted package.</param>
	/// <param name="access">The type of access.</param>
	/// <param name="sharing">The type of sharing.</param>
	public static EncryptedPackageEnvelope Open(string envelopeFileName, FileAccess access, FileShare sharing)
	{
		return new EncryptedPackageEnvelope(envelopeFileName, access, sharing);
	}

	/// <summary>Open the encrypted package in the specified stream.</summary>
	/// <returns>The <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" /> that is opened.</returns>
	/// <param name="envelopeStream">The stream containing the encrypted package.</param>
	public static EncryptedPackageEnvelope Open(Stream envelopeStream)
	{
		return new EncryptedPackageEnvelope(envelopeStream);
	}

	/// <summary>Gets a value specifying whether the specified file is an OLE compound file with an encrypted package stream.</summary>
	/// <returns>true if the specified file is an OLE compound file with an encrypted package stream; otherwise, false. </returns>
	/// <param name="fileName">The file that is tested.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fileName" /> is null.</exception>
	public static bool IsEncryptedPackageEnvelope(string fileName)
	{
		bool flag = false;
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		StorageRoot storageRoot = null;
		try
		{
			storageRoot = StorageRoot.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			return ContainsEncryptedPackageStream(storageRoot);
		}
		catch (IOException ex)
		{
			if (ex.InnerException is COMException { ErrorCode: -2147286960 })
			{
				return false;
			}
			throw;
		}
		finally
		{
			storageRoot?.Close();
		}
	}

	/// <summary>Gets a value specifying whether the specified stream is an OLE compound file with an encrypted package stream.</summary>
	/// <returns>true if the specified stream is an OLE compound file with an encrypted package stream; otherwise, false. </returns>
	/// <param name="stream">The stream that is tested.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.</exception>
	public static bool IsEncryptedPackageEnvelope(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		bool flag = false;
		StorageRoot storageRoot = null;
		try
		{
			storageRoot = StorageRoot.CreateOnStream(stream, FileMode.Open);
			return ContainsEncryptedPackageStream(storageRoot);
		}
		catch (IOException ex)
		{
			if (ex.InnerException is COMException { ErrorCode: -2147286960 })
			{
				return false;
			}
			throw;
		}
		finally
		{
			storageRoot?.Close();
		}
	}

	/// <summary>Flush the stream for both the <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" /> and its package content.</summary>
	public void Flush()
	{
		CheckDisposed();
		if (_package != null)
		{
			_package.Flush();
		}
		if (_packageStream != null)
		{
			_packageStream.Flush();
		}
		Invariant.Assert(_root != null, "The envelope cannot be null");
		_root.Flush();
	}

	/// <summary>Closes the encrypted package and the OLE compound file that holds it.</summary>
	public void Close()
	{
		Dispose();
	}

	/// <summary>Releases all resources used by the <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" />. </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Gets the encrypted package inside the <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" />.</summary>
	/// <returns>The <see cref="T:System.IO.Packaging.Package" /> in the envelope. </returns>
	public Package GetPackage()
	{
		CheckDisposed();
		Invariant.Assert(!_handedOutPackageStream, "Copy of package stream has been already handed out");
		if (_package == null)
		{
			EnsurePackageStream();
			FileAccess fileAccess = (FileAccess)0;
			if (_packageStream.CanRead)
			{
				fileAccess |= FileAccess.Read;
			}
			if (_packageStream.CanWrite)
			{
				fileAccess |= FileAccess.Write;
			}
			fileAccess &= FileOpenAccess;
			_package = Package.Open(_packageStream, FileMode.Open, fileAccess);
		}
		_handedOutPackage = true;
		return _package;
	}

	internal Stream GetPackageStream()
	{
		CheckDisposed();
		Invariant.Assert(!_handedOutPackage, "Copy of package has been already handed out");
		EnsurePackageStream();
		_handedOutPackageStream = true;
		if (_package != null)
		{
			try
			{
				_package.Close();
			}
			finally
			{
				_package = null;
			}
		}
		return _packageStream;
	}

	private void InitializeRMForCreate(PublishLicense publishLicense, CryptoProvider cryptoProvider)
	{
		DataSpaceManager dataSpaceManager = _root.GetDataSpaceManager();
		dataSpaceManager.DefineTransform(RightsManagementEncryptionTransform.ClassTransformIdentifier, EncryptionTransformName);
		string[] transformStack = new string[1] { EncryptionTransformName };
		_dataSpaceName = DataspaceLabelRMEncryptionNoCompression;
		dataSpaceManager.DefineDataSpace(transformStack, _dataSpaceName);
		RightsManagementEncryptionTransform rightsManagementEncryptionTransform = dataSpaceManager.GetTransformFromName(EncryptionTransformName) as RightsManagementEncryptionTransform;
		_rmi = new RightsManagementInformation(rightsManagementEncryptionTransform);
		rightsManagementEncryptionTransform.SavePublishLicense(publishLicense);
		rightsManagementEncryptionTransform.CryptoProvider = cryptoProvider;
	}

	private void InitForOpen()
	{
		StreamInfo streamInfo = new StreamInfo(_root, PackageStreamName);
		if (!streamInfo.InternalExists())
		{
			throw new FileFormatException(SR.PackageNotFound);
		}
		List<IDataTransform> transformsForStreamInfo = _root.GetDataSpaceManager().GetTransformsForStreamInfo(streamInfo);
		RightsManagementEncryptionTransform rightsManagementEncryptionTransform = null;
		foreach (IDataTransform item in transformsForStreamInfo)
		{
			if (item.TransformIdentifier is string text && string.CompareOrdinal(text.ToUpperInvariant(), RightsManagementEncryptionTransform.ClassTransformIdentifier.ToUpperInvariant()) == 0)
			{
				if (rightsManagementEncryptionTransform != null)
				{
					throw new FileFormatException(SR.MultipleRightsManagementEncryptionTransformFound);
				}
				rightsManagementEncryptionTransform = item as RightsManagementEncryptionTransform;
			}
		}
		if (rightsManagementEncryptionTransform == null)
		{
			throw new FileFormatException(SR.RightsManagementEncryptionTransformNotFound);
		}
		_rmi = new RightsManagementInformation(rightsManagementEncryptionTransform);
	}

	private static bool ContainsEncryptedPackageStream(StorageRoot root)
	{
		return new StreamInfo(root, PackageStreamName).InternalExists();
	}

	private void EnsurePackageStream()
	{
		if (_packageStream == null)
		{
			StreamInfo streamInfo = new StreamInfo(_root, PackageStreamName);
			if (!streamInfo.InternalExists())
			{
				throw new FileFormatException(SR.PackageNotFound);
			}
			_packageStream = streamInfo.GetStream(FileMode.Open, FileOpenAccess);
		}
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.Packaging.EncryptedPackageEnvelope" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		try
		{
			if (!disposing)
			{
				return;
			}
			try
			{
				if (_package != null)
				{
					_package.Close();
				}
			}
			finally
			{
				_package = null;
				try
				{
					if (_packageStream != null)
					{
						_packageStream.Close();
					}
				}
				finally
				{
					_packageStream = null;
					try
					{
						if (_packageProperties != null)
						{
							_packageProperties.Dispose();
						}
					}
					finally
					{
						_packageProperties = null;
						try
						{
							if (_root != null)
							{
								_root.Close();
							}
						}
						finally
						{
							_root = null;
						}
					}
				}
			}
		}
		finally
		{
			_disposed = true;
		}
	}

	private void CheckDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(null, SR.EncryptedPackageEnvelopeDisposed);
		}
	}

	private void EmbedPackage(Stream packageStream)
	{
		StreamInfo streamInfo = new StreamInfo(_root, PackageStreamName);
		_packageStream = streamInfo.Create(FileMode.Create, _root.OpenAccess, _dataSpaceName);
		if (packageStream != null)
		{
			PackagingUtilities.CopyStream(packageStream, _packageStream, long.MaxValue, 4096);
			_package = Package.Open(_packageStream, FileMode.Open, FileOpenAccess);
		}
		else
		{
			_package = Package.Open(_packageStream, FileMode.Create, FileAccess.ReadWrite);
			_package.Flush();
			_packageStream.Flush();
		}
	}

	private void ThrowIfRMEncryptionInfoInvalid(PublishLicense publishLicense, CryptoProvider cryptoProvider)
	{
		if (publishLicense == null)
		{
			throw new ArgumentNullException("publishLicense");
		}
		if (cryptoProvider == null)
		{
			throw new ArgumentNullException("cryptoProvider");
		}
	}
}
