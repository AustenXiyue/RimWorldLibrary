using System.Collections;
using System.Collections.Generic;

namespace System.Linq;

/// <summary>Provides a set of static (Shared in Visual Basic) methods for querying objects that implement <see cref="T:System.Collections.Generic.IEnumerable`1" />.</summary>
public static class Enumerable
{
	private abstract class AppendPrependIterator<TSource> : Iterator<TSource>, IIListProvider<TSource>, IEnumerable<TSource>, IEnumerable
	{
		protected readonly IEnumerable<TSource> _source;

		protected IEnumerator<TSource> _enumerator;

		protected AppendPrependIterator(IEnumerable<TSource> source)
		{
			_source = source;
		}

		protected void GetSourceEnumerator()
		{
			_enumerator = _source.GetEnumerator();
		}

		public abstract AppendPrependIterator<TSource> Append(TSource item);

		public abstract AppendPrependIterator<TSource> Prepend(TSource item);

		protected bool LoadFromEnumerator()
		{
			if (_enumerator.MoveNext())
			{
				_current = _enumerator.Current;
				return true;
			}
			Dispose();
			return false;
		}

		public override void Dispose()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
			}
			base.Dispose();
		}

		public abstract TSource[] ToArray();

		public abstract List<TSource> ToList();

		public abstract int GetCount(bool onlyIfCheap);
	}

	private class AppendPrepend1Iterator<TSource> : AppendPrependIterator<TSource>
	{
		private readonly TSource _item;

		private readonly bool _appending;

		public AppendPrepend1Iterator(IEnumerable<TSource> source, TSource item, bool appending)
			: base(source)
		{
			_item = item;
			_appending = appending;
		}

		public override Iterator<TSource> Clone()
		{
			return new AppendPrepend1Iterator<TSource>(_source, _item, _appending);
		}

		public override bool MoveNext()
		{
			switch (_state)
			{
			case 1:
				_state = 2;
				if (!_appending)
				{
					_current = _item;
					return true;
				}
				goto case 2;
			case 2:
				GetSourceEnumerator();
				_state = 3;
				goto case 3;
			case 3:
				if (LoadFromEnumerator())
				{
					return true;
				}
				if (_appending)
				{
					_current = _item;
					return true;
				}
				break;
			}
			Dispose();
			return false;
		}

		public override AppendPrependIterator<TSource> Append(TSource item)
		{
			if (_appending)
			{
				return new AppendPrependN<TSource>(_source, null, new SingleLinkedNode<TSource>(_item).Add(item), 0, 2);
			}
			return new AppendPrependN<TSource>(_source, new SingleLinkedNode<TSource>(_item), new SingleLinkedNode<TSource>(item), 1, 1);
		}

		public override AppendPrependIterator<TSource> Prepend(TSource item)
		{
			if (_appending)
			{
				return new AppendPrependN<TSource>(_source, new SingleLinkedNode<TSource>(item), new SingleLinkedNode<TSource>(_item), 1, 1);
			}
			return new AppendPrependN<TSource>(_source, new SingleLinkedNode<TSource>(_item).Add(item), null, 2, 0);
		}

		private TSource[] LazyToArray()
		{
			System.Collections.Generic.LargeArrayBuilder<TSource> largeArrayBuilder = new System.Collections.Generic.LargeArrayBuilder<TSource>(initialize: true);
			if (!_appending)
			{
				largeArrayBuilder.SlowAdd(_item);
			}
			largeArrayBuilder.AddRange(_source);
			if (_appending)
			{
				largeArrayBuilder.SlowAdd(_item);
			}
			return largeArrayBuilder.ToArray();
		}

		public override TSource[] ToArray()
		{
			int count = GetCount(onlyIfCheap: true);
			if (count == -1)
			{
				return LazyToArray();
			}
			TSource[] array = new TSource[count];
			int arrayIndex;
			if (_appending)
			{
				arrayIndex = 0;
			}
			else
			{
				array[0] = _item;
				arrayIndex = 1;
			}
			System.Collections.Generic.EnumerableHelpers.Copy(_source, array, arrayIndex, count - 1);
			if (_appending)
			{
				array[array.Length - 1] = _item;
			}
			return array;
		}

		public override List<TSource> ToList()
		{
			int count = GetCount(onlyIfCheap: true);
			List<TSource> list = ((count == -1) ? new List<TSource>() : new List<TSource>(count));
			if (!_appending)
			{
				list.Add(_item);
			}
			list.AddRange(_source);
			if (_appending)
			{
				list.Add(_item);
			}
			return list;
		}

		public override int GetCount(bool onlyIfCheap)
		{
			if (_source is IIListProvider<TSource> iIListProvider)
			{
				int count = iIListProvider.GetCount(onlyIfCheap);
				if (count != -1)
				{
					return count + 1;
				}
				return -1;
			}
			if (onlyIfCheap && !(_source is ICollection<TSource>))
			{
				return -1;
			}
			return _source.Count() + 1;
		}
	}

	private class AppendPrependN<TSource> : AppendPrependIterator<TSource>
	{
		private readonly SingleLinkedNode<TSource> _prepended;

		private readonly SingleLinkedNode<TSource> _appended;

		private readonly int _prependCount;

		private readonly int _appendCount;

		private SingleLinkedNode<TSource> _node;

		public AppendPrependN(IEnumerable<TSource> source, SingleLinkedNode<TSource> prepended, SingleLinkedNode<TSource> appended, int prependCount, int appendCount)
			: base(source)
		{
			_prepended = prepended;
			_appended = appended;
			_prependCount = prependCount;
			_appendCount = appendCount;
		}

		public override Iterator<TSource> Clone()
		{
			return new AppendPrependN<TSource>(_source, _prepended, _appended, _prependCount, _appendCount);
		}

		public override bool MoveNext()
		{
			switch (_state)
			{
			case 1:
				_node = _prepended;
				_state = 2;
				goto case 2;
			case 2:
				if (_node != null)
				{
					_current = _node.Item;
					_node = _node.Linked;
					return true;
				}
				GetSourceEnumerator();
				_state = 3;
				goto case 3;
			case 3:
				if (LoadFromEnumerator())
				{
					return true;
				}
				if (_appended == null)
				{
					return false;
				}
				_enumerator = _appended.GetEnumerator(_appendCount);
				_state = 4;
				goto case 4;
			case 4:
				return LoadFromEnumerator();
			default:
				Dispose();
				return false;
			}
		}

		public override AppendPrependIterator<TSource> Append(TSource item)
		{
			SingleLinkedNode<TSource> appended = ((_appended != null) ? _appended.Add(item) : new SingleLinkedNode<TSource>(item));
			return new AppendPrependN<TSource>(_source, _prepended, appended, _prependCount, _appendCount + 1);
		}

		public override AppendPrependIterator<TSource> Prepend(TSource item)
		{
			SingleLinkedNode<TSource> prepended = ((_prepended != null) ? _prepended.Add(item) : new SingleLinkedNode<TSource>(item));
			return new AppendPrependN<TSource>(_source, prepended, _appended, _prependCount + 1, _appendCount);
		}

		private TSource[] LazyToArray()
		{
			SparseArrayBuilder<TSource> sparseArrayBuilder = new SparseArrayBuilder<TSource>(initialize: true);
			if (_prepended != null)
			{
				sparseArrayBuilder.Reserve(_prependCount);
			}
			sparseArrayBuilder.AddRange(_source);
			if (_appended != null)
			{
				sparseArrayBuilder.Reserve(_appendCount);
			}
			TSource[] array = sparseArrayBuilder.ToArray();
			int num = 0;
			for (SingleLinkedNode<TSource> singleLinkedNode = _prepended; singleLinkedNode != null; singleLinkedNode = singleLinkedNode.Linked)
			{
				array[num++] = singleLinkedNode.Item;
			}
			num = array.Length - 1;
			for (SingleLinkedNode<TSource> singleLinkedNode2 = _appended; singleLinkedNode2 != null; singleLinkedNode2 = singleLinkedNode2.Linked)
			{
				array[num--] = singleLinkedNode2.Item;
			}
			return array;
		}

		public override TSource[] ToArray()
		{
			int count = GetCount(onlyIfCheap: true);
			if (count == -1)
			{
				return LazyToArray();
			}
			TSource[] array = new TSource[count];
			int num = 0;
			for (SingleLinkedNode<TSource> singleLinkedNode = _prepended; singleLinkedNode != null; singleLinkedNode = singleLinkedNode.Linked)
			{
				array[num] = singleLinkedNode.Item;
				num++;
			}
			if (_source is ICollection<TSource> collection)
			{
				collection.CopyTo(array, num);
			}
			else
			{
				foreach (TSource item in _source)
				{
					array[num] = item;
					num++;
				}
			}
			num = array.Length;
			for (SingleLinkedNode<TSource> singleLinkedNode2 = _appended; singleLinkedNode2 != null; singleLinkedNode2 = singleLinkedNode2.Linked)
			{
				num--;
				array[num] = singleLinkedNode2.Item;
			}
			return array;
		}

		public override List<TSource> ToList()
		{
			int count = GetCount(onlyIfCheap: true);
			List<TSource> list = ((count == -1) ? new List<TSource>() : new List<TSource>(count));
			for (SingleLinkedNode<TSource> singleLinkedNode = _prepended; singleLinkedNode != null; singleLinkedNode = singleLinkedNode.Linked)
			{
				list.Add(singleLinkedNode.Item);
			}
			list.AddRange(_source);
			if (_appended != null)
			{
				IEnumerator<TSource> enumerator = _appended.GetEnumerator(_appendCount);
				while (enumerator.MoveNext())
				{
					list.Add(enumerator.Current);
				}
			}
			return list;
		}

		public override int GetCount(bool onlyIfCheap)
		{
			if (_source is IIListProvider<TSource> iIListProvider)
			{
				int count = iIListProvider.GetCount(onlyIfCheap);
				if (count != -1)
				{
					return count + _appendCount + _prependCount;
				}
				return -1;
			}
			if (onlyIfCheap && !(_source is ICollection<TSource>))
			{
				return -1;
			}
			return _source.Count() + _appendCount + _prependCount;
		}
	}

	private sealed class Concat2Iterator<TSource> : ConcatIterator<TSource>
	{
		internal readonly IEnumerable<TSource> _first;

		internal readonly IEnumerable<TSource> _second;

		internal Concat2Iterator(IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			_first = first;
			_second = second;
		}

		public override Iterator<TSource> Clone()
		{
			return new Concat2Iterator<TSource>(_first, _second);
		}

		internal override ConcatIterator<TSource> Concat(IEnumerable<TSource> next)
		{
			bool hasOnlyCollections = next is ICollection<TSource> && _first is ICollection<TSource> && _second is ICollection<TSource>;
			return new ConcatNIterator<TSource>(this, next, 2, hasOnlyCollections);
		}

		public override int GetCount(bool onlyIfCheap)
		{
			if (!System.Collections.Generic.EnumerableHelpers.TryGetCount(_first, out var count))
			{
				if (onlyIfCheap)
				{
					return -1;
				}
				count = _first.Count();
			}
			if (!System.Collections.Generic.EnumerableHelpers.TryGetCount(_second, out var count2))
			{
				if (onlyIfCheap)
				{
					return -1;
				}
				count2 = _second.Count();
			}
			return checked(count + count2);
		}

		internal override IEnumerable<TSource> GetEnumerable(int index)
		{
			return index switch
			{
				0 => _first, 
				1 => _second, 
				_ => null, 
			};
		}

		public override TSource[] ToArray()
		{
			SparseArrayBuilder<TSource> sparseArrayBuilder = new SparseArrayBuilder<TSource>(initialize: true);
			bool num = sparseArrayBuilder.ReserveOrAdd(_first);
			bool flag = sparseArrayBuilder.ReserveOrAdd(_second);
			TSource[] array = sparseArrayBuilder.ToArray();
			if (num)
			{
				Marker marker = sparseArrayBuilder.Markers.First();
				System.Collections.Generic.EnumerableHelpers.Copy(_first, array, 0, marker.Count);
			}
			if (flag)
			{
				Marker marker2 = sparseArrayBuilder.Markers.Last();
				System.Collections.Generic.EnumerableHelpers.Copy(_second, array, marker2.Index, marker2.Count);
			}
			return array;
		}
	}

	private sealed class ConcatNIterator<TSource> : ConcatIterator<TSource>
	{
		private readonly ConcatIterator<TSource> _tail;

		private readonly IEnumerable<TSource> _head;

		private readonly int _headIndex;

		private readonly bool _hasOnlyCollections;

		private ConcatNIterator<TSource> PreviousN => _tail as ConcatNIterator<TSource>;

		internal ConcatNIterator(ConcatIterator<TSource> tail, IEnumerable<TSource> head, int headIndex, bool hasOnlyCollections)
		{
			_tail = tail;
			_head = head;
			_headIndex = headIndex;
			_hasOnlyCollections = hasOnlyCollections;
		}

		public override Iterator<TSource> Clone()
		{
			return new ConcatNIterator<TSource>(_tail, _head, _headIndex, _hasOnlyCollections);
		}

		internal override ConcatIterator<TSource> Concat(IEnumerable<TSource> next)
		{
			if (_headIndex == 2147483645)
			{
				return new Concat2Iterator<TSource>(this, next);
			}
			bool hasOnlyCollections = _hasOnlyCollections && next is ICollection<TSource>;
			return new ConcatNIterator<TSource>(this, next, _headIndex + 1, hasOnlyCollections);
		}

		public override int GetCount(bool onlyIfCheap)
		{
			if (onlyIfCheap && !_hasOnlyCollections)
			{
				return -1;
			}
			int num = 0;
			ConcatNIterator<TSource> concatNIterator = this;
			checked
			{
				ConcatNIterator<TSource> concatNIterator2;
				do
				{
					concatNIterator2 = concatNIterator;
					IEnumerable<TSource> head = concatNIterator2._head;
					int num2 = (head as ICollection<TSource>)?.Count ?? head.Count();
					num += num2;
				}
				while ((concatNIterator = concatNIterator2.PreviousN) != null);
				return num + concatNIterator2._tail.GetCount(onlyIfCheap);
			}
		}

		internal override IEnumerable<TSource> GetEnumerable(int index)
		{
			if (index > _headIndex)
			{
				return null;
			}
			ConcatNIterator<TSource> concatNIterator = this;
			ConcatNIterator<TSource> concatNIterator2;
			do
			{
				concatNIterator2 = concatNIterator;
				if (index == concatNIterator2._headIndex)
				{
					return concatNIterator2._head;
				}
			}
			while ((concatNIterator = concatNIterator2.PreviousN) != null);
			return concatNIterator2._tail.GetEnumerable(index);
		}

		public override TSource[] ToArray()
		{
			if (!_hasOnlyCollections)
			{
				return LazyToArray();
			}
			return PreallocatingToArray();
		}

		private TSource[] LazyToArray()
		{
			SparseArrayBuilder<TSource> sparseArrayBuilder = new SparseArrayBuilder<TSource>(initialize: true);
			System.Collections.Generic.ArrayBuilder<int> arrayBuilder = default(System.Collections.Generic.ArrayBuilder<int>);
			int num = 0;
			while (true)
			{
				IEnumerable<TSource> enumerable = GetEnumerable(num);
				if (enumerable == null)
				{
					break;
				}
				if (sparseArrayBuilder.ReserveOrAdd(enumerable))
				{
					arrayBuilder.Add(num);
				}
				num++;
			}
			TSource[] array = sparseArrayBuilder.ToArray();
			System.Collections.Generic.ArrayBuilder<Marker> markers = sparseArrayBuilder.Markers;
			for (int i = 0; i < markers.Count; i++)
			{
				Marker marker = markers[i];
				System.Collections.Generic.EnumerableHelpers.Copy(GetEnumerable(arrayBuilder[i]), array, marker.Index, marker.Count);
			}
			return array;
		}

		private TSource[] PreallocatingToArray()
		{
			int count = GetCount(onlyIfCheap: true);
			if (count == 0)
			{
				return Array.Empty<TSource>();
			}
			TSource[] array = new TSource[count];
			int num = array.Length;
			ConcatNIterator<TSource> concatNIterator = this;
			checked
			{
				ConcatNIterator<TSource> concatNIterator2;
				do
				{
					concatNIterator2 = concatNIterator;
					ICollection<TSource> collection = (ICollection<TSource>)concatNIterator2._head;
					int count2 = collection.Count;
					if (count2 > 0)
					{
						num -= count2;
						collection.CopyTo(array, num);
					}
				}
				while ((concatNIterator = concatNIterator2.PreviousN) != null);
				Concat2Iterator<TSource> concat2Iterator = (Concat2Iterator<TSource>)concatNIterator2._tail;
				ICollection<TSource> collection2 = (ICollection<TSource>)concat2Iterator._second;
				int count3 = collection2.Count;
				if (count3 > 0)
				{
					collection2.CopyTo(array, num - count3);
				}
				if (num > count3)
				{
					((ICollection<TSource>)concat2Iterator._first).CopyTo(array, 0);
				}
				return array;
			}
		}
	}

	private abstract class ConcatIterator<TSource> : Iterator<TSource>, IIListProvider<TSource>, IEnumerable<TSource>, IEnumerable
	{
		private IEnumerator<TSource> _enumerator;

		public override void Dispose()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
			}
			base.Dispose();
		}

		internal abstract IEnumerable<TSource> GetEnumerable(int index);

		internal abstract ConcatIterator<TSource> Concat(IEnumerable<TSource> next);

		public override bool MoveNext()
		{
			if (_state == 1)
			{
				_enumerator = GetEnumerable(0).GetEnumerator();
				_state = 2;
			}
			if (_state > 1)
			{
				while (true)
				{
					if (_enumerator.MoveNext())
					{
						_current = _enumerator.Current;
						return true;
					}
					IEnumerable<TSource> enumerable = GetEnumerable(_state++ - 1);
					if (enumerable == null)
					{
						break;
					}
					_enumerator.Dispose();
					_enumerator = enumerable.GetEnumerator();
				}
				Dispose();
			}
			return false;
		}

		public abstract int GetCount(bool onlyIfCheap);

		public abstract TSource[] ToArray();

		public List<TSource> ToList()
		{
			int count = GetCount(onlyIfCheap: true);
			List<TSource> list = ((count != -1) ? new List<TSource>(count) : new List<TSource>());
			int num = 0;
			while (true)
			{
				IEnumerable<TSource> enumerable = GetEnumerable(num);
				if (enumerable == null)
				{
					break;
				}
				list.AddRange(enumerable);
				num++;
			}
			return list;
		}
	}

	private sealed class DefaultIfEmptyIterator<TSource> : Iterator<TSource>, IIListProvider<TSource>, IEnumerable<TSource>, IEnumerable
	{
		private readonly IEnumerable<TSource> _source;

		private readonly TSource _default;

		private IEnumerator<TSource> _enumerator;

		public DefaultIfEmptyIterator(IEnumerable<TSource> source, TSource defaultValue)
		{
			_source = source;
			_default = defaultValue;
		}

		public override Iterator<TSource> Clone()
		{
			return new DefaultIfEmptyIterator<TSource>(_source, _default);
		}

		public override bool MoveNext()
		{
			switch (_state)
			{
			case 1:
				_enumerator = _source.GetEnumerator();
				if (_enumerator.MoveNext())
				{
					_current = _enumerator.Current;
					_state = 2;
				}
				else
				{
					_current = _default;
					_state = -1;
				}
				return true;
			case 2:
				if (_enumerator.MoveNext())
				{
					_current = _enumerator.Current;
					return true;
				}
				break;
			}
			Dispose();
			return false;
		}

		public override void Dispose()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
			}
			base.Dispose();
		}

		public TSource[] ToArray()
		{
			TSource[] array = _source.ToArray();
			if (array.Length != 0)
			{
				return array;
			}
			return new TSource[1] { _default };
		}

		public List<TSource> ToList()
		{
			List<TSource> list = _source.ToList();
			if (list.Count == 0)
			{
				list.Add(_default);
			}
			return list;
		}

		public int GetCount(bool onlyIfCheap)
		{
			int num = ((onlyIfCheap && !(_source is ICollection<TSource>) && !(_source is ICollection)) ? ((_source is IIListProvider<TSource> iIListProvider) ? iIListProvider.GetCount(onlyIfCheap: true) : (-1)) : _source.Count());
			if (num != 0)
			{
				return num;
			}
			return 1;
		}
	}

	private sealed class DistinctIterator<TSource> : Iterator<TSource>, IIListProvider<TSource>, IEnumerable<TSource>, IEnumerable
	{
		private readonly IEnumerable<TSource> _source;

		private readonly IEqualityComparer<TSource> _comparer;

		private Set<TSource> _set;

		private IEnumerator<TSource> _enumerator;

		public DistinctIterator(IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
		{
			_source = source;
			_comparer = comparer;
		}

		public override Iterator<TSource> Clone()
		{
			return new DistinctIterator<TSource>(_source, _comparer);
		}

		public override bool MoveNext()
		{
			int state = _state;
			TSource current;
			if (state != 1)
			{
				if (state == 2)
				{
					while (_enumerator.MoveNext())
					{
						current = _enumerator.Current;
						if (_set.Add(current))
						{
							_current = current;
							return true;
						}
					}
				}
				Dispose();
				return false;
			}
			_enumerator = _source.GetEnumerator();
			if (!_enumerator.MoveNext())
			{
				Dispose();
				return false;
			}
			current = _enumerator.Current;
			_set = new Set<TSource>(_comparer);
			_set.Add(current);
			_current = current;
			_state = 2;
			return true;
		}

		public override void Dispose()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
				_set = null;
			}
			base.Dispose();
		}

		private Set<TSource> FillSet()
		{
			Set<TSource> set = new Set<TSource>(_comparer);
			set.UnionWith(_source);
			return set;
		}

		public TSource[] ToArray()
		{
			return FillSet().ToArray();
		}

		public List<TSource> ToList()
		{
			return FillSet().ToList();
		}

		public int GetCount(bool onlyIfCheap)
		{
			if (!onlyIfCheap)
			{
				return FillSet().Count;
			}
			return -1;
		}
	}

	internal abstract class Iterator<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IDisposable, IEnumerator
	{
		private readonly int _threadId;

		internal int _state;

		internal TSource _current;

		public TSource Current => _current;

		object IEnumerator.Current => Current;

		protected Iterator()
		{
			_threadId = Environment.CurrentManagedThreadId;
		}

		public abstract Iterator<TSource> Clone();

		public virtual void Dispose()
		{
			_current = default(TSource);
			_state = -1;
		}

		public IEnumerator<TSource> GetEnumerator()
		{
			Iterator<TSource> obj = ((_state == 0 && _threadId == Environment.CurrentManagedThreadId) ? this : Clone());
			obj._state = 1;
			return obj;
		}

		public abstract bool MoveNext();

		public virtual IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
		{
			return new SelectEnumerableIterator<TSource, TResult>(this, selector);
		}

		public virtual IEnumerable<TSource> Where(Func<TSource, bool> predicate)
		{
			return new WhereEnumerableIterator<TSource>(this, predicate);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		void IEnumerator.Reset()
		{
			throw Error.NotSupported();
		}
	}

	private sealed class ListPartition<TSource> : Iterator<TSource>, IPartition<TSource>, IIListProvider<TSource>, IEnumerable<TSource>, IEnumerable
	{
		private readonly IList<TSource> _source;

		private readonly int _minIndexInclusive;

		private readonly int _maxIndexInclusive;

		private int Count
		{
			get
			{
				int count = _source.Count;
				if (count <= _minIndexInclusive)
				{
					return 0;
				}
				return Math.Min(count - 1, _maxIndexInclusive) - _minIndexInclusive + 1;
			}
		}

		public ListPartition(IList<TSource> source, int minIndexInclusive, int maxIndexInclusive)
		{
			_source = source;
			_minIndexInclusive = minIndexInclusive;
			_maxIndexInclusive = maxIndexInclusive;
		}

		public override Iterator<TSource> Clone()
		{
			return new ListPartition<TSource>(_source, _minIndexInclusive, _maxIndexInclusive);
		}

		public override bool MoveNext()
		{
			int num = _state - 1;
			if ((uint)num <= (uint)(_maxIndexInclusive - _minIndexInclusive) && num < _source.Count - _minIndexInclusive)
			{
				_current = _source[_minIndexInclusive + num];
				_state++;
				return true;
			}
			Dispose();
			return false;
		}

		public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
		{
			return new SelectListPartitionIterator<TSource, TResult>(_source, selector, _minIndexInclusive, _maxIndexInclusive);
		}

		public IPartition<TSource> Skip(int count)
		{
			int num = _minIndexInclusive + count;
			if ((uint)num <= (uint)_maxIndexInclusive)
			{
				return new ListPartition<TSource>(_source, num, _maxIndexInclusive);
			}
			return EmptyPartition<TSource>.Instance;
		}

		public IPartition<TSource> Take(int count)
		{
			int num = _minIndexInclusive + count - 1;
			if ((uint)num < (uint)_maxIndexInclusive)
			{
				return new ListPartition<TSource>(_source, _minIndexInclusive, num);
			}
			return this;
		}

		public TSource TryGetElementAt(int index, out bool found)
		{
			if ((uint)index <= (uint)(_maxIndexInclusive - _minIndexInclusive) && index < _source.Count - _minIndexInclusive)
			{
				found = true;
				return _source[_minIndexInclusive + index];
			}
			found = false;
			return default(TSource);
		}

		public TSource TryGetFirst(out bool found)
		{
			if (_source.Count > _minIndexInclusive)
			{
				found = true;
				return _source[_minIndexInclusive];
			}
			found = false;
			return default(TSource);
		}

		public TSource TryGetLast(out bool found)
		{
			int num = _source.Count - 1;
			if (num >= _minIndexInclusive)
			{
				found = true;
				return _source[Math.Min(num, _maxIndexInclusive)];
			}
			found = false;
			return default(TSource);
		}

		public TSource[] ToArray()
		{
			int count = Count;
			if (count == 0)
			{
				return Array.Empty<TSource>();
			}
			TSource[] array = new TSource[count];
			int num = 0;
			int num2 = _minIndexInclusive;
			while (num != array.Length)
			{
				array[num] = _source[num2];
				num++;
				num2++;
			}
			return array;
		}

		public List<TSource> ToList()
		{
			int count = Count;
			if (count == 0)
			{
				return new List<TSource>();
			}
			List<TSource> list = new List<TSource>(count);
			int num = _minIndexInclusive + count;
			for (int i = _minIndexInclusive; i != num; i++)
			{
				list.Add(_source[i]);
			}
			return list;
		}

		public int GetCount(bool onlyIfCheap)
		{
			return Count;
		}
	}

	private sealed class EnumerablePartition<TSource> : Iterator<TSource>, IPartition<TSource>, IIListProvider<TSource>, IEnumerable<TSource>, IEnumerable
	{
		private readonly IEnumerable<TSource> _source;

		private readonly int _minIndexInclusive;

		private readonly int _maxIndexInclusive;

		private IEnumerator<TSource> _enumerator;

		private bool HasLimit => _maxIndexInclusive != -1;

		private int Limit => _maxIndexInclusive + 1 - _minIndexInclusive;

		internal EnumerablePartition(IEnumerable<TSource> source, int minIndexInclusive, int maxIndexInclusive)
		{
			_source = source;
			_minIndexInclusive = minIndexInclusive;
			_maxIndexInclusive = maxIndexInclusive;
		}

		public override Iterator<TSource> Clone()
		{
			return new EnumerablePartition<TSource>(_source, _minIndexInclusive, _maxIndexInclusive);
		}

		public override void Dispose()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
			}
			base.Dispose();
		}

		public int GetCount(bool onlyIfCheap)
		{
			if (onlyIfCheap)
			{
				return -1;
			}
			if (!HasLimit)
			{
				return Math.Max(_source.Count() - _minIndexInclusive, 0);
			}
			using IEnumerator<TSource> en = _source.GetEnumerator();
			return Math.Max((int)SkipAndCount((uint)(_maxIndexInclusive + 1), en) - _minIndexInclusive, 0);
		}

		public override bool MoveNext()
		{
			int num = _state - 3;
			if (num < -2)
			{
				Dispose();
				return false;
			}
			int state = _state;
			if (state != 1)
			{
				if (state != 2)
				{
					goto IL_0054;
				}
			}
			else
			{
				_enumerator = _source.GetEnumerator();
				_state = 2;
			}
			if (SkipBeforeFirst(_enumerator))
			{
				_state = 3;
				goto IL_0054;
			}
			goto IL_009b;
			IL_009b:
			Dispose();
			return false;
			IL_0054:
			if ((!HasLimit || num < Limit) && _enumerator.MoveNext())
			{
				if (HasLimit)
				{
					_state++;
				}
				_current = _enumerator.Current;
				return true;
			}
			goto IL_009b;
		}

		public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
		{
			return new SelectIPartitionIterator<TSource, TResult>(this, selector);
		}

		public IPartition<TSource> Skip(int count)
		{
			int num = _minIndexInclusive + count;
			if (!HasLimit)
			{
				if (num < 0)
				{
					return new EnumerablePartition<TSource>(this, count, -1);
				}
			}
			else if ((uint)num > (uint)_maxIndexInclusive)
			{
				return EmptyPartition<TSource>.Instance;
			}
			return new EnumerablePartition<TSource>(_source, num, _maxIndexInclusive);
		}

		public IPartition<TSource> Take(int count)
		{
			int num = _minIndexInclusive + count - 1;
			if (!HasLimit)
			{
				if (num < 0)
				{
					return new EnumerablePartition<TSource>(this, 0, count - 1);
				}
			}
			else if ((uint)num >= (uint)_maxIndexInclusive)
			{
				return this;
			}
			return new EnumerablePartition<TSource>(_source, _minIndexInclusive, num);
		}

		public TSource TryGetElementAt(int index, out bool found)
		{
			if (index >= 0 && (!HasLimit || index < Limit))
			{
				using IEnumerator<TSource> enumerator = _source.GetEnumerator();
				if (SkipBefore(_minIndexInclusive + index, enumerator) && enumerator.MoveNext())
				{
					found = true;
					return enumerator.Current;
				}
			}
			found = false;
			return default(TSource);
		}

		public TSource TryGetFirst(out bool found)
		{
			using (IEnumerator<TSource> enumerator = _source.GetEnumerator())
			{
				if (SkipBeforeFirst(enumerator) && enumerator.MoveNext())
				{
					found = true;
					return enumerator.Current;
				}
			}
			found = false;
			return default(TSource);
		}

		public TSource TryGetLast(out bool found)
		{
			using (IEnumerator<TSource> enumerator = _source.GetEnumerator())
			{
				if (SkipBeforeFirst(enumerator) && enumerator.MoveNext())
				{
					int num = Limit - 1;
					int num2 = ((!HasLimit) ? int.MinValue : 0);
					TSource current;
					do
					{
						num--;
						current = enumerator.Current;
					}
					while (num >= num2 && enumerator.MoveNext());
					found = true;
					return current;
				}
			}
			found = false;
			return default(TSource);
		}

		public TSource[] ToArray()
		{
			using (IEnumerator<TSource> enumerator = _source.GetEnumerator())
			{
				if (SkipBeforeFirst(enumerator) && enumerator.MoveNext())
				{
					int num = Limit - 1;
					int num2 = ((!HasLimit) ? int.MinValue : 0);
					int maxCapacity = (HasLimit ? Limit : int.MaxValue);
					System.Collections.Generic.LargeArrayBuilder<TSource> largeArrayBuilder = new System.Collections.Generic.LargeArrayBuilder<TSource>(maxCapacity);
					do
					{
						num--;
						largeArrayBuilder.Add(enumerator.Current);
					}
					while (num >= num2 && enumerator.MoveNext());
					return largeArrayBuilder.ToArray();
				}
			}
			return Array.Empty<TSource>();
		}

		public List<TSource> ToList()
		{
			List<TSource> list = new List<TSource>();
			using (IEnumerator<TSource> enumerator = _source.GetEnumerator())
			{
				if (SkipBeforeFirst(enumerator) && enumerator.MoveNext())
				{
					int num = Limit - 1;
					int num2 = ((!HasLimit) ? int.MinValue : 0);
					do
					{
						num--;
						list.Add(enumerator.Current);
					}
					while (num >= num2 && enumerator.MoveNext());
				}
			}
			return list;
		}

		private bool SkipBeforeFirst(IEnumerator<TSource> en)
		{
			return SkipBefore(_minIndexInclusive, en);
		}

		private static bool SkipBefore(int index, IEnumerator<TSource> en)
		{
			return SkipAndCount(index, en) == index;
		}

		private static int SkipAndCount(int index, IEnumerator<TSource> en)
		{
			return (int)SkipAndCount((uint)index, en);
		}

		private static uint SkipAndCount(uint index, IEnumerator<TSource> en)
		{
			for (uint num = 0u; num < index; num++)
			{
				if (!en.MoveNext())
				{
					return num;
				}
			}
			return index;
		}
	}

	private sealed class RangeIterator : Iterator<int>, IPartition<int>, IIListProvider<int>, IEnumerable<int>, IEnumerable
	{
		private readonly int _start;

		private readonly int _end;

		public RangeIterator(int start, int count)
		{
			_start = start;
			_end = start + count;
		}

		public override Iterator<int> Clone()
		{
			return new RangeIterator(_start, _end - _start);
		}

		public override bool MoveNext()
		{
			switch (_state)
			{
			case 1:
				_current = _start;
				_state = 2;
				return true;
			case 2:
				if (++_current != _end)
				{
					return true;
				}
				break;
			}
			_state = -1;
			return false;
		}

		public override void Dispose()
		{
			_state = -1;
		}

		public override IEnumerable<TResult> Select<TResult>(Func<int, TResult> selector)
		{
			return new SelectIPartitionIterator<int, TResult>(this, selector);
		}

		public int[] ToArray()
		{
			int[] array = new int[_end - _start];
			int num = _start;
			for (int i = 0; i != array.Length; i++)
			{
				array[i] = num;
				num++;
			}
			return array;
		}

		public List<int> ToList()
		{
			List<int> list = new List<int>(_end - _start);
			for (int i = _start; i != _end; i++)
			{
				list.Add(i);
			}
			return list;
		}

		public int GetCount(bool onlyIfCheap)
		{
			return _end - _start;
		}

		public IPartition<int> Skip(int count)
		{
			if (count >= _end - _start)
			{
				return EmptyPartition<int>.Instance;
			}
			return new RangeIterator(_start + count, _end - _start - count);
		}

		public IPartition<int> Take(int count)
		{
			int num = _end - _start;
			if (count >= num)
			{
				return this;
			}
			return new RangeIterator(_start, count);
		}

		public int TryGetElementAt(int index, out bool found)
		{
			if ((uint)index < (uint)(_end - _start))
			{
				found = true;
				return _start + index;
			}
			found = false;
			return 0;
		}

		public int TryGetFirst(out bool found)
		{
			found = true;
			return _start;
		}

		public int TryGetLast(out bool found)
		{
			found = true;
			return _end - 1;
		}
	}

	private sealed class RepeatIterator<TResult> : Iterator<TResult>, IPartition<TResult>, IIListProvider<TResult>, IEnumerable<TResult>, IEnumerable
	{
		private readonly int _count;

		public RepeatIterator(TResult element, int count)
		{
			_current = element;
			_count = count;
		}

		public override Iterator<TResult> Clone()
		{
			return new RepeatIterator<TResult>(_current, _count);
		}

		public override void Dispose()
		{
			_state = -1;
		}

		public override bool MoveNext()
		{
			int num = _state - 1;
			if (num >= 0 && num != _count)
			{
				_state++;
				return true;
			}
			Dispose();
			return false;
		}

		public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
		{
			return new SelectIPartitionIterator<TResult, TResult2>(this, selector);
		}

		public TResult[] ToArray()
		{
			TResult[] array = new TResult[_count];
			if (_current != null)
			{
				Array.Fill(array, _current);
			}
			return array;
		}

		public List<TResult> ToList()
		{
			List<TResult> list = new List<TResult>(_count);
			for (int i = 0; i != _count; i++)
			{
				list.Add(_current);
			}
			return list;
		}

		public int GetCount(bool onlyIfCheap)
		{
			return _count;
		}

		public IPartition<TResult> Skip(int count)
		{
			if (count >= _count)
			{
				return EmptyPartition<TResult>.Instance;
			}
			return new RepeatIterator<TResult>(_current, _count - count);
		}

		public IPartition<TResult> Take(int count)
		{
			if (count >= _count)
			{
				return this;
			}
			return new RepeatIterator<TResult>(_current, count);
		}

		public TResult TryGetElementAt(int index, out bool found)
		{
			if ((uint)index < (uint)_count)
			{
				found = true;
				return _current;
			}
			found = false;
			return default(TResult);
		}

		public TResult TryGetFirst(out bool found)
		{
			found = true;
			return _current;
		}

		public TResult TryGetLast(out bool found)
		{
			found = true;
			return _current;
		}
	}

	private sealed class ReverseIterator<TSource> : Iterator<TSource>, IIListProvider<TSource>, IEnumerable<TSource>, IEnumerable
	{
		private readonly IEnumerable<TSource> _source;

		private TSource[] _buffer;

		public ReverseIterator(IEnumerable<TSource> source)
		{
			_source = source;
		}

		public override Iterator<TSource> Clone()
		{
			return new ReverseIterator<TSource>(_source);
		}

		public override bool MoveNext()
		{
			if (_state - 2 <= -2)
			{
				Dispose();
				return false;
			}
			int state = _state;
			if (state == 1)
			{
				Buffer<TSource> buffer = new Buffer<TSource>(_source);
				_buffer = buffer._items;
				_state = buffer._count + 2;
			}
			int num = _state - 3;
			if (num != -1)
			{
				_current = _buffer[num];
				_state--;
				return true;
			}
			Dispose();
			return false;
		}

		public override void Dispose()
		{
			_buffer = null;
			base.Dispose();
		}

		public TSource[] ToArray()
		{
			TSource[] array = _source.ToArray();
			Array.Reverse(array);
			return array;
		}

		public List<TSource> ToList()
		{
			List<TSource> list = _source.ToList();
			list.Reverse();
			return list;
		}

		public int GetCount(bool onlyIfCheap)
		{
			if (onlyIfCheap)
			{
				IEnumerable<TSource> source = _source;
				if (source != null)
				{
					if (source is IIListProvider<TSource> iIListProvider)
					{
						return iIListProvider.GetCount(onlyIfCheap: true);
					}
					if (source is ICollection<TSource> collection)
					{
						return collection.Count;
					}
					if (source is ICollection collection2)
					{
						return collection2.Count;
					}
				}
				return -1;
			}
			return _source.Count();
		}
	}

	private sealed class SelectEnumerableIterator<TSource, TResult> : Iterator<TResult>, IIListProvider<TResult>, IEnumerable<TResult>, IEnumerable
	{
		private readonly IEnumerable<TSource> _source;

		private readonly Func<TSource, TResult> _selector;

		private IEnumerator<TSource> _enumerator;

		public SelectEnumerableIterator(IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			_source = source;
			_selector = selector;
		}

		public override Iterator<TResult> Clone()
		{
			return new SelectEnumerableIterator<TSource, TResult>(_source, _selector);
		}

		public override void Dispose()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
			}
			base.Dispose();
		}

		public override bool MoveNext()
		{
			int state = _state;
			if (state != 1)
			{
				if (state != 2)
				{
					goto IL_005a;
				}
			}
			else
			{
				_enumerator = _source.GetEnumerator();
				_state = 2;
			}
			if (_enumerator.MoveNext())
			{
				_current = _selector(_enumerator.Current);
				return true;
			}
			Dispose();
			goto IL_005a;
			IL_005a:
			return false;
		}

		public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
		{
			return new SelectEnumerableIterator<TSource, TResult2>(_source, Utilities.CombineSelectors(_selector, selector));
		}

		public TResult[] ToArray()
		{
			System.Collections.Generic.LargeArrayBuilder<TResult> largeArrayBuilder = new System.Collections.Generic.LargeArrayBuilder<TResult>(initialize: true);
			foreach (TSource item in _source)
			{
				largeArrayBuilder.Add(_selector(item));
			}
			return largeArrayBuilder.ToArray();
		}

		public List<TResult> ToList()
		{
			List<TResult> list = new List<TResult>();
			foreach (TSource item in _source)
			{
				list.Add(_selector(item));
			}
			return list;
		}

		public int GetCount(bool onlyIfCheap)
		{
			if (onlyIfCheap)
			{
				return -1;
			}
			int num = 0;
			foreach (TSource item in _source)
			{
				_selector(item);
				num = checked(num + 1);
			}
			return num;
		}
	}

	private sealed class SelectArrayIterator<TSource, TResult> : Iterator<TResult>, IPartition<TResult>, IIListProvider<TResult>, IEnumerable<TResult>, IEnumerable
	{
		private readonly TSource[] _source;

		private readonly Func<TSource, TResult> _selector;

		public SelectArrayIterator(TSource[] source, Func<TSource, TResult> selector)
		{
			_source = source;
			_selector = selector;
		}

		public override Iterator<TResult> Clone()
		{
			return new SelectArrayIterator<TSource, TResult>(_source, _selector);
		}

		public override bool MoveNext()
		{
			if ((_state < 1) | (_state == _source.Length + 1))
			{
				Dispose();
				return false;
			}
			int num = _state++ - 1;
			_current = _selector(_source[num]);
			return true;
		}

		public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
		{
			return new SelectArrayIterator<TSource, TResult2>(_source, Utilities.CombineSelectors(_selector, selector));
		}

		public TResult[] ToArray()
		{
			TResult[] array = new TResult[_source.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = _selector(_source[i]);
			}
			return array;
		}

		public List<TResult> ToList()
		{
			TSource[] source = _source;
			List<TResult> list = new List<TResult>(source.Length);
			for (int i = 0; i < source.Length; i++)
			{
				list.Add(_selector(source[i]));
			}
			return list;
		}

		public int GetCount(bool onlyIfCheap)
		{
			if (!onlyIfCheap)
			{
				TSource[] source = _source;
				foreach (TSource arg in source)
				{
					_selector(arg);
				}
			}
			return _source.Length;
		}

		public IPartition<TResult> Skip(int count)
		{
			if (count >= _source.Length)
			{
				return EmptyPartition<TResult>.Instance;
			}
			return new SelectListPartitionIterator<TSource, TResult>(_source, _selector, count, int.MaxValue);
		}

		public IPartition<TResult> Take(int count)
		{
			if (count < _source.Length)
			{
				return new SelectListPartitionIterator<TSource, TResult>(_source, _selector, 0, count - 1);
			}
			return this;
		}

		public TResult TryGetElementAt(int index, out bool found)
		{
			if ((uint)index < (uint)_source.Length)
			{
				found = true;
				return _selector(_source[index]);
			}
			found = false;
			return default(TResult);
		}

		public TResult TryGetFirst(out bool found)
		{
			found = true;
			return _selector(_source[0]);
		}

		public TResult TryGetLast(out bool found)
		{
			found = true;
			return _selector(_source[_source.Length - 1]);
		}
	}

	private sealed class SelectListIterator<TSource, TResult> : Iterator<TResult>, IPartition<TResult>, IIListProvider<TResult>, IEnumerable<TResult>, IEnumerable
	{
		private readonly List<TSource> _source;

		private readonly Func<TSource, TResult> _selector;

		private List<TSource>.Enumerator _enumerator;

		public SelectListIterator(List<TSource> source, Func<TSource, TResult> selector)
		{
			_source = source;
			_selector = selector;
		}

		public override Iterator<TResult> Clone()
		{
			return new SelectListIterator<TSource, TResult>(_source, _selector);
		}

		public override bool MoveNext()
		{
			int state = _state;
			if (state != 1)
			{
				if (state != 2)
				{
					goto IL_005a;
				}
			}
			else
			{
				_enumerator = _source.GetEnumerator();
				_state = 2;
			}
			if (_enumerator.MoveNext())
			{
				_current = _selector(_enumerator.Current);
				return true;
			}
			Dispose();
			goto IL_005a;
			IL_005a:
			return false;
		}

		public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
		{
			return new SelectListIterator<TSource, TResult2>(_source, Utilities.CombineSelectors(_selector, selector));
		}

		public TResult[] ToArray()
		{
			int count = _source.Count;
			if (count == 0)
			{
				return Array.Empty<TResult>();
			}
			TResult[] array = new TResult[count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = _selector(_source[i]);
			}
			return array;
		}

		public List<TResult> ToList()
		{
			int count = _source.Count;
			List<TResult> list = new List<TResult>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(_selector(_source[i]));
			}
			return list;
		}

		public int GetCount(bool onlyIfCheap)
		{
			int count = _source.Count;
			if (!onlyIfCheap)
			{
				for (int i = 0; i < count; i++)
				{
					_selector(_source[i]);
				}
			}
			return count;
		}

		public IPartition<TResult> Skip(int count)
		{
			return new SelectListPartitionIterator<TSource, TResult>(_source, _selector, count, int.MaxValue);
		}

		public IPartition<TResult> Take(int count)
		{
			return new SelectListPartitionIterator<TSource, TResult>(_source, _selector, 0, count - 1);
		}

		public TResult TryGetElementAt(int index, out bool found)
		{
			if ((uint)index < (uint)_source.Count)
			{
				found = true;
				return _selector(_source[index]);
			}
			found = false;
			return default(TResult);
		}

		public TResult TryGetFirst(out bool found)
		{
			if (_source.Count != 0)
			{
				found = true;
				return _selector(_source[0]);
			}
			found = false;
			return default(TResult);
		}

		public TResult TryGetLast(out bool found)
		{
			int count = _source.Count;
			if (count != 0)
			{
				found = true;
				return _selector(_source[count - 1]);
			}
			found = false;
			return default(TResult);
		}
	}

	private sealed class SelectIListIterator<TSource, TResult> : Iterator<TResult>, IPartition<TResult>, IIListProvider<TResult>, IEnumerable<TResult>, IEnumerable
	{
		private readonly IList<TSource> _source;

		private readonly Func<TSource, TResult> _selector;

		private IEnumerator<TSource> _enumerator;

		public SelectIListIterator(IList<TSource> source, Func<TSource, TResult> selector)
		{
			_source = source;
			_selector = selector;
		}

		public override Iterator<TResult> Clone()
		{
			return new SelectIListIterator<TSource, TResult>(_source, _selector);
		}

		public override bool MoveNext()
		{
			int state = _state;
			if (state != 1)
			{
				if (state != 2)
				{
					goto IL_005a;
				}
			}
			else
			{
				_enumerator = _source.GetEnumerator();
				_state = 2;
			}
			if (_enumerator.MoveNext())
			{
				_current = _selector(_enumerator.Current);
				return true;
			}
			Dispose();
			goto IL_005a;
			IL_005a:
			return false;
		}

		public override void Dispose()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
			}
			base.Dispose();
		}

		public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
		{
			return new SelectIListIterator<TSource, TResult2>(_source, Utilities.CombineSelectors(_selector, selector));
		}

		public TResult[] ToArray()
		{
			int count = _source.Count;
			if (count == 0)
			{
				return Array.Empty<TResult>();
			}
			TResult[] array = new TResult[count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = _selector(_source[i]);
			}
			return array;
		}

		public List<TResult> ToList()
		{
			int count = _source.Count;
			List<TResult> list = new List<TResult>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(_selector(_source[i]));
			}
			return list;
		}

		public int GetCount(bool onlyIfCheap)
		{
			int count = _source.Count;
			if (!onlyIfCheap)
			{
				for (int i = 0; i < count; i++)
				{
					_selector(_source[i]);
				}
			}
			return count;
		}

		public IPartition<TResult> Skip(int count)
		{
			return new SelectListPartitionIterator<TSource, TResult>(_source, _selector, count, int.MaxValue);
		}

		public IPartition<TResult> Take(int count)
		{
			return new SelectListPartitionIterator<TSource, TResult>(_source, _selector, 0, count - 1);
		}

		public TResult TryGetElementAt(int index, out bool found)
		{
			if ((uint)index < (uint)_source.Count)
			{
				found = true;
				return _selector(_source[index]);
			}
			found = false;
			return default(TResult);
		}

		public TResult TryGetFirst(out bool found)
		{
			if (_source.Count != 0)
			{
				found = true;
				return _selector(_source[0]);
			}
			found = false;
			return default(TResult);
		}

		public TResult TryGetLast(out bool found)
		{
			int count = _source.Count;
			if (count != 0)
			{
				found = true;
				return _selector(_source[count - 1]);
			}
			found = false;
			return default(TResult);
		}
	}

	private sealed class SelectIPartitionIterator<TSource, TResult> : Iterator<TResult>, IPartition<TResult>, IIListProvider<TResult>, IEnumerable<TResult>, IEnumerable
	{
		private readonly IPartition<TSource> _source;

		private readonly Func<TSource, TResult> _selector;

		private IEnumerator<TSource> _enumerator;

		public SelectIPartitionIterator(IPartition<TSource> source, Func<TSource, TResult> selector)
		{
			_source = source;
			_selector = selector;
		}

		public override Iterator<TResult> Clone()
		{
			return new SelectIPartitionIterator<TSource, TResult>(_source, _selector);
		}

		public override bool MoveNext()
		{
			int state = _state;
			if (state != 1)
			{
				if (state != 2)
				{
					goto IL_005a;
				}
			}
			else
			{
				_enumerator = _source.GetEnumerator();
				_state = 2;
			}
			if (_enumerator.MoveNext())
			{
				_current = _selector(_enumerator.Current);
				return true;
			}
			Dispose();
			goto IL_005a;
			IL_005a:
			return false;
		}

		public override void Dispose()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
			}
			base.Dispose();
		}

		public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
		{
			return new SelectIPartitionIterator<TSource, TResult2>(_source, Utilities.CombineSelectors(_selector, selector));
		}

		public IPartition<TResult> Skip(int count)
		{
			return new SelectIPartitionIterator<TSource, TResult>(_source.Skip(count), _selector);
		}

		public IPartition<TResult> Take(int count)
		{
			return new SelectIPartitionIterator<TSource, TResult>(_source.Take(count), _selector);
		}

		public TResult TryGetElementAt(int index, out bool found)
		{
			bool found2;
			TSource arg = _source.TryGetElementAt(index, out found2);
			found = found2;
			if (!found2)
			{
				return default(TResult);
			}
			return _selector(arg);
		}

		public TResult TryGetFirst(out bool found)
		{
			bool found2;
			TSource arg = _source.TryGetFirst(out found2);
			found = found2;
			if (!found2)
			{
				return default(TResult);
			}
			return _selector(arg);
		}

		public TResult TryGetLast(out bool found)
		{
			bool found2;
			TSource arg = _source.TryGetLast(out found2);
			found = found2;
			if (!found2)
			{
				return default(TResult);
			}
			return _selector(arg);
		}

		private TResult[] LazyToArray()
		{
			System.Collections.Generic.LargeArrayBuilder<TResult> largeArrayBuilder = new System.Collections.Generic.LargeArrayBuilder<TResult>(initialize: true);
			foreach (TSource item in _source)
			{
				largeArrayBuilder.Add(_selector(item));
			}
			return largeArrayBuilder.ToArray();
		}

		private TResult[] PreallocatingToArray(int count)
		{
			TResult[] array = new TResult[count];
			int num = 0;
			foreach (TSource item in _source)
			{
				array[num] = _selector(item);
				num++;
			}
			return array;
		}

		public TResult[] ToArray()
		{
			int count = _source.GetCount(onlyIfCheap: true);
			return count switch
			{
				-1 => LazyToArray(), 
				0 => Array.Empty<TResult>(), 
				_ => PreallocatingToArray(count), 
			};
		}

		public List<TResult> ToList()
		{
			int count = _source.GetCount(onlyIfCheap: true);
			List<TResult> list;
			switch (count)
			{
			case -1:
				list = new List<TResult>();
				break;
			case 0:
				return new List<TResult>();
			default:
				list = new List<TResult>(count);
				break;
			}
			foreach (TSource item in _source)
			{
				list.Add(_selector(item));
			}
			return list;
		}

		public int GetCount(bool onlyIfCheap)
		{
			if (!onlyIfCheap)
			{
				foreach (TSource item in _source)
				{
					_selector(item);
				}
			}
			return _source.GetCount(onlyIfCheap);
		}
	}

	private sealed class SelectListPartitionIterator<TSource, TResult> : Iterator<TResult>, IPartition<TResult>, IIListProvider<TResult>, IEnumerable<TResult>, IEnumerable
	{
		private readonly IList<TSource> _source;

		private readonly Func<TSource, TResult> _selector;

		private readonly int _minIndexInclusive;

		private readonly int _maxIndexInclusive;

		private int Count
		{
			get
			{
				int count = _source.Count;
				if (count <= _minIndexInclusive)
				{
					return 0;
				}
				return Math.Min(count - 1, _maxIndexInclusive) - _minIndexInclusive + 1;
			}
		}

		public SelectListPartitionIterator(IList<TSource> source, Func<TSource, TResult> selector, int minIndexInclusive, int maxIndexInclusive)
		{
			_source = source;
			_selector = selector;
			_minIndexInclusive = minIndexInclusive;
			_maxIndexInclusive = maxIndexInclusive;
		}

		public override Iterator<TResult> Clone()
		{
			return new SelectListPartitionIterator<TSource, TResult>(_source, _selector, _minIndexInclusive, _maxIndexInclusive);
		}

		public override bool MoveNext()
		{
			int num = _state - 1;
			if ((uint)num <= (uint)(_maxIndexInclusive - _minIndexInclusive) && num < _source.Count - _minIndexInclusive)
			{
				_current = _selector(_source[_minIndexInclusive + num]);
				_state++;
				return true;
			}
			Dispose();
			return false;
		}

		public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
		{
			return new SelectListPartitionIterator<TSource, TResult2>(_source, Utilities.CombineSelectors(_selector, selector), _minIndexInclusive, _maxIndexInclusive);
		}

		public IPartition<TResult> Skip(int count)
		{
			int num = _minIndexInclusive + count;
			if ((uint)num <= (uint)_maxIndexInclusive)
			{
				return new SelectListPartitionIterator<TSource, TResult>(_source, _selector, num, _maxIndexInclusive);
			}
			return EmptyPartition<TResult>.Instance;
		}

		public IPartition<TResult> Take(int count)
		{
			int num = _minIndexInclusive + count - 1;
			if ((uint)num < (uint)_maxIndexInclusive)
			{
				return new SelectListPartitionIterator<TSource, TResult>(_source, _selector, _minIndexInclusive, num);
			}
			return this;
		}

		public TResult TryGetElementAt(int index, out bool found)
		{
			if ((uint)index <= (uint)(_maxIndexInclusive - _minIndexInclusive) && index < _source.Count - _minIndexInclusive)
			{
				found = true;
				return _selector(_source[_minIndexInclusive + index]);
			}
			found = false;
			return default(TResult);
		}

		public TResult TryGetFirst(out bool found)
		{
			if (_source.Count > _minIndexInclusive)
			{
				found = true;
				return _selector(_source[_minIndexInclusive]);
			}
			found = false;
			return default(TResult);
		}

		public TResult TryGetLast(out bool found)
		{
			int num = _source.Count - 1;
			if (num >= _minIndexInclusive)
			{
				found = true;
				return _selector(_source[Math.Min(num, _maxIndexInclusive)]);
			}
			found = false;
			return default(TResult);
		}

		public TResult[] ToArray()
		{
			int count = Count;
			if (count == 0)
			{
				return Array.Empty<TResult>();
			}
			TResult[] array = new TResult[count];
			int num = 0;
			int num2 = _minIndexInclusive;
			while (num != array.Length)
			{
				array[num] = _selector(_source[num2]);
				num++;
				num2++;
			}
			return array;
		}

		public List<TResult> ToList()
		{
			int count = Count;
			if (count == 0)
			{
				return new List<TResult>();
			}
			List<TResult> list = new List<TResult>(count);
			int num = _minIndexInclusive + count;
			for (int i = _minIndexInclusive; i != num; i++)
			{
				list.Add(_selector(_source[i]));
			}
			return list;
		}

		public int GetCount(bool onlyIfCheap)
		{
			int count = Count;
			if (!onlyIfCheap)
			{
				int num = _minIndexInclusive + count;
				for (int i = _minIndexInclusive; i != num; i++)
				{
					_selector(_source[i]);
				}
			}
			return count;
		}
	}

	private sealed class SelectManySingleSelectorIterator<TSource, TResult> : Iterator<TResult>, IIListProvider<TResult>, IEnumerable<TResult>, IEnumerable
	{
		private readonly IEnumerable<TSource> _source;

		private readonly Func<TSource, IEnumerable<TResult>> _selector;

		private IEnumerator<TSource> _sourceEnumerator;

		private IEnumerator<TResult> _subEnumerator;

		internal SelectManySingleSelectorIterator(IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
		{
			_source = source;
			_selector = selector;
		}

		public override Iterator<TResult> Clone()
		{
			return new SelectManySingleSelectorIterator<TSource, TResult>(_source, _selector);
		}

		public override void Dispose()
		{
			if (_subEnumerator != null)
			{
				_subEnumerator.Dispose();
				_subEnumerator = null;
			}
			if (_sourceEnumerator != null)
			{
				_sourceEnumerator.Dispose();
				_sourceEnumerator = null;
			}
			base.Dispose();
		}

		public int GetCount(bool onlyIfCheap)
		{
			if (onlyIfCheap)
			{
				return -1;
			}
			int num = 0;
			foreach (TSource item in _source)
			{
				num = checked(num + _selector(item).Count());
			}
			return num;
		}

		public override bool MoveNext()
		{
			switch (_state)
			{
			case 1:
				_sourceEnumerator = _source.GetEnumerator();
				_state = 2;
				goto case 2;
			case 2:
			{
				if (!_sourceEnumerator.MoveNext())
				{
					break;
				}
				TSource current = _sourceEnumerator.Current;
				_subEnumerator = _selector(current).GetEnumerator();
				_state = 3;
				goto case 3;
			}
			case 3:
				if (!_subEnumerator.MoveNext())
				{
					_subEnumerator.Dispose();
					_subEnumerator = null;
					_state = 2;
					goto case 2;
				}
				_current = _subEnumerator.Current;
				return true;
			}
			Dispose();
			return false;
		}

		public TResult[] ToArray()
		{
			SparseArrayBuilder<TResult> sparseArrayBuilder = new SparseArrayBuilder<TResult>(initialize: true);
			System.Collections.Generic.ArrayBuilder<IEnumerable<TResult>> arrayBuilder = default(System.Collections.Generic.ArrayBuilder<IEnumerable<TResult>>);
			foreach (TSource item in _source)
			{
				IEnumerable<TResult> enumerable = _selector(item);
				if (sparseArrayBuilder.ReserveOrAdd(enumerable))
				{
					arrayBuilder.Add(enumerable);
				}
			}
			TResult[] array = sparseArrayBuilder.ToArray();
			System.Collections.Generic.ArrayBuilder<Marker> markers = sparseArrayBuilder.Markers;
			for (int i = 0; i < markers.Count; i++)
			{
				Marker marker = markers[i];
				System.Collections.Generic.EnumerableHelpers.Copy(arrayBuilder[i], array, marker.Index, marker.Count);
			}
			return array;
		}

		public List<TResult> ToList()
		{
			List<TResult> list = new List<TResult>();
			foreach (TSource item in _source)
			{
				list.AddRange(_selector(item));
			}
			return list;
		}
	}

	private abstract class UnionIterator<TSource> : Iterator<TSource>, IIListProvider<TSource>, IEnumerable<TSource>, IEnumerable
	{
		internal readonly IEqualityComparer<TSource> _comparer;

		private IEnumerator<TSource> _enumerator;

		private Set<TSource> _set;

		protected UnionIterator(IEqualityComparer<TSource> comparer)
		{
			_comparer = comparer;
		}

		public sealed override void Dispose()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
				_set = null;
			}
			base.Dispose();
		}

		internal abstract IEnumerable<TSource> GetEnumerable(int index);

		internal abstract UnionIterator<TSource> Union(IEnumerable<TSource> next);

		private void SetEnumerator(IEnumerator<TSource> enumerator)
		{
			_enumerator?.Dispose();
			_enumerator = enumerator;
		}

		private void StoreFirst()
		{
			Set<TSource> set = new Set<TSource>(_comparer);
			TSource current = _enumerator.Current;
			set.Add(current);
			_current = current;
			_set = set;
		}

		private bool GetNext()
		{
			Set<TSource> set = _set;
			while (_enumerator.MoveNext())
			{
				TSource current = _enumerator.Current;
				if (set.Add(current))
				{
					_current = current;
					return true;
				}
			}
			return false;
		}

		public sealed override bool MoveNext()
		{
			if (_state == 1)
			{
				for (IEnumerable<TSource> enumerable = GetEnumerable(0); enumerable != null; enumerable = GetEnumerable(_state - 1))
				{
					IEnumerator<TSource> enumerator = enumerable.GetEnumerator();
					_state++;
					if (enumerator.MoveNext())
					{
						SetEnumerator(enumerator);
						StoreFirst();
						return true;
					}
				}
			}
			else if (_state > 0)
			{
				while (true)
				{
					if (GetNext())
					{
						return true;
					}
					IEnumerable<TSource> enumerable2 = GetEnumerable(_state - 1);
					if (enumerable2 == null)
					{
						break;
					}
					SetEnumerator(enumerable2.GetEnumerator());
					_state++;
				}
			}
			Dispose();
			return false;
		}

		private Set<TSource> FillSet()
		{
			Set<TSource> set = new Set<TSource>(_comparer);
			int num = 0;
			while (true)
			{
				IEnumerable<TSource> enumerable = GetEnumerable(num);
				if (enumerable == null)
				{
					break;
				}
				set.UnionWith(enumerable);
				num++;
			}
			return set;
		}

		public TSource[] ToArray()
		{
			return FillSet().ToArray();
		}

		public List<TSource> ToList()
		{
			return FillSet().ToList();
		}

		public int GetCount(bool onlyIfCheap)
		{
			if (!onlyIfCheap)
			{
				return FillSet().Count;
			}
			return -1;
		}
	}

	private sealed class UnionIterator2<TSource> : UnionIterator<TSource>
	{
		private readonly IEnumerable<TSource> _first;

		private readonly IEnumerable<TSource> _second;

		public UnionIterator2(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
			: base(comparer)
		{
			_first = first;
			_second = second;
		}

		public override Iterator<TSource> Clone()
		{
			return new UnionIterator2<TSource>(_first, _second, _comparer);
		}

		internal override IEnumerable<TSource> GetEnumerable(int index)
		{
			return index switch
			{
				0 => _first, 
				1 => _second, 
				_ => null, 
			};
		}

		internal override UnionIterator<TSource> Union(IEnumerable<TSource> next)
		{
			return new UnionIteratorN<TSource>(new SingleLinkedNode<IEnumerable<TSource>>(_first).Add(_second).Add(next), 2, _comparer);
		}
	}

	private sealed class UnionIteratorN<TSource> : UnionIterator<TSource>
	{
		private readonly SingleLinkedNode<IEnumerable<TSource>> _sources;

		private readonly int _headIndex;

		public UnionIteratorN(SingleLinkedNode<IEnumerable<TSource>> sources, int headIndex, IEqualityComparer<TSource> comparer)
			: base(comparer)
		{
			_sources = sources;
			_headIndex = headIndex;
		}

		public override Iterator<TSource> Clone()
		{
			return new UnionIteratorN<TSource>(_sources, _headIndex, _comparer);
		}

		internal override IEnumerable<TSource> GetEnumerable(int index)
		{
			if (index <= _headIndex)
			{
				return _sources.GetNode(_headIndex - index).Item;
			}
			return null;
		}

		internal override UnionIterator<TSource> Union(IEnumerable<TSource> next)
		{
			if (_headIndex == 2147483645)
			{
				return new UnionIterator2<TSource>(this, next, _comparer);
			}
			return new UnionIteratorN<TSource>(_sources.Add(next), _headIndex + 1, _comparer);
		}
	}

	private sealed class WhereEnumerableIterator<TSource> : Iterator<TSource>, IIListProvider<TSource>, IEnumerable<TSource>, IEnumerable
	{
		private readonly IEnumerable<TSource> _source;

		private readonly Func<TSource, bool> _predicate;

		private IEnumerator<TSource> _enumerator;

		public WhereEnumerableIterator(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			_source = source;
			_predicate = predicate;
		}

		public override Iterator<TSource> Clone()
		{
			return new WhereEnumerableIterator<TSource>(_source, _predicate);
		}

		public override void Dispose()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
			}
			base.Dispose();
		}

		public int GetCount(bool onlyIfCheap)
		{
			if (onlyIfCheap)
			{
				return -1;
			}
			int num = 0;
			foreach (TSource item in _source)
			{
				if (_predicate(item))
				{
					num = checked(num + 1);
				}
			}
			return num;
		}

		public override bool MoveNext()
		{
			int state = _state;
			if (state != 1)
			{
				if (state != 2)
				{
					goto IL_0061;
				}
			}
			else
			{
				_enumerator = _source.GetEnumerator();
				_state = 2;
			}
			while (_enumerator.MoveNext())
			{
				TSource current = _enumerator.Current;
				if (_predicate(current))
				{
					_current = current;
					return true;
				}
			}
			Dispose();
			goto IL_0061;
			IL_0061:
			return false;
		}

		public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
		{
			return new WhereSelectEnumerableIterator<TSource, TResult>(_source, _predicate, selector);
		}

		public TSource[] ToArray()
		{
			System.Collections.Generic.LargeArrayBuilder<TSource> largeArrayBuilder = new System.Collections.Generic.LargeArrayBuilder<TSource>(initialize: true);
			foreach (TSource item in _source)
			{
				if (_predicate(item))
				{
					largeArrayBuilder.Add(item);
				}
			}
			return largeArrayBuilder.ToArray();
		}

		public List<TSource> ToList()
		{
			List<TSource> list = new List<TSource>();
			foreach (TSource item in _source)
			{
				if (_predicate(item))
				{
					list.Add(item);
				}
			}
			return list;
		}

		public override IEnumerable<TSource> Where(Func<TSource, bool> predicate)
		{
			return new WhereEnumerableIterator<TSource>(_source, Utilities.CombinePredicates(_predicate, predicate));
		}
	}

	internal sealed class WhereArrayIterator<TSource> : Iterator<TSource>, IIListProvider<TSource>, IEnumerable<TSource>, IEnumerable
	{
		private readonly TSource[] _source;

		private readonly Func<TSource, bool> _predicate;

		public WhereArrayIterator(TSource[] source, Func<TSource, bool> predicate)
		{
			_source = source;
			_predicate = predicate;
		}

		public override Iterator<TSource> Clone()
		{
			return new WhereArrayIterator<TSource>(_source, _predicate);
		}

		public int GetCount(bool onlyIfCheap)
		{
			if (onlyIfCheap)
			{
				return -1;
			}
			int num = 0;
			TSource[] source = _source;
			foreach (TSource arg in source)
			{
				if (_predicate(arg))
				{
					num = checked(num + 1);
				}
			}
			return num;
		}

		public override bool MoveNext()
		{
			int num = _state - 1;
			TSource[] source = _source;
			while ((uint)num < (uint)source.Length)
			{
				TSource val = source[num];
				num = _state++;
				if (_predicate(val))
				{
					_current = val;
					return true;
				}
			}
			Dispose();
			return false;
		}

		public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
		{
			return new WhereSelectArrayIterator<TSource, TResult>(_source, _predicate, selector);
		}

		public TSource[] ToArray()
		{
			System.Collections.Generic.LargeArrayBuilder<TSource> largeArrayBuilder = new System.Collections.Generic.LargeArrayBuilder<TSource>(_source.Length);
			TSource[] source = _source;
			foreach (TSource val in source)
			{
				if (_predicate(val))
				{
					largeArrayBuilder.Add(val);
				}
			}
			return largeArrayBuilder.ToArray();
		}

		public List<TSource> ToList()
		{
			List<TSource> list = new List<TSource>();
			TSource[] source = _source;
			foreach (TSource val in source)
			{
				if (_predicate(val))
				{
					list.Add(val);
				}
			}
			return list;
		}

		public override IEnumerable<TSource> Where(Func<TSource, bool> predicate)
		{
			return new WhereArrayIterator<TSource>(_source, Utilities.CombinePredicates(_predicate, predicate));
		}
	}

	private sealed class WhereListIterator<TSource> : Iterator<TSource>, IIListProvider<TSource>, IEnumerable<TSource>, IEnumerable
	{
		private readonly List<TSource> _source;

		private readonly Func<TSource, bool> _predicate;

		private List<TSource>.Enumerator _enumerator;

		public WhereListIterator(List<TSource> source, Func<TSource, bool> predicate)
		{
			_source = source;
			_predicate = predicate;
		}

		public override Iterator<TSource> Clone()
		{
			return new WhereListIterator<TSource>(_source, _predicate);
		}

		public int GetCount(bool onlyIfCheap)
		{
			if (onlyIfCheap)
			{
				return -1;
			}
			int num = 0;
			for (int i = 0; i < _source.Count; i++)
			{
				TSource arg = _source[i];
				if (_predicate(arg))
				{
					num = checked(num + 1);
				}
			}
			return num;
		}

		public override bool MoveNext()
		{
			int state = _state;
			if (state != 1)
			{
				if (state != 2)
				{
					goto IL_0061;
				}
			}
			else
			{
				_enumerator = _source.GetEnumerator();
				_state = 2;
			}
			while (_enumerator.MoveNext())
			{
				TSource current = _enumerator.Current;
				if (_predicate(current))
				{
					_current = current;
					return true;
				}
			}
			Dispose();
			goto IL_0061;
			IL_0061:
			return false;
		}

		public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
		{
			return new WhereSelectListIterator<TSource, TResult>(_source, _predicate, selector);
		}

		public TSource[] ToArray()
		{
			System.Collections.Generic.LargeArrayBuilder<TSource> largeArrayBuilder = new System.Collections.Generic.LargeArrayBuilder<TSource>(_source.Count);
			for (int i = 0; i < _source.Count; i++)
			{
				TSource val = _source[i];
				if (_predicate(val))
				{
					largeArrayBuilder.Add(val);
				}
			}
			return largeArrayBuilder.ToArray();
		}

		public List<TSource> ToList()
		{
			List<TSource> list = new List<TSource>();
			for (int i = 0; i < _source.Count; i++)
			{
				TSource val = _source[i];
				if (_predicate(val))
				{
					list.Add(val);
				}
			}
			return list;
		}

		public override IEnumerable<TSource> Where(Func<TSource, bool> predicate)
		{
			return new WhereListIterator<TSource>(_source, Utilities.CombinePredicates(_predicate, predicate));
		}
	}

	private sealed class WhereSelectArrayIterator<TSource, TResult> : Iterator<TResult>, IIListProvider<TResult>, IEnumerable<TResult>, IEnumerable
	{
		private readonly TSource[] _source;

		private readonly Func<TSource, bool> _predicate;

		private readonly Func<TSource, TResult> _selector;

		public WhereSelectArrayIterator(TSource[] source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
		{
			_source = source;
			_predicate = predicate;
			_selector = selector;
		}

		public override Iterator<TResult> Clone()
		{
			return new WhereSelectArrayIterator<TSource, TResult>(_source, _predicate, _selector);
		}

		public int GetCount(bool onlyIfCheap)
		{
			if (onlyIfCheap)
			{
				return -1;
			}
			int num = 0;
			TSource[] source = _source;
			foreach (TSource arg in source)
			{
				if (_predicate(arg))
				{
					_selector(arg);
					num = checked(num + 1);
				}
			}
			return num;
		}

		public override bool MoveNext()
		{
			int num = _state - 1;
			TSource[] source = _source;
			while ((uint)num < (uint)source.Length)
			{
				TSource arg = source[num];
				num = _state++;
				if (_predicate(arg))
				{
					_current = _selector(arg);
					return true;
				}
			}
			Dispose();
			return false;
		}

		public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
		{
			return new WhereSelectArrayIterator<TSource, TResult2>(_source, _predicate, Utilities.CombineSelectors(_selector, selector));
		}

		public TResult[] ToArray()
		{
			System.Collections.Generic.LargeArrayBuilder<TResult> largeArrayBuilder = new System.Collections.Generic.LargeArrayBuilder<TResult>(_source.Length);
			TSource[] source = _source;
			foreach (TSource arg in source)
			{
				if (_predicate(arg))
				{
					largeArrayBuilder.Add(_selector(arg));
				}
			}
			return largeArrayBuilder.ToArray();
		}

		public List<TResult> ToList()
		{
			List<TResult> list = new List<TResult>();
			TSource[] source = _source;
			foreach (TSource arg in source)
			{
				if (_predicate(arg))
				{
					list.Add(_selector(arg));
				}
			}
			return list;
		}
	}

	private sealed class WhereSelectListIterator<TSource, TResult> : Iterator<TResult>, IIListProvider<TResult>, IEnumerable<TResult>, IEnumerable
	{
		private readonly List<TSource> _source;

		private readonly Func<TSource, bool> _predicate;

		private readonly Func<TSource, TResult> _selector;

		private List<TSource>.Enumerator _enumerator;

		public WhereSelectListIterator(List<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
		{
			_source = source;
			_predicate = predicate;
			_selector = selector;
		}

		public override Iterator<TResult> Clone()
		{
			return new WhereSelectListIterator<TSource, TResult>(_source, _predicate, _selector);
		}

		public int GetCount(bool onlyIfCheap)
		{
			if (onlyIfCheap)
			{
				return -1;
			}
			int num = 0;
			for (int i = 0; i < _source.Count; i++)
			{
				TSource arg = _source[i];
				if (_predicate(arg))
				{
					_selector(arg);
					num = checked(num + 1);
				}
			}
			return num;
		}

		public override bool MoveNext()
		{
			int state = _state;
			if (state != 1)
			{
				if (state != 2)
				{
					goto IL_006c;
				}
			}
			else
			{
				_enumerator = _source.GetEnumerator();
				_state = 2;
			}
			while (_enumerator.MoveNext())
			{
				TSource current = _enumerator.Current;
				if (_predicate(current))
				{
					_current = _selector(current);
					return true;
				}
			}
			Dispose();
			goto IL_006c;
			IL_006c:
			return false;
		}

		public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
		{
			return new WhereSelectListIterator<TSource, TResult2>(_source, _predicate, Utilities.CombineSelectors(_selector, selector));
		}

		public TResult[] ToArray()
		{
			System.Collections.Generic.LargeArrayBuilder<TResult> largeArrayBuilder = new System.Collections.Generic.LargeArrayBuilder<TResult>(_source.Count);
			for (int i = 0; i < _source.Count; i++)
			{
				TSource arg = _source[i];
				if (_predicate(arg))
				{
					largeArrayBuilder.Add(_selector(arg));
				}
			}
			return largeArrayBuilder.ToArray();
		}

		public List<TResult> ToList()
		{
			List<TResult> list = new List<TResult>();
			for (int i = 0; i < _source.Count; i++)
			{
				TSource arg = _source[i];
				if (_predicate(arg))
				{
					list.Add(_selector(arg));
				}
			}
			return list;
		}
	}

	private sealed class WhereSelectEnumerableIterator<TSource, TResult> : Iterator<TResult>, IIListProvider<TResult>, IEnumerable<TResult>, IEnumerable
	{
		private readonly IEnumerable<TSource> _source;

		private readonly Func<TSource, bool> _predicate;

		private readonly Func<TSource, TResult> _selector;

		private IEnumerator<TSource> _enumerator;

		public WhereSelectEnumerableIterator(IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
		{
			_source = source;
			_predicate = predicate;
			_selector = selector;
		}

		public override Iterator<TResult> Clone()
		{
			return new WhereSelectEnumerableIterator<TSource, TResult>(_source, _predicate, _selector);
		}

		public override void Dispose()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
			}
			base.Dispose();
		}

		public int GetCount(bool onlyIfCheap)
		{
			if (onlyIfCheap)
			{
				return -1;
			}
			int num = 0;
			foreach (TSource item in _source)
			{
				if (_predicate(item))
				{
					_selector(item);
					num = checked(num + 1);
				}
			}
			return num;
		}

		public override bool MoveNext()
		{
			int state = _state;
			if (state != 1)
			{
				if (state != 2)
				{
					goto IL_006c;
				}
			}
			else
			{
				_enumerator = _source.GetEnumerator();
				_state = 2;
			}
			while (_enumerator.MoveNext())
			{
				TSource current = _enumerator.Current;
				if (_predicate(current))
				{
					_current = _selector(current);
					return true;
				}
			}
			Dispose();
			goto IL_006c;
			IL_006c:
			return false;
		}

		public override IEnumerable<TResult2> Select<TResult2>(Func<TResult, TResult2> selector)
		{
			return new WhereSelectEnumerableIterator<TSource, TResult2>(_source, _predicate, Utilities.CombineSelectors(_selector, selector));
		}

		public TResult[] ToArray()
		{
			System.Collections.Generic.LargeArrayBuilder<TResult> largeArrayBuilder = new System.Collections.Generic.LargeArrayBuilder<TResult>(initialize: true);
			foreach (TSource item in _source)
			{
				if (_predicate(item))
				{
					largeArrayBuilder.Add(_selector(item));
				}
			}
			return largeArrayBuilder.ToArray();
		}

		public List<TResult> ToList()
		{
			List<TResult> list = new List<TResult>();
			foreach (TSource item in _source)
			{
				if (_predicate(item))
				{
					list.Add(_selector(item));
				}
			}
			return list;
		}
	}

	/// <summary>Applies an accumulator function over a sequence.</summary>
	/// <returns>The final accumulator value.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to aggregate over.</param>
	/// <param name="func">An accumulator function to be invoked on each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="func" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (func == null)
		{
			throw Error.ArgumentNull("func");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		TSource val = enumerator.Current;
		while (enumerator.MoveNext())
		{
			val = func(val, enumerator.Current);
		}
		return val;
	}

	/// <summary>Applies an accumulator function over a sequence. The specified seed value is used as the initial accumulator value.</summary>
	/// <returns>The final accumulator value.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to aggregate over.</param>
	/// <param name="seed">The initial accumulator value.</param>
	/// <param name="func">An accumulator function to be invoked on each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="func" /> is null.</exception>
	public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (func == null)
		{
			throw Error.ArgumentNull("func");
		}
		TAccumulate val = seed;
		foreach (TSource item in source)
		{
			val = func(val, item);
		}
		return val;
	}

	/// <summary>Applies an accumulator function over a sequence. The specified seed value is used as the initial accumulator value, and the specified function is used to select the result value.</summary>
	/// <returns>The transformed final accumulator value.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to aggregate over.</param>
	/// <param name="seed">The initial accumulator value.</param>
	/// <param name="func">An accumulator function to be invoked on each element.</param>
	/// <param name="resultSelector">A function to transform the final accumulator value into the result value.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
	/// <typeparam name="TResult">The type of the resulting value.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="func" /> or <paramref name="resultSelector" /> is null.</exception>
	public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (func == null)
		{
			throw Error.ArgumentNull("func");
		}
		if (resultSelector == null)
		{
			throw Error.ArgumentNull("resultSelector");
		}
		TAccumulate val = seed;
		foreach (TSource item in source)
		{
			val = func(val, item);
		}
		return resultSelector(val);
	}

	/// <summary>Determines whether a sequence contains any elements.</summary>
	/// <returns>true if the source sequence contains any elements; otherwise, false.</returns>
	/// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1" /> to check for emptiness.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static bool Any<TSource>(this IEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		return enumerator.MoveNext();
	}

	/// <summary>Determines whether any element of a sequence satisfies a condition.</summary>
	/// <returns>true if any elements in the source sequence pass the test in the specified predicate; otherwise, false.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements to apply the predicate to.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Determines whether all elements of a sequence satisfy a condition.</summary>
	/// <returns>true if every element of the source sequence passes the test in the specified predicate, or if the sequence is empty; otherwise, false.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the elements to apply the predicate to.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		foreach (TSource item in source)
		{
			if (!predicate(item))
			{
				return false;
			}
		}
		return true;
	}

	public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource element)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (!(source is AppendPrependIterator<TSource> appendPrependIterator))
		{
			return new AppendPrepend1Iterator<TSource>(source, element, appending: true);
		}
		return appendPrependIterator.Append(element);
	}

	public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource element)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (!(source is AppendPrependIterator<TSource> appendPrependIterator))
		{
			return new AppendPrepend1Iterator<TSource>(source, element, appending: false);
		}
		return appendPrependIterator.Prepend(element);
	}

	/// <summary>Computes the average of a sequence of <see cref="T:System.Int32" /> values.</summary>
	/// <returns>The average of the sequence of values.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Int32" /> values to calculate the average of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static double Average(this IEnumerable<int> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		checked
		{
			using IEnumerator<int> enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				throw Error.NoElements();
			}
			long num = enumerator.Current;
			long num2 = 1L;
			while (enumerator.MoveNext())
			{
				num += enumerator.Current;
				num2++;
			}
			return (double)num / (double)num2;
		}
	}

	/// <summary>Computes the average of a sequence of nullable <see cref="T:System.Int32" /> values.</summary>
	/// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Int32" /> values to calculate the average of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum of the elements in the sequence is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
	public static double? Average(this IEnumerable<int?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		checked
		{
			using (IEnumerator<int?> enumerator = source.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					int? current = enumerator.Current;
					if (!current.HasValue)
					{
						continue;
					}
					long num = current.GetValueOrDefault();
					long num2 = 1L;
					while (enumerator.MoveNext())
					{
						current = enumerator.Current;
						if (current.HasValue)
						{
							num += current.GetValueOrDefault();
							num2++;
						}
					}
					return (double)num / (double)num2;
				}
			}
			return null;
		}
	}

	/// <summary>Computes the average of a sequence of <see cref="T:System.Int64" /> values.</summary>
	/// <returns>The average of the sequence of values.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Int64" /> values to calculate the average of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static double Average(this IEnumerable<long> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		checked
		{
			using IEnumerator<long> enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				throw Error.NoElements();
			}
			long num = enumerator.Current;
			long num2 = 1L;
			while (enumerator.MoveNext())
			{
				num += enumerator.Current;
				num2++;
			}
			return (double)num / (double)num2;
		}
	}

	/// <summary>Computes the average of a sequence of nullable <see cref="T:System.Int64" /> values.</summary>
	/// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Int64" /> values to calculate the average of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum of the elements in the sequence is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
	public static double? Average(this IEnumerable<long?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		checked
		{
			using (IEnumerator<long?> enumerator = source.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					long? current = enumerator.Current;
					if (!current.HasValue)
					{
						continue;
					}
					long num = current.GetValueOrDefault();
					long num2 = 1L;
					while (enumerator.MoveNext())
					{
						current = enumerator.Current;
						if (current.HasValue)
						{
							num += current.GetValueOrDefault();
							num2++;
						}
					}
					return (double)num / (double)num2;
				}
			}
			return null;
		}
	}

	/// <summary>Computes the average of a sequence of <see cref="T:System.Single" /> values.</summary>
	/// <returns>The average of the sequence of values.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Single" /> values to calculate the average of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static float Average(this IEnumerable<float> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using IEnumerator<float> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		double num = enumerator.Current;
		long num2 = 1L;
		while (enumerator.MoveNext())
		{
			num += (double)enumerator.Current;
			num2++;
		}
		return (float)(num / (double)num2);
	}

	/// <summary>Computes the average of a sequence of nullable <see cref="T:System.Single" /> values.</summary>
	/// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Single" /> values to calculate the average of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static float? Average(this IEnumerable<float?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using (IEnumerator<float?> enumerator = source.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				float? current = enumerator.Current;
				if (!current.HasValue)
				{
					continue;
				}
				double num = current.GetValueOrDefault();
				long num2 = 1L;
				while (enumerator.MoveNext())
				{
					current = enumerator.Current;
					if (current.HasValue)
					{
						num += (double)current.GetValueOrDefault();
						num2 = checked(num2 + 1);
					}
				}
				return (float)(num / (double)num2);
			}
		}
		return null;
	}

	/// <summary>Computes the average of a sequence of <see cref="T:System.Double" /> values.</summary>
	/// <returns>The average of the sequence of values.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Double" /> values to calculate the average of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static double Average(this IEnumerable<double> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using IEnumerator<double> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		double num = enumerator.Current;
		long num2 = 1L;
		while (enumerator.MoveNext())
		{
			num += enumerator.Current;
			num2++;
		}
		return num / (double)num2;
	}

	/// <summary>Computes the average of a sequence of nullable <see cref="T:System.Double" /> values.</summary>
	/// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Double" /> values to calculate the average of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static double? Average(this IEnumerable<double?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using (IEnumerator<double?> enumerator = source.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				double? current = enumerator.Current;
				if (!current.HasValue)
				{
					continue;
				}
				double num = current.GetValueOrDefault();
				long num2 = 1L;
				while (enumerator.MoveNext())
				{
					current = enumerator.Current;
					if (current.HasValue)
					{
						num += current.GetValueOrDefault();
						num2 = checked(num2 + 1);
					}
				}
				return num / (double)num2;
			}
		}
		return null;
	}

	/// <summary>Computes the average of a sequence of <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>The average of the sequence of values.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Decimal" /> values to calculate the average of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static decimal Average(this IEnumerable<decimal> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using IEnumerator<decimal> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		decimal current = enumerator.Current;
		long num = 1L;
		while (enumerator.MoveNext())
		{
			current += enumerator.Current;
			num++;
		}
		return current / (decimal)num;
	}

	/// <summary>Computes the average of a sequence of nullable <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Decimal" /> values to calculate the average of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum of the elements in the sequence is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
	public static decimal? Average(this IEnumerable<decimal?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using (IEnumerator<decimal?> enumerator = source.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				decimal? current = enumerator.Current;
				if (!current.HasValue)
				{
					continue;
				}
				decimal valueOrDefault = current.GetValueOrDefault();
				long num = 1L;
				while (enumerator.MoveNext())
				{
					current = enumerator.Current;
					if (current.HasValue)
					{
						valueOrDefault += current.GetValueOrDefault();
						num++;
					}
				}
				return valueOrDefault / (decimal)num;
			}
		}
		return null;
	}

	/// <summary>Computes the average of a sequence of <see cref="T:System.Int32" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The average of the sequence of values.</returns>
	/// <param name="source">A sequence of values to calculate the average of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	/// <exception cref="T:System.OverflowException">The sum of the elements in the sequence is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
	public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		checked
		{
			using IEnumerator<TSource> enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				throw Error.NoElements();
			}
			long num = selector(enumerator.Current);
			long num2 = 1L;
			while (enumerator.MoveNext())
			{
				num += selector(enumerator.Current);
				num2++;
			}
			return (double)num / (double)num2;
		}
	}

	/// <summary>Computes the average of a sequence of nullable <see cref="T:System.Int32" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
	/// <param name="source">A sequence of values to calculate the average of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum of the elements in the sequence is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
	public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		checked
		{
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					int? num = selector(enumerator.Current);
					if (!num.HasValue)
					{
						continue;
					}
					long num2 = num.GetValueOrDefault();
					long num3 = 1L;
					while (enumerator.MoveNext())
					{
						num = selector(enumerator.Current);
						if (num.HasValue)
						{
							num2 += num.GetValueOrDefault();
							num3++;
						}
					}
					return (double)num2 / (double)num3;
				}
			}
			return null;
		}
	}

	/// <summary>Computes the average of a sequence of <see cref="T:System.Int64" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The average of the sequence of values.</returns>
	/// <param name="source">A sequence of values to calculate the average of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of source.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	/// <exception cref="T:System.OverflowException">The sum of the elements in the sequence is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
	public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		checked
		{
			using IEnumerator<TSource> enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				throw Error.NoElements();
			}
			long num = selector(enumerator.Current);
			long num2 = 1L;
			while (enumerator.MoveNext())
			{
				num += selector(enumerator.Current);
				num2++;
			}
			return (double)num / (double)num2;
		}
	}

	/// <summary>Computes the average of a sequence of nullable <see cref="T:System.Int64" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
	/// <param name="source">A sequence of values to calculate the average of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		checked
		{
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					long? num = selector(enumerator.Current);
					if (!num.HasValue)
					{
						continue;
					}
					long num2 = num.GetValueOrDefault();
					long num3 = 1L;
					while (enumerator.MoveNext())
					{
						num = selector(enumerator.Current);
						if (num.HasValue)
						{
							num2 += num.GetValueOrDefault();
							num3++;
						}
					}
					return (double)num2 / (double)num3;
				}
			}
			return null;
		}
	}

	/// <summary>Computes the average of a sequence of <see cref="T:System.Single" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The average of the sequence of values.</returns>
	/// <param name="source">A sequence of values to calculate the average of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static float Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		double num = selector(enumerator.Current);
		long num2 = 1L;
		while (enumerator.MoveNext())
		{
			num += (double)selector(enumerator.Current);
			num2++;
		}
		return (float)(num / (double)num2);
	}

	/// <summary>Computes the average of a sequence of nullable <see cref="T:System.Single" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
	/// <param name="source">A sequence of values to calculate the average of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static float? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using (IEnumerator<TSource> enumerator = source.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				float? num = selector(enumerator.Current);
				if (!num.HasValue)
				{
					continue;
				}
				double num2 = num.GetValueOrDefault();
				long num3 = 1L;
				while (enumerator.MoveNext())
				{
					num = selector(enumerator.Current);
					if (num.HasValue)
					{
						num2 += (double)num.GetValueOrDefault();
						num3 = checked(num3 + 1);
					}
				}
				return (float)(num2 / (double)num3);
			}
		}
		return null;
	}

	/// <summary>Computes the average of a sequence of <see cref="T:System.Double" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The average of the sequence of values.</returns>
	/// <param name="source">A sequence of values to calculate the average of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		double num = selector(enumerator.Current);
		long num2 = 1L;
		while (enumerator.MoveNext())
		{
			num += selector(enumerator.Current);
			num2++;
		}
		return num / (double)num2;
	}

	/// <summary>Computes the average of a sequence of nullable <see cref="T:System.Double" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
	/// <param name="source">A sequence of values to calculate the average of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using (IEnumerator<TSource> enumerator = source.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				double? num = selector(enumerator.Current);
				if (!num.HasValue)
				{
					continue;
				}
				double num2 = num.GetValueOrDefault();
				long num3 = 1L;
				while (enumerator.MoveNext())
				{
					num = selector(enumerator.Current);
					if (num.HasValue)
					{
						num2 += num.GetValueOrDefault();
						num3 = checked(num3 + 1);
					}
				}
				return num2 / (double)num3;
			}
		}
		return null;
	}

	/// <summary>Computes the average of a sequence of <see cref="T:System.Decimal" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The average of the sequence of values.</returns>
	/// <param name="source">A sequence of values that are used to calculate an average.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	/// <exception cref="T:System.OverflowException">The sum of the elements in the sequence is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
	public static decimal Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		decimal num = selector(enumerator.Current);
		long num2 = 1L;
		while (enumerator.MoveNext())
		{
			num += selector(enumerator.Current);
			num2++;
		}
		return num / (decimal)num2;
	}

	/// <summary>Computes the average of a sequence of nullable <see cref="T:System.Decimal" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
	/// <param name="source">A sequence of values to calculate the average of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum of the elements in the sequence is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
	public static decimal? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using (IEnumerator<TSource> enumerator = source.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				decimal? num = selector(enumerator.Current);
				if (!num.HasValue)
				{
					continue;
				}
				decimal valueOrDefault = num.GetValueOrDefault();
				long num2 = 1L;
				while (enumerator.MoveNext())
				{
					num = selector(enumerator.Current);
					if (num.HasValue)
					{
						valueOrDefault += num.GetValueOrDefault();
						num2++;
					}
				}
				return valueOrDefault / (decimal)num2;
			}
		}
		return null;
	}

	/// <summary>Filters the elements of an <see cref="T:System.Collections.IEnumerable" /> based on a specified type.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains elements from the input sequence of type <paramref name="TResult" />.</returns>
	/// <param name="source">The <see cref="T:System.Collections.IEnumerable" /> whose elements to filter.</param>
	/// <typeparam name="TResult">The type to filter the elements of the sequence on.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static IEnumerable<TResult> OfType<TResult>(this IEnumerable source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return OfTypeIterator<TResult>(source);
	}

	private static IEnumerable<TResult> OfTypeIterator<TResult>(IEnumerable source)
	{
		foreach (object item in source)
		{
			if (item is TResult)
			{
				yield return (TResult)item;
			}
		}
	}

	/// <summary>Casts the elements of an <see cref="T:System.Collections.IEnumerable" /> to the specified type.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains each element of the source sequence cast to the specified type.</returns>
	/// <param name="source">The <see cref="T:System.Collections.IEnumerable" /> that contains the elements to be cast to type <paramref name="TResult" />.</param>
	/// <typeparam name="TResult">The type to cast the elements of <paramref name="source" /> to.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">An element in the sequence cannot be cast to type <paramref name="TResult" />.</exception>
	public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source)
	{
		if (source is IEnumerable<TResult> result)
		{
			return result;
		}
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return CastIterator<TResult>(source);
	}

	private static IEnumerable<TResult> CastIterator<TResult>(IEnumerable source)
	{
		foreach (object item in source)
		{
			yield return (TResult)item;
		}
	}

	/// <summary>Concatenates two sequences.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the concatenated elements of the two input sequences.</returns>
	/// <param name="first">The first sequence to concatenate.</param>
	/// <param name="second">The sequence to concatenate to the first sequence.</param>
	/// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="first" /> or <paramref name="second" /> is null.</exception>
	public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
	{
		if (first == null)
		{
			throw Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw Error.ArgumentNull("second");
		}
		if (!(first is ConcatIterator<TSource> concatIterator))
		{
			return new Concat2Iterator<TSource>(first, second);
		}
		return concatIterator.Concat(second);
	}

	/// <summary>Determines whether a sequence contains a specified element by using the default equality comparer.</summary>
	/// <returns>true if the source sequence contains an element that has the specified value; otherwise, false.</returns>
	/// <param name="source">A sequence in which to locate a value.</param>
	/// <param name="value">The value to locate in the sequence.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
	{
		if (!(source is ICollection<TSource> collection))
		{
			return source.Contains(value, null);
		}
		return collection.Contains(value);
	}

	/// <summary>Determines whether a sequence contains a specified element by using a specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
	/// <returns>true if the source sequence contains an element that has the specified value; otherwise, false.</returns>
	/// <param name="source">A sequence in which to locate a value.</param>
	/// <param name="value">The value to locate in the sequence.</param>
	/// <param name="comparer">An equality comparer to compare values.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<TSource>.Default;
		}
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		foreach (TSource item in source)
		{
			if (comparer.Equals(item, value))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Returns the number of elements in a sequence.</summary>
	/// <returns>The number of elements in the input sequence.</returns>
	/// <param name="source">A sequence that contains elements to be counted.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The number of elements in <paramref name="source" /> is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
	public static int Count<TSource>(this IEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (source is ICollection<TSource> collection)
		{
			return collection.Count;
		}
		if (source is IIListProvider<TSource> iIListProvider)
		{
			return iIListProvider.GetCount(onlyIfCheap: false);
		}
		if (source is ICollection collection2)
		{
			return collection2.Count;
		}
		int num = 0;
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			num = checked(num + 1);
		}
		return num;
	}

	/// <summary>Returns a number that represents how many elements in the specified sequence satisfy a condition.</summary>
	/// <returns>A number that represents how many elements in the sequence satisfy the condition in the predicate function.</returns>
	/// <param name="source">A sequence that contains elements to be tested and counted.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The number of elements in <paramref name="source" /> is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
	public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		int num = 0;
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				num = checked(num + 1);
			}
		}
		return num;
	}

	/// <summary>Returns an <see cref="T:System.Int64" /> that represents the total number of elements in a sequence.</summary>
	/// <returns>The number of elements in the source sequence.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the elements to be counted.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The number of elements exceeds <see cref="F:System.Int64.MaxValue" />.</exception>
	public static long LongCount<TSource>(this IEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		long num = 0L;
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			num = checked(num + 1);
		}
		return num;
	}

	/// <summary>Returns an <see cref="T:System.Int64" /> that represents how many elements in a sequence satisfy a condition.</summary>
	/// <returns>A number that represents how many elements in the sequence satisfy the condition in the predicate function.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the elements to be counted.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The number of matching elements exceeds <see cref="F:System.Int64.MaxValue" />.</exception>
	public static long LongCount<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		long num = 0L;
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				num = checked(num + 1);
			}
		}
		return num;
	}

	/// <summary>Returns the elements of the specified sequence or the type parameter's default value in a singleton collection if the sequence is empty.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> object that contains the default value for the <paramref name="TSource" /> type if <paramref name="source" /> is empty; otherwise, <paramref name="source" />.</returns>
	/// <param name="source">The sequence to return a default value for if it is empty.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source)
	{
		return source.DefaultIfEmpty(default(TSource));
	}

	/// <summary>Returns the elements of the specified sequence or the specified value in a singleton collection if the sequence is empty.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <paramref name="defaultValue" /> if <paramref name="source" /> is empty; otherwise, <paramref name="source" />.</returns>
	/// <param name="source">The sequence to return the specified value for if it is empty.</param>
	/// <param name="defaultValue">The value to return if the sequence is empty.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return new DefaultIfEmptyIterator<TSource>(source, defaultValue);
	}

	/// <summary>Returns distinct elements from a sequence by using the default equality comparer to compare values.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains distinct elements from the source sequence.</returns>
	/// <param name="source">The sequence to remove duplicate elements from.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source)
	{
		return source.Distinct(null);
	}

	/// <summary>Returns distinct elements from a sequence by using a specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains distinct elements from the source sequence.</returns>
	/// <param name="source">The sequence to remove duplicate elements from.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return new DistinctIterator<TSource>(source, comparer);
	}

	/// <summary>Returns the element at a specified index in a sequence.</summary>
	/// <returns>The element at the specified position in the source sequence.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return an element from.</param>
	/// <param name="index">The zero-based index of the element to retrieve.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0 or greater than or equal to the number of elements in <paramref name="source" />.</exception>
	public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (source is IPartition<TSource> partition)
		{
			bool found;
			TSource result = partition.TryGetElementAt(index, out found);
			if (found)
			{
				return result;
			}
		}
		else
		{
			if (source is IList<TSource> list)
			{
				return list[index];
			}
			if (index >= 0)
			{
				using IEnumerator<TSource> enumerator = source.GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (index == 0)
					{
						return enumerator.Current;
					}
					index--;
				}
			}
		}
		throw Error.ArgumentOutOfRange("index");
	}

	/// <summary>Returns the element at a specified index in a sequence or a default value if the index is out of range.</summary>
	/// <returns>default(<paramref name="TSource" />) if the index is outside the bounds of the source sequence; otherwise, the element at the specified position in the source sequence.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return an element from.</param>
	/// <param name="index">The zero-based index of the element to retrieve.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (source is IPartition<TSource> partition)
		{
			bool found;
			return partition.TryGetElementAt(index, out found);
		}
		if (index >= 0)
		{
			if (source is IList<TSource> list)
			{
				if (index < list.Count)
				{
					return list[index];
				}
			}
			else
			{
				using IEnumerator<TSource> enumerator = source.GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (index == 0)
					{
						return enumerator.Current;
					}
					index--;
				}
			}
		}
		return default(TSource);
	}

	/// <summary>Returns the input typed as <see cref="T:System.Collections.Generic.IEnumerable`1" />.</summary>
	/// <returns>The input sequence typed as <see cref="T:System.Collections.Generic.IEnumerable`1" />.</returns>
	/// <param name="source">The sequence to type as <see cref="T:System.Collections.Generic.IEnumerable`1" />.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source)
	{
		return source;
	}

	/// <summary>Returns an empty <see cref="T:System.Collections.Generic.IEnumerable`1" /> that has the specified type argument.</summary>
	/// <returns>An empty <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose type argument is <paramref name="TResult" />.</returns>
	/// <typeparam name="TResult">The type to assign to the type parameter of the returned generic <see cref="T:System.Collections.Generic.IEnumerable`1" />.</typeparam>
	public static IEnumerable<TResult> Empty<TResult>()
	{
		return Array.Empty<TResult>();
	}

	/// <summary>Produces the set difference of two sequences by using the default equality comparer to compare values.</summary>
	/// <returns>A sequence that contains the set difference of the elements of two sequences.</returns>
	/// <param name="first">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements that are not also in <paramref name="second" /> will be returned.</param>
	/// <param name="second">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements that also occur in the first sequence will cause those elements to be removed from the returned sequence.</param>
	/// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="first" /> or <paramref name="second" /> is null.</exception>
	public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
	{
		if (first == null)
		{
			throw Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw Error.ArgumentNull("second");
		}
		return ExceptIterator(first, second, null);
	}

	/// <summary>Produces the set difference of two sequences by using the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</summary>
	/// <returns>A sequence that contains the set difference of the elements of two sequences.</returns>
	/// <param name="first">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements that are not also in <paramref name="second" /> will be returned.</param>
	/// <param name="second">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements that also occur in the first sequence will cause those elements to be removed from the returned sequence.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</param>
	/// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="first" /> or <paramref name="second" /> is null.</exception>
	public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		if (first == null)
		{
			throw Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw Error.ArgumentNull("second");
		}
		return ExceptIterator(first, second, comparer);
	}

	private static IEnumerable<TSource> ExceptIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		Set<TSource> set = new Set<TSource>(comparer);
		foreach (TSource item in second)
		{
			set.Add(item);
		}
		foreach (TSource item2 in first)
		{
			if (set.Add(item2))
			{
				yield return item2;
			}
		}
	}

	/// <summary>Returns the first element of a sequence.</summary>
	/// <returns>The first element in the specified sequence.</returns>
	/// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return the first element of.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The source sequence is empty.</exception>
	public static TSource First<TSource>(this IEnumerable<TSource> source)
	{
		bool found;
		TSource result = source.TryGetFirst(out found);
		if (!found)
		{
			throw Error.NoElements();
		}
		return result;
	}

	/// <summary>Returns the first element in a sequence that satisfies a specified condition.</summary>
	/// <returns>The first element in the sequence that passes the test in the specified predicate function.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return an element from.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">No element satisfies the condition in <paramref name="predicate" />.-or-The source sequence is empty.</exception>
	public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		bool found;
		TSource result = source.TryGetFirst(predicate, out found);
		if (!found)
		{
			throw Error.NoMatch();
		}
		return result;
	}

	/// <summary>Returns the first element of a sequence, or a default value if the sequence contains no elements.</summary>
	/// <returns>default(<paramref name="TSource" />) if <paramref name="source" /> is empty; otherwise, the first element in <paramref name="source" />.</returns>
	/// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return the first element of.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
	{
		bool found;
		return source.TryGetFirst(out found);
	}

	/// <summary>Returns the first element of the sequence that satisfies a condition or a default value if no such element is found.</summary>
	/// <returns>default(<paramref name="TSource" />) if <paramref name="source" /> is empty or if no element passes the test specified by <paramref name="predicate" />; otherwise, the first element in <paramref name="source" /> that passes the test specified by <paramref name="predicate" />.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return an element from.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		bool found;
		return source.TryGetFirst(predicate, out found);
	}

	private static TSource TryGetFirst<TSource>(this IEnumerable<TSource> source, out bool found)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (source is IPartition<TSource> partition)
		{
			return partition.TryGetFirst(out found);
		}
		if (source is IList<TSource> list)
		{
			if (list.Count > 0)
			{
				found = true;
				return list[0];
			}
		}
		else
		{
			using IEnumerator<TSource> enumerator = source.GetEnumerator();
			if (enumerator.MoveNext())
			{
				found = true;
				return enumerator.Current;
			}
		}
		found = false;
		return default(TSource);
	}

	private static TSource TryGetFirst<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, out bool found)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		if (source is OrderedEnumerable<TSource> orderedEnumerable)
		{
			return orderedEnumerable.TryGetFirst(predicate, out found);
		}
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				found = true;
				return item;
			}
		}
		found = false;
		return default(TSource);
	}

	/// <summary>Correlates the elements of two sequences based on equality of keys and groups the results. The default equality comparer is used to compare keys.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains elements of type <paramref name="TResult" /> that are obtained by performing a grouped join on two sequences.</returns>
	/// <param name="outer">The first sequence to join.</param>
	/// <param name="inner">The sequence to join to the first sequence.</param>
	/// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
	/// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
	/// <param name="resultSelector">A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
	/// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
	/// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
	/// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
	/// <typeparam name="TResult">The type of the result elements.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="outer" /> or <paramref name="inner" /> or <paramref name="outerKeySelector" /> or <paramref name="innerKeySelector" /> or <paramref name="resultSelector" /> is null.</exception>
	public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
	{
		if (outer == null)
		{
			throw Error.ArgumentNull("outer");
		}
		if (inner == null)
		{
			throw Error.ArgumentNull("inner");
		}
		if (outerKeySelector == null)
		{
			throw Error.ArgumentNull("outerKeySelector");
		}
		if (innerKeySelector == null)
		{
			throw Error.ArgumentNull("innerKeySelector");
		}
		if (resultSelector == null)
		{
			throw Error.ArgumentNull("resultSelector");
		}
		return GroupJoinIterator(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
	}

	/// <summary>Correlates the elements of two sequences based on key equality and groups the results. A specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> is used to compare keys.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains elements of type <paramref name="TResult" /> that are obtained by performing a grouped join on two sequences.</returns>
	/// <param name="outer">The first sequence to join.</param>
	/// <param name="inner">The sequence to join to the first sequence.</param>
	/// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
	/// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
	/// <param name="resultSelector">A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to hash and compare keys.</param>
	/// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
	/// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
	/// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
	/// <typeparam name="TResult">The type of the result elements.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="outer" /> or <paramref name="inner" /> or <paramref name="outerKeySelector" /> or <paramref name="innerKeySelector" /> or <paramref name="resultSelector" /> is null.</exception>
	public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		if (outer == null)
		{
			throw Error.ArgumentNull("outer");
		}
		if (inner == null)
		{
			throw Error.ArgumentNull("inner");
		}
		if (outerKeySelector == null)
		{
			throw Error.ArgumentNull("outerKeySelector");
		}
		if (innerKeySelector == null)
		{
			throw Error.ArgumentNull("innerKeySelector");
		}
		if (resultSelector == null)
		{
			throw Error.ArgumentNull("resultSelector");
		}
		return GroupJoinIterator(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
	}

	private static IEnumerable<TResult> GroupJoinIterator<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		using IEnumerator<TOuter> e = outer.GetEnumerator();
		if (e.MoveNext())
		{
			Lookup<TKey, TInner> lookup = Lookup<TKey, TInner>.CreateForJoin(inner, innerKeySelector, comparer);
			do
			{
				TOuter current = e.Current;
				yield return resultSelector(current, lookup[outerKeySelector(current)]);
			}
			while (e.MoveNext());
		}
	}

	/// <summary>Groups the elements of a sequence according to a specified key selector function.</summary>
	/// <returns>An IEnumerable&lt;IGrouping&lt;TKey, TSource&gt;&gt; in C# or IEnumerable(Of IGrouping(Of TKey, TSource)) in Visual Basic where each <see cref="T:System.Linq.IGrouping`2" /> object contains a sequence of objects and a key.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements to group.</param>
	/// <param name="keySelector">A function to extract the key for each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
	public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return new GroupedEnumerable<TSource, TKey>(source, keySelector, null);
	}

	/// <summary>Groups the elements of a sequence according to a specified key selector function and compares the keys by using a specified comparer.</summary>
	/// <returns>An IEnumerable&lt;IGrouping&lt;TKey, TSource&gt;&gt; in C# or IEnumerable(Of IGrouping(Of TKey, TSource)) in Visual Basic where each <see cref="T:System.Linq.IGrouping`2" /> object contains a collection of objects and a key.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements to group.</param>
	/// <param name="keySelector">A function to extract the key for each element.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
	public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		return new GroupedEnumerable<TSource, TKey>(source, keySelector, comparer);
	}

	/// <summary>Groups the elements of a sequence according to a specified key selector function and projects the elements for each group by using a specified function.</summary>
	/// <returns>An IEnumerable&lt;IGrouping&lt;TKey, TElement&gt;&gt; in C# or IEnumerable(Of IGrouping(Of TKey, TElement)) in Visual Basic where each <see cref="T:System.Linq.IGrouping`2" /> object contains a collection of objects of type <paramref name="TElement" /> and a key.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements to group.</param>
	/// <param name="keySelector">A function to extract the key for each element.</param>
	/// <param name="elementSelector">A function to map each source element to an element in the <see cref="T:System.Linq.IGrouping`2" />.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <typeparam name="TElement">The type of the elements in the <see cref="T:System.Linq.IGrouping`2" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> is null.</exception>
	public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
	{
		return new GroupedEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
	}

	/// <summary>Groups the elements of a sequence according to a key selector function. The keys are compared by using a comparer and each group's elements are projected by using a specified function.</summary>
	/// <returns>An IEnumerable&lt;IGrouping&lt;TKey, TElement&gt;&gt; in C# or IEnumerable(Of IGrouping(Of TKey, TElement)) in Visual Basic where each <see cref="T:System.Linq.IGrouping`2" /> object contains a collection of objects of type <paramref name="TElement" /> and a key.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements to group.</param>
	/// <param name="keySelector">A function to extract the key for each element.</param>
	/// <param name="elementSelector">A function to map each source element to an element in an <see cref="T:System.Linq.IGrouping`2" />.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <typeparam name="TElement">The type of the elements in the <see cref="T:System.Linq.IGrouping`2" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> is null.</exception>
	public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		return new GroupedEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
	}

	/// <summary>Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key.</summary>
	/// <returns>A collection of elements of type <paramref name="TResult" /> where each element represents a projection over a group and its key.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements to group.</param>
	/// <param name="keySelector">A function to extract the key for each element.</param>
	/// <param name="resultSelector">A function to create a result value from each group.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector" />.</typeparam>
	public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
	{
		return new GroupedResultEnumerable<TSource, TKey, TResult>(source, keySelector, resultSelector, null);
	}

	/// <summary>Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. The elements of each group are projected by using a specified function.</summary>
	/// <returns>A collection of elements of type <paramref name="TResult" /> where each element represents a projection over a group and its key.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements to group.</param>
	/// <param name="keySelector">A function to extract the key for each element.</param>
	/// <param name="elementSelector">A function to map each source element to an element in an <see cref="T:System.Linq.IGrouping`2" />.</param>
	/// <param name="resultSelector">A function to create a result value from each group.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <typeparam name="TElement">The type of the elements in each <see cref="T:System.Linq.IGrouping`2" />.</typeparam>
	/// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector" />.</typeparam>
	public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
	{
		return new GroupedResultEnumerable<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, null);
	}

	/// <summary>Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. The keys are compared by using a specified comparer.</summary>
	/// <returns>A collection of elements of type <paramref name="TResult" /> where each element represents a projection over a group and its key.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements to group.</param>
	/// <param name="keySelector">A function to extract the key for each element.</param>
	/// <param name="resultSelector">A function to create a result value from each group.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys with.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector" />.</typeparam>
	public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		return new GroupedResultEnumerable<TSource, TKey, TResult>(source, keySelector, resultSelector, comparer);
	}

	/// <summary>Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. Key values are compared by using a specified comparer, and the elements of each group are projected by using a specified function.</summary>
	/// <returns>A collection of elements of type <paramref name="TResult" /> where each element represents a projection over a group and its key.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements to group.</param>
	/// <param name="keySelector">A function to extract the key for each element.</param>
	/// <param name="elementSelector">A function to map each source element to an element in an <see cref="T:System.Linq.IGrouping`2" />.</param>
	/// <param name="resultSelector">A function to create a result value from each group.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys with.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <typeparam name="TElement">The type of the elements in each <see cref="T:System.Linq.IGrouping`2" />.</typeparam>
	/// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector" />.</typeparam>
	public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		return new GroupedResultEnumerable<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, comparer);
	}

	/// <summary>Produces the set intersection of two sequences by using the default equality comparer to compare values.</summary>
	/// <returns>A sequence that contains the elements that form the set intersection of two sequences.</returns>
	/// <param name="first">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose distinct elements that also appear in <paramref name="second" /> will be returned.</param>
	/// <param name="second">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose distinct elements that also appear in the first sequence will be returned.</param>
	/// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="first" /> or <paramref name="second" /> is null.</exception>
	public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
	{
		if (first == null)
		{
			throw Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw Error.ArgumentNull("second");
		}
		return IntersectIterator(first, second, null);
	}

	/// <summary>Produces the set intersection of two sequences by using the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</summary>
	/// <returns>A sequence that contains the elements that form the set intersection of two sequences.</returns>
	/// <param name="first">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose distinct elements that also appear in <paramref name="second" /> will be returned.</param>
	/// <param name="second">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose distinct elements that also appear in the first sequence will be returned.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</param>
	/// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="first" /> or <paramref name="second" /> is null.</exception>
	public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		if (first == null)
		{
			throw Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw Error.ArgumentNull("second");
		}
		return IntersectIterator(first, second, comparer);
	}

	private static IEnumerable<TSource> IntersectIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		Set<TSource> set = new Set<TSource>(comparer);
		foreach (TSource item in second)
		{
			set.Add(item);
		}
		foreach (TSource item2 in first)
		{
			if (set.Remove(item2))
			{
				yield return item2;
			}
		}
	}

	/// <summary>Correlates the elements of two sequences based on matching keys. The default equality comparer is used to compare keys.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that has elements of type <paramref name="TResult" /> that are obtained by performing an inner join on two sequences.</returns>
	/// <param name="outer">The first sequence to join.</param>
	/// <param name="inner">The sequence to join to the first sequence.</param>
	/// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
	/// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
	/// <param name="resultSelector">A function to create a result element from two matching elements.</param>
	/// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
	/// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
	/// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
	/// <typeparam name="TResult">The type of the result elements.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="outer" /> or <paramref name="inner" /> or <paramref name="outerKeySelector" /> or <paramref name="innerKeySelector" /> or <paramref name="resultSelector" /> is null.</exception>
	public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
	{
		if (outer == null)
		{
			throw Error.ArgumentNull("outer");
		}
		if (inner == null)
		{
			throw Error.ArgumentNull("inner");
		}
		if (outerKeySelector == null)
		{
			throw Error.ArgumentNull("outerKeySelector");
		}
		if (innerKeySelector == null)
		{
			throw Error.ArgumentNull("innerKeySelector");
		}
		if (resultSelector == null)
		{
			throw Error.ArgumentNull("resultSelector");
		}
		return JoinIterator(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
	}

	/// <summary>Correlates the elements of two sequences based on matching keys. A specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> is used to compare keys.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that has elements of type <paramref name="TResult" /> that are obtained by performing an inner join on two sequences.</returns>
	/// <param name="outer">The first sequence to join.</param>
	/// <param name="inner">The sequence to join to the first sequence.</param>
	/// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
	/// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
	/// <param name="resultSelector">A function to create a result element from two matching elements.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to hash and compare keys.</param>
	/// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
	/// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
	/// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
	/// <typeparam name="TResult">The type of the result elements.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="outer" /> or <paramref name="inner" /> or <paramref name="outerKeySelector" /> or <paramref name="innerKeySelector" /> or <paramref name="resultSelector" /> is null.</exception>
	public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		if (outer == null)
		{
			throw Error.ArgumentNull("outer");
		}
		if (inner == null)
		{
			throw Error.ArgumentNull("inner");
		}
		if (outerKeySelector == null)
		{
			throw Error.ArgumentNull("outerKeySelector");
		}
		if (innerKeySelector == null)
		{
			throw Error.ArgumentNull("innerKeySelector");
		}
		if (resultSelector == null)
		{
			throw Error.ArgumentNull("resultSelector");
		}
		return JoinIterator(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
	}

	private static IEnumerable<TResult> JoinIterator<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
	{
		using IEnumerator<TOuter> e = outer.GetEnumerator();
		if (!e.MoveNext())
		{
			yield break;
		}
		Lookup<TKey, TInner> lookup = Lookup<TKey, TInner>.CreateForJoin(inner, innerKeySelector, comparer);
		if (lookup.Count == 0)
		{
			yield break;
		}
		do
		{
			TOuter item = e.Current;
			Grouping<TKey, TInner> grouping = lookup.GetGrouping(outerKeySelector(item), create: false);
			if (grouping != null)
			{
				int count = grouping._count;
				TInner[] elements = grouping._elements;
				int i = 0;
				while (i != count)
				{
					yield return resultSelector(item, elements[i]);
					int num = i + 1;
					i = num;
				}
			}
		}
		while (e.MoveNext());
	}

	/// <summary>Returns the last element of a sequence.</summary>
	/// <returns>The value at the last position in the source sequence.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return the last element of.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The source sequence is empty.</exception>
	public static TSource Last<TSource>(this IEnumerable<TSource> source)
	{
		bool found;
		TSource result = source.TryGetLast(out found);
		if (!found)
		{
			throw Error.NoElements();
		}
		return result;
	}

	/// <summary>Returns the last element of a sequence that satisfies a specified condition.</summary>
	/// <returns>The last element in the sequence that passes the test in the specified predicate function.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return an element from.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">No element satisfies the condition in <paramref name="predicate" />.-or-The source sequence is empty.</exception>
	public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		bool found;
		TSource result = source.TryGetLast(predicate, out found);
		if (!found)
		{
			throw Error.NoMatch();
		}
		return result;
	}

	/// <summary>Returns the last element of a sequence, or a default value if the sequence contains no elements.</summary>
	/// <returns>default(<paramref name="TSource" />) if the source sequence is empty; otherwise, the last element in the <see cref="T:System.Collections.Generic.IEnumerable`1" />.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return the last element of.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source)
	{
		bool found;
		return source.TryGetLast(out found);
	}

	/// <summary>Returns the last element of a sequence that satisfies a condition or a default value if no such element is found.</summary>
	/// <returns>default(<paramref name="TSource" />) if the sequence is empty or if no elements pass the test in the predicate function; otherwise, the last element that passes the test in the predicate function.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return an element from.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		bool found;
		return source.TryGetLast(predicate, out found);
	}

	private static TSource TryGetLast<TSource>(this IEnumerable<TSource> source, out bool found)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (source is IPartition<TSource> partition)
		{
			return partition.TryGetLast(out found);
		}
		if (source is IList<TSource> { Count: var count } list)
		{
			if (count > 0)
			{
				found = true;
				return list[count - 1];
			}
		}
		else
		{
			using IEnumerator<TSource> enumerator = source.GetEnumerator();
			if (enumerator.MoveNext())
			{
				TSource current;
				do
				{
					current = enumerator.Current;
				}
				while (enumerator.MoveNext());
				found = true;
				return current;
			}
		}
		found = false;
		return default(TSource);
	}

	private static TSource TryGetLast<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, out bool found)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		if (source is OrderedEnumerable<TSource> orderedEnumerable)
		{
			return orderedEnumerable.TryGetLast(predicate, out found);
		}
		if (source is IList<TSource> list)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				TSource val = list[num];
				if (predicate(val))
				{
					found = true;
					return val;
				}
			}
		}
		else
		{
			using IEnumerator<TSource> enumerator = source.GetEnumerator();
			while (enumerator.MoveNext())
			{
				TSource val2 = enumerator.Current;
				if (!predicate(val2))
				{
					continue;
				}
				while (enumerator.MoveNext())
				{
					TSource current = enumerator.Current;
					if (predicate(current))
					{
						val2 = current;
					}
				}
				found = true;
				return val2;
			}
		}
		found = false;
		return default(TSource);
	}

	/// <summary>Creates a <see cref="T:System.Linq.Lookup`2" /> from an <see cref="T:System.Collections.Generic.IEnumerable`1" /> according to a specified key selector function.</summary>
	/// <returns>A <see cref="T:System.Linq.Lookup`2" /> that contains keys and values.</returns>
	/// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1" /> to create a <see cref="T:System.Linq.Lookup`2" /> from.</param>
	/// <param name="keySelector">A function to extract a key from each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
	public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return source.ToLookup(keySelector, null);
	}

	/// <summary>Creates a <see cref="T:System.Linq.Lookup`2" /> from an <see cref="T:System.Collections.Generic.IEnumerable`1" /> according to a specified key selector function and key comparer.</summary>
	/// <returns>A <see cref="T:System.Linq.Lookup`2" /> that contains keys and values.</returns>
	/// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1" /> to create a <see cref="T:System.Linq.Lookup`2" /> from.</param>
	/// <param name="keySelector">A function to extract a key from each element.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
	public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return Lookup<TKey, TSource>.Create(source, keySelector, comparer);
	}

	/// <summary>Creates a <see cref="T:System.Linq.Lookup`2" /> from an <see cref="T:System.Collections.Generic.IEnumerable`1" /> according to specified key selector and element selector functions.</summary>
	/// <returns>A <see cref="T:System.Linq.Lookup`2" /> that contains values of type <paramref name="TElement" /> selected from the input sequence.</returns>
	/// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1" /> to create a <see cref="T:System.Linq.Lookup`2" /> from.</param>
	/// <param name="keySelector">A function to extract a key from each element.</param>
	/// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> is null.</exception>
	public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
	{
		return source.ToLookup(keySelector, elementSelector, null);
	}

	/// <summary>Creates a <see cref="T:System.Linq.Lookup`2" /> from an <see cref="T:System.Collections.Generic.IEnumerable`1" /> according to a specified key selector function, a comparer and an element selector function.</summary>
	/// <returns>A <see cref="T:System.Linq.Lookup`2" /> that contains values of type <paramref name="TElement" /> selected from the input sequence.</returns>
	/// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1" /> to create a <see cref="T:System.Linq.Lookup`2" /> from.</param>
	/// <param name="keySelector">A function to extract a key from each element.</param>
	/// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> is null.</exception>
	public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		if (elementSelector == null)
		{
			throw Error.ArgumentNull("elementSelector");
		}
		return Lookup<TKey, TElement>.Create(source, keySelector, elementSelector, comparer);
	}

	/// <summary>Returns the maximum value in a sequence of <see cref="T:System.Int32" /> values.</summary>
	/// <returns>The maximum value in the sequence.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Int32" /> values to determine the maximum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static int Max(this IEnumerable<int> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using IEnumerator<int> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		int num = enumerator.Current;
		while (enumerator.MoveNext())
		{
			int current = enumerator.Current;
			if (current > num)
			{
				num = current;
			}
		}
		return num;
	}

	/// <summary>Returns the maximum value in a sequence of nullable <see cref="T:System.Int32" /> values.</summary>
	/// <returns>A value of type Nullable&lt;Int32&gt; in C# or Nullable(Of Int32) in Visual Basic that corresponds to the maximum value in the sequence. </returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Int32" /> values to determine the maximum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static int? Max(this IEnumerable<int?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		int? result = null;
		using (IEnumerator<int?> enumerator = source.GetEnumerator())
		{
			do
			{
				if (!enumerator.MoveNext())
				{
					return result;
				}
				result = enumerator.Current;
			}
			while (!result.HasValue);
			int num = result.GetValueOrDefault();
			if (num >= 0)
			{
				while (enumerator.MoveNext())
				{
					int? current = enumerator.Current;
					int valueOrDefault = current.GetValueOrDefault();
					if (valueOrDefault > num)
					{
						num = valueOrDefault;
						result = current;
					}
				}
			}
			else
			{
				while (enumerator.MoveNext())
				{
					int? current2 = enumerator.Current;
					int valueOrDefault2 = current2.GetValueOrDefault();
					if (current2.HasValue && valueOrDefault2 > num)
					{
						num = valueOrDefault2;
						result = current2;
					}
				}
			}
		}
		return result;
	}

	/// <summary>Returns the maximum value in a sequence of <see cref="T:System.Int64" /> values.</summary>
	/// <returns>The maximum value in the sequence.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Int64" /> values to determine the maximum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static long Max(this IEnumerable<long> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using IEnumerator<long> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		long num = enumerator.Current;
		while (enumerator.MoveNext())
		{
			long current = enumerator.Current;
			if (current > num)
			{
				num = current;
			}
		}
		return num;
	}

	/// <summary>Returns the maximum value in a sequence of nullable <see cref="T:System.Int64" /> values.</summary>
	/// <returns>A value of type Nullable&lt;Int64&gt; in C# or Nullable(Of Int64) in Visual Basic that corresponds to the maximum value in the sequence. </returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Int64" /> values to determine the maximum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static long? Max(this IEnumerable<long?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		long? result = null;
		using (IEnumerator<long?> enumerator = source.GetEnumerator())
		{
			do
			{
				if (!enumerator.MoveNext())
				{
					return result;
				}
				result = enumerator.Current;
			}
			while (!result.HasValue);
			long num = result.GetValueOrDefault();
			if (num >= 0)
			{
				while (enumerator.MoveNext())
				{
					long? current = enumerator.Current;
					long valueOrDefault = current.GetValueOrDefault();
					if (valueOrDefault > num)
					{
						num = valueOrDefault;
						result = current;
					}
				}
			}
			else
			{
				while (enumerator.MoveNext())
				{
					long? current2 = enumerator.Current;
					long valueOrDefault2 = current2.GetValueOrDefault();
					if (current2.HasValue && valueOrDefault2 > num)
					{
						num = valueOrDefault2;
						result = current2;
					}
				}
			}
		}
		return result;
	}

	/// <summary>Returns the maximum value in a sequence of <see cref="T:System.Double" /> values.</summary>
	/// <returns>The maximum value in the sequence.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Double" /> values to determine the maximum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static double Max(this IEnumerable<double> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using IEnumerator<double> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		double num = enumerator.Current;
		while (double.IsNaN(num))
		{
			if (!enumerator.MoveNext())
			{
				return num;
			}
			num = enumerator.Current;
		}
		while (enumerator.MoveNext())
		{
			double current = enumerator.Current;
			if (current > num)
			{
				num = current;
			}
		}
		return num;
	}

	/// <summary>Returns the maximum value in a sequence of nullable <see cref="T:System.Double" /> values.</summary>
	/// <returns>A value of type Nullable&lt;Double&gt; in C# or Nullable(Of Double) in Visual Basic that corresponds to the maximum value in the sequence.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Double" /> values to determine the maximum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static double? Max(this IEnumerable<double?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		double? result = null;
		using IEnumerator<double?> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = enumerator.Current;
			if (!result.HasValue)
			{
				continue;
			}
			double num = result.GetValueOrDefault();
			while (double.IsNaN(num))
			{
				if (!enumerator.MoveNext())
				{
					return result;
				}
				double? current = enumerator.Current;
				if (current.HasValue)
				{
					double? num2 = (result = current);
					num = num2.GetValueOrDefault();
				}
			}
			while (enumerator.MoveNext())
			{
				double? current2 = enumerator.Current;
				double valueOrDefault = current2.GetValueOrDefault();
				if (current2.HasValue && valueOrDefault > num)
				{
					num = valueOrDefault;
					result = current2;
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Returns the maximum value in a sequence of <see cref="T:System.Single" /> values.</summary>
	/// <returns>The maximum value in the sequence.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Single" /> values to determine the maximum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static float Max(this IEnumerable<float> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using IEnumerator<float> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		float num = enumerator.Current;
		while (float.IsNaN(num))
		{
			if (!enumerator.MoveNext())
			{
				return num;
			}
			num = enumerator.Current;
		}
		while (enumerator.MoveNext())
		{
			float current = enumerator.Current;
			if (current > num)
			{
				num = current;
			}
		}
		return num;
	}

	/// <summary>Returns the maximum value in a sequence of nullable <see cref="T:System.Single" /> values.</summary>
	/// <returns>A value of type Nullable&lt;Single&gt; in C# or Nullable(Of Single) in Visual Basic that corresponds to the maximum value in the sequence.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Single" /> values to determine the maximum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static float? Max(this IEnumerable<float?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		float? result = null;
		using IEnumerator<float?> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = enumerator.Current;
			if (!result.HasValue)
			{
				continue;
			}
			float num = result.GetValueOrDefault();
			while (float.IsNaN(num))
			{
				if (!enumerator.MoveNext())
				{
					return result;
				}
				float? current = enumerator.Current;
				if (current.HasValue)
				{
					float? num2 = (result = current);
					num = num2.GetValueOrDefault();
				}
			}
			while (enumerator.MoveNext())
			{
				float? current2 = enumerator.Current;
				float valueOrDefault = current2.GetValueOrDefault();
				if (current2.HasValue && valueOrDefault > num)
				{
					num = valueOrDefault;
					result = current2;
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Returns the maximum value in a sequence of <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>The maximum value in the sequence.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Decimal" /> values to determine the maximum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static decimal Max(this IEnumerable<decimal> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using IEnumerator<decimal> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		decimal num = enumerator.Current;
		while (enumerator.MoveNext())
		{
			decimal current = enumerator.Current;
			if (current > num)
			{
				num = current;
			}
		}
		return num;
	}

	/// <summary>Returns the maximum value in a sequence of nullable <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>A value of type Nullable&lt;Decimal&gt; in C# or Nullable(Of Decimal) in Visual Basic that corresponds to the maximum value in the sequence. </returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Decimal" /> values to determine the maximum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static decimal? Max(this IEnumerable<decimal?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		decimal? result = null;
		using IEnumerator<decimal?> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = enumerator.Current;
			if (!result.HasValue)
			{
				continue;
			}
			decimal num = result.GetValueOrDefault();
			while (enumerator.MoveNext())
			{
				decimal? current = enumerator.Current;
				decimal valueOrDefault = current.GetValueOrDefault();
				if (current.HasValue && valueOrDefault > num)
				{
					num = valueOrDefault;
					result = current;
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Returns the maximum value in a generic sequence.</summary>
	/// <returns>The maximum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the maximum value of.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static TSource Max<TSource>(this IEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		Comparer<TSource> @default = Comparer<TSource>.Default;
		TSource val = default(TSource);
		if (val == null)
		{
			using IEnumerator<TSource> enumerator = source.GetEnumerator();
			do
			{
				if (!enumerator.MoveNext())
				{
					return val;
				}
				val = enumerator.Current;
			}
			while (val == null);
			while (enumerator.MoveNext())
			{
				TSource current = enumerator.Current;
				if (current != null && @default.Compare(current, val) > 0)
				{
					val = current;
				}
			}
		}
		else
		{
			using IEnumerator<TSource> enumerator2 = source.GetEnumerator();
			if (!enumerator2.MoveNext())
			{
				throw Error.NoElements();
			}
			val = enumerator2.Current;
			while (enumerator2.MoveNext())
			{
				TSource current2 = enumerator2.Current;
				if (@default.Compare(current2, val) > 0)
				{
					val = current2;
				}
			}
		}
		return val;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the maximum <see cref="T:System.Int32" /> value.</summary>
	/// <returns>The maximum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the maximum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		int num = selector(enumerator.Current);
		while (enumerator.MoveNext())
		{
			int num2 = selector(enumerator.Current);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the maximum nullable <see cref="T:System.Int32" /> value.</summary>
	/// <returns>The value of type Nullable&lt;Int32&gt; in C# or Nullable(Of Int32) in Visual Basic that corresponds to the maximum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the maximum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static int? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		int? result = null;
		using (IEnumerator<TSource> enumerator = source.GetEnumerator())
		{
			do
			{
				if (!enumerator.MoveNext())
				{
					return result;
				}
				result = selector(enumerator.Current);
			}
			while (!result.HasValue);
			int num = result.GetValueOrDefault();
			if (num >= 0)
			{
				while (enumerator.MoveNext())
				{
					int? num2 = selector(enumerator.Current);
					int valueOrDefault = num2.GetValueOrDefault();
					if (valueOrDefault > num)
					{
						num = valueOrDefault;
						result = num2;
					}
				}
			}
			else
			{
				while (enumerator.MoveNext())
				{
					int? num3 = selector(enumerator.Current);
					int valueOrDefault2 = num3.GetValueOrDefault();
					if (num3.HasValue && valueOrDefault2 > num)
					{
						num = valueOrDefault2;
						result = num3;
					}
				}
			}
		}
		return result;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the maximum <see cref="T:System.Int64" /> value.</summary>
	/// <returns>The maximum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the maximum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static long Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		long num = selector(enumerator.Current);
		while (enumerator.MoveNext())
		{
			long num2 = selector(enumerator.Current);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the maximum nullable <see cref="T:System.Int64" /> value.</summary>
	/// <returns>The value of type Nullable&lt;Int64&gt; in C# or Nullable(Of Int64) in Visual Basic that corresponds to the maximum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the maximum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static long? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		long? result = null;
		using (IEnumerator<TSource> enumerator = source.GetEnumerator())
		{
			do
			{
				if (!enumerator.MoveNext())
				{
					return result;
				}
				result = selector(enumerator.Current);
			}
			while (!result.HasValue);
			long num = result.GetValueOrDefault();
			if (num >= 0)
			{
				while (enumerator.MoveNext())
				{
					long? num2 = selector(enumerator.Current);
					long valueOrDefault = num2.GetValueOrDefault();
					if (valueOrDefault > num)
					{
						num = valueOrDefault;
						result = num2;
					}
				}
			}
			else
			{
				while (enumerator.MoveNext())
				{
					long? num3 = selector(enumerator.Current);
					long valueOrDefault2 = num3.GetValueOrDefault();
					if (num3.HasValue && valueOrDefault2 > num)
					{
						num = valueOrDefault2;
						result = num3;
					}
				}
			}
		}
		return result;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the maximum <see cref="T:System.Single" /> value.</summary>
	/// <returns>The maximum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the maximum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static float Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		float num = selector(enumerator.Current);
		while (float.IsNaN(num))
		{
			if (!enumerator.MoveNext())
			{
				return num;
			}
			num = selector(enumerator.Current);
		}
		while (enumerator.MoveNext())
		{
			float num2 = selector(enumerator.Current);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the maximum nullable <see cref="T:System.Single" /> value.</summary>
	/// <returns>The value of type Nullable&lt;Single&gt; in C# or Nullable(Of Single) in Visual Basic that corresponds to the maximum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the maximum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static float? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		float? result = null;
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = selector(enumerator.Current);
			if (!result.HasValue)
			{
				continue;
			}
			float num = result.GetValueOrDefault();
			while (float.IsNaN(num))
			{
				if (!enumerator.MoveNext())
				{
					return result;
				}
				float? num2 = selector(enumerator.Current);
				if (num2.HasValue)
				{
					float? num3 = (result = num2);
					num = num3.GetValueOrDefault();
				}
			}
			while (enumerator.MoveNext())
			{
				float? num4 = selector(enumerator.Current);
				float valueOrDefault = num4.GetValueOrDefault();
				if (num4.HasValue && valueOrDefault > num)
				{
					num = valueOrDefault;
					result = num4;
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the maximum <see cref="T:System.Double" /> value.</summary>
	/// <returns>The maximum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the maximum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static double Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		double num = selector(enumerator.Current);
		while (double.IsNaN(num))
		{
			if (!enumerator.MoveNext())
			{
				return num;
			}
			num = selector(enumerator.Current);
		}
		while (enumerator.MoveNext())
		{
			double num2 = selector(enumerator.Current);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the maximum nullable <see cref="T:System.Double" /> value.</summary>
	/// <returns>The value of type Nullable&lt;Double&gt; in C# or Nullable(Of Double) in Visual Basic that corresponds to the maximum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the maximum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static double? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		double? result = null;
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = selector(enumerator.Current);
			if (!result.HasValue)
			{
				continue;
			}
			double num = result.GetValueOrDefault();
			while (double.IsNaN(num))
			{
				if (!enumerator.MoveNext())
				{
					return result;
				}
				double? num2 = selector(enumerator.Current);
				if (num2.HasValue)
				{
					double? num3 = (result = num2);
					num = num3.GetValueOrDefault();
				}
			}
			while (enumerator.MoveNext())
			{
				double? num4 = selector(enumerator.Current);
				double valueOrDefault = num4.GetValueOrDefault();
				if (num4.HasValue && valueOrDefault > num)
				{
					num = valueOrDefault;
					result = num4;
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the maximum <see cref="T:System.Decimal" /> value.</summary>
	/// <returns>The maximum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the maximum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static decimal Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		decimal num = selector(enumerator.Current);
		while (enumerator.MoveNext())
		{
			decimal num2 = selector(enumerator.Current);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the maximum nullable <see cref="T:System.Decimal" /> value.</summary>
	/// <returns>The value of type Nullable&lt;Decimal&gt; in C# or Nullable(Of Decimal) in Visual Basic that corresponds to the maximum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the maximum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static decimal? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		decimal? result = null;
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = selector(enumerator.Current);
			if (!result.HasValue)
			{
				continue;
			}
			decimal num = result.GetValueOrDefault();
			while (enumerator.MoveNext())
			{
				decimal? num2 = selector(enumerator.Current);
				decimal valueOrDefault = num2.GetValueOrDefault();
				if (num2.HasValue && valueOrDefault > num)
				{
					num = valueOrDefault;
					result = num2;
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Invokes a transform function on each element of a generic sequence and returns the maximum resulting value.</summary>
	/// <returns>The maximum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the maximum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static TResult Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		Comparer<TResult> @default = Comparer<TResult>.Default;
		TResult val = default(TResult);
		if (val == null)
		{
			using IEnumerator<TSource> enumerator = source.GetEnumerator();
			do
			{
				if (!enumerator.MoveNext())
				{
					return val;
				}
				val = selector(enumerator.Current);
			}
			while (val == null);
			while (enumerator.MoveNext())
			{
				TResult val2 = selector(enumerator.Current);
				if (val2 != null && @default.Compare(val2, val) > 0)
				{
					val = val2;
				}
			}
		}
		else
		{
			using IEnumerator<TSource> enumerator2 = source.GetEnumerator();
			if (!enumerator2.MoveNext())
			{
				throw Error.NoElements();
			}
			val = selector(enumerator2.Current);
			while (enumerator2.MoveNext())
			{
				TResult val3 = selector(enumerator2.Current);
				if (@default.Compare(val3, val) > 0)
				{
					val = val3;
				}
			}
		}
		return val;
	}

	/// <summary>Returns the minimum value in a sequence of <see cref="T:System.Int32" /> values.</summary>
	/// <returns>The minimum value in the sequence.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Int32" /> values to determine the minimum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static int Min(this IEnumerable<int> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using IEnumerator<int> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		int num = enumerator.Current;
		while (enumerator.MoveNext())
		{
			int current = enumerator.Current;
			if (current < num)
			{
				num = current;
			}
		}
		return num;
	}

	/// <summary>Returns the minimum value in a sequence of nullable <see cref="T:System.Int32" /> values.</summary>
	/// <returns>A value of type Nullable&lt;Int32&gt; in C# or Nullable(Of Int32) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Int32" /> values to determine the minimum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static int? Min(this IEnumerable<int?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		int? result = null;
		using IEnumerator<int?> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = enumerator.Current;
			if (!result.HasValue)
			{
				continue;
			}
			int num = result.GetValueOrDefault();
			while (enumerator.MoveNext())
			{
				int? current = enumerator.Current;
				int valueOrDefault = current.GetValueOrDefault();
				if (current.HasValue && valueOrDefault < num)
				{
					num = valueOrDefault;
					result = current;
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Returns the minimum value in a sequence of <see cref="T:System.Int64" /> values.</summary>
	/// <returns>The minimum value in the sequence.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Int64" /> values to determine the minimum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static long Min(this IEnumerable<long> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using IEnumerator<long> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		long num = enumerator.Current;
		while (enumerator.MoveNext())
		{
			long current = enumerator.Current;
			if (current < num)
			{
				num = current;
			}
		}
		return num;
	}

	/// <summary>Returns the minimum value in a sequence of nullable <see cref="T:System.Int64" /> values.</summary>
	/// <returns>A value of type Nullable&lt;Int64&gt; in C# or Nullable(Of Int64) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Int64" /> values to determine the minimum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static long? Min(this IEnumerable<long?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		long? result = null;
		using IEnumerator<long?> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = enumerator.Current;
			if (!result.HasValue)
			{
				continue;
			}
			long num = result.GetValueOrDefault();
			while (enumerator.MoveNext())
			{
				long? current = enumerator.Current;
				long valueOrDefault = current.GetValueOrDefault();
				if (current.HasValue && valueOrDefault < num)
				{
					num = valueOrDefault;
					result = current;
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Returns the minimum value in a sequence of <see cref="T:System.Single" /> values.</summary>
	/// <returns>The minimum value in the sequence.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Single" /> values to determine the minimum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static float Min(this IEnumerable<float> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using IEnumerator<float> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		float num = enumerator.Current;
		while (enumerator.MoveNext())
		{
			float current = enumerator.Current;
			if (current < num)
			{
				num = current;
			}
			else if (float.IsNaN(current))
			{
				return current;
			}
		}
		return num;
	}

	/// <summary>Returns the minimum value in a sequence of nullable <see cref="T:System.Single" /> values.</summary>
	/// <returns>A value of type Nullable&lt;Single&gt; in C# or Nullable(Of Single) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Single" /> values to determine the minimum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static float? Min(this IEnumerable<float?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		float? result = null;
		using IEnumerator<float?> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = enumerator.Current;
			if (!result.HasValue)
			{
				continue;
			}
			float num = result.GetValueOrDefault();
			while (enumerator.MoveNext())
			{
				float? current = enumerator.Current;
				if (current.HasValue)
				{
					float valueOrDefault = current.GetValueOrDefault();
					if (valueOrDefault < num)
					{
						num = valueOrDefault;
						result = current;
					}
					else if (float.IsNaN(valueOrDefault))
					{
						return current;
					}
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Returns the minimum value in a sequence of <see cref="T:System.Double" /> values.</summary>
	/// <returns>The minimum value in the sequence.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Double" /> values to determine the minimum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static double Min(this IEnumerable<double> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using IEnumerator<double> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		double num = enumerator.Current;
		while (enumerator.MoveNext())
		{
			double current = enumerator.Current;
			if (current < num)
			{
				num = current;
			}
			else if (double.IsNaN(current))
			{
				return current;
			}
		}
		return num;
	}

	/// <summary>Returns the minimum value in a sequence of nullable <see cref="T:System.Double" /> values.</summary>
	/// <returns>A value of type Nullable&lt;Double&gt; in C# or Nullable(Of Double) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Double" /> values to determine the minimum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static double? Min(this IEnumerable<double?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		double? result = null;
		using IEnumerator<double?> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = enumerator.Current;
			if (!result.HasValue)
			{
				continue;
			}
			double num = result.GetValueOrDefault();
			while (enumerator.MoveNext())
			{
				double? current = enumerator.Current;
				if (current.HasValue)
				{
					double valueOrDefault = current.GetValueOrDefault();
					if (valueOrDefault < num)
					{
						num = valueOrDefault;
						result = current;
					}
					else if (double.IsNaN(valueOrDefault))
					{
						return current;
					}
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Returns the minimum value in a sequence of <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>The minimum value in the sequence.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Decimal" /> values to determine the minimum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static decimal Min(this IEnumerable<decimal> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using IEnumerator<decimal> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		decimal num = enumerator.Current;
		while (enumerator.MoveNext())
		{
			decimal current = enumerator.Current;
			if (current < num)
			{
				num = current;
			}
		}
		return num;
	}

	/// <summary>Returns the minimum value in a sequence of nullable <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>A value of type Nullable&lt;Decimal&gt; in C# or Nullable(Of Decimal) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Decimal" /> values to determine the minimum value of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static decimal? Min(this IEnumerable<decimal?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		decimal? result = null;
		using IEnumerator<decimal?> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = enumerator.Current;
			if (!result.HasValue)
			{
				continue;
			}
			decimal num = result.GetValueOrDefault();
			while (enumerator.MoveNext())
			{
				decimal? current = enumerator.Current;
				decimal valueOrDefault = current.GetValueOrDefault();
				if (current.HasValue && valueOrDefault < num)
				{
					num = valueOrDefault;
					result = current;
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Returns the minimum value in a generic sequence.</summary>
	/// <returns>The minimum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the minimum value of.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static TSource Min<TSource>(this IEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		Comparer<TSource> @default = Comparer<TSource>.Default;
		TSource val = default(TSource);
		if (val == null)
		{
			using IEnumerator<TSource> enumerator = source.GetEnumerator();
			do
			{
				if (!enumerator.MoveNext())
				{
					return val;
				}
				val = enumerator.Current;
			}
			while (val == null);
			while (enumerator.MoveNext())
			{
				TSource current = enumerator.Current;
				if (current != null && @default.Compare(current, val) < 0)
				{
					val = current;
				}
			}
		}
		else
		{
			using IEnumerator<TSource> enumerator2 = source.GetEnumerator();
			if (!enumerator2.MoveNext())
			{
				throw Error.NoElements();
			}
			val = enumerator2.Current;
			while (enumerator2.MoveNext())
			{
				TSource current2 = enumerator2.Current;
				if (@default.Compare(current2, val) < 0)
				{
					val = current2;
				}
			}
		}
		return val;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the minimum <see cref="T:System.Int32" /> value.</summary>
	/// <returns>The minimum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the minimum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static int Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		int num = selector(enumerator.Current);
		while (enumerator.MoveNext())
		{
			int num2 = selector(enumerator.Current);
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the minimum nullable <see cref="T:System.Int32" /> value.</summary>
	/// <returns>The value of type Nullable&lt;Int32&gt; in C# or Nullable(Of Int32) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the minimum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static int? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		int? result = null;
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = selector(enumerator.Current);
			if (!result.HasValue)
			{
				continue;
			}
			int num = result.GetValueOrDefault();
			while (enumerator.MoveNext())
			{
				int? num2 = selector(enumerator.Current);
				int valueOrDefault = num2.GetValueOrDefault();
				if (num2.HasValue && valueOrDefault < num)
				{
					num = valueOrDefault;
					result = num2;
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the minimum <see cref="T:System.Int64" /> value.</summary>
	/// <returns>The minimum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the minimum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static long Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		long num = selector(enumerator.Current);
		while (enumerator.MoveNext())
		{
			long num2 = selector(enumerator.Current);
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the minimum nullable <see cref="T:System.Int64" /> value.</summary>
	/// <returns>The value of type Nullable&lt;Int64&gt; in C# or Nullable(Of Int64) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the minimum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static long? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		long? result = null;
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = selector(enumerator.Current);
			if (!result.HasValue)
			{
				continue;
			}
			long num = result.GetValueOrDefault();
			while (enumerator.MoveNext())
			{
				long? num2 = selector(enumerator.Current);
				long valueOrDefault = num2.GetValueOrDefault();
				if (num2.HasValue && valueOrDefault < num)
				{
					num = valueOrDefault;
					result = num2;
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the minimum <see cref="T:System.Single" /> value.</summary>
	/// <returns>The minimum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the minimum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static float Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		float num = selector(enumerator.Current);
		while (enumerator.MoveNext())
		{
			float num2 = selector(enumerator.Current);
			if (num2 < num)
			{
				num = num2;
			}
			else if (float.IsNaN(num2))
			{
				return num2;
			}
		}
		return num;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the minimum nullable <see cref="T:System.Single" /> value.</summary>
	/// <returns>The value of type Nullable&lt;Single&gt; in C# or Nullable(Of Single) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the minimum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static float? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		float? result = null;
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = selector(enumerator.Current);
			if (!result.HasValue)
			{
				continue;
			}
			float num = result.GetValueOrDefault();
			while (enumerator.MoveNext())
			{
				float? num2 = selector(enumerator.Current);
				if (num2.HasValue)
				{
					float valueOrDefault = num2.GetValueOrDefault();
					if (valueOrDefault < num)
					{
						num = valueOrDefault;
						result = num2;
					}
					else if (float.IsNaN(valueOrDefault))
					{
						return num2;
					}
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the minimum <see cref="T:System.Double" /> value.</summary>
	/// <returns>The minimum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the minimum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static double Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		double num = selector(enumerator.Current);
		while (enumerator.MoveNext())
		{
			double num2 = selector(enumerator.Current);
			if (num2 < num)
			{
				num = num2;
			}
			else if (double.IsNaN(num2))
			{
				return num2;
			}
		}
		return num;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the minimum nullable <see cref="T:System.Double" /> value.</summary>
	/// <returns>The value of type Nullable&lt;Double&gt; in C# or Nullable(Of Double) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the minimum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static double? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		double? result = null;
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = selector(enumerator.Current);
			if (!result.HasValue)
			{
				continue;
			}
			double num = result.GetValueOrDefault();
			while (enumerator.MoveNext())
			{
				double? num2 = selector(enumerator.Current);
				if (num2.HasValue)
				{
					double valueOrDefault = num2.GetValueOrDefault();
					if (valueOrDefault < num)
					{
						num = valueOrDefault;
						result = num2;
					}
					else if (double.IsNaN(valueOrDefault))
					{
						return num2;
					}
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the minimum <see cref="T:System.Decimal" /> value.</summary>
	/// <returns>The minimum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the minimum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="source" /> contains no elements.</exception>
	public static decimal Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw Error.NoElements();
		}
		decimal num = selector(enumerator.Current);
		while (enumerator.MoveNext())
		{
			decimal num2 = selector(enumerator.Current);
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	/// <summary>Invokes a transform function on each element of a sequence and returns the minimum nullable <see cref="T:System.Decimal" /> value.</summary>
	/// <returns>The value of type Nullable&lt;Decimal&gt; in C# or Nullable(Of Decimal) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the minimum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static decimal? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		decimal? result = null;
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			result = selector(enumerator.Current);
			if (!result.HasValue)
			{
				continue;
			}
			decimal num = result.GetValueOrDefault();
			while (enumerator.MoveNext())
			{
				decimal? num2 = selector(enumerator.Current);
				decimal valueOrDefault = num2.GetValueOrDefault();
				if (num2.HasValue && valueOrDefault < num)
				{
					num = valueOrDefault;
					result = num2;
				}
			}
			return result;
		}
		return result;
	}

	/// <summary>Invokes a transform function on each element of a generic sequence and returns the minimum resulting value.</summary>
	/// <returns>The minimum value in the sequence.</returns>
	/// <param name="source">A sequence of values to determine the minimum value of.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static TResult Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		Comparer<TResult> @default = Comparer<TResult>.Default;
		TResult val = default(TResult);
		if (val == null)
		{
			using IEnumerator<TSource> enumerator = source.GetEnumerator();
			do
			{
				if (!enumerator.MoveNext())
				{
					return val;
				}
				val = selector(enumerator.Current);
			}
			while (val == null);
			while (enumerator.MoveNext())
			{
				TResult val2 = selector(enumerator.Current);
				if (val2 != null && @default.Compare(val2, val) < 0)
				{
					val = val2;
				}
			}
		}
		else
		{
			using IEnumerator<TSource> enumerator2 = source.GetEnumerator();
			if (!enumerator2.MoveNext())
			{
				throw Error.NoElements();
			}
			val = selector(enumerator2.Current);
			while (enumerator2.MoveNext())
			{
				TResult val3 = selector(enumerator2.Current);
				if (@default.Compare(val3, val) < 0)
				{
					val = val3;
				}
			}
		}
		return val;
	}

	/// <summary>Sorts the elements of a sequence in ascending order according to a key.</summary>
	/// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted according to a key.</returns>
	/// <param name="source">A sequence of values to order.</param>
	/// <param name="keySelector">A function to extract a key from an element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
	public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return new OrderedEnumerable<TSource, TKey>(source, keySelector, null, descending: false, null);
	}

	/// <summary>Sorts the elements of a sequence in ascending order by using a specified comparer.</summary>
	/// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted according to a key.</returns>
	/// <param name="source">A sequence of values to order.</param>
	/// <param name="keySelector">A function to extract a key from an element.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
	public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		return new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, descending: false, null);
	}

	/// <summary>Sorts the elements of a sequence in descending order according to a key.</summary>
	/// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted in descending order according to a key.</returns>
	/// <param name="source">A sequence of values to order.</param>
	/// <param name="keySelector">A function to extract a key from an element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
	public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return new OrderedEnumerable<TSource, TKey>(source, keySelector, null, descending: true, null);
	}

	/// <summary>Sorts the elements of a sequence in descending order by using a specified comparer.</summary>
	/// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted in descending order according to a key.</returns>
	/// <param name="source">A sequence of values to order.</param>
	/// <param name="keySelector">A function to extract a key from an element.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
	public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		return new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, descending: true, null);
	}

	/// <summary>Performs a subsequent ordering of the elements in a sequence in ascending order according to a key.</summary>
	/// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted according to a key.</returns>
	/// <param name="source">An <see cref="T:System.Linq.IOrderedEnumerable`1" /> that contains elements to sort.</param>
	/// <param name="keySelector">A function to extract a key from each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
	public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, null, descending: false);
	}

	/// <summary>Performs a subsequent ordering of the elements in a sequence in ascending order by using a specified comparer.</summary>
	/// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted according to a key.</returns>
	/// <param name="source">An <see cref="T:System.Linq.IOrderedEnumerable`1" /> that contains elements to sort.</param>
	/// <param name="keySelector">A function to extract a key from each element.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
	public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, comparer, descending: false);
	}

	/// <summary>Performs a subsequent ordering of the elements in a sequence in descending order, according to a key.</summary>
	/// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted in descending order according to a key.</returns>
	/// <param name="source">An <see cref="T:System.Linq.IOrderedEnumerable`1" /> that contains elements to sort.</param>
	/// <param name="keySelector">A function to extract a key from each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
	public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, null, descending: true);
	}

	/// <summary>Performs a subsequent ordering of the elements in a sequence in descending order by using a specified comparer.</summary>
	/// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted in descending order according to a key.</returns>
	/// <param name="source">An <see cref="T:System.Linq.IOrderedEnumerable`1" /> that contains elements to sort.</param>
	/// <param name="keySelector">A function to extract a key from each element.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
	public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, comparer, descending: true);
	}

	/// <summary>Generates a sequence of integral numbers within a specified range.</summary>
	/// <returns>An IEnumerable&lt;Int32&gt; in C# or IEnumerable(Of Int32) in Visual Basic that contains a range of sequential integral numbers.</returns>
	/// <param name="start">The value of the first integer in the sequence.</param>
	/// <param name="count">The number of sequential integers to generate.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than 0.-or-<paramref name="start" /> + <paramref name="count" /> -1 is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
	public static IEnumerable<int> Range(int start, int count)
	{
		long num = (long)start + (long)count - 1;
		if (count < 0 || num > int.MaxValue)
		{
			throw Error.ArgumentOutOfRange("count");
		}
		if (count == 0)
		{
			return EmptyPartition<int>.Instance;
		}
		return new RangeIterator(start, count);
	}

	/// <summary>Generates a sequence that contains one repeated value.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains a repeated value.</returns>
	/// <param name="element">The value to be repeated.</param>
	/// <param name="count">The number of times to repeat the value in the generated sequence.</param>
	/// <typeparam name="TResult">The type of the value to be repeated in the result sequence.</typeparam>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than 0.</exception>
	public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count)
	{
		if (count < 0)
		{
			throw Error.ArgumentOutOfRange("count");
		}
		if (count == 0)
		{
			return EmptyPartition<TResult>.Instance;
		}
		return new RepeatIterator<TResult>(element, count);
	}

	/// <summary>Inverts the order of the elements in a sequence.</summary>
	/// <returns>A sequence whose elements correspond to those of the input sequence in reverse order.</returns>
	/// <param name="source">A sequence of values to reverse.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return new ReverseIterator<TSource>(source);
	}

	/// <summary>Projects each element of a sequence into a new form.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements are the result of invoking the transform function on each element of <paramref name="source" />.</returns>
	/// <param name="source">A sequence of values to invoke a transform function on.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		if (source is Iterator<TSource> iterator)
		{
			return iterator.Select(selector);
		}
		if (source is IList<TSource> source2)
		{
			if (source is TSource[] array)
			{
				if (array.Length != 0)
				{
					return new SelectArrayIterator<TSource, TResult>(array, selector);
				}
				return EmptyPartition<TResult>.Instance;
			}
			if (source is List<TSource> source3)
			{
				return new SelectListIterator<TSource, TResult>(source3, selector);
			}
			return new SelectIListIterator<TSource, TResult>(source2, selector);
		}
		if (source is IPartition<TSource> partition)
		{
			if (!(partition is EmptyPartition<TSource>))
			{
				return new SelectIPartitionIterator<TSource, TResult>(partition, selector);
			}
			return EmptyPartition<TResult>.Instance;
		}
		return new SelectEnumerableIterator<TSource, TResult>(source, selector);
	}

	/// <summary>Projects each element of a sequence into a new form by incorporating the element's index.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements are the result of invoking the transform function on each element of <paramref name="source" />.</returns>
	/// <param name="source">A sequence of values to invoke a transform function on.</param>
	/// <param name="selector">A transform function to apply to each source element; the second parameter of the function represents the index of the source element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		return SelectIterator(source, selector);
	}

	private static IEnumerable<TResult> SelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
	{
		int index = -1;
		foreach (TSource item in source)
		{
			index = checked(index + 1);
			yield return selector(item, index);
		}
	}

	/// <summary>Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IEnumerable`1" /> and flattens the resulting sequences into one sequence.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
	/// <param name="source">A sequence of values to project.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TResult">The type of the elements of the sequence returned by <paramref name="selector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		return new SelectManySingleSelectorIterator<TSource, TResult>(source, selector);
	}

	/// <summary>Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IEnumerable`1" />, and flattens the resulting sequences into one sequence. The index of each source element is used in the projected form of that element.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements are the result of invoking the one-to-many transform function on each element of an input sequence.</returns>
	/// <param name="source">A sequence of values to project.</param>
	/// <param name="selector">A transform function to apply to each source element; the second parameter of the function represents the index of the source element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TResult">The type of the elements of the sequence returned by <paramref name="selector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		return SelectManyIterator(source, selector);
	}

	private static IEnumerable<TResult> SelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
	{
		int index = -1;
		foreach (TSource item in source)
		{
			index = checked(index + 1);
			foreach (TResult item2 in selector(item, index))
			{
				yield return item2;
			}
		}
	}

	/// <summary>Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IEnumerable`1" />, flattens the resulting sequences into one sequence, and invokes a result selector function on each element therein. The index of each source element is used in the intermediate projected form of that element.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements are the result of invoking the one-to-many transform function <paramref name="collectionSelector" /> on each element of <paramref name="source" /> and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
	/// <param name="source">A sequence of values to project.</param>
	/// <param name="collectionSelector">A transform function to apply to each source element; the second parameter of the function represents the index of the source element.</param>
	/// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TCollection">The type of the intermediate elements collected by <paramref name="collectionSelector" />.</typeparam>
	/// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
	public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (collectionSelector == null)
		{
			throw Error.ArgumentNull("collectionSelector");
		}
		if (resultSelector == null)
		{
			throw Error.ArgumentNull("resultSelector");
		}
		return SelectManyIterator(source, collectionSelector, resultSelector);
	}

	private static IEnumerable<TResult> SelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		int index = -1;
		foreach (TSource element in source)
		{
			index = checked(index + 1);
			foreach (TCollection item in collectionSelector(element, index))
			{
				yield return resultSelector(element, item);
			}
		}
	}

	/// <summary>Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IEnumerable`1" />, flattens the resulting sequences into one sequence, and invokes a result selector function on each element therein.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements are the result of invoking the one-to-many transform function <paramref name="collectionSelector" /> on each element of <paramref name="source" /> and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
	/// <param name="source">A sequence of values to project.</param>
	/// <param name="collectionSelector">A transform function to apply to each element of the input sequence.</param>
	/// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TCollection">The type of the intermediate elements collected by <paramref name="collectionSelector" />.</typeparam>
	/// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
	public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (collectionSelector == null)
		{
			throw Error.ArgumentNull("collectionSelector");
		}
		if (resultSelector == null)
		{
			throw Error.ArgumentNull("resultSelector");
		}
		return SelectManyIterator(source, collectionSelector, resultSelector);
	}

	private static IEnumerable<TResult> SelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		foreach (TSource element in source)
		{
			foreach (TCollection item in collectionSelector(element))
			{
				yield return resultSelector(element, item);
			}
		}
	}

	/// <summary>Determines whether two sequences are equal by comparing the elements by using the default equality comparer for their type.</summary>
	/// <returns>true if the two source sequences are of equal length and their corresponding elements are equal according to the default equality comparer for their type; otherwise, false.</returns>
	/// <param name="first">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to compare to <paramref name="second" />.</param>
	/// <param name="second">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to compare to the first sequence.</param>
	/// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="first" /> or <paramref name="second" /> is null.</exception>
	public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
	{
		return first.SequenceEqual(second, null);
	}

	/// <summary>Determines whether two sequences are equal by comparing their elements by using a specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
	/// <returns>true if the two source sequences are of equal length and their corresponding elements compare equal according to <paramref name="comparer" />; otherwise, false.</returns>
	/// <param name="first">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to compare to <paramref name="second" />.</param>
	/// <param name="second">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to compare to the first sequence.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to use to compare elements.</param>
	/// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="first" /> or <paramref name="second" /> is null.</exception>
	public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<TSource>.Default;
		}
		if (first == null)
		{
			throw Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw Error.ArgumentNull("second");
		}
		if (first is ICollection<TSource> collection && second is ICollection<TSource> collection2)
		{
			if (collection.Count != collection2.Count)
			{
				return false;
			}
			if (collection is IList<TSource> list && collection2 is IList<TSource> list2)
			{
				int count = collection.Count;
				for (int i = 0; i < count; i++)
				{
					if (!comparer.Equals(list[i], list2[i]))
					{
						return false;
					}
				}
				return true;
			}
		}
		using IEnumerator<TSource> enumerator = first.GetEnumerator();
		using IEnumerator<TSource> enumerator2 = second.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!enumerator2.MoveNext() || !comparer.Equals(enumerator.Current, enumerator2.Current))
			{
				return false;
			}
		}
		return !enumerator2.MoveNext();
	}

	/// <summary>Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.</summary>
	/// <returns>The single element of the input sequence.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return the single element of.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The input sequence contains more than one element.-or-The input sequence is empty.</exception>
	public static TSource Single<TSource>(this IEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (source is IList<TSource> { Count: var count } list)
		{
			switch (count)
			{
			case 0:
				throw Error.NoElements();
			case 1:
				return list[0];
			}
		}
		else
		{
			using IEnumerator<TSource> enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				throw Error.NoElements();
			}
			TSource current = enumerator.Current;
			if (!enumerator.MoveNext())
			{
				return current;
			}
		}
		throw Error.MoreThanOneElement();
	}

	/// <summary>Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.</summary>
	/// <returns>The single element of the input sequence that satisfies a condition.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return a single element from.</param>
	/// <param name="predicate">A function to test an element for a condition.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">No element satisfies the condition in <paramref name="predicate" />.-or-More than one element satisfies the condition in <paramref name="predicate" />.-or-The source sequence is empty.</exception>
	public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		using (IEnumerator<TSource> enumerator = source.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				TSource current = enumerator.Current;
				if (!predicate(current))
				{
					continue;
				}
				while (enumerator.MoveNext())
				{
					if (predicate(enumerator.Current))
					{
						throw Error.MoreThanOneMatch();
					}
				}
				return current;
			}
		}
		throw Error.NoMatch();
	}

	/// <summary>Returns the only element of a sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.</summary>
	/// <returns>The single element of the input sequence, or default(<paramref name="TSource" />) if the sequence contains no elements.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return the single element of.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The input sequence contains more than one element.</exception>
	public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (source is IList<TSource> { Count: var count } list)
		{
			switch (count)
			{
			case 0:
				return default(TSource);
			case 1:
				return list[0];
			}
		}
		else
		{
			using IEnumerator<TSource> enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				return default(TSource);
			}
			TSource current = enumerator.Current;
			if (!enumerator.MoveNext())
			{
				return current;
			}
		}
		throw Error.MoreThanOneElement();
	}

	/// <summary>Returns the only element of a sequence that satisfies a specified condition or a default value if no such element exists; this method throws an exception if more than one element satisfies the condition.</summary>
	/// <returns>The single element of the input sequence that satisfies the condition, or default(<paramref name="TSource" />) if no such element is found.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return a single element from.</param>
	/// <param name="predicate">A function to test an element for a condition.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		using (IEnumerator<TSource> enumerator = source.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				TSource current = enumerator.Current;
				if (!predicate(current))
				{
					continue;
				}
				while (enumerator.MoveNext())
				{
					if (predicate(enumerator.Current))
					{
						throw Error.MoreThanOneMatch();
					}
				}
				return current;
			}
		}
		return default(TSource);
	}

	/// <summary>Bypasses a specified number of elements in a sequence and then returns the remaining elements.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the elements that occur after the specified index in the input sequence.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return elements from.</param>
	/// <param name="count">The number of elements to skip before returning the remaining elements.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (count <= 0)
		{
			if (source is Iterator<TSource> || source is IPartition<TSource>)
			{
				return source;
			}
			count = 0;
		}
		else if (source is IPartition<TSource> partition)
		{
			return partition.Skip(count);
		}
		if (source is IList<TSource> source2)
		{
			return new ListPartition<TSource>(source2, count, int.MaxValue);
		}
		return new EnumerablePartition<TSource>(source, count, -1);
	}

	/// <summary>Bypasses elements in a sequence as long as a specified condition is true and then returns the remaining elements.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the elements from the input sequence starting at the first element in the linear series that does not pass the test specified by <paramref name="predicate" />.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return elements from.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		return SkipWhileIterator(source, predicate);
	}

	private static IEnumerable<TSource> SkipWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		using IEnumerator<TSource> e = source.GetEnumerator();
		while (e.MoveNext())
		{
			TSource current = e.Current;
			if (!predicate(current))
			{
				yield return current;
				while (e.MoveNext())
				{
					yield return e.Current;
				}
				yield break;
			}
		}
	}

	/// <summary>Bypasses elements in a sequence as long as a specified condition is true and then returns the remaining elements. The element's index is used in the logic of the predicate function.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the elements from the input sequence starting at the first element in the linear series that does not pass the test specified by <paramref name="predicate" />.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return elements from.</param>
	/// <param name="predicate">A function to test each source element for a condition; the second parameter of the function represents the index of the source element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		return SkipWhileIterator(source, predicate);
	}

	private static IEnumerable<TSource> SkipWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		using IEnumerator<TSource> e = source.GetEnumerator();
		int index = -1;
		while (e.MoveNext())
		{
			index = checked(index + 1);
			TSource current = e.Current;
			if (!predicate(current, index))
			{
				yield return current;
				while (e.MoveNext())
				{
					yield return e.Current;
				}
				yield break;
			}
		}
	}

	public static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, int count)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (count <= 0)
		{
			return source.Skip(0);
		}
		return SkipLastIterator(source, count);
	}

	private static IEnumerable<TSource> SkipLastIterator<TSource>(IEnumerable<TSource> source, int count)
	{
		Queue<TSource> queue = new Queue<TSource>();
		using IEnumerator<TSource> e = source.GetEnumerator();
		while (e.MoveNext())
		{
			if (queue.Count == count)
			{
				do
				{
					yield return queue.Dequeue();
					queue.Enqueue(e.Current);
				}
				while (e.MoveNext());
				break;
			}
			queue.Enqueue(e.Current);
		}
	}

	/// <summary>Computes the sum of a sequence of <see cref="T:System.Int32" /> values.</summary>
	/// <returns>The sum of the values in the sequence.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Int32" /> values to calculate the sum of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
	public static int Sum(this IEnumerable<int> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		int num = 0;
		foreach (int item in source)
		{
			num = checked(num + item);
		}
		return num;
	}

	/// <summary>Computes the sum of a sequence of nullable <see cref="T:System.Int32" /> values.</summary>
	/// <returns>The sum of the values in the sequence.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Int32" /> values to calculate the sum of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
	public static int? Sum(this IEnumerable<int?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		int num = 0;
		foreach (int? item in source)
		{
			if (item.HasValue)
			{
				num = checked(num + item.GetValueOrDefault());
			}
		}
		return num;
	}

	/// <summary>Computes the sum of a sequence of <see cref="T:System.Int64" /> values.</summary>
	/// <returns>The sum of the values in the sequence.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Int64" /> values to calculate the sum of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
	public static long Sum(this IEnumerable<long> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		long num = 0L;
		foreach (long item in source)
		{
			num = checked(num + item);
		}
		return num;
	}

	/// <summary>Computes the sum of a sequence of nullable <see cref="T:System.Int64" /> values.</summary>
	/// <returns>The sum of the values in the sequence.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Int64" /> values to calculate the sum of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
	public static long? Sum(this IEnumerable<long?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		long num = 0L;
		foreach (long? item in source)
		{
			if (item.HasValue)
			{
				num = checked(num + item.GetValueOrDefault());
			}
		}
		return num;
	}

	/// <summary>Computes the sum of a sequence of <see cref="T:System.Single" /> values.</summary>
	/// <returns>The sum of the values in the sequence.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Single" /> values to calculate the sum of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static float Sum(this IEnumerable<float> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		double num = 0.0;
		foreach (float item in source)
		{
			num += (double)item;
		}
		return (float)num;
	}

	/// <summary>Computes the sum of a sequence of nullable <see cref="T:System.Single" /> values.</summary>
	/// <returns>The sum of the values in the sequence.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Single" /> values to calculate the sum of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static float? Sum(this IEnumerable<float?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		double num = 0.0;
		foreach (float? item in source)
		{
			if (item.HasValue)
			{
				num += (double)item.GetValueOrDefault();
			}
		}
		return (float)num;
	}

	/// <summary>Computes the sum of a sequence of <see cref="T:System.Double" /> values.</summary>
	/// <returns>The sum of the values in the sequence.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Double" /> values to calculate the sum of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static double Sum(this IEnumerable<double> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		double num = 0.0;
		foreach (double item in source)
		{
			num += item;
		}
		return num;
	}

	/// <summary>Computes the sum of a sequence of nullable <see cref="T:System.Double" /> values.</summary>
	/// <returns>The sum of the values in the sequence.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Double" /> values to calculate the sum of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static double? Sum(this IEnumerable<double?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		double num = 0.0;
		foreach (double? item in source)
		{
			if (item.HasValue)
			{
				num += item.GetValueOrDefault();
			}
		}
		return num;
	}

	/// <summary>Computes the sum of a sequence of <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>The sum of the values in the sequence.</returns>
	/// <param name="source">A sequence of <see cref="T:System.Decimal" /> values to calculate the sum of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
	public static decimal Sum(this IEnumerable<decimal> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		decimal result = default(decimal);
		foreach (decimal item in source)
		{
			result += item;
		}
		return result;
	}

	/// <summary>Computes the sum of a sequence of nullable <see cref="T:System.Decimal" /> values.</summary>
	/// <returns>The sum of the values in the sequence.</returns>
	/// <param name="source">A sequence of nullable <see cref="T:System.Decimal" /> values to calculate the sum of.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
	public static decimal? Sum(this IEnumerable<decimal?> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		decimal value = default(decimal);
		foreach (decimal? item in source)
		{
			if (item.HasValue)
			{
				value += item.GetValueOrDefault();
			}
		}
		return value;
	}

	/// <summary>Computes the sum of the sequence of <see cref="T:System.Int32" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The sum of the projected values.</returns>
	/// <param name="source">A sequence of values that are used to calculate a sum.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
	public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		int num = 0;
		foreach (TSource item in source)
		{
			num = checked(num + selector(item));
		}
		return num;
	}

	/// <summary>Computes the sum of the sequence of nullable <see cref="T:System.Int32" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The sum of the projected values.</returns>
	/// <param name="source">A sequence of values that are used to calculate a sum.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
	public static int? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		int num = 0;
		foreach (TSource item in source)
		{
			int? num2 = selector(item);
			if (num2.HasValue)
			{
				num = checked(num + num2.GetValueOrDefault());
			}
		}
		return num;
	}

	/// <summary>Computes the sum of the sequence of <see cref="T:System.Int64" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The sum of the projected values.</returns>
	/// <param name="source">A sequence of values that are used to calculate a sum.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
	public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
	{
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		long num = 0L;
		foreach (TSource item in source)
		{
			num = checked(num + selector(item));
		}
		return num;
	}

	/// <summary>Computes the sum of the sequence of nullable <see cref="T:System.Int64" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The sum of the projected values.</returns>
	/// <param name="source">A sequence of values that are used to calculate a sum.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
	public static long? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		long num = 0L;
		foreach (TSource item in source)
		{
			long? num2 = selector(item);
			if (num2.HasValue)
			{
				num = checked(num + num2.GetValueOrDefault());
			}
		}
		return num;
	}

	/// <summary>Computes the sum of the sequence of <see cref="T:System.Single" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The sum of the projected values.</returns>
	/// <param name="source">A sequence of values that are used to calculate a sum.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static float Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		double num = 0.0;
		foreach (TSource item in source)
		{
			num += (double)selector(item);
		}
		return (float)num;
	}

	/// <summary>Computes the sum of the sequence of nullable <see cref="T:System.Single" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The sum of the projected values.</returns>
	/// <param name="source">A sequence of values that are used to calculate a sum.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static float? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		double num = 0.0;
		foreach (TSource item in source)
		{
			float? num2 = selector(item);
			if (num2.HasValue)
			{
				num += (double)num2.GetValueOrDefault();
			}
		}
		return (float)num;
	}

	/// <summary>Computes the sum of the sequence of <see cref="T:System.Double" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The sum of the projected values.</returns>
	/// <param name="source">A sequence of values that are used to calculate a sum.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static double Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		double num = 0.0;
		foreach (TSource item in source)
		{
			num += selector(item);
		}
		return num;
	}

	/// <summary>Computes the sum of the sequence of nullable <see cref="T:System.Double" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The sum of the projected values.</returns>
	/// <param name="source">A sequence of values that are used to calculate a sum.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	public static double? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		double num = 0.0;
		foreach (TSource item in source)
		{
			double? num2 = selector(item);
			if (num2.HasValue)
			{
				num += num2.GetValueOrDefault();
			}
		}
		return num;
	}

	/// <summary>Computes the sum of the sequence of <see cref="T:System.Decimal" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The sum of the projected values.</returns>
	/// <param name="source">A sequence of values that are used to calculate a sum.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
	public static decimal Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		decimal result = default(decimal);
		foreach (TSource item in source)
		{
			result += selector(item);
		}
		return result;
	}

	/// <summary>Computes the sum of the sequence of nullable <see cref="T:System.Decimal" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
	/// <returns>The sum of the projected values.</returns>
	/// <param name="source">A sequence of values that are used to calculate a sum.</param>
	/// <param name="selector">A transform function to apply to each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
	/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
	public static decimal? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		decimal value = default(decimal);
		foreach (TSource item in source)
		{
			decimal? num = selector(item);
			if (num.HasValue)
			{
				value += num.GetValueOrDefault();
			}
		}
		return value;
	}

	/// <summary>Returns a specified number of contiguous elements from the start of a sequence.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the specified number of elements from the start of the input sequence.</returns>
	/// <param name="source">The sequence to return elements from.</param>
	/// <param name="count">The number of elements to return.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (count <= 0)
		{
			return EmptyPartition<TSource>.Instance;
		}
		if (source is IPartition<TSource> partition)
		{
			return partition.Take(count);
		}
		if (source is IList<TSource> source2)
		{
			return new ListPartition<TSource>(source2, 0, count - 1);
		}
		return new EnumerablePartition<TSource>(source, 0, count - 1);
	}

	/// <summary>Returns elements from a sequence as long as a specified condition is true.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the elements from the input sequence that occur before the element at which the test no longer passes.</returns>
	/// <param name="source">A sequence to return elements from.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		return TakeWhileIterator(source, predicate);
	}

	private static IEnumerable<TSource> TakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		foreach (TSource item in source)
		{
			if (!predicate(item))
			{
				break;
			}
			yield return item;
		}
	}

	/// <summary>Returns elements from a sequence as long as a specified condition is true. The element's index is used in the logic of the predicate function.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains elements from the input sequence that occur before the element at which the test no longer passes.</returns>
	/// <param name="source">The sequence to return elements from.</param>
	/// <param name="predicate">A function to test each source element for a condition; the second parameter of the function represents the index of the source element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		return TakeWhileIterator(source, predicate);
	}

	private static IEnumerable<TSource> TakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		int index = -1;
		foreach (TSource item in source)
		{
			index = checked(index + 1);
			if (!predicate(item, index))
			{
				break;
			}
			yield return item;
		}
	}

	public static IEnumerable<TSource> TakeLast<TSource>(this IEnumerable<TSource> source, int count)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (count <= 0)
		{
			return EmptyPartition<TSource>.Instance;
		}
		return TakeLastIterator(source, count);
	}

	private static IEnumerable<TSource> TakeLastIterator<TSource>(IEnumerable<TSource> source, int count)
	{
		Queue<TSource> queue;
		using (IEnumerator<TSource> enumerator = source.GetEnumerator())
		{
			if (!enumerator.MoveNext())
			{
				yield break;
			}
			queue = new Queue<TSource>();
			queue.Enqueue(enumerator.Current);
			while (enumerator.MoveNext())
			{
				if (queue.Count < count)
				{
					queue.Enqueue(enumerator.Current);
					continue;
				}
				do
				{
					queue.Dequeue();
					queue.Enqueue(enumerator.Current);
				}
				while (enumerator.MoveNext());
				break;
			}
		}
		do
		{
			yield return queue.Dequeue();
		}
		while (queue.Count > 0);
	}

	/// <summary>Creates an array from a <see cref="T:System.Collections.Generic.IEnumerable`1" />.</summary>
	/// <returns>An array that contains the elements from the input sequence.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to create an array from.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (!(source is IIListProvider<TSource> iIListProvider))
		{
			return System.Collections.Generic.EnumerableHelpers.ToArray(source);
		}
		return iIListProvider.ToArray();
	}

	/// <summary>Creates a <see cref="T:System.Collections.Generic.List`1" /> from an <see cref="T:System.Collections.Generic.IEnumerable`1" />.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.List`1" /> that contains elements from the input sequence.</returns>
	/// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1" /> to create a <see cref="T:System.Collections.Generic.List`1" /> from.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (!(source is IIListProvider<TSource> iIListProvider))
		{
			return new List<TSource>(source);
		}
		return iIListProvider.ToList();
	}

	/// <summary>Creates a <see cref="T:System.Collections.Generic.Dictionary`2" /> from an <see cref="T:System.Collections.Generic.IEnumerable`1" /> according to a specified key selector function.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2" /> that contains keys and values.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to create a <see cref="T:System.Collections.Generic.Dictionary`2" /> from.</param>
	/// <param name="keySelector">A function to extract a key from each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> is null.-or-<paramref name="keySelector" /> produces a key that is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="keySelector" /> produces duplicate keys for two elements.</exception>
	public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return source.ToDictionary(keySelector, null);
	}

	/// <summary>Creates a <see cref="T:System.Collections.Generic.Dictionary`2" /> from an <see cref="T:System.Collections.Generic.IEnumerable`1" /> according to a specified key selector function and key comparer.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2" /> that contains keys and values.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to create a <see cref="T:System.Collections.Generic.Dictionary`2" /> from.</param>
	/// <param name="keySelector">A function to extract a key from each element.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the keys returned by <paramref name="keySelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> is null.-or-<paramref name="keySelector" /> produces a key that is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="keySelector" /> produces duplicate keys for two elements.</exception>
	public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		int num = 0;
		if (source is ICollection<TSource> collection)
		{
			num = collection.Count;
			if (num == 0)
			{
				return new Dictionary<TKey, TSource>(comparer);
			}
			if (collection is TSource[] source2)
			{
				return ToDictionary(source2, keySelector, comparer);
			}
			if (collection is List<TSource> source3)
			{
				return ToDictionary(source3, keySelector, comparer);
			}
		}
		Dictionary<TKey, TSource> dictionary = new Dictionary<TKey, TSource>(num, comparer);
		foreach (TSource item in source)
		{
			dictionary.Add(keySelector(item), item);
		}
		return dictionary;
	}

	private static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(TSource[] source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		Dictionary<TKey, TSource> dictionary = new Dictionary<TKey, TSource>(source.Length, comparer);
		for (int i = 0; i < source.Length; i++)
		{
			dictionary.Add(keySelector(source[i]), source[i]);
		}
		return dictionary;
	}

	private static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(List<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		Dictionary<TKey, TSource> dictionary = new Dictionary<TKey, TSource>(source.Count, comparer);
		foreach (TSource item in source)
		{
			dictionary.Add(keySelector(item), item);
		}
		return dictionary;
	}

	/// <summary>Creates a <see cref="T:System.Collections.Generic.Dictionary`2" /> from an <see cref="T:System.Collections.Generic.IEnumerable`1" /> according to specified key selector and element selector functions.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2" /> that contains values of type <paramref name="TElement" /> selected from the input sequence.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to create a <see cref="T:System.Collections.Generic.Dictionary`2" /> from.</param>
	/// <param name="keySelector">A function to extract a key from each element.</param>
	/// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> is null.-or-<paramref name="keySelector" /> produces a key that is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="keySelector" /> produces duplicate keys for two elements.</exception>
	public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
	{
		return source.ToDictionary(keySelector, elementSelector, null);
	}

	/// <summary>Creates a <see cref="T:System.Collections.Generic.Dictionary`2" /> from an <see cref="T:System.Collections.Generic.IEnumerable`1" /> according to a specified key selector function, a comparer, and an element selector function.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2" /> that contains values of type <paramref name="TElement" /> selected from the input sequence.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to create a <see cref="T:System.Collections.Generic.Dictionary`2" /> from.</param>
	/// <param name="keySelector">A function to extract a key from each element.</param>
	/// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
	/// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
	/// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> is null.-or-<paramref name="keySelector" /> produces a key that is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="keySelector" /> produces duplicate keys for two elements.</exception>
	public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		if (elementSelector == null)
		{
			throw Error.ArgumentNull("elementSelector");
		}
		int num = 0;
		if (source is ICollection<TSource> collection)
		{
			num = collection.Count;
			if (num == 0)
			{
				return new Dictionary<TKey, TElement>(comparer);
			}
			if (collection is TSource[] source2)
			{
				return ToDictionary(source2, keySelector, elementSelector, comparer);
			}
			if (collection is List<TSource> source3)
			{
				return ToDictionary(source3, keySelector, elementSelector, comparer);
			}
		}
		Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>(num, comparer);
		foreach (TSource item in source)
		{
			dictionary.Add(keySelector(item), elementSelector(item));
		}
		return dictionary;
	}

	private static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(TSource[] source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>(source.Length, comparer);
		for (int i = 0; i < source.Length; i++)
		{
			dictionary.Add(keySelector(source[i]), elementSelector(source[i]));
		}
		return dictionary;
	}

	private static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(List<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>(source.Count, comparer);
		foreach (TSource item in source)
		{
			dictionary.Add(keySelector(item), elementSelector(item));
		}
		return dictionary;
	}

	public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source)
	{
		return source.ToHashSet(null);
	}

	public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return new HashSet<TSource>(source, comparer);
	}

	/// <summary>Produces the set union of two sequences by using the default equality comparer.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the elements from both input sequences, excluding duplicates.</returns>
	/// <param name="first">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose distinct elements form the first set for the union.</param>
	/// <param name="second">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose distinct elements form the second set for the union.</param>
	/// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="first" /> or <paramref name="second" /> is null.</exception>
	public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
	{
		return first.Union(second, null);
	}

	/// <summary>Produces the set union of two sequences by using a specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the elements from both input sequences, excluding duplicates.</returns>
	/// <param name="first">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose distinct elements form the first set for the union.</param>
	/// <param name="second">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose distinct elements form the second set for the union.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</param>
	/// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="first" /> or <paramref name="second" /> is null.</exception>
	public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		if (first == null)
		{
			throw Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw Error.ArgumentNull("second");
		}
		if (!(first is UnionIterator<TSource> unionIterator) || !Utilities.AreEqualityComparersEqual(comparer, unionIterator._comparer))
		{
			return new UnionIterator2<TSource>(first, second, comparer);
		}
		return unionIterator.Union(second);
	}

	/// <summary>Filters a sequence of values based on a predicate.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains elements from the input sequence that satisfy the condition.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to filter.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		if (source is Iterator<TSource> iterator)
		{
			return iterator.Where(predicate);
		}
		if (source is TSource[] array)
		{
			if (array.Length != 0)
			{
				return new WhereArrayIterator<TSource>(array, predicate);
			}
			return EmptyPartition<TSource>.Instance;
		}
		if (source is List<TSource> source2)
		{
			return new WhereListIterator<TSource>(source2, predicate);
		}
		return new WhereEnumerableIterator<TSource>(source, predicate);
	}

	/// <summary>Filters a sequence of values based on a predicate. Each element's index is used in the logic of the predicate function.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains elements from the input sequence that satisfy the condition.</returns>
	/// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to filter.</param>
	/// <param name="predicate">A function to test each source element for a condition; the second parameter of the function represents the index of the source element.</param>
	/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
	public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		return WhereIterator(source, predicate);
	}

	private static IEnumerable<TSource> WhereIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		int index = -1;
		foreach (TSource item in source)
		{
			index = checked(index + 1);
			if (predicate(item, index))
			{
				yield return item;
			}
		}
	}

	/// <summary>Applies a specified function to the corresponding elements of two sequences, producing a sequence of the results.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains merged elements of two input sequences.</returns>
	/// <param name="first">The first sequence to merge.</param>
	/// <param name="second">The second sequence to merge.</param>
	/// <param name="resultSelector">A function that specifies how to merge the elements from the two sequences.</param>
	/// <typeparam name="TFirst">The type of the elements of the first input sequence.</typeparam>
	/// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
	/// <typeparam name="TResult">The type of the elements of the result sequence.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="first" /> or <paramref name="second" /> is null.</exception>
	public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
	{
		if (first == null)
		{
			throw Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw Error.ArgumentNull("second");
		}
		if (resultSelector == null)
		{
			throw Error.ArgumentNull("resultSelector");
		}
		return ZipIterator(first, second, resultSelector);
	}

	private static IEnumerable<TResult> ZipIterator<TFirst, TSecond, TResult>(IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
	{
		using IEnumerator<TFirst> e1 = first.GetEnumerator();
		using IEnumerator<TSecond> e2 = second.GetEnumerator();
		while (e1.MoveNext() && e2.MoveNext())
		{
			yield return resultSelector(e1.Current, e2.Current);
		}
	}
}
