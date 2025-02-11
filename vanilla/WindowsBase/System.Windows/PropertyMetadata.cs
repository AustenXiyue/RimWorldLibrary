using System.Collections;
using System.Diagnostics;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.WindowsBase;
using MS.Utility;

namespace System.Windows;

/// <summary>Defines certain behavior aspects of a dependency property as it is applied to a specific type, including conditions it was registered with. </summary>
public class PropertyMetadata
{
	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal enum MetadataFlags : uint
	{
		DefaultValueModifiedID = 1u,
		SealedID = 2u,
		Inherited = 0x10u,
		UI_IsAnimationProhibitedID = 0x20u,
		FW_AffectsMeasureID = 0x40u,
		FW_AffectsArrangeID = 0x80u,
		FW_AffectsParentMeasureID = 0x100u,
		FW_AffectsParentArrangeID = 0x200u,
		FW_AffectsRenderID = 0x400u,
		FW_OverridesInheritanceBehaviorID = 0x800u,
		FW_IsNotDataBindableID = 0x1000u,
		FW_BindsTwoWayByDefaultID = 0x2000u,
		FW_ShouldBeJournaledID = 0x4000u,
		FW_SubPropertiesDoNotAffectRenderID = 0x8000u,
		FW_SubPropertiesDoNotAffectRenderModifiedID = 0x10000u,
		FW_InheritsModifiedID = 0x100000u,
		FW_OverridesInheritanceBehaviorModifiedID = 0x200000u,
		FW_ShouldBeJournaledModifiedID = 0x1000000u,
		FW_UpdatesSourceOnLostFocusByDefaultID = 0x2000000u,
		FW_DefaultUpdateSourceTriggerModifiedID = 0x4000000u,
		FW_ReadOnlyID = 0x8000000u,
		FW_DefaultUpdateSourceTriggerEnumBit1 = 0x40000000u,
		FW_DefaultUpdateSourceTriggerEnumBit2 = 0x80000000u
	}

	private static FreezeValueCallback _defaultFreezeValueCallback = DefaultFreezeValueCallback;

	private object _defaultValue;

	private PropertyChangedCallback _propertyChangedCallback;

	private CoerceValueCallback _coerceValueCallback;

	private FreezeValueCallback _freezeValueCallback;

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal MetadataFlags _flags;

	private static readonly UncommonField<FrugalMapBase> _defaultValueFactoryCache = new UncommonField<FrugalMapBase>();

	private static FrugalMapIterationCallback _removalCallback = DefaultValueCacheRemovalCallback;

	private static FrugalMapIterationCallback _promotionCallback = DefaultValueCachePromotionCallback;

	/// <summary> Gets or sets the default value of the dependency property. </summary>
	/// <returns>The default value of the property. The default value on a <see cref="T:System.Windows.PropertyMetadata" /> instance created with the parameterless constructor will be <see cref="F:System.Windows.DependencyProperty.UnsetValue" />; see Remarks.</returns>
	/// <exception cref="T:System.ArgumentException">Cannot be set to the value <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> once created; see Remarks.</exception>
	/// <exception cref="T:System.InvalidOperationException">Cannot set a metadata property once it is applied to a dependency property operation.</exception>
	public object DefaultValue
	{
		get
		{
			if (!(_defaultValue is DefaultValueFactory defaultValueFactory))
			{
				return _defaultValue;
			}
			return defaultValueFactory.DefaultValue;
		}
		set
		{
			if (Sealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			if (value == DependencyProperty.UnsetValue)
			{
				throw new ArgumentException(SR.DefaultValueMayNotBeUnset);
			}
			_defaultValue = value;
			SetModified(MetadataFlags.DefaultValueModifiedID);
		}
	}

	internal bool UsingDefaultValueFactory => _defaultValue is DefaultValueFactory;

