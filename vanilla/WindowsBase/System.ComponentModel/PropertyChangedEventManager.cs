using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using MS.Internal;

namespace System.ComponentModel;

/// <summary>Provides a <see cref="T:System.Windows.WeakEventManager" /> implementation so that you can use the "weak event listener" pattern to attach listeners for the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> event.</summary>
public class PropertyChangedEventManager : WeakEventManager
{
	private ListenerList _proposedAllListenersList;

	private List<string> _toRemove = new List<string>();

	private static readonly string AllListenersKey = "<All Listeners>";

	private static PropertyChangedEventManager CurrentManager
	{
		get
		{
			Type typeFromHandle = typeof(PropertyChangedEventManager);
			PropertyChangedEventManager propertyChangedEventManager = (PropertyChangedEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
			if (propertyChangedEventManager == null)
			{
				propertyChangedEventManager = new PropertyChangedEventManager();
				WeakEventManager.SetCurrentManager(typeFromHandle, propertyChangedEventManager);
			}
			return propertyChangedEventManager;
		}
	}

	private PropertyChangedEventManager()
	{
	}

	/// <summary>Adds the specified listener to the list of listeners on the specified source.</summary>
	/// <param name="source">The object with the event.</param>
	/// <param name="listener">The object to add as a listener.</param>
	/// <param name="propertyName">The name of the property that exists on <paramref name="source" /> upon which to listen for changes. Set to <see cref="F:System.String.Empty" /> to indicate "any property".</param>
	public static void AddListener(INotifyPropertyChanged source, IWeakEventListener listener, string propertyName)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		CurrentManager.PrivateAddListener(source, listener, propertyName);
	}

	/// <summary>Removes the specified listener from the list of listeners on the provided source.</summary>
	/// <param name="source">The object to remove the listener from.</param>
	/// <param name="listener">The listener to remove.</param>
	/// <param name="propertyName">The name of the property that exists on <paramref name="source" /> upon which to stop listening for changes. Set to <see cref="F:System.String.Empty" /> to indicate "any property".</param>
	public static void RemoveListener(INotifyPropertyChanged source, IWeakEventListener listener, string propertyName)
	{
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		CurrentManager.PrivateRemoveListener(source, listener, propertyName);
	}

