using System;

namespace Iced.Intel.EncoderInternal;

[Flags]
internal enum EncoderFlags : uint
{
	None = 0u,
	B = 1u,
	X = 2u,
	R = 4u,
	W = 8u,
	ModRM = 0x10u,
	Sib = 0x20u,
	REX = 0x40u,
	P66 = 0x80u,
	P67 = 0x100u,
	R2 = 0x200u,
	Broadcast = 0x400u,
	HighLegacy8BitRegs = 0x800u,
	Displ = 0x1000u,
	PF0 = 0x2000u,
	RegIsMemory = 0x4000u,
	MustUseSib = 0x8000u,
	VvvvvShift = 0x1Bu,
	VvvvvMask = 0x1Fu
}
