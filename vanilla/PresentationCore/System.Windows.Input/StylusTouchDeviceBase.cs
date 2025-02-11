using System.Windows.Media;

namespace System.Windows.Input;

internal abstract class StylusTouchDeviceBase : TouchDevice
{
	[ThreadStatic]
	private static int _activeDeviceCount;

	private TouchAction _lastAction = TouchAction.Move;

	private StylusPointDescription _stylusPointDescription = new StylusPointDescription(new StylusPointPropertyInfo[5]
	{
		StylusPointPropertyInfoDefaults.X,
		StylusPointPropertyInfoDefaults.Y,
		StylusPointPropertyInfoDefaults.NormalPressure,
		StylusPointPropertyInfoDefaults.Width,
		StylusPointPropertyInfoDefaults.Height
	});

	internal const double CentimetersPerInch = 2.54;

	public bool PromotingToOther { get; protected set; }

	internal bool DownHandled { get; private set; }

	internal StylusDeviceBase StylusDevice { get; private set; }

	internal bool IsPrimary { get; private set; }

	internal static int ActiveDeviceCount => _activeDeviceCount;

	internal StylusTouchDeviceBase(StylusDeviceBase stylusDevice)
		: base(stylusDevice.Id)
	{
		StylusDevice = stylusDevice;
		_stylusPointDescription = StylusDevice?.TabletDevice?.TabletDeviceImpl?.StylusPointDescription ?? _stylusPointDescription;
		PromotingToOther = true;
	}

	public override TouchPoint GetTouchPoint(IInputElement relativeTo)
	{
		Point position = StylusDevice.GetPosition(relativeTo);
		Rect bounds = GetBounds(StylusDevice.RawStylusPoint, position, relativeTo);
		return new TouchPoint(this, position, bounds, _lastAction);
	}

	private Rect GetBounds(StylusPoint stylusPoint, Point position, IInputElement relativeTo)
	{
		GetRootTransforms(relativeTo, out var elementToRoot, out var rootToElement);
		return GetBounds(stylusPoint, position, relativeTo, elementToRoot, rootToElement);
	}

	private Rect GetBounds(StylusPoint stylusPoint, Point position, IInputElement relativeTo, GeneralTransform elementToRoot, GeneralTransform rootToElement)
	{
		double stylusPointWidthOrHeight = GetStylusPointWidthOrHeight(stylusPoint, isWidth: true);
		double stylusPointWidthOrHeight2 = GetStylusPointWidthOrHeight(stylusPoint, isWidth: false);
		if (elementToRoot == null || !elementToRoot.TryTransform(position, out var result))
		{
			result = position;
		}
		Rect rect = new Rect(result.X - stylusPointWidthOrHeight * 0.5, result.Y - stylusPointWidthOrHeight2 * 0.5, stylusPointWidthOrHeight, stylusPointWidthOrHeight2);
		return rootToElement?.TransformBounds(rect) ?? rect;
	}

	protected abstract double GetStylusPointWidthOrHeight(StylusPoint stylusPoint, bool isWidth);

	public override TouchPointCollection GetIntermediateTouchPoints(IInputElement relativeTo)
	{
		StylusPointCollection stylusPoints = StylusDevice.GetStylusPoints(relativeTo, _stylusPointDescription);
		int count = stylusPoints.Count;
		TouchPointCollection touchPointCollection = new TouchPointCollection();
		GetRootTransforms(relativeTo, out var elementToRoot, out var rootToElement);
		for (int i = 0; i < count; i++)
		{
			StylusPoint stylusPoint = stylusPoints[i];
			Point position = new Point(stylusPoint.X, stylusPoint.Y);
			Rect bounds = GetBounds(stylusPoint, position, relativeTo, elementToRoot, rootToElement);
			TouchPoint item = new TouchPoint(this, position, bounds, _lastAction);
			touchPointCollection.Add(item);
		}
		return touchPointCollection;
	}

	private void GetRootTransforms(IInputElement relativeTo, out GeneralTransform elementToRoot, out GeneralTransform rootToElement)
	{
		elementToRoot = (rootToElement = null);
		DependencyObject containingVisual = InputElement.GetContainingVisual(relativeTo as DependencyObject);
		if (containingVisual != null)
		{
			Visual visual = PresentationSource.CriticalFromVisual(containingVisual)?.RootVisual;
			Visual containingVisual2D = VisualTreeHelper.GetContainingVisual2D(containingVisual);
			if (visual != null && containingVisual2D != null)
			{
				elementToRoot = containingVisual2D.TransformToAncestor(visual);
				rootToElement = visual.TransformToDescendant(containingVisual2D);
			}
		}
	}

	internal void ChangeActiveSource(PresentationSource activeSource)
	{
		SetActiveSource(activeSource);
	}

	internal void OnActivate()
	{
		Activate();
		_activeDeviceCount++;
		if (_activeDeviceCount == 1)
		{
			IsPrimary = true;
			OnActivateImpl();
		}
		PromotingToOther = true;
	}

	protected abstract void OnActivateImpl();

	internal void OnDeactivate()
	{
		Deactivate();
		PromotingToOther = true;
		DownHandled = false;
		_activeDeviceCount--;
		OnDeactivateImpl();
		IsPrimary = false;
	}

	protected abstract void OnDeactivateImpl();

	internal bool OnDown()
	{
		_lastAction = TouchAction.Down;
		DownHandled = ReportDown();
		return DownHandled;
	}

	internal bool OnMove()
	{
		_lastAction = TouchAction.Move;
		return ReportMove();
	}

	internal bool OnUp()
	{
		_lastAction = TouchAction.Up;
		return ReportUp();
	}
}
