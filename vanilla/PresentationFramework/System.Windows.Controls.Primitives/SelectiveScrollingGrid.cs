using System.Windows.Data;
using System.Windows.Media;

namespace System.Windows.Controls.Primitives;

/// <summary>A panel that can hold specified cells in place when the view is scrolled.</summary>
public class SelectiveScrollingGrid : Grid
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.SelectiveScrollingGrid.SelectiveScrollingOrientation" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.SelectiveScrollingGrid.SelectiveScrollingOrientation" /> dependency property.</returns>
	public static readonly DependencyProperty SelectiveScrollingOrientationProperty = DependencyProperty.RegisterAttached("SelectiveScrollingOrientation", typeof(SelectiveScrollingOrientation), typeof(SelectiveScrollingGrid), new FrameworkPropertyMetadata(SelectiveScrollingOrientation.Both, OnSelectiveScrollingOrientationChanged));

	/// <summary>Gets the direction that cells can scroll for a specified object.</summary>
	/// <returns>The direction that cells can scroll.</returns>
	/// <param name="obj">The object whose scrolling orientation is retrieved.</param>
	public static SelectiveScrollingOrientation GetSelectiveScrollingOrientation(DependencyObject obj)
	{
		return (SelectiveScrollingOrientation)obj.GetValue(SelectiveScrollingOrientationProperty);
	}

	/// <summary>Sets the direction that cells can scroll for a specified object.</summary>
	/// <param name="obj">The object whose scrolling orientation is set.</param>
	/// <param name="value">The scrolling orientation.</param>
	public static void SetSelectiveScrollingOrientation(DependencyObject obj, SelectiveScrollingOrientation value)
	{
		obj.SetValue(SelectiveScrollingOrientationProperty, value);
	}

	private static void OnSelectiveScrollingOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		UIElement uIElement = d as UIElement;
		SelectiveScrollingOrientation selectiveScrollingOrientation = (SelectiveScrollingOrientation)e.NewValue;
		ScrollViewer scrollViewer = DataGridHelper.FindVisualParent<ScrollViewer>(uIElement);
		if (scrollViewer == null || uIElement == null)
		{
			return;
		}
		Transform renderTransform = uIElement.RenderTransform;
		if (renderTransform != null)
		{
			BindingOperations.ClearBinding(renderTransform, TranslateTransform.XProperty);
			BindingOperations.ClearBinding(renderTransform, TranslateTransform.YProperty);
		}
		if (selectiveScrollingOrientation == SelectiveScrollingOrientation.Both)
		{
			uIElement.RenderTransform = null;
			return;
		}
		TranslateTransform translateTransform = new TranslateTransform();
		if (selectiveScrollingOrientation != SelectiveScrollingOrientation.Horizontal)
		{
			Binding binding = new Binding("ContentHorizontalOffset");
			binding.Source = scrollViewer;
			BindingOperations.SetBinding(translateTransform, TranslateTransform.XProperty, binding);
		}
		if (selectiveScrollingOrientation != SelectiveScrollingOrientation.Vertical)
		{
			Binding binding2 = new Binding("ContentVerticalOffset");
			binding2.Source = scrollViewer;
			BindingOperations.SetBinding(translateTransform, TranslateTransform.YProperty, binding2);
		}
		uIElement.RenderTransform = translateTransform;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.SelectiveScrollingGrid" /> class. </summary>
	public SelectiveScrollingGrid()
	{
	}
}
