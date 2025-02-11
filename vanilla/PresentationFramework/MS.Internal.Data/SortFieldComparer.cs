using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class SortFieldComparer : IComparer
{
	private struct SortPropertyInfo
	{
		internal int index;

		internal PropertyPath info;

		internal bool descending;

		internal object GetValue(object o)
		{
			if (o is CachedValueItem)
			{
				return GetValueFromCVI((CachedValueItem)o);
			}
			return GetValueCore(o);
		}

		private object GetValueFromCVI(CachedValueItem cvi)
		{
			object obj = cvi[index];
			if (obj == DependencyProperty.UnsetValue)
			{
				object obj2 = (cvi[index] = GetValueCore(cvi.OriginalItem));
				obj = obj2;
			}
			return obj;
		}

		private object GetValueCore(object o)
		{
			object obj;
			if (info == null)
			{
				obj = o;
			}
			else
			{
				using (info.SetContext(o))
				{
					obj = info.GetValue();
				}
			}
			if (obj == DependencyProperty.UnsetValue || BindingExpressionBase.IsNullValue(obj))
			{
				obj = null;
			}
			return obj;
		}
	}

	private struct CachedValueItem
	{
		private object _item;

		private object[] _values;

		public object OriginalItem => _item;

		public object this[int index]
		{
			get
			{
				return _values[index];
			}
			set
			{
				_values[index] = value;
				if (++index < _values.Length)
				{
					_values[index] = DependencyProperty.UnsetValue;
				}
			}
		}

		public void Initialize(object item, int nFields)
		{
			_item = item;
			_values = new object[nFields];
			_values[0] = DependencyProperty.UnsetValue;
		}
	}

	private SortPropertyInfo[] _fields;

	private SortDescriptionCollection _sortFields;

	private Comparer _comparer;

	internal IComparer BaseComparer => _comparer;

	internal SortFieldComparer(SortDescriptionCollection sortFields, CultureInfo culture)
	{
		_sortFields = sortFields;
		_fields = CreatePropertyInfo(_sortFields);
		_comparer = ((culture == null || culture == CultureInfo.InvariantCulture) ? Comparer.DefaultInvariant : ((culture == CultureInfo.CurrentCulture) ? Comparer.Default : new Comparer(culture)));
	}

	public int Compare(object o1, object o2)
	{
		int num = 0;
		for (int i = 0; i < _fields.Length; i++)
		{
			object value = _fields[i].GetValue(o1);
			object value2 = _fields[i].GetValue(o2);
			num = _comparer.Compare(value, value2);
			if (_fields[i].descending)
			{
				num = -num;
			}
			if (num != 0)
			{
				break;
			}
		}
		return num;
	}

	internal static void SortHelper(ArrayList al, IComparer comparer)
	{
		if (!(comparer is SortFieldComparer sortFieldComparer))
		{
			al.Sort(comparer);
			return;
		}
		int count = al.Count;
		int nFields = sortFieldComparer._fields.Length;
		CachedValueItem[] array = new CachedValueItem[count];
		for (int i = 0; i < count; i++)
		{
			array[i].Initialize(al[i], nFields);
		}
		Array.Sort(array, sortFieldComparer);
		for (int j = 0; j < count; j++)
		{
			al[j] = array[j].OriginalItem;
		}
	}

	private SortPropertyInfo[] CreatePropertyInfo(SortDescriptionCollection sortFields)
	{
		SortPropertyInfo[] array = new SortPropertyInfo[sortFields.Count];
		for (int i = 0; i < sortFields.Count; i++)
		{
			PropertyPath info = ((!string.IsNullOrEmpty(sortFields[i].PropertyName)) ? new PropertyPath(sortFields[i].PropertyName) : null);
			array[i].index = i;
			array[i].info = info;
			array[i].descending = sortFields[i].Direction == ListSortDirection.Descending;
		}
		return array;
	}
}
