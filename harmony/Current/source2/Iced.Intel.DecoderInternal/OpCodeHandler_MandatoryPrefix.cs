using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_MandatoryPrefix : OpCodeHandlerModRM
{
	private readonly OpCodeHandler[] handlers;

	public OpCodeHandler_MandatoryPrefix(OpCodeHandler handler, OpCodeHandler handler66, OpCodeHandler handlerF3, OpCodeHandler handlerF2)
	{
		handlers = new OpCodeHandler[4]
		{
			handler ?? throw new ArgumentNullException("handler"),
			handler66 ?? throw new ArgumentNullException("handler66"),
			handlerF3 ?? throw new ArgumentNullException("handlerF3"),
			handlerF2 ?? throw new ArgumentNullException("handlerF2")
		};
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		decoder.ClearMandatoryPrefix(ref instruction);
		handlers[(uint)decoder.state.zs.mandatoryPrefix].Decode(decoder, ref instruction);
	}
}
