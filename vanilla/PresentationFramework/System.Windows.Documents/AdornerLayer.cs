using System.Collections;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.Media;

namespace System.Windows.Documents;

/// <summary>Represents a surface for rendering adorners.</summary>
public class AdornerLayer : FrameworkElement
{
	internal class AdornerInfo
	{
		private Adorner _adorner;

		private Size _computedSize;

		private GeneralTransform _transform;

		private int _zOrder;

		private Geometry _clip;

		internal Adorner Adorner => _adorner;

		internal Size RenderSize
		{
			get
			{
				return _computedSize;
			}
			set
			{
				_computedSize = value;
			}
		}

		internal GeneralTransform Transform
		{
			get
			{
				return _transform;
			}
			set
			{
				_transform = value;
			}
		}

		internal int ZOrder
		{
			get
			{
				return _zOrder;
			}
			set
			{
				_zOrder = value;
			}
		}

		internal Geometry Clip
		{
			get
			{
				return _clip;
			}
			set
			{
				_clip = value;
			}
		}

		internal AdornerInfo(Adorner adorner)
		{
			Invariant.Assert(adorner != null);
			_adorner = adorner;
		}
	}

	private HybridDictionary _elementMap = new HybridDictionary(10);

	private SortedList _zOrderMap = new SortedList(10);

	private const int DefaultZOrder = int.MaxValue;

	private VisualCollection _children;

	/// <summary>Gets the number of child <see cref="T:System.Windows.Media.Visual" /> objects in this instance of <see cref="T:System.Windows.Documents.AdornerLayer" />.</summary>
	/// <returns>The number of child <see cref="T:System.Windows.Media.Visual" /> objects in this instance of <see cref="T:System.Windows.Documents.AdornerLayer" />.</returns>
	protected override int VisualChildrenCount => _children.Count;

