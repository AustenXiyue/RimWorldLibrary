using System.Collections.Generic;
using MS.Internal;

namespace System.Windows.Controls.Primitives;

/// <summary>Used to arrange overflow <see cref="T:System.Windows.Controls.ToolBar" /> items.</summary>
public class ToolBarOverflowPanel : Panel
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.ToolBarOverflowPanel.WrapWidth" /> dependency property.</summary>
	public static readonly DependencyProperty WrapWidthProperty = DependencyProperty.Register("WrapWidth", typeof(double), typeof(ToolBarOverflowPanel), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), IsWrapWidthValid);

	private double _wrapWidth;

	private Size _panelSize;

	/// <summary>Gets or sets the recommended width for an overflow <see cref="T:System.Windows.Controls.ToolBar" /> before items flow to the next line.  </summary>
	/// <returns>A double value that represents the recommended width of the <see cref="T:System.Windows.Controls.ToolBar" />.</returns>
	public double WrapWidth
	{
		get
		{
			return (double)GetValue(WrapWidthProperty);
		}
		set
		{
			SetValue(WrapWidthProperty, value);
		}
	}

	private ToolBar ToolBar => base.TemplatedParent as ToolBar;

	private ToolBarPanel ToolBarPanel => ToolBar?.ToolBarPanel;

	private static bool IsWrapWidthValid(object value)
	{
		double num = (double)value;
		if (!double.IsNaN(num))
		{
			if (DoubleUtil.GreaterThanOrClose(num, 0.0))
			{
				return !double.IsPositiveInfinity(num);
			}
			return false;
		}
		return true;
	}

	/// <summary>Remeasures the <see cref="T:System.Windows.Controls.Primitives.ToolBarOverflowPanel" />. </summary>
	/// <returns>The desired size.</returns>
	/// <param name="constraint">Constraint size is an upper limit. The return value should not exceed this size.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		Size size = default(Size);
		_panelSize = default(Size);
		_wrapWidth = (double.IsNaN(WrapWidth) ? constraint.Width : WrapWidth);
		UIElementCollection internalChildren = base.InternalChildren;
		int num = internalChildren.Count;
		ToolBarPanel toolBarPanel = ToolBarPanel;
		if (toolBarPanel != null)
		{
			List<UIElement> generatedItemsCollection = toolBarPanel.GeneratedItemsCollection;
			int num2 = generatedItemsCollection?.Count ?? 0;
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				UIElement uIElement = generatedItemsCollection[i];
				if (uIElement == null || !ToolBar.GetIsOverflowItem(uIElement) || uIElement is Separator)
				{
					continue;
				}
				if (num3 < num)
				{
					if (internalChildren[num3] != uIElement)
					{
						internalChildren.Insert(num3, uIElement);
						num++;
					}
				}
				else
				{
					internalChildren.Add(uIElement);
					num++;
				}
				num3++;
			}
		}
		for (int j = 0; j < num; j++)
		{
			UIElement uIElement2 = internalChildren[j];
			uIElement2.Measure(constraint);
			Size desiredSize = uIElement2.DesiredSize;
			if (DoubleUtil.GreaterThan(desiredSize.Width, _wrapWidth))
			{
				_wrapWidth = desiredSize.Width;
			}
		}
		_wrapWidth = Math.Min(_wrapWidth, constraint.Width);
		for (int k = 0; k < internalChildren.Count; k++)
		{
			Size desiredSize2 = internalChildren[k].DesiredSize;
			if (DoubleUtil.GreaterThan(size.Width + desiredSize2.Width, _wrapWidth))
			{
				_panelSize.Width = Math.Max(size.Width, _panelSize.Width);
				_panelSize.Height += size.Height;
				size = desiredSize2;
				if (DoubleUtil.GreaterThan(desiredSize2.Width, _wrapWidth))
				{
					_panelSize.Width = Math.Max(desiredSize2.Width, _panelSize.Width);
					_panelSize.Height += desiredSize2.Height;
					size = default(Size);
				}
			}
			else
			{
				size.Width += desiredSize2.Width;
				size.Height = Math.Max(desiredSize2.Height, size.Height);
			}
		}
		_panelSize.Width = Math.Max(size.Width, _panelSize.Width);
		_panelSize.Height += size.Height;
		return _panelSize;
	}

	/// <summary>Arranges and sizes the content of a <see cref="T:System.Windows.Controls.Primitives.ToolBarOverflowPanel" /> object.</summary>
	/// <returns>The size of the <see cref="T:System.Windows.Controls.Primitives.ToolBarOverflowPanel" />.</returns>
	/// <param name="arrangeBounds">Size that a toolbar overflow panel assumes to position child elements.</param>
	protected override Size ArrangeOverride(Size arrangeBounds)
	{
		int start = 0;
		Size size = default(Size);
		double num = 0.0;
		_wrapWidth = Math.Min(_wrapWidth, arrangeBounds.Width);
		UIElementCollection children = base.Children;
		for (int i = 0; i < children.Count; i++)
		{
			Size desiredSize = children[i].DesiredSize;
			if (DoubleUtil.GreaterThan(size.Width + desiredSize.Width, _wrapWidth))
			{
				arrangeLine(num, size.Height, start, i);
				num += size.Height;
				start = i;
				size = desiredSize;
			}
			else
			{
				size.Width += desiredSize.Width;
				size.Height = Math.Max(desiredSize.Height, size.Height);
			}
		}
		arrangeLine(num, size.Height, start, children.Count);
		return _panelSize;
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Controls.UIElementCollection" />.</summary>
	/// <returns>A new collection.</returns>
	/// <param name="logicalParent">Logical parent of the new collection.</param>
	protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
	{
		return new UIElementCollection(this, (base.TemplatedParent == null) ? logicalParent : null);
	}

	private void arrangeLine(double y, double lineHeight, int start, int end)
	{
		double num = 0.0;
		UIElementCollection children = base.Children;
		for (int i = start; i < end; i++)
		{
			UIElement uIElement = children[i];
			uIElement.Arrange(new Rect(num, y, uIElement.DesiredSize.Width, lineHeight));
			num += uIElement.DesiredSize.Width;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.ToolBarOverflowPanel" /> class.</summary>
	public ToolBarOverflowPanel()
	{
	}
}
