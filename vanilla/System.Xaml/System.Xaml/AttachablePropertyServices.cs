using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Xaml;

/// <summary>Provides static helper methods that obtain values and accessor method information from an attachable property and that work with an attached property store.</summary>
public static class AttachablePropertyServices
{
	private sealed class DefaultAttachedPropertyStore
	{
		private Lazy<ConditionalWeakTable<object, Dictionary<AttachableMemberIdentifier, object>>> instanceStorage = new Lazy<ConditionalWeakTable<object, Dictionary<AttachableMemberIdentifier, object>>>();

		public void CopyPropertiesTo(object instance, KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
		{
			if (instanceStorage.IsValueCreated && instanceStorage.Value.TryGetValue(instance, out var value))
			{
				lock (value)
				{
					((ICollection<KeyValuePair<AttachableMemberIdentifier, object>>)value).CopyTo(array, index);
				}
			}
		}

		public int GetPropertyCount(object instance)
		{
			if (instanceStorage.IsValueCreated && instanceStorage.Value.TryGetValue(instance, out var value))
			{
				lock (value)
				{
					return value.Count;
				}
			}
			return 0;
		}

		public bool RemoveProperty(object instance, AttachableMemberIdentifier name)
		{
			if (instanceStorage.IsValueCreated && instanceStorage.Value.TryGetValue(instance, out var value))
			{
				lock (value)
				{
					return value.Remove(name);
				}
			}
			return false;
		}

		public void SetProperty(object instance, AttachableMemberIdentifier name, object value)
		{
			if (!instanceStorage.Value.TryGetValue(instance, out var value2))
			{
				value2 = new Dictionary<AttachableMemberIdentifier, object>();
				try
				{
					instanceStorage.Value.Add(instance, value2);
				}
				catch (ArgumentException)
				{
					if (!instanceStorage.Value.TryGetValue(instanceStorage, out value2))
					{
						throw new InvalidOperationException(System.SR.DefaultAttachablePropertyStoreCannotAddInstance);
					}
				}
			}
			lock (value2)
			{
				value2[name] = value;
			}
		}

		public bool TryGetProperty<T>(object instance, AttachableMemberIdentifier name, out T value)
		{
			if (instanceStorage.IsValueCreated && instanceStorage.Value.TryGetValue(instance, out var value2))
			{
				lock (value2)
				{
					if (value2.TryGetValue(name, out var value3) && value3 is T)
					{
						value = (T)value3;
						return true;
					}
				}
			}
			value = default(T);
			return false;
		}
	}

	private static DefaultAttachedPropertyStore attachedProperties = new DefaultAttachedPropertyStore();

	/// <summary>Returns the count of the attachable property entries that are in the specified store.</summary>
	/// <returns>The integer count of entries in the store.</returns>
	/// <param name="instance">A specific attachable property store that implements <see cref="T:System.Xaml.IAttachedPropertyStore" />; or any non-null object to access a static default attachable property store.</param>
	public static int GetAttachedPropertyCount(object instance)
	{
		if (instance == null)
		{
			return 0;
		}
		if (instance is IAttachedPropertyStore attachedPropertyStore)
		{
			return attachedPropertyStore.PropertyCount;
		}
		return attachedProperties.GetPropertyCount(instance);
	}

