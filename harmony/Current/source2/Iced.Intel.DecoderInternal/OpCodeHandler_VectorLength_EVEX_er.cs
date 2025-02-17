using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_VectorLength_EVEX_er : OpCodeHandlerModRM
{
	private readonly OpCodeHandler[] handlers;

	public OpCodeHandler_VectorLength_EVEX_er(OpCodeHandler handler128, OpCodeHandler handler256, OpCodeHandler handler512)
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
		int num = (int)decoder.state.vectorLength;
		if (decoder.state.mod == 3 && (decoder.state.zs.flags & StateFlags.b) != 0)
		{
			num = 2;
		}
		handlers[num].Decode(decoder, ref instruction);
	}
}
