using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.XR.Tango;

[NativeConditional("PLATFORM_ANDROID")]
[NativeHeader("Modules/AR/Tango/TangoScriptApi.h")]
internal static class TangoInputTracking
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool Internal_TryGetPoseAtTime(out PoseData pose);

	internal static bool TryGetPoseAtTime(out PoseData pose)
	{
		return Internal_TryGetPoseAtTime(out pose);
	}
}