	/// <summary>Copies all attachable property/value pairs from a specified attachable property store and into a destination array.</summary>
	/// <param name="instance">A specific attachable property store that implements <see cref="T:System.Xaml.IAttachedPropertyStore" />; or any non-null object to access a static default attachable property store.</param>
	/// <param name="array">The destination array. The array is a generic array, should be passed undimensioned, and should have components of <see cref="T:System.Xaml.AttachableMemberIdentifier" /> and object.</param>
	/// <param name="index">The source index into which to copy.</param>
	public static void CopyPropertiesTo(object instance, KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
	{
		if (instance != null)
		{
			if (instance is IAttachedPropertyStore attachedPropertyStore)
			{
				attachedPropertyStore.CopyPropertiesTo(array, index);
			}
			else
			{
				attachedProperties.CopyPropertiesTo(instance, array, index);
			}
		}
	}

	/// <summary>Removes the entry for the specified attachable property from the specified store.</summary>
	/// <returns>true if an attachable property entry for <paramref name="name" /> was found in the store and removed from it; otherwise, false.</returns>
	/// <param name="instance">A specific attachable property store that implements <see cref="T:System.Xaml.IAttachedPropertyStore" />; or any non-null object to access a static default attachable property store.</param>
	/// <param name="name">The identifier for the attachable property entry to remove from the store.</param>
	public static bool RemoveProperty(object instance, AttachableMemberIdentifier name)
	{
		if (instance == null)
		{
			return false;
		}
		if (instance is IAttachedPropertyStore attachedPropertyStore)
		{
			return attachedPropertyStore.RemoveProperty(name);
		}
		return attachedProperties.RemoveProperty(instance, name);
	}

	/// <summary>Sets a value for the specified attachable property in the specified store.</summary>
	/// <param name="instance">A specific attachable property store that implements <see cref="T:System.Xaml.IAttachedPropertyStore" />; or any non-null object to use a static default attachable property store.</param>
	/// <param name="name">The identifier of the attachable property entry for which to set a value.</param>
	/// <param name="value">The value to set.</param>
	/// <exception cref="T:System.InvalidOperationException">A value could not be set in the store. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	public static void SetProperty(object instance, AttachableMemberIdentifier name, object value)
	{
		if (instance != null)
		{
			ArgumentNullException.ThrowIfNull(name, "name");
			if (instance is IAttachedPropertyStore attachedPropertyStore)
			{
				attachedPropertyStore.SetProperty(name, value);
			}
			else
			{
				attachedProperties.SetProperty(instance, name, value);
			}
		}
	}

	/// <summary>Attempts to get a value for the specified attachable property in the specified store. Does not throw an exception if the specific attachable property does not exist in the store.</summary>
	/// <returns>true if an attachable property entry for <paramref name="name" /> was found in the store and a value was posted to <paramref name="value" />; otherwise, false.</returns>
	/// <param name="instance">A specific attachable property store that implements <see cref="T:System.Xaml.IAttachedPropertyStore" />; or any non-null object to use a static default attachable property store.</param>
	/// <param name="name">The identifier of the attachable property entry for which to get a value.</param>
	/// <param name="value">Out parameter. When this method returns, contains the destination object for the value if <paramref name="name" /> exists in the store and has a value.</param>
	public static bool TryGetProperty(object instance, AttachableMemberIdentifier name, out object value)
	{
		return AttachablePropertyServices.TryGetProperty<object>(instance, name, out value);
	}

	/// <summary>Attempts to get a value for the specified attachable property in the specified store, returning a generic output form. Does not throw an exception if the specific attachable property does not exist in the store.</summary>
	/// <returns>true if an attachable property entry for <paramref name="name" /> was found in the store and a value was posted to <paramref name="value" />; otherwise, false.</returns>
	/// <param name="instance">A specific attachable property store that implements <see cref="T:System.Xaml.IAttachedPropertyStore" />; or any non-null object to access a static default attachable property store.</param>
	/// <param name="name">The identifier of the attachable property entry for which to get a value.</param>
	/// <param name="value">Out parameter. When this method returns, contains the destination object for the value if <paramref name="name" /> exists in the store and has a value.</param>
	/// <typeparam name="T">The expected type of the output.</typeparam>
	public static bool TryGetProperty<T>(object instance, AttachableMemberIdentifier name, out T value)
	{
		if (instance == null)
		{
			value = default(T);
			return false;
		}
		if (instance is IAttachedPropertyStore attachedPropertyStore)
		{
			if (attachedPropertyStore.TryGetProperty(name, out var value2) && value2 is T)
			{
				value = (T)value2;
				return true;
			}
			value = default(T);
			return false;
		}
		return attachedProperties.TryGetProperty<T>(instance, name, out value);
	}
}
