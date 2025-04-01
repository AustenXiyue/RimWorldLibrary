namespace System.Runtime.InteropServices;

/// <summary>Wraps objects the marshaler should marshal as a VT_UNKNOWN.</summary>
[Serializable]
[ComVisible(true)]
public sealed class UnknownWrapper
{
	private object m_WrappedObject;

	/// <summary>Gets the object contained by this wrapper.</summary>
	/// <returns>The wrapped object.</returns>
	public object WrappedObject => m_WrappedObject;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.UnknownWrapper" /> class with the object to be wrapped.</summary>
	/// <param name="obj">The object being wrapped. </param>
	public UnknownWrapper(object obj)
	{
		m_WrappedObject = obj;
	}
}
