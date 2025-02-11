using System.Runtime.InteropServices.ComTypes;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal interface IStream
{
	void Read(byte[] pv, int cb, out int pcbRead);

	void Write(byte[] pv, int cb, out int pcbWritten);

	void Seek(long dlibMove, int dwOrigin, out long plibNewPosition);

	void SetSize(long libNewSize);

	void CopyTo(IStream pstm, long cb, out long pcbRead, out long pcbWritten);

	void Commit(int grfCommitFlags);

	void Revert();

	void LockRegion(long libOffset, long cb, int dwLockType);

	void UnlockRegion(long libOffset, long cb, int dwLockType);

	void Stat(out STATSTG pstatstg, int grfStatFlag);

	void Clone(out IStream ppstm);
}
