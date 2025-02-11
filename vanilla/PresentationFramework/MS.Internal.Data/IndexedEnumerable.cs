using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class IndexedEnumerable : IEnumerable, IWeakEventListener
{
	private class FilteredEnumerator : IEnumerator, IDisposable
	{
		private IEnumerable _enumerable;

		private IEnumerator _enumerator;

		private IndexedEnumerable _indexedEnumerable;

		private Predicate<object> _filterCallback;

		object IEnumerator.Current => _enumerator.Current;

		public FilteredEnumerator(IndexedEnumerable indexedEnumerable, IEnumerable enumerable, Predicate<object> filterCallback)
		{
			_enumerable = enumerable;
			_enumerator = _enumerable.GetEnumerator();
			_filterCallback = filterCallback;
			_indexedEnumerable = indexedEnumerable;
		}

		void IEnumerator.Reset()
		{
			if (_indexedEnumerable._enumerable == null)
			{
				throw new InvalidOperationException(SR.EnumeratorVersionChanged);
			}
			Dispose();
			_enumerator = _enumerable.GetEnumerator();
		}

		bool IEnumerator.MoveNext()
		{
			if (_indexedEnumerable._enumerable == null)
			{
				throw new InvalidOperationException(SR.EnumeratorVersionChanged);
			}
			bool result;
			if (_filterCallback == null)
			{
				result = _enumerator.MoveNext();
			}
			else
			{
				while ((result = _enumerator.MoveNext()) && !_filterCallback(_enumerator.Current))
				{
				}
			}
			return result;
		}

		public void Dispose()
		{
			if (_enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
			_enumerator = null;
		}
	}

	private IEnumerable _enumerable;

	private IEnumerator _enumerator;

	private IEnumerator _changeTracker;

	private ICollection _collection;

	private IList _list;

	private CollectionView _collectionView;

	private int _enumeratorVersion;

	private object _cachedItem;

	private int _cachedIndex = -1;

	private int _cachedVersion = -1;

	private int _cachedCount = -1;

	private bool? _cachedIsEmpty;

	private PropertyInfo _reflectedCount;

	private PropertyInfo _reflectedItemAt;

	private MethodInfo _reflectedIndexOf;

	private Predicate<object> _filterCallback;

	internal int Count
	{
		get
		{
			EnsureCacheCurrent();
			int value = 0;
			if (GetNativeCount(out value))
			{
				return value;
			}
			if (_cachedCount >= 0)
			{
				return _cachedCount;
			}
			value = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					_ = enumerator.Current;
					value++;
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			_cachedCount = value;
			_cachedIsEmpty = _cachedCount == 0;
			return value;
		}
	}

	internal bool IsEmpty
	{
		get
		{
			if (GetNativeIsEmpty(out var isEmpty))
			{
				return isEmpty;
			}
			if (_cachedIsEmpty.HasValue)
			{
				return _cachedIsEmpty.Value;
			}
			IEnumerator enumerator = GetEnumerator();
			_cachedIsEmpty = !enumerator.MoveNext();
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
			if (_cachedIsEmpty.Value)
			{
				_cachedCount = 0;
			}
			return _cachedIsEmpty.Value;
		}
	}

	internal object this[int index]
	{
		get
		{
			if (GetNativeItemAt(index, out var value))
			{
				return value;
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			int num = index - _cachedIndex;
			if (num < 0)
			{
				UseNewEnumerator();
				num = index + 1;
			}
			if (EnsureCacheCurrent())
			{
				if (index == _cachedIndex)
				{
					return _cachedItem;
				}
			}
			else
			{
				num = index + 1;
			}
			while (num > 0 && _enumerator.MoveNext())
			{
				num--;
			}
			if (num != 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			CacheCurrentItem(index, _enumerator.Current);
			return _cachedItem;
		}
	}

	internal IEnumerable Enumerable => _enumerable;

	internal ICollection Collection => _collection;

	internal IList List => _list;

	internal CollectionView CollectionView => _collectionView;

	private Predicate<object> FilterCallback => _filterCallback;

	internal IndexedEnumerable(IEnumerable collection)
		: this(collection, null)
	{
	}

	internal IndexedEnumerable(IEnumerable collection, Predicate<object> filterCallback)
	{
		_filterCallback = filterCallback;
		SetCollection(collection);
		if (List == null && collection is INotifyCollectionChanged source)
		{
			CollectionChangedEventManager.AddHandler(source, OnCollectionChanged);
		}
	}

	internal int IndexOf(object item)
	{
		if (GetNativeIndexOf(item, out var value))
		{
			return value;
		}
		if (EnsureCacheCurrent() && item == _cachedItem)
		{
			return _cachedIndex;
		}
		value = -1;
		if (_cachedIndex >= 0)
		{
			UseNewEnumerator();
		}
		int num = 0;
		while (_enumerator.MoveNext())
		{
			if (object.Equals(_enumerator.Current, item))
			{
				value = num;
				break;
			}
			num++;
		}
		if (value >= 0)
		{
			CacheCurrentItem(value, _enumerator.Current);
		}
		else
		{
			ClearAllCaches();
			DisposeEnumerator(ref _enumerator);
		}
		return value;
	}

	public IEnumerator GetEnumerator()
	{
		return new FilteredEnumerator(this, Enumerable, FilterCallback);
	}

	internal static void CopyTo(IEnumerable collection, Array array, int index)
	{
		Invariant.Assert(collection != null, "collection is null");
		Invariant.Assert(array != null, "target array is null");
		Invariant.Assert(array.Rank == 1, "expected array of rank=1");
		Invariant.Assert(index >= 0, "index must be positive");
		if (collection is ICollection collection2)
		{
			collection2.CopyTo(array, index);
			return;
		}
		foreach (object item in collection)
		{
			if (index < array.Length)
			{
				((IList)array)[index] = item;
				index++;
				continue;
			}
			throw new ArgumentException(SR.CopyToNotEnoughSpace, "index");
		}
	}

	internal void Invalidate()
	{
		ClearAllCaches();
		if (List == null && Enumerable is INotifyCollectionChanged source)
		{
			CollectionChangedEventManager.RemoveHandler(source, OnCollectionChanged);
		}
		_enumerable = null;
		DisposeEnumerator(ref _enumerator);
		DisposeEnumerator(ref _changeTracker);
		_collection = null;
		_list = null;
		_filterCallback = null;
	}

	private void CacheCurrentItem(int index, object item)
	{
		_cachedIndex = index;
		_cachedItem = item;
		_cachedVersion = _enumeratorVersion;
	}

	private bool EnsureCacheCurrent()
	{
		int num = EnsureEnumerator();
		if (num != _cachedVersion)
		{
			ClearAllCaches();
			_cachedVersion = num;
		}
		if (num == _cachedVersion)
		{
			return _cachedIndex >= 0;
		}
		return false;
	}

	private int EnsureEnumerator()
	{
		if (_enumerator == null)
		{
			UseNewEnumerator();
		}
		else
		{
			try
			{
				_changeTracker.MoveNext();
			}
			catch (InvalidOperationException)
			{
				UseNewEnumerator();
			}
		}
		return _enumeratorVersion;
	}

	private void UseNewEnumerator()
	{
		_enumeratorVersion++;
		DisposeEnumerator(ref _changeTracker);
		_changeTracker = _enumerable.GetEnumerator();
		DisposeEnumerator(ref _enumerator);
		_enumerator = GetEnumerator();
		_cachedIndex = -1;
		_cachedItem = null;
	}

	private void InvalidateEnumerator()
	{
		_enumeratorVersion++;
		DisposeEnumerator(ref _enumerator);
		ClearAllCaches();
	}

	private void DisposeEnumerator(ref IEnumerator ie)
	{
		if (ie is IDisposable disposable)
		{
			disposable.Dispose();
		}
		ie = null;
	}

	private void ClearAllCaches()
	{
		_cachedItem = null;
		_cachedIndex = -1;
		_cachedCount = -1;
	}

	private void SetCollection(IEnumerable collection)
	{
		Invariant.Assert(collection != null);
		_enumerable = collection;
		_collection = collection as ICollection;
		_list = collection as IList;
		_collectionView = collection as CollectionView;
		if (List != null || CollectionView != null)
		{
			return;
		}
		Type type = collection.GetType();
		MethodInfo method = type.GetMethod("IndexOf", new Type[1] { typeof(object) });
		if (method != null && method.ReturnType == typeof(int))
		{
			_reflectedIndexOf = method;
		}
		MemberInfo[] defaultMembers = type.GetDefaultMembers();
		for (int i = 0; i <= defaultMembers.Length - 1; i++)
		{
			PropertyInfo propertyInfo = defaultMembers[i] as PropertyInfo;
			if (propertyInfo != null)
			{
				ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
				if (indexParameters.Length == 1 && indexParameters[0].ParameterType.IsAssignableFrom(typeof(int)))
				{
					_reflectedItemAt = propertyInfo;
					break;
				}
			}
		}
		if (Collection == null)
		{
			PropertyInfo property = type.GetProperty("Count", typeof(int));
			if (property != null)
			{
				_reflectedCount = property;
			}
		}
	}

	private bool GetNativeCount(out int value)
	{
		bool result = false;
		value = -1;
		if (Collection != null)
		{
			value = Collection.Count;
			result = true;
		}
		else if (CollectionView != null)
		{
			value = CollectionView.Count;
			result = true;
		}
		else if (_reflectedCount != null)
		{
			try
			{
				value = (int)_reflectedCount.GetValue(Enumerable, null);
				result = true;
			}
			catch (MethodAccessException)
			{
				_reflectedCount = null;
				result = false;
			}
		}
		return result;
	}

	private bool GetNativeIsEmpty(out bool isEmpty)
	{
		bool result = false;
		isEmpty = true;
		if (Collection != null)
		{
			isEmpty = Collection.Count == 0;
			result = true;
		}
		else if (CollectionView != null)
		{
			isEmpty = CollectionView.IsEmpty;
			result = true;
		}
		else if (_reflectedCount != null)
		{
			try
			{
				isEmpty = (int)_reflectedCount.GetValue(Enumerable, null) == 0;
				result = true;
			}
			catch (MethodAccessException)
			{
				_reflectedCount = null;
				result = false;
			}
		}
		return result;
	}

	private bool GetNativeIndexOf(object item, out int value)
	{
		bool result = false;
		value = -1;
		if (List != null && FilterCallback == null)
		{
			value = List.IndexOf(item);
			result = true;
		}
		else if (CollectionView != null)
		{
			value = CollectionView.IndexOf(item);
			result = true;
		}
		else if (_reflectedIndexOf != null)
		{
			try
			{
				value = (int)_reflectedIndexOf.Invoke(Enumerable, new object[1] { item });
				result = true;
			}
			catch (MethodAccessException)
			{
				_reflectedIndexOf = null;
				result = false;
			}
		}
		return result;
	}

	private bool GetNativeItemAt(int index, out object value)
	{
		bool result = false;
		value = null;
		if (List != null)
		{
			value = List[index];
			result = true;
		}
		else if (CollectionView != null)
		{
			value = CollectionView.GetItemAt(index);
			result = true;
		}
		else if (_reflectedItemAt != null)
		{
			try
			{
				value = _reflectedItemAt.GetValue(Enumerable, new object[1] { index });
				result = true;
			}
			catch (MethodAccessException)
			{
				_reflectedItemAt = null;
				result = false;
			}
		}
		return result;
	}

	bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return ReceiveWeakEvent(managerType, sender, e);
	}

	protected virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return false;
	}

	private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		InvalidateEnumerator();
	}
}
