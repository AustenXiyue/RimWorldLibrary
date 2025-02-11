using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.WindowsBase;
using MS.Utility;

namespace System.Windows;

/// <summary>Provides a base class for the event manager that is used in the weak event pattern. The manager adds and removes listeners for events (or callbacks) that also use the pattern.</summary>
public abstract class WeakEventManager : DispatcherObject
{
	internal struct Listener
	{
		private WeakReference _target;

		private WeakReference _handler;

		public object Target => _target.Target;

		public Delegate Handler
		{
			get
			{
				if (_handler == null)
				{
					return null;
				}
				return (Delegate)_handler.Target;
			}
		}

		public bool HasHandler => _handler != null;

		public Listener(object target)
		{
			if (target == null)
			{
				target = StaticSource;
			}
			_target = new WeakReference(target);
			_handler = null;
		}

		public Listener(object target, Delegate handler)
		{
			_target = new WeakReference(target);
			_handler = new WeakReference(handler);
		}

		public bool Matches(object target, Delegate handler)
		{
			if (target == Target)
			{
				return object.Equals(handler, Handler);
			}
			return false;
		}
	}

	/// <summary>Provides a built-in collection list for storing listeners for a <see cref="T:System.Windows.WeakEventManager" />.</summary>
	protected class ListenerList
	{
		private FrugalObjectList<Listener> _list;

		private int _users;

		private ConditionalWeakTable<object, object> _cwt = new ConditionalWeakTable<object, object>();

		private static ListenerList s_empty = new ListenerList();

		/// <summary>Gets or sets a specific listener item in the <see cref="T:System.Windows.WeakEventManager.ListenerList" /> .</summary>
		/// <returns>The item at that index, or a null reference if no item was at that index.</returns>
		/// <param name="index">The zero-based index of the listener in the list.</param>
		public IWeakEventListener this[int index] => (IWeakEventListener)_list[index].Target;

		/// <summary>Gets the number of items contained in the <see cref="T:System.Windows.WeakEventManager.ListenerList" />.</summary>
		/// <returns>The number of items contained in the <see cref="T:System.Windows.WeakEventManager.ListenerList" />.</returns>
		public int Count => _list.Count;

		/// <summary>Gets a value that declares whether this <see cref="T:System.Windows.WeakEventManager.ListenerList" />  is empty.</summary>
		/// <returns>true if the list is empty; otherwise, false.</returns>
		public bool IsEmpty => _list.Count == 0;

		/// <summary>Gets a value that represents an empty list for purposes of comparisons.</summary>
		/// <returns>The empty list representation.</returns>
		public static ListenerList Empty => s_empty;

