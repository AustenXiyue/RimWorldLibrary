using System;
using System.Collections.Generic;
using Iced.Intel.Internal;

namespace Iced.Intel.DecoderInternal;

internal ref struct TableDeserializer
{
	private DataReader reader;

	private readonly OpCodeHandlerReader handlerReader;

	private readonly List<HandlerInfo> idToHandler;

	private readonly OpCodeHandler[] handlerArray;

	public TableDeserializer(OpCodeHandlerReader handlerReader, int maxIds, ReadOnlySpan<byte> data)
	{
		this.handlerReader = handlerReader;
		reader = new DataReader(data);
		idToHandler = new List<HandlerInfo>(maxIds);
		handlerArray = new OpCodeHandler[1];
	}

	public void Deserialize()
	{
		while (reader.CanRead)
		{
			switch ((SerializedDataKind)reader.ReadByte())
			{
			case SerializedDataKind.HandlerReference:
				idToHandler.Add(new HandlerInfo(ReadHandler()));
				break;
			case SerializedDataKind.ArrayReference:
				idToHandler.Add(new HandlerInfo(ReadHandlers((int)reader.ReadCompressedUInt32())));
				break;
			default:
				throw new InvalidOperationException();
			}
		}
		if (reader.CanRead)
		{
			throw new InvalidOperationException();
		}
	}

	public LegacyOpCodeHandlerKind ReadLegacyOpCodeHandlerKind()
	{
		return (LegacyOpCodeHandlerKind)reader.ReadByte();
	}

	public VexOpCodeHandlerKind ReadVexOpCodeHandlerKind()
	{
		return (VexOpCodeHandlerKind)reader.ReadByte();
	}

	public EvexOpCodeHandlerKind ReadEvexOpCodeHandlerKind()
	{
		return (EvexOpCodeHandlerKind)reader.ReadByte();
	}

	public Code ReadCode()
	{
		return (Code)reader.ReadCompressedUInt32();
	}

	public Register ReadRegister()
	{
		return (Register)reader.ReadByte();
	}

	public DecoderOptions ReadDecoderOptions()
	{
		return (DecoderOptions)reader.ReadCompressedUInt32();
	}

	public HandlerFlags ReadHandlerFlags()
	{
		return (HandlerFlags)reader.ReadCompressedUInt32();
	}

	public LegacyHandlerFlags ReadLegacyHandlerFlags()
	{
		return (LegacyHandlerFlags)reader.ReadCompressedUInt32();
	}

	public TupleType ReadTupleType()
	{
		return (TupleType)reader.ReadByte();
	}

	public bool ReadBoolean()
	{
		return reader.ReadByte() != 0;
	}

	public int ReadInt32()
	{
		return (int)reader.ReadCompressedUInt32();
	}

	public OpCodeHandler ReadHandler()
	{
		return ReadHandlerOrNull() ?? throw new InvalidOperationException();
	}

	public OpCodeHandler? ReadHandlerOrNull()
	{
		if (handlerReader.ReadHandlers(ref this, handlerArray, 0) != 1)
		{
			throw new InvalidOperationException();
		}
		return handlerArray[0];
	}

	public OpCodeHandler?[] ReadHandlers(int count)
	{
		OpCodeHandler[] array = new OpCodeHandler[count];
		int num;
		for (int i = 0; i < array.Length; i += num)
		{
			num = handlerReader.ReadHandlers(ref this, array, i);
			if (num <= 0 || (uint)(i + num) > (uint)array.Length)
			{
				throw new InvalidOperationException();
			}
		}
		return array;
	}

	public OpCodeHandler ReadHandlerReference()
	{
		uint index = reader.ReadByte();
		return idToHandler[(int)index].handler ?? throw new InvalidOperationException();
	}

	public OpCodeHandler[] ReadArrayReference(uint kind)
	{
		if (reader.ReadByte() != kind)
		{
			throw new InvalidOperationException();
		}
		return GetTable(reader.ReadByte());
	}

	public OpCodeHandler[] GetTable(uint index)
	{
		return idToHandler[(int)index].handlers ?? throw new InvalidOperationException();
	}
}
