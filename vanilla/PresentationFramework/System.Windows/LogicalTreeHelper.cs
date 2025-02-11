using System.Collections;
using MS.Internal.Controls;

namespace System.Windows;

/// <summary>Provides static helper methods for querying objects in the logical tree.</summary>
public static class LogicalTreeHelper
{
	private class EnumeratorWrapper : IEnumerable
	{
		private IEnumerator _enumerator;

		private static EnumeratorWrapper _emptyInstance;

		internal static EnumeratorWrapper Empty
		{
			get
			{
				if (_emptyInstance == null)
				{
					_emptyInstance = new EnumeratorWrapper(null);
				}
				return _emptyInstance;
			}
		}

		public EnumeratorWrapper(IEnumerator enumerator)
		{
			if (enumerator != null)
			{
				_enumerator = enumerator;
			}
			else
			{
				_enumerator = EmptyEnumerator.Instance;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _enumerator;
		}
	}

	/// <summary>Attempts to find and return an object that has the specified name. The search starts from the specified object and continues into subnodes of the logical tree. </summary>
	/// <returns>The object with the matching name, if one is found; returns null if no matching name was found in the logical tree.</returns>
	/// <param name="logicalTreeNode">The object to start searching from. This object must be either a <see cref="T:System.Windows.FrameworkElement" /> or a <see cref="T:System.Windows.FrameworkContentElement" />.</param>
	/// <param name="elementName">The name of the object to find.</param>
	public static DependencyObject FindLogicalNode(DependencyObject logicalTreeNode, string elementName)
	{
		if (logicalTreeNode == null)
		{
			throw new ArgumentNullException("logicalTreeNode");
		}
		if (elementName == null)
		{
			throw new ArgumentNullException("elementName");
		}
		if (elementName == string.Empty)
		{
			throw new ArgumentException(SR.StringEmpty, "elementName");
		}
		DependencyObject dependencyObject = null;
		DependencyObject dependencyObject2 = null;
		if (logicalTreeNode is IFrameworkInputElement frameworkInputElement && frameworkInputElement.Name == elementName)
		{
			dependencyObject = logicalTreeNode;
		}
		if (dependencyObject == null)
		{
			IEnumerator enumerator = null;
			enumerator = GetLogicalChildren(logicalTreeNode);
			if (enumerator != null)
			{
				enumerator.Reset();
				while (dependencyObject == null && enumerator.MoveNext())
				{
					if (enumerator.Current is DependencyObject logicalTreeNode2)
					{
						dependencyObject = FindLogicalNode(logicalTreeNode2, elementName);
					}
				}
			}
		}
		return dependencyObject;
	}

	/// <summary>Returns the parent object of the specified object by processing the logical tree.</summary>
	/// <returns>The requested parent object.</returns>
	/// <param name="current">The object to find the parent object for. This is expected to be either a <see cref="T:System.Windows.FrameworkElement" /> or a <see cref="T:System.Windows.FrameworkContentElement" />.</param>
	public static DependencyObject GetParent(DependencyObject current)
	{
		if (current == null)
		{
			throw new ArgumentNullException("current");
		}
		if (current is FrameworkElement frameworkElement)
		{
			return frameworkElement.Parent;
		}
		if (current is FrameworkContentElement frameworkContentElement)
		{
			return frameworkContentElement.Parent;
		}
		return null;
	}

	/// <summary>Returns the collection of immediate child objects of the specified object, by processing the logical tree.</summary>
	/// <returns>The enumerable collection of immediate child objects from the logical tree of the specified object.</returns>
	/// <param name="current">The object from which to start processing the logical tree. This is expected to be either a <see cref="T:System.Windows.FrameworkElement" /> or <see cref="T:System.Windows.FrameworkContentElement" />.</param>
	public static IEnumerable GetChildren(DependencyObject current)
	{
		if (current == null)
		{
			throw new ArgumentNullException("current");
		}
		if (current is FrameworkElement frameworkElement)
		{
			return new EnumeratorWrapper(frameworkElement.LogicalChildren);
		}
		if (current is FrameworkContentElement frameworkContentElement)
		{
			return new EnumeratorWrapper(frameworkContentElement.LogicalChildren);
		}
		return EnumeratorWrapper.Empty;
	}

