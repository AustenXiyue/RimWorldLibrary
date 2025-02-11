using System.Threading;

namespace System.Runtime.InteropServices;

/// <summary>Tracks outstanding handles and forces a garbage collection when the specified threshold is reached.</summary>
public sealed class HandleCollector
{
	private const int deltaPercent = 10;

	private string name;

	private int initialThreshold;

	private int maximumThreshold;

	private int threshold;

	private int handleCount;

	private int[] gc_counts = new int[3];

	private int gc_gen;

	/// <summary>Gets the number of handles collected.</summary>
	/// <returns>The number of handles collected.</returns>
	public int Count => handleCount;

	/// <summary>Gets a value that specifies the point at which collections should begin.</summary>
	/// <returns>A value that specifies the point at which collections should begin.</returns>
	public int InitialThreshold => initialThreshold;

	/// <summary>Gets a value that specifies the point at which collections must occur.</summary>
	/// <returns>A value that specifies the point at which collections must occur.</returns>
	public int MaximumThreshold => maximumThreshold;

	/// <summary>Gets the name of a <see cref="T:System.Runtime.InteropServices.HandleCollector" /> object.</summary>
	/// <returns>This <see cref="P:System.Runtime.InteropServices.HandleCollector.Name" /> property allows you to name collectors that track handle types separately.</returns>
	public string Name => name;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.HandleCollector" /> class using a name and a threshold at which to begin handle collection. </summary>
	/// <param name="name">A name for the collector. This parameter allows you to name collectors that track handle types separately.</param>
	/// <param name="initialThreshold">A value that specifies the point at which collections should begin.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="initialThreshold" /> parameter is less than 0.</exception>
	public HandleCollector(string name, int initialThreshold)
		: this(name, initialThreshold, int.MaxValue)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.HandleCollector" /> class using a name, a threshold at which to begin handle collection, and a threshold at which handle collection must occur. </summary>
	/// <param name="name">A name for the collector.  This parameter allows you to name collectors that track handle types separately.</param>
	/// <param name="initialThreshold">A value that specifies the point at which collections should begin.</param>
	/// <param name="maximumThreshold">A value that specifies the point at which collections must occur. This should be set to the maximum number of available handles.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="initialThreshold" /> parameter is less than 0.-or-The <paramref name="maximumThreshold" /> parameter is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="maximumThreshold" /> parameter is less than the <paramref name="initialThreshold" /> parameter.</exception>
	public HandleCollector(string name, int initialThreshold, int maximumThreshold)
	{
		if (initialThreshold < 0)
		{
			throw new ArgumentOutOfRangeException("initialThreshold", global::SR.GetString("Non-negative number required."));
		}
		if (maximumThreshold < 0)
		{
			throw new ArgumentOutOfRangeException("maximumThreshold", global::SR.GetString("Non-negative number required."));
		}
		if (initialThreshold > maximumThreshold)
		{
			throw new ArgumentException(global::SR.GetString("maximumThreshold cannot be less than initialThreshold."));
		}
		if (name != null)
		{
			this.name = name;
		}
		else
		{
			this.name = string.Empty;
		}
		this.initialThreshold = initialThreshold;
		this.maximumThreshold = maximumThreshold;
		threshold = initialThreshold;
		handleCount = 0;
	}

	/// <summary>Increments the current handle count.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Runtime.InteropServices.HandleCollector.Count" /> property is less than 0.</exception>
	public void Add()
	{
		int num = -1;
		Interlocked.Increment(ref handleCount);
		if (handleCount < 0)
		{
			throw new InvalidOperationException(global::SR.GetString("Handle collector count overflows or underflows."));
		}
		if (handleCount > threshold)
		{
			lock (this)
			{
				threshold = handleCount + handleCount / 10;
				num = gc_gen;
				if (gc_gen < 2)
				{
					gc_gen++;
				}
			}
		}
		if (num >= 0 && (num == 0 || gc_counts[num] == GC.CollectionCount(num)))
		{
			GC.Collect(num);
			Thread.Sleep(10 * num);
		}
		for (int i = 1; i < 3; i++)
		{
			gc_counts[i] = GC.CollectionCount(i);
		}
	}

	/// <summary>Decrements the current handle count.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Runtime.InteropServices.HandleCollector.Count" /> property is less than 0.</exception>
	public void Remove()
	{
		Interlocked.Decrement(ref handleCount);
		if (handleCount < 0)
		{
			throw new InvalidOperationException(global::SR.GetString("Handle collector count overflows or underflows."));
		}
		int num = handleCount + handleCount / 10;
		if (num < threshold - threshold / 10)
		{
			lock (this)
			{
				if (num > initialThreshold)
				{
					threshold = num;
				}
				else
				{
					threshold = initialThreshold;
				}
				gc_gen = 0;
			}
		}
		for (int i = 1; i < 3; i++)
		{
			gc_counts[i] = GC.CollectionCount(i);
		}
	}
}
