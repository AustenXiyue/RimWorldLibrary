using System;

namespace Iced.Intel.EncoderInternal;

internal sealed class LegacyHandler : OpCodeHandler
{
	private readonly uint tableByte1;

	private readonly uint tableByte2;

	private readonly uint mandatoryPrefix;

	private static Op[] CreateOps(EncFlags1 encFlags1)
	{
		int num = (int)(encFlags1 & EncFlags1.Legacy_OpMask);
		int num2 = (int)(((uint)encFlags1 >> 7) & 0x7F);
		int num3 = (int)(((uint)encFlags1 >> 14) & 0x7F);
		int num4 = (int)(((uint)encFlags1 >> 21) & 0x7F);
		if (num4 != 0)
		{
			return new Op[4]
			{
				OpHandlerData.LegacyOps[num - 1],
				OpHandlerData.LegacyOps[num2 - 1],
				OpHandlerData.LegacyOps[num3 - 1],
				OpHandlerData.LegacyOps[num4 - 1]
			};
		}
		if (num3 != 0)
		{
			return new Op[3]
			{
				OpHandlerData.LegacyOps[num - 1],
				OpHandlerData.LegacyOps[num2 - 1],
				OpHandlerData.LegacyOps[num3 - 1]
			};
		}
		if (num2 != 0)
		{
			return new Op[2]
			{
				OpHandlerData.LegacyOps[num - 1],
				OpHandlerData.LegacyOps[num2 - 1]
			};
		}
		if (num != 0)
		{
			return new Op[1] { OpHandlerData.LegacyOps[num - 1] };
		}
		return Array2.Empty<Op>();
	}

	public LegacyHandler(EncFlags1 encFlags1, EncFlags2 encFlags2, EncFlags3 encFlags3)
		: base(encFlags2, encFlags3, isSpecialInstr: false, null, CreateOps(encFlags1))
	{
		switch ((LegacyOpCodeTable)(((uint)encFlags2 >> 17) & 7))
		{
		case LegacyOpCodeTable.MAP0:
			tableByte1 = 0u;
			tableByte2 = 0u;
			break;
		case LegacyOpCodeTable.MAP0F:
			tableByte1 = 15u;
			tableByte2 = 0u;
			break;
		case LegacyOpCodeTable.MAP0F38:
			tableByte1 = 15u;
			tableByte2 = 56u;
			break;
		case LegacyOpCodeTable.MAP0F3A:
			tableByte1 = 15u;
			tableByte2 = 58u;
			break;
		default:
			throw new InvalidOperationException();
		}
		mandatoryPrefix = (MandatoryPrefixByte)(((uint)encFlags2 >> 20) & 3) switch
		{
			MandatoryPrefixByte.None => 0u, 
			MandatoryPrefixByte.P66 => 102u, 
			MandatoryPrefixByte.PF3 => 243u, 
			MandatoryPrefixByte.PF2 => 242u, 
			_ => throw new InvalidOperationException(), 
		};
	}

	public override void Encode(Encoder encoder, in Instruction instruction)
	{
		uint num = mandatoryPrefix;
		encoder.WritePrefixes(in instruction, num != 243);
		if (num != 0)
		{
			encoder.WriteByteInternal(num);
		}
		num = (uint)encoder.EncoderFlags;
		num &= 0x4F;
		if (num != 0)
		{
			if ((encoder.EncoderFlags & EncoderFlags.HighLegacy8BitRegs) != 0)
			{
				encoder.ErrorMessage = "Registers AH, CH, DH, BH can't be used if there's a REX prefix. Use AL, CL, DL, BL, SPL, BPL, SIL, DIL, R8L-R15L instead.";
			}
			num |= 0x40;
			encoder.WriteByteInternal(num);
		}
		if ((num = tableByte1) != 0)
		{
			encoder.WriteByteInternal(num);
			if ((num = tableByte2) != 0)
			{
				encoder.WriteByteInternal(num);
			}
		}
	}
}
