using System;
using System.Collections;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Media;

namespace MS.Internal.Annotations.Anchoring;

internal sealed class PathNode
{
	internal static readonly DependencyProperty HiddenParentProperty = DependencyProperty.RegisterAttached("HiddenParent", typeof(DependencyObject), typeof(PathNode));

	private DependencyObject _node;

	private ArrayList _children = new ArrayList(1);

	public DependencyObject Node => _node;

	public IList Children => _children;

	internal PathNode(DependencyObject node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		_node = node;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PathNode pathNode))
		{
			return false;
		}
		return _node.Equals(pathNode.Node);
	}

	public override int GetHashCode()
	{
		if (_node == null)
		{
			return base.GetHashCode();
		}
		return _node.GetHashCode();
	}

	internal static PathNode BuildPathForElements(ICollection nodes)
	{
		if (nodes == null)
		{
			throw new ArgumentNullException("nodes");
		}
		PathNode pathNode = null;
		foreach (DependencyObject node in nodes)
		{
			PathNode pathNode2 = BuildPathForElement(node);
			if (pathNode == null)
			{
				pathNode = pathNode2;
			}
			else
			{
				AddBranchToPath(pathNode, pathNode2);
			}
		}
		pathNode?.FreezeChildren();
		return pathNode;
	}

	internal static DependencyObject GetParent(DependencyObject node)
	{
		DependencyObject dependencyObject = node;
		DependencyObject dependencyObject2;
		while (true)
		{
			dependencyObject2 = (DependencyObject)dependencyObject.GetValue(HiddenParentProperty);
			if (dependencyObject2 == null && dependencyObject is Visual reference)
			{
				dependencyObject2 = VisualTreeHelper.GetParent(reference);
			}
			if (dependencyObject2 == null)
			{
				dependencyObject2 = LogicalTreeHelper.GetParent(dependencyObject);
			}
			if ((dependencyObject2 == null || dependencyObject2 is FrameworkElement || dependencyObject2 is FrameworkContentElement) ? true : false)
			{
				break;
			}
			dependencyObject = dependencyObject2;
		}
		return dependencyObject2;
	}

	private static PathNode BuildPathForElement(DependencyObject node)
	{
		PathNode pathNode = null;
		while (node != null)
		{
			PathNode pathNode2 = new PathNode(node);
			if (pathNode != null)
			{
				pathNode2.AddChild(pathNode);
			}
			pathNode = pathNode2;
			if (node.ReadLocalValue(AnnotationService.ServiceProperty) != DependencyProperty.UnsetValue)
			{
				break;
			}
			node = GetParent(node);
		}
		return pathNode;
	}

	private static PathNode AddBranchToPath(PathNode path, PathNode branch)
	{
		PathNode pathNode = path;
		PathNode pathNode2 = branch;
		while (pathNode.Node.Equals(pathNode2.Node) && pathNode2._children.Count > 0)
		{
			bool flag = false;
			PathNode pathNode3 = (PathNode)pathNode2._children[0];
			foreach (PathNode child in pathNode._children)
			{
				if (child.Equals(pathNode3))
				{
					flag = true;
					pathNode2 = pathNode3;
					pathNode = child;
					break;
				}
			}
			if (!flag)
			{
				pathNode.AddChild(pathNode3);
				break;
			}
		}
		return path;
	}

	private void AddChild(object child)
	{
		_children.Add(child);
	}

	private void FreezeChildren()
	{
		foreach (PathNode child in _children)
		{
			child.FreezeChildren();
		}
		_children = ArrayList.ReadOnly(_children);
	}
}
