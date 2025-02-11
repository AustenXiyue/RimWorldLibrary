using System.Collections;
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal.Controls;

namespace System.Windows.Controls;

/// <summary>Represents the element used in a <see cref="T:System.Windows.Controls.ControlTemplate" /> to specify where a decorated control is placed relative to other elements in the <see cref="T:System.Windows.Controls.ControlTemplate" />.</summary>
[ContentProperty("Child")]
public class AdornedElementPlaceholder : FrameworkElement, IAddChild
{
	private UIElement _child;

	private TemplatedAdorner _templatedAdorner;

	/// <summary>Gets the <see cref="T:System.Windows.UIElement" /> that this <see cref="T:System.Windows.Controls.AdornedElementPlaceholder" /> object is reserving space for.</summary>
	/// <returns>The <see cref="T:System.Windows.UIElement" /> that this <see cref="T:System.Windows.Controls.AdornedElementPlaceholder" /> object is reserving space for. The default is null.</returns>
	public UIElement AdornedElement
	{
		get
		{
			if (TemplatedAdorner != null)
			{
				return TemplatedAdorner.AdornedElement;
			}
			return null;
		}
	}

	/// <summary>Gets or sets the single child object of this <see cref="T:System.Windows.Controls.AdornedElementPlaceholder" /> object.</summary>
	/// <returns>The single child object of this <see cref="T:System.Windows.Controls.AdornedElementPlaceholder" /> object. The default is null.</returns>
	[DefaultValue(null)]
	public virtual UIElement Child
	{
		get
		{
			return _child;
		}
		set
		{
			UIElement child = _child;
			if (child != value)
			{
				RemoveVisualChild(child);
				RemoveLogicalChild(child);
				_child = value;
				AddVisualChild(_child);
				AddLogicalChild(value);
				InvalidateMeasure();
			}
		}
	}

	/// <summary>Gets the number of visual child objects.</summary>
	/// <returns>Either 0 or 1. The default is 0.</returns>
	protected override int VisualChildrenCount => (_child != null) ? 1 : 0;

	/// <summary>Gets an enumerator for the logical child elements of this <see cref="T:System.Windows.Controls.AdornedElementPlaceholder" /> object.</summary>
	/// <returns>An enumerator for the logical child elements of this <see cref="T:System.Windows.Controls.AdornedElementPlaceholder" /> object. The default is null.</returns>
	protected internal override IEnumerator LogicalChildren => new SingleChildEnumerator(_child);

	private TemplatedAdorner TemplatedAdorner
	{
		get
		{
			if (_templatedAdorner == null && base.TemplatedParent is FrameworkElement reference)
			{
				_templatedAdorner = VisualTreeHelper.GetParent(reference) as TemplatedAdorner;
				if (_templatedAdorner != null && _templatedAdorner.ReferenceElement == null)
				{
					_templatedAdorner.ReferenceElement = this;
				}
			}
			return _templatedAdorner;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.AdornedElementPlaceholder" /> class.</summary>
	public AdornedElementPlaceholder()
	{
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="value"> An object to add as a child.</param>
	void IAddChild.AddChild(object value)
	{
		if (value != null)
		{
			if (!(value is UIElement))
			{
				throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(UIElement)), "value");
			}
			if (Child != null)
			{
				throw new ArgumentException(SR.Format(SR.CanOnlyHaveOneChild, GetType(), value.GetType()));
			}
			Child = (UIElement)value;
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="text"> A string to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	/// <summary>Retrieves the <see cref="T:System.Windows.Media.Visual" /> child object at the specified index.</summary>
	/// <returns>The visual child object at the specified index.</returns>
	/// <param name="index">The index that specifies the child object to retrieve.</param>
	protected override Visual GetVisualChild(int index)
	{
		if (_child == null || index != 0)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return _child;
	}

	/// <summary>Raises the <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> event. This method is called when <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> is set to true internally.</summary>
	/// <param name="e">Arguments of the event.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Controls.AdornedElementPlaceholder" /> object is not part of a template.</exception>
	protected override void OnInitialized(EventArgs e)
	{
		if (base.TemplatedParent == null)
		{
			throw new InvalidOperationException(SR.AdornedElementPlaceholderMustBeInTemplate);
		}
		base.OnInitialized(e);
	}

	/// <summary>Determines the size of the <see cref="T:System.Windows.Controls.AdornedElementPlaceholder" /> object.</summary>
	/// <returns>The desired size of this <see cref="T:System.Windows.Controls.AdornedElementPlaceholder" /> object.</returns>
	/// <param name="constraint">An upper limit value that the return value should not exceed.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Controls.AdornedElementPlaceholder" /> object is not part of a template.</exception>
	protected override Size MeasureOverride(Size constraint)
	{
		if (base.TemplatedParent == null)
		{
			throw new InvalidOperationException(SR.AdornedElementPlaceholderMustBeInTemplate);
		}
		if (AdornedElement == null)
		{
			return new Size(0.0, 0.0);
		}
		Size renderSize = AdornedElement.RenderSize;
		Child?.Measure(renderSize);
		return renderSize;
	}

	/// <summary>Positions the first visual child object and returns the size in layout required by this <see cref="T:System.Windows.Controls.AdornedElementPlaceholder" /> object.</summary>
	/// <returns>The actual size needed by the element.</returns>
	/// <param name="arrangeBounds">The size that this <see cref="T:System.Windows.Controls.AdornedElementPlaceholder" /> object should use to arrange its child element.</param>
	protected override Size ArrangeOverride(Size arrangeBounds)
	{
		Child?.Arrange(new Rect(arrangeBounds));
		return arrangeBounds;
	}
}
