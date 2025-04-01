using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace System;

/// <summary>Supports all classes in the .NET Framework class hierarchy and provides low-level services to derived classes. This is the ultimate base class of all classes in the .NET Framework; it is the root of the type hierarchy.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ClassInterface(ClassInterfaceType.AutoDual)]
[ComVisible(true)]
public class Object
{
	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object. </param>
	/// <filterpriority>2</filterpriority>
	public virtual bool Equals(object obj)
	{
		return this == obj;
	}

	/// <summary>Determines whether the specified object instances are considered equal.</summary>
	/// <returns>true if the objects are considered equal; otherwise, false. If both <paramref name="objA" /> and <paramref name="objB" /> are null, the method returns true.</returns>
	/// <param name="objA">The first object to compare. </param>
	/// <param name="objB">The second object to compare. </param>
	/// <filterpriority>2</filterpriority>
	public static bool Equals(object objA, object objB)
	{
		if (objA == objB)
		{
			return true;
		}
		if (objA == null || objB == null)
		{
			return false;
		}
		return objA.Equals(objB);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public Object()
	{
	}

	/// <summary>Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.</summary>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	~Object()
	{
	}

	/// <summary>Serves as a hash function for a particular type. </summary>
	/// <returns>A hash code for the current object.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual int GetHashCode()
	{
		return InternalGetHashCode(this);
	}

	/// <summary>Gets the <see cref="T:System.Type" /> of the current instance.</summary>
	/// <returns>The exact runtime type of the current instance.</returns>
	/// <filterpriority>2</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern Type GetType();

	/// <summary>Creates a shallow copy of the current <see cref="T:System.Object" />.</summary>
	/// <returns>A shallow copy of the current <see cref="T:System.Object" />.</returns>
	[MethodImpl(MethodImplOptions.InternalCall)]
	protected extern object MemberwiseClone();

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual string ToString()
	{
		return GetType().ToString();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> instances are the same instance.</summary>
	/// <returns>true if <paramref name="objA" /> is the same instance as <paramref name="objB" /> or if both are null; otherwise, false.</returns>
	/// <param name="objA">The first object to compare. </param>
	/// <param name="objB">The second object  to compare. </param>
	/// <filterpriority>2</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static bool ReferenceEquals(object objA, object objB)
	{
		return objA == objB;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern int InternalGetHashCode(object o);

	private void FieldGetter(string typeName, string fieldName, ref object val)
	{
	}

	private void FieldSetter(string typeName, string fieldName, object val)
	{
	}
}
