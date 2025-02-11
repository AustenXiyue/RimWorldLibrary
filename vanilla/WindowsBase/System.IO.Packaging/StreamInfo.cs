using System.Runtime.InteropServices;
using MS.Internal.IO.Packaging.CompoundFile;
using MS.Internal.WindowsBase;

namespace System.IO.Packaging;

/// <summary>Provides access and information for manipulating I/O streams within a <see cref="T:System.IO.Packaging.Package" />.    </summary>
public class StreamInfo
{
	private const FileMode defaultFileOpenMode = FileMode.OpenOrCreate;

	private const FileMode defaultFileCreateMode = FileMode.Create;

	private const string defaultDataSpace = null;

	private StorageInfo parentStorage;

	private StreamInfoCore core;

	private CompoundFileStreamReference _streamReference;

	private FileAccess openFileAccess;

	private CompressionOption _compressionOption;

	private EncryptionOption _encryptionOption;

	private bool _needToGetTransformInfo = true;

	/// <summary>Gets the <see cref="T:System.IO.Packaging.CompressionOption" /> setting for the stream.</summary>
	/// <returns>The compression option setting for the package stream.</returns>
	public CompressionOption CompressionOption
	{
		get
		{
			if (StreamInfoDisposed)
			{
				return CompressionOption.NotCompressed;
			}
			EnsureTransformInformation();
			return _compressionOption;
		}
	}

	/// <summary>Gets the <see cref="T:System.IO.Packaging.EncryptionOption" /> setting for the stream.</summary>
	/// <returns>The encryption option setting for the package stream.</returns>
	public EncryptionOption EncryptionOption
	{
		get
		{
			if (StreamInfoDisposed)
			{
				return EncryptionOption.None;
			}
			EnsureTransformInformation();
			return _encryptionOption;
		}
	}

	/// <summary>Gets the name of the stream.</summary>
	/// <returns>The name of this stream.</returns>
	public string Name
	{
		get
		{
			if (StreamInfoDisposed)
			{
				return "";
			}
			return core.streamName;
		}
	}

	internal bool StreamInfoDisposed
	{
		get
		{
			if (core.streamName != null)
			{
				return parentStorage.StorageDisposed;
			}
			return true;
		}
	}

	internal CompoundFileStreamReference StreamReference => _streamReference;

	private void BuildStreamInfoRelativeToStorage(StorageInfo parent, string path)
	{
		parentStorage = parent;
		core = parentStorage.CoreForChildStream(path);
	}

	private StreamInfo(StorageRoot root, string streamPath)
		: this((StorageInfo)root, streamPath)
	{
	}

	internal StreamInfo(StorageInfo parent, string streamName)
		: this(parent, streamName, CompressionOption.NotCompressed, EncryptionOption.None)
	{
	}

	internal StreamInfo(StorageInfo parent, string streamName, CompressionOption compressionOption, EncryptionOption encryptionOption)
	{
		ContainerUtilities.CheckAgainstNull(parent, "parent");
		ContainerUtilities.CheckStringAgainstNullAndEmpty(streamName, "streamName");
		BuildStreamInfoRelativeToStorage(parent, streamName);
		_compressionOption = compressionOption;
		_encryptionOption = encryptionOption;
		_streamReference = new CompoundFileStreamReference(parentStorage.FullNameInternal, core.streamName);
	}

	/// <summary>Returns a stream opened in a default <see cref="T:System.IO.FileMode" /> and <see cref="T:System.IO.FileAccess" />.</summary>
	/// <returns>The I/O stream opened in a default <see cref="T:System.IO.Packaging.Package" /> root <see cref="T:System.IO.FileMode" /> and <see cref="T:System.IO.FileAccess" />..</returns>
	public Stream GetStream()
	{
		return GetStream(FileMode.OpenOrCreate, parentStorage.Root.OpenAccess);
	}

	/// <summary>Returns an I/O stream opened in a specified <see cref="T:System.IO.FileMode" />.</summary>
	/// <returns>The stream opened in the specified file <paramref name="mode" />.</returns>
	/// <param name="mode">The file mode in which to open the stream.</param>
	public Stream GetStream(FileMode mode)
	{
		return GetStream(mode, parentStorage.Root.OpenAccess);
	}

