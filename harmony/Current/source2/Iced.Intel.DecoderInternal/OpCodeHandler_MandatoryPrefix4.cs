using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_MandatoryPrefix4 : OpCodeHandler
{
	private readonly OpCodeHandler handlerNP;

	private readonly OpCodeHandler handler66;

	private readonly OpCodeHandler handlerF3;

	private readonly OpCodeHandler handlerF2;

	private readonly uint flags;

	public OpCodeHandler_MandatoryPrefix4(OpCodeHandler handlerNP, OpCodeHandler handler66, OpCodeHandler handlerF3, OpCodeHandler handlerF2, uint flags)
	{
		this.handlerNP = handlerNP ?? throw new ArgumentNullException("handlerNP");
		this.handler66 = handler66 ?? throw new ArgumentNullException("handler66");
		this.handlerF3 = handlerF3 ?? throw new ArgumentNullException("handlerF3");
		this.handlerF2 = handlerF2 ?? throw new ArgumentNullException("handlerF2");
		this.flags = flags;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		OpCodeHandler opCodeHandler;
		switch (decoder.state.zs.mandatoryPrefix)
		{
		case MandatoryPrefixByte.None:
			opCodeHandler = handlerNP;
			break;
		case MandatoryPrefixByte.P66:
			opCodeHandler = handler66;
			break;
		case MandatoryPrefixByte.PF3:
			if ((flags & 4) != 0)
			{
				decoder.ClearMandatoryPrefixF3(ref instruction);
			}
			opCodeHandler = handlerF3;
			break;
		case MandatoryPrefixByte.PF2:
			if ((flags & 8) != 0)
			{
				decoder.ClearMandatoryPrefixF2(ref instruction);
			}
			opCodeHandler = handlerF2;
			break;
		default:
			throw new InvalidOperationException();
		}
		if (opCodeHandler.HasModRM && (flags & 0x10) != 0)
		{
			decoder.ReadModRM();
		}
		opCodeHandler.Decode(decoder, ref instruction);
	}
}
