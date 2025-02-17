namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_PrefixEsCsSsDs : OpCodeHandler
{
	private readonly Register seg;

	public OpCodeHandler_PrefixEsCsSsDs(Register seg)
	{
		this.seg = seg;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (!decoder.is64bMode || decoder.state.zs.segmentPrio <= 0)
		{
			instruction.SegmentPrefix = seg;
		}
		decoder.ResetRexPrefixState();
		decoder.CallOpCodeHandlerXXTable(ref instruction);
	}
}
