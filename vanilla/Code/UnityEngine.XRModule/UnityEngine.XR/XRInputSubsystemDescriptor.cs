using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.XR;

[NativeType(Header = "Modules/XR/Subsystems/Input/XRInputSubsystemDescriptor.h")]
[NativeHeader("Modules/XR/XRPrefix.h")]
[UsedByNativeCode]
[NativeConditional("ENABLE_XR")]
public class XRInputSubsystemDescriptor : IntegratedSubsystemDescriptor<XRInputSubsystem>
{
	[NativeConditional("ENABLE_XR")]
	public extern bool disablesLegacyInput
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}
}
