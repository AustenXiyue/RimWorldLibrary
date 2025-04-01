using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>Represents a weak reference, which references an object while still allowing that object to be reclaimed by garbage collection.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class WeakReference : ISerializable
{
	private bool isLongReference;

	private GCHandle gcHandle;

	/// <summary>Gets an indication whether the object referenced by the current <see cref="T:System.WeakReference" /> object has been garbage collected.</summary>
	/// <returns>true if the object referenced by the current <see cref="T:System.WeakReference" /> object has not been garbage collected and is still accessible; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsAlive => Target != null;

	/// <summary>Gets or sets the object (the target) referenced by the current <see cref="T:System.WeakReference" /> object.</summary>
	/// <returns>null if the object referenced by the current <see cref="T:System.WeakReference" /> object has been garbage collected; otherwise, a reference to the object referenced by the current <see cref="T:System.WeakReference" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">The reference to the target object is invalid. This exception can be thrown while setting this property if the value is a null reference or if the object has been finalized during the set operation.</exception>
	/// <filterpriority>2</filterpriority>
	public virtual object Target
	{
		get
		{
			if (!gcHandle.IsAllocated)
			{
				return null;
			}
			return gcHandle.Target;
		}
		set
		{
			gcHandle.Target = value;
		}
	}

	/// <summary>Gets an indication whether the object referenced by the current <see cref="T:System.WeakReference" /> object is tracked after it is finalized.</summary>
	/// <returns>true if the object the current <see cref="T:System.WeakReference" /> object refers to is tracked after finalization; or false if the object is only tracked until finalization.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool TrackResurrection => isLongReference;

	private void AllocateHandle(object target)
	{
		if (isLongReference)
		{
			gcHandle = GCHandle.Alloc(target, GCHandleType.WeakTrackResurrection);
		}
		else
		{
			gcHandle = GCHandle.Alloc(target, GCHandleType.Weak);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.WeakReference" /> class, referencing the specified object.</summary>
	/// <param name="target">The object to track or null. </param>
	public WeakReference(object target)
		: this(target, trackResurrection: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.WeakReference" /> class, referencing the specified object and using the specified resurrection tracking.</summary>
	/// <param name="target">An object to track. </param>
	/// <param name="trackResurrection">Indicates when to stop tracking the object. If true, the object is tracked after finalization; if false, the object is only tracked until finalization. </param>
	public WeakReference(object target, bool trackResurrection)
	{
		isLongReference = trackResurrection;
		AllocateHandle(target);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.WeakReference" /> class, using deserialized data from the specified serialization and stream objects.</summary>
	/// <param name="info">An object that holds all the data needed to serialize or deserialize the current <see cref="T:System.WeakReference" /> object. </param>
	/// <param name="context">(Reserved) Describes the source and destination of the serialized stream specified by <paramref name="info" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	protected WeakReference(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		isLongReference = info.GetBoolean("TrackResurrection");
		object value = info.GetValue("TrackedObject", typeof(object));
		AllocateHandle(value);
	}

	/// <summary>Discards the reference to the target represented by the current <see cref="T:System.WeakReference" /> object.</summary>
	~WeakReference()
	{
		gcHandle.Free();
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with all the data needed to serialize the current <see cref="T:System.WeakReference" /> object.</summary>
	/// <param name="info">An object that holds all the data needed to serialize or deserialize the current <see cref="T:System.WeakReference" /> object. </param>
	/// <param name="context">(Reserved) The location where serialized data is stored and retrieved. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("TrackResurrection", TrackResurrection);
		try
		{
			info.AddValue("TrackedObject", Target);
		}
		catch (Exception)
		{
			info.AddValue("TrackedObject", null);
		}
	}
}
/// <summary>Represents a typed weak reference, which references an object while still allowing that object to be reclaimed by garbage collection.</summary>
/// <typeparam name="T">The type of the object referenced.</typeparam>
[Serializable]
public sealed class WeakReference<T> : ISerializable where T : class
{
	private GCHandle handle;

	private bool trackResurrection;

	/// <summary>Initializes a new instance of the <see cref="T:System.WeakReference`1" /> class that references the specified object.</summary>
	/// <param name="target">The object to reference, or null.</param>
	public WeakReference(T target)
		: this(target, trackResurrection: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.WeakReference`1" /> class that references the specified object and uses the specified resurrection tracking.</summary>
	/// <param name="target">The object to reference, or null.</param>
	/// <param name="trackResurrection">true to track the object after finalization; false to track the object only until finalization.</param>
	public WeakReference(T target, bool trackResurrection)
	{
		this.trackResurrection = trackResurrection;
		handle = GCHandle.Alloc(target, trackResurrection ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak);
	}

	private WeakReference(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		trackResurrection = info.GetBoolean("TrackResurrection");
		object value = info.GetValue("TrackedObject", typeof(T));
		handle = GCHandle.Alloc(value, trackResurrection ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak);
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with all the data necessary to serialize the current <see cref="T:System.WeakReference`1" /> object.</summary>
	/// <param name="info">An object that holds all the data necessary to serialize or deserialize the current <see cref="T:System.WeakReference`1" /> object.</param>
	/// <param name="context">The location where serialized data is stored and retrieved.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	[SecurityCritical]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("TrackResurrection", trackResurrection);
		if (handle.IsAllocated)
		{
			info.AddValue("TrackedObject", handle.Target);
		}
		else
		{
			info.AddValue("TrackedObject", null);
		}
	}

	/// <summary>Sets the target object that is referenced by this <see cref="T:System.WeakReference`1" /> object.</summary>
	/// <param name="target">The new target object.</param>
	public void SetTarget(T target)
	{
		handle.Target = target;
	}

	/// <summary>Tries to retrieve the target object that is referenced by the current <see cref="T:System.WeakReference`1" /> object.</summary>
	/// <returns>true if the target was retrieved; otherwise, false.</returns>
	/// <param name="target">When this method returns, contains the target object, if it is available. This parameter is treated as uninitialized.</param>
	public bool TryGetTarget(out T target)
	{
		if (!handle.IsAllocated)
		{
			target = null;
			return false;
		}
		target = (T)handle.Target;
		return target != null;
	}

	~WeakReference()
	{
		handle.Free();
	}
}
