namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Wbinvd : OpCodeHandler
{
	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if ((decoder.options & DecoderOptions.NoWbnoinvd) != 0 || decoder.state.zs.mandatoryPrefix != MandatoryPrefixByte.PF3)
		{
			instruction.InternalSetCodeNoCheck(Code.Wbinvd);
			return;
		}
		decoder.ClearMandatoryPrefixF3(ref instruction);
		instruction.InternalSetCodeNoCheck(Code.Wbnoinvd);
	}
}
