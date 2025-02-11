using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;

namespace MS.Internal.Data;

internal sealed class PropertyPathWorker : IWeakEventListener
{
	private class ContextHelper : IDisposable
	{
		private PropertyPathWorker _owner;

		public ContextHelper(PropertyPathWorker owner)
		{
			_owner = owner;
		}

		public void SetContext(object rootItem)
		{
			_owner.TreeContext = rootItem as DependencyObject;
			_owner.AttachToRootItem(rootItem);
		}

		void IDisposable.Dispose()
		{
			_owner.DetachFromRootItem();
			_owner.TreeContext = null;
			GC.SuppressFinalize(this);
		}
	}

	private class IListIndexerArg
	{
		private int _arg;

		public int Value => _arg;

		public IListIndexerArg(int arg)
		{
			_arg = arg;
		}
	}

	private struct SourceValueState
	{
		public ICollectionView collectionView;

		public object item;

		public object info;

		public Type type;

		public object[] args;
	}

	private static readonly char[] s_comma = new char[1] { ',' };

	private static readonly char[] s_dot = new char[1] { '.' };

	private static readonly object NoParent = new NamedObject("NoParent");

	private static readonly object AsyncRequestPending = new NamedObject("AsyncRequestPending");

	internal static readonly object IListIndexOutOfRange = new NamedObject("IListIndexOutOfRange");

	private static readonly IList<Type> IListIndexerAllowlist = new Type[7]
	{
		typeof(ArrayList),
		typeof(IList),
		typeof(List<>),
		typeof(Collection<>),
		typeof(ReadOnlyCollection<>),
		typeof(StringCollection),
		typeof(LinkTargetCollection)
	};

	private PropertyPath _parent;

	private PropertyPathStatus _status;

	private object _treeContext;

	private object _rootItem;

	private SourceValueState[] _arySVS;

	private ContextHelper _contextHelper;

	private ClrBindingWorker _host;

	private DataBindEngine _engine;

	private bool _dependencySourcesChanged;

	private bool _isDynamic;

	private bool _needsDirectNotification;

	private bool? _isDBNullValidForUpdate;

	internal int Length => _parent.Length;

	internal PropertyPathStatus Status => _status;

	internal DependencyObject TreeContext
	{
		get
		{
			return BindingExpressionBase.GetReference(_treeContext) as DependencyObject;
		}
		set
		{
			_treeContext = BindingExpressionBase.CreateReference(value);
		}
	}

	internal bool IsDBNullValidForUpdate
	{
		get
		{
			if (!_isDBNullValidForUpdate.HasValue)
			{
				DetermineWhetherDBNullIsValid();
			}
			return _isDBNullValidForUpdate.Value;
		}
	}

	internal object SourceItem
	{
		get
		{
			int num = Length - 1;
			object obj = ((num >= 0) ? GetItem(num) : null);
			if (obj == BindingExpression.NullDataItem)
			{
				obj = null;
			}
			return obj;
		}
	}

	internal string SourcePropertyName
	{
		get
		{
			int num = Length - 1;
			if (num < 0)
			{
				return null;
			}
			switch (SVI[num].type)
			{
			case SourceValueType.Property:
			{
				SetPropertyInfo(GetAccessor(num), out var pi, out var pd, out var dp, out var dpa);
				if (dp == null)
				{
					if (!(pi != null))
					{
						if (pd == null)
						{
							return dpa?.PropertyName;
						}
						return pd.Name;
					}
					return pi.Name;
				}
				return dp.Name;
			}
			case SourceValueType.Indexer:
			{
				string path = _parent.Path;
				int startIndex = path.LastIndexOf('[');
				return path.Substring(startIndex);
			}
			default:
				return null;
			}
		}
	}

	internal bool NeedsDirectNotification
	{
		get
		{
			return _needsDirectNotification;
		}
		private set
		{
			if (value)
			{
				_dependencySourcesChanged = true;
			}
			_needsDirectNotification = value;
		}
	}

	private bool IsDynamic => _isDynamic;

	private SourceValueInfo[] SVI => _parent.SVI;

	private DataBindEngine Engine => _engine;

	internal PropertyPathWorker(PropertyPath path)
		: this(path, DataBindEngine.CurrentDataBindEngine)
	{
	}

	internal PropertyPathWorker(PropertyPath path, ClrBindingWorker host, bool isDynamic, DataBindEngine engine)
		: this(path, engine)
	{
		_host = host;
		_isDynamic = isDynamic;
	}

	private PropertyPathWorker(PropertyPath path, DataBindEngine engine)
	{
		_parent = path;
		_arySVS = new SourceValueState[path.Length];
		_engine = engine;
		for (int num = _arySVS.Length - 1; num >= 0; num--)
		{
			_arySVS[num].item = BindingExpressionBase.CreateReference(BindingExpression.NullDataItem);
		}
	}

	internal void SetTreeContext(WeakReference wr)
	{
		_treeContext = BindingExpressionBase.CreateReference(wr);
	}

