using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace MS.Internal.Documents;

internal class UIElementIsland : ContainerVisual, IContentHost, IDisposable
{
	private UIElement _child;

	private bool _layoutInProgress;

	internal UIElement Root => _child;

	IEnumerator<IInputElement> IContentHost.HostedElements
	{
		get
		{
			List<IInputElement> list = new List<IInputElement>();
			if (_child != null)
			{
				list.Add(_child);
			}
			return list.GetEnumerator();
		}
	}

	internal event DesiredSizeChangedEventHandler DesiredSizeChanged;

	internal UIElementIsland(UIElement child)
	{
		SetFlags(value: true, VisualFlags.IsLayoutIslandRoot);
		_child = child;
		if (_child != null)
		{
			if (VisualTreeHelper.GetParent(_child) is Visual visual)
			{
				Invariant.Assert(visual is UIElementIsland, "Parent should always be a UIElementIsland.");
				((UIElementIsland)visual).Dispose();
			}
			base.Children.Add(_child);
		}
	}

	internal Size DoLayout(Size availableSize, bool horizontalAutoSize, bool verticalAutoSize)
	{
		Size size = default(Size);
		if (_child != null)
		{
			if (_child is FrameworkElement && ((FrameworkElement)_child).Parent != null)
			{
				SetValue(FrameworkElement.FlowDirectionProperty, ((FrameworkElement)_child).Parent.GetValue(FrameworkElement.FlowDirectionProperty));
			}
			try
			{
				_layoutInProgress = true;
				_child.Measure(availableSize);
				size.Width = (horizontalAutoSize ? _child.DesiredSize.Width : availableSize.Width);
				size.Height = (verticalAutoSize ? _child.DesiredSize.Height : availableSize.Height);
				_child.Arrange(new Rect(size));
			}
			finally
			{
				_layoutInProgress = false;
			}
		}
		return size;
	}

	public void Dispose()
	{
		if (_child != null)
		{
			base.Children.Clear();
			_child = null;
		}
		GC.SuppressFinalize(this);
	}

	IInputElement IContentHost.InputHitTest(Point point)
	{
		return null;
	}

	ReadOnlyCollection<Rect> IContentHost.GetRectangles(ContentElement child)
	{
		return new ReadOnlyCollection<Rect>(new List<Rect>());
	}

	void IContentHost.OnChildDesiredSizeChanged(UIElement child)
	{
		Invariant.Assert(child == _child);
		if (!_layoutInProgress && this.DesiredSizeChanged != null)
		{
			this.DesiredSizeChanged(this, new DesiredSizeChangedEventArgs(child));
		}
	}
}
