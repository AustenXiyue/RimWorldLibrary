using System;
using System.IO;

namespace ICSharpCode.SharpZipLib.Tar;

public class TarOutputStream : Stream
{
	private long currBytes;

	private int assemblyBufferLength;

	private bool isClosed;

	protected long currSize;

	protected byte[] blockBuffer;

	protected byte[] assemblyBuffer;

	protected TarBuffer buffer;

	protected Stream outputStream;

	public bool IsStreamOwner
	{
		get
		{
			return buffer.IsStreamOwner;
		}
		set
		{
			buffer.IsStreamOwner = value;
		}
	}

	public override bool CanRead => outputStream.CanRead;

	public override bool CanSeek => outputStream.CanSeek;

	public override bool CanWrite => outputStream.CanWrite;

	public override long Length => outputStream.Length;

	public override long Position
	{
		get
		{
			return outputStream.Position;
		}
		set
		{
			outputStream.Position = value;
		}
	}

	public int RecordSize => buffer.RecordSize;

	private bool IsEntryOpen => currBytes < currSize;

	public TarOutputStream(Stream outputStream)
		: this(outputStream, 20)
	{
	}

	public TarOutputStream(Stream outputStream, int blockFactor)
	{
		if (outputStream == null)
		{
			throw new ArgumentNullException("outputStream");
		}
		this.outputStream = outputStream;
		buffer = TarBuffer.CreateOutputTarBuffer(outputStream, blockFactor);
		assemblyBuffer = new byte[512];
		blockBuffer = new byte[512];
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return outputStream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		outputStream.SetLength(value);
	}

	public override int ReadByte()
	{
		return outputStream.ReadByte();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return outputStream.Read(buffer, offset, count);
	}

	public override void Flush()
	{
		outputStream.Flush();
	}

	public void Finish()
	{
		if (IsEntryOpen)
		{
			CloseEntry();
		}
		WriteEofBlock();
	}

	protected override void Dispose(bool disposing)
	{
		if (!isClosed)
		{
			isClosed = true;
			Finish();
			buffer.Close();
		}
	}

	[Obsolete("Use RecordSize property instead")]
	public int GetRecordSize()
	{
		return buffer.RecordSize;
	}

	public void PutNextEntry(TarEntry entry)
	{
		if (entry == null)
		{
			throw new ArgumentNullException("entry");
		}
		if (entry.TarHeader.Name.Length > 100)
		{
			TarHeader tarHeader = new TarHeader();
			tarHeader.TypeFlag = 76;
			tarHeader.Name += "././@LongLink";
			tarHeader.Mode = 420;
			tarHeader.UserId = entry.UserId;
			tarHeader.GroupId = entry.GroupId;
			tarHeader.GroupName = entry.GroupName;
			tarHeader.UserName = entry.UserName;
			tarHeader.LinkName = "";
			tarHeader.Size = entry.TarHeader.Name.Length + 1;
			tarHeader.WriteHeader(blockBuffer);
			buffer.WriteBlock(blockBuffer);
			int num = 0;
			while (num < entry.TarHeader.Name.Length + 1)
			{
				Array.Clear(blockBuffer, 0, blockBuffer.Length);
				TarHeader.GetAsciiBytes(entry.TarHeader.Name, num, blockBuffer, 0, 512);
				num += 512;
				buffer.WriteBlock(blockBuffer);
			}
		}
		entry.WriteEntryHeader(blockBuffer);
		buffer.WriteBlock(blockBuffer);
		currBytes = 0L;
		currSize = (entry.IsDirectory ? 0 : entry.Size);
	}

	public void CloseEntry()
	{
		if (assemblyBufferLength > 0)
		{
			Array.Clear(assemblyBuffer, assemblyBufferLength, assemblyBuffer.Length - assemblyBufferLength);
			buffer.WriteBlock(assemblyBuffer);
			currBytes += assemblyBufferLength;
			assemblyBufferLength = 0;
		}
		if (currBytes < currSize)
		{
			string message = $"Entry closed at '{currBytes}' before the '{currSize}' bytes specified in the header were written";
			throw new TarException(message);
		}
	}

	public override void WriteByte(byte value)
	{
		Write(new byte[1] { value }, 0, 1);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Cannot be negative");
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException("offset and count combination is invalid");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Cannot be negative");
		}
		if (currBytes + count > currSize)
		{
			string message = $"request to write '{count}' bytes exceeds size in header of '{currSize}' bytes";
			throw new ArgumentOutOfRangeException("count", message);
		}
		if (assemblyBufferLength > 0)
		{
			if (assemblyBufferLength + count >= blockBuffer.Length)
			{
				int num = blockBuffer.Length - assemblyBufferLength;
				Array.Copy(assemblyBuffer, 0, blockBuffer, 0, assemblyBufferLength);
				Array.Copy(buffer, offset, blockBuffer, assemblyBufferLength, num);
				this.buffer.WriteBlock(blockBuffer);
				currBytes += blockBuffer.Length;
				offset += num;
				count -= num;
				assemblyBufferLength = 0;
			}
			else
			{
				Array.Copy(buffer, offset, assemblyBuffer, assemblyBufferLength, count);
				offset += count;
				assemblyBufferLength += count;
				count -= count;
			}
		}
		while (count > 0)
		{
			if (count < blockBuffer.Length)
			{
				Array.Copy(buffer, offset, assemblyBuffer, assemblyBufferLength, count);
				assemblyBufferLength += count;
				break;
			}
			this.buffer.WriteBlock(buffer, offset);
			int num2 = blockBuffer.Length;
			currBytes += num2;
			count -= num2;
			offset += num2;
		}
	}

	private void WriteEofBlock()
	{
		Array.Clear(blockBuffer, 0, blockBuffer.Length);
		buffer.WriteBlock(blockBuffer);
		buffer.WriteBlock(blockBuffer);
	}
}
