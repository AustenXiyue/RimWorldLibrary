using System.Runtime.InteropServices;
using MS.Internal;
using MS.Internal.IO.Packaging.CompoundFile;
using MS.Internal.WindowsBase;

namespace System.IO.Packaging;

internal class StorageRoot : StorageInfo
{
	private const FileMode defaultFileMode = FileMode.OpenOrCreate;

	private const FileAccess defaultFileAccess = FileAccess.ReadWrite;

	private const FileShare defaultFileShare = FileShare.None;

	private const int defaultSectorSize = 512;

	private const int stgFormatDocFile = 5;

	private IStorage rootIStorage;

	private DataSpaceManager dataSpaceManager;

	private bool containerIsReadOnly;

	private bool dataSpaceManagerInitializationInProgress;

	internal FileAccess OpenAccess
	{
		get
		{
			CheckRootDisposedStatus();
			if (containerIsReadOnly)
			{
				return FileAccess.Read;
			}
			return FileAccess.ReadWrite;
		}
	}

	internal bool RootDisposed => rootIStorage == null;

	private StorageRoot(IStorage root, bool readOnly)
		: base(root)
	{
		rootIStorage = root;
		containerIsReadOnly = readOnly;
		dataSpaceManagerInitializationInProgress = false;
	}

	internal static StorageRoot CreateOnStream(Stream baseStream)
	{
		if (baseStream == null)
		{
			throw new ArgumentNullException("baseStream");
		}
		if (baseStream.Length == 0L)
		{
			return CreateOnStream(baseStream, FileMode.Create);
		}
		return CreateOnStream(baseStream, FileMode.Open);
	}

	internal static StorageRoot CreateOnStream(Stream baseStream, FileMode mode)
	{
		if (baseStream == null)
		{
			throw new ArgumentNullException("baseStream");
		}
		int num = 16;
		if (baseStream.CanRead)
		{
			if (baseStream.CanWrite)
			{
				num |= 2;
			}
			else
			{
				num |= 0;
				if (FileMode.Create == mode)
				{
					throw new ArgumentException(SR.CanNotCreateContainerOnReadOnlyStream);
				}
			}
			int num2;
			IStorage ppstgOpen;
			if (FileMode.Create == mode)
			{
				num2 = SafeNativeCompoundFileMethods.SafeStgCreateDocfileOnStream(baseStream, num | 0x1000, out ppstgOpen);
			}
			else
			{
				if (FileMode.Open != mode)
				{
					throw new ArgumentException(SR.CreateModeMustBeCreateOrOpen);
				}
				num2 = SafeNativeCompoundFileMethods.SafeStgOpenStorageOnStream(baseStream, num, out ppstgOpen);
			}
			if (num2 == 0)
			{
				return CreateOnIStorage(ppstgOpen);
			}
			throw new IOException(SR.UnableToCreateOnStream, new COMException(SR.CFAPIFailure, num2));
		}
		throw new ArgumentException(SR.CanNotCreateStorageRootOnNonReadableStream);
	}

