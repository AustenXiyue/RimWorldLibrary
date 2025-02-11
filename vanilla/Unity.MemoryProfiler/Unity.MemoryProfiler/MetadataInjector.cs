using UnityEngine;

namespace Unity.MemoryProfiler;

internal static class MetadataInjector
{
	public static DefaultMetadataCollect DefaultCollector;

	public static long CollectorCount;

	public static byte DefaultCollectorInjected;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void PlayerInitMetadata()
	{
		InitializeMetadataCollection();
	}

	private static void InitializeMetadataCollection()
	{
		DefaultCollector = new DefaultMetadataCollect();
	}
}
