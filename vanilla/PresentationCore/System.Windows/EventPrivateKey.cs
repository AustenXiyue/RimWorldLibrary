namespace System.Windows;

/// <summary>Provides unique identification for events whose handlers are stored into an internal hashtable. </summary>
public class EventPrivateKey
{
	private int _globalIndex;

	internal int GlobalIndex => _globalIndex;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.EventPrivateKey" /> class. </summary>
	public EventPrivateKey()
	{
		_globalIndex = GlobalEventManager.GetNextAvailableGlobalIndex(this);
	}
}
