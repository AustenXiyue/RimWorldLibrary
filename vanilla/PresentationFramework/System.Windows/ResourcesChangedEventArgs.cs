namespace System.Windows;

internal class ResourcesChangedEventArgs : EventArgs
{
	private ResourcesChangeInfo _info;

	internal ResourcesChangeInfo Info => _info;

	internal ResourcesChangedEventArgs(ResourcesChangeInfo info)
	{
		_info = info;
	}
}
