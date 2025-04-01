namespace System.ComponentModel.Composition;

/// <summary>Notifies a part when its imports have been satisfied.</summary>
public interface IPartImportsSatisfiedNotification
{
	/// <summary>Called when a part's imports have been satisfied and it is safe to use.</summary>
	void OnImportsSatisfied();
}
