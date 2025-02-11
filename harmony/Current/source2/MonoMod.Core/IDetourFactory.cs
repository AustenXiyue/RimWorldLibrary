using System;

namespace MonoMod.Core;

[CLSCompliant(true)]
internal interface IDetourFactory
{
	ICoreDetour CreateDetour(CreateDetourRequest request);

	ICoreNativeDetour CreateNativeDetour(CreateNativeDetourRequest request);
}
