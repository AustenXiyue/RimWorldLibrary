using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Markup;
using MS.Internal.Controls;
using MS.Internal.Data;

namespace System.Windows.Data;

/// <summary>Describes a collection of <see cref="T:System.Windows.Data.Binding" /> objects attached to a single binding target property.</summary>
[ContentProperty("Bindings")]
public class MultiBinding : BindingBase, IAddChild
{
	private BindingCollection _bindingCollection;

	/// <summary>Gets the collection of <see cref="T:System.Windows.Data.Binding" /> objects within this <see cref="T:System.Windows.Data.MultiBinding" /> instance.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Data.Binding" /> objects. <see cref="T:System.Windows.Data.MultiBinding" /> currently supports only objects of type <see cref="T:System.Windows.Data.Binding" /> and not <see cref="T:System.Windows.Data.MultiBinding" /> or <see cref="T:System.Windows.Data.PriorityBinding" />. Adding a <see cref="T:System.Windows.Data.Binding" /> child to a <see cref="T:System.Windows.Data.MultiBinding" /> object implicitly adds the child to the <see cref="T:System.Windows.Data.BindingBase" /> collection for the <see cref="T:System.Windows.Data.MultiBinding" /> object.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public Collection<BindingBase> Bindings => _bindingCollection;

	/// <summary>Gets or sets a value that indicates the direction of the data flow of this binding.</summary>
	/// <returns>One of the <see cref="T:System.Windows.Data.BindingMode" /> values. The default value is <see cref="F:System.Windows.Data.BindingMode.Default" />, which returns the default binding mode value of the target dependency property. However, the default value varies for each dependency property. In general, user-editable control properties, such as <see cref="P:System.Windows.Controls.TextBox.Text" />, default to two-way bindings, whereas most other properties default to one-way bindings.A programmatic way to determine whether a dependency property binds one-way or two-way by default is to get the property metadata of the property using <see cref="M:System.Windows.DependencyProperty.GetMetadata(System.Type)" /> and then check the Boolean value of the <see cref="P:System.Windows.FrameworkPropertyMetadata.BindsTwoWayByDefault" /> property.</returns>
	[DefaultValue(BindingMode.Default)]
	public BindingMode Mode
	{
		get
		{
			return GetFlagsWithinMask(BindingFlags.PropagationMask) switch
			{
				BindingFlags.OneWay => BindingMode.OneWay, 
				BindingFlags.TwoWay => BindingMode.TwoWay, 
				BindingFlags.OneWayToSource => BindingMode.OneWayToSource, 
				BindingFlags.OneTime => BindingMode.OneTime, 
				BindingFlags.PropDefault => BindingMode.Default, 
				_ => BindingMode.TwoWay, 
			};
		}
		set
		{
			CheckSealed();
			ChangeFlagsWithinMask(BindingFlags.PropagationMask, BindingBase.FlagsFrom(value));
		}
	}

	/// <summary>Gets or sets a value that determines the timing of binding source updates.</summary>
	/// <returns>One of the <see cref="T:System.Windows.Data.UpdateSourceTrigger" /> values. The default value is <see cref="F:System.Windows.Data.UpdateSourceTrigger.Default" />, which returns the default <see cref="T:System.Windows.Data.UpdateSourceTrigger" /> value of the target dependency property. However, the default value for most dependency properties is <see cref="F:System.Windows.Data.UpdateSourceTrigger.PropertyChanged" />, while the <see cref="P:System.Windows.Controls.TextBox.Text" /> property has a default value of <see cref="F:System.Windows.Data.UpdateSourceTrigger.LostFocus" />.A programmatic way to determine the default <see cref="P:System.Windows.Data.Binding.UpdateSourceTrigger" /> value of a dependency property is to get the property metadata of the property using <see cref="M:System.Windows.DependencyProperty.GetMetadata(System.Type)" /> and then check the value of the <see cref="P:System.Windows.FrameworkPropertyMetadata.DefaultUpdateSourceTrigger" /> property.</returns>
	[DefaultValue(UpdateSourceTrigger.PropertyChanged)]
	public UpdateSourceTrigger UpdateSourceTrigger
	{
		get
		{
			return GetFlagsWithinMask(BindingFlags.UpdateDefault) switch
			{
				BindingFlags.OneTime => UpdateSourceTrigger.PropertyChanged, 
				BindingFlags.UpdateOnLostFocus => UpdateSourceTrigger.LostFocus, 
				BindingFlags.UpdateExplicitly => UpdateSourceTrigger.Explicit, 
				BindingFlags.UpdateDefault => UpdateSourceTrigger.Default, 
				_ => UpdateSourceTrigger.Default, 
			};
		}
		set
		{
			CheckSealed();
			ChangeFlagsWithinMask(BindingFlags.UpdateDefault, BindingBase.FlagsFrom(value));
		}
	}

