using System.Collections;
using System.Windows.Controls;

namespace System.Windows.Navigation;

internal class DisposeTreeQueueItem
{
	private object _root;

	internal object Dispatch(object o)
	{
		DisposeElement(_root);
		return null;
	}

	internal void DisposeElement(object node)
	{
		if (node is DependencyObject dependencyObject)
		{
			bool flag = false;
			IEnumerator logicalChildren = LogicalTreeHelper.GetLogicalChildren(dependencyObject);
			if (logicalChildren != null)
			{
				while (logicalChildren.MoveNext())
				{
					flag = true;
					object current = logicalChildren.Current;
					DisposeElement(current);
				}
			}
			if (!flag && dependencyObject is ContentControl { ContentIsNotLogical: not false, Content: not null } contentControl)
			{
				DisposeElement(contentControl.Content);
			}
		}
		if (node is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}

	internal DisposeTreeQueueItem(object node)
	{
		_root = node;
	}
}
