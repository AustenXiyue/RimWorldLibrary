using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace System.Windows;

internal class DescendentsWalker<T> : DescendentsWalkerBase
{
	private VisitedCallback<T> _callback;

	private T _data;

	protected T Data => _data;

	public DescendentsWalker(TreeWalkPriority priority, VisitedCallback<T> callback)
		: this(priority, callback, default(T))
	{
	}

	public DescendentsWalker(TreeWalkPriority priority, VisitedCallback<T> callback, T data)
		: base(priority)
	{
		_callback = callback;
		_data = data;
	}

	public void StartWalk(DependencyObject startNode)
	{
		StartWalk(startNode, skipStartNode: false);
	}

	public virtual void StartWalk(DependencyObject startNode, bool skipStartNode)
	{
		_startNode = startNode;
		bool flag = true;
		if (!skipStartNode)
		{
			DependencyObject startNode2 = _startNode;
			if ((startNode2 is FrameworkElement || startNode2 is FrameworkContentElement) ? true : false)
			{
				flag = _callback(_startNode, _data, _priority == TreeWalkPriority.VisualTree);
			}
		}
		if (flag)
		{
			IterateChildren(_startNode);
		}
	}

	private void IterateChildren(DependencyObject d)
	{
		_recursionDepth++;
		if (d is FrameworkElement { HasLogicalChildren: var hasLogicalChildren } frameworkElement)
		{
			if (_priority == TreeWalkPriority.VisualTree)
			{
				WalkFrameworkElementVisualThenLogicalChildren(frameworkElement, hasLogicalChildren);
			}
			else if (_priority == TreeWalkPriority.LogicalTree)
			{
				WalkFrameworkElementLogicalThenVisualChildren(frameworkElement, hasLogicalChildren);
			}
		}
		else if (d is FrameworkContentElement frameworkContentElement)
		{
			if (frameworkContentElement.HasLogicalChildren)
			{
				WalkLogicalChildren(null, frameworkContentElement, frameworkContentElement.LogicalChildren);
			}
		}
		else if (d is Visual v)
		{
			WalkVisualChildren(v);
		}
		else if (d is Visual3D v2)
		{
			WalkVisualChildren(v2);
		}
		_recursionDepth--;
	}

	private void WalkVisualChildren(Visual v)
	{
		v.IsVisualChildrenIterationInProgress = true;
		try
		{
			int internalVisual2DOr3DChildrenCount = v.InternalVisual2DOr3DChildrenCount;
			for (int i = 0; i < internalVisual2DOr3DChildrenCount; i++)
			{
				DependencyObject dependencyObject = v.InternalGet2DOr3DVisualChild(i);
				if (dependencyObject != null)
				{
					bool visitedViaVisualTree = true;
					VisitNode(dependencyObject, visitedViaVisualTree);
				}
			}
		}
		finally
		{
			v.IsVisualChildrenIterationInProgress = false;
		}
	}

	private void WalkVisualChildren(Visual3D v)
	{
		v.IsVisualChildrenIterationInProgress = true;
		try
		{
			int internalVisual2DOr3DChildrenCount = v.InternalVisual2DOr3DChildrenCount;
			for (int i = 0; i < internalVisual2DOr3DChildrenCount; i++)
			{
				DependencyObject dependencyObject = v.InternalGet2DOr3DVisualChild(i);
				if (dependencyObject != null)
				{
					bool visitedViaVisualTree = true;
					VisitNode(dependencyObject, visitedViaVisualTree);
				}
			}
		}
		finally
		{
			v.IsVisualChildrenIterationInProgress = false;
		}
	}

	private void WalkLogicalChildren(FrameworkElement feParent, FrameworkContentElement fceParent, IEnumerator logicalChildren)
	{
		if (feParent != null)
		{
			feParent.IsLogicalChildrenIterationInProgress = true;
		}
		else
		{
			fceParent.IsLogicalChildrenIterationInProgress = true;
		}
		try
		{
			if (logicalChildren == null)
			{
				return;
			}
			while (logicalChildren.MoveNext())
			{
				if (logicalChildren.Current is DependencyObject d)
				{
					bool visitedViaVisualTree = false;
					VisitNode(d, visitedViaVisualTree);
				}
			}
		}
		finally
		{
			if (feParent != null)
			{
				feParent.IsLogicalChildrenIterationInProgress = false;
			}
			else
			{
				fceParent.IsLogicalChildrenIterationInProgress = false;
			}
		}
	}

