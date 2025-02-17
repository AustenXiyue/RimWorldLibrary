namespace System.Runtime.InteropServices;

/// <summary>Use <see cref="T:System.Runtime.InteropServices.ComTypes.IEnumMoniker" /> instead.</summary>
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("00000102-0000-0000-c000-000000000046")]
[Obsolete]
public interface UCOMIEnumMoniker
{
	/// <summary>Retrieves a specified number of items in the enumeration sequence.</summary>
	/// <returns>S_OK if the <paramref name="pceltFetched" /> parameter equals the <paramref name="celt" /> parameter; otherwise, S_FALSE.</returns>
	/// <param name="celt">The number of monikers to return in <paramref name="rgelt" />. </param>
	/// <param name="rgelt">On successful return, a reference to the enumerated monikers. </param>
	/// <param name="pceltFetched">On successful return, a reference to the actual number of monikers enumerated in <paramref name="rgelt" />. </param>
	[PreserveSig]
	int Next(int celt, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] UCOMIMoniker[] rgelt, out int pceltFetched);

	/// <summary>Skips over a specified number of items in the enumeration sequence.</summary>
	/// <returns>S_OK if the number of elements skipped equals the <paramref name="celt" /> parameter; otherwise, S_FALSE.</returns>
	/// <param name="celt">The number of elements to skip in the enumeration. </param>
	[PreserveSig]
	int Skip(int celt);

	/// <summary>Resets the enumeration sequence to the beginning.</summary>
	/// <returns>An HRESULT with the value S_OK.</returns>
	[PreserveSig]
	int Reset();

	/// <summary>Creates another enumerator that contains the same enumeration state as the current one.</summary>
	/// <param name="ppenum">On successful return, a reference to the newly created enumerator. </param>
	void Clone(out UCOMIEnumMoniker ppenum);
}