		/// <summary>Initializes a new instance of the <see cref="T:System.Windows.WeakEventManager.ListenerList" /> class.</summary>
		public ListenerList()
		{
			_list = new FrugalObjectList<Listener>();
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Windows.WeakEventManager.ListenerList" /> class with the specified initial capacity.</summary>
		/// <param name="capacity">The number of items that should be allocated in the initial list.</param>
		public ListenerList(int capacity)
		{
			_list = new FrugalObjectList<Listener>(capacity);
		}

		internal Listener GetListener(int index)
		{
			return _list[index];
		}

		/// <summary>Adds a <see cref="T:System.Windows.IWeakEventListener" /> object to the <see cref="T:System.Windows.WeakEventManager.ListenerList" />.</summary>
		/// <param name="listener">The listener element to add to the <see cref="T:System.Windows.WeakEventManager.ListenerList" />.</param>
		public void Add(IWeakEventListener listener)
		{
			Invariant.Assert(_users == 0, "Cannot modify a ListenerList that is in use");
			_list.Add(new Listener(listener));
		}

		/// <summary>Removes the first occurrence of a listener item from the <see cref="T:System.Windows.WeakEventManager.ListenerList" />. </summary>
		/// <param name="listener">The item to remove.</param>
		public void Remove(IWeakEventListener listener)
		{
			Invariant.Assert(_users == 0, "Cannot modify a ListenerList that is in use");
			for (int num = _list.Count - 1; num >= 0; num--)
			{
				if (_list[num].Target == listener)
				{
					_list.RemoveAt(num);
					break;
				}
			}
		}

		/// <summary>Adds an event handler to the <see cref="T:System.Windows.WeakEventManager.ListenerList" />.</summary>
		/// <param name="handler">The event handler to add to the <see cref="T:System.Windows.WeakEventManager.ListenerList" />.</param>
		public void AddHandler(Delegate handler)
		{
			Invariant.Assert(_users == 0, "Cannot modify a ListenerList that is in use");
			object obj = handler.Target;
			if (obj == null)
			{
				obj = StaticSource;
			}
			_list.Add(new Listener(obj, handler));
			AddHandlerToCWT(obj, handler);
		}

		private void AddHandlerToCWT(object target, Delegate handler)
		{
			if (!_cwt.TryGetValue(target, out var value))
			{
				_cwt.Add(target, handler);
				return;
			}
			List<Delegate> list = value as List<Delegate>;
			if (list == null)
			{
				Delegate item = value as Delegate;
				list = new List<Delegate>();
				list.Add(item);
				_cwt.Remove(target);
				_cwt.Add(target, list);
			}
			list.Add(handler);
		}

		/// <summary>Removes an event handler from the <see cref="T:System.Windows.WeakEventManager.ListenerList" />.</summary>
		/// <param name="handler">The event handler to remove from the <see cref="T:System.Windows.WeakEventManager.ListenerList" />.</param>
		public void RemoveHandler(Delegate handler)
		{
			Invariant.Assert(_users == 0, "Cannot modify a ListenerList that is in use");
			object obj = handler.Target;
			if (obj == null)
			{
				obj = StaticSource;
			}
			for (int num = _list.Count - 1; num >= 0; num--)
			{
				if (_list[num].Matches(obj, handler))
				{
					_list.RemoveAt(num);
					break;
				}
			}
			if (!_cwt.TryGetValue(obj, out var value))
			{
				return;
			}
			if (!(value is List<Delegate> list))
			{
				_cwt.Remove(obj);
				return;
			}
			list.Remove(handler);
			if (list.Count == 0)
			{
				_cwt.Remove(obj);
			}
		}

		internal void Add(Listener listener)
		{
			Invariant.Assert(_users == 0, "Cannot modify a ListenerList that is in use");
			object target = listener.Target;
			if (target != null)
			{
				_list.Add(listener);
				if (listener.HasHandler)
				{
					AddHandlerToCWT(target, listener.Handler);
				}
			}
		}

		/// <summary>Checks to see whether the provided list is in use, and if so, sets the list reference parameter to a copy of that list rather than the original.</summary>
		/// <returns>true if the provided list was in use at the time of call and therefore the <paramref name="list" /> parameter reference was reset to be a copy. false if the provided list was not in use, in which case the <paramref name="list" /> parameter reference remains unaltered.</returns>
		/// <param name="list">The list to check for use state and potentially copy.</param>
		public static bool PrepareForWriting(ref ListenerList list)
		{
			bool num = list.BeginUse();
			list.EndUse();
			if (num)
			{
				list = list.Clone();
			}
			return num;
		}

		/// <summary>Delivers the event being managed to each listener in the <see cref="T:System.Windows.WeakEventManager.ListenerList" />.</summary>
		/// <returns>true if any of the listeners in the <see cref="T:System.Windows.WeakEventManager.ListenerList" /> refer to an object that has been garbage collected; otherwise, false.</returns>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="args">An object that contains the event data.</param>
		/// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager" /> that calls this method.</param>
		public virtual bool DeliverEvent(object sender, EventArgs args, Type managerType)
		{
			bool flag = false;
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				Listener listener = GetListener(i);
				flag |= DeliverEvent(ref listener, sender, args, managerType);
			}
			return flag;
		}

		internal bool DeliverEvent(ref Listener listener, object sender, EventArgs args, Type managerType)
		{
			object target = listener.Target;
			bool num = target == null;
			if (!num)
			{
				if (listener.HasHandler)
				{
					EventHandler eventHandler = (EventHandler)listener.Handler;
					if (eventHandler != null)
					{
						eventHandler(sender, args);
						return num;
					}
				}
				else if (target is IWeakEventListener weakEventListener)
				{
					bool flag = weakEventListener.ReceiveWeakEvent(managerType, sender, args);
					if (!flag)
					{
						Invariant.Assert(flag, SR.ListenerDidNotHandleEvent, SR.Format(SR.ListenerDidNotHandleEventDetail, weakEventListener.GetType(), managerType));
					}
				}
			}
			return num;
		}

