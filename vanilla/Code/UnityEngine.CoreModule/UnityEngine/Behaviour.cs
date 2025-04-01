using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[UsedByNativeCode]
[NativeHeader("Runtime/Mono/MonoBehaviour.h")]
public class Behaviour : Component
{
	[NativeProperty]
	[RequiredByNativeCode]
	public extern bool enabled
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty]
	public extern bool isActiveAndEnabled
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("IsAddedToManager")]
		get;
	}
}
