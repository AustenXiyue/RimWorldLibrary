using System;

namespace Iced.Intel;

[Flags]
internal enum StateFlags : uint
{
	IpRel64 = 1u,
	IpRel32 = 2u,
	HasRex = 8u,
	b = 0x10u,
	z = 0x20u,
	IsInvalid = 0x40u,
	W = 0x80u,
	NoImm = 0x100u,
	Addr64 = 0x200u,
	BranchImm8 = 0x400u,
	Xbegin = 0x800u,
	Lock = 0x1000u,
	AllowLock = 0x2000u,
	NoMoreBytes = 0x4000u,
	Has66 = 0x8000u,
	MvexSssMask = 7u,
	MvexSssShift = 0x10u,
	MvexEH = 0x80000u,
	EncodingMask = 7u,
	EncodingShift = 0x1Du
}
