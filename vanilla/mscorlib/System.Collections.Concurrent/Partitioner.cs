using System.Collections.Generic;
using System.Threading;

namespace System.Collections.Concurrent;

/// <summary>Represents a particular manner of splitting a data source into multiple partitions.</summary>
/// <typeparam name="TSource">Type of the elements in the collection.</typeparam>
public abstract class Partitioner<TSource>
{
	/// <summary>Gets whether additional partitions can be created dynamically.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Concurrent.Partitioner`1" /> can create partitions dynamically as they are requested; false if the <see cref="T:System.Collections.Concurrent.Partitioner`1" /> can only allocate partitions statically.</returns>
	public virtual bool SupportsDynamicPartitions => false;

	/// <summary>Partitions the underlying collection into the given number of partitions.</summary>
	/// <returns>A list containing <paramref name="partitionCount" /> enumerators.</returns>
	/// <param name="partitionCount">The number of partitions to create.</param>
	public abstract IList<IEnumerator<TSource>> GetPartitions(int partitionCount);

	/// <summary>Creates an object that can partition the underlying collection into a variable number of partitions.</summary>
	/// <returns>An object that can create partitions over the underlying data source.</returns>
	/// <exception cref="T:System.NotSupportedException">Dynamic partitioning is not supported by the base class. You must implement it in a derived class.</exception>
	public virtual IEnumerable<TSource> GetDynamicPartitions()
	{
		throw new NotSupportedException("Dynamic partitions are not supported by this partitioner.");
	}

	/// <summary>Creates a new partitioner instance.</summary>
	protected Partitioner()
	{
	}
}
/// <summary>Provides common partitioning strategies for arrays, lists, and enumerables.</summary>
public static class Partitioner
{
	private abstract class DynamicPartitionEnumerator_Abstract<TSource, TSourceReader> : IEnumerator<KeyValuePair<long, TSource>>, IDisposable, IEnumerator
	{
		protected readonly TSourceReader _sharedReader;

		protected static int s_defaultMaxChunkSize = GetDefaultChunkSize<TSource>();

		protected SharedInt _currentChunkSize;

		protected SharedInt _localOffset;

		private const int CHUNK_DOUBLING_RATE = 3;

		private int _doublingCountdown;

		protected readonly int _maxChunkSize;

		protected readonly SharedLong _sharedIndex;

		protected abstract bool HasNoElementsLeft { get; set; }

		public abstract KeyValuePair<long, TSource> Current { get; }

		object IEnumerator.Current => Current;

		protected DynamicPartitionEnumerator_Abstract(TSourceReader sharedReader, SharedLong sharedIndex)
			: this(sharedReader, sharedIndex, useSingleChunking: false)
		{
		}

		protected DynamicPartitionEnumerator_Abstract(TSourceReader sharedReader, SharedLong sharedIndex, bool useSingleChunking)
		{
			_sharedReader = sharedReader;
			_sharedIndex = sharedIndex;
			_maxChunkSize = (useSingleChunking ? 1 : s_defaultMaxChunkSize);
		}

		protected abstract bool GrabNextChunk(int requestedChunkSize);

		public abstract void Dispose();

		public void Reset()
		{
			throw new NotSupportedException();
		}

		public bool MoveNext()
		{
			if (_localOffset == null)
			{
				_localOffset = new SharedInt(-1);
				_currentChunkSize = new SharedInt(0);
				_doublingCountdown = 3;
			}
			if (_localOffset.Value < _currentChunkSize.Value - 1)
			{
				_localOffset.Value++;
				return true;
			}
			int requestedChunkSize;
			if (_currentChunkSize.Value == 0)
			{
				requestedChunkSize = 1;
			}
			else if (_doublingCountdown > 0)
			{
				requestedChunkSize = _currentChunkSize.Value;
			}
			else
			{
				requestedChunkSize = Math.Min(_currentChunkSize.Value * 2, _maxChunkSize);
				_doublingCountdown = 3;
			}
			_doublingCountdown--;
			if (GrabNextChunk(requestedChunkSize))
			{
				_localOffset.Value = 0;
				return true;
			}
			return false;
		}
	}