	private void WalkFrameworkElementVisualThenLogicalChildren(FrameworkElement feParent, bool hasLogicalChildren)
	{
		WalkVisualChildren(feParent);
		List<Popup> value = Popup.RegisteredPopupsField.GetValue(feParent);
		if (value != null)
		{
			foreach (Popup item in value)
			{
				bool visitedViaVisualTree = false;
				VisitNode(item, visitedViaVisualTree);
			}
		}
		feParent.IsLogicalChildrenIterationInProgress = true;
		try
		{
			if (!hasLogicalChildren)
			{
				return;
			}
			IEnumerator logicalChildren = feParent.LogicalChildren;
			if (logicalChildren == null)
			{
				return;
			}
			while (logicalChildren.MoveNext())
			{
				object current2 = logicalChildren.Current;
				if (current2 is FrameworkElement frameworkElement)
				{
					if (VisualTreeHelper.GetParent(frameworkElement) != frameworkElement.Parent)
					{
						bool visitedViaVisualTree2 = false;
						VisitNode(frameworkElement, visitedViaVisualTree2);
					}
				}
				else if (current2 is FrameworkContentElement d)
				{
					bool visitedViaVisualTree3 = false;
					VisitNode(d, visitedViaVisualTree3);
				}
			}
		}
		finally
		{
			feParent.IsLogicalChildrenIterationInProgress = false;
		}
	}

	private void WalkFrameworkElementLogicalThenVisualChildren(FrameworkElement feParent, bool hasLogicalChildren)
	{
		if (hasLogicalChildren)
		{
			WalkLogicalChildren(feParent, null, feParent.LogicalChildren);
		}
		feParent.IsVisualChildrenIterationInProgress = true;
		try
		{
			int internalVisualChildrenCount = feParent.InternalVisualChildrenCount;
			for (int i = 0; i < internalVisualChildrenCount; i++)
			{
				Visual visual = feParent.InternalGetVisualChild(i);
				if (visual != null && visual is FrameworkElement frameworkElement && VisualTreeHelper.GetParent(visual) != frameworkElement.Parent)
				{
					bool visitedViaVisualTree = true;
					VisitNode(visual, visitedViaVisualTree);
				}
			}
		}
		finally
		{
			feParent.IsVisualChildrenIterationInProgress = false;
		}
		List<Popup> value = Popup.RegisteredPopupsField.GetValue(feParent);
		if (value == null)
		{
			return;
		}
		foreach (Popup item in value)
		{
			bool visitedViaVisualTree2 = false;
			VisitNode(item, visitedViaVisualTree2);
		}
	}

	private void VisitNode(FrameworkElement fe, bool visitedViaVisualTree)
	{
		if (_recursionDepth <= ContextLayoutManager.s_LayoutRecursionLimit)
		{
			int num = _nodes.IndexOf(fe);
			if (num != -1)
			{
				_nodes.RemoveAt(num);
				return;
			}
			DependencyObject parent = VisualTreeHelper.GetParent(fe);
			DependencyObject parent2 = fe.Parent;
			if (parent != null && parent2 != null && parent != parent2)
			{
				_nodes.Add(fe);
			}
			_VisitNode(fe, visitedViaVisualTree);
			return;
		}
		throw new InvalidOperationException(SR.LogicalTreeLoop);
	}

	private void VisitNode(DependencyObject d, bool visitedViaVisualTree)
	{
		if (_recursionDepth <= ContextLayoutManager.s_LayoutRecursionLimit)
		{
			if (d is FrameworkElement fe)
			{
				VisitNode(fe, visitedViaVisualTree);
			}
			else if (d is FrameworkContentElement)
			{
				_VisitNode(d, visitedViaVisualTree);
			}
			else
			{
				IterateChildren(d);
			}
			return;
		}
		throw new InvalidOperationException(SR.LogicalTreeLoop);
	}

	protected virtual void _VisitNode(DependencyObject d, bool visitedViaVisualTree)
	{
		if (_callback(d, _data, visitedViaVisualTree))
		{
			IterateChildren(d);
		}
	}
}
