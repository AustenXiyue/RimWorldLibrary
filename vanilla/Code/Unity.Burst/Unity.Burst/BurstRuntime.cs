using System;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using UnityEngine;

namespace Unity.Burst;

public static class BurstRuntime
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct HashCode32<T>
	{
		public static readonly int Value = HashStringWithFNV1A32(typeof(T).AssemblyQualifiedName);
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct HashCode64<T>
	{
		public static readonly long Value = HashStringWithFNV1A64(typeof(T).AssemblyQualifiedName);
	}

	private unsafe delegate void NativeLogDelegate(byte* message, int logType, byte* filename, int lineNumber);

	private class LogHelper
	{
		public static readonly SharedStatic<FunctionPointer<NativeLogDelegate>> Instance = SharedStatic<FunctionPointer<NativeLogDelegate>>.GetOrCreate<LogHelper>();
	}

	private static readonly NativeLogDelegate ManagedNativeLog;

	public static int GetHashCode32<T>()
	{
		return HashCode32<T>.Value;
	}

	public static int GetHashCode32(Type type)
	{
		return HashStringWithFNV1A32(type.AssemblyQualifiedName);
	}

	public static long GetHashCode64<T>()
	{
		return HashCode64<T>.Value;
	}

	public static long GetHashCode64(Type type)
	{
		return HashStringWithFNV1A64(type.AssemblyQualifiedName);
	}

	internal static int HashStringWithFNV1A32(string text)
	{
		uint num = 2166136261u;
		foreach (char c in text)
		{
			num = 16777619 * (num ^ (byte)(c & 0xFF));
			num = 16777619 * (num ^ (byte)((int)c >> 8));
		}
		return (int)num;
	}

	internal static long HashStringWithFNV1A64(string text)
	{
		ulong num = 14695981039346656037uL;
		foreach (char c in text)
		{
			num = 1099511628211L * (num ^ (byte)(c & 0xFF));
			num = 1099511628211L * (num ^ (byte)((int)c >> 8));
		}
		return (long)num;
	}

	internal unsafe static void Log(byte* message, int logType, byte* fileName, int lineNumber)
	{
		FunctionPointer<NativeLogDelegate> data = LogHelper.Instance.Data;
		if (data.IsCreated)
		{
			data.Invoke(message, logType, fileName, lineNumber);
		}
	}

	[MonoPInvokeCallback(typeof(NativeLogDelegate))]
	private unsafe static void ManagedNativeLogImpl(byte* message, int logType, byte* filename, int lineNumber)
	{
		if (message != null)
		{
			int i;
			for (i = 0; message[i] != 0; i++)
			{
			}
			string @string = Encoding.UTF8.GetString(message, i);
			switch (logType)
			{
			case 1:
				Debug.LogWarning(@string);
				break;
			case 2:
				Debug.LogError(@string);
				break;
			default:
				Debug.Log(@string);
				break;
			}
		}
	}

	unsafe static BurstRuntime()
	{
		ManagedNativeLog = ManagedNativeLogImpl;
		LogHelper.Instance.Data = new FunctionPointer<NativeLogDelegate>(Marshal.GetFunctionPointerForDelegate(ManagedNativeLog));
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	internal static void Initialize()
	{
	}
}
