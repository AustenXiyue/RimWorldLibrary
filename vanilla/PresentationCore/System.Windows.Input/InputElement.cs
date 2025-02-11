using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MS.Internal;

namespace System.Windows.Input;

internal static class InputElement
{
	private static DependencyObjectType ContentElementType = DependencyObjectType.FromSystemTypeInternal(typeof(ContentElement));

	private static DependencyObjectType UIElementType = DependencyObjectType.FromSystemTypeInternal(typeof(UIElement));

	private static DependencyObjectType UIElement3DType = DependencyObjectType.FromSystemTypeInternal(typeof(UIElement3D));

	internal static bool IsValid(IInputElement e)
	{
		return IsValid(e as DependencyObject);
	}

	internal static bool IsValid(DependencyObject o)
	{
		if (o is UIElement || o is ContentElement || o is UIElement3D)
		{
			return true;
		}
		return false;
	}

	internal static DependencyObject GetContainingUIElement(DependencyObject o, bool onlyTraverse2D)
	{
		DependencyObject result = null;
		if (o != null)
		{
			if (o is UIElement)
			{
				result = o;
			}
			else if (o is UIElement3D && !onlyTraverse2D)
			{
				result = o;
			}
			else if (o is ContentElement contentElement)
			{
				DependencyObject parent = ContentOperations.GetParent(contentElement);
				if (parent != null)
				{
					result = GetContainingUIElement(parent, onlyTraverse2D);
				}
				else
				{
					parent = contentElement.GetUIParentCore();
					if (parent != null)
					{
						result = GetContainingUIElement(parent, onlyTraverse2D);
					}
				}
			}
			else if (o is Visual reference)
			{
				DependencyObject parent2 = VisualTreeHelper.GetParent(reference);
				if (parent2 != null)
				{
					result = GetContainingUIElement(parent2, onlyTraverse2D);
				}
			}
			else if (!onlyTraverse2D && o is Visual3D reference2)
			{
				DependencyObject parent3 = VisualTreeHelper.GetParent(reference2);
				if (parent3 != null)
				{
					result = GetContainingUIElement(parent3, onlyTraverse2D);
				}
			}
		}
		return result;
	}

	internal static DependencyObject GetContainingUIElement(DependencyObject o)
	{
		return GetContainingUIElement(o, onlyTraverse2D: false);
	}

	internal static IInputElement GetContainingInputElement(DependencyObject o, bool onlyTraverse2D)
	{
		IInputElement result = null;
		if (o != null)
		{
			if (o is UIElement uIElement)
			{
				result = uIElement;
			}
			else if (o is ContentElement contentElement)
			{
				result = contentElement;
			}
			else if (o is UIElement3D uIElement3D && !onlyTraverse2D)
			{
				result = uIElement3D;
			}
			else if (o is Visual reference)
			{
				DependencyObject parent = VisualTreeHelper.GetParent(reference);
				if (parent != null)
				{
					result = GetContainingInputElement(parent, onlyTraverse2D);
				}
			}
			else if (!onlyTraverse2D && o is Visual3D reference2)
			{
				DependencyObject parent2 = VisualTreeHelper.GetParent(reference2);
				if (parent2 != null)
				{
					result = GetContainingInputElement(parent2, onlyTraverse2D);
				}
			}
		}
		return result;
	}

	internal static IInputElement GetContainingInputElement(DependencyObject o)
	{
		return GetContainingInputElement(o, onlyTraverse2D: false);
	}

	internal static DependencyObject GetContainingVisual(DependencyObject o)
	{
		DependencyObject dependencyObject = null;
		if (o != null)
		{
			if (o is UIElement uIElement)
			{
				dependencyObject = uIElement;
			}
			else if (o is Visual3D visual3D)
			{
				dependencyObject = visual3D;
			}
			else if (o is ContentElement contentElement)
			{
				DependencyObject parent = ContentOperations.GetParent(contentElement);
				if (parent != null)
				{
					dependencyObject = GetContainingVisual(parent);
				}
				else
				{
					parent = contentElement.GetUIParentCore();
					if (parent != null)
					{
						dependencyObject = GetContainingVisual(parent);
					}
				}
			}
			else
			{
				dependencyObject = o as Visual;
				if (dependencyObject == null)
				{
					dependencyObject = o as Visual3D;
				}
			}
		}
		return dependencyObject;
	}

	internal static DependencyObject GetRootVisual(DependencyObject o)
	{
		return GetRootVisual(o, enable2DTo3DTransition: true);
	}

	internal static DependencyObject GetRootVisual(DependencyObject o, bool enable2DTo3DTransition)
	{
		DependencyObject dependencyObject = GetContainingVisual(o);
		DependencyObject parent;
		while (dependencyObject != null && (parent = VisualTreeHelper.GetParent(dependencyObject)) != null && (enable2DTo3DTransition || !(dependencyObject is Visual) || !(parent is Visual3D)))
		{
			dependencyObject = parent;
		}
		return dependencyObject;
	}

	internal static Point TranslatePoint(Point pt, DependencyObject from, DependencyObject to)
	{
		bool translated = false;
		return TranslatePoint(pt, from, to, out translated);
	}

	internal static Point TranslatePoint(Point pt, DependencyObject from, DependencyObject to, out bool translated)
	{
		translated = false;
		Point result = pt;
		DependencyObject containingVisual = GetContainingVisual(from);
		Visual visual = GetRootVisual(from) as Visual;
		Visual visual2 = containingVisual as Visual;
		if (containingVisual != null && visual2 == null)
		{
			visual2 = VisualTreeHelper.GetContainingVisual2D(containingVisual);
		}
		if (visual2 != null && visual != null)
		{
			if (visual2.TrySimpleTransformToAncestor(visual, inverse: false, out var generalTransform, out var simpleTransform))
			{
				result = simpleTransform.Transform(result);
			}
			else if (!generalTransform.TryTransform(result, out result))
			{
				return default(Point);
			}
			if (to != null)
			{
				DependencyObject containingVisual2 = GetContainingVisual(to);
				Visual visual3 = GetRootVisual(to) as Visual;
				if (containingVisual2 == null || visual3 == null)
				{
					return default(Point);
				}
				if (visual != visual3)
				{
					HwndSource hwndSource = PresentationSource.CriticalFromVisual(visual) as HwndSource;
					HwndSource hwndSource2 = PresentationSource.CriticalFromVisual(visual3) as HwndSource;
					if (hwndSource == null || hwndSource.CriticalHandle == IntPtr.Zero || hwndSource.CompositionTarget == null || hwndSource2 == null || hwndSource2.CriticalHandle == IntPtr.Zero || hwndSource2.CompositionTarget == null)
					{
						return default(Point);
					}
					result = PointUtil.RootToClient(result, hwndSource);
					result = PointUtil.ScreenToClient(PointUtil.ClientToScreen(result, hwndSource), hwndSource2);
					result = PointUtil.ClientToRoot(result, hwndSource2);
				}
				Visual visual4 = containingVisual2 as Visual;
				if (visual4 == null)
				{
					visual4 = VisualTreeHelper.GetContainingVisual2D(containingVisual2);
				}
				if (visual4.TrySimpleTransformToAncestor(visual3, inverse: true, out var generalTransform2, out var simpleTransform2))
				{
					result = simpleTransform2.Transform(result);
				}
				else
				{
					if (generalTransform2 == null)
					{
						return default(Point);
					}
					if (!generalTransform2.TryTransform(result, out result))
					{
						return default(Point);
					}
				}
			}
			translated = true;
			return result;
		}
		return default(Point);
	}
}
