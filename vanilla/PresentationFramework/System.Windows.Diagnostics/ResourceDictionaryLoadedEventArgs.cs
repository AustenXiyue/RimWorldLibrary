namespace System.Windows.Diagnostics;

public class ResourceDictionaryLoadedEventArgs : EventArgs
{
	public ResourceDictionaryInfo ResourceDictionaryInfo { get; private set; }

	internal ResourceDictionaryLoadedEventArgs(ResourceDictionaryInfo resourceDictionaryInfo)
	{
		ResourceDictionaryInfo = resourceDictionaryInfo;
	}
}
