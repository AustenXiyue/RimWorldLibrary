using System;
using System.Collections.Generic;
using System.IO;
using System.Security.RightsManagement;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal class RightsManagementEncryptedStream : Stream
{
	private Random _random;

	private Stream _baseStream;

	private long _streamCachedLength = -1L;

	private long _streamOnDiskLength = -1L;

	private long _streamPosition;

	private CryptoProvider _cryptoProvider;

	private const int _prefixLengthSize = 8;

	private byte[] _randomBuffer;

	private MemoryStreamBlock _comparisonBlock;

	private SparseMemoryStream _readCache = new SparseMemoryStream(2147483647L, long.MaxValue, autoCloseSmallBlockGaps: false);

	private SparseMemoryStream _writeCache = new SparseMemoryStream(2147483647L, long.MaxValue, autoCloseSmallBlockGaps: false);

	private const long _autoFlushHighWaterMark = 16384L;

	public override bool CanRead
	{
		get
		{
			if (_baseStream != null && _baseStream.CanRead && _baseStream.CanSeek)
			{
				return _cryptoProvider.CanDecrypt;
			}
			return false;
		}
	}

	public override bool CanSeek
	{
		get
		{
			if (_baseStream != null)
			{
				return _baseStream.CanSeek;
			}
			return false;
		}
	}

	public override bool CanWrite
	{
		get
		{
			if (_baseStream != null && _baseStream.CanWrite && _baseStream.CanRead && _baseStream.CanSeek && _cryptoProvider.CanDecrypt)
			{
				return _cryptoProvider.CanEncrypt;
			}
			return false;
		}
	}

	public override long Length
	{
		get
		{
			CheckDisposed();
			return _streamCachedLength;
		}
	}

	public override long Position
	{
		get
		{
			CheckDisposed();
			return _streamPosition;
		}
		set
		{
			Seek(value, SeekOrigin.Begin);
		}
	}

	public override void Flush()
	{
		CheckDisposed();
		FlushCache();
		_baseStream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		CheckDisposed();
		long num = 0L;
		num = checked(origin switch
		{
			SeekOrigin.Begin => offset, 
			SeekOrigin.Current => _streamPosition + offset, 
			SeekOrigin.End => Length + offset, 
			_ => throw new ArgumentOutOfRangeException("origin", SR.SeekOriginInvalid), 
		});
		if (num < 0)
		{
			throw new ArgumentOutOfRangeException("offset", SR.SeekNegative);
		}
		_streamPosition = num;
		return _streamPosition;
	}

	public override void SetLength(long newLength)
	{
		CheckDisposed();
		if (newLength < 0)
		{
			throw new ArgumentOutOfRangeException("newLength", SR.CannotMakeStreamLengthNegative);
		}
		_streamCachedLength = newLength;
		FlushLength();
		if (_streamPosition > Length)
		{
			_streamPosition = Length;
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		CheckDisposed();
		PackagingUtilities.VerifyStreamReadArgs(this, buffer, offset, count);
		int num = InternalRead(_streamPosition, buffer, offset, count);
		FlushCacheIfNecessary();
		checked
		{
			_streamPosition += num;
			return num;
		}
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		CheckDisposed();
		PackagingUtilities.VerifyStreamWriteArgs(this, buffer, offset, count);
		_writeCache.Seek(Position, SeekOrigin.Begin);
		_writeCache.Write(buffer, offset, count);
		if (_writeCache.Length > Length)
		{
			SetLength(_writeCache.Length);
		}
		checked
		{
			_streamPosition += count;
			FlushCacheIfNecessary();
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && _baseStream != null)
			{
				FlushCache();
				_baseStream.Close();
				_readCache.Close();
				_writeCache.Close();
			}
		}
		finally
		{
			_baseStream = null;
			_readCache = null;
			_writeCache = null;
			base.Dispose(disposing);
		}
	}

	internal RightsManagementEncryptedStream(Stream baseStream, CryptoProvider cryptoProvider)
	{
		if (!cryptoProvider.CanDecrypt)
		{
			throw new ArgumentException(SR.CryptoProviderCanNotDecrypt, "cryptoProvider");
		}
		if (!cryptoProvider.CanMergeBlocks)
		{
			throw new ArgumentException(SR.CryptoProviderCanNotMergeBlocks, "cryptoProvider");
		}
		_baseStream = baseStream;
		_cryptoProvider = cryptoProvider;
		ParseStreamLength();
	}

	private void ParseStreamLength()
	{
		if (_streamCachedLength >= 0)
		{
			return;
		}
		_baseStream.Seek(0L, SeekOrigin.Begin);
		byte[] array = new byte[8];
		int num = PackagingUtilities.ReliableRead(_baseStream, array, 0, array.Length);
		if (num == 0)
		{
			_streamOnDiskLength = 0L;
		}
		else
		{
			if (num < 8)
			{
				throw new FileFormatException(SR.EncryptedDataStreamCorrupt);
			}
			_streamOnDiskLength = checked((long)BitConverter.ToUInt64(array, 0));
		}
		_streamCachedLength = _streamOnDiskLength;
	}

	private int InternalRead(long streamPosition, byte[] buffer, int offset, int count)
	{
		int num = count;
		checked
		{
			if (streamPosition + count > Length)
			{
				num = (int)(Length - streamPosition);
			}
			if (num <= 0)
			{
				return 0;
			}
			int num2 = ReadFromCache(_writeCache, streamPosition, num, buffer, offset);
			if (num2 > 0)
			{
				return num2;
			}
			long num3 = FindOffsetOfNextAvailableBlockAfter(_writeCache, streamPosition);
			if (num3 >= 0 && streamPosition + num > num3)
			{
				num = (int)(num3 - streamPosition);
			}
			num2 = ReadFromCache(_readCache, streamPosition, num, buffer, offset);
			if (num2 > 0)
			{
				return num2;
			}
			long num4 = FindOffsetOfNextAvailableBlockAfter(_readCache, streamPosition);
			if (num4 >= 0 && streamPosition + num > num4)
			{
				num = (int)(num4 - streamPosition);
			}
			FetchBlockIntoReadCache(streamPosition, num);
			return ReadFromCache(_readCache, streamPosition, num, buffer, offset);
		}
	}

	private int ReadFromCache(SparseMemoryStream cache, long start, int count, byte[] buffer, int bufferOffset)
	{
		IList<MemoryStreamBlock> memoryBlockCollection = cache.MemoryBlockCollection;
		bool match;
		int index = FindIndexOfBlockAtOffset(cache, start, out match);
		int result = 0;
		checked
		{
			if (match)
			{
				MemoryStreamBlock memoryStreamBlock = memoryBlockCollection[index];
				PackagingUtilities.CalculateOverlap(memoryStreamBlock.Offset, memoryStreamBlock.Stream.Length, start, count, out var overlapBlockOffset, out var overlapBlockSize);
				if (overlapBlockSize > 0)
				{
					memoryStreamBlock.Stream.Seek(overlapBlockOffset - memoryStreamBlock.Offset, SeekOrigin.Begin);
					result = memoryStreamBlock.Stream.Read(buffer, bufferOffset, (int)overlapBlockSize);
				}
			}
			return result;
		}
	}

	private int FindIndexOfBlockAtOffset(SparseMemoryStream cache, long start, out bool match)
	{
		if (cache.MemoryBlockCollection.Count == 0)
		{
			match = false;
			return 0;
		}
		if (_comparisonBlock == null)
		{
			_comparisonBlock = new MemoryStreamBlock(null, start);
		}
		else
		{
			_comparisonBlock.Offset = start;
		}
		int num = cache.MemoryBlockCollection.BinarySearch(_comparisonBlock);
		if (num < 0)
		{
			num = ~num;
			match = false;
		}
		else
		{
			match = true;
		}
		return num;
	}

	private long FindOffsetOfNextAvailableBlockAfter(SparseMemoryStream cache, long start)
	{
		bool match;
		int num = FindIndexOfBlockAtOffset(cache, start, out match);
		if (num >= cache.MemoryBlockCollection.Count)
		{
			return -1L;
		}
		return cache.MemoryBlockCollection[num].Offset;
	}

	private void FetchBlockIntoReadCache(long start, int count)
	{
		int blockSize = _cryptoProvider.BlockSize;
		CalcBlockData(start, count, _cryptoProvider.CanMergeBlocks, ref blockSize, out var firstBlockOffset, out var blockCount);
		checked
		{
			_baseStream.Seek(8 + firstBlockOffset, SeekOrigin.Begin);
			int num = (int)(blockCount * blockSize);
			byte[] array = new byte[num];
			int num2 = PackagingUtilities.ReliableRead(_baseStream, array, 0, num, _cryptoProvider.BlockSize);
			if (num2 < _cryptoProvider.BlockSize)
			{
				throw new FileFormatException(SR.EncryptedDataStreamCorrupt);
			}
			int num3 = _cryptoProvider.BlockSize;
			int num4 = unchecked(num2 / num3);
			if (_cryptoProvider.CanMergeBlocks)
			{
				num3 *= num4;
				num4 = 1;
			}
			byte[] array2 = new byte[num3];
			_readCache.Seek(firstBlockOffset, SeekOrigin.Begin);
			for (long num5 = 0L; num5 < num4; num5++)
			{
				Array.Copy(array, num5 * num3, array2, 0L, num3);
				byte[] buffer = _cryptoProvider.Decrypt(array2);
				_readCache.Write(buffer, 0, num3);
			}
		}
	}

	private void FlushLength()
	{
		if (_streamCachedLength >= 0 && _streamCachedLength != _streamOnDiskLength)
		{
			_baseStream.Seek(0L, SeekOrigin.Begin);
			byte[] bytes = BitConverter.GetBytes((ulong)_streamCachedLength);
			_baseStream.Write(bytes, 0, bytes.Length);
			int blockSize = _cryptoProvider.BlockSize;
			long length = checked(8 + GetBlockSpanCount(blockSize, 0L, _streamCachedLength) * blockSize);
			_baseStream.SetLength(length);
			_streamOnDiskLength = _streamCachedLength;
		}
	}

	private static long GetBlockNo(long blockSize, long index)
	{
		return index / blockSize;
	}

	private static long GetBlockSpanCount(long blockSize, long index, long size)
	{
		if (size == 0L)
		{
			return 0L;
		}
		return checked(GetBlockNo(blockSize, index + size - 1) - GetBlockNo(blockSize, index) + 1);
	}

	private static void CalcBlockData(long start, long size, bool canMergeBlocks, ref int blockSize, out long firstBlockOffset, out long blockCount)
	{
		long blockNo = GetBlockNo(blockSize, start);
		checked
		{
			firstBlockOffset = blockNo * blockSize;
			blockCount = GetBlockSpanCount(blockSize, start, size);
			if (canMergeBlocks)
			{
				blockSize = (int)(blockSize * blockCount);
				blockCount = 1L;
			}
		}
	}

	private void CheckDisposed()
	{
		if (_baseStream == null)
		{
			throw new ObjectDisposedException(null, SR.StreamObjectDisposed);
		}
	}

	private void FlushCacheIfNecessary()
	{
		if (checked(_readCache.MemoryConsumption + _writeCache.MemoryConsumption) > 16384)
		{
			FlushCache();
		}
	}

	private void FlushCache()
	{
		FlushLength();
		long num = 0L;
		byte[] array = null;
		checked
		{
			foreach (MemoryStreamBlock item in _writeCache.MemoryBlockCollection)
			{
				long num2 = item.Offset;
				long num3 = item.Stream.Length;
				if (num2 < num)
				{
					num3 = num2 + num3 - num;
					num2 = num;
				}
				if (num3 > 0)
				{
					int blockSize = _cryptoProvider.BlockSize;
					CalcBlockData(num2, num3, _cryptoProvider.CanMergeBlocks, ref blockSize, out var firstBlockOffset, out var blockCount);
					int num4 = (int)(blockCount * blockSize);
					if (array == null || array.Length < num4)
					{
						array = new byte[Math.Max(4096, num4)];
					}
					int num5 = InternalReliableRead(firstBlockOffset, array, 0, num4);
					if (num5 < num4)
					{
						RandomFillUp(array, num5, num4 - num5);
					}
					byte[] buffer = _cryptoProvider.Encrypt(array);
					_baseStream.Seek(firstBlockOffset + 8, SeekOrigin.Begin);
					_baseStream.Write(buffer, 0, num4);
					num = firstBlockOffset + num4;
				}
			}
			_writeCache.SetLength(0L);
			_readCache.SetLength(0L);
		}
	}

	private int InternalReliableRead(long streamPosition, byte[] buffer, int offset, int count)
	{
		checked
		{
			int i;
			int num;
			for (i = 0; i < count; i += num)
			{
				num = InternalRead(streamPosition + i, buffer, offset + i, count - i);
				if (num == 0)
				{
					break;
				}
			}
			return i;
		}
	}

	private void RandomFillUp(byte[] buffer, int offset, int count)
	{
		if (count != 0)
		{
			if (_random == null)
			{
				_random = new Random();
			}
			if (_randomBuffer == null || _randomBuffer.Length < count)
			{
				_randomBuffer = new byte[Math.Max(16, count)];
			}
			_random.NextBytes(_randomBuffer);
			Array.Copy(_randomBuffer, 0, buffer, offset, count);
		}
	}
}
