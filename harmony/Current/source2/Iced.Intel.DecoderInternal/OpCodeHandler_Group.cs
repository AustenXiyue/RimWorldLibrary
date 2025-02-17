using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Group : OpCodeHandlerModRM
{
	private readonly OpCodeHandler[] groupHandlers;

	public OpCodeHandler_Group(OpCodeHandler[] groupHandlers)
	{
		this.groupHandlers = groupHandlers ?? throw new ArgumentNullException("groupHandlers");
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		groupHandlers[decoder.state.reg].Decode(decoder, ref instruction);
	}
}