	private class DynamicPartitionerForIEnumerable<TSource> : OrderablePartitioner<TSource>
	{
		private class InternalPartitionEnumerable : IEnumerable<KeyValuePair<long, TSource>>, IEnumerable, IDisposable
		{
			private readonly IEnumerator<TSource> _sharedReader;

			private SharedLong _sharedIndex;

			private volatile KeyValuePair<long, TSource>[] _fillBuffer;

			private volatile int _fillBufferSize;

			private volatile int _fillBufferCurrentPosition;

			private volatile int _activeCopiers;

			private SharedBool _hasNoElementsLeft;

			private SharedBool _sourceDepleted;

			private object _sharedLock;

			private bool _disposed;

			private SharedInt _activePartitionCount;

			private readonly bool _useSingleChunking;

			internal InternalPartitionEnumerable(IEnumerator<TSource> sharedReader, bool useSingleChunking, bool isStaticPartitioning)
			{
				_sharedReader = sharedReader;
				_sharedIndex = new SharedLong(-1L);
				_hasNoElementsLeft = new SharedBool(value: false);
				_sourceDepleted = new SharedBool(value: false);
				_sharedLock = new object();
				_useSingleChunking = useSingleChunking;
				if (!_useSingleChunking)
				{
					_fillBuffer = new KeyValuePair<long, TSource>[((PlatformHelper.ProcessorCount <= 4) ? 1 : 4) * GetDefaultChunkSize<TSource>()];
				}
				if (isStaticPartitioning)
				{
					_activePartitionCount = new SharedInt(0);
				}
				else
				{
					_activePartitionCount = null;
				}
			}

			public IEnumerator<KeyValuePair<long, TSource>> GetEnumerator()
			{
				if (_disposed)
				{
					throw new ObjectDisposedException("Can not call GetEnumerator on partitions after the source enumerable is disposed");
				}
				return new InternalPartitionEnumerator(_sharedReader, _sharedIndex, _hasNoElementsLeft, _activePartitionCount, this, _useSingleChunking);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			private void TryCopyFromFillBuffer(KeyValuePair<long, TSource>[] destArray, int requestedChunkSize, ref int actualNumElementsGrabbed)
			{
				actualNumElementsGrabbed = 0;
				KeyValuePair<long, TSource>[] fillBuffer = _fillBuffer;
				if (fillBuffer != null && _fillBufferCurrentPosition < _fillBufferSize)
				{
					Interlocked.Increment(ref _activeCopiers);
					int num = Interlocked.Add(ref _fillBufferCurrentPosition, requestedChunkSize);
					int num2 = num - requestedChunkSize;
					if (num2 < _fillBufferSize)
					{
						actualNumElementsGrabbed = ((num < _fillBufferSize) ? num : (_fillBufferSize - num2));
						Array.Copy(fillBuffer, num2, destArray, 0, actualNumElementsGrabbed);
					}
					Interlocked.Decrement(ref _activeCopiers);
				}
			}

			internal bool GrabChunk(KeyValuePair<long, TSource>[] destArray, int requestedChunkSize, ref int actualNumElementsGrabbed)
			{
				actualNumElementsGrabbed = 0;
				if (_hasNoElementsLeft.Value)
				{
					return false;
				}
				if (_useSingleChunking)
				{
					return GrabChunk_Single(destArray, requestedChunkSize, ref actualNumElementsGrabbed);
				}
				return GrabChunk_Buffered(destArray, requestedChunkSize, ref actualNumElementsGrabbed);
			}

			internal bool GrabChunk_Single(KeyValuePair<long, TSource>[] destArray, int requestedChunkSize, ref int actualNumElementsGrabbed)
			{
				lock (_sharedLock)
				{
					if (_hasNoElementsLeft.Value)
					{
						return false;
					}
					try
					{
						if (_sharedReader.MoveNext())
						{
							_sharedIndex.Value = checked(_sharedIndex.Value + 1);
							destArray[0] = new KeyValuePair<long, TSource>(_sharedIndex.Value, _sharedReader.Current);
							actualNumElementsGrabbed = 1;
							return true;
						}
						_sourceDepleted.Value = true;
						_hasNoElementsLeft.Value = true;
						return false;
					}
					catch
					{
						_sourceDepleted.Value = true;
						_hasNoElementsLeft.Value = true;
						throw;
					}
				}
			}

			internal bool GrabChunk_Buffered(KeyValuePair<long, TSource>[] destArray, int requestedChunkSize, ref int actualNumElementsGrabbed)
			{
				TryCopyFromFillBuffer(destArray, requestedChunkSize, ref actualNumElementsGrabbed);
				if (actualNumElementsGrabbed == requestedChunkSize)
				{
					return true;
				}
				if (_sourceDepleted.Value)
				{
					_hasNoElementsLeft.Value = true;
					_fillBuffer = null;
					return actualNumElementsGrabbed > 0;
				}
				lock (_sharedLock)
				{
					if (_sourceDepleted.Value)
					{
						return actualNumElementsGrabbed > 0;
					}
					try
					{
						if (_activeCopiers > 0)
						{
							SpinWait spinWait = default(SpinWait);
							while (_activeCopiers > 0)
							{
								spinWait.SpinOnce();
							}
						}
						while (actualNumElementsGrabbed < requestedChunkSize)
						{
							if (_sharedReader.MoveNext())
							{
								_sharedIndex.Value = checked(_sharedIndex.Value + 1);
								destArray[actualNumElementsGrabbed] = new KeyValuePair<long, TSource>(_sharedIndex.Value, _sharedReader.Current);
								actualNumElementsGrabbed++;
								continue;
							}
							_sourceDepleted.Value = true;
							break;
						}
						KeyValuePair<long, TSource>[] fillBuffer = _fillBuffer;
						if (!_sourceDepleted.Value && fillBuffer != null && _fillBufferCurrentPosition >= fillBuffer.Length)
						{
							for (int i = 0; i < fillBuffer.Length; i++)
							{
								if (_sharedReader.MoveNext())
								{
									_sharedIndex.Value = checked(_sharedIndex.Value + 1);
									fillBuffer[i] = new KeyValuePair<long, TSource>(_sharedIndex.Value, _sharedReader.Current);
									continue;
								}
								_sourceDepleted.Value = true;
								_fillBufferSize = i;
								break;
							}
							_fillBufferCurrentPosition = 0;
						}
					}
					catch
					{
						_sourceDepleted.Value = true;
						_hasNoElementsLeft.Value = true;
						throw;
					}
				}
				return actualNumElementsGrabbed > 0;
			}

			public void Dispose()
			{
				if (!_disposed)
				{
					_disposed = true;
					_sharedReader.Dispose();
				}
			}
		}

