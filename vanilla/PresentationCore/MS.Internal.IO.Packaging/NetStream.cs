using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading;
using MS.Internal.PresentationCore;

namespace MS.Internal.IO.Packaging;

internal class NetStream : Stream
{
	private class Block : IComparable
	{
		private long _offset;

		private int _length;

		internal long End => checked(_offset + _length);

		internal long Offset
		{
			get
			{
				return _offset;
			}
			set
			{
				_offset = value;
			}
		}

		internal int Length
		{
			get
			{
				return _length;
			}
			set
			{
				_length = value;
			}
		}

		internal Block(long offset, int length)
		{
			_offset = offset;
			_length = length;
		}

		int IComparable.CompareTo(object x)
		{
			Block block = (Block)x;
			if (_offset < block._offset)
			{
				return -1;
			}
			if (_offset > block._offset)
			{
				return 1;
			}
			if (_length == block._length)
			{
				return 0;
			}
			if (_length < block._length)
			{
				return -1;
			}
			return 1;
		}

		internal bool Mergeable(Block b)
		{
			checked
			{
				if (_offset <= b._offset)
				{
					return _offset + _length - b._offset >= 0;
				}
				return b._offset + b._length - _offset >= 0;
			}
		}

		internal void Merge(Block b)
		{
			_length = checked((int)(Math.Max(_offset + _length, b._offset + b._length) - _offset));
		}
	}

	private enum ReadEvent
	{
		FullDownloadReadEvent,
		ByteRangeReadEvent,
		MaxReadEventEnum
	}

	private Uri _uri;

	private WebRequest _originalRequest;

	private Stream _tempFileStream;

	private long _position;

	private object _syncObject = new object();

	private volatile bool _disposed;

	private const int _readTimeOut = 40000;

	private const int _additionalRequestMinSize = 4096;

	private const int _bufferSize = 4096;

	private const int _tempFileSyncTimeout = 5000;

	private uint _additionalRequestThreshold = 16384u;

	private Stream _responseStream;

	private byte[] _readBuf;

	private string _tempFileName;

	private long _fullStreamLength;

	private volatile bool _fullDownloadComplete;

	private long _highWaterMark;

	private EventWaitHandle[] _readEventHandles = new EventWaitHandle[2];

	private Mutex _tempFileMutex = new Mutex(initiallyOwned: false);

	private bool _allowByteRangeRequests;

	private ByteRangeDownloader _byteRangeDownloader;

	private bool _inAdditionalRequest;

	private ArrayList _byteRangesAvailable;

	public override bool CanRead => !_disposed;

	public override bool CanSeek => !_disposed;

	public override bool CanWrite => false;

	public override long Position
	{
		get
		{
			CheckDisposed();
			return _position;
		}
		set
		{
			CheckDisposed();
			if (value < 0)
			{
				throw new ArgumentException(SR.SeekNegative);
			}
			_position = value;
		}
	}

	public override long Length
	{
		get
		{
			CheckDisposed();
			if (_fullStreamLength < 0)
			{
				long position = _position;
				_position = _highWaterMark;
				byte[] array = new byte[4096];
				while (Read(array, 0, array.Length) > 0)
				{
				}
				_position = position;
			}
			return _fullStreamLength;
		}
	}

