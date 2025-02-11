using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class ViewManager : HybridDictionary
{
	private const int InactivityThreshold = 2;

	private HybridDictionary _inactiveViewTables = new HybridDictionary();

	private static object StaticObject = new object();

	internal static WeakReference StaticWeakRef = new WeakReference(StaticObject);

	internal static WeakReference NullWeakRef = new WeakReference(null);

	public new CollectionRecord this[object o]
	{
		get
		{
			WeakRefKey weakRefKey = new WeakRefKey(o);
			return (CollectionRecord)base[weakRefKey];
		}
	}

	internal static ViewManager Current => DataBindEngine.CurrentDataBindEngine.ViewManager;

	internal void Add(object collection, CollectionRecord cr)
	{
		Add((object)new WeakRefKey(collection), (object?)cr);
		DataBindEngine.CurrentDataBindEngine.ScheduleCleanup();
	}

	internal ViewRecord GetViewRecord(object collection, CollectionViewSource cvs, Type collectionViewType, bool createView, Func<object, object> GetSourceItem)
	{
		ViewRecord viewRecord = GetExistingView(collection, cvs, collectionViewType, GetSourceItem);
		if (viewRecord != null || !createView)
		{
			return viewRecord;
		}
		IListSource listSource = collection as IListSource;
		IList list = null;
		if (listSource != null)
		{
			list = listSource.GetList();
			viewRecord = GetExistingView(list, cvs, collectionViewType, GetSourceItem);
			if (viewRecord != null)
			{
				return CacheView(collection, cvs, (CollectionView)viewRecord.View, viewRecord);
			}
		}
		ICollectionView collectionView = collection as ICollectionView;
		if (collectionView != null)
		{
			collectionView = new CollectionViewProxy(collectionView);
		}
		else if (collectionViewType == null)
		{
			if (collection is ICollectionViewFactory collectionViewFactory)
			{
				collectionView = collectionViewFactory.CreateView();
			}
			else
			{
				IList list2 = ((list != null) ? list : (collection as IList));
				if (list2 != null)
				{
					collectionView = ((!(list2 is IBindingList bindingList)) ? ((ICollectionView)new ListCollectionView(list2)) : ((ICollectionView)new BindingListCollectionView(bindingList)));
				}
				else if (collection is IEnumerable source)
				{
					collectionView = new EnumerableCollectionView(source);
				}
			}
		}
		else
		{
			if (!typeof(ICollectionView).IsAssignableFrom(collectionViewType))
			{
				throw new ArgumentException(SR.Format(SR.CollectionView_WrongType, collectionViewType.Name));
			}
			object obj = ((list != null) ? list : collection);
			try
			{
				collectionView = Activator.CreateInstance(collectionViewType, BindingFlags.CreateInstance, null, new object[1] { obj }, null) as ICollectionView;
			}
			catch (MissingMethodException innerException)
			{
				throw new ArgumentException(SR.Format(SR.CollectionView_ViewTypeInsufficient, collectionViewType.Name, collection.GetType()), innerException);
			}
		}
		if (collectionView != null)
		{
			CollectionView collectionView2 = collectionView as CollectionView;
			if (collectionView2 == null)
			{
				collectionView2 = new CollectionViewProxy(collectionView);
			}
			if (list != null)
			{
				viewRecord = CacheView(list, cvs, collectionView2, null);
			}
			viewRecord = CacheView(collection, cvs, collectionView2, viewRecord);
			BindingOperations.OnCollectionViewRegistering(collectionView2);
		}
		return viewRecord;
	}

	private CollectionRecord EnsureCollectionRecord(object collection, Func<object, object> GetSourceItem = null)
	{
		CollectionRecord collectionRecord = this[collection];
		if (collectionRecord == null)
		{
			collectionRecord = new CollectionRecord();
			Add(collection, collectionRecord);
			object parent = GetSourceItem?.Invoke(collection);
			if (collection is IEnumerable collection2)
			{
				BindingOperations.OnCollectionRegistering(collection2, parent);
			}
		}
		return collectionRecord;
	}

	internal void RegisterCollectionSynchronizationCallback(IEnumerable collection, object context, CollectionSynchronizationCallback synchronizationCallback)
	{
		CollectionRecord collectionRecord = EnsureCollectionRecord(collection);
		collectionRecord.SynchronizationInfo = new SynchronizationInfo(context, synchronizationCallback);
		ViewTable viewTable = collectionRecord.ViewTable;
		if (viewTable == null)
		{
			return;
		}
		bool isSynchronized = collectionRecord.SynchronizationInfo.IsSynchronized;
		foreach (DictionaryEntry item in viewTable)
		{
			if (((ViewRecord)item.Value).View is CollectionView collectionView)
			{
				collectionView.SetAllowsCrossThreadChanges(isSynchronized);
			}
		}
	}

	internal SynchronizationInfo GetSynchronizationInfo(IEnumerable collection)
	{
		return this[collection]?.SynchronizationInfo ?? SynchronizationInfo.None;
	}

	public void AccessCollection(IEnumerable collection, Action accessMethod, bool writeAccess)
	{
		GetSynchronizationInfo(collection).AccessCollection(collection, accessMethod, writeAccess);
	}

	private ViewRecord GetExistingView(object collection, CollectionViewSource cvs, Type collectionViewType, Func<object, object> GetSourceItem)
	{
		CollectionView collectionView = collection as CollectionView;
		ViewRecord result;
		if (collectionView == null)
		{
			ViewTable viewTable = EnsureCollectionRecord(collection, GetSourceItem).ViewTable;
			if (viewTable != null)
			{
				ViewRecord viewRecord = viewTable[cvs];
				if (viewRecord != null)
				{
					collectionView = (CollectionView)viewRecord.View;
				}
				result = viewRecord;
				if (_inactiveViewTables.Contains(viewTable))
				{
					_inactiveViewTables[viewTable] = 2;
				}
			}
			else
			{
				result = null;
			}
		}
		else
		{
			result = new ViewRecord(collectionView);
		}
		if (collectionView != null)
		{
			ValidateViewType(collectionView, collectionViewType);
		}
		return result;
	}

	private ViewRecord CacheView(object collection, CollectionViewSource cvs, CollectionView cv, ViewRecord vr)
	{
		CollectionRecord collectionRecord = this[collection];
		ViewTable viewTable = collectionRecord.ViewTable;
		if (viewTable == null)
		{
			viewTable = (collectionRecord.ViewTable = new ViewTable());
			if (!(collection is INotifyCollectionChanged))
			{
				_inactiveViewTables.Add(viewTable, 2);
			}
		}
		if (vr == null)
		{
			vr = new ViewRecord(cv);
		}
		else if (cv == null)
		{
			cv = (CollectionView)vr.View;
		}
		cv.SetViewManagerData(viewTable);
		viewTable[cvs] = vr;
		return vr;
	}

	internal bool Purge()
	{
		int count = _inactiveViewTables.Count;
		if (count > 0)
		{
			ViewTable[] array = new ViewTable[count];
			_inactiveViewTables.Keys.CopyTo(array, 0);
			for (int i = 0; i < count; i++)
			{
				ViewTable key = array[i];
				int num = (int)_inactiveViewTables[key];
				if (--num > 0)
				{
					_inactiveViewTables[key] = num;
				}
				else
				{
					_inactiveViewTables.Remove(key);
				}
			}
		}
		ArrayList arrayList = new ArrayList();
		bool flag = false;
		IDictionaryEnumerator dictionaryEnumerator = GetEnumerator();
		try
		{
			while (dictionaryEnumerator.MoveNext())
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)dictionaryEnumerator.Current;
				WeakRefKey weakRefKey = (WeakRefKey)dictionaryEntry.Key;
				CollectionRecord collectionRecord = (CollectionRecord)dictionaryEntry.Value;
				if (weakRefKey.Target == null || !collectionRecord.IsAlive)
				{
					arrayList.Add(weakRefKey);
					continue;
				}
				ViewTable viewTable = collectionRecord.ViewTable;
				if (viewTable == null || !viewTable.Purge())
				{
					continue;
				}
				flag = true;
				if (viewTable.Count == 0)
				{
					collectionRecord.ViewTable = null;
					if (!collectionRecord.IsAlive)
					{
						arrayList.Add(weakRefKey);
					}
				}
			}
		}
		finally
		{
			IDisposable disposable = dictionaryEnumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		for (int j = 0; j < arrayList.Count; j++)
		{
			Remove(arrayList[j]);
		}
		return arrayList.Count > 0 || flag;
	}

	private void ValidateViewType(CollectionView cv, Type collectionViewType)
	{
		if (collectionViewType != null)
		{
			Type type = ((!(cv is CollectionViewProxy collectionViewProxy)) ? cv.GetType() : collectionViewProxy.ProxiedView.GetType());
			if (type != collectionViewType)
			{
				throw new ArgumentException(SR.Format(SR.CollectionView_NameTypeDuplicity, collectionViewType, type));
			}
		}
	}
}
