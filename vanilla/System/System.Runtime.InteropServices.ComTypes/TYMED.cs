namespace System.Runtime.InteropServices.ComTypes;

/// <summary>Provides the managed definition of the TYMED structure.</summary>
[Flags]
public enum TYMED
{
	/// <summary>The storage medium is a global memory handle (HGLOBAL). Allocate the global handle with the GMEM_SHARE flag. If the <see cref="T:System.Runtime.InteropServices.ComTypes.STGMEDIUM" /><see cref="F:System.Runtime.InteropServices.ComTypes.STGMEDIUM.pUnkForRelease" /> member is null, the destination process should use GlobalFree to release the memory.</summary>
	TYMED_HGLOBAL = 1,
	/// <summary>The storage medium is a disk file identified by a path. If the STGMEDIUM<see cref="F:System.Runtime.InteropServices.ComTypes.STGMEDIUM.pUnkForRelease" /> member is null, the destination process should use OpenFile to delete the file.</summary>
	TYMED_FILE = 2,
	/// <summary>The storage medium is a stream object identified by an IStream pointer. Use ISequentialStream::Read to read the data. If the <see cref="T:System.Runtime.InteropServices.ComTypes.STGMEDIUM" /><see cref="F:System.Runtime.InteropServices.ComTypes.STGMEDIUM.pUnkForRelease" /> member is not null, the destination process should use IStream::Release to release the stream component.</summary>
	TYMED_ISTREAM = 4,
	/// <summary>The storage medium is a storage component identified by an IStorage pointer. The data is in the streams and storages contained by this IStorage instance. If the <see cref="T:System.Runtime.InteropServices.ComTypes.STGMEDIUM" /><see cref="F:System.Runtime.InteropServices.ComTypes.STGMEDIUM.pUnkForRelease" /> member is not null, the destination process should use IStorage::Release to release the storage component.</summary>
	TYMED_ISTORAGE = 8,
	/// <summary>The storage medium is a Graphics Device Interface (GDI) component (HBITMAP). If the <see cref="T:System.Runtime.InteropServices.ComTypes.STGMEDIUM" /><see cref="F:System.Runtime.InteropServices.ComTypes.STGMEDIUM.pUnkForRelease" /> member is null, the destination process should use DeleteObject to delete the bitmap.</summary>
	TYMED_GDI = 0x10,
	/// <summary>The storage medium is a metafile (HMETAFILE). Use the Windows or WIN32 functions to access the metafile's data. If the <see cref="T:System.Runtime.InteropServices.ComTypes.STGMEDIUM" /><see cref="F:System.Runtime.InteropServices.ComTypes.STGMEDIUM.pUnkForRelease" /> member is null, the destination process should use DeleteMetaFile to delete the bitmap.</summary>
	TYMED_MFPICT = 0x20,
	/// <summary>The storage medium is an enhanced metafile. If the <see cref="T:System.Runtime.InteropServices.ComTypes.STGMEDIUM" /><see cref="F:System.Runtime.InteropServices.ComTypes.STGMEDIUM.pUnkForRelease" /> member is null, the destination process should use DeleteEnhMetaFile to delete the bitmap.</summary>
	TYMED_ENHMF = 0x40,
	/// <summary>No data is being passed.</summary>
	TYMED_NULL = 0
}
