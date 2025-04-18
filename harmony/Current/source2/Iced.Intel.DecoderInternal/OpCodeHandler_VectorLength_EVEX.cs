using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VectorLength_EVEX : OpCodeHandlerModRM
{
	private readonly OpCodeHandler[] handlers;

	public OpCodeHandler_VectorLength_EVEX(OpCodeHandler handler128, OpCodeHandler handler256, OpCodeHandler handler512)
	{
		handlers = new OpCodeHandler[4]
		{
			handler128 ?? throw new ArgumentNullException("handler128"),
			handler256 ?? throw new ArgumentNullException("handler256"),
			handler512 ?? throw new ArgumentNullException("handler512"),
			OpCodeHandler_Invalid.Instance
		};
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		handlers[decoder.state.vectorLength].Decode(decoder, ref instruction);
	}
}
