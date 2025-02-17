using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_W : OpCodeHandlerModRM
{
	private readonly OpCodeHandler handlerW0;

	private readonly OpCodeHandler handlerW1;

	public OpCodeHandler_W(OpCodeHandler handlerW0, OpCodeHandler handlerW1)
	{
		this.handlerW0 = handlerW0 ?? throw new ArgumentNullException("handlerW0");
		this.handlerW1 = handlerW1 ?? throw new ArgumentNullException("handlerW1");
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		(((decoder.state.zs.flags & StateFlags.W) != 0) ? handlerW1 : handlerW0).Decode(decoder, ref instruction);
	}
}
