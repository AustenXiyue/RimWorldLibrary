using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.ConstrainedExecution;

/// <summary>Ensures that all finalization code in derived classes is marked as critical.</summary>
[ComVisible(true)]
[SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
public abstract class CriticalFinalizerObject
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.ConstrainedExecution.CriticalFinalizerObject" /> class.</summary>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	protected CriticalFinalizerObject()
	{
	}

	/// <summary>Releases all the resources used by the <see cref="T:System.Runtime.ConstrainedExecution.CriticalFinalizerObject" /> class.</summary>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	~CriticalFinalizerObject()
	{
	}
}
