using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.Data;

namespace System.Windows.Data;

/// <summary>Contains a collection of bindings and <see cref="T:System.Windows.Controls.ValidationRule" /> objects that are used to validate an object.</summary>
public class BindingGroup : DependencyObject
{
	private class GetValueTable
	{
		private Collection<GetValueTableEntry> _table = new Collection<GetValueTableEntry>();

		public GetValueTableEntry this[object item, string propertyName]
		{
			get
			{
				for (int num = _table.Count - 1; num >= 0; num--)
				{
					GetValueTableEntry getValueTableEntry = _table[num];
					if (propertyName == getValueTableEntry.PropertyName && ItemsControl.EqualsEx(item, getValueTableEntry.Item))
					{
						return getValueTableEntry;
					}
				}
				return null;
			}
		}

		public GetValueTableEntry this[BindingExpressionBase bindingExpressionBase]
		{
			get
			{
				for (int num = _table.Count - 1; num >= 0; num--)
				{
					GetValueTableEntry getValueTableEntry = _table[num];
					if (bindingExpressionBase == getValueTableEntry.BindingExpressionBase)
					{
						return getValueTableEntry;
					}
				}
				return null;
			}
		}

		public void EnsureEntry(BindingExpressionBase bindingExpressionBase)
		{
			if (this[bindingExpressionBase] == null)
			{
				_table.Add(new GetValueTableEntry(bindingExpressionBase));
			}
		}

		public bool Update(BindingExpression bindingExpression)
		{
			GetValueTableEntry getValueTableEntry = this[bindingExpression];
			bool num = getValueTableEntry == null;
			if (num)
			{
				_table.Add(new GetValueTableEntry(bindingExpression));
				return num;
			}
			getValueTableEntry.Update(bindingExpression);
			return num;
		}

		public List<BindingExpressionBase> RemoveRootBinding(BindingExpressionBase rootBindingExpression)
		{
			List<BindingExpressionBase> list = new List<BindingExpressionBase>();
			for (int num = _table.Count - 1; num >= 0; num--)
			{
				BindingExpressionBase bindingExpressionBase = _table[num].BindingExpressionBase;
				if (bindingExpressionBase.RootBindingExpression == rootBindingExpression)
				{
					list.Add(bindingExpressionBase);
					_table.RemoveAt(num);
				}
			}
			return list;
		}

		public void AddUniqueItems(IList<WeakReference> list)
		{
			for (int num = _table.Count - 1; num >= 0; num--)
			{
				if (_table[num].BindingExpressionBase.StatusInternal != BindingStatusInternal.PathError)
				{
					WeakReference itemReference = _table[num].ItemReference;
					if (itemReference != null && FindIndexOf(itemReference, list) < 0)
					{
						list.Add(itemReference);
					}
				}
			}
		}

		public object GetValue(BindingExpressionBase bindingExpressionBase)
		{
			GetValueTableEntry getValueTableEntry = this[bindingExpressionBase];
			if (getValueTableEntry == null)
			{
				return DependencyProperty.UnsetValue;
			}
			return getValueTableEntry.Value;
		}

		public void SetValue(BindingExpressionBase bindingExpressionBase, object value)
		{
			GetValueTableEntry getValueTableEntry = this[bindingExpressionBase];
			if (getValueTableEntry != null)
			{
				getValueTableEntry.Value = value;
			}
		}

		public void ResetValues()
		{
			for (int num = _table.Count - 1; num >= 0; num--)
			{
				_table[num].Value = DeferredTargetValue;
			}
		}

		public void UseSourceValue(BindingExpressionBase rootBindingExpression)
		{
			for (int num = _table.Count - 1; num >= 0; num--)
			{
				if (_table[num].BindingExpressionBase.RootBindingExpression == rootBindingExpression)
				{
					_table[num].Value = DeferredSourceValue;
				}
			}
		}

		public GetValueTableEntry GetFirstEntry()
		{
			if (_table.Count <= 0)
			{
				return null;
			}
			return _table[0];
		}
	}

	private class GetValueTableEntry
	{
		private BindingExpressionBase _bindingExpressionBase;

		private WeakReference _itemWR;

		private string _propertyName;

		private object _value = DeferredTargetValue;

		public object Item => _itemWR.Target;

		public WeakReference ItemReference => _itemWR;

		public string PropertyName => _propertyName;

		public BindingExpressionBase BindingExpressionBase => _bindingExpressionBase;

		public object Value
		{
			get
			{
				if (_value == DeferredTargetValue)
				{
					_value = _bindingExpressionBase.RootBindingExpression.GetRawProposedValue();
				}
				else if (_value == DeferredSourceValue)
				{
					BindingExpression bindingExpression = _bindingExpressionBase as BindingExpression;
					_value = ((bindingExpression != null) ? bindingExpression.SourceValue : DependencyProperty.UnsetValue);
				}
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public GetValueTableEntry(BindingExpressionBase bindingExpressionBase)
		{
			_bindingExpressionBase = bindingExpressionBase;
		}

		public void Update(BindingExpression bindingExpression)
		{
			object sourceItem = bindingExpression.SourceItem;
			if (sourceItem == null)
			{
				_itemWR = null;
			}
			else if (_itemWR == null)
			{
				_itemWR = new WeakReference(sourceItem);
			}
			else
			{
				_itemWR.Target = bindingExpression.SourceItem;
			}
			_propertyName = bindingExpression.SourcePropertyName;
		}
	}

	private class ProposedValueTable
	{
		private Collection<ProposedValueEntry> _table = new Collection<ProposedValueEntry>();

		public int Count => _table.Count;

		public ProposedValueEntry this[object item, string propertyName]
		{
			get
			{
				int num = IndexOf(item, propertyName);
				if (num >= 0)
				{
					return _table[num];
				}
				return null;
			}
		}

