using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MS.Internal.Data;

internal class CommitManager
{
	private class Set<T> : Dictionary<T, object>, IEnumerable<T>, IEnumerable
	{
		public Set()
		{
		}

		public Set(IDictionary<T, object> other)
			: base(other)
		{
		}

		public Set(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		public void Add(T item)
		{
			if (!ContainsKey(item))
			{
				Add(item, null);
			}
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return base.Keys.GetEnumerator();
		}

		public List<T> ToList()
		{
			return new List<T>(base.Keys);
		}
	}

	private Set<BindingGroup> _bindingGroups = new Set<BindingGroup>();

	private Set<BindingExpressionBase> _bindings = new Set<BindingExpressionBase>();

	private static readonly List<BindingGroup> EmptyBindingGroupList = new List<BindingGroup>();

	private static readonly List<BindingExpressionBase> EmptyBindingList = new List<BindingExpressionBase>();

	internal bool IsEmpty
	{
		get
		{
			if (_bindings.Count == 0)
			{
				return _bindingGroups.Count == 0;
			}
			return false;
		}
	}

	internal void AddBindingGroup(BindingGroup bindingGroup)
	{
		_bindingGroups.Add(bindingGroup);
	}

	internal void RemoveBindingGroup(BindingGroup bindingGroup)
	{
		_bindingGroups.Remove(bindingGroup);
	}

	internal void AddBinding(BindingExpressionBase binding)
	{
		_bindings.Add(binding);
	}

	internal void RemoveBinding(BindingExpressionBase binding)
	{
		_bindings.Remove(binding);
	}

	internal List<BindingGroup> GetBindingGroupsInScope(DependencyObject element)
	{
		List<BindingGroup> list = _bindingGroups.ToList();
		List<BindingGroup> list2 = EmptyBindingGroupList;
		foreach (BindingGroup item in list)
		{
			DependencyObject owner = item.Owner;
			if (owner != null && IsInScope(element, owner))
			{
				if (list2 == EmptyBindingGroupList)
				{
					list2 = new List<BindingGroup>();
				}
				list2.Add(item);
			}
		}
		return list2;
	}

	internal List<BindingExpressionBase> GetBindingsInScope(DependencyObject element)
	{
		List<BindingExpressionBase> list = _bindings.ToList();
		List<BindingExpressionBase> list2 = EmptyBindingList;
		foreach (BindingExpressionBase item in list)
		{
			DependencyObject targetElement = item.TargetElement;
			if (targetElement != null && item.IsEligibleForCommit && IsInScope(element, targetElement))
			{
				if (list2 == EmptyBindingList)
				{
					list2 = new List<BindingExpressionBase>();
				}
				list2.Add(item);
			}
		}
		return list2;
	}

	internal bool Purge()
	{
		bool flag = false;
		int count = _bindings.Count;
		if (count > 0)
		{
			foreach (BindingExpressionBase item in _bindings.ToList())
			{
				_ = item.TargetElement;
			}
		}
		flag = flag || _bindings.Count < count;
		count = _bindingGroups.Count;
		if (count > 0)
		{
			foreach (BindingGroup item2 in _bindingGroups.ToList())
			{
				_ = item2.Owner;
			}
		}
		return flag || _bindingGroups.Count < count;
	}

	private bool IsInScope(DependencyObject ancestor, DependencyObject element)
	{
		if (ancestor != null)
		{
			return VisualTreeHelper.IsAncestorOf(ancestor, element);
		}
		return true;
	}
}
