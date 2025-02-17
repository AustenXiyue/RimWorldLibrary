using System;
using System.Buffers;
using Iced.Intel;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Architectures.AltEntryFactories;

internal sealed class IcedAltEntryFactory : IAltEntryFactory
{
	private sealed class PtrCodeReader : CodeReader
	{
		public IntPtr Base { get; }

		public int Position { get; private set; }

		public PtrCodeReader(IntPtr basePtr)
		{
			Base = basePtr;
			Position = 0;
		}

		public unsafe override int ReadByte()
		{
			return *(byte*)((nint)Base + Position++);
		}
	}

	private sealed class NullCodeWriter : CodeWriter
	{
		public override void WriteByte(byte value)
		{
		}
	}

	private sealed class BufferCodeWriter : CodeWriter, IDisposable
	{
		private readonly ArrayPool<byte> pool;

		private byte[]? buffer;

		private int pos;

		public ReadOnlyMemory<byte> Data => buffer.AsMemory().Slice(0, pos);

		public BufferCodeWriter()
		{
			pool = ArrayPool<byte>.Shared;
		}

		public override void WriteByte(byte value)
		{
			if (buffer == null)
			{
				buffer = pool.Rent(8);
			}
			if (buffer.Length <= pos)
			{
				byte[] destinationArray = pool.Rent(buffer.Length * 2);
				Array.Copy(buffer, destinationArray, buffer.Length);
				pool.Return(buffer);
				buffer = destinationArray;
			}
			buffer[pos++] = value;
		}

		public void Reset()
		{
			pos = 0;
		}

		public void Dispose()
		{
			if (buffer != null)
			{
				byte[] array = buffer;
				buffer = null;
				pool.Return(array);
			}
		}
	}

	private readonly ISystem system;

	private readonly IMemoryAllocator alloc;

	private readonly int bitness;

	public IcedAltEntryFactory(ISystem system, int bitness)
	{
		this.system = system;
		this.bitness = bitness;
		alloc = system.MemoryAllocator;
	}