		public ProposedValueEntry this[int index] => _table[index];

		public ProposedValueEntry this[BindingExpression bindExpr] => this[bindExpr.SourceItem, bindExpr.SourcePropertyName];

		public void Add(BindingExpressionBase.ProposedValue proposedValue)
		{
			BindingExpression bindingExpression = proposedValue.BindingExpression;
			object sourceItem = bindingExpression.SourceItem;
			string sourcePropertyName = bindingExpression.SourcePropertyName;
			object rawValue = proposedValue.RawValue;
			object convertedValue = proposedValue.ConvertedValue;
			Remove(sourceItem, sourcePropertyName);
			_table.Add(new ProposedValueEntry(sourceItem, sourcePropertyName, rawValue, convertedValue, bindingExpression));
		}

		public void Remove(object item, string propertyName)
		{
			int num = IndexOf(item, propertyName);
			if (num >= 0)
			{
				_table.RemoveAt(num);
			}
		}

		public void Remove(BindingExpression bindExpr)
		{
			if (_table.Count > 0)
			{
				Remove(bindExpr.SourceItem, bindExpr.SourcePropertyName);
			}
		}

		public void Remove(ProposedValueEntry entry)
		{
			_table.Remove(entry);
		}

		public void Clear()
		{
			_table.Clear();
		}

		public void AddUniqueItems(IList<WeakReference> list)
		{
			for (int num = _table.Count - 1; num >= 0; num--)
			{
				WeakReference itemReference = _table[num].ItemReference;
				if (itemReference != null && FindIndexOf(itemReference, list) < 0)
				{
					list.Add(itemReference);
				}
			}
		}

		public void UpdateDependents()
		{
			for (int num = _table.Count - 1; num >= 0; num--)
			{
				Collection<BindingExpressionBase> dependents = _table[num].Dependents;
				if (dependents != null)
				{
					for (int num2 = dependents.Count - 1; num2 >= 0; num2--)
					{
						if (!dependents[num2].IsDetached)
						{
							dependents[num2].UpdateTarget();
						}
					}
				}
			}
		}

		public bool HasValidationError(ValidationError validationError)
		{
			for (int num = _table.Count - 1; num >= 0; num--)
			{
				if (validationError == _table[num].ValidationError)
				{
					return true;
				}
			}
			return false;
		}

		private int IndexOf(object item, string propertyName)
		{
			for (int num = _table.Count - 1; num >= 0; num--)
			{
				ProposedValueEntry proposedValueEntry = _table[num];
				if (propertyName == proposedValueEntry.PropertyName && ItemsControl.EqualsEx(item, proposedValueEntry.Item))
				{
					return num;
				}
			}
			return -1;
		}
	}

	internal class ProposedValueEntry
	{
		private WeakReference _itemReference;

		private string _propertyName;

		private object _rawValue;

		private object _convertedValue;

		private ValidationError _error;

		private Binding _binding;

		private Collection<BindingExpressionBase> _dependents;

		public object Item => _itemReference.Target;

		public string PropertyName => _propertyName;

		public object RawValue => _rawValue;

		public object ConvertedValue => _convertedValue;

		public ValidationError ValidationError => _error;

		public Binding Binding => _binding;

		public WeakReference ItemReference => _itemReference;

		public Collection<BindingExpressionBase> Dependents => _dependents;

		public ProposedValueEntry(object item, string propertyName, object rawValue, object convertedValue, BindingExpression bindExpr)
		{
			_itemReference = new WeakReference(item);
			_propertyName = propertyName;
			_rawValue = rawValue;
			_convertedValue = convertedValue;
			_error = bindExpr.ValidationError;
			_binding = bindExpr.ParentBinding;
		}

		public void AddDependent(BindingExpressionBase dependent)
		{
			if (_dependents == null)
			{
				_dependents = new Collection<BindingExpressionBase>();
			}
			_dependents.Add(dependent);
		}
	}

	private class BindingExpressionCollection : ObservableCollection<BindingExpressionBase>
	{
		protected override void InsertItem(int index, BindingExpressionBase item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			base.InsertItem(index, item);
		}

		protected override void SetItem(int index, BindingExpressionBase item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			base.SetItem(index, item);
		}
	}

	private ValidationRuleCollection _validationRules;

	private string _name;

	private bool _notifyOnValidationError;

	private bool _sharesProposedValues;

	private bool _validatesOnNotifyDataError = true;

	private DataBindEngine _engine;

	private BindingExpressionCollection _bindingExpressions;

	private bool _isItemsValid;

	private ValidationStep _validationStep = (ValidationStep)(-1);

	private GetValueTable _getValueTable = new GetValueTable();

	private ProposedValueTable _proposedValueTable = new ProposedValueTable();

	private BindingExpression[] _proposedValueBindingExpressions;

	private Collection<WeakReference> _itemsRW;

	private WeakReadOnlyCollection<object> _items;

	private CultureInfo _culture;

	private Dictionary<WeakReference, List<ValidationError>> _notifyDataErrors = new Dictionary<WeakReference, List<ValidationError>>();

	internal static readonly object DeferredTargetValue = new NamedObject("DeferredTargetValue");

	internal static readonly object DeferredSourceValue = new NamedObject("DeferredSourceValue");

	private static WeakReference<DependencyObject> NullInheritanceContext = new WeakReference<DependencyObject>(null);

	private WeakReference<DependencyObject> _inheritanceContext = NullInheritanceContext;

	private bool _hasMultipleInheritanceContexts;

	/// <summary>Gets the object that this <see cref="T:System.Windows.Data.BindingGroup" /> is assigned to.</summary>
	/// <returns>The object that this <see cref="T:System.Windows.Data.BindingGroup" /> is assigned to.</returns>
	public DependencyObject Owner => InheritanceContext;