	/// <summary>Returns an I/O stream opened in a specified <see cref="T:System.IO.FileMode" /> and with a given <see cref="T:System.IO.FileAccess" />.</summary>
	/// <returns>The I/O stream opened in the specified <see cref="T:System.IO.FileMode" /> and with a given <see cref="T:System.IO.FileAccess" />.</returns>
	/// <param name="mode">The file mode in which to open the stream.</param>
	/// <param name="access">The file access mode in which to read or write to the stream.</param>
	public Stream GetStream(FileMode mode, FileAccess access)
	{
		CheckDisposedStatus();
		int grfMode = 0;
		IStream ppstm = null;
		openFileAccess = access;
		if (parentStorage.Root.OpenAccess == FileAccess.ReadWrite)
		{
			access = FileAccess.ReadWrite;
		}
		SafeNativeCompoundFileMethods.UpdateModeFlagFromFileAccess(access, ref grfMode);
		grfMode |= 0x10;
		CheckAccessMode(grfMode);
		switch (mode)
		{
		case FileMode.Append:
			throw new ArgumentException(SR.FileModeUnsupported);
		case FileMode.Create:
			CreateTimeReadOnlyCheck(openFileAccess);
			if (core.exposedStream != null)
			{
				((Stream)core.exposedStream).Close();
			}
			core.exposedStream = null;
			if (core.safeIStream != null)
			{
				((IDisposable)core.safeIStream).Dispose();
				core.safeIStream = null;
			}
			grfMode |= 0x1000;
			ppstm = CreateStreamOnParentIStorage(core.streamName, grfMode);
			break;
		case FileMode.CreateNew:
			throw new ArgumentException(SR.FileModeUnsupported);
		case FileMode.Open:
			if (core.safeIStream != null)
			{
				return CFStreamOfClone(openFileAccess);
			}
			ppstm = OpenStreamOnParentIStorage(core.streamName, grfMode);
			break;
		case FileMode.OpenOrCreate:
			if (core.safeIStream != null)
			{
				return CFStreamOfClone(openFileAccess);
			}
			if (FileAccess.Read != parentStorage.Root.OpenAccess && FileAccess.Read != openFileAccess)
			{
				if (!parentStorage.Exists)
				{
					parentStorage.Create();
				}
				int num = parentStorage.SafeIStorage.CreateStream(core.streamName, grfMode, 0, 0, out ppstm);
				if (num != 0 && -2147286960 != num)
				{
					throw new IOException(SR.UnableToCreateStream, new COMException(SR.Format(SR.NamedAPIFailure, "IStorage.CreateStream"), num));
				}
				parentStorage.InvalidateEnumerators();
			}
			if (ppstm == null)
			{
				ppstm = OpenStreamOnParentIStorage(core.streamName, grfMode);
			}
			break;
		case FileMode.Truncate:
			throw new ArgumentException(SR.FileModeUnsupported);
		default:
			throw new ArgumentException(SR.FileModeInvalid);
		}
		core.safeIStream = ppstm;
		Stream stream = BuildStreamOnUnderlyingIStream(core.safeIStream, openFileAccess, this);
		core.exposedStream = stream;
		return stream;
	}

	internal Stream Create()
	{
		return Create(FileMode.Create, parentStorage.Root.OpenAccess, null);
	}

	private Stream Create(FileMode mode)
	{
		return Create(mode, parentStorage.Root.OpenAccess, null);
	}

	internal Stream Create(string dataSpaceLabel)
	{
		return Create(FileMode.Create, parentStorage.Root.OpenAccess, dataSpaceLabel);
	}

	private Stream Create(FileMode mode, FileAccess access)
	{
		return Create(mode, access, null);
	}

	internal Stream Create(FileMode mode, FileAccess access, string dataSpace)
	{
		CheckDisposedStatus();
		int grfMode = 0;
		IStream stream = null;
		DataSpaceManager dataSpaceManager = null;
		CreateTimeReadOnlyCheck(access);
		if (dataSpace != null)
		{
			if (dataSpace.Length == 0)
			{
				throw new ArgumentException(SR.DataSpaceLabelInvalidEmpty);
			}
			dataSpaceManager = parentStorage.Root.GetDataSpaceManager();
			if (!dataSpaceManager.DataSpaceIsDefined(dataSpace))
			{
				throw new ArgumentException(SR.DataSpaceLabelUndefined);
			}
		}
		openFileAccess = access;
		if (parentStorage.Root.OpenAccess == FileAccess.ReadWrite)
		{
			access = FileAccess.ReadWrite;
		}
		SafeNativeCompoundFileMethods.UpdateModeFlagFromFileAccess(access, ref grfMode);
		grfMode |= 0x10;
		CheckAccessMode(grfMode);
		switch (mode)
		{
		case FileMode.Create:
			if (core.exposedStream != null)
			{
				((Stream)core.exposedStream).Close();
			}
			core.exposedStream = null;
			if (core.safeIStream != null)
			{
				((IDisposable)core.safeIStream).Dispose();
				core.safeIStream = null;
			}
			grfMode |= 0x1000;
			stream = CreateStreamOnParentIStorage(core.streamName, grfMode);
			break;
		case FileMode.CreateNew:
			if (core.safeIStream != null)
			{
				throw new IOException(SR.StreamAlreadyExist);
			}
			stream = CreateStreamOnParentIStorage(core.streamName, grfMode);
			break;
		default:
			throw new ArgumentException(SR.FileModeInvalid);
		}
		core.safeIStream = stream;
		core.dataSpaceLabel = dataSpace;
		if (dataSpace != null)
		{
			dataSpaceManager.CreateDataSpaceMapping(new CompoundFileStreamReference(parentStorage.FullNameInternal, core.streamName), core.dataSpaceLabel);
		}
		Stream stream2 = BuildStreamOnUnderlyingIStream(core.safeIStream, openFileAccess, this);
		_needToGetTransformInfo = false;
		core.exposedStream = stream2;
		return stream2;
	}

