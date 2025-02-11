using System.Windows.Media;
using MS.Internal.PresentationFramework;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Defines an area where you can arrange child elements either horizontally or vertically, relative to each other. </summary>
public class DockPanel : Panel
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DockPanel.LastChildFill" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DockPanel.LastChildFill" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty LastChildFillProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DockPanel.Dock" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DockPanel.Dock" /> attached property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty DockProperty;

	/// <summary>Gets or sets a value that indicates whether the last child element within a <see cref="T:System.Windows.Controls.DockPanel" /> stretches to fill the remaining available space.   </summary>
	/// <returns>true if the last child element stretches to fill the remaining space; otherwise false. The default value is true.</returns>
	public bool LastChildFill
	{
		get
		{
			return (bool)GetValue(LastChildFillProperty);
		}
		set
		{
			SetValue(LastChildFillProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 9;

	static DockPanel()
	{
		LastChildFillProperty = DependencyProperty.Register("LastChildFill", typeof(bool), typeof(DockPanel), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange));
		DockProperty = DependencyProperty.RegisterAttached("Dock", typeof(Dock), typeof(DockPanel), new FrameworkPropertyMetadata(Dock.Left, OnDockChanged), IsValidDock);
		ControlsTraceLogger.AddControl(TelemetryControls.DockPanel);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DockPanel" /> class. </summary>
	public DockPanel()
	{
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.DockPanel.Dock" /> attached property for a specified <see cref="T:System.Windows.UIElement" />.</summary>
	/// <returns>The <see cref="P:System.Windows.Controls.DockPanel.Dock" /> property value for the element.</returns>
	/// <param name="element">The element from which the property value is read.</param>
	[AttachedPropertyBrowsableForChildren]
	public static Dock GetDock(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (Dock)element.GetValue(DockProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.DockPanel.Dock" /> attached property to a specified element. </summary>
	/// <param name="element">The element to which the attached property is written.</param>
	/// <param name="dock">The needed <see cref="T:System.Windows.Controls.Dock" /> value.</param>
	public static void SetDock(UIElement element, Dock dock)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(DockProperty, dock);
	}

	private static void OnDockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement reference && VisualTreeHelper.GetParent(reference) is DockPanel dockPanel)
		{
			dockPanel.InvalidateMeasure();
		}
	}

	/// <summary>Measures the child elements of a <see cref="T:System.Windows.Controls.DockPanel" /> prior to arranging them during the <see cref="M:System.Windows.Controls.DockPanel.ArrangeOverride(System.Windows.Size)" /> pass.</summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> that represents the element size you want.</returns>
	/// <param name="constraint">A maximum <see cref="T:System.Windows.Size" /> to not exceed.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		UIElementCollection internalChildren = base.InternalChildren;
		double val = 0.0;
		double val2 = 0.0;
		double num = 0.0;
		double num2 = 0.0;
		int i = 0;
		for (int count = internalChildren.Count; i < count; i++)
		{
			UIElement uIElement = internalChildren[i];
			if (uIElement != null)
			{
				Size availableSize = new Size(Math.Max(0.0, constraint.Width - num), Math.Max(0.0, constraint.Height - num2));
				uIElement.Measure(availableSize);
				Size desiredSize = uIElement.DesiredSize;
				switch (GetDock(uIElement))
				{
				case Dock.Left:
				case Dock.Right:
					val2 = Math.Max(val2, num2 + desiredSize.Height);
					num += desiredSize.Width;
					break;
				case Dock.Top:
				case Dock.Bottom:
					val = Math.Max(val, num + desiredSize.Width);
					num2 += desiredSize.Height;
					break;
				}
			}
		}
		val = Math.Max(val, num);
		val2 = Math.Max(val2, num2);
		return new Size(val, val2);
	}

	/// <summary>Arranges the content (child elements) of a <see cref="T:System.Windows.Controls.DockPanel" /> element.</summary>
	/// <returns>The <see cref="T:System.Windows.Size" /> that represents the arranged size of this <see cref="T:System.Windows.Controls.DockPanel" /> element.</returns>
	/// <param name="arrangeSize">The <see cref="T:System.Windows.Size" /> this element uses to arrange its child elements.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		UIElementCollection internalChildren = base.InternalChildren;
		int count = internalChildren.Count;
		int num = count - (LastChildFill ? 1 : 0);
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		for (int i = 0; i < count; i++)
		{
			UIElement uIElement = internalChildren[i];
			if (uIElement == null)
			{
				continue;
			}
			Size desiredSize = uIElement.DesiredSize;
			Rect finalRect = new Rect(num2, num3, Math.Max(0.0, arrangeSize.Width - (num2 + num4)), Math.Max(0.0, arrangeSize.Height - (num3 + num5)));
			if (i < num)
			{
				switch (GetDock(uIElement))
				{
				case Dock.Left:
					num2 += desiredSize.Width;
					finalRect.Width = desiredSize.Width;
					break;
				case Dock.Right:
					num4 += desiredSize.Width;
					finalRect.X = Math.Max(0.0, arrangeSize.Width - num4);
					finalRect.Width = desiredSize.Width;
					break;
				case Dock.Top:
					num3 += desiredSize.Height;
					finalRect.Height = desiredSize.Height;
					break;
				case Dock.Bottom:
					num5 += desiredSize.Height;
					finalRect.Y = Math.Max(0.0, arrangeSize.Height - num5);
					finalRect.Height = desiredSize.Height;
					break;
				}
			}
			uIElement.Arrange(finalRect);
		}
		return arrangeSize;
	}

	internal static bool IsValidDock(object o)
	{
		Dock dock = (Dock)o;
		if (dock != 0 && dock != Dock.Top && dock != Dock.Right)
		{
			return dock == Dock.Bottom;
		}
		return true;
	}
}
