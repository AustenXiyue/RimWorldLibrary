namespace MS.Internal;

internal enum SynchronizedInputStates
{
	NoOpportunity = 1,
	HadOpportunity = 2,
	Handled = 4,
	Discarded = 8
}
