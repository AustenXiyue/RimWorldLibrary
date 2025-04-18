using System;

namespace Iced.Intel.EncoderInternal;

[Flags]
internal enum EncFlags2 : uint
{
	None = 0u,
	OpCodeShift = 0u,
	OpCodeIs2Bytes = 0x10000u,
	TableShift = 0x11u,
	TableMask = 7u,
	MandatoryPrefixShift = 0x14u,
	MandatoryPrefixMask = 3u,
	WBitShift = 0x16u,
	WBitMask = 3u,
	LBitShift = 0x18u,
	LBitMask = 7u,
	GroupIndexShift = 0x1Bu,
	GroupIndexMask = 7u,
	HasMandatoryPrefix = 0x40000000u,
	HasGroupIndex = 0x80000000u
}
