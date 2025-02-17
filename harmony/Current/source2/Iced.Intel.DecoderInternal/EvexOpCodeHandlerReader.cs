using System;

namespace Iced.Intel.DecoderInternal;

internal sealed class EvexOpCodeHandlerReader : OpCodeHandlerReader
{
	public override int ReadHandlers(ref TableDeserializer deserializer, OpCodeHandler?[] result, int resultIndex)
	{
		ref OpCodeHandler reference = ref result[resultIndex];
		switch (deserializer.ReadEvexOpCodeHandlerKind())
		{
		case EvexOpCodeHandlerKind.Invalid:
			reference = OpCodeHandler_Invalid.Instance;
			return 1;
		case EvexOpCodeHandlerKind.Invalid2:
			result[resultIndex] = OpCodeHandler_Invalid.Instance;
			result[resultIndex + 1] = OpCodeHandler_Invalid.Instance;
			return 2;
		case EvexOpCodeHandlerKind.Dup:
		{
			int num = deserializer.ReadInt32();
			OpCodeHandler opCodeHandler = deserializer.ReadHandler();
			for (int i = 0; i < num; i++)
			{
				result[resultIndex + i] = opCodeHandler;
			}
			return num;
		}
		case EvexOpCodeHandlerKind.HandlerReference:
			reference = deserializer.ReadHandlerReference();
			return 1;
		case EvexOpCodeHandlerKind.ArrayReference:
			throw new InvalidOperationException();
		case EvexOpCodeHandlerKind.RM:
			reference = new OpCodeHandler_RM(deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case EvexOpCodeHandlerKind.Group:
			reference = new OpCodeHandler_Group(deserializer.ReadArrayReference(4u));
			return 1;
		case EvexOpCodeHandlerKind.W:
			reference = new OpCodeHandler_W(deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case EvexOpCodeHandlerKind.MandatoryPrefix2:
			reference = new OpCodeHandler_MandatoryPrefix2(deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case EvexOpCodeHandlerKind.VectorLength:
			reference = new OpCodeHandler_VectorLength_EVEX(deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case EvexOpCodeHandlerKind.VectorLength_er:
			reference = new OpCodeHandler_VectorLength_EVEX_er(deserializer.ReadHandler(), deserializer.ReadHandler(), deserializer.ReadHandler());
			return 1;
		case EvexOpCodeHandlerKind.Ed_V_Ib:
		{
			Code code;
			reference = new OpCodeHandler_EVEX_Ed_V_Ib(deserializer.ReadRegister(), code = deserializer.ReadCode(), code + 1, deserializer.ReadTupleType(), deserializer.ReadTupleType());
			return 1;
		}
		case EvexOpCodeHandlerKind.Ev_VX:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_EVEX_Ev_VX(code, code + 1, deserializer.ReadTupleType(), deserializer.ReadTupleType());
			return 1;
		}
		case EvexOpCodeHandlerKind.Ev_VX_Ib:
		{
			Code code;
			reference = new OpCodeHandler_EVEX_Ev_VX_Ib(deserializer.ReadRegister(), code = deserializer.ReadCode(), code + 1);
			return 1;
		}
		case EvexOpCodeHandlerKind.Gv_W_er:
		{
			Code code;
			reference = new OpCodeHandler_EVEX_Gv_W_er(deserializer.ReadRegister(), code = deserializer.ReadCode(), code + 1, deserializer.ReadTupleType(), deserializer.ReadBoolean());
			return 1;
		}
		case EvexOpCodeHandlerKind.GvM_VX_Ib:
		{
			Code code;
			reference = new OpCodeHandler_EVEX_GvM_VX_Ib(deserializer.ReadRegister(), code = deserializer.ReadCode(), code + 1, deserializer.ReadTupleType(), deserializer.ReadTupleType());
			return 1;
		}
		case EvexOpCodeHandlerKind.HkWIb_3:
			reference = new OpCodeHandler_EVEX_HkWIb(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: false);
			return 1;
		case EvexOpCodeHandlerKind.HkWIb_3b:
			reference = new OpCodeHandler_EVEX_HkWIb(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: true);
			return 1;
		case EvexOpCodeHandlerKind.HWIb:
			reference = new OpCodeHandler_EVEX_HWIb(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.KkHW_3:
			reference = new OpCodeHandler_EVEX_KkHW(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: false);
			return 1;
		case EvexOpCodeHandlerKind.KkHW_3b:
			reference = new OpCodeHandler_EVEX_KkHW(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: true);
			return 1;
		case EvexOpCodeHandlerKind.KkHWIb_sae_3:
			reference = new OpCodeHandler_EVEX_KkHWIb_sae(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: false);
			return 1;
		case EvexOpCodeHandlerKind.KkHWIb_sae_3b:
			reference = new OpCodeHandler_EVEX_KkHWIb_sae(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: true);
			return 1;
		case EvexOpCodeHandlerKind.KkHWIb_3:
			reference = new OpCodeHandler_EVEX_KkHWIb(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: false);
			return 1;
		case EvexOpCodeHandlerKind.KkHWIb_3b:
			reference = new OpCodeHandler_EVEX_KkHWIb(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: true);
			return 1;
		case EvexOpCodeHandlerKind.KkWIb_3:
			reference = new OpCodeHandler_EVEX_KkWIb(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: false);
			return 1;
		case EvexOpCodeHandlerKind.KkWIb_3b:
			reference = new OpCodeHandler_EVEX_KkWIb(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: true);
			return 1;
		case EvexOpCodeHandlerKind.KP1HW:
			reference = new OpCodeHandler_EVEX_KP1HW(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.KR:
			reference = new OpCodeHandler_EVEX_KR(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case EvexOpCodeHandlerKind.MV:
			reference = new OpCodeHandler_EVEX_MV(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.V_H_Ev_er:
		{
			Code code;
			reference = new OpCodeHandler_EVEX_V_H_Ev_er(deserializer.ReadRegister(), code = deserializer.ReadCode(), code + 1, deserializer.ReadTupleType(), deserializer.ReadTupleType());
			return 1;
		}
		case EvexOpCodeHandlerKind.V_H_Ev_Ib:
		{
			Code code;
			reference = new OpCodeHandler_EVEX_V_H_Ev_Ib(deserializer.ReadRegister(), code = deserializer.ReadCode(), code + 1, deserializer.ReadTupleType(), deserializer.ReadTupleType());
			return 1;
		}
		case EvexOpCodeHandlerKind.VHM:
			reference = new OpCodeHandler_EVEX_VHM(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.VHW_3:
			reference = new OpCodeHandler_EVEX_VHW(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.VHW_4:
			reference = new OpCodeHandler_EVEX_VHW(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.VHWIb:
			reference = new OpCodeHandler_EVEX_VHWIb(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.VK:
			reference = new OpCodeHandler_EVEX_VK(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case EvexOpCodeHandlerKind.Vk_VSIB:
			reference = new OpCodeHandler_EVEX_Vk_VSIB(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.VkEv_REXW_2:
			reference = new OpCodeHandler_EVEX_VkEv_REXW(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case EvexOpCodeHandlerKind.VkEv_REXW_3:
			reference = new OpCodeHandler_EVEX_VkEv_REXW(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadCode());
			return 1;
		case EvexOpCodeHandlerKind.VkHM:
			reference = new OpCodeHandler_EVEX_VkHM(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.VkHW_3:
			reference = new OpCodeHandler_EVEX_VkHW(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: false);
			return 1;
		case EvexOpCodeHandlerKind.VkHW_3b:
			reference = new OpCodeHandler_EVEX_VkHW(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: true);
			return 1;
		case EvexOpCodeHandlerKind.VkHW_5:
			reference = new OpCodeHandler_EVEX_VkHW(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: false);
			return 1;
		case EvexOpCodeHandlerKind.VkHW_er_4:
			reference = new OpCodeHandler_EVEX_VkHW_er(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), deserializer.ReadBoolean(), canBroadcast: false);
			return 1;
		case EvexOpCodeHandlerKind.VkHW_er_4b:
			reference = new OpCodeHandler_EVEX_VkHW_er(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), deserializer.ReadBoolean(), canBroadcast: true);
			return 1;
		case EvexOpCodeHandlerKind.VkHW_er_ur_3:
			reference = new OpCodeHandler_EVEX_VkHW_er_ur(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: false);
			return 1;
		case EvexOpCodeHandlerKind.VkHW_er_ur_3b:
			reference = new OpCodeHandler_EVEX_VkHW_er_ur(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: true);
			return 1;
		case EvexOpCodeHandlerKind.VkHWIb_3:
			reference = new OpCodeHandler_EVEX_VkHWIb(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: false);
			return 1;
		case EvexOpCodeHandlerKind.VkHWIb_3b:
			reference = new OpCodeHandler_EVEX_VkHWIb(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: true);
			return 1;
		case EvexOpCodeHandlerKind.VkHWIb_5:
			reference = new OpCodeHandler_EVEX_VkHWIb(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: false);
			return 1;
		case EvexOpCodeHandlerKind.VkHWIb_er_4:
			reference = new OpCodeHandler_EVEX_VkHWIb_er(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: false);
			return 1;
		case EvexOpCodeHandlerKind.VkHWIb_er_4b:
			reference = new OpCodeHandler_EVEX_VkHWIb_er(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: true);
			return 1;
		case EvexOpCodeHandlerKind.VkM:
			reference = new OpCodeHandler_EVEX_VkM(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.VkW_3:
			reference = new OpCodeHandler_EVEX_VkW(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: false);
			return 1;
		case EvexOpCodeHandlerKind.VkW_3b:
			reference = new OpCodeHandler_EVEX_VkW(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: true);
			return 1;
		case EvexOpCodeHandlerKind.VkW_4:
			reference = new OpCodeHandler_EVEX_VkW(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: false);
			return 1;
		case EvexOpCodeHandlerKind.VkW_4b:
			reference = new OpCodeHandler_EVEX_VkW(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: true);
			return 1;
		case EvexOpCodeHandlerKind.VkW_er_4:
			reference = new OpCodeHandler_EVEX_VkW_er(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), deserializer.ReadBoolean());
			return 1;
		case EvexOpCodeHandlerKind.VkW_er_5:
			reference = new OpCodeHandler_EVEX_VkW_er(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), deserializer.ReadBoolean());
			return 1;
		case EvexOpCodeHandlerKind.VkW_er_6:
			reference = new OpCodeHandler_EVEX_VkW_er(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), deserializer.ReadBoolean(), deserializer.ReadBoolean());
			return 1;
		case EvexOpCodeHandlerKind.VkWIb_3:
			reference = new OpCodeHandler_EVEX_VkWIb(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: false);
			return 1;
		case EvexOpCodeHandlerKind.VkWIb_3b:
			reference = new OpCodeHandler_EVEX_VkWIb(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), canBroadcast: true);
			return 1;
		case EvexOpCodeHandlerKind.VkWIb_er:
			reference = new OpCodeHandler_EVEX_VkWIb_er(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.VM:
			reference = new OpCodeHandler_EVEX_VM(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.VSIB_k1:
			reference = new OpCodeHandler_EVEX_VSIB_k1(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.VSIB_k1_VX:
			reference = new OpCodeHandler_EVEX_VSIB_k1_VX(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.VW:
			reference = new OpCodeHandler_EVEX_VW(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.VW_er:
			reference = new OpCodeHandler_EVEX_VW_er(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.VX_Ev:
		{
			Code code = deserializer.ReadCode();
			reference = new OpCodeHandler_EVEX_VX_Ev(code, code + 1, deserializer.ReadTupleType(), deserializer.ReadTupleType());
			return 1;
		}
		case EvexOpCodeHandlerKind.WkHV:
			reference = new OpCodeHandler_EVEX_WkHV(deserializer.ReadRegister(), deserializer.ReadCode());
			return 1;
		case EvexOpCodeHandlerKind.WkV_3:
			reference = new OpCodeHandler_EVEX_WkV(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.WkV_4a:
			reference = new OpCodeHandler_EVEX_WkV(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.WkV_4b:
			reference = new OpCodeHandler_EVEX_WkV(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType(), deserializer.ReadBoolean());
			return 1;
		case EvexOpCodeHandlerKind.WkVIb:
			reference = new OpCodeHandler_EVEX_WkVIb(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.WkVIb_er:
			reference = new OpCodeHandler_EVEX_WkVIb_er(deserializer.ReadRegister(), deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		case EvexOpCodeHandlerKind.WV:
			reference = new OpCodeHandler_EVEX_WV(deserializer.ReadRegister(), deserializer.ReadCode(), deserializer.ReadTupleType());
			return 1;
		default:
			throw new InvalidOperationException();
		}
	}
}
