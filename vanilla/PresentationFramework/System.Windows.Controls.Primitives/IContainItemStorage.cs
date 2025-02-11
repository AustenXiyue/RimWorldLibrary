namespace System.Windows.Controls.Primitives;

/// <summary>Provides a property bag implementation for item panels.</summary>
public interface IContainItemStorage
{
	/// <summary>Stores the specified property and value and associates them with the specified item.</summary>
	/// <param name="item">The item to associate the value and property with.</param>
	/// <param name="dp">The property that is associated with the specified item.</param>
	/// <param name="value">The value of the associated property.</param>
	void StoreItemValue(object item, DependencyProperty dp, object value);

	/// <summary>Returns the value of the specified property that is associated with the specified item.</summary>
	/// <returns>The value of the specified property that is associated with the specified item.</returns>
	/// <param name="item">The item that has the specified property associated with it.</param>
	/// <param name="dp">The property whose value to return.</param>
	object ReadItemValue(object item, DependencyProperty dp);

	/// <summary>Removes the association between the specified item and property. </summary>
	/// <param name="item">The associated item.</param>
	/// <param name="dp">The associated property.</param>
	void ClearItemValue(object item, DependencyProperty dp);

	/// <summary>Removes the specified property from all property bags.</summary>
	/// <param name="dp">The property to remove.</param>
	void ClearValue(DependencyProperty dp);

	/// <summary>Clears all property associations.</summary>
	void Clear();
}
