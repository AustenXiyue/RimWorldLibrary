using System;

namespace Iced.Intel.EncoderInternal;

internal sealed class XopHandler : OpCodeHandler
{
	private readonly uint table;

	private readonly uint lastByte;

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
				OpHandlerData.XopOps[num - 1],
				OpHandlerData.XopOps[num2 - 1],
				OpHandlerData.XopOps[num3 - 1],
				OpHandlerData.XopOps[num4 - 1]
			};
		}
		if (num3 != 0)
		{
			return new Op[3]
			{
				OpHandlerData.XopOps[num - 1],
				OpHandlerData.XopOps[num2 - 1],
				OpHandlerData.XopOps[num3 - 1]
			};
		}
		if (num2 != 0)
		{
			return new Op[2]
			{
				OpHandlerData.XopOps[num - 1],
				OpHandlerData.XopOps[num2 - 1]
			};
		}
		if (num != 0)
		{
			return new Op[1] { OpHandlerData.XopOps[num - 1] };
		}
		return Array2.Empty<Op>();
	}

	public XopHandler(EncFlags1 encFlags1, EncFlags2 encFlags2, EncFlags3 encFlags3)
		: base(encFlags2, encFlags3, isSpecialInstr: false, null, CreateOps(encFlags1))
	{
		table = 8 + (((uint)encFlags2 >> 17) & 7);
		LBit lBit = (LBit)(((uint)encFlags2 >> 24) & 7);
		if (lBit == LBit.L1 || lBit == LBit.L256)
		{
			lastByte = 4u;
		}
		if ((((uint)encFlags2 >> 22) & 3) == 1)
		{
			lastByte |= 128u;
		}
		lastByte |= ((uint)encFlags2 >> 20) & 3;
	}

	public override void Encode(Encoder encoder, in Instruction instruction)
	{
		encoder.WritePrefixes(in instruction);
		encoder.WriteByteInternal(143u);
		uint encoderFlags = (uint)encoder.EncoderFlags;
		uint num = table;
		num |= (~encoderFlags & 7) << 5;
		encoder.WriteByteInternal(num);
		num = lastByte;
		num |= (~encoderFlags >> 24) & 0x78;
		encoder.WriteByteInternal(num);
	}
}
