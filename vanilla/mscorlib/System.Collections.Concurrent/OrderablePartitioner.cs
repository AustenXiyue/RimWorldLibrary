using System.Collections.Generic;

namespace System.Collections.Concurrent;

/// <summary>Represents a particular manner of splitting an orderable data source into multiple partitions.</summary>
/// <typeparam name="TSource">Type of the elements in the collection.</typeparam>
public abstract class OrderablePartitioner<TSource> : Partitioner<TSource>
{
	private class EnumerableDropIndices : IEnumerable<TSource>, IEnumerable, IDisposable
	{
		private readonly IEnumerable<KeyValuePair<long, TSource>> _source;

		public EnumerableDropIndices(IEnumerable<KeyValuePair<long, TSource>> source)
		{
			_source = source;
		}

		public IEnumerator<TSource> GetEnumerator()
		{
			return new EnumeratorDropIndices(_source.GetEnumerator());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Dispose()
		{
			if (_source is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}

	private class EnumeratorDropIndices : IEnumerator<TSource>, IDisposable, IEnumerator
	{
		private readonly IEnumerator<KeyValuePair<long, TSource>> _source;

		public TSource Current => _source.Current.Value;

		object IEnumerator.Current => Current;

		public EnumeratorDropIndices(IEnumerator<KeyValuePair<long, TSource>> source)
		{
			_source = source;
		}

		public bool MoveNext()
		{
			return _source.MoveNext();
		}

		public void Dispose()
		{
			_source.Dispose();
		}

		public void Reset()
		{
			_source.Reset();
		}
	}

	/// <summary>Gets whether elements in each partition are yielded in the order of increasing keys.</summary>
	/// <returns>true if the elements in each partition are yielded in the order of increasing keys; otherwise false.</returns>
	public bool KeysOrderedInEachPartition { get; private set; }

	/// <summary>Gets whether elements in an earlier partition always come before elements in a later partition.</summary>
	/// <returns>true if the elements in an earlier partition always come before elements in a later partition; otherwise false.</returns>
	public bool KeysOrderedAcrossPartitions { get; private set; }

	/// <summary>Gets whether order keys are normalized.</summary>
	/// <returns>true if the keys are normalized; otherwise false.</returns>
	public bool KeysNormalized { get; private set; }

	/// <summary>Called from constructors in derived classes to initialize the <see cref="T:System.Collections.Concurrent.OrderablePartitioner`1" /> class with the specified constraints on the index keys.</summary>
	/// <param name="keysOrderedInEachPartition">Indicates whether the elements in each partition are yielded in the order of increasing keys.</param>
	/// <param name="keysOrderedAcrossPartitions">Indicates whether elements in an earlier partition always come before elements in a later partition. If true, each element in partition 0 has a smaller order key than any element in partition 1, each element in partition 1 has a smaller order key than any element in partition 2, and so on.</param>
	/// <param name="keysNormalized">Indicates whether keys are normalized. If true, all order keys are distinct integers in the range [0 .. numberOfElements-1]. If false, order keys must still be distinct, but only their relative order is considered, not their absolute values.</param>
	protected OrderablePartitioner(bool keysOrderedInEachPartition, bool keysOrderedAcrossPartitions, bool keysNormalized)
	{
		KeysOrderedInEachPartition = keysOrderedInEachPartition;
		KeysOrderedAcrossPartitions = keysOrderedAcrossPartitions;
		KeysNormalized = keysNormalized;
	}

	/// <summary>Partitions the underlying collection into the specified number of orderable partitions.</summary>
	/// <returns>A list containing <paramref name="partitionCount" /> enumerators.</returns>
	/// <param name="partitionCount">The number of partitions to create.</param>
	public abstract IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount);

	/// <summary>Creates an object that can partition the underlying collection into a variable number of partitions.</summary>
	/// <returns>An object that can create partitions over the underlying data source.</returns>
	/// <exception cref="T:System.NotSupportedException">Dynamic partitioning is not supported by this partitioner.</exception>
	public virtual IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions()
	{
		throw new NotSupportedException("Dynamic partitions are not supported by this partitioner.");
	}

	/// <summary>Partitions the underlying collection into the given number of ordered partitions.</summary>
	/// <returns>A list containing <paramref name="partitionCount" /> enumerators.</returns>
	/// <param name="partitionCount">The number of partitions to create.</param>
	public override IList<IEnumerator<TSource>> GetPartitions(int partitionCount)
	{
		IList<IEnumerator<KeyValuePair<long, TSource>>> orderablePartitions = GetOrderablePartitions(partitionCount);
		if (orderablePartitions.Count != partitionCount)
		{
			throw new InvalidOperationException("GetPartitions returned an incorrect number of partitions.");
		}
		IEnumerator<TSource>[] array = new IEnumerator<TSource>[partitionCount];
		for (int i = 0; i < partitionCount; i++)
		{
			array[i] = new EnumeratorDropIndices(orderablePartitions[i]);
		}
		return array;
	}

	/// <summary>Creates an object that can partition the underlying collection into a variable number of partitions.</summary>
	/// <returns>An object that can create partitions over the underlying data source.</returns>
	/// <exception cref="T:System.NotSupportedException">Dynamic partitioning is not supported by the base class. It must be implemented in derived classes.</exception>
	public override IEnumerable<TSource> GetDynamicPartitions()
	{
		return new EnumerableDropIndices(GetOrderableDynamicPartitions());
	}
}
