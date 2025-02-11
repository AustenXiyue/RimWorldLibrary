using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace MS.Internal.Data;

internal abstract class BindingWorker
{
	internal enum Feature
	{
		XmlWorker,
		PendingGetValueRequest,
		PendingSetValueRequest,
		LastFeatureId
	}

	private BindingExpression _bindingExpression;

	private UncommonValueTable _values;

	internal virtual Type SourcePropertyType => null;

	internal virtual bool CanUpdate => false;

	internal BindingExpression ParentBindingExpression => _bindingExpression;

	internal Type TargetPropertyType => TargetProperty.PropertyType;

	internal virtual bool IsDBNullValidForUpdate => false;

	internal virtual object SourceItem => null;

	internal virtual string SourcePropertyName => null;

	protected Binding ParentBinding => ParentBindingExpression.ParentBinding;

	protected bool IsDynamic => ParentBindingExpression.IsDynamic;

	internal bool IsReflective => ParentBindingExpression.IsReflective;

	protected bool IgnoreSourcePropertyChange => ParentBindingExpression.IgnoreSourcePropertyChange;

	protected object DataItem => ParentBindingExpression.DataItem;

	protected DependencyObject TargetElement => ParentBindingExpression.TargetElement;

	protected DependencyProperty TargetProperty => ParentBindingExpression.TargetProperty;

	protected DataBindEngine Engine => ParentBindingExpression.Engine;

	protected Dispatcher Dispatcher => ParentBindingExpression.Dispatcher;

	protected BindingStatusInternal Status
	{
		get
		{
			return ParentBindingExpression.StatusInternal;
		}
		set
		{
			ParentBindingExpression.SetStatus(value);
		}
	}

	static BindingWorker()
	{
	}

	protected BindingWorker(BindingExpression b)
	{
		_bindingExpression = b;
	}

	internal virtual void AttachDataItem()
	{
	}

	internal virtual void DetachDataItem()
	{
	}

	internal virtual void OnCurrentChanged(ICollectionView collectionView, EventArgs args)
	{
	}

	internal virtual object RawValue()
	{
		return null;
	}

	internal virtual void UpdateValue(object value)
	{
	}

	internal virtual void RefreshValue()
	{
	}

	internal virtual bool UsesDependencyProperty(DependencyObject d, DependencyProperty dp)
	{
		return false;
	}

	internal virtual void OnSourceInvalidation(DependencyObject d, DependencyProperty dp, bool isASubPropertyChange)
	{
	}

	internal virtual bool IsPathCurrent()
	{
		return true;
	}

	protected void SetTransferIsPending(bool value)
	{
		ParentBindingExpression.IsTransferPending = value;
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
