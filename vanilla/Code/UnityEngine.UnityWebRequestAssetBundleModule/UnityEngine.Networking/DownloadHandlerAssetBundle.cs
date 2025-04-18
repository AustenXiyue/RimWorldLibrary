using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;

namespace UnityEngine.Networking;

[StructLayout(LayoutKind.Sequential)]
[NativeHeader("Modules/UnityWebRequestAssetBundle/Public/DownloadHandlerAssetBundle.h")]
public sealed class DownloadHandlerAssetBundle : DownloadHandler
{
	public extern AssetBundle assetBundle
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr Create(DownloadHandlerAssetBundle obj, string url, uint crc);

	private static IntPtr CreateCached(DownloadHandlerAssetBundle obj, string url, string name, Hash128 hash, uint crc)
	{
		return CreateCached_Injected(obj, url, name, ref hash, crc);
	}

	private void InternalCreateAssetBundle(string url, uint crc)
	{
		m_Ptr = Create(this, url, crc);
	}

	private void InternalCreateAssetBundleCached(string url, string name, Hash128 hash, uint crc)
	{
		m_Ptr = CreateCached(this, url, name, hash, crc);
	}

	public DownloadHandlerAssetBundle(string url, uint crc)
	{
		InternalCreateAssetBundle(url, crc);
	}

	public DownloadHandlerAssetBundle(string url, uint version, uint crc)
	{
		InternalCreateAssetBundleCached(url, "", new Hash128(0u, 0u, 0u, version), crc);
	}

	public DownloadHandlerAssetBundle(string url, Hash128 hash, uint crc)
	{
		InternalCreateAssetBundleCached(url, "", hash, crc);
	}

	public DownloadHandlerAssetBundle(string url, string name, Hash128 hash, uint crc)
	{
		InternalCreateAssetBundleCached(url, name, hash, crc);
	}

	public DownloadHandlerAssetBundle(string url, CachedAssetBundle cachedBundle, uint crc)
	{
		InternalCreateAssetBundleCached(url, cachedBundle.name, cachedBundle.hash, crc);
	}

	protected override byte[] GetData()
	{
		throw new NotSupportedException("Raw data access is not supported for asset bundles");
	}

	protected override string GetText()
	{
		throw new NotSupportedException("String access is not supported for asset bundles");
	}

	public static AssetBundle GetContent(UnityWebRequest www)
	{
		return DownloadHandler.GetCheckedDownloader<DownloadHandlerAssetBundle>(www).assetBundle;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr CreateCached_Injected(DownloadHandlerAssetBundle obj, string url, string name, ref Hash128 hash, uint crc);
}
