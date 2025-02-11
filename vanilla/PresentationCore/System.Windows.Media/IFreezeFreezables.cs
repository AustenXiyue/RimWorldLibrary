namespace System.Windows.Media;

internal interface IFreezeFreezables
{
	bool FreezeFreezables { get; }

	bool TryFreeze(string value, Freezable freezable);

	Freezable TryGetFreezable(string value);
}
