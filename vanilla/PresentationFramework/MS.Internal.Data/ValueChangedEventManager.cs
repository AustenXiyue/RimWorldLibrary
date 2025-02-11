using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace MS.Internal.Data;

internal class ValueChangedEventManager : WeakEventManager
{
	private class ValueChangedRecord
	{
		private PropertyDescriptor _pd;

		private ValueChangedEventManager _manager;

		private object _source;

		private ListenerList<ValueChangedEventArgs> _listeners = new ListenerList<ValueChangedEventArgs>();

		private ValueChangedEventArgs _eventArgs;

		public bool IsEmpty
		{
			get
			{
				bool flag = _listeners.IsEmpty;
				if (!flag && HasIgnorableListeners)
				{
					flag = true;
					int i = 0;
					for (int count = _listeners.Count; i < count; i++)
					{
						if (!IsIgnorable(_listeners.GetListener(i).Target))
						{
							flag = false;
							break;
						}
					}
				}
				return flag;
			}
		}

		private bool HasIgnorableListeners { get; set; }

		public ValueChangedRecord(ValueChangedEventManager manager, object source, PropertyDescriptor pd)
		{
			_manager = manager;
			_source = source;
			_pd = pd;
			_eventArgs = new ValueChangedEventArgs(pd);
			pd.AddValueChanged(source, OnValueChanged);
		}

		public void Add(IWeakEventListener listener, EventHandler<ValueChangedEventArgs> handler)
		{
			ListenerList list = _listeners;
			if (ListenerList.PrepareForWriting(ref list))
			{
				_listeners = (ListenerList<ValueChangedEventArgs>)list;
			}
			if (handler != null)
			{
				_listeners.AddHandler(handler);
				if (!HasIgnorableListeners && IsIgnorable(handler.Target))
				{
					HasIgnorableListeners = true;
				}
			}
			else
			{
				_listeners.Add(listener);
			}
		}

		public void Remove(IWeakEventListener listener, EventHandler<ValueChangedEventArgs> handler)
		{
			ListenerList list = _listeners;
			if (ListenerList.PrepareForWriting(ref list))
			{
				_listeners = (ListenerList<ValueChangedEventArgs>)list;
			}
			if (handler != null)
			{
				_listeners.RemoveHandler(handler);
			}
			else
			{
				_listeners.Remove(listener);
			}
			if (IsEmpty)
			{
				StopListening();
			}
		}

		public bool Purge()
		{
			ListenerList list = _listeners;
			if (ListenerList.PrepareForWriting(ref list))
			{
				_listeners = (ListenerList<ValueChangedEventArgs>)list;
			}
			return _listeners.Purge();
		}

		public void StopListening()
		{
			if (_source != null)
			{
				_pd.RemoveValueChanged(_source, OnValueChanged);
				_source = null;
			}
		}

		private void OnValueChanged(object sender, EventArgs e)
		{
			using (_manager.ReadLock)
			{
				_listeners.BeginUse();
			}
			try
			{
				_manager.DeliverEventToList(sender, _eventArgs, _listeners);
			}
			finally
			{
				_listeners.EndUse();
			}
		}

		private bool IsIgnorable(object target)
		{
			return target is ValueTable;
		}
	}

	private List<PropertyDescriptor> _toRemove = new List<PropertyDescriptor>();

	private static ValueChangedEventManager CurrentManager
	{
		get
		{
			Type typeFromHandle = typeof(ValueChangedEventManager);
			ValueChangedEventManager valueChangedEventManager = (ValueChangedEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
			if (valueChangedEventManager == null)
			{
				valueChangedEventManager = new ValueChangedEventManager();
				WeakEventManager.SetCurrentManager(typeFromHandle, valueChangedEventManager);
			}
			return valueChangedEventManager;
		}
	}

	private ValueChangedEventManager()
	{
	}

	public static void AddListener(object source, IWeakEventListener listener, PropertyDescriptor pd)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		CurrentManager.PrivateAddListener(source, listener, pd);
	}

