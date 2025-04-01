using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a simple list of delegates. This class cannot be inherited.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public sealed class EventHandlerList : IDisposable
{
	private sealed class ListEntry
	{
		internal ListEntry next;

		internal object key;

		internal Delegate handler;

		public ListEntry(object key, Delegate handler, ListEntry next)
		{
			this.next = next;
			this.key = key;
			this.handler = handler;
		}
	}

	private ListEntry head;

	private Component parent;

	/// <summary>Gets or sets the delegate for the specified object.</summary>
	/// <returns>The delegate for the specified key, or null if a delegate does not exist.</returns>
	/// <param name="key">An object to find in the list. </param>
	public Delegate this[object key]
	{
		get
		{
			ListEntry listEntry = null;
			if (parent == null || parent.CanRaiseEventsInternal)
			{
				listEntry = Find(key);
			}
			return listEntry?.handler;
		}
		set
		{
			ListEntry listEntry = Find(key);
			if (listEntry != null)
			{
				listEntry.handler = value;
			}
			else
			{
				head = new ListEntry(key, value, head);
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.EventHandlerList" /> class. </summary>
	public EventHandlerList()
	{
	}

	internal EventHandlerList(Component parent)
	{
		this.parent = parent;
	}

	/// <summary>Adds a delegate to the list.</summary>
	/// <param name="key">The object that owns the event. </param>
	/// <param name="value">The delegate to add to the list. </param>
	public void AddHandler(object key, Delegate value)
	{
		ListEntry listEntry = Find(key);
		if (listEntry != null)
		{
			listEntry.handler = Delegate.Combine(listEntry.handler, value);
		}
		else
		{
			head = new ListEntry(key, value, head);
		}
	}

	/// <summary>Adds a list of delegates to the current list.</summary>
	/// <param name="listToAddFrom">The list to add.</param>
	public void AddHandlers(EventHandlerList listToAddFrom)
	{
		for (ListEntry next = listToAddFrom.head; next != null; next = next.next)
		{
			AddHandler(next.key, next.handler);
		}
	}

	/// <summary>Disposes the delegate list.</summary>
	public void Dispose()
	{
		head = null;
	}

	private ListEntry Find(object key)
	{
		ListEntry next = head;
		while (next != null && next.key != key)
		{
			next = next.next;
		}
		return next;
	}

	/// <summary>Removes a delegate from the list.</summary>
	/// <param name="key">The object that owns the event. </param>
	/// <param name="value">The delegate to remove from the list. </param>
	public void RemoveHandler(object key, Delegate value)
	{
		ListEntry listEntry = Find(key);
		if (listEntry != null)
		{
			listEntry.handler = Delegate.Remove(listEntry.handler, value);
		}
	}
}
