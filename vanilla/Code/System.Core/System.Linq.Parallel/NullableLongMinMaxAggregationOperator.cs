using System.Collections.Generic;
using System.Threading;

namespace System.Linq.Parallel;

internal sealed class NullableLongMinMaxAggregationOperator : InlinedAggregationOperator<long?, long?, long?>
{
	private class NullableLongMinMaxAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<long?>
	{
		private QueryOperatorEnumerator<long?, TKey> _source;

		private int _sign;

		internal NullableLongMinMaxAggregationOperatorEnumerator(QueryOperatorEnumerator<long?, TKey> source, int partitionIndex, int sign, CancellationToken cancellationToken)
			: base(partitionIndex, cancellationToken)
		{
			_source = source;
			_sign = sign;
		}

		protected override bool MoveNextCore(ref long? currentElement)
		{
			QueryOperatorEnumerator<long?, TKey> source = _source;
			TKey currentKey = default(TKey);
			if (source.MoveNext(ref currentElement, ref currentKey))
			{
				int num = 0;
				if (_sign == -1)
				{
					long? currentElement2 = null;
					while (source.MoveNext(ref currentElement2, ref currentKey))
					{
						if ((num++ & 0x3F) == 0)
						{
							CancellationState.ThrowIfCanceled(_cancellationToken);
						}
						if (!currentElement.HasValue || currentElement2 < currentElement)
						{
							currentElement = currentElement2;
						}
					}
				}
				else
				{
					long? currentElement3 = null;
					while (source.MoveNext(ref currentElement3, ref currentKey))
					{
						if ((num++ & 0x3F) == 0)
						{
							CancellationState.ThrowIfCanceled(_cancellationToken);
						}
						if (!currentElement.HasValue || currentElement3 > currentElement)
						{
							currentElement = currentElement3;
						}
					}
				}
				return true;
			}
			return false;
		}

		protected override void Dispose(bool disposing)
		{
			_source.Dispose();
		}
	}

	private readonly int _sign;

	internal NullableLongMinMaxAggregationOperator(IEnumerable<long?> child, int sign)
		: base(child)
	{
		_sign = sign;
	}

	protected override long? InternalAggregate(ref Exception singularExceptionToThrow)
	{
		using IEnumerator<long?> enumerator = GetEnumerator(ParallelMergeOptions.FullyBuffered, suppressOrderPreservation: true);
		if (!enumerator.MoveNext())
		{
			return null;
		}
		long? num = enumerator.Current;
		if (_sign == -1)
		{
			while (enumerator.MoveNext())
			{
				long? current = enumerator.Current;
				if (!num.HasValue || current < num)
				{
					num = current;
				}
			}
		}
		else
		{
			while (enumerator.MoveNext())
			{
				long? current2 = enumerator.Current;
				if (!num.HasValue || current2 > num)
				{
					num = current2;
				}
			}
		}
		return num;
	}

	protected override QueryOperatorEnumerator<long?, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<long?, TKey> source, object sharedData, CancellationToken cancellationToken)
	{
		return new NullableLongMinMaxAggregationOperatorEnumerator<TKey>(source, index, _sign, cancellationToken);
	}
}
