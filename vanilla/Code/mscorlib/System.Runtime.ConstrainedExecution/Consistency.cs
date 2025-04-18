namespace System.Runtime.ConstrainedExecution;

/// <summary>Specifies a reliability contract.</summary>
[Serializable]
public enum Consistency
{
	/// <summary>In the face of exceptional conditions, the CLR makes no guarantees regarding state consistency; that is, the condition might corrupt the process.</summary>
	MayCorruptProcess,
	/// <summary>In the face of exceptional conditions, the common language runtime (CLR) makes no guarantees regarding state consistency in the current application domain.</summary>
	MayCorruptAppDomain,
	/// <summary>In the face of exceptional conditions, the method is guaranteed to limit state corruption to the current instance.</summary>
	MayCorruptInstance,
	/// <summary>In the face of exceptional conditions, the method is guaranteed not to corrupt state. </summary>
	WillNotCorruptState
}
