using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Markup;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.ComponentModel;
using MS.Internal.KnownBoxes;
using MS.Internal.WindowsBase;
using MS.Utility;

namespace System.Windows;

/// <summary>Represents an object that participates in the dependency property system.</summary>
[TypeDescriptionProvider(typeof(DependencyObjectProvider))]
[NameScopeProperty("NameScope", typeof(NameScope))]
public class DependencyObject : DispatcherObject
{
	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static readonly DependencyProperty DirectDependencyProperty = DependencyProperty.Register("__Direct", typeof(object), typeof(DependencyProperty));

	private static readonly UncommonField<EventHandler> InheritanceContextChangedHandlersField = new UncommonField<EventHandler>();

	private DependencyObjectType _dType;

	internal object _contextStorage;

	private EffectiveValueEntry[] _effectiveValues;

	private uint _packedData;

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static readonly object ExpressionInAlternativeStore = new NamedObject("ExpressionInAlternativeStore");

	private static AlternativeExpressionStorageCallback _getExpressionCore;

	internal static readonly UncommonField<object> DependentListMapField = new UncommonField<object>();

	internal static DependencyObjectType DType = DependencyObjectType.FromSystemTypeInternal(typeof(DependencyObject));

	private const int NestedOperationMaximum = 153;

	/// <summary>Gets the <see cref="T:System.Windows.DependencyObjectType" /> that wraps the CLR type of this instance. </summary>
	/// <returns>A <see cref="T:System.Windows.DependencyObjectType" /> that wraps the CLR type of this instance. </returns>
	public DependencyObjectType DependencyObjectType
	{
		get
		{
			if (_dType == null)
			{
				_dType = DependencyObjectType.FromSystemTypeInternal(GetType());
			}
			return _dType;
		}
	}

	/// <summary>Gets a value that indicates whether this instance is currently sealed (read-only).</summary>
	/// <returns>true if this instance is sealed; otherwise, false.</returns>
	public bool IsSealed => DO_Sealed;

	private bool CanModifyEffectiveValues
	{
		get
		{
			return (_packedData & 0x80000) != 0;
		}
		set
		{
			if (value)
			{
				_packedData |= 524288u;
			}
			else
			{
				_packedData &= 4294443007u;
			}
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal bool IsInheritanceContextSealed
	{
		get
		{
			return (_packedData & 0x1000000) != 0;
		}
		set
		{
			if (value)
			{
				_packedData |= 16777216u;
			}
			else
			{
				_packedData &= 4278190079u;
			}
		}
	}

	private bool DO_Sealed
	{
		get
		{
			return (_packedData & 0x400000) != 0;
		}
		set
		{
			if (value)
			{
				_packedData |= 4194304u;
			}
			else
			{
				_packedData &= 4290772991u;
			}
		}
	}

	internal bool Freezable_Frozen
	{
		get
		{
			return DO_Sealed;
		}
		set
		{
			DO_Sealed = value;
		}
	}

	internal bool Freezable_HasMultipleInheritanceContexts
	{
		get
		{
			return (_packedData & 0x2000000) != 0;
		}
		set
		{
			if (value)
			{
				_packedData |= 33554432u;
			}
			else
			{
				_packedData &= 4261412863u;
			}
		}
	}

	internal bool Freezable_UsingHandlerList
	{
		get
		{
			return (_packedData & 0x4000000) != 0;
		}
		set
		{
			if (value)
			{
				_packedData |= 67108864u;
			}
			else
			{
				_packedData &= 4227858431u;
			}
		}
	}

	internal bool Freezable_UsingContextList
	{
		get
		{
			return (_packedData & 0x8000000) != 0;
		}
		set
		{
			if (value)
			{
				_packedData |= 134217728u;
			}
			else
			{
				_packedData &= 4160749567u;
			}
		}
	}

	internal bool Freezable_UsingSingletonHandler
	{
		get
		{
			return (_packedData & 0x10000000) != 0;
		}
		set
		{
			if (value)
			{
				_packedData |= 268435456u;
			}
			else
			{
				_packedData &= 4026531839u;
			}
		}
	}

	internal bool Freezable_UsingSingletonContext
	{
		get
		{
			return (_packedData & 0x20000000) != 0;
		}
		set
		{
			if (value)
			{
				_packedData |= 536870912u;
			}
			else
			{
				_packedData &= 3758096383u;
			}
		}
	}

	internal bool Animatable_IsResourceInvalidationNecessary
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return (_packedData & 0x40000000) != 0;
		}
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		set
		{
			if (value)
			{
				_packedData |= 1073741824u;
			}
			else
			{
				_packedData &= 3221225471u;
			}
		}
	}

