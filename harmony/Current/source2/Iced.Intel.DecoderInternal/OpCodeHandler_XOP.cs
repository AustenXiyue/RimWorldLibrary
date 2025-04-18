using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_XOP : OpCodeHandlerModRM
{
	private readonly OpCodeHandler handler_reg0;

	public OpCodeHandler_XOP(OpCodeHandler handler_reg0)
	{
		this.handler_reg0 = handler_reg0 ?? throw new ArgumentNullException("handler_reg0");
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((decoder.state.modrm & 0x1F) < 8)
		{
			handler_reg0.Decode(decoder, ref instruction);
		}
		else
		{
			decoder.XOP(ref instruction);
		}
	}
}
