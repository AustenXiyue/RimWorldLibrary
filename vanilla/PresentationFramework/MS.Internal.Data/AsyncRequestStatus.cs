namespace MS.Internal.Data;

internal enum AsyncRequestStatus
{
	Waiting,
	Working,
	Completed,
	Cancelled,
	Failed
}
