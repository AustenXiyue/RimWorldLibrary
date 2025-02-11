using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace MS.Internal.Data;

internal sealed class XDeferredAxisSource
{
	private class Record
	{
		private IEnumerable _xda;

		private DifferencingCollection _dc;

		private ReadOnlyObservableCollection<object> _rooc;

		public IEnumerable XDA => _xda;

		public DifferencingCollection DC => _dc;

		public ReadOnlyObservableCollection<object> Collection => _rooc;

		public Record(IEnumerable xda)
		{
			_xda = xda;
			if (xda != null)
			{
				_dc = new DifferencingCollection(xda);
				_rooc = new ReadOnlyObservableCollection<object>(_dc);
			}
		}
	}

	private WeakReference _component;

	private PropertyDescriptor _propertyDescriptor;

	private HybridDictionary _table;

	private const string FullCollectionKey = "%%FullCollection%%";

	public IEnumerable this[string name]
	{
		get
		{
			Record record = (Record)_table[name];
			if (record == null)
			{
				object target = _component.Target;
				if (target == null)
				{
					return null;
				}
				IEnumerable enumerable = _propertyDescriptor.GetValue(target) as IEnumerable;
				if (enumerable != null && name != "%%FullCollection%%")
				{
					MemberInfo[] defaultMembers = enumerable.GetType().GetDefaultMembers();
					PropertyInfo propertyInfo = ((defaultMembers.Length != 0) ? (defaultMembers[0] as PropertyInfo) : null);
					enumerable = ((propertyInfo == null) ? null : (propertyInfo.GetValue(enumerable, BindingFlags.GetProperty, null, new object[1] { name }, CultureInfo.InvariantCulture) as IEnumerable));
				}
				record = new Record(enumerable);
				_table[name] = record;
			}
			else
			{
				record.DC.Update(record.XDA);
			}
			return record.Collection;
		}
	}

	internal IEnumerable FullCollection => this["%%FullCollection%%"];

	internal XDeferredAxisSource(object component, PropertyDescriptor pd)
	{
		_component = new WeakReference(component);
		_propertyDescriptor = pd;
		_table = new HybridDictionary();
	}
}
