using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>Represents a multicast delegate; that is, a delegate that can have more than one element in its invocation list.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
public abstract class MulticastDelegate : Delegate
{
	private Delegate[] delegates;

	internal bool HasSingleTarget => delegates == null;

	/// <summary>Initializes a new instance of the <see cref="T:System.MulticastDelegate" /> class.</summary>
	/// <param name="target">The object on which <paramref name="method" /> is defined. </param>
	/// <param name="method">The name of the method for which a delegate is created. </param>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	protected MulticastDelegate(object target, string method)
		: base(target, method)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.MulticastDelegate" /> class.</summary>
	/// <param name="target">The type of object on which <paramref name="method" /> is defined. </param>
	/// <param name="method">The name of the static method for which a delegate is created. </param>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	protected MulticastDelegate(Type target, string method)
		: base(target, method)
	{
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with all the data needed to serialize this instance.</summary>
	/// <param name="info">An object that holds all the data needed to serialize or deserialize this instance. </param>
	/// <param name="context">(Reserved) The location where serialized data is stored and retrieved. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">A serialization error occurred.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	[SecurityCritical]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
	}

	protected sealed override object DynamicInvokeImpl(object[] args)
	{
		if (delegates == null)
		{
			return base.DynamicInvokeImpl(args);
		}
		int num = 0;
		int num2 = delegates.Length;
		object result;
		do
		{
			result = delegates[num].DynamicInvoke(args);
		}
		while (++num < num2);
		return result;
	}

