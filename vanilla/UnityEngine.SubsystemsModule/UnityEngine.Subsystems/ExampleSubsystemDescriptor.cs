using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Subsystems;

[UsedByNativeCode]
[NativeType(Header = "Modules/Subsystems/Example/ExampleSubsystemDescriptor.h")]
public class ExampleSubsystemDescriptor : IntegratedSubsystemDescriptor<ExampleSubsystem>
{
	public extern bool supportsEditorMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern bool disableBackbufferMSAA
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern bool stereoscopicBackbuffer
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern bool usePBufferEGL
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}
}
