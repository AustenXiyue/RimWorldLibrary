using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;

namespace MS.Internal.Data;

internal class ClrBindingWorker : BindingWorker
{
	private static readonly AsyncRequestCallback DoGetValueCallback = OnGetValueCallback;

	private static readonly AsyncRequestCallback CompleteGetValueCallback = OnCompleteGetValueCallback;

	private static readonly DispatcherOperationCallback CompleteGetValueLocalCallback = OnCompleteGetValueOperation;

	private static readonly AsyncRequestCallback DoSetValueCallback = OnSetValueCallback;

	private static readonly AsyncRequestCallback CompleteSetValueCallback = OnCompleteSetValueCallback;

	private static readonly DispatcherOperationCallback CompleteSetValueLocalCallback = OnCompleteSetValueOperation;

	private PropertyPathWorker _pathWorker;

	internal override Type SourcePropertyType => PW.GetType(PW.Length - 1);

	internal override bool IsDBNullValidForUpdate => PW.IsDBNullValidForUpdate;

	internal override object SourceItem => PW.SourceItem;

	internal override string SourcePropertyName => PW.SourcePropertyName;

	internal override bool CanUpdate
	{
		get
		{
			PropertyPathWorker pW = PW;
			int num = PW.Length - 1;
			if (num < 0)
			{
				return false;
			}
			object item = pW.GetItem(num);
			if (item == null || item == BindingExpression.NullDataItem)
			{
				return false;
			}
			object accessor = pW.GetAccessor(num);
			if (accessor == null || (accessor == DependencyProperty.UnsetValue && XmlWorker == null))
			{
				return false;
			}
			return true;
		}
	}

	internal bool TransfersDefaultValue => base.ParentBinding.TransfersDefaultValue;

	internal bool ValidatesOnNotifyDataErrors => base.ParentBindingExpression.ValidatesOnNotifyDataErrors;

	private PropertyPathWorker PW => _pathWorker;

	private XmlBindingWorker XmlWorker => (XmlBindingWorker)GetValue(Feature.XmlWorker, null);

