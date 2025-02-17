using System;

namespace Iced.Intel.EncoderInternal;

internal sealed class EvexHandler : OpCodeHandler
{
	private sealed class TryConvertToDisp8NImpl
	{
		public static bool TryConvertToDisp8N(Encoder encoder, OpCodeHandler handler, in Instruction instruction, int displ, out sbyte compressedValue)
		{
			int disp8N = (int)TupleTypeTable.GetDisp8N(((EvexHandler)handler).tupleType, (encoder.EncoderFlags & EncoderFlags.Broadcast) != 0);
			int num = displ / disp8N;
			if (num * disp8N == displ && -128 <= num && num <= 127)
			{
				compressedValue = (sbyte)num;
				return true;
			}
			compressedValue = 0;
			return false;
		}
	}

	private readonly WBit wbit;

	private readonly TupleType tupleType;

	private readonly uint table;

	private readonly uint p1Bits;

	private readonly uint llBits;

	private readonly uint mask_W;

	private readonly uint mask_LL;

	private static readonly TryConvertToDisp8N tryConvertToDisp8N = TryConvertToDisp8NImpl.TryConvertToDisp8N;

	private static Op[] CreateOps(EncFlags1 encFlags1)
	{
		int num = (int)(encFlags1 & EncFlags1.XOP_OpMask);
		int num2 = (int)(((uint)encFlags1 >> 5) & 0x1F);
		int num3 = (int)(((uint)encFlags1 >> 10) & 0x1F);
		int num4 = (int)(((uint)encFlags1 >> 15) & 0x1F);
		if (num4 != 0)
		{
			return new Op[4]
			{
				OpHandlerData.EvexOps[num - 1],
				OpHandlerData.EvexOps[num2 - 1],
				OpHandlerData.EvexOps[num3 - 1],
				OpHandlerData.EvexOps[num4 - 1]
			};
		}
		if (num3 != 0)
		{
			return new Op[3]
			{
				OpHandlerData.EvexOps[num - 1],
				OpHandlerData.EvexOps[num2 - 1],
				OpHandlerData.EvexOps[num3 - 1]
			};
		}
		if (num2 != 0)
		{
			return new Op[2]
			{
				OpHandlerData.EvexOps[num - 1],
				OpHandlerData.EvexOps[num2 - 1]
			};
		}
		if (num != 0)
		{
			return new Op[1] { OpHandlerData.EvexOps[num - 1] };
		}
		return Array2.Empty<Op>();
	}

	public EvexHandler(EncFlags1 encFlags1, EncFlags2 encFlags2, EncFlags3 encFlags3)
		: base(encFlags2, encFlags3, isSpecialInstr: false, tryConvertToDisp8N, CreateOps(encFlags1))
	{
		tupleType = (TupleType)(((uint)encFlags3 >> 7) & 0x1F);
		table = ((uint)encFlags2 >> 17) & 7;
		p1Bits = 4 | (((uint)encFlags2 >> 20) & 3);
		wbit = (WBit)(((uint)encFlags2 >> 22) & 3);
		if (wbit == WBit.W1)
		{
			p1Bits |= 128u;
		}
		switch ((LBit)(((uint)encFlags2 >> 24) & 7))
		{
		case LBit.LIG:
			llBits = 0u;
			mask_LL = 96u;
			break;
		case LBit.L0:
		case LBit.LZ:
		case LBit.L128:
			llBits = 0u;
			break;
		case LBit.L1:
		case LBit.L256:
			llBits = 32u;
			break;
		case LBit.L512:
			llBits = 64u;
			break;
		default:
			throw new InvalidOperationException();
		}
		if (wbit == WBit.WIG)
		{
			mask_W |= 128u;
		}
	}

	public override void Encode(Encoder encoder, in Instruction instruction)
	{
		encoder.WritePrefixes(in instruction);
		uint encoderFlags = (uint)encoder.EncoderFlags;
		encoder.WriteByteInternal(98u);
		uint num = table;
		num |= (encoderFlags & 7) << 5;
		num |= (encoderFlags >> 5) & 0x10;
		num ^= 0xFFFFFFF0u;
		encoder.WriteByteInternal(num);
		num = p1Bits;
		num |= (~encoderFlags >> 24) & 0x78;
		num |= mask_W & encoder.Internal_EVEX_WIG;
		encoder.WriteByteInternal(num);
		num = instruction.InternalOpMask;
		if (num != 0)
		{
			if ((EncFlags3 & EncFlags3.OpMaskRegister) == 0)
			{
				encoder.ErrorMessage = "The instruction doesn't support opmask registers";
			}
		}
		else if (((uint)EncFlags3 & 0x80000000u) != 0)
		{
			encoder.ErrorMessage = "The instruction must use an opmask register";
		}
		num |= (encoderFlags >> 28) & 8;
		if (instruction.SuppressAllExceptions)
		{
			if ((EncFlags3 & EncFlags3.SuppressAllExceptions) == 0)
			{
				encoder.ErrorMessage = "The instruction doesn't support suppress-all-exceptions";
			}
			num |= 0x10;
		}
		RoundingControl roundingControl = instruction.RoundingControl;
		if (roundingControl != 0)
		{
			if ((EncFlags3 & EncFlags3.RoundingControl) == 0)
			{
				encoder.ErrorMessage = "The instruction doesn't support rounding control";
			}
			num |= 0x10;
			num |= (uint)((int)(roundingControl - 1) << 5);
		}
		else if ((EncFlags3 & EncFlags3.SuppressAllExceptions) == 0 || !instruction.SuppressAllExceptions)
		{
			num |= llBits;
		}
		if ((encoderFlags & 0x400) != 0)
		{
			num |= 0x10;
		}
		else if (instruction.IsBroadcast)
		{
			encoder.ErrorMessage = "The instruction doesn't support broadcasting";
		}
		if (instruction.ZeroingMasking)
		{
			if ((EncFlags3 & EncFlags3.ZeroingMasking) == 0)
			{
				encoder.ErrorMessage = "The instruction doesn't support zeroing masking";
			}
			num |= 0x80;
		}
		num ^= 8;
		num |= mask_LL & encoder.Internal_EVEX_LIG;
		encoder.WriteByteInternal(num);
	}
}
