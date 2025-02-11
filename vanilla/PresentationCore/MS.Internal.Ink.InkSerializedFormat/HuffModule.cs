using System;
using System.Collections.Generic;

namespace MS.Internal.Ink.InkSerializedFormat;

internal class HuffModule
{
	private DeltaDelta _defaultDtxf;

	private List<HuffCodec> _huffCodecs = new List<HuffCodec>();

	private HuffCodec[] _defaultHuffCodecs = new HuffCodec[8];

	private DeltaDelta DefaultDeltaDelta
	{
		get
		{
			if (_defaultDtxf == null)
			{
				_defaultDtxf = new DeltaDelta();
			}
			return _defaultDtxf;
		}
	}

	internal HuffModule()
	{
	}

	internal HuffCodec GetDefCodec(uint index)
	{
		HuffCodec huffCodec = null;
		if (8 > index)
		{
			huffCodec = _defaultHuffCodecs[index];
			if (huffCodec == null)
			{
				huffCodec = new HuffCodec(index);
				_defaultHuffCodecs[index] = huffCodec;
			}
			return huffCodec;
		}
		throw new ArgumentOutOfRangeException("index");
	}

	internal HuffCodec FindCodec(byte algoData)
	{
		byte b = (byte)(algoData & 0x1F);
		if (b < 8)
		{
			return GetDefCodec(b);
		}
		if (b >= _huffCodecs.Count + 8)
		{
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("invalid codec computed"));
		}
		return _huffCodecs[b - 8];
	}

	internal DataXform FindDtXf(byte algoData)
	{
		return DefaultDeltaDelta;
	}
}
