using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Runtime.InteropServices;

/// <summary>Provides a way to access a managed object from unmanaged memory.</summary>
[MonoTODO("Struct should be [StructLayout(LayoutKind.Sequential)] but will need to be reordered for that.")]
[ComVisible(true)]
public struct GCHandle
{
	private int handle;

	/// <summary>Gets a value indicating whether the handle is allocated.</summary>
	/// <returns>true if the handle is allocated; otherwise, false.</returns>
	public bool IsAllocated => handle != 0;

	/// <summary>Gets or sets the object this handle represents.</summary>
	/// <returns>The object this handle represents.</returns>
	/// <exception cref="T:System.InvalidOperationException">The handle was freed, or never initialized. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public object Target
	{
		get
		{
			if (!IsAllocated)
			{
				throw new InvalidOperationException(Locale.GetText("Handle is not allocated"));
			}
			return GetTarget(handle);
		}
		set
		{
			handle = GetTargetHandle(value, handle, (GCHandleType)(-1));
		}
	}

	private GCHandle(IntPtr h)
	{
		handle = (int)h;
	}

	private GCHandle(object obj)
		: this(obj, GCHandleType.Normal)
	{
	}

	internal GCHandle(object value, GCHandleType type)
	{
		if (type < GCHandleType.Weak || type > GCHandleType.Pinned)
		{
			type = GCHandleType.Normal;
		}
		handle = GetTargetHandle(value, 0, type);
	}

	/// <summary>Retrieves the address of an object in a <see cref="F:System.Runtime.InteropServices.GCHandleType.Pinned" /> handle.</summary>
	/// <returns>The address of the of the Pinned object as an <see cref="T:System.IntPtr" />.</returns>
	/// <exception cref="T:System.InvalidOperationException">The handle is any type other than <see cref="F:System.Runtime.InteropServices.GCHandleType.Pinned" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public IntPtr AddrOfPinnedObject()
	{
		IntPtr addrOfPinnedObject = GetAddrOfPinnedObject(handle);
		if (addrOfPinnedObject == (IntPtr)(-1))
		{
			throw new ArgumentException("Object contains non-primitive or non-blittable data.");
		}
		if (addrOfPinnedObject == (IntPtr)(-2))
		{
			throw new InvalidOperationException("Handle is not pinned.");
		}
		return addrOfPinnedObject;
	}

	/// <summary>Allocates a <see cref="F:System.Runtime.InteropServices.GCHandleType.Normal" /> handle for the specified object.</summary>
	/// <returns>A new <see cref="T:System.Runtime.InteropServices.GCHandle" /> that protects the object from garbage collection. This <see cref="T:System.Runtime.InteropServices.GCHandle" /> must be released with <see cref="M:System.Runtime.InteropServices.GCHandle.Free" /> when it is no longer needed.</returns>
	/// <param name="value">The object that uses the <see cref="T:System.Runtime.InteropServices.GCHandle" />. </param>
	/// <exception cref="T:System.ArgumentException">An instance with nonprimitive (non-blittable) members cannot be pinned. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static GCHandle Alloc(object value)
	{
		return new GCHandle(value);
	}

	/// <summary>Allocates a handle of the specified type for the specified object.</summary>
	/// <returns>A new <see cref="T:System.Runtime.InteropServices.GCHandle" /> of the specified type. This <see cref="T:System.Runtime.InteropServices.GCHandle" /> must be released with <see cref="M:System.Runtime.InteropServices.GCHandle.Free" /> when it is no longer needed.</returns>
	/// <param name="value">The object that uses the <see cref="T:System.Runtime.InteropServices.GCHandle" />. </param>
	/// <param name="type">One of the <see cref="T:System.Runtime.InteropServices.GCHandleType" /> values, indicating the type of <see cref="T:System.Runtime.InteropServices.GCHandle" /> to create. </param>
	/// <exception cref="T:System.ArgumentException">An instance with nonprimitive (non-blittable) members cannot be pinned. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static GCHandle Alloc(object value, GCHandleType type)
	{
		return new GCHandle(value, type);
	}

	/// <summary>Releases a <see cref="T:System.Runtime.InteropServices.GCHandle" />.</summary>
	/// <exception cref="T:System.InvalidOperationException">The handle was freed or never initialized. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public void Free()
	{
		int num = handle;
		if (num != 0 && Interlocked.CompareExchange(ref handle, 0, num) == num)
		{
			FreeHandle(num);
			return;
		}
		throw new InvalidOperationException("Handle is not initialized.");
	}

	/// <summary>A <see cref="T:System.Runtime.InteropServices.GCHandle" /> is stored using an internal integer representation.</summary>
	/// <returns>The integer value.</returns>
	/// <param name="value">The <see cref="T:System.Runtime.InteropServices.GCHandle" /> for which the integer is required. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static explicit operator IntPtr(GCHandle value)
	{
		return (IntPtr)value.handle;
	}

