using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX2 : OpCodeHandlerModRM
{
	private readonly OpCodeHandler handlerMem;

	public OpCodeHandler_VEX2(OpCodeHandler handlerMem)
	{
		this.handlerMem = handlerMem ?? throw new ArgumentNullException("handlerMem");
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.is64bMode)
		{
			decoder.VEX2(ref instruction);
		}
		else if (decoder.state.mod == 3)
		{
			decoder.VEX2(ref instruction);
		}
		else
		{
			handlerMem.Decode(decoder, ref instruction);
		}
	}
}
