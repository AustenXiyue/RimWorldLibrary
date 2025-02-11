using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.Data;

namespace System.Windows.Data;

/// <summary>Contains information about a single instance of a <see cref="T:System.Windows.Data.Binding" />. </summary>
public sealed class BindingExpression : BindingExpressionBase, IDataBindEngineClient, IWeakEventListener
{
	internal enum SourceType
	{
		Unknown,
		CLR,
		XML
	}

	private enum AttachAttempt
	{
		First,
		Again,
		Last
	}

	private class ChangingValueHelper : IDisposable
	{
		private BindingExpression _bindingExpression;

		internal ChangingValueHelper(BindingExpression b)
		{
			_bindingExpression = b;
			b.CancelPendingTasks();
		}

		public void Dispose()
		{
			_bindingExpression.TransferValue();
			GC.SuppressFinalize(this);
		}
	}

	private WeakReference _ctxElement;

	private object _dataItem;

	private BindingWorker _worker;

	private Type _sourceType;

	internal static readonly object NullDataItem = new NamedObject("NullDataItem");

	internal static readonly object IgnoreDefaultValue = new NamedObject("IgnoreDefaultValue");

	internal static readonly object StaticSource = new NamedObject("StaticSource");

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

	/// <summary>Returns the <see cref="T:System.Windows.Data.Binding" /> object of the current <see cref="T:System.Windows.Data.BindingExpression" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.Binding" /> object of the current binding expression.</returns>
	public Binding ParentBinding => (Binding)base.ParentBindingBase;

	/// <summary>Gets the binding source object that this <see cref="T:System.Windows.Data.BindingExpression" /> uses.</summary>
	/// <returns>The binding source object that this <see cref="T:System.Windows.Data.BindingExpression" /> uses.</returns>
	public object DataItem => BindingExpressionBase.GetReference(_dataItem);

	/// <summary>Gets the binding source object for this <see cref="T:System.Windows.Data.BindingExpression" />.</summary>
	/// <returns>The binding source object for this <see cref="T:System.Windows.Data.BindingExpression" />.</returns>
	public object ResolvedSource => SourceItem;

	/// <summary>Gets the name of the binding source property for this <see cref="T:System.Windows.Data.BindingExpression" />. </summary>
	/// <returns>The name of the binding source property for this <see cref="T:System.Windows.Data.BindingExpression" />.</returns>
	public string ResolvedSourcePropertyName => SourcePropertyName;

	internal object DataSource
	{
		get
		{
			DependencyObject targetElement = base.TargetElement;
			if (targetElement == null)
			{
				return null;
			}
			if (_ctxElement != null)
			{
				return GetDataSourceForDataContext(ContextElement);
			}
			return ParentBinding.SourceReference.GetObject(targetElement, new ObjectRefArgs());
		}
	}

	internal DependencyObject ContextElement
	{
		get
		{
			if (_ctxElement != null)
			{
				return _ctxElement.Target as DependencyObject;
			}
			return null;
		}
	}

	internal CollectionViewSource CollectionViewSource
	{
		get
		{
			WeakReference weakReference = (WeakReference)GetValue(Feature.CollectionViewSource, null);
			if (weakReference != null)
			{
				return (CollectionViewSource)weakReference.Target;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				ClearValue(Feature.CollectionViewSource);
			}
			else
			{
				SetValue(Feature.CollectionViewSource, new WeakReference(value));
			}
		}
	}

	internal bool IgnoreSourcePropertyChange
	{
		get
		{
			if (base.IsTransferPending || base.IsInUpdate)
			{
				return true;
			}
			return false;
		}
	}

	internal PropertyPath Path => ParentBinding.Path;

	internal IValueConverter Converter
	{
		get
		{
			return (IValueConverter)GetValue(Feature.Converter, null);
		}
		set
		{
			SetValue(Feature.Converter, value, null);
		}
	}

	internal Type ConverterSourceType => _sourceType;

	internal object SourceItem
	{
		get
		{
			if (Worker == null)
			{
				return null;
			}
			return Worker.SourceItem;
		}
	}

	internal string SourcePropertyName
	{
		get
		{
			if (Worker == null)
			{
				return null;
			}
			return Worker.SourcePropertyName;
		}
	}

	internal object SourceValue
	{
		get
		{
			if (Worker == null)
			{
				return DependencyProperty.UnsetValue;
			}
			return Worker.RawValue();
		}
	}

	internal override bool IsParentBindingUpdateTriggerDefault => ParentBinding.UpdateSourceTrigger == UpdateSourceTrigger.Default;

	internal override bool IsDisconnected => BindingExpressionBase.GetReference(_dataItem) == BindingExpressionBase.DisconnectedItem;

	private bool CanActivate => base.StatusInternal != BindingStatusInternal.Unattached;

	private BindingWorker Worker => _worker;

	private DynamicValueConverter DynamicConverter
	{
		get
		{
			if (!HasValue(Feature.DynamicConverter))
			{
				Invariant.Assert(Worker != null);
				SetValue(Feature.DynamicConverter, new DynamicValueConverter(base.IsReflective, Worker.SourcePropertyType, Worker.TargetPropertyType), null);
			}
			return (DynamicValueConverter)GetValue(Feature.DynamicConverter, null);
		}
	}

	private DataSourceProvider DataProvider
	{
		get
		{
			return (DataSourceProvider)GetValue(Feature.DataProvider, null);
		}
		set
		{
			SetValue(Feature.DataProvider, value, null);
		}
	}