		/// <summary>Removes all entries from the list where the underlying reference target is a null reference.</summary>
		/// <returns>Returns true if any entries were purged; otherwise, false.</returns>
		public bool Purge()
		{
			Invariant.Assert(_users == 0, "Cannot modify a ListenerList that is in use");
			bool result = false;
			for (int num = _list.Count - 1; num >= 0; num--)
			{
				if (_list[num].Target == null)
				{
					_list.RemoveAt(num);
					result = true;
				}
			}
			return result;
		}

		/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.WeakEventManager.ListenerList" />. </summary>
		/// <returns>A modifiable clone of the current object. </returns>
		public virtual ListenerList Clone()
		{
			ListenerList listenerList = new ListenerList();
			CopyTo(listenerList);
			return listenerList;
		}

		/// <summary>Copies the current <see cref="T:System.Windows.WeakEventManager.ListenerList" /> to the specified <see cref="T:System.Windows.WeakEventManager.ListenerList" /></summary>
		/// <param name="newList">The object to copy to.</param>
		protected void CopyTo(ListenerList newList)
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				Listener listener = GetListener(i);
				if (listener.Target == null)
				{
					continue;
				}
				if (listener.HasHandler)
				{
					Delegate handler = listener.Handler;
					if ((object)handler != null)
					{
						newList.AddHandler(handler);
					}
				}
				else if (listener.Target is IWeakEventListener listener2)
				{
					newList.Add(listener2);
				}
			}
		}

		/// <summary>Declares the list to be in use. This prevents direct changes to the list during iterations of the list items.</summary>
		/// <returns>true if the list was already declared to be in use; otherwise, false.</returns>
		public bool BeginUse()
		{
			return Interlocked.Increment(ref _users) != 1;
		}

		/// <summary>Unlocks the locked state initiated by <see cref="M:System.Windows.WeakEventManager.ListenerList.BeginUse" />.</summary>
		public void EndUse()
		{
			Interlocked.Decrement(ref _users);
		}
	}

	/// <summary>Provides a type-safe collection list for storing listeners for a <see cref="T:System.Windows.WeakEventManager" />. This class defines a type parameter for the event data that is used.</summary>
	/// <typeparam name="TEventArgs">The type that holds the event data.</typeparam>
	protected class ListenerList<TEventArgs> : ListenerList where TEventArgs : EventArgs
	{
		/// <summary>Initializes a new instance of the <see cref="T:System.Windows.WeakEventManager.ListenerList`1" /> class.</summary>
		public ListenerList()
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Windows.WeakEventManager.ListenerList`1" /> class with the specified initial capacity.</summary>
		/// <param name="capacity">The number of items that should be allocated in the initial list.</param>
		public ListenerList(int capacity)
			: base(capacity)
		{
		}

		/// <summary>Delivers the event being managed to each listener in the <see cref="T:System.Windows.WeakEventManager.ListenerList`1" />.</summary>
		/// <returns>true if any of the listeners in the <see cref="T:System.Windows.WeakEventManager.ListenerList`1" /> refer to an object that has been garbage collected; otherwise, false.</returns>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An object that contains the event data.</param>
		/// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager" /> that calls this method.</param>
		public override bool DeliverEvent(object sender, EventArgs e, Type managerType)
		{
			TEventArgs e2 = (TEventArgs)e;
			bool flag = false;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				Listener listener = GetListener(i);
				if (listener.Target != null)
				{
					EventHandler<TEventArgs> eventHandler = (EventHandler<TEventArgs>)listener.Handler;
					if (eventHandler != null)
					{
						eventHandler(sender, e2);
					}
					else
					{
						flag |= DeliverEvent(ref listener, sender, e, managerType);
					}
				}
				else
				{
					flag = true;
				}
			}
			return flag;
		}

		/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.WeakEventManager.ListenerList" />, making deep copies of the values.</summary>
		/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
		public override ListenerList Clone()
		{
			ListenerList<TEventArgs> listenerList = new ListenerList<TEventArgs>();
			CopyTo(listenerList);
			return listenerList;
		}
	}

	private WeakEventTable _table;

	private static readonly object StaticSource;

	private static MethodInfo s_DeliverEventMethodInfo;

	/// <summary>Establishes a read-lock on the underlying data table, and returns an <see cref="T:System.IDisposable" />.</summary>
	/// <returns>An object that can be used to establish a lock on the data table members and then be appropriately disposed with a using construct.</returns>
	protected IDisposable ReadLock => Table.ReadLock;

	/// <summary>Establishes a write-lock on the underlying data table, and returns an<see cref="T:System.IDisposable" />.</summary>
	/// <returns>An object that can be used to establish a lock on the data table members and then be appropriately disposed with a using construct.</returns>
	protected IDisposable WriteLock => Table.WriteLock;

	/// <summary>Gets or sets the data being stored for the specified source.</summary>
	/// <returns>Data being stored by the manager for this source.</returns>
	/// <param name="source">The zero-based index of the requested source.</param>
	protected object this[object source]
	{
		get
		{
			return Table[this, source];
		}
		set
		{
			Table[this, source] = value;
		}
	}

	internal static MethodInfo DeliverEventMethodInfo => s_DeliverEventMethodInfo;

	private WeakEventTable Table => _table;

	/// <summary>Initializes base class values when it is used as the initializer by the constructor of a derived class.</summary>
	protected WeakEventManager()
	{
		_table = WeakEventTable.CurrentWeakEventTable;
	}

	static WeakEventManager()
	{
		StaticSource = new NamedObject("StaticSource");
		s_DeliverEventMethodInfo = typeof(WeakEventManager).GetMethod("DeliverEvent", BindingFlags.Instance | BindingFlags.NonPublic);
	}

	/// <summary>Returns a new object to contain listeners to an event.</summary>
	/// <returns>A new object to contain listeners to an event.</returns>
	protected virtual ListenerList NewListenerList()
	{
		return new ListenerList();
	}

	/// <summary>When overridden in a derived class, starts listening for the event being managed. After the <see cref="M:System.Windows.WeakEventManager.StartListening(System.Object)" /> method is first called, the manager should be in the state of calling <see cref="M:System.Windows.WeakEventManager.DeliverEvent(System.Object,System.EventArgs)" /> or <see cref="M:System.Windows.WeakEventManager.DeliverEventToList(System.Object,System.EventArgs,System.Windows.WeakEventManager.ListenerList)" /> whenever the relevant event from the provided source is handled.</summary>
	/// <param name="source">The source to begin listening on.</param>
	protected abstract void StartListening(object source);

	/// <summary>When overridden in a derived class, stops listening on the provided source for the event being managed.</summary>
	/// <param name="source">The source to stop listening on.</param>
	protected abstract void StopListening(object source);

	/// <summary>Returns the <see cref="T:System.Windows.WeakEventManager" /> implementation that is used for the provided type.</summary>
	/// <returns>The matching <see cref="T:System.Windows.WeakEventManager" /> implementation.</returns>
	/// <param name="managerType">The type to obtain the <see cref="T:System.Windows.WeakEventManager" /> for.</param>
	protected static WeakEventManager GetCurrentManager(Type managerType)
	{
		return WeakEventTable.CurrentWeakEventTable[managerType];
	}

	/// <summary>Sets the current manager for the specified manager type.</summary>
	/// <param name="managerType">The type to set the new event manager.</param>
	/// <param name="manager">The new event manager.</param>
	protected static void SetCurrentManager(Type managerType, WeakEventManager manager)
	{
		WeakEventTable.CurrentWeakEventTable[managerType] = manager;
	}

	internal static WeakEventManager GetCurrentManager(Type eventSourceType, string eventName)
	{
		return WeakEventTable.CurrentWeakEventTable[eventSourceType, eventName];
	}

	internal static void SetCurrentManager(Type eventSourceType, string eventName, WeakEventManager manager)
	{
		WeakEventTable.CurrentWeakEventTable[eventSourceType, eventName] = manager;
	}

	/// <summary>Removes all listeners for the specified source.</summary>
	/// <param name="source">The source to remove listener information for.</param>
	protected void Remove(object source)
	{
		Table.Remove(this, source);
	}

	/// <summary>Adds the provided listener to the provided source for the event being managed.</summary>
	/// <param name="source">The source to attach listeners to.</param>
	/// <param name="listener">The listening class (which must implement <see cref="T:System.Windows.IWeakEventListener" />).</param>
	protected void ProtectedAddListener(object source, IWeakEventListener listener)
	{
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		AddListener(source, listener, null);
	}

	/// <summary>Removes a previously added listener from the provided source.</summary>
	/// <param name="source">The source to remove listeners from.</param>
	/// <param name="listener">The listening class (which must implement <see cref="T:System.Windows.IWeakEventListener" />).</param>
	protected void ProtectedRemoveListener(object source, IWeakEventListener listener)
	{
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		RemoveListener(source, listener, null);
	}

	/// <summary>Adds the specified delegate as an event handler of the specified source.</summary>
	/// <param name="source">The source object that the handler delegate subscribes to.</param>
	/// <param name="handler">The delegate that handles the event that is raised by <paramref name="source" />.</param>
	protected void ProtectedAddHandler(object source, Delegate handler)
	{
		if ((object)handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		AddListener(source, null, handler);
	}

	/// <summary>Removes the previously added handler from the specified source.</summary>
	/// <param name="source">The source to remove the handler from. </param>
	/// <param name="handler">The delegate to remove from <paramref name="source" />.</param>
	protected void ProtectedRemoveHandler(object source, Delegate handler)
	{
		if ((object)handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		RemoveListener(source, null, handler);
	}

	private void AddListener(object source, IWeakEventListener listener, Delegate handler)
	{
		object source2 = ((source != null) ? source : StaticSource);
		using (Table.WriteLock)
		{
			ListenerList list = (ListenerList)Table[this, source2];
			if (list == null)
			{
				list = NewListenerList();
				Table[this, source2] = list;
				StartListening(source);
			}
			if (ListenerList.PrepareForWriting(ref list))
			{
				Table[this, source] = list;
			}
			if ((object)handler != null)
			{
				list.AddHandler(handler);
			}
			else
			{
				list.Add(listener);
			}
			ScheduleCleanup();
		}
	}

	private void RemoveListener(object source, object target, Delegate handler)
	{
		object source2 = ((source != null) ? source : StaticSource);
		using (Table.WriteLock)
		{
			ListenerList list = (ListenerList)Table[this, source2];
			if (list != null)
			{
				if (ListenerList.PrepareForWriting(ref list))
				{
					Table[this, source2] = list;
				}
				if ((object)handler != null)
				{
					list.RemoveHandler(handler);
				}
				else
				{
					list.Remove((IWeakEventListener)target);
				}
				if (list.IsEmpty)
				{
					Table.Remove(this, source2);
					StopListening(source);
				}
			}
		}
	}

	/// <summary>Delivers the event being managed to each listener.</summary>
	/// <param name="sender">The object on which the event is being handled.</param>
	/// <param name="args">An <see cref="T:System.EventArgs" /> that contains the event data for the event to deliver.</param>
	protected void DeliverEvent(object sender, EventArgs args)
	{
		object source = ((sender != null) ? sender : StaticSource);
		ListenerList listenerList;
		using (Table.ReadLock)
		{
			listenerList = (ListenerList)Table[this, source];
			if (listenerList == null)
			{
				listenerList = ListenerList.Empty;
			}
			listenerList.BeginUse();
		}
		try
		{
			DeliverEventToList(sender, args, listenerList);
		}
		finally
		{
			listenerList.EndUse();
		}
	}

	/// <summary>Delivers the event being managed to each listener in the provided list.</summary>
	/// <param name="sender">The object on which the event is being handled.</param>
	/// <param name="args">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	/// <param name="list">The provided <see cref="T:System.Windows.WeakEventManager.ListenerList" />.</param>
	protected void DeliverEventToList(object sender, EventArgs args, ListenerList list)
	{
		if (list.DeliverEvent(sender, args, GetType()))
		{
			ScheduleCleanup();
		}
	}

	/// <summary>Requests that a purge of unused entries in the underlying listener list be performed on a lower priority thread.</summary>
	protected void ScheduleCleanup()
	{
		Table.ScheduleCleanup();
	}

	/// <summary>Removes inactive listener entries from the data list for the provided source. Returns true if some entries were actually removed from the list.</summary>
	/// <returns>true if some entries were actually removed; otherwise, false.</returns>
	/// <param name="source">The source for events being listened to.</param>
	/// <param name="data">The data to check. This object is expected to be a <see cref="T:System.Windows.WeakEventManager.ListenerList" /> implementation.</param>
	/// <param name="purgeAll">true to stop listening to <paramref name="source" />, and completely remove all entries from <paramref name="data" />.</param>
	protected virtual bool Purge(object source, object data, bool purgeAll)
	{
		bool result = false;
		bool flag = purgeAll || source == null;
		if (!flag)
		{
			ListenerList list = (ListenerList)data;
			if (ListenerList.PrepareForWriting(ref list) && source != null)
			{
				Table[this, source] = list;
			}
			if (list.Purge())
			{
				result = true;
			}
			flag = list.IsEmpty;
		}
		if (flag && source != null)
		{
			StopListening(source);
			if (!purgeAll)
			{
				Table.Remove(this, source);
				result = true;
			}
		}
		return result;
	}

	internal bool PurgeInternal(object source, object data, bool purgeAll)
	{
		return Purge(source, data, purgeAll);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static bool Cleanup()
	{
		return WeakEventTable.Cleanup();
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static void SetCleanupEnabled(bool value)
	{
		WeakEventTable.CurrentWeakEventTable.IsCleanupEnabled = value;
	}
}
/// <summary>Provides a type-safe <see cref="T:System.Windows.WeakEventManager" /> that enables you to specify the event handler to use for the "weak event listener" pattern. This class defines a type parameter for the source of the event and a type parameter for the event data that is used.</summary>
/// <typeparam name="TEventSource">The type that raises the event.</typeparam>
/// <typeparam name="TEventArgs">The type that holds the event data.</typeparam>
public class WeakEventManager<TEventSource, TEventArgs> : WeakEventManager where TEventArgs : EventArgs
{
	private Delegate _handler;

	private string _eventName;

	private EventInfo _eventInfo;

	private WeakEventManager(string eventName)
	{
		_eventName = eventName;
		_eventInfo = typeof(TEventSource).GetEvent(_eventName);
		if (_eventInfo == null)
		{
			throw new ArgumentException(SR.Format(SR.EventNotFound, typeof(TEventSource).FullName, eventName));
		}
		_handler = Delegate.CreateDelegate(_eventInfo.EventHandlerType, this, WeakEventManager.DeliverEventMethodInfo);
	}

	/// <summary>Adds the specified event handler to the specified event.</summary>
	/// <param name="source">The source object that raises the specified event.</param>
	/// <param name="eventName">The name of the event to subscribe to.</param>
	/// <param name="handler">The delegate that handles the event.</param>
	public static void AddHandler(TEventSource source, string eventName, EventHandler<TEventArgs> handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager(eventName).ProtectedAddHandler(source, handler);
	}

	/// <summary>Removes the specified event handler from the specified event.</summary>
	/// <param name="source">The source object that raises the specified event.</param>
	/// <param name="eventName">The name of the event to remove the handler from.</param>
	/// <param name="handler">The delegate to remove</param>
	public static void RemoveHandler(TEventSource source, string eventName, EventHandler<TEventArgs> handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager(eventName).ProtectedRemoveHandler(source, handler);
	}

	/// <summary>Returns a new object to contain listeners to an event.</summary>
	/// <returns>A new object to contain listeners to an event.</returns>
	protected override ListenerList NewListenerList()
	{
		return new ListenerList<TEventArgs>();
	}

	/// <summary>Starts listening for the event on the specified object.</summary>
	/// <param name="source">The object to that raises the event.</param>
	protected override void StartListening(object source)
	{
		_eventInfo.AddEventHandler(source, _handler);
	}

	/// <summary>Stops listening for the event on the specified object.</summary>
	/// <param name="source">The object to that raises the event.</param>
	protected override void StopListening(object source)
	{
		_eventInfo.RemoveEventHandler(source, _handler);
	}

	private static WeakEventManager<TEventSource, TEventArgs> CurrentManager(string eventName)
	{
		WeakEventManager<TEventSource, TEventArgs> weakEventManager = (WeakEventManager<TEventSource, TEventArgs>)WeakEventManager.GetCurrentManager(typeof(TEventSource), eventName);
		if (weakEventManager == null)
		{
			weakEventManager = new WeakEventManager<TEventSource, TEventArgs>(eventName);
			WeakEventManager.SetCurrentManager(typeof(TEventSource), eventName, weakEventManager);
		}
		return weakEventManager;
	}
}
