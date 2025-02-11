using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;

namespace System.Windows.Documents;

/// <summary>Abstract class that represents a <see cref="T:System.Windows.FrameworkElement" /> that decorates a <see cref="T:System.Windows.UIElement" />.</summary>
public abstract class Adorner : FrameworkElement
{
	private readonly UIElement _adornedElement;

	private bool _isClipEnabled;

	internal Geometry AdornerClip
	{
		get
		{
			return base.Clip;
		}
		set
		{
			base.Clip = value;
		}
	}

	internal Transform AdornerTransform
	{
		get
		{
			return base.RenderTransform;
		}
		set
		{
			base.RenderTransform = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.UIElement" /> that this adorner is bound to.</summary>
	/// <returns>The element that this adorner is bound to. The default value is null.</returns>
	public UIElement AdornedElement => _adornedElement;

	/// <summary>Gets or sets a value that indicates whether clipping of the adorner is enabled.</summary>
	/// <returns>A Boolean value indicating whether clipping of the adorner is enabled.If this property is false, the adorner is not clipped.If this property is true, the adorner is clipped using the same clipping geometry as the adorned element.The default value is false.</returns>
	public bool IsClipEnabled
	{
		get
		{
			return _isClipEnabled;
		}
		set
		{
			_isClipEnabled = value;
			InvalidateArrange();
			AdornerLayer.GetAdornerLayer(_adornedElement).InvalidateArrange();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Adorner" /> class.</summary>
	/// <param name="adornedElement">The element to bind the adorner to.</param>
	/// <exception cref="T:System.ArgumentNullException">adornedElement is null.</exception>
	protected Adorner(UIElement adornedElement)
	{
		if (adornedElement == null)
		{
			throw new ArgumentNullException("adornedElement");
		}
		_adornedElement = adornedElement;
		_isClipEnabled = false;
		Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(CreateFlowDirectionBinding), this);
	}

	/// <summary>Implements any custom measuring behavior for the adorner.</summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> object representing the amount of layout space needed by the adorner.</returns>
	/// <param name="constraint">A size to constrain the adorner to.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		Size size = new Size(AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height);
		int visualChildrenCount = VisualChildrenCount;
		for (int i = 0; i < visualChildrenCount; i++)
		{
			if (GetVisualChild(i) is UIElement uIElement)
			{
				uIElement.Measure(size);
			}
		}
		return size;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.UIElement.GetLayoutClip(System.Windows.Size)" />.</summary>
	/// <returns>The potential clipping geometry. See Remarks.</returns>
	/// <param name="layoutSlotSize">The available size provided by the element.</param>
	protected override Geometry GetLayoutClip(Size layoutSlotSize)
	{
		return null;
	}

	/// <summary>Returns a <see cref="T:System.Windows.Media.Transform" /> for the adorner, based on the transform that is currently applied to the adorned element.</summary>
	/// <returns>A transform to apply to the adorner.</returns>
	/// <param name="transform">The transform that is currently applied to the adorned element.</param>
	public virtual GeneralTransform GetDesiredTransform(GeneralTransform transform)
	{
		return transform;
	}

	private static object CreateFlowDirectionBinding(object o)
	{
		Adorner adorner = (Adorner)o;
		Binding binding = new Binding("FlowDirection");
		binding.Mode = BindingMode.OneWay;
		binding.Source = adorner.AdornedElement;
		adorner.SetBinding(FrameworkElement.FlowDirectionProperty, binding);
		return null;
	}

	internal virtual bool NeedsUpdate(Size oldSize)
	{
		return !DoubleUtil.AreClose(AdornedElement.RenderSize, oldSize);
	}
}
