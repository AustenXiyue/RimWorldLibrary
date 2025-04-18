using System.Diagnostics;
using System.Threading;

namespace System.Buffers;

internal sealed class ConfigurableArrayPool<T> : ArrayPool<T>
{
	private sealed class Bucket
	{
		internal readonly int _bufferLength;

		private readonly T[]?[] _buffers;

		private readonly int _poolId;

		private SpinLock _lock;

		private int _index;

		internal int Id => GetHashCode();

		internal Bucket(int bufferLength, int numberOfBuffers, int poolId)
		{
			_lock = new SpinLock(Debugger.IsAttached);
			_buffers = new T[numberOfBuffers][];
			_bufferLength = bufferLength;
			_poolId = poolId;
		}

		internal T[]? Rent()
		{
			T[][] buffers = _buffers;
			T[] array = null;
			bool lockTaken = false;
			bool flag = false;
			try
			{
				_lock.Enter(ref lockTaken);
				if (_index < buffers.Length)
				{
					array = buffers[_index];
					buffers[_index++] = null;
					flag = array == null;
				}
			}
			finally
			{
				if (lockTaken)
				{
					_lock.Exit(useMemoryBarrier: false);
				}
			}
			if (flag)
			{
				array = new T[_bufferLength];
			}
			return array;
		}

		internal void Return(T[] array)
		{
			if (array.Length != _bufferLength)
			{
				throw new ArgumentException("Buffer not from this pool", "array");
			}
			bool lockTaken = false;
			try
			{
				_lock.Enter(ref lockTaken);
				if (_index != 0)
				{
					_buffers[--_index] = array;
				}
			}
			finally
			{
				if (lockTaken)
				{
					_lock.Exit(useMemoryBarrier: false);
				}
			}
		}
	}

	private const int DefaultMaxArrayLength = 1048576;

	private const int DefaultMaxNumberOfArraysPerBucket = 50;

	private readonly Bucket[] _buckets;

	private int Id => GetHashCode();

	internal ConfigurableArrayPool()
		: this(1048576, 50)
	{
	}

	internal ConfigurableArrayPool(int maxArrayLength, int maxArraysPerBucket)
	{
		if (maxArrayLength <= 0)
		{
			throw new ArgumentOutOfRangeException("maxArrayLength");
		}
		if (maxArraysPerBucket <= 0)
		{
			throw new ArgumentOutOfRangeException("maxArraysPerBucket");
		}
		if (maxArrayLength > 1073741824)
		{
			maxArrayLength = 1073741824;
		}
		else if (maxArrayLength < 16)
		{
			maxArrayLength = 16;
		}
		int id = Id;
		Bucket[] array = new Bucket[Utilities.SelectBucketIndex(maxArrayLength) + 1];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Bucket(Utilities.GetMaxSizeForBucket(i), maxArraysPerBucket, id);
		}
		_buckets = array;
	}

	public override T[] Rent(int minimumLength)
	{
		if (minimumLength < 0)
		{
			throw new ArgumentOutOfRangeException("minimumLength");
		}
		if (minimumLength == 0)
		{
			return ArrayEx.Empty<T>();
		}
		int num = Utilities.SelectBucketIndex(minimumLength);
		if (num < _buckets.Length)
		{
			int num2 = num;
			do
			{
				T[] array = _buckets[num2].Rent();
				if (array != null)
				{
					return array;
				}
			}
			while (++num2 < _buckets.Length && num2 != num + 2);
			return new T[_buckets[num]._bufferLength];
		}
		return new T[minimumLength];
	}

	public override void Return(T[] array, bool clearArray = false)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Length == 0)
		{
			return;
		}
		int num = Utilities.SelectBucketIndex(array.Length);
		if (num < _buckets.Length)
		{
			if (clearArray)
			{
				Array.Clear(array, 0, array.Length);
			}
			_buckets[num].Return(array);
		}
	}
}
