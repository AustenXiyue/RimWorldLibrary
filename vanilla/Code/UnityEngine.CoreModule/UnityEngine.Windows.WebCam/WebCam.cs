using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Windows.WebCam;

[NativeHeader("PlatformDependent/Win/Webcam/WebCam.h")]
[StaticAccessor("WebCam::GetInstance()", StaticAccessorType.Dot)]
[MovedFrom("UnityEngine.XR.WSA.WebCam")]
public class WebCam
{
	public static extern WebCamMode Mode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetWebCamMode")]
		[NativeConditional("(PLATFORM_WIN || PLATFORM_WINRT) && !PLATFORM_XBOXONE")]
		get;
	}
}
