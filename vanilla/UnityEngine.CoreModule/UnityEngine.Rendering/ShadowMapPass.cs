using System;

namespace UnityEngine.Rendering;

[Flags]
public enum ShadowMapPass
{
	PointlightPositiveX = 1,
	PointlightNegativeX = 2,
	PointlightPositiveY = 4,
	PointlightNegativeY = 8,
	PointlightPositiveZ = 0x10,
	PointlightNegativeZ = 0x20,
	DirectionalCascade0 = 0x40,
	DirectionalCascade1 = 0x80,
	DirectionalCascade2 = 0x100,
	DirectionalCascade3 = 0x200,
	Spotlight = 0x400,
	Pointlight = 0x3F,
	Directional = 0x3C0,
	All = 0x7FF
}
