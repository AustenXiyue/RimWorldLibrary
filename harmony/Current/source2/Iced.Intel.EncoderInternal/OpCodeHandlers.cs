using System;

namespace Iced.Intel.EncoderInternal;

internal static class OpCodeHandlers
{
	public static readonly OpCodeHandler[] Handlers;

	static OpCodeHandlers()
	{
		uint[] encFlags = EncoderData.EncFlags1;
		uint[] encFlags2 = EncoderData.EncFlags2;
		uint[] encFlags3 = EncoderData.EncFlags3;
		OpCodeHandler[] array = new OpCodeHandler[4834];
		int i = 0;
		InvalidHandler invalidHandler = new InvalidHandler();
		for (; i < encFlags.Length; i++)
		{
			EncFlags3 encFlags4 = (EncFlags3)encFlags3[i];
			OpCodeHandler opCodeHandler;
			switch ((EncodingKind)(encFlags4 & EncFlags3.EncodingMask))
			{
			case EncodingKind.Legacy:
			{
				Code code = (Code)i;
				opCodeHandler = ((code != 0) ? ((code > Code.DeclareQword) ? ((code != Code.Zero_bytes) ? ((OpCodeHandler)new LegacyHandler((EncFlags1)encFlags[i], (EncFlags2)encFlags2[i], encFlags4)) : ((OpCodeHandler)new ZeroBytesHandler(code))) : new DeclareDataHandler(code)) : invalidHandler);
				break;
			}
			case EncodingKind.VEX:
				opCodeHandler = new VexHandler((EncFlags1)encFlags[i], (EncFlags2)encFlags2[i], encFlags4);
				break;
			case EncodingKind.EVEX:
				opCodeHandler = new EvexHandler((EncFlags1)encFlags[i], (EncFlags2)encFlags2[i], encFlags4);
				break;
			case EncodingKind.XOP:
				opCodeHandler = new XopHandler((EncFlags1)encFlags[i], (EncFlags2)encFlags2[i], encFlags4);
				break;
			case EncodingKind.D3NOW:
				opCodeHandler = new D3nowHandler((EncFlags2)encFlags2[i], encFlags4);
				break;
			case EncodingKind.MVEX:
				opCodeHandler = invalidHandler;
				break;
			default:
				throw new InvalidOperationException();
			}
			array[i] = opCodeHandler;
		}
		if (i != array.Length)
		{
			throw new InvalidOperationException();
		}
		Handlers = array;
	}
}