	internal bool IAnimatable_HasAnimatedProperties
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return (_packedData & 0x80000000u) != 0;
		}
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		set
		{
			if (value)
			{
				_packedData |= 2147483648u;
			}
			else
			{
				_packedData &= 2147483647u;
			}
		}
	}

	internal virtual DependencyObject InheritanceContext
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return null;
		}
	}

	internal virtual bool HasMultipleInheritanceContexts => false;

	internal bool CanBeInheritanceContext
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return (_packedData & 0x200000) != 0;
		}
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		set
		{
			if (value)
			{
				_packedData |= 2097152u;
			}
			else
			{
				_packedData &= 4292870143u;
			}
		}
	}

	internal EffectiveValueEntry[] EffectiveValues
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return _effectiveValues;
		}
	}

	internal uint EffectiveValuesCount
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return _packedData & 0x3FF;
		}
		private set
		{
			_packedData = (_packedData & 0xFFFFFC00u) | (value & 0x3FF);
		}
	}

	internal uint InheritableEffectiveValuesCount
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return (_packedData >> 10) & 0x1FF;
		}
		set
		{
			_packedData = ((value & 0x1FF) << 10) | (_packedData & 0xFFF803FFu);
		}
	}

	private bool IsInPropertyInitialization
	{
		get
		{
			return (_packedData & 0x800000) != 0;
		}
		set
		{
			if (value)
			{
				_packedData |= 8388608u;
			}
			else
			{
				_packedData &= 4286578687u;
			}
		}
	}

	internal DependencyObject InheritanceParent
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			if ((_packedData & 0x3E100000) == 0)
			{
				return (DependencyObject)_contextStorage;
			}
			return null;
		}
	}

	internal bool IsSelfInheritanceParent
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		get
		{
			return (_packedData & 0x100000) != 0;
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal virtual int EffectiveValuesInitialSize => 2;

	internal event EventHandler InheritanceContextChanged
	{
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		add
		{
			EventHandler value2 = InheritanceContextChangedHandlersField.GetValue(this);
			value2 = ((value2 == null) ? value : ((EventHandler)Delegate.Combine(value2, value)));
			InheritanceContextChangedHandlersField.SetValue(this, value2);
		}
		[MS.Internal.WindowsBase.FriendAccessAllowed]
		remove
		{
			EventHandler value2 = InheritanceContextChangedHandlersField.GetValue(this);
			if (value2 != null)
			{
				value2 = (EventHandler)Delegate.Remove(value2, value);
				if (value2 == null)
				{
					InheritanceContextChangedHandlersField.ClearValue(this);
				}
				else
				{
					InheritanceContextChangedHandlersField.SetValue(this, value2);
				}
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DependencyObject" /> class. </summary>
	public DependencyObject()
	{
		Initialize();
	}

	private void Initialize()
	{
		CanBeInheritanceContext = true;
		CanModifyEffectiveValues = true;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal virtual void Seal()
	{
		PropertyMetadata.PromoteAllCachedDefaultValues(this);
		DependentListMapField.ClearValue(this);
		DO_Sealed = true;
	}

	/// <summary>Determines whether a provided <see cref="T:System.Windows.DependencyObject" /> is equivalent to the current <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>true if the two instances are the same; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Windows.DependencyObject" />  to compare to the current instance.</param>
	public sealed override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	/// <summary>Gets a hash code for this <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <returns>A signed 32-bit integer hash code.</returns>
	public sealed override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Returns the current effective value of a dependency property on this instance of a <see cref="T:System.Windows.DependencyObject" />. </summary>
	/// <returns>Returns the current effective value.</returns>
	/// <param name="dp">The <see cref="T:System.Windows.DependencyProperty" /> identifier of the property to retrieve the value for.</param>
	/// <exception cref="T:System.InvalidOperationException">The specified <paramref name="dp" /> or its value was invalid, or the specified <paramref name="dp" /> does not exist.</exception>
	public object GetValue(DependencyProperty dp)
	{
		VerifyAccess();
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		return GetValueEntry(LookupEntry(dp.GlobalIndex), dp, null, RequestFlags.FullyResolved).Value;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal EffectiveValueEntry GetValueEntry(EntryIndex entryIndex, DependencyProperty dp, PropertyMetadata metadata, RequestFlags requests)
	{
		EffectiveValueEntry result;
		if (dp.ReadOnly)
		{
			if (metadata == null)
			{
				metadata = dp.GetMetadata(DependencyObjectType);
			}
			GetReadOnlyValueCallback getReadOnlyValueCallback = metadata.GetReadOnlyValueCallback;
			if (getReadOnlyValueCallback != null)
			{
				result = new EffectiveValueEntry(dp);
				result.Value = getReadOnlyValueCallback(this, out var source);
				result.BaseValueSourceInternal = source;
				return result;
			}
		}
		result = ((!entryIndex.Found) ? new EffectiveValueEntry(dp, BaseValueSourceInternal.Unknown) : (((requests & RequestFlags.RawEntry) == 0) ? GetEffectiveValue(entryIndex, dp, requests) : _effectiveValues[entryIndex.Index]));
		if (result.Value == DependencyProperty.UnsetValue)
		{
			if (dp.IsPotentiallyInherited)
			{
				if (metadata == null)
				{
					metadata = dp.GetMetadata(DependencyObjectType);
				}
				if (metadata.IsInherited)
				{
					DependencyObject inheritanceParent = InheritanceParent;
					if (inheritanceParent != null)
					{
						entryIndex = inheritanceParent.LookupEntry(dp.GlobalIndex);
						if (entryIndex.Found)
						{
							result = inheritanceParent.GetEffectiveValue(entryIndex, dp, requests & RequestFlags.DeferredReferences);
							result.BaseValueSourceInternal = BaseValueSourceInternal.Inherited;
						}
					}
				}
				if (result.Value != DependencyProperty.UnsetValue)
				{
					return result;
				}
			}
			if ((requests & RequestFlags.SkipDefault) == 0)
			{
				if (dp.IsPotentiallyUsingDefaultValueFactory)
				{
					if (metadata == null)
					{
						metadata = dp.GetMetadata(DependencyObjectType);
					}
					if ((requests & (RequestFlags)20) != 0 && metadata.UsingDefaultValueFactory)
					{
						result.BaseValueSourceInternal = BaseValueSourceInternal.Default;
						result.Value = new DeferredMutableDefaultReference(metadata, this, dp);
						return result;
					}
				}
				else if (!dp.IsDefaultValueChanged)
				{
					return EffectiveValueEntry.CreateDefaultValueEntry(dp, dp.DefaultMetadata.DefaultValue);
				}
				if (metadata == null)
				{
					metadata = dp.GetMetadata(DependencyObjectType);
				}
				return EffectiveValueEntry.CreateDefaultValueEntry(dp, metadata.GetDefaultValue(this, dp));
			}
		}
		return result;
	}

	private EffectiveValueEntry GetEffectiveValue(EntryIndex entryIndex, DependencyProperty dp, RequestFlags requests)
	{
		EffectiveValueEntry effectiveValueEntry = _effectiveValues[entryIndex.Index];
		EffectiveValueEntry flattenedEntry = effectiveValueEntry.GetFlattenedEntry(requests);
		if ((requests & (RequestFlags)20) != 0 || !flattenedEntry.IsDeferredReference)
		{
			return flattenedEntry;
		}
		if (!effectiveValueEntry.HasModifiers)
		{
			if (!effectiveValueEntry.HasExpressionMarker)
			{
				object value = ((DeferredReference)effectiveValueEntry.Value).GetValue(effectiveValueEntry.BaseValueSourceInternal);
				if (!dp.IsValidValue(value))
				{
					throw new InvalidOperationException(SR.Format(SR.InvalidPropertyValue, value, dp.Name));
				}
				entryIndex = CheckEntryIndex(entryIndex, dp.GlobalIndex);
				effectiveValueEntry.Value = value;
				_effectiveValues[entryIndex.Index] = effectiveValueEntry;
				return effectiveValueEntry;
			}
		}
		else
		{
			ModifiedValue modifiedValue = effectiveValueEntry.ModifiedValue;
			DeferredReference deferredReference = null;
			bool flag = false;
			if (effectiveValueEntry.IsCoercedWithCurrentValue && !effectiveValueEntry.IsAnimated)
			{
				deferredReference = modifiedValue.CoercedValue as DeferredReference;
			}
			if (deferredReference == null && effectiveValueEntry.IsExpression && !effectiveValueEntry.IsAnimated && !effectiveValueEntry.IsCoerced)
			{
				deferredReference = (DeferredReference)modifiedValue.ExpressionValue;
				flag = true;
			}
			if (deferredReference == null)
			{
				return flattenedEntry;
			}
			object value2 = deferredReference.GetValue(effectiveValueEntry.BaseValueSourceInternal);
			if (!dp.IsValidValue(value2))
			{
				throw new InvalidOperationException(SR.Format(SR.InvalidPropertyValue, value2, dp.Name));
			}
			entryIndex = CheckEntryIndex(entryIndex, dp.GlobalIndex);
			if (flag)
			{
				effectiveValueEntry.SetExpressionValue(value2, modifiedValue.BaseValue);
			}
			else
			{
				effectiveValueEntry.SetCoercedValue(value2, null, skipBaseValueChecks: true, effectiveValueEntry.IsCoercedWithCurrentValue);
			}
			_effectiveValues[entryIndex.Index] = effectiveValueEntry;
			flattenedEntry.Value = value2;
		}
		return flattenedEntry;
	}

	/// <summary>Sets the local value of a dependency property, specified by its dependency property identifier. </summary>
	/// <param name="dp">The identifier of the dependency property to set.</param>
	/// <param name="value">The new local value.</param>
	/// <exception cref="T:System.InvalidOperationException">Attempted to modify a read-only dependency property, or a property on a sealed <see cref="T:System.Windows.DependencyObject" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> was not the correct type as registered for the <paramref name="dp" /> property.</exception>
	public void SetValue(DependencyProperty dp, object value)
	{
		VerifyAccess();
		PropertyMetadata metadata = SetupPropertyChange(dp);
		SetValueCommon(dp, value, metadata, coerceWithDeferredReference: false, coerceWithCurrentValue: false, OperationType.Unknown, isInternal: false);
	}

	/// <summary>Sets the value of a dependency property without changing its value source. </summary>
	/// <param name="dp">The identifier of the dependency property to set.</param>
	/// <param name="value">The new local value.</param>
	/// <exception cref="T:System.InvalidOperationException">Attempted to modify a read-only dependency property, or a property on a sealed <see cref="T:System.Windows.DependencyObject" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> was not the correct type as registered for the <paramref name="dp" /> property.</exception>
	public void SetCurrentValue(DependencyProperty dp, object value)
	{
		VerifyAccess();
		PropertyMetadata metadata = SetupPropertyChange(dp);
		SetValueCommon(dp, value, metadata, coerceWithDeferredReference: false, coerceWithCurrentValue: true, OperationType.Unknown, isInternal: false);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void SetValue(DependencyProperty dp, bool value)
	{
		SetValue(dp, BooleanBoxes.Box(value));
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void SetCurrentValue(DependencyProperty dp, bool value)
	{
		SetCurrentValue(dp, BooleanBoxes.Box(value));
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void SetValueInternal(DependencyProperty dp, object value)
	{
		VerifyAccess();
		PropertyMetadata metadata = SetupPropertyChange(dp);
		SetValueCommon(dp, value, metadata, coerceWithDeferredReference: false, coerceWithCurrentValue: false, OperationType.Unknown, isInternal: true);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void SetCurrentValueInternal(DependencyProperty dp, object value)
	{
		VerifyAccess();
		PropertyMetadata metadata = SetupPropertyChange(dp);
		SetValueCommon(dp, value, metadata, coerceWithDeferredReference: false, coerceWithCurrentValue: true, OperationType.Unknown, isInternal: true);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void SetDeferredValue(DependencyProperty dp, DeferredReference deferredReference)
	{
		PropertyMetadata metadata = SetupPropertyChange(dp);
		SetValueCommon(dp, deferredReference, metadata, coerceWithDeferredReference: true, coerceWithCurrentValue: false, OperationType.Unknown, isInternal: false);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void SetCurrentDeferredValue(DependencyProperty dp, DeferredReference deferredReference)
	{
		PropertyMetadata metadata = SetupPropertyChange(dp);
		SetValueCommon(dp, deferredReference, metadata, coerceWithDeferredReference: true, coerceWithCurrentValue: true, OperationType.Unknown, isInternal: false);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void SetMutableDefaultValue(DependencyProperty dp, object value)
	{
		PropertyMetadata metadata = SetupPropertyChange(dp);
		SetValueCommon(dp, value, metadata, coerceWithDeferredReference: false, coerceWithCurrentValue: false, OperationType.ChangeMutableDefaultValue, isInternal: false);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void SetValue(DependencyPropertyKey dp, bool value)
	{
		SetValue(dp, BooleanBoxes.Box(value));
	}

	/// <summary>Sets the local value of a read-only dependency property, specified by the <see cref="T:System.Windows.DependencyPropertyKey" /> identifier of the dependency property. </summary>
	/// <param name="key">The <see cref="T:System.Windows.DependencyPropertyKey" /> identifier of the property to set.</param>
	/// <param name="value">The new local value.</param>
	public void SetValue(DependencyPropertyKey key, object value)
	{
		VerifyAccess();
		DependencyProperty dp;
		PropertyMetadata metadata = SetupPropertyChange(key, out dp);
		SetValueCommon(dp, value, metadata, coerceWithDeferredReference: false, coerceWithCurrentValue: false, OperationType.Unknown, isInternal: false);
	}

	private PropertyMetadata SetupPropertyChange(DependencyProperty dp)
	{
		if (dp != null)
		{
			if (!dp.ReadOnly)
			{
				return dp.GetMetadata(DependencyObjectType);
			}
			throw new InvalidOperationException(SR.Format(SR.ReadOnlyChangeNotAllowed, dp.Name));
		}
		throw new ArgumentNullException("dp");
	}

	private PropertyMetadata SetupPropertyChange(DependencyPropertyKey key, out DependencyProperty dp)
	{
		if (key != null)
		{
			dp = key.DependencyProperty;
			dp.VerifyReadOnlyKey(key);
			return dp.GetMetadata(DependencyObjectType);
		}
		throw new ArgumentNullException("key");
	}

	private void SetValueCommon(DependencyProperty dp, object value, PropertyMetadata metadata, bool coerceWithDeferredReference, bool coerceWithCurrentValue, OperationType operationType, bool isInternal)
	{
		if (IsSealed)
		{
			throw new InvalidOperationException(SR.Format(SR.SetOnReadOnlyObjectNotAllowed, this));
		}
		Expression expression = null;
		DependencySource[] array = null;
		EntryIndex entryIndex = LookupEntry(dp.GlobalIndex);
		if (value == DependencyProperty.UnsetValue)
		{
			ClearValueCommon(entryIndex, dp, metadata);
			return;
		}
		bool flag = false;
		bool flag2 = value == ExpressionInAlternativeStore;
		if (!flag2)
		{
			bool flag3 = (isInternal ? dp.IsValidValueInternal(value) : dp.IsValidValue(value));
			if (!flag3 || dp.IsObjectType)
			{
				expression = value as Expression;
				if (expression != null)
				{
					if (!expression.Attachable)
					{
						throw new ArgumentException(SR.SharingNonSharableExpression);
					}
					array = expression.GetSources();
					ValidateSources(this, array, expression);
				}
				else
				{
					flag = value is DeferredReference;
					if (!flag && !flag3)
					{
						throw new ArgumentException(SR.Format(SR.InvalidPropertyValue, value, dp.Name));
					}
				}
			}
		}
		EffectiveValueEntry oldEntry;
		if (operationType == OperationType.ChangeMutableDefaultValue)
		{
			oldEntry = new EffectiveValueEntry(dp, BaseValueSourceInternal.Default);
			oldEntry.Value = value;
		}
		else
		{
			oldEntry = GetValueEntry(entryIndex, dp, metadata, RequestFlags.RawEntry);
		}
		Expression expression2 = (oldEntry.HasExpressionMarker ? _getExpressionCore(this, dp, metadata) : (oldEntry.IsExpression ? (oldEntry.LocalValue as Expression) : null));
		bool flag4 = false;
		if (expression2 != null && expression == null)
		{
			if (flag)
			{
				value = ((DeferredReference)value).GetValue(BaseValueSourceInternal.Local);
			}
			flag4 = expression2.SetValue(this, dp, value);
			entryIndex = CheckEntryIndex(entryIndex, dp.GlobalIndex);
		}
		EffectiveValueEntry newEntry;
		if (flag4)
		{
			newEntry = ((!entryIndex.Found) ? EffectiveValueEntry.CreateDefaultValueEntry(dp, metadata.GetDefaultValue(this, dp)) : _effectiveValues[entryIndex.Index]);
			coerceWithCurrentValue = false;
		}
		else
		{
			if (coerceWithCurrentValue && expression2 != null)
			{
				expression2 = null;
			}
			newEntry = new EffectiveValueEntry(dp, BaseValueSourceInternal.Local);
			if (expression2 != null)
			{
				DependencySource[] sources = expression2.GetSources();
				UpdateSourceDependentLists(this, dp, sources, expression2, add: false);
				expression2.OnDetach(this, dp);
				expression2.MarkDetached();
				entryIndex = CheckEntryIndex(entryIndex, dp.GlobalIndex);
			}
			if (expression == null)
			{
				newEntry.HasExpressionMarker = flag2;
				newEntry.Value = value;
			}
			else
			{
				SetEffectiveValue(entryIndex, dp, dp.GlobalIndex, metadata, expression, BaseValueSourceInternal.Local);
				object defaultValue = metadata.GetDefaultValue(this, dp);
				entryIndex = CheckEntryIndex(entryIndex, dp.GlobalIndex);
				SetExpressionValue(entryIndex, defaultValue, expression);
				UpdateSourceDependentLists(this, dp, array, expression, add: true);
				expression.MarkAttached();
				expression.OnAttach(this, dp);
				entryIndex = CheckEntryIndex(entryIndex, dp.GlobalIndex);
				newEntry = EvaluateExpression(entryIndex, dp, expression, metadata, oldEntry, _effectiveValues[entryIndex.Index]);
				entryIndex = CheckEntryIndex(entryIndex, dp.GlobalIndex);
			}
		}
		UpdateEffectiveValue(entryIndex, dp, metadata, oldEntry, ref newEntry, coerceWithDeferredReference, coerceWithCurrentValue, operationType);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal bool ProvideSelfAsInheritanceContext(object value, DependencyProperty dp)
	{
		if (value is DependencyObject doValue)
		{
			return ProvideSelfAsInheritanceContext(doValue, dp);
		}
		return false;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal bool ProvideSelfAsInheritanceContext(DependencyObject doValue, DependencyProperty dp)
	{
		if (doValue != null && ShouldProvideInheritanceContext(doValue, dp) && (doValue is Freezable || (CanBeInheritanceContext && !doValue.IsInheritanceContextSealed)))
		{
			DependencyObject inheritanceContext = doValue.InheritanceContext;
			doValue.AddInheritanceContext(this, dp);
			if (this == doValue.InheritanceContext)
			{
				return this != inheritanceContext;
			}
			return false;
		}
		return false;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal bool RemoveSelfAsInheritanceContext(object value, DependencyProperty dp)
	{
		if (value is DependencyObject doValue)
		{
			return RemoveSelfAsInheritanceContext(doValue, dp);
		}
		return false;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal bool RemoveSelfAsInheritanceContext(DependencyObject doValue, DependencyProperty dp)
	{
		if (doValue != null && ShouldProvideInheritanceContext(doValue, dp) && (doValue is Freezable || (CanBeInheritanceContext && !doValue.IsInheritanceContextSealed)))
		{
			DependencyObject inheritanceContext = doValue.InheritanceContext;
			doValue.RemoveInheritanceContext(this, dp);
			if (this == inheritanceContext)
			{
				return doValue.InheritanceContext != inheritanceContext;
			}
			return false;
		}
		return false;
	}

	/// <summary>Clears the local value of a property. The property to be cleared is specified by a <see cref="T:System.Windows.DependencyProperty" /> identifier. </summary>
	/// <param name="dp">The dependency property to be cleared, identified by a <see cref="T:System.Windows.DependencyProperty" /> object reference.</param>
	/// <exception cref="T:System.InvalidOperationException">Attempted to call <see cref="M:System.Windows.DependencyObject.ClearValue(System.Windows.DependencyProperty)" /> on a sealed <see cref="T:System.Windows.DependencyObject" />.</exception>
	public void ClearValue(DependencyProperty dp)
	{
		VerifyAccess();
		PropertyMetadata metadata = SetupPropertyChange(dp);
		EntryIndex entryIndex = LookupEntry(dp.GlobalIndex);
		ClearValueCommon(entryIndex, dp, metadata);
	}

	/// <summary>Clears the local value of a read-only property. The property to be cleared is specified by a <see cref="T:System.Windows.DependencyPropertyKey" />. </summary>
	/// <param name="key">The key for the dependency property to be cleared.</param>
	/// <exception cref="T:System.InvalidOperationException">Attempted to call <see cref="M:System.Windows.DependencyObject.ClearValue(System.Windows.DependencyProperty)" /> on a sealed <see cref="T:System.Windows.DependencyObject" />.</exception>
	public void ClearValue(DependencyPropertyKey key)
	{
		VerifyAccess();
		DependencyProperty dp;
		PropertyMetadata metadata = SetupPropertyChange(key, out dp);
		EntryIndex entryIndex = LookupEntry(dp.GlobalIndex);
		ClearValueCommon(entryIndex, dp, metadata);
	}

	private void ClearValueCommon(EntryIndex entryIndex, DependencyProperty dp, PropertyMetadata metadata)
	{
		if (IsSealed)
		{
			throw new InvalidOperationException(SR.Format(SR.ClearOnReadOnlyObjectNotAllowed, this));
		}
		EffectiveValueEntry valueEntry = GetValueEntry(entryIndex, dp, metadata, RequestFlags.RawEntry);
		object localValue = valueEntry.LocalValue;
		Expression expression = (valueEntry.IsExpression ? (localValue as Expression) : null);
		if (expression != null)
		{
			DependencySource[] sources = expression.GetSources();
			UpdateSourceDependentLists(this, dp, sources, expression, add: false);
			expression.OnDetach(this, dp);
			expression.MarkDetached();
			entryIndex = CheckEntryIndex(entryIndex, dp.GlobalIndex);
		}
		EffectiveValueEntry newEntry = new EffectiveValueEntry(dp, BaseValueSourceInternal.Local);
		UpdateEffectiveValue(entryIndex, dp, metadata, valueEntry, ref newEntry, coerceWithDeferredReference: false, coerceWithCurrentValue: false, OperationType.Unknown);
	}

	internal bool ContainsValue(DependencyProperty dp)
	{
		EntryIndex entryIndex = LookupEntry(dp.GlobalIndex);
		if (!entryIndex.Found)
		{
			return false;
		}
		EffectiveValueEntry effectiveValueEntry = _effectiveValues[entryIndex.Index];
		return (effectiveValueEntry.IsCoercedWithCurrentValue ? effectiveValueEntry.ModifiedValue.CoercedValue : effectiveValueEntry.LocalValue) != DependencyProperty.UnsetValue;
	}

	internal static void ChangeExpressionSources(Expression expr, DependencyObject d, DependencyProperty dp, DependencySource[] newSources)
	{
		if (!expr.ForwardsInvalidations)
		{
			EntryIndex entryIndex = d.LookupEntry(dp.GlobalIndex);
			if (!entryIndex.Found || d._effectiveValues[entryIndex.Index].LocalValue != expr)
			{
				throw new ArgumentException(SR.SourceChangeExpressionMismatch);
			}
		}
		DependencySource[] sources = expr.GetSources();
		if (sources != null)
		{
			UpdateSourceDependentLists(d, dp, sources, expr, add: false);
		}
		if (newSources != null)
		{
			UpdateSourceDependentLists(d, dp, newSources, expr, add: true);
		}
	}

	/// <summary>Coerces the value of the specified dependency property. This is accomplished by invoking any <see cref="T:System.Windows.CoerceValueCallback" /> function specified in property metadata for the dependency property as it exists on the calling <see cref="T:System.Windows.DependencyObject" />.</summary>
	/// <param name="dp">The identifier for the dependency property to coerce.</param>
	/// <exception cref="T:System.InvalidOperationException">The specified <paramref name="dp" /> or its value were invalid or do not exist.</exception>
	public void CoerceValue(DependencyProperty dp)
	{
		VerifyAccess();
		EntryIndex entryIndex = LookupEntry(dp.GlobalIndex);
		PropertyMetadata metadata = dp.GetMetadata(DependencyObjectType);
		if (entryIndex.Found)
		{
			EffectiveValueEntry valueEntry = GetValueEntry(entryIndex, dp, metadata, RequestFlags.RawEntry);
			if (valueEntry.IsCoercedWithCurrentValue)
			{
				SetCurrentValue(dp, valueEntry.ModifiedValue.CoercedValue);
				return;
			}
		}
		EffectiveValueEntry newEntry = new EffectiveValueEntry(dp, FullValueSource.IsCoerced);
		UpdateEffectiveValue(entryIndex, dp, metadata, default(EffectiveValueEntry), ref newEntry, coerceWithDeferredReference: false, coerceWithCurrentValue: false, OperationType.Unknown);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void InvalidateSubProperty(DependencyProperty dp)
	{
		NotifyPropertyChange(new DependencyPropertyChangedEventArgs(dp, dp.GetMetadata(DependencyObjectType), GetValue(dp)));
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void NotifySubPropertyChange(DependencyProperty dp)
	{
		InvalidateSubProperty(dp);
		if (this is Freezable freezable)
		{
			freezable.FireChanged();
		}
	}

	/// <summary>Re-evaluates the effective value for the specified dependency property</summary>
	/// <param name="dp">The <see cref="T:System.Windows.DependencyProperty" /> identifier of the property to invalidate.</param>
	public void InvalidateProperty(DependencyProperty dp)
	{
		InvalidateProperty(dp, preserveCurrentValue: false);
	}

	internal void InvalidateProperty(DependencyProperty dp, bool preserveCurrentValue)
	{
		VerifyAccess();
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		EffectiveValueEntry newEntry = new EffectiveValueEntry(dp, BaseValueSourceInternal.Unknown);
		newEntry.IsCoercedWithCurrentValue = preserveCurrentValue;
		UpdateEffectiveValue(LookupEntry(dp.GlobalIndex), dp, dp.GetMetadata(DependencyObjectType), default(EffectiveValueEntry), ref newEntry, coerceWithDeferredReference: false, coerceWithCurrentValue: false, OperationType.Unknown);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal UpdateResult UpdateEffectiveValue(EntryIndex entryIndex, DependencyProperty dp, PropertyMetadata metadata, EffectiveValueEntry oldEntry, ref EffectiveValueEntry newEntry, bool coerceWithDeferredReference, bool coerceWithCurrentValue, OperationType operationType)
	{
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		int targetIndex = dp.GlobalIndex;
		if (oldEntry.BaseValueSourceInternal == BaseValueSourceInternal.Unknown)
		{
			oldEntry = GetValueEntry(entryIndex, dp, metadata, RequestFlags.RawEntry);
		}
		object oldValue = oldEntry.GetFlattenedEntry(RequestFlags.FullyResolved).Value;
		object controlValue = null;
		if (coerceWithCurrentValue)
		{
			controlValue = newEntry.Value;
			newEntry = new EffectiveValueEntry(dp, FullValueSource.IsCoerced);
		}
		if (newEntry.BaseValueSourceInternal != 0 && newEntry.BaseValueSourceInternal < oldEntry.BaseValueSourceInternal)
		{
			return (UpdateResult)0;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (newEntry.Value == DependencyProperty.UnsetValue)
		{
			flag2 = newEntry.FullValueSource == FullValueSource.IsCoerced;
			flag = true;
			if (newEntry.BaseValueSourceInternal == BaseValueSourceInternal.Local)
			{
				flag3 = true;
			}
		}
		if (flag || (!newEntry.IsAnimated && (oldEntry.IsAnimated || (oldEntry.IsExpression && newEntry.IsExpression && newEntry.ModifiedValue.BaseValue == oldEntry.ModifiedValue.BaseValue))))
		{
			if (!flag2)
			{
				newEntry = EvaluateEffectiveValue(entryIndex, dp, metadata, oldEntry, newEntry, operationType);
				entryIndex = CheckEntryIndex(entryIndex, targetIndex);
				bool flag4 = newEntry.Value != DependencyProperty.UnsetValue;
				if (!flag4 && metadata.IsInherited)
				{
					DependencyObject inheritanceParent = InheritanceParent;
					if (inheritanceParent != null)
					{
						EntryIndex entryIndex2 = inheritanceParent.LookupEntry(dp.GlobalIndex);
						if (entryIndex2.Found)
						{
							flag4 = true;
							newEntry = inheritanceParent._effectiveValues[entryIndex2.Index].GetFlattenedEntry(RequestFlags.FullyResolved);
							newEntry.BaseValueSourceInternal = BaseValueSourceInternal.Inherited;
						}
					}
				}
				if (!flag4)
				{
					newEntry = EffectiveValueEntry.CreateDefaultValueEntry(dp, metadata.GetDefaultValue(this, dp));
				}
			}
			else if (!oldEntry.HasModifiers)
			{
				newEntry = oldEntry;
			}
			else
			{
				newEntry = new EffectiveValueEntry(dp, oldEntry.BaseValueSourceInternal);
				ModifiedValue modifiedValue = oldEntry.ModifiedValue;
				object baseValue2 = (newEntry.Value = modifiedValue.BaseValue);
				newEntry.HasExpressionMarker = oldEntry.HasExpressionMarker;
				if (oldEntry.IsExpression)
				{
					newEntry.SetExpressionValue(modifiedValue.ExpressionValue, baseValue2);
				}
				if (oldEntry.IsAnimated)
				{
					newEntry.SetAnimatedValue(modifiedValue.AnimatedValue, baseValue2);
				}
			}
		}
		if (coerceWithCurrentValue)
		{
			object value = newEntry.GetFlattenedEntry(RequestFlags.CoercionBaseValue).Value;
			ProcessCoerceValue(dp, metadata, ref entryIndex, ref targetIndex, ref newEntry, ref oldEntry, ref oldValue, value, controlValue, null, coerceWithDeferredReference, coerceWithCurrentValue, skipBaseValueChecks: false);
			entryIndex = CheckEntryIndex(entryIndex, targetIndex);
		}
		if (metadata.CoerceValueCallback != null && (!flag3 || newEntry.FullValueSource != (FullValueSource)1))
		{
			object value2 = newEntry.GetFlattenedEntry(RequestFlags.CoercionBaseValue).Value;
			ProcessCoerceValue(dp, metadata, ref entryIndex, ref targetIndex, ref newEntry, ref oldEntry, ref oldValue, value2, null, metadata.CoerceValueCallback, coerceWithDeferredReference, coerceWithCurrentValue: false, skipBaseValueChecks: false);
			entryIndex = CheckEntryIndex(entryIndex, targetIndex);
		}
		if (dp.DesignerCoerceValueCallback != null)
		{
			ProcessCoerceValue(dp, metadata, ref entryIndex, ref targetIndex, ref newEntry, ref oldEntry, ref oldValue, newEntry.GetFlattenedEntry(RequestFlags.FullyResolved).Value, null, dp.DesignerCoerceValueCallback, coerceWithDeferredReference: false, coerceWithCurrentValue: false, skipBaseValueChecks: true);
			entryIndex = CheckEntryIndex(entryIndex, targetIndex);
		}
		UpdateResult updateResult = (UpdateResult)0;
		if (newEntry.FullValueSource != (FullValueSource)1)
		{
			bool flag5 = false;
			if (newEntry.BaseValueSourceInternal == BaseValueSourceInternal.Inherited)
			{
				if (IsTreeWalkOperation(operationType) && (newEntry.IsCoerced || newEntry.IsAnimated))
				{
					operationType = OperationType.Unknown;
					updateResult |= UpdateResult.InheritedValueOverridden;
				}
				else if (!IsSelfInheritanceParent)
				{
					flag5 = true;
				}
			}
			if (flag5)
			{
				UnsetEffectiveValue(entryIndex, dp, metadata);
			}
			else
			{
				SetEffectiveValue(entryIndex, dp, metadata, newEntry, oldEntry);
			}
		}
		else
		{
			UnsetEffectiveValue(entryIndex, dp, metadata);
		}
		bool flag6 = !Equals(dp, oldValue, newEntry.GetFlattenedEntry(RequestFlags.FullyResolved).Value);
		if (flag6)
		{
			updateResult |= UpdateResult.ValueChanged;
		}
		if (flag6 || (operationType == OperationType.ChangeMutableDefaultValue && oldEntry.BaseValueSourceInternal != newEntry.BaseValueSourceInternal) || (metadata.IsInherited && oldEntry.BaseValueSourceInternal != newEntry.BaseValueSourceInternal && operationType != OperationType.AddChild && operationType != OperationType.RemoveChild && operationType != OperationType.Inherit))
		{
			updateResult |= UpdateResult.NotificationSent;
			NotifyPropertyChange(new DependencyPropertyChangedEventArgs(dp, metadata, flag6, oldEntry, newEntry, operationType));
		}
		bool flag7 = oldEntry.FullValueSource == (FullValueSource)11;
		bool flag8 = newEntry.FullValueSource == (FullValueSource)11;
		if (updateResult != 0 || flag7 != flag8)
		{
			if (flag7)
			{
				RemoveSelfAsInheritanceContext(oldEntry.LocalValue, dp);
			}
			if (flag8)
			{
				ProvideSelfAsInheritanceContext(newEntry.LocalValue, dp);
			}
		}
		return updateResult;
	}

	private void ProcessCoerceValue(DependencyProperty dp, PropertyMetadata metadata, ref EntryIndex entryIndex, ref int targetIndex, ref EffectiveValueEntry newEntry, ref EffectiveValueEntry oldEntry, ref object oldValue, object baseValue, object controlValue, CoerceValueCallback coerceValueCallback, bool coerceWithDeferredReference, bool coerceWithCurrentValue, bool skipBaseValueChecks)
	{
		if (newEntry.IsDeferredReference && (!coerceWithDeferredReference || dp.OwnerType != metadata.CoerceValueCallback.Method.DeclaringType))
		{
			baseValue = ((DeferredReference)baseValue).GetValue(newEntry.BaseValueSourceInternal);
			newEntry.SetCoersionBaseValue(baseValue);
			entryIndex = CheckEntryIndex(entryIndex, targetIndex);
		}
		object obj = (coerceWithCurrentValue ? controlValue : coerceValueCallback(this, baseValue));
		entryIndex = CheckEntryIndex(entryIndex, targetIndex);
		if (Equals(dp, obj, baseValue))
		{
			return;
		}
		if (obj == DependencyProperty.UnsetValue)
		{
			if (oldEntry.IsDeferredReference)
			{
				DeferredReference deferredReference = (DeferredReference)oldValue;
				oldValue = deferredReference.GetValue(oldEntry.BaseValueSourceInternal);
				entryIndex = CheckEntryIndex(entryIndex, targetIndex);
			}
			obj = oldValue;
		}
		if (!dp.IsValidValue(obj) && (!coerceWithCurrentValue || !(obj is DeferredReference)))
		{
			throw new ArgumentException(SR.Format(SR.InvalidPropertyValue, obj, dp.Name));
		}
		newEntry.SetCoercedValue(obj, baseValue, skipBaseValueChecks, coerceWithCurrentValue);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void NotifyPropertyChange(DependencyPropertyChangedEventArgs args)
	{
		OnPropertyChanged(args);
		if (!args.IsAValueChange && !args.IsASubPropertyChange)
		{
			return;
		}
		DependencyProperty property = args.Property;
		object value = DependentListMapField.GetValue(this);
		if (value == null)
		{
			return;
		}
		FrugalMap frugalMap = (FrugalMap)value;
		object obj = frugalMap[property.GlobalIndex];
		if (obj != DependencyProperty.UnsetValue)
		{
			if (((DependentList)obj).IsEmpty)
			{
				frugalMap[property.GlobalIndex] = DependencyProperty.UnsetValue;
			}
			else
			{
				((DependentList)obj).InvalidateDependents(this, args);
			}
		}
		property = DirectDependencyProperty;
		obj = frugalMap[property.GlobalIndex];
		if (obj != DependencyProperty.UnsetValue)
		{
			if (((DependentList)obj).IsEmpty)
			{
				frugalMap[property.GlobalIndex] = DependencyProperty.UnsetValue;
			}
			else
			{
				((DependentList)obj).InvalidateDependents(this, new DependencyPropertyChangedEventArgs(property, null, null));
			}
		}
	}

	private EffectiveValueEntry EvaluateExpression(EntryIndex entryIndex, DependencyProperty dp, Expression expr, PropertyMetadata metadata, EffectiveValueEntry oldEntry, EffectiveValueEntry newEntry)
	{
		object obj = expr.GetValue(this, dp);
		if (obj != DependencyProperty.UnsetValue && obj != Expression.NoValue)
		{
			if (!(obj is DeferredReference) && !dp.IsValidValue(obj))
			{
				throw new InvalidOperationException(SR.Format(SR.InvalidPropertyValue, obj, dp.Name));
			}
		}
		else
		{
			if (obj == Expression.NoValue)
			{
				newEntry.SetExpressionValue(Expression.NoValue, expr);
				if (!dp.ReadOnly)
				{
					EvaluateBaseValueCore(dp, metadata, ref newEntry);
					obj = newEntry.GetFlattenedEntry(RequestFlags.FullyResolved).Value;
				}
				else
				{
					obj = DependencyProperty.UnsetValue;
				}
			}
			if (obj == DependencyProperty.UnsetValue)
			{
				obj = metadata.GetDefaultValue(this, dp);
			}
		}
		newEntry.SetExpressionValue(obj, expr);
		return newEntry;
	}

	private EffectiveValueEntry EvaluateEffectiveValue(EntryIndex entryIndex, DependencyProperty dp, PropertyMetadata metadata, EffectiveValueEntry oldEntry, EffectiveValueEntry newEntry, OperationType operationType)
	{
		object obj = DependencyProperty.UnsetValue;
		bool flag = newEntry.BaseValueSourceInternal == BaseValueSourceInternal.Local;
		bool num = flag && newEntry.Value == DependencyProperty.UnsetValue;
		bool flag2 = false;
		bool flag3;
		if (newEntry.BaseValueSourceInternal == BaseValueSourceInternal.Unknown && newEntry.IsCoercedWithCurrentValue)
		{
			flag3 = true;
			newEntry.IsCoercedWithCurrentValue = false;
		}
		else
		{
			flag3 = false;
		}
		if (num)
		{
			newEntry.BaseValueSourceInternal = BaseValueSourceInternal.Unknown;
		}
		else
		{
			obj = (flag ? newEntry.LocalValue : oldEntry.LocalValue);
			if (obj == ExpressionInAlternativeStore)
			{
				obj = DependencyProperty.UnsetValue;
			}
			else
			{
				flag2 = (flag ? newEntry.IsExpression : oldEntry.IsExpression);
			}
		}
		if (obj != DependencyProperty.UnsetValue)
		{
			newEntry = new EffectiveValueEntry(dp, BaseValueSourceInternal.Local);
			newEntry.Value = obj;
			if (flag2)
			{
				newEntry = EvaluateExpression(entryIndex, dp, (Expression)obj, metadata, oldEntry, newEntry);
				entryIndex = CheckEntryIndex(entryIndex, dp.GlobalIndex);
				obj = newEntry.ModifiedValue.ExpressionValue;
			}
		}
		if (!dp.ReadOnly)
		{
			EvaluateBaseValueCore(dp, metadata, ref newEntry);
			if (newEntry.BaseValueSourceInternal == BaseValueSourceInternal.Unknown)
			{
				newEntry = EffectiveValueEntry.CreateDefaultValueEntry(dp, metadata.GetDefaultValue(this, dp));
			}
			obj = newEntry.GetFlattenedEntry(RequestFlags.FullyResolved).Value;
			if (flag3 && oldEntry.IsCoercedWithCurrentValue && oldEntry.BaseValueSourceInternal == newEntry.BaseValueSourceInternal && Equals(dp, oldEntry.ModifiedValue.BaseValue, obj))
			{
				object coercedValue = oldEntry.ModifiedValue.CoercedValue;
				newEntry.SetCoercedValue(coercedValue, obj, skipBaseValueChecks: true, coerceWithCurrentValue: true);
				obj = coercedValue;
			}
			entryIndex = CheckEntryIndex(entryIndex, dp.GlobalIndex);
			if (oldEntry.IsAnimated)
			{
				newEntry.ResetCoercedValue();
				EvaluateAnimatedValueCore(dp, metadata, ref newEntry);
				obj = newEntry.GetFlattenedEntry(RequestFlags.FullyResolved).Value;
			}
		}
		if (obj == DependencyProperty.UnsetValue)
		{
			newEntry = EffectiveValueEntry.CreateDefaultValueEntry(dp, metadata.GetDefaultValue(this, dp));
		}
		return newEntry;
	}

	internal virtual void EvaluateBaseValueCore(DependencyProperty dp, PropertyMetadata metadata, ref EffectiveValueEntry newEntry)
	{
	}

	internal virtual void EvaluateAnimatedValueCore(DependencyProperty dp, PropertyMetadata metadata, ref EffectiveValueEntry newEntry)
	{
	}

	/// <summary>Invoked whenever the effective value of any dependency property on this <see cref="T:System.Windows.DependencyObject" /> has been updated. The specific dependency property that changed is reported in the event data. </summary>
	/// <param name="e">Event data that will contain the dependency property identifier of interest, the property metadata for the type, and old and new values.</param>
	protected virtual void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == null)
		{
			throw new ArgumentException(SR.Format(SR.ReferenceIsNull, "e.Property"), "e");
		}
		if (e.IsAValueChange || e.IsASubPropertyChange || e.OperationType == OperationType.ChangeMutableDefaultValue)
		{
			PropertyMetadata metadata = e.Metadata;
			if (metadata != null && metadata.PropertyChangedCallback != null)
			{
				metadata.PropertyChangedCallback(this, e);
			}
		}
	}

	/// <summary>Returns a value that indicates whether serialization processes should serialize the value for the provided dependency property.</summary>
	/// <returns>true if the dependency property that is supplied should be value-serialized; otherwise, false.</returns>
	/// <param name="dp">The identifier for the dependency property that should be serialized.</param>
	protected internal virtual bool ShouldSerializeProperty(DependencyProperty dp)
	{
		return ContainsValue(dp);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal BaseValueSourceInternal GetValueSource(DependencyProperty dp, PropertyMetadata metadata, out bool hasModifiers)
	{
		bool isExpression;
		bool isAnimated;
		bool isCoerced;
		bool isCurrent;
		return GetValueSource(dp, metadata, out hasModifiers, out isExpression, out isAnimated, out isCoerced, out isCurrent);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal BaseValueSourceInternal GetValueSource(DependencyProperty dp, PropertyMetadata metadata, out bool hasModifiers, out bool isExpression, out bool isAnimated, out bool isCoerced, out bool isCurrent)
	{
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		EntryIndex entryIndex = LookupEntry(dp.GlobalIndex);
		if (entryIndex.Found)
		{
			EffectiveValueEntry effectiveValueEntry = _effectiveValues[entryIndex.Index];
			hasModifiers = effectiveValueEntry.HasModifiers;
			isExpression = effectiveValueEntry.IsExpression;
			isAnimated = effectiveValueEntry.IsAnimated;
			isCoerced = effectiveValueEntry.IsCoerced;
			isCurrent = effectiveValueEntry.IsCoercedWithCurrentValue;
			return effectiveValueEntry.BaseValueSourceInternal;
		}
		isExpression = false;
		isAnimated = false;
		isCoerced = false;
		isCurrent = false;
		if (dp.ReadOnly)
		{
			if (metadata == null)
			{
				metadata = dp.GetMetadata(DependencyObjectType);
			}
			GetReadOnlyValueCallback getReadOnlyValueCallback = metadata.GetReadOnlyValueCallback;
			if (getReadOnlyValueCallback != null)
			{
				getReadOnlyValueCallback(this, out var source);
				hasModifiers = false;
				return source;
			}
		}
		if (dp.IsPotentiallyInherited)
		{
			if (metadata == null)
			{
				metadata = dp.GetMetadata(DependencyObjectType);
			}
			if (metadata.IsInherited)
			{
				DependencyObject inheritanceParent = InheritanceParent;
				if (inheritanceParent != null && inheritanceParent.LookupEntry(dp.GlobalIndex).Found)
				{
					hasModifiers = false;
					return BaseValueSourceInternal.Inherited;
				}
			}
		}
		hasModifiers = false;
		return BaseValueSourceInternal.Default;
	}

	/// <summary>Returns the local value of a dependency property, if it exists. </summary>
	/// <returns>Returns the local value, or returns the sentinel value <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> if no local value is set.</returns>
	/// <param name="dp">The <see cref="T:System.Windows.DependencyProperty" /> identifier of the property to retrieve the value for.</param>
	public object ReadLocalValue(DependencyProperty dp)
	{
		VerifyAccess();
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		EntryIndex entryIndex = LookupEntry(dp.GlobalIndex);
		return ReadLocalValueEntry(entryIndex, dp, allowDeferredReferences: false);
	}

	internal object ReadLocalValueEntry(EntryIndex entryIndex, DependencyProperty dp, bool allowDeferredReferences)
	{
		if (!entryIndex.Found)
		{
			return DependencyProperty.UnsetValue;
		}
		EffectiveValueEntry effectiveValueEntry = _effectiveValues[entryIndex.Index];
		object obj = (effectiveValueEntry.IsCoercedWithCurrentValue ? effectiveValueEntry.ModifiedValue.CoercedValue : effectiveValueEntry.LocalValue);
		if (!allowDeferredReferences && effectiveValueEntry.IsDeferredReference && obj is DeferredReference deferredReference)
		{
			obj = deferredReference.GetValue(effectiveValueEntry.BaseValueSourceInternal);
		}
		if (obj == ExpressionInAlternativeStore)
		{
			obj = DependencyProperty.UnsetValue;
		}
		return obj;
	}

	/// <summary>Creates a specialized enumerator for determining which dependency properties have locally set values on this <see cref="T:System.Windows.DependencyObject" />. </summary>
	/// <returns>A specialized local value enumerator.</returns>
	public LocalValueEnumerator GetLocalValueEnumerator()
	{
		VerifyAccess();
		uint effectiveValuesCount = EffectiveValuesCount;
		LocalValueEntry[] array = new LocalValueEntry[effectiveValuesCount];
		int count = 0;
		for (uint num = 0u; num < effectiveValuesCount; num++)
		{
			DependencyProperty dependencyProperty = DependencyProperty.RegisteredPropertyList.List[_effectiveValues[num].PropertyIndex];
			if (dependencyProperty != null)
			{
				object obj = ReadLocalValueEntry(new EntryIndex(num), dependencyProperty, allowDeferredReferences: false);
				if (obj != DependencyProperty.UnsetValue)
				{
					array[count++] = new LocalValueEntry(dependencyProperty, obj);
				}
			}
		}
		return new LocalValueEnumerator(array, count);
	}

	internal static void UpdateSourceDependentLists(DependencyObject d, DependencyProperty dp, DependencySource[] sources, Expression expr, bool add)
	{
		if (sources == null)
		{
			return;
		}
		if (expr.ForwardsInvalidations)
		{
			d = null;
			dp = null;
		}
		foreach (DependencySource dependencySource in sources)
		{
			if (dependencySource.DependencyObject.IsSealed)
			{
				continue;
			}
			object value = DependentListMapField.GetValue(dependencySource.DependencyObject);
			FrugalMap frugalMap = ((value == null) ? default(FrugalMap) : ((FrugalMap)value));
			object obj = frugalMap[dependencySource.DependencyProperty.GlobalIndex];
			if (add)
			{
				DependentList dependentList2 = (DependentList)((obj != DependencyProperty.UnsetValue) ? ((DependentList)obj) : (frugalMap[dependencySource.DependencyProperty.GlobalIndex] = new DependentList()));
				dependentList2.Add(d, dp, expr);
			}
			else if (obj != DependencyProperty.UnsetValue)
			{
				DependentList obj2 = (DependentList)obj;
				obj2.Remove(d, dp, expr);
				if (obj2.IsEmpty)
				{
					frugalMap[dependencySource.DependencyProperty.GlobalIndex] = DependencyProperty.UnsetValue;
				}
			}
			DependentListMapField.SetValue(dependencySource.DependencyObject, frugalMap);
		}
	}

	internal static void ValidateSources(DependencyObject d, DependencySource[] newSources, Expression expr)
	{
		if (newSources == null)
		{
			return;
		}
		Dispatcher dispatcher = d.Dispatcher;
		for (int i = 0; i < newSources.Length; i++)
		{
			Dispatcher dispatcher2 = newSources[i].DependencyObject.Dispatcher;
			if (dispatcher2 != dispatcher && (!expr.SupportsUnboundSources || dispatcher2 != null))
			{
				throw new ArgumentException(SR.SourcesMustBeInSameThread);
			}
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static void RegisterForAlternativeExpressionStorage(AlternativeExpressionStorageCallback getExpressionCore, out AlternativeExpressionStorageCallback getExpression)
	{
		_getExpressionCore = getExpressionCore;
		getExpression = GetExpression;
	}

	internal bool HasAnyExpression()
	{
		EffectiveValueEntry[] effectiveValues = EffectiveValues;
		uint effectiveValuesCount = EffectiveValuesCount;
		bool result = false;
		for (uint num = 0u; num < effectiveValuesCount; num++)
		{
			DependencyProperty dependencyProperty = DependencyProperty.RegisteredPropertyList.List[effectiveValues[num].PropertyIndex];
			if (dependencyProperty != null)
			{
				EntryIndex entryIndex = new EntryIndex(num);
				if (HasExpression(entryIndex, dependencyProperty))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal bool HasExpression(EntryIndex entryIndex, DependencyProperty dp)
	{
		if (!entryIndex.Found)
		{
			return false;
		}
		EffectiveValueEntry effectiveValueEntry = _effectiveValues[entryIndex.Index];
		object localValue = effectiveValueEntry.LocalValue;
		if (!effectiveValueEntry.HasExpressionMarker)
		{
			return localValue is Expression;
		}
		return true;
	}

	private static Expression GetExpression(DependencyObject d, DependencyProperty dp, PropertyMetadata metadata)
	{
		EntryIndex entryIndex = d.LookupEntry(dp.GlobalIndex);
		if (!entryIndex.Found)
		{
			return null;
		}
		EffectiveValueEntry effectiveValueEntry = d._effectiveValues[entryIndex.Index];
		if (effectiveValueEntry.HasExpressionMarker)
		{
			if (_getExpressionCore != null)
			{
				return _getExpressionCore(d, dp, metadata);
			}
			return null;
		}
		if (effectiveValueEntry.IsExpression)
		{
			return (Expression)effectiveValueEntry.LocalValue;
		}
		return null;
	}

	internal virtual void AddInheritanceContext(DependencyObject context, DependencyProperty property)
	{
	}

	internal virtual void RemoveInheritanceContext(DependencyObject context, DependencyProperty property)
	{
	}

	internal virtual bool ShouldProvideInheritanceContext(DependencyObject target, DependencyProperty property)
	{
		return true;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void OnInheritanceContextChanged(EventArgs args)
	{
		InheritanceContextChangedHandlersField.GetValue(this)?.Invoke(this, args);
		CanModifyEffectiveValues = false;
		try
		{
			uint effectiveValuesCount = EffectiveValuesCount;
			for (uint num = 0u; num < effectiveValuesCount; num++)
			{
				DependencyProperty dependencyProperty = DependencyProperty.RegisteredPropertyList.List[_effectiveValues[num].PropertyIndex];
				if (dependencyProperty != null)
				{
					object obj = ReadLocalValueEntry(new EntryIndex(num), dependencyProperty, allowDeferredReferences: true);
					if (obj != DependencyProperty.UnsetValue && obj is DependencyObject dependencyObject && dependencyObject.InheritanceContext == this)
					{
						dependencyObject.OnInheritanceContextChanged(args);
					}
				}
			}
		}
		finally
		{
			CanModifyEffectiveValues = true;
		}
		OnInheritanceContextChangedCore(args);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal virtual void OnInheritanceContextChangedCore(EventArgs args)
	{
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static bool IsTreeWalkOperation(OperationType operation)
	{
		if (operation != OperationType.AddChild && operation != OperationType.RemoveChild)
		{
			return operation == OperationType.Inherit;
		}
		return true;
	}

	[Conditional("DEBUG")]
	internal void Debug_AssertNoInheritanceContextListeners()
	{
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void BeginPropertyInitialization()
	{
		IsInPropertyInitialization = true;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void EndPropertyInitialization()
	{
		IsInPropertyInitialization = false;
		if (_effectiveValues == null)
		{
			return;
		}
		uint effectiveValuesCount = EffectiveValuesCount;
		if (effectiveValuesCount != 0)
		{
			uint num = effectiveValuesCount;
			if ((double)((float)num / (float)_effectiveValues.Length) < 0.8)
			{
				EffectiveValueEntry[] array = new EffectiveValueEntry[num];
				Array.Copy(_effectiveValues, 0L, array, 0L, effectiveValuesCount);
				_effectiveValues = array;
			}
		}
	}

	private void SetInheritanceParent(DependencyObject newParent)
	{
		if (_contextStorage != null)
		{
			_contextStorage = newParent;
		}
		else if (newParent != null)
		{
			if (IsSelfInheritanceParent)
			{
				MergeInheritableProperties(newParent);
			}
			else
			{
				_contextStorage = newParent;
			}
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void SetIsSelfInheritanceParent()
	{
		DependencyObject inheritanceParent = InheritanceParent;
		if (inheritanceParent != null)
		{
			MergeInheritableProperties(inheritanceParent);
			SetInheritanceParent(null);
		}
		_packedData |= 1048576u;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void SynchronizeInheritanceParent(DependencyObject parent)
	{
		if (IsSelfInheritanceParent)
		{
			return;
		}
		if (parent != null)
		{
			if (!parent.IsSelfInheritanceParent)
			{
				SetInheritanceParent(parent.InheritanceParent);
			}
			else
			{
				SetInheritanceParent(parent);
			}
		}
		else
		{
			SetInheritanceParent(null);
		}
	}

	private void MergeInheritableProperties(DependencyObject inheritanceParent)
	{
		EffectiveValueEntry[] effectiveValues = inheritanceParent.EffectiveValues;
		uint effectiveValuesCount = inheritanceParent.EffectiveValuesCount;
		for (uint num = 0u; num < effectiveValuesCount; num++)
		{
			EffectiveValueEntry effectiveValueEntry = effectiveValues[num];
			DependencyProperty dependencyProperty = DependencyProperty.RegisteredPropertyList.List[effectiveValueEntry.PropertyIndex];
			if (dependencyProperty == null)
			{
				continue;
			}
			PropertyMetadata metadata = dependencyProperty.GetMetadata(DependencyObjectType);
			if (metadata.IsInherited)
			{
				object value = inheritanceParent.GetValueEntry(new EntryIndex(num), dependencyProperty, metadata, (RequestFlags)12).Value;
				if (value != DependencyProperty.UnsetValue)
				{
					EntryIndex entryIndex = LookupEntry(dependencyProperty.GlobalIndex);
					SetEffectiveValue(entryIndex, dependencyProperty, dependencyProperty.GlobalIndex, metadata, value, BaseValueSourceInternal.Inherited);
				}
			}
		}
	}

	private EntryIndex CheckEntryIndex(EntryIndex entryIndex, int targetIndex)
	{
		if (EffectiveValuesCount != 0 && _effectiveValues.Length > entryIndex.Index)
		{
			EffectiveValueEntry effectiveValueEntry = _effectiveValues[entryIndex.Index];
			if (effectiveValueEntry.PropertyIndex == targetIndex)
			{
				return new EntryIndex(entryIndex.Index);
			}
		}
		return LookupEntry(targetIndex);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal EntryIndex LookupEntry(int targetIndex)
	{
		uint num = 0u;
		uint num2 = EffectiveValuesCount;
		if (num2 == 0)
		{
			return new EntryIndex(0u, found: false);
		}
		while (num2 - num > 3)
		{
			uint num3 = (num2 + num) / 2;
			int propertyIndex = _effectiveValues[num3].PropertyIndex;
			if (targetIndex == propertyIndex)
			{
				return new EntryIndex(num3);
			}
			if (targetIndex <= propertyIndex)
			{
				num2 = num3;
			}
			else
			{
				num = num3 + 1;
			}
		}
		do
		{
			int propertyIndex = _effectiveValues[num].PropertyIndex;
			if (propertyIndex == targetIndex)
			{
				return new EntryIndex(num);
			}
			if (propertyIndex > targetIndex)
			{
				break;
			}
			num++;
		}
		while (num < num2);
		return new EntryIndex(num, found: false);
	}

	private void InsertEntry(EffectiveValueEntry entry, uint entryIndex)
	{
		if (!CanModifyEffectiveValues)
		{
			throw new InvalidOperationException(SR.LocalValueEnumerationInvalidated);
		}
		uint effectiveValuesCount = EffectiveValuesCount;
		if (effectiveValuesCount != 0)
		{
			if (_effectiveValues.Length == effectiveValuesCount)
			{
				int num = (int)((double)effectiveValuesCount * (IsInPropertyInitialization ? 2.0 : 1.2));
				if (num == effectiveValuesCount)
				{
					num++;
				}
				EffectiveValueEntry[] array = new EffectiveValueEntry[num];
				Array.Copy(_effectiveValues, 0L, array, 0L, entryIndex);
				array[entryIndex] = entry;
				Array.Copy(_effectiveValues, entryIndex, array, entryIndex + 1, effectiveValuesCount - entryIndex);
				_effectiveValues = array;
			}
			else
			{
				Array.Copy(_effectiveValues, entryIndex, _effectiveValues, entryIndex + 1, effectiveValuesCount - entryIndex);
				_effectiveValues[entryIndex] = entry;
			}
		}
		else
		{
			if (_effectiveValues == null)
			{
				_effectiveValues = new EffectiveValueEntry[EffectiveValuesInitialSize];
			}
			_effectiveValues[0] = entry;
		}
		EffectiveValuesCount = effectiveValuesCount + 1;
	}

	private void RemoveEntry(uint entryIndex, DependencyProperty dp)
	{
		if (!CanModifyEffectiveValues)
		{
			throw new InvalidOperationException(SR.LocalValueEnumerationInvalidated);
		}
		uint effectiveValuesCount = EffectiveValuesCount;
		Array.Copy(_effectiveValues, entryIndex + 1, _effectiveValues, entryIndex, effectiveValuesCount - entryIndex - 1);
		effectiveValuesCount = (EffectiveValuesCount = effectiveValuesCount - 1);
		_effectiveValues[effectiveValuesCount].Clear();
	}

	internal void SetEffectiveValue(EntryIndex entryIndex, DependencyProperty dp, PropertyMetadata metadata, EffectiveValueEntry newEntry, EffectiveValueEntry oldEntry)
	{
		if (metadata != null && metadata.IsInherited && (newEntry.BaseValueSourceInternal != BaseValueSourceInternal.Inherited || newEntry.IsCoerced || newEntry.IsAnimated) && !IsSelfInheritanceParent)
		{
			SetIsSelfInheritanceParent();
			entryIndex = CheckEntryIndex(entryIndex, dp.GlobalIndex);
		}
		bool flag = false;
		if (oldEntry.HasExpressionMarker && !newEntry.HasExpressionMarker)
		{
			BaseValueSourceInternal baseValueSourceInternal = newEntry.BaseValueSourceInternal;
			flag = baseValueSourceInternal == BaseValueSourceInternal.ThemeStyle || baseValueSourceInternal == BaseValueSourceInternal.ThemeStyleTrigger || baseValueSourceInternal == BaseValueSourceInternal.Style || baseValueSourceInternal == BaseValueSourceInternal.TemplateTrigger || baseValueSourceInternal == BaseValueSourceInternal.StyleTrigger || baseValueSourceInternal == BaseValueSourceInternal.ParentTemplate || baseValueSourceInternal == BaseValueSourceInternal.ParentTemplateTrigger;
		}
		if (flag)
		{
			newEntry.RestoreExpressionMarker();
		}
		else if (oldEntry.IsExpression && oldEntry.ModifiedValue.ExpressionValue == Expression.NoValue)
		{
			newEntry.SetExpressionValue(newEntry.Value, oldEntry.ModifiedValue.BaseValue);
		}
		if (entryIndex.Found)
		{
			_effectiveValues[entryIndex.Index] = newEntry;
			return;
		}
		InsertEntry(newEntry, entryIndex.Index);
		if (metadata != null && metadata.IsInherited)
		{
			InheritableEffectiveValuesCount++;
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal void SetEffectiveValue(EntryIndex entryIndex, DependencyProperty dp, int targetIndex, PropertyMetadata metadata, object value, BaseValueSourceInternal valueSource)
	{
		if (metadata != null && metadata.IsInherited && valueSource != BaseValueSourceInternal.Inherited && !IsSelfInheritanceParent)
		{
			SetIsSelfInheritanceParent();
			entryIndex = CheckEntryIndex(entryIndex, dp.GlobalIndex);
		}
		EffectiveValueEntry effectiveValueEntry;
		if (entryIndex.Found)
		{
			effectiveValueEntry = _effectiveValues[entryIndex.Index];
		}
		else
		{
			effectiveValueEntry = default(EffectiveValueEntry);
			effectiveValueEntry.PropertyIndex = targetIndex;
			InsertEntry(effectiveValueEntry, entryIndex.Index);
			if (metadata != null && metadata.IsInherited)
			{
				InheritableEffectiveValuesCount++;
			}
		}
		bool flag = value == ExpressionInAlternativeStore;
		if (!flag && effectiveValueEntry.HasExpressionMarker && (valueSource == BaseValueSourceInternal.ThemeStyle || valueSource == BaseValueSourceInternal.ThemeStyleTrigger || valueSource == BaseValueSourceInternal.Style || valueSource == BaseValueSourceInternal.TemplateTrigger || valueSource == BaseValueSourceInternal.StyleTrigger || valueSource == BaseValueSourceInternal.ParentTemplate || valueSource == BaseValueSourceInternal.ParentTemplateTrigger))
		{
			effectiveValueEntry.BaseValueSourceInternal = valueSource;
			effectiveValueEntry.SetExpressionValue(value, ExpressionInAlternativeStore);
			effectiveValueEntry.ResetAnimatedValue();
			effectiveValueEntry.ResetCoercedValue();
		}
		else if (effectiveValueEntry.IsExpression && effectiveValueEntry.ModifiedValue.ExpressionValue == Expression.NoValue)
		{
			effectiveValueEntry.SetExpressionValue(value, effectiveValueEntry.ModifiedValue.BaseValue);
		}
		else
		{
			effectiveValueEntry.BaseValueSourceInternal = valueSource;
			effectiveValueEntry.ResetValue(value, flag);
		}
		_effectiveValues[entryIndex.Index] = effectiveValueEntry;
	}

	internal void UnsetEffectiveValue(EntryIndex entryIndex, DependencyProperty dp, PropertyMetadata metadata)
	{
		if (entryIndex.Found)
		{
			RemoveEntry(entryIndex.Index, dp);
			if (metadata != null && metadata.IsInherited)
			{
				InheritableEffectiveValuesCount--;
			}
		}
	}

	private void SetExpressionValue(EntryIndex entryIndex, object value, object baseValue)
	{
		EffectiveValueEntry effectiveValueEntry = _effectiveValues[entryIndex.Index];
		effectiveValueEntry.SetExpressionValue(value, baseValue);
		effectiveValueEntry.ResetAnimatedValue();
		effectiveValueEntry.ResetCoercedValue();
		_effectiveValues[entryIndex.Index] = effectiveValueEntry;
	}

	private bool Equals(DependencyProperty dp, object value1, object value2)
	{
		if (dp.IsValueType || dp.IsStringType)
		{
			return object.Equals(value1, value2);
		}
		return value1 == value2;
	}
}
