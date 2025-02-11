using System;
using System.Collections.Generic;
using System.IO;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

internal class SparseMemoryStream : Stream
{
	private TrackingMemoryStreamFactory _trackingMemoryStreamFactory = new TrackingMemoryStreamFactory();

	private string _isolatedStorageStreamFileName;

	private Stream _isolatedStorageStream;

	private const int _fixBlockInMemoryOverhead = 100;

	private bool _disposedFlag;

	private bool _isolatedStorageMode;

	private long _currentStreamLength;

	private long _currentStreamPosition;

	private List<MemoryStreamBlock> _memoryStreamList;

	private MemoryStreamBlock _searchBlock;

	private long _lowWaterMark;

	private long _highWaterMark;

	private bool _autoCloseSmallBlockGaps;

	public override bool CanRead => !_disposedFlag;

	public override bool CanSeek => !_disposedFlag;

	public override bool CanWrite => !_disposedFlag;

	public override long Length
	{
		get
		{
			CheckDisposed();
			return _currentStreamLength;
		}
	}

	public override long Position
	{
		get
		{
			CheckDisposed();
			return _currentStreamPosition;
		}
		set
		{
			CheckDisposed();
			Seek(value, SeekOrigin.Begin);
		}
	}

	internal List<MemoryStreamBlock> MemoryBlockCollection
	{
		get
		{
			CheckDisposed();
			return _memoryStreamList;
		}
	}

	internal long MemoryConsumption
	{
		get
		{
			CheckDisposed();
			return _trackingMemoryStreamFactory.CurrentMemoryConsumption;
		}
	}

	public override void SetLength(long newLength)
	{
		CheckDisposed();
		if (newLength < 0)
		{
			throw new ArgumentOutOfRangeException("newLength");
		}
		if (_currentStreamLength != newLength)
		{
			if (_isolatedStorageMode)
			{
				lock (PackagingUtilities.IsolatedStorageFileLock)
				{
					_isolatedStorageStream.SetLength(newLength);
				}
			}
			else if (_currentStreamLength > newLength)
			{
				int num = _memoryStreamList.BinarySearch(GetSearchBlockForOffset(newLength));
				checked
				{
					if (num < 0)
					{
						num = ~num;
					}
					else
					{
						MemoryStreamBlock memoryStreamBlock = _memoryStreamList[num];
						long num2 = newLength - memoryStreamBlock.Offset;
						if (num2 > 0)
						{
							memoryStreamBlock.Stream.SetLength(num2);
							num++;
						}
					}
				}
				for (int i = num; i < _memoryStreamList.Count; i++)
				{
					_memoryStreamList[i].Stream.Close();
				}
				_memoryStreamList.RemoveRange(num, _memoryStreamList.Count - num);
			}
			_currentStreamLength = newLength;
			if (_currentStreamPosition > _currentStreamLength)
			{
				_currentStreamPosition = _currentStreamLength;
			}
		}
		SwitchModeIfNecessary();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		CheckDisposed();
		long currentStreamPosition = _currentStreamPosition;
		currentStreamPosition = checked(origin switch
		{
			SeekOrigin.Begin => offset, 
			SeekOrigin.Current => currentStreamPosition + offset, 
			SeekOrigin.End => _currentStreamLength + offset, 
			_ => throw new ArgumentOutOfRangeException("origin"), 
		});
		if (currentStreamPosition < 0)
		{
			throw new ArgumentException(SR.SeekNegative);
		}
		_currentStreamPosition = currentStreamPosition;
		return _currentStreamPosition;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		CheckDisposed();
		PackagingUtilities.VerifyStreamReadArgs(this, buffer, offset, count);
		if (count == 0)
		{
			return 0;
		}
		if (_currentStreamLength <= _currentStreamPosition)
		{
			return 0;
		}
		int num = (int)Math.Min(count, _currentStreamLength - _currentStreamPosition);
		checked
		{
			int num2;
			if (_isolatedStorageMode)
			{
				lock (PackagingUtilities.IsolatedStorageFileLock)
				{
					_isolatedStorageStream.Seek(_currentStreamPosition, SeekOrigin.Begin);
					num2 = _isolatedStorageStream.Read(buffer, offset, num);
				}
			}
			else
			{
				Array.Clear(buffer, offset, num);
				int i = _memoryStreamList.BinarySearch(GetSearchBlockForOffset(_currentStreamPosition));
				if (i < 0)
				{
					i = ~i;
				}
				for (; i < _memoryStreamList.Count; i++)
				{
					MemoryStreamBlock memoryStreamBlock = _memoryStreamList[i];
					PackagingUtilities.CalculateOverlap(memoryStreamBlock.Offset, (int)memoryStreamBlock.Stream.Length, _currentStreamPosition, num, out var overlapBlockOffset, out var overlapBlockSize);
					if (overlapBlockSize <= 0)
					{
						break;
					}
					Array.Copy(memoryStreamBlock.Stream.GetBuffer(), (int)(overlapBlockOffset - memoryStreamBlock.Offset), buffer, (int)(offset + overlapBlockOffset - _currentStreamPosition), (int)overlapBlockSize);
				}
				num2 = num;
			}
			_currentStreamPosition += num2;
			return num2;
		}
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		CheckDisposed();
		PackagingUtilities.VerifyStreamWriteArgs(this, buffer, offset, count);
		if (count == 0)
		{
			return;
		}
		checked
		{
			if (_isolatedStorageMode)
			{
				lock (PackagingUtilities.IsolatedStorageFileLock)
				{
					_isolatedStorageStream.Seek(_currentStreamPosition, SeekOrigin.Begin);
					_isolatedStorageStream.Write(buffer, offset, count);
				}
				_currentStreamPosition += count;
			}
			else
			{
				WriteAndCollapseBlocks(buffer, offset, count);
			}
			_currentStreamLength = Math.Max(_currentStreamLength, _currentStreamPosition);
			SwitchModeIfNecessary();
		}
	}

