using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MS.Internal;

namespace System.Windows.Input;

/// <summary>Provides a <see cref="T:System.Windows.WeakEventManager" /> implementation so that you can use the "weak event listener" pattern to attach listeners for the <see cref="E:System.Windows.Input.ICommand.CanExecuteChanged" /> event.</summary>
public class CanExecuteChangedEventManager : WeakEventManager
{
	private class HandlerSink
	{
		private CanExecuteChangedEventManager _manager;

		private WeakReference _source;

		private WeakReference _originalHandler;

		private EventHandler _onCanExecuteChangedHandler;

		public bool IsInactive
		{
			get
			{
				if (_source != null && _source.IsAlive && _originalHandler != null)
				{
					return !_originalHandler.IsAlive;
				}
				return true;
			}
		}

		public EventHandler<EventArgs> Handler
		{
			get
			{
				if (_originalHandler == null)
				{
					return null;
				}
				return (EventHandler<EventArgs>)_originalHandler.Target;
			}
		}

		public HandlerSink(CanExecuteChangedEventManager manager, ICommand source, EventHandler<EventArgs> originalHandler)
		{
			_manager = manager;
			_source = new WeakReference(source);
			_originalHandler = new WeakReference(originalHandler);
			_onCanExecuteChangedHandler = OnCanExecuteChanged;
			source.CanExecuteChanged += _onCanExecuteChangedHandler;
		}

		public bool Matches(ICommand source, EventHandler<EventArgs> handler)
		{
			if (_source != null && (ICommand)_source.Target == source)
			{
				if (_originalHandler != null)
				{
					return (EventHandler<EventArgs>)_originalHandler.Target == handler;
				}
				return false;
			}
			return false;
		}

		public void Detach(bool isOnOriginalThread)
		{
			if (_source != null)
			{
				ICommand command = (ICommand)_source.Target;
				if (command != null && isOnOriginalThread)
				{
					command.CanExecuteChanged -= _onCanExecuteChangedHandler;
				}
				_source = null;
				_originalHandler = null;
			}
		}

		private void OnCanExecuteChanged(object sender, EventArgs e)
		{
			if (_source != null)
			{
				if (sender is CommandManager)
				{
					sender = _source.Target;
				}
				EventHandler<EventArgs> eventHandler = (EventHandler<EventArgs>)_originalHandler.Target;
				if (eventHandler != null)
				{
					eventHandler(sender, e);
				}
				else
				{
					_manager.ScheduleCleanup();
				}
			}
		}
	}

	private ConditionalWeakTable<object, object> _cwt = new ConditionalWeakTable<object, object>();

	private static readonly object StaticSource = new NamedObject("StaticSource");

