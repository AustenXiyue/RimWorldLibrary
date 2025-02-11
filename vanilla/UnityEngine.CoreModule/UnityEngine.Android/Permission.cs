using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Android;

[NativeHeader("Runtime/Export/Android/AndroidPermissions.bindings.h")]
[UsedByNativeCode]
public struct Permission
{
	public const string Camera = "android.permission.CAMERA";

	public const string Microphone = "android.permission.RECORD_AUDIO";

	public const string FineLocation = "android.permission.ACCESS_FINE_LOCATION";

	public const string CoarseLocation = "android.permission.ACCESS_COARSE_LOCATION";

	public const string ExternalStorageRead = "android.permission.READ_EXTERNAL_STORAGE";

	public const string ExternalStorageWrite = "android.permission.WRITE_EXTERNAL_STORAGE";

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("PermissionsBindings", StaticAccessorType.DoubleColon)]
	public static extern bool HasUserAuthorizedPermission(string permission);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("PermissionsBindings", StaticAccessorType.DoubleColon)]
	public static extern void RequestUserPermission(string permission);
}
