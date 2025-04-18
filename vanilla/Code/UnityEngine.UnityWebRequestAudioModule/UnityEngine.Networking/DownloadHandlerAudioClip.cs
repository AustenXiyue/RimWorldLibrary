using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;

namespace UnityEngine.Networking;

[StructLayout(LayoutKind.Sequential)]
[NativeHeader("Modules/UnityWebRequestAudio/Public/DownloadHandlerAudioClip.h")]
public sealed class DownloadHandlerAudioClip : DownloadHandler
{
	[NativeThrows]
	public extern AudioClip audioClip
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern bool streamAudio
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool compressed
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr Create(DownloadHandlerAudioClip obj, string url, AudioType audioType);

	private void InternalCreateAudioClip(string url, AudioType audioType)
	{
		m_Ptr = Create(this, url, audioType);
	}

	public DownloadHandlerAudioClip(string url, AudioType audioType)
	{
		InternalCreateAudioClip(url, audioType);
	}

	public DownloadHandlerAudioClip(Uri uri, AudioType audioType)
	{
		InternalCreateAudioClip(uri.AbsoluteUri, audioType);
	}

	protected override byte[] GetData()
	{
		return DownloadHandler.InternalGetByteArray(this);
	}

	protected override string GetText()
	{
		throw new NotSupportedException("String access is not supported for audio clips");
	}

	public static AudioClip GetContent(UnityWebRequest www)
	{
		return DownloadHandler.GetCheckedDownloader<DownloadHandlerAudioClip>(www).audioClip;
	}
}
