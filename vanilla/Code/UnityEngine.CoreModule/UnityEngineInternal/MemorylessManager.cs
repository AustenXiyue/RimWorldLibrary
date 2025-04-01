using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngineInternal;

[NativeHeader("Runtime/Misc/PlayerSettings.h")]
public class MemorylessManager
{
	public static MemorylessMode depthMemorylessMode
	{
		get
		{
			return GetFramebufferDepthMemorylessMode();
		}
		set
		{
			SetFramebufferDepthMemorylessMode(value);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("GetPlayerSettings()", StaticAccessorType.Dot)]
	[NativeMethod(Name = "GetFramebufferDepthMemorylessMode")]
	internal static extern MemorylessMode GetFramebufferDepthMemorylessMode();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "SetFramebufferDepthMemorylessMode")]
	[StaticAccessor("GetPlayerSettings()", StaticAccessorType.Dot)]
	internal static extern void SetFramebufferDepthMemorylessMode(MemorylessMode mode);
}
