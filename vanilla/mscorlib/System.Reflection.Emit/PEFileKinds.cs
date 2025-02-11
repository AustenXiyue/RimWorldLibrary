using System.Runtime.InteropServices;

namespace System.Reflection.Emit;

/// <summary>Specifies the type of the portable executable (PE) file.</summary>
[Serializable]
[ComVisible(true)]
public enum PEFileKinds
{
	/// <summary>The portable executable (PE) file is a DLL.</summary>
	Dll = 1,
	/// <summary>The application is a console (not a Windows-based) application.</summary>
	ConsoleApplication,
	/// <summary>The application is a Windows-based application.</summary>
	WindowApplication
}
