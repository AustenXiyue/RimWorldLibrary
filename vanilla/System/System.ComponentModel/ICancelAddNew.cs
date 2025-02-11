namespace System.ComponentModel;

/// <summary>Adds transactional capability when adding a new item to a collection.</summary>
public interface ICancelAddNew
{
	/// <summary>Discards a pending new item from the collection.</summary>
	/// <param name="itemIndex">The index of the item that was previously added to the collection. </param>
	void CancelNew(int itemIndex);

	/// <summary>Commits a pending new item to the collection.</summary>
	/// <param name="itemIndex">The index of the item that was previously added to the collection. </param>
	void EndNew(int itemIndex);
}