	public static void RemoveListener(object source, IWeakEventListener listener, PropertyDescriptor pd)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		CurrentManager.PrivateRemoveListener(source, listener, pd);
	}

	public static void AddHandler(object source, EventHandler<ValueChangedEventArgs> handler, PropertyDescriptor pd)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		if (handler.GetInvocationList().Length != 1)
		{
			throw new NotSupportedException(SR.NoMulticastHandlers);
		}
		CurrentManager.PrivateAddHandler(source, handler, pd);
	}

	public static void RemoveHandler(object source, EventHandler<ValueChangedEventArgs> handler, PropertyDescriptor pd)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		if (handler.GetInvocationList().Length != 1)
		{
			throw new NotSupportedException(SR.NoMulticastHandlers);
		}
		CurrentManager.PrivateRemoveHandler(source, handler, pd);
	}

	protected override ListenerList NewListenerList()
	{
		return new ListenerList<ValueChangedEventArgs>();
	}

	protected override void StartListening(object source)
	{
	}

	protected override void StopListening(object source)
	{
	}

	protected override bool Purge(object source, object data, bool purgeAll)
	{
		bool result = false;
		HybridDictionary hybridDictionary = (HybridDictionary)data;
		if (!BaseAppContextSwitches.EnableWeakEventMemoryImprovements)
		{
			ICollection keys = hybridDictionary.Keys;
			PropertyDescriptor[] array = new PropertyDescriptor[keys.Count];
			keys.CopyTo(array, 0);
			for (int num = array.Length - 1; num >= 0; num--)
			{
				bool flag = purgeAll || source == null;
				ValueChangedRecord valueChangedRecord = (ValueChangedRecord)hybridDictionary[array[num]];
				if (!flag)
				{
					if (valueChangedRecord.Purge())
					{
						result = true;
					}
					flag = valueChangedRecord.IsEmpty;
				}
				if (flag)
				{
					valueChangedRecord.StopListening();
					if (!purgeAll)
					{
						hybridDictionary.Remove(array[num]);
					}
				}
			}
		}
		else
		{
			IDictionaryEnumerator enumerator = hybridDictionary.GetEnumerator();
			while (enumerator.MoveNext())
			{
				bool flag2 = purgeAll || source == null;
				ValueChangedRecord valueChangedRecord2 = (ValueChangedRecord)enumerator.Value;
				if (!flag2)
				{
					if (valueChangedRecord2.Purge())
					{
						result = true;
					}
					flag2 = valueChangedRecord2.IsEmpty;
				}
				if (flag2)
				{
					valueChangedRecord2.StopListening();
					if (!purgeAll)
					{
						_toRemove.Add((PropertyDescriptor)enumerator.Key);
					}
				}
			}
			if (_toRemove.Count > 0)
			{
				foreach (PropertyDescriptor item in _toRemove)
				{
					hybridDictionary.Remove(item);
				}
				_toRemove.Clear();
				_toRemove.TrimExcess();
			}
		}
		if (hybridDictionary.Count == 0)
		{
			result = true;
			if (source != null)
			{
				Remove(source);
			}
		}
		return result;
	}

	private void PrivateAddListener(object source, IWeakEventListener listener, PropertyDescriptor pd)
	{
		AddListener(source, pd, listener, null);
	}

	private void PrivateRemoveListener(object source, IWeakEventListener listener, PropertyDescriptor pd)
	{
		RemoveListener(source, pd, listener, null);
	}

	private void PrivateAddHandler(object source, EventHandler<ValueChangedEventArgs> handler, PropertyDescriptor pd)
	{
		AddListener(source, pd, null, handler);
	}

	private void PrivateRemoveHandler(object source, EventHandler<ValueChangedEventArgs> handler, PropertyDescriptor pd)
	{
		RemoveListener(source, pd, null, handler);
	}

	private void AddListener(object source, PropertyDescriptor pd, IWeakEventListener listener, EventHandler<ValueChangedEventArgs> handler)
	{
		using (base.WriteLock)
		{
			HybridDictionary hybridDictionary = (HybridDictionary)base[source];
			if (hybridDictionary == null)
			{
				hybridDictionary = (HybridDictionary)(base[source] = new HybridDictionary());
			}
			ValueChangedRecord valueChangedRecord = (ValueChangedRecord)hybridDictionary[pd];
			if (valueChangedRecord == null)
			{
				valueChangedRecord = (ValueChangedRecord)(hybridDictionary[pd] = new ValueChangedRecord(this, source, pd));
			}
			valueChangedRecord.Add(listener, handler);
			ScheduleCleanup();
		}
	}

	private void RemoveListener(object source, PropertyDescriptor pd, IWeakEventListener listener, EventHandler<ValueChangedEventArgs> handler)
	{
		using (base.WriteLock)
		{
			HybridDictionary hybridDictionary = (HybridDictionary)base[source];
			if (hybridDictionary == null)
			{
				return;
			}
			ValueChangedRecord valueChangedRecord = (ValueChangedRecord)hybridDictionary[pd];
			if (valueChangedRecord != null)
			{
				valueChangedRecord.Remove(listener, handler);
				if (valueChangedRecord.IsEmpty)
				{
					hybridDictionary.Remove(pd);
				}
			}
			if (hybridDictionary.Count == 0)
			{
				Remove(source);
			}
		}
	}
}
