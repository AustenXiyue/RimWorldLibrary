using System;

namespace MonoMod.Core.Platforms;

[Flags]
internal enum ArchitectureFeature
{
	None = 0,
	FixedInstructionSize = 1,
	Immediate64 = 2,
	CreateAltEntryPoint = 4
}
