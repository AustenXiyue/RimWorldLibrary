using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls;
using MS.Internal;
using MS.Internal.Data;

namespace System.Windows.Data;

/// <summary>Contains instance information about a single instance of a <see cref="T:System.Windows.Data.PriorityBinding" />.</summary>
public sealed class PriorityBindingExpression : BindingExpressionBase
{
	private const int NoActiveBindingExpressions = -1;

	private const int UnknownActiveBindingExpression = -2;

	private Collection<BindingExpressionBase> _list = new Collection<BindingExpressionBase>();

	private int _activeIndex = -2;

	private bool _isInInvalidateBinding;

	/// <summary>Gets the <see cref="T:System.Windows.Data.PriorityBinding" /> object from which this <see cref="T:System.Windows.Data.PriorityBindingExpression" /> is created.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.PriorityBinding" /> object from which this <see cref="T:System.Windows.Data.PriorityBindingExpression" /> is created.</returns>
	public PriorityBinding ParentPriorityBinding => (PriorityBinding)base.ParentBindingBase;

	/// <summary>Gets the collection of <see cref="T:System.Windows.Data.BindingExpression" /> objects inside this instance of <see cref="T:System.Windows.Data.PriorityBindingExpression" />.</summary>
	/// <returns>A read-only collection of the <see cref="T:System.Windows.Data.BindingExpression" /> objects. Although the return type is a collection of <see cref="T:System.Windows.Data.BindingExpressionBase" /> objects, the returned collection only contains <see cref="T:System.Windows.Data.BindingExpression" /> objects because the <see cref="T:System.Windows.Data.PriorityBinding" /> class currently supports only <see cref="T:System.Windows.Data.Binding" /> objects.</returns>
	public ReadOnlyCollection<BindingExpressionBase> BindingExpressions => new ReadOnlyCollection<BindingExpressionBase>(MutableBindingExpressions);

	/// <summary>Gets the active <see cref="T:System.Windows.Data.BindingExpression" /> object.</summary>
	/// <returns>The active <see cref="T:System.Windows.Data.BindingExpression" /> object; or null, if no <see cref="T:System.Windows.Data.BindingExpression" /> object is active. Although the return type is <see cref="T:System.Windows.Data.BindingExpressionBase" />, the returned object is only a <see cref="T:System.Windows.Data.BindingExpression" /> object because the <see cref="T:System.Windows.Data.PriorityBinding" /> class currently supports only <see cref="T:System.Windows.Data.Binding" /> objects.</returns>
	public BindingExpressionBase ActiveBindingExpression
	{
		get
		{
			if (_activeIndex >= 0)
			{
				return MutableBindingExpressions[_activeIndex];
			}
			return null;
		}
	}

	/// <summary>Gets a value that indicates whether the parent binding has a failed validation rule.</summary>
	/// <returns>true if the parent binding has a failed validation rule; otherwise, false.</returns>
	public override bool HasValidationError
	{
		get
		{
			if (_activeIndex >= 0)
			{
				return MutableBindingExpressions[_activeIndex].HasValidationError;
			}
			return false;
		}
	}

	internal int AttentiveBindingExpressions
	{
		get
		{
			if (_activeIndex != -1)
			{
				return _activeIndex + 1;
			}
			return MutableBindingExpressions.Count;
		}
	}

	private Collection<BindingExpressionBase> MutableBindingExpressions => _list;

	private PriorityBindingExpression(PriorityBinding binding, BindingExpressionBase owner)
		: base(binding, owner)
	{
	}

	/// <summary>Updates the target on the active binding. </summary>
	public override void UpdateTarget()
	{
		ActiveBindingExpression?.UpdateTarget();
	}

	/// <summary>Updates the source on the active binding.</summary>
	public override void UpdateSource()
	{
		ActiveBindingExpression?.UpdateSource();
	}

	internal override bool SetValue(DependencyObject d, DependencyProperty dp, object value)
	{
		BindingExpressionBase activeBindingExpression = ActiveBindingExpression;
		bool flag;
		if (activeBindingExpression != null)
		{
			flag = activeBindingExpression.SetValue(d, dp, value);
			if (flag)
			{
				base.Value = activeBindingExpression.Value;
				AdoptProperties(activeBindingExpression);
				NotifyCommitManager();
			}
		}
		else
		{
			flag = true;
		}
		return flag;
	}