	internal ClrBindingWorker(BindingExpression b, DataBindEngine engine)
		: base(b)
	{
		PropertyPath propertyPath = base.ParentBinding.Path;
		if (base.ParentBinding.XPath != null)
		{
			propertyPath = PrepareXmlBinding(propertyPath);
		}
		if (propertyPath == null)
		{
			propertyPath = new PropertyPath(string.Empty);
		}
		if (base.ParentBinding.Path == null)
		{
			base.ParentBinding.UsePath(propertyPath);
		}
		_pathWorker = new PropertyPathWorker(propertyPath, this, base.IsDynamic, engine);
		_pathWorker.SetTreeContext(base.ParentBindingExpression.TargetElementReference);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private PropertyPath PrepareXmlBinding(PropertyPath path)
	{
		if (path == null)
		{
			DependencyProperty targetProperty = base.TargetProperty;
			Type propertyType = targetProperty.PropertyType;
			string path2 = ((propertyType == typeof(object)) ? ((targetProperty != BindingExpressionBase.NoTargetProperty && targetProperty != Selector.SelectedValueProperty && !(targetProperty.OwnerType == typeof(LiveShapingList))) ? ((targetProperty != FrameworkElement.DataContextProperty && targetProperty != CollectionViewSource.SourceProperty) ? "/" : string.Empty) : "/InnerText") : ((!propertyType.IsAssignableFrom(typeof(XmlDataCollection))) ? "/InnerText" : string.Empty));
			path = new PropertyPath(path2);
		}
		if (path.SVI.Length != 0)
		{
			SetValue(Feature.XmlWorker, new XmlBindingWorker(this, path.SVI[0].drillIn == DrillIn.Never));
		}
		return path;
	}

	internal override void AttachDataItem()
	{
		object obj;
		if (XmlWorker == null)
		{
			obj = base.DataItem;
		}
		else
		{
			XmlWorker.AttachDataItem();
			obj = XmlWorker.RawValue();
		}
		PW.AttachToRootItem(obj);
		if (PW.Length == 0)
		{
			base.ParentBindingExpression.SetupDefaultValueConverter(obj.GetType());
		}
	}

	internal override void DetachDataItem()
	{
		PW.DetachFromRootItem();
		if (XmlWorker != null)
		{
			XmlWorker.DetachDataItem();
		}
		AsyncGetValueRequest asyncGetValueRequest = (AsyncGetValueRequest)GetValue(Feature.PendingGetValueRequest, null);
		if (asyncGetValueRequest != null)
		{
			asyncGetValueRequest.Cancel();
			ClearValue(Feature.PendingGetValueRequest);
		}
		AsyncSetValueRequest asyncSetValueRequest = (AsyncSetValueRequest)GetValue(Feature.PendingSetValueRequest, null);
		if (asyncSetValueRequest != null)
		{
			asyncSetValueRequest.Cancel();
			ClearValue(Feature.PendingSetValueRequest);
		}
	}

	internal override object RawValue()
	{
		object result = PW.RawValue();
		SetStatus(PW.Status);
		return result;
	}

	internal override void RefreshValue()
	{
		PW.RefreshValue();
	}

	internal override void UpdateValue(object value)
	{
		int level = PW.Length - 1;
		object item = PW.GetItem(level);
		if (item != null && item != BindingExpression.NullDataItem)
		{
			if (base.ParentBinding.IsAsync && !(PW.GetAccessor(level) is DependencyProperty))
			{
				RequestAsyncSetValue(item, value);
			}
			else
			{
				PW.SetValue(item, value);
			}
		}
	}

	internal override void OnCurrentChanged(ICollectionView collectionView, EventArgs args)
	{
		if (XmlWorker != null)
		{
			XmlWorker.OnCurrentChanged(collectionView, args);
		}
		PW.OnCurrentChanged(collectionView);
	}

	internal override bool UsesDependencyProperty(DependencyObject d, DependencyProperty dp)
	{
		return PW.UsesDependencyProperty(d, dp);
	}

	internal override void OnSourceInvalidation(DependencyObject d, DependencyProperty dp, bool isASubPropertyChange)
	{
		PW.OnDependencyPropertyChanged(d, dp, isASubPropertyChange);
	}

	internal override bool IsPathCurrent()
	{
		object rootItem = ((XmlWorker == null) ? base.DataItem : XmlWorker.RawValue());
		return PW.IsPathCurrent(rootItem);
	}

	internal void CancelPendingTasks()
	{
		base.ParentBindingExpression.CancelPendingTasks();
	}

	internal bool AsyncGet(object item, int level)
	{
		if (base.ParentBinding.IsAsync)
		{
			RequestAsyncGetValue(item, level);
			return true;
		}
		return false;
	}

	internal void ReplaceCurrentItem(ICollectionView oldCollectionView, ICollectionView newCollectionView)
	{
		if (oldCollectionView != null)
		{
			CurrentChangedEventManager.RemoveHandler(oldCollectionView, base.ParentBindingExpression.OnCurrentChanged);
			if (base.IsReflective)
			{
				CurrentChangingEventManager.RemoveHandler(oldCollectionView, base.ParentBindingExpression.OnCurrentChanging);
			}
		}
		if (newCollectionView != null)
		{
			CurrentChangedEventManager.AddHandler(newCollectionView, base.ParentBindingExpression.OnCurrentChanged);
			if (base.IsReflective)
			{
				CurrentChangingEventManager.AddHandler(newCollectionView, base.ParentBindingExpression.OnCurrentChanging);
			}
		}
	}

	internal void NewValueAvailable(bool dependencySourcesChanged, bool initialValue, bool isASubPropertyChange)
	{
		SetStatus(PW.Status);
		BindingExpression parentBindingExpression = base.ParentBindingExpression;
		parentBindingExpression.BindingGroup?.UpdateTable(parentBindingExpression);
		if (dependencySourcesChanged)
		{
			ReplaceDependencySources();
		}
		if (base.Status != BindingStatusInternal.AsyncRequestPending)
		{
			if (!initialValue)
			{
				parentBindingExpression.ScheduleTransfer(isASubPropertyChange);
			}
			else
			{
				SetTransferIsPending(value: false);
			}
		}
	}

	internal void SetupDefaultValueConverter(Type type)
	{
		base.ParentBindingExpression.SetupDefaultValueConverter(type);
	}

	internal bool IsValidValue(object value)
	{
		return base.TargetProperty.IsValidValue(value);
	}

	internal void OnSourcePropertyChanged(object o, string propName)
	{
		int level;
		if (base.IgnoreSourcePropertyChange || (level = PW.LevelForPropertyChange(o, propName)) < 0)
		{
			return;
		}
		if (base.Dispatcher.Thread == Thread.CurrentThread)
		{
			PW.OnPropertyChangedAtLevel(level);
			return;
		}
		SetTransferIsPending(value: true);
		if (base.ParentBindingExpression.TargetWantsCrossThreadNotifications && base.TargetElement is LiveShapingItem liveShapingItem)
		{
			liveShapingItem.OnCrossThreadPropertyChange(base.TargetProperty);
		}
		base.Engine.Marshal(ScheduleTransferOperation, null);
	}

	internal void OnDataErrorsChanged(INotifyDataErrorInfo indei, string propName)
	{
		if (base.Dispatcher.Thread == Thread.CurrentThread)
		{
			base.ParentBindingExpression.UpdateNotifyDataErrors(indei, propName, DependencyProperty.UnsetValue);
		}
		else if (!base.ParentBindingExpression.IsDataErrorsChangedPending)
		{
			base.ParentBindingExpression.IsDataErrorsChangedPending = true;
			base.Engine.Marshal(delegate(object arg)
			{
				object[] array = (object[])arg;
				base.ParentBindingExpression.UpdateNotifyDataErrors((INotifyDataErrorInfo)array[0], (string)array[1], DependencyProperty.UnsetValue);
				return (object)null;
			}, new object[2] { indei, propName });
		}
	}

	internal void OnXmlValueChanged()
	{
		object item = PW.GetItem(0);
		OnSourcePropertyChanged(item, null);
	}

	internal void UseNewXmlItem(object item)
	{
		PW.DetachFromRootItem();
		PW.AttachToRootItem(item);
		if (base.Status != BindingStatusInternal.AsyncRequestPending)
		{
			base.ParentBindingExpression.ScheduleTransfer(isASubPropertyChange: false);
		}
	}

	internal object GetResultNode()
	{
		return PW.GetItem(0);
	}

	internal DependencyObject CheckTarget()
	{
		return base.TargetElement;
	}

	internal void ReportGetValueError(int k, object item, Exception ex)
	{
		if (TraceData.IsEnabled)
		{
			SourceValueInfo sourceValueInfo = PW.GetSourceValueInfo(k);
			Type type = PW.GetType(k);
			string text = ((k > 0) ? PW.GetSourceValueInfo(k - 1).name : string.Empty);
			TraceData.TraceAndNotify(base.ParentBindingExpression.TraceLevel, TraceData.CannotGetClrRawValue(sourceValueInfo.propertyName, type.Name, text, AvTrace.TypeName(item)), base.ParentBindingExpression, ex);
		}
	}

	internal void ReportSetValueError(int k, object item, object value, Exception ex)
	{
		if (TraceData.IsEnabled)
		{
			SourceValueInfo sourceValueInfo = PW.GetSourceValueInfo(k);
			Type type = PW.GetType(k);
			TraceData.TraceAndNotify(TraceEventType.Error, TraceData.CannotSetClrRawValue(sourceValueInfo.propertyName, type.Name, AvTrace.TypeName(item), AvTrace.ToStringHelper(value), AvTrace.TypeName(value)), base.ParentBindingExpression, ex);
		}
	}

	internal void ReportRawValueErrors(int k, object item, object info)
	{
		if (TraceData.IsEnabled)
		{
			if (item == null)
			{
				TraceData.TraceAndNotify(TraceEventType.Information, TraceData.MissingDataItem, base.ParentBindingExpression);
			}
			if (info == null)
			{
				TraceData.TraceAndNotify(TraceEventType.Information, TraceData.MissingInfo, base.ParentBindingExpression);
			}
			if (item == BindingExpression.NullDataItem)
			{
				TraceData.TraceAndNotify(TraceEventType.Information, TraceData.NullDataItem, base.ParentBindingExpression);
			}
		}
	}

	internal void ReportBadXPath(TraceEventType traceType)
	{
		XmlWorker?.ReportBadXPath(traceType);
	}

	private void SetStatus(PropertyPathStatus status)
	{
		switch (status)
		{
		case PropertyPathStatus.Inactive:
			base.Status = BindingStatusInternal.Inactive;
			break;
		case PropertyPathStatus.Active:
			base.Status = BindingStatusInternal.Active;
			break;
		case PropertyPathStatus.PathError:
			base.Status = BindingStatusInternal.PathError;
			break;
		case PropertyPathStatus.AsyncRequestPending:
			base.Status = BindingStatusInternal.AsyncRequestPending;
			break;
		}
	}

	private void ReplaceDependencySources()
	{
		if (base.ParentBindingExpression.IsDetaching)
		{
			return;
		}
		int num = PW.Length;
		if (PW.NeedsDirectNotification)
		{
			num++;
		}
		WeakDependencySource[] array = new WeakDependencySource[num];
		int n = 0;
		if (base.IsDynamic)
		{
			for (int i = 0; i < PW.Length; i++)
			{
				if (PW.GetAccessor(i) is DependencyProperty dp && PW.GetItem(i) is DependencyObject item)
				{
					array[n++] = new WeakDependencySource(item, dp);
				}
			}
			if (PW.NeedsDirectNotification)
			{
				DependencyObject dependencyObject = PW.RawValue() as Freezable;
				if (dependencyObject != null)
				{
					array[n++] = new WeakDependencySource(dependencyObject, DependencyObject.DirectDependencyProperty);
				}
			}
		}
		base.ParentBindingExpression.ChangeWorkerSources(array, n);
	}

	private void RequestAsyncGetValue(object item, int level)
	{
		string nameFromInfo = GetNameFromInfo(PW.GetAccessor(level));
		Invariant.Assert(nameFromInfo != null, "Async GetValue expects a name");
		((AsyncGetValueRequest)GetValue(Feature.PendingGetValueRequest, null))?.Cancel();
		AsyncGetValueRequest asyncGetValueRequest = new AsyncGetValueRequest(item, nameFromInfo, base.ParentBinding.AsyncState, DoGetValueCallback, CompleteGetValueCallback, this, level);
		SetValue(Feature.PendingGetValueRequest, asyncGetValueRequest);
		base.Engine.AddAsyncRequest(base.TargetElement, asyncGetValueRequest);
	}

	private static object OnGetValueCallback(AsyncDataRequest adr)
	{
		AsyncGetValueRequest asyncGetValueRequest = (AsyncGetValueRequest)adr;
		object value = ((ClrBindingWorker)asyncGetValueRequest.Args[0]).PW.GetValue(asyncGetValueRequest.SourceItem, (int)asyncGetValueRequest.Args[1]);
		if (value == PropertyPathWorker.IListIndexOutOfRange)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return value;
	}

	private static object OnCompleteGetValueCallback(AsyncDataRequest adr)
	{
		AsyncGetValueRequest asyncGetValueRequest = (AsyncGetValueRequest)adr;
		((ClrBindingWorker)asyncGetValueRequest.Args[0]).Engine?.Marshal(CompleteGetValueLocalCallback, asyncGetValueRequest);
		return null;
	}

	private static object OnCompleteGetValueOperation(object arg)
	{
		AsyncGetValueRequest asyncGetValueRequest = (AsyncGetValueRequest)arg;
		((ClrBindingWorker)asyncGetValueRequest.Args[0]).CompleteGetValue(asyncGetValueRequest);
		return null;
	}

	private void CompleteGetValue(AsyncGetValueRequest request)
	{
		if ((AsyncGetValueRequest)GetValue(Feature.PendingGetValueRequest, null) != request)
		{
			return;
		}
		ClearValue(Feature.PendingGetValueRequest);
		int num = (int)request.Args[1];
		if (CheckTarget() == null)
		{
			return;
		}
		switch (request.Status)
		{
		case AsyncRequestStatus.Completed:
			PW.OnNewValue(num, request.Result);
			SetStatus(PW.Status);
			if (num == PW.Length - 1)
			{
				base.ParentBindingExpression.TransferValue(request.Result, isASubPropertyChange: false);
			}
			break;
		case AsyncRequestStatus.Failed:
			ReportGetValueError(num, request.SourceItem, request.Exception);
			PW.OnNewValue(num, DependencyProperty.UnsetValue);
			break;
		}
	}

	private void RequestAsyncSetValue(object item, object value)
	{
		string nameFromInfo = GetNameFromInfo(PW.GetAccessor(PW.Length - 1));
		Invariant.Assert(nameFromInfo != null, "Async SetValue expects a name");
		((AsyncSetValueRequest)GetValue(Feature.PendingSetValueRequest, null))?.Cancel();
		AsyncSetValueRequest asyncSetValueRequest = new AsyncSetValueRequest(item, nameFromInfo, value, base.ParentBinding.AsyncState, DoSetValueCallback, CompleteSetValueCallback, this);
		SetValue(Feature.PendingSetValueRequest, asyncSetValueRequest);
		base.Engine.AddAsyncRequest(base.TargetElement, asyncSetValueRequest);
	}

	private static object OnSetValueCallback(AsyncDataRequest adr)
	{
		AsyncSetValueRequest asyncSetValueRequest = (AsyncSetValueRequest)adr;
		((ClrBindingWorker)asyncSetValueRequest.Args[0]).PW.SetValue(asyncSetValueRequest.TargetItem, asyncSetValueRequest.Value);
		return null;
	}

	private static object OnCompleteSetValueCallback(AsyncDataRequest adr)
	{
		AsyncSetValueRequest asyncSetValueRequest = (AsyncSetValueRequest)adr;
		((ClrBindingWorker)asyncSetValueRequest.Args[0]).Engine?.Marshal(CompleteSetValueLocalCallback, asyncSetValueRequest);
		return null;
	}

	private static object OnCompleteSetValueOperation(object arg)
	{
		AsyncSetValueRequest asyncSetValueRequest = (AsyncSetValueRequest)arg;
		((ClrBindingWorker)asyncSetValueRequest.Args[0]).CompleteSetValue(asyncSetValueRequest);
		return null;
	}

	private void CompleteSetValue(AsyncSetValueRequest request)
	{
		if ((AsyncSetValueRequest)GetValue(Feature.PendingSetValueRequest, null) != request)
		{
			return;
		}
		ClearValue(Feature.PendingSetValueRequest);
		if (CheckTarget() == null)
		{
			return;
		}
		AsyncRequestStatus status = request.Status;
		if (status == AsyncRequestStatus.Completed || status != AsyncRequestStatus.Failed)
		{
			return;
		}
		object obj = base.ParentBinding.DoFilterException(base.ParentBindingExpression, request.Exception);
		if (obj is Exception ex)
		{
			if (TraceData.IsEnabled)
			{
				int k = PW.Length - 1;
				_ = request.Value;
				ReportSetValueError(k, request.TargetItem, request.Value, ex);
			}
		}
		else if (obj is ValidationError validationError)
		{
			Validation.MarkInvalid(base.ParentBindingExpression, validationError);
		}
	}

	private string GetNameFromInfo(object info)
	{
		MemberInfo memberInfo;
		if ((memberInfo = info as MemberInfo) != null)
		{
			return memberInfo.Name;
		}
		if (info is PropertyDescriptor propertyDescriptor)
		{
			return propertyDescriptor.Name;
		}
		if (info is DynamicObjectAccessor dynamicObjectAccessor)
		{
			return dynamicObjectAccessor.PropertyName;
		}
		return null;
	}

	private object ScheduleTransferOperation(object arg)
	{
		PW.RefreshValue();
		return null;
	}
}
