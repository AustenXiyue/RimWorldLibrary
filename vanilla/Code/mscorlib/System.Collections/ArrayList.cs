using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Collections;

/// <summary>Implements the <see cref="T:System.Collections.IList" /> interface using an array whose size is dynamically increased as required.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
[DebuggerTypeProxy(typeof(ArrayListDebugView))]
[DebuggerDisplay("Count = {Count}")]
public class ArrayList : IList, ICollection, IEnumerable, ICloneable
{
	[Serializable]
	private class IListWrapper : ArrayList
	{
		[Serializable]
		private sealed class IListWrapperEnumWrapper : IEnumerator, ICloneable
		{
			private IEnumerator _en;

			private int _remaining;

			private int _initialStartIndex;

			private int _initialCount;

			private bool _firstCall;

			public object Current
			{
				get
				{
					if (_firstCall)
					{
						throw new InvalidOperationException(Environment.GetResourceString("Enumeration has not started. Call MoveNext."));
					}
					if (_remaining < 0)
					{
						throw new InvalidOperationException(Environment.GetResourceString("Enumeration already finished."));
					}
					return _en.Current;
				}
			}

			private IListWrapperEnumWrapper()
			{
			}

			internal IListWrapperEnumWrapper(IListWrapper listWrapper, int startIndex, int count)
			{
				_en = listWrapper.GetEnumerator();
				_initialStartIndex = startIndex;
				_initialCount = count;
				while (startIndex-- > 0 && _en.MoveNext())
				{
				}
				_remaining = count;
				_firstCall = true;
			}

			public object Clone()
			{
				return new IListWrapperEnumWrapper
				{
					_en = (IEnumerator)((ICloneable)_en).Clone(),
					_initialStartIndex = _initialStartIndex,
					_initialCount = _initialCount,
					_remaining = _remaining,
					_firstCall = _firstCall
				};
			}

			public bool MoveNext()
			{
				if (_firstCall)
				{
					_firstCall = false;
					if (_remaining-- > 0)
					{
						return _en.MoveNext();
					}
					return false;
				}
				if (_remaining < 0)
				{
					return false;
				}
				if (_en.MoveNext())
				{
					return _remaining-- > 0;
				}
				return false;
			}

			public void Reset()
			{
				_en.Reset();
				int initialStartIndex = _initialStartIndex;
				while (initialStartIndex-- > 0 && _en.MoveNext())
				{
				}
				_remaining = _initialCount;
				_firstCall = true;
			}
		}

		private IList _list;