		private class InternalPartitionEnumerator : DynamicPartitionEnumerator_Abstract<TSource, IEnumerator<TSource>>
		{
			private KeyValuePair<long, TSource>[] _localList;

			private readonly SharedBool _hasNoElementsLeft;

			private readonly SharedInt _activePartitionCount;

			private InternalPartitionEnumerable _enumerable;

			protected override bool HasNoElementsLeft
			{
				get
				{
					return _hasNoElementsLeft.Value;
				}
				set
				{
					_hasNoElementsLeft.Value = true;
				}
			}

			public override KeyValuePair<long, TSource> Current
			{
				get
				{
					if (_currentChunkSize == null)
					{
						throw new InvalidOperationException("MoveNext must be called at least once before calling Current.");
					}
					return _localList[_localOffset.Value];
				}
			}

			internal InternalPartitionEnumerator(IEnumerator<TSource> sharedReader, SharedLong sharedIndex, SharedBool hasNoElementsLeft, SharedInt activePartitionCount, InternalPartitionEnumerable enumerable, bool useSingleChunking)
				: base(sharedReader, sharedIndex, useSingleChunking)
			{
				_hasNoElementsLeft = hasNoElementsLeft;
				_enumerable = enumerable;
				_activePartitionCount = activePartitionCount;
				if (_activePartitionCount != null)
				{
					Interlocked.Increment(ref _activePartitionCount.Value);
				}
			}

			protected override bool GrabNextChunk(int requestedChunkSize)
			{
				if (HasNoElementsLeft)
				{
					return false;
				}
				if (_localList == null)
				{
					_localList = new KeyValuePair<long, TSource>[_maxChunkSize];
				}
				return _enumerable.GrabChunk(_localList, requestedChunkSize, ref _currentChunkSize.Value);
			}