	internal NetStream(Stream responseStream, long fullStreamLength, Uri uri, WebRequest originalRequest, WebResponse originalResponse)
	{
		Invariant.Assert(uri != null);
		Invariant.Assert(responseStream != null);
		Invariant.Assert(originalRequest != null);
		Invariant.Assert(originalResponse != null);
		_uri = uri;
		_fullStreamLength = fullStreamLength;
		_responseStream = responseStream;
		_originalRequest = originalRequest;
		if (fullStreamLength > 0 && (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal) || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal)))
		{
			_allowByteRangeRequests = true;
			_readEventHandles[1] = new AutoResetEvent(initialState: false);
		}
		_readEventHandles[0] = new AutoResetEvent(initialState: false);
		StartFullDownload();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		CheckDisposed();
		PackagingUtilities.VerifyStreamReadArgs(this, buffer, offset, count);
		if (count == 0)
		{
			return count;
		}
		int num = 0;
		checked
		{
			if (offset + count > buffer.Length)
			{
				throw new ArgumentException(SR.IOBufferOverflow, "buffer");
			}
			count = Math.Min(GetData(new Block(_position, count)), count);
			if (count > 0)
			{
				try
				{
					_tempFileMutex.WaitOne();
					lock (PackagingUtilities.IsolatedStorageFileLock)
					{
						_tempFileStream.Seek(_position, SeekOrigin.Begin);
						num = _tempFileStream.Read(buffer, offset, count);
					}
				}
				finally
				{
					_tempFileMutex.ReleaseMutex();
				}
				_position += num;
			}
			return num;
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		CheckDisposed();
		long num = 0L;
		num = checked(origin switch
		{
			SeekOrigin.Begin => offset, 
			SeekOrigin.Current => _position + offset, 
			SeekOrigin.End => Length + offset, 
			_ => throw new ArgumentOutOfRangeException("origin", SR.SeekOriginInvalid), 
		});
		if (num < 0)
		{
			throw new ArgumentException(SR.SeekNegative);
		}
		_position = num;
		return _position;
	}

	public override void SetLength(long newLength)
	{
		throw new NotSupportedException(SR.SetLengthNotSupported);
	}

	public override void Write(byte[] buf, int offset, int count)
	{
		throw new NotSupportedException(SR.WriteNotSupported);
	}

	public override void Flush()
	{
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!disposing)
			{
				return;
			}
			lock (_syncObject)
			{
				if (_disposed)
				{
					return;
				}
				try
				{
					_disposed = true;
					if (_readEventHandles[0] != null)
					{
						_readEventHandles[0].Set();
					}
					if (_readEventHandles[1] != null)
					{
						_readEventHandles[1].Set();
					}
					FreeByteRangeDownloader();
					if (_readEventHandles[0] != null)
					{
						_readEventHandles[0].Close();
						_readEventHandles[0] = null;
					}
					if (_readEventHandles[1] != null)
					{
						_readEventHandles[1].Close();
						_readEventHandles[1] = null;
					}
					if (_responseStream != null)
					{
						_responseStream.Close();
					}
					FreeTempFile();
				}
				finally
				{
					_responseStream = null;
					_readEventHandles = null;
					_byteRangesAvailable = null;
					_readBuf = null;
				}
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	private void StartFullDownload()
	{
		_highWaterMark = 0L;
		_readBuf = new byte[4096];
		lock (PackagingUtilities.IsoStoreSyncRoot)
		{
			lock (PackagingUtilities.IsolatedStorageFileLock)
			{
				_tempFileStream = PackagingUtilities.CreateUserScopedIsolatedStorageFileStreamWithRandomName(3, out _tempFileName);
			}
		}
		_responseStream.BeginRead(_readBuf, 0, _readBuf.Length, ReadCallBack, this);
	}

	private void CheckDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("Stream");
		}
	}

	private void ReadCallBack(IAsyncResult ar)
	{
		checked
		{
			lock (_syncObject)
			{
				try
				{
					if (_disposed)
					{
						return;
					}
					int num = _responseStream.EndRead(ar);
					if (num > 0)
					{
						try
						{
							_tempFileMutex.WaitOne();
							lock (PackagingUtilities.IsolatedStorageFileLock)
							{
								_tempFileStream.Seek(_highWaterMark, SeekOrigin.Begin);
								_tempFileStream.Write(_readBuf, 0, num);
								_tempFileStream.Flush();
							}
							_highWaterMark += num;
						}
						finally
						{
							_tempFileMutex.ReleaseMutex();
						}
					}
					else if (_fullStreamLength < 0)
					{
						_fullStreamLength = _highWaterMark;
					}
					if (_fullStreamLength == _highWaterMark)
					{
						_fullDownloadComplete = true;
					}
				}
				finally
				{
					if (!_disposed && _readEventHandles[0] != null)
					{
						_readEventHandles[0].Set();
					}
				}
			}
		}
	}

	private void EnsureDownloader()
	{
		if (_byteRangeDownloader == null)
		{
			_byteRangeDownloader = new ByteRangeDownloader(_uri, _tempFileStream, _readEventHandles[1].SafeWaitHandle, _tempFileMutex);
			_byteRangeDownloader.Proxy = _originalRequest.Proxy;
			_byteRangeDownloader.Credentials = _originalRequest.Credentials;
			_byteRangeDownloader.CachePolicy = _originalRequest.CachePolicy;
			_byteRangesAvailable = new ArrayList();
		}
	}

	private void MakeByteRangeRequest(Block block)
	{
		if (block.Offset <= int.MaxValue - block.Length + 1)
		{
			EnsureDownloader();
			if (block.Length < 4096)
			{
				block.Length = 4096;
			}
			TrimBlockToStreamLength(block);
			if (block.Length > 0)
			{
				int[,] byteRanges = new int[1, 2] { 
				{
					checked((int)block.Offset),
					block.Length
				} };
				_byteRangeDownloader.RequestByteRanges(byteRanges);
				_inAdditionalRequest = true;
			}
		}
	}

	private void GetByteRangeData()
	{
		int[,] downloadedByteRanges = _byteRangeDownloader.GetDownloadedByteRanges();
		if (downloadedByteRanges.GetLength(0) > 0)
		{
			_byteRangesAvailable.Insert(0, new Block(0L, (int)_highWaterMark));
			for (int i = 0; i < downloadedByteRanges.GetLength(0); i++)
			{
				_byteRangesAvailable.Add(new Block(downloadedByteRanges[i, 0], downloadedByteRanges[i, 1]));
			}
			_byteRangesAvailable.Sort();
			MergeByteRanges(_byteRangesAvailable);
			_inAdditionalRequest = false;
		}
	}

	private int BytesInByteRangeAvailable(Block block)
	{
		int num = 0;
		if (_byteRangesAvailable != null)
		{
			TrimBlockToStreamLength(block);
			foreach (Block item in _byteRangesAvailable)
			{
				if (item.Offset <= block.Offset && item.End > block.Offset)
				{
					num = Math.Min(block.Length, checked((int)(item.End - block.Offset)));
				}
				if (num > 0 || item.Offset >= block.End)
				{
					break;
				}
			}
		}
		return num;
	}

	private int TrimByteRangeRequest(Block block)
	{
		int num = 0;
		checked
		{
			if (_byteRangesAvailable != null)
			{
				foreach (Block item in _byteRangesAvailable)
				{
					if (block.End <= item.Offset)
					{
						break;
					}
					if (block.Offset >= item.Offset && item.End > block.Offset)
					{
						if (block.End <= item.End)
						{
							num = block.Length;
						}
						else
						{
							num = (int)(item.End - block.Offset);
							block.Offset = item.End;
						}
						block.Length -= num;
					}
					if (block.Offset <= item.Offset && block.End > item.Offset && block.End <= item.End)
					{
						block.Length = (int)(item.Offset - block.Offset);
					}
					if (num > 0)
					{
						break;
					}
				}
			}
			return num;
		}
	}

	private void MergeByteRanges(ArrayList ranges)
	{
		checked
		{
			for (int i = 0; i + 1 < ranges.Count; i++)
			{
				Block block = (Block)ranges[i];
				while (block.Mergeable((Block)ranges[i + 1]))
				{
					block.Merge((Block)ranges[i + 1]);
					ranges.RemoveAt(i + 1);
					if (i + 1 >= ranges.Count)
					{
						break;
					}
				}
			}
		}
	}

	private int HandleByteRangeReadEvent(Block block)
	{
		int num = 0;
		checked
		{
			if (_highWaterMark > block.Offset)
			{
				num = (int)Math.Min(block.Length, _highWaterMark - block.Offset);
			}
			if (num == block.Length)
			{
				_additionalRequestThreshold *= 2u;
			}
			else if (!_byteRangeDownloader.ErroredOut)
			{
				GetByteRangeData();
				num = BytesInByteRangeAvailable(block);
			}
			else
			{
				_allowByteRangeRequests = false;
			}
			return num;
		}
	}

	private int HandleFullDownloadReadEvent(Block block)
	{
		int result = 0;
		if (_fullDownloadComplete)
		{
			TrimBlockToStreamLength(block);
			result = block.Length;
		}
		else
		{
			_responseStream.BeginRead(_readBuf, 0, _readBuf.Length, ReadCallBack, this);
			if (_highWaterMark > block.Offset)
			{
				result = checked((int)Math.Min(block.Length, _highWaterMark - block.Offset));
			}
		}
		return result;
	}

	private int GetData(Block block)
	{
		TrimBlockToStreamLength(block);
		if (block.Length == 0)
		{
			return 0;
		}
		int num = 0;
		while (num == 0)
		{
			lock (_syncObject)
			{
				if (_highWaterMark > block.Offset)
				{
					num = (int)Math.Min(block.Length, _highWaterMark - block.Offset);
				}
				else
				{
					num = TrimByteRangeRequest(block);
					if (_allowByteRangeRequests && !_inAdditionalRequest && _highWaterMark <= long.MaxValue - (long)_additionalRequestThreshold && block.Offset > _highWaterMark + _additionalRequestThreshold && (_byteRangeDownloader == null || !_byteRangeDownloader.ErroredOut) && block.Length > 0)
					{
						MakeByteRangeRequest(block);
					}
				}
			}
			if (num != 0)
			{
				continue;
			}
			ReadEvent readEvent;
			if (_allowByteRangeRequests)
			{
				WaitHandle[] readEventHandles = _readEventHandles;
				int num2 = WaitHandle.WaitAny(readEventHandles);
				if (num2 > 128)
				{
					num2 -= 128;
				}
				readEvent = (ReadEvent)num2;
			}
			else
			{
				readEvent = ReadEvent.FullDownloadReadEvent;
				_readEventHandles[(int)readEvent].WaitOne();
			}
			lock (_syncObject)
			{
				if (readEvent == ReadEvent.ByteRangeReadEvent)
				{
					num = HandleByteRangeReadEvent(block);
					continue;
				}
				num = HandleFullDownloadReadEvent(block);
				if (_fullDownloadComplete)
				{
					ReleaseFullDownloadResources();
					break;
				}
			}
		}
		return num;
	}

	private void TrimBlockToStreamLength(Block block)
	{
		if (_fullStreamLength >= 0)
		{
			block.Length = checked((int)Math.Min(block.Length, _fullStreamLength - block.Offset));
		}
	}

	private void ReleaseFullDownloadResources()
	{
		if (_readBuf == null)
		{
			return;
		}
		_byteRangesAvailable = null;
		_readBuf = null;
		try
		{
			try
			{
				FreeByteRangeDownloader();
				if (_readEventHandles[0] != null)
				{
					_readEventHandles[0].Close();
					_readEventHandles[0] = null;
				}
			}
			finally
			{
				if (_responseStream != null)
				{
					_responseStream.Close();
				}
			}
		}
		finally
		{
			_responseStream = null;
		}
	}

	private void FreeByteRangeDownloader()
	{
		if (_byteRangeDownloader == null)
		{
			return;
		}
		try
		{
			((IDisposable)_byteRangeDownloader).Dispose();
			if (_readEventHandles[1] != null)
			{
				_readEventHandles[1].Close();
				_readEventHandles[1] = null;
			}
		}
		finally
		{
			_byteRangeDownloader = null;
		}
	}

	private void FreeTempFile()
	{
		bool flag = false;
		Invariant.Assert(_tempFileStream != null);
		try
		{
			flag = _tempFileMutex.WaitOne(5000, exitContext: false);
			lock (PackagingUtilities.IsolatedStorageFileLock)
			{
				_tempFileStream.Close();
			}
		}
		finally
		{
			if (flag)
			{
				_tempFileMutex.ReleaseMutex();
				_tempFileMutex.Close();
			}
			_tempFileStream = null;
			_tempFileName = null;
			_tempFileMutex = null;
		}
	}
}
