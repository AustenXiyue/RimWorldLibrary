using System.Collections.Generic;
using System.Threading;

namespace System.Linq.Parallel;

internal class OrderedHashRepartitionStream<TInputOutput, THashKey, TOrderKey> : HashRepartitionStream<TInputOutput, THashKey, TOrderKey>
{
	internal OrderedHashRepartitionStream(PartitionedStream<TInputOutput, TOrderKey> inputStream, Func<TInputOutput, THashKey> hashKeySelector, IEqualityComparer<THashKey> hashKeyComparer, IEqualityComparer<TInputOutput> elementComparer, CancellationToken cancellationToken)
		: base(inputStream.PartitionCount, inputStream.KeyComparer, hashKeyComparer, elementComparer)
	{
		_partitions = new OrderedHashRepartitionEnumerator<TInputOutput, THashKey, TOrderKey>[inputStream.PartitionCount];
		CountdownEvent barrier = new CountdownEvent(inputStream.PartitionCount);
		ListChunk<Pair<TInputOutput, THashKey>>[][] valueExchangeMatrix = JaggedArray<ListChunk<Pair<TInputOutput, THashKey>>>.Allocate(inputStream.PartitionCount, inputStream.PartitionCount);
		ListChunk<TOrderKey>[][] keyExchangeMatrix = JaggedArray<ListChunk<TOrderKey>>.Allocate(inputStream.PartitionCount, inputStream.PartitionCount);
		for (int i = 0; i < inputStream.PartitionCount; i++)
		{
			_partitions[i] = new OrderedHashRepartitionEnumerator<TInputOutput, THashKey, TOrderKey>(inputStream[i], inputStream.PartitionCount, i, hashKeySelector, this, barrier, valueExchangeMatrix, keyExchangeMatrix, cancellationToken);
		}
	}
}
