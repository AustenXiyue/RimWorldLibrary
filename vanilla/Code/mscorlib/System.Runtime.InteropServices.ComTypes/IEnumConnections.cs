namespace System.Runtime.InteropServices.ComTypes;

/// <summary>Manages the definition of the IEnumConnections interface.</summary>
[ComImport]
[Guid("B196B287-BAB4-101A-B69C-00AA00341D07")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IEnumConnections
{
	/// <summary>Retrieves a specified number of items in the enumeration sequence.</summary>
	/// <returns>S_OK if the <paramref name="pceltFetched" /> parameter equals the <paramref name="celt" /> parameter; otherwise, S_FALSE.</returns>
	/// <param name="celt">The number of <see cref="T:System.Runtime.InteropServices.CONNECTDATA" /> structures to return in <paramref name="rgelt" />. </param>
	/// <param name="rgelt">When this method returns, contains a reference to the enumerated connections. This parameter is passed uninitialized.</param>
	/// <param name="pceltFetched">When this method returns, contains a reference to the actual number of connections enumerated in <paramref name="rgelt" />. </param>
	[PreserveSig]
	int Next(int celt, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] CONNECTDATA[] rgelt, IntPtr pceltFetched);

	/// <summary>Skips a specified number of items in the enumeration sequence.</summary>
	/// <returns>S_OK if the number of elements skipped equals the <paramref name="celt" /> parameter; otherwise, S_FALSE.</returns>
	/// <param name="celt">The number of elements to skip in the enumeration. </param>
	[PreserveSig]
	int Skip(int celt);

	/// <summary>Resets the enumeration sequence to the beginning.</summary>
	void Reset();

	/// <summary>Creates a new enumerator that contains the same enumeration state as the current one.</summary>
	/// <param name="ppenum">When this method returns, contains a reference to the newly created enumerator. This parameter is passed uninitialized.</param>
	void Clone(out IEnumConnections ppenum);
}
