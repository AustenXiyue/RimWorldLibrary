using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MS.Internal;

internal class DeferredElementTreeState
{
	private Dictionary<DependencyObject, DependencyObject> _oldCoreParents = new Dictionary<DependencyObject, DependencyObject>();

	private Dictionary<DependencyObject, DependencyObject> _oldLogicalParents = new Dictionary<DependencyObject, DependencyObject>();

	public bool IsEmpty
	{
		get
		{
			if (_oldCoreParents.Count == 0)
			{
				return _oldLogicalParents.Count == 0;
			}
			return false;
		}
	}

	public void SetCoreParent(DependencyObject element, DependencyObject parent)
	{
		if (!_oldCoreParents.ContainsKey(element))
		{
			_oldCoreParents[element] = parent;
		}
	}

	public static DependencyObject GetCoreParent(DependencyObject element, DeferredElementTreeState treeState)
	{
		DependencyObject result = null;
		if (treeState != null && treeState._oldCoreParents.ContainsKey(element))
		{
			result = treeState._oldCoreParents[element];
		}
		else if (element is Visual reference)
		{
			result = VisualTreeHelper.GetParent(reference);
		}
		else if (element is ContentElement reference2)
		{
			result = ContentOperations.GetParent(reference2);
		}
		else if (element is Visual3D reference3)
		{
			result = VisualTreeHelper.GetParent(reference3);
		}
		return result;
	}

	public static DependencyObject GetInputElementParent(DependencyObject element, DeferredElementTreeState treeState)
	{
		DependencyObject dependencyObject = element;
		do
		{
			dependencyObject = GetCoreParent(dependencyObject, treeState);
		}
		while (dependencyObject != null && !InputElement.IsValid(dependencyObject));
		return dependencyObject;
	}

	public void SetLogicalParent(DependencyObject element, DependencyObject parent)
	{
		if (!_oldLogicalParents.ContainsKey(element))
		{
			_oldLogicalParents[element] = parent;
		}
	}

	public static DependencyObject GetLogicalParent(DependencyObject element, DeferredElementTreeState treeState)
	{
		DependencyObject result = null;
		if (treeState != null && treeState._oldLogicalParents.ContainsKey(element))
		{
			result = treeState._oldLogicalParents[element];
		}
		else
		{
			if (element is UIElement uIElement)
			{
				result = uIElement.GetUIParentCore();
			}
			if (element is ContentElement contentElement)
			{
				result = contentElement.GetUIParentCore();
			}
		}
		return result;
	}

	public void Clear()
	{
		_oldCoreParents.Clear();
		_oldLogicalParents.Clear();
	}
}
