using System.Collections.Generic;
using System.Threading;

namespace System.Linq.Parallel;

internal sealed class NullableLongAverageAggregationOperator : InlinedAggregationOperator<long?, Pair<long, long>, double?>
{
	private class NullableLongAverageAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<Pair<long, long>>
	{
		private QueryOperatorEnumerator<long?, TKey> _source;

		internal NullableLongAverageAggregationOperatorEnumerator(QueryOperatorEnumerator<long?, TKey> source, int partitionIndex, CancellationToken cancellationToken)
			: base(partitionIndex, cancellationToken)
		{
			_source = source;
		}

		protected override bool MoveNextCore(ref Pair<long, long> currentElement)
		{
			long num = 0L;
			long num2 = 0L;
			QueryOperatorEnumerator<long?, TKey> source = _source;
			long? currentElement2 = null;
			TKey currentKey = default(TKey);
			int num3 = 0;
			while (source.MoveNext(ref currentElement2, ref currentKey))
			{
				if ((num3++ & 0x3F) == 0)
				{
					CancellationState.ThrowIfCanceled(_cancellationToken);
				}
				checked
				{
					if (currentElement2.HasValue)
					{
						num += currentElement2.GetValueOrDefault();
						num2++;
					}
				}
			}
			currentElement = new Pair<long, long>(num, num2);
			return num2 > 0;
		}

		protected override void Dispose(bool disposing)
		{
			_source.Dispose();
		}
	}

	internal NullableLongAverageAggregationOperator(IEnumerable<long?> child)
		: base(child)
	{
	}

	protected override double? InternalAggregate(ref Exception singularExceptionToThrow)
	{
		checked
		{
			using IEnumerator<Pair<long, long>> enumerator = GetEnumerator(ParallelMergeOptions.FullyBuffered, suppressOrderPreservation: true);
			if (!enumerator.MoveNext())
			{
				return null;
			}
			Pair<long, long> current = enumerator.Current;
			while (enumerator.MoveNext())
			{
				current.First += enumerator.Current.First;
				current.Second += enumerator.Current.Second;
			}
			return (double)current.First / (double)current.Second;
		}
	}

	protected override QueryOperatorEnumerator<Pair<long, long>, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<long?, TKey> source, object sharedData, CancellationToken cancellationToken)
	{
		return new NullableLongAverageAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
	}
}
