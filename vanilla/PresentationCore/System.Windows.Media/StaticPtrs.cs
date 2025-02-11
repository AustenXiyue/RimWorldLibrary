namespace System.Windows.Media;

internal static class StaticPtrs
{
	internal static StreamDescriptor.Dispose pfnDispose;

	internal static StreamDescriptor.Read pfnRead;

	internal static StreamDescriptor.Seek pfnSeek;

	internal static StreamDescriptor.Stat pfnStat;

	internal static StreamDescriptor.Write pfnWrite;

	internal static StreamDescriptor.CopyTo pfnCopyTo;

	internal static StreamDescriptor.SetSize pfnSetSize;

	internal static StreamDescriptor.Commit pfnCommit;

	internal static StreamDescriptor.Revert pfnRevert;

	internal static StreamDescriptor.LockRegion pfnLockRegion;

	internal static StreamDescriptor.UnlockRegion pfnUnlockRegion;

	internal static StreamDescriptor.Clone pfnClone;

	internal static StreamDescriptor.CanWrite pfnCanWrite;

	internal static StreamDescriptor.CanSeek pfnCanSeek;

	unsafe static StaticPtrs()
	{
		pfnDispose = StreamDescriptor.StaticDispose;
		pfnClone = StreamAsIStream.Clone;
		pfnCommit = StreamAsIStream.Commit;
		pfnCopyTo = StreamAsIStream.CopyTo;
		pfnLockRegion = StreamAsIStream.LockRegion;
		pfnRead = StreamAsIStream.Read;
		pfnRevert = StreamAsIStream.Revert;
		pfnSeek = StreamAsIStream.Seek;
		pfnSetSize = StreamAsIStream.SetSize;
		pfnStat = StreamAsIStream.Stat;
		pfnUnlockRegion = StreamAsIStream.UnlockRegion;
		pfnWrite = StreamAsIStream.Write;
		pfnCanWrite = StreamAsIStream.CanWrite;
		pfnCanSeek = StreamAsIStream.CanSeek;
	}
}