			public override void Dispose()
			{
				if (_activePartitionCount != null && Interlocked.Decrement(ref _activePartitionCount.Value) == 0)
				{
					_enumerable.Dispose();
				}
			}
		}

		private IEnumerable<TSource> _source;

		private readonly bool _useSingleChunking;

		public override bool SupportsDynamicPartitions => true;

		internal DynamicPartitionerForIEnumerable(IEnumerable<TSource> source, EnumerablePartitionerOptions partitionerOptions)
			: base(keysOrderedInEachPartition: true, keysOrderedAcrossPartitions: false, keysNormalized: true)
		{
			_source = source;
			_useSingleChunking = (partitionerOptions & EnumerablePartitionerOptions.NoBuffering) != 0;
		}

		public override IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount)
		{
			if (partitionCount <= 0)
			{
				throw new ArgumentOutOfRangeException("partitionCount");
			}
			IEnumerator<KeyValuePair<long, TSource>>[] array = new IEnumerator<KeyValuePair<long, TSource>>[partitionCount];
			IEnumerable<KeyValuePair<long, TSource>> enumerable = new InternalPartitionEnumerable(_source.GetEnumerator(), _useSingleChunking, isStaticPartitioning: true);
			for (int i = 0; i < partitionCount; i++)
			{
				array[i] = enumerable.GetEnumerator();
			}
			return array;
		}

		public override IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions()
		{
			return new InternalPartitionEnumerable(_source.GetEnumerator(), _useSingleChunking, isStaticPartitioning: false);
		}
	}

	private abstract class DynamicPartitionerForIndexRange_Abstract<TSource, TCollection> : OrderablePartitioner<TSource>
	{
		private TCollection _data;

		public override bool SupportsDynamicPartitions => true;

		protected DynamicPartitionerForIndexRange_Abstract(TCollection data)
			: base(keysOrderedInEachPartition: true, keysOrderedAcrossPartitions: false, keysNormalized: true)
		{
			_data = data;
		}

		protected abstract IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions_Factory(TCollection data);

		public override IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount)
		{
			if (partitionCount <= 0)
			{
				throw new ArgumentOutOfRangeException("partitionCount");
			}
			IEnumerator<KeyValuePair<long, TSource>>[] array = new IEnumerator<KeyValuePair<long, TSource>>[partitionCount];
			IEnumerable<KeyValuePair<long, TSource>> orderableDynamicPartitions_Factory = GetOrderableDynamicPartitions_Factory(_data);
			for (int i = 0; i < partitionCount; i++)
			{
				array[i] = orderableDynamicPartitions_Factory.GetEnumerator();
			}
			return array;
		}

