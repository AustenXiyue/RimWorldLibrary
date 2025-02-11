namespace System.Windows.Diagnostics;

public class ResourceDictionaryUnloadedEventArgs : EventArgs
{
	public ResourceDictionaryInfo ResourceDictionaryInfo { get; private set; }

	internal ResourceDictionaryUnloadedEventArgs(ResourceDictionaryInfo resourceDictionaryInfo)
	{
		ResourceDictionaryInfo = resourceDictionaryInfo;
	}
}
