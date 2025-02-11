using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace MS.Internal.Interop;

[ComImport]
[ComVisible(true)]
[Guid("0000000C-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IStream
{
	void Read(nint bufferBase, int sizeInBytes, nint refToNumBytesRead);

	void Write(nint bufferBase, int sizeInBytes, nint refToNumBytesWritten);

	void Seek(long offset, int origin, nint refToNewOffsetNullAllowed);

	void SetSize(long newSize);

	void CopyTo(IStream targetStream, long bytesToCopy, nint refToNumBytesRead, nint refToNumBytesWritten);

	void Commit(int commitFlags);

	void Revert();

	void LockRegion(long offset, long sizeInBytes, int lockType);

	void UnlockRegion(long offset, long sizeInBytes, int lockType);

	void Stat(out STATSTG statStructure, int statFlag);

	void Clone(out IStream newStream);
}
