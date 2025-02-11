using System.Collections.Generic;
using MS.Internal.WindowsBase;

namespace System.Windows;

/// <summary>Implements an underlying type cache for all <see cref="T:System.Windows.DependencyObject" /> derived types.</summary>
public class DependencyObjectType
{
	private int _id;

	private Type _systemType;

	private DependencyObjectType _baseDType;

	private static Dictionary<Type, DependencyObjectType> DTypeFromCLRType = new Dictionary<Type, DependencyObjectType>();

	private static int DTypeCount = 0;

	private static readonly object _lock = new object();

	/// <summary>Gets a zero-based unique identifier for constant-time array lookup operations.</summary>
	/// <returns>An internal identifier.</returns>
	public int Id => _id;

	/// <summary> Gets the common language runtime (CLR) system type represented by this <see cref="T:System.Windows.DependencyObjectType" />. </summary>
	/// <returns>The CLR system type represented by this <see cref="T:System.Windows.DependencyObjectType" />.</returns>
	public Type SystemType => _systemType;

	/// <summary>Gets the <see cref="T:System.Windows.DependencyObjectType" /> of the immediate base class of the current <see cref="T:System.Windows.DependencyObjectType" />. </summary>
	/// <returns>The type of the base class.</returns>
	public DependencyObjectType BaseType => _baseDType;

	/// <summary>Gets the name of the represented common language runtime (CLR) system type. </summary>
	/// <returns>The name of the represented CLR system type.</returns>
	public string Name => SystemType.Name;

	/// <summary>Returns a <see cref="T:System.Windows.DependencyObjectType" /> that represents a given system (CLR) type.</summary>
	/// <returns>A <see cref="T:System.Windows.DependencyObjectType" /> that represents the system (CLR) type.</returns>
	/// <param name="systemType">The system (CLR) type to convert.</param>
	public static DependencyObjectType FromSystemType(Type systemType)
	{
		if (systemType == null)
		{
			throw new ArgumentNullException("systemType");
		}
		if (!typeof(DependencyObject).IsAssignableFrom(systemType))
		{
			throw new ArgumentException(SR.Format(SR.DTypeNotSupportForSystemType, systemType.Name));
		}
		return FromSystemTypeInternal(systemType);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static DependencyObjectType FromSystemTypeInternal(Type systemType)
	{
		lock (_lock)
		{
			return FromSystemTypeRecursive(systemType);
		}
	}

	private static DependencyObjectType FromSystemTypeRecursive(Type systemType)
	{
		if (!DTypeFromCLRType.TryGetValue(systemType, out var value))
		{
			value = new DependencyObjectType();
			value._systemType = systemType;
			DTypeFromCLRType[systemType] = value;
			if (systemType != typeof(DependencyObject))
			{
				value._baseDType = FromSystemTypeRecursive(systemType.BaseType);
			}
			value._id = DTypeCount++;
		}
		return value;
	}

	/// <summary>Determines whether the specified object is an instance of the current <see cref="T:System.Windows.DependencyObjectType" />.</summary>
	/// <returns>true if the class represented by the current <see cref="T:System.Windows.DependencyObjectType" /> is in the inheritance hierarchy of the <see cref="T:System.Windows.DependencyObject" /> passed as <paramref name="d" />; otherwise, false.</returns>
	/// <param name="dependencyObject">The object to compare with the current <see cref="T:System.Windows.DependencyObjectType" />.</param>
	public bool IsInstanceOfType(DependencyObject dependencyObject)
	{
		if (dependencyObject != null)
		{
			DependencyObjectType dependencyObjectType = dependencyObject.DependencyObjectType;
			do
			{
				if (dependencyObjectType.Id == Id)
				{
					return true;
				}
				dependencyObjectType = dependencyObjectType._baseDType;
			}
			while (dependencyObjectType != null);
		}
		return false;
	}

	/// <summary>Determines whether the current <see cref="T:System.Windows.DependencyObjectType" /> derives from the specified <see cref="T:System.Windows.DependencyObjectType" />.</summary>
	/// <returns>true if the <paramref name="dependencyObjectType" /> parameter and the current <see cref="T:System.Windows.DependencyObjectType" /> represent types of classes, and the class represented by the current <see cref="T:System.Windows.DependencyObjectType" /> derives from the class represented by <paramref name="dependencyObjectType" />. Otherwise, false. This method also returns false if <paramref name="dependencyObjectType" /> and the current <see cref="T:System.Windows.DependencyObjectType" /> represent the same class.</returns>
	/// <param name="dependencyObjectType">The <see cref="T:System.Windows.DependencyObjectType" /> to compare.</param>
	public bool IsSubclassOf(DependencyObjectType dependencyObjectType)
	{
		if (dependencyObjectType != null)
		{
			for (DependencyObjectType baseDType = _baseDType; baseDType != null; baseDType = baseDType._baseDType)
			{
				if (baseDType.Id == dependencyObjectType.Id)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Windows.DependencyObjectType" />.</summary>
	/// <returns>A 32-bit signed integer hash code. </returns>
	public override int GetHashCode()
	{
		return _id;
	}

	private DependencyObjectType()
	{
	}
}
