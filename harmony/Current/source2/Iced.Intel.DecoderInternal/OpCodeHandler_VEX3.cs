using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VEX3 : OpCodeHandlerModRM
{
	private readonly OpCodeHandler handlerMem;

	public OpCodeHandler_VEX3(OpCodeHandler handlerMem)
	{
		this.handlerMem = handlerMem ?? throw new ArgumentNullException("handlerMem");
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.is64bMode)
		{
			decoder.VEX3(ref instruction);
		}
		else if (decoder.state.mod == 3)
		{
			decoder.VEX3(ref instruction);
		}
		else
		{
			handlerMem.Decode(decoder, ref instruction);
		}
	}
}
