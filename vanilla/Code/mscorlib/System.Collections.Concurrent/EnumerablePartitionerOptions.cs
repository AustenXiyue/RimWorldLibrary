namespace System.Collections.Concurrent;

/// <summary>Specifies options to control the buffering behavior of a partitioner</summary>
[Flags]
public enum EnumerablePartitionerOptions
{
	/// <summary>Use the default behavior, which is to use buffering to achieve optimal performance.</summary>
	None = 0,
	/// <summary>Create a partitioner that takes items from the source enumerable one at a time and does not use intermediate storage that can be accessed more efficiently by multiple threads. This option provides support for low latency (items will be processed as soon as they are available from the source) and provides partial support for dependencies between items (a thread cannot deadlock waiting for an item that the thread itself is responsible for processing).</summary>
	NoBuffering = 1
}
