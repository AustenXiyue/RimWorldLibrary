using System.Windows.Media;
using MS.Utility;

namespace System.Windows;

internal class DescendentsWalkerBase
{
	internal DependencyObject _startNode;

	internal TreeWalkPriority _priority;

	internal FrugalStructList<DependencyObject> _nodes;

	internal int _recursionDepth;

	protected DescendentsWalkerBase(TreeWalkPriority priority)
	{
		_startNode = null;
		_priority = priority;
		_recursionDepth = 0;
		_nodes = default(FrugalStructList<DependencyObject>);
	}

	internal bool WasVisited(DependencyObject d)
	{
		DependencyObject dependencyObject = d;
		while (dependencyObject != _startNode && dependencyObject != null)
		{
			DependencyObject dependencyObject2;
			if (dependencyObject is FrameworkElement frameworkElement)
			{
				dependencyObject2 = frameworkElement.Parent;
				DependencyObject parent = VisualTreeHelper.GetParent(frameworkElement);
				if (parent != null && dependencyObject2 != null && parent != dependencyObject2)
				{
					return _nodes.Contains(dependencyObject);
				}
				if (parent != null)
				{
					dependencyObject = parent;
					continue;
				}
			}
			else
			{
				dependencyObject2 = ((dependencyObject is FrameworkContentElement frameworkContentElement) ? frameworkContentElement.Parent : null);
			}
			dependencyObject = dependencyObject2;
		}
		return dependencyObject != null;
	}
}
