using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;
using MS.Internal;
using MS.Internal.Data;

namespace System.Windows.Data;

/// <summary>Contains instance information about a single instance of a <see cref="T:System.Windows.Data.MultiBinding" />.</summary>
public sealed class MultiBindingExpression : BindingExpressionBase, IDataBindEngineClient
{
	private Collection<BindingExpressionBase> _list = new Collection<BindingExpressionBase>();

	private IMultiValueConverter _converter;

	private object[] _tempValues;

	private Type[] _tempTypes;

	DependencyObject IDataBindEngineClient.TargetElement
	{
		get
		{
			if (base.UsingMentor)
			{
				return Helper.FindMentor(base.TargetElement);
			}
			return base.TargetElement;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Data.MultiBinding" /> object from which this <see cref="T:System.Windows.Data.MultiBindingExpression" /> is created.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.MultiBinding" /> object from which this <see cref="T:System.Windows.Data.MultiBindingExpression" /> is created.</returns>
	public MultiBinding ParentMultiBinding => (MultiBinding)base.ParentBindingBase;

	/// <summary>Gets the collection of <see cref="T:System.Windows.Data.BindingExpression" /> objects in this instance of <see cref="T:System.Windows.Data.MultiBindingExpression" />.</summary>
	/// <returns>A read-only collection of the <see cref="T:System.Windows.Data.BindingExpression" /> objects. Even though the return type is a collection of <see cref="T:System.Windows.Data.BindingExpressionBase" /> objects the returned collection would only contain <see cref="T:System.Windows.Data.BindingExpression" /> objects because the <see cref="T:System.Windows.Data.MultiBinding" /> class currently only supports <see cref="T:System.Windows.Data.Binding" /> objects.</returns>
	public ReadOnlyCollection<BindingExpressionBase> BindingExpressions => new ReadOnlyCollection<BindingExpressionBase>(MutableBindingExpressions);

	internal override bool IsParentBindingUpdateTriggerDefault => ParentMultiBinding.UpdateSourceTrigger == UpdateSourceTrigger.Default;

	/// <summary>Gets the <see cref="T:System.Windows.Controls.ValidationError" /> object that caused this instance of <see cref="T:System.Windows.Data.MultiBindingExpression" /> to be invalid.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.ValidationError" /> object that caused this instance of <see cref="T:System.Windows.Data.MultiBindingExpression" /> to be invalid.</returns>
	public override ValidationError ValidationError
	{
		get
		{
			ValidationError validationError = base.ValidationError;
			if (validationError == null)
			{
				for (int i = 0; i < MutableBindingExpressions.Count; i++)
				{
					validationError = MutableBindingExpressions[i].ValidationError;
					if (validationError != null)
					{
						break;
					}
				}
			}
			return validationError;
		}
	}

	/// <summary>Returns a value that indicates whether any of the inner <see cref="T:System.Windows.Data.Binding" /> objects or the <see cref="T:System.Windows.Data.MultiBinding" /> itself has a failing validation rule.</summary>
	/// <returns>true if at least one of the inner <see cref="T:System.Windows.Data.Binding" /> objects or the <see cref="T:System.Windows.Data.MultiBinding" /> itself has a failing validation rule; otherwise, false.</returns>
	public override bool HasError
	{
		get
		{
			bool hasError = base.HasError;
			if (!hasError)
			{
				for (int i = 0; i < MutableBindingExpressions.Count; i++)
				{
					if (MutableBindingExpressions[i].HasError)
					{
						return true;
					}
				}
			}
			return hasError;
		}
	}

	/// <summary>Gets a value that indicates whether the parent binding has a failed validation rule.</summary>
	/// <returns>true if the parent binding has a failed validation rule; otherwise, false.</returns>
	public override bool HasValidationError
	{
		get
		{
			bool hasValidationError = base.HasValidationError;
			if (!hasValidationError)
			{
				for (int i = 0; i < MutableBindingExpressions.Count; i++)
				{
					if (MutableBindingExpressions[i].HasValidationError)
					{
						return true;
					}
				}
			}
			return hasValidationError;
		}
	}

	private Collection<BindingExpressionBase> MutableBindingExpressions => _list;

	private IMultiValueConverter Converter
	{
		get
		{
			return _converter;
		}
		set
		{
			_converter = value;
		}
	}

	private MultiBindingExpression(MultiBinding binding, BindingExpressionBase owner)
		: base(binding, owner)
	{
		int count = binding.Bindings.Count;
		_tempValues = new object[count];
		_tempTypes = new Type[count];
	}

	void IDataBindEngineClient.TransferValue()
	{
		TransferValue();
	}

	void IDataBindEngineClient.UpdateValue()
	{
		UpdateValue();
	}

	bool IDataBindEngineClient.AttachToContext(bool lastChance)
	{
		AttachToContext(lastChance);
		return !base.TransferIsDeferred;
	}

	void IDataBindEngineClient.VerifySourceReference(bool lastChance)
	{
	}

	void IDataBindEngineClient.OnTargetUpdated()
	{
		OnTargetUpdated();
	}

	/// <summary>Sends the current binding target value to the binding source properties in <see cref="F:System.Windows.Data.BindingMode.TwoWay" /> or <see cref="F:System.Windows.Data.BindingMode.OneWayToSource" /> bindings.</summary>
	public override void UpdateSource()
	{
		if (MutableBindingExpressions.Count == 0)
		{
			throw new InvalidOperationException(SR.BindingExpressionIsDetached);
		}
		base.NeedsUpdate = true;
		Update();
	}

	/// <summary>Forces a data transfer from the binding source properties to the binding target property.</summary>
	public override void UpdateTarget()
	{
		if (MutableBindingExpressions.Count == 0)
		{
			throw new InvalidOperationException(SR.BindingExpressionIsDetached);
		}
		UpdateTarget(includeInnerBindings: true);
	}

	internal static MultiBindingExpression CreateBindingExpression(DependencyObject d, DependencyProperty dp, MultiBinding binding, BindingExpressionBase owner)
	{
		FrameworkPropertyMetadata frameworkPropertyMetadata = dp.GetMetadata(d.DependencyObjectType) as FrameworkPropertyMetadata;
		if ((frameworkPropertyMetadata != null && !frameworkPropertyMetadata.IsDataBindingAllowed) || dp.ReadOnly)
		{
			throw new ArgumentException(SR.Format(SR.PropertyNotBindable, dp.Name), "dp");
		}
		MultiBindingExpression multiBindingExpression = new MultiBindingExpression(binding, owner);
		multiBindingExpression.ResolvePropertyDefaultSettings(binding.Mode, binding.UpdateSourceTrigger, frameworkPropertyMetadata);
		return multiBindingExpression;
	}

	private void AttachToContext(bool lastChance)
	{
		DependencyObject targetElement = base.TargetElement;
		if (targetElement == null)
		{
			return;
		}
		bool flag = TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach);
		_converter = ParentMultiBinding.Converter;
		if (_converter == null && string.IsNullOrEmpty(base.EffectiveStringFormat) && TraceData.IsEnabled)
		{
			TraceData.TraceAndNotify(TraceEventType.Error, TraceData.MultiBindingHasNoConverter, this, new object[1] { ParentMultiBinding });
		}
		if (flag)
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.AttachToContext(TraceData.Identify(this), lastChance ? " (last chance)" : string.Empty), this);
		}
		base.TransferIsDeferred = true;
		bool flag2 = true;
		int count = MutableBindingExpressions.Count;
		for (int i = 0; i < count; i++)
		{
			if (MutableBindingExpressions[i].StatusInternal == BindingStatusInternal.Unattached)
			{
				flag2 = false;
			}
		}
		if (!flag2 && !lastChance)
		{
			if (flag)
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.ChildNotAttached(TraceData.Identify(this)), this);
			}
			return;
		}
		if (base.UsesLanguage)
		{
			WeakDependencySource[] commonSources = new WeakDependencySource[1]
			{
				new WeakDependencySource(base.TargetElement, FrameworkElement.LanguageProperty)
			};
			WeakDependencySource[] newSources = BindingExpressionBase.CombineSources(-1, MutableBindingExpressions, MutableBindingExpressions.Count, null, commonSources);
			ChangeSources(newSources);
		}
		bool flag3 = base.IsOneWayToSource;
		if (ShouldUpdateWithCurrentValue(targetElement, out var currentValue))
		{
			flag3 = true;
			ChangeValue(currentValue, notify: false);
			base.NeedsUpdate = true;
		}
		SetStatus(BindingStatusInternal.Active);
		if (!flag3)
		{
			UpdateTarget(includeInnerBindings: false);
		}
		else
		{
			UpdateValue();
		}
	}

	internal override bool AttachOverride(DependencyObject d, DependencyProperty dp)
	{
		if (!base.AttachOverride(d, dp))
		{
			return false;
		}
		DependencyObject targetElement = base.TargetElement;
		if (targetElement == null)
		{
			return false;
		}
		if (base.IsUpdateOnLostFocus)
		{
			LostFocusEventManager.AddHandler(targetElement, OnLostFocus);
		}
		base.TransferIsDeferred = true;
		int count = ParentMultiBinding.Bindings.Count;
		for (int i = 0; i < count; i++)
		{
			AttachBindingExpression(i, replaceExisting: false);
		}
		AttachToContext(lastChance: false);
		if (base.TransferIsDeferred)
		{
			base.Engine.AddTask(this, TaskOps.AttachToContext);
			if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach))
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.DeferAttachToContext(TraceData.Identify(this)), this);
			}
		}
		return true;
	}

	internal override void DetachOverride()
	{
		DependencyObject targetElement = base.TargetElement;
		if (targetElement != null && base.IsUpdateOnLostFocus)
		{
			LostFocusEventManager.RemoveHandler(targetElement, OnLostFocus);
		}
		for (int num = MutableBindingExpressions.Count - 1; num >= 0; num--)
		{
			BindingExpressionBase bindingExpressionBase = MutableBindingExpressions[num];
			if (bindingExpressionBase != null)
			{
				bindingExpressionBase.Detach();
				MutableBindingExpressions.RemoveAt(num);
			}
		}
		ChangeSources(null);
		base.DetachOverride();
	}

	internal override void InvalidateChild(BindingExpressionBase bindingExpression)
	{
		int num = MutableBindingExpressions.IndexOf(bindingExpression);
		if (0 <= num && base.IsDynamic)
		{
			base.NeedsDataTransfer = true;
			Transfer();
		}
	}

	internal override void ChangeSourcesForChild(BindingExpressionBase bindingExpression, WeakDependencySource[] newSources)
	{
		int num = MutableBindingExpressions.IndexOf(bindingExpression);
		if (num >= 0)
		{
			WeakDependencySource[] commonSources = null;
			if (base.UsesLanguage)
			{
				commonSources = new WeakDependencySource[1]
				{
					new WeakDependencySource(base.TargetElement, FrameworkElement.LanguageProperty)
				};
			}
			WeakDependencySource[] newSources2 = BindingExpressionBase.CombineSources(num, MutableBindingExpressions, MutableBindingExpressions.Count, newSources, commonSources);
			ChangeSources(newSources2);
		}
	}

	internal override void ReplaceChild(BindingExpressionBase bindingExpression)
	{
		int num = MutableBindingExpressions.IndexOf(bindingExpression);
		DependencyObject targetElement = base.TargetElement;
		if (num >= 0 && targetElement != null)
		{
			bindingExpression.Detach();
			AttachBindingExpression(num, replaceExisting: true);
		}
	}

	internal override void UpdateBindingGroup(BindingGroup bg)
	{
		int i = 0;
		for (int num = MutableBindingExpressions.Count - 1; i < num; i++)
		{
			MutableBindingExpressions[i].UpdateBindingGroup(bg);
		}
	}

	internal override object ConvertProposedValue(object value)
	{
		if (!ConvertProposedValueImpl(value, out var result))
		{
			result = DependencyProperty.UnsetValue;
			ValidationError validationError = new ValidationError(ConversionValidationRule.Instance, this, SR.Format(SR.Validation_ConversionFailed, value), null);
			UpdateValidationError(validationError);
		}
		return result;
	}

	private bool ConvertProposedValueImpl(object value, out object result)
	{
		DependencyObject targetElement = base.TargetElement;
		if (targetElement == null)
		{
			result = DependencyProperty.UnsetValue;
			return false;
		}
		result = GetValuesForChildBindings(value);
		if (base.IsDetached)
		{
			return false;
		}
		if (result == DependencyProperty.UnsetValue)
		{
			SetStatus(BindingStatusInternal.UpdateSourceError);
			return false;
		}
		object[] array = (object[])result;
		if (array == null)
		{
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.BadMultiConverterForUpdate(Converter.GetType().Name, AvTrace.ToStringHelper(value), AvTrace.TypeName(value)), this);
			}
			result = DependencyProperty.UnsetValue;
			return false;
		}
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer))
		{
			for (int i = 0; i < array.Length; i++)
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.UserConvertBackMulti(TraceData.Identify(this), i, TraceData.Identify(array[i])), this);
			}
		}
		int num = MutableBindingExpressions.Count;
		if (array.Length != num && TraceData.IsEnabled)
		{
			TraceData.TraceAndNotify(TraceEventType.Information, TraceData.MultiValueConverterMismatch, this, new object[4]
			{
				Converter.GetType().Name,
				num,
				array.Length,
				TraceData.DescribeTarget(targetElement, base.TargetProperty)
			});
		}
		if (array.Length < num)
		{
			num = array.Length;
		}
		bool result2 = true;
		for (int j = 0; j < num; j++)
		{
			value = array[j];
			if (value != Binding.DoNothing && value != DependencyProperty.UnsetValue)
			{
				BindingExpressionBase bindingExpressionBase = MutableBindingExpressions[j];
				bindingExpressionBase.SetValue(targetElement, base.TargetProperty, value);
				value = bindingExpressionBase.GetRawProposedValue();
				if (!bindingExpressionBase.Validate(value, ValidationStep.RawProposedValue))
				{
					value = DependencyProperty.UnsetValue;
				}
				value = bindingExpressionBase.ConvertProposedValue(value);
			}
			else if (value == DependencyProperty.UnsetValue && TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Information, TraceData.UnsetValueInMultiBindingExpressionUpdate(Converter.GetType().Name, AvTrace.ToStringHelper(value), j, _tempTypes[j]), this);
			}
			if (value == DependencyProperty.UnsetValue)
			{
				result2 = false;
			}
			array[j] = value;
		}
		Array.Clear(_tempTypes, 0, _tempTypes.Length);
		result = array;
		return result2;
	}

	private object GetValuesForChildBindings(object rawValue)
	{
		if (Converter == null)
		{
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.MultiValueConverterMissingForUpdate, this);
			}
			return DependencyProperty.UnsetValue;
		}
		CultureInfo culture = GetCulture();
		int count = MutableBindingExpressions.Count;
		for (int i = 0; i < count; i++)
		{
			if (MutableBindingExpressions[i] is BindingExpression { UseDefaultValueConverter: not false } bindingExpression)
			{
				_tempTypes[i] = bindingExpression.ConverterSourceType;
			}
			else
			{
				_tempTypes[i] = base.TargetProperty.PropertyType;
			}
		}
		return Converter.ConvertBack(rawValue, _tempTypes, ParentMultiBinding.ConverterParameter, culture);
	}

	internal override bool ObtainConvertedProposedValue(BindingGroup bindingGroup)
	{
		bool result = true;
		if (base.NeedsUpdate)
		{
			object obj = bindingGroup.GetValue(this);
			if (obj != DependencyProperty.UnsetValue)
			{
				obj = ConvertProposedValue(obj);
				if (obj == DependencyProperty.UnsetValue)
				{
					result = false;
				}
				else if (obj is object[] array)
				{
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] == DependencyProperty.UnsetValue)
						{
							result = false;
						}
					}
				}
			}
			StoreValueInBindingGroup(obj, bindingGroup);
		}
		else
		{
			bindingGroup.UseSourceValue(this);
		}
		return result;
	}

	internal override object UpdateSource(object convertedValue)
	{
		if (convertedValue == DependencyProperty.UnsetValue)
		{
			SetStatus(BindingStatusInternal.UpdateSourceError);
			return convertedValue;
		}
		object[] array = convertedValue as object[];
		int num = MutableBindingExpressions.Count;
		if (array.Length < num)
		{
			num = array.Length;
		}
		BeginSourceUpdate();
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			object obj = array[i];
			if (obj != Binding.DoNothing)
			{
				BindingExpressionBase bindingExpressionBase = MutableBindingExpressions[i];
				bindingExpressionBase.UpdateSource(obj);
				if (bindingExpressionBase.StatusInternal == BindingStatusInternal.UpdateSourceError)
				{
					SetStatus(BindingStatusInternal.UpdateSourceError);
				}
				flag = true;
			}
		}
		if (!flag)
		{
			base.IsInUpdate = false;
		}
		EndSourceUpdate();
		OnSourceUpdated();
		return convertedValue;
	}

	internal override bool UpdateSource(BindingGroup bindingGroup)
	{
		bool result = true;
		if (base.NeedsUpdate)
		{
			object value = bindingGroup.GetValue(this);
			UpdateSource(value);
			if (value == DependencyProperty.UnsetValue)
			{
				result = false;
			}
		}
		return result;
	}

	internal override void StoreValueInBindingGroup(object value, BindingGroup bindingGroup)
	{
		bindingGroup.SetValue(this, value);
		if (value is object[] array)
		{
			int num = MutableBindingExpressions.Count;
			if (array.Length < num)
			{
				num = array.Length;
			}
			for (int i = 0; i < num; i++)
			{
				MutableBindingExpressions[i].StoreValueInBindingGroup(array[i], bindingGroup);
			}
		}
		else
		{
			for (int num2 = MutableBindingExpressions.Count - 1; num2 >= 0; num2--)
			{
				MutableBindingExpressions[num2].StoreValueInBindingGroup(DependencyProperty.UnsetValue, bindingGroup);
			}
		}
	}

	internal override bool Validate(object value, ValidationStep validationStep)
	{
		if (value == Binding.DoNothing)
		{
			return true;
		}
		if (value == DependencyProperty.UnsetValue)
		{
			SetStatus(BindingStatusInternal.UpdateSourceError);
			return false;
		}
		bool result = base.Validate(value, validationStep);
		if (validationStep != 0)
		{
			object[] array = value as object[];
			int num = MutableBindingExpressions.Count;
			if (array.Length < num)
			{
				num = array.Length;
			}
			for (int i = 0; i < num; i++)
			{
				value = array[i];
				if (value != DependencyProperty.UnsetValue && value != Binding.DoNothing && !MutableBindingExpressions[i].Validate(value, validationStep))
				{
					array[i] = DependencyProperty.UnsetValue;
				}
			}
		}
		return result;
	}

	internal override bool CheckValidationRules(BindingGroup bindingGroup, ValidationStep validationStep)
	{
		if (!base.NeedsValidation)
		{
			return true;
		}
		if ((uint)validationStep <= 3u)
		{
			object value = bindingGroup.GetValue(this);
			bool num = Validate(value, validationStep);
			if (num && validationStep == ValidationStep.CommittedValue)
			{
				base.NeedsValidation = false;
			}
			return num;
		}
		throw new InvalidOperationException(SR.Format(SR.ValidationRule_UnknownStep, validationStep, bindingGroup));
	}

	internal override bool ValidateAndConvertProposedValue(out Collection<ProposedValue> values)
	{
		values = null;
		object rawProposedValue = GetRawProposedValue();
		if (!Validate(rawProposedValue, ValidationStep.RawProposedValue))
		{
			return false;
		}
		object valuesForChildBindings = GetValuesForChildBindings(rawProposedValue);
		if (base.IsDetached || valuesForChildBindings == DependencyProperty.UnsetValue || valuesForChildBindings == null)
		{
			return false;
		}
		int num = MutableBindingExpressions.Count;
		object[] array = (object[])valuesForChildBindings;
		if (array.Length < num)
		{
			num = array.Length;
		}
		values = new Collection<ProposedValue>();
		bool flag = true;
		for (int i = 0; i < num; i++)
		{
			object obj = array[i];
			if (obj == Binding.DoNothing)
			{
				continue;
			}
			if (obj == DependencyProperty.UnsetValue)
			{
				flag = false;
				continue;
			}
			BindingExpressionBase bindingExpressionBase = MutableBindingExpressions[i];
			bindingExpressionBase.Value = obj;
			if (!bindingExpressionBase.NeedsValidation)
			{
				continue;
			}
			Collection<ProposedValue> values2;
			bool flag2 = bindingExpressionBase.ValidateAndConvertProposedValue(out values2);
			if (values2 != null)
			{
				int j = 0;
				for (int count = values2.Count; j < count; j++)
				{
					values.Add(values2[j]);
				}
			}
			flag = flag && flag2;
		}
		return flag;
	}

	internal override object GetSourceItem(object newValue)
	{
		if (newValue == null)
		{
			return null;
		}
		int count = MutableBindingExpressions.Count;
		for (int i = 0; i < count; i++)
		{
			if (ItemsControl.EqualsEx(MutableBindingExpressions[i].GetValue(null, null), newValue))
			{
				return MutableBindingExpressions[i].GetSourceItem(newValue);
			}
		}
		return null;
	}

	private BindingExpressionBase AttachBindingExpression(int i, bool replaceExisting)
	{
		DependencyObject targetElement = base.TargetElement;
		if (targetElement == null)
		{
			return null;
		}
		BindingBase bindingBase = ParentMultiBinding.Bindings[i];
		MultiBinding.CheckTrigger(bindingBase);
		BindingExpressionBase bindingExpressionBase = bindingBase.CreateBindingExpression(targetElement, base.TargetProperty, this);
		if (replaceExisting)
		{
			MutableBindingExpressions[i] = bindingExpressionBase;
		}
		else
		{
			MutableBindingExpressions.Add(bindingExpressionBase);
		}
		bindingExpressionBase.Attach(targetElement, base.TargetProperty);
		return bindingExpressionBase;
	}

	internal override void HandlePropertyInvalidation(DependencyObject d, DependencyPropertyChangedEventArgs args)
	{
		DependencyProperty property = args.Property;
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer))
		{
			TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.GotPropertyChanged(TraceData.Identify(this), TraceData.Identify(d), property.Name));
		}
		bool flag = true;
		base.TransferIsDeferred = true;
		if (base.UsesLanguage && d == base.TargetElement && property == FrameworkElement.LanguageProperty)
		{
			InvalidateCulture();
			base.NeedsDataTransfer = true;
		}
		if (base.IsDetached)
		{
			return;
		}
		int count = MutableBindingExpressions.Count;
		for (int i = 0; i < count; i++)
		{
			BindingExpressionBase bindingExpressionBase = MutableBindingExpressions[i];
			if (bindingExpressionBase == null)
			{
				continue;
			}
			DependencySource[] sources = bindingExpressionBase.GetSources();
			if (sources != null)
			{
				foreach (DependencySource dependencySource in sources)
				{
					if (dependencySource.DependencyObject == d && dependencySource.DependencyProperty == property)
					{
						bindingExpressionBase.OnPropertyInvalidation(d, args);
						break;
					}
				}
			}
			if (bindingExpressionBase.IsDisconnected)
			{
				flag = false;
			}
		}
		base.TransferIsDeferred = false;
		if (flag)
		{
			Transfer();
		}
		else
		{
			Disconnect();
		}
	}

	internal override bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return false;
	}

	internal override void OnLostFocus(object sender, RoutedEventArgs e)
	{
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer))
		{
			TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.GotEvent(TraceData.Identify(this), "LostFocus", TraceData.Identify(sender)));
		}
		Update();
	}

	private void UpdateTarget(bool includeInnerBindings)
	{
		base.TransferIsDeferred = true;
		if (includeInnerBindings)
		{
			foreach (BindingExpressionBase mutableBindingExpression in MutableBindingExpressions)
			{
				mutableBindingExpression.UpdateTarget();
			}
		}
		base.TransferIsDeferred = false;
		base.NeedsDataTransfer = true;
		Transfer();
		base.NeedsUpdate = false;
	}

	private void Transfer()
	{
		if (base.NeedsDataTransfer && base.StatusInternal != 0 && !base.TransferIsDeferred)
		{
			TransferValue();
		}
	}

	private void TransferValue()
	{
		base.IsInTransfer = true;
		base.NeedsDataTransfer = false;
		DependencyObject targetElement = base.TargetElement;
		int num;
		if (targetElement != null)
		{
			bool flag = TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer);
			object unsetValue = DependencyProperty.UnsetValue;
			object obj = _tempValues;
			CultureInfo culture = GetCulture();
			int count = MutableBindingExpressions.Count;
			for (int i = 0; i < count; i++)
			{
				_tempValues[i] = MutableBindingExpressions[i].GetValue(targetElement, base.TargetProperty);
				if (flag)
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GetRawValueMulti(TraceData.Identify(this), i, TraceData.Identify(_tempValues[i])), this);
				}
			}
			if (Converter != null)
			{
				obj = Converter.Convert(_tempValues, base.TargetProperty.PropertyType, ParentMultiBinding.ConverterParameter, culture);
				if (base.IsDetached)
				{
					return;
				}
				if (flag)
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.UserConverter(TraceData.Identify(this), TraceData.Identify(obj)), this);
				}
			}
			else
			{
				if (base.EffectiveStringFormat == null)
				{
					if (TraceData.IsEnabled)
					{
						TraceData.TraceAndNotify(TraceEventType.Error, TraceData.MultiValueConverterMissingForTransfer, this);
					}
					goto IL_037a;
				}
				for (int j = 0; j < _tempValues.Length; j++)
				{
					if (_tempValues[j] == DependencyProperty.UnsetValue)
					{
						obj = DependencyProperty.UnsetValue;
						break;
					}
				}
			}
			if (base.EffectiveStringFormat == null || obj == Binding.DoNothing || obj == DependencyProperty.UnsetValue)
			{
				unsetValue = obj;
			}
			else
			{
				try
				{
					unsetValue = ((obj != _tempValues) ? string.Format(culture, base.EffectiveStringFormat, obj) : string.Format(culture, base.EffectiveStringFormat, _tempValues));
					if (flag)
					{
						TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.FormattedValue(TraceData.Identify(this), TraceData.Identify(unsetValue)), this);
					}
				}
				catch (FormatException)
				{
					unsetValue = DependencyProperty.UnsetValue;
					if (flag)
					{
						TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.FormattingFailed(TraceData.Identify(this), base.EffectiveStringFormat), this);
					}
				}
			}
			Array.Clear(_tempValues, 0, _tempValues.Length);
			if (unsetValue != Binding.DoNothing)
			{
				if (base.EffectiveTargetNullValue != DependencyProperty.UnsetValue && BindingExpressionBase.IsNullValue(unsetValue))
				{
					unsetValue = base.EffectiveTargetNullValue;
					if (flag)
					{
						TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.NullConverter(TraceData.Identify(this), TraceData.Identify(unsetValue)), this);
					}
				}
				if (unsetValue != DependencyProperty.UnsetValue && !base.TargetProperty.IsValidValue(unsetValue))
				{
					if (TraceData.IsEnabled)
					{
						TraceData.TraceAndNotify(base.TraceLevel, TraceData.BadValueAtTransfer, this, new object[2] { unsetValue, this });
					}
					if (flag)
					{
						TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.BadValueAtTransferExtended(TraceData.Identify(this), TraceData.Identify(unsetValue)), this);
					}
					unsetValue = DependencyProperty.UnsetValue;
				}
				if (unsetValue == DependencyProperty.UnsetValue)
				{
					unsetValue = UseFallbackValue();
					if (flag)
					{
						TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.UseFallback(TraceData.Identify(this), TraceData.Identify(unsetValue)), this);
					}
				}
				if (flag)
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.TransferValue(TraceData.Identify(this), TraceData.Identify(unsetValue)), this);
				}
				if (base.IsInUpdate)
				{
					num = ((!ItemsControl.EqualsEx(unsetValue, base.Value)) ? 1 : 0);
					if (num == 0)
					{
						goto IL_036c;
					}
				}
				else
				{
					num = 1;
				}
				ChangeValue(unsetValue, notify: true);
				Invalidate(isASubPropertyChange: false);
				Validation.ClearInvalid(this);
				goto IL_036c;
			}
		}
		goto IL_037a;
		IL_037a:
		base.IsInTransfer = false;
		return;
		IL_036c:
		Clean();
		if (num != 0)
		{
			OnTargetUpdated();
		}
		goto IL_037a;
	}

	private void OnTargetUpdated()
	{
		if (!base.NotifyOnTargetUpdated)
		{
			return;
		}
		DependencyObject targetElement = base.TargetElement;
		if (targetElement != null)
		{
			if (base.IsAttaching && this == targetElement.ReadLocalValue(base.TargetProperty))
			{
				base.Engine.AddTask(this, TaskOps.RaiseTargetUpdatedEvent);
			}
			else
			{
				BindingExpression.OnTargetUpdated(targetElement, base.TargetProperty);
			}
		}
	}

	private void OnSourceUpdated()
	{
		if (base.NotifyOnSourceUpdated)
		{
			DependencyObject targetElement = base.TargetElement;
			if (targetElement != null)
			{
				BindingExpression.OnSourceUpdated(targetElement, base.TargetProperty);
			}
		}
	}

	internal override bool ShouldReactToDirtyOverride()
	{
		foreach (BindingExpressionBase mutableBindingExpression in MutableBindingExpressions)
		{
			if (!mutableBindingExpression.ShouldReactToDirtyOverride())
			{
				return false;
			}
		}
		return true;
	}

	internal override bool UpdateOverride()
	{
		if (!base.NeedsUpdate || !base.IsReflective || base.IsInTransfer || base.StatusInternal == BindingStatusInternal.Unattached)
		{
			return true;
		}
		return UpdateValue();
	}
}
