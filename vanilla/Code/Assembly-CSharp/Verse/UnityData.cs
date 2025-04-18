using System;
using System.Threading;
using UnityEngine;

namespace Verse;

public static class UnityData
{
	private static bool initialized;

	public static bool isEditor;

	public static string dataPath;

	public static RuntimePlatform platform;

	public static string persistentDataPath;

	private static int mainThreadId;

	private static int workerThreads;

	public static bool IsInMainThread => mainThreadId == Thread.CurrentThread.ManagedThreadId;

	public static bool Is32BitBuild => IntPtr.Size == 4;

	public static bool Is64BitBuild => IntPtr.Size == 8;

	public static int GetIdealBatchCount(int items)
	{
		return Mathf.Max(items / workerThreads, 4);
	}

	static UnityData()
	{
		if (!initialized && !UnityDataInitializer.initializing)
		{
			Log.Warning("Used UnityData before it's initialized.");
		}
	}

	public static void CopyUnityData()
	{
		mainThreadId = Thread.CurrentThread.ManagedThreadId;
		isEditor = Application.isEditor;
		dataPath = Application.dataPath;
		platform = Application.platform;
		persistentDataPath = Application.persistentDataPath;
		workerThreads = Environment.ProcessorCount;
		initialized = true;
	}
}
