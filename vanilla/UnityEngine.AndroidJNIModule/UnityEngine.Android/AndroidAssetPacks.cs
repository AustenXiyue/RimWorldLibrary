using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Android;

[StaticAccessor("AndroidAssetPacksBindingsHelpers", StaticAccessorType.DoubleColon)]
[NativeHeader("Modules/AndroidJNI/Public/AndroidAssetPacksBindingsHelpers.h")]
public static class AndroidAssetPacks
{
	public static bool coreUnityAssetPacksDownloaded => CoreUnityAssetPacksDownloaded();

	internal static string dataPackName => GetDataPackName();

	internal static string streamingAssetsPackName => GetStreamingAssetsPackName();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeConditional("PLATFORM_ANDROID")]
	private static extern bool CoreUnityAssetPacksDownloaded();

	public static string[] GetCoreUnityAssetPackNames()
	{
		return new string[0];
	}

	public static void GetAssetPackStateAsync(string[] assetPackNames, Action<ulong, AndroidAssetPackState[]> callback)
	{
	}

	public static GetAssetPackStateAsyncOperation GetAssetPackStateAsync(string[] assetPackNames)
	{
		return null;
	}

	public static void DownloadAssetPackAsync(string[] assetPackNames, Action<AndroidAssetPackInfo> callback)
	{
	}

	public static DownloadAssetPackAsyncOperation DownloadAssetPackAsync(string[] assetPackNames)
	{
		return null;
	}

	public static void RequestToUseMobileDataAsync(Action<AndroidAssetPackUseMobileDataRequestResult> callback)
	{
	}

	public static RequestToUseMobileDataAsyncOperation RequestToUseMobileDataAsync()
	{
		return null;
	}

	public static string GetAssetPackPath(string assetPackName)
	{
		return "";
	}

	public static void CancelAssetPackDownload(string[] assetPackNames)
	{
	}

	public static void RemoveAssetPack(string assetPackName)
	{
	}

	private static string GetDataPackName()
	{
		return "UnityDataAssetPack";
	}

	private static string GetStreamingAssetsPackName()
	{
		return "UnityStreamingAssetsPack";
	}
}
