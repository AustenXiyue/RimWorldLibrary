namespace System.Runtime.InteropServices.ComTypes;

/// <summary>Provides the managed definition of the IEnumSTATDATA interface.</summary>
[ComImport]
[Guid("00000103-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IEnumSTATDATA
{
	/// <summary>Creates a new enumerator that contains the same enumeration state as the current enumerator.</summary>
	/// <param name="newEnum">When this method returns, contains a reference to the newly created enumerator. This parameter is passed uninitialized.</param>
	void Clone(out IEnumSTATDATA newEnum);

	/// <summary>Retrieves a specified number of items in the enumeration sequence.</summary>
	/// <returns>S_OK if the <paramref name="pceltFetched" /> parameter equals the <paramref name="celt" /> parameter; otherwise, S_FALSE.</returns>
	/// <param name="celt">The number of <see cref="T:System.Runtime.InteropServices.ComTypes.STATDATA" /> references to return in <paramref name="rgelt" />.</param>
	/// <param name="rgelt">When this method returns, contains a reference to the enumerated <see cref="T:System.Runtime.InteropServices.ComTypes.STATDATA" /> references. This parameter is passed uninitialized.</param>
	/// <param name="pceltFetched">When this parameter returns, contains a reference to the actual number of references enumerated in <paramref name="rgelt" />. This parameter is passed uninitialized.</param>
	[PreserveSig]
	int Next(int celt, [Out][MarshalAs(UnmanagedType.LPArray)] STATDATA[] rgelt, [Out][MarshalAs(UnmanagedType.LPArray)] int[] pceltFetched);

	/// <summary>Resets the enumeration sequence to the beginning.</summary>
	/// <returns>An HRESULT with the value S_OK.</returns>
	[PreserveSig]
	int Reset();

	/// <summary>Skips a specified number of items in the enumeration sequence.</summary>
	/// <returns>S_OK if the number of elements skipped equals the <paramref name="celt" /> parameter; otherwise, S_FALSE.</returns>
	/// <param name="celt">The number of elements to skip in the enumeration.</param>
	[PreserveSig]
	int Skip(int celt);
}
