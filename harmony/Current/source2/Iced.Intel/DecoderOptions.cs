using System;

namespace Iced.Intel;

[Flags]
internal enum DecoderOptions : uint
{
	None = 0u,
	NoInvalidCheck = 1u,
	AMD = 2u,
	ForceReservedNop = 4u,
	Umov = 8u,
	Xbts = 0x10u,
	Cmpxchg486A = 0x20u,
	OldFpu = 0x40u,
	Pcommit = 0x80u,
	Loadall286 = 0x100u,
	Loadall386 = 0x200u,
	Cl1invmb = 0x400u,
	MovTr = 0x800u,
	Jmpe = 0x1000u,
	NoPause = 0x2000u,
	NoWbnoinvd = 0x4000u,
	Udbg = 0x8000u,
	NoMPFX_0FBC = 0x10000u,
	NoMPFX_0FBD = 0x20000u,
	NoLahfSahf64 = 0x40000u,
	MPX = 0x80000u,
	Cyrix = 0x100000u,
	Cyrix_SMINT_0F7E = 0x200000u,
	Cyrix_DMI = 0x400000u,
	ALTINST = 0x800000u,
	KNC = 0x1000000u
}