	/// <summary>Gets a collection of <see cref="T:System.Windows.Controls.ValidationRule" /> objects that validate the source objects in the <see cref="T:System.Windows.Data.BindingGroup" />.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Controls.ValidationRule" /> objects that validate the source objects in the <see cref="T:System.Windows.Data.BindingGroup" />. </returns>
	public Collection<ValidationRule> ValidationRules => _validationRules;

	/// <summary>Gets a collection of <see cref="T:System.Windows.Data.BindingExpression" /> objects that contains information for each Binding in the <see cref="T:System.Windows.Data.BindingGroup" />.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Data.BindingExpression" /> objects that contains information for each binding in the <see cref="T:System.Windows.Data.BindingGroup" />.</returns>
	public Collection<BindingExpressionBase> BindingExpressions => _bindingExpressions;

	/// <summary>Gets or sets the name that identifies the <see cref="T:System.Windows.Data.BindingGroup" />, which can be used to include and exclude Binding objects in the <see cref="T:System.Windows.Data.BindingGroup" />.</summary>
	/// <returns>The name that identifies the <see cref="T:System.Windows.Data.BindingGroup" />.</returns>
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	/// <summary>Gets or sets whether the <see cref="E:System.Windows.Controls.Validation.Error" /> event occurs when the state of a <see cref="T:System.Windows.Controls.ValidationRule" /> changes.</summary>
	/// <returns>true if the <see cref="E:System.Windows.Controls.Validation.Error" /> event occurs when the state of a <see cref="T:System.Windows.Controls.ValidationRule" /> changes; otherwise, false. The default is false.</returns>
	public bool NotifyOnValidationError
	{
		get
		{
			return _notifyOnValidationError;
		}
		set
		{
			_notifyOnValidationError = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to include the <see cref="T:System.Windows.Controls.NotifyDataErrorValidationRule" />.</summary>
	/// <returns>true to include the <see cref="T:System.Windows.Controls.NotifyDataErrorValidationRule" />; otherwise, false. The default is true.</returns>
	public bool ValidatesOnNotifyDataError
	{
		get
		{
			return _validatesOnNotifyDataError;
		}
		set
		{
			_validatesOnNotifyDataError = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Data.BindingGroup" /> reuses target values that have not been committed to the source.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Data.BindingGroup" /> reuses target values that have not been committed to the source; otherwise, false. The default is false.</returns>
	public bool SharesProposedValues
	{
		get
		{
			return _sharesProposedValues;
		}
		set
		{
			if (_sharesProposedValues != value)
			{
				_proposedValueTable.Clear();
				_sharesProposedValues = value;
			}
		}
	}

	/// <summary>Gets whether each source in the binding can discard pending changes and restore the original values.</summary>
	/// <returns>true if each source in the binding can discard pending changes and restore the original values; otherwise, false.</returns>
	public bool CanRestoreValues
	{
		get
		{
			IList items = Items;
			for (int num = items.Count - 1; num >= 0; num--)
			{
				if (!(items[num] is IEditableObject))
				{
					return false;
				}
			}
			return true;
		}
	}

	/// <summary>Gets the sources that are used by the Binding objects in the <see cref="T:System.Windows.Data.BindingGroup" />.</summary>
	/// <returns>The sources that are used by the Binding objects in the <see cref="T:System.Windows.Data.BindingGroup" />.</returns>
	public IList Items
	{
		get
		{
			EnsureItems();
			return _items;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Data.BindingGroup" /> contains a proposed value that has not been written to the source.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Data.BindingGroup" /> contains a proposed value that has not been written to the source; otherwise, false.</returns>
	public bool IsDirty
	{
		get
		{
			if (_proposedValueTable.Count > 0)
			{
				return true;
			}
			foreach (BindingExpressionBase bindingExpression in _bindingExpressions)
			{
				if (bindingExpression.IsDirty)
				{
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Data.BindingGroup" /> has a failed validation rule.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Data.BindingGroup" /> has a failed validation rule; otherwise, false.</returns>
	public bool HasValidationError
	{
		get
		{
			ValidationErrorCollection superset;
			bool isPure;
			return GetValidationErrors(out superset, out isPure);
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.Windows.Controls.ValidationError" /> objects that caused the <see cref="T:System.Windows.Data.BindingGroup" /> to be invalid.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Controls.ValidationError" /> objects that caused <see cref="T:System.Windows.Data.BindingGroup" /> to be invalid.  The value is null if there are no errors.</returns>
	public ReadOnlyCollection<ValidationError> ValidationErrors
	{
		get
		{
			if (GetValidationErrors(out var superset, out var isPure))
			{
				if (isPure)
				{
					return new ReadOnlyCollection<ValidationError>(superset);
				}
				List<ValidationError> list = new List<ValidationError>();
				foreach (ValidationError item in superset)
				{
					if (Belongs(item))
					{
						list.Add(item);
					}
				}
				return new ReadOnlyCollection<ValidationError>(list);
			}
			return null;
		}
	}

	private DataBindEngine Engine => _engine;

	internal override DependencyObject InheritanceContext
	{
		get
		{
			if (!_inheritanceContext.TryGetTarget(out var target))
			{
				CheckDetach(target);
			}
			return target;
		}
	}

	internal override bool HasMultipleInheritanceContexts => _hasMultipleInheritanceContexts;

	private bool IsEditing { get; set; }

	private bool IsItemsValid
	{
		get
		{
			return _isItemsValid;
		}
		set
		{
			_isItemsValid = value;
			if (!value && (IsEditing || ValidatesOnNotifyDataError))
			{
				EnsureItems();
			}
		}
	}

	private bool ValidatesOnDataTransfer
	{
		get
		{
			if (ValidationRules != null)
			{
				for (int num = ValidationRules.Count - 1; num >= 0; num--)
				{
					if (ValidationRules[num].ValidatesOnTargetUpdated)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.BindingGroup" /> class. </summary>
	public BindingGroup()
	{
		_validationRules = new ValidationRuleCollection();
		Initialize();
	}

	internal BindingGroup(BindingGroup master)
	{
		_validationRules = master._validationRules;
		_name = master._name;
		_notifyOnValidationError = master._notifyOnValidationError;
		_sharesProposedValues = master._sharesProposedValues;
		_validatesOnNotifyDataError = master._validatesOnNotifyDataError;
		Initialize();
	}

	private void Initialize()
	{
		_engine = DataBindEngine.CurrentDataBindEngine;
		_bindingExpressions = new BindingExpressionCollection();
		((INotifyCollectionChanged)_bindingExpressions).CollectionChanged += OnBindingsChanged;
		_itemsRW = new Collection<WeakReference>();
		_items = new WeakReadOnlyCollection<object>(_itemsRW);
	}

	private bool GetValidationErrors(out ValidationErrorCollection superset, out bool isPure)
	{
		superset = null;
		isPure = true;
		DependencyObject dependencyObject = Helper.FindMentor(this);
		if (dependencyObject == null)
		{
			return false;
		}
		superset = Validation.GetErrorsInternal(dependencyObject);
		if (superset == null || superset.Count == 0)
		{
			return false;
		}
		for (int num = superset.Count - 1; num >= 0; num--)
		{
			ValidationError error = superset[num];
			if (!Belongs(error))
			{
				isPure = false;
				break;
			}
		}
		return true;
	}

	private bool Belongs(ValidationError error)
	{
		if (error.BindingInError != this && !_proposedValueTable.HasValidationError(error))
		{
			if (error.BindingInError is BindingExpressionBase bindingExpressionBase)
			{
				return bindingExpressionBase.BindingGroup == this;
			}
			return false;
		}
		return true;
	}

	/// <summary>Begins an edit transaction on the sources in the <see cref="T:System.Windows.Data.BindingGroup" />.</summary>
	public void BeginEdit()
	{
		if (IsEditing)
		{
			return;
		}
		IList items = Items;
		for (int num = items.Count - 1; num >= 0; num--)
		{
			if (items[num] is IEditableObject editableObject)
			{
				editableObject.BeginEdit();
			}
		}
		IsEditing = true;
	}

	/// <summary>Runs all the <see cref="T:System.Windows.Controls.ValidationRule" /> objects and updates the binding sources if all validation rules succeed.</summary>
	/// <returns>true if every <see cref="T:System.Windows.Controls.ValidationRule" /> succeeds and the values are committed to the sources; otherwise, false.</returns>
	public bool CommitEdit()
	{
		bool flag = UpdateAndValidate(ValidationStep.CommittedValue);
		IsEditing = IsEditing && !flag;
		return flag;
	}

	/// <summary>Ends the edit transaction and discards the pending changes.</summary>
	public void CancelEdit()
	{
		ClearValidationErrors();
		IList items = Items;
		for (int num = items.Count - 1; num >= 0; num--)
		{
			if (items[num] is IEditableObject editableObject)
			{
				editableObject.CancelEdit();
			}
		}
		for (int num2 = _bindingExpressions.Count - 1; num2 >= 0; num2--)
		{
			_bindingExpressions[num2].UpdateTarget();
		}
		_proposedValueTable.UpdateDependents();
		_proposedValueTable.Clear();
		IsEditing = false;
	}

	/// <summary>Runs the converter on the binding and the <see cref="T:System.Windows.Controls.ValidationRule" /> objects that have the <see cref="P:System.Windows.Controls.ValidationRule.ValidationStep" /> property set to <see cref="F:System.Windows.Controls.ValidationStep.RawProposedValue" /> or <see cref="F:System.Windows.Controls.ValidationStep.ConvertedProposedValue" />.</summary>
	/// <returns>true if the validation rules succeed; otherwise, false.</returns>
	public bool ValidateWithoutUpdate()
	{
		return UpdateAndValidate(ValidationStep.ConvertedProposedValue);
	}

	/// <summary>Runs the converter on the binding and the <see cref="T:System.Windows.Controls.ValidationRule" /> objects that have the <see cref="P:System.Windows.Controls.ValidationRule.ValidationStep" /> property set to <see cref="F:System.Windows.Controls.ValidationStep.RawProposedValue" />, <see cref="F:System.Windows.Controls.ValidationStep.ConvertedProposedValue" />, or <see cref="F:System.Windows.Controls.ValidationStep.UpdatedValue" /> and saves the values of the targets to the source objects if all the validation rules succeed.</summary>
	/// <returns>true if all validation rules succeed; otherwise, false.</returns>
	public bool UpdateSources()
	{
		return UpdateAndValidate(ValidationStep.UpdatedValue);
	}

	/// <summary>Returns the proposed value for the specified property and item.</summary>
	/// <returns>The proposed property value. </returns>
	/// <param name="item">The object that contains the specified property.</param>
	/// <param name="propertyName">The property whose proposed value to get.</param>
	/// <exception cref="T:System.InvalidOperationException">There is not a binding for the specified item and property.</exception>
	/// <exception cref="T:System.Windows.Data.ValueUnavailableException">The value of the specified property is not available, due to a conversion error or because an earlier validation rule failed.</exception>
	public object GetValue(object item, string propertyName)
	{
		if (TryGetValueImpl(item, propertyName, out var value))
		{
			return value;
		}
		if (value == Binding.DoNothing)
		{
			throw new ValueUnavailableException(SR.Format(SR.BindingGroup_NoEntry, item, propertyName));
		}
		throw new ValueUnavailableException(SR.Format(SR.BindingGroup_ValueUnavailable, item, propertyName));
	}

	/// <summary>Attempts to get the proposed value for the specified property and item.</summary>
	/// <returns>true if value is the proposed value for the specified property; otherwise, false.</returns>
	/// <param name="item">The object that contains the specified property.</param>
	/// <param name="propertyName">The property whose proposed value to get.</param>
	/// <param name="value">When this method returns, contains an object that represents the proposed property value. This parameter is passed uninitialized. </param>
	public bool TryGetValue(object item, string propertyName, out object value)
	{
		bool result = TryGetValueImpl(item, propertyName, out value);
		if (value == Binding.DoNothing)
		{
			value = DependencyProperty.UnsetValue;
		}
		return result;
	}

	private bool TryGetValueImpl(object item, string propertyName, out object value)
	{
		GetValueTableEntry getValueTableEntry = _getValueTable[item, propertyName];
		if (getValueTableEntry == null)
		{
			ProposedValueEntry proposedValueEntry = _proposedValueTable[item, propertyName];
			if (proposedValueEntry != null)
			{
				switch (_validationStep)
				{
				case ValidationStep.RawProposedValue:
					value = proposedValueEntry.RawValue;
					return true;
				case ValidationStep.ConvertedProposedValue:
				case ValidationStep.UpdatedValue:
				case ValidationStep.CommittedValue:
					value = proposedValueEntry.ConvertedValue;
					return value != DependencyProperty.UnsetValue;
				}
			}
			value = Binding.DoNothing;
			return false;
		}
		ValidationStep validationStep = _validationStep;
		if ((uint)validationStep <= 3u)
		{
			value = getValueTableEntry.Value;
		}
		else
		{
			value = getValueTableEntry.BindingExpressionBase.RootBindingExpression.GetRawProposedValue();
		}
		if (value == Binding.DoNothing)
		{
			BindingExpression bindingExpression = (BindingExpression)getValueTableEntry.BindingExpressionBase;
			value = bindingExpression.SourceValue;
		}
		return value != DependencyProperty.UnsetValue;
	}

	internal override void AddInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		if (property != null && property.PropertyType != typeof(BindingGroup) && TraceData.IsEnabled)
		{
			string text = ((property != null) ? property.Name : "(null)");
			TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.BindingGroupWrongProperty(text, context.GetType().FullName));
		}
		_inheritanceContext.TryGetTarget(out var target);
		InheritanceContextHelper.AddInheritanceContext(context, this, ref _hasMultipleInheritanceContexts, ref target);
		CheckDetach(target);
		_inheritanceContext = ((target == null) ? NullInheritanceContext : new WeakReference<DependencyObject>(target));
		if (property == FrameworkElement.BindingGroupProperty && !_hasMultipleInheritanceContexts && (ValidatesOnDataTransfer || ValidatesOnNotifyDataError) && Helper.FindMentor(this) is UIElement uIElement)
		{
			uIElement.LayoutUpdated += OnLayoutUpdated;
		}
		if (_hasMultipleInheritanceContexts && property != ItemsControl.ItemBindingGroupProperty && TraceData.IsEnabled)
		{
			TraceData.TraceAndNotify(TraceEventType.Warning, TraceData.BindingGroupMultipleInheritance);
		}
	}

	internal override void RemoveInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		_inheritanceContext.TryGetTarget(out var target);
		InheritanceContextHelper.RemoveInheritanceContext(context, this, ref _hasMultipleInheritanceContexts, ref target);
		CheckDetach(target);
		_inheritanceContext = ((target == null) ? NullInheritanceContext : new WeakReference<DependencyObject>(target));
	}

	private void CheckDetach(DependencyObject newOwner)
	{
		if (newOwner == null && _inheritanceContext != NullInheritanceContext)
		{
			Engine.CommitManager.RemoveBindingGroup(this);
		}
	}

	internal void UpdateTable(BindingExpression bindingExpression)
	{
		if (_getValueTable.Update(bindingExpression))
		{
			_proposedValueTable.Remove(bindingExpression);
		}
		IsItemsValid = false;
	}

	internal void AddToValueTable(BindingExpressionBase bindingExpressionBase)
	{
		_getValueTable.EnsureEntry(bindingExpressionBase);
	}

	internal object GetValue(BindingExpressionBase bindingExpressionBase)
	{
		return _getValueTable.GetValue(bindingExpressionBase);
	}

	internal void SetValue(BindingExpressionBase bindingExpressionBase, object value)
	{
		_getValueTable.SetValue(bindingExpressionBase, value);
	}

	internal void UseSourceValue(BindingExpressionBase bindingExpressionBase)
	{
		_getValueTable.UseSourceValue(bindingExpressionBase);
	}

	internal ProposedValueEntry GetProposedValueEntry(object item, string propertyName)
	{
		return _proposedValueTable[item, propertyName];
	}

	internal void RemoveProposedValueEntry(ProposedValueEntry entry)
	{
		_proposedValueTable.Remove(entry);
	}

	internal void AddBindingForProposedValue(BindingExpressionBase dependent, object item, string propertyName)
	{
		_proposedValueTable[item, propertyName]?.AddDependent(dependent);
	}

	internal void AddValidationError(ValidationError validationError)
	{
		DependencyObject dependencyObject = Helper.FindMentor(this);
		if (dependencyObject != null)
		{
			Validation.AddValidationError(validationError, dependencyObject, NotifyOnValidationError);
		}
	}

	internal void RemoveValidationError(ValidationError validationError)
	{
		DependencyObject dependencyObject = Helper.FindMentor(this);
		if (dependencyObject != null)
		{
			Validation.RemoveValidationError(validationError, dependencyObject, NotifyOnValidationError);
		}
	}

	private void ClearValidationErrors(ValidationStep validationStep)
	{
		ClearValidationErrorsImpl(validationStep, allSteps: false);
	}

	private void ClearValidationErrors()
	{
		ClearValidationErrorsImpl(ValidationStep.RawProposedValue, allSteps: true);
	}

	private void ClearValidationErrorsImpl(ValidationStep validationStep, bool allSteps)
	{
		DependencyObject dependencyObject = Helper.FindMentor(this);
		if (dependencyObject == null)
		{
			return;
		}
		ValidationErrorCollection errorsInternal = Validation.GetErrorsInternal(dependencyObject);
		if (errorsInternal == null)
		{
			return;
		}
		for (int num = errorsInternal.Count - 1; num >= 0; num--)
		{
			ValidationError validationError = errorsInternal[num];
			if ((allSteps || validationError.RuleInError.ValidationStep == validationStep) && (validationError.BindingInError == this || _proposedValueTable.HasValidationError(validationError)))
			{
				RemoveValidationError(validationError);
			}
		}
	}

	private void EnsureItems()
	{
		if (IsItemsValid)
		{
			return;
		}
		IList<WeakReference> list = new Collection<WeakReference>();
		DependencyObject dependencyObject = Helper.FindMentor(this);
		if (dependencyObject != null)
		{
			object value = dependencyObject.GetValue(FrameworkElement.DataContextProperty);
			if (value != null && value != CollectionView.NewItemPlaceholder && value != BindingExpressionBase.DisconnectedItem)
			{
				WeakReference weakReference = ((_itemsRW.Count > 0) ? _itemsRW[0] : null);
				if (weakReference == null || !ItemsControl.EqualsEx(value, weakReference.Target))
				{
					weakReference = new WeakReference(value);
				}
				list.Add(weakReference);
			}
		}
		_getValueTable.AddUniqueItems(list);
		_proposedValueTable.AddUniqueItems(list);
		for (int num = _itemsRW.Count - 1; num >= 0; num--)
		{
			int num2 = FindIndexOf(_itemsRW[num], list);
			if (num2 >= 0)
			{
				list.RemoveAt(num2);
			}
			else
			{
				if (ValidatesOnNotifyDataError && _itemsRW[num].Target is INotifyDataErrorInfo source)
				{
					ErrorsChangedEventManager.RemoveHandler(source, OnErrorsChanged);
				}
				_itemsRW.RemoveAt(num);
			}
		}
		for (int num3 = list.Count - 1; num3 >= 0; num3--)
		{
			_itemsRW.Add(list[num3]);
			if (IsEditing && list[num3].Target is IEditableObject editableObject)
			{
				editableObject.BeginEdit();
			}
			if (ValidatesOnNotifyDataError && list[num3].Target is INotifyDataErrorInfo notifyDataErrorInfo)
			{
				ErrorsChangedEventManager.AddHandler(notifyDataErrorInfo, OnErrorsChanged);
				UpdateNotifyDataErrors(notifyDataErrorInfo, list[num3]);
			}
		}
		IsItemsValid = true;
	}

	private void OnLayoutUpdated(object sender, EventArgs e)
	{
		DependencyObject dependencyObject = Helper.FindMentor(this);
		if (dependencyObject is UIElement uIElement)
		{
			uIElement.LayoutUpdated -= OnLayoutUpdated;
		}
		Helper.DowncastToFEorFCE(dependencyObject, out var fe, out var fce, throwIfNeither: false);
		if (fe != null)
		{
			fe.DataContextChanged += OnDataContextChanged;
		}
		else if (fce != null)
		{
			fce.DataContextChanged += OnDataContextChanged;
		}
		ValidateOnDataTransfer();
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue != BindingExpressionBase.DisconnectedItem)
		{
			IsItemsValid = false;
			ValidateOnDataTransfer();
		}
	}

	private void ValidateOnDataTransfer()
	{
		if (!ValidatesOnDataTransfer)
		{
			return;
		}
		DependencyObject dependencyObject = Helper.FindMentor(this);
		if (dependencyObject == null || ValidationRules.Count == 0)
		{
			return;
		}
		Collection<ValidationError> collection;
		if (!Validation.GetHasError(dependencyObject))
		{
			collection = null;
		}
		else
		{
			collection = new Collection<ValidationError>();
			ReadOnlyCollection<ValidationError> errors = Validation.GetErrors(dependencyObject);
			int i = 0;
			for (int count = errors.Count; i < count; i++)
			{
				ValidationError validationError = errors[i];
				if (validationError.RuleInError.ValidatesOnTargetUpdated && validationError.BindingInError == this)
				{
					collection.Add(validationError);
				}
			}
		}
		CultureInfo culture = GetCulture();
		int j = 0;
		for (int count2 = ValidationRules.Count; j < count2; j++)
		{
			ValidationRule validationRule = ValidationRules[j];
			if (!validationRule.ValidatesOnTargetUpdated)
			{
				continue;
			}
			try
			{
				ValidationResult validationResult = validationRule.Validate(DependencyProperty.UnsetValue, culture, this);
				if (!validationResult.IsValid)
				{
					AddValidationError(new ValidationError(validationRule, this, validationResult.ErrorContent, null));
				}
			}
			catch (ValueUnavailableException ex)
			{
				AddValidationError(new ValidationError(validationRule, this, ex.Message, ex));
			}
		}
		if (collection != null)
		{
			int k = 0;
			for (int count3 = collection.Count; k < count3; k++)
			{
				RemoveValidationError(collection[k]);
			}
		}
	}

	private bool UpdateAndValidate(ValidationStep validationStep)
	{
		DependencyObject dependencyObject = Helper.FindMentor(this);
		if (dependencyObject != null && dependencyObject.GetValue(FrameworkElement.DataContextProperty) == CollectionView.NewItemPlaceholder)
		{
			return true;
		}
		PrepareProposedValuesForUpdate(dependencyObject, validationStep >= ValidationStep.UpdatedValue);
		bool flag = true;
		_validationStep = ValidationStep.RawProposedValue;
		while (_validationStep <= validationStep && flag)
		{
			switch (_validationStep)
			{
			case ValidationStep.RawProposedValue:
				_getValueTable.ResetValues();
				break;
			case ValidationStep.ConvertedProposedValue:
				flag = ObtainConvertedProposedValues();
				break;
			case ValidationStep.UpdatedValue:
				flag = UpdateValues();
				break;
			case ValidationStep.CommittedValue:
				flag = CommitValues();
				break;
			}
			if (!CheckValidationRules())
			{
				flag = false;
			}
			_validationStep++;
		}
		ResetProposedValuesAfterUpdate(dependencyObject, flag && validationStep == ValidationStep.CommittedValue);
		_validationStep = (ValidationStep)(-1);
		_getValueTable.ResetValues();
		NotifyCommitManager();
		return flag;
	}

	private void UpdateNotifyDataErrors(INotifyDataErrorInfo indei, WeakReference itemWR)
	{
		if (itemWR == null)
		{
			int num = FindIndexOf(indei, _itemsRW);
			if (num < 0)
			{
				return;
			}
			itemWR = _itemsRW[num];
		}
		List<object> dataErrors;
		try
		{
			dataErrors = BindingExpression.GetDataErrors(indei, string.Empty);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalApplicationException(ex))
			{
				throw;
			}
			return;
		}
		UpdateNotifyDataErrorValidationErrors(itemWR, dataErrors);
	}

	private void UpdateNotifyDataErrorValidationErrors(WeakReference itemWR, List<object> errors)
	{
		if (!_notifyDataErrors.TryGetValue(itemWR, out var value))
		{
			value = null;
		}
		BindingExpressionBase.GetValidationDelta(value, errors, out var toAdd, out var toRemove);
		if (toAdd != null && toAdd.Count > 0)
		{
			ValidationRule instance = NotifyDataErrorValidationRule.Instance;
			if (value == null)
			{
				value = new List<ValidationError>();
			}
			foreach (object item in toAdd)
			{
				ValidationError validationError = new ValidationError(instance, this, item, null);
				value.Add(validationError);
				AddValidationError(validationError);
			}
		}
		if (toRemove != null && toRemove.Count > 0)
		{
			foreach (ValidationError item2 in toRemove)
			{
				value.Remove(item2);
				RemoveValidationError(item2);
			}
			if (value.Count == 0)
			{
				value = null;
			}
		}
		if (value == null)
		{
			_notifyDataErrors.Remove(itemWR);
		}
		else
		{
			_notifyDataErrors[itemWR] = value;
		}
	}

	private bool ObtainConvertedProposedValues()
	{
		bool flag = true;
		for (int num = _bindingExpressions.Count - 1; num >= 0; num--)
		{
			flag = _bindingExpressions[num].ObtainConvertedProposedValue(this) && flag;
		}
		return flag;
	}

	private bool UpdateValues()
	{
		bool flag = true;
		for (int num = _bindingExpressions.Count - 1; num >= 0; num--)
		{
			flag = _bindingExpressions[num].UpdateSource(this) && flag;
		}
		if (_proposedValueBindingExpressions != null)
		{
			for (int num2 = _proposedValueBindingExpressions.Length - 1; num2 >= 0; num2--)
			{
				BindingExpression bindingExpression = _proposedValueBindingExpressions[num2];
				ProposedValueEntry proposedValueEntry = _proposedValueTable[bindingExpression];
				flag = bindingExpression.UpdateSource(proposedValueEntry.ConvertedValue) != DependencyProperty.UnsetValue && flag;
			}
		}
		return flag;
	}

	private bool CheckValidationRules()
	{
		bool result = true;
		ClearValidationErrors(_validationStep);
		for (int num = _bindingExpressions.Count - 1; num >= 0; num--)
		{
			if (!_bindingExpressions[num].CheckValidationRules(this, _validationStep))
			{
				result = false;
			}
		}
		if (_validationStep >= ValidationStep.UpdatedValue && _proposedValueBindingExpressions != null)
		{
			for (int num2 = _proposedValueBindingExpressions.Length - 1; num2 >= 0; num2--)
			{
				if (!_proposedValueBindingExpressions[num2].CheckValidationRules(this, _validationStep))
				{
					result = false;
				}
			}
		}
		CultureInfo culture = GetCulture();
		int i = 0;
		for (int count = _validationRules.Count; i < count; i++)
		{
			ValidationRule validationRule = _validationRules[i];
			if (validationRule.ValidationStep != _validationStep)
			{
				continue;
			}
			try
			{
				ValidationResult validationResult = validationRule.Validate(DependencyProperty.UnsetValue, culture, this);
				if (!validationResult.IsValid)
				{
					AddValidationError(new ValidationError(validationRule, this, validationResult.ErrorContent, null));
					result = false;
				}
			}
			catch (ValueUnavailableException ex)
			{
				AddValidationError(new ValidationError(validationRule, this, ex.Message, ex));
				result = false;
			}
		}
		return result;
	}

	private bool CommitValues()
	{
		bool result = true;
		IList items = Items;
		for (int num = items.Count - 1; num >= 0; num--)
		{
			if (items[num] is IEditableObject editableObject)
			{
				try
				{
					editableObject.EndEdit();
				}
				catch (Exception ex)
				{
					if (CriticalExceptions.IsCriticalApplicationException(ex))
					{
						throw;
					}
					ValidationError validationError = new ValidationError(ExceptionValidationRule.Instance, this, ex.Message, ex);
					AddValidationError(validationError);
					result = false;
				}
			}
		}
		return result;
	}

	private static int FindIndexOf(WeakReference wr, IList<WeakReference> list)
	{
		object target = wr.Target;
		if (target == null)
		{
			return -1;
		}
		return FindIndexOf(target, list);
	}

	private static int FindIndexOf(object item, IList<WeakReference> list)
	{
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (ItemsControl.EqualsEx(item, list[i].Target))
			{
				return i;
			}
		}
		return -1;
	}

	private CultureInfo GetCulture()
	{
		if (_culture == null)
		{
			DependencyObject dependencyObject = Helper.FindMentor(this);
			if (dependencyObject != null)
			{
				_culture = ((XmlLanguage)dependencyObject.GetValue(FrameworkElement.LanguageProperty)).GetSpecificCulture();
			}
		}
		return _culture;
	}

	private void OnBindingsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
		{
			BindingExpressionBase exprBase = e.NewItems[0] as BindingExpressionBase;
			exprBase.JoinBindingGroup(this, explicitJoin: true);
			break;
		}
		case NotifyCollectionChangedAction.Remove:
		{
			BindingExpressionBase exprBase = e.OldItems[0] as BindingExpressionBase;
			RemoveBindingExpression(exprBase);
			break;
		}
		case NotifyCollectionChangedAction.Replace:
		{
			BindingExpressionBase exprBase = e.OldItems[0] as BindingExpressionBase;
			RemoveBindingExpression(exprBase);
			exprBase = e.NewItems[0] as BindingExpressionBase;
			exprBase.JoinBindingGroup(this, explicitJoin: true);
			break;
		}
		case NotifyCollectionChangedAction.Reset:
			RemoveAllBindingExpressions();
			break;
		}
		IsItemsValid = false;
	}

	private void RemoveBindingExpression(BindingExpressionBase exprBase)
	{
		BindingExpressionBase rootBindingExpression = exprBase.RootBindingExpression;
		if (SharesProposedValues && rootBindingExpression.NeedsValidation)
		{
			rootBindingExpression.ValidateAndConvertProposedValue(out var values);
			PreserveProposedValues(values);
		}
		foreach (BindingExpressionBase item in _getValueTable.RemoveRootBinding(rootBindingExpression))
		{
			item.OnBindingGroupChanged(joining: false);
			_bindingExpressions.Remove(item);
		}
		rootBindingExpression.LeaveBindingGroup();
	}

	private void RemoveAllBindingExpressions()
	{
		GetValueTableEntry firstEntry;
		while ((firstEntry = _getValueTable.GetFirstEntry()) != null)
		{
			RemoveBindingExpression(firstEntry.BindingExpressionBase);
		}
	}

	private void PreserveProposedValues(Collection<BindingExpressionBase.ProposedValue> proposedValues)
	{
		if (proposedValues != null)
		{
			int i = 0;
			for (int count = proposedValues.Count; i < count; i++)
			{
				_proposedValueTable.Add(proposedValues[i]);
			}
		}
	}

	private void PrepareProposedValuesForUpdate(DependencyObject mentor, bool isUpdating)
	{
		int count = _proposedValueTable.Count;
		if (count == 0 || !isUpdating)
		{
			return;
		}
		_proposedValueBindingExpressions = new BindingExpression[count];
		for (int i = 0; i < count; i++)
		{
			ProposedValueEntry proposedValueEntry = _proposedValueTable[i];
			Binding binding = proposedValueEntry.Binding;
			Binding binding2 = new Binding();
			binding2.Source = proposedValueEntry.Item;
			binding2.Mode = BindingMode.TwoWay;
			binding2.Path = new PropertyPath(proposedValueEntry.PropertyName, binding.Path.PathParameters);
			binding2.ValidatesOnDataErrors = binding.ValidatesOnDataErrors;
			binding2.ValidatesOnNotifyDataErrors = binding.ValidatesOnNotifyDataErrors;
			binding2.ValidatesOnExceptions = binding.ValidatesOnExceptions;
			Collection<ValidationRule> validationRulesInternal = binding.ValidationRulesInternal;
			if (validationRulesInternal != null)
			{
				int j = 0;
				for (int count2 = validationRulesInternal.Count; j < count2; j++)
				{
					binding2.ValidationRules.Add(validationRulesInternal[j]);
				}
			}
			BindingExpression bindingExpression = (BindingExpression)BindingExpressionBase.CreateUntargetedBindingExpression(mentor, binding2);
			bindingExpression.Attach(mentor);
			bindingExpression.NeedsUpdate = true;
			_proposedValueBindingExpressions[i] = bindingExpression;
		}
	}

	private void ResetProposedValuesAfterUpdate(DependencyObject mentor, bool isFullUpdate)
	{
		if (_proposedValueBindingExpressions != null)
		{
			int i = 0;
			for (int num = _proposedValueBindingExpressions.Length; i < num; i++)
			{
				BindingExpression obj = _proposedValueBindingExpressions[i];
				ValidationError validationError = obj.ValidationError;
				obj.Detach();
				if (validationError != null)
				{
					ValidationError validationError2 = new ValidationError(validationError.RuleInError, this, validationError.ErrorContent, validationError.Exception);
					AddValidationError(validationError2);
				}
			}
			_proposedValueBindingExpressions = null;
		}
		if (isFullUpdate)
		{
			_proposedValueTable.UpdateDependents();
			_proposedValueTable.Clear();
		}
	}

	private void NotifyCommitManager()
	{
		if (!Engine.IsShutDown)
		{
			if (Owner != null && (IsDirty || HasValidationError))
			{
				Engine.CommitManager.AddBindingGroup(this);
			}
			else
			{
				Engine.CommitManager.RemoveBindingGroup(this);
			}
		}
	}

	private void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e)
	{
		if (base.Dispatcher.Thread == Thread.CurrentThread)
		{
			UpdateNotifyDataErrors((INotifyDataErrorInfo)sender, null);
			return;
		}
		Engine.Marshal(delegate(object arg)
		{
			UpdateNotifyDataErrors((INotifyDataErrorInfo)arg, null);
			return (object)null;
		}, sender);
	}
}
