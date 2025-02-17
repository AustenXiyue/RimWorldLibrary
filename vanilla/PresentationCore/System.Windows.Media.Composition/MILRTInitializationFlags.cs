namespace System.Windows.Media.Composition;

[Flags]
internal enum MILRTInitializationFlags
{
	MIL_RT_INITIALIZE_DEFAULT = 0,
	MIL_RT_SOFTWARE_ONLY = 1,
	MIL_RT_HARDWARE_ONLY = 2,
	MIL_RT_NULL = 3,
	MIL_RT_TYPE_MASK = 3,
	MIL_RT_PRESENT_IMMEDIATELY = 4,
	MIL_RT_PRESENT_RETAIN_CONTENTS = 8,
	MIL_RT_FULLSCREEN = 0x10,
	MIL_RT_LINEAR_GAMMA = 0x20,
	MIL_RT_NEED_DESTINATION_ALPHA = 0x40,
	MIL_RT_ALLOW_LOW_PRECISION = 0x80,
	MIL_RT_SINGLE_THREADED_USAGE = 0x100,
	MIL_RT_RENDER_NONCLIENT = 0x200,
	MIL_RT_PRESENT_FLIP = 0x400,
	MIL_RT_FULLSCREEN_NO_AUTOROTATE = 0x800,
	MIL_RT_DISABLE_DISPLAY_CLIPPING = 0x1000,
	MIL_RT_DISABLE_MULTIMON_DISPLAY_CLIPPING = 0x4000,
	MIL_RT_IS_DISABLE_MULTIMON_DISPLAY_CLIPPING_VALID = 0x8000,
	MIL_RT_DISABLE_DIRTY_RECTANGLES = 0x10000,
	MIL_UCE_RT_ENABLE_OCCLUSION = 0x10000,
	MIL_RT_USE_REF_RAST = 0x1000000,
	MIL_RT_USE_RGB_RAST = 0x2000000,
	MIL_RT_FULLSCREEN_TRANSPOSE_XY = 0x10000000,
	MIL_RT_PRESENT_USING_MASK = -1073741824,
	MIL_RT_PRESENT_USING_HAL = 0,
	MIL_RT_PRESENT_USING_BITBLT = 0x40000000,
	MIL_RT_PRESENT_USING_ALPHABLEND = int.MinValue,
	MIL_RT_PRESENT_USING_ULW = -1073741824,
	FORCE_DWORD = -1
}