	internal static StorageRoot Open(string path)
	{
		return Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 512);
	}

	internal static StorageRoot Open(string path, FileMode mode)
	{
		return Open(path, mode, FileAccess.ReadWrite, FileShare.None, 512);
	}

	internal static StorageRoot Open(string path, FileMode mode, FileAccess access)
	{
		return Open(path, mode, access, FileShare.None, 512);
	}

	internal static StorageRoot Open(string path, FileMode mode, FileAccess access, FileShare share)
	{
		return Open(path, mode, access, share, 512);
	}

	internal static StorageRoot Open(string path, FileMode mode, FileAccess access, FileShare share, int sectorSize)
	{
		int grfMode = 0;
		int num = 0;
		ContainerUtilities.CheckStringAgainstNullAndEmpty(path, "Path");
		Guid riid = new Guid(11, 0, 0, 192, 0, 0, 0, 0, 0, 0, 70);
		switch (mode)
		{
		case FileMode.Append:
			throw new ArgumentException(SR.FileModeUnsupported);
		case FileMode.Create:
			grfMode |= 0x1000;
			break;
		case FileMode.CreateNew:
			if (new FileInfo(path).Exists)
			{
				throw new IOException(SR.FileAlreadyExists);
			}
			goto case FileMode.Create;
		case FileMode.OpenOrCreate:
			if (new FileInfo(path).Exists)
			{
				break;
			}
			goto case FileMode.Create;
		case FileMode.Truncate:
			throw new ArgumentException(SR.FileModeUnsupported);
		default:
			throw new ArgumentException(SR.FileModeInvalid);
		case FileMode.Open:
			break;
		}
		SafeNativeCompoundFileMethods.UpdateModeFlagFromFileAccess(access, ref grfMode);
		if ((share & FileShare.Inheritable) != 0)
		{
			throw new ArgumentException(SR.FileShareUnsupported);
		}
		grfMode = share switch
		{
			FileShare.None => grfMode | 0x10, 
			FileShare.Read => grfMode | 0x20, 
			FileShare.Write => grfMode | 0x30, 
			FileShare.ReadWrite => grfMode | 0x40, 
			_ => throw new ArgumentException(SR.FileShareInvalid), 
		};
		num = (((grfMode & 0x1000) == 0) ? SafeNativeCompoundFileMethods.SafeStgOpenStorageEx(path, grfMode, 5, 0, IntPtr.Zero, IntPtr.Zero, ref riid, out var ppObjectOpen) : SafeNativeCompoundFileMethods.SafeStgCreateStorageEx(path, grfMode, 5, 0, IntPtr.Zero, IntPtr.Zero, ref riid, out ppObjectOpen));
		return num switch
		{
			0 => CreateOnIStorage(ppObjectOpen), 
			-2147287038 => throw new FileNotFoundException(SR.ContainerNotFound), 
			-2147286785 => throw new ArgumentException(SR.StorageFlagsUnsupported, new COMException(SR.CFAPIFailure, num)), 
			_ => throw new IOException(SR.ContainerCanNotOpen, new COMException(SR.CFAPIFailure, num)), 
		};
	}

	internal void Close()
	{
		if (rootIStorage == null)
		{
			return;
		}
		if (dataSpaceManager != null)
		{
			dataSpaceManager.Dispose();
			dataSpaceManager = null;
		}
		try
		{
			if (!containerIsReadOnly)
			{
				rootIStorage.Commit(0);
			}
		}
		finally
		{
			StorageInfo.RecursiveStorageInfoCoreRelease(core);
			rootIStorage = null;
		}
	}

	internal void Flush()
	{
		CheckRootDisposedStatus();
		if (!containerIsReadOnly)
		{
			rootIStorage.Commit(0);
		}
	}

	internal DataSpaceManager GetDataSpaceManager()
	{
		CheckRootDisposedStatus();
		if (dataSpaceManager == null)
		{
			if (dataSpaceManagerInitializationInProgress)
			{
				return null;
			}
			dataSpaceManagerInitializationInProgress = true;
			dataSpaceManager = new DataSpaceManager(this);
			dataSpaceManagerInitializationInProgress = false;
		}
		return dataSpaceManager;
	}

	internal IStorage GetRootIStorage()
	{
		return rootIStorage;
	}

	internal void CheckRootDisposedStatus()
	{
		if (RootDisposed)
		{
			throw new ObjectDisposedException(null, SR.StorageRootDisposed);
		}
	}

	private static StorageRoot CreateOnIStorage(IStorage root)
	{
		Invariant.Assert(root != null);
		root.Stat(out var pstatstg, 1);
		bool readOnly = 1 != (pstatstg.grfMode & 1) && 2 != (pstatstg.grfMode & 2);
		return new StorageRoot(root, readOnly);
	}
}
