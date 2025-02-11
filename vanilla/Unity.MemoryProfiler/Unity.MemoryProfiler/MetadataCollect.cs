using System;
using UnityEngine.Profiling.Memory.Experimental;

namespace Unity.MemoryProfiler;

public abstract class MetadataCollect : IDisposable
{
	private bool disposed;

	public MetadataCollect()
	{
		if (MetadataInjector.DefaultCollector != null && MetadataInjector.DefaultCollector != this && MetadataInjector.DefaultCollectorInjected != 0)
		{
			UnityEngine.Profiling.Memory.Experimental.MemoryProfiler.createMetaData -= MetadataInjector.DefaultCollector.CollectMetadata;
			MetadataInjector.CollectorCount--;
			MetadataInjector.DefaultCollectorInjected = 0;
		}
		UnityEngine.Profiling.Memory.Experimental.MemoryProfiler.createMetaData += CollectMetadata;
		MetadataInjector.CollectorCount++;
	}

	public abstract void CollectMetadata(MetaData data);

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			UnityEngine.Profiling.Memory.Experimental.MemoryProfiler.createMetaData -= CollectMetadata;
			MetadataInjector.CollectorCount--;
			if (MetadataInjector.DefaultCollector != null && MetadataInjector.CollectorCount < 1 && MetadataInjector.DefaultCollector != this)
			{
				MetadataInjector.DefaultCollectorInjected = 1;
				UnityEngine.Profiling.Memory.Experimental.MemoryProfiler.createMetaData += MetadataInjector.DefaultCollector.CollectMetadata;
				MetadataInjector.CollectorCount++;
			}
		}
	}
}