		public override int Capacity
		{
			get
			{
				return _list.Count;
			}
			set
			{
				if (value < Count)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("capacity was less than the current size."));
				}
			}
		}

		public override int Count => _list.Count;

		public override bool IsReadOnly => _list.IsReadOnly;

		public override bool IsFixedSize => _list.IsFixedSize;

		public override bool IsSynchronized => _list.IsSynchronized;

		public override object this[int index]
		{
			get
			{
				return _list[index];
			}
			set
			{
				_list[index] = value;
				_version++;
			}
		}

		public override object SyncRoot => _list.SyncRoot;

		internal IListWrapper(IList list)
		{
			_list = list;
			_version = 0;
		}

		public override int Add(object obj)
		{
			int result = _list.Add(obj);
			_version++;
			return result;
		}

		public override void AddRange(ICollection c)
		{
			InsertRange(Count, c);
		}

		public override int BinarySearch(int index, int count, object value, IComparer comparer)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
			}
			if (Count - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			if (comparer == null)
			{
				comparer = Comparer.Default;
			}
			int num = index;
			int num2 = index + count - 1;
			while (num <= num2)
			{
				int num3 = (num + num2) / 2;
				int num4 = comparer.Compare(value, _list[num3]);
				if (num4 == 0)
				{
					return num3;
				}
				if (num4 < 0)
				{
					num2 = num3 - 1;
				}
				else
				{
					num = num3 + 1;
				}
			}
			return ~num;
		}

		public override void Clear()
		{
			if (_list.IsFixedSize)
			{
				throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
			}
			_list.Clear();
			_version++;
		}

		public override object Clone()
		{
			return new IListWrapper(_list);
		}

		public override bool Contains(object obj)
		{
			return _list.Contains(obj);
		}

		public override void CopyTo(Array array, int index)
		{
			_list.CopyTo(array, index);
		}

		public override void CopyTo(int index, Array array, int arrayIndex, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (index < 0 || arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "arrayIndex", Environment.GetResourceString("Non-negative number required."));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
			}
			if (array.Length - arrayIndex < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			if (array.Rank != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("Only single dimensional arrays are supported for the requested action."));
			}
			if (_list.Count - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			for (int i = index; i < index + count; i++)
			{
				array.SetValue(_list[i], arrayIndex++);
			}
		}

		public override IEnumerator GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		public override IEnumerator GetEnumerator(int index, int count)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
			}
			if (_list.Count - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			return new IListWrapperEnumWrapper(this, index, count);
		}

		public override int IndexOf(object value)
		{
			return _list.IndexOf(value);
		}

		public override int IndexOf(object value, int startIndex)
		{
			return IndexOf(value, startIndex, _list.Count - startIndex);
		}

		public override int IndexOf(object value, int startIndex, int count)
		{
			if (startIndex < 0 || startIndex > Count)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			if (count < 0 || startIndex > Count - count)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count must be positive and count must refer to a location within the string/array/collection."));
			}
			int num = startIndex + count;
			if (value == null)
			{
				for (int i = startIndex; i < num; i++)
				{
					if (_list[i] == null)
					{
						return i;
					}
				}
				return -1;
			}
			for (int j = startIndex; j < num; j++)
			{
				if (_list[j] != null && _list[j].Equals(value))
				{
					return j;
				}
			}
			return -1;
		}

		public override void Insert(int index, object obj)
		{
			_list.Insert(index, obj);
			_version++;
		}

		public override void InsertRange(int index, ICollection c)
		{
			if (c == null)
			{
				throw new ArgumentNullException("c", Environment.GetResourceString("Collection cannot be null."));
			}
			if (index < 0 || index > Count)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			if (c.Count <= 0)
			{
				return;
			}
			if (_list is ArrayList arrayList)
			{
				arrayList.InsertRange(index, c);
			}
			else
			{
				IEnumerator enumerator = c.GetEnumerator();
				while (enumerator.MoveNext())
				{
					_list.Insert(index++, enumerator.Current);
				}
			}
			_version++;
		}

		public override int LastIndexOf(object value)
		{
			return LastIndexOf(value, _list.Count - 1, _list.Count);
		}

		public override int LastIndexOf(object value, int startIndex)
		{
			return LastIndexOf(value, startIndex, startIndex + 1);
		}

		public override int LastIndexOf(object value, int startIndex, int count)
		{
			if (_list.Count == 0)
			{
				return -1;
			}
			if (startIndex < 0 || startIndex >= _list.Count)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			if (count < 0 || count > startIndex + 1)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count must be positive and count must refer to a location within the string/array/collection."));
			}
			int num = startIndex - count + 1;
			if (value == null)
			{
				for (int num2 = startIndex; num2 >= num; num2--)
				{
					if (_list[num2] == null)
					{
						return num2;
					}
				}
				return -1;
			}
			for (int num3 = startIndex; num3 >= num; num3--)
			{
				if (_list[num3] != null && _list[num3].Equals(value))
				{
					return num3;
				}
			}
			return -1;
		}

		public override void Remove(object value)
		{
			int num = IndexOf(value);
			if (num >= 0)
			{
				RemoveAt(num);
			}
		}

		public override void RemoveAt(int index)
		{
			_list.RemoveAt(index);
			_version++;
		}

		public override void RemoveRange(int index, int count)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
			}
			if (_list.Count - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			if (count > 0)
			{
				_version++;
			}
			while (count > 0)
			{
				_list.RemoveAt(index);
				count--;
			}
		}

		public override void Reverse(int index, int count)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
			}
			if (_list.Count - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			int num = index;
			int num2 = index + count - 1;
			while (num < num2)
			{
				object value = _list[num];
				_list[num++] = _list[num2];
				_list[num2--] = value;
			}
			_version++;
		}

		public override void SetRange(int index, ICollection c)
		{
			if (c == null)
			{
				throw new ArgumentNullException("c", Environment.GetResourceString("Collection cannot be null."));
			}
			if (index < 0 || index > _list.Count - c.Count)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			if (c.Count > 0)
			{
				IEnumerator enumerator = c.GetEnumerator();
				while (enumerator.MoveNext())
				{
					_list[index++] = enumerator.Current;
				}
				_version++;
			}
		}

		public override ArrayList GetRange(int index, int count)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
			}
			if (_list.Count - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			return new Range(this, index, count);
		}

		public override void Sort(int index, int count, IComparer comparer)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
			}
			if (_list.Count - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			object[] array = new object[count];
			CopyTo(index, array, 0, count);
			Array.Sort(array, 0, count, comparer);
			for (int i = 0; i < count; i++)
			{
				_list[i + index] = array[i];
			}
			_version++;
		}

		public override object[] ToArray()
		{
			object[] array = new object[Count];
			_list.CopyTo(array, 0);
			return array;
		}

		[SecuritySafeCritical]
		public override Array ToArray(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			Array array = Array.UnsafeCreateInstance(type, _list.Count);
			_list.CopyTo(array, 0);
			return array;
		}

		public override void TrimToSize()
		{
		}
	}

	[Serializable]
	private class SyncArrayList : ArrayList
	{
		private ArrayList _list;

		private object _root;

		public override int Capacity
		{
			get
			{
				lock (_root)
				{
					return _list.Capacity;
				}
			}
			set
			{
				lock (_root)
				{
					_list.Capacity = value;
				}
			}
		}

		public override int Count
		{
			get
			{
				lock (_root)
				{
					return _list.Count;
				}
			}
		}

		public override bool IsReadOnly => _list.IsReadOnly;

		public override bool IsFixedSize => _list.IsFixedSize;

		public override bool IsSynchronized => true;

		public override object this[int index]
		{
			get
			{
				lock (_root)
				{
					return _list[index];
				}
			}
			set
			{
				lock (_root)
				{
					_list[index] = value;
				}
			}
		}

		public override object SyncRoot => _root;

		internal SyncArrayList(ArrayList list)
			: base(trash: false)
		{
			_list = list;
			_root = list.SyncRoot;
		}

		public override int Add(object value)
		{
			lock (_root)
			{
				return _list.Add(value);
			}
		}

		public override void AddRange(ICollection c)
		{
			lock (_root)
			{
				_list.AddRange(c);
			}
		}

		public override int BinarySearch(object value)
		{
			lock (_root)
			{
				return _list.BinarySearch(value);
			}
		}

		public override int BinarySearch(object value, IComparer comparer)
		{
			lock (_root)
			{
				return _list.BinarySearch(value, comparer);
			}
		}

		public override int BinarySearch(int index, int count, object value, IComparer comparer)
		{
			lock (_root)
			{
				return _list.BinarySearch(index, count, value, comparer);
			}
		}

		public override void Clear()
		{
			lock (_root)
			{
				_list.Clear();
			}
		}

		public override object Clone()
		{
			lock (_root)
			{
				return new SyncArrayList((ArrayList)_list.Clone());
			}
		}

		public override bool Contains(object item)
		{
			lock (_root)
			{
				return _list.Contains(item);
			}
		}

		public override void CopyTo(Array array)
		{
			lock (_root)
			{
				_list.CopyTo(array);
			}
		}

		public override void CopyTo(Array array, int index)
		{
			lock (_root)
			{
				_list.CopyTo(array, index);
			}
		}

		public override void CopyTo(int index, Array array, int arrayIndex, int count)
		{
			lock (_root)
			{
				_list.CopyTo(index, array, arrayIndex, count);
			}
		}

		public override IEnumerator GetEnumerator()
		{
			lock (_root)
			{
				return _list.GetEnumerator();
			}
		}

		public override IEnumerator GetEnumerator(int index, int count)
		{
			lock (_root)
			{
				return _list.GetEnumerator(index, count);
			}
		}

		public override int IndexOf(object value)
		{
			lock (_root)
			{
				return _list.IndexOf(value);
			}
		}

		public override int IndexOf(object value, int startIndex)
		{
			lock (_root)
			{
				return _list.IndexOf(value, startIndex);
			}
		}

		public override int IndexOf(object value, int startIndex, int count)
		{
			lock (_root)
			{
				return _list.IndexOf(value, startIndex, count);
			}
		}

		public override void Insert(int index, object value)
		{
			lock (_root)
			{
				_list.Insert(index, value);
			}
		}

		public override void InsertRange(int index, ICollection c)
		{
			lock (_root)
			{
				_list.InsertRange(index, c);
			}
		}

		public override int LastIndexOf(object value)
		{
			lock (_root)
			{
				return _list.LastIndexOf(value);
			}
		}

		public override int LastIndexOf(object value, int startIndex)
		{
			lock (_root)
			{
				return _list.LastIndexOf(value, startIndex);
			}
		}

		public override int LastIndexOf(object value, int startIndex, int count)
		{
			lock (_root)
			{
				return _list.LastIndexOf(value, startIndex, count);
			}
		}

		public override void Remove(object value)
		{
			lock (_root)
			{
				_list.Remove(value);
			}
		}

		public override void RemoveAt(int index)
		{
			lock (_root)
			{
				_list.RemoveAt(index);
			}
		}

		public override void RemoveRange(int index, int count)
		{
			lock (_root)
			{
				_list.RemoveRange(index, count);
			}
		}

		public override void Reverse(int index, int count)
		{
			lock (_root)
			{
				_list.Reverse(index, count);
			}
		}

		public override void SetRange(int index, ICollection c)
		{
			lock (_root)
			{
				_list.SetRange(index, c);
			}
		}

		public override ArrayList GetRange(int index, int count)
		{
			lock (_root)
			{
				return _list.GetRange(index, count);
			}
		}

		public override void Sort()
		{
			lock (_root)
			{
				_list.Sort();
			}
		}

		public override void Sort(IComparer comparer)
		{
			lock (_root)
			{
				_list.Sort(comparer);
			}
		}

		public override void Sort(int index, int count, IComparer comparer)
		{
			lock (_root)
			{
				_list.Sort(index, count, comparer);
			}
		}

		public override object[] ToArray()
		{
			lock (_root)
			{
				return _list.ToArray();
			}
		}

		public override Array ToArray(Type type)
		{
			lock (_root)
			{
				return _list.ToArray(type);
			}
		}

		public override void TrimToSize()
		{
			lock (_root)
			{
				_list.TrimToSize();
			}
		}
	}

	[Serializable]
	private class SyncIList : IList, ICollection, IEnumerable
	{
		private IList _list;

		private object _root;

		public virtual int Count
		{
			get
			{
				lock (_root)
				{
					return _list.Count;
				}
			}
		}

		public virtual bool IsReadOnly => _list.IsReadOnly;

		public virtual bool IsFixedSize => _list.IsFixedSize;

		public virtual bool IsSynchronized => true;

		public virtual object this[int index]
		{
			get
			{
				lock (_root)
				{
					return _list[index];
				}
			}
			set
			{
				lock (_root)
				{
					_list[index] = value;
				}
			}
		}

		public virtual object SyncRoot => _root;

		internal SyncIList(IList list)
		{
			_list = list;
			_root = list.SyncRoot;
		}

		public virtual int Add(object value)
		{
			lock (_root)
			{
				return _list.Add(value);
			}
		}

		public virtual void Clear()
		{
			lock (_root)
			{
				_list.Clear();
			}
		}

		public virtual bool Contains(object item)
		{
			lock (_root)
			{
				return _list.Contains(item);
			}
		}

		public virtual void CopyTo(Array array, int index)
		{
			lock (_root)
			{
				_list.CopyTo(array, index);
			}
		}

		public virtual IEnumerator GetEnumerator()
		{
			lock (_root)
			{
				return _list.GetEnumerator();
			}
		}

		public virtual int IndexOf(object value)
		{
			lock (_root)
			{
				return _list.IndexOf(value);
			}
		}

		public virtual void Insert(int index, object value)
		{
			lock (_root)
			{
				_list.Insert(index, value);
			}
		}

		public virtual void Remove(object value)
		{
			lock (_root)
			{
				_list.Remove(value);
			}
		}

		public virtual void RemoveAt(int index)
		{
			lock (_root)
			{
				_list.RemoveAt(index);
			}
		}
	}

	[Serializable]
	private class FixedSizeList : IList, ICollection, IEnumerable
	{
		private IList _list;

		public virtual int Count => _list.Count;

		public virtual bool IsReadOnly => _list.IsReadOnly;

		public virtual bool IsFixedSize => true;

		public virtual bool IsSynchronized => _list.IsSynchronized;

		public virtual object this[int index]
		{
			get
			{
				return _list[index];
			}
			set
			{
				_list[index] = value;
			}
		}

		public virtual object SyncRoot => _list.SyncRoot;

		internal FixedSizeList(IList l)
		{
			_list = l;
		}

		public virtual int Add(object obj)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
		}

		public virtual void Clear()
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
		}

		public virtual bool Contains(object obj)
		{
			return _list.Contains(obj);
		}

		public virtual void CopyTo(Array array, int index)
		{
			_list.CopyTo(array, index);
		}

		public virtual IEnumerator GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		public virtual int IndexOf(object value)
		{
			return _list.IndexOf(value);
		}

		public virtual void Insert(int index, object obj)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
		}

		public virtual void Remove(object value)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
		}

		public virtual void RemoveAt(int index)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
		}
	}

	[Serializable]
	private class FixedSizeArrayList : ArrayList
	{
		private ArrayList _list;

		public override int Count => _list.Count;

		public override bool IsReadOnly => _list.IsReadOnly;

		public override bool IsFixedSize => true;

		public override bool IsSynchronized => _list.IsSynchronized;

		public override object this[int index]
		{
			get
			{
				return _list[index];
			}
			set
			{
				_list[index] = value;
				_version = _list._version;
			}
		}

		public override object SyncRoot => _list.SyncRoot;

		public override int Capacity
		{
			get
			{
				return _list.Capacity;
			}
			set
			{
				throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
			}
		}

		internal FixedSizeArrayList(ArrayList l)
		{
			_list = l;
			_version = _list._version;
		}

		public override int Add(object obj)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
		}

		public override void AddRange(ICollection c)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
		}

		public override int BinarySearch(int index, int count, object value, IComparer comparer)
		{
			return _list.BinarySearch(index, count, value, comparer);
		}

		public override void Clear()
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
		}

		public override object Clone()
		{
			return new FixedSizeArrayList(_list)
			{
				_list = (ArrayList)_list.Clone()
			};
		}

		public override bool Contains(object obj)
		{
			return _list.Contains(obj);
		}

		public override void CopyTo(Array array, int index)
		{
			_list.CopyTo(array, index);
		}

		public override void CopyTo(int index, Array array, int arrayIndex, int count)
		{
			_list.CopyTo(index, array, arrayIndex, count);
		}

		public override IEnumerator GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		public override IEnumerator GetEnumerator(int index, int count)
		{
			return _list.GetEnumerator(index, count);
		}

		public override int IndexOf(object value)
		{
			return _list.IndexOf(value);
		}

		public override int IndexOf(object value, int startIndex)
		{
			return _list.IndexOf(value, startIndex);
		}

		public override int IndexOf(object value, int startIndex, int count)
		{
			return _list.IndexOf(value, startIndex, count);
		}

		public override void Insert(int index, object obj)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
		}

		public override void InsertRange(int index, ICollection c)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
		}

		public override int LastIndexOf(object value)
		{
			return _list.LastIndexOf(value);
		}

		public override int LastIndexOf(object value, int startIndex)
		{
			return _list.LastIndexOf(value, startIndex);
		}

		public override int LastIndexOf(object value, int startIndex, int count)
		{
			return _list.LastIndexOf(value, startIndex, count);
		}

		public override void Remove(object value)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
		}

		public override void RemoveAt(int index)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
		}

		public override void RemoveRange(int index, int count)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
		}

		public override void SetRange(int index, ICollection c)
		{
			_list.SetRange(index, c);
			_version = _list._version;
		}

		public override ArrayList GetRange(int index, int count)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
			}
			if (Count - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			return new Range(this, index, count);
		}

		public override void Reverse(int index, int count)
		{
			_list.Reverse(index, count);
			_version = _list._version;
		}

		public override void Sort(int index, int count, IComparer comparer)
		{
			_list.Sort(index, count, comparer);
			_version = _list._version;
		}

		public override object[] ToArray()
		{
			return _list.ToArray();
		}

		public override Array ToArray(Type type)
		{
			return _list.ToArray(type);
		}

		public override void TrimToSize()
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection was of a fixed size."));
		}
	}

	[Serializable]
	private class ReadOnlyList : IList, ICollection, IEnumerable
	{
		private IList _list;

		public virtual int Count => _list.Count;

		public virtual bool IsReadOnly => true;

		public virtual bool IsFixedSize => true;

		public virtual bool IsSynchronized => _list.IsSynchronized;

		public virtual object this[int index]
		{
			get
			{
				return _list[index];
			}
			set
			{
				throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
			}
		}

		public virtual object SyncRoot => _list.SyncRoot;

		internal ReadOnlyList(IList l)
		{
			_list = l;
		}

		public virtual int Add(object obj)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}

		public virtual void Clear()
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}

		public virtual bool Contains(object obj)
		{
			return _list.Contains(obj);
		}

		public virtual void CopyTo(Array array, int index)
		{
			_list.CopyTo(array, index);
		}

		public virtual IEnumerator GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		public virtual int IndexOf(object value)
		{
			return _list.IndexOf(value);
		}

		public virtual void Insert(int index, object obj)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}

		public virtual void Remove(object value)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}

		public virtual void RemoveAt(int index)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}
	}

	[Serializable]
	private class ReadOnlyArrayList : ArrayList
	{
		private ArrayList _list;

		public override int Count => _list.Count;

		public override bool IsReadOnly => true;

		public override bool IsFixedSize => true;

		public override bool IsSynchronized => _list.IsSynchronized;

		public override object this[int index]
		{
			get
			{
				return _list[index];
			}
			set
			{
				throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
			}
		}

		public override object SyncRoot => _list.SyncRoot;

		public override int Capacity
		{
			get
			{
				return _list.Capacity;
			}
			set
			{
				throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
			}
		}

		internal ReadOnlyArrayList(ArrayList l)
		{
			_list = l;
		}

		public override int Add(object obj)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}

		public override void AddRange(ICollection c)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}

		public override int BinarySearch(int index, int count, object value, IComparer comparer)
		{
			return _list.BinarySearch(index, count, value, comparer);
		}

		public override void Clear()
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}

		public override object Clone()
		{
			return new ReadOnlyArrayList(_list)
			{
				_list = (ArrayList)_list.Clone()
			};
		}

		public override bool Contains(object obj)
		{
			return _list.Contains(obj);
		}

		public override void CopyTo(Array array, int index)
		{
			_list.CopyTo(array, index);
		}

		public override void CopyTo(int index, Array array, int arrayIndex, int count)
		{
			_list.CopyTo(index, array, arrayIndex, count);
		}

		public override IEnumerator GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		public override IEnumerator GetEnumerator(int index, int count)
		{
			return _list.GetEnumerator(index, count);
		}

		public override int IndexOf(object value)
		{
			return _list.IndexOf(value);
		}

		public override int IndexOf(object value, int startIndex)
		{
			return _list.IndexOf(value, startIndex);
		}

		public override int IndexOf(object value, int startIndex, int count)
		{
			return _list.IndexOf(value, startIndex, count);
		}

		public override void Insert(int index, object obj)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}

		public override void InsertRange(int index, ICollection c)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}

		public override int LastIndexOf(object value)
		{
			return _list.LastIndexOf(value);
		}

		public override int LastIndexOf(object value, int startIndex)
		{
			return _list.LastIndexOf(value, startIndex);
		}

		public override int LastIndexOf(object value, int startIndex, int count)
		{
			return _list.LastIndexOf(value, startIndex, count);
		}

		public override void Remove(object value)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}

		public override void RemoveAt(int index)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}

		public override void RemoveRange(int index, int count)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}

		public override void SetRange(int index, ICollection c)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}

		public override ArrayList GetRange(int index, int count)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
			}
			if (Count - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			return new Range(this, index, count);
		}

		public override void Reverse(int index, int count)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}

		public override void Sort(int index, int count, IComparer comparer)
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}

		public override object[] ToArray()
		{
			return _list.ToArray();
		}

		public override Array ToArray(Type type)
		{
			return _list.ToArray(type);
		}

		public override void TrimToSize()
		{
			throw new NotSupportedException(Environment.GetResourceString("Collection is read-only."));
		}
	}

	[Serializable]
	private sealed class ArrayListEnumerator : IEnumerator, ICloneable
	{
		private ArrayList list;

		private int index;

		private int endIndex;

		private int version;

		private object currentElement;

		private int startIndex;

		public object Current
		{
			get
			{
				if (index < startIndex)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration has not started. Call MoveNext."));
				}
				if (index > endIndex)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration already finished."));
				}
				return currentElement;
			}
		}

		internal ArrayListEnumerator(ArrayList list, int index, int count)
		{
			this.list = list;
			startIndex = index;
			this.index = index - 1;
			endIndex = this.index + count;
			version = list._version;
			currentElement = null;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public bool MoveNext()
		{
			if (version != list._version)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
			}
			if (index < endIndex)
			{
				currentElement = list[++index];
				return true;
			}
			index = endIndex + 1;
			return false;
		}

		public void Reset()
		{
			if (version != list._version)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
			}
			index = startIndex - 1;
		}
	}

	[Serializable]
	private class Range : ArrayList
	{
		private ArrayList _baseList;

		private int _baseIndex;

		private int _baseSize;

		private int _baseVersion;

		public override int Capacity
		{
			get
			{
				return _baseList.Capacity;
			}
			set
			{
				if (value < Count)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("capacity was less than the current size."));
				}
			}
		}

		public override int Count
		{
			get
			{
				InternalUpdateRange();
				return _baseSize;
			}
		}

		public override bool IsReadOnly => _baseList.IsReadOnly;

		public override bool IsFixedSize => _baseList.IsFixedSize;

		public override bool IsSynchronized => _baseList.IsSynchronized;

		public override object SyncRoot => _baseList.SyncRoot;

		public override object this[int index]
		{
			get
			{
				InternalUpdateRange();
				if (index < 0 || index >= _baseSize)
				{
					throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
				}
				return _baseList[_baseIndex + index];
			}
			set
			{
				InternalUpdateRange();
				if (index < 0 || index >= _baseSize)
				{
					throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
				}
				_baseList[_baseIndex + index] = value;
				InternalUpdateVersion();
			}
		}

		internal Range(ArrayList list, int index, int count)
			: base(trash: false)
		{
			_baseList = list;
			_baseIndex = index;
			_baseSize = count;
			_baseVersion = list._version;
			_version = list._version;
		}

		private void InternalUpdateRange()
		{
			if (_baseVersion != _baseList._version)
			{
				throw new InvalidOperationException(Environment.GetResourceString("This range in the underlying list is invalid. A possible cause is that elements were removed."));
			}
		}

		private void InternalUpdateVersion()
		{
			_baseVersion++;
			_version++;
		}

		public override int Add(object value)
		{
			InternalUpdateRange();
			_baseList.Insert(_baseIndex + _baseSize, value);
			InternalUpdateVersion();
			return _baseSize++;
		}

		public override void AddRange(ICollection c)
		{
			if (c == null)
			{
				throw new ArgumentNullException("c");
			}
			InternalUpdateRange();
			int count = c.Count;
			if (count > 0)
			{
				_baseList.InsertRange(_baseIndex + _baseSize, c);
				InternalUpdateVersion();
				_baseSize += count;
			}
		}

		public override int BinarySearch(int index, int count, object value, IComparer comparer)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
			}
			if (_baseSize - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			InternalUpdateRange();
			int num = _baseList.BinarySearch(_baseIndex + index, count, value, comparer);
			if (num >= 0)
			{
				return num - _baseIndex;
			}
			return num + _baseIndex;
		}

		public override void Clear()
		{
			InternalUpdateRange();
			if (_baseSize != 0)
			{
				_baseList.RemoveRange(_baseIndex, _baseSize);
				InternalUpdateVersion();
				_baseSize = 0;
			}
		}

		public override object Clone()
		{
			InternalUpdateRange();
			return new Range(_baseList, _baseIndex, _baseSize)
			{
				_baseList = (ArrayList)_baseList.Clone()
			};
		}

		public override bool Contains(object item)
		{
			InternalUpdateRange();
			if (item == null)
			{
				for (int i = 0; i < _baseSize; i++)
				{
					if (_baseList[_baseIndex + i] == null)
					{
						return true;
					}
				}
				return false;
			}
			for (int j = 0; j < _baseSize; j++)
			{
				if (_baseList[_baseIndex + j] != null && _baseList[_baseIndex + j].Equals(item))
				{
					return true;
				}
			}
			return false;
		}

		public override void CopyTo(Array array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (array.Rank != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("Only single dimensional arrays are supported for the requested action."));
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Non-negative number required."));
			}
			if (array.Length - index < _baseSize)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			InternalUpdateRange();
			_baseList.CopyTo(_baseIndex, array, index, _baseSize);
		}

		public override void CopyTo(int index, Array array, int arrayIndex, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (array.Rank != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("Only single dimensional arrays are supported for the requested action."));
			}
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
			}
			if (array.Length - arrayIndex < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			if (_baseSize - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			InternalUpdateRange();
			_baseList.CopyTo(_baseIndex + index, array, arrayIndex, count);
		}

		public override IEnumerator GetEnumerator()
		{
			return GetEnumerator(0, _baseSize);
		}

		public override IEnumerator GetEnumerator(int index, int count)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
			}
			if (_baseSize - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			InternalUpdateRange();
			return _baseList.GetEnumerator(_baseIndex + index, count);
		}

		public override ArrayList GetRange(int index, int count)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
			}
			if (_baseSize - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			InternalUpdateRange();
			return new Range(this, index, count);
		}

		public override int IndexOf(object value)
		{
			InternalUpdateRange();
			int num = _baseList.IndexOf(value, _baseIndex, _baseSize);
			if (num >= 0)
			{
				return num - _baseIndex;
			}
			return -1;
		}

		public override int IndexOf(object value, int startIndex)
		{
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Non-negative number required."));
			}
			if (startIndex > _baseSize)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			InternalUpdateRange();
			int num = _baseList.IndexOf(value, _baseIndex + startIndex, _baseSize - startIndex);
			if (num >= 0)
			{
				return num - _baseIndex;
			}
			return -1;
		}

		public override int IndexOf(object value, int startIndex, int count)
		{
			if (startIndex < 0 || startIndex > _baseSize)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			if (count < 0 || startIndex > _baseSize - count)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count must be positive and count must refer to a location within the string/array/collection."));
			}
			InternalUpdateRange();
			int num = _baseList.IndexOf(value, _baseIndex + startIndex, count);
			if (num >= 0)
			{
				return num - _baseIndex;
			}
			return -1;
		}

		public override void Insert(int index, object value)
		{
			if (index < 0 || index > _baseSize)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			InternalUpdateRange();
			_baseList.Insert(_baseIndex + index, value);
			InternalUpdateVersion();
			_baseSize++;
		}

		public override void InsertRange(int index, ICollection c)
		{
			if (index < 0 || index > _baseSize)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			if (c == null)
			{
				throw new ArgumentNullException("c");
			}
			InternalUpdateRange();
			int count = c.Count;
			if (count > 0)
			{
				_baseList.InsertRange(_baseIndex + index, c);
				_baseSize += count;
				InternalUpdateVersion();
			}
		}

		public override int LastIndexOf(object value)
		{
			InternalUpdateRange();
			int num = _baseList.LastIndexOf(value, _baseIndex + _baseSize - 1, _baseSize);
			if (num >= 0)
			{
				return num - _baseIndex;
			}
			return -1;
		}

		public override int LastIndexOf(object value, int startIndex)
		{
			return LastIndexOf(value, startIndex, startIndex + 1);
		}

		public override int LastIndexOf(object value, int startIndex, int count)
		{
			InternalUpdateRange();
			if (_baseSize == 0)
			{
				return -1;
			}
			if (startIndex >= _baseSize)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Non-negative number required."));
			}
			int num = _baseList.LastIndexOf(value, _baseIndex + startIndex, count);
			if (num >= 0)
			{
				return num - _baseIndex;
			}
			return -1;
		}

		public override void RemoveAt(int index)
		{
			if (index < 0 || index >= _baseSize)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			InternalUpdateRange();
			_baseList.RemoveAt(_baseIndex + index);
			InternalUpdateVersion();
			_baseSize--;
		}

		public override void RemoveRange(int index, int count)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
			}
			if (_baseSize - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			InternalUpdateRange();
			if (count > 0)
			{
				_baseList.RemoveRange(_baseIndex + index, count);
				InternalUpdateVersion();
				_baseSize -= count;
			}
		}

		public override void Reverse(int index, int count)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
			}
			if (_baseSize - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			InternalUpdateRange();
			_baseList.Reverse(_baseIndex + index, count);
			InternalUpdateVersion();
		}

		public override void SetRange(int index, ICollection c)
		{
			InternalUpdateRange();
			if (index < 0 || index >= _baseSize)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			_baseList.SetRange(_baseIndex + index, c);
			if (c.Count > 0)
			{
				InternalUpdateVersion();
			}
		}

		public override void Sort(int index, int count, IComparer comparer)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
			}
			if (_baseSize - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
			}
			InternalUpdateRange();
			_baseList.Sort(_baseIndex + index, count, comparer);
			InternalUpdateVersion();
		}

		public override object[] ToArray()
		{
			InternalUpdateRange();
			object[] array = new object[_baseSize];
			Array.Copy(_baseList._items, _baseIndex, array, 0, _baseSize);
			return array;
		}

		[SecuritySafeCritical]
		public override Array ToArray(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			InternalUpdateRange();
			Array array = Array.UnsafeCreateInstance(type, _baseSize);
			_baseList.CopyTo(_baseIndex, array, 0, _baseSize);
			return array;
		}

		public override void TrimToSize()
		{
			throw new NotSupportedException(Environment.GetResourceString("The specified operation is not supported on Ranges."));
		}
	}

	[Serializable]
	private sealed class ArrayListEnumeratorSimple : IEnumerator, ICloneable
	{
		private ArrayList list;

		private int index;

		private int version;

		private object currentElement;

		[NonSerialized]
		private bool isArrayList;

		private static object dummyObject = new object();

		public object Current
		{
			get
			{
				object obj = currentElement;
				if (dummyObject == obj)
				{
					if (index == -1)
					{
						throw new InvalidOperationException(Environment.GetResourceString("Enumeration has not started. Call MoveNext."));
					}
					throw new InvalidOperationException(Environment.GetResourceString("Enumeration already finished."));
				}
				return obj;
			}
		}

		internal ArrayListEnumeratorSimple(ArrayList list)
		{
			this.list = list;
			index = -1;
			version = list._version;
			isArrayList = list.GetType() == typeof(ArrayList);
			currentElement = dummyObject;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public bool MoveNext()
		{
			if (version != list._version)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
			}
			if (isArrayList)
			{
				if (index < list._size - 1)
				{
					currentElement = list._items[++index];
					return true;
				}
				currentElement = dummyObject;
				index = list._size;
				return false;
			}
			if (index < list.Count - 1)
			{
				currentElement = list[++index];
				return true;
			}
			index = list.Count;
			currentElement = dummyObject;
			return false;
		}

		public void Reset()
		{
			if (version != list._version)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Collection was modified; enumeration operation may not execute."));
			}
			currentElement = dummyObject;
			index = -1;
		}
	}

	internal class ArrayListDebugView
	{
		private ArrayList arrayList;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public object[] Items => arrayList.ToArray();

		public ArrayListDebugView(ArrayList arrayList)
		{
			if (arrayList == null)
			{
				throw new ArgumentNullException("arrayList");
			}
			this.arrayList = arrayList;
		}
	}

	private object[] _items;

	private int _size;

	private int _version;

	[NonSerialized]
	private object _syncRoot;

	private const int _defaultCapacity = 4;

	private static readonly object[] emptyArray = EmptyArray<object>.Value;

	/// <summary>Gets or sets the number of elements that the <see cref="T:System.Collections.ArrayList" /> can contain.</summary>
	/// <returns>The number of elements that the <see cref="T:System.Collections.ArrayList" /> can contain.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <see cref="P:System.Collections.ArrayList.Capacity" /> is set to a value that is less than <see cref="P:System.Collections.ArrayList.Count" />.</exception>
	/// <exception cref="T:System.OutOfMemoryException">There is not enough memory available on the system.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual int Capacity
	{
		get
		{
			return _items.Length;
		}
		set
		{
			if (value < _size)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("capacity was less than the current size."));
			}
			if (value == _items.Length)
			{
				return;
			}
			if (value > 0)
			{
				object[] array = new object[value];
				if (_size > 0)
				{
					Array.Copy(_items, 0, array, 0, _size);
				}
				_items = array;
			}
			else
			{
				_items = new object[4];
			}
		}
	}

	/// <summary>Gets the number of elements actually contained in the <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <returns>The number of elements actually contained in the <see cref="T:System.Collections.ArrayList" />.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual int Count => _size;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.ArrayList" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.ArrayList" /> has a fixed size; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsFixedSize => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.ArrayList" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.ArrayList" /> is read-only; otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsReadOnly => false;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ArrayList" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ArrayList" /> is synchronized (thread safe); otherwise, false. The default is false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ArrayList" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual object SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
			}
			return _syncRoot;
		}
	}

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.ArrayList.Count" />. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual object this[int index]
	{
		get
		{
			if (index < 0 || index >= _size)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			return _items[index];
		}
		set
		{
			if (index < 0 || index >= _size)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
			}
			_items[index] = value;
			_version++;
		}
	}

	internal ArrayList(bool trash)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ArrayList" /> class that is empty and has the default initial capacity.</summary>
	public ArrayList()
	{
		_items = emptyArray;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ArrayList" /> class that is empty and has the specified initial capacity.</summary>
	/// <param name="capacity">The number of elements that the new list can initially store. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than zero. </exception>
	public ArrayList(int capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("'{0}' must be non-negative.", "capacity"));
		}
		if (capacity == 0)
		{
			_items = emptyArray;
		}
		else
		{
			_items = new object[capacity];
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ArrayList" /> class that contains elements copied from the specified collection and that has the same initial capacity as the number of elements copied.</summary>
	/// <param name="c">The <see cref="T:System.Collections.ICollection" /> whose elements are copied to the new list. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="c" /> is null. </exception>
	public ArrayList(ICollection c)
	{
		if (c == null)
		{
			throw new ArgumentNullException("c", Environment.GetResourceString("Collection cannot be null."));
		}
		int count = c.Count;
		if (count == 0)
		{
			_items = emptyArray;
			return;
		}
		_items = new object[count];
		AddRange(c);
	}

	/// <summary>Creates an <see cref="T:System.Collections.ArrayList" /> wrapper for a specific <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The <see cref="T:System.Collections.ArrayList" /> wrapper around the <see cref="T:System.Collections.IList" />.</returns>
	/// <param name="list">The <see cref="T:System.Collections.IList" /> to wrap.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="list" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	public static ArrayList Adapter(IList list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		return new IListWrapper(list);
	}

	/// <summary>Adds an object to the end of the <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <returns>The <see cref="T:System.Collections.ArrayList" /> index at which the <paramref name="value" /> has been added.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to be added to the end of the <see cref="T:System.Collections.ArrayList" />. The value can be null. </param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList" /> is read-only.-or- The <see cref="T:System.Collections.ArrayList" /> has a fixed size. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual int Add(object value)
	{
		if (_size == _items.Length)
		{
			EnsureCapacity(_size + 1);
		}
		_items[_size] = value;
		_version++;
		return _size++;
	}

	/// <summary>Adds the elements of an <see cref="T:System.Collections.ICollection" /> to the end of the <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <param name="c">The <see cref="T:System.Collections.ICollection" /> whose elements should be added to the end of the <see cref="T:System.Collections.ArrayList" />. The collection itself cannot be null, but it can contain elements that are null. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="c" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList" /> is read-only.-or- The <see cref="T:System.Collections.ArrayList" /> has a fixed size. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void AddRange(ICollection c)
	{
		InsertRange(_size, c);
	}

	/// <summary>Searches a range of elements in the sorted <see cref="T:System.Collections.ArrayList" /> for an element using the specified comparer and returns the zero-based index of the element.</summary>
	/// <returns>The zero-based index of <paramref name="value" /> in the sorted <see cref="T:System.Collections.ArrayList" />, if <paramref name="value" /> is found; otherwise, a negative number, which is the bitwise complement of the index of the next element that is larger than <paramref name="value" /> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.ArrayList.Count" />.</returns>
	/// <param name="index">The zero-based starting index of the range to search. </param>
	/// <param name="count">The length of the range to search. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to locate. The value can be null. </param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing elements.-or- null to use the default comparer that is the <see cref="T:System.IComparable" /> implementation of each element. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in the <see cref="T:System.Collections.ArrayList" />.-or- <paramref name="comparer" /> is null and neither <paramref name="value" /> nor the elements of <see cref="T:System.Collections.ArrayList" /> implement the <see cref="T:System.IComparable" /> interface. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null and <paramref name="value" /> is not of the same type as the elements of the <see cref="T:System.Collections.ArrayList" />. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="count" /> is less than zero. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual int BinarySearch(int index, int count, object value, IComparer comparer)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (_size - index < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		return Array.BinarySearch(_items, index, count, value, comparer);
	}

	/// <summary>Searches the entire sorted <see cref="T:System.Collections.ArrayList" /> for an element using the default comparer and returns the zero-based index of the element.</summary>
	/// <returns>The zero-based index of <paramref name="value" /> in the sorted <see cref="T:System.Collections.ArrayList" />, if <paramref name="value" /> is found; otherwise, a negative number, which is the bitwise complement of the index of the next element that is larger than <paramref name="value" /> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.ArrayList.Count" />.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate. The value can be null. </param>
	/// <exception cref="T:System.ArgumentException">Neither <paramref name="value" /> nor the elements of <see cref="T:System.Collections.ArrayList" /> implement the <see cref="T:System.IComparable" /> interface. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="value" /> is not of the same type as the elements of the <see cref="T:System.Collections.ArrayList" />. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual int BinarySearch(object value)
	{
		return BinarySearch(0, Count, value, null);
	}

	/// <summary>Searches the entire sorted <see cref="T:System.Collections.ArrayList" /> for an element using the specified comparer and returns the zero-based index of the element.</summary>
	/// <returns>The zero-based index of <paramref name="value" /> in the sorted <see cref="T:System.Collections.ArrayList" />, if <paramref name="value" /> is found; otherwise, a negative number, which is the bitwise complement of the index of the next element that is larger than <paramref name="value" /> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.ArrayList.Count" />.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate. The value can be null. </param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing elements.-or- null to use the default comparer that is the <see cref="T:System.IComparable" /> implementation of each element. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparer" /> is null and neither <paramref name="value" /> nor the elements of <see cref="T:System.Collections.ArrayList" /> implement the <see cref="T:System.IComparable" /> interface. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null and <paramref name="value" /> is not of the same type as the elements of the <see cref="T:System.Collections.ArrayList" />. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual int BinarySearch(object value, IComparer comparer)
	{
		return BinarySearch(0, Count, value, comparer);
	}

	/// <summary>Removes all elements from the <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList" /> is read-only.-or- The <see cref="T:System.Collections.ArrayList" /> has a fixed size. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Clear()
	{
		if (_size > 0)
		{
			Array.Clear(_items, 0, _size);
			_size = 0;
		}
		_version++;
	}

	/// <summary>Creates a shallow copy of the <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <returns>A shallow copy of the <see cref="T:System.Collections.ArrayList" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual object Clone()
	{
		ArrayList arrayList = new ArrayList(_size);
		arrayList._size = _size;
		arrayList._version = _version;
		Array.Copy(_items, 0, arrayList._items, 0, _size);
		return arrayList;
	}

	/// <summary>Determines whether an element is in the <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.ArrayList" />; otherwise, false.</returns>
	/// <param name="item">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.ArrayList" />. The value can be null. </param>
	/// <filterpriority>1</filterpriority>
	public virtual bool Contains(object item)
	{
		if (item == null)
		{
			for (int i = 0; i < _size; i++)
			{
				if (_items[i] == null)
				{
					return true;
				}
			}
			return false;
		}
		for (int j = 0; j < _size; j++)
		{
			if (_items[j] != null && _items[j].Equals(item))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Copies the entire <see cref="T:System.Collections.ArrayList" /> to a compatible one-dimensional <see cref="T:System.Array" />, starting at the beginning of the target array.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ArrayList" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.ArrayList" /> is greater than the number of elements that the destination <paramref name="array" /> can contain. </exception>
	/// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.ArrayList" /> cannot be cast automatically to the type of the destination <paramref name="array" />. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void CopyTo(Array array)
	{
		CopyTo(array, 0);
	}

	/// <summary>Copies the entire <see cref="T:System.Collections.ArrayList" /> to a compatible one-dimensional <see cref="T:System.Array" />, starting at the specified index of the target array.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ArrayList" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="arrayIndex" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.ArrayList" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />. </exception>
	/// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.ArrayList" /> cannot be cast automatically to the type of the destination <paramref name="array" />. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void CopyTo(Array array, int arrayIndex)
	{
		if (array != null && array.Rank != 1)
		{
			throw new ArgumentException(Environment.GetResourceString("Only single dimensional arrays are supported for the requested action."));
		}
		Array.Copy(_items, 0, array, arrayIndex, _size);
	}

	/// <summary>Copies a range of elements from the <see cref="T:System.Collections.ArrayList" /> to a compatible one-dimensional <see cref="T:System.Array" />, starting at the specified index of the target array.</summary>
	/// <param name="index">The zero-based index in the source <see cref="T:System.Collections.ArrayList" /> at which copying begins. </param>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ArrayList" />. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins. </param>
	/// <param name="count">The number of elements to copy. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="arrayIndex" /> is less than zero.-or- <paramref name="count" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or- <paramref name="index" /> is equal to or greater than the <see cref="P:System.Collections.ArrayList.Count" /> of the source <see cref="T:System.Collections.ArrayList" />.-or- The number of elements from <paramref name="index" /> to the end of the source <see cref="T:System.Collections.ArrayList" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />. </exception>
	/// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.ArrayList" /> cannot be cast automatically to the type of the destination <paramref name="array" />. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void CopyTo(int index, Array array, int arrayIndex, int count)
	{
		if (_size - index < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		if (array != null && array.Rank != 1)
		{
			throw new ArgumentException(Environment.GetResourceString("Only single dimensional arrays are supported for the requested action."));
		}
		Array.Copy(_items, index, array, arrayIndex, count);
	}

	private void EnsureCapacity(int min)
	{
		if (_items.Length < min)
		{
			int num = ((_items.Length == 0) ? 4 : (_items.Length * 2));
			if ((uint)num > 2146435071u)
			{
				num = 2146435071;
			}
			if (num < min)
			{
				num = min;
			}
			Capacity = num;
		}
	}

	/// <summary>Returns an <see cref="T:System.Collections.IList" /> wrapper with a fixed size.</summary>
	/// <returns>An <see cref="T:System.Collections.IList" /> wrapper with a fixed size.</returns>
	/// <param name="list">The <see cref="T:System.Collections.IList" /> to wrap. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="list" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public static IList FixedSize(IList list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		return new FixedSizeList(list);
	}

	/// <summary>Returns an <see cref="T:System.Collections.ArrayList" /> wrapper with a fixed size.</summary>
	/// <returns>An <see cref="T:System.Collections.ArrayList" /> wrapper with a fixed size.</returns>
	/// <param name="list">The <see cref="T:System.Collections.ArrayList" /> to wrap. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="list" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public static ArrayList FixedSize(ArrayList list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		return new FixedSizeArrayList(list);
	}

	/// <summary>Returns an enumerator for the entire <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the entire <see cref="T:System.Collections.ArrayList" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual IEnumerator GetEnumerator()
	{
		return new ArrayListEnumeratorSimple(this);
	}

	/// <summary>Returns an enumerator for a range of elements in the <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the specified range of elements in the <see cref="T:System.Collections.ArrayList" />.</returns>
	/// <param name="index">The zero-based starting index of the <see cref="T:System.Collections.ArrayList" /> section that the enumerator should refer to. </param>
	/// <param name="count">The number of elements in the <see cref="T:System.Collections.ArrayList" /> section that the enumerator should refer to. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="count" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="count" /> do not specify a valid range in the <see cref="T:System.Collections.ArrayList" />. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual IEnumerator GetEnumerator(int index, int count)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (_size - index < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		return new ArrayListEnumerator(this, index, count);
	}

	/// <summary>Searches for the specified <see cref="T:System.Object" /> and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the entire <see cref="T:System.Collections.ArrayList" />, if found; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.ArrayList" />. The value can be null. </param>
	/// <filterpriority>1</filterpriority>
	public virtual int IndexOf(object value)
	{
		return Array.IndexOf((Array)_items, value, 0, _size);
	}

	/// <summary>Searches for the specified <see cref="T:System.Object" /> and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.ArrayList" /> that extends from the specified index to the last element.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the range of elements in the <see cref="T:System.Collections.ArrayList" /> that extends from <paramref name="startIndex" /> to the last element, if found; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.ArrayList" />. The value can be null. </param>
	/// <param name="startIndex">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for the <see cref="T:System.Collections.ArrayList" />. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual int IndexOf(object value, int startIndex)
	{
		if (startIndex > _size)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		return Array.IndexOf((Array)_items, value, startIndex, _size - startIndex);
	}

	/// <summary>Searches for the specified <see cref="T:System.Object" /> and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.ArrayList" /> that starts at the specified index and contains the specified number of elements.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the range of elements in the <see cref="T:System.Collections.ArrayList" /> that starts at <paramref name="startIndex" /> and contains <paramref name="count" /> number of elements, if found; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.ArrayList" />. The value can be null. </param>
	/// <param name="startIndex">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
	/// <param name="count">The number of elements in the section to search. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for the <see cref="T:System.Collections.ArrayList" />.-or- <paramref name="count" /> is less than zero.-or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in the <see cref="T:System.Collections.ArrayList" />. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual int IndexOf(object value, int startIndex, int count)
	{
		if (startIndex > _size)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (count < 0 || startIndex > _size - count)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Count must be positive and count must refer to a location within the string/array/collection."));
		}
		return Array.IndexOf((Array)_items, value, startIndex, count);
	}

	/// <summary>Inserts an element into the <see cref="T:System.Collections.ArrayList" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to insert. The value can be null. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="index" /> is greater than <see cref="P:System.Collections.ArrayList.Count" />. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList" /> is read-only.-or- The <see cref="T:System.Collections.ArrayList" /> has a fixed size. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Insert(int index, object value)
	{
		if (index < 0 || index > _size)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Insertion index was out of range. Must be non-negative and less than or equal to size."));
		}
		if (_size == _items.Length)
		{
			EnsureCapacity(_size + 1);
		}
		if (index < _size)
		{
			Array.Copy(_items, index, _items, index + 1, _size - index);
		}
		_items[index] = value;
		_size++;
		_version++;
	}

	/// <summary>Inserts the elements of a collection into the <see cref="T:System.Collections.ArrayList" /> at the specified index.</summary>
	/// <param name="index">The zero-based index at which the new elements should be inserted. </param>
	/// <param name="c">The <see cref="T:System.Collections.ICollection" /> whose elements should be inserted into the <see cref="T:System.Collections.ArrayList" />. The collection itself cannot be null, but it can contain elements that are null. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="c" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="index" /> is greater than <see cref="P:System.Collections.ArrayList.Count" />. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList" /> is read-only.-or- The <see cref="T:System.Collections.ArrayList" /> has a fixed size. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void InsertRange(int index, ICollection c)
	{
		if (c == null)
		{
			throw new ArgumentNullException("c", Environment.GetResourceString("Collection cannot be null."));
		}
		if (index < 0 || index > _size)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		int count = c.Count;
		if (count > 0)
		{
			EnsureCapacity(_size + count);
			if (index < _size)
			{
				Array.Copy(_items, index, _items, index + count, _size - index);
			}
			object[] array = new object[count];
			c.CopyTo(array, 0);
			array.CopyTo(_items, index);
			_size += count;
			_version++;
		}
	}

	/// <summary>Searches for the specified <see cref="T:System.Object" /> and returns the zero-based index of the last occurrence within the entire <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" /> within the entire the <see cref="T:System.Collections.ArrayList" />, if found; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.ArrayList" />. The value can be null. </param>
	/// <filterpriority>2</filterpriority>
	public virtual int LastIndexOf(object value)
	{
		return LastIndexOf(value, _size - 1, _size);
	}

	/// <summary>Searches for the specified <see cref="T:System.Object" /> and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Collections.ArrayList" /> that extends from the first element to the specified index.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" /> within the range of elements in the <see cref="T:System.Collections.ArrayList" /> that extends from the first element to <paramref name="startIndex" />, if found; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.ArrayList" />. The value can be null. </param>
	/// <param name="startIndex">The zero-based starting index of the backward search. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for the <see cref="T:System.Collections.ArrayList" />. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual int LastIndexOf(object value, int startIndex)
	{
		if (startIndex >= _size)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		return LastIndexOf(value, startIndex, startIndex + 1);
	}

	/// <summary>Searches for the specified <see cref="T:System.Object" /> and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Collections.ArrayList" /> that contains the specified number of elements and ends at the specified index.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" /> within the range of elements in the <see cref="T:System.Collections.ArrayList" /> that contains <paramref name="count" /> number of elements and ends at <paramref name="startIndex" />, if found; otherwise, -1.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to locate in the <see cref="T:System.Collections.ArrayList" />. The value can be null. </param>
	/// <param name="startIndex">The zero-based starting index of the backward search. </param>
	/// <param name="count">The number of elements in the section to search. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for the <see cref="T:System.Collections.ArrayList" />.-or- <paramref name="count" /> is less than zero.-or- <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in the <see cref="T:System.Collections.ArrayList" />. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual int LastIndexOf(object value, int startIndex, int count)
	{
		if (Count != 0 && (startIndex < 0 || count < 0))
		{
			throw new ArgumentOutOfRangeException((startIndex < 0) ? "startIndex" : "count", Environment.GetResourceString("Non-negative number required."));
		}
		if (_size == 0)
		{
			return -1;
		}
		if (startIndex >= _size || count > startIndex + 1)
		{
			throw new ArgumentOutOfRangeException((startIndex >= _size) ? "startIndex" : "count", Environment.GetResourceString("Larger than collection size."));
		}
		return Array.LastIndexOf((Array)_items, value, startIndex, count);
	}

	/// <summary>Returns a read-only <see cref="T:System.Collections.IList" /> wrapper.</summary>
	/// <returns>A read-only <see cref="T:System.Collections.IList" /> wrapper around <paramref name="list" />.</returns>
	/// <param name="list">The <see cref="T:System.Collections.IList" /> to wrap. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="list" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public static IList ReadOnly(IList list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		return new ReadOnlyList(list);
	}

	/// <summary>Returns a read-only <see cref="T:System.Collections.ArrayList" /> wrapper.</summary>
	/// <returns>A read-only <see cref="T:System.Collections.ArrayList" /> wrapper around <paramref name="list" />.</returns>
	/// <param name="list">The <see cref="T:System.Collections.ArrayList" /> to wrap. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="list" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public static ArrayList ReadOnly(ArrayList list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		return new ReadOnlyArrayList(list);
	}

	/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to remove from the <see cref="T:System.Collections.ArrayList" />. The value can be null. </param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList" /> is read-only.-or- The <see cref="T:System.Collections.ArrayList" /> has a fixed size. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Remove(object obj)
	{
		int num = IndexOf(obj);
		if (num >= 0)
		{
			RemoveAt(num);
		}
	}

	/// <summary>Removes the element at the specified index of the <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <param name="index">The zero-based index of the element to remove. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.ArrayList.Count" />. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList" /> is read-only.-or- The <see cref="T:System.Collections.ArrayList" /> has a fixed size. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void RemoveAt(int index)
	{
		if (index < 0 || index >= _size)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		_size--;
		if (index < _size)
		{
			Array.Copy(_items, index + 1, _items, index, _size - index);
		}
		_items[_size] = null;
		_version++;
	}

	/// <summary>Removes a range of elements from the <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <param name="index">The zero-based starting index of the range of elements to remove. </param>
	/// <param name="count">The number of elements to remove. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="count" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in the <see cref="T:System.Collections.ArrayList" />. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList" /> is read-only.-or- The <see cref="T:System.Collections.ArrayList" /> has a fixed size. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void RemoveRange(int index, int count)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (_size - index < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		if (count > 0)
		{
			int num = _size;
			_size -= count;
			if (index < _size)
			{
				Array.Copy(_items, index + count, _items, index, _size - index);
			}
			while (num > _size)
			{
				_items[--num] = null;
			}
			_version++;
		}
	}

	/// <summary>Returns an <see cref="T:System.Collections.ArrayList" /> whose elements are copies of the specified value.</summary>
	/// <returns>An <see cref="T:System.Collections.ArrayList" /> with <paramref name="count" /> number of elements, all of which are copies of <paramref name="value" />.</returns>
	/// <param name="value">The <see cref="T:System.Object" /> to copy multiple times in the new <see cref="T:System.Collections.ArrayList" />. The value can be null. </param>
	/// <param name="count">The number of times <paramref name="value" /> should be copied. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero. </exception>
	/// <filterpriority>2</filterpriority>
	public static ArrayList Repeat(object value, int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		ArrayList arrayList = new ArrayList((count > 4) ? count : 4);
		for (int i = 0; i < count; i++)
		{
			arrayList.Add(value);
		}
		return arrayList;
	}

	/// <summary>Reverses the order of the elements in the entire <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList" /> is read-only. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void Reverse()
	{
		Reverse(0, Count);
	}

	/// <summary>Reverses the order of the elements in the specified range.</summary>
	/// <param name="index">The zero-based starting index of the range to reverse. </param>
	/// <param name="count">The number of elements in the range to reverse. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="count" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in the <see cref="T:System.Collections.ArrayList" />. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList" /> is read-only. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void Reverse(int index, int count)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (_size - index < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		Array.Reverse(_items, index, count);
		_version++;
	}

	/// <summary>Copies the elements of a collection over a range of elements in the <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <param name="index">The zero-based <see cref="T:System.Collections.ArrayList" /> index at which to start copying the elements of <paramref name="c" />. </param>
	/// <param name="c">The <see cref="T:System.Collections.ICollection" /> whose elements to copy to the <see cref="T:System.Collections.ArrayList" />. The collection itself cannot be null, but it can contain elements that are null. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="index" /> plus the number of elements in <paramref name="c" /> is greater than <see cref="P:System.Collections.ArrayList.Count" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="c" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList" /> is read-only. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void SetRange(int index, ICollection c)
	{
		if (c == null)
		{
			throw new ArgumentNullException("c", Environment.GetResourceString("Collection cannot be null."));
		}
		int count = c.Count;
		if (index < 0 || index > _size - count)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (count > 0)
		{
			c.CopyTo(_items, index);
			_version++;
		}
	}

	/// <summary>Returns an <see cref="T:System.Collections.ArrayList" /> which represents a subset of the elements in the source <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ArrayList" /> which represents a subset of the elements in the source <see cref="T:System.Collections.ArrayList" />.</returns>
	/// <param name="index">The zero-based <see cref="T:System.Collections.ArrayList" /> index at which the range starts. </param>
	/// <param name="count">The number of elements in the range. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="count" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in the <see cref="T:System.Collections.ArrayList" />. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual ArrayList GetRange(int index, int count)
	{
		if (index < 0 || count < 0)
		{
			throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("Non-negative number required."));
		}
		if (_size - index < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		return new Range(this, index, count);
	}

	/// <summary>Sorts the elements in the entire <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList" /> is read-only. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Sort()
	{
		Sort(0, Count, Comparer.Default);
	}

	/// <summary>Sorts the elements in the entire <see cref="T:System.Collections.ArrayList" /> using the specified comparer.</summary>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing elements.-or- A null reference (Nothing in Visual Basic) to use the <see cref="T:System.IComparable" /> implementation of each element. </param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList" /> is read-only. </exception>
	/// <exception cref="T:System.InvalidOperationException">An error occurred while comparing two elements.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Sort(IComparer comparer)
	{
		Sort(0, Count, comparer);
	}

	/// <summary>Sorts the elements in a range of elements in <see cref="T:System.Collections.ArrayList" /> using the specified comparer.</summary>
	/// <param name="index">The zero-based starting index of the range to sort. </param>
	/// <param name="count">The length of the range to sort. </param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing elements.-or- A null reference (Nothing in Visual Basic) to use the <see cref="T:System.IComparable" /> implementation of each element. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or- <paramref name="count" /> is less than zero. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="count" /> do not specify a valid range in the <see cref="T:System.Collections.ArrayList" />. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList" /> is read-only. </exception>
	/// <exception cref="T:System.InvalidOperationException">An error occurred while comparing two elements.</exception>
	/// <filterpriority>1</filterpriority>
	public virtual void Sort(int index, int count, IComparer comparer)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("Non-negative number required."));
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Non-negative number required."));
		}
		if (_size - index < count)
		{
			throw new ArgumentException(Environment.GetResourceString("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
		}
		Array.Sort(_items, index, count, comparer);
		_version++;
	}

	/// <summary>Returns an <see cref="T:System.Collections.IList" /> wrapper that is synchronized (thread safe).</summary>
	/// <returns>An <see cref="T:System.Collections.IList" /> wrapper that is synchronized (thread safe).</returns>
	/// <param name="list">The <see cref="T:System.Collections.IList" /> to synchronize. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="list" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
	public static IList Synchronized(IList list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		return new SyncIList(list);
	}

	/// <summary>Returns an <see cref="T:System.Collections.ArrayList" /> wrapper that is synchronized (thread safe).</summary>
	/// <returns>An <see cref="T:System.Collections.ArrayList" /> wrapper that is synchronized (thread safe).</returns>
	/// <param name="list">The <see cref="T:System.Collections.ArrayList" /> to synchronize. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="list" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
	public static ArrayList Synchronized(ArrayList list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		return new SyncArrayList(list);
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.ArrayList" /> to a new <see cref="T:System.Object" /> array.</summary>
	/// <returns>An <see cref="T:System.Object" /> array containing copies of the elements of the <see cref="T:System.Collections.ArrayList" />.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual object[] ToArray()
	{
		object[] array = new object[_size];
		Array.Copy(_items, 0, array, 0, _size);
		return array;
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.ArrayList" /> to a new array of the specified element type.</summary>
	/// <returns>An array of the specified element type containing copies of the elements of the <see cref="T:System.Collections.ArrayList" />.</returns>
	/// <param name="type">The element <see cref="T:System.Type" /> of the destination array to create and copy elements to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null. </exception>
	/// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.ArrayList" /> cannot be cast automatically to the specified type. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public virtual Array ToArray(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		Array array = Array.UnsafeCreateInstance(type, _size);
		Array.Copy(_items, 0, array, 0, _size);
		return array;
	}

	/// <summary>Sets the capacity to the actual number of elements in the <see cref="T:System.Collections.ArrayList" />.</summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.ArrayList" /> is read-only.-or- The <see cref="T:System.Collections.ArrayList" /> has a fixed size. </exception>
	/// <filterpriority>2</filterpriority>
	public virtual void TrimToSize()
	{
		Capacity = _size;
	}
}
