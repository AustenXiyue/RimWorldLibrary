namespace System.Runtime.InteropServices.ComTypes;

/// <summary>Provides the managed definition of the IConnectionPoint interface.</summary>
[ComImport]
[Guid("B196B286-BAB4-101A-B69C-00AA00341D07")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IConnectionPoint
{
	/// <summary>Returns the IID of the outgoing interface managed by this connection point.</summary>
	/// <param name="pIID">When this parameter returns, contains the IID of the outgoing interface managed by this connection point. This parameter is passed uninitialized.</param>
	void GetConnectionInterface(out Guid pIID);

	/// <summary>Retrieves the IConnectionPointContainer interface pointer to the connectable object that conceptually owns this connection point.</summary>
	/// <param name="ppCPC">When this parameter returns, contains the connectable object's IConnectionPointContainer interface. This parameter is passed uninitialized.</param>
	void GetConnectionPointContainer(out IConnectionPointContainer ppCPC);

	/// <summary>Establishes an advisory connection between the connection point and the caller's sink object.</summary>
	/// <param name="pUnkSink">A reference to the sink to receive calls for the outgoing interface managed by this connection point. </param>
	/// <param name="pdwCookie">When this method returns, contains the connection cookie. This parameter is passed uninitialized.</param>
	void Advise([MarshalAs(UnmanagedType.Interface)] object pUnkSink, out int pdwCookie);

	/// <summary>Terminates an advisory connection previously established through the <see cref="M:System.Runtime.InteropServices.ComTypes.IConnectionPoint.Advise(System.Object,System.Int32@)" /> method.</summary>
	/// <param name="dwCookie">The connection cookie previously returned from the <see cref="M:System.Runtime.InteropServices.ComTypes.IConnectionPoint.Advise(System.Object,System.Int32@)" /> method. </param>
	void Unadvise(int dwCookie);

	/// <summary>Creates an enumerator object for iteration through the connections that exist to this connection point.</summary>
	/// <param name="ppEnum">When this method returns, contains the newly created enumerator. This parameter is passed uninitialized.</param>
	void EnumConnections(out IEnumConnections ppEnum);
}
