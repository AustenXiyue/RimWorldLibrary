namespace System.Windows.Data;

internal enum BindingStatusInternal : byte
{
	Unattached,
	Inactive,
	Active,
	Detached,
	AsyncRequestPending,
	PathError,
	UpdateTargetError,
	UpdateSourceError
}
