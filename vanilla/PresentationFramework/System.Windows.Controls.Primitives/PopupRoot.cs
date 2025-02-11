using System.Windows.Automation.Peers;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls.Primitives;

internal sealed class PopupRoot : FrameworkElement
{
	private Decorator _transformDecorator;

	private AdornerDecorator _adornerDecorator;

	protected override int VisualChildrenCount => 1;

	internal UIElement Child
	{
		get
		{
			return _adornerDecorator.Child;
		}
		set
		{
			_adornerDecorator.Child = value;
		}
	}

	internal Vector AnimationOffset
	{
		get
		{
			if (_adornerDecorator.RenderTransform is TranslateTransform translateTransform)
			{
				return new Vector(translateTransform.X, translateTransform.Y);
			}
			return default(Vector);
		}
	}

	internal Transform Transform
	{
		set
		{
			_transformDecorator.LayoutTransform = value;
		}
	}

	static PopupRoot()
	{
		UIElement.SnapsToDevicePixelsProperty.OverrideMetadata(typeof(PopupRoot), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
	}

	internal PopupRoot()
	{
		Initialize();
	}

	private void Initialize()
	{
		_transformDecorator = new Decorator();
		AddVisualChild(_transformDecorator);
		_transformDecorator.ClipToBounds = true;
		_adornerDecorator = new NonLogicalAdornerDecorator();
		_transformDecorator.Child = _adornerDecorator;
	}

	protected override Visual GetVisualChild(int index)
	{
		if (index != 0)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return _transformDecorator;
	}

	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new PopupRootAutomationPeer(this);
	}

	protected override Size MeasureOverride(Size constraint)
	{
		Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
		Popup popup = base.Parent as Popup;
		try
		{
			_transformDecorator.Measure(availableSize);
		}
		catch (Exception savedException)
		{
			if (popup != null)
			{
				popup.SavedException = savedException;
			}
			throw;
		}
		availableSize = _transformDecorator.DesiredSize;
		if (popup != null)
		{
			Size popupSizeRestrictions = GetPopupSizeRestrictions(popup, availableSize, out var restrictWidth, out var restrictHeight);
			if (restrictWidth || restrictHeight)
			{
				if (restrictWidth == restrictHeight)
				{
					availableSize = Get2DRestrictedDesiredSize(popupSizeRestrictions);
				}
				else
				{
					Size availableSize2 = new Size(restrictWidth ? popupSizeRestrictions.Width : double.PositiveInfinity, restrictHeight ? popupSizeRestrictions.Height : double.PositiveInfinity);
					_transformDecorator.Measure(availableSize2);
					availableSize = _transformDecorator.DesiredSize;
					popupSizeRestrictions = GetPopupSizeRestrictions(popup, availableSize, out restrictWidth, out restrictHeight);
					if (restrictWidth || restrictHeight)
					{
						availableSize = Get2DRestrictedDesiredSize(popupSizeRestrictions);
					}
				}
			}
		}
		return availableSize;
	}

	private Size GetPopupSizeRestrictions(Popup popup, Size desiredSize, out bool restrictWidth, out bool restrictHeight)
	{
		Size result = popup.RestrictSize(desiredSize);
		restrictWidth = Math.Abs(result.Width - desiredSize.Width) > 0.01;
		restrictHeight = Math.Abs(result.Height - desiredSize.Height) > 0.01;
		return result;
	}

	private Size Get2DRestrictedDesiredSize(Size restrictedSize)
	{
		_transformDecorator.Measure(restrictedSize);
		Size desiredSize = _transformDecorator.DesiredSize;
		return new Size(Math.Min(restrictedSize.Width, desiredSize.Width), Math.Min(restrictedSize.Height, desiredSize.Height));
	}

	protected override Size ArrangeOverride(Size arrangeSize)
	{
		_transformDecorator.Arrange(new Rect(arrangeSize));
		return arrangeSize;
	}

	internal void SetupLayoutBindings(Popup popup)
	{
		Binding binding = new Binding("Width");
		binding.Mode = BindingMode.OneWay;
		binding.Source = popup;
		_adornerDecorator.SetBinding(FrameworkElement.WidthProperty, binding);
		binding = new Binding("Height");
		binding.Mode = BindingMode.OneWay;
		binding.Source = popup;
		_adornerDecorator.SetBinding(FrameworkElement.HeightProperty, binding);
		binding = new Binding("MinWidth");
		binding.Mode = BindingMode.OneWay;
		binding.Source = popup;
		_adornerDecorator.SetBinding(FrameworkElement.MinWidthProperty, binding);
		binding = new Binding("MinHeight");
		binding.Mode = BindingMode.OneWay;
		binding.Source = popup;
		_adornerDecorator.SetBinding(FrameworkElement.MinHeightProperty, binding);
		binding = new Binding("MaxWidth");
		binding.Mode = BindingMode.OneWay;
		binding.Source = popup;
		_adornerDecorator.SetBinding(FrameworkElement.MaxWidthProperty, binding);
		binding = new Binding("MaxHeight");
		binding.Mode = BindingMode.OneWay;
		binding.Source = popup;
		_adornerDecorator.SetBinding(FrameworkElement.MaxHeightProperty, binding);
	}

	internal void SetupFadeAnimation(Duration duration, bool visible)
	{
		DoubleAnimation animation = new DoubleAnimation(visible ? 0.0 : 1.0, visible ? 1.0 : 0.0, duration, FillBehavior.HoldEnd);
		BeginAnimation(UIElement.OpacityProperty, animation);
	}

	internal void SetupTranslateAnimations(PopupAnimation animationType, Duration duration, bool animateFromRight, bool animateFromBottom)
	{
		UIElement child = Child;
		if (child == null)
		{
			return;
		}
		TranslateTransform translateTransform = _adornerDecorator.RenderTransform as TranslateTransform;
		if (translateTransform == null)
		{
			translateTransform = new TranslateTransform();
			_adornerDecorator.RenderTransform = translateTransform;
		}
		if (animationType == PopupAnimation.Scroll)
		{
			FlowDirection num = (FlowDirection)child.GetValue(FrameworkElement.FlowDirectionProperty);
			FlowDirection flowDirection = base.FlowDirection;
			if (num != flowDirection)
			{
				animateFromRight = !animateFromRight;
			}
			double width = _adornerDecorator.RenderSize.Width;
			DoubleAnimation animation = new DoubleAnimation(animateFromRight ? width : (0.0 - width), 0.0, duration, FillBehavior.Stop);
			translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
		}
		double height = _adornerDecorator.RenderSize.Height;
		DoubleAnimation animation2 = new DoubleAnimation(animateFromBottom ? height : (0.0 - height), 0.0, duration, FillBehavior.Stop);
		translateTransform.BeginAnimation(TranslateTransform.YProperty, animation2);
	}

	internal void StopAnimations()
	{
		BeginAnimation(UIElement.OpacityProperty, null);
		if (_adornerDecorator.RenderTransform is TranslateTransform translateTransform)
		{
			translateTransform.BeginAnimation(TranslateTransform.XProperty, null);
			translateTransform.BeginAnimation(TranslateTransform.YProperty, null);
		}
	}

	internal override bool IgnoreModelParentBuildRoute(RoutedEventArgs e)
	{
		if (e is QueryCursorEventArgs)
		{
			return true;
		}
		if (Child is FrameworkElement frameworkElement)
		{
			return frameworkElement.IgnoreModelParentBuildRoute(e);
		}
		return base.IgnoreModelParentBuildRoute(e);
	}
}
