using System;

namespace Iced.Intel.EncoderInternal;

internal sealed class ZeroBytesHandler : OpCodeHandler
{
	public ZeroBytesHandler(Code code)
		: base(EncFlags2.None, EncFlags3.Bit16or32 | EncFlags3.Bit64, isSpecialInstr: true, null, Array2.Empty<Op>())
	{
	}

	public override void Encode(Encoder encoder, in Instruction instruction)
	{
	}
}