	/// <summary>Adds the specified event handler, which is called when specified source raises the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> event for the specified property.</summary>
	/// <param name="source">The source object that the raises the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> event.</param>
	/// <param name="handler">The delegate that handles the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> event.</param>
	/// <param name="propertyName">The name of the property that exists on <paramref name="source" /> upon which to listen for changes. Set to <see cref="F:System.String.Empty" /> to indicate "any property".</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="handler" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">More than one method is associated with <paramref name="handler" />.</exception>
	public static void AddHandler(INotifyPropertyChanged source, EventHandler<PropertyChangedEventArgs> handler, string propertyName)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.PrivateAddHandler(source, handler, propertyName);
	}

	/// <summary>Removes the specified event handler from the specified source.</summary>
	/// <param name="source">The source object that the raises the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> event.</param>
	/// <param name="handler">The delegate that handles the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> event.</param>
	/// <param name="propertyName">The name of the property that exists on <paramref name="source" /> upon which to stop listening for changes. Set to <see cref="F:System.String.Empty" /> to indicate "any property".</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="handler" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">More than one method is associated with <paramref name="handler" />.</exception>
	public static void RemoveHandler(INotifyPropertyChanged source, EventHandler<PropertyChangedEventArgs> handler, string propertyName)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.PrivateRemoveHandler(source, handler, propertyName);
	}

	/// <summary>Returns a new object to contain listeners to the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> event.</summary>
	/// <returns>A new object to contain listeners to the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> event.</returns>
	protected override ListenerList NewListenerList()
	{
		return new ListenerList<PropertyChangedEventArgs>();
	}

	/// <summary>Begins listening for the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> event on the provided source.</summary>
	/// <param name="source">The object on which to start listening for <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" />.</param>
	protected override void StartListening(object source)
	{
		((INotifyPropertyChanged)source).PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Stops listening for the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> event on the provided source.</summary>
	/// <param name="source">The source object on which to stop listening for <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" />.</param>
	protected override void StopListening(object source)
	{
		((INotifyPropertyChanged)source).PropertyChanged -= OnPropertyChanged;
	}

	/// <summary>Removes listeners that are no longer active from the data for the provided source. </summary>
	/// <returns>true if some entries were removed; otherwise, false.</returns>
	/// <param name="source">Source object to remove all listeners from.</param>
	/// <param name="data">The data to purge. This is expected to be a <see cref="T:System.Collections.Specialized.HybridDictionary" />.</param>
	/// <param name="purgeAll">Set to true to stop listening and to completely remove all data.</param>
	protected override bool Purge(object source, object data, bool purgeAll)
	{
		bool flag = false;
		if (!purgeAll)
		{
			HybridDictionary hybridDictionary = (HybridDictionary)data;
			int num = 0;
			if (!BaseAppContextSwitches.EnableWeakEventMemoryImprovements)
			{
				ICollection keys = hybridDictionary.Keys;
				string[] array = new string[keys.Count];
				keys.CopyTo(array, 0);
				for (int num2 = array.Length - 1; num2 >= 0; num2--)
				{
					if (array[num2] == AllListenersKey)
					{
						num++;
					}
					else
					{
						bool flag2 = source == null;
						if (!flag2)
						{
							ListenerList list = (ListenerList)hybridDictionary[array[num2]];
							if (ListenerList.PrepareForWriting(ref list))
							{
								hybridDictionary[array[num2]] = list;
							}
							if (list.Purge())
							{
								flag = true;
							}
							flag2 = list.IsEmpty;
						}
						if (flag2)
						{
							hybridDictionary.Remove(array[num2]);
						}
					}
				}
			}
			else
			{
				HybridDictionary hybridDictionary2 = null;
				IDictionaryEnumerator enumerator = hybridDictionary.GetEnumerator();
				while (enumerator.MoveNext())
				{
					string text = (string)enumerator.Key;
					if (text == AllListenersKey)
					{
						num++;
						continue;
					}
					bool flag3 = source == null;
					if (!flag3)
					{
						ListenerList list2 = (ListenerList)enumerator.Value;
						bool num3 = ListenerList.PrepareForWriting(ref list2);
						bool flag4 = false;
						if (list2.Purge())
						{
							flag4 = true;
							flag = true;
						}
						if (num3 && flag4)
						{
							if (hybridDictionary2 == null)
							{
								hybridDictionary2 = new HybridDictionary();
							}
							hybridDictionary2[text] = list2;
						}
						flag3 = list2.IsEmpty;
					}
					if (flag3)
					{
						_toRemove.Add(text);
					}
				}
				if (_toRemove.Count > 0)
				{
					foreach (string item in _toRemove)
					{
						hybridDictionary.Remove(item);
					}
					_toRemove.Clear();
					_toRemove.TrimExcess();
				}
				if (hybridDictionary2 != null)
				{
					IDictionaryEnumerator enumerator3 = hybridDictionary2.GetEnumerator();
					while (enumerator3.MoveNext())
					{
						string key = (string)enumerator3.Key;
						ListenerList value = (ListenerList)enumerator3.Value;
						hybridDictionary[key] = value;
					}
				}
			}
			if (hybridDictionary.Count == num)
			{
				purgeAll = true;
				if (source != null)
				{
					Remove(source);
				}
			}
			else if (flag)
			{
				hybridDictionary.Remove(AllListenersKey);
				_proposedAllListenersList = null;
			}
		}
		if (purgeAll)
		{
			if (source != null)
			{
				StopListening(source);
			}
			flag = true;
		}
		return flag;
	}

	private void PrivateAddListener(INotifyPropertyChanged source, IWeakEventListener listener, string propertyName)
	{
		AddListener(source, propertyName, listener, null);
	}

	private void PrivateRemoveListener(INotifyPropertyChanged source, IWeakEventListener listener, string propertyName)
	{
		RemoveListener(source, propertyName, listener, null);
	}

	private void PrivateAddHandler(INotifyPropertyChanged source, EventHandler<PropertyChangedEventArgs> handler, string propertyName)
	{
		AddListener(source, propertyName, null, handler);
	}

	private void PrivateRemoveHandler(INotifyPropertyChanged source, EventHandler<PropertyChangedEventArgs> handler, string propertyName)
	{
		RemoveListener(source, propertyName, null, handler);
	}

	private void AddListener(INotifyPropertyChanged source, string propertyName, IWeakEventListener listener, EventHandler<PropertyChangedEventArgs> handler)
	{
		using (base.WriteLock)
		{
			HybridDictionary hybridDictionary = (HybridDictionary)base[source];
			if (hybridDictionary == null)
			{
				hybridDictionary = (HybridDictionary)(base[source] = new HybridDictionary(caseInsensitive: true));
				StartListening(source);
			}
			ListenerList list = (ListenerList)hybridDictionary[propertyName];
			if (list == null)
			{
				list = (ListenerList)(hybridDictionary[propertyName] = new ListenerList<PropertyChangedEventArgs>());
			}
			if (ListenerList.PrepareForWriting(ref list))
			{
				hybridDictionary[propertyName] = list;
			}
			if (handler != null)
			{
				((ListenerList<PropertyChangedEventArgs>)list).AddHandler(handler);
			}
			else
			{
				list.Add(listener);
			}
			hybridDictionary.Remove(AllListenersKey);
			_proposedAllListenersList = null;
			ScheduleCleanup();
		}
	}

	private void RemoveListener(INotifyPropertyChanged source, string propertyName, IWeakEventListener listener, EventHandler<PropertyChangedEventArgs> handler)
	{
		using (base.WriteLock)
		{
			HybridDictionary hybridDictionary = (HybridDictionary)base[source];
			if (hybridDictionary == null)
			{
				return;
			}
			ListenerList list = (ListenerList)hybridDictionary[propertyName];
			if (list != null)
			{
				if (ListenerList.PrepareForWriting(ref list))
				{
					hybridDictionary[propertyName] = list;
				}
				if (handler != null)
				{
					((ListenerList<PropertyChangedEventArgs>)list).RemoveHandler(handler);
				}
				else
				{
					list.Remove(listener);
				}
				if (list.IsEmpty)
				{
					hybridDictionary.Remove(propertyName);
				}
			}
			if (hybridDictionary.Count == 0)
			{
				StopListening(source);
				Remove(source);
			}
			hybridDictionary.Remove(AllListenersKey);
			_proposedAllListenersList = null;
		}
	}

	private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
	{
		string propertyName = args.PropertyName;
		ListenerList listenerList;
		using (base.ReadLock)
		{
			HybridDictionary hybridDictionary = (HybridDictionary)base[sender];
			if (hybridDictionary == null)
			{
				listenerList = ListenerList.Empty;
			}
			else if (!string.IsNullOrEmpty(propertyName))
			{
				ListenerList<PropertyChangedEventArgs> listenerList2 = (ListenerList<PropertyChangedEventArgs>)hybridDictionary[propertyName];
				ListenerList<PropertyChangedEventArgs> listenerList3 = (ListenerList<PropertyChangedEventArgs>)hybridDictionary[string.Empty];
				if (listenerList3 == null)
				{
					listenerList = ((listenerList2 == null) ? ListenerList.Empty : listenerList2);
				}
				else if (listenerList2 != null)
				{
					listenerList = new ListenerList<PropertyChangedEventArgs>(listenerList2.Count + listenerList3.Count);
					int i = 0;
					for (int count = listenerList2.Count; i < count; i++)
					{
						listenerList.Add(listenerList2.GetListener(i));
					}
					int j = 0;
					for (int count2 = listenerList3.Count; j < count2; j++)
					{
						listenerList.Add(listenerList3.GetListener(j));
					}
				}
				else
				{
					listenerList = listenerList3;
				}
			}
			else
			{
				listenerList = (ListenerList)hybridDictionary[AllListenersKey];
				if (listenerList == null)
				{
					int num = 0;
					foreach (DictionaryEntry item in hybridDictionary)
					{
						num += ((ListenerList)item.Value).Count;
					}
					listenerList = new ListenerList<PropertyChangedEventArgs>(num);
					foreach (DictionaryEntry item2 in hybridDictionary)
					{
						ListenerList listenerList4 = (ListenerList)item2.Value;
						int k = 0;
						for (int count3 = listenerList4.Count; k < count3; k++)
						{
							listenerList.Add(listenerList4.GetListener(k));
						}
					}
					_proposedAllListenersList = listenerList;
				}
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
		if (_proposedAllListenersList != listenerList)
		{
			return;
		}
		using (base.WriteLock)
		{
			if (_proposedAllListenersList == listenerList)
			{
				HybridDictionary hybridDictionary2 = (HybridDictionary)base[sender];
				if (hybridDictionary2 != null)
				{
					hybridDictionary2[AllListenersKey] = listenerList;
				}
				_proposedAllListenersList = null;
			}
		}
	}
}
