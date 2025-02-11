using System;
using System.Collections;
using System.Collections.Generic;

namespace MS.Internal;

internal class ListOfObject : IList<object>, ICollection<object>, IEnumerable<object>, IEnumerable
{
	private class ObjectEnumerator : IEnumerator<object>, IEnumerator, IDisposable
	{
		private IEnumerator _ie;

		object IEnumerator<object>.Current => _ie.Current;

		object IEnumerator.Current => _ie.Current;

		public ObjectEnumerator(IList list)
		{
			_ie = list.GetEnumerator();
		}

		void IDisposable.Dispose()
		{
			_ie = null;
		}

		bool IEnumerator.MoveNext()
		{
			return _ie.MoveNext();
		}

		void IEnumerator.Reset()
		{
			_ie.Reset();
		}
	}

	private IList _list;

	object IList<object>.this[int index]
	{
		get
		{
			return _list[index];
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	int ICollection<object>.Count => _list.Count;

	bool ICollection<object>.IsReadOnly => true;

	internal ListOfObject(IList list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		_list = list;
	}

	int IList<object>.IndexOf(object item)
	{
		return _list.IndexOf(item);
	}

	void IList<object>.Insert(int index, object item)
	{
		throw new NotImplementedException();
	}

	void IList<object>.RemoveAt(int index)
	{
		throw new NotImplementedException();
	}

	void ICollection<object>.Add(object item)
	{
		throw new NotImplementedException();
	}

	void ICollection<object>.Clear()
	{
		throw new NotImplementedException();
	}

	bool ICollection<object>.Contains(object item)
	{
		return _list.Contains(item);
	}

	void ICollection<object>.CopyTo(object[] array, int arrayIndex)
	{
		_list.CopyTo(array, arrayIndex);
	}

	bool ICollection<object>.Remove(object item)
	{
		throw new NotImplementedException();
	}

	IEnumerator<object> IEnumerable<object>.GetEnumerator()
	{
		return new ObjectEnumerator(_list);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<object>)this).GetEnumerator();
	}
}
