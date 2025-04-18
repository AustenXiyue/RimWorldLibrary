using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Reservednop : OpCodeHandlerModRM
{
	private readonly OpCodeHandler reservedNopHandler;

	private readonly OpCodeHandler otherHandler;

	public OpCodeHandler_Reservednop(OpCodeHandler reservedNopHandler, OpCodeHandler otherHandler)
	{
		this.reservedNopHandler = reservedNopHandler ?? throw new ArgumentNullException("reservedNopHandler");
		this.otherHandler = otherHandler ?? throw new ArgumentNullException("otherHandler");
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		(((decoder.options & DecoderOptions.ForceReservedNop) != 0) ? reservedNopHandler : otherHandler).Decode(decoder, ref instruction);
	}
}
