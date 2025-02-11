namespace MS.Internal;

internal enum DitherType
{
	DitherTypeNone = 0,
	DitherTypeSolid = 0,
	DitherTypeOrdered4x4 = 1,
	DitherTypeOrdered8x8 = 2,
	DitherTypeOrdered16x16 = 3,
	DitherTypeSpiral4x4 = 4,
	DitherTypeSpiral8x8 = 5,
	DitherTypeDualSpiral4x4 = 6,
	DitherTypeDualSpiral8x8 = 7,
	DitherTypeErrorDiffusion = 8
}
