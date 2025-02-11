using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.XR;

[NativeHeader("Modules/XR/XRPrefix.h")]
[UsedByNativeCode]
[NativeType(Header = "Modules/XR/Subsystems/Planes/XRMeshSubsystemDescriptor.h")]
public class XRMeshSubsystemDescriptor : IntegratedSubsystemDescriptor<XRMeshSubsystem>
{
}
