using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Markup;
using MS.Internal;

namespace System.Windows.Data;

/// <summary>Defines the common characteristics of the <see cref="T:System.Windows.Data.Binding" />, <see cref="T:System.Windows.Data.PriorityBinding" />, and <see cref="T:System.Windows.Data.MultiBinding" /> classes. </summary>
[MarkupExtensionReturnType(typeof(object))]
[Localizability(LocalizationCategory.None, Modifiability = Modifiability.Unmodifiable, Readability = Readability.Unreadable)]
public abstract class BindingBase : MarkupExtension
{
	[Flags]
	internal enum BindingFlags : uint
	{
		OneWay = 1u,
		TwoWay = 3u,
		OneWayToSource = 2u,
		OneTime = 0u,
		PropDefault = 4u,
		NotifyOnTargetUpdated = 8u,
		NotifyOnSourceUpdated = 0x800000u,
		NotifyOnValidationError = 0x200000u,
		UpdateDefault = 0xC00u,
		UpdateOnPropertyChanged = 0u,
		UpdateOnLostFocus = 0x400u,
		UpdateExplicitly = 0x800u,
		PathGeneratedInternally = 0x2000u,
		ValidatesOnExceptions = 0x1000000u,
		ValidatesOnDataErrors = 0x2000000u,
		ValidatesOnNotifyDataErrors = 0x20000000u,
		PropagationMask = 7u,
		UpdateMask = 0xC00u,
		Default = 0x20000C04u,
		IllegalInput = 0x4000000u
	}

	internal enum Feature
	{
		FallbackValue,
		StringFormat,
		TargetNullValue,
		BindingGroupName,
		Delay,
		XPath,
		Culture,
		AsyncState,
		ObjectSource,
		RelativeSource,
		ElementSource,
		Converter,
		ConverterParameter,
		ValidationRules,
		ExceptionFilterCallback,
		AttachedPropertiesInPath,
		LastFeatureId
	}

	private BindingFlags _flags = BindingFlags.Default;

	private bool _isSealed;

	private UncommonValueTable _values;

	/// <summary>Gets or sets the value to use when the binding is unable to return a value.</summary>
	/// <returns>The default value is <see cref="F:System.Windows.DependencyProperty.UnsetValue" />.</returns>
	public object FallbackValue
	{
		get
		{
			return GetValue(Feature.FallbackValue, DependencyProperty.UnsetValue);
		}
		set
		{
			CheckSealed();
			SetValue(Feature.FallbackValue, value);
		}
	}

	/// <summary>Gets or sets a string that specifies how to format the binding if it displays the bound value as a string.</summary>
	/// <returns>A string that specifies how to format the binding if it displays the bound value as a string.</returns>
	[DefaultValue(null)]
	public string StringFormat
	{
		get
		{
			return (string)GetValue(Feature.StringFormat, null);
		}
		set
		{
			CheckSealed();
			SetValue(Feature.StringFormat, value, null);
		}
	}

	/// <summary>Gets or sets the value that is used in the target when the value of the source is null.</summary>
	/// <returns>The value that is used in the target when the value of the source is null.</returns>
	public object TargetNullValue
	{
		get
		{
			return GetValue(Feature.TargetNullValue, DependencyProperty.UnsetValue);
		}
		set
		{
			CheckSealed();
			SetValue(Feature.TargetNullValue, value);
		}
	}

	/// <summary>Gets or sets the name of the <see cref="T:System.Windows.Data.BindingGroup" /> to which this binding belongs.</summary>
	/// <returns>The name of the <see cref="T:System.Windows.Data.BindingGroup" /> to which this binding belongs.</returns>
	[DefaultValue("")]
	public string BindingGroupName
	{
		get
		{
			return (string)GetValue(Feature.BindingGroupName, string.Empty);
		}
		set
		{
			CheckSealed();
			SetValue(Feature.BindingGroupName, value, string.Empty);
		}
	}

	/// <summary>Gets or sets the amount of time, in milliseconds, to wait before updating the binding source after the value on the target changes.</summary>
	/// <returns>The amount of time, in milliseconds, to wait before updating the binding source.</returns>
	[DefaultValue(0)]
	public int Delay
	{
		get
		{
			return (int)GetValue(Feature.Delay, 0);
		}
		set
		{
			CheckSealed();
			SetValue(Feature.Delay, value, 0);
		}
	}

	internal BindingFlags Flags => _flags;

	internal virtual CultureInfo ConverterCultureInternal => null;

	internal virtual Collection<ValidationRule> ValidationRulesInternal => null;

	internal virtual bool ValidatesOnNotifyDataErrorsInternal => false;

	static BindingBase()
	{
	}

	internal BindingBase()
	{
	}

	/// <summary>Returns a value that indicates whether serialization processes should serialize the effective value of the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> property on instances of this class.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> property value should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeFallbackValue()
	{
		return HasValue(Feature.FallbackValue);
	}

	/// <summary>Returns a value that indicates whether the <see cref="P:System.Windows.Data.BindingBase.TargetNullValue" /> property should be serialized.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Data.BindingBase.TargetNullValue" /> property should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeTargetNullValue()
	{
		return HasValue(Feature.TargetNullValue);
	}

