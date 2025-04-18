using System;

namespace Iced.Intel.EncoderInternal;

internal sealed class VexHandler : OpCodeHandler
{
	private readonly uint table;

	private readonly uint lastByte;

	private readonly uint mask_W_L;

	private readonly uint mask_L;

	private readonly uint W1;

	private static Op[] CreateOps(EncFlags1 encFlags1)
	{
		int num = (int)(encFlags1 & EncFlags1.VEX_OpMask);
		int num2 = (int)(((uint)encFlags1 >> 6) & 0x3F);
		int num3 = (int)(((uint)encFlags1 >> 12) & 0x3F);
		int num4 = (int)(((uint)encFlags1 >> 18) & 0x3F);
		int num5 = (int)(((uint)encFlags1 >> 24) & 0x3F);
		if (num5 != 0)
		{
			return new Op[5]
			{
				OpHandlerData.VexOps[num - 1],
				OpHandlerData.VexOps[num2 - 1],
				OpHandlerData.VexOps[num3 - 1],
				OpHandlerData.VexOps[num4 - 1],
				OpHandlerData.VexOps[num5 - 1]
			};
		}
		if (num4 != 0)
		{
			return new Op[4]
			{
				OpHandlerData.VexOps[num - 1],
				OpHandlerData.VexOps[num2 - 1],
				OpHandlerData.VexOps[num3 - 1],
				OpHandlerData.VexOps[num4 - 1]
			};
		}
		if (num3 != 0)
		{
			return new Op[3]
			{
				OpHandlerData.VexOps[num - 1],
				OpHandlerData.VexOps[num2 - 1],
				OpHandlerData.VexOps[num3 - 1]
			};
		}
		if (num2 != 0)
		{
			return new Op[2]
			{
				OpHandlerData.VexOps[num - 1],
				OpHandlerData.VexOps[num2 - 1]
			};
		}
		if (num != 0)
		{
			return new Op[1] { OpHandlerData.VexOps[num - 1] };
		}
		return Array2.Empty<Op>();
	}

	public VexHandler(EncFlags1 encFlags1, EncFlags2 encFlags2, EncFlags3 encFlags3)
		: base(encFlags2, encFlags3, isSpecialInstr: false, null, CreateOps(encFlags1))
	{
		table = ((uint)encFlags2 >> 17) & 7;
		WBit wBit = (WBit)(((uint)encFlags2 >> 22) & 3);
		W1 = ((wBit == WBit.W1) ? uint.MaxValue : 0u);
		LBit lBit = (LBit)(((uint)encFlags2 >> 24) & 7);
		if (lBit == LBit.L1 || lBit == LBit.L256)
		{
			lastByte = 4u;
		}
		if (W1 != 0)
		{
			lastByte |= 128u;
		}
		lastByte |= ((uint)encFlags2 >> 20) & 3;
		if (wBit == WBit.WIG)
		{
			mask_W_L |= 128u;
		}
		if (lBit == LBit.LIG)
		{
			mask_W_L |= 4u;
			mask_L |= 4u;
		}
	}

	public override void Encode(Encoder encoder, in Instruction instruction)
	{
		encoder.WritePrefixes(in instruction);
		uint encoderFlags = (uint)encoder.EncoderFlags;
		uint num = lastByte;
		num |= (~encoderFlags >> 24) & 0x78;
		if ((encoder.Internal_PreventVEX2 | W1 | (table - 1) | (encoderFlags & 0xB)) != 0)
		{
			encoder.WriteByteInternal(196u);
			uint num2 = table;
			num2 |= (~encoderFlags & 7) << 5;
			encoder.WriteByteInternal(num2);
			num |= mask_W_L & encoder.Internal_VEX_WIG_LIG;
			encoder.WriteByteInternal(num);
		}
		else
		{
			encoder.WriteByteInternal(197u);
			num |= (~encoderFlags & 4) << 5;
			num |= mask_L & encoder.Internal_VEX_LIG;
			encoder.WriteByteInternal(num);
		}
	}
}
