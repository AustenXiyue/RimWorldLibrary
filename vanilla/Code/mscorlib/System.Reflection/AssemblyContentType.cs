using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Provides information about the type of code contained in an assembly.</summary>
[Serializable]
[ComVisible(false)]
public enum AssemblyContentType
{
	/// <summary>The assembly contains .NET Framework code.</summary>
	Default,
	/// <summary>The assembly contains Windows Runtime code.</summary>
	WindowsRuntime
}
