using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace System.Windows.Media;

internal struct StreamDescriptor
{
	internal delegate void Dispose(ref StreamDescriptor pSD);

	internal delegate int Read(ref StreamDescriptor pSD, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] buffer, uint cb, out uint cbRead);

	internal unsafe delegate int Seek(ref StreamDescriptor pSD, long offset, uint origin, long* plibNewPostion);

	internal delegate int Stat(ref StreamDescriptor pSD, out STATSTG statstg, uint grfStatFlag);

	internal delegate int Write(ref StreamDescriptor pSD, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] buffer, uint cb, out uint cbWritten);

	internal delegate int CopyTo(ref StreamDescriptor pSD, nint pstm, long cb, out long cbRead, out long cbWritten);

	internal delegate int SetSize(ref StreamDescriptor pSD, long value);

	internal delegate int Revert(ref StreamDescriptor pSD);

	internal delegate int Commit(ref StreamDescriptor pSD, uint grfCommitFlags);

	internal delegate int LockRegion(ref StreamDescriptor pSD, long libOffset, long cb, uint dwLockType);

	internal delegate int UnlockRegion(ref StreamDescriptor pSD, long libOffset, long cb, uint dwLockType);

	internal delegate int Clone(ref StreamDescriptor pSD, out nint stream);

	internal delegate int CanWrite(ref StreamDescriptor pSD, out bool canWrite);

	internal delegate int CanSeek(ref StreamDescriptor pSD, out bool canSeek);

	internal Dispose pfnDispose;

	internal Read pfnRead;

	internal Seek pfnSeek;

	internal Stat pfnStat;

	internal Write pfnWrite;

	internal CopyTo pfnCopyTo;

	internal SetSize pfnSetSize;

	internal Commit pfnCommit;

	internal Revert pfnRevert;

	internal LockRegion pfnLockRegion;

	internal UnlockRegion pfnUnlockRegion;

	internal Clone pfnClone;

	internal CanWrite pfnCanWrite;

	internal CanSeek pfnCanSeek;

	internal GCHandle m_handle;

	internal static void StaticDispose(ref StreamDescriptor pSD)
	{
		_ = (StreamAsIStream)pSD.m_handle.Target;
		GCHandle handle = pSD.m_handle;
		handle.Free();
	}
}
