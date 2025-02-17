using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_EVEX : OpCodeHandlerModRM
{
	private readonly OpCodeHandler handlerMem;

	public OpCodeHandler_EVEX(OpCodeHandler handlerMem)
	{
		this.handlerMem = handlerMem ?? throw new ArgumentNullException("handlerMem");
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (decoder.is64bMode)
		{
			decoder.EVEX_MVEX(ref instruction);
		}
		else if (decoder.state.mod == 3)
		{
			decoder.EVEX_MVEX(ref instruction);
		}
		else
		{
			handlerMem.Decode(decoder, ref instruction);
		}
	}
}
