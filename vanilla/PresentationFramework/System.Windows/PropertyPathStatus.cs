namespace System.Windows;

internal enum PropertyPathStatus : byte
{
	Inactive,
	Active,
	PathError,
	AsyncRequestPending
}
