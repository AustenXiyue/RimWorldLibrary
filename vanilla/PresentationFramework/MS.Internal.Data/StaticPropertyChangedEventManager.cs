using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace MS.Internal.Data;

internal class StaticPropertyChangedEventManager : WeakEventManager
{
	private class TypeRecord
	{
		private Type _type;

		private HybridDictionary _dict;

		private StaticPropertyChangedEventManager _manager;

		private ListenerList<PropertyChangedEventArgs> _proposedAllListenersList;

		private List<string> _toRemove = new List<string>();

		public Type Type => _type;

		public bool IsEmpty => _dict.Count == 0;

		public ListenerList ProposedAllListenersList => _proposedAllListenersList;

		private static MethodInfo OnStaticPropertyChangedMethodInfo => typeof(TypeRecord).GetMethod("OnStaticPropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);

		public TypeRecord(Type type, StaticPropertyChangedEventManager manager)
		{
			_type = type;
			_manager = manager;
			_dict = new HybridDictionary(caseInsensitive: true);
		}

		public void StartListening()
		{
			EventInfo @event = _type.GetEvent(StaticPropertyChanged, BindingFlags.Static | BindingFlags.Public);
			if (@event != null)
			{
				Delegate handler = Delegate.CreateDelegate(@event.EventHandlerType, this, OnStaticPropertyChangedMethodInfo);
				@event.AddEventHandler(null, handler);
			}
		}

		public void StopListening()
		{
			EventInfo @event = _type.GetEvent(StaticPropertyChanged, BindingFlags.Static | BindingFlags.Public);
			if (@event != null)
			{
				Delegate handler = Delegate.CreateDelegate(@event.EventHandlerType, this, OnStaticPropertyChangedMethodInfo);
				@event.RemoveEventHandler(null, handler);
			}
		}

		private void OnStaticPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			HandleStaticPropertyChanged(e);
		}

		public void HandleStaticPropertyChanged(PropertyChangedEventArgs e)
		{
			_manager.OnStaticPropertyChanged(this, e);
		}

		public void AddHandler(EventHandler<PropertyChangedEventArgs> handler, string propertyName)
		{
			PropertyRecord propertyRecord = (PropertyRecord)_dict[propertyName];
			if (propertyRecord == null)
			{
				propertyRecord = new PropertyRecord(propertyName, this);
				_dict[propertyName] = propertyRecord;
				propertyRecord.StartListening(_type);
			}
			propertyRecord.AddHandler(handler);
			_dict.Remove(AllListenersKey);
			_proposedAllListenersList = null;
			_manager.ScheduleCleanup();
		}

		public void RemoveHandler(EventHandler<PropertyChangedEventArgs> handler, string propertyName)
		{
			PropertyRecord propertyRecord = (PropertyRecord)_dict[propertyName];
			if (propertyRecord != null)
			{
				propertyRecord.RemoveHandler(handler);
				if (propertyRecord.IsEmpty)
				{
					_dict.Remove(propertyName);
				}
				_dict.Remove(AllListenersKey);
				_proposedAllListenersList = null;
			}
		}

		public ListenerList GetListenerList(string propertyName)
		{
			ListenerList listenerList3;
			if (!string.IsNullOrEmpty(propertyName))
			{
				ListenerList<PropertyChangedEventArgs> listenerList = ((PropertyRecord)_dict[propertyName])?.List;
				ListenerList<PropertyChangedEventArgs> listenerList2 = ((PropertyRecord)_dict[string.Empty])?.List;
				if (listenerList2 == null)
				{
					listenerList3 = ((listenerList == null) ? ListenerList.Empty : listenerList);
				}
				else if (listenerList != null)
				{
					listenerList3 = new ListenerList<PropertyChangedEventArgs>(listenerList.Count + listenerList2.Count);
					int i = 0;
					for (int count = listenerList.Count; i < count; i++)
					{
						listenerList3.Add(listenerList[i]);
					}
					int j = 0;
					for (int count2 = listenerList2.Count; j < count2; j++)
					{
						listenerList3.Add(listenerList2[j]);
					}
				}
				else
				{
					listenerList3 = listenerList2;
				}
			}
			else
			{
				ListenerList<PropertyChangedEventArgs> listenerList4 = ((PropertyRecord)_dict[AllListenersKey])?.List;
				if (listenerList4 == null)
				{
					int num = 0;
					foreach (DictionaryEntry item in _dict)
					{
						num += ((PropertyRecord)item.Value).List.Count;
					}
					listenerList4 = new ListenerList<PropertyChangedEventArgs>(num);
					foreach (DictionaryEntry item2 in _dict)
					{
						ListenerList list = ((PropertyRecord)item2.Value).List;
						int k = 0;
						for (int count3 = list.Count; k < count3; k++)
						{
							listenerList4.Add(list.GetListener(k));
						}
					}
					_proposedAllListenersList = listenerList4;
				}
				listenerList3 = listenerList4;
			}
			return listenerList3;
		}

