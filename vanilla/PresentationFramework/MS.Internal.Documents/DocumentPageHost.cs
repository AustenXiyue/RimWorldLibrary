using System;
using System.Windows;
using System.Windows.Media;

namespace MS.Internal.Documents;

internal class DocumentPageHost : FrameworkElement
{
	internal Point CachedOffset;

	private Visual _pageVisual;

	internal Visual PageVisual
	{
		get
		{
			return _pageVisual;
		}
		set
		{
			if (_pageVisual != null)
			{
				ContainerVisual containerVisual = VisualTreeHelper.GetParent(_pageVisual) as ContainerVisual;
				Invariant.Assert(containerVisual != null);
				containerVisual.Children.Clear();
				RemoveVisualChild(containerVisual);
			}
			_pageVisual = value;
			if (_pageVisual != null)
			{
				ContainerVisual containerVisual = new ContainerVisual();
				AddVisualChild(containerVisual);
				containerVisual.Children.Add(_pageVisual);
				containerVisual.SetValue(FrameworkElement.FlowDirectionProperty, FlowDirection.LeftToRight);
			}
		}
	}

	protected override int VisualChildrenCount => (_pageVisual != null) ? 1 : 0;

	internal DocumentPageHost()
	{
	}

	internal static void DisconnectPageVisual(Visual pageVisual)
	{
		if (VisualTreeHelper.GetParent(pageVisual) is Visual visual)
		{
			((VisualTreeHelper.GetParent((visual as ContainerVisual) ?? throw new ArgumentException(SR.DocumentPageView_ParentNotDocumentPageHost, "pageVisual")) as DocumentPageHost) ?? throw new ArgumentException(SR.DocumentPageView_ParentNotDocumentPageHost, "pageVisual")).PageVisual = null;
		}
	}

	protected override Visual GetVisualChild(int index)
	{
		if (index != 0 || _pageVisual == null)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return VisualTreeHelper.GetParent(_pageVisual) as Visual;
	}
}
