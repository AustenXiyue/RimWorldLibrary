using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NVorbis.Ogg;

public class ContainerReader : IContainerReader, IDisposable
{
	private class PageHeader
	{
		public int StreamSerial { get; set; }

		public PageFlags Flags { get; set; }

		public long GranulePosition { get; set; }

		public int SequenceNumber { get; set; }

		public long DataOffset { get; set; }

		public int[] PacketSizes { get; set; }

		public bool LastPacketContinues { get; set; }

		public bool IsResync { get; set; }
	}

	private Crc _crc = new Crc();

	private BufferedReadStream _stream;

	private Dictionary<int, PacketReader> _packetReaders;

	private List<int> _disposedStreamSerials;

	private long _nextPageOffset;

	private int _pageCount;

	private byte[] _readBuffer = new byte[65025];

	private long _containerBits;

	private long _wasteBits;

	public int[] StreamSerials => _packetReaders.Keys.ToArray();

	public int PagesRead => _pageCount;

	public bool CanSeek => _stream.CanSeek;

	public long WasteBits => _wasteBits;

	public event EventHandler<NewStreamEventArgs> NewStream;

	public ContainerReader(string path)
		: this(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), closeOnDispose: true)
	{
	}

	public ContainerReader(Stream stream, bool closeOnDispose)
	{
		_packetReaders = new Dictionary<int, PacketReader>();
		_disposedStreamSerials = new List<int>();
		_stream = (stream as BufferedReadStream) ?? new BufferedReadStream(stream)
		{
			CloseBaseStream = closeOnDispose
		};
	}

	public bool Init()
	{
		_stream.TakeLock();
		try
		{
			return GatherNextPage() != -1;
		}
		finally
		{
			_stream.ReleaseLock();
		}
	}

	public void Dispose()
	{
		int[] streamSerials = StreamSerials;
		foreach (int key in streamSerials)
		{
			_packetReaders[key].Dispose();
		}
		_nextPageOffset = 0L;
		_containerBits = 0L;
		_wasteBits = 0L;
		_stream.Dispose();
	}

	public IPacketProvider GetStream(int streamSerial)
	{
		if (!_packetReaders.TryGetValue(streamSerial, out var value))
		{
			throw new ArgumentOutOfRangeException("streamSerial");
		}
		return value;
	}

	public bool FindNextStream()
	{
		if (!CanSeek)
		{
			throw new InvalidOperationException();
		}
		int count = _packetReaders.Count;
		while (_packetReaders.Count == count)
		{
			_stream.TakeLock();
			try
			{
				if (GatherNextPage() == -1)
				{
					break;
				}
			}
			finally
			{
				_stream.ReleaseLock();
			}
		}
		return count > _packetReaders.Count;
	}

	public int GetTotalPageCount()
	{
		if (!CanSeek)
		{
			throw new InvalidOperationException();
		}
		while (true)
		{
			_stream.TakeLock();
			try
			{
				if (GatherNextPage() == -1)
				{
					break;
				}
			}
			finally
			{
				_stream.ReleaseLock();
			}
		}
		return _pageCount;
	}

	private PageHeader ReadPageHeader(long position)
	{
		_stream.Seek(position, SeekOrigin.Begin);
		if (_stream.Read(_readBuffer, 0, 27) != 27)
		{
			return null;
		}
		if (_readBuffer[0] != 79 || _readBuffer[1] != 103 || _readBuffer[2] != 103 || _readBuffer[3] != 83)
		{
			return null;
		}
		if (_readBuffer[4] != 0)
		{
			return null;
		}
		PageHeader pageHeader = new PageHeader();
		pageHeader.Flags = (PageFlags)_readBuffer[5];
		pageHeader.GranulePosition = BitConverter.ToInt64(_readBuffer, 6);
		pageHeader.StreamSerial = BitConverter.ToInt32(_readBuffer, 14);
		pageHeader.SequenceNumber = BitConverter.ToInt32(_readBuffer, 18);
		uint checkCrc = BitConverter.ToUInt32(_readBuffer, 22);
		_crc.Reset();
		for (int i = 0; i < 22; i++)
		{
			_crc.Update(_readBuffer[i]);
		}
		_crc.Update(0);
		_crc.Update(0);
		_crc.Update(0);
		_crc.Update(0);
		_crc.Update(_readBuffer[26]);
		int num = _readBuffer[26];
		if (_stream.Read(_readBuffer, 0, num) != num)
		{
			return null;
		}
		List<int> list = new List<int>(num);
		int num2 = 0;
		int num3 = 0;
		for (int j = 0; j < num; j++)
		{
			byte b = _readBuffer[j];
			_crc.Update(b);
			if (num3 == list.Count)
			{
				list.Add(0);
			}
			list[num3] += b;
			if (b < byte.MaxValue)
			{
				num3++;
				pageHeader.LastPacketContinues = false;
			}
			else
			{
				pageHeader.LastPacketContinues = true;
			}
			num2 += b;
		}
		pageHeader.PacketSizes = list.ToArray();
		pageHeader.DataOffset = position + 27 + num;
		if (_stream.Read(_readBuffer, 0, num2) != num2)
		{
			return null;
		}
		for (int k = 0; k < num2; k++)
		{
			_crc.Update(_readBuffer[k]);
		}
		if (_crc.Test(checkCrc))
		{
			_containerBits += 8 * (27 + num);
			_pageCount++;
			return pageHeader;
		}
		return null;
	}

	private PageHeader FindNextPageHeader()
	{
		long num = _nextPageOffset;
		bool isResync = false;
		PageHeader pageHeader;
		while ((pageHeader = ReadPageHeader(num)) == null)
		{
			isResync = true;
			_wasteBits += 8L;
			num = (_stream.Position = num + 1);
			int num3 = 0;
			do
			{
				switch (_stream.ReadByte())
				{
				case 79:
					if (_stream.ReadByte() == 103 && _stream.ReadByte() == 103 && _stream.ReadByte() == 83)
					{
						num += num3;
						goto end_IL_0032;
					}
					_stream.Seek(-3L, SeekOrigin.Current);
					break;
				case -1:
					return null;
				}
				_wasteBits += 8L;
				continue;
				end_IL_0032:
				break;
			}
			while (++num3 < 65536);
			if (num3 == 65536)
			{
				return null;
			}
		}
		pageHeader.IsResync = isResync;
		_nextPageOffset = pageHeader.DataOffset;
		for (int i = 0; i < pageHeader.PacketSizes.Length; i++)
		{
			_nextPageOffset += pageHeader.PacketSizes[i];
		}
		return pageHeader;
	}

	private bool AddPage(PageHeader hdr)
	{
		if (!_packetReaders.TryGetValue(hdr.StreamSerial, out var value))
		{
			value = new PacketReader(this, hdr.StreamSerial);
		}
		value.ContainerBits += _containerBits;
		_containerBits = 0L;
		bool isContinued = hdr.PacketSizes.Length == 1 && hdr.LastPacketContinues;
		bool isContinuation = (hdr.Flags & PageFlags.ContinuesPacket) == PageFlags.ContinuesPacket;
		bool isEndOfStream = false;
		bool isResync = hdr.IsResync;
		long num = hdr.DataOffset;
		int num2 = hdr.PacketSizes.Length;
		int[] packetSizes = hdr.PacketSizes;
		foreach (int num3 in packetSizes)
		{
			Packet packet = new Packet(this, num, num3);
			packet.PageGranulePosition = hdr.GranulePosition;
			packet.IsEndOfStream = isEndOfStream;
			packet.PageSequenceNumber = hdr.SequenceNumber;
			packet.IsContinued = isContinued;
			packet.IsContinuation = isContinuation;
			packet.IsResync = isResync;
			Packet packet2 = packet;
			value.AddPacket(packet2);
			num += num3;
			isContinuation = false;
			isResync = false;
			if (--num2 == 1)
			{
				isContinued = hdr.LastPacketContinues;
				isEndOfStream = (hdr.Flags & PageFlags.EndOfStream) == PageFlags.EndOfStream;
			}
		}
		if (!_packetReaders.ContainsKey(hdr.StreamSerial))
		{
			_packetReaders.Add(hdr.StreamSerial, value);
			return true;
		}
		return false;
	}

	private int GatherNextPage()
	{
		PageHeader pageHeader;
		while (true)
		{
			pageHeader = FindNextPageHeader();
			if (pageHeader == null)
			{
				return -1;
			}
			if (!_disposedStreamSerials.Contains(pageHeader.StreamSerial))
			{
				if (!AddPage(pageHeader))
				{
					break;
				}
				EventHandler<NewStreamEventArgs> newStream = this.NewStream;
				if (newStream == null)
				{
					break;
				}
				NewStreamEventArgs newStreamEventArgs = new NewStreamEventArgs(_packetReaders[pageHeader.StreamSerial]);
				newStream(this, newStreamEventArgs);
				if (!newStreamEventArgs.IgnoreStream)
				{
					break;
				}
				_packetReaders[pageHeader.StreamSerial].Dispose();
			}
		}
		return pageHeader.StreamSerial;
	}

	internal void DisposePacketReader(PacketReader packetReader)
	{
		_disposedStreamSerials.Add(packetReader.StreamSerial);
		_packetReaders.Remove(packetReader.StreamSerial);
	}

	internal int PacketReadByte(long offset)
	{
		_stream.TakeLock();
		try
		{
			_stream.Position = offset;
			return _stream.ReadByte();
		}
		finally
		{
			_stream.ReleaseLock();
		}
	}

	internal void PacketDiscardThrough(long offset)
	{
		_stream.TakeLock();
		try
		{
			_stream.DiscardThrough(offset);
		}
		finally
		{
			_stream.ReleaseLock();
		}
	}

	internal void GatherNextPage(int streamSerial)
	{
		if (!_packetReaders.ContainsKey(streamSerial))
		{
			throw new ArgumentOutOfRangeException("streamSerial");
		}
		int num;
		do
		{
			_stream.TakeLock();
			try
			{
				if (_packetReaders[streamSerial].HasEndOfStream)
				{
					break;
				}
				num = GatherNextPage();
				if (num != -1)
				{
					continue;
				}
				foreach (KeyValuePair<int, PacketReader> packetReader in _packetReaders)
				{
					if (!packetReader.Value.HasEndOfStream)
					{
						packetReader.Value.SetEndOfStream();
					}
				}
				break;
			}
			finally
			{
				_stream.ReleaseLock();
			}
		}
		while (num != streamSerial);
	}
}
