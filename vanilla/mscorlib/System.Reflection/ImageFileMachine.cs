using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Identifies the platform targeted by an executable.</summary>
[Serializable]
[ComVisible(true)]
public enum ImageFileMachine
{
	/// <summary>Targets a 32-bit Intel processor.</summary>
	I386 = 332,
	/// <summary>Targets a 64-bit Intel processor.</summary>
	IA64 = 512,
	/// <summary>Targets a 64-bit AMD processor.</summary>
	AMD64 = 34404,
	/// <summary>Targets an ARM processor.</summary>
	ARM = 452
}