	/// <summary>Determines whether this multicast delegate and the specified object are equal.</summary>
	/// <returns>true if <paramref name="obj" /> and this instance have the same invocation lists; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this instance. </param>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <filterpriority>2</filterpriority>
	public sealed override bool Equals(object obj)
	{
		if (!base.Equals(obj))
		{
			return false;
		}
		if (!(obj is MulticastDelegate multicastDelegate))
		{
			return false;
		}
		if (delegates == null && multicastDelegate.delegates == null)
		{
			return true;
		}
		if ((delegates == null) ^ (multicastDelegate.delegates == null))
		{
			return false;
		}
		if (delegates.Length != multicastDelegate.delegates.Length)
		{
			return false;
		}
		for (int i = 0; i < delegates.Length; i++)
		{
			if (!delegates[i].Equals(multicastDelegate.delegates[i]))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <filterpriority>2</filterpriority>
	public sealed override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Returns a static method represented by the current <see cref="T:System.MulticastDelegate" />.</summary>
	/// <returns>A static method represented by the current <see cref="T:System.MulticastDelegate" />.</returns>
	protected override MethodInfo GetMethodImpl()
	{
		if (delegates != null)
		{
			return delegates[delegates.Length - 1].Method;
		}
		return base.GetMethodImpl();
	}

	/// <summary>Returns the invocation list of this multicast delegate, in invocation order.</summary>
	/// <returns>An array of delegates whose invocation lists collectively match the invocation list of this instance.</returns>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <filterpriority>2</filterpriority>
	public sealed override Delegate[] GetInvocationList()
	{
		if (delegates != null)
		{
			return (Delegate[])delegates.Clone();
		}
		return new Delegate[1] { this };
	}

	/// <summary>Combines this <see cref="T:System.Delegate" /> with the specified <see cref="T:System.Delegate" /> to form a new delegate.</summary>
	/// <returns>A delegate that is the new root of the <see cref="T:System.MulticastDelegate" /> invocation list.</returns>
	/// <param name="follow">The delegate to combine with this delegate. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="follow" /> does not have the same type as this instance.</exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	protected sealed override Delegate CombineImpl(Delegate follow)
	{
		if ((object)follow == null)
		{
			return this;
		}
		MulticastDelegate multicastDelegate = (MulticastDelegate)follow;
		MulticastDelegate multicastDelegate2 = Delegate.AllocDelegateLike_internal(this);
		if (delegates == null && multicastDelegate.delegates == null)
		{
			multicastDelegate2.delegates = new Delegate[2] { this, multicastDelegate };
		}
		else if (delegates == null)
		{
			multicastDelegate2.delegates = new Delegate[1 + multicastDelegate.delegates.Length];
			multicastDelegate2.delegates[0] = this;
			Array.Copy(multicastDelegate.delegates, 0, multicastDelegate2.delegates, 1, multicastDelegate.delegates.Length);
		}
		else if (multicastDelegate.delegates == null)
		{
			multicastDelegate2.delegates = new Delegate[delegates.Length + 1];
			Array.Copy(delegates, 0, multicastDelegate2.delegates, 0, delegates.Length);
			multicastDelegate2.delegates[multicastDelegate2.delegates.Length - 1] = multicastDelegate;
		}
		else
		{
			multicastDelegate2.delegates = new Delegate[delegates.Length + multicastDelegate.delegates.Length];
			Array.Copy(delegates, 0, multicastDelegate2.delegates, 0, delegates.Length);
			Array.Copy(multicastDelegate.delegates, 0, multicastDelegate2.delegates, delegates.Length, multicastDelegate.delegates.Length);
		}
		return multicastDelegate2;
	}

	private int LastIndexOf(Delegate[] haystack, Delegate[] needle)
	{
		if (haystack.Length < needle.Length)
		{
			return -1;
		}
		if (haystack.Length == needle.Length)
		{
			for (int i = 0; i < haystack.Length; i++)
			{
				if (!haystack[i].Equals(needle[i]))
				{
					return -1;
				}
			}
			return 0;
		}
		int num = haystack.Length - needle.Length;
		while (num >= 0)
		{
			int j;
			for (j = 0; needle[j].Equals(haystack[num]); j++)
			{
				if (j == needle.Length - 1)
				{
					return num - j;
				}
				num++;
			}
			num -= j + 1;
		}
		return -1;
	}

	/// <summary>Removes an element from the invocation list of this <see cref="T:System.MulticastDelegate" /> that is equal to the specified delegate.</summary>
	/// <returns>If <paramref name="value" /> is found in the invocation list for this instance, then a new <see cref="T:System.Delegate" /> without <paramref name="value" /> in its invocation list; otherwise, this instance with its original invocation list.</returns>
	/// <param name="value">The delegate to search for in the invocation list. </param>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	protected sealed override Delegate RemoveImpl(Delegate value)
	{
		if ((object)value == null)
		{
			return this;
		}
		MulticastDelegate multicastDelegate = (MulticastDelegate)value;
		if (delegates == null && multicastDelegate.delegates == null)
		{
			if (!Equals(multicastDelegate))
			{
				return this;
			}
			return null;
		}
		if (delegates == null)
		{
			Delegate[] array = multicastDelegate.delegates;
			foreach (Delegate obj in array)
			{
				if (Equals(obj))
				{
					return null;
				}
			}
			return this;
		}
		if (multicastDelegate.delegates == null)
		{
			int num = Array.LastIndexOf(delegates, multicastDelegate);
			if (num == -1)
			{
				return this;
			}
			if (delegates.Length <= 1)
			{
				throw new InvalidOperationException();
			}
			if (delegates.Length == 2)
			{
				return delegates[(num == 0) ? 1u : 0u];
			}
			MulticastDelegate multicastDelegate2 = Delegate.AllocDelegateLike_internal(this);
			multicastDelegate2.delegates = new Delegate[delegates.Length - 1];
			Array.Copy(delegates, multicastDelegate2.delegates, num);
			Array.Copy(delegates, num + 1, multicastDelegate2.delegates, num, delegates.Length - num - 1);
			return multicastDelegate2;
		}
		if (delegates.Equals(multicastDelegate.delegates))
		{
			return null;
		}
		int num2 = LastIndexOf(delegates, multicastDelegate.delegates);
		if (num2 == -1)
		{
			return this;
		}
		MulticastDelegate multicastDelegate3 = Delegate.AllocDelegateLike_internal(this);
		multicastDelegate3.delegates = new Delegate[delegates.Length - multicastDelegate.delegates.Length];
		Array.Copy(delegates, multicastDelegate3.delegates, num2);
		Array.Copy(delegates, num2 + multicastDelegate.delegates.Length, multicastDelegate3.delegates, num2, delegates.Length - num2 - multicastDelegate.delegates.Length);
		return multicastDelegate3;
	}

	/// <summary>Determines whether two <see cref="T:System.MulticastDelegate" /> objects are equal.</summary>
	/// <returns>true if <paramref name="d1" /> and <paramref name="d2" /> have the same invocation lists; otherwise, false.</returns>
	/// <param name="d1">The left operand. </param>
	/// <param name="d2">The right operand. </param>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(MulticastDelegate d1, MulticastDelegate d2)
	{
		return d1?.Equals(d2) ?? ((object)d2 == null);
	}

	/// <summary>Determines whether two <see cref="T:System.MulticastDelegate" /> objects are not equal.</summary>
	/// <returns>true if <paramref name="d1" /> and <paramref name="d2" /> do not have the same invocation lists; otherwise, false.</returns>
	/// <param name="d1">The left operand. </param>
	/// <param name="d2">The right operand. </param>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(MulticastDelegate d1, MulticastDelegate d2)
	{
		if ((object)d1 == null)
		{
			return (object)d2 != null;
		}
		return !d1.Equals(d2);
	}
}