	private static CanExecuteChangedEventManager CurrentManager
	{
		get
		{
			Type typeFromHandle = typeof(CanExecuteChangedEventManager);
			CanExecuteChangedEventManager canExecuteChangedEventManager = (CanExecuteChangedEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
			if (canExecuteChangedEventManager == null)
			{
				canExecuteChangedEventManager = new CanExecuteChangedEventManager();
				WeakEventManager.SetCurrentManager(typeFromHandle, canExecuteChangedEventManager);
			}
			return canExecuteChangedEventManager;
		}
	}

	private CanExecuteChangedEventManager()
	{
	}

	/// <summary>Adds the specified delegate as an event handler of the specified source.</summary>
	/// <param name="source">The source object that the raises the <see cref="E:System.Windows.Input.ICommand.CanExecuteChanged" /> event.</param>
	/// <param name="handler">The delegate that handles the <see cref="E:System.Windows.Input.ICommand.CanExecuteChanged" /> event.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.--or-- <paramref name="handler" /> is null.</exception>
	public static void AddHandler(ICommand source, EventHandler<EventArgs> handler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.PrivateAddHandler(source, handler);
	}

	/// <summary>Removes the specified event handler from the specified source.</summary>
	/// <param name="source">The source object that the raises the <see cref="E:System.Windows.Input.ICommand.CanExecuteChanged" /> event.</param>
	/// <param name="handler">The delegate that handles the <see cref="E:System.Windows.Input.ICommand.CanExecuteChanged" /> event.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.--or-- <paramref name="handler" /> is null.</exception>
	public static void RemoveHandler(ICommand source, EventHandler<EventArgs> handler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.PrivateRemoveHandler(source, handler);
	}

	/// <summary>Begins listening for the <see cref="E:System.Windows.Input.ICommand.CanExecuteChanged" /> event on the specified source.</summary>
	/// <param name="source">The object with the event.</param>
	protected override void StartListening(object source)
	{
	}

	/// <summary>Stops listening for the <see cref="E:System.Windows.Input.ICommand.CanExecuteChanged" /> event on the specified source.</summary>
	/// <param name="source">The source object to stop listening for.</param>
	protected override void StopListening(object source)
	{
	}

	/// <summary>Removes inactive listener entries from the data list for the provided source.</summary>
	/// <returns>true if some entries were actually removed; otherwise, false.</returns>
	/// <param name="source">The source for events being listened to.</param>
	/// <param name="data">The data to check. This object is expected to be a <see cref="T:System.Windows.WeakEventManager.ListenerList" /> implementation.</param>
	/// <param name="purgeAll">true to stop listening to <paramref name="source" />, and completely remove all entries from <paramref name="data" />.</param>
	protected override bool Purge(object source, object data, bool purgeAll)
	{
		bool isOnOriginalThread = !purgeAll || CheckAccess();
		List<HandlerSink> list = data as List<HandlerSink>;
		List<HandlerSink> list2 = null;
		bool flag = false;
		bool flag2 = purgeAll || source == null;
		if (!flag2)
		{
			foreach (HandlerSink item in list)
			{
				if (item.IsInactive)
				{
					if (list2 == null)
					{
						list2 = new List<HandlerSink>();
					}
					list2.Add(item);
				}
			}
			flag2 = list2 != null && list2.Count == list.Count;
		}
		if (flag2)
		{
			list2 = list;
		}
		flag = list2 != null;
		if (flag2 && !purgeAll && source != null)
		{
			Remove(source);
		}
		if (flag)
		{
			foreach (HandlerSink item2 in list2)
			{
				EventHandler<EventArgs> handler = item2.Handler;
				item2.Detach(isOnOriginalThread);
				if (!flag2)
				{
					list.Remove(item2);
				}
				if (handler != null)
				{
					RemoveHandlerFromCWT(handler, _cwt);
				}
			}
		}
		return flag;
	}

	private void PrivateAddHandler(ICommand source, EventHandler<EventArgs> handler)
	{
		List<HandlerSink> list = (List<HandlerSink>)base[source];
		if (list == null)
		{
			list = (List<HandlerSink>)(base[source] = new List<HandlerSink>());
		}
		HandlerSink item = new HandlerSink(this, source, handler);
		list.Add(item);
		AddHandlerToCWT(handler, _cwt);
	}

	private void PrivateRemoveHandler(ICommand source, EventHandler<EventArgs> handler)
	{
		List<HandlerSink> list = (List<HandlerSink>)base[source];
		if (list == null)
		{
			return;
		}
		HandlerSink handlerSink = null;
		bool flag = false;
		foreach (HandlerSink item in list)
		{
			if (item.Matches(source, handler))
			{
				handlerSink = item;
				break;
			}
			if (item.IsInactive)
			{
				flag = true;
			}
		}
		if (handlerSink != null)
		{
			list.Remove(handlerSink);
			handlerSink.Detach(isOnOriginalThread: true);
			RemoveHandlerFromCWT(handler, _cwt);
		}
		if (flag)
		{
			ScheduleCleanup();
		}
	}

	private void AddHandlerToCWT(Delegate handler, ConditionalWeakTable<object, object> cwt)
	{
		object obj = handler.Target;
		if (obj == null)
		{
			obj = StaticSource;
		}
		if (!cwt.TryGetValue(obj, out var value))
		{
			cwt.Add(obj, handler);
			return;
		}
		List<Delegate> list = value as List<Delegate>;
		if (list == null)
		{
			Delegate item = value as Delegate;
			list = new List<Delegate>();
			list.Add(item);
			cwt.Remove(obj);
			cwt.Add(obj, list);
		}
		list.Add(handler);
	}

	private void RemoveHandlerFromCWT(Delegate handler, ConditionalWeakTable<object, object> cwt)
	{
		object obj = handler.Target;
		if (obj == null)
		{
			obj = StaticSource;
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
}
