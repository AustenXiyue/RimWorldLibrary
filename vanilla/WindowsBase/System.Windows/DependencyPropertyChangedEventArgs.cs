using MS.Internal.WindowsBase;

namespace System.Windows;

/// <summary>Provides data for various property changed events. Typically these events report effective value changes in the value of a read-only dependency property. Another usage is as part of a <see cref="T:System.Windows.PropertyChangedCallback" /> implementation.</summary>
public struct DependencyPropertyChangedEventArgs
{
	private enum PrivateFlags : byte
	{
		IsAValueChange = 1,
		IsASubPropertyChange
	}

	private DependencyProperty _property;

	private PropertyMetadata _metadata;

	private PrivateFlags _flags;

	private EffectiveValueEntry _oldEntry;

	private EffectiveValueEntry _newEntry;

	private OperationType _operationType;

	/// <summary>Gets the identifier for the dependency property where the value change occurred.</summary>
	/// <returns>The identifier field of the dependency property where the value change occurred.</returns>
	public DependencyProperty Property => _property;

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal bool IsAValueChange
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.IsAValueChange);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.IsAValueChange, value);
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal bool IsASubPropertyChange
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.IsASubPropertyChange);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.IsASubPropertyChange, value);
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal PropertyMetadata Metadata => _metadata;

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal OperationType OperationType => _operationType;

	/// <summary>Gets the value of the property before the change.</summary>
	/// <returns>The property value before the change.</returns>
	public object OldValue
	{
		get
		{
			EffectiveValueEntry flattenedEntry = OldEntry.GetFlattenedEntry(RequestFlags.FullyResolved);
			if (flattenedEntry.IsDeferredReference)
			{
				flattenedEntry.Value = ((DeferredReference)flattenedEntry.Value).GetValue(flattenedEntry.BaseValueSourceInternal);
			}
			return flattenedEntry.Value;
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal EffectiveValueEntry OldEntry => _oldEntry;

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal BaseValueSourceInternal OldValueSource => _oldEntry.BaseValueSourceInternal;

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal bool IsOldValueModified => _oldEntry.HasModifiers;

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal bool IsOldValueDeferred => _oldEntry.IsDeferredReference;

	/// <summary>Gets the value of the property after the change.</summary>
	/// <returns>The property value after the change.</returns>
	public object NewValue
	{
		get
		{
			EffectiveValueEntry flattenedEntry = NewEntry.GetFlattenedEntry(RequestFlags.FullyResolved);
			if (flattenedEntry.IsDeferredReference)
			{
				flattenedEntry.Value = ((DeferredReference)flattenedEntry.Value).GetValue(flattenedEntry.BaseValueSourceInternal);
			}
			return flattenedEntry.Value;
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal EffectiveValueEntry NewEntry => _newEntry;

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal BaseValueSourceInternal NewValueSource => _newEntry.BaseValueSourceInternal;

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal bool IsNewValueModified => _newEntry.HasModifiers;

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal bool IsNewValueDeferred => _newEntry.IsDeferredReference;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> class.</summary>
	/// <param name="property">The identifier for the dependency property that changed.</param>
	/// <param name="oldValue">The value of the property before the change reported by the relevant event or state change.</param>
	/// <param name="newValue">The value of the property after the change reported by the relevant event or state change.</param>
	public DependencyPropertyChangedEventArgs(DependencyProperty property, object oldValue, object newValue)
	{
		_property = property;
		_metadata = null;
		_oldEntry = new EffectiveValueEntry(property);
		_newEntry = _oldEntry;
		_oldEntry.Value = oldValue;
		_newEntry.Value = newValue;
		_flags = (PrivateFlags)0;
		_operationType = OperationType.Unknown;
		IsAValueChange = true;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal DependencyPropertyChangedEventArgs(DependencyProperty property, PropertyMetadata metadata, object oldValue, object newValue)
	{
		_property = property;
		_metadata = metadata;
		_oldEntry = new EffectiveValueEntry(property);
		_newEntry = _oldEntry;
		_oldEntry.Value = oldValue;
		_newEntry.Value = newValue;
		_flags = (PrivateFlags)0;
		_operationType = OperationType.Unknown;
		IsAValueChange = true;
	}

	internal DependencyPropertyChangedEventArgs(DependencyProperty property, PropertyMetadata metadata, object value)
	{
		_property = property;
		_metadata = metadata;
		_oldEntry = new EffectiveValueEntry(property);
		_oldEntry.Value = value;
		_newEntry = _oldEntry;
		_flags = (PrivateFlags)0;
		_operationType = OperationType.Unknown;
		IsASubPropertyChange = true;
	}

	internal DependencyPropertyChangedEventArgs(DependencyProperty property, PropertyMetadata metadata, bool isAValueChange, EffectiveValueEntry oldEntry, EffectiveValueEntry newEntry, OperationType operationType)
	{
		_property = property;
		_metadata = metadata;
		_oldEntry = oldEntry;
		_newEntry = newEntry;
		_flags = (PrivateFlags)0;
		_operationType = operationType;
		IsAValueChange = isAValueChange;
		IsASubPropertyChange = operationType == OperationType.ChangeMutableDefaultValue;
	}

	/// <summary>Gets a hash code  for this <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" />. </summary>
	/// <returns>A signed 32-bit integer hash code. </returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Determines whether the provided object is equivalent to the current <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" />.</summary>
	/// <returns>true if the provided object is equivalent to the current <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" />; otherwise, false.</returns>
	/// <param name="obj">The object to compare to the current <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" />.</param>
	public override bool Equals(object obj)
	{
		return Equals((DependencyPropertyChangedEventArgs)obj);
	}

	/// <summary>Determines whether the provided <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> is equivalent to the current <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" />.</summary>
	/// <returns>true if the provided <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> is equivalent to the current <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" />; otherwise, false.</returns>
	/// <param name="args">The <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> to compare to the current <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /></param>
	public bool Equals(DependencyPropertyChangedEventArgs args)
	{
		if (_property == args._property && _metadata == args._metadata && _oldEntry.Value == args._oldEntry.Value && _newEntry.Value == args._newEntry.Value && _flags == args._flags && _oldEntry.BaseValueSourceInternal == args._oldEntry.BaseValueSourceInternal && _newEntry.BaseValueSourceInternal == args._newEntry.BaseValueSourceInternal && _oldEntry.HasModifiers == args._oldEntry.HasModifiers && _newEntry.HasModifiers == args._newEntry.HasModifiers && _oldEntry.IsDeferredReference == args._oldEntry.IsDeferredReference && _newEntry.IsDeferredReference == args._newEntry.IsDeferredReference)
		{
			return _operationType == args._operationType;
		}
		return false;
	}

	/// <summary>Determines whether two specified <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> objects have the same value.</summary>
	/// <returns>true if the two <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> instances are equivalent; otherwise, false.</returns>
	/// <param name="left">The first <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> to compare.</param>
	/// <param name="right">The second <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> to compare.</param>
	public static bool operator ==(DependencyPropertyChangedEventArgs left, DependencyPropertyChangedEventArgs right)
	{
		return left.Equals(right);
	}

	/// <summary>Determines whether two specified <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> objects are different.</summary>
	/// <returns>true if the two <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> instances are different; otherwise, false.</returns>
	/// <param name="left">The first <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> to compare.</param>
	/// <param name="right">The second <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" /> to compare.</param>
	public static bool operator !=(DependencyPropertyChangedEventArgs left, DependencyPropertyChangedEventArgs right)
	{
		return !left.Equals(right);
	}

	private void WritePrivateFlag(PrivateFlags bit, bool value)
	{
		if (value)
		{
			_flags |= bit;
		}
		else
		{
			_flags &= (PrivateFlags)(byte)(~(int)bit);
		}
	}

	private bool ReadPrivateFlag(PrivateFlags bit)
	{
		return (_flags & bit) != 0;
	}
}
