using System.Runtime.CompilerServices;

namespace System.Collections.Specialized;

/// <summary>Describes the action that caused a <see cref="E:System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged" /> event. </summary>
[TypeForwardedFrom("WindowsBase, Version=3.0.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
public enum NotifyCollectionChangedAction
{
	/// <summary>One or more items were added to the collection.</summary>
	Add,
	/// <summary>One or more items were removed from the collection.</summary>
	Remove,
	/// <summary>One or more items were replaced in the collection.</summary>
	Replace,
	/// <summary>One or more items were moved within the collection.</summary>
	Move,
	/// <summary>The content of the collection changed dramatically.</summary>
	Reset
}
