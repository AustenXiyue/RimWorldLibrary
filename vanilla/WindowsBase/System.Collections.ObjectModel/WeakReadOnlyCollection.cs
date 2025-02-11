using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using MS.Internal.WindowsBase;

namespace System.Collections.ObjectModel;

[Serializable]
[ComVisible(false)]
[DebuggerDisplay("Count = {Count}")]
[MS.Internal.WindowsBase.FriendAccessAllowed]
internal class WeakReadOnlyCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection
{
	private class WeakEnumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private IEnumerator ie;

		public T Current
		{
			get
			{
				if (ie.Current is WeakReference weakReference)
				{
					return (T)weakReference.Target;
				}
				return default(T);
			}
		}

		object IEnumerator.Current => Current;

		public WeakEnumerator(IEnumerator ie)
		{
			this.ie = ie;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			return ie.MoveNext();
		}

		void IEnumerator.Reset()
		{
			ie.Reset();
		}
	}

	private IList<WeakReference> list;

	[NonSerialized]
	private object _syncRoot;

	public int Count => list.Count;

	public T this[int index] => (T)list[index].Target;

	bool ICollection<T>.IsReadOnly => true;

	T IList<T>.this[int index]
	{
		get
		{
			return (T)list[index].Target;
		}
		set
		{
			throw new NotSupportedException(SR.NotSupported_ReadOnlyCollection);
		}
	}

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				if (list is ICollection collection)
				{
					_syncRoot = collection.SyncRoot;
				}
				else
				{
					Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
				}
			}
			return _syncRoot;
		}
	}

	bool IList.IsFixedSize => true;

	bool IList.IsReadOnly => true;

	object IList.this[int index]
	{
		get
		{
			return (T)list[index].Target;
		}
		set
		{
			throw new NotSupportedException(SR.NotSupported_ReadOnlyCollection);
		}
	}

	public WeakReadOnlyCollection(IList<WeakReference> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		this.list = list;
	}

	public bool Contains(T value)
	{
		return CreateDereferencedList().Contains(value);
	}

	public void CopyTo(T[] array, int index)
	{
		CreateDereferencedList().CopyTo(array, index);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return new WeakEnumerator(list.GetEnumerator());
	}

	public int IndexOf(T value)
	{
		return CreateDereferencedList().IndexOf(value);
	}

	void ICollection<T>.Add(T value)
	{
		throw new NotSupportedException(SR.NotSupported_ReadOnlyCollection);
	}

	void ICollection<T>.Clear()
	{
		throw new NotSupportedException(SR.NotSupported_ReadOnlyCollection);
	}

	void IList<T>.Insert(int index, T value)
	{
		throw new NotSupportedException(SR.NotSupported_ReadOnlyCollection);
	}

	bool ICollection<T>.Remove(T value)
	{
		throw new NotSupportedException(SR.NotSupported_ReadOnlyCollection);
	}

	void IList<T>.RemoveAt(int index)
	{
		throw new NotSupportedException(SR.NotSupported_ReadOnlyCollection);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new WeakEnumerator(((IEnumerable)list).GetEnumerator());
	}

	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(SR.Arg_RankMultiDimNotSupported);
		}
		if (array.GetLowerBound(0) != 0)
		{
			throw new ArgumentException(SR.Arg_NonZeroLowerBound);
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", SR.ArgumentOutOfRange_NeedNonNegNum);
		}
		if (array.Length - index < Count)
		{
			throw new ArgumentException(SR.Arg_ArrayPlusOffTooSmall);
		}
		IList<T> list = CreateDereferencedList();
		if (array is T[] array2)
		{
			list.CopyTo(array2, index);
			return;
		}
		Type elementType = array.GetType().GetElementType();
		Type typeFromHandle = typeof(T);
		if (!elementType.IsAssignableFrom(typeFromHandle) && !typeFromHandle.IsAssignableFrom(elementType))
		{
			throw new ArgumentException(SR.Argument_InvalidArrayType);
		}
		if (!(array is object[] array3))
		{
			throw new ArgumentException(SR.Argument_InvalidArrayType);
		}
		int count = list.Count;
		try
		{
			for (int i = 0; i < count; i++)
			{
				array3[index++] = list[i];
			}
		}
		catch (ArrayTypeMismatchException)
		{
			throw new ArgumentException(SR.Argument_InvalidArrayType);
		}
	}

	int IList.Add(object value)
	{
		throw new NotSupportedException(SR.NotSupported_ReadOnlyCollection);
	}

	void IList.Clear()
	{
		throw new NotSupportedException(SR.NotSupported_ReadOnlyCollection);
	}

	private static bool IsCompatibleObject(object value)
	{
		if (!(value is T))
		{
			if (value == null)
			{
				return default(T) == null;
			}
			return false;
		}
		return true;
	}

	bool IList.Contains(object value)
	{
		if (IsCompatibleObject(value))
		{
			return Contains((T)value);
		}
		return false;
	}

	int IList.IndexOf(object value)
	{
		if (IsCompatibleObject(value))
		{
			return IndexOf((T)value);
		}
		return -1;
	}

	void IList.Insert(int index, object value)
	{
		throw new NotSupportedException(SR.NotSupported_ReadOnlyCollection);
	}

	void IList.Remove(object value)
	{
		throw new NotSupportedException(SR.NotSupported_ReadOnlyCollection);
	}

	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException(SR.NotSupported_ReadOnlyCollection);
	}

	private IList<T> CreateDereferencedList()
	{
		int count = this.list.Count;
		List<T> list = new List<T>(count);
		for (int i = 0; i < count; i++)
		{
			list.Add((T)this.list[i].Target);
		}
		return list;
	}
}