	internal static PriorityBindingExpression CreateBindingExpression(DependencyObject d, DependencyProperty dp, PriorityBinding binding, BindingExpressionBase owner)
	{
		if (dp.GetMetadata(d.DependencyObjectType) is FrameworkPropertyMetadata { IsDataBindingAllowed: false } || dp.ReadOnly)
		{
			throw new ArgumentException(SR.Format(SR.PropertyNotBindable, dp.Name), "dp");
		}
		return new PriorityBindingExpression(binding, owner);
	}

	internal override bool AttachOverride(DependencyObject d, DependencyProperty dp)
	{
		if (!base.AttachOverride(d, dp))
		{
			return false;
		}
		if (base.TargetElement == null)
		{
			return false;
		}
		SetStatus(BindingStatusInternal.Active);
		int count = ParentPriorityBinding.Bindings.Count;
		_activeIndex = -1;
		for (int i = 0; i < count; i++)
		{
			AttachBindingExpression(i, replaceExisting: false);
		}
		return true;
	}

	internal override void DetachOverride()
	{
		int count = MutableBindingExpressions.Count;
		for (int i = 0; i < count; i++)
		{
			MutableBindingExpressions[i]?.Detach();
		}
		ChangeSources(null);
		base.DetachOverride();
	}

	internal override void InvalidateChild(BindingExpressionBase bindingExpression)
	{
		if (_isInInvalidateBinding)
		{
			return;
		}
		_isInInvalidateBinding = true;
		int num = MutableBindingExpressions.IndexOf(bindingExpression);
		DependencyObject targetElement = base.TargetElement;
		if (targetElement != null && 0 <= num && num < AttentiveBindingExpressions)
		{
			if (num != _activeIndex || (bindingExpression.StatusInternal != BindingStatusInternal.Active && !bindingExpression.UsingFallbackValue))
			{
				ChooseActiveBindingExpression(targetElement);
			}
			base.UsingFallbackValue = false;
			BindingExpressionBase activeBindingExpression = ActiveBindingExpression;
			object obj = ((activeBindingExpression != null) ? activeBindingExpression.GetValue(targetElement, base.TargetProperty) : UseFallbackValue());
			ChangeValue(obj, notify: true);
			if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer))
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.PriorityTransfer(TraceData.Identify(this), TraceData.Identify(obj), _activeIndex, TraceData.Identify(activeBindingExpression)), this);
			}
			if (!base.IsAttaching)
			{
				targetElement.InvalidateProperty(base.TargetProperty);
			}
		}
		_isInInvalidateBinding = false;
	}

	internal override void ChangeSourcesForChild(BindingExpressionBase bindingExpression, WeakDependencySource[] newSources)
	{
		int num = MutableBindingExpressions.IndexOf(bindingExpression);
		if (num >= 0)
		{
			WeakDependencySource[] newSources2 = BindingExpressionBase.CombineSources(num, MutableBindingExpressions, AttentiveBindingExpressions, newSources);
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
			bindingExpression = AttachBindingExpression(num, replaceExisting: true);
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

	internal override bool ShouldReactToDirtyOverride()
	{
		return ActiveBindingExpression?.ShouldReactToDirtyOverride() ?? false;
	}

	internal override object GetRawProposedValue()
	{
		BindingExpressionBase activeBindingExpression = ActiveBindingExpression;
		if (activeBindingExpression != null)
		{
			return activeBindingExpression.GetRawProposedValue();
		}
		return DependencyProperty.UnsetValue;
	}

	internal override object ConvertProposedValue(object rawValue)
	{
		BindingExpressionBase activeBindingExpression = ActiveBindingExpression;
		if (activeBindingExpression != null)
		{
			return activeBindingExpression.ConvertProposedValue(rawValue);
		}
		return DependencyProperty.UnsetValue;
	}

	internal override bool ObtainConvertedProposedValue(BindingGroup bindingGroup)
	{
		return ActiveBindingExpression?.ObtainConvertedProposedValue(bindingGroup) ?? true;
	}

	internal override object UpdateSource(object convertedValue)
	{
		BindingExpressionBase activeBindingExpression = ActiveBindingExpression;
		object result;
		if (activeBindingExpression != null)
		{
			result = activeBindingExpression.UpdateSource(convertedValue);
			if (activeBindingExpression.StatusInternal == BindingStatusInternal.UpdateSourceError)
			{
				SetStatus(BindingStatusInternal.UpdateSourceError);
			}
		}
		else
		{
			result = DependencyProperty.UnsetValue;
		}
		return result;
	}

	internal override bool UpdateSource(BindingGroup bindingGroup)
	{
		bool result = true;
		BindingExpressionBase activeBindingExpression = ActiveBindingExpression;
		if (activeBindingExpression != null)
		{
			result = activeBindingExpression.UpdateSource(bindingGroup);
			if (activeBindingExpression.StatusInternal == BindingStatusInternal.UpdateSourceError)
			{
				SetStatus(BindingStatusInternal.UpdateSourceError);
			}
		}
		return result;
	}

	internal override void StoreValueInBindingGroup(object value, BindingGroup bindingGroup)
	{
		ActiveBindingExpression?.StoreValueInBindingGroup(value, bindingGroup);
	}

	internal override bool Validate(object value, ValidationStep validationStep)
	{
		return ActiveBindingExpression?.Validate(value, validationStep) ?? true;
	}

	internal override bool CheckValidationRules(BindingGroup bindingGroup, ValidationStep validationStep)
	{
		return ActiveBindingExpression?.CheckValidationRules(bindingGroup, validationStep) ?? true;
	}

	internal override bool ValidateAndConvertProposedValue(out Collection<ProposedValue> values)
	{
		BindingExpressionBase activeBindingExpression = ActiveBindingExpression;
		if (activeBindingExpression != null)
		{
			return activeBindingExpression.ValidateAndConvertProposedValue(out values);
		}
		values = null;
		return true;
	}

	internal override object GetSourceItem(object newValue)
	{
		BindingExpressionBase activeBindingExpression = ActiveBindingExpression;
		if (activeBindingExpression != null)
		{
			return activeBindingExpression.GetSourceItem(newValue);
		}
		return true;
	}

	internal override void UpdateCommitState()
	{
		BindingExpressionBase activeBindingExpression = ActiveBindingExpression;
		if (activeBindingExpression != null)
		{
			AdoptProperties(activeBindingExpression);
		}
	}

	private BindingExpressionBase AttachBindingExpression(int i, bool replaceExisting)
	{
		DependencyObject targetElement = base.TargetElement;
		if (targetElement == null)
		{
			return null;
		}
		BindingExpressionBase bindingExpressionBase = ParentPriorityBinding.Bindings[i].CreateBindingExpression(targetElement, base.TargetProperty, this);
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

	private void ChooseActiveBindingExpression(DependencyObject target)
	{
		int count = MutableBindingExpressions.Count;
		int i;
		for (i = 0; i < count; i++)
		{
			BindingExpressionBase bindingExpressionBase = MutableBindingExpressions[i];
			if (bindingExpressionBase.StatusInternal == BindingStatusInternal.Inactive)
			{
				bindingExpressionBase.Activate();
			}
			if (bindingExpressionBase.StatusInternal == BindingStatusInternal.Active || bindingExpressionBase.UsingFallbackValue)
			{
				break;
			}
		}
		int num = ((i < count) ? i : (-1));
		if (num == _activeIndex)
		{
			return;
		}
		int activeIndex = _activeIndex;
		_activeIndex = num;
		AdoptProperties(ActiveBindingExpression);
		WeakDependencySource[] newSources = BindingExpressionBase.CombineSources(-1, MutableBindingExpressions, AttentiveBindingExpressions, null);
		ChangeSources(newSources);
		if (num != -1)
		{
			for (i = activeIndex; i > num; i--)
			{
				MutableBindingExpressions[i].Deactivate();
			}
		}
	}

	private void ChangeValue()
	{
	}

	internal override void HandlePropertyInvalidation(DependencyObject d, DependencyPropertyChangedEventArgs args)
	{
		DependencyProperty property = args.Property;
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GotPropertyChanged(TraceData.Identify(this), TraceData.Identify(d), property.Name), this);
		}
		for (int i = 0; i < AttentiveBindingExpressions; i++)
		{
			BindingExpressionBase bindingExpressionBase = MutableBindingExpressions[i];
			DependencySource[] sources = bindingExpressionBase.GetSources();
			if (sources == null)
			{
				continue;
			}
			foreach (DependencySource dependencySource in sources)
			{
				if (dependencySource.DependencyObject == d && dependencySource.DependencyProperty == property)
				{
					bindingExpressionBase.OnPropertyInvalidation(d, args);
					break;
				}
			}
		}
	}
}
