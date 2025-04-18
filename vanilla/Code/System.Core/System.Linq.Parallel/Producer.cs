namespace System.Linq.Parallel;

internal struct Producer<TKey>
{
	internal readonly TKey MaxKey;

	internal readonly int ProducerIndex;

	internal Producer(TKey maxKey, int producerIndex)
	{
		MaxKey = maxKey;
		ProducerIndex = producerIndex;
	}
}
