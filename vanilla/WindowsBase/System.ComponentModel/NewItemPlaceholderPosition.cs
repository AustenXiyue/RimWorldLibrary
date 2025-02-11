namespace System.ComponentModel;

/// <summary>Specifies where the placeholder for a new item appears in the collection.</summary>
public enum NewItemPlaceholderPosition
{
	/// <summary>The collection does not use a new item placeholder. The position of items that are added depends on the underlying collection.  Usually, they are added at the end of the collection.</summary>
	None,
	/// <summary>The placeholder for a new item appears at the beginning of the collection.  New items are at the beginning of the collection, after the new item placeholder. </summary>
	AtBeginning,
	/// <summary>The placeholder for a new item appears at the end of the collection. New items are added at the end of the collection, before the new item placeholder.</summary>
	AtEnd
}
