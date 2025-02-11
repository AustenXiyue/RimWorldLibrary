using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Documents;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents a layout control that aligns a bullet and another visual object.</summary>
public class BulletDecorator : Decorator
{
	private class DoubleChildEnumerator : IEnumerator
	{
		private int _index = -1;

		private object _child1;

		private object _child2;

		object IEnumerator.Current => _index switch
		{
			0 => _child1, 
			1 => _child2, 
			_ => null, 
		};

		internal DoubleChildEnumerator(object child1, object child2)
		{
			_child1 = child1;
			_child2 = child2;
		}

		bool IEnumerator.MoveNext()
		{
			_index++;
			return _index < 2;
		}

		void IEnumerator.Reset()
		{
			_index = -1;
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.BulletDecorator.Background" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.BulletDecorator.Background" /> dependency property. </returns>
	public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(typeof(BulletDecorator), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

	private UIElement _bullet;

	/// <summary>Gets or sets the background color for a <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" /> control. </summary>
	/// <returns>The background color for the <see cref="P:System.Windows.Controls.Primitives.BulletDecorator.Bullet" /> and <see cref="P:System.Windows.Controls.Decorator.Child" /> of a <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" />. The default is null.</returns>
	public Brush Background
	{
		get
		{
			return (Brush)GetValue(BackgroundProperty);
		}
		set
		{
			SetValue(BackgroundProperty, value);
		}
	}

	/// <summary>Gets or sets the object to use as the bullet in a <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" />.</summary>
	/// <returns>The <see cref="T:System.Windows.UIElement" /> to use as the bullet. The default is null.</returns>
	public UIElement Bullet
	{
		get
		{
			return _bullet;
		}
		set
		{
			if (_bullet != value)
			{
				if (_bullet != null)
				{
					RemoveVisualChild(_bullet);
					RemoveLogicalChild(_bullet);
				}
				_bullet = value;
				AddLogicalChild(value);
				AddVisualChild(value);
				UIElement child = Child;
				if (child != null)
				{
					RemoveVisualChild(child);
					AddVisualChild(child);
				}
				InvalidateMeasure();
			}
		}
	}

	/// <summary>Gets an enumerator for the logical child elements of the <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" /> control.</summary>
	/// <returns>The enumerator that provides access to the logical child elements of the <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" /> control. The default is null.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			if (_bullet == null)
			{
				return base.LogicalChildren;
			}
			if (Child == null)
			{
				return new SingleChildEnumerator(_bullet);
			}
			return new DoubleChildEnumerator(_bullet, Child);
		}
	}

	/// <summary>Gets the number of visual child elements for the <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" /> control.</summary>
	/// <returns>The number of visual elements that are defined for the <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" />. The default is 0.</returns>
	protected override int VisualChildrenCount => ((Child != null) ? 1 : 0) + ((_bullet != null) ? 1 : 0);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" /> class.</summary>
	public BulletDecorator()
	{
	}

	/// <summary>Renders the content of a <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" /> control.</summary>
	/// <param name="dc">The <see cref="T:System.Windows.Media.DrawingContext" /> for the <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" />. </param>
	protected override void OnRender(DrawingContext dc)
	{
		Brush background = Background;
		if (background != null)
		{
			dc.DrawRectangle(background, null, new Rect(0.0, 0.0, base.RenderSize.Width, base.RenderSize.Height));
		}
	}

	/// <summary>Gets the child element that is at the specified index.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Visual" /> child element that is at the specified index.</returns>
	/// <param name="index">The specified index for the child element.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0. -or-<paramref name="index" /> is greater than or equal to <see cref="P:System.Windows.Controls.Primitives.BulletDecorator.VisualChildrenCount" />.</exception>
	protected override Visual GetVisualChild(int index)
	{
		if (index < 0 || index > VisualChildrenCount - 1)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		if (index == 0 && _bullet != null)
		{
			return _bullet;
		}
		return Child;
	}

	/// <summary>Overrides the default measurement behavior for the objects of a <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" /> control.</summary>
	/// <returns>The required <see cref="T:System.Windows.Size" /> for the <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" /> control, based on the size of its <see cref="P:System.Windows.Controls.Primitives.BulletDecorator.Bullet" /> and <see cref="P:System.Windows.Controls.Decorator.Child" /> objects.</returns>
	/// <param name="constraint">The upper <see cref="T:System.Windows.Size" /> limit of the <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" />.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		Size size = default(Size);
		Size size2 = default(Size);
		UIElement bullet = Bullet;
		UIElement child = Child;
		if (bullet != null)
		{
			bullet.Measure(constraint);
			size = bullet.DesiredSize;
		}
		if (child != null)
		{
			Size availableSize = constraint;
			availableSize.Width = Math.Max(0.0, availableSize.Width - size.Width);
			child.Measure(availableSize);
			size2 = child.DesiredSize;
		}
		return new Size(size.Width + size2.Width, Math.Max(size.Height, size2.Height));
	}