		public override IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions()
		{
			return GetOrderableDynamicPartitions_Factory(_data);
		}
	}

	private abstract class DynamicPartitionEnumeratorForIndexRange_Abstract<TSource, TSourceReader> : DynamicPartitionEnumerator_Abstract<TSource, TSourceReader>
	{
		protected int _startIndex;

		protected abstract int SourceCount { get; }

		protected override bool HasNoElementsLeft
		{
			get
			{
				return Volatile.Read(ref _sharedIndex.Value) >= SourceCount - 1;
			}
			set
			{
			}
		}

		protected DynamicPartitionEnumeratorForIndexRange_Abstract(TSourceReader sharedReader, SharedLong sharedIndex)
			: base(sharedReader, sharedIndex)
		{
		}

		protected override bool GrabNextChunk(int requestedChunkSize)
		{
			while (!HasNoElementsLeft)
			{
				long num = Volatile.Read(ref _sharedIndex.Value);
				if (HasNoElementsLeft)
				{
					return false;
				}
				long num2 = Math.Min(SourceCount - 1, num + requestedChunkSize);
				if (Interlocked.CompareExchange(ref _sharedIndex.Value, num2, num) == num)
				{
					_currentChunkSize.Value = (int)(num2 - num);
					_localOffset.Value = -1;
					_startIndex = (int)(num + 1);
					return true;
				}
			}
			return false;
		}

		public override void Dispose()
		{
		}
	}

	private class DynamicPartitionerForIList<TSource> : DynamicPartitionerForIndexRange_Abstract<TSource, IList<TSource>>
	{
		private class InternalPartitionEnumerable : IEnumerable<KeyValuePair<long, TSource>>, IEnumerable
		{
			private readonly IList<TSource> _sharedReader;

			private SharedLong _sharedIndex;

			internal InternalPartitionEnumerable(IList<TSource> sharedReader)
			{
				_sharedReader = sharedReader;
				_sharedIndex = new SharedLong(-1L);
			}

			public IEnumerator<KeyValuePair<long, TSource>> GetEnumerator()
			{
				return new InternalPartitionEnumerator(_sharedReader, _sharedIndex);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		private class InternalPartitionEnumerator : DynamicPartitionEnumeratorForIndexRange_Abstract<TSource, IList<TSource>>
		{
			protected override int SourceCount => _sharedReader.Count;

			public override KeyValuePair<long, TSource> Current
			{
				get
				{
					if (_currentChunkSize == null)
					{
						throw new InvalidOperationException("MoveNext must be called at least once before calling Current.");
					}
					return new KeyValuePair<long, TSource>(_startIndex + _localOffset.Value, _sharedReader[_startIndex + _localOffset.Value]);
				}
			}

			internal InternalPartitionEnumerator(IList<TSource> sharedReader, SharedLong sharedIndex)
				: base(sharedReader, sharedIndex)
			{
			}
		}

		internal DynamicPartitionerForIList(IList<TSource> source)
			: base(source)
		{
		}

		protected override IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions_Factory(IList<TSource> _data)
		{
			return new InternalPartitionEnumerable(_data);
		}
	}

	private class DynamicPartitionerForArray<TSource> : DynamicPartitionerForIndexRange_Abstract<TSource, TSource[]>
	{
		private class InternalPartitionEnumerable : IEnumerable<KeyValuePair<long, TSource>>, IEnumerable
		{
			private readonly TSource[] _sharedReader;

			private SharedLong _sharedIndex;

			internal InternalPartitionEnumerable(TSource[] sharedReader)
			{
				_sharedReader = sharedReader;
				_sharedIndex = new SharedLong(-1L);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public IEnumerator<KeyValuePair<long, TSource>> GetEnumerator()
			{
				return new InternalPartitionEnumerator(_sharedReader, _sharedIndex);
			}
		}

		private class InternalPartitionEnumerator : DynamicPartitionEnumeratorForIndexRange_Abstract<TSource, TSource[]>
		{
			protected override int SourceCount => _sharedReader.Length;

			public override KeyValuePair<long, TSource> Current
			{
				get
				{
					if (_currentChunkSize == null)
					{
						throw new InvalidOperationException("MoveNext must be called at least once before calling Current.");
					}
					return new KeyValuePair<long, TSource>(_startIndex + _localOffset.Value, _sharedReader[_startIndex + _localOffset.Value]);
				}
			}

			internal InternalPartitionEnumerator(TSource[] sharedReader, SharedLong sharedIndex)
				: base(sharedReader, sharedIndex)
			{
			}
		}

		internal DynamicPartitionerForArray(TSource[] source)
			: base(source)
		{
		}

		protected override IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions_Factory(TSource[] _data)
		{
			return new InternalPartitionEnumerable(_data);
		}
	}

	private abstract class StaticIndexRangePartitioner<TSource, TCollection> : OrderablePartitioner<TSource>
	{
		protected abstract int SourceCount { get; }

		protected StaticIndexRangePartitioner()
			: base(keysOrderedInEachPartition: true, keysOrderedAcrossPartitions: true, keysNormalized: true)
		{
		}

		protected abstract IEnumerator<KeyValuePair<long, TSource>> CreatePartition(int startIndex, int endIndex);

		public override IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount)
		{
			if (partitionCount <= 0)
			{
				throw new ArgumentOutOfRangeException("partitionCount");
			}
			int num = SourceCount / partitionCount;
			int num2 = SourceCount % partitionCount;
			IEnumerator<KeyValuePair<long, TSource>>[] array = new IEnumerator<KeyValuePair<long, TSource>>[partitionCount];
			int num3 = -1;
			for (int i = 0; i < partitionCount; i++)
			{
				int num4 = num3 + 1;
				num3 = ((i >= num2) ? (num4 + num - 1) : (num4 + num));
				array[i] = CreatePartition(num4, num3);
			}
			return array;
		}
	}

	private abstract class StaticIndexRangePartition<TSource> : IEnumerator<KeyValuePair<long, TSource>>, IDisposable, IEnumerator
	{
		protected readonly int _startIndex;

		protected readonly int _endIndex;

		protected volatile int _offset;

		public abstract KeyValuePair<long, TSource> Current { get; }

		object IEnumerator.Current => Current;

		protected StaticIndexRangePartition(int startIndex, int endIndex)
		{
			_startIndex = startIndex;
			_endIndex = endIndex;
			_offset = startIndex - 1;
		}

		public void Dispose()
		{
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}

		public bool MoveNext()
		{
			if (_offset < _endIndex)
			{
				_offset++;
				return true;
			}
			_offset = _endIndex + 1;
			return false;
		}
	}

	private class StaticIndexRangePartitionerForIList<TSource> : StaticIndexRangePartitioner<TSource, IList<TSource>>
	{
		private IList<TSource> _list;

		protected override int SourceCount => _list.Count;

		internal StaticIndexRangePartitionerForIList(IList<TSource> list)
		{
			_list = list;
		}

		protected override IEnumerator<KeyValuePair<long, TSource>> CreatePartition(int startIndex, int endIndex)
		{
			return new StaticIndexRangePartitionForIList<TSource>(_list, startIndex, endIndex);
		}
	}

	private class StaticIndexRangePartitionForIList<TSource> : StaticIndexRangePartition<TSource>
	{
		private volatile IList<TSource> _list;

		public override KeyValuePair<long, TSource> Current
		{
			get
			{
				if (_offset < _startIndex)
				{
					throw new InvalidOperationException("MoveNext must be called at least once before calling Current.");
				}
				return new KeyValuePair<long, TSource>(_offset, _list[_offset]);
			}
		}

		internal StaticIndexRangePartitionForIList(IList<TSource> list, int startIndex, int endIndex)
			: base(startIndex, endIndex)
		{
			_list = list;
		}
	}

	private class StaticIndexRangePartitionerForArray<TSource> : StaticIndexRangePartitioner<TSource, TSource[]>
	{
		private TSource[] _array;

		protected override int SourceCount => _array.Length;

		internal StaticIndexRangePartitionerForArray(TSource[] array)
		{
			_array = array;
		}

		protected override IEnumerator<KeyValuePair<long, TSource>> CreatePartition(int startIndex, int endIndex)
		{
			return new StaticIndexRangePartitionForArray<TSource>(_array, startIndex, endIndex);
		}
	}

	private class StaticIndexRangePartitionForArray<TSource> : StaticIndexRangePartition<TSource>
	{
		private volatile TSource[] _array;

		public override KeyValuePair<long, TSource> Current
		{
			get
			{
				if (_offset < _startIndex)
				{
					throw new InvalidOperationException("MoveNext must be called at least once before calling Current.");
				}
				return new KeyValuePair<long, TSource>(_offset, _array[_offset]);
			}
		}

		internal StaticIndexRangePartitionForArray(TSource[] array, int startIndex, int endIndex)
			: base(startIndex, endIndex)
		{
			_array = array;
		}
	}

	private class SharedInt
	{
		internal volatile int Value;

		internal SharedInt(int value)
		{
			Value = value;
		}
	}

	private class SharedBool
	{
		internal volatile bool Value;

		internal SharedBool(bool value)
		{
			Value = value;
		}
	}

	private class SharedLong
	{
		internal long Value;

		internal SharedLong(long value)
		{
			Value = value;
		}
	}

	private const int DEFAULT_BYTES_PER_UNIT = 128;

	private const int DEFAULT_BYTES_PER_CHUNK = 512;

	/// <summary>Creates an orderable partitioner from an <see cref="T:System.Collections.Generic.IList`1" /> instance.</summary>
	/// <returns>An orderable partitioner based on the input list.</returns>
	/// <param name="list">The list to be partitioned.</param>
	/// <param name="loadBalance">A Boolean value that indicates whether the created partitioner should dynamically load balance between partitions rather than statically partition.</param>
	/// <typeparam name="TSource">Type of the elements in source list.</typeparam>
	public static OrderablePartitioner<TSource> Create<TSource>(IList<TSource> list, bool loadBalance)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (loadBalance)
		{
			return new DynamicPartitionerForIList<TSource>(list);
		}
		return new StaticIndexRangePartitionerForIList<TSource>(list);
	}

	/// <summary>Creates an orderable partitioner from a <see cref="T:System.Array" /> instance.</summary>
	/// <returns>An orderable partitioner based on the input array.</returns>
	/// <param name="array">The array to be partitioned.</param>
	/// <param name="loadBalance">A Boolean value that indicates whether the created partitioner should dynamically load balance between partitions rather than statically partition.</param>
	/// <typeparam name="TSource">Type of the elements in source array.</typeparam>
	public static OrderablePartitioner<TSource> Create<TSource>(TSource[] array, bool loadBalance)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (loadBalance)
		{
			return new DynamicPartitionerForArray<TSource>(array);
		}
		return new StaticIndexRangePartitionerForArray<TSource>(array);
	}

	/// <summary>Creates an orderable partitioner from a <see cref="T:System.Collections.Generic.IEnumerable`1" /> instance.</summary>
	/// <returns>An orderable partitioner based on the input array.</returns>
	/// <param name="source">The enumerable to be partitioned.</param>
	/// <typeparam name="TSource">Type of the elements in source enumerable.</typeparam>
	public static OrderablePartitioner<TSource> Create<TSource>(IEnumerable<TSource> source)
	{
		return Create(source, EnumerablePartitionerOptions.None);
	}

	/// <summary>Creates an orderable partitioner from a <see cref="T:System.Collections.Generic.IEnumerable`1" /> instance.</summary>
	/// <returns>An orderable partitioner based on the input array.</returns>
	/// <param name="source">The enumerable to be partitioned.</param>
	/// <param name="partitionerOptions">Options to control the buffering behavior of the partitioner.</param>
	/// <typeparam name="TSource">Type of the elements in source enumerable.</typeparam>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="partitionerOptions" /> argument specifies an invalid value for <see cref="T:System.Collections.Concurrent.EnumerablePartitionerOptions" />.</exception>
	public static OrderablePartitioner<TSource> Create<TSource>(IEnumerable<TSource> source, EnumerablePartitionerOptions partitionerOptions)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if ((partitionerOptions & ~EnumerablePartitionerOptions.NoBuffering) != 0)
		{
			throw new ArgumentOutOfRangeException("partitionerOptions");
		}
		return new DynamicPartitionerForIEnumerable<TSource>(source, partitionerOptions);
	}

	/// <summary>Creates a partitioner that chunks the user-specified range.</summary>
	/// <returns>A partitioner.</returns>
	/// <param name="fromInclusive">The lower, inclusive bound of the range.</param>
	/// <param name="toExclusive">The upper, exclusive bound of the range.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="toExclusive" /> argument is less than or equal to the <paramref name="fromInclusive" /> argument.</exception>
	public static OrderablePartitioner<Tuple<long, long>> Create(long fromInclusive, long toExclusive)
	{
		int num = 3;
		if (toExclusive <= fromInclusive)
		{
			throw new ArgumentOutOfRangeException("toExclusive");
		}
		long num2 = (toExclusive - fromInclusive) / (PlatformHelper.ProcessorCount * num);
		if (num2 == 0L)
		{
			num2 = 1L;
		}
		return Create(CreateRanges(fromInclusive, toExclusive, num2), EnumerablePartitionerOptions.NoBuffering);
	}

	/// <summary>Creates a partitioner that chunks the user-specified range.</summary>
	/// <returns>A partitioner.</returns>
	/// <param name="fromInclusive">The lower, inclusive bound of the range.</param>
	/// <param name="toExclusive">The upper, exclusive bound of the range.</param>
	/// <param name="rangeSize">The size of each subrange.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="toExclusive" /> argument is less than or equal to the <paramref name="fromInclusive" /> argument.-or-The <paramref name="rangeSize" /> argument is less than or equal to 0.</exception>
	public static OrderablePartitioner<Tuple<long, long>> Create(long fromInclusive, long toExclusive, long rangeSize)
	{
		if (toExclusive <= fromInclusive)
		{
			throw new ArgumentOutOfRangeException("toExclusive");
		}
		if (rangeSize <= 0)
		{
			throw new ArgumentOutOfRangeException("rangeSize");
		}
		return Create(CreateRanges(fromInclusive, toExclusive, rangeSize), EnumerablePartitionerOptions.NoBuffering);
	}

	private static IEnumerable<Tuple<long, long>> CreateRanges(long fromInclusive, long toExclusive, long rangeSize)
	{
		bool shouldQuit = false;
		for (long i = fromInclusive; i < toExclusive; i += rangeSize)
		{
			if (shouldQuit)
			{
				break;
			}
			long item = i;
			long num;
			try
			{
				num = checked(i + rangeSize);
			}
			catch (OverflowException)
			{
				num = toExclusive;
				shouldQuit = true;
			}
			if (num > toExclusive)
			{
				num = toExclusive;
			}
			yield return new Tuple<long, long>(item, num);
		}
	}

	/// <summary>Creates a partitioner that chunks the user-specified range.</summary>
	/// <returns>A partitioner.</returns>
	/// <param name="fromInclusive">The lower, inclusive bound of the range.</param>
	/// <param name="toExclusive">The upper, exclusive bound of the range.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="toExclusive" /> argument is less than or equal to the <paramref name="fromInclusive" /> argument.</exception>
	public static OrderablePartitioner<Tuple<int, int>> Create(int fromInclusive, int toExclusive)
	{
		int num = 3;
		if (toExclusive <= fromInclusive)
		{
			throw new ArgumentOutOfRangeException("toExclusive");
		}
		int num2 = (toExclusive - fromInclusive) / (PlatformHelper.ProcessorCount * num);
		if (num2 == 0)
		{
			num2 = 1;
		}
		return Create(CreateRanges(fromInclusive, toExclusive, num2), EnumerablePartitionerOptions.NoBuffering);
	}

	/// <summary>Creates a partitioner that chunks the user-specified range.</summary>
	/// <returns>A partitioner.</returns>
	/// <param name="fromInclusive">The lower, inclusive bound of the range.</param>
	/// <param name="toExclusive">The upper, exclusive bound of the range.</param>
	/// <param name="rangeSize">The size of each subrange.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="toExclusive" /> argument is less than or equal to the <paramref name="fromInclusive" /> argument.-or-The <paramref name="rangeSize" /> argument is less than or equal to 0.</exception>
	public static OrderablePartitioner<Tuple<int, int>> Create(int fromInclusive, int toExclusive, int rangeSize)
	{
		if (toExclusive <= fromInclusive)
		{
			throw new ArgumentOutOfRangeException("toExclusive");
		}
		if (rangeSize <= 0)
		{
			throw new ArgumentOutOfRangeException("rangeSize");
		}
		return Create(CreateRanges(fromInclusive, toExclusive, rangeSize), EnumerablePartitionerOptions.NoBuffering);
	}

	private static IEnumerable<Tuple<int, int>> CreateRanges(int fromInclusive, int toExclusive, int rangeSize)
	{
		bool shouldQuit = false;
		for (int i = fromInclusive; i < toExclusive; i += rangeSize)
		{
			if (shouldQuit)
			{
				break;
			}
			int item = i;
			int num;
			try
			{
				num = checked(i + rangeSize);
			}
			catch (OverflowException)
			{
				num = toExclusive;
				shouldQuit = true;
			}
			if (num > toExclusive)
			{
				num = toExclusive;
			}
			yield return new Tuple<int, int>(item, num);
		}
	}

	private static int GetDefaultChunkSize<TSource>()
	{
		if (default(TSource) != null || Nullable.GetUnderlyingType(typeof(TSource)) != null)
		{
			return 128;
		}
		return 512 / IntPtr.Size;
	}
}
