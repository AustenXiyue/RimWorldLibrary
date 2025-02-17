namespace RimWorld;

public interface IStoreSettingsParent
{
	bool StorageTabVisible { get; }

	StorageSettings GetStoreSettings();

	StorageSettings GetParentStoreSettings();

	void Notify_SettingsChanged();
}
