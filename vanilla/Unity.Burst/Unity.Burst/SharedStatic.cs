using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Burst.LowLevel;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Unity.Burst;

public readonly struct SharedStatic<T> where T : struct
{
	private unsafe readonly void* _buffer;

	public unsafe ref T Data => ref Unsafe.AsRef<T>(_buffer);

	public unsafe void* UnsafeDataPointer => _buffer;

	private unsafe SharedStatic(void* buffer)
	{
		_buffer = buffer;
	}

	public unsafe static SharedStatic<T> GetOrCreate<TContext>(uint alignment = 0u)
	{
		return new SharedStatic<T>(SharedStatic.GetOrCreateSharedStaticInternal(BurstRuntime.GetHashCode64<TContext>(), 0L, (uint)UnsafeUtility.SizeOf<T>(), (alignment == 0) ? 4u : alignment));
	}

	public unsafe static SharedStatic<T> GetOrCreate<TContext, TSubContext>(uint alignment = 0u)
	{
		return new SharedStatic<T>(SharedStatic.GetOrCreateSharedStaticInternal(BurstRuntime.GetHashCode64<TContext>(), BurstRuntime.GetHashCode64<TSubContext>(), (uint)UnsafeUtility.SizeOf<T>(), (alignment == 0) ? 4u : alignment));
	}

	public unsafe static SharedStatic<T> GetOrCreate(Type contextType, uint alignment = 0u)
	{
		return new SharedStatic<T>(SharedStatic.GetOrCreateSharedStaticInternal(BurstRuntime.GetHashCode64(contextType), 0L, (uint)UnsafeUtility.SizeOf<T>(), (alignment == 0) ? 4u : alignment));
	}

	public unsafe static SharedStatic<T> GetOrCreate(Type contextType, Type subContextType, uint alignment = 0u)
	{
		return new SharedStatic<T>(SharedStatic.GetOrCreateSharedStaticInternal(BurstRuntime.GetHashCode64(contextType), BurstRuntime.GetHashCode64(subContextType), (uint)UnsafeUtility.SizeOf<T>(), (alignment == 0) ? 4u : alignment));
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private static void CheckIf_T_IsUnmanagedOrThrow()
	{
		if (!UnsafeUtility.IsUnmanaged<T>())
		{
			throw new InvalidOperationException($"The type {typeof(T)} used in SharedStatic<{typeof(T)}> must be unmanaged (contain no managed types).");
		}
	}
}
internal static class SharedStatic
{
	private static readonly Dictionary<long, Type> HashToType = new Dictionary<long, Type>();

	public unsafe static void* GetOrCreateSharedStaticInternal(Type typeContext, Type subTypeContext, uint sizeOf, uint alignment)
	{
		return GetOrCreateSharedStaticInternal(GetSafeHashCode64(typeContext), GetSafeHashCode64(subTypeContext), sizeOf, alignment);
	}

	private static long GetSafeHashCode64(Type type)
	{
		long hashCode = BurstRuntime.GetHashCode64(type);
		lock (HashToType)
		{
			if (HashToType.TryGetValue(hashCode, out var value))
			{
				if (value != type)
				{
					string message = $"The type `{type}` has a hash conflict with `{value}`";
					Debug.LogError(message);
					throw new InvalidOperationException(message);
				}
			}
			else
			{
				HashToType.Add(hashCode, type);
			}
		}
		return hashCode;
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private static void CheckSizeOf(uint sizeOf)
	{
		if (sizeOf == 0)
		{
			throw new ArgumentException("sizeOf must be > 0", "sizeOf");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private unsafe static void CheckResult(void* result)
	{
		if (result == null)
		{
			throw new InvalidOperationException("Unable to create a SharedStatic for this key. It is likely that the same key was used to allocate a shared memory with a smaller size while the new size requested is bigger");
		}
	}

	public unsafe static void* GetOrCreateSharedStaticInternal(long getHashCode64, long getSubHashCode64, uint sizeOf, uint alignment)
	{
		Hash128 key = new Hash128((ulong)getHashCode64, (ulong)getSubHashCode64);
		return BurstCompilerService.GetOrCreateSharedMemory(ref key, sizeOf, alignment);
	}
}