	/// <summary>Gets or sets a reference to a <see cref="T:System.Windows.PropertyChangedCallback" /> implementation specified in this metadata.</summary>
	/// <returns>A <see cref="T:System.Windows.PropertyChangedCallback" /> implementation reference.</returns>
	/// <exception cref="T:System.InvalidOperationException">Cannot set a metadata property once it is applied to a dependency property operation.</exception>
	public PropertyChangedCallback PropertyChangedCallback
	{
		get
		{
			return _propertyChangedCallback;
		}
		set
		{
			if (Sealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			_propertyChangedCallback = value;
		}
	}

	/// <summary>Gets or sets a reference to a <see cref="T:System.Windows.CoerceValueCallback" /> implementation specified in this metadata.</summary>
	/// <returns>A <see cref="T:System.Windows.CoerceValueCallback" /> implementation reference.</returns>
	/// <exception cref="T:System.InvalidOperationException">Cannot set a metadata property once it is applied to a dependency property operation.</exception>
	public CoerceValueCallback CoerceValueCallback
	{
		get
		{
			return _coerceValueCallback;
		}
		set
		{
			if (Sealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			_coerceValueCallback = value;
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal virtual GetReadOnlyValueCallback GetReadOnlyValueCallback => null;

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal FreezeValueCallback FreezeValueCallback
	{
		get
		{
			if (_freezeValueCallback != null)
			{
				return _freezeValueCallback;
			}
			return _defaultFreezeValueCallback;
		}
		set
		{
			if (Sealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			_freezeValueCallback = value;
		}
	}

	/// <summary>Gets a value that determines whether the metadata has been applied to a property in some way, resulting in the immutable state of that metadata instance. </summary>
	/// <returns>true if the metadata instance is immutable; otherwise, false.</returns>
	protected bool IsSealed => Sealed;

	internal bool IsDefaultValueModified => IsModified(MetadataFlags.DefaultValueModifiedID);

	internal bool IsInherited
	{
		get
		{
			return (MetadataFlags.Inherited & _flags) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= MetadataFlags.Inherited;
			}
			else
			{
				_flags &= (MetadataFlags)4294967279u;
			}
		}
	}

	internal bool Sealed
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return ReadFlag(MetadataFlags.SealedID);
		}
		set
		{
			WriteFlag(MetadataFlags.SealedID, value);
		}
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.PropertyMetadata" /> class. </summary>
	public PropertyMetadata()
	{
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.PropertyMetadata" /> class with a specified default value for the dependency property that this metadata will be applied to. </summary>
	/// <param name="defaultValue">The default value to specify for a dependency property, usually provided as a value of some specific type.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="defaultValue" /> cannot be set to the value <see cref="F:System.Windows.DependencyProperty.UnsetValue" />; see Remarks.</exception>
	public PropertyMetadata(object defaultValue)
	{
		DefaultValue = defaultValue;
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.PropertyMetadata" /> class with the specified <see cref="T:System.Windows.PropertyChangedCallback" /> implementation reference. </summary>
	/// <param name="propertyChangedCallback">Reference to a handler implementation that is to be called by the property system whenever the effective value of the property changes.</param>
	public PropertyMetadata(PropertyChangedCallback propertyChangedCallback)
	{
		PropertyChangedCallback = propertyChangedCallback;
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.PropertyMetadata" /> class with the specified default value and <see cref="T:System.Windows.PropertyChangedCallback" /> implementation reference. </summary>
	/// <param name="defaultValue">The default value of the dependency property, usually provided as a value of some specific type.</param>
	/// <param name="propertyChangedCallback">Reference to a handler implementation that is to be called by the property system whenever the effective value of the property changes.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="defaultValue" /> cannot be set to the value <see cref="F:System.Windows.DependencyProperty.UnsetValue" />; see Remarks.</exception>
	public PropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback)
	{
		DefaultValue = defaultValue;
		PropertyChangedCallback = propertyChangedCallback;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.PropertyMetadata" /> class with the specified default value and callbacks. </summary>
	/// <param name="defaultValue">The default value of the dependency property, usually provided as a value of some specific type.</param>
	/// <param name="propertyChangedCallback">Reference to a handler implementation that is to be called by the property system whenever the effective value of the property changes.</param>
	/// <param name="coerceValueCallback">Reference to a handler implementation that is to be called whenever the property system calls <see cref="M:System.Windows.DependencyObject.CoerceValue(System.Windows.DependencyProperty)" /> against this property.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="defaultValue" /> cannot be set to the value <see cref="F:System.Windows.DependencyProperty.UnsetValue" />; see Remarks.</exception>
	public PropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback)
	{
		DefaultValue = defaultValue;
		PropertyChangedCallback = propertyChangedCallback;
		CoerceValueCallback = coerceValueCallback;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal object GetDefaultValue(DependencyObject owner, DependencyProperty property)
	{
		if (!(_defaultValue is DefaultValueFactory defaultValueFactory))
		{
			return _defaultValue;
		}
		if (owner.IsSealed)
		{
			return defaultValueFactory.DefaultValue;
		}
		object cachedDefaultValue = GetCachedDefaultValue(owner, property);
		if (cachedDefaultValue != DependencyProperty.UnsetValue)
		{
			return cachedDefaultValue;
		}
		cachedDefaultValue = defaultValueFactory.CreateDefaultValue(owner, property);
		property.ValidateFactoryDefaultValue(cachedDefaultValue);
		SetCachedDefaultValue(owner, property, cachedDefaultValue);
		return cachedDefaultValue;
	}

	private object GetCachedDefaultValue(DependencyObject owner, DependencyProperty property)
	{
		FrugalMapBase value = _defaultValueFactoryCache.GetValue(owner);
		if (value == null)
		{
			return DependencyProperty.UnsetValue;
		}
		return value.Search(property.GlobalIndex);
	}

	private void SetCachedDefaultValue(DependencyObject owner, DependencyProperty property, object value)
	{
		FrugalMapBase frugalMapBase = _defaultValueFactoryCache.GetValue(owner);
		if (frugalMapBase == null)
		{
			frugalMapBase = new SingleObjectMap();
			_defaultValueFactoryCache.SetValue(owner, frugalMapBase);
		}
		else if (!(frugalMapBase is HashObjectMap))
		{
			FrugalMapBase frugalMapBase2 = new HashObjectMap();
			frugalMapBase.Promote(frugalMapBase2);
			frugalMapBase = frugalMapBase2;
			_defaultValueFactoryCache.SetValue(owner, frugalMapBase);
		}
		frugalMapBase.InsertEntry(property.GlobalIndex, value);
	}

	internal void ClearCachedDefaultValue(DependencyObject owner, DependencyProperty property)
	{
		FrugalMapBase value = _defaultValueFactoryCache.GetValue(owner);
		if (value.Count == 1)
		{
			_defaultValueFactoryCache.ClearValue(owner);
		}
		else
		{
			value.RemoveEntry(property.GlobalIndex);
		}
	}

	internal static void PromoteAllCachedDefaultValues(DependencyObject owner)
	{
		_defaultValueFactoryCache.GetValue(owner)?.Iterate(null, _promotionCallback);
	}

	internal static void RemoveAllCachedDefaultValues(Freezable owner)
	{
		FrugalMapBase value = _defaultValueFactoryCache.GetValue(owner);
		if (value != null)
		{
			value.Iterate(null, _removalCallback);
			_defaultValueFactoryCache.ClearValue(owner);
		}
	}

	private static void DefaultValueCacheRemovalCallback(ArrayList list, int key, object value)
	{
		if (value is Freezable freezable)
		{
			freezable.ClearContextAndHandlers();
			freezable.Freeze();
		}
	}

	private static void DefaultValueCachePromotionCallback(ArrayList list, int key, object value)
	{
		if (value is Freezable freezable)
		{
			freezable.FireChanged();
		}
	}

	internal bool DefaultValueWasSet()
	{
		return IsModified(MetadataFlags.DefaultValueModifiedID);
	}

	private static bool DefaultFreezeValueCallback(DependencyObject d, DependencyProperty dp, EntryIndex entryIndex, PropertyMetadata metadata, bool isChecking)
	{
		if (isChecking && d.HasExpression(entryIndex, dp))
		{
			if (TraceFreezable.IsEnabled)
			{
				TraceFreezable.Trace(TraceEventType.Warning, TraceFreezable.UnableToFreezeExpression, d, dp, dp.OwnerType);
			}
			return false;
		}
		if (!dp.IsValueType)
		{
			object value = d.GetValueEntry(entryIndex, dp, metadata, RequestFlags.FullyResolved).Value;
			if (value != null)
			{
				if (value is Freezable freezable)
				{
					if (!freezable.Freeze(isChecking))
					{
						if (TraceFreezable.IsEnabled)
						{
							TraceFreezable.Trace(TraceEventType.Warning, TraceFreezable.UnableToFreezeFreezableSubProperty, d, dp, dp.OwnerType);
						}
						return false;
					}
				}
				else if (value is DispatcherObject { Dispatcher: not null } dispatcherObject)
				{
					if (TraceFreezable.IsEnabled)
					{
						TraceFreezable.Trace(TraceEventType.Warning, TraceFreezable.UnableToFreezeDispatcherObjectWithThreadAffinity, d, dp, dp.OwnerType, dispatcherObject);
					}
					return false;
				}
			}
		}
		return true;
	}

	internal virtual PropertyMetadata CreateInstance()
	{
		return new PropertyMetadata();
	}

	internal PropertyMetadata Copy(DependencyProperty dp)
	{
		PropertyMetadata propertyMetadata = CreateInstance();
		propertyMetadata.InvokeMerge(this, dp);
		return propertyMetadata;
	}

	/// <summary>Merges this metadata with the base metadata. </summary>
	/// <param name="baseMetadata">The base metadata to merge with this instance's values.</param>
	/// <param name="dp">The dependency property to which this metadata is being applied.</param>
	protected virtual void Merge(PropertyMetadata baseMetadata, DependencyProperty dp)
	{
		if (baseMetadata == null)
		{
			throw new ArgumentNullException("baseMetadata");
		}
		if (Sealed)
		{
			throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
		}
		if (!IsModified(MetadataFlags.DefaultValueModifiedID))
		{
			_defaultValue = baseMetadata.DefaultValue;
		}
		if (baseMetadata.PropertyChangedCallback != null)
		{
			Delegate[] invocationList = baseMetadata.PropertyChangedCallback.GetInvocationList();
			if (invocationList.Length != 0)
			{
				PropertyChangedCallback a = (PropertyChangedCallback)invocationList[0];
				for (int i = 1; i < invocationList.Length; i++)
				{
					a = (PropertyChangedCallback)Delegate.Combine(a, (PropertyChangedCallback)invocationList[i]);
				}
				a = (PropertyChangedCallback)Delegate.Combine(a, _propertyChangedCallback);
				_propertyChangedCallback = a;
			}
		}
		if (_coerceValueCallback == null)
		{
			_coerceValueCallback = baseMetadata.CoerceValueCallback;
		}
		if (_freezeValueCallback == null)
		{
			_freezeValueCallback = baseMetadata.FreezeValueCallback;
		}
	}

	internal void InvokeMerge(PropertyMetadata baseMetadata, DependencyProperty dp)
	{
		Merge(baseMetadata, dp);
	}

	/// <summary>Called when this metadata has been applied to a property, which indicates that the metadata is being sealed. </summary>
	/// <param name="dp">The dependency property to which the metadata has been applied.</param>
	/// <param name="targetType">The type associated with this metadata if this is type-specific metadata. If this is default metadata, this value is a null reference.</param>
	protected virtual void OnApply(DependencyProperty dp, Type targetType)
	{
	}

	internal void Seal(DependencyProperty dp, Type targetType)
	{
		OnApply(dp, targetType);
		Sealed = true;
	}

	private void SetModified(MetadataFlags id)
	{
		_flags |= id;
	}

	private bool IsModified(MetadataFlags id)
	{
		return (id & _flags) != 0;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void WriteFlag(MetadataFlags id, bool value)
	{
		if (value)
		{
			_flags |= id;
		}
		else
		{
			_flags &= ~id;
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal bool ReadFlag(MetadataFlags id)
	{
		return (id & _flags) != 0;
	}
}
