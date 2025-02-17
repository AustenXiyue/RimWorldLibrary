using System;

namespace Iced.Intel.EncoderInternal;

internal sealed class InvalidHandler : OpCodeHandler
{
	internal const string ERROR_MESSAGE = "Can't encode an invalid instruction";

	public InvalidHandler()
		: base(EncFlags2.None, EncFlags3.Bit16or32 | EncFlags3.Bit64, isSpecialInstr: false, null, Array2.Empty<Op>())
	{
	}

	public override void Encode(Encoder encoder, in Instruction instruction)
	{
		encoder.ErrorMessage = "Can't encode an invalid instruction";
	}
}
