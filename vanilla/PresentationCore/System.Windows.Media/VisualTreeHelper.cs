using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using MS.Internal;
using MS.Internal.Media;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Provides utility methods that perform common tasks involving nodes in a visual tree.</summary>
public static class VisualTreeHelper
{
	private static void CheckVisualReferenceArgument(DependencyObject reference)
	{
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
	}

	[FriendAccessAllowed]
	internal static bool IsVisualType(DependencyObject reference)
	{
		if (!(reference is Visual))
		{
			return reference is Visual3D;
		}
		return true;
	}

	/// <summary>Returns the number of children that the specified visual object contains.</summary>
	/// <returns>The number of child visuals that the parent visual contains.</returns>
	/// <param name="reference">The parent visual that is referenced as a <see cref="T:System.Windows.DependencyObject" />.</param>
	public static int GetChildrenCount(DependencyObject reference)
	{
		VisualTreeUtils.AsNonNullVisual(reference, out var visual, out var visual3D);
		return visual3D?.InternalVisual2DOr3DChildrenCount ?? visual.InternalVisual2DOr3DChildrenCount;
	}

	/// <summary>Returns the child visual object from the specified collection index within a specified parent.</summary>
	/// <returns>The index value of the child visual object.</returns>
	/// <param name="reference">The parent visual, referenced as a <see cref="T:System.Windows.DependencyObject" />.</param>
	/// <param name="childIndex">The index that represents the child visual that is contained by <paramref name="reference" />.</param>
	public static DependencyObject GetChild(DependencyObject reference, int childIndex)
	{
		VisualTreeUtils.AsNonNullVisual(reference, out var visual, out var visual3D);
		if (visual3D != null)
		{
			return visual3D.InternalGet2DOr3DVisualChild(childIndex);
		}
		return visual.InternalGet2DOr3DVisualChild(childIndex);
	}

	public static DpiScale GetDpi(Visual visual)
	{
		return visual.GetDpi();
	}

	public static void SetRootDpi(Visual visual, DpiScale dpiInfo)
	{
		if ((object)dpiInfo == null)
		{
			throw new NullReferenceException("dpiInfo cannot be null");
		}
		if (visual.InternalVisualParent != null)
		{
			throw new InvalidOperationException("UpdateDPI should only be called on the root of a Visual tree");
		}
		DpiFlags dpiFlags = DpiUtil.UpdateDpiScalesAndGetIndex(dpiInfo.PixelsPerInchX, dpiInfo.PixelsPerInchY);
		visual.RecursiveSetDpiScaleVisualFlags(new DpiRecursiveChangeArgs(dpiFlags, visual.GetDpi(), dpiInfo));
	}

	/// <summary>Returns a <see cref="T:System.Windows.DependencyObject" /> value that represents the parent of the visual object.</summary>
	/// <returns>The parent of the visual.</returns>
	/// <param name="reference">The visual whose parent is returned.</param>
	public static DependencyObject GetParent(DependencyObject reference)
	{
		VisualTreeUtils.AsNonNullVisual(reference, out var visual, out var visual3D);
		if (visual3D != null)
		{
			return visual3D.InternalVisualParent;
		}
		return visual.InternalVisualParent;
	}

	[FriendAccessAllowed]
	internal static DependencyObject GetParentInternal(DependencyObject reference)
	{
		VisualTreeUtils.AsVisualInternal(reference, out var visual, out var visual3D);
		if (visual != null)
		{
			return visual.InternalVisualParent;
		}
		return visual3D?.InternalVisualParent;
	}

	internal static Visual GetContainingVisual2D(DependencyObject reference)
	{
		Visual visual = null;
		while (reference != null)
		{
			visual = reference as Visual;
			if (visual != null)
			{
				break;
			}
			reference = GetParent(reference);
		}
		return visual;
	}

	internal static Visual3D GetContainingVisual3D(DependencyObject reference)
	{
		Visual3D visual3D = null;
		while (reference != null)
		{
			visual3D = reference as Visual3D;
			if (visual3D != null)
			{
				break;
			}
			reference = GetParent(reference);
		}
		return visual3D;
	}

	internal static bool IsAncestorOf(DependencyObject reference, DependencyObject descendant)
	{
		VisualTreeUtils.AsNonNullVisual(reference, out var visual, out var visual3D);
		return visual3D?.IsAncestorOf(descendant) ?? visual.IsAncestorOf(descendant);
	}