	/// <summary>Returns the collection of immediate child objects of the specified <see cref="T:System.Windows.FrameworkElement" /> by processing the logical tree. </summary>
	/// <returns>The enumerable collection of immediate child objects starting from <paramref name="current" /> in the logical tree.</returns>
	/// <param name="current">The object from which to start processing the logical tree.</param>
	public static IEnumerable GetChildren(FrameworkElement current)
	{
		if (current == null)
		{
			throw new ArgumentNullException("current");
		}
		return new EnumeratorWrapper(current.LogicalChildren);
	}

	/// <summary>Returns the collection of immediate child objects of the specified <see cref="T:System.Windows.FrameworkContentElement" /> by processing the logical tree. </summary>
	/// <returns>The enumerable collection of immediate child objects starting from <paramref name="current" /> in the logical tree.</returns>
	/// <param name="current">The object from which to start processing the logical tree.</param>
	public static IEnumerable GetChildren(FrameworkContentElement current)
	{
		if (current == null)
		{
			throw new ArgumentNullException("current");
		}
		return new EnumeratorWrapper(current.LogicalChildren);
	}

	/// <summary>Attempts to bring the requested UI element into view and raises the <see cref="E:System.Windows.FrameworkElement.RequestBringIntoView" /> event on the target in order to report the results.</summary>
	/// <param name="current">The UI element to bring into view.</param>
	public static void BringIntoView(DependencyObject current)
	{
		if (current == null)
		{
			throw new ArgumentNullException("current");
		}
		if (current is FrameworkElement frameworkElement)
		{
			frameworkElement.BringIntoView();
		}
		if (current is FrameworkContentElement frameworkContentElement)
		{
			frameworkContentElement.BringIntoView();
		}
	}

	internal static void AddLogicalChild(DependencyObject parent, object child)
	{
		if (child != null && parent != null)
		{
			if (parent is FrameworkElement frameworkElement)
			{
				frameworkElement.AddLogicalChild(child);
			}
			else if (parent is FrameworkContentElement frameworkContentElement)
			{
				frameworkContentElement.AddLogicalChild(child);
			}
		}
	}

	internal static void AddLogicalChild(FrameworkElement parentFE, FrameworkContentElement parentFCE, object child)
	{
		if (child != null)
		{
			if (parentFE != null)
			{
				parentFE.AddLogicalChild(child);
			}
			else
			{
				parentFCE?.AddLogicalChild(child);
			}
		}
	}

	internal static void RemoveLogicalChild(DependencyObject parent, object child)
	{
		if (child != null && parent != null)
		{
			if (parent is FrameworkElement frameworkElement)
			{
				frameworkElement.RemoveLogicalChild(child);
			}
			else if (parent is FrameworkContentElement frameworkContentElement)
			{
				frameworkContentElement.RemoveLogicalChild(child);
			}
		}
	}

	internal static void RemoveLogicalChild(FrameworkElement parentFE, FrameworkContentElement parentFCE, object child)
	{
		if (child != null)
		{
			if (parentFE != null)
			{
				parentFE.RemoveLogicalChild(child);
			}
			else
			{
				parentFCE.RemoveLogicalChild(child);
			}
		}
	}

	internal static IEnumerator GetLogicalChildren(DependencyObject current)
	{
		if (current is FrameworkElement frameworkElement)
		{
			return frameworkElement.LogicalChildren;
		}
		if (current is FrameworkContentElement frameworkContentElement)
		{
			return frameworkContentElement.LogicalChildren;
		}
		return EmptyEnumerator.Instance;
	}
}
