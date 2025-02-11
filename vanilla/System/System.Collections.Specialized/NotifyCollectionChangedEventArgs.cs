using System.Runtime.CompilerServices;

namespace System.Collections.Specialized;

/// <summary>Provides data for the <see cref="E:System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged" /> event.</summary>
[TypeForwardedFrom("WindowsBase, Version=3.0.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
public class NotifyCollectionChangedEventArgs : EventArgs
{
	private NotifyCollectionChangedAction _action;

	private IList _newItems;

	private IList _oldItems;

	private int _newStartingIndex = -1;

	private int _oldStartingIndex = -1;

	/// <summary>Gets the action that caused the event. </summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.NotifyCollectionChangedAction" /> value that describes the action that caused the event.</returns>
	public NotifyCollectionChangedAction Action => _action;

	/// <summary>Gets the list of new items involved in the change.</summary>
	/// <returns>The list of new items involved in the change.</returns>
	public IList NewItems => _newItems;

	/// <summary>Gets the list of items affected by a <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" />, Remove, or Move action.</summary>
	/// <returns>The list of items affected by a <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" />, Remove, or Move action.</returns>
	public IList OldItems => _oldItems;

	/// <summary>Gets the index at which the change occurred.</summary>
	/// <returns>The zero-based index at which the change occurred.</returns>
	public int NewStartingIndex => _newStartingIndex;

	/// <summary>Gets the index at which a <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Move" />, Remove, or Replace action occurred.</summary>
	/// <returns>The zero-based index at which a <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Move" />, Remove, or Replace action occurred.</returns>
	public int OldStartingIndex => _oldStartingIndex;

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Reset" /> change.</summary>
	/// <param name="action">The action that caused the event. This must be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Reset" />.</param>
	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action)
	{
		if (action != NotifyCollectionChangedAction.Reset)
		{
			throw new ArgumentException(global::SR.GetString("Constructor supports only the '{0}' action.", NotifyCollectionChangedAction.Reset), "action");
		}
		InitializeAdd(action, null, -1);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a one-item change.</summary>
	/// <param name="action">The action that caused the event. This can be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Reset" />, <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Add" />, or <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Remove" />.</param>
	/// <param name="changedItem">The item that is affected by the change.</param>
	/// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Reset, Add, or Remove, or if <paramref name="action" /> is Reset and <paramref name="changedItem" /> is not null.</exception>
	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem)
	{
		if (action != 0 && action != NotifyCollectionChangedAction.Remove && action != NotifyCollectionChangedAction.Reset)
		{
			throw new ArgumentException(global::SR.GetString("Constructor only supports either a Reset, Add, or Remove action."), "action");
		}
		if (action == NotifyCollectionChangedAction.Reset)
		{
			if (changedItem != null)
			{
				throw new ArgumentException(global::SR.GetString("Reset action must be initialized with no changed items."), "action");
			}
			InitializeAdd(action, null, -1);
		}
		else
		{
			InitializeAddOrRemove(action, new object[1] { changedItem }, -1);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a one-item change.</summary>
	/// <param name="action">The action that caused the event. This can be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Reset" />, <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Add" />, or <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Remove" />.</param>
	/// <param name="changedItem">The item that is affected by the change.</param>
	/// <param name="index">The index where the change occurred.</param>
	/// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Reset, Add, or Remove, or if <paramref name="action" /> is Reset and either <paramref name="changedItems" /> is not null or <paramref name="index" /> is not -1.</exception>
	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index)
	{
		if (action != 0 && action != NotifyCollectionChangedAction.Remove && action != NotifyCollectionChangedAction.Reset)
		{
			throw new ArgumentException(global::SR.GetString("Constructor only supports either a Reset, Add, or Remove action."), "action");
		}
		if (action == NotifyCollectionChangedAction.Reset)
		{
			if (changedItem != null)
			{
				throw new ArgumentException(global::SR.GetString("Reset action must be initialized with no changed items."), "action");
			}
			if (index != -1)
			{
				throw new ArgumentException(global::SR.GetString("Reset action must be initialized with index -1."), "action");
			}
			InitializeAdd(action, null, -1);
		}
		else
		{
			InitializeAddOrRemove(action, new object[1] { changedItem }, index);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a multi-item change.</summary>
	/// <param name="action">The action that caused the event. This can be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Reset" />, <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Add" />, or <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Remove" />.</param>
	/// <param name="changedItems">The items that are affected by the change.</param>
	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems)
	{
		if (action != 0 && action != NotifyCollectionChangedAction.Remove && action != NotifyCollectionChangedAction.Reset)
		{
			throw new ArgumentException(global::SR.GetString("Constructor only supports either a Reset, Add, or Remove action."), "action");
		}
		if (action == NotifyCollectionChangedAction.Reset)
		{
			if (changedItems != null)
			{
				throw new ArgumentException(global::SR.GetString("Reset action must be initialized with no changed items."), "action");
			}
			InitializeAdd(action, null, -1);
		}
		else
		{
			if (changedItems == null)
			{
				throw new ArgumentNullException("changedItems");
			}
			InitializeAddOrRemove(action, changedItems, -1);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a multi-item change or a <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Reset" /> change.</summary>
	/// <param name="action">The action that caused the event. This can be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Reset" />, <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Add" />, or <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Remove" />.</param>
	/// <param name="changedItems">The items affected by the change.</param>
	/// <param name="startingIndex">The index where the change occurred.</param>
	/// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Reset, Add, or Remove, if <paramref name="action" /> is Reset and either <paramref name="changedItems" /> is not null or <paramref name="startingIndex" /> is not -1, or if action is Add or Remove and <paramref name="startingIndex" /> is less than -1.</exception>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="action" /> is Add or Remove and <paramref name="changedItems" /> is null.</exception>
	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
	{
		if (action != 0 && action != NotifyCollectionChangedAction.Remove && action != NotifyCollectionChangedAction.Reset)
		{
			throw new ArgumentException(global::SR.GetString("Constructor only supports either a Reset, Add, or Remove action."), "action");
		}
		if (action == NotifyCollectionChangedAction.Reset)
		{
			if (changedItems != null)
			{
				throw new ArgumentException(global::SR.GetString("Reset action must be initialized with no changed items."), "action");
			}
			if (startingIndex != -1)
			{
				throw new ArgumentException(global::SR.GetString("Reset action must be initialized with index -1."), "action");
			}
			InitializeAdd(action, null, -1);
		}
		else
		{
			if (changedItems == null)
			{
				throw new ArgumentNullException("changedItems");
			}
			if (startingIndex < -1)
			{
				throw new ArgumentException(global::SR.GetString("Index cannot be negative."), "startingIndex");
			}
			InitializeAddOrRemove(action, changedItems, startingIndex);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a one-item <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" /> change.</summary>
	/// <param name="action">The action that caused the event. This can only be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" />.</param>
	/// <param name="newItem">The new item that is replacing the original item.</param>
	/// <param name="oldItem">The original item that is replaced.</param>
	/// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Replace.</exception>
	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem)
	{
		if (action != NotifyCollectionChangedAction.Replace)
		{
			throw new ArgumentException(global::SR.GetString("Constructor supports only the '{0}' action.", NotifyCollectionChangedAction.Replace), "action");
		}
		InitializeMoveOrReplace(action, new object[1] { newItem }, new object[1] { oldItem }, -1, -1);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a one-item <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" /> change.</summary>
	/// <param name="action">The action that caused the event. This can be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" />.</param>
	/// <param name="newItem">The new item that is replacing the original item.</param>
	/// <param name="oldItem">The original item that is replaced.</param>
	/// <param name="index">The index of the item being replaced.</param>
	/// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Replace.</exception>
	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
	{
		if (action != NotifyCollectionChangedAction.Replace)
		{
			throw new ArgumentException(global::SR.GetString("Constructor supports only the '{0}' action.", NotifyCollectionChangedAction.Replace), "action");
		}
		InitializeMoveOrReplace(action, new object[1] { newItem }, new object[1] { oldItem }, index, index);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a multi-item <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" /> change.</summary>
	/// <param name="action">The action that caused the event. This can only be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" />.</param>
	/// <param name="newItems">The new items that are replacing the original items.</param>
	/// <param name="oldItems">The original items that are replaced.</param>
	/// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Replace.</exception>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="oldItems" /> or <paramref name="newItems" /> is null.</exception>
	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems)
	{
		if (action != NotifyCollectionChangedAction.Replace)
		{
			throw new ArgumentException(global::SR.GetString("Constructor supports only the '{0}' action.", NotifyCollectionChangedAction.Replace), "action");
		}
		if (newItems == null)
		{
			throw new ArgumentNullException("newItems");
		}
		if (oldItems == null)
		{
			throw new ArgumentNullException("oldItems");
		}
		InitializeMoveOrReplace(action, newItems, oldItems, -1, -1);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a multi-item <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" /> change.</summary>
	/// <param name="action">The action that caused the event. This can only be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" />.</param>
	/// <param name="newItems">The new items that are replacing the original items.</param>
	/// <param name="oldItems">The original items that are replaced.</param>
	/// <param name="startingIndex">The index of the first item of the items that are being replaced.</param>
	/// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Replace.</exception>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="oldItems" /> or <paramref name="newItems" /> is null.</exception>
	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex)
	{
		if (action != NotifyCollectionChangedAction.Replace)
		{
			throw new ArgumentException(global::SR.GetString("Constructor supports only the '{0}' action.", NotifyCollectionChangedAction.Replace), "action");
		}
		if (newItems == null)
		{
			throw new ArgumentNullException("newItems");
		}
		if (oldItems == null)
		{
			throw new ArgumentNullException("oldItems");
		}
		InitializeMoveOrReplace(action, newItems, oldItems, startingIndex, startingIndex);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a one-item <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Move" /> change.</summary>
	/// <param name="action">The action that caused the event. This can only be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Move" />.</param>
	/// <param name="changedItem">The item affected by the change.</param>
	/// <param name="index">The new index for the changed item.</param>
	/// <param name="oldIndex">The old index for the changed item.</param>
	/// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Move or <paramref name="index" /> is less than 0.</exception>
	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex)
	{
		if (action != NotifyCollectionChangedAction.Move)
		{
			throw new ArgumentException(global::SR.GetString("Constructor supports only the '{0}' action.", NotifyCollectionChangedAction.Move), "action");
		}
		if (index < 0)
		{
			throw new ArgumentException(global::SR.GetString("Index cannot be negative."), "index");
		}
		object[] array = new object[1] { changedItem };
		InitializeMoveOrReplace(action, array, array, index, oldIndex);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a multi-item <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Move" /> change.</summary>
	/// <param name="action">The action that caused the event. This can only be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Move" />.</param>
	/// <param name="changedItems">The items affected by the change.</param>
	/// <param name="index">The new index for the changed items.</param>
	/// <param name="oldIndex">The old index for the changed items.</param>
	/// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Move or <paramref name="index" /> is less than 0.</exception>
	public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex)
	{
		if (action != NotifyCollectionChangedAction.Move)
		{
			throw new ArgumentException(global::SR.GetString("Constructor supports only the '{0}' action.", NotifyCollectionChangedAction.Move), "action");
		}
		if (index < 0)
		{
			throw new ArgumentException(global::SR.GetString("Index cannot be negative."), "index");
		}
		InitializeMoveOrReplace(action, changedItems, changedItems, index, oldIndex);
	}

	internal NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int newIndex, int oldIndex)
	{
		_action = action;
		_newItems = ((newItems == null) ? null : ArrayList.ReadOnly(newItems));
		_oldItems = ((oldItems == null) ? null : ArrayList.ReadOnly(oldItems));
		_newStartingIndex = newIndex;
		_oldStartingIndex = oldIndex;
	}

	private void InitializeAddOrRemove(NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
	{
		switch (action)
		{
		case NotifyCollectionChangedAction.Add:
			InitializeAdd(action, changedItems, startingIndex);
			break;
		case NotifyCollectionChangedAction.Remove:
			InitializeRemove(action, changedItems, startingIndex);
			break;
		}
	}

	private void InitializeAdd(NotifyCollectionChangedAction action, IList newItems, int newStartingIndex)
	{
		_action = action;
		_newItems = ((newItems == null) ? null : ArrayList.ReadOnly(newItems));
		_newStartingIndex = newStartingIndex;
	}

	private void InitializeRemove(NotifyCollectionChangedAction action, IList oldItems, int oldStartingIndex)
	{
		_action = action;
		_oldItems = ((oldItems == null) ? null : ArrayList.ReadOnly(oldItems));
		_oldStartingIndex = oldStartingIndex;
	}

	private void InitializeMoveOrReplace(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex, int oldStartingIndex)
	{
		InitializeAdd(action, newItems, startingIndex);
		InitializeRemove(action, oldItems, oldStartingIndex);
	}
}
