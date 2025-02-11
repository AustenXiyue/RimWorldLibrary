namespace MS.Internal.AppModel;

internal interface IJournalState
{
	CustomJournalStateInternal GetJournalState(JournalReason journalReason);

	void RestoreJournalState(CustomJournalStateInternal state);
}