	public IntPtr CreateAlternateEntrypoint(IntPtr entrypoint, int minLength, out IDisposable? handle)
	{
		PtrCodeReader ptrCodeReader = new PtrCodeReader(entrypoint);
		Decoder decoder = Decoder.Create(bitness, ptrCodeReader, (ulong)(long)entrypoint, DecoderOptions.NoInvalidCheck | DecoderOptions.AMD);
		InstructionList instructionList = new InstructionList();
		while (ptrCodeReader.Position < minLength)
		{
			decoder.Decode(out instructionList.AllocUninitializedElement());
		}
		bool flag = false;
		foreach (ref Instruction item in instructionList)
		{
			if (item.IsIPRelativeMemoryOperand)
			{
				flag = true;
				break;
			}
		}
		Instruction instruction = instructionList[instructionList.Count - 1];
		if (instruction.Mnemonic == Mnemonic.Call)
		{
			Encoder encoder = Encoder.Create(bitness, new NullCodeWriter());
			Instruction instruction2 = instruction;
			instruction2.Code = instruction.Code switch
			{
				Code.Call_rel16 => Code.Jmp_rel16, 
				Code.Call_rel32_32 => Code.Jmp_rel32_32, 
				Code.Call_rel32_64 => Code.Jmp_rel32_64, 
				Code.Jmp_rm16 => Code.Jmp_rm16, 
				Code.Jmp_rm32 => Code.Jmp_rm32, 
				Code.Jmp_rm64 => Code.Jmp_rm64, 
				Code.Call_m1616 => Code.Jmp_m1616, 
				Code.Call_m1632 => Code.Jmp_m1632, 
				Code.Call_m1664 => Code.Jmp_m1664, 
				Code.Call_ptr1616 => Code.Jmp_ptr1616, 
				Code.Call_ptr1632 => Code.Jmp_ptr1632, 
				_ => throw new InvalidOperationException($"Unrecognized call opcode {instruction.Code}"), 
			};
			instruction2.Length = (int)encoder.Encode(in instruction2, instruction2.IP);
			ulong nextIP = instruction.NextIP;
			Instruction instruction3;
			bool flag2;
			Instruction instruction4;
			if (bitness == 32)
			{
				instruction3 = Instruction.Create(Code.Pushd_imm32, (uint)nextIP);
				instruction3.Length = (int)encoder.Encode(in instruction3, instruction2.IP);
				instruction3.IP = instruction2.IP;
				instruction2.IP += (ulong)instruction3.Length;
				flag2 = false;
				instruction4 = default(Instruction);
			}
			else
			{
				flag2 = true;
				instruction4 = Instruction.CreateDeclareQword(nextIP);
				instruction3 = Instruction.Create(Code.Push_rm64, new MemoryOperand(Register.RIP, (long)instruction2.NextIP));
				instruction3.Length = (int)encoder.Encode(in instruction3, instruction2.IP);
				instruction3.IP = instruction2.IP;
				instruction2.IP += (ulong)instruction3.Length;
				instruction4.IP = instruction2.NextIP;
				instruction3.MemoryDisplacement64 = instruction4.IP;
			}
			instructionList.RemoveAt(instructionList.Count - 1);
			instructionList.Add(in instruction3);
			instructionList.Add(in instruction2);
			if (flag2)
			{
				instructionList.Add(in instruction4);
			}
		}
		else
		{
			instructionList.Add(Instruction.CreateBranch((bitness == 64) ? Code.Jmp_rel32_64 : Code.Jmp_rel32_32, decoder.IP));
		}
		int size = ptrCodeReader.Position + 5;
		using BufferCodeWriter bufferCodeWriter = new BufferCodeWriter();
		IAllocatedMemory allocated;
		ReadOnlyMemory<byte> data;
		while (true)
		{
			bufferCodeWriter.Reset();
			if (flag)
			{
				Helpers.Assert(alloc.TryAllocateInRange(new PositionedAllocationRequest(entrypoint, (nint)entrypoint + int.MinValue, (nint)entrypoint + int.MaxValue, new AllocationRequest(size)
				{
					Executable = true
				}), out allocated), null, "alloc.TryAllocateInRange(\n                        new(entrypoint, (nint)entrypoint + int.MinValue, (nint)entrypoint + int.MaxValue,\n                        new(estTotalSize) { Executable = true }), out allocated)");
			}
			else
			{
				Helpers.Assert(alloc.TryAllocate(new AllocationRequest(size)
				{
					Executable = true
				}, out allocated), null, "alloc.TryAllocate(new(estTotalSize) { Executable = true }, out allocated)");
			}
			IntPtr baseAddress = allocated.BaseAddress;
			if (!BlockEncoder.TryEncode(bitness, new InstructionBlock(bufferCodeWriter, instructionList, (ulong)(long)baseAddress), out string errorMessage, out BlockEncoderResult _))
			{
				allocated.Dispose();
				bool isEnabled;
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogErrorStringHandler(44, 1, out isEnabled);
				if (isEnabled)
				{
					message.AppendLiteral("BlockEncoder failed to encode instructions: ");
					message.AppendFormatted(errorMessage);
				}
				_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Error(ref message);
				throw new InvalidOperationException("BlockEncoder failed to encode instructions: " + errorMessage);
			}
			data = bufferCodeWriter.Data;
			if (data.Length == allocated.Size)
			{
				break;
			}
			data = bufferCodeWriter.Data;
			size = data.Length;
			allocated.Dispose();
		}
		ISystem obj = system;
		IntPtr baseAddress2 = allocated.BaseAddress;
		data = bufferCodeWriter.Data;
		obj.PatchData(PatchTargetKind.Executable, baseAddress2, data.Span, default(Span<byte>));
		handle = allocated;
		return allocated.BaseAddress;
	}
}