		public void StoreAllListenersList(ListenerList<PropertyChangedEventArgs> list)
		{
			if (_proposedAllListenersList == list)
			{
				_dict[AllListenersKey] = new PropertyRecord(AllListenersKey, this, list);
				_proposedAllListenersList = null;
			}
		}

		public bool Purge(bool purgeAll)
		{
			bool flag = false;
			if (!purgeAll)
			{
				if (!BaseAppContextSwitches.EnableWeakEventMemoryImprovements)
				{
					ICollection keys = _dict.Keys;
					string[] array = new string[keys.Count];
					keys.CopyTo(array, 0);
					for (int num = array.Length - 1; num >= 0; num--)
					{
						if (!(array[num] == AllListenersKey))
						{
							PropertyRecord propertyRecord = (PropertyRecord)_dict[array[num]];
							if (propertyRecord.Purge())
							{
								flag = true;
							}
							if (propertyRecord.IsEmpty)
							{
								propertyRecord.StopListening(_type);
								_dict.Remove(array[num]);
							}
						}
					}
				}
				else
				{
					IDictionaryEnumerator enumerator = _dict.GetEnumerator();
					while (enumerator.MoveNext())
					{
						string text = (string)enumerator.Key;
						if (!(text == AllListenersKey))
						{
							PropertyRecord propertyRecord2 = (PropertyRecord)enumerator.Value;
							if (propertyRecord2.Purge())
							{
								flag = true;
							}
							if (propertyRecord2.IsEmpty)
							{
								propertyRecord2.StopListening(_type);
								_toRemove.Add(text);
							}
						}
					}
					if (_toRemove.Count > 0)
					{
						foreach (string item in _toRemove)
						{
							_dict.Remove(item);
						}
						_toRemove.Clear();
						_toRemove.TrimExcess();
					}
				}
				if (flag)
				{
					_dict.Remove(AllListenersKey);
					_proposedAllListenersList = null;
				}
				if (IsEmpty)
				{
					StopListening();
				}
			}
			else
			{
				flag = true;
				StopListening();
				foreach (DictionaryEntry item2 in _dict)
				{
					((PropertyRecord)item2.Value).StopListening(_type);
				}
			}
			return flag;
		}
	}

	private class PropertyRecord
	{
		private string _propertyName;

		private ListenerList<PropertyChangedEventArgs> _list;

		private TypeRecord _typeRecord;

		public bool IsEmpty => _list.IsEmpty;

		public ListenerList<PropertyChangedEventArgs> List => _list;

		private static MethodInfo OnStaticPropertyChangedMethodInfo => typeof(PropertyRecord).GetMethod("OnStaticPropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);

		public PropertyRecord(string propertyName, TypeRecord owner)
			: this(propertyName, owner, new ListenerList<PropertyChangedEventArgs>())
		{
		}

		public PropertyRecord(string propertyName, TypeRecord owner, ListenerList<PropertyChangedEventArgs> list)
		{
			_propertyName = propertyName;
			_typeRecord = owner;
			_list = list;
		}

		public void StartListening(Type type)
		{
			string name = _propertyName + "Changed";
			EventInfo @event = type.GetEvent(name, BindingFlags.Static | BindingFlags.Public);
			if (@event != null)
			{
				Delegate handler = Delegate.CreateDelegate(@event.EventHandlerType, this, OnStaticPropertyChangedMethodInfo);
				@event.AddEventHandler(null, handler);
			}
		}

		public void StopListening(Type type)
		{
			string name = _propertyName + "Changed";
			EventInfo @event = type.GetEvent(name, BindingFlags.Static | BindingFlags.Public);
			if (@event != null)
			{
				Delegate handler = Delegate.CreateDelegate(@event.EventHandlerType, this, OnStaticPropertyChangedMethodInfo);
				@event.RemoveEventHandler(null, handler);
			}
		}

		private void OnStaticPropertyChanged(object sender, EventArgs e)
		{
			_typeRecord.HandleStaticPropertyChanged(new PropertyChangedEventArgs(_propertyName));
		}

		public void AddHandler(EventHandler<PropertyChangedEventArgs> handler)
		{
			ListenerList list = _list;
			if (ListenerList.PrepareForWriting(ref list))
			{
				_list = (ListenerList<PropertyChangedEventArgs>)list;
			}
			_list.AddHandler(handler);
		}

		public void RemoveHandler(EventHandler<PropertyChangedEventArgs> handler)
		{
			ListenerList list = _list;
			if (ListenerList.PrepareForWriting(ref list))
			{
				_list = (ListenerList<PropertyChangedEventArgs>)list;
			}
			_list.RemoveHandler(handler);
		}

		public bool Purge()
		{
			ListenerList list = _list;
			if (ListenerList.PrepareForWriting(ref list))
			{
				_list = (ListenerList<PropertyChangedEventArgs>)list;
			}
			return _list.Purge();
		}
	}

