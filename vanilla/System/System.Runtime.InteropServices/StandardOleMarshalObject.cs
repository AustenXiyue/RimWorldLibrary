namespace System.Runtime.InteropServices;

/// <summary>Replaces the standard common language runtime (CLR) free-threaded marshaler with the standard OLE STA marshaler.</summary>
[System.MonoLimitation("The runtime does nothing special apart from what it already does with marshal-by-ref objects")]
[ComVisible(true)]
public class StandardOleMarshalObject : MarshalByRefObject
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.StandardOleMarshalObject" /> class. </summary>
	protected StandardOleMarshalObject()
	{
	}
}
