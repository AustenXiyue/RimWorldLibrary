using System;

namespace Iced.Intel.EncoderInternal;

[Flags]
internal enum EncFlags1 : uint
{
	None = 0u,
	Legacy_OpMask = 0x7Fu,
	Legacy_Op0Shift = 0u,
	Legacy_Op1Shift = 7u,
	Legacy_Op2Shift = 0xEu,
	Legacy_Op3Shift = 0x15u,
	VEX_OpMask = 0x3Fu,
	VEX_Op0Shift = 0u,
	VEX_Op1Shift = 6u,
	VEX_Op2Shift = 0xCu,
	VEX_Op3Shift = 0x12u,
	VEX_Op4Shift = 0x18u,
	XOP_OpMask = 0x1Fu,
	XOP_Op0Shift = 0u,
	XOP_Op1Shift = 5u,
	XOP_Op2Shift = 0xAu,
	XOP_Op3Shift = 0xFu,
	EVEX_OpMask = 0x1Fu,
	EVEX_Op0Shift = 0u,
	EVEX_Op1Shift = 5u,
	EVEX_Op2Shift = 0xAu,
	EVEX_Op3Shift = 0xFu,
	MVEX_OpMask = 0xFu,
	MVEX_Op0Shift = 0u,
	MVEX_Op1Shift = 4u,
	MVEX_Op2Shift = 8u,
	MVEX_Op3Shift = 0xCu,
	IgnoresRoundingControl = 0x40000000u,
	AmdLockRegBit = 0x80000000u
}
