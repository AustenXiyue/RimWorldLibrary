using System;

namespace Iced.Intel.DecoderInternal;

[Flags]
internal enum HandlerFlags : uint
{
	None = 0u,
	Xacquire = 1u,
	Xrelease = 2u,
	XacquireXreleaseNoLock = 4u,
	Lock = 8u
}
