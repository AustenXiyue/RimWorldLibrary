namespace System.Runtime;

/// <summary>Indicates whether the next blocking garbage collection compacts the large object heap (LOH). </summary>
public enum GCLargeObjectHeapCompactionMode
{
	/// <summary>Blocking garbage collections do not compact the large object heap (LOH).</summary>
	Default = 1,
	/// <summary>The large object heap (LOH) will be compacted during the next blocking garbage collection. </summary>
	CompactOnce
}
