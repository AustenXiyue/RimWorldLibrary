using System.Runtime.InteropServices;

namespace System.Diagnostics;

/// <summary>Modifies code generation for runtime just-in-time (JIT) debugging. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module, AllowMultiple = false)]
public sealed class DebuggableAttribute : Attribute
{
	/// <summary>Specifies the debugging mode for the just-in-time (JIT) compiler.</summary>
	[Flags]
	[ComVisible(true)]
	public enum DebuggingModes
	{
		/// <summary>In the .NET Framework version 2.0, JIT tracking information is always generated, and this flag has the same effect as <see cref="F:System.Diagnostics.DebuggableAttribute.DebuggingModes.Default" /> with the exception of the <see cref="P:System.Diagnostics.DebuggableAttribute.IsJITTrackingEnabled" /> property being false, which has no meaning in version 2.0.</summary>
		None = 0,
		/// <summary>Instructs the just-in-time (JIT) compiler to use its default behavior, which includes enabling optimizations, disabling Edit and Continue support, and using symbol store sequence points if present. In the .NET Framework version 2.0, JIT tracking information, the Microsoft intermediate language (MSIL) offset to the native-code offset within a method, is always generated.</summary>
		Default = 1,
		/// <summary>Disable optimizations performed by the compiler to make your output file smaller, faster, and more efficient. Optimizations result in code rearrangement in the output file, which can make debugging difficult. Typically optimization should be disabled while debugging. </summary>
		DisableOptimizations = 0x100,
		/// <summary>Use the implicit MSIL sequence points, not the program database (PDB) sequence points. The symbolic information normally includes at least one Microsoft intermediate language (MSIL) offset for each source line. When the just-in-time (JIT) compiler is about to compile a method, it asks the profiling services for a list of MSIL offsets that should be preserved. These MSIL offsets are called sequence points.</summary>
		IgnoreSymbolStoreSequencePoints = 2,
		/// <summary>Enable edit and continue. Edit and continue enables you to make changes to your source code while your program is in break mode. The ability to edit and continue is compiler dependent. </summary>
		EnableEditAndContinue = 4
	}

	private DebuggingModes m_debuggingModes;

	/// <summary>Gets a value that indicates whether the runtime will track information during code generation for the debugger.</summary>
	/// <returns>true if the runtime will track information during code generation for the debugger; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsJITTrackingEnabled => (m_debuggingModes & DebuggingModes.Default) != 0;

	/// <summary>Gets a value that indicates whether the runtime optimizer is disabled.</summary>
	/// <returns>true if the runtime optimizer is disabled; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsJITOptimizerDisabled => (m_debuggingModes & DebuggingModes.DisableOptimizations) != 0;

	/// <summary>Gets the debugging modes for the attribute.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.Diagnostics.DebuggableAttribute.DebuggingModes" /> values describing the debugging mode for the just-in-time (JIT) compiler. The default is <see cref="F:System.Diagnostics.DebuggableAttribute.DebuggingModes.Default" />. </returns>
	/// <filterpriority>2</filterpriority>
	public DebuggingModes DebuggingFlags => m_debuggingModes;

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.DebuggableAttribute" /> class, using the specified tracking and optimization options for the just-in-time (JIT) compiler.</summary>
	/// <param name="isJITTrackingEnabled">true to enable debugging; otherwise, false. </param>
	/// <param name="isJITOptimizerDisabled">true to disable the optimizer for execution; otherwise, false. </param>
	public DebuggableAttribute(bool isJITTrackingEnabled, bool isJITOptimizerDisabled)
	{
		m_debuggingModes = DebuggingModes.None;
		if (isJITTrackingEnabled)
		{
			m_debuggingModes |= DebuggingModes.Default;
		}
		if (isJITOptimizerDisabled)
		{
			m_debuggingModes |= DebuggingModes.DisableOptimizations;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.DebuggableAttribute" /> class, using the specified debugging modes for the just-in-time (JIT) compiler. </summary>
	/// <param name="modes">A bitwise combination of the <see cref="T:System.Diagnostics.DebuggableAttribute.DebuggingModes" />  values specifying the debugging mode for the JIT compiler.</param>
	public DebuggableAttribute(DebuggingModes modes)
	{
		m_debuggingModes = modes;
	}
}
