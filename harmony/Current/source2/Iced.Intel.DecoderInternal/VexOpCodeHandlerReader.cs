using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class VexOpCodeHandlerReader : OpCodeHandlerReader
{
	public override int ReadHandlers(ref TableDeserializer deserializer, OpCodeHandler?[] result, int resultIndex)
	{
		ref OpCodeHandler reference = ref result[resultIndex];
		switch (deserializer.ReadVexOpCodeHandlerKind())
		{
		case VexOpCodeHandlerKind.Invalid:
			reference = OpCodeHandler_Invalid.Instance;
			return 1;
		case VexOpCodeHandlerKind.Invalid2:
			result[resultIndex] = OpCodeHandler_Invalid.Instance;
			result[resultIndex + 1] = OpCodeHandler_Invalid.Instance;
			return 2;
		case VexOpCodeHandlerKind.Dup:
		{
			int num = deserializer.ReadInt32();
			OpCodeHandler opCodeHandler = deserializer.ReadHandlerOrNull();
			for (int i = 0; i < num; i++)
			{
				result[resultIndex + i] = opCodeHandler;
			}
			return num;
		}
		case VexOpCodeHandlerKind.Null:
			reference = null;
			return 1;
		case VexOpCodeHandlerKind.Invalid_NoModRM:
			reference = OpCodeHandler_Invalid_NoModRM.Instance;
			return 1;
		case VexOpCodeHandlerKind.Bitness:
			reference = new OpCodeHandler_Bitness(deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case VexOpCodeHandlerKind.Bitness_DontReadModRM:
			reference = new OpCodeHandler_Bitness_DontReadModRM(deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case VexOpCodeHandlerKind.HandlerReference:
			reference = deserializer.ReadHandlerReference();
			return 1;
		case VexOpCodeHandlerKind.ArrayReference:
			throw new InvalidOperationException();
		case VexOpCodeHandlerKind.RM:
			reference = new OpCodeHandler_RM(deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case VexOpCodeHandlerKind.Group:
			reference = new OpCodeHandler_Group(deserializer.ReadArrayReference(6u));
			return 1;
		case VexOpCodeHandlerKind.Group8x64:
			reference = new OpCodeHandler_Group8x64(deserializer.ReadArrayReference(6u), deserializer.ReadArrayReference(6u));
			return 1;
		case VexOpCodeHandlerKind.W:
			reference = new OpCodeHandler_W(deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case VexOpCodeHandlerKind.MandatoryPrefix2_1:
			reference = new OpCodeHandler_MandatoryPrefix2(deserializer.ReadHandler());
			return 1;
		case VexOpCodeHandlerKind.MandatoryPrefix2_4:
			reference = new OpCodeHandler_MandatoryPrefix2(deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case VexOpCodeHandlerKind.MandatoryPrefix2_NoModRM:
			reference = new OpCodeHandler_MandatoryPrefix2_NoModRM(deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case VexOpCodeHandlerKind.VectorLength_NoModRM:
			reference = new OpCodeHandler_VectorLength_NoModRM_VEX(deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case VexOpCodeHandlerKind.VectorLength:
			reference = new OpCodeHandler_VectorLength_VEX(deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case VexOpCodeHandlerKind.Ed_V_Ib:
		{
			Code code;
			reference = new OpCodeHandler_VEX_Ed_V_Ib(deserializer.ReadRegister(), code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.Ev_VX:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_VEX_Ev_VX(code, code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.G_VK:
			reference = new OpCodeHandler_VEX_G_VK(deserializer.ReadCode(), deserializer.ReadRegister());
			return 1;
		case VexOpCodeHandlerKind.Gv_Ev_Gv:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_VEX_Gv_Ev_Gv(code, code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.Gv_Ev_Ib:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_VEX_Gv_Ev_Ib(code, code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.Gv_Ev_Id:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_VEX_Gv_Ev_Id(code, code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.Gv_GPR_Ib:
		{
			Code code;
			reference = new OpCodeHandler_VEX_Gv_GPR_Ib(deserializer.ReadRegister(), code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.Gv_Gv_Ev:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_VEX_Gv_Gv_Ev(code, code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.Gv_RX:
		{
			Code code;
			reference = new OpCodeHandler_VEX_Gv_RX(deserializer.ReadRegister(), code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.Gv_W:
		{
			Code code;
			reference = new OpCodeHandler_VEX_Gv_W(deserializer.ReadRegister(), code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.GvM_VX_Ib:
		{
			Code code;
			reference = new OpCodeHandler_VEX_GvM_VX_Ib(deserializer.ReadRegister(), code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.HRIb:
			reference = new OpCodeHandler_VEX_HRIb(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.Hv_Ed_Id:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_VEX_Hv_Ed_Id(code, code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.Hv_Ev:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_VEX_Hv_Ev(code, code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.M:
			reference = new OpCodeHandler_VEX_M(deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.MHV:
			reference = new OpCodeHandler_VEX_MHV(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.M_VK:
			reference = new OpCodeHandler_VEX_M_VK(deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.MV:
			reference = new OpCodeHandler_VEX_MV(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.rDI_VX_RX:
			reference = new OpCodeHandler_VEX_rDI_VX_RX(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.RdRq:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_VEX_RdRq(code, code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.Simple:
			reference = new OpCodeHandler_VEX_Simple(deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VHEv:
		{
			Code code;
			reference = new OpCodeHandler_VEX_VHEv(deserializer.ReadRegister(), code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.VHEvIb:
		{
			Code code;
			reference = new OpCodeHandler_VEX_VHEvIb(deserializer.ReadRegister(), code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.VHIs4W:
			reference = new OpCodeHandler_VEX_VHIs4W(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VHIs5W:
			reference = new OpCodeHandler_VEX_VHIs5W(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VHM:
			reference = new OpCodeHandler_VEX_VHM(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VHW_2:
			reference = new OpCodeHandler_VEX_VHW(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VHW_3:
			reference = new OpCodeHandler_VEX_VHW(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VHW_4:
			reference = new OpCodeHandler_VEX_VHW(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VHWIb_2:
			reference = new OpCodeHandler_VEX_VHWIb(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VHWIb_4:
			reference = new OpCodeHandler_VEX_VHWIb(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VHWIs4:
			reference = new OpCodeHandler_VEX_VHWIs4(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VHWIs5:
			reference = new OpCodeHandler_VEX_VHWIs5(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VK_HK_RK:
			reference = new OpCodeHandler_VEX_VK_HK_RK(deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VK_R:
			reference = new OpCodeHandler_VEX_VK_R(deserializer.ReadCode(), deserializer.ReadRegister());
			return 1;
		case VexOpCodeHandlerKind.VK_RK:
			reference = new OpCodeHandler_VEX_VK_RK(deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VK_RK_Ib:
			reference = new OpCodeHandler_VEX_VK_RK_Ib(deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VK_WK:
			reference = new OpCodeHandler_VEX_VK_WK(deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VM:
			reference = new OpCodeHandler_VEX_VM(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VW_2:
			reference = new OpCodeHandler_VEX_VW(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VW_3:
			reference = new OpCodeHandler_VEX_VW(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VWH:
			reference = new OpCodeHandler_VEX_VWH(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VWIb_2:
			reference = new OpCodeHandler_VEX_VWIb(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VWIb_3:
		{
			Code code;
			reference = new OpCodeHandler_VEX_VWIb(deserializer.ReadRegister(), code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.VX_Ev:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_VEX_VX_Ev(code, code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.VX_VSIB_HX:
			reference = new OpCodeHandler_VEX_VX_VSIB_HX(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.WHV:
			reference = new OpCodeHandler_VEX_WHV(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.WV:
			reference = new OpCodeHandler_VEX_WV(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.WVIb:
			reference = new OpCodeHandler_VEX_WVIb(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VT_SIBMEM:
			reference = new OpCodeHandler_VEX_VT_SIBMEM(deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.SIBMEM_VT:
			reference = new OpCodeHandler_VEX_SIBMEM_VT(deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VT:
			reference = new OpCodeHandler_VEX_VT(deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VT_RT_HT:
			reference = new OpCodeHandler_VEX_VT_RT_HT(deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.Options_DontReadModRM:
			reference = new OpCodeHandler_Options_DontReadModRM(deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadDecoderOptions());
			return 1;
		case VexOpCodeHandlerKind.Gq_HK_RK:
			reference = new OpCodeHandler_VEX_Gq_HK_RK(deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.VK_R_Ib:
			reference = new OpCodeHandler_VEX_VK_R_Ib(deserializer.ReadCode(), deserializer.ReadRegister());
			return 1;
		case VexOpCodeHandlerKind.Gv_Ev:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_VEX_Gv_Ev(code, code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.Ev:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_VEX_Ev(code, code + 1);
			return 1;
		}
		case VexOpCodeHandlerKind.K_Jb:
			reference = new OpCodeHandler_VEX_K_Jb(deserializer.ReadCode());
			return 1;
		case VexOpCodeHandlerKind.K_Jz:
			reference = new OpCodeHandler_VEX_K_Jz(deserializer.ReadCode());
			return 1;
		default:
			throw new InvalidOperationException();
		}
	}
}
