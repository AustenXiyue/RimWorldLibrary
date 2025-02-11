using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MS.Internal.PresentationCore;

namespace MS.Internal.Media;

internal static class VisualTreeUtils
{
	public const string BitmapEffectObsoleteMessage = "BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.";

	internal static void PropagateFlags(DependencyObject element, VisualFlags flags, VisualProxyFlags proxyFlags)
	{
		AsVisualInternal(element, out var visual, out var visual3D);
		if (visual != null)
		{
			Visual.PropagateFlags(visual, flags, proxyFlags);
		}
		else
		{
			Visual3D.PropagateFlags(visual3D, flags, proxyFlags);
		}
	}

	internal static void SetFlagsToRoot(DependencyObject element, bool value, VisualFlags flags)
	{
		AsVisualInternal(element, out var visual, out var visual3D);
		if (visual != null)
		{
			visual.SetFlagsToRoot(value, flags);
		}
		else
		{
			visual3D?.SetFlagsToRoot(value, flags);
		}
	}

	internal static DependencyObject FindFirstAncestorWithFlagsAnd(DependencyObject element, VisualFlags flags)
	{
		AsVisualInternal(element, out var visual, out var visual3D);
		if (visual != null)
		{
			return visual.FindFirstAncestorWithFlagsAnd(flags);
		}
		return visual3D?.FindFirstAncestorWithFlagsAnd(flags);
	}

	internal static PointHitTestResult AsNearestPointHitTestResult(HitTestResult result)
	{
		if (result == null)
		{
			return null;
		}
		if (result is PointHitTestResult result2)
		{
			return result2;
		}
		if (result is RayHitTestResult { VisualHit: var visual3D } rayHitTestResult)
		{
			Matrix3D identity = Matrix3D.Identity;
			while (true)
			{
				if (visual3D.Transform != null)
				{
					identity.Append(visual3D.Transform.Value);
				}
				if (!(visual3D.InternalVisualParent is Visual3D visual3D2))
				{
					break;
				}
				visual3D = visual3D2;
			}
			if (visual3D.InternalVisualParent is Viewport3DVisual viewport3DVisual)
			{
				Point4D point = (Point4D)rayHitTestResult.PointHit * identity;
				Point pointHit = viewport3DVisual.WorldToViewport(point);
				return new PointHitTestResult(viewport3DVisual, pointHit);
			}
			return null;
		}
		return null;
	}

	internal static void EnsureNonNullVisual(DependencyObject element)
	{
		EnsureVisual(element, allowNull: false);
	}

	internal static void EnsureVisual(DependencyObject element)
	{
		EnsureVisual(element, allowNull: true);
	}

	private static void EnsureVisual(DependencyObject element, bool allowNull)
	{
		if (element == null)
		{
			if (!allowNull)
			{
				throw new ArgumentNullException("element");
			}
			return;
		}
		if (!(element is Visual) && !(element is Visual3D))
		{
			throw new ArgumentException(SR.Visual_NotAVisual);
		}
		element.VerifyAccess();
	}

	internal static void AsNonNullVisual(DependencyObject element, out Visual visual, out Visual3D visual3D)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		AsVisual(element, out visual, out visual3D);
	}

	internal static void AsVisual(DependencyObject element, out Visual visual, out Visual3D visual3D)
	{
		bool flag = AsVisualHelper(element, out visual, out visual3D);
		if (element != null)
		{
			if (!flag)
			{
				throw new InvalidOperationException(SR.Format(SR.Visual_NotAVisual, element.GetType()));
			}
			element.VerifyAccess();
		}
	}

	internal static bool AsVisualInternal(DependencyObject element, out Visual visual, out Visual3D visual3D)
	{
		bool num = AsVisualHelper(element, out visual, out visual3D);
		if (!num)
		{
		}
		return num;
	}

	private static bool AsVisualHelper(DependencyObject element, out Visual visual, out Visual3D visual3D)
	{
		if (element is Visual visual2)
		{
			visual = visual2;
			visual3D = null;
			return true;
		}
		if (element is Visual3D visual3D2)
		{
			visual = null;
			visual3D = visual3D2;
			return true;
		}
		visual = null;
		visual3D = null;
		return false;
	}
}