	/// <summary>Overrides the default content arrangement behavior for the <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" /> control.</summary>
	/// <returns>The computed size of the <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" /> control. </returns>
	/// <param name="arrangeSize">The available <see cref="T:System.Windows.Size" /> to use to lay out the content of the <see cref="T:System.Windows.Controls.Primitives.BulletDecorator" /> control.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		UIElement bullet = Bullet;
		UIElement child = Child;
		double x = 0.0;
		double num = 0.0;
		Size size = default(Size);
		if (bullet != null)
		{
			bullet.Arrange(new Rect(bullet.DesiredSize));
			size = bullet.RenderSize;
			x = size.Width;
		}
		if (child != null)
		{
			Size size2 = arrangeSize;
			if (bullet != null)
			{
				size2.Width = Math.Max(child.DesiredSize.Width, arrangeSize.Width - bullet.DesiredSize.Width);
				size2.Height = Math.Max(child.DesiredSize.Height, arrangeSize.Height);
			}
			child.Arrange(new Rect(x, 0.0, size2.Width, size2.Height));
			double num2 = GetFirstLineHeight(child) * 0.5;
			num += Math.Max(0.0, num2 - size.Height * 0.5);
		}
		if (bullet != null && !DoubleUtil.IsZero(num))
		{
			bullet.Arrange(new Rect(0.0, num, bullet.DesiredSize.Width, bullet.DesiredSize.Height));
		}
		return arrangeSize;
	}

	private double GetFirstLineHeight(UIElement element)
	{
		UIElement uIElement = FindText(element);
		ReadOnlyCollection<LineResult> readOnlyCollection = null;
		if (uIElement != null)
		{
			TextBlock textBlock = (TextBlock)uIElement;
			if (textBlock.IsLayoutDataValid)
			{
				readOnlyCollection = textBlock.GetLineResults();
			}
		}
		else
		{
			uIElement = FindFlowDocumentScrollViewer(element);
			if (uIElement != null && ((IServiceProvider)uIElement).GetService(typeof(ITextView)) is TextDocumentView { IsValid: not false, Columns: { Count: >0 } columns })
			{
				ReadOnlyCollection<ParagraphResult> paragraphs = columns[0].Paragraphs;
				if (paragraphs != null && paragraphs.Count > 0 && paragraphs[0] is ContainerParagraphResult containerParagraphResult && containerParagraphResult.Paragraphs[0] is TextParagraphResult textParagraphResult)
				{
					readOnlyCollection = textParagraphResult.Lines;
				}
			}
		}
		if (readOnlyCollection != null && readOnlyCollection.Count > 0)
		{
			Point result = default(Point);
			uIElement.TransformToAncestor(element).TryTransform(result, out result);
			return readOnlyCollection[0].LayoutBox.Height + result.Y * 2.0;
		}
		return element.RenderSize.Height;
	}

	private TextBlock FindText(Visual root)
	{
		if (root is TextBlock result)
		{
			return result;
		}
		if (root is ContentPresenter reference)
		{
			if (VisualTreeHelper.GetChildrenCount(reference) == 1)
			{
				DependencyObject child = VisualTreeHelper.GetChild(reference, 0);
				TextBlock textBlock = child as TextBlock;
				if (textBlock == null && child is AccessText reference2 && VisualTreeHelper.GetChildrenCount(reference2) == 1)
				{
					textBlock = VisualTreeHelper.GetChild(reference2, 0) as TextBlock;
				}
				return textBlock;
			}
		}
		else if (root is AccessText reference3 && VisualTreeHelper.GetChildrenCount(reference3) == 1)
		{
			return VisualTreeHelper.GetChild(reference3, 0) as TextBlock;
		}
		return null;
	}

	private FlowDocumentScrollViewer FindFlowDocumentScrollViewer(Visual root)
	{
		if (root is FlowDocumentScrollViewer result)
		{
			return result;
		}
		if (root is ContentPresenter reference && VisualTreeHelper.GetChildrenCount(reference) == 1)
		{
			return VisualTreeHelper.GetChild(reference, 0) as FlowDocumentScrollViewer;
		}
		return null;
	}
}
