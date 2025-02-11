namespace System.ComponentModel;

/// <summary>Indicates whether a class converts property change events to <see cref="E:System.ComponentModel.IBindingList.ListChanged" /> events.</summary>
public interface IRaiseItemChangedEvents
{
	/// <summary>Gets a value indicating whether the <see cref="T:System.ComponentModel.IRaiseItemChangedEvents" /> object raises <see cref="E:System.ComponentModel.IBindingList.ListChanged" /> events.</summary>
	/// <returns>true if the <see cref="T:System.ComponentModel.IRaiseItemChangedEvents" /> object raises <see cref="E:System.ComponentModel.IBindingList.ListChanged" /> events when one of its property values changes; otherwise, false.</returns>
	bool RaisesItemChangedEvents { get; }
}
