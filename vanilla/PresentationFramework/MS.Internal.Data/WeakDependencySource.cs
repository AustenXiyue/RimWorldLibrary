using System;
using System.Windows;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class WeakDependencySource
{
	private object _item;

	private DependencyProperty _dp;

	internal DependencyObject DependencyObject => (DependencyObject)BindingExpressionBase.GetReference(_item);

	internal DependencyProperty DependencyProperty => _dp;

	internal WeakDependencySource(DependencyObject item, DependencyProperty dp)
	{
		_item = BindingExpressionBase.CreateReference(item);
		_dp = dp;
	}

	internal WeakDependencySource(WeakReference wr, DependencyProperty dp)
	{
		_item = wr;
		_dp = dp;
	}
}
