using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal class CompoundFileDeflateTransform : IDeflateTransform
{
	private byte[] _headerBuf = new byte[12];

	private const int _defaultBlockSize = 4096;

	private const int _maxAllowableBlockSize = 1048575;

	private const int _ulongSize = 4;

	private const uint _blockHeaderToken = 4000u;

	private const int _blockHeaderSize = 12;

	private const int DEFAULT_WINDOW_BITS = 15;

	private const int DEFAULT_MEM_LEVEL = 8;

	public void Decompress(Stream source, Stream sink)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (sink == null)
		{
			throw new ArgumentNullException("sink");
		}
		Invariant.Assert(source.CanRead);
		Invariant.Assert(sink.CanWrite, "Logic Error - Cannot decompress into a read-only stream");
		long position = -1L;
		try
		{
			if (source.CanSeek)
			{
				position = source.Position;
				source.Position = 0L;
			}
			if (sink.CanSeek)
			{
				sink.Position = 0L;
			}
			ThrowIfZLibError(System.IO.Compression.ZLibNative.CreateZLibStreamForInflate(out var zLibStreamHandle, 15));
			byte[] buffer = null;
			byte[] buffer2 = null;
			GCHandle gcHandle = default(GCHandle);
			GCHandle gcHandle2 = default(GCHandle);
			try
			{
				long num = 0L;
				int uncompressedSize;
				int compressedSize;
				while (ReadBlockHeader(source, out uncompressedSize, out compressedSize))
				{
					AllocOrRealloc(compressedSize, ref buffer, ref gcHandle);
					AllocOrRealloc(uncompressedSize, ref buffer2, ref gcHandle2);
					int num2 = PackagingUtilities.ReliableRead(source, buffer, 0, compressedSize);
					if (num2 > 0)
					{
						if (compressedSize != num2)
						{
							throw new FileFormatException(SR.CorruptStream);
						}
						zLibStreamHandle.NextIn = gcHandle.AddrOfPinnedObject();
						zLibStreamHandle.NextOut = gcHandle2.AddrOfPinnedObject();
						zLibStreamHandle.AvailIn = (uint)num2;
						zLibStreamHandle.AvailOut = (uint)buffer2.Length;
						ThrowIfZLibError(zLibStreamHandle.Inflate(System.IO.Compression.ZLibNative.FlushCode.SyncFlush));
						checked
						{
							int num3 = buffer2.Length - (int)zLibStreamHandle.AvailOut;
							if (num3 != uncompressedSize)
							{
								throw new FileFormatException(SR.CorruptStream);
							}
							num += num3;
							sink.Write(buffer2, 0, num3);
						}
					}
					else if (compressedSize != 0)
					{
						throw new FileFormatException(SR.CorruptStream);
					}
				}
				if (sink.CanSeek)
				{
					sink.SetLength(num);
				}
			}
			finally
			{
				if (gcHandle.IsAllocated)
				{
					gcHandle.Free();
				}
				if (gcHandle2.IsAllocated)
				{
					gcHandle2.Free();
				}
			}
		}
		finally
		{
			if (source.CanSeek)
			{
				source.Position = position;
			}
		}
	}

	public void Compress(Stream source, Stream sink)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (sink == null)
		{
			throw new ArgumentNullException("sink");
		}
		Invariant.Assert(source.CanRead);
		Invariant.Assert(sink.CanWrite, "Logic Error - Cannot compress into a read-only stream");
		long position = -1L;
		try
		{
			int num;
			if (source.CanSeek)
			{
				position = source.Position;
				source.Position = 0L;
				num = (int)Math.Min(source.Length, 4096L);
			}
			else
			{
				num = 4096;
			}
			if (sink.CanSeek)
			{
				sink.Position = 0L;
			}
			ThrowIfZLibError(System.IO.Compression.ZLibNative.CreateZLibStreamForDeflate(out var zLibStreamHandle, System.IO.Compression.ZLibNative.CompressionLevel.DefaultCompression, 15, 8, System.IO.Compression.ZLibNative.CompressionStrategy.DefaultStrategy));
			long num2 = 0L;
			byte[] buffer = null;
			byte[] buffer2 = null;
			GCHandle gcHandle = default(GCHandle);
			GCHandle gcHandle2 = default(GCHandle);
			try
			{
				AllocOrRealloc(num, ref buffer, ref gcHandle);
				AllocOrRealloc(6144, ref buffer2, ref gcHandle2);
				BinaryWriter binaryWriter = new BinaryWriter(sink);
				int num3;
				while ((num3 = PackagingUtilities.ReliableRead(source, buffer, 0, buffer.Length)) > 0)
				{
					Invariant.Assert(num3 <= num);
					zLibStreamHandle.NextIn = gcHandle.AddrOfPinnedObject();
					zLibStreamHandle.NextOut = gcHandle2.AddrOfPinnedObject();
					zLibStreamHandle.AvailIn = (uint)num3;
					zLibStreamHandle.AvailOut = (uint)buffer2.Length;
					ThrowIfZLibError(zLibStreamHandle.Deflate(System.IO.Compression.ZLibNative.FlushCode.SyncFlush));
					checked
					{
						int num4 = buffer2.Length - (int)zLibStreamHandle.AvailOut;
						Invariant.Assert(num4 > 0, "compressing non-zero bytes creates a non-empty block");
						Invariant.Assert(zLibStreamHandle.AvailIn == 0, "Expecting all data to be compressed!");
						binaryWriter.Write(4000u);
						binaryWriter.Write((uint)num3);
						binaryWriter.Write((uint)num4);
						num2 += _headerBuf.Length;
						sink.Write(buffer2, 0, num4);
						num2 += num4;
					}
				}
				if (sink.CanSeek)
				{
					sink.SetLength(num2);
				}
			}
			finally
			{
				if (gcHandle.IsAllocated)
				{
					gcHandle.Free();
				}
				if (gcHandle2.IsAllocated)
				{
					gcHandle2.Free();
				}
			}
		}
		finally
		{
			if (sink.CanSeek)
			{
				source.Position = position;
			}
		}
	}

	private static void AllocOrRealloc(int size, ref byte[] buffer, ref GCHandle gcHandle)
	{
		Invariant.Assert(size >= 0, "Cannot allocate negative number of bytes");
		if (buffer != null)
		{
			if (buffer.Length >= size)
			{
				return;
			}
			size = Math.Max(size, buffer.Length + (buffer.Length >> 1));
			if (gcHandle.IsAllocated)
			{
				gcHandle.Free();
			}
		}
		buffer = new byte[size];
		gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
	}

	private bool ReadBlockHeader(Stream source, out int uncompressedSize, out int compressedSize)
	{
		int num = PackagingUtilities.ReliableRead(source, _headerBuf, 0, _headerBuf.Length);
		checked
		{
			if (num > 0)
			{
				if (num < _headerBuf.Length)
				{
					throw new FileFormatException(SR.CorruptStream);
				}
				if (BitConverter.ToUInt32(_headerBuf, 0) != 4000)
				{
					throw new FileFormatException(SR.CorruptStream);
				}
				uncompressedSize = (int)BitConverter.ToUInt32(_headerBuf, 4);
				compressedSize = (int)BitConverter.ToUInt32(_headerBuf, 8);
				if (uncompressedSize < 0 || uncompressedSize > 1048575 || compressedSize < 0 || compressedSize > 1048575)
				{
					throw new FileFormatException(SR.CorruptStream);
				}
			}
			else
			{
				uncompressedSize = (compressedSize = 0);
			}
			return num > 0;
		}
	}

	private static void ThrowIfZLibError(System.IO.Compression.ZLibNative.ErrorCode retVal)
	{
		bool flag = false;
		bool flag2 = false;
		switch (retVal)
		{
		case System.IO.Compression.ZLibNative.ErrorCode.Ok:
			return;
		case System.IO.Compression.ZLibNative.ErrorCode.StreamEnd:
			flag = true;
			break;
		case System.IO.Compression.ZLibNative.ErrorCode.NeedDictionary:
			flag2 = true;
			break;
		case System.IO.Compression.ZLibNative.ErrorCode.StreamError:
			flag2 = true;
			break;
		case System.IO.Compression.ZLibNative.ErrorCode.DataError:
			flag2 = true;
			break;
		case System.IO.Compression.ZLibNative.ErrorCode.MemError:
			throw new OutOfMemoryException();
		case System.IO.Compression.ZLibNative.ErrorCode.BufError:
			flag = true;
			break;
		case System.IO.Compression.ZLibNative.ErrorCode.VersionError:
			throw new InvalidOperationException(SR.Format(SR.ZLibVersionError, Encoding.UTF8.GetString(global::Interop.Zlib.ZLibVersion, 0, global::Interop.Zlib.ZLibVersion.Length)));
		default:
			throw new IOException();
		}
		if (flag)
		{
			throw new InvalidOperationException();
		}
		if (flag2)
		{
			throw new FileFormatException(SR.CorruptStream);
		}
	}
}