	internal object GetItem(int level)
	{
		return BindingExpressionBase.GetReference(_arySVS[level].item);
	}

	internal object GetAccessor(int level)
	{
		return _arySVS[level].info;
	}

	internal object[] GetIndexerArguments(int level)
	{
		object[] args = _arySVS[level].args;
		if (args != null && args.Length == 1 && args[0] is IListIndexerArg listIndexerArg)
		{
			return new object[1] { listIndexerArg.Value };
		}
		return args;
	}

	internal Type GetType(int level)
	{
		return _arySVS[level].type;
	}

	internal IDisposable SetContext(object rootItem)
	{
		if (_contextHelper == null)
		{
			_contextHelper = new ContextHelper(this);
		}
		_contextHelper.SetContext(rootItem);
		return _contextHelper;
	}

	internal void AttachToRootItem(object rootItem)
	{
		_rootItem = BindingExpressionBase.CreateReference(rootItem);
		UpdateSourceValueState(-1, null);
	}

	internal void DetachFromRootItem()
	{
		_rootItem = BindingExpression.NullDataItem;
		UpdateSourceValueState(-1, null);
		_rootItem = null;
	}

	internal object GetValue(object item, int level)
	{
		bool flag = IsExtendedTraceEnabled(TraceDataLevel.CreateExpression);
		object obj = DependencyProperty.UnsetValue;
		SetPropertyInfo(_arySVS[level].info, out var pi, out var pd, out var dp, out var dpa);
		switch (SVI[level].type)
		{
		case SourceValueType.Property:
			if (pi != null)
			{
				obj = pi.GetValue(item, null);
			}
			else if (pd != null)
			{
				bool indexerIsNext = level + 1 < SVI.Length && SVI[level + 1].type == SourceValueType.Indexer;
				obj = Engine.GetValue(item, pd, indexerIsNext);
			}
			else if (dp != null)
			{
				DependencyObject dependencyObject = (DependencyObject)item;
				obj = ((level == Length - 1 && _host != null && !_host.TransfersDefaultValue) ? (Helper.HasDefaultValue(dependencyObject, dp) ? BindingExpression.IgnoreDefaultValue : dependencyObject.GetValue(dp)) : dependencyObject.GetValue(dp));
			}
			else if (dpa != null)
			{
				obj = dpa.GetValue(item);
			}
			break;
		case SourceValueType.Indexer:
			if (pi != null)
			{
				object[] args = _arySVS[level].args;
				if (args != null && args.Length == 1 && args[0] is IListIndexerArg { Value: var value })
				{
					IList list = (IList)item;
					obj = ((0 > value || value >= list.Count) ? IListIndexOutOfRange : list[value]);
				}
				else
				{
					obj = pi.GetValue(item, BindingFlags.GetProperty, null, args, CultureInfo.InvariantCulture);
				}
			}
			else
			{
				if (!(_arySVS[level].info is DynamicIndexerAccessor dynamicIndexerAccessor))
				{
					throw new NotSupportedException(SR.IndexedPropDescNotImplemented);
				}
				obj = dynamicIndexerAccessor.GetValue(item, _arySVS[level].args);
			}
			break;
		case SourceValueType.Direct:
			obj = item;
			break;
		}
		if (flag)
		{
			object obj2 = _arySVS[level].info;
			if (obj2 == DependencyProperty.UnsetValue)
			{
				obj2 = null;
			}
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GetValue(TraceData.Identify(_host.ParentBindingExpression), level, TraceData.Identify(item), TraceData.IdentifyAccessor(obj2), TraceData.Identify(obj)), _host.ParentBindingExpression);
		}
		return obj;
	}

	internal void SetValue(object item, object value)
	{
		bool num = IsExtendedTraceEnabled(TraceDataLevel.CreateExpression);
		int num2 = _arySVS.Length - 1;
		SetPropertyInfo(_arySVS[num2].info, out var pi, out var pd, out var dp, out var dpa);
		if (num)
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.SetValue(TraceData.Identify(_host.ParentBindingExpression), num2, TraceData.Identify(item), TraceData.IdentifyAccessor(_arySVS[num2].info), TraceData.Identify(value)), _host.ParentBindingExpression);
		}
		switch (SVI[num2].type)
		{
		case SourceValueType.Property:
			if (pd != null)
			{
				pd.SetValue(item, value);
			}
			else if (pi != null)
			{
				pi.SetValue(item, value, null);
			}
			else if (dp != null)
			{
				((DependencyObject)item).SetValue(dp, value);
			}
			else
			{
				dpa?.SetValue(item, value);
			}
			break;
		case SourceValueType.Indexer:
			if (pi != null)
			{
				pi.SetValue(item, value, BindingFlags.SetProperty, null, GetIndexerArguments(num2), CultureInfo.InvariantCulture);
				break;
			}
			if (_arySVS[num2].info is DynamicIndexerAccessor dynamicIndexerAccessor)
			{
				dynamicIndexerAccessor.SetValue(item, _arySVS[num2].args, value);
				break;
			}
			throw new NotSupportedException(SR.IndexedPropDescNotImplemented);
		}
	}

	internal object RawValue()
	{
		object obj = RawValue(Length - 1);
		if (obj == AsyncRequestPending)
		{
			obj = DependencyProperty.UnsetValue;
		}
		return obj;
	}

	internal void RefreshValue()
	{
		for (int i = 1; i < _arySVS.Length; i++)
		{
			if (!ItemsControl.EqualsEx(BindingExpressionBase.GetReference(_arySVS[i].item), RawValue(i - 1)))
			{
				UpdateSourceValueState(i - 1, null);
				return;
			}
		}
		UpdateSourceValueState(Length - 1, null);
	}

	internal int LevelForPropertyChange(object item, string propertyName)
	{
		bool flag = propertyName == "Item[]";
		for (int i = 0; i < _arySVS.Length; i++)
		{
			object obj = BindingExpressionBase.GetReference(_arySVS[i].item);
			if (obj == BindingExpression.StaticSource)
			{
				obj = null;
			}
			if (obj == item && (string.IsNullOrEmpty(propertyName) || (flag && SVI[i].type == SourceValueType.Indexer) || string.Equals(SVI[i].propertyName, propertyName, StringComparison.OrdinalIgnoreCase)))
			{
				return i;
			}
		}
		return -1;
	}

	internal void OnPropertyChangedAtLevel(int level)
	{
		UpdateSourceValueState(level, null);
	}

	internal void OnCurrentChanged(ICollectionView collectionView)
	{
		for (int i = 0; i < Length; i++)
		{
			if (_arySVS[i].collectionView == collectionView)
			{
				_host.CancelPendingTasks();
				UpdateSourceValueState(i, collectionView);
				break;
			}
		}
	}

	internal bool UsesDependencyProperty(DependencyObject d, DependencyProperty dp)
	{
		if (dp == DependencyObject.DirectDependencyProperty)
		{
			return true;
		}
		for (int i = 0; i < _arySVS.Length; i++)
		{
			if (_arySVS[i].info == dp && BindingExpressionBase.GetReference(_arySVS[i].item) == d)
			{
				return true;
			}
		}
		return false;
	}

	internal void OnDependencyPropertyChanged(DependencyObject d, DependencyProperty dp, bool isASubPropertyChange)
	{
		if (dp == DependencyObject.DirectDependencyProperty)
		{
			UpdateSourceValueState(_arySVS.Length, null, BindingExpression.NullDataItem, isASubPropertyChange);
			return;
		}
		for (int i = 0; i < _arySVS.Length; i++)
		{
			if (_arySVS[i].info == dp && BindingExpressionBase.GetReference(_arySVS[i].item) == d)
			{
				UpdateSourceValueState(i, null, BindingExpression.NullDataItem, isASubPropertyChange);
				break;
			}
		}
	}

	internal void OnNewValue(int level, object value)
	{
		_status = PropertyPathStatus.Active;
		if (level < Length - 1)
		{
			UpdateSourceValueState(level, null, value, isASubPropertyChange: false);
		}
	}

	internal SourceValueInfo GetSourceValueInfo(int level)
	{
		return SVI[level];
	}

	internal static bool IsIndexedProperty(PropertyInfo pi)
	{
		bool result = false;
		try
		{
			result = pi != null && pi.GetIndexParameters().Length != 0;
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalApplicationException(ex))
			{
				throw;
			}
		}
		return result;
	}

	private void UpdateSourceValueState(int k, ICollectionView collectionView)
	{
		UpdateSourceValueState(k, collectionView, BindingExpression.NullDataItem, isASubPropertyChange: false);
	}

	private void UpdateSourceValueState(int k, ICollectionView collectionView, object newValue, bool isASubPropertyChange)
	{
		DependencyObject dependencyObject = null;
		if (_host != null)
		{
			dependencyObject = _host.CheckTarget();
			if (_rootItem != BindingExpression.NullDataItem && dependencyObject == null)
			{
				return;
			}
		}
		int num = k;
		object obj = null;
		bool flag = _host == null || k < 0;
		_status = PropertyPathStatus.Active;
		_dependencySourcesChanged = false;
		if (collectionView != null)
		{
			ReplaceItem(k, collectionView.CurrentItem, NoParent);
		}
		for (k++; k < _arySVS.Length; k++)
		{
			isASubPropertyChange = false;
			ICollectionView collectionView2 = _arySVS[k].collectionView;
			obj = ((newValue == BindingExpression.NullDataItem) ? RawValue(k - 1) : newValue);
			newValue = BindingExpression.NullDataItem;
			if (obj == AsyncRequestPending)
			{
				_status = PropertyPathStatus.AsyncRequestPending;
				break;
			}
			if (!flag && obj == BindingExpressionBase.DisconnectedItem && _arySVS[k - 1].info == FrameworkElement.DataContextProperty)
			{
				flag = true;
			}
			ReplaceItem(k, BindingExpression.NullDataItem, obj);
			ICollectionView collectionView3 = _arySVS[k].collectionView;
			if (collectionView2 != collectionView3 && _host != null)
			{
				_host.ReplaceCurrentItem(collectionView2, collectionView3);
			}
		}
		if (_host != null)
		{
			if (num < _arySVS.Length)
			{
				NeedsDirectNotification = _status == PropertyPathStatus.Active && _arySVS.Length != 0 && SVI[_arySVS.Length - 1].type != SourceValueType.Direct && !(_arySVS[_arySVS.Length - 1].info is DependencyProperty) && typeof(DependencyObject).IsAssignableFrom(_arySVS[_arySVS.Length - 1].type);
			}
			if (!flag && _arySVS.Length != 0 && _arySVS[_arySVS.Length - 1].info == FrameworkElement.DataContextProperty && RawValue() == BindingExpressionBase.DisconnectedItem)
			{
				flag = true;
			}
			_host.NewValueAvailable(_dependencySourcesChanged, flag, isASubPropertyChange);
		}
		GC.KeepAlive(dependencyObject);
	}

	private void ReplaceItem(int k, object newO, object parent)
	{
		bool flag = IsExtendedTraceEnabled(TraceDataLevel.Transfer);
		SourceValueState svs = default(SourceValueState);
		object reference = BindingExpressionBase.GetReference(_arySVS[k].item);
		if (IsDynamic && SVI[k].type != SourceValueType.Direct)
		{
			PropertyPath.DowncastAccessor(_arySVS[k].info, out var dp, out var pi, out var pd, out var _);
			if (reference == BindingExpression.StaticSource)
			{
				Type type = ((pi != null) ? pi.DeclaringType : pd?.ComponentType);
				if (type != null)
				{
					StaticPropertyChangedEventManager.RemoveHandler(type, OnStaticPropertyChanged, SVI[k].propertyName);
				}
			}
			else if (dp != null)
			{
				_dependencySourcesChanged = true;
			}
			else if (reference is INotifyPropertyChanged source)
			{
				PropertyChangedEventManager.RemoveHandler(source, OnPropertyChanged, SVI[k].propertyName);
			}
			else if (pd != null && reference != null)
			{
				ValueChangedEventManager.RemoveHandler(reference, OnValueChanged, pd);
			}
		}
		if (_host != null && k == Length - 1 && IsDynamic && _host.ValidatesOnNotifyDataErrors && reference is INotifyDataErrorInfo source2)
		{
			ErrorsChangedEventManager.RemoveHandler(source2, OnErrorsChanged);
		}
		_isDBNullValidForUpdate = null;
		if (newO == null || parent == DependencyProperty.UnsetValue || parent == BindingExpression.NullDataItem || parent == BindingExpressionBase.DisconnectedItem)
		{
			_arySVS[k].item = BindingExpressionBase.ReplaceReference(_arySVS[k].item, newO);
			if (parent == DependencyProperty.UnsetValue || parent == BindingExpression.NullDataItem || parent == BindingExpressionBase.DisconnectedItem)
			{
				_arySVS[k].collectionView = null;
			}
			if (flag)
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.ReplaceItemShort(TraceData.Identify(_host.ParentBindingExpression), k, TraceData.Identify(newO)), _host.ParentBindingExpression);
			}
			return;
		}
		if (newO != BindingExpression.NullDataItem)
		{
			parent = newO;
			GetInfo(k, newO, ref svs);
			svs.collectionView = _arySVS[k].collectionView;
		}
		else
		{
			DrillIn drillIn = SVI[k].drillIn;
			ICollectionView collectionView = null;
			if (drillIn != DrillIn.Always)
			{
				GetInfo(k, parent, ref svs);
			}
			if (svs.info == null)
			{
				collectionView = CollectionViewSource.GetDefaultCollectionView(parent, TreeContext, (object x) => BindingExpressionBase.GetReference((k == 0) ? _rootItem : _arySVS[k - 1].item));
				if (collectionView != null && drillIn != DrillIn.Always && collectionView != parent)
				{
					GetInfo(k, collectionView, ref svs);
				}
			}
			if (svs.info == null && drillIn != 0 && collectionView != null)
			{
				newO = collectionView.CurrentItem;
				if (newO != null)
				{
					GetInfo(k, newO, ref svs);
					svs.collectionView = collectionView;
				}
				else
				{
					svs = _arySVS[k];
					svs.collectionView = collectionView;
					if (!SystemXmlHelper.IsEmptyXmlDataCollection(parent))
					{
						svs.item = BindingExpressionBase.ReplaceReference(svs.item, BindingExpression.NullDataItem);
						if (svs.info == null)
						{
							svs.info = DependencyProperty.UnsetValue;
						}
					}
				}
			}
		}
		if (svs.info == null)
		{
			svs.item = BindingExpressionBase.ReplaceReference(svs.item, BindingExpression.NullDataItem);
			_arySVS[k] = svs;
			_status = PropertyPathStatus.PathError;
			ReportNoInfoError(k, parent);
			return;
		}
		_arySVS[k] = svs;
		newO = BindingExpressionBase.GetReference(svs.item);
		if (flag)
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.ReplaceItemLong(TraceData.Identify(_host.ParentBindingExpression), k, TraceData.Identify(newO), TraceData.IdentifyAccessor(svs.info)), _host.ParentBindingExpression);
		}
		if (IsDynamic && SVI[k].type != SourceValueType.Direct)
		{
			Engine.RegisterForCacheChanges(newO, svs.info);
			PropertyPath.DowncastAccessor(svs.info, out var dp2, out var pi2, out var pd2, out var _);
			if (newO == BindingExpression.StaticSource)
			{
				Type type2 = ((pi2 != null) ? pi2.DeclaringType : pd2?.ComponentType);
				if (type2 != null)
				{
					StaticPropertyChangedEventManager.AddHandler(type2, OnStaticPropertyChanged, SVI[k].propertyName);
				}
			}
			else if (dp2 != null)
			{
				_dependencySourcesChanged = true;
			}
			else if (newO is INotifyPropertyChanged source3)
			{
				PropertyChangedEventManager.AddHandler(source3, OnPropertyChanged, SVI[k].propertyName);
			}
			else if (pd2 != null && newO != null)
			{
				ValueChangedEventManager.AddHandler(newO, OnValueChanged, pd2);
			}
		}
		if (_host == null || k != Length - 1)
		{
			return;
		}
		_host.SetupDefaultValueConverter(svs.type);
		if (_host.IsReflective)
		{
			CheckReadOnly(newO, svs.info);
		}
		if (_host.ValidatesOnNotifyDataErrors && newO is INotifyDataErrorInfo notifyDataErrorInfo)
		{
			if (IsDynamic)
			{
				ErrorsChangedEventManager.AddHandler(notifyDataErrorInfo, OnErrorsChanged);
			}
			_host.OnDataErrorsChanged(notifyDataErrorInfo, SourcePropertyName);
		}
	}

	private void ReportNoInfoError(int k, object parent)
	{
		if (!TraceData.IsEnabled)
		{
			return;
		}
		BindingExpression bindingExpression = ((_host != null) ? _host.ParentBindingExpression : null);
		if (bindingExpression != null && bindingExpression.IsInPriorityBindingExpression)
		{
			return;
		}
		if (!SystemXmlHelper.IsEmptyXmlDataCollection(parent))
		{
			SourceValueInfo sourceValueInfo = SVI[k];
			bool flag = sourceValueInfo.drillIn == DrillIn.Always;
			string text = ((sourceValueInfo.type != SourceValueType.Indexer) ? sourceValueInfo.name : ("[" + sourceValueInfo.name + "]"));
			string text2 = TraceData.DescribeSourceObject(parent);
			string text3 = (flag ? "current item of collection" : "object");
			if (parent == null)
			{
				TraceData.TraceAndNotify(TraceEventType.Information, TraceData.NullItem(text, text3), bindingExpression);
				return;
			}
			if (parent == CollectionView.NewItemPlaceholder || parent == DataGrid.NewItemPlaceholder)
			{
				TraceData.TraceAndNotify(TraceEventType.Information, TraceData.PlaceholderItem(text, text3), bindingExpression);
				return;
			}
			TraceData.TraceAndNotify(bindingExpression?.TraceLevel ?? TraceEventType.Error, TraceData.ClrReplaceItem(text, text2, text3), bindingExpression, new object[1] { bindingExpression }, new object[3] { text, parent, flag });
		}
		else
		{
			TraceEventType traceType = bindingExpression?.TraceLevel ?? TraceEventType.Error;
			_host.ReportBadXPath(traceType);
		}
	}

	internal bool IsPathCurrent(object rootItem)
	{
		if (Status != PropertyPathStatus.Active)
		{
			return false;
		}
		object obj = rootItem;
		int i = 0;
		for (int length = Length; i < length; i++)
		{
			ICollectionView collectionView = _arySVS[i].collectionView;
			if (collectionView != null)
			{
				obj = collectionView.CurrentItem;
			}
			if (PropertyPath.IsStaticProperty(_arySVS[i].info))
			{
				obj = BindingExpression.StaticSource;
			}
			if (!ItemsControl.EqualsEx(obj, BindingExpressionBase.GetReference(_arySVS[i].item)) && !IsNonIdempotentProperty(i - 1))
			{
				return false;
			}
			if (i < length - 1)
			{
				obj = GetValue(obj, i);
			}
		}
		return true;
	}

	private bool IsNonIdempotentProperty(int level)
	{
		if (level < 0 || !(_arySVS[level].info is PropertyDescriptor pd))
		{
			return false;
		}
		return SystemXmlLinqHelper.IsXLinqNonIdempotentProperty(pd);
	}

	private void GetInfo(int k, object item, ref SourceValueState svs)
	{
		object reference = BindingExpressionBase.GetReference(_arySVS[k].item);
		bool flag = IsExtendedTraceEnabled(TraceDataLevel.Transfer);
		Type reflectionType = ReflectionHelper.GetReflectionType(reference);
		Type reflectionType2 = ReflectionHelper.GetReflectionType(item);
		Type type = null;
		if (reflectionType2 == reflectionType && reference != BindingExpression.NullDataItem && !(_arySVS[k].info is PropertyDescriptor))
		{
			svs = _arySVS[k];
			svs.item = BindingExpressionBase.ReplaceReference(svs.item, item);
			if (flag)
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GetInfo_Reuse(TraceData.Identify(_host.ParentBindingExpression), k, TraceData.IdentifyAccessor(svs.info)), _host.ParentBindingExpression);
			}
			return;
		}
		if (reflectionType2 == null && SVI[k].type != SourceValueType.Direct)
		{
			svs.info = null;
			svs.args = null;
			svs.type = null;
			svs.item = BindingExpressionBase.ReplaceReference(svs.item, item);
			if (flag)
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GetInfo_Null(TraceData.Identify(_host.ParentBindingExpression), k), _host.ParentBindingExpression);
			}
			return;
		}
		int index;
		bool flag2 = !PropertyPath.IsParameterIndex(SVI[k].name, out index);
		if (flag2)
		{
			AccessorInfo accessorInfo = Engine.AccessorTable[SVI[k].type, reflectionType2, SVI[k].name];
			if (accessorInfo != null)
			{
				svs.info = accessorInfo.Accessor;
				svs.type = accessorInfo.PropertyType;
				svs.args = accessorInfo.Args;
				if (PropertyPath.IsStaticProperty(svs.info))
				{
					item = BindingExpression.StaticSource;
				}
				svs.item = BindingExpressionBase.ReplaceReference(svs.item, item);
				if (IsDynamic && SVI[k].type == SourceValueType.Property && svs.info is DependencyProperty)
				{
					_dependencySourcesChanged = true;
				}
				if (flag)
				{
					TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GetInfo_Cache(TraceData.Identify(_host.ParentBindingExpression), k, reflectionType2.Name, SVI[k].name, TraceData.IdentifyAccessor(svs.info)), _host.ParentBindingExpression);
				}
				return;
			}
		}
		object obj = null;
		object[] array = null;
		switch (SVI[k].type)
		{
		case SourceValueType.Property:
		{
			obj = _parent.ResolvePropertyName(k, item, reflectionType2, TreeContext);
			if (flag)
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GetInfo_Property(TraceData.Identify(_host.ParentBindingExpression), k, reflectionType2.Name, SVI[k].name, TraceData.IdentifyAccessor(obj)), _host.ParentBindingExpression);
			}
			PropertyPath.DowncastAccessor(obj, out var dp, out var pi, out var pd, out var doa);
			if (dp != null)
			{
				type = dp.PropertyType;
				if (IsDynamic)
				{
					_dependencySourcesChanged = true;
				}
			}
			else if (pi != null)
			{
				type = pi.PropertyType;
			}
			else if (pd != null)
			{
				type = pd.PropertyType;
			}
			else if (doa != null)
			{
				type = doa.PropertyType;
			}
			break;
		}
		case SourceValueType.Indexer:
		{
			IndexerParameterInfo[] array2 = _parent.ResolveIndexerParams(k, TreeContext);
			if (array2.Length == 1 && (array2[0].type == null || array2[0].type == typeof(string)))
			{
				string name = (string)array2[0].value;
				if (ShouldConvertIndexerToProperty(item, ref name))
				{
					_parent.ReplaceIndexerByProperty(k, name);
					goto case SourceValueType.Property;
				}
			}
			array = new object[array2.Length];
			MemberInfo[][] array3 = new MemberInfo[2][]
			{
				GetIndexers(reflectionType2, k),
				null
			};
			bool flag3 = item is IList;
			if (flag3)
			{
				array3[1] = typeof(IList).GetDefaultMembers();
			}
			int num = 0;
			while (obj == null && num < array3.Length)
			{
				if (array3[num] != null)
				{
					MemberInfo[] array4 = array3[num];
					for (int i = 0; i < array4.Length; i++)
					{
						PropertyInfo propertyInfo = array4[i] as PropertyInfo;
						if (propertyInfo != null && MatchIndexerParameters(propertyInfo, array2, array, flag3))
						{
							obj = propertyInfo;
							type = reflectionType2.GetElementType();
							if (type == null)
							{
								type = propertyInfo.PropertyType;
							}
							break;
						}
					}
				}
				num++;
			}
			if (obj == null && SystemCoreHelper.IsIDynamicMetaObjectProvider(item) && MatchIndexerParameters(null, array2, array, isIList: false))
			{
				obj = SystemCoreHelper.GetIndexerAccessor(array.Length);
				type = typeof(object);
			}
			if (flag)
			{
				TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GetInfo_Indexer(TraceData.Identify(_host.ParentBindingExpression), k, reflectionType2.Name, SVI[k].name, TraceData.IdentifyAccessor(obj)), _host.ParentBindingExpression);
			}
			break;
		}
		case SourceValueType.Direct:
			if (!(item is ICollectionView) || _host == null || _host.IsValidValue(item))
			{
				obj = DependencyProperty.UnsetValue;
				type = reflectionType2;
				if (Length == 1 && item is Freezable && item != TreeContext)
				{
					obj = DependencyObject.DirectDependencyProperty;
					_dependencySourcesChanged = true;
				}
			}
			break;
		}
		if (PropertyPath.IsStaticProperty(obj))
		{
			item = BindingExpression.StaticSource;
		}
		svs.info = obj;
		svs.args = array;
		svs.type = type;
		svs.item = BindingExpressionBase.ReplaceReference(svs.item, item);
		if (flag2 && obj != null && !(obj is PropertyDescriptor))
		{
			Engine.AccessorTable[SVI[k].type, reflectionType2, SVI[k].name] = new AccessorInfo(obj, type, array);
		}
	}

	private MemberInfo[] GetIndexers(Type type, int k)
	{
		if (k > 0 && _arySVS[k - 1].info == IndexerPropertyInfo.Instance)
		{
			List<MemberInfo> list = new List<MemberInfo>();
			string name = SVI[k - 1].name;
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.Name == name && IsIndexedProperty(propertyInfo))
				{
					list.Add(propertyInfo);
				}
			}
			return list.ToArray();
		}
		return type.GetDefaultMembers();
	}

	private bool MatchIndexerParameters(PropertyInfo pi, IndexerParameterInfo[] aryInfo, object[] args, bool isIList)
	{
		ParameterInfo[] array = pi?.GetIndexParameters();
		if (array != null && array.Length != aryInfo.Length)
		{
			return false;
		}
		for (int i = 0; i < args.Length; i++)
		{
			IndexerParameterInfo indexerParameterInfo = aryInfo[i];
			Type type = ((array != null) ? array[i].ParameterType : typeof(object));
			if (indexerParameterInfo.type != null)
			{
				if (type.IsAssignableFrom(indexerParameterInfo.type))
				{
					args.SetValue(indexerParameterInfo.value, i);
					continue;
				}
				return false;
			}
			try
			{
				object obj = null;
				if (type == typeof(int))
				{
					if (int.TryParse((string)indexerParameterInfo.value, NumberStyles.Integer, TypeConverterHelper.InvariantEnglishUS.NumberFormat, out var result))
					{
						obj = result;
					}
				}
				else
				{
					TypeConverter converter = TypeDescriptor.GetConverter(type);
					if (converter != null && converter.CanConvertFrom(typeof(string)))
					{
						obj = converter.ConvertFromString(null, TypeConverterHelper.InvariantEnglishUS, (string)indexerParameterInfo.value);
					}
				}
				if (obj == null && type.IsAssignableFrom(typeof(string)))
				{
					obj = indexerParameterInfo.value;
				}
				if (obj != null)
				{
					args.SetValue(obj, i);
					continue;
				}
				return false;
			}
			catch (Exception ex)
			{
				if (CriticalExceptions.IsCriticalApplicationException(ex))
				{
					throw;
				}
				return false;
			}
			catch
			{
				return false;
			}
		}
		if (isIList && array.Length == 1 && array[0].ParameterType == typeof(int))
		{
			bool flag = true;
			if (!FrameworkAppContextSwitches.IListIndexerHidesCustomIndexer)
			{
				Type type2 = pi.DeclaringType;
				if (type2.IsGenericType)
				{
					type2 = type2.GetGenericTypeDefinition();
				}
				flag = IListIndexerAllowlist.Contains(type2);
			}
			if (flag)
			{
				args[0] = new IListIndexerArg((int)args[0]);
			}
		}
		return true;
	}

	private bool ShouldConvertIndexerToProperty(object item, ref string name)
	{
		if (SystemDataHelper.IsDataRowView(item))
		{
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(item);
			if (properties[name] != null)
			{
				return true;
			}
			if (int.TryParse(name, NumberStyles.Integer, TypeConverterHelper.InvariantEnglishUS.NumberFormat, out var result) && 0 <= result && result < properties.Count)
			{
				name = properties[result].Name;
				return true;
			}
		}
		return false;
	}

	private object RawValue(int k)
	{
		if (k < 0)
		{
			return BindingExpressionBase.GetReference(_rootItem);
		}
		if (k >= _arySVS.Length)
		{
			return DependencyProperty.UnsetValue;
		}
		object reference = BindingExpressionBase.GetReference(_arySVS[k].item);
		object info = _arySVS[k].info;
		if (reference != BindingExpression.NullDataItem && info != null && (reference != null || info == DependencyProperty.UnsetValue))
		{
			object obj = DependencyProperty.UnsetValue;
			if (!(info is DependencyProperty) && SVI[k].type != SourceValueType.Direct && _host != null && _host.AsyncGet(reference, k))
			{
				_status = PropertyPathStatus.AsyncRequestPending;
				return AsyncRequestPending;
			}
			try
			{
				obj = GetValue(reference, k);
			}
			catch (Exception ex)
			{
				if (CriticalExceptions.IsCriticalApplicationException(ex))
				{
					throw;
				}
				BindingOperations.LogException(ex);
				if (_host != null)
				{
					_host.ReportGetValueError(k, reference, ex);
				}
			}
			catch
			{
				if (_host != null)
				{
					_host.ReportGetValueError(k, reference, new InvalidOperationException(SR.Format(SR.NonCLSException, "GetValue")));
				}
			}
			if (obj == IListIndexOutOfRange)
			{
				obj = DependencyProperty.UnsetValue;
				if (_host != null)
				{
					_host.ReportGetValueError(k, reference, new ArgumentOutOfRangeException("index"));
				}
			}
			return obj;
		}
		if (_host != null)
		{
			_host.ReportRawValueErrors(k, reference, info);
		}
		return DependencyProperty.UnsetValue;
	}

	private void SetPropertyInfo(object info, out PropertyInfo pi, out PropertyDescriptor pd, out DependencyProperty dp, out DynamicPropertyAccessor dpa)
	{
		pi = null;
		pd = null;
		dpa = null;
		dp = info as DependencyProperty;
		if (dp != null)
		{
			return;
		}
		pi = info as PropertyInfo;
		if (pi == null)
		{
			pd = info as PropertyDescriptor;
			if (pd == null)
			{
				dpa = info as DynamicPropertyAccessor;
			}
		}
	}

	private void CheckReadOnly(object item, object info)
	{
		SetPropertyInfo(info, out var pi, out var pd, out var dp, out var dpa);
		if (pi != null)
		{
			if (IsPropertyReadOnly(item, pi))
			{
				throw new InvalidOperationException(SR.Format(SR.CannotWriteToReadOnly, item.GetType(), pi.Name));
			}
		}
		else if (pd != null)
		{
			if (pd.IsReadOnly)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotWriteToReadOnly, item.GetType(), pd.Name));
			}
		}
		else if (dp != null)
		{
			if (dp.ReadOnly)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotWriteToReadOnly, item.GetType(), dp.Name));
			}
		}
		else if (dpa != null && dpa.IsReadOnly)
		{
			throw new InvalidOperationException(SR.Format(SR.CannotWriteToReadOnly, item.GetType(), dpa.PropertyName));
		}
	}

	private bool IsPropertyReadOnly(object item, PropertyInfo pi)
	{
		if (!pi.CanWrite)
		{
			return true;
		}
		MethodInfo methodInfo = null;
		try
		{
			methodInfo = pi.GetSetMethod(nonPublic: true);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalApplicationException(ex))
			{
				throw;
			}
		}
		if (methodInfo == null || methodInfo.IsPublic)
		{
			return false;
		}
		return true;
	}

	private void DetermineWhetherDBNullIsValid()
	{
		bool value = false;
		object item = GetItem(Length - 1);
		if (item != null && AssemblyHelper.IsLoaded(UncommonAssembly.System_Data_Common))
		{
			value = DetermineWhetherDBNullIsValid(item);
		}
		_isDBNullValidForUpdate = value;
	}

	private bool DetermineWhetherDBNullIsValid(object item)
	{
		SetPropertyInfo(_arySVS[Length - 1].info, out var pi, out var pd, out var _, out var _);
		string text = ((pd != null) ? pd.Name : ((pi != null) ? pi.Name : null));
		object arg = ((text == "Item" && pi != null) ? _arySVS[Length - 1].args[0] : null);
		return SystemDataHelper.DetermineWhetherDBNullIsValid(item, text, arg);
	}

	bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return false;
	}

	private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (IsExtendedTraceEnabled(TraceDataLevel.Transfer))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GotEvent(TraceData.Identify(_host.ParentBindingExpression), "PropertyChanged", TraceData.Identify(sender)), _host.ParentBindingExpression);
		}
		_host.OnSourcePropertyChanged(sender, e.PropertyName);
	}

	private void OnValueChanged(object sender, ValueChangedEventArgs e)
	{
		if (IsExtendedTraceEnabled(TraceDataLevel.Transfer))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GotEvent(TraceData.Identify(_host.ParentBindingExpression), "ValueChanged", TraceData.Identify(sender)), _host.ParentBindingExpression);
		}
		_host.OnSourcePropertyChanged(sender, e.PropertyDescriptor.Name);
	}

	private void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e)
	{
		if (e.PropertyName == SourcePropertyName)
		{
			_host.OnDataErrorsChanged((INotifyDataErrorInfo)sender, e.PropertyName);
		}
	}

	private void OnStaticPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (IsExtendedTraceEnabled(TraceDataLevel.Transfer))
		{
			TraceData.TraceAndNotifyWithNoParameters(TraceEventType.Warning, TraceData.GotEvent(TraceData.Identify(_host.ParentBindingExpression), "PropertyChanged", "(static)"), _host.ParentBindingExpression);
		}
		_host.OnSourcePropertyChanged(sender, e.PropertyName);
	}

	private bool IsExtendedTraceEnabled(TraceDataLevel level)
	{
		if (_host != null)
		{
			return TraceData.IsExtendedTraceEnabled(_host.ParentBindingExpression, level);
		}
		return false;
	}
}