	/// <summary>Gets or sets a value that indicates whether to raise the <see cref="E:System.Windows.FrameworkElement.SourceUpdated" /> event when a value is transferred from the binding target to the binding source.</summary>
	/// <returns>true if the <see cref="E:System.Windows.FrameworkElement.SourceUpdated" /> event will be raised when the binding source value is updated; otherwise, false. The default value is false.</returns>
	[DefaultValue(false)]
	public bool NotifyOnSourceUpdated
	{
		get
		{
			return TestFlag(BindingFlags.NotifyOnSourceUpdated);
		}
		set
		{
			if (TestFlag(BindingFlags.NotifyOnSourceUpdated) != value)
			{
				CheckSealed();
				ChangeFlag(BindingFlags.NotifyOnSourceUpdated, value);
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether to raise the <see cref="E:System.Windows.FrameworkElement.TargetUpdated" /> event when a value is transferred from the binding source to the binding target.</summary>
	/// <returns>true if the <see cref="E:System.Windows.FrameworkElement.TargetUpdated" /> event will be raised when the binding target value is updated; otherwise, false. The default value is false.</returns>
	[DefaultValue(false)]
	public bool NotifyOnTargetUpdated
	{
		get
		{
			return TestFlag(BindingFlags.NotifyOnTargetUpdated);
		}
		set
		{
			if (TestFlag(BindingFlags.NotifyOnTargetUpdated) != value)
			{
				CheckSealed();
				ChangeFlag(BindingFlags.NotifyOnTargetUpdated, value);
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether to raise the <see cref="E:System.Windows.Controls.Validation.Error" /> attached event on the bound element.</summary>
	/// <returns>true if the <see cref="E:System.Windows.Controls.Validation.Error" /> attached event will be raised on the bound element when there is a validation error during source updates; otherwise, false. The default value is false.</returns>
	[DefaultValue(false)]
	public bool NotifyOnValidationError
	{
		get
		{
			return TestFlag(BindingFlags.NotifyOnValidationError);
		}
		set
		{
			if (TestFlag(BindingFlags.NotifyOnValidationError) != value)
			{
				CheckSealed();
				ChangeFlag(BindingFlags.NotifyOnValidationError, value);
			}
		}
	}

	/// <summary>Gets or sets the converter to use to convert the source values to or from the target value.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.Data.IMultiValueConverter" /> that indicates the converter to use. The default value is null.</returns>
	[DefaultValue(null)]
	public IMultiValueConverter Converter
	{
		get
		{
			return (IMultiValueConverter)GetValue(Feature.Converter, null);
		}
		set
		{
			CheckSealed();
			SetValue(Feature.Converter, value, null);
		}
	}

	/// <summary>Gets or sets an optional parameter to pass to a converter as additional information.</summary>
	/// <returns>A parameter to pass to a converter. The default value is null.</returns>
	[DefaultValue(null)]
	public object ConverterParameter
	{
		get
		{
			return GetValue(Feature.ConverterParameter, null);
		}
		set
		{
			CheckSealed();
			SetValue(Feature.ConverterParameter, value, null);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Globalization.CultureInfo" /> object that applies to any converter assigned to bindings wrapped by the <see cref="T:System.Windows.Data.MultiBinding" /> or on the <see cref="T:System.Windows.Data.MultiBinding" /> itself.</summary>
	/// <returns>A valid <see cref="T:System.Globalization.CultureInfo" />.</returns>
	[DefaultValue(null)]
	[TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
	public CultureInfo ConverterCulture
	{
		get
		{
			return (CultureInfo)GetValue(Feature.Culture, null);
		}
		set
		{
			CheckSealed();
			SetValue(Feature.Culture, value, null);
		}
	}

	/// <summary>Gets the collection of <see cref="T:System.Windows.Controls.ValidationRule" /> objects for this instance of <see cref="T:System.Windows.Data.MultiBinding" />.</summary>
	/// <returns>The collection of <see cref="T:System.Windows.Controls.ValidationRule" /> objects for this instance of <see cref="T:System.Windows.Data.MultiBinding" />.</returns>
	public Collection<ValidationRule> ValidationRules
	{
		get
		{
			if (!HasValue(Feature.ValidationRules))
			{
				SetValue(Feature.ValidationRules, new ValidationRuleCollection());
			}
			return (ValidationRuleCollection)GetValue(Feature.ValidationRules, null);
		}
	}

	/// <summary>Gets or sets a handler you can use to provide custom logic for handling exceptions that the binding engine encounters during the update of the binding source value. This is only applicable if you have associated the <see cref="T:System.Windows.Controls.ExceptionValidationRule" /> with your <see cref="T:System.Windows.Data.MultiBinding" /> object.</summary>
	/// <returns>A method that provides custom logic for handling exceptions that the binding engine encounters during the update of the binding source value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public UpdateSourceExceptionFilterCallback UpdateSourceExceptionFilter
	{
		get
		{
			return (UpdateSourceExceptionFilterCallback)GetValue(Feature.ExceptionFilterCallback, null);
		}
		set
		{
			SetValue(Feature.ExceptionFilterCallback, value, null);
		}
	}

	/// <summary>Gets or sets a value that indicates whether to include the <see cref="T:System.Windows.Controls.ExceptionValidationRule" />.</summary>
	/// <returns>true to include the <see cref="T:System.Windows.Controls.ExceptionValidationRule" />; otherwise, false.</returns>
	[DefaultValue(false)]
	public bool ValidatesOnExceptions
	{
		get
		{
			return TestFlag(BindingFlags.ValidatesOnExceptions);
		}
		set
		{
			if (TestFlag(BindingFlags.ValidatesOnExceptions) != value)
			{
				CheckSealed();
				ChangeFlag(BindingFlags.ValidatesOnExceptions, value);
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether to include the <see cref="T:System.Windows.Controls.DataErrorValidationRule" />.</summary>
	/// <returns>true to include the <see cref="T:System.Windows.Controls.DataErrorValidationRule" />; otherwise, false.</returns>
	[DefaultValue(false)]
	public bool ValidatesOnDataErrors
	{
		get
		{
			return TestFlag(BindingFlags.ValidatesOnDataErrors);
		}
		set
		{
			if (TestFlag(BindingFlags.ValidatesOnDataErrors) != value)
			{
				CheckSealed();
				ChangeFlag(BindingFlags.ValidatesOnDataErrors, value);
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether to include the <see cref="T:System.Windows.Controls.NotifyDataErrorValidationRule" />.</summary>
	/// <returns>true to include the <see cref="T:System.Windows.Controls.NotifyDataErrorValidationRule" />; otherwise, false. The default is true.</returns>
	[DefaultValue(true)]
	public bool ValidatesOnNotifyDataErrors
	{
		get
		{
			return TestFlag(BindingFlags.ValidatesOnNotifyDataErrors);
		}
		set
		{
			if (TestFlag(BindingFlags.ValidatesOnNotifyDataErrors) != value)
			{
				CheckSealed();
				ChangeFlag(BindingFlags.ValidatesOnNotifyDataErrors, value);
			}
		}
	}

	internal override Collection<ValidationRule> ValidationRulesInternal => (ValidationRuleCollection)GetValue(Feature.ValidationRules, null);

	internal override CultureInfo ConverterCultureInternal => ConverterCulture;

	internal override bool ValidatesOnNotifyDataErrorsInternal => ValidatesOnNotifyDataErrors;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.MultiBinding" /> class.</summary>
	public MultiBinding()
	{
		_bindingCollection = new BindingCollection(this, OnBindingCollectionChanged);
	}

	/// <summary>Adds a child object.</summary>
	/// <param name="value">The child object to add. </param>
	void IAddChild.AddChild(object value)
	{
		if (value is BindingBase item)
		{
			Bindings.Add(item);
			return;
		}
		throw new ArgumentException(SR.Format(SR.ChildHasWrongType, GetType().Name, "BindingBase", value.GetType().FullName), "value");
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Data.MultiBinding.Bindings" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeBindings()
	{
		if (Bindings != null)
		{
			return Bindings.Count > 0;
		}
		return false;
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Data.MultiBinding.ValidationRules" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeValidationRules()
	{
		if (HasValue(Feature.ValidationRules))
		{
			return ValidationRules.Count > 0;
		}
		return false;
	}

	internal override BindingExpressionBase CreateBindingExpressionOverride(DependencyObject target, DependencyProperty dp, BindingExpressionBase owner)
	{
		if (Converter == null && string.IsNullOrEmpty(base.StringFormat))
		{
			throw new InvalidOperationException(SR.MultiBindingHasNoConverter);
		}
		for (int i = 0; i < Bindings.Count; i++)
		{
			CheckTrigger(Bindings[i]);
		}
		return MultiBindingExpression.CreateBindingExpression(target, dp, this, owner);
	}

	internal override ValidationRule LookupValidationRule(Type type)
	{
		return BindingBase.LookupValidationRule(type, ValidationRulesInternal);
	}

	internal object DoFilterException(object bindExpr, Exception exception)
	{
		UpdateSourceExceptionFilterCallback updateSourceExceptionFilterCallback = (UpdateSourceExceptionFilterCallback)GetValue(Feature.ExceptionFilterCallback, null);
		if (updateSourceExceptionFilterCallback != null)
		{
			return updateSourceExceptionFilterCallback(bindExpr, exception);
		}
		return exception;
	}

	internal static void CheckTrigger(BindingBase bb)
	{
		if (bb is Binding { UpdateSourceTrigger: not UpdateSourceTrigger.PropertyChanged, UpdateSourceTrigger: not UpdateSourceTrigger.Default })
		{
			throw new InvalidOperationException(SR.NoUpdateSourceTriggerForInnerBindingOfMultiBinding);
		}
	}

	internal override BindingBase CreateClone()
	{
		return new MultiBinding();
	}

	internal override void InitializeClone(BindingBase baseClone, BindingMode mode)
	{
		MultiBinding multiBinding = (MultiBinding)baseClone;
		CopyValue(Feature.Converter, multiBinding);
		CopyValue(Feature.ConverterParameter, multiBinding);
		CopyValue(Feature.Culture, multiBinding);
		CopyValue(Feature.ValidationRules, multiBinding);
		CopyValue(Feature.ExceptionFilterCallback, multiBinding);
		for (int i = 0; i < _bindingCollection.Count; i++)
		{
			multiBinding._bindingCollection.Add(_bindingCollection[i].Clone(mode));
		}
		base.InitializeClone(baseClone, mode);
	}

	private void OnBindingCollectionChanged()
	{
		CheckSealed();
	}
}
