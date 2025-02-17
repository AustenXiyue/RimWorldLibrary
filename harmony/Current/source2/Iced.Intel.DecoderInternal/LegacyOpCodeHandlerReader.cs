using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class LegacyOpCodeHandlerReader : OpCodeHandlerReader
{
	public override int ReadHandlers(ref TableDeserializer deserializer, OpCodeHandler?[] result, int resultIndex)
	{
		ref OpCodeHandler reference = ref result[resultIndex];
		switch (deserializer.ReadLegacyOpCodeHandlerKind())
		{
		case LegacyOpCodeHandlerKind.Bitness:
			reference = new OpCodeHandler_Bitness(deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case LegacyOpCodeHandlerKind.Bitness_DontReadModRM:
			reference = new OpCodeHandler_Bitness_DontReadModRM(deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case LegacyOpCodeHandlerKind.Invalid:
			reference = OpCodeHandler_Invalid.Instance;
			return 1;
		case LegacyOpCodeHandlerKind.Invalid_NoModRM:
			reference = OpCodeHandler_Invalid_NoModRM.Instance;
			return 1;
		case LegacyOpCodeHandlerKind.Invalid2:
			result[resultIndex] = OpCodeHandler_Invalid.Instance;
			result[resultIndex + 1] = OpCodeHandler_Invalid.Instance;
			return 2;
		case LegacyOpCodeHandlerKind.Dup:
		{
			int num = deserializer.ReadInt32();
			OpCodeHandler opCodeHandler = deserializer.ReadHandlerOrNull();
			for (int i = 0; i < num; i++)
			{
				result[resultIndex + i] = opCodeHandler;
			}
			return num;
		}
		case LegacyOpCodeHandlerKind.Null:
			reference = null;
			return 1;
		case LegacyOpCodeHandlerKind.HandlerReference:
			reference = deserializer.ReadHandlerReference();
			return 1;
		case LegacyOpCodeHandlerKind.ArrayReference:
			throw new InvalidOperationException();
		case LegacyOpCodeHandlerKind.RM:
			reference = new OpCodeHandler_RM(deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case LegacyOpCodeHandlerKind.Options1632_1:
			reference = new OpCodeHandler_Options1632(deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadDecoderOptions());
			return 1;
		case LegacyOpCodeHandlerKind.Options1632_2:
			reference = new OpCodeHandler_Options1632(deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadDecoderOptions(), deserializer.ReadHandler(), deserializer.ReadDecoderOptions());
			return 1;
		case LegacyOpCodeHandlerKind.Options3:
			reference = new OpCodeHandler_Options(deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadDecoderOptions());
			return 1;
		case LegacyOpCodeHandlerKind.Options5:
			reference = new OpCodeHandler_Options(deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadDecoderOptions(), deserializer.ReadHandler(), deserializer.ReadDecoderOptions());
			return 1;
		case LegacyOpCodeHandlerKind.Options_DontReadModRM:
			reference = new OpCodeHandler_Options_DontReadModRM(deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadDecoderOptions());
			return 1;
		case LegacyOpCodeHandlerKind.AnotherTable:
			reference = new OpCodeHandler_AnotherTable(deserializer.ReadArrayReference(8u));
			return 1;
		case LegacyOpCodeHandlerKind.Group:
			reference = new OpCodeHandler_Group(deserializer.ReadArrayReference(8u));
			return 1;
		case LegacyOpCodeHandlerKind.Group8x64:
			reference = new OpCodeHandler_Group8x64(deserializer.ReadArrayReference(8u), deserializer.ReadArrayReference(8u));
			return 1;
		case LegacyOpCodeHandlerKind.Group8x8:
			reference = new OpCodeHandler_Group8x8(deserializer.ReadArrayReference(8u), deserializer.ReadArrayReference(8u));
			return 1;
		case LegacyOpCodeHandlerKind.MandatoryPrefix:
			reference = new OpCodeHandler_MandatoryPrefix(deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case LegacyOpCodeHandlerKind.MandatoryPrefix4:
			reference = new OpCodeHandler_MandatoryPrefix4(deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler(), (uint)deserializer.ReadInt32());
			return 1;
		case LegacyOpCodeHandlerKind.MandatoryPrefix_NoModRM:
			reference = new OpCodeHandler_MandatoryPrefix_NoModRM(deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case LegacyOpCodeHandlerKind.MandatoryPrefix3:
			reference = new OpCodeHandler_MandatoryPrefix3(deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadLegacyHandlerFlags());
			return 1;
		case LegacyOpCodeHandlerKind.D3NOW:
			reference = new OpCodeHandler_D3NOW();
			return 1;
		case LegacyOpCodeHandlerKind.EVEX:
			reference = new OpCodeHandler_EVEX(deserializer.ReadHandler());
			return 1;
		case LegacyOpCodeHandlerKind.VEX2:
			reference = new OpCodeHandler_VEX2(deserializer.ReadHandler());
			return 1;
		case LegacyOpCodeHandlerKind.VEX3:
			reference = new OpCodeHandler_VEX3(deserializer.ReadHandler());
			return 1;
		case LegacyOpCodeHandlerKind.XOP:
			reference = new OpCodeHandler_XOP(deserializer.ReadHandler());
			return 1;
		case LegacyOpCodeHandlerKind.AL_DX:
			reference = new OpCodeHandler_AL_DX(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Ap:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ap(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.B_BM:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_B_BM(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.B_Ev:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_B_Ev(code, code + 1, deserializer.ReadBoolean());
			return 1;
		}
		case LegacyOpCodeHandlerKind.B_MIB:
			reference = new OpCodeHandler_B_MIB(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.BM_B:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_BM_B(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.BranchIw:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_BranchIw(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.BranchSimple:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_BranchSimple(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.C_R_3a:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_C_R(code, code + 1, deserializer.ReadRegister());
			return 1;
		}
		case LegacyOpCodeHandlerKind.C_R_3b:
			reference = new OpCodeHandler_C_R(deserializer.ReadCode(), Code.INVALID, deserializer.ReadRegister());
			return 1;
		case LegacyOpCodeHandlerKind.DX_AL:
			reference = new OpCodeHandler_DX_AL(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.DX_eAX:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_DX_eAX(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.eAX_DX:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_eAX_DX(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Eb_1:
			reference = new OpCodeHandler_Eb(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Eb_2:
			reference = new OpCodeHandler_Eb(deserializer.ReadCode(), deserializer.ReadHandlerFlags());
			return 1;
		case LegacyOpCodeHandlerKind.Eb_CL:
			reference = new OpCodeHandler_Eb_CL(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Eb_Gb_1:
			reference = new OpCodeHandler_Eb_Gb(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Eb_Gb_2:
			reference = new OpCodeHandler_Eb_Gb(deserializer.ReadCode(), deserializer.ReadHandlerFlags());
			return 1;
		case LegacyOpCodeHandlerKind.Eb_Ib_1:
			reference = new OpCodeHandler_Eb_Ib(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Eb_Ib_2:
			reference = new OpCodeHandler_Eb_Ib(deserializer.ReadCode(), deserializer.ReadHandlerFlags());
			return 1;
		case LegacyOpCodeHandlerKind.Eb1:
			reference = new OpCodeHandler_Eb_1(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Ed_V_Ib:
		{
			Code code;
			reference = new OpCodeHandler_Ed_V_Ib(code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ep:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ep(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_3a:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_3b:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev(code, code + 1, Code.INVALID);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_4:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev(code, code + 1, code + 2, deserializer.ReadHandlerFlags());
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_CL:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_CL(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_Gv_32_64:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_Gv_32_64(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_Gv_3a:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_Gv(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_Gv_3b:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_Gv(code, code + 1, Code.INVALID);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_Gv_4:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_Gv(code, code + 1, code + 2, deserializer.ReadHandlerFlags());
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_Gv_CL:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_Gv_CL(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_Gv_Ib:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_Gv_Ib(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_Gv_REX:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_Gv_REX(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_Ib_3:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_Ib(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_Ib_4:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_Ib(code, code + 1, code + 2, deserializer.ReadHandlerFlags());
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_Ib2_3:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_Ib2(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_Ib2_4:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_Ib2(code, code + 1, code + 2, deserializer.ReadHandlerFlags());
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_Iz_3:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_Iz(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_Iz_4:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_Iz(code, code + 1, code + 2, deserializer.ReadHandlerFlags());
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_P:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_P(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_REXW_1a:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_REXW(code, Code.INVALID, (uint)deserializer.ReadInt32());
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_REXW:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_REXW(code, code + 1, (uint)deserializer.ReadInt32());
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_Sw:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_Sw(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev_VX:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_VX(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ev1:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ev_1(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Evj:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Evj(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Evw:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Evw(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ew:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ew(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gb_Eb:
			reference = new OpCodeHandler_Gb_Eb(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Gdq_Ev:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gdq_Ev(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Eb:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_Eb(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Eb_REX:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_Eb_REX(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Ev_32_64:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_Ev_32_64(code, code + 1, deserializer.ReadBoolean(), deserializer.ReadBoolean());
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Ev_3a:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_Ev(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Ev_3b:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_Ev(code, code + 1, Code.INVALID);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Ev_Ib:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_Ev_Ib(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Ev_Ib_REX:
		{
			Code code;
			reference = new OpCodeHandler_Gv_Ev_Ib_REX(code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Ev_Iz:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_Ev_Iz(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Ev_REX:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_Ev_REX(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Ev2:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_Ev2(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Ev3:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_Ev3(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Ew:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_Ew(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_M:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_M(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_M_as:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_M_as(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Ma:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_Ma(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Mp_2:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_Mp(code, code + 1, Code.INVALID);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Mp_3:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_Mp(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_Mv:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_Mv(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_N:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_N(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_N_Ib_REX:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Gv_N_Ib_REX(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_RX:
		{
			Code code;
			reference = new OpCodeHandler_Gv_RX(code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Gv_W:
		{
			Code code;
			reference = new OpCodeHandler_Gv_W(code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.GvM_VX_Ib:
		{
			Code code;
			reference = new OpCodeHandler_GvM_VX_Ib(code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Ib:
			reference = new OpCodeHandler_Ib(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Ib3:
			reference = new OpCodeHandler_Ib3(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.IbReg:
			reference = new OpCodeHandler_IbReg(deserializer.ReadCode(), deserializer.ReadRegister());
			return 1;
		case LegacyOpCodeHandlerKind.IbReg2:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_IbReg2(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Iw_Ib:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Iw_Ib(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Jb:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Jb(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Jb2:
			reference = new OpCodeHandler_Jb2(deserializer.ReadCode(), deserializer.ReadCode(), deserializer.ReadCode(), deserializer.ReadCode(), deserializer.ReadCode(), deserializer.ReadCode(), deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Jdisp:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Jdisp(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Jx:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Jx(code, code + 1, deserializer.ReadCode());
			return 1;
		}
		case LegacyOpCodeHandlerKind.Jz:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Jz(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.M_1:
			reference = new OpCodeHandler_M(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.M_2:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_M(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.M_REXW_2:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_M_REXW(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.M_REXW_4:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_M_REXW(code, code + 1, deserializer.ReadHandlerFlags(), deserializer.ReadHandlerFlags());
			return 1;
		}
		case LegacyOpCodeHandlerKind.MemBx:
			reference = new OpCodeHandler_MemBx(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Mf_1:
			reference = new OpCodeHandler_Mf(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Mf_2a:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Mf(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Mf_2b:
			reference = new OpCodeHandler_Mf(deserializer.ReadCode(), deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.MIB_B:
			reference = new OpCodeHandler_MIB_B(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.MP:
			reference = new OpCodeHandler_MP(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Ms:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ms(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.MV:
			reference = new OpCodeHandler_MV(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Mv_Gv:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Mv_Gv(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Mv_Gv_REXW:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Mv_Gv_REXW(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.NIb:
			reference = new OpCodeHandler_NIb(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Ob_Reg:
			reference = new OpCodeHandler_Ob_Reg(deserializer.ReadCode(), deserializer.ReadRegister());
			return 1;
		case LegacyOpCodeHandlerKind.Ov_Reg:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Ov_Reg(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.P_Ev:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_P_Ev(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.P_Ev_Ib:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_P_Ev_Ib(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.P_Q:
			reference = new OpCodeHandler_P_Q(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.P_Q_Ib:
			reference = new OpCodeHandler_P_Q_Ib(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.P_R:
			reference = new OpCodeHandler_P_R(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.P_W:
			reference = new OpCodeHandler_P_W(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.PushEv:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_PushEv(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.PushIb2:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_PushIb2(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.PushIz:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_PushIz(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.PushOpSizeReg_4a:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_PushOpSizeReg(code, code + 1, code + 2, deserializer.ReadRegister());
			return 1;
		}
		case LegacyOpCodeHandlerKind.PushOpSizeReg_4b:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_PushOpSizeReg(code, code + 1, Code.INVALID, deserializer.ReadRegister());
			return 1;
		}
		case LegacyOpCodeHandlerKind.PushSimple2:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_PushSimple2(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.PushSimpleReg:
		{
			Code code;
			reference = new OpCodeHandler_PushSimpleReg(deserializer.ReadInt32(), code = deserializer.ReadCode(), code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Q_P:
			reference = new OpCodeHandler_Q_P(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.R_C_3a:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_R_C(code, code + 1, deserializer.ReadRegister());
			return 1;
		}
		case LegacyOpCodeHandlerKind.R_C_3b:
			reference = new OpCodeHandler_R_C(deserializer.ReadCode(), Code.INVALID, deserializer.ReadRegister());
			return 1;
		case LegacyOpCodeHandlerKind.rDI_P_N:
			reference = new OpCodeHandler_rDI_P_N(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.rDI_VX_RX:
			reference = new OpCodeHandler_rDI_VX_RX(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Reg:
			reference = new OpCodeHandler_Reg(deserializer.ReadCode(), deserializer.ReadRegister());
			return 1;
		case LegacyOpCodeHandlerKind.Reg_Ib2:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Reg_Ib2(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Reg_Iz:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Reg_Iz(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Reg_Ob:
			reference = new OpCodeHandler_Reg_Ob(deserializer.ReadCode(), deserializer.ReadRegister());
			return 1;
		case LegacyOpCodeHandlerKind.Reg_Ov:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Reg_Ov(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Reg_Xb:
			reference = new OpCodeHandler_Reg_Xb(deserializer.ReadCode(), deserializer.ReadRegister());
			return 1;
		case LegacyOpCodeHandlerKind.Reg_Xv:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Reg_Xv(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Reg_Xv2:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Reg_Xv2(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Reg_Yb:
			reference = new OpCodeHandler_Reg_Yb(deserializer.ReadCode(), deserializer.ReadRegister());
			return 1;
		case LegacyOpCodeHandlerKind.Reg_Yv:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Reg_Yv(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.RegIb:
			reference = new OpCodeHandler_RegIb(deserializer.ReadCode(), deserializer.ReadRegister());
			return 1;
		case LegacyOpCodeHandlerKind.RegIb3:
			reference = new OpCodeHandler_RegIb3(deserializer.ReadInt32());
			return 1;
		case LegacyOpCodeHandlerKind.RegIz2:
			reference = new OpCodeHandler_RegIz2(deserializer.ReadInt32());
			return 1;
		case LegacyOpCodeHandlerKind.Reservednop:
			reference = new OpCodeHandler_Reservednop(deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case LegacyOpCodeHandlerKind.RIb:
			reference = new OpCodeHandler_RIb(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.RIbIb:
			reference = new OpCodeHandler_RIbIb(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Rv:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Rv(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Rv_32_64:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Rv_32_64(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.RvMw_Gw:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_RvMw_Gw(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Simple:
			reference = new OpCodeHandler_Simple(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Simple_ModRM:
			reference = new OpCodeHandler_Simple_ModRM(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Simple2_3a:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Simple2(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Simple2_3b:
			reference = new OpCodeHandler_Simple2(deserializer.ReadCode(), deserializer.ReadCode(), deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Simple2Iw:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Simple2Iw(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Simple3:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Simple3(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Simple4:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Simple4(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Simple4b:
		{
			Code code = deserializer.ReadCode();
			Code code2 = deserializer.ReadCode();
			reference = new OpCodeHandler_Simple4(code, code2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Simple5:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Simple5(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Simple5_a32:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Simple5_a32(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Simple5_ModRM_as:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Simple5_ModRM_as(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.SimpleReg:
			reference = new OpCodeHandler_SimpleReg(deserializer.ReadCode(), deserializer.ReadInt32());
			return 1;
		case LegacyOpCodeHandlerKind.ST_STi:
			reference = new OpCodeHandler_ST_STi(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.STi:
			reference = new OpCodeHandler_STi(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.STi_ST:
			reference = new OpCodeHandler_STi_ST(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Sw_Ev:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Sw_Ev(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.V_Ev:
		{
			Code code;
			reference = new OpCodeHandler_V_Ev(code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.VM:
			reference = new OpCodeHandler_VM(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.VN:
			reference = new OpCodeHandler_VN(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.VQ:
			reference = new OpCodeHandler_VQ(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.VRIbIb:
			reference = new OpCodeHandler_VRIbIb(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.VW_2:
			reference = new OpCodeHandler_VW(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.VW_3:
			reference = new OpCodeHandler_VW(deserializer.ReadCode(), deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.VWIb_2:
			reference = new OpCodeHandler_VWIb(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.VWIb_3:
		{
			Code code;
			reference = new OpCodeHandler_VWIb(code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.VX_E_Ib:
		{
			Code code;
			reference = new OpCodeHandler_VX_E_Ib(code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.VX_Ev:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_VX_Ev(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Wbinvd:
			reference = new OpCodeHandler_Wbinvd();
			return 1;
		case LegacyOpCodeHandlerKind.WV:
			reference = new OpCodeHandler_WV(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Xb_Yb:
			reference = new OpCodeHandler_Xb_Yb(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Xchg_Reg_rAX:
			reference = new OpCodeHandler_Xchg_Reg_rAX(deserializer.ReadInt32());
			return 1;
		case LegacyOpCodeHandlerKind.Xv_Yv:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Xv_Yv(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Yb_Reg:
			reference = new OpCodeHandler_Yb_Reg(deserializer.ReadCode(), deserializer.ReadRegister());
			return 1;
		case LegacyOpCodeHandlerKind.Yb_Xb:
			reference = new OpCodeHandler_Yb_Xb(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Yv_Reg:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Yv_Reg(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Yv_Reg2:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Yv_Reg2(code, code + 1);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Yv_Xv:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Yv_Xv(code, code + 1, code + 2);
			return 1;
		}
		case LegacyOpCodeHandlerKind.M_Sw:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_M_Sw(code);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Sw_M:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_Sw_M(code);
			return 1;
		}
		case LegacyOpCodeHandlerKind.Rq:
			reference = new OpCodeHandler_Rq(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.Gd_Rd:
			reference = new OpCodeHandler_Gd_Rd(deserializer.ReadCode());
			return 1;
		case LegacyOpCodeHandlerKind.PrefixEsCsSsDs:
			reference = new OpCodeHandler_PrefixEsCsSsDs(deserializer.ReadRegister());
			return 1;
		case LegacyOpCodeHandlerKind.PrefixFsGs:
			reference = new OpCodeHandler_PrefixFsGs(deserializer.ReadRegister());
			return 1;
		case LegacyOpCodeHandlerKind.Prefix66:
			reference = new OpCodeHandler_Prefix66();
			return 1;
		case LegacyOpCodeHandlerKind.Prefix67:
			reference = new OpCodeHandler_Prefix67();
			return 1;
		case LegacyOpCodeHandlerKind.PrefixF0:
			reference = new OpCodeHandler_PrefixF0();
			return 1;
		case LegacyOpCodeHandlerKind.PrefixF2:
			reference = new OpCodeHandler_PrefixF2();
			return 1;
		case LegacyOpCodeHandlerKind.PrefixF3:
			reference = new OpCodeHandler_PrefixF3();
			return 1;
		case LegacyOpCodeHandlerKind.PrefixREX:
			reference = new OpCodeHandler_PrefixREX(deserializer.ReadHandler(), (uint)deserializer.ReadInt32());
			return 1;
		default:
			throw new InvalidOperationException();
		}
	}
}
