using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security.Principal;
using System.Text;
using System.Xml;
using Microsoft.Win32;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal static class PackagingUtilities
{
	private class SafeIsolatedStorageFileStream : IsolatedStorageFileStream
	{
		private string _path;

		private ReliableIsolatedStorageFileFolder _folder;

		private bool _disposed;

		internal SafeIsolatedStorageFileStream(string path, FileMode mode, FileAccess access, FileShare share, ReliableIsolatedStorageFileFolder folder)
			: base(path, mode, access, share, folder.IsoFile)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			_path = path;
			_folder = folder;
			_folder.AddRef();
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}
			if (disposing)
			{
				base.Dispose(disposing);
				if (_path != null)
				{
					DeleteIsolatedStorageFile(_path);
					_path = null;
				}
				_folder.DecRef();
				_folder = null;
				GC.SuppressFinalize(this);
			}
			_disposed = true;
		}
	}

	private class ReliableIsolatedStorageFileFolder : IDisposable
	{
		private static IsolatedStorageFile _file;

		private static bool _userHasProfile;

		private int _refCount;

		private bool _disposed;

		internal IsolatedStorageFile IsoFile
		{
			get
			{
				CheckDisposed();
				return _file;
			}
		}

		internal void AddRef()
		{
			checked
			{
				lock (IsoStoreSyncRoot)
				{
					CheckDisposed();
					_refCount++;
				}
			}
		}

		internal void DecRef()
		{
			checked
			{
				lock (IsoStoreSyncRoot)
				{
					CheckDisposed();
					_refCount--;
					if (_refCount <= 0)
					{
						Dispose();
					}
				}
			}
		}

		internal bool IsDisposed()
		{
			return _disposed;
		}

		internal ReliableIsolatedStorageFileFolder()
		{
			_userHasProfile = UserHasProfile();
			_file = GetCurrentStore();
		}

		internal Stream GetStream(string fileName)
		{
			CheckDisposed();
			return new SafeIsolatedStorageFileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None, this);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}

		protected virtual void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					lock (IsoStoreSyncRoot)
					{
						IsolatedStorageFile file = _file;
						_file = null;
						GC.SuppressFinalize(this);
						if (!_disposed)
						{
							_disposed = true;
							using (file)
							{
								lock (IsolatedStorageFileLock)
								{
									file.Remove();
									return;
								}
							}
						}
						return;
					}
				}
				using IsolatedStorageFile isolatedStorageFile2 = GetCurrentStore();
				isolatedStorageFile2.Remove();
			}
			catch (IsolatedStorageException)
			{
			}
		}

		private IsolatedStorageFile GetCurrentStore()
		{
			if (_userHasProfile)
			{
				return IsolatedStorageFile.GetUserStoreForDomain();
			}
			return IsolatedStorageFile.GetMachineStoreForDomain();
		}

		~ReliableIsolatedStorageFileFolder()
		{
			Dispose(disposing: false);
		}

		private void CheckDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("ReliableIsolatedStorageFileFolder");
			}
		}
	}

	internal static readonly string RelationshipNamespaceUri = "http://schemas.openxmlformats.org/package/2006/relationships";

	internal static readonly ContentType RelationshipPartContentType = new ContentType("application/vnd.openxmlformats-package.relationships+xml");

	internal const string ContainerFileExtension = "xps";

	internal const string XamlFileExtension = "xaml";

	private static object _isoStoreSyncObject = new object();

	private static object _isolatedStorageFileLock = new object();

	private static ReliableIsolatedStorageFileFolder _defaultFile;

	private const string XmlNamespace = "xmlns";

	private const string _encodingAttribute = "encoding";

	private static readonly string _webNameUTF8 = Encoding.UTF8.WebName.ToUpperInvariant();

	private static readonly string _webNameUnicode = Encoding.Unicode.WebName.ToUpperInvariant();

	private const string _profileListKeyName = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList";

	private const string _fullProfileListKeyName = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList";

	internal static object IsoStoreSyncRoot => _isoStoreSyncObject;

	internal static object IsolatedStorageFileLock => _isolatedStorageFileLock;

	internal static void PerformInitailReadAndVerifyEncoding(XmlTextReader reader)
	{
		Invariant.Assert(reader != null && reader.ReadState == ReadState.Initial);
		if (reader.Read() && reader.NodeType == XmlNodeType.XmlDeclaration && reader.Depth == 0)
		{
			string attribute = reader.GetAttribute("encoding");
			if (attribute != null && attribute.Length > 0)
			{
				attribute = attribute.ToUpperInvariant();
				if (string.Equals(attribute, _webNameUTF8, StringComparison.Ordinal) || string.Equals(attribute, _webNameUnicode, StringComparison.Ordinal))
				{
					return;
				}
				throw new FileFormatException(SR.EncodingNotSupported);
			}
		}
		if (!(reader.Encoding is UnicodeEncoding) && !(reader.Encoding is UTF8Encoding))
		{
			throw new FileFormatException(SR.EncodingNotSupported);
		}
	}

	internal static void VerifyStreamReadArgs(Stream s, byte[] buffer, int offset, int count)
	{
		if (!s.CanRead)
		{
			throw new NotSupportedException(SR.ReadNotSupported);
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", SR.OffsetNegative);
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", SR.ReadCountNegative);
		}
		if (checked(offset + count) > buffer.Length)
		{
			throw new ArgumentException(SR.ReadBufferTooSmall, "buffer");
		}
	}

	internal static void VerifyStreamWriteArgs(Stream s, byte[] buffer, int offset, int count)
	{
		if (!s.CanWrite)
		{
			throw new NotSupportedException(SR.WriteNotSupported);
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", SR.OffsetNegative);
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", SR.WriteCountNegative);
		}
		if (checked(offset + count) > buffer.Length)
		{
			throw new ArgumentException(SR.WriteBufferTooSmall, "buffer");
		}
	}

	internal static int ReliableRead(Stream stream, byte[] buffer, int offset, int count)
	{
		return ReliableRead(stream, buffer, offset, count, count);
	}

	internal static int ReliableRead(Stream stream, byte[] buffer, int offset, int requestedCount, int requiredCount)
	{
		Invariant.Assert(stream != null);
		Invariant.Assert(buffer != null);
		Invariant.Assert(buffer.Length != 0);
		Invariant.Assert(offset >= 0);
		Invariant.Assert(requestedCount >= 0);
		Invariant.Assert(requiredCount >= 0);
		Invariant.Assert(checked(offset + requestedCount) <= buffer.Length);
		Invariant.Assert(requiredCount <= requestedCount);
		int i;
		int num;
		for (i = 0; i < requiredCount; i += num)
		{
			num = stream.Read(buffer, offset + i, requestedCount - i);
			if (num == 0)
			{
				break;
			}
		}
		return i;
	}

	internal static int ReliableRead(BinaryReader reader, byte[] buffer, int offset, int count)
	{
		return ReliableRead(reader, buffer, offset, count, count);
	}

	internal static int ReliableRead(BinaryReader reader, byte[] buffer, int offset, int requestedCount, int requiredCount)
	{
		Invariant.Assert(reader != null);
		Invariant.Assert(buffer != null);
		Invariant.Assert(buffer.Length != 0);
		Invariant.Assert(offset >= 0);
		Invariant.Assert(requestedCount >= 0);
		Invariant.Assert(requiredCount >= 0);
		Invariant.Assert(checked(offset + requestedCount) <= buffer.Length);
		Invariant.Assert(requiredCount <= requestedCount);
		int i;
		int num;
		for (i = 0; i < requiredCount; i += num)
		{
			num = reader.Read(buffer, offset + i, requestedCount - i);
			if (num == 0)
			{
				break;
			}
		}
		return i;
	}

	internal static long CopyStream(Stream sourceStream, Stream targetStream, long bytesToCopy, int bufferSize)
	{
		Invariant.Assert(sourceStream != null);
		Invariant.Assert(targetStream != null);
		Invariant.Assert(bytesToCopy >= 0);
		Invariant.Assert(bufferSize > 0);
		byte[] buffer = new byte[bufferSize];
		long num = bytesToCopy;
		while (num > 0)
		{
			int num2 = sourceStream.Read(buffer, 0, (int)Math.Min(num, bufferSize));
			if (num2 == 0)
			{
				targetStream.Flush();
				return bytesToCopy - num;
			}
			targetStream.Write(buffer, 0, num2);
			num -= num2;
		}
		targetStream.Flush();
		return bytesToCopy;
	}

	internal static Stream CreateUserScopedIsolatedStorageFileStreamWithRandomName(int retryCount, out string fileName)
	{
		if (retryCount < 0 || retryCount > 100)
		{
			throw new ArgumentOutOfRangeException("retryCount");
		}
		Stream stream = null;
		fileName = null;
		while (true)
		{
			try
			{
				fileName = Path.GetRandomFileName();
				lock (IsoStoreSyncRoot)
				{
					lock (IsolatedStorageFileLock)
					{
						return GetDefaultIsolatedStorageFile().GetStream(fileName);
					}
				}
			}
			catch (IOException)
			{
				if (--retryCount < 0)
				{
					throw;
				}
			}
		}
	}

	internal static void CalculateOverlap(long block1Offset, long block1Size, long block2Offset, long block2Size, out long overlapBlockOffset, out long overlapBlockSize)
	{
		overlapBlockOffset = Math.Max(block1Offset, block2Offset);
		overlapBlockSize = checked(Math.Min(block1Offset + block1Size, block2Offset + block2Size) - overlapBlockOffset);
		if (overlapBlockSize <= 0)
		{
			overlapBlockSize = 0L;
		}
	}

	internal static int GetNonXmlnsAttributeCount(XmlReader reader)
	{
		int num = 0;
		while (reader.MoveToNextAttribute())
		{
			if (string.CompareOrdinal(reader.Name, "xmlns") != 0 && string.CompareOrdinal(reader.Prefix, "xmlns") != 0)
			{
				num++;
			}
		}
		reader.MoveToElement();
		return num;
	}

	private static void DeleteIsolatedStorageFile(string fileName)
	{
		lock (IsoStoreSyncRoot)
		{
			lock (IsolatedStorageFileLock)
			{
				try
				{
					GetDefaultIsolatedStorageFile().IsoFile.DeleteFile(fileName);
				}
				catch (IsolatedStorageException)
				{
				}
			}
		}
	}

	private static ReliableIsolatedStorageFileFolder GetDefaultIsolatedStorageFile()
	{
		if (_defaultFile == null || _defaultFile.IsDisposed())
		{
			_defaultFile = new ReliableIsolatedStorageFileFolder();
		}
		return _defaultFile;
	}

	private static bool UserHasProfile()
	{
		bool flag = false;
		RegistryKey registryKey = null;
		try
		{
			string value = WindowsIdentity.GetCurrent().User.Value;
			registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList\\" + value);
			return registryKey != null;
		}
		finally
		{
			registryKey?.Close();
		}
	}
}
