using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Iced.Intel.BlockEncoderInternal;

namespace Iced.Intel;

internal sealed class BlockEncoder
{
	private sealed class NullCodeWriter : CodeWriter
	{
		public static readonly NullCodeWriter? Instance = new NullCodeWriter();

		private NullCodeWriter()
		{
		}

		public override void WriteByte(byte value)
		{
		}
	}

	private readonly int bitness;

	private readonly BlockEncoderOptions options;

	private readonly Block?[]? blocks;

	private readonly Encoder? nullEncoder;

	private readonly Dictionary<ulong, Instr?>? toInstr;

	internal int Bitness => bitness;

	internal bool FixBranches => (options & BlockEncoderOptions.DontFixBranches) == 0;

	private bool ReturnRelocInfos => (options & BlockEncoderOptions.ReturnRelocInfos) != 0;

	private bool ReturnNewInstructionOffsets => (options & BlockEncoderOptions.ReturnNewInstructionOffsets) != 0;

	private bool ReturnConstantOffsets => (options & BlockEncoderOptions.ReturnConstantOffsets) != 0;

	private BlockEncoder(int bitness, InstructionBlock[]? instrBlocks, BlockEncoderOptions options)
	{
		if (bitness != 16 && bitness != 32 && bitness != 64)
		{
			throw new ArgumentOutOfRangeException("bitness");
		}
		if (instrBlocks == null)
		{
			throw new ArgumentNullException("instrBlocks");
		}
		this.bitness = bitness;
		nullEncoder = Encoder.Create(bitness, NullCodeWriter.Instance);
		this.options = options;
		blocks = new Block[instrBlocks.Length];
		int num = 0;
		for (int i = 0; i < instrBlocks.Length; i++)
		{
			IList<Instruction> instructions = instrBlocks[i].Instructions;
			if (instructions == null)
			{
				throw new ArgumentException();
			}
			Block block = new Block(this, instrBlocks[i].CodeWriter, instrBlocks[i].RIP, ReturnRelocInfos ? new List<RelocInfo>() : null);
			blocks[i] = block;
			Instr[] array = new Instr[instructions.Count];
			ulong num2 = instrBlocks[i].RIP;
			for (int j = 0; j < array.Length; j++)
			{
				Instr instr = Instr.Create(this, block, instructions[j]);
				instr.IP = num2;
				array[j] = instr;
				num++;
				num2 += instr.Size;
			}
			block.SetInstructions(array);
		}
		Array.Sort(blocks, (Block? a, Block? b) => a.RIP.CompareTo(b.RIP));
		Dictionary<ulong, Instr> dictionary = (toInstr = new Dictionary<ulong, Instr>(num));
		bool flag = false;
		Block[] array2 = blocks;
		for (int k = 0; k < array2.Length; k++)
		{
			Instr[] instructions2 = array2[k].Instructions;
			foreach (Instr instr2 in instructions2)
			{
				ulong origIP = instr2.OrigIP;
				if (dictionary.TryGetValue(origIP, out var _))
				{
					if (origIP != 0L)
					{
						throw new ArgumentException($"Multiple instructions with the same IP: 0x{origIP:X}");
					}
					flag = true;
				}
				else
				{
					dictionary[origIP] = instr2;
				}
			}
		}
		if (flag)
		{
			dictionary.Remove(0uL);
		}
		array2 = blocks;
		foreach (Block obj in array2)
		{
			ulong num3 = obj.RIP;
			Instr[] instructions2 = obj.Instructions;
			foreach (Instr instr3 in instructions2)
			{
				instr3.IP = num3;
				if (!instr3.Done)
				{
					instr3.Initialize(this);
				}
				num3 += instr3.Size;
			}
		}
	}

	public static bool TryEncode(int bitness, InstructionBlock block, [_003Cb2ffb5d6_002D6a81_002D4f20_002D8e75_002D7064682f7f7c_003ENotNullWhen(false)] out string? errorMessage, out BlockEncoderResult result, BlockEncoderOptions options = BlockEncoderOptions.None)
	{
		if (TryEncode(bitness, new InstructionBlock[1] { block }, out errorMessage, out BlockEncoderResult[] result2, options))
		{
			result = result2[0];
			return true;
		}
		result = default(BlockEncoderResult);
		return false;
	}

