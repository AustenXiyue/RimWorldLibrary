using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_RM : OpCodeHandlerModRM
{
	private readonly OpCodeHandler reg;

	private readonly OpCodeHandler mem;

	public OpCodeHandler_RM(OpCodeHandler reg, OpCodeHandler mem)
	{
		this.reg = reg ?? throw new ArgumentNullException("reg");
		this.mem = mem ?? throw new ArgumentNullException("mem");
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		((decoder.state.mod == 3) ? reg : mem).Decode(decoder, ref instruction);
	}
}
