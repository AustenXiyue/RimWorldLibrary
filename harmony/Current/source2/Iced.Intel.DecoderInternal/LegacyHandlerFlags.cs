using System;

namespace Iced.Intel.DecoderInternal;

[Flags]
internal enum LegacyHandlerFlags : uint
{
	HandlerReg = 1u,
	HandlerMem = 2u,
	Handler66Reg = 4u,
	Handler66Mem = 8u,
	HandlerF3Reg = 0x10u,
	HandlerF3Mem = 0x20u,
	HandlerF2Reg = 0x40u,
	HandlerF2Mem = 0x80u
}