	internal static bool IsAncestorOf(DependencyObject ancestor, DependencyObject descendant, Type stopType)
	{
		if (ancestor == null)
		{
			throw new ArgumentNullException("ancestor");
		}
		if (descendant == null)
		{
			throw new ArgumentNullException("descendant");
		}
		VisualTreeUtils.EnsureVisual(ancestor);
		VisualTreeUtils.EnsureVisual(descendant);
		DependencyObject dependencyObject = descendant;
		while (dependencyObject != null && dependencyObject != ancestor && !stopType.IsInstanceOfType(dependencyObject))
		{
			dependencyObject = ((dependencyObject is Visual visual) ? visual.InternalVisualParent : ((!(dependencyObject is Visual3D visual3D)) ? null : visual3D.InternalVisualParent));
		}
		return dependencyObject == ancestor;
	}

	internal static DependencyObject FindCommonAncestor(DependencyObject reference, DependencyObject otherVisual)
	{
		VisualTreeUtils.AsNonNullVisual(reference, out var visual, out var visual3D);
		if (visual3D != null)
		{
			return visual3D.FindCommonVisualAncestor(otherVisual);
		}
		return visual.FindCommonVisualAncestor(otherVisual);
	}

	/// <summary>Return the clip region of the specified <see cref="T:System.Windows.Media.Visual" /> as a <see cref="T:System.Windows.Media.Geometry" /> value.</summary>
	/// <returns>The clip region value of the <see cref="T:System.Windows.Media.Visual" /> returned as a <see cref="T:System.Windows.Media.Geometry" /> type.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> whose clip region value is returned.</param>
	public static Geometry GetClip(Visual reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualClip;
	}

	/// <summary>Returns the opacity of the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>A value of type <see cref="T:System.Double" /> that represents the opacity value of the <see cref="T:System.Windows.Media.Visual" />.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> whose opacity value is returned.</param>
	public static double GetOpacity(Visual reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualOpacity;
	}

	/// <summary>Returns a <see cref="T:System.Windows.Media.Brush" /> value that represents the opacity mask of the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.Media.Brush" /> that represents the opacity mask value of the <see cref="T:System.Windows.Media.Visual" />.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> whose opacity mask value is returned.</param>
	public static Brush GetOpacityMask(Visual reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualOpacityMask;
	}

	/// <summary>Returns the offset of the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Vector" /> that represents the offset value of the <see cref="T:System.Windows.Media.Visual" />.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> whose offset is returned.</param>
	public static Vector GetOffset(Visual reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualOffset;
	}

	/// <summary>Returns a <see cref="T:System.Windows.Media.Transform" /> value for the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The transform value of the <see cref="T:System.Windows.Media.Visual" />, or null if <paramref name="reference" /> does not have a transform defined.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> whose transform value is returned.</param>
	public static Transform GetTransform(Visual reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualTransform;
	}

	/// <summary>Returns an X-coordinate (vertical) guideline collection.</summary>
	/// <returns>The X-coordinate guideline collection of the <see cref="T:System.Windows.Media.Visual" />.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> whose X-coordinate guideline collection is returned.</param>
	public static DoubleCollection GetXSnappingGuidelines(Visual reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualXSnappingGuidelines;
	}

	/// <summary>Returns a Y-coordinate (horizontal) guideline collection.</summary>
	/// <returns>The Y-coordinate guideline collection of the <see cref="T:System.Windows.Media.Visual" />.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> whose Y-coordinate guideline collection is returned.</param>
	public static DoubleCollection GetYSnappingGuidelines(Visual reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualYSnappingGuidelines;
	}

	/// <summary>Returns the drawing content of the specified <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The drawing content of the <see cref="T:System.Windows.Media.Visual" /> returned as a <see cref="T:System.Windows.Media.DrawingGroup" /> type.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> whose drawing content is returned.</param>
	public static DrawingGroup GetDrawing(Visual reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.GetDrawing();
	}

	/// <summary>Returns the cached bounding box rectangle for the specified <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The bounding box rectangle for the <see cref="T:System.Windows.Media.Visual" />.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> whose bounding box value is computed.</param>
	public static Rect GetContentBounds(Visual reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualContentBounds;
	}

	/// <summary>Returns the cached bounding box rectangle for the specified <see cref="T:System.Windows.Media.Media3D.Visual3D" />.</summary>
	/// <returns>The bounding box 3D rectangle for the <see cref="T:System.Windows.Media.Media3D.Visual3D" />.</returns>
	/// <param name="reference">The 3D visual whose bounding box value is computed.</param>
	public static Rect3D GetContentBounds(Visual3D reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualContentBounds;
	}

	/// <summary>Returns the union of all the content bounding boxes for all the descendants of the <see cref="T:System.Windows.Media.Visual" />, which includes the content bounding box of the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The bounding box rectangle for the specified <see cref="T:System.Windows.Media.Visual" />.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> whose bounding box value for all descendants is computed.</param>
	public static Rect GetDescendantBounds(Visual reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualDescendantBounds;
	}

