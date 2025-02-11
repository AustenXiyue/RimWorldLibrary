using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Input.StylusPointer;

internal sealed class PointerTouchDevice : StylusTouchDeviceBase
{
	private PointerStylusDevice _stylusDevice;

	internal PointerTouchDevice(PointerStylusDevice stylusDevice)
		: base(stylusDevice)
	{
		_stylusDevice = stylusDevice;
	}

	protected override void OnManipulationEnded(bool cancel)
	{
		base.OnManipulationEnded(cancel);
		if (cancel)
		{
			base.PromotingToOther = true;
		}
		else
		{
			base.PromotingToOther = false;
		}
	}

	protected override void OnManipulationStarted()
	{
		base.OnManipulationStarted();
		base.PromotingToOther = false;
	}

	protected override double GetStylusPointWidthOrHeight(StylusPoint stylusPoint, bool isWidth)
	{
		double num = 96.0;
		if (ActiveSource?.RootVisual != null)
		{
			num = VisualTreeHelper.GetDpi(ActiveSource.RootVisual).PixelsPerInchX;
		}
		StylusPointProperty stylusPointProperty = (isWidth ? StylusPointProperties.Width : StylusPointProperties.Height);
		double result = 0.0;
		if (stylusPoint.HasProperty(stylusPointProperty))
		{
			result = stylusPoint.GetPropertyValue(stylusPointProperty);
			StylusPointPropertyInfo propertyInfo = stylusPoint.Description.GetPropertyInfo(stylusPointProperty);
			result = (DoubleUtil.AreClose(propertyInfo.Resolution, 0.0) ? 0.0 : (result / (double)propertyInfo.Resolution));
			if (propertyInfo.Unit == StylusPointPropertyUnit.Centimeters)
			{
				result /= 2.54;
			}
			result *= num;
		}
		return result;
	}

	protected override void OnActivateImpl()
	{
	}

	protected override void OnDeactivateImpl()
	{
	}
}
