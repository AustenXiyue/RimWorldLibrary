using MS.Internal.PtsHost.UnsafeNativeMethods;

namespace MS.Internal.PtsHost;

internal sealed class UpdateRecord
{
	internal DirtyTextRange Dtr;

	internal BaseParagraph FirstPara;

	internal BaseParagraph SyncPara;

	internal PTS.FSKCHANGE ChangeType;

	internal UpdateRecord Next;

	internal bool InProcessing;

	internal UpdateRecord()
	{
		Dtr = new DirtyTextRange(0, 0, 0);
		FirstPara = (SyncPara = null);
		ChangeType = PTS.FSKCHANGE.fskchNone;
		Next = null;
		InProcessing = false;
	}

	internal void MergeWithNext()
	{
		int num = Next.Dtr.StartIndex - Dtr.StartIndex;
		Dtr.PositionsAdded += num + Next.Dtr.PositionsAdded;
		Dtr.PositionsRemoved += num + Next.Dtr.PositionsRemoved;
		SyncPara = Next.SyncPara;
		Next = Next.Next;
	}
}
