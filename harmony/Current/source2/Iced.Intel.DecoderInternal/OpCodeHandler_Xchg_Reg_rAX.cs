namespace Iced.Intel.DecoderInternal;

internal sealed class OpCodeHandler_Xchg_Reg_rAX : OpCodeHandler
{
	private readonly int index;

	private readonly Code[] codes;

	private static readonly Code[] s_codes = new Code[48]
	{
		Code.Nopw,
		Code.Xchg_r16_AX,
		Code.Xchg_r16_AX,
		Code.Xchg_r16_AX,
		Code.Xchg_r16_AX,
		Code.Xchg_r16_AX,
		Code.Xchg_r16_AX,
		Code.Xchg_r16_AX,
		Code.Xchg_r16_AX,
		Code.Xchg_r16_AX,
		Code.Xchg_r16_AX,
		Code.Xchg_r16_AX,
		Code.Xchg_r16_AX,
		Code.Xchg_r16_AX,
		Code.Xchg_r16_AX,
		Code.Xchg_r16_AX,
		Code.Nopd,
		Code.Xchg_r32_EAX,
		Code.Xchg_r32_EAX,
		Code.Xchg_r32_EAX,
		Code.Xchg_r32_EAX,
		Code.Xchg_r32_EAX,
		Code.Xchg_r32_EAX,
		Code.Xchg_r32_EAX,
		Code.Xchg_r32_EAX,
		Code.Xchg_r32_EAX,
		Code.Xchg_r32_EAX,
		Code.Xchg_r32_EAX,
		Code.Xchg_r32_EAX,
		Code.Xchg_r32_EAX,
		Code.Xchg_r32_EAX,
		Code.Xchg_r32_EAX,
		Code.Nopq,
		Code.Xchg_r64_RAX,
		Code.Xchg_r64_RAX,
		Code.Xchg_r64_RAX,
		Code.Xchg_r64_RAX,
		Code.Xchg_r64_RAX,
		Code.Xchg_r64_RAX,
		Code.Xchg_r64_RAX,
		Code.Xchg_r64_RAX,
		Code.Xchg_r64_RAX,
		Code.Xchg_r64_RAX,
		Code.Xchg_r64_RAX,
		Code.Xchg_r64_RAX,
		Code.Xchg_r64_RAX,
		Code.Xchg_r64_RAX,
		Code.Xchg_r64_RAX
	};

	public OpCodeHandler_Xchg_Reg_rAX(int index)
	{
		this.index = index;
		codes = s_codes;
	}

	public override void Decode(Decoder decoder, ref Instruction instruction)
	{
		if (index == 0 && decoder.state.zs.mandatoryPrefix == MandatoryPrefixByte.PF3 && (decoder.options & DecoderOptions.NoPause) == 0)
		{
			decoder.ClearMandatoryPrefixF3(ref instruction);
			instruction.InternalSetCodeNoCheck(Code.Pause);
			return;
		}
		int operandSize = (int)decoder.state.operandSize;
		int num = index + (int)decoder.state.zs.extraBaseRegisterBase;
		instruction.InternalSetCodeNoCheck(codes[operandSize * 16 + num]);
		if (num != 0)
		{
			Register op0Register = (Register)(operandSize * 16 + num + 21);
			instruction.Op0Register = op0Register;
			instruction.Op1Register = (Register)(operandSize * 16 + 21);
		}
	}
}