	/// <summary>Gets an enumerator that can iterate the logical child elements of this <see cref="T:System.Windows.Documents.AdornerLayer" /> element. </summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" />. This property has no default value.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			if (VisualChildrenCount == 0)
			{
				return EmptyEnumerator.Instance;
			}
			return _children.GetEnumerator();
		}
	}

	internal HybridDictionary ElementMap => _elementMap;

	internal override int EffectiveValuesInitialSize => 4;

	internal AdornerLayer()
		: this(Dispatcher.CurrentDispatcher)
	{
	}

	internal AdornerLayer(Dispatcher context)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		base.LayoutUpdated += OnLayoutUpdated;
		_children = new VisualCollection(this);
	}

	/// <summary>Adds an adorner to the adorner layer.</summary>
	/// <param name="adorner">The adorner to add.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when adorner is null.</exception>
	public void Add(Adorner adorner)
	{
		Add(adorner, int.MaxValue);
	}

	/// <summary>Removes the specified <see cref="T:System.Windows.Documents.Adorner" /> from the adorner layer.</summary>
	/// <param name="adorner">The <see cref="T:System.Windows.Documents.Adorner" /> to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when adorner is null.</exception>
	public void Remove(Adorner adorner)
	{
		if (adorner == null)
		{
			throw new ArgumentNullException("adorner");
		}
		if (ElementMap[adorner.AdornedElement] is ArrayList adornerInfos)
		{
			AdornerInfo adornerInfo = GetAdornerInfo(adornerInfos, adorner);
			if (adornerInfo != null)
			{
				RemoveAdornerInfo(ElementMap, adorner, adorner.AdornedElement);
				RemoveAdornerInfo(_zOrderMap, adorner, adornerInfo.ZOrder);
				_children.Remove(adorner);
				RemoveLogicalChild(adorner);
			}
		}
	}

	/// <summary>Updates the layout and redraws all of the adorners in the adorner layer.</summary>
	public void Update()
	{
		foreach (UIElement key in ElementMap.Keys)
		{
			ArrayList arrayList = (ArrayList)ElementMap[key];
			int num = 0;
			if (arrayList != null)
			{
				while (num < arrayList.Count)
				{
					InvalidateAdorner((AdornerInfo)arrayList[num++]);
				}
			}
		}
		UpdateAdorner(null);
	}

	/// <summary>Updates the layout and redraws all of the adorners in the adorner layer that are bound to the specified <see cref="T:System.Windows.UIElement" />.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> associated with the adorners to update.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when element is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">Raised when the specified element cannot be found.</exception>
	public void Update(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (!(ElementMap[element] is ArrayList arrayList))
		{
			throw new InvalidOperationException(SR.AdornedElementNotFound);
		}
		int num = 0;
		while (num < arrayList.Count)
		{
			InvalidateAdorner((AdornerInfo)arrayList[num++]);
		}
		UpdateAdorner(element);
	}

	/// <summary>Returns an array of adorners that are bound to the specified <see cref="T:System.Windows.UIElement" />.</summary>
	/// <returns>An array of adorners that decorate the specified <see cref="T:System.Windows.UIElement" />, or null if there are no adorners bound to the specified element.</returns>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> to retrieve an array of adorners for.</param>
	public Adorner[] GetAdorners(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (!(ElementMap[element] is ArrayList { Count: not 0 } arrayList))
		{
			return null;
		}
		Adorner[] array = new Adorner[arrayList.Count];
		for (int i = 0; i < arrayList.Count; i++)
		{
			array[i] = ((AdornerInfo)arrayList[i]).Adorner;
		}
		return array;
	}

	/// <summary>Gets an <see cref="T:System.Windows.Media.AdornerHitTestResult" /> for a specified point.</summary>
	/// <returns>An <see cref="T:System.Windows.Media.AdornerHitTestResult" /> for the specified point.</returns>
	/// <param name="point">The point to hit test.</param>
	public AdornerHitTestResult AdornerHitTest(Point point)
	{
		PointHitTestResult pointHitTestResult = VisualTreeUtils.AsNearestPointHitTestResult(VisualTreeHelper.HitTest(this, point, include2DOn3D: false));
		if (pointHitTestResult != null && pointHitTestResult.VisualHit != null)
		{
			for (Visual visual = pointHitTestResult.VisualHit; visual != this; visual = (Visual)VisualTreeHelper.GetParent(visual))
			{
				if (visual is Adorner)
				{
					return new AdornerHitTestResult(pointHitTestResult.VisualHit, pointHitTestResult.PointHit, visual as Adorner);
				}
			}
			return null;
		}
		return null;
	}

	/// <summary>Returns the first adorner layer in the visual tree above a specified <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>An adorner layer for the specified visual, or null if no adorner layer can be found.</returns>
	/// <param name="visual">The visual element for which to find an adorner layer.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when visual is null.</exception>
	public static AdornerLayer GetAdornerLayer(Visual visual)
	{
		if (visual == null)
		{
			throw new ArgumentNullException("visual");
		}
		for (Visual visual2 = VisualTreeHelper.GetParent(visual) as Visual; visual2 != null; visual2 = VisualTreeHelper.GetParent(visual2) as Visual)
		{
			if (visual2 is AdornerDecorator)
			{
				return ((AdornerDecorator)visual2).AdornerLayer;
			}
			if (visual2 is ScrollContentPresenter)
			{
				return ((ScrollContentPresenter)visual2).AdornerLayer;
			}
		}
		return null;
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.Visual" /> child at the specified <paramref name="index" /> position.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Visual" /> child of the parent <see cref="T:System.Windows.Documents.AdornerLayer" /> element.</returns>
	/// <param name="index">The index position of the wanted <see cref="T:System.Windows.Media.Visual" /> child.</param>
	protected override Visual GetVisualChild(int index)
	{
		return _children[index];
	}

	/// <summary>Measures the size required for child elements and determines a size for the <see cref="T:System.Windows.Documents.AdornerLayer" />.</summary>
	/// <returns>This method always returns a <see cref="T:System.Windows.Size" /> of (0,0).</returns>
	/// <param name="constraint">Unused.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		DictionaryEntry[] array = new DictionaryEntry[_zOrderMap.Count];
		_zOrderMap.CopyTo(array, 0);
		for (int i = 0; i < array.Length; i++)
		{
			ArrayList arrayList = (ArrayList)array[i].Value;
			int num = 0;
			while (num < arrayList.Count)
			{
				((AdornerInfo)arrayList[num++]).Adorner.Measure(constraint);
			}
		}
		return default(Size);
	}

	/// <summary>Positions child elements and determines a size for the <see cref="T:System.Windows.Documents.AdornerLayer" />.</summary>
	/// <returns>The actual size needed by the element.  This return value is typically the same as the value passed to finalSize.</returns>
	/// <param name="finalSize">The size reserved for this element by its parent.</param>
	protected override Size ArrangeOverride(Size finalSize)
	{
		DictionaryEntry[] array = new DictionaryEntry[_zOrderMap.Count];
		_zOrderMap.CopyTo(array, 0);
		for (int i = 0; i < array.Length; i++)
		{
			ArrayList arrayList = (ArrayList)array[i].Value;
			int num = 0;
			while (num < arrayList.Count)
			{
				AdornerInfo adornerInfo = (AdornerInfo)arrayList[num++];
				if (!adornerInfo.Adorner.IsArrangeValid)
				{
					adornerInfo.Adorner.Arrange(new Rect(default(Point), adornerInfo.Adorner.DesiredSize));
					GeneralTransform desiredTransform = adornerInfo.Adorner.GetDesiredTransform(adornerInfo.Transform);
					GeneralTransform proposedTransform = GetProposedTransform(adornerInfo.Adorner, desiredTransform);
					int num2 = _children.IndexOf(adornerInfo.Adorner);
					if (num2 >= 0)
					{
						Transform adornerTransform = proposedTransform?.AffineTransform;
						((Adorner)_children[num2]).AdornerTransform = adornerTransform;
					}
				}
				if (adornerInfo.Adorner.IsClipEnabled)
				{
					adornerInfo.Adorner.AdornerClip = adornerInfo.Clip;
				}
				else if (adornerInfo.Adorner.AdornerClip != null)
				{
					adornerInfo.Adorner.AdornerClip = null;
				}
			}
		}
		return finalSize;
	}

	internal void Add(Adorner adorner, int zOrder)
	{
		if (adorner == null)
		{
			throw new ArgumentNullException("adorner");
		}
		AdornerInfo adornerInfo = new AdornerInfo(adorner);
		adornerInfo.ZOrder = zOrder;
		AddAdornerInfo(ElementMap, adornerInfo, adorner.AdornedElement);
		AddAdornerToVisualTree(adornerInfo, zOrder);
		AddLogicalChild(adorner);
		UpdateAdorner(adorner.AdornedElement);
	}

	internal void InvalidateAdorner(AdornerInfo adornerInfo)
	{
		adornerInfo.Adorner.InvalidateMeasure();
		adornerInfo.Adorner.InvalidateVisual();
		adornerInfo.RenderSize = new Size(double.NaN, double.NaN);
		adornerInfo.Transform = null;
	}

	internal void OnLayoutUpdated(object sender, EventArgs args)
	{
		if (ElementMap.Count != 0)
		{
			UpdateAdorner(null);
		}
	}

	internal void SetAdornerZOrder(Adorner adorner, int zOrder)
	{
		if (!(ElementMap[adorner.AdornedElement] is ArrayList adornerInfos))
		{
			throw new InvalidOperationException(SR.AdornedElementNotFound);
		}
		AdornerInfo adornerInfo = GetAdornerInfo(adornerInfos, adorner);
		if (adornerInfo == null)
		{
			throw new InvalidOperationException(SR.AdornerNotFound);
		}
		RemoveAdornerInfo(_zOrderMap, adorner, adornerInfo.ZOrder);
		_children.Remove(adorner);
		adornerInfo.ZOrder = zOrder;
		AddAdornerToVisualTree(adornerInfo, zOrder);
		InvalidateAdorner(adornerInfo);
		UpdateAdorner(adorner.AdornedElement);
	}

	internal int GetAdornerZOrder(Adorner adorner)
	{
		if (!(ElementMap[adorner.AdornedElement] is ArrayList adornerInfos))
		{
			throw new InvalidOperationException(SR.AdornedElementNotFound);
		}
		return (GetAdornerInfo(adornerInfos, adorner) ?? throw new InvalidOperationException(SR.AdornerNotFound)).ZOrder;
	}

	private void AddAdornerToVisualTree(AdornerInfo adornerInfo, int zOrder)
	{
		Adorner adorner = adornerInfo.Adorner;
		AddAdornerInfo(_zOrderMap, adornerInfo, zOrder);
		ArrayList arrayList = (ArrayList)_zOrderMap[zOrder];
		if (arrayList.Count > 1)
		{
			int num = arrayList.IndexOf(adornerInfo);
			int index = _children.IndexOf(((AdornerInfo)arrayList[num - 1]).Adorner) + 1;
			_children.Insert(index, adorner);
			return;
		}
		IList keyList = _zOrderMap.GetKeyList();
		int num2 = keyList.IndexOf(zOrder) - 1;
		if (num2 < 0)
		{
			_children.Insert(0, adorner);
			return;
		}
		arrayList = (ArrayList)_zOrderMap[keyList[num2]];
		int index2 = _children.IndexOf(((AdornerInfo)arrayList[arrayList.Count - 1]).Adorner) + 1;
		_children.Insert(index2, adorner);
	}

	private void Clear(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (!(ElementMap[element] is ArrayList arrayList))
		{
			throw new InvalidOperationException(SR.AdornedElementNotFound);
		}
		while (arrayList.Count > 0)
		{
			AdornerInfo adornerInfo = arrayList[0] as AdornerInfo;
			Remove(adornerInfo.Adorner);
		}
		ElementMap.Remove(element);
	}

	private void UpdateElementAdorners(UIElement element)
	{
		if (!(VisualTreeHelper.GetParent(this) is Visual ancestor) || !(ElementMap[element] is ArrayList arrayList))
		{
			return;
		}
		bool flag = false;
		GeneralTransform generalTransform = element.TransformToAncestor(ancestor);
		for (int i = 0; i < arrayList.Count; i++)
		{
			AdornerInfo adornerInfo = (AdornerInfo)arrayList[i];
			Size renderSize = element.RenderSize;
			Geometry geometry = null;
			bool flag2 = false;
			if (adornerInfo.Adorner.IsClipEnabled)
			{
				geometry = GetClipGeometry(adornerInfo.Adorner.AdornedElement, adornerInfo.Adorner);
				if ((adornerInfo.Clip == null && geometry != null) || (adornerInfo.Clip != null && geometry == null) || (adornerInfo.Clip != null && geometry != null && adornerInfo.Clip.Bounds != geometry.Bounds))
				{
					flag2 = true;
				}
			}
			if (adornerInfo.Adorner.NeedsUpdate(adornerInfo.RenderSize) || adornerInfo.Transform == null || generalTransform.AffineTransform == null || adornerInfo.Transform.AffineTransform == null || generalTransform.AffineTransform.Value != adornerInfo.Transform.AffineTransform.Value || flag2)
			{
				InvalidateAdorner(adornerInfo);
				adornerInfo.RenderSize = renderSize;
				adornerInfo.Transform = generalTransform;
				if (adornerInfo.Adorner.IsClipEnabled)
				{
					adornerInfo.Clip = geometry;
				}
				flag = true;
			}
		}
		if (flag)
		{
			InvalidateMeasure();
		}
	}

	private void UpdateAdorner(UIElement element)
	{
		if (!(VisualTreeHelper.GetParent(this) is Visual ancestor))
		{
			return;
		}
		ArrayList arrayList = new ArrayList(1);
		if (element != null)
		{
			if (!element.IsDescendantOf(ancestor))
			{
				arrayList.Add(element);
			}
			else
			{
				UpdateElementAdorners(element);
			}
		}
		else
		{
			ICollection keys = ElementMap.Keys;
			UIElement[] array = new UIElement[keys.Count];
			keys.CopyTo(array, 0);
			foreach (UIElement uIElement in array)
			{
				if (!uIElement.IsDescendantOf(ancestor))
				{
					arrayList.Add(uIElement);
				}
				else
				{
					UpdateElementAdorners(uIElement);
				}
			}
		}
		for (int j = 0; j < arrayList.Count; j++)
		{
			Clear((UIElement)arrayList[j]);
		}
	}

	private CombinedGeometry GetClipGeometry(Visual element, Adorner adorner)
	{
		Visual visual = null;
		if (!(VisualTreeHelper.GetParent(this) is Visual visual2))
		{
			return null;
		}
		CombinedGeometry combinedGeometry = null;
		if (!visual2.IsAncestorOf(element))
		{
			return null;
		}
		while (element != visual2 && element != null)
		{
			Geometry clip = VisualTreeHelper.GetClip(element);
			if (clip != null)
			{
				if (combinedGeometry == null)
				{
					combinedGeometry = new CombinedGeometry(clip, null);
				}
				else
				{
					GeneralTransform generalTransform = visual.TransformToAncestor(element);
					combinedGeometry.Transform = generalTransform.AffineTransform;
					combinedGeometry = new CombinedGeometry(combinedGeometry, clip);
					combinedGeometry.GeometryCombineMode = GeometryCombineMode.Intersect;
				}
				visual = element;
			}
			element = (Visual)VisualTreeHelper.GetParent(element);
		}
		if (combinedGeometry != null)
		{
			GeneralTransform generalTransform2 = visual.TransformToAncestor(visual2);
			if (generalTransform2 == null)
			{
				combinedGeometry = null;
			}
			else
			{
				TransformGroup transformGroup = new TransformGroup();
				transformGroup.Children.Add(generalTransform2.AffineTransform);
				generalTransform2 = visual2.TransformToDescendant(adorner);
				if (generalTransform2 == null)
				{
					combinedGeometry = null;
				}
				else
				{
					transformGroup.Children.Add(generalTransform2.AffineTransform);
					combinedGeometry.Transform = transformGroup;
				}
			}
		}
		return combinedGeometry;
	}

	private bool RemoveAdornerInfo(IDictionary infoMap, Adorner adorner, object key)
	{
		if (infoMap[key] is ArrayList arrayList)
		{
			AdornerInfo adornerInfo = GetAdornerInfo(arrayList, adorner);
			if (adornerInfo != null)
			{
				arrayList.Remove(adornerInfo);
				if (arrayList.Count == 0)
				{
					infoMap.Remove(key);
				}
				return true;
			}
		}
		return false;
	}

	private AdornerInfo GetAdornerInfo(ArrayList adornerInfos, Adorner adorner)
	{
		if (adornerInfos != null)
		{
			for (int i = 0; i < adornerInfos.Count; i++)
			{
				if (((AdornerInfo)adornerInfos[i]).Adorner == adorner)
				{
					return (AdornerInfo)adornerInfos[i];
				}
			}
		}
		return null;
	}

	private void AddAdornerInfo(IDictionary infoMap, AdornerInfo adornerInfo, object key)
	{
		ArrayList arrayList2 = (ArrayList)((infoMap[key] != null) ? ((ArrayList)infoMap[key]) : (infoMap[key] = new ArrayList(1)));
		arrayList2.Add(adornerInfo);
	}

	private GeneralTransform GetProposedTransform(Adorner adorner, GeneralTransform sourceTransform)
	{
		if (adorner.FlowDirection != base.FlowDirection)
		{
			GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
			MatrixTransform value = new MatrixTransform(new Matrix(-1.0, 0.0, 0.0, 1.0, adorner.RenderSize.Width, 0.0));
			generalTransformGroup.Children.Add(value);
			if (sourceTransform != null && sourceTransform != Transform.Identity)
			{
				generalTransformGroup.Children.Add(sourceTransform);
			}
			return generalTransformGroup;
		}
		return sourceTransform;
	}
}
