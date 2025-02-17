namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_PrefixFsGs : OpCodeHandler
{
	private readonly Register seg;

	public OpCodeHandler_PrefixFsGs(Register seg)
	{
		this.seg = seg;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.SegmentPrefix = seg;
		decoder.state.zs.segmentPrio = 1;
		decoder.ResetRexPrefixState();
		decoder.CallOpCodeHandlerXXTable(ref instruction);
	}
}