	private BindingExpression(Binding binding, BindingExpressionBase owner)
		: base(binding, owner)
	{
		base.UseDefaultValueConverter = ParentBinding.Converter == null;
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.CreateExpression))
		{
			PropertyPath path = binding.Path;
			string o = ((path != null) ? path.Path : string.Empty);
			if (string.IsNullOrEmpty(binding.XPath))
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.BindingPath(TraceData.Identify(o)), this);
			}
			else
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.BindingXPathAndPath(TraceData.Identify(binding.XPath), TraceData.Identify(o)), this);
			}
		}
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
		AttachToContext((!lastChance) ? AttachAttempt.Again : AttachAttempt.Last);
		return base.StatusInternal != BindingStatusInternal.Unattached;
	}

	void IDataBindEngineClient.VerifySourceReference(bool lastChance)
	{
		DependencyObject targetElement = base.TargetElement;
		if (targetElement != null)
		{
			ObjectRef sourceReference = ParentBinding.SourceReference;
			DependencyObject d = ((!base.UsingMentor) ? targetElement : Helper.FindMentor(targetElement));
			ObjectRefArgs args = new ObjectRefArgs
			{
				ResolveNamesInTemplate = base.ResolveNamesInTemplate
			};
			if (sourceReference.GetDataObject(d, args) != DataItem)
			{
				AttachToContext((!lastChance) ? AttachAttempt.Again : AttachAttempt.Last);
			}
		}
	}

	void IDataBindEngineClient.OnTargetUpdated()
	{
		OnTargetUpdated();
	}

	/// <summary>Sends the current binding target value to the binding source property in <see cref="F:System.Windows.Data.BindingMode.TwoWay" /> or <see cref="F:System.Windows.Data.BindingMode.OneWayToSource" /> bindings.</summary>
	/// <exception cref="T:System.InvalidOperationException">The binding has been detached from its target.</exception>
	public override void UpdateSource()
	{
		if (base.IsDetached)
		{
			throw new InvalidOperationException(SR.BindingExpressionIsDetached);
		}
		base.NeedsUpdate = true;
		Update();
	}

	/// <summary>Forces a data transfer from the binding source property to the binding target property.</summary>
	/// <exception cref="T:System.InvalidOperationException">The binding has been detached from its target.</exception>
	public override void UpdateTarget()
	{
		if (base.IsDetached)
		{
			throw new InvalidOperationException(SR.BindingExpressionIsDetached);
		}
		if (Worker != null)
		{
			Worker.RefreshValue();
		}
	}

	internal override void OnPropertyInvalidation(DependencyObject d, DependencyPropertyChangedEventArgs args)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		DependencyProperty property = args.Property;
		if (property == null)
		{
			throw new InvalidOperationException(SR.Format(SR.ArgumentPropertyMustNotBeNull, "Property", "args"));
		}
		bool flag = !IgnoreSourcePropertyChange;
		if (property == FrameworkElement.DataContextProperty && d == ContextElement)
		{
			flag = true;
		}
		else if (property == CollectionViewSource.ViewProperty && d == CollectionViewSource)
		{
			flag = true;
		}
		else if (property == FrameworkElement.LanguageProperty && base.UsesLanguage && d == base.TargetElement)
		{
			flag = true;
		}
		else if (flag)
		{
			flag = Worker != null && Worker.UsesDependencyProperty(d, property);
		}
		if (flag)
		{
			base.OnPropertyInvalidation(d, args);
		}
	}

	internal override void InvalidateChild(BindingExpressionBase bindingExpression)
	{
	}

	internal override void ChangeSourcesForChild(BindingExpressionBase bindingExpression, WeakDependencySource[] newSources)
	{
	}

	internal override void ReplaceChild(BindingExpressionBase bindingExpression)
	{
	}

	internal override void UpdateBindingGroup(BindingGroup bg)
	{
		bg.UpdateTable(this);
	}

	internal static BindingExpression CreateBindingExpression(DependencyObject d, DependencyProperty dp, Binding binding, BindingExpressionBase parent)
	{
		FrameworkPropertyMetadata frameworkPropertyMetadata = dp.GetMetadata(d.DependencyObjectType) as FrameworkPropertyMetadata;
		if ((frameworkPropertyMetadata != null && !frameworkPropertyMetadata.IsDataBindingAllowed) || dp.ReadOnly)
		{
			throw new ArgumentException(SR.Format(SR.PropertyNotBindable, dp.Name), "dp");
		}
		BindingExpression bindingExpression = new BindingExpression(binding, parent);
		bindingExpression.ResolvePropertyDefaultSettings(binding.Mode, binding.UpdateSourceTrigger, frameworkPropertyMetadata);
		if (bindingExpression.IsReflective && binding.XPath == null && (binding.Path == null || string.IsNullOrEmpty(binding.Path.Path)))
		{
			throw new InvalidOperationException(SR.TwoWayBindingNeedsPath);
		}
		return bindingExpression;
	}

	internal void SetupDefaultValueConverter(Type type)
	{
		if (!base.UseDefaultValueConverter)
		{
			return;
		}
		if (base.IsInMultiBindingExpression)
		{
			Converter = null;
			_sourceType = type;
		}
		else if (type != null && type != _sourceType)
		{
			_sourceType = type;
			IValueConverter valueConverter = base.Engine.GetDefaultValueConverter(type, base.TargetProperty.PropertyType, base.IsReflective);
			if (valueConverter == null && TraceData.IsEnabled)
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Error, TraceData.CannotCreateDefaultValueConverter(type, base.TargetProperty.PropertyType, base.IsReflective ? "two-way" : "one-way"), this);
			}
			if (valueConverter == DefaultValueConverter.ValueConverterNotNeeded)
			{
				valueConverter = null;
			}
			Converter = valueConverter;
		}
	}

	internal static bool HasLocalDataContext(DependencyObject d)
	{
		bool hasModifiers;
		BaseValueSourceInternal valueSource = d.GetValueSource(FrameworkElement.DataContextProperty, null, out hasModifiers);
		if (valueSource != BaseValueSourceInternal.Inherited)
		{
			return valueSource != BaseValueSourceInternal.Default || hasModifiers;
		}
		return false;
	}

	internal override bool AttachOverride(DependencyObject target, DependencyProperty dp)
	{
		if (!base.AttachOverride(target, dp))
		{
			return false;
		}
		if (ParentBinding.SourceReference == null || ParentBinding.SourceReference.UsesMentor)
		{
			DependencyObject dependencyObject = Helper.FindMentor(target);
			if (dependencyObject != target)
			{
				InheritanceContextChangedEventManager.AddHandler(target, OnInheritanceContextChanged);
				base.UsingMentor = true;
				if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach))
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.UseMentor(TraceData.Identify(this), TraceData.Identify(dependencyObject)), this);
				}
			}
		}
		if (base.IsUpdateOnLostFocus)
		{
			Invariant.Assert(!base.IsInMultiBindingExpression, "Source BindingExpressions of a MultiBindingExpression should never be UpdateOnLostFocus.");
			LostFocusEventManager.AddHandler(target, OnLostFocus);
		}
		AttachToContext(AttachAttempt.First);
		if (base.StatusInternal == BindingStatusInternal.Unattached)
		{
			base.Engine.AddTask(this, TaskOps.AttachToContext);
			if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach))
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.DeferAttachToContext(TraceData.Identify(this)), this);
			}
		}
		GC.KeepAlive(target);
		return true;
	}

	internal override void DetachOverride()
	{
		Deactivate();
		DetachFromContext();
		DependencyObject targetElement = base.TargetElement;
		if (targetElement != null && base.IsUpdateOnLostFocus)
		{
			LostFocusEventManager.RemoveHandler(targetElement, OnLostFocus);
		}
		if (base.ValidatesOnNotifyDataErrors)
		{
			WeakReference weakReference = (WeakReference)GetValue(Feature.DataErrorValue, null);
			INotifyDataErrorInfo notifyDataErrorInfo = ((weakReference == null) ? null : (weakReference.Target as INotifyDataErrorInfo));
			if (notifyDataErrorInfo != null)
			{
				ErrorsChangedEventManager.RemoveHandler(notifyDataErrorInfo, OnErrorsChanged);
				SetValue(Feature.DataErrorValue, null, null);
			}
		}
		ChangeValue(DependencyProperty.UnsetValue, notify: false);
		base.DetachOverride();
	}

	private void AttachToContext(AttachAttempt attempt)
	{
		DependencyObject targetElement = base.TargetElement;
		if (targetElement == null)
		{
			return;
		}
		bool flag = TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach);
		bool isTracing = TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach);
		if (attempt == AttachAttempt.First)
		{
			ObjectRef sourceReference = ParentBinding.SourceReference;
			if (sourceReference != null && sourceReference.TreeContextIsRequired(targetElement))
			{
				if (flag)
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.SourceRequiresTreeContext(TraceData.Identify(this), sourceReference.Identify()), this);
				}
				return;
			}
		}
		bool flag2 = attempt == AttachAttempt.Last;
		if (flag)
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.AttachToContext(TraceData.Identify(this), flag2 ? " (last chance)" : string.Empty), this);
		}
		if (!flag2 && ParentBinding.TreeContextIsRequired && (targetElement.GetValue(XmlAttributeProperties.XmlnsDictionaryProperty) == null || targetElement.GetValue(XmlAttributeProperties.XmlNamespaceMapsProperty) == null))
		{
			if (flag)
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.PathRequiresTreeContext(TraceData.Identify(this), ParentBinding.Path.Path), this);
			}
			return;
		}
		DependencyObject dependencyObject = ((!base.UsingMentor) ? targetElement : Helper.FindMentor(targetElement));
		if (dependencyObject == null)
		{
			if (flag)
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.NoMentorExtended(TraceData.Identify(this)), this);
			}
			if (flag2)
			{
				SetStatus(BindingStatusInternal.PathError);
				if (TraceData.IsEnabled)
				{
					TraceData.TraceAndNotify(TraceEventType.Error, TraceData.NoMentor, this);
				}
			}
			return;
		}
		DependencyObject dependencyObject2 = null;
		bool flag3 = true;
		if (ParentBinding.SourceReference == null)
		{
			dependencyObject2 = dependencyObject;
			if (base.TargetProperty == FrameworkElement.DataContextProperty || (base.TargetProperty == ContentPresenter.ContentProperty && targetElement is ContentPresenter) || (base.UsingMentor && targetElement is CollectionViewSource collectionViewSource && collectionViewSource.PropertyForInheritanceContext == FrameworkElement.DataContextProperty))
			{
				dependencyObject2 = FrameworkElement.GetFrameworkParent(dependencyObject2);
				flag3 = dependencyObject2 != null;
			}
		}
		else if (ParentBinding.SourceReference is RelativeObjectRef { ReturnsDataContext: not false } relativeObjectRef)
		{
			object @object = relativeObjectRef.GetObject(dependencyObject, new ObjectRefArgs
			{
				IsTracing = isTracing
			});
			dependencyObject2 = @object as DependencyObject;
			flag3 = @object != DependencyProperty.UnsetValue;
		}
		if (flag)
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.ContextElement(TraceData.Identify(this), TraceData.Identify(dependencyObject2), flag3 ? "OK" : "error"), this);
		}
		if (!flag3)
		{
			if (flag2)
			{
				SetStatus(BindingStatusInternal.PathError);
				if (TraceData.IsEnabled)
				{
					TraceData.TraceAndNotify(TraceEventType.Error, TraceData.NoDataContext, this);
				}
			}
			return;
		}
		object obj;
		ObjectRef sourceReference2;
		if (dependencyObject2 != null)
		{
			obj = dependencyObject2.GetValue(FrameworkElement.DataContextProperty);
			if (obj == null && !flag2 && !HasLocalDataContext(dependencyObject2))
			{
				if (flag)
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.NullDataContext(TraceData.Identify(this)), this);
				}
				return;
			}
		}
		else if ((sourceReference2 = ParentBinding.SourceReference) != null)
		{
			ObjectRefArgs objectRefArgs = new ObjectRefArgs
			{
				IsTracing = isTracing,
				ResolveNamesInTemplate = base.ResolveNamesInTemplate
			};
			obj = sourceReference2.GetDataObject(dependencyObject, objectRefArgs);
			if (obj == DependencyProperty.UnsetValue)
			{
				if (flag2)
				{
					SetStatus(BindingStatusInternal.PathError);
					if (TraceData.IsEnabled)
					{
						TraceData.TraceAndNotify(base.TraceLevel, TraceData.NoSource(sourceReference2), this);
					}
				}
				return;
			}
			if (!flag2 && objectRefArgs.NameResolvedInOuterScope)
			{
				base.Engine.AddTask(this, TaskOps.VerifySourceReference);
			}
		}
		else
		{
			obj = null;
		}
		if (dependencyObject2 != null)
		{
			_ctxElement = new WeakReference(dependencyObject2);
		}
		ChangeWorkerSources(null, 0);
		if (!base.UseDefaultValueConverter)
		{
			Converter = ParentBinding.Converter;
			if (Converter == null)
			{
				throw new InvalidOperationException(SR.MissingValueConverter);
			}
		}
		JoinBindingGroup(base.IsReflective, dependencyObject2);
		SetStatus(BindingStatusInternal.Inactive);
		if (base.IsInPriorityBindingExpression)
		{
			base.ParentPriorityBindingExpression.InvalidateChild(this);
		}
		else
		{
			Activate(obj);
		}
		GC.KeepAlive(targetElement);
	}

	private void DetachFromContext()
	{
		if (HasValue(Feature.DataProvider))
		{
			DataChangedEventManager.RemoveHandler(DataProvider, OnDataChanged);
		}
		if (!base.UseDefaultValueConverter)
		{
			Converter = null;
		}
		if (!base.IsInBindingExpressionCollection)
		{
			ChangeSources(null);
		}
		if (base.UsingMentor)
		{
			DependencyObject targetElement = base.TargetElement;
			if (targetElement != null)
			{
				InheritanceContextChangedEventManager.RemoveHandler(targetElement, OnInheritanceContextChanged);
			}
		}
		_ctxElement = null;
	}

	internal override void Activate()
	{
		if (!CanActivate)
		{
			return;
		}
		if (_ctxElement == null)
		{
			if (base.StatusInternal != BindingStatusInternal.Inactive)
			{
				return;
			}
			DependencyObject dependencyObject = base.TargetElement;
			if (dependencyObject == null)
			{
				return;
			}
			if (base.UsingMentor)
			{
				dependencyObject = Helper.FindMentor(dependencyObject);
				if (dependencyObject == null)
				{
					SetStatus(BindingStatusInternal.PathError);
					if (TraceData.IsEnabled)
					{
						TraceData.TraceAndNotify(TraceEventType.Error, TraceData.NoMentor, this);
					}
					return;
				}
			}
			Activate(ParentBinding.SourceReference.GetDataObject(dependencyObject, new ObjectRefArgs
			{
				ResolveNamesInTemplate = base.ResolveNamesInTemplate
			}));
			return;
		}
		DependencyObject contextElement = ContextElement;
		if (contextElement == null)
		{
			SetStatus(BindingStatusInternal.PathError);
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.NoDataContext, this);
			}
		}
		else
		{
			object value = contextElement.GetValue(FrameworkElement.DataContextProperty);
			if (base.StatusInternal == BindingStatusInternal.Inactive || !ItemsControl.EqualsEx(value, DataItem))
			{
				Activate(value);
			}
		}
	}

	internal void Activate(object item)
	{
		DependencyObject targetElement = base.TargetElement;
		if (targetElement == null)
		{
			return;
		}
		if (item == BindingExpressionBase.DisconnectedItem)
		{
			Disconnect();
			return;
		}
		bool flag = TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach);
		Deactivate();
		if (!ParentBinding.BindsDirectlyToSource)
		{
			CollectionViewSource collectionViewSource2 = (CollectionViewSource = item as CollectionViewSource);
			if (collectionViewSource2 != null)
			{
				item = collectionViewSource2.CollectionView;
				ChangeWorkerSources(null, 0);
				if (flag)
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.UseCVS(TraceData.Identify(this), TraceData.Identify(collectionViewSource2)), this);
				}
			}
			else
			{
				item = DereferenceDataProvider(item);
			}
		}
		_dataItem = BindingExpressionBase.CreateReference(item);
		if (flag)
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.ActivateItem(TraceData.Identify(this), TraceData.Identify(item)), this);
		}
		if (Worker == null)
		{
			CreateWorker();
		}
		SetStatus(BindingStatusInternal.Active);
		Worker.AttachDataItem();
		bool flag2 = base.IsOneWayToSource;
		if (ShouldUpdateWithCurrentValue(targetElement, out var currentValue))
		{
			flag2 = true;
			ChangeValue(currentValue, notify: false);
			base.NeedsUpdate = true;
		}
		if (!flag2)
		{
			ValidationError error;
			object initialValue = GetInitialValue(targetElement, out error);
			bool flag3 = initialValue == NullDataItem;
			if (!flag3)
			{
				TransferValue(initialValue, isASubPropertyChange: false);
			}
			if (error != null)
			{
				UpdateValidationError(error, flag3);
			}
		}
		else if (!base.IsInMultiBindingExpression)
		{
			UpdateValue();
		}
		GC.KeepAlive(targetElement);
	}

	private object GetInitialValue(DependencyObject target, out ValidationError error)
	{
		BindingGroup bindingGroup = base.RootBindingExpression.FindBindingGroup(isReflective: true, ContextElement);
		BindingGroup.ProposedValueEntry proposedValueEntry;
		object obj;
		if (bindingGroup == null || (proposedValueEntry = bindingGroup.GetProposedValueEntry(SourceItem, SourcePropertyName)) == null)
		{
			error = null;
			obj = DependencyProperty.UnsetValue;
		}
		else
		{
			error = proposedValueEntry.ValidationError;
			if (!base.IsReflective || !base.TargetProperty.IsValidValue(proposedValueEntry.RawValue))
			{
				obj = ((proposedValueEntry.ConvertedValue != DependencyProperty.UnsetValue) ? proposedValueEntry.ConvertedValue : UseFallbackValue());
			}
			else
			{
				target.SetValue(base.TargetProperty, proposedValueEntry.RawValue);
				obj = NullDataItem;
				bindingGroup.RemoveProposedValueEntry(proposedValueEntry);
			}
			if (obj != NullDataItem)
			{
				bindingGroup.AddBindingForProposedValue(this, SourceItem, SourcePropertyName);
			}
		}
		return obj;
	}

	internal override void Deactivate()
	{
		if (base.StatusInternal != BindingStatusInternal.Inactive)
		{
			if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach))
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.Deactivate(TraceData.Identify(this)), this);
			}
			CancelPendingTasks();
			if (Worker != null)
			{
				Worker.DetachDataItem();
			}
			ChangeValue(BindingExpressionBase.DefaultValueObject, notify: false);
			_dataItem = null;
			SetStatus(BindingStatusInternal.Inactive);
		}
	}

	internal override void Disconnect()
	{
		_dataItem = BindingExpressionBase.CreateReference(BindingExpressionBase.DisconnectedItem);
		if (Worker != null)
		{
			Worker.AttachDataItem();
			base.Disconnect();
		}
	}

	private object DereferenceDataProvider(object item)
	{
		DataSourceProvider dataSourceProvider = item as DataSourceProvider;
		DataSourceProvider dataSourceProvider2 = DataProvider;
		if (dataSourceProvider != dataSourceProvider2)
		{
			if (dataSourceProvider2 != null)
			{
				DataChangedEventManager.RemoveHandler(dataSourceProvider2, OnDataChanged);
			}
			DataProvider = dataSourceProvider;
			dataSourceProvider2 = dataSourceProvider;
			if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Attach))
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.UseDataProvider(TraceData.Identify(this), TraceData.Identify(dataSourceProvider)), this);
			}
			if (dataSourceProvider != null)
			{
				DataChangedEventManager.AddHandler(dataSourceProvider, OnDataChanged);
				dataSourceProvider.InitialLoad();
			}
		}
		if (dataSourceProvider2 == null)
		{
			return item;
		}
		return dataSourceProvider2.Data;
	}

	internal override object GetSourceItem(object newValue)
	{
		return SourceItem;
	}

	private void CreateWorker()
	{
		Invariant.Assert(Worker == null, "duplicate call to CreateWorker");
		_worker = new ClrBindingWorker(this, base.Engine);
	}

	internal void ChangeWorkerSources(WeakDependencySource[] newWorkerSources, int n)
	{
		int destinationIndex = 0;
		int num = n;
		DependencyObject contextElement = ContextElement;
		CollectionViewSource collectionViewSource = CollectionViewSource;
		bool usesLanguage = base.UsesLanguage;
		if (contextElement != null)
		{
			num++;
		}
		if (collectionViewSource != null)
		{
			num++;
		}
		if (usesLanguage)
		{
			num++;
		}
		WeakDependencySource[] array = ((num > 0) ? new WeakDependencySource[num] : null);
		if (contextElement != null)
		{
			array[destinationIndex++] = new WeakDependencySource(_ctxElement, FrameworkElement.DataContextProperty);
		}
		if (collectionViewSource != null)
		{
			WeakReference weakReference = GetValue(Feature.CollectionViewSource, null) as WeakReference;
			array[destinationIndex++] = ((weakReference != null) ? new WeakDependencySource(weakReference, CollectionViewSource.ViewProperty) : new WeakDependencySource(collectionViewSource, CollectionViewSource.ViewProperty));
		}
		if (usesLanguage)
		{
			array[destinationIndex++] = new WeakDependencySource(base.TargetElementReference, FrameworkElement.LanguageProperty);
		}
		if (n > 0)
		{
			Array.Copy(newWorkerSources, 0, array, destinationIndex, n);
		}
		ChangeSources(array);
	}

	private void TransferValue()
	{
		TransferValue(DependencyProperty.UnsetValue, isASubPropertyChange: false);
	}

	internal void TransferValue(object newValue, bool isASubPropertyChange)
	{
		DependencyObject targetElement = base.TargetElement;
		if (targetElement == null || Worker == null)
		{
			return;
		}
		Type effectiveTargetType = GetEffectiveTargetType();
		IValueConverter valueConverter = null;
		bool flag = TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer);
		base.IsTransferPending = false;
		base.IsInTransfer = true;
		base.UsingFallbackValue = false;
		object obj = ((newValue == DependencyProperty.UnsetValue) ? Worker.RawValue() : newValue);
		UpdateNotifyDataErrors(obj);
		if (flag)
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GetRawValue(TraceData.Identify(this), TraceData.Identify(obj)), this);
		}
		if (obj != DependencyProperty.UnsetValue)
		{
			bool flag2 = false;
			if (!base.UseDefaultValueConverter)
			{
				obj = Converter.Convert(obj, effectiveTargetType, ParentBinding.ConverterParameter, GetCulture());
				if (base.IsDetached)
				{
					return;
				}
				if (flag)
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.UserConverter(TraceData.Identify(this), TraceData.Identify(obj)), this);
				}
				if (obj != null && obj != Binding.DoNothing && obj != DependencyProperty.UnsetValue && !effectiveTargetType.IsAssignableFrom(obj.GetType()))
				{
					valueConverter = DynamicConverter;
				}
			}
			else
			{
				valueConverter = Converter;
			}
			if (obj != Binding.DoNothing && obj != DependencyProperty.UnsetValue)
			{
				if (base.EffectiveTargetNullValue != DependencyProperty.UnsetValue && BindingExpressionBase.IsNullValue(obj))
				{
					obj = base.EffectiveTargetNullValue;
					if (flag)
					{
						TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.NullConverter(TraceData.Identify(this), TraceData.Identify(obj)), this);
					}
				}
				else if (obj == DBNull.Value && (Converter == null || base.UseDefaultValueConverter))
				{
					if (effectiveTargetType.IsGenericType && effectiveTargetType.GetGenericTypeDefinition() == typeof(Nullable<>))
					{
						obj = null;
					}
					else
					{
						obj = DependencyProperty.UnsetValue;
						flag2 = true;
					}
					if (flag)
					{
						TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.ConvertDBNull(TraceData.Identify(this), TraceData.Identify(obj)), this);
					}
				}
				else if (valueConverter != null || base.EffectiveStringFormat != null)
				{
					obj = ConvertHelper(valueConverter, obj, effectiveTargetType, base.TargetElement, GetCulture());
					if (flag)
					{
						TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.DefaultConverter(TraceData.Identify(this), TraceData.Identify(obj)), this);
					}
				}
			}
			if (!flag2 && obj == DependencyProperty.UnsetValue)
			{
				SetStatus(BindingStatusInternal.UpdateTargetError);
			}
		}
		int num;
		if (obj != Binding.DoNothing)
		{
			if (!base.IsInMultiBindingExpression && obj != IgnoreDefaultValue && obj != DependencyProperty.UnsetValue && !base.TargetProperty.IsValidValue(obj))
			{
				if (TraceData.IsEnabled && !base.IsInBindingExpressionCollection)
				{
					TraceData.TraceAndNotify(base.TraceLevel, TraceData.BadValueAtTransfer, this, new object[2] { obj, this });
				}
				if (flag)
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.BadValueAtTransferExtended(TraceData.Identify(this), TraceData.Identify(obj)), this);
				}
				obj = DependencyProperty.UnsetValue;
				if (base.StatusInternal == BindingStatusInternal.Active)
				{
					SetStatus(BindingStatusInternal.UpdateTargetError);
				}
			}
			if (obj == DependencyProperty.UnsetValue)
			{
				obj = UseFallbackValue();
				if (flag)
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.UseFallback(TraceData.Identify(this), TraceData.Identify(obj)), this);
				}
			}
			if (obj == IgnoreDefaultValue)
			{
				obj = Expression.NoValue;
			}
			if (flag)
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.TransferValue(TraceData.Identify(this), TraceData.Identify(obj)), this);
			}
			if (base.IsInUpdate)
			{
				num = ((!ItemsControl.EqualsEx(obj, base.Value)) ? 1 : 0);
				if (num == 0)
				{
					goto IL_03a8;
				}
			}
			else
			{
				num = 1;
			}
			ChangeValue(obj, notify: true);
			Invalidate(isASubPropertyChange);
			ValidateOnTargetUpdated();
			goto IL_03a8;
		}
		goto IL_03b6;
		IL_03a8:
		Clean();
		if (num != 0)
		{
			OnTargetUpdated();
		}
		goto IL_03b6;
		IL_03b6:
		base.IsInTransfer = false;
		GC.KeepAlive(targetElement);
	}

	private void ValidateOnTargetUpdated()
	{
		ValidationError validationError = null;
		Collection<ValidationRule> validationRulesInternal = ParentBinding.ValidationRulesInternal;
		CultureInfo cultureInfo = null;
		bool flag = ParentBinding.ValidatesOnDataErrors;
		if (validationRulesInternal != null)
		{
			object obj = DependencyProperty.UnsetValue;
			object obj2 = DependencyProperty.UnsetValue;
			foreach (ValidationRule item in validationRulesInternal)
			{
				if (!item.ValidatesOnTargetUpdated)
				{
					continue;
				}
				if (item is DataErrorValidationRule)
				{
					flag = false;
				}
				object value;
				switch (item.ValidationStep)
				{
				case ValidationStep.RawProposedValue:
					if (obj == DependencyProperty.UnsetValue)
					{
						obj = GetRawProposedValue();
					}
					value = obj;
					break;
				case ValidationStep.ConvertedProposedValue:
				case ValidationStep.UpdatedValue:
				case ValidationStep.CommittedValue:
					if (obj2 == DependencyProperty.UnsetValue)
					{
						obj2 = Worker.RawValue();
					}
					value = obj2;
					break;
				default:
					throw new InvalidOperationException(SR.Format(SR.ValidationRule_UnknownStep, item.ValidationStep, item));
				}
				if (cultureInfo == null)
				{
					cultureInfo = GetCulture();
				}
				validationError = RunValidationRule(item, value, cultureInfo);
				if (validationError != null)
				{
					break;
				}
			}
		}
		if (flag && validationError == null)
		{
			if (cultureInfo == null)
			{
				cultureInfo = GetCulture();
			}
			validationError = RunValidationRule(DataErrorValidationRule.Instance, this, cultureInfo);
		}
		UpdateValidationError(validationError);
	}

	private ValidationError RunValidationRule(ValidationRule validationRule, object value, CultureInfo culture)
	{
		ValidationResult validationResult = validationRule.Validate(value, culture, this);
		if (validationResult.IsValid)
		{
			return null;
		}
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.ValidationRuleFailed(TraceData.Identify(this), TraceData.Identify(validationRule)), this);
		}
		return new ValidationError(validationRule, this, validationResult.ErrorContent, null);
	}

	private object ConvertHelper(IValueConverter converter, object value, Type targetType, object parameter, CultureInfo culture)
	{
		string effectiveStringFormat = base.EffectiveStringFormat;
		Invariant.Assert(converter != null || effectiveStringFormat != null);
		object obj = null;
		try
		{
			if (effectiveStringFormat != null)
			{
				return string.Format(culture, effectiveStringFormat, value);
			}
			return converter.Convert(value, targetType, parameter, culture);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalApplicationException(ex))
			{
				throw;
			}
			if (TraceData.IsEnabled)
			{
				string text = (string.IsNullOrEmpty(effectiveStringFormat) ? converter.GetType().Name : "StringFormat");
				TraceData.TraceAndNotify(base.TraceLevel, TraceData.BadConverterForTransfer(text, AvTrace.ToStringHelper(value), AvTrace.TypeName(value)), this, ex);
			}
			return DependencyProperty.UnsetValue;
		}
		catch
		{
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(base.TraceLevel, TraceData.BadConverterForTransfer(converter.GetType().Name, AvTrace.ToStringHelper(value), AvTrace.TypeName(value)), this);
			}
			return DependencyProperty.UnsetValue;
		}
	}

	private object ConvertBackHelper(IValueConverter converter, object value, Type sourceType, object parameter, CultureInfo culture)
	{
		Invariant.Assert(converter != null);
		object obj = null;
		try
		{
			return converter.ConvertBack(value, sourceType, parameter, culture);
		}
		catch (Exception ex)
		{
			Exception ex2 = CriticalExceptions.Unwrap(ex);
			if (CriticalExceptions.IsCriticalApplicationException(ex2))
			{
				throw;
			}
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.BadConverterForUpdate(AvTrace.ToStringHelper(base.Value), AvTrace.TypeName(value)), this, ex2);
			}
			ProcessException(ex2, base.ValidatesOnExceptions);
			return DependencyProperty.UnsetValue;
		}
		catch
		{
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.BadConverterForUpdate(AvTrace.ToStringHelper(base.Value), AvTrace.TypeName(value)), this);
			}
			return DependencyProperty.UnsetValue;
		}
	}

	internal void ScheduleTransfer(bool isASubPropertyChange)
	{
		if (isASubPropertyChange && Converter != null)
		{
			isASubPropertyChange = false;
		}
		TransferValue(DependencyProperty.UnsetValue, isASubPropertyChange);
	}

	private void OnTargetUpdated()
	{
		if (!base.NotifyOnTargetUpdated)
		{
			return;
		}
		DependencyObject targetElement = base.TargetElement;
		if (targetElement != null && ((!base.IsInMultiBindingExpression && (!base.IsInPriorityBindingExpression || this == base.ParentPriorityBindingExpression.ActiveBindingExpression)) || (base.IsAttaching && (base.StatusInternal == BindingStatusInternal.Active || base.UsingFallbackValue))))
		{
			if (base.IsAttaching && base.RootBindingExpression == targetElement.ReadLocalValue(base.TargetProperty))
			{
				base.Engine.AddTask(this, TaskOps.RaiseTargetUpdatedEvent);
			}
			else
			{
				OnTargetUpdated(targetElement, base.TargetProperty);
			}
		}
	}

	private void OnSourceUpdated()
	{
		if (base.NotifyOnSourceUpdated)
		{
			DependencyObject targetElement = base.TargetElement;
			if (targetElement != null && !base.IsInMultiBindingExpression && (!base.IsInPriorityBindingExpression || this == base.ParentPriorityBindingExpression.ActiveBindingExpression))
			{
				OnSourceUpdated(targetElement, base.TargetProperty);
			}
		}
	}

	internal override bool ShouldReactToDirtyOverride()
	{
		return DataItem != BindingExpressionBase.DisconnectedItem;
	}

	internal override bool UpdateOverride()
	{
		if (!base.NeedsUpdate || !base.IsReflective || base.IsInTransfer || Worker == null || !Worker.CanUpdate)
		{
			return true;
		}
		return UpdateValue();
	}

	internal override object ConvertProposedValue(object value)
	{
		object p = value;
		bool flag = TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer);
		if (flag)
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.UpdateRawValue(TraceData.Identify(this), TraceData.Identify(value)), this);
		}
		Type sourcePropertyType = Worker.SourcePropertyType;
		IValueConverter valueConverter = null;
		CultureInfo culture = GetCulture();
		if (Converter != null)
		{
			if (!base.UseDefaultValueConverter)
			{
				value = Converter.ConvertBack(value, sourcePropertyType, ParentBinding.ConverterParameter, culture);
				if (base.IsDetached)
				{
					return Binding.DoNothing;
				}
				if (flag)
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.UserConvertBack(TraceData.Identify(this), TraceData.Identify(value)), this);
				}
				if (value != Binding.DoNothing && value != DependencyProperty.UnsetValue && !IsValidValueForUpdate(value, sourcePropertyType))
				{
					valueConverter = DynamicConverter;
				}
			}
			else
			{
				valueConverter = Converter;
			}
		}
		if (value != Binding.DoNothing && value != DependencyProperty.UnsetValue)
		{
			if (BindingExpressionBase.IsNullValue(value))
			{
				if (value == null || !IsValidValueForUpdate(value, sourcePropertyType))
				{
					value = ((!Worker.IsDBNullValidForUpdate) ? NullValueForType(sourcePropertyType) : DBNull.Value);
				}
			}
			else if (valueConverter != null)
			{
				value = ConvertBackHelper(valueConverter, value, sourcePropertyType, base.TargetElement, culture);
				if (flag)
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.DefaultConvertBack(TraceData.Identify(this), TraceData.Identify(value)), this);
				}
			}
		}
		if (flag)
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.Update(TraceData.Identify(this), TraceData.Identify(value)), this);
		}
		if (value == DependencyProperty.UnsetValue && ValidationError == null)
		{
			ValidationError validationError = new ValidationError(ConversionValidationRule.Instance, this, SR.Format(SR.Validation_ConversionFailed, p), null);
			UpdateValidationError(validationError);
		}
		return value;
	}

	internal override bool ObtainConvertedProposedValue(BindingGroup bindingGroup)
	{
		bool result = true;
		object obj;
		if (base.NeedsUpdate)
		{
			obj = bindingGroup.GetValue(this);
			if (obj != DependencyProperty.UnsetValue)
			{
				obj = ConvertProposedValue(obj);
				if (obj == DependencyProperty.UnsetValue)
				{
					result = false;
				}
			}
		}
		else
		{
			obj = BindingGroup.DeferredSourceValue;
		}
		bindingGroup.SetValue(this, obj);
		return result;
	}

	internal override object UpdateSource(object value)
	{
		if (value == DependencyProperty.UnsetValue)
		{
			SetStatus(BindingStatusInternal.UpdateSourceError);
		}
		if (value == Binding.DoNothing || value == DependencyProperty.UnsetValue || ShouldIgnoreUpdate())
		{
			return value;
		}
		try
		{
			BeginSourceUpdate();
			Worker.UpdateValue(value);
		}
		catch (Exception ex)
		{
			Exception ex2 = CriticalExceptions.Unwrap(ex);
			if (CriticalExceptions.IsCriticalApplicationException(ex2))
			{
				throw;
			}
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.WorkerUpdateFailed, this, ex2);
			}
			ProcessException(ex2, base.ValidatesOnExceptions || base.BindingGroup != null);
			SetStatus(BindingStatusInternal.UpdateSourceError);
			value = DependencyProperty.UnsetValue;
		}
		catch
		{
			if (TraceData.IsEnabled)
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.WorkerUpdateFailed, this);
			}
			SetStatus(BindingStatusInternal.UpdateSourceError);
			value = DependencyProperty.UnsetValue;
		}
		finally
		{
			EndSourceUpdate();
		}
		OnSourceUpdated();
		return value;
	}

	internal override bool UpdateSource(BindingGroup bindingGroup)
	{
		bool result = true;
		if (base.NeedsUpdate)
		{
			object value = bindingGroup.GetValue(this);
			value = UpdateSource(value);
			bindingGroup.SetValue(this, value);
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
	}

	internal override bool Validate(object value, ValidationStep validationStep)
	{
		bool flag = base.Validate(value, validationStep);
		switch (validationStep)
		{
		case ValidationStep.UpdatedValue:
			if (flag && ParentBinding.ValidatesOnDataErrors)
			{
				ValidationError validationError = GetValidationErrors(validationStep);
				if (validationError != null && validationError.RuleInError != DataErrorValidationRule.Instance)
				{
					validationError = null;
				}
				ValidationError validationError2 = RunValidationRule(DataErrorValidationRule.Instance, this, GetCulture());
				if (validationError2 != null)
				{
					UpdateValidationError(validationError2);
					flag = false;
				}
				else if (validationError != null)
				{
					UpdateValidationError(null);
				}
			}
			break;
		case ValidationStep.CommittedValue:
			if (flag)
			{
				base.NeedsValidation = false;
			}
			break;
		}
		return flag;
	}

	internal override bool CheckValidationRules(BindingGroup bindingGroup, ValidationStep validationStep)
	{
		if (!base.NeedsValidation)
		{
			return true;
		}
		object value;
		switch (validationStep)
		{
		case ValidationStep.RawProposedValue:
			value = GetRawProposedValue();
			break;
		case ValidationStep.ConvertedProposedValue:
			value = bindingGroup.GetValue(this);
			break;
		case ValidationStep.UpdatedValue:
		case ValidationStep.CommittedValue:
			value = this;
			break;
		default:
			throw new InvalidOperationException(SR.Format(SR.ValidationRule_UnknownStep, validationStep, bindingGroup));
		}
		return Validate(value, validationStep);
	}

	internal override bool ValidateAndConvertProposedValue(out Collection<ProposedValue> values)
	{
		values = null;
		object rawProposedValue = GetRawProposedValue();
		bool flag = Validate(rawProposedValue, ValidationStep.RawProposedValue);
		object obj = (flag ? ConvertProposedValue(rawProposedValue) : DependencyProperty.UnsetValue);
		if (obj != Binding.DoNothing)
		{
			flag = obj != DependencyProperty.UnsetValue && Validate(obj, ValidationStep.ConvertedProposedValue);
		}
		else
		{
			obj = DependencyProperty.UnsetValue;
		}
		values = new Collection<ProposedValue>();
		values.Add(new ProposedValue(this, rawProposedValue, obj));
		return flag;
	}

	private bool IsValidValueForUpdate(object value, Type sourceType)
	{
		if (value == null)
		{
			return true;
		}
		if (sourceType.IsAssignableFrom(value.GetType()))
		{
			return true;
		}
		if (Convert.IsDBNull(value))
		{
			return Worker.IsDBNullValidForUpdate;
		}
		return false;
	}

	private void ProcessException(Exception ex, bool validate)
	{
		object obj = null;
		ValidationError validationError = null;
		if (ExceptionFilterExists())
		{
			obj = CallDoFilterException(ex);
			if (obj == null)
			{
				return;
			}
			validationError = obj as ValidationError;
		}
		if (validationError == null && validate)
		{
			ValidationRule instance = ExceptionValidationRule.Instance;
			validationError = ((obj != null) ? new ValidationError(instance, this, obj, ex) : new ValidationError(instance, this, ex.Message, ex));
		}
		if (validationError != null)
		{
			UpdateValidationError(validationError);
		}
	}

	private bool ShouldIgnoreUpdate()
	{
		if (base.TargetProperty.OwnerType != typeof(Selector) && base.TargetProperty != ComboBox.TextProperty)
		{
			return false;
		}
		DependencyObject contextElement = ContextElement;
		object obj;
		if (contextElement == null)
		{
			DependencyObject dependencyObject = base.TargetElement;
			if (dependencyObject != null && base.UsingMentor)
			{
				dependencyObject = Helper.FindMentor(dependencyObject);
			}
			if (dependencyObject == null)
			{
				return true;
			}
			obj = ParentBinding.SourceReference.GetDataObject(dependencyObject, new ObjectRefArgs
			{
				ResolveNamesInTemplate = base.ResolveNamesInTemplate
			});
		}
		else
		{
			obj = contextElement.GetValue(FrameworkElement.DataContextProperty);
		}
		if (!ParentBinding.BindsDirectlyToSource)
		{
			if (obj is CollectionViewSource collectionViewSource)
			{
				obj = collectionViewSource.CollectionView;
			}
			else if (obj is DataSourceProvider dataSourceProvider)
			{
				obj = dataSourceProvider.Data;
			}
		}
		if (!ItemsControl.EqualsEx(DataItem, obj))
		{
			return true;
		}
		return !Worker.IsPathCurrent();
	}

	internal void UpdateNotifyDataErrors(INotifyDataErrorInfo indei, string propertyName, object value)
	{
		if (!base.ValidatesOnNotifyDataErrors || base.IsDetached)
		{
			return;
		}
		WeakReference weakReference = (WeakReference)GetValue(Feature.DataErrorValue, null);
		INotifyDataErrorInfo notifyDataErrorInfo = ((weakReference == null) ? null : (weakReference.Target as INotifyDataErrorInfo));
		if (value != DependencyProperty.UnsetValue && value != notifyDataErrorInfo && base.IsDynamic)
		{
			if (notifyDataErrorInfo != null)
			{
				ErrorsChangedEventManager.RemoveHandler(notifyDataErrorInfo, OnErrorsChanged);
			}
			INotifyDataErrorInfo notifyDataErrorInfo2 = value as INotifyDataErrorInfo;
			object value2 = BindingExpressionBase.ReplaceReference(weakReference, notifyDataErrorInfo2);
			SetValue(Feature.DataErrorValue, value2, null);
			notifyDataErrorInfo = notifyDataErrorInfo2;
			if (notifyDataErrorInfo2 != null)
			{
				ErrorsChangedEventManager.AddHandler(notifyDataErrorInfo2, OnErrorsChanged);
			}
		}
		base.IsDataErrorsChangedPending = false;
		try
		{
			List<object> dataErrors = GetDataErrors(indei, propertyName);
			List<object> dataErrors2 = GetDataErrors(notifyDataErrorInfo, string.Empty);
			List<object> errors = MergeErrors(dataErrors, dataErrors2);
			UpdateNotifyDataErrorValidationErrors(errors);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalApplicationException(ex))
			{
				throw;
			}
		}
	}

	private void UpdateNotifyDataErrors(object value)
	{
		if (base.ValidatesOnNotifyDataErrors)
		{
			UpdateNotifyDataErrors(SourceItem as INotifyDataErrorInfo, SourcePropertyName, value);
		}
	}

	internal static List<object> GetDataErrors(INotifyDataErrorInfo indei, string propertyName)
	{
		List<object> list = null;
		if (indei != null && indei.HasErrors)
		{
			for (int num = 3; num >= 0; num--)
			{
				try
				{
					list = new List<object>();
					IEnumerable errors = indei.GetErrors(propertyName);
					if (errors == null)
					{
						break;
					}
					foreach (object item in errors)
					{
						list.Add(item);
					}
				}
				catch (InvalidOperationException)
				{
					if (num == 0)
					{
						throw;
					}
					continue;
				}
				break;
			}
		}
		if (list != null && list.Count == 0)
		{
			list = null;
		}
		return list;
	}

	private List<object> MergeErrors(List<object> list1, List<object> list2)
	{
		if (list1 == null)
		{
			return list2;
		}
		if (list2 == null)
		{
			return list1;
		}
		foreach (object item in list2)
		{
			list1.Add(item);
		}
		return list1;
	}

	private void OnDataContextChanged(DependencyObject contextElement)
	{
		if (!base.IsInUpdate && CanActivate)
		{
			if (base.IsReflective && base.RootBindingExpression.ParentBindingBase.BindingGroupName == string.Empty)
			{
				RejoinBindingGroup(base.IsReflective, contextElement);
			}
			object value = contextElement.GetValue(FrameworkElement.DataContextProperty);
			if (!ItemsControl.EqualsEx(DataItem, value))
			{
				Activate(value);
			}
		}
	}

	internal void OnCurrentChanged(object sender, EventArgs e)
	{
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GotEvent(TraceData.Identify(this), "CurrentChanged", TraceData.Identify(sender)), this);
		}
		Worker.OnCurrentChanged(sender as ICollectionView, e);
	}

	internal void OnCurrentChanging(object sender, CurrentChangingEventArgs e)
	{
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GotEvent(TraceData.Identify(this), "CurrentChanging", TraceData.Identify(sender)), this);
		}
		Update();
	}

	private void OnDataChanged(object sender, EventArgs e)
	{
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GotEvent(TraceData.Identify(this), "DataChanged", TraceData.Identify(sender)), this);
		}
		Activate(sender);
	}

	private void OnInheritanceContextChanged(object sender, EventArgs e)
	{
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GotEvent(TraceData.Identify(this), "InheritanceContextChanged", TraceData.Identify(sender)), this);
		}
		if (base.StatusInternal == BindingStatusInternal.Unattached)
		{
			base.Engine.CancelTask(this, TaskOps.AttachToContext);
			AttachToContext(AttachAttempt.Again);
			if (base.StatusInternal == BindingStatusInternal.Unattached)
			{
				base.Engine.AddTask(this, TaskOps.AttachToContext);
			}
		}
		else
		{
			AttachToContext(AttachAttempt.Last);
		}
	}

	internal override void OnLostFocus(object sender, RoutedEventArgs e)
	{
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GotEvent(TraceData.Identify(this), "LostFocus", TraceData.Identify(sender)), this);
		}
		Update();
	}

	private void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e)
	{
		if (base.Dispatcher.Thread == Thread.CurrentThread)
		{
			UpdateNotifyDataErrors(DependencyProperty.UnsetValue);
		}
		else if (!base.IsDataErrorsChangedPending)
		{
			base.IsDataErrorsChangedPending = true;
			base.Engine.Marshal(delegate
			{
				UpdateNotifyDataErrors(DependencyProperty.UnsetValue);
				return (object)null;
			}, null);
		}
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the listener handled the event; otherwise, false.</returns>
	/// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager" /> calling this method. This only recognizes manager objects of type <see cref="T:System.Collections.Specialized.CollectionChangedEventManager" />.</param>
	/// <param name="sender">Object that originated the event.</param>
	/// <param name="e">Event data.</param>
	bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return false;
	}

	private object CallDoFilterException(Exception ex)
	{
		if (ParentBinding.UpdateSourceExceptionFilter != null)
		{
			return ParentBinding.DoFilterException(this, ex);
		}
		if (base.IsInMultiBindingExpression)
		{
			return base.ParentMultiBindingExpression.ParentMultiBinding.DoFilterException(this, ex);
		}
		return null;
	}

	private bool ExceptionFilterExists()
	{
		if (ParentBinding.UpdateSourceExceptionFilter == null)
		{
			if (base.IsInMultiBindingExpression)
			{
				return base.ParentMultiBindingExpression.ParentMultiBinding.UpdateSourceExceptionFilter != null;
			}
			return false;
		}
		return true;
	}

	internal IDisposable ChangingValue()
	{
		return new ChangingValueHelper(this);
	}

	internal void CancelPendingTasks()
	{
		base.Engine.CancelTasks(this);
	}

	private void Replace()
	{
		DependencyObject targetElement = base.TargetElement;
		if (targetElement != null)
		{
			if (base.IsInBindingExpressionCollection)
			{
				base.ParentBindingExpressionBase.ReplaceChild(this);
			}
			else
			{
				BindingOperations.SetBinding(targetElement, base.TargetProperty, ParentBinding);
			}
		}
	}

	internal static void OnTargetUpdated(DependencyObject d, DependencyProperty dp)
	{
		DataTransferEventArgs dataTransferEventArgs = new DataTransferEventArgs(d, dp);
		dataTransferEventArgs.RoutedEvent = Binding.TargetUpdatedEvent;
		FrameworkObject frameworkObject = new FrameworkObject(d);
		if (!frameworkObject.IsValid && d != null)
		{
			frameworkObject.Reset(Helper.FindMentor(d));
		}
		frameworkObject.RaiseEvent(dataTransferEventArgs);
	}

	internal static void OnSourceUpdated(DependencyObject d, DependencyProperty dp)
	{
		DataTransferEventArgs dataTransferEventArgs = new DataTransferEventArgs(d, dp);
		dataTransferEventArgs.RoutedEvent = Binding.SourceUpdatedEvent;
		FrameworkObject frameworkObject = new FrameworkObject(d);
		if (!frameworkObject.IsValid && d != null)
		{
			frameworkObject.Reset(Helper.FindMentor(d));
		}
		frameworkObject.RaiseEvent(dataTransferEventArgs);
	}

	internal override void HandlePropertyInvalidation(DependencyObject d, DependencyPropertyChangedEventArgs args)
	{
		DependencyProperty property = args.Property;
		if (TraceData.IsExtendedTraceEnabled(this, TraceDataLevel.Transfer))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GotPropertyChanged(TraceData.Identify(this), TraceData.Identify(d), property.Name), this);
		}
		if (property == FrameworkElement.DataContextProperty)
		{
			DependencyObject contextElement = ContextElement;
			if (d == contextElement)
			{
				base.IsTransferPending = false;
				OnDataContextChanged(contextElement);
			}
		}
		if (property == CollectionViewSource.ViewProperty)
		{
			CollectionViewSource collectionViewSource = CollectionViewSource;
			if (d == collectionViewSource)
			{
				Activate(collectionViewSource);
			}
		}
		if (property == FrameworkElement.LanguageProperty && base.UsesLanguage && d == base.TargetElement)
		{
			InvalidateCulture();
			TransferValue();
		}
		if (Worker != null)
		{
			Worker.OnSourceInvalidation(d, property, args.IsASubPropertyChange);
		}
	}

	private void SetDataItem(object newItem)
	{
		_dataItem = BindingExpressionBase.CreateReference(newItem);
	}

	private object GetDataSourceForDataContext(DependencyObject d)
	{
		BindingExpression bindingExpression = null;
		for (DependencyObject dependencyObject = d; dependencyObject != null; dependencyObject = FrameworkElement.GetFrameworkParent(dependencyObject))
		{
			if (HasLocalDataContext(dependencyObject))
			{
				bindingExpression = BindingOperations.GetBindingExpression(dependencyObject, FrameworkElement.DataContextProperty);
				break;
			}
		}
		return bindingExpression?.DataSource;
	}
}
