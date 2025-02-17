namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_D3NOW : OpCodeHandlerModRM
{
	internal static readonly Code[] CodeValues = CreateCodeValues();

	private readonly Code[] codeValues = CodeValues;

	private static Code[] CreateCodeValues()
	{
		Code[] array = new Code[256];
		array[191] = Code.D3NOW_Pavgusb_mm_mmm64;
		array[187] = Code.D3NOW_Pswapd_mm_mmm64;
		array[183] = Code.D3NOW_Pmulhrw_mm_mmm64;
		array[182] = Code.D3NOW_Pfrcpit2_mm_mmm64;
		array[180] = Code.D3NOW_Pfmul_mm_mmm64;
		array[176] = Code.D3NOW_Pfcmpeq_mm_mmm64;
		array[174] = Code.D3NOW_Pfacc_mm_mmm64;
		array[170] = Code.D3NOW_Pfsubr_mm_mmm64;
		array[167] = Code.D3NOW_Pfrsqit1_mm_mmm64;
		array[166] = Code.D3NOW_Pfrcpit1_mm_mmm64;
		array[164] = Code.D3NOW_Pfmax_mm_mmm64;
		array[160] = Code.D3NOW_Pfcmpgt_mm_mmm64;
		array[158] = Code.D3NOW_Pfadd_mm_mmm64;
		array[154] = Code.D3NOW_Pfsub_mm_mmm64;
		array[151] = Code.D3NOW_Pfrsqrt_mm_mmm64;
		array[150] = Code.D3NOW_Pfrcp_mm_mmm64;
		array[148] = Code.D3NOW_Pfmin_mm_mmm64;
		array[144] = Code.D3NOW_Pfcmpge_mm_mmm64;
		array[142] = Code.D3NOW_Pfpnacc_mm_mmm64;
		array[138] = Code.D3NOW_Pfnacc_mm_mmm64;
		array[135] = Code.D3NOW_Pfrsqrtv_mm_mmm64;
		array[134] = Code.D3NOW_Pfrcpv_mm_mmm64;
		array[29] = Code.D3NOW_Pf2id_mm_mmm64;
		array[28] = Code.D3NOW_Pf2iw_mm_mmm64;
		array[13] = Code.D3NOW_Pi2fd_mm_mmm64;
		array[12] = Code.D3NOW_Pi2fw_mm_mmm64;
		return array;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		instruction.Op0Register = (Register)(decoder.state.reg + 225);
		if (decoder.state.mod == 3)
		{
			instruction.Op1Register = (Register)(decoder.state.rm + 225);
		}
		else
		{
			instruction.Op1Kind = OpKind.Memory;
			decoder.ReadOpMem(ref instruction);
		}
		Code code = codeValues[decoder.ReadByte()];
		if ((uint)(code - 4181) <= 1u && ((decoder.options & DecoderOptions.Cyrix) == 0 || decoder.Bitness == 64))
		{
			code = Code.INVALID;
		}
		instruction.InternalSetCodeNoCheck(code);
		if (code == Code.INVALID)
		{
			decoder.SetInvalidInstruction();
		}
	}
}