	public override void Flush()
	{
		CheckDisposed();
	}

	internal void WriteToStream(Stream stream)
	{
		if (_isolatedStorageMode)
		{
			lock (PackagingUtilities.IsolatedStorageFileLock)
			{
				_isolatedStorageStream.Seek(0L, SeekOrigin.Begin);
				PackagingUtilities.CopyStream(_isolatedStorageStream, stream, long.MaxValue, 524288);
				return;
			}
		}
		CopyMemoryBlocksToStream(stream);
	}

	internal SparseMemoryStream(long lowWaterMark, long highWaterMark)
		: this(lowWaterMark, highWaterMark, autoCloseSmallBlockGaps: true)
	{
	}

	internal SparseMemoryStream(long lowWaterMark, long highWaterMark, bool autoCloseSmallBlockGaps)
	{
		Invariant.Assert(lowWaterMark >= 0 && highWaterMark >= 0);
		Invariant.Assert(lowWaterMark < highWaterMark);
		Invariant.Assert(lowWaterMark <= int.MaxValue);
		_memoryStreamList = new List<MemoryStreamBlock>(5);
		_lowWaterMark = lowWaterMark;
		_highWaterMark = highWaterMark;
		_autoCloseSmallBlockGaps = autoCloseSmallBlockGaps;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!disposing || _disposedFlag)
			{
				return;
			}
			foreach (MemoryStreamBlock memoryStream in _memoryStreamList)
			{
				memoryStream.Stream.Close();
			}
			if (_isolatedStorageStream != null)
			{
				_isolatedStorageStream.Close();
			}
		}
		finally
		{
			_disposedFlag = true;
			_isolatedStorageStream = null;
			_memoryStreamList = null;
			base.Dispose(disposing);
		}
	}

	private void CheckDisposed()
	{
		if (_disposedFlag)
		{
			throw new ObjectDisposedException(SR.StreamObjectDisposed);
		}
	}

	private MemoryStreamBlock GetSearchBlockForOffset(long offset)
	{
		if (_searchBlock == null)
		{
			_searchBlock = new MemoryStreamBlock(null, offset);
		}
		else
		{
			_searchBlock.Offset = offset;
		}
		return _searchBlock;
	}

	private bool CanCollapseWithPreviousBlock(MemoryStreamBlock memStreamBlock, long offset, long length)
	{
		if (!_autoCloseSmallBlockGaps || memStreamBlock == null)
		{
			return false;
		}
		checked
		{
			long num = offset - (memStreamBlock.Offset + memStreamBlock.Stream.Length);
			if (num <= 100 && num + length + memStreamBlock.Stream.Length <= int.MaxValue)
			{
				return true;
			}
			return false;
		}
	}

	private void WriteAndCollapseBlocks(byte[] buffer, int offset, int count)
	{
		int num = _memoryStreamList.BinarySearch(GetSearchBlockForOffset(_currentStreamPosition));
		bool flag = false;
		MemoryStreamBlock memoryStreamBlock = null;
		MemoryStreamBlock memoryStreamBlock2 = null;
		checked
		{
			if (num < 0)
			{
				num = ~num;
				if (num != 0)
				{
					memoryStreamBlock2 = _memoryStreamList[num - 1];
				}
				if (CanCollapseWithPreviousBlock(memoryStreamBlock2, _currentStreamPosition, count))
				{
					memoryStreamBlock2.Stream.Seek(0L, SeekOrigin.End);
					SkipWrite(memoryStreamBlock2.Stream, memoryStreamBlock2.EndOffset, _currentStreamPosition);
					memoryStreamBlock2.Stream.Write(buffer, offset, count);
					flag = true;
				}
			}
			else
			{
				memoryStreamBlock2 = _memoryStreamList[num];
				if (memoryStreamBlock2.Stream.Length + count <= int.MaxValue)
				{
					memoryStreamBlock2.Stream.Seek(_currentStreamPosition - memoryStreamBlock2.Offset, SeekOrigin.Begin);
					memoryStreamBlock2.Stream.Write(buffer, offset, count);
					flag = true;
					num++;
				}
				else
				{
					memoryStreamBlock2.Stream.SetLength(_currentStreamPosition - memoryStreamBlock2.Offset);
				}
			}
			if (!flag)
			{
				memoryStreamBlock2 = ConstructMemoryStreamFromWriteRequest(buffer, _currentStreamPosition, count, offset);
				_memoryStreamList.Insert(num, memoryStreamBlock2);
				num++;
			}
			_currentStreamPosition += count;
			int i;
			for (i = num; i < _memoryStreamList.Count && _memoryStreamList[i].EndOffset <= _currentStreamPosition; i++)
			{
				_memoryStreamList[i].Stream.Close();
			}
			if (i - num > 0)
			{
				_memoryStreamList.RemoveRange(num, i - num);
			}
			long num2 = -1L;
			if (num < _memoryStreamList.Count)
			{
				memoryStreamBlock = _memoryStreamList[num];
				num2 = _currentStreamPosition - memoryStreamBlock.Offset;
			}
			else
			{
				memoryStreamBlock = null;
			}
			if (num2 <= 0)
			{
				if (memoryStreamBlock != null && CanCollapseWithPreviousBlock(memoryStreamBlock2, memoryStreamBlock.Offset, memoryStreamBlock.Stream.Length))
				{
					_memoryStreamList.RemoveAt(num);
					memoryStreamBlock2.Stream.Seek(0L, SeekOrigin.End);
					SkipWrite(memoryStreamBlock2.Stream, _currentStreamPosition, memoryStreamBlock.Offset);
					memoryStreamBlock2.Stream.Write(memoryStreamBlock.Stream.GetBuffer(), 0, (int)memoryStreamBlock.Stream.Length);
				}
				return;
			}
			_memoryStreamList.RemoveAt(num);
			int num3 = (int)(memoryStreamBlock.Stream.Length - num2);
			if (memoryStreamBlock2.Stream.Length + num3 <= int.MaxValue)
			{
				memoryStreamBlock2.Stream.Seek(0L, SeekOrigin.End);
				memoryStreamBlock2.Stream.Write(memoryStreamBlock.Stream.GetBuffer(), (int)num2, num3);
			}
			else
			{
				memoryStreamBlock = ConstructMemoryStreamFromWriteRequest(memoryStreamBlock.Stream.GetBuffer(), _currentStreamPosition, num3, (int)num2);
				_memoryStreamList.Insert(num, memoryStreamBlock);
			}
		}
	}

	private MemoryStreamBlock ConstructMemoryStreamFromWriteRequest(byte[] buffer, long writeRequestOffset, int writeRequestSize, int bufferOffset)
	{
		MemoryStreamBlock memoryStreamBlock = new MemoryStreamBlock(_trackingMemoryStreamFactory.Create(writeRequestSize), writeRequestOffset);
		memoryStreamBlock.Stream.Seek(0L, SeekOrigin.Begin);
		memoryStreamBlock.Stream.Write(buffer, bufferOffset, writeRequestSize);
		return memoryStreamBlock;
	}

	private void SwitchModeIfNecessary()
	{
		if (_isolatedStorageMode)
		{
			if (_isolatedStorageStream.Length >= _lowWaterMark)
			{
				return;
			}
			if (_isolatedStorageStream.Length > 0)
			{
				MemoryStreamBlock memoryStreamBlock = new MemoryStreamBlock(_trackingMemoryStreamFactory.Create((int)_isolatedStorageStream.Length), 0L);
				lock (PackagingUtilities.IsolatedStorageFileLock)
				{
					_isolatedStorageStream.Seek(0L, SeekOrigin.Begin);
					memoryStreamBlock.Stream.Seek(0L, SeekOrigin.Begin);
					PackagingUtilities.CopyStream(_isolatedStorageStream, memoryStreamBlock.Stream, long.MaxValue, 524288);
				}
				_memoryStreamList.Add(memoryStreamBlock);
			}
			_isolatedStorageMode = false;
			lock (PackagingUtilities.IsolatedStorageFileLock)
			{
				_isolatedStorageStream.SetLength(0L);
				_isolatedStorageStream.Flush();
				return;
			}
		}
		if (_trackingMemoryStreamFactory.CurrentMemoryConsumption <= _highWaterMark)
		{
			return;
		}
		lock (PackagingUtilities.IsoStoreSyncRoot)
		{
			lock (PackagingUtilities.IsolatedStorageFileLock)
			{
				EnsureIsolatedStoreStream();
				CopyMemoryBlocksToStream(_isolatedStorageStream);
			}
		}
		_isolatedStorageMode = true;
		foreach (MemoryStreamBlock memoryStream in _memoryStreamList)
		{
			memoryStream.Stream.Close();
		}
		_memoryStreamList.Clear();
	}

	private void CopyMemoryBlocksToStream(Stream targetStream)
	{
		long num = 0L;
		checked
		{
			foreach (MemoryStreamBlock memoryStream in _memoryStreamList)
			{
				num = SkipWrite(targetStream, num, memoryStream.Offset);
				targetStream.Write(memoryStream.Stream.GetBuffer(), 0, (int)memoryStream.Stream.Length);
				num += memoryStream.Stream.Length;
			}
			if (num < _currentStreamLength)
			{
				num = SkipWrite(targetStream, num, _currentStreamLength);
			}
			targetStream.Flush();
		}
	}

	private long SkipWrite(Stream targetStream, long currentPos, long offset)
	{
		long num = offset - currentPos;
		if (num > 0)
		{
			byte[] array = new byte[Math.Min(524288L, num)];
			while (num > 0)
			{
				int num2 = (int)Math.Min(num, array.Length);
				targetStream.Write(array, 0, num2);
				num -= num2;
			}
		}
		return offset;
	}

	private void EnsureIsolatedStoreStream()
	{
		if (_isolatedStorageStream == null)
		{
			_isolatedStorageStream = PackagingUtilities.CreateUserScopedIsolatedStorageFileStreamWithRandomName(3, out _isolatedStorageStreamFileName);
		}
	}
}