	/// <summary>Returns an object that should be set on the property where this binding and extension are applied.</summary>
	/// <returns>The value to set on the binding target property.</returns>
	/// <param name="serviceProvider">The object that can provide services for the markup extension. May be null; see the Remarks section for more information.</param>
	public sealed override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (serviceProvider == null)
		{
			return this;
		}
		Helper.CheckCanReceiveMarkupExtension(this, serviceProvider, out var targetDependencyObject, out var targetDependencyProperty);
		if (targetDependencyObject == null || targetDependencyProperty == null)
		{
			return this;
		}
		return CreateBindingExpression(targetDependencyObject, targetDependencyProperty);
	}

	internal abstract BindingExpressionBase CreateBindingExpressionOverride(DependencyObject targetObject, DependencyProperty targetProperty, BindingExpressionBase owner);

	internal bool TestFlag(BindingFlags flag)
	{
		return (_flags & flag) != 0;
	}

	internal void SetFlag(BindingFlags flag)
	{
		_flags |= flag;
	}

	internal void ClearFlag(BindingFlags flag)
	{
		_flags &= ~flag;
	}

	internal void ChangeFlag(BindingFlags flag, bool value)
	{
		if (value)
		{
			_flags |= flag;
		}
		else
		{
			_flags &= ~flag;
		}
	}

	internal BindingFlags GetFlagsWithinMask(BindingFlags mask)
	{
		return _flags & mask;
	}

	internal void ChangeFlagsWithinMask(BindingFlags mask, BindingFlags flags)
	{
		_flags = (_flags & ~mask) | (flags & mask);
	}

	internal static BindingFlags FlagsFrom(BindingMode bindingMode)
	{
		return bindingMode switch
		{
			BindingMode.OneWay => BindingFlags.OneWay, 
			BindingMode.TwoWay => BindingFlags.TwoWay, 
			BindingMode.OneWayToSource => BindingFlags.OneWayToSource, 
			BindingMode.OneTime => BindingFlags.OneTime, 
			BindingMode.Default => BindingFlags.PropDefault, 
			_ => BindingFlags.IllegalInput, 
		};
	}

	internal static BindingFlags FlagsFrom(UpdateSourceTrigger updateSourceTrigger)
	{
		return updateSourceTrigger switch
		{
			UpdateSourceTrigger.Default => BindingFlags.UpdateDefault, 
			UpdateSourceTrigger.PropertyChanged => BindingFlags.OneTime, 
			UpdateSourceTrigger.LostFocus => BindingFlags.UpdateOnLostFocus, 
			UpdateSourceTrigger.Explicit => BindingFlags.UpdateExplicitly, 
			_ => BindingFlags.IllegalInput, 
		};
	}

	internal BindingExpressionBase CreateBindingExpression(DependencyObject targetObject, DependencyProperty targetProperty)
	{
		_isSealed = true;
		return CreateBindingExpressionOverride(targetObject, targetProperty, null);
	}

	internal BindingExpressionBase CreateBindingExpression(DependencyObject targetObject, DependencyProperty targetProperty, BindingExpressionBase owner)
	{
		_isSealed = true;
		return CreateBindingExpressionOverride(targetObject, targetProperty, owner);
	}

	internal void CheckSealed()
	{
		if (_isSealed)
		{
			throw new InvalidOperationException(SR.ChangeSealedBinding);
		}
	}

	internal ValidationRule GetValidationRule(Type type)
	{
		if (TestFlag(BindingFlags.ValidatesOnExceptions) && type == typeof(ExceptionValidationRule))
		{
			return ExceptionValidationRule.Instance;
		}
		if (TestFlag(BindingFlags.ValidatesOnDataErrors) && type == typeof(DataErrorValidationRule))
		{
			return DataErrorValidationRule.Instance;
		}
		if (TestFlag(BindingFlags.ValidatesOnNotifyDataErrors) && type == typeof(NotifyDataErrorValidationRule))
		{
			return NotifyDataErrorValidationRule.Instance;
		}
		return LookupValidationRule(type);
	}

	internal virtual ValidationRule LookupValidationRule(Type type)
	{
		return null;
	}

	internal static ValidationRule LookupValidationRule(Type type, Collection<ValidationRule> collection)
	{
		if (collection == null)
		{
			return null;
		}
		for (int i = 0; i < collection.Count; i++)
		{
			if (type.IsInstanceOfType(collection[i]))
			{
				return collection[i];
			}
		}
		return null;
	}

	internal BindingBase Clone(BindingMode mode)
	{
		BindingBase bindingBase = CreateClone();
		InitializeClone(bindingBase, mode);
		return bindingBase;
	}

	internal virtual void InitializeClone(BindingBase clone, BindingMode mode)
	{
		clone._flags = _flags;
		CopyValue(Feature.FallbackValue, clone);
		clone._isSealed = _isSealed;
		CopyValue(Feature.StringFormat, clone);
		CopyValue(Feature.TargetNullValue, clone);
		CopyValue(Feature.BindingGroupName, clone);
		clone.ChangeFlagsWithinMask(BindingFlags.PropagationMask, FlagsFrom(mode));
	}

	internal abstract BindingBase CreateClone();

	internal bool HasValue(Feature id)
	{
		return _values.HasValue((int)id);
	}

	internal object GetValue(Feature id, object defaultValue)
	{
		return _values.GetValue((int)id, defaultValue);
	}

	internal void SetValue(Feature id, object value)
	{
		_values.SetValue((int)id, value);
	}

	internal void SetValue(Feature id, object value, object defaultValue)
	{
		if (object.Equals(value, defaultValue))
		{
			_values.ClearValue((int)id);
		}
		else
		{
			_values.SetValue((int)id, value);
		}
	}

	internal void ClearValue(Feature id)
	{
		_values.ClearValue((int)id);
	}

	internal void CopyValue(Feature id, BindingBase clone)
	{
		if (HasValue(id))
		{
			clone.SetValue(id, GetValue(id, null));
		}
	}
}