	public static bool TryEncode(int bitness, InstructionBlock[] blocks, [_003Cb2ffb5d6_002D6a81_002D4f20_002D8e75_002D7064682f7f7c_003ENotNullWhen(false)] out string? errorMessage, [_003Cb2ffb5d6_002D6a81_002D4f20_002D8e75_002D7064682f7f7c_003ENotNullWhen(true)] out BlockEncoderResult[]? result, BlockEncoderOptions options = BlockEncoderOptions.None)
	{
		return new BlockEncoder(bitness, blocks, options).Encode(out errorMessage, out result);
	}

	private bool Encode([_003Cb2ffb5d6_002D6a81_002D4f20_002D8e75_002D7064682f7f7c_003ENotNullWhen(false)] out string? errorMessage, [_003Cb2ffb5d6_002D6a81_002D4f20_002D8e75_002D7064682f7f7c_003ENotNullWhen(true)] out BlockEncoderResult[]? result)
	{
		Block[] array;
		for (int i = 0; i < 5; i++)
		{
			bool flag = false;
			array = blocks;
			foreach (Block obj in array)
			{
				ulong num = obj.RIP;
				ulong num2 = 0uL;
				Instr[] instructions = obj.Instructions;
				foreach (Instr instr in instructions)
				{
					instr.IP = num;
					if (!instr.Done)
					{
						uint size = instr.Size;
						if (instr.Optimize(num2))
						{
							if (instr.Size > size)
							{
								errorMessage = "Internal error: new size > old size";
								result = null;
								return false;
							}
							if (instr.Size < size)
							{
								num2 += size - instr.Size;
								flag = true;
							}
						}
						else if (instr.Size != size)
						{
							errorMessage = "Internal error: new size != old size";
							result = null;
							return false;
						}
					}
					num += instr.Size;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		array = blocks;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].InitializeData();
		}
		BlockEncoderResult[] array2 = new BlockEncoderResult[blocks.Length];
		for (int l = 0; l < blocks.Length; l++)
		{
			Block block = blocks[l];
			Encoder encoder = Encoder.Create(bitness, block.CodeWriter);
			ulong num3 = block.RIP;
			uint[] array3 = (ReturnNewInstructionOffsets ? new uint[block.Instructions.Length] : null);
			ConstantOffsets[] array4 = (ReturnConstantOffsets ? new ConstantOffsets[block.Instructions.Length] : null);
			Instr[] instructions2 = block.Instructions;
			for (int m = 0; m < instructions2.Length; m++)
			{
				Instr instr2 = instructions2[m];
				uint bytesWritten = block.CodeWriter.BytesWritten;
				bool isOriginalInstruction;
				if (array4 != null)
				{
					errorMessage = instr2.TryEncode(encoder, out array4[m], out isOriginalInstruction);
				}
				else
				{
					errorMessage = instr2.TryEncode(encoder, out var _, out isOriginalInstruction);
				}
				if (errorMessage != null)
				{
					result = null;
					return false;
				}
				uint num4 = block.CodeWriter.BytesWritten - bytesWritten;
				if (num4 != instr2.Size)
				{
					errorMessage = "Internal error: didn't write all bytes";
					result = null;
					return false;
				}
				if (array3 != null)
				{
					if (isOriginalInstruction)
					{
						array3[m] = (uint)(num3 - block.RIP);
					}
					else
					{
						array3[m] = uint.MaxValue;
					}
				}
				num3 += num4;
			}
			array2[l] = new BlockEncoderResult(block.RIP, block.relocInfos, array3, array4);
			block.WriteData();
		}
		errorMessage = null;
		result = array2;
		return true;
	}

	internal TargetInstr GetTarget(ulong address)
	{
		if (toInstr.TryGetValue(address, out Instr value))
		{
			return new TargetInstr(value);
		}
		return new TargetInstr(address);
	}

	internal uint GetInstructionSize(in Instruction instruction, ulong ip)
	{
		if (!nullEncoder.TryEncode(in instruction, ip, out uint encodedLength, out string _))
		{
			return 15u;
		}
		return encodedLength;
	}
}
