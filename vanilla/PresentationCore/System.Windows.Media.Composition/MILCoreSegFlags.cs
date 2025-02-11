namespace System.Windows.Media.Composition;

[Flags]
internal enum MILCoreSegFlags
{
	SegTypeLine = 1,
	SegTypeBezier = 2,
	SegTypeMask = 3,
	SegIsAGap = 4,
	SegSmoothJoin = 8,
	SegClosed = 0x10,
	SegIsCurved = 0x20,
	FORCE_DWORD = -1
}
