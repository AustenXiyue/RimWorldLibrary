using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Data;
using MS.Internal.Documents;

namespace System.Windows.Data;

/// <summary>Represents the base class for <see cref="T:System.Windows.Data.BindingExpression" />, <see cref="T:System.Windows.Data.PriorityBindingExpression" />, and <see cref="T:System.Windows.Data.MultiBindingExpression" />.</summary>
public abstract class BindingExpressionBase : Expression, IWeakEventListener
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
		UpdateOnPropertyChanged = 0u,
		UpdateOnLostFocus = 0x400u,
		UpdateExplicitly = 0x800u,
		UpdateDefault = 0xC00u,
		PathGeneratedInternally = 0x2000u,
		ValidatesOnExceptions = 0x1000000u,
		ValidatesOnDataErrors = 0x2000000u,
		ValidatesOnNotifyDataErrors = 0x20000000u,
		Default = 0xC04u,
		IllegalInput = 0x4000000u,
		PropagationMask = 7u,
		UpdateMask = 0xC00u
	}

	[Flags]
	private enum PrivateFlags : uint
	{
		iSourceToTarget = 1u,
		iTargetToSource = 2u,
		iPropDefault = 4u,
		iNotifyOnTargetUpdated = 8u,
		iDefaultValueConverter = 0x10u,
		iInTransfer = 0x20u,
		iInUpdate = 0x40u,
		iTransferPending = 0x80u,
		iNeedDataTransfer = 0x100u,
		iTransferDeferred = 0x200u,
		iUpdateOnLostFocus = 0x400u,
		iUpdateExplicitly = 0x800u,
		iUpdateDefault = 0xC00u,
		iNeedsUpdate = 0x1000u,
		iPathGeneratedInternally = 0x2000u,
		iUsingMentor = 0x4000u,
		iResolveNamesInTemplate = 0x8000u,
		iDetaching = 0x10000u,
		iNeedsCollectionView = 0x20000u,
		iInPriorityBindingExpression = 0x40000u,
		iInMultiBindingExpression = 0x80000u,
		iUsingFallbackValue = 0x100000u,
		iNotifyOnValidationError = 0x200000u,
		iAttaching = 0x400000u,
		iNotifyOnSourceUpdated = 0x800000u,
		iValidatesOnExceptions = 0x1000000u,
		iValidatesOnDataErrors = 0x2000000u,
		iIllegalInput = 0x4000000u,
		iNeedsValidation = 0x8000000u,
		iTargetWantsXTNotification = 0x10000000u,
		iValidatesOnNotifyDataErrors = 0x20000000u,
		iDataErrorsChangedPending = 0x40000000u,
		iDeferUpdateForComposition = 0x80000000u,
		iPropagationMask = 7u,
		iUpdateMask = 0xC00u,
		iAdoptionMask = 0x8001003u
	}

	internal class ProposedValue
	{
		private BindingExpression _bindingExpression;

		private object _rawValue;

		private object _convertedValue;

		internal BindingExpression BindingExpression => _bindingExpression;

		internal object RawValue => _rawValue;

		internal object ConvertedValue => _convertedValue;

		internal ProposedValue(BindingExpression bindingExpression, object rawValue, object convertedValue)
		{
			_bindingExpression = bindingExpression;
			_rawValue = rawValue;
			_convertedValue = convertedValue;
		}
	}

	internal enum Feature
	{
		ParentBindingExpressionBase,
		ValidationError,
		NotifyDataErrors,
		EffectiveStringFormat,
		EffectiveTargetNullValue,
		BindingGroup,
		Timer,
		UpdateTargetOperation,
		Converter,
		SourceType,
		DataProvider,
		CollectionViewSource,
		DynamicConverter,
		DataErrorValue,
		LastFeatureId
	}

	internal static readonly DependencyProperty NoTargetProperty;

	private BindingBase _binding;

	private WeakReference _targetElement;

	private DependencyProperty _targetProperty;

	private DataBindEngine _engine;

	private PrivateFlags _flags;

	private object _value = DefaultValueObject;

	private BindingStatusInternal _status;

	private WeakDependencySource[] _sources;

	private object _culture = DefaultValueObject;

	internal static readonly object DefaultValueObject;

	internal static readonly object DisconnectedItem;

	private static readonly WeakReference<BindingGroup> NullBindingGroupReference;

	private UncommonValueTable _values;

	/// <summary>Gets the element that is the binding target object of this binding expression.</summary>
	/// <returns>The element that is the binding target object of this binding expression.</returns>
	public DependencyObject Target => TargetElement;

	/// <summary>Gets the binding target property of this binding expression.</summary>
	/// <returns>The binding target property of this binding expression.</returns>
	public DependencyProperty TargetProperty => _targetProperty;

	/// <summary>Gets the <see cref="T:System.Windows.Data.BindingBase" /> object from which this <see cref="T:System.Windows.Data.BindingExpressionBase" /> object is created.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.BindingBase" /> object from which this <see cref="T:System.Windows.Data.BindingExpressionBase" /> object is created.</returns>
	public BindingBase ParentBindingBase => _binding;

	/// <summary>Gets the <see cref="T:System.Windows.Data.BindingGroup" /> that this binding expression belongs to.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.BindingGroup" /> that this binding expression belongs to. This property returns null if the <see cref="T:System.Windows.Data.BindingExpressionBase" /> is not part of a  <see cref="T:System.Windows.Data.BindingGroup" />.</returns>
	public BindingGroup BindingGroup
	{
		get
		{
			WeakReference<BindingGroup> weakReference = (WeakReference<BindingGroup>)RootBindingExpression.GetValue(Feature.BindingGroup, null);
			if (weakReference == null)
			{
				return null;
			}
			if (!weakReference.TryGetTarget(out var target))
			{
				return null;
			}
			return target;
		}
	}

	/// <summary>Gets the status of the binding expression.</summary>
	/// <returns>A <see cref="T:System.Windows.Data.BindingStatus" /> value that describes the status of the binding expression.</returns>
	public BindingStatus Status => (BindingStatus)_status;

	internal BindingStatusInternal StatusInternal => _status;

	/// <summary>Gets the <see cref="T:System.Windows.Controls.ValidationError" /> that caused this instance of <see cref="T:System.Windows.Data.BindingExpressionBase" /> to be invalid.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.ValidationError" /> that caused this instance of <see cref="T:System.Windows.Data.BindingExpressionBase" /> to be invalid.</returns>
	public virtual ValidationError ValidationError => BaseValidationError;

	internal ValidationError BaseValidationError => (ValidationError)GetValue(Feature.ValidationError, null);

	internal List<ValidationError> NotifyDataErrors => (List<ValidationError>)GetValue(Feature.NotifyDataErrors, null);

	/// <summary>Gets a value that indicates whether the parent binding has a failed validation rule.</summary>
	/// <returns>true if the parent binding has a failed validation rule; otherwise, false.</returns>
	public virtual bool HasError => HasValidationError;

	/// <summary>Gets a value that indicates whether the parent binding has a failed validation rule.</summary>
	/// <returns>true if the parent binding has a failed validation rule; otherwise, false.</returns>
	public virtual bool HasValidationError
	{
		get
		{
			if (!HasValue(Feature.ValidationError))
			{
				return HasValue(Feature.NotifyDataErrors);
			}
			return true;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the target of the binding has a value that has not been written to the source.</summary>
	/// <returns>true if the target has a value that has not been written to the source; otherwise, false.</returns>
	public bool IsDirty => NeedsUpdate;

	/// <summary>Gets a collection of <see cref="T:System.Windows.Controls.ValidationError" /> objects that caused this instance of <see cref="T:System.Windows.Data.BindingExpressionBase" /> to be invalid.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Controls.ValidationError" /> objects that caused this instance of <see cref="T:System.Windows.Data.BindingExpressionBase" /> to be invalid.  The value is null if there are no errors.</returns>
	public virtual ReadOnlyCollection<ValidationError> ValidationErrors
	{
		get
		{
			if (HasError)
			{
				List<ValidationError> list;
				if (!HasValue(Feature.ValidationError))
				{
					list = NotifyDataErrors;
				}
				else
				{
					list = ((NotifyDataErrors != null) ? new List<ValidationError>(NotifyDataErrors) : new List<ValidationError>());
					list.Insert(0, BaseValidationError);
				}
				return new ReadOnlyCollection<ValidationError>(list);
			}
			return null;
		}
	}

	internal bool IsAttaching
	{
		get
		{
			return TestFlag(PrivateFlags.iAttaching);
		}
		set
		{
			ChangeFlag(PrivateFlags.iAttaching, value);
		}
	}

	internal bool IsDetaching
	{
		get
		{
			return TestFlag(PrivateFlags.iDetaching);
		}
		set
		{
			ChangeFlag(PrivateFlags.iDetaching, value);
		}
	}

	internal bool IsDetached => _status == BindingStatusInternal.Detached;

	private bool IsAttached
	{
		get
		{
			if (_status != 0 && _status != BindingStatusInternal.Detached)
			{
				return !IsDetaching;
			}
			return false;
		}
	}

	internal bool IsDynamic
	{
		get
		{
			if (TestFlag(PrivateFlags.iSourceToTarget))
			{
				if (IsInMultiBindingExpression)
				{
					return ParentMultiBindingExpression.IsDynamic;
				}
				return true;
			}
			return false;
		}
	}

	internal bool IsReflective
	{
		get
		{
			if (TestFlag(PrivateFlags.iTargetToSource))
			{
				if (IsInMultiBindingExpression)
				{
					return ParentMultiBindingExpression.IsReflective;
				}
				return true;
			}
			return false;
		}
		set
		{
			ChangeFlag(PrivateFlags.iTargetToSource, value);
		}
	}

	internal bool UseDefaultValueConverter
	{
		get
		{
			return TestFlag(PrivateFlags.iDefaultValueConverter);
		}
		set
		{
			ChangeFlag(PrivateFlags.iDefaultValueConverter, value);
		}
	}

	internal bool IsOneWayToSource => (_flags & PrivateFlags.iPropagationMask) == PrivateFlags.iTargetToSource;

	internal bool IsUpdateOnPropertyChanged => (_flags & PrivateFlags.iUpdateDefault) == 0;

	internal bool IsUpdateOnLostFocus => TestFlag(PrivateFlags.iUpdateOnLostFocus);

	internal bool IsTransferPending
	{
		get
		{
			return TestFlag(PrivateFlags.iTransferPending);
		}
		set
		{
			ChangeFlag(PrivateFlags.iTransferPending, value);
		}
	}

	internal bool TransferIsDeferred
	{
		get
		{
			return TestFlag(PrivateFlags.iTransferDeferred);
		}
		set
		{
			ChangeFlag(PrivateFlags.iTransferDeferred, value);
		}
	}

	internal bool IsInTransfer
	{
		get
		{
			return TestFlag(PrivateFlags.iInTransfer);
		}
		set
		{
			ChangeFlag(PrivateFlags.iInTransfer, value);
		}
	}

	internal bool IsInUpdate
	{
		get
		{
			return TestFlag(PrivateFlags.iInUpdate);
		}
		set
		{
			ChangeFlag(PrivateFlags.iInUpdate, value);
		}
	}

	internal bool UsingFallbackValue
	{
		get
		{
			return TestFlag(PrivateFlags.iUsingFallbackValue);
		}
		set
		{
			ChangeFlag(PrivateFlags.iUsingFallbackValue, value);
		}
	}

	internal bool UsingMentor
	{
		get
		{
			return TestFlag(PrivateFlags.iUsingMentor);
		}
		set
		{
			ChangeFlag(PrivateFlags.iUsingMentor, value);
		}
	}

	internal bool ResolveNamesInTemplate
	{
		get
		{
			return TestFlag(PrivateFlags.iResolveNamesInTemplate);
		}
		set
		{
			ChangeFlag(PrivateFlags.iResolveNamesInTemplate, value);
		}
	}

	internal bool NeedsDataTransfer
	{
		get
		{
			return TestFlag(PrivateFlags.iNeedDataTransfer);
		}
		set
		{
			ChangeFlag(PrivateFlags.iNeedDataTransfer, value);
		}
	}

	internal bool NeedsUpdate
	{
		get
		{
			return TestFlag(PrivateFlags.iNeedsUpdate);
		}
		set
		{
			ChangeFlag(PrivateFlags.iNeedsUpdate, value);
			if (value)
			{
				NeedsValidation = true;
			}
		}
	}

	internal bool NeedsValidation
	{
		get
		{
			if (!TestFlag(PrivateFlags.iNeedsValidation))
			{
				return HasValue(Feature.ValidationError);
			}
			return true;
		}
		set
		{
			ChangeFlag(PrivateFlags.iNeedsValidation, value);
		}
	}

	internal bool NotifyOnTargetUpdated
	{
		get
		{
			return TestFlag(PrivateFlags.iNotifyOnTargetUpdated);
		}
		set
		{
			ChangeFlag(PrivateFlags.iNotifyOnTargetUpdated, value);
		}
	}

	internal bool NotifyOnSourceUpdated
	{
		get
		{
			return TestFlag(PrivateFlags.iNotifyOnSourceUpdated);
		}
		set
		{
			ChangeFlag(PrivateFlags.iNotifyOnSourceUpdated, value);
		}
	}

	internal bool NotifyOnValidationError
	{
		get
		{
			return TestFlag(PrivateFlags.iNotifyOnValidationError);
		}
		set
		{
			ChangeFlag(PrivateFlags.iNotifyOnValidationError, value);
		}
	}

	internal bool IsInPriorityBindingExpression => TestFlag(PrivateFlags.iInPriorityBindingExpression);

	internal bool IsInMultiBindingExpression => TestFlag(PrivateFlags.iInMultiBindingExpression);

	internal bool IsInBindingExpressionCollection => TestFlag(PrivateFlags.iInPriorityBindingExpression | PrivateFlags.iInMultiBindingExpression);

	internal bool ValidatesOnExceptions => TestFlag(PrivateFlags.iValidatesOnExceptions);

	internal bool ValidatesOnDataErrors => TestFlag(PrivateFlags.iValidatesOnDataErrors);

	internal bool TargetWantsCrossThreadNotifications
	{
		get
		{
			return TestFlag(PrivateFlags.iTargetWantsXTNotification);
		}
		set
		{
			ChangeFlag(PrivateFlags.iTargetWantsXTNotification, value);
		}
	}

	internal bool IsDataErrorsChangedPending
	{
		get
		{
			return TestFlag(PrivateFlags.iDataErrorsChangedPending);
		}
		set
		{
			ChangeFlag(PrivateFlags.iDataErrorsChangedPending, value);
		}
	}

	internal bool IsUpdateDeferredForComposition
	{
		get
		{
			return TestFlag(PrivateFlags.iDeferUpdateForComposition);
		}
		set
		{
			ChangeFlag(PrivateFlags.iDeferUpdateForComposition, value);
		}
	}

	internal bool ValidatesOnNotifyDataErrors => TestFlag(PrivateFlags.iValidatesOnNotifyDataErrors);

	internal MultiBindingExpression ParentMultiBindingExpression => GetValue(Feature.ParentBindingExpressionBase, null) as MultiBindingExpression;

	internal PriorityBindingExpression ParentPriorityBindingExpression => GetValue(Feature.ParentBindingExpressionBase, null) as PriorityBindingExpression;

	internal BindingExpressionBase ParentBindingExpressionBase => (BindingExpressionBase)GetValue(Feature.ParentBindingExpressionBase, null);

	internal object FallbackValue => ConvertFallbackValue(ParentBindingBase.FallbackValue, TargetProperty, this);

	internal object DefaultValue
	{
		get
		{
			DependencyObject targetElement = TargetElement;
			if (targetElement != null)
			{
				return TargetProperty.GetDefaultValue(targetElement.DependencyObjectType);
			}
			return DependencyProperty.UnsetValue;
		}
	}

	internal string EffectiveStringFormat => (string)GetValue(Feature.EffectiveStringFormat, null);

	internal object EffectiveTargetNullValue => GetValue(Feature.EffectiveTargetNullValue, DependencyProperty.UnsetValue);

	internal BindingExpressionBase RootBindingExpression
	{
		get
		{
			BindingExpressionBase bindingExpressionBase = this;
			for (BindingExpressionBase parentBindingExpressionBase = ParentBindingExpressionBase; parentBindingExpressionBase != null; parentBindingExpressionBase = bindingExpressionBase.ParentBindingExpressionBase)
			{
				bindingExpressionBase = parentBindingExpressionBase;
			}
			return bindingExpressionBase;
		}
	}

	internal virtual bool IsParentBindingUpdateTriggerDefault => false;

	internal bool UsesLanguage => ParentBindingBase.ConverterCultureInternal == null;

	internal bool IsEligibleForCommit
	{
		get
		{
			if (IsDetaching)
			{
				return false;
			}
			switch (StatusInternal)
			{
			case BindingStatusInternal.Unattached:
			case BindingStatusInternal.Inactive:
			case BindingStatusInternal.Detached:
			case BindingStatusInternal.PathError:
				return false;
			default:
				return true;
			}
		}
	}

	internal DependencyObject TargetElement
	{
		get
		{
			if (_targetElement != null)
			{
				if (_targetElement.Target is DependencyObject result)
				{
					return result;
				}
				_targetElement = null;
				Detach();
			}
			return null;
		}
	}

	internal WeakReference TargetElementReference => _targetElement;

	internal DataBindEngine Engine => _engine;

	internal Dispatcher Dispatcher
	{
		get
		{
			if (_engine == null)
			{
				return null;
			}
			return _engine.Dispatcher;
		}
	}

	internal object Value
	{
		get
		{
			if (_value == DefaultValueObject)
			{
				ChangeValue(UseFallbackValue(), notify: false);
			}
			return _value;
		}
		set
		{
			ChangeValue(value, notify: true);
			Dirty();
		}
	}

	internal WeakDependencySource[] WeakSources => _sources;

	internal virtual bool IsDisconnected => false;

	internal TraceEventType TraceLevel
	{
		get
		{
			if (ParentBindingBase.FallbackValue != DependencyProperty.UnsetValue)
			{
				return TraceEventType.Warning;
			}
			if (IsInBindingExpressionCollection)
			{
				return TraceEventType.Warning;
			}
			return TraceEventType.Error;
		}
	}

	internal event EventHandler<BindingValueChangedEventArgs> ValueChanged;

	static BindingExpressionBase()
	{
		NoTargetProperty = DependencyProperty.RegisterAttached("NoTarget", typeof(object), typeof(BindingExpressionBase), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
		DefaultValueObject = new NamedObject("DefaultValue");
		DisconnectedItem = new NamedObject("DisconnectedItem");
		NullBindingGroupReference = new WeakReference<BindingGroup>(null);
	}

	internal BindingExpressionBase(BindingBase binding, BindingExpressionBase parent)
		: base(ExpressionMode.SupportsUnboundSources)
	{
		if (binding == null)
		{
			throw new ArgumentNullException("binding");
		}
		_binding = binding;
		SetValue(Feature.ParentBindingExpressionBase, parent, null);
		_flags = (PrivateFlags)binding.Flags;
		if (parent != null)
		{
			ResolveNamesInTemplate = parent.ResolveNamesInTemplate;
			Type type = parent.GetType();
			if (type == typeof(MultiBindingExpression))
			{
				ChangeFlag(PrivateFlags.iInMultiBindingExpression, value: true);
			}
			else if (type == typeof(PriorityBindingExpression))
			{
				ChangeFlag(PrivateFlags.iInPriorityBindingExpression, value: true);
			}
		}
		PresentationTraceLevel traceLevel = PresentationTraceSources.GetTraceLevel(binding);
		if (traceLevel > PresentationTraceLevel.None)
		{
			PresentationTraceSources.SetTraceLevel(this, traceLevel);
		}
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.CreateExpression))
		{
			if (parent == null)
			{
				TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.CreatedExpression(TraceData.Identify(this), TraceData.Identify(binding)), this);
			}
			else
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.CreatedExpressionInParent(TraceData.Identify(this), TraceData.Identify(binding), TraceData.Identify(parent)), this);
			}
		}
		if (LookupValidationRule(typeof(ExceptionValidationRule)) != null)
		{
			ChangeFlag(PrivateFlags.iValidatesOnExceptions, value: true);
		}
		if (LookupValidationRule(typeof(DataErrorValidationRule)) != null)
		{
			ChangeFlag(PrivateFlags.iValidatesOnDataErrors, value: true);
		}
	}

	/// <summary>Forces a data transfer from the binding source to the binding target.</summary>
	public virtual void UpdateTarget()
	{
	}

	/// <summary>Sends the current binding target value to the binding source in <see cref="F:System.Windows.Data.BindingMode.TwoWay" /> or <see cref="F:System.Windows.Data.BindingMode.OneWayToSource" /> bindings.</summary>
	public virtual void UpdateSource()
	{
	}

	/// <summary>Runs any <see cref="T:System.Windows.Controls.ValidationRule" /> objects on the associated <see cref="T:System.Windows.Data.Binding" /> that have the <see cref="P:System.Windows.Controls.ValidationRule.ValidationStep" /> property set to <see cref="F:System.Windows.Controls.ValidationStep.RawProposedValue" /> or <see cref="F:System.Windows.Controls.ValidationStep.ConvertedProposedValue" />. This method does not update the source.</summary>
	/// <returns>true if the validation rules succeed; otherwise, false.</returns>
	public bool ValidateWithoutUpdate()
	{
		if (!NeedsValidation)
		{
			return true;
		}
		Collection<ProposedValue> values;
		return ValidateAndConvertProposedValue(out values);
	}

	internal sealed override void OnAttach(DependencyObject d, DependencyProperty dp)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		Attach(d, dp);
	}

	internal sealed override void OnDetach(DependencyObject d, DependencyProperty dp)
	{
		Detach();
	}

	internal override object GetValue(DependencyObject d, DependencyProperty dp)
	{
		return Value;
	}

	internal override bool SetValue(DependencyObject d, DependencyProperty dp, object value)
	{
		if (IsReflective)
		{
			Value = value;
			return true;
		}
		return false;
	}

	internal override void OnPropertyInvalidation(DependencyObject d, DependencyPropertyChangedEventArgs args)
	{
		if (!IsDetached)
		{
			IsTransferPending = true;
			if (Dispatcher.Thread == Thread.CurrentThread)
			{
				HandlePropertyInvalidation(d, args);
				return;
			}
			Engine.Marshal(HandlePropertyInvalidationOperation, new object[2] { d, args });
		}
	}

	internal override DependencySource[] GetSources()
	{
		int num = ((_sources != null) ? _sources.Length : 0);
		if (num == 0)
		{
			return null;
		}
		DependencySource[] array = new DependencySource[num];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			DependencyObject dependencyObject = _sources[i].DependencyObject;
			if (dependencyObject != null)
			{
				array[num2++] = new DependencySource(dependencyObject, _sources[i].DependencyProperty);
			}
		}
		if (num2 < num)
		{
			DependencySource[] sourceArray = array;
			array = new DependencySource[num2];
			Array.Copy(sourceArray, 0, array, 0, num2);
		}
		return array;
	}

	internal override Expression Copy(DependencyObject targetObject, DependencyProperty targetDP)
	{
		return ParentBindingBase.CreateBindingExpression(targetObject, targetDP);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the listener handled the event; otherwise, false.</returns>
	/// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager" /> calling this method. This only recognizes manager objects of type <see cref="T:System.Collections.Specialized.CollectionChangedEventManager" />.</param>
	/// <param name="sender">Object that originated the event.</param>
	/// <param name="e">Event data.</param>
	bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return ReceiveWeakEvent(managerType, sender, e);
	}

	internal static BindingExpressionBase CreateUntargetedBindingExpression(DependencyObject d, BindingBase binding)
	{
		return binding.CreateBindingExpression(d, NoTargetProperty);
	}

	internal void Attach(DependencyObject d)
	{
		Attach(d, NoTargetProperty);
	}

	internal virtual bool AttachOverride(DependencyObject target, DependencyProperty dp)
	{
		_targetElement = new WeakReference(target);
		_targetProperty = dp;
		DataBindEngine currentDataBindEngine = DataBindEngine.CurrentDataBindEngine;
		if (currentDataBindEngine == null || currentDataBindEngine.IsShutDown)
		{
			return false;
		}
		_engine = currentDataBindEngine;
		DetermineEffectiveStringFormat();
		DetermineEffectiveTargetNullValue();
		DetermineEffectiveUpdateBehavior();
		DetermineEffectiveValidatesOnNotifyDataErrors();
		if (dp == TextBox.TextProperty && IsReflective && !IsInBindingExpressionCollection && target is TextBoxBase textBoxBase)
		{
			textBoxBase.PreviewTextInput += OnPreviewTextInput;
		}
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.AttachExpression(TraceData.Identify(this), target.GetType().FullName, dp.Name, AvTrace.GetHashCodeHelper(target)), this);
		}
		return true;
	}

	internal virtual void DetachOverride()
	{
		UpdateValidationError(null);
		UpdateNotifyDataErrorValidationErrors(null);
		if (TargetProperty == TextBox.TextProperty && IsReflective && !IsInBindingExpressionCollection && TargetElement is TextBoxBase textBoxBase)
		{
			textBoxBase.PreviewTextInput -= OnPreviewTextInput;
		}
		_engine = null;
		_targetElement = null;
		_targetProperty = null;
		SetStatus(BindingStatusInternal.Detached);
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.DetachExpression(TraceData.Identify(this)), this);
		}
	}

	internal abstract void InvalidateChild(BindingExpressionBase bindingExpression);

	internal abstract void ChangeSourcesForChild(BindingExpressionBase bindingExpression, WeakDependencySource[] newSources);

	internal abstract void ReplaceChild(BindingExpressionBase bindingExpression);

	internal abstract void HandlePropertyInvalidation(DependencyObject d, DependencyPropertyChangedEventArgs args);

	private object HandlePropertyInvalidationOperation(object o)
	{
		object[] array = (object[])o;
		HandlePropertyInvalidation((DependencyObject)array[0], (DependencyPropertyChangedEventArgs)array[1]);
		return null;
	}

	internal void OnBindingGroupChanged(bool joining)
	{
		if (joining)
		{
			if (IsParentBindingUpdateTriggerDefault)
			{
				if (IsUpdateOnLostFocus)
				{
					LostFocusEventManager.RemoveHandler(TargetElement, OnLostFocus);
				}
				SetUpdateSourceTrigger(UpdateSourceTrigger.Explicit);
			}
		}
		else if (IsParentBindingUpdateTriggerDefault)
		{
			Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(RestoreUpdateTriggerOperation), null);
		}
	}

	private object RestoreUpdateTriggerOperation(object arg)
	{
		DependencyObject targetElement = TargetElement;
		if (!IsDetached && targetElement != null)
		{
			FrameworkPropertyMetadata fwMetaData = TargetProperty.GetMetadata(targetElement.DependencyObjectType) as FrameworkPropertyMetadata;
			UpdateSourceTrigger defaultUpdateSourceTrigger = GetDefaultUpdateSourceTrigger(fwMetaData);
			SetUpdateSourceTrigger(defaultUpdateSourceTrigger);
			if (IsUpdateOnLostFocus)
			{
				LostFocusEventManager.AddHandler(targetElement, OnLostFocus);
			}
		}
		return null;
	}

	internal abstract void UpdateBindingGroup(BindingGroup bg);

	internal bool UpdateValue()
	{
		ValidationError baseValidationError = BaseValidationError;
		if (StatusInternal == BindingStatusInternal.UpdateSourceError)
		{
			SetStatus(BindingStatusInternal.Active);
		}
		object rawProposedValue = GetRawProposedValue();
		if (!Validate(rawProposedValue, ValidationStep.RawProposedValue))
		{
			return false;
		}
		rawProposedValue = ConvertProposedValue(rawProposedValue);
		if (!Validate(rawProposedValue, ValidationStep.ConvertedProposedValue))
		{
			return false;
		}
		rawProposedValue = UpdateSource(rawProposedValue);
		if (!Validate(rawProposedValue, ValidationStep.UpdatedValue))
		{
			return false;
		}
		rawProposedValue = CommitSource(rawProposedValue);
		if (!Validate(rawProposedValue, ValidationStep.CommittedValue))
		{
			return false;
		}
		if (BaseValidationError == baseValidationError)
		{
			UpdateValidationError(null);
		}
		EndSourceUpdate();
		NotifyCommitManager();
		return !HasValue(Feature.ValidationError);
	}

	internal virtual object GetRawProposedValue()
	{
		object obj = Value;
		if (ItemsControl.EqualsEx(obj, EffectiveTargetNullValue))
		{
			obj = null;
		}
		return obj;
	}

	internal abstract object ConvertProposedValue(object rawValue);

	internal abstract bool ObtainConvertedProposedValue(BindingGroup bindingGroup);

	internal abstract object UpdateSource(object convertedValue);

	internal abstract bool UpdateSource(BindingGroup bindingGroup);

	internal virtual object CommitSource(object value)
	{
		return value;
	}

	internal abstract void StoreValueInBindingGroup(object value, BindingGroup bindingGroup);

	internal virtual bool Validate(object value, ValidationStep validationStep)
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
		ValidationError validationError = GetValidationErrors(validationStep);
		if (validationError != null && validationError.RuleInError == DataErrorValidationRule.Instance)
		{
			validationError = null;
		}
		Collection<ValidationRule> validationRulesInternal = ParentBindingBase.ValidationRulesInternal;
		if (validationRulesInternal != null)
		{
			CultureInfo culture = GetCulture();
			foreach (ValidationRule item in validationRulesInternal)
			{
				if (item.ValidationStep != validationStep)
				{
					continue;
				}
				ValidationResult validationResult = item.Validate(value, culture, this);
				if (!validationResult.IsValid)
				{
					if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer))
					{
						TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.ValidationRuleFailed(TraceData.Identify(this), TraceData.Identify(item)), this);
					}
					UpdateValidationError(new ValidationError(item, this, validationResult.ErrorContent, null));
					return false;
				}
			}
		}
		if (validationError != null && validationError == GetValidationErrors(validationStep))
		{
			UpdateValidationError(null);
		}
		return true;
	}

	internal abstract bool CheckValidationRules(BindingGroup bindingGroup, ValidationStep validationStep);

	internal abstract bool ValidateAndConvertProposedValue(out Collection<ProposedValue> values);

	internal CultureInfo GetCulture()
	{
		if (_culture == DefaultValueObject)
		{
			_culture = ParentBindingBase.ConverterCultureInternal;
			if (_culture == null)
			{
				DependencyObject targetElement = TargetElement;
				if (targetElement != null)
				{
					if (IsInTransfer && TargetProperty == FrameworkElement.LanguageProperty)
					{
						if (TraceData.IsEnabled)
						{
							TraceData.TraceAndNotify(TraceEventType.Critical, TraceData.RequiresExplicitCulture, this, new object[2] { TargetProperty.Name, this });
						}
						throw new InvalidOperationException(SR.Format(SR.RequiresExplicitCulture, TargetProperty.Name));
					}
					_culture = ((XmlLanguage)targetElement.GetValue(FrameworkElement.LanguageProperty)).GetSpecificCulture();
				}
			}
		}
		return (CultureInfo)_culture;
	}

	internal void InvalidateCulture()
	{
		_culture = DefaultValueObject;
	}

	internal void BeginSourceUpdate()
	{
		ChangeFlag(PrivateFlags.iInUpdate, value: true);
	}

	internal void EndSourceUpdate()
	{
		if (IsInUpdate && IsDynamic && StatusInternal == BindingStatusInternal.Active)
		{
			UndoManager undoManager = ((!(Target is TextBoxBase textBoxBase)) ? null : textBoxBase.TextContainer.UndoManager);
			if (undoManager != null && undoManager.OpenedUnit != null && undoManager.OpenedUnit.GetType() != typeof(TextParentUndoUnit))
			{
				if (!HasValue(Feature.UpdateTargetOperation))
				{
					DispatcherOperation value = Dispatcher.BeginInvoke(DispatcherPriority.Send, new DispatcherOperationCallback(UpdateTargetCallback), null);
					SetValue(Feature.UpdateTargetOperation, value);
				}
			}
			else
			{
				UpdateTarget();
			}
		}
		ChangeFlag(PrivateFlags.iInUpdate | PrivateFlags.iNeedsUpdate, value: false);
	}

	private object UpdateTargetCallback(object unused)
	{
		ClearValue(Feature.UpdateTargetOperation);
		IsInUpdate = true;
		UpdateTarget();
		IsInUpdate = false;
		return null;
	}

	internal bool ShouldUpdateWithCurrentValue(DependencyObject target, out object currentValue)
	{
		if (IsReflective && !new FrameworkObject(target).IsInitialized)
		{
			DependencyProperty targetProperty = TargetProperty;
			EntryIndex entryIndex = target.LookupEntry(targetProperty.GlobalIndex);
			if (entryIndex.Found)
			{
				EffectiveValueEntry valueEntry = target.GetValueEntry(entryIndex, targetProperty, null, RequestFlags.RawEntry);
				if (valueEntry.IsCoercedWithCurrentValue)
				{
					currentValue = valueEntry.GetFlattenedEntry(RequestFlags.FullyResolved).Value;
					if (valueEntry.IsDeferredReference)
					{
						DeferredReference deferredReference = (DeferredReference)currentValue;
						currentValue = deferredReference.GetValue(valueEntry.BaseValueSourceInternal);
					}
					return true;
				}
			}
		}
		currentValue = null;
		return false;
	}

	internal void ChangeValue(object newValue, bool notify)
	{
		object oldValue = ((_value != DefaultValueObject) ? _value : DependencyProperty.UnsetValue);
		_value = newValue;
		if (notify && this.ValueChanged != null)
		{
			this.ValueChanged(this, new BindingValueChangedEventArgs(oldValue, newValue));
		}
	}

	internal void Clean()
	{
		if (NeedsUpdate)
		{
			NeedsUpdate = false;
		}
		if (!IsInUpdate)
		{
			NeedsValidation = false;
			NotifyCommitManager();
		}
	}

	internal void Dirty()
	{
		if (ShouldReactToDirty())
		{
			NeedsUpdate = true;
			if (!HasValue(Feature.Timer))
			{
				ProcessDirty();
			}
			else
			{
				DispatcherTimer obj = (DispatcherTimer)GetValue(Feature.Timer, null);
				obj.Stop();
				obj.Start();
			}
			NotifyCommitManager();
		}
	}

	private bool ShouldReactToDirty()
	{
		if (IsInTransfer || !IsAttached)
		{
			return false;
		}
		return ShouldReactToDirtyOverride();
	}

	internal virtual bool ShouldReactToDirtyOverride()
	{
		return true;
	}

	private void ProcessDirty()
	{
		if (IsUpdateOnPropertyChanged)
		{
			DispatcherOperation dispatcherOperation = (DispatcherOperation)GetValue(Feature.UpdateTargetOperation, null);
			if (dispatcherOperation != null)
			{
				ClearValue(Feature.UpdateTargetOperation);
				dispatcherOperation.Abort();
			}
			if (Helper.IsComposing(Target, TargetProperty))
			{
				IsUpdateDeferredForComposition = true;
			}
			else
			{
				Update();
			}
		}
	}

	private void OnTimerTick(object sender, EventArgs e)
	{
		ProcessDirty();
	}

	private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
	{
		if (IsUpdateDeferredForComposition && e.TextComposition.Source == TargetElement && e.TextComposition.Stage == TextCompositionStage.Done)
		{
			IsUpdateDeferredForComposition = false;
			Dirty();
		}
	}

	internal void Invalidate(bool isASubPropertyChange)
	{
		if (IsAttaching)
		{
			return;
		}
		DependencyObject targetElement = TargetElement;
		if (targetElement == null)
		{
			return;
		}
		if (IsInBindingExpressionCollection)
		{
			ParentBindingExpressionBase.InvalidateChild(this);
		}
		else if (TargetProperty != NoTargetProperty)
		{
			if (!isASubPropertyChange)
			{
				targetElement.InvalidateProperty(TargetProperty);
			}
			else
			{
				targetElement.NotifySubPropertyChange(TargetProperty);
			}
		}
	}

	internal object UseFallbackValue()
	{
		object obj = FallbackValue;
		if (obj == DefaultValueObject)
		{
			obj = DependencyProperty.UnsetValue;
		}
		if (obj == DependencyProperty.UnsetValue && IsOneWayToSource)
		{
			obj = DefaultValue;
		}
		if (obj != DependencyProperty.UnsetValue)
		{
			UsingFallbackValue = true;
		}
		else
		{
			if (StatusInternal == BindingStatusInternal.Active)
			{
				SetStatus(BindingStatusInternal.UpdateTargetError);
			}
			if (!IsInBindingExpressionCollection)
			{
				obj = DefaultValue;
				if (TraceData.IsEnabled)
				{
					TraceData.TraceAndNotify(TraceEventType.Information, TraceData.NoValueToTransfer, this);
				}
			}
		}
		return obj;
	}

	internal static bool IsNullValue(object value)
	{
		if (value == null)
		{
			return true;
		}
		if (Convert.IsDBNull(value))
		{
			return true;
		}
		if (SystemDataHelper.IsSqlNull(value))
		{
			return true;
		}
		return false;
	}

	internal object NullValueForType(Type type)
	{
		if (type == null)
		{
			return null;
		}
		if (SystemDataHelper.IsSqlNullableType(type))
		{
			return SystemDataHelper.NullValueForSqlNullableType(type);
		}
		if (!type.IsValueType)
		{
			return null;
		}
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			return null;
		}
		return DependencyProperty.UnsetValue;
	}

	internal ValidationRule LookupValidationRule(Type type)
	{
		ValidationRule validationRule = ParentBindingBase.GetValidationRule(type);
		if (validationRule == null && HasValue(Feature.ParentBindingExpressionBase))
		{
			validationRule = ParentBindingExpressionBase.LookupValidationRule(type);
		}
		return validationRule;
	}

	internal void JoinBindingGroup(bool isReflective, DependencyObject contextElement)
	{
		BindingGroup bindingGroup = RootBindingExpression.FindBindingGroup(isReflective, contextElement);
		if (bindingGroup != null)
		{
			JoinBindingGroup(bindingGroup, explicitJoin: false);
		}
	}

	internal void LeaveBindingGroup()
	{
		BindingExpressionBase rootBindingExpression = RootBindingExpression;
		BindingGroup bindingGroup = rootBindingExpression.BindingGroup;
		if (bindingGroup != null)
		{
			bindingGroup.BindingExpressions.Remove(rootBindingExpression);
			rootBindingExpression.ClearValue(Feature.BindingGroup);
		}
	}

	internal void RejoinBindingGroup(bool isReflective, DependencyObject contextElement)
	{
		BindingExpressionBase rootBindingExpression = RootBindingExpression;
		BindingGroup bindingGroup = rootBindingExpression.BindingGroup;
		WeakReference<BindingGroup> weakReference = (WeakReference<BindingGroup>)rootBindingExpression.GetValue(Feature.BindingGroup, null);
		rootBindingExpression.SetValue(Feature.BindingGroup, null, weakReference);
		BindingGroup bindingGroup2;
		try
		{
			bindingGroup2 = rootBindingExpression.FindBindingGroup(isReflective, contextElement);
		}
		finally
		{
			rootBindingExpression.SetValue(Feature.BindingGroup, weakReference, null);
		}
		if (bindingGroup != bindingGroup2)
		{
			rootBindingExpression.LeaveBindingGroup();
			if (bindingGroup2 != null)
			{
				JoinBindingGroup(bindingGroup2, explicitJoin: false);
			}
		}
	}

	internal BindingGroup FindBindingGroup(bool isReflective, DependencyObject contextElement)
	{
		if ((WeakReference<BindingGroup>)GetValue(Feature.BindingGroup, null) != null)
		{
			return BindingGroup;
		}
		string bindingGroupName = ParentBindingBase.BindingGroupName;
		if (bindingGroupName == null)
		{
			MarkAsNonGrouped();
			return null;
		}
		if (string.IsNullOrEmpty(bindingGroupName))
		{
			if (!isReflective || contextElement == null)
			{
				return null;
			}
			BindingGroup bindingGroup = (BindingGroup)contextElement.GetValue(FrameworkElement.BindingGroupProperty);
			if (bindingGroup == null)
			{
				MarkAsNonGrouped();
				return null;
			}
			DependencyProperty dataContextProperty = FrameworkElement.DataContextProperty;
			DependencyObject inheritanceContext = bindingGroup.InheritanceContext;
			if (inheritanceContext == null || !ItemsControl.EqualsEx(contextElement.GetValue(dataContextProperty), inheritanceContext.GetValue(dataContextProperty)))
			{
				MarkAsNonGrouped();
				return null;
			}
			return bindingGroup;
		}
		DependencyProperty bindingGroupProperty = FrameworkElement.BindingGroupProperty;
		FrameworkObject frameworkObject = new FrameworkObject(Helper.FindMentor(TargetElement));
		while (frameworkObject.DO != null)
		{
			BindingGroup bindingGroup2 = (BindingGroup)frameworkObject.DO.GetValue(bindingGroupProperty);
			if (bindingGroup2 == null)
			{
				MarkAsNonGrouped();
				return null;
			}
			if (bindingGroup2.Name == bindingGroupName)
			{
				if (bindingGroup2.SharesProposedValues && TraceData.IsEnabled)
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.SharesProposedValuesRequriesImplicitBindingGroup(TraceData.Identify(this), bindingGroupName, TraceData.Identify(bindingGroup2)), this);
				}
				return bindingGroup2;
			}
			frameworkObject = frameworkObject.FrameworkParent;
		}
		if (TraceData.IsEnabled)
		{
			TraceData.TraceAndNotify(TraceEventType.Error, TraceData.BindingGroupNameMatchFailed(bindingGroupName), this);
		}
		MarkAsNonGrouped();
		return null;
	}

	internal void JoinBindingGroup(BindingGroup bg, bool explicitJoin)
	{
		BindingExpressionBase bindingExpressionBase = null;
		for (BindingExpressionBase bindingExpressionBase2 = this; bindingExpressionBase2 != null; bindingExpressionBase2 = bindingExpressionBase2.ParentBindingExpressionBase)
		{
			bindingExpressionBase = bindingExpressionBase2;
			bindingExpressionBase2.OnBindingGroupChanged(joining: true);
			bg.AddToValueTable(bindingExpressionBase2);
		}
		if (!bindingExpressionBase.HasValue(Feature.BindingGroup))
		{
			bindingExpressionBase.SetValue(Feature.BindingGroup, new WeakReference<BindingGroup>(bg));
			if (!explicitJoin || !bg.BindingExpressions.Contains(bindingExpressionBase))
			{
				bg.BindingExpressions.Add(bindingExpressionBase);
			}
			if (explicitJoin)
			{
				bindingExpressionBase.UpdateBindingGroup(bg);
				if (bg.SharesProposedValues && TraceData.IsEnabled)
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.SharesProposedValuesRequriesImplicitBindingGroup(TraceData.Identify(bindingExpressionBase), bindingExpressionBase.ParentBindingBase.BindingGroupName, TraceData.Identify(bg)), this);
				}
			}
		}
		else if (bindingExpressionBase.BindingGroup != bg)
		{
			throw new InvalidOperationException(SR.BindingGroup_CannotChangeGroups);
		}
	}

	private void MarkAsNonGrouped()
	{
		if (!(this is BindingExpression))
		{
			SetValue(Feature.BindingGroup, NullBindingGroupReference);
		}
	}

	internal void NotifyCommitManager()
	{
		if (!IsReflective || IsDetached || Engine.IsShutDown)
		{
			return;
		}
		bool flag = IsEligibleForCommit && (IsDirty || HasValidationError);
		BindingExpressionBase rootBindingExpression = RootBindingExpression;
		BindingGroup bindingGroup = rootBindingExpression.BindingGroup;
		rootBindingExpression.UpdateCommitState();
		if (bindingGroup == null)
		{
			if (rootBindingExpression != this && !flag)
			{
				flag = rootBindingExpression.IsEligibleForCommit && (rootBindingExpression.IsDirty || rootBindingExpression.HasValidationError);
			}
			if (flag)
			{
				Engine.CommitManager.AddBinding(rootBindingExpression);
			}
			else
			{
				Engine.CommitManager.RemoveBinding(rootBindingExpression);
			}
		}
		else
		{
			if (!flag)
			{
				flag = bindingGroup.Owner != null && (bindingGroup.IsDirty || bindingGroup.HasValidationError);
			}
			if (flag)
			{
				Engine.CommitManager.AddBindingGroup(bindingGroup);
			}
			else
			{
				Engine.CommitManager.RemoveBindingGroup(bindingGroup);
			}
		}
	}

	internal virtual void UpdateCommitState()
	{
	}

	internal void AdoptProperties(BindingExpressionBase bb)
	{
		PrivateFlags privateFlags = bb?._flags ?? (~(PrivateFlags.iAdoptionMask | PrivateFlags.iUpdateDefault | PrivateFlags.iPropDefault | PrivateFlags.iNotifyOnTargetUpdated | PrivateFlags.iDefaultValueConverter | PrivateFlags.iInTransfer | PrivateFlags.iInUpdate | PrivateFlags.iTransferPending | PrivateFlags.iNeedDataTransfer | PrivateFlags.iTransferDeferred | PrivateFlags.iPathGeneratedInternally | PrivateFlags.iUsingMentor | PrivateFlags.iResolveNamesInTemplate | PrivateFlags.iDetaching | PrivateFlags.iNeedsCollectionView | PrivateFlags.iInPriorityBindingExpression | PrivateFlags.iInMultiBindingExpression | PrivateFlags.iUsingFallbackValue | PrivateFlags.iNotifyOnValidationError | PrivateFlags.iAttaching | PrivateFlags.iNotifyOnSourceUpdated | PrivateFlags.iValidatesOnExceptions | PrivateFlags.iValidatesOnDataErrors | PrivateFlags.iIllegalInput | PrivateFlags.iTargetWantsXTNotification | PrivateFlags.iValidatesOnNotifyDataErrors | PrivateFlags.iDataErrorsChangedPending | PrivateFlags.iDeferUpdateForComposition));
		_flags = (PrivateFlags)(((uint)_flags & 0xF7FFEFFCu) | (uint)(privateFlags & PrivateFlags.iAdoptionMask));
	}

	internal virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return false;
	}

	internal virtual void OnLostFocus(object sender, RoutedEventArgs e)
	{
	}

	internal abstract object GetSourceItem(object newValue);

	private bool TestFlag(PrivateFlags flag)
	{
		return (_flags & flag) != 0;
	}

	private void ChangeFlag(PrivateFlags flag, bool value)
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

	internal void Attach(DependencyObject target, DependencyProperty dp)
	{
		target?.VerifyAccess();
		IsAttaching = true;
		AttachOverride(target, dp);
		IsAttaching = false;
	}

	internal void Detach()
	{
		if (!IsDetached && !IsDetaching)
		{
			IsDetaching = true;
			LeaveBindingGroup();
			NotifyCommitManager();
			DetachOverride();
			IsDetaching = false;
		}
	}

	internal virtual void Disconnect()
	{
		object obj = DependencyProperty.UnsetValue;
		DependencyProperty targetProperty = TargetProperty;
		if (targetProperty == ContentControl.ContentProperty || targetProperty == ContentPresenter.ContentProperty || targetProperty == HeaderedItemsControl.HeaderProperty || targetProperty == HeaderedContentControl.HeaderProperty)
		{
			obj = DisconnectedItem;
		}
		if (targetProperty.PropertyType == typeof(IEnumerable))
		{
			obj = null;
		}
		if (obj != DependencyProperty.UnsetValue)
		{
			ChangeValue(obj, notify: false);
			Invalidate(isASubPropertyChange: false);
		}
	}

	internal void SetStatus(BindingStatusInternal status)
	{
		if (IsDetached && status != _status)
		{
			throw new InvalidOperationException(SR.Format(SR.BindingExpressionStatusChanged, _status, status));
		}
		_status = status;
	}

	internal static object ConvertFallbackValue(object value, DependencyProperty dp, object sender)
	{
		Exception e;
		object obj = ConvertValue(value, dp, out e);
		if (obj == DefaultValueObject && TraceData.IsEnabled)
		{
			TraceData.TraceAndNotify(TraceEventType.Error, TraceData.FallbackConversionFailed(AvTrace.ToStringHelper(value), AvTrace.TypeName(value), dp.Name, dp.PropertyType.Name), sender as BindingExpressionBase, e);
		}
		return obj;
	}

	internal static object ConvertTargetNullValue(object value, DependencyProperty dp, object sender)
	{
		Exception e;
		object obj = ConvertValue(value, dp, out e);
		if (obj == DefaultValueObject && TraceData.IsEnabled)
		{
			TraceData.TraceAndNotify(TraceEventType.Error, TraceData.TargetNullValueConversionFailed(AvTrace.ToStringHelper(value), AvTrace.TypeName(value), dp.Name, dp.PropertyType.Name), sender as BindingExpressionBase, e);
		}
		return obj;
	}

	private static object ConvertValue(object value, DependencyProperty dp, out Exception e)
	{
		e = null;
		object obj;
		if (value == DependencyProperty.UnsetValue || dp.IsValidValue(value))
		{
			obj = value;
		}
		else
		{
			obj = null;
			bool flag = false;
			TypeConverter converter = DefaultValueConverter.GetConverter(dp.PropertyType);
			if (converter != null && converter.CanConvertFrom(value.GetType()))
			{
				try
				{
					obj = converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
					flag = dp.IsValidValue(obj);
				}
				catch (Exception ex)
				{
					e = ex;
				}
				catch
				{
				}
			}
			if (!flag)
			{
				obj = DefaultValueObject;
			}
		}
		return obj;
	}

	internal virtual void Activate()
	{
	}

	internal virtual void Deactivate()
	{
	}

	internal bool Update()
	{
		if (HasValue(Feature.Timer))
		{
			((DispatcherTimer)GetValue(Feature.Timer, null)).Stop();
		}
		return UpdateOverride();
	}

	internal virtual bool UpdateOverride()
	{
		return true;
	}

	internal void UpdateValidationError(ValidationError validationError, bool skipBindingGroup = false)
	{
		ValidationError baseValidationError = BaseValidationError;
		SetValue(Feature.ValidationError, validationError, null);
		if (validationError != null)
		{
			AddValidationError(validationError, skipBindingGroup);
		}
		if (baseValidationError != null)
		{
			RemoveValidationError(baseValidationError, skipBindingGroup);
		}
	}

	internal void UpdateNotifyDataErrorValidationErrors(List<object> errors)
	{
		GetValidationDelta(NotifyDataErrors, errors, out var toAdd, out var toRemove);
		if (toAdd != null && toAdd.Count > 0)
		{
			ValidationRule instance = NotifyDataErrorValidationRule.Instance;
			List<ValidationError> list = NotifyDataErrors;
			if (list == null)
			{
				list = new List<ValidationError>();
				SetValue(Feature.NotifyDataErrors, list);
			}
			foreach (object item in toAdd)
			{
				ValidationError validationError = new ValidationError(instance, this, item, null);
				list.Add(validationError);
				AddValidationError(validationError);
			}
		}
		if (toRemove == null || toRemove.Count <= 0)
		{
			return;
		}
		List<ValidationError> notifyDataErrors = NotifyDataErrors;
		foreach (ValidationError item2 in toRemove)
		{
			notifyDataErrors.Remove(item2);
			RemoveValidationError(item2);
		}
		if (notifyDataErrors.Count == 0)
		{
			ClearValue(Feature.NotifyDataErrors);
		}
	}

	internal static void GetValidationDelta(List<ValidationError> previousErrors, List<object> errors, out List<object> toAdd, out List<ValidationError> toRemove)
	{
		if (previousErrors == null || previousErrors.Count == 0)
		{
			toAdd = errors;
			toRemove = null;
			return;
		}
		if (errors == null || errors.Count == 0)
		{
			toAdd = null;
			toRemove = new List<ValidationError>(previousErrors);
			return;
		}
		toAdd = new List<object>();
		toRemove = new List<ValidationError>(previousErrors);
		for (int num = errors.Count - 1; num >= 0; num--)
		{
			object obj = errors[num];
			int num2;
			for (num2 = toRemove.Count - 1; num2 >= 0; num2--)
			{
				if (ItemsControl.EqualsEx(toRemove[num2].ErrorContent, obj))
				{
					toRemove.RemoveAt(num2);
					break;
				}
			}
			if (num2 < 0)
			{
				toAdd.Add(obj);
			}
		}
	}

	internal void AddValidationError(ValidationError validationError, bool skipBindingGroup = false)
	{
		Validation.AddValidationError(validationError, TargetElement, NotifyOnValidationError);
		if (!skipBindingGroup)
		{
			BindingGroup?.AddValidationError(validationError);
		}
	}

	internal void RemoveValidationError(ValidationError validationError, bool skipBindingGroup = false)
	{
		Validation.RemoveValidationError(validationError, TargetElement, NotifyOnValidationError);
		if (!skipBindingGroup)
		{
			BindingGroup?.RemoveValidationError(validationError);
		}
	}

	internal ValidationError GetValidationErrors(ValidationStep validationStep)
	{
		ValidationError baseValidationError = BaseValidationError;
		if (baseValidationError == null || baseValidationError.RuleInError.ValidationStep != validationStep)
		{
			return null;
		}
		return baseValidationError;
	}

	internal void ChangeSources(WeakDependencySource[] newSources)
	{
		if (IsInBindingExpressionCollection)
		{
			ParentBindingExpressionBase.ChangeSourcesForChild(this, newSources);
		}
		else
		{
			ChangeSources(TargetElement, TargetProperty, newSources);
		}
		_sources = newSources;
	}

	internal static WeakDependencySource[] CombineSources(int index, Collection<BindingExpressionBase> bindingExpressions, int count, WeakDependencySource[] newSources, WeakDependencySource[] commonSources = null)
	{
		if (index == count)
		{
			count++;
		}
		List<WeakDependencySource> list = new List<WeakDependencySource>();
		if (commonSources != null)
		{
			for (int i = 0; i < commonSources.Length; i++)
			{
				list.Add(commonSources[i]);
			}
		}
		for (int j = 0; j < count; j++)
		{
			BindingExpressionBase bindingExpressionBase = bindingExpressions[j];
			WeakDependencySource[] array = ((j == index) ? newSources : bindingExpressionBase?.WeakSources);
			int num = ((array != null) ? array.Length : 0);
			for (int k = 0; k < num; k++)
			{
				WeakDependencySource weakDependencySource = array[k];
				for (int l = 0; l < list.Count; l++)
				{
					WeakDependencySource weakDependencySource2 = list[l];
					if (weakDependencySource.DependencyObject == weakDependencySource2.DependencyObject && weakDependencySource.DependencyProperty == weakDependencySource2.DependencyProperty)
					{
						weakDependencySource = null;
						break;
					}
				}
				if (weakDependencySource != null)
				{
					list.Add(weakDependencySource);
				}
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list.ToArray();
	}

	internal void ResolvePropertyDefaultSettings(BindingMode mode, UpdateSourceTrigger updateTrigger, FrameworkPropertyMetadata fwMetaData)
	{
		if (mode == BindingMode.Default)
		{
			BindingFlags bindingFlags = BindingFlags.OneWay;
			if (fwMetaData != null && fwMetaData.BindsTwoWayByDefault)
			{
				bindingFlags = BindingFlags.TwoWay;
			}
			ChangeFlag(PrivateFlags.iPropagationMask, value: false);
			ChangeFlag((PrivateFlags)bindingFlags, value: true);
			if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.CreateExpression))
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.ResolveDefaultMode(TraceData.Identify(this), (bindingFlags == BindingFlags.OneWay) ? BindingMode.OneWay : BindingMode.TwoWay), this);
			}
		}
		if (updateTrigger == UpdateSourceTrigger.Default)
		{
			UpdateSourceTrigger defaultUpdateSourceTrigger = GetDefaultUpdateSourceTrigger(fwMetaData);
			SetUpdateSourceTrigger(defaultUpdateSourceTrigger);
			if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.CreateExpression))
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.ResolveDefaultUpdate(TraceData.Identify(this), defaultUpdateSourceTrigger), this);
			}
		}
		Invariant.Assert((_flags & PrivateFlags.iUpdateDefault) != PrivateFlags.iUpdateDefault, "BindingExpression should not have Default update trigger");
	}

	internal UpdateSourceTrigger GetDefaultUpdateSourceTrigger(FrameworkPropertyMetadata fwMetaData)
	{
		if (!IsInMultiBindingExpression)
		{
			return fwMetaData?.DefaultUpdateSourceTrigger ?? UpdateSourceTrigger.PropertyChanged;
		}
		return UpdateSourceTrigger.Explicit;
	}

	internal void SetUpdateSourceTrigger(UpdateSourceTrigger ust)
	{
		ChangeFlag(PrivateFlags.iUpdateDefault, value: false);
		ChangeFlag((PrivateFlags)BindingBase.FlagsFrom(ust), value: true);
	}

	internal Type GetEffectiveTargetType()
	{
		Type result = TargetProperty.PropertyType;
		for (BindingExpressionBase parentBindingExpressionBase = ParentBindingExpressionBase; parentBindingExpressionBase != null; parentBindingExpressionBase = parentBindingExpressionBase.ParentBindingExpressionBase)
		{
			if (parentBindingExpressionBase is MultiBindingExpression)
			{
				result = typeof(object);
				break;
			}
		}
		return result;
	}

	internal void DetermineEffectiveStringFormat()
	{
		Type type = TargetProperty.PropertyType;
		if (type != typeof(string))
		{
			return;
		}
		string stringFormat = ParentBindingBase.StringFormat;
		for (BindingExpressionBase parentBindingExpressionBase = ParentBindingExpressionBase; parentBindingExpressionBase != null; parentBindingExpressionBase = parentBindingExpressionBase.ParentBindingExpressionBase)
		{
			if (parentBindingExpressionBase is MultiBindingExpression)
			{
				type = typeof(object);
				break;
			}
			if (stringFormat == null && parentBindingExpressionBase is PriorityBindingExpression)
			{
				stringFormat = parentBindingExpressionBase.ParentBindingBase.StringFormat;
			}
		}
		if (type == typeof(string) && !string.IsNullOrEmpty(stringFormat))
		{
			SetValue(Feature.EffectiveStringFormat, Helper.GetEffectiveStringFormat(stringFormat), null);
		}
	}

	internal void DetermineEffectiveTargetNullValue()
	{
		_ = TargetProperty.PropertyType;
		object obj = ParentBindingBase.TargetNullValue;
		BindingExpressionBase parentBindingExpressionBase = ParentBindingExpressionBase;
		while (parentBindingExpressionBase != null && !(parentBindingExpressionBase is MultiBindingExpression))
		{
			if (obj == DependencyProperty.UnsetValue && parentBindingExpressionBase is PriorityBindingExpression)
			{
				obj = parentBindingExpressionBase.ParentBindingBase.TargetNullValue;
			}
			parentBindingExpressionBase = parentBindingExpressionBase.ParentBindingExpressionBase;
		}
		if (obj != DependencyProperty.UnsetValue)
		{
			obj = ConvertTargetNullValue(obj, TargetProperty, this);
			if (obj == DefaultValueObject)
			{
				obj = DependencyProperty.UnsetValue;
			}
		}
		SetValue(Feature.EffectiveTargetNullValue, obj, DependencyProperty.UnsetValue);
	}

	private void DetermineEffectiveUpdateBehavior()
	{
		if (!IsReflective)
		{
			return;
		}
		for (BindingExpressionBase parentBindingExpressionBase = ParentBindingExpressionBase; parentBindingExpressionBase != null; parentBindingExpressionBase = parentBindingExpressionBase.ParentBindingExpressionBase)
		{
			if (parentBindingExpressionBase is MultiBindingExpression)
			{
				return;
			}
		}
		int delay = ParentBindingBase.Delay;
		if (delay > 0 && IsUpdateOnPropertyChanged)
		{
			DispatcherTimer dispatcherTimer = new DispatcherTimer();
			SetValue(Feature.Timer, dispatcherTimer);
			dispatcherTimer.Interval = TimeSpan.FromMilliseconds(delay);
			dispatcherTimer.Tick += OnTimerTick;
		}
	}

	internal void DetermineEffectiveValidatesOnNotifyDataErrors()
	{
		bool flag = ParentBindingBase.ValidatesOnNotifyDataErrorsInternal;
		BindingExpressionBase parentBindingExpressionBase = ParentBindingExpressionBase;
		while (flag && parentBindingExpressionBase != null)
		{
			flag = parentBindingExpressionBase.ValidatesOnNotifyDataErrors;
			parentBindingExpressionBase = parentBindingExpressionBase.ParentBindingExpressionBase;
		}
		ChangeFlag(PrivateFlags.iValidatesOnNotifyDataErrors, flag);
	}

	internal static object CreateReference(object item)
	{
		if (item != null && !(item is BindingListCollectionView) && item != BindingExpression.NullDataItem && item != DisconnectedItem)
		{
			item = new WeakReference(item);
		}
		return item;
	}

	internal static object CreateReference(WeakReference item)
	{
		return item;
	}

	internal static object ReplaceReference(object oldReference, object item)
	{
		if (item != null && !(item is BindingListCollectionView) && item != BindingExpression.NullDataItem && item != DisconnectedItem)
		{
			if (oldReference is WeakReference weakReference)
			{
				weakReference.Target = item;
				item = weakReference;
			}
			else
			{
				item = new WeakReference(item);
			}
		}
		return item;
	}

	internal static object GetReference(object reference)
	{
		if (reference == null)
		{
			return null;
		}
		if (reference is WeakReference weakReference)
		{
			return weakReference.Target;
		}
		return reference;
	}

	internal static void InitializeTracing(BindingExpressionBase expr, DependencyObject d, DependencyProperty dp)
	{
		_ = expr.ParentBindingBase;
	}

	private void ChangeSources(DependencyObject target, DependencyProperty dp, WeakDependencySource[] newSources)
	{
		DependencySource[] array;
		if (newSources != null)
		{
			array = new DependencySource[newSources.Length];
			int num = 0;
			for (int i = 0; i < newSources.Length; i++)
			{
				DependencyObject dependencyObject = newSources[i].DependencyObject;
				if (dependencyObject != null)
				{
					array[num++] = new DependencySource(dependencyObject, newSources[i].DependencyProperty);
				}
			}
			if (num < newSources.Length)
			{
				DependencySource[] array2;
				if (num > 0)
				{
					array2 = new DependencySource[num];
					Array.Copy(array, 0, array2, 0, num);
				}
				else
				{
					array2 = null;
				}
				array = array2;
			}
		}
		else
		{
			array = null;
		}
		ChangeSources(target, dp, array);
	}

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
}
