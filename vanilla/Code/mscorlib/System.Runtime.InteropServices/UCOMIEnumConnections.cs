namespace System.Runtime.InteropServices;

/// <summary>Use <see cref="T:System.Runtime.InteropServices.ComTypes.IEnumConnections" /> instead. </summary>
[ComImport]
[Obsolete("Use System.Runtime.InteropServices.ComTypes.IEnumConnections instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
[Guid("B196B287-BAB4-101A-B69C-00AA00341D07")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface UCOMIEnumConnections
{
	/// <summary>Retrieves a specified number of items in the enumeration sequence.</summary>
	/// <returns>S_OK if the <paramref name="pceltFetched" /> parameter equals the <paramref name="celt" /> parameter; otherwise, S_FALSE.</returns>
	/// <param name="celt">The number of <see cref="T:System.Runtime.InteropServices.CONNECTDATA" /> structures to return in <paramref name="rgelt" />. </param>
	/// <param name="rgelt">On successful return, a reference to the enumerated connections. </param>
	/// <param name="pceltFetched">On successful return, a reference to the actual number of connections enumerated in <paramref name="rgelt" />. </param>
	[PreserveSig]
	int Next(int celt, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] CONNECTDATA[] rgelt, out int pceltFetched);

	/// <summary>Skips over a specified number of items in the enumeration sequence.</summary>
	/// <returns>S_OK if the number of elements skipped equals the <paramref name="celt" /> parameter; otherwise, S_FALSE.</returns>
	/// <param name="celt">The number of elements to skip in the enumeration. </param>
	[PreserveSig]
	int Skip(int celt);

	/// <summary>Resets the enumeration sequence to the beginning.</summary>
	[PreserveSig]
	void Reset();

	/// <summary>Creates another enumerator that contains the same enumeration state as the current one.</summary>
	/// <param name="ppenum">On successful return, a reference to the newly created enumerator. </param>
	void Clone(out UCOMIEnumConnections ppenum);
}