	/// <summary>Returns the union of all the content bounding boxes for all the descendants of the specified <see cref="T:System.Windows.Media.Media3D.Visual3D" />, which includes the content bounding box of the <see cref="T:System.Windows.Media.Media3D.Visual3D" />.</summary>
	/// <returns>Returns the bounding box 3D rectangle for the 3D visual.</returns>
	/// <param name="reference">The 3D visual whose bounding box value for all descendants is computed.</param>
	public static Rect3D GetDescendantBounds(Visual3D reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualDescendantBounds;
	}

	/// <summary>Returns the <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> value for the specified <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> for this <see cref="T:System.Windows.Media.Visual" />.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> that contains the bitmap effect.</param>
	public static BitmapEffect GetBitmapEffect(Visual reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualBitmapEffect;
	}

	/// <summary>Returns the <see cref="T:System.Windows.Media.Effects.BitmapEffectInput" /> value for the specified <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Effects.BitmapEffectInput" /> for this <see cref="T:System.Windows.Media.Visual" />.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> that contains the bitmap effect input value.</param>
	public static BitmapEffectInput GetBitmapEffectInput(Visual reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualBitmapEffectInput;
	}

	/// <summary>Gets the bitmap effect for the specified <see cref="T:System.Windows.Media.Visual" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Effects.Effect" /> applied to <paramref name="reference" />.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> to get the bitmap effect for.</param>
	public static Effect GetEffect(Visual reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualEffect;
	}

	/// <summary>Retrieves the cached representation of the specified <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.CacheMode" /> for <paramref name="reference" />.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> to get the <see cref="T:System.Windows.Media.CacheMode" /> for.</param>
	public static CacheMode GetCacheMode(Visual reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualCacheMode;
	}

	/// <summary>Returns the edge mode of the specified <see cref="T:System.Windows.Media.Visual" /> as an <see cref="T:System.Windows.Media.EdgeMode" /> value.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.EdgeMode" /> value of the <see cref="T:System.Windows.Media.Visual" />.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> whose edge mode value is returned.</param>
	public static EdgeMode GetEdgeMode(Visual reference)
	{
		CheckVisualReferenceArgument(reference);
		return reference.VisualEdgeMode;
	}

	/// <summary>Returns the topmost <see cref="T:System.Windows.Media.Visual" /> object of a hit test by specifying a <see cref="T:System.Windows.Point" />.</summary>
	/// <returns>The hit test result of the <see cref="T:System.Windows.Media.Visual" />, returned as a <see cref="T:System.Windows.Media.HitTestResult" /> type.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> to hit test.</param>
	/// <param name="point">The point value to hit test against.</param>
	public static HitTestResult HitTest(Visual reference, Point point)
	{
		return HitTest(reference, point, include2DOn3D: true);
	}

	[FriendAccessAllowed]
	internal static HitTestResult HitTest(Visual reference, Point point, bool include2DOn3D)
	{
		CheckVisualReferenceArgument(reference);
		return reference.HitTest(point, include2DOn3D);
	}

	/// <summary>Initiates a hit test on the specified <see cref="T:System.Windows.Media.Visual" />, with caller-defined <see cref="T:System.Windows.Media.HitTestFilterCallback" /> and <see cref="T:System.Windows.Media.HitTestResultCallback" /> methods.</summary>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Visual" /> to hit test.</param>
	/// <param name="filterCallback">The method that represents the hit test filter callback value.</param>
	/// <param name="resultCallback">The method that represents the hit test result callback value.</param>
	/// <param name="hitTestParameters">The parameter value to hit test against.</param>
	public static void HitTest(Visual reference, HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, HitTestParameters hitTestParameters)
	{
		CheckVisualReferenceArgument(reference);
		reference.HitTest(filterCallback, resultCallback, hitTestParameters);
	}

	/// <summary>Initiates a hit test on the specified <see cref="T:System.Windows.Media.Media3D.Visual3D" />, with caller-defined <see cref="T:System.Windows.Media.HitTestFilterCallback" /> and <see cref="T:System.Windows.Media.HitTestResultCallback" /> methods.</summary>
	/// <param name="reference">The <see cref="T:System.Windows.Media.Media3D.Visual3D" /> to hit test.</param>
	/// <param name="filterCallback">The method that represents the hit test filter callback value.</param>
	/// <param name="resultCallback">The method that represents the hit test result callback value.</param>
	/// <param name="hitTestParameters">The 3D parameter value to hit test against.</param>
	public static void HitTest(Visual3D reference, HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, HitTestParameters3D hitTestParameters)
	{
		CheckVisualReferenceArgument(reference);
		reference.HitTest(filterCallback, resultCallback, hitTestParameters);
	}
}
