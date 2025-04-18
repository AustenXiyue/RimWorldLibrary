using System;

namespace MonoMod.Core.Platforms;

[Flags]
internal enum RuntimeFeature
{
	None = 0,
	PreciseGC = 1,
	CompileMethodHook = 2,
	ILDetour = 4,
	GenericSharing = 8,
	ListGenericInstantiations = 0x40,
	DisableInlining = 0x10,
	Uninlining = 0x20,
	RequiresMethodPinning = 0x80,
	RequiresMethodIdentification = 0x100,
	RequiresBodyThunkWalking = 0x200,
	HasKnownABI = 0x400,
	RequiresCustomMethodCompile = 0x800
}
