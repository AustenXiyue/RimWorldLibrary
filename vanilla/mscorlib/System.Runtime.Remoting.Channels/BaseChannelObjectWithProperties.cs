using System.Collections;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Runtime.Remoting.Channels;

/// <summary>Provides a base implementation of a channel object that exposes a dictionary interface to its properties.</summary>
[ComVisible(true)]
public abstract class BaseChannelObjectWithProperties : IDictionary, ICollection, IEnumerable
{
	private Hashtable table;

	/// <summary>Gets the number of properties associated with the channel object.</summary>
	/// <returns>The number of properties associated with the channel object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual int Count
	{
		[SecuritySafeCritical]
		get
		{
			return table.Count;
		}
	}

	/// <summary>Gets a value that indicates whether the number of properties that can be entered into the channel object is fixed.</summary>
	/// <returns>true if the number of properties that can be entered into the channel object is fixed; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual bool IsFixedSize
	{
		[SecuritySafeCritical]
		get
		{
			return true;
		}
	}

	/// <summary>Gets a value that indicates whether the collection of properties in the channel object is read-only.</summary>
	/// <returns>true if the collection of properties in the channel object is read-only; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual bool IsReadOnly
	{
		[SecuritySafeCritical]
		get
		{
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether the dictionary of channel object properties is synchronized.</summary>
	/// <returns>true if the dictionary of channel object properties is synchronized; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual bool IsSynchronized
	{
		[SecuritySafeCritical]
		get
		{
			return false;
		}
	}

	/// <summary>When overridden in a derived class, gets or sets the property that is associated with the specified key.</summary>
	/// <returns>The property that is associated with the specified key.</returns>
	/// <param name="key">The key of the property to get or set. </param>
	/// <exception cref="T:System.NotImplementedException">The property is accessed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual object this[object key]
	{
		[SecuritySafeCritical]
		get
		{
			throw new NotImplementedException();
		}
		[SecuritySafeCritical]
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>When overridden in a derived class, gets a <see cref="T:System.Collections.ICollection" /> of keys that the channel object properties are associated with.</summary>
	/// <returns>A <see cref="T:System.Collections.ICollection" /> of keys that the channel object properties are associated with.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual ICollection Keys
	{
		[SecuritySafeCritical]
		get
		{
			return table.Keys;
		}
	}

	/// <summary>Gets a <see cref="T:System.Collections.IDictionary" /> of the channel properties associated with the channel object.</summary>
	/// <returns>A <see cref="T:System.Collections.IDictionary" /> of the channel properties associated with the channel object.</returns>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	public virtual IDictionary Properties => this;

	/// <summary>Gets an object that is used to synchronize access to the <see cref="T:System.Runtime.Remoting.Channels.BaseChannelObjectWithProperties" />.</summary>
	/// <returns>An object that is used to synchronize access to the <see cref="T:System.Runtime.Remoting.Channels.BaseChannelObjectWithProperties" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual object SyncRoot
	{
		[SecuritySafeCritical]
		get
		{
			return this;
		}
	}

	/// <summary>Gets a <see cref="T:System.Collections.ICollection" /> of the values of the properties associated with the channel object.</summary>
	/// <returns>A <see cref="T:System.Collections.ICollection" /> of the values of the properties associated with the channel object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual ICollection Values
	{
		[SecuritySafeCritical]
		get
		{
			return table.Values;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.Channels.BaseChannelObjectWithProperties" /> class.</summary>
	protected BaseChannelObjectWithProperties()
	{
		table = new Hashtable();
	}

	/// <summary>Throws a <see cref="T:System.NotSupportedException" />.</summary>
	/// <param name="key">The key that is associated with the object in the <paramref name="value" /> parameter. </param>
	/// <param name="value">The value to add. </param>
	/// <exception cref="T:System.NotSupportedException">The method is called. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public virtual void Add(object key, object value)
	{
		throw new NotSupportedException();
	}

	/// <summary>Throws a <see cref="T:System.NotSupportedException" />.</summary>
	/// <exception cref="T:System.NotSupportedException">The method is called. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public virtual void Clear()
	{
		throw new NotSupportedException();
	}

	/// <summary>Returns a value that indicates whether the channel object contains a property that is associated with the specified key.</summary>
	/// <returns>true if the channel object contains a property associated with the specified key; otherwise, false.</returns>
	/// <param name="key">The key of the property to look for. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public virtual bool Contains(object key)
	{
		return table.Contains(key);
	}

	/// <summary>Throws a <see cref="T:System.NotSupportedException" />.</summary>
	/// <param name="array">The array to copy the properties to. </param>
	/// <param name="index">The index at which to begin copying. </param>
	/// <exception cref="T:System.NotSupportedException">The method is called. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public virtual void CopyTo(Array array, int index)
	{
		throw new NotSupportedException();
	}

	/// <summary>Returns a <see cref="T:System.Collections.IDictionaryEnumerator" /> that enumerates over all the properties associated with the channel object.</summary>
	/// <returns>A <see cref="T:System.Collections.IDictionaryEnumerator" /> that enumerates over all the properties associated with the channel object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public virtual IDictionaryEnumerator GetEnumerator()
	{
		return table.GetEnumerator();
	}

	/// <summary>Returns a <see cref="T:System.Collections.IEnumerator" /> that enumerates over all the properties that are associated with the channel object.</summary>
	/// <returns>A <see cref="T:System.Collections.IEnumerator" /> that enumerates over all the properties that are associated with the channel object.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return table.GetEnumerator();
	}

	/// <summary>Throws a <see cref="T:System.NotSupportedException" />.</summary>
	/// <param name="key">The key of the object to be removed. </param>
	/// <exception cref="T:System.NotSupportedException">The method is called. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public virtual void Remove(object key)
	{
		throw new NotSupportedException();
	}
}
