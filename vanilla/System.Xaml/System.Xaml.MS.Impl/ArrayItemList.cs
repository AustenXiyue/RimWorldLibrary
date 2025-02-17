using System.Collections;
using System.Collections.Generic;

namespace System.Xaml.MS.Impl;

internal sealed class ArrayItemList<T> : FrugalListBase<T>
{
	internal class ArrayCompacter : Compacter
	{
		private ArrayItemList<T> _targetStore;

		private T[] _sourceArray;

		private T[] _targetArray;

		public ArrayCompacter(ArrayItemList<T> store, int newCount)
			: base((FrugalListBase<T>)store, newCount)
		{
			_sourceArray = store._entries;
			int num = Math.Max(newCount + (newCount >> 2), 9);
			if (num + (num >> 2) >= _sourceArray.Length)
			{
				_targetStore = store;
			}
			else
			{
				_targetStore = new ArrayItemList<T>(num);
			}
			_targetArray = _targetStore._entries;
		}

		protected override void IncludeOverride(int start, int end)
		{
			int num = end - start;
			Array.Copy(_sourceArray, start, _targetArray, _validItemCount, num);
			_validItemCount += num;
		}

		public override FrugalListBase<T> Finish()
		{
			T val = default(T);
			if (_sourceArray == _targetArray)
			{
				int i = _validItemCount;
				for (int count = _store.Count; i < count; i++)
				{
					_sourceArray[i] = val;
				}
			}
			_targetStore._count = _validItemCount;
			return _targetStore;
		}
	}

	private const int MINSIZE = 9;

	private const int GROWTH = 3;

	private const int LARGEGROWTH = 18;

	private T[] _entries;

	public override int Capacity => _entries.Length;

	public ArrayItemList(int size)
	{
		size += 2;
		size -= size % 3;
		_entries = new T[size];
	}

	public ArrayItemList(ICollection collection)
	{
		_count = collection.Count;
		_entries = new T[_count];
		collection.CopyTo(_entries, 0);
	}

	public ArrayItemList(ICollection<T> collection)
	{
		_count = collection.Count;
		_entries = new T[_count];
		collection.CopyTo(_entries, 0);
	}

	public override FrugalListStoreState Add(T value)
	{
		if (_count < _entries.Length)
		{
			_entries[_count] = value;
			_count++;
		}
		else
		{
			int num = _entries.Length;
			num = ((num >= 18) ? (num + (num >> 2)) : (num + 3));
			T[] array = new T[num];
			Array.Copy(_entries, 0, array, 0, _entries.Length);
			_entries = array;
			_entries[_count] = value;
			_count++;
		}
		return FrugalListStoreState.Success;
	}

	public override void Clear()
	{
		_entries.AsSpan(0, _count).Clear();
		_count = 0;
	}

	public override bool Contains(T value)
	{
		return IndexOf(value) >= 0;
	}

	public override int IndexOf(T value)
	{
		return Array.IndexOf(_entries, value, 0, _count);
	}

	public override void Insert(int index, T value)
	{
		if (_count < _entries.Length)
		{
			Array.Copy(_entries, index, _entries, index + 1, _count - index);
			_entries[index] = value;
			_count++;
			return;
		}
		throw new ArgumentOutOfRangeException("index");
	}

	public override void SetAt(int index, T value)
	{
		_entries[index] = value;
	}

	public override bool Remove(T value)
	{
		int num = IndexOf(value);
		if (num >= 0)
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	public override void RemoveAt(int index)
	{
		int num = _count - index - 1;
		if (num > 0)
		{
			Array.Copy(_entries, index + 1, _entries, index, num);
		}
		_entries[_count - 1] = default(T);
		_count--;
	}

	public override T EntryAt(int index)
	{
		return _entries[index];
	}

	public override void Promote(FrugalListBase<T> oldList)
	{
		for (int i = 0; i < oldList.Count; i++)
		{
			if (Add(oldList.EntryAt(i)) != 0)
			{
				throw new ArgumentException(System.SR.Format(System.SR.FrugalList_TargetMapCannotHoldAllData, oldList.ToString(), ToString()), "oldList");
			}
		}
	}

	public void Promote(SixItemList<T> oldList)
	{
		int count = oldList.Count;
		SetCount(oldList.Count);
		switch (count)
		{
		case 6:
			SetAt(0, oldList.EntryAt(0));
			SetAt(1, oldList.EntryAt(1));
			SetAt(2, oldList.EntryAt(2));
			SetAt(3, oldList.EntryAt(3));
			SetAt(4, oldList.EntryAt(4));
			SetAt(5, oldList.EntryAt(5));
			break;
		case 5:
			SetAt(0, oldList.EntryAt(0));
			SetAt(1, oldList.EntryAt(1));
			SetAt(2, oldList.EntryAt(2));
			SetAt(3, oldList.EntryAt(3));
			SetAt(4, oldList.EntryAt(4));
			break;
		case 4:
			SetAt(0, oldList.EntryAt(0));
			SetAt(1, oldList.EntryAt(1));
			SetAt(2, oldList.EntryAt(2));
			SetAt(3, oldList.EntryAt(3));
			break;
		case 3:
			SetAt(0, oldList.EntryAt(0));
			SetAt(1, oldList.EntryAt(1));
			SetAt(2, oldList.EntryAt(2));
			break;
		case 2:
			SetAt(0, oldList.EntryAt(0));
			SetAt(1, oldList.EntryAt(1));
			break;
		case 1:
			SetAt(0, oldList.EntryAt(0));
			break;
		default:
			throw new ArgumentOutOfRangeException("oldList");
		case 0:
			break;
		}
	}

	public void Promote(ArrayItemList<T> oldList)
	{
		int count = oldList.Count;
		if (_entries.Length >= count)
		{
			SetCount(oldList.Count);
			for (int i = 0; i < count; i++)
			{
				SetAt(i, oldList.EntryAt(i));
			}
			return;
		}
		throw new ArgumentException(System.SR.Format(System.SR.FrugalList_TargetMapCannotHoldAllData, oldList.ToString(), ToString()), "oldList");
	}

	public override T[] ToArray()
	{
		return _entries.AsSpan(0, _count).ToArray();
	}

	public override void CopyTo(T[] array, int index)
	{
		_entries.AsSpan(0, _count).CopyTo(array.AsSpan(index));
	}

	public override object Clone()
	{
		ArrayItemList<T> arrayItemList = new ArrayItemList<T>(Capacity);
		arrayItemList.Promote(this);
		return arrayItemList;
	}

	private void SetCount(int value)
	{
		if (value >= 0 && value <= _entries.Length)
		{
			_count = value;
			return;
		}
		throw new ArgumentOutOfRangeException("value");
	}

	public override Compacter NewCompacter(int newCount)
	{
		return new ArrayCompacter(this, newCount);
	}
}
