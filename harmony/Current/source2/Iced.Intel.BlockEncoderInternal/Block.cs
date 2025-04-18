using System;
using System.Collections.Generic;

namespace Iced.Intel.BlockEncoderInternal;

internal sealed class Block
{
	public readonly CodeWriterImpl CodeWriter;

	public readonly ulong RIP;

	public readonly List<RelocInfo>? relocInfos;

	private Instr[] instructions;

	private readonly List<BlockData> dataList;

	private readonly ulong alignment;

	private readonly List<BlockData> validData;

	private ulong validDataAddress;

	private ulong validDataAddressAligned;

	public Instr[] Instructions => instructions;

	public bool CanAddRelocInfos => relocInfos != null;

	public Block(BlockEncoder blockEncoder, CodeWriter codeWriter, ulong rip, List<RelocInfo>? relocInfos)
	{
		CodeWriter = new CodeWriterImpl(codeWriter);
		RIP = rip;
		this.relocInfos = relocInfos;
		instructions = Array2.Empty<Instr>();
		dataList = new List<BlockData>();
		alignment = (uint)blockEncoder.Bitness / 8u;
		validData = new List<BlockData>();
	}

	internal void SetInstructions(Instr[] instructions)
	{
		this.instructions = instructions;
	}

	public BlockData AllocPointerLocation()
	{
		BlockData blockData = new BlockData
		{
			IsValid = true
		};
		dataList.Add(blockData);
		return blockData;
	}

	public void InitializeData()
	{
		ulong num;
		if (Instructions.Length != 0)
		{
			Instr instr = Instructions[Instructions.Length - 1];
			num = instr.IP + instr.Size;
		}
		else
		{
			num = RIP;
		}
		validDataAddress = num;
		ulong num2 = (validDataAddressAligned = (num + alignment - 1) & ~(alignment - 1));
		foreach (BlockData data in dataList)
		{
			if (data.IsValid)
			{
				data.__dont_use_address = num2;
				data.__dont_use_address_initd = true;
				validData.Add(data);
				num2 += alignment;
			}
		}
	}

	public void WriteData()
	{
		if (validData.Count == 0)
		{
			return;
		}
		CodeWriterImpl codeWriter = CodeWriter;
		int num = (int)(validDataAddressAligned - validDataAddress);
		for (int i = 0; i < num; i++)
		{
			codeWriter.WriteByte(204);
		}
		List<RelocInfo> list = relocInfos;
		if ((int)alignment == 8)
		{
			foreach (BlockData validDatum in validData)
			{
				list?.Add(new RelocInfo(RelocKind.Offset64, validDatum.Address));
				uint num2 = (uint)validDatum.Data;
				codeWriter.WriteByte((byte)num2);
				codeWriter.WriteByte((byte)(num2 >> 8));
				codeWriter.WriteByte((byte)(num2 >> 16));
				codeWriter.WriteByte((byte)(num2 >> 24));
				num2 = (uint)(validDatum.Data >> 32);
				codeWriter.WriteByte((byte)num2);
				codeWriter.WriteByte((byte)(num2 >> 8));
				codeWriter.WriteByte((byte)(num2 >> 16));
				codeWriter.WriteByte((byte)(num2 >> 24));
			}
			return;
		}
		throw new InvalidOperationException();
	}

	public void AddRelocInfo(RelocInfo relocInfo)
	{
		relocInfos?.Add(relocInfo);
	}
}
