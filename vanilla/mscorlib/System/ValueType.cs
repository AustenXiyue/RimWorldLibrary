using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System;

/// <summary>Provides the base class for value types.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public abstract class ValueType
{
	private static class Internal
	{
		public static int hash_code_of_ptr_seed;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ValueType" /> class. </summary>
	protected ValueType()
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool InternalEquals(object o1, object o2, out object[] fields);

	internal static bool DefaultEquals(object o1, object o2)
	{
		if (o1 == null && o2 == null)
		{
			return true;
		}
		if (o1 == null || o2 == null)
		{
			return false;
		}
		RuntimeType obj = (RuntimeType)o1.GetType();
		RuntimeType runtimeType = (RuntimeType)o2.GetType();
		if (obj != runtimeType)
		{
			return false;
		}
		object[] fields;
		bool result = InternalEquals(o1, o2, out fields);
		if (fields == null)
		{
			return result;
		}
		for (int i = 0; i < fields.Length; i += 2)
		{
			object obj2 = fields[i];
			object obj3 = fields[i + 1];
			if (obj2 == null)
			{
				if (obj3 != null)
				{
					return false;
				}
			}
			else if (!obj2.Equals(obj3))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Indicates whether this instance and a specified object are equal.</summary>
	/// <returns>true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false. </returns>
	/// <param name="obj">The object to compare with the current instance. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object obj)
	{
		return DefaultEquals(this, obj);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern int InternalGetHashCode(object o, out object[] fields);

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		object[] fields;
		int num = InternalGetHashCode(this, out fields);
		if (fields != null)
		{
			for (int i = 0; i < fields.Length; i++)
			{
				if (fields[i] != null)
				{
					num ^= fields[i].GetHashCode();
				}
			}
		}
		return num;
	}

	internal static int GetHashCodeOfPtr(IntPtr ptr)
	{
		int num = (int)ptr;
		int hash_code_of_ptr_seed = Internal.hash_code_of_ptr_seed;
		if (hash_code_of_ptr_seed == 0)
		{
			hash_code_of_ptr_seed = num;
			Interlocked.CompareExchange(ref Internal.hash_code_of_ptr_seed, hash_code_of_ptr_seed, 0);
			hash_code_of_ptr_seed = Internal.hash_code_of_ptr_seed;
		}
		return num - hash_code_of_ptr_seed;
	}

	/// <summary>Returns the fully qualified type name of this instance.</summary>
	/// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		return GetType().FullName;
	}
}