	private Stream BuildStreamOnUnderlyingIStream(IStream underlyingIStream, FileAccess access, StreamInfo parent)
	{
		Stream stream = new CFStream(underlyingIStream, access, parent);
		if (core.dataSpaceLabel == null)
		{
			return new BufferedStream(stream);
		}
		return parentStorage.Root.GetDataSpaceManager().CreateDataSpaceStream(StreamReference, stream);
	}

	private void CreateTimeReadOnlyCheck(FileAccess access)
	{
		if (FileAccess.Read == parentStorage.Root.OpenAccess)
		{
			throw new IOException(SR.CanNotCreateInReadOnly);
		}
		if (access == FileAccess.Read)
		{
			throw new ArgumentException(SR.CanNotCreateAsReadOnly);
		}
	}

	private IStream CreateStreamOnParentIStorage(string name, int mode)
	{
		IStream ppstm = null;
		int num = 0;
		if (!parentStorage.Exists)
		{
			parentStorage.Create();
		}
		num = parentStorage.SafeIStorage.CreateStream(name, mode, 0, 0, out ppstm);
		if (-2147286785 == num)
		{
			throw new ArgumentException(SR.StorageFlagsUnsupported);
		}
		if (num != 0)
		{
			throw new IOException(SR.UnableToCreateStream, new COMException(SR.Format(SR.NamedAPIFailure, "IStorage.CreateStream"), num));
		}
		parentStorage.InvalidateEnumerators();
		return ppstm;
	}

	private IStream OpenStreamOnParentIStorage(string name, int mode)
	{
		IStream ppstm = null;
		int num = 0;
		num = parentStorage.SafeIStorage.OpenStream(name, 0, mode, 0, out ppstm);
		if (num != 0)
		{
			throw new IOException(SR.UnableToOpenStream, new COMException(SR.Format(SR.NamedAPIFailure, "IStorage.OpenStream"), num));
		}
		return ppstm;
	}

	internal void Delete()
	{
		CheckDisposedStatus();
		if (InternalExists())
		{
			if (core.safeIStream != null)
			{
				((IDisposable)core.safeIStream).Dispose();
				core.safeIStream = null;
			}
			parentStorage.DestroyElement(core.streamName);
			parentStorage.InvalidateEnumerators();
		}
	}

	internal bool InternalExists()
	{
		if (core.safeIStream != null)
		{
			return true;
		}
		if (!parentStorage.Exists)
		{
			return false;
		}
		return parentStorage.SafeIStorage.OpenStream(core.streamName, 0, 16, 0, out core.safeIStream) == 0;
	}

	private void VerifyExists()
	{
		if (!InternalExists())
		{
			throw new IOException(SR.StreamNotExist);
		}
	}

	private Stream CFStreamOfClone(FileAccess access)
	{
		long plibNewPosition = 0L;
		IStream ppstm = null;
		core.safeIStream.Clone(out ppstm);
		ppstm.Seek(0L, 0, out plibNewPosition);
		Stream stream = BuildStreamOnUnderlyingIStream(ppstm, access, this);
		core.exposedStream = stream;
		return stream;
	}

	internal void CheckDisposedStatus()
	{
		if (StreamInfoDisposed)
		{
			throw new ObjectDisposedException(null, SR.StreamInfoDisposed);
		}
	}

	internal void CheckAccessMode(int grfMode)
	{
		if (core.safeIStream != null && core.exposedStream == null)
		{
			core.safeIStream.Stat(out var pstatstg, 1);
			if (grfMode != pstatstg.grfMode)
			{
				((IDisposable)core.safeIStream).Dispose();
				core.safeIStream = null;
			}
		}
	}

	private void EnsureTransformInformation()
	{
		if (!_needToGetTransformInfo || !InternalExists())
		{
			return;
		}
		_encryptionOption = EncryptionOption.None;
		_compressionOption = CompressionOption.NotCompressed;
		foreach (IDataTransform item in parentStorage.Root.GetDataSpaceManager().GetTransformsForStreamInfo(this))
		{
			if (item.TransformIdentifier is string text)
			{
				string strA = text.ToUpperInvariant();
				if (string.CompareOrdinal(strA, RightsManagementEncryptionTransform.ClassTransformIdentifier.ToUpperInvariant()) == 0 && item is RightsManagementEncryptionTransform)
				{
					_encryptionOption = EncryptionOption.RightsManagement;
				}
				else if (string.CompareOrdinal(strA, CompressionTransform.ClassTransformIdentifier.ToUpperInvariant()) == 0 && item is CompressionTransform)
				{
					_compressionOption = CompressionOption.Maximum;
				}
			}
		}
		_needToGetTransformInfo = false;
	}
}
