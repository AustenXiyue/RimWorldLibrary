using System;
using System.Collections.ObjectModel;

namespace MS.Internal;

internal static class GenericsInstances
{
	private static ObservableCollection<object> s_OC_Empty = new ObservableCollection<object>();

	private static ReadOnlyObservableCollection<object> s_ROOC_Empty = new ReadOnlyObservableCollection<object>(new ObservableCollection<object>());

	private static Predicate<object> s_PM_Empty = PredicateMethod;

	private static bool PredicateMethod(object item)
	{
		return false;
	}
}
