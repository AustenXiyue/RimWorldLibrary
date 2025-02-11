using System.Windows.Controls;
using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>Provides an <see cref="T:System.Windows.Documents.AdornerLayer" /> for the child elements in the visual tree. </summary>
/// <exception cref="T:System.ArgumentException">An attempt is made to add more than a single child to the <see cref="T:System.Windows.Documents.AdornerDecorator" />.</exception>
public class AdornerDecorator : Decorator
{
	private readonly AdornerLayer _adornerLayer;

	/// <summary>Gets the <see cref="T:System.Windows.Documents.AdornerLayer" /> associated with this <see cref="T:System.Windows.Documents.AdornerDecorator" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Documents.AdornerLayer" /> associated with this adorner decorator. </returns>
	public AdornerLayer AdornerLayer => _adornerLayer;

	/// <summary>Gets or sets the single child of an <see cref="T:System.Windows.Documents.AdornerDecorator" />.</summary>
	/// <returns>The single child of an <see cref="T:System.Windows.Documents.AdornerDecorator" />. This property has no default value.</returns>
	public override UIElement Child
	{
		get
		{
			return base.Child;
		}
		set
		{
			Visual child = base.Child;
			if (child == value)
			{
				return;
			}
			if (value == null)
			{
				base.Child = null;
				RemoveVisualChild(_adornerLayer);
				return;
			}
			base.Child = value;
			if (child == null)
			{
				AddVisualChild(_adornerLayer);
			}
		}
	}

	/// <summary>Gets the number of child <see cref="T:System.Windows.Media.Visual" /> objects in this instance of <see cref="T:System.Windows.Documents.AdornerDecorator" />.</summary>
	/// <returns>Either returns 2 (one for the <see cref="T:System.Windows.Documents.AdornerLayer" /> and one for the <see cref="P:System.Windows.Documents.AdornerDecorator.Child" />) or the property returns 0 if the <see cref="T:System.Windows.Documents.AdornerDecorator" /> has no child.</returns>
	protected override int VisualChildrenCount
	{
		get
		{
			if (base.Child != null)
			{
				return 2;
			}
			return 0;
		}
	}

	internal override int EffectiveValuesInitialSize => 6;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.AdornerDecorator" /> class.</summary>
	public AdornerDecorator()
	{
		_adornerLayer = new AdornerLayer();
	}

	/// <summary>Measures the size required for child elements and determines a size for the <see cref="T:System.Windows.Documents.AdornerDecorator" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> object representing the amount of layout space needed by the <see cref="T:System.Windows.Documents.AdornerDecorator" />.</returns>
	/// <param name="constraint">A size to constrain the <see cref="T:System.Windows.Documents.AdornerDecorator" /> to.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		Size result = base.MeasureOverride(constraint);
		if (VisualTreeHelper.GetParent(_adornerLayer) != null)
		{
			_adornerLayer.Measure(constraint);
		}
		return result;
	}

	/// <summary>Positions child elements and determines a size for the <see cref="T:System.Windows.Documents.AdornerDecorator" />.</summary>
	/// <returns>The actual size needed by the element.  This return value is typically the same as the value passed to finalSize.</returns>
	/// <param name="finalSize">The size reserved for this element by its parent.</param>
	protected override Size ArrangeOverride(Size finalSize)
	{
		Size result = base.ArrangeOverride(finalSize);
		if (VisualTreeHelper.GetParent(_adornerLayer) != null)
		{
			_adornerLayer.Arrange(new Rect(finalSize));
		}
		return result;
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.Visual" /> child at the specified <paramref name="index" /> position.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Visual" /> child of the parent <see cref="T:System.Windows.Controls.Viewbox" /> element.</returns>
	/// <param name="index">The index position of the wanted <see cref="T:System.Windows.Media.Visual" /> child.</param>
	protected override Visual GetVisualChild(int index)
	{
		if (base.Child == null)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return index switch
		{
			0 => base.Child, 
			1 => _adornerLayer, 
			_ => throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange), 
		};
	}
}