	/// <summary>A <see cref="T:System.Runtime.InteropServices.GCHandle" /> is stored using an internal integer representation.</summary>
	/// <returns>The stored <see cref="T:System.Runtime.InteropServices.GCHandle" /> object using an internal integer representation.</returns>
	/// <param name="value">An <see cref="T:System.IntPtr" /> that indicates the handle for which the conversion is required. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static explicit operator GCHandle(IntPtr value)
	{
		if (value == IntPtr.Zero)
		{
			throw new InvalidOperationException("GCHandle value cannot be zero");
		}
		if (!CheckCurrentDomain((int)value))
		{
			throw new ArgumentException("GCHandle value belongs to a different domain");
		}
		return new GCHandle(value);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool CheckCurrentDomain(int handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern object GetTarget(int handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int GetTargetHandle(object obj, int handle, GCHandleType type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void FreeHandle(int handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr GetAddrOfPinnedObject(int handle);

	/// <summary>Returns a value indicating whether two <see cref="T:System.Runtime.InteropServices.GCHandle" /> objects are equal.</summary>
	/// <returns>true if the <paramref name="a" /> and <paramref name="b" /> parameters are equal; otherwise, false.</returns>
	/// <param name="a">A <see cref="T:System.Runtime.InteropServices.GCHandle" /> object to compare with the <paramref name="b" /> parameter. </param>
	/// <param name="b">A <see cref="T:System.Runtime.InteropServices.GCHandle" /> object to compare with the <paramref name="a" /> parameter.  </param>
	public static bool operator ==(GCHandle a, GCHandle b)
	{
		return a.handle == b.handle;
	}

	/// <summary>Returns a value indicating whether two <see cref="T:System.Runtime.InteropServices.GCHandle" /> objects are not equal.</summary>
	/// <returns>true if the <paramref name="a" /> and <paramref name="b" /> parameters are not equal; otherwise, false.</returns>
	/// <param name="a">A <see cref="T:System.Runtime.InteropServices.GCHandle" /> object to compare with the <paramref name="b" /> parameter. </param>
	/// <param name="b">A <see cref="T:System.Runtime.InteropServices.GCHandle" /> object to compare with the <paramref name="a" /> parameter.  </param>
	public static bool operator !=(GCHandle a, GCHandle b)
	{
		return !(a == b);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Runtime.InteropServices.GCHandle" /> object is equal to the current <see cref="T:System.Runtime.InteropServices.GCHandle" /> object.</summary>
	/// <returns>true if the specified <see cref="T:System.Runtime.InteropServices.GCHandle" /> object is equal to the current <see cref="T:System.Runtime.InteropServices.GCHandle" /> object; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.Runtime.InteropServices.GCHandle" /> object to compare with the current <see cref="T:System.Runtime.InteropServices.GCHandle" /> object.</param>
	public override bool Equals(object o)
	{
		if (!(o is GCHandle))
		{
			return false;
		}
		return this == (GCHandle)o;
	}

	/// <summary>Returns an identifier for the current <see cref="T:System.Runtime.InteropServices.GCHandle" /> object.</summary>
	/// <returns>An identifier for the current <see cref="T:System.Runtime.InteropServices.GCHandle" /> object.</returns>
	public override int GetHashCode()
	{
		return handle.GetHashCode();
	}

	/// <summary>Returns a new <see cref="T:System.Runtime.InteropServices.GCHandle" /> object created from a handle to a managed object.</summary>
	/// <returns>A new <see cref="T:System.Runtime.InteropServices.GCHandle" /> object that corresponds to the value parameter.  </returns>
	/// <param name="value">An <see cref="T:System.IntPtr" /> handle to a managed object to create a <see cref="T:System.Runtime.InteropServices.GCHandle" /> object from.</param>
	/// <exception cref="T:System.InvalidOperationException">The value of the <paramref name="value" /> parameter is <see cref="F:System.IntPtr.Zero" />.</exception>
	public static GCHandle FromIntPtr(IntPtr value)
	{
		return (GCHandle)value;
	}

	/// <summary>Returns the internal integer representation of a <see cref="T:System.Runtime.InteropServices.GCHandle" /> object.</summary>
	/// <returns>An <see cref="T:System.IntPtr" /> object that represents a <see cref="T:System.Runtime.InteropServices.GCHandle" /> object. </returns>
	/// <param name="value">A <see cref="T:System.Runtime.InteropServices.GCHandle" /> object to retrieve an internal integer representation from.</param>
	public static IntPtr ToIntPtr(GCHandle value)
	{
		return (IntPtr)value;
	}
}