	private static readonly string AllListenersKey = "<All Listeners>";

	private static readonly string StaticPropertyChanged = "StaticPropertyChanged";

	private static StaticPropertyChangedEventManager CurrentManager
	{
		get
		{
			Type typeFromHandle = typeof(StaticPropertyChangedEventManager);
			StaticPropertyChangedEventManager staticPropertyChangedEventManager = (StaticPropertyChangedEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
			if (staticPropertyChangedEventManager == null)
			{
				staticPropertyChangedEventManager = new StaticPropertyChangedEventManager();
				WeakEventManager.SetCurrentManager(typeFromHandle, staticPropertyChangedEventManager);
			}
			return staticPropertyChangedEventManager;
		}
	}

	private StaticPropertyChangedEventManager()
	{
	}

	public static void AddHandler(Type type, EventHandler<PropertyChangedEventArgs> handler, string propertyName)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.PrivateAddHandler(type, handler, propertyName);
	}

	public static void RemoveHandler(Type type, EventHandler<PropertyChangedEventArgs> handler, string propertyName)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		CurrentManager.PrivateRemoveHandler(type, handler, propertyName);
	}

	protected override ListenerList NewListenerList()
	{
		return new ListenerList<PropertyChangedEventArgs>();
	}

	protected override void StartListening(object source)
	{
	}

	protected override void StopListening(object source)
	{
	}

	protected override bool Purge(object source, object data, bool purgeAll)
	{
		TypeRecord typeRecord = (TypeRecord)data;
		bool result = typeRecord.Purge(purgeAll);
		if (!purgeAll && typeRecord.IsEmpty)
		{
			Remove(typeRecord.Type);
		}
		return result;
	}

	private void PrivateAddHandler(Type type, EventHandler<PropertyChangedEventArgs> handler, string propertyName)
	{
		using (base.WriteLock)
		{
			TypeRecord typeRecord = (TypeRecord)base[type];
			if (typeRecord == null)
			{
				typeRecord = (TypeRecord)(base[type] = new TypeRecord(type, this));
				typeRecord.StartListening();
			}
			typeRecord.AddHandler(handler, propertyName);
		}
	}

	private void PrivateRemoveHandler(Type type, EventHandler<PropertyChangedEventArgs> handler, string propertyName)
	{
		using (base.WriteLock)
		{
			TypeRecord typeRecord = (TypeRecord)base[type];
			if (typeRecord != null)
			{
				typeRecord.RemoveHandler(handler, propertyName);
				if (typeRecord.IsEmpty)
				{
					typeRecord.StopListening();
					Remove(typeRecord.Type);
				}
			}
		}
	}

	private void OnStaticPropertyChanged(TypeRecord typeRecord, PropertyChangedEventArgs args)
	{
		ListenerList listenerList;
		using (base.ReadLock)
		{
			listenerList = typeRecord.GetListenerList(args.PropertyName);
			listenerList.BeginUse();
		}
		try
		{
			DeliverEventToList(null, args, listenerList);
		}
		finally
		{
			listenerList.EndUse();
		}
		if (listenerList == typeRecord.ProposedAllListenersList)
		{
			using (base.WriteLock)
			{
				typeRecord.StoreAllListenersList((ListenerList<PropertyChangedEventArgs>)listenerList);
			}
		}
	}
}
