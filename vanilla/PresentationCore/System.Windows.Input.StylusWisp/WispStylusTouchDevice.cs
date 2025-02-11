using MS.Internal;

namespace System.Windows.Input.StylusWisp;

internal sealed class WispStylusTouchDevice : StylusTouchDeviceBase
{
	private WispLogic _stylusLogic;

	private WispLogic.StagingAreaInputItemList _storedStagingAreaItems;

	private static object NoMousePromotionStylusDevice = new object();

	internal WispLogic.StagingAreaInputItemList StoredStagingAreaItems
	{
		get
		{
			if (_storedStagingAreaItems == null)
			{
				_storedStagingAreaItems = new WispLogic.StagingAreaInputItemList();
			}
			return _storedStagingAreaItems;
		}
	}

	internal WispStylusTouchDevice(StylusDeviceBase stylusDevice)
		: base(stylusDevice)
	{
		_stylusLogic = StylusLogic.GetCurrentStylusLogicAs<WispLogic>();
		base.PromotingToOther = true;
	}

	protected override double GetStylusPointWidthOrHeight(StylusPoint stylusPoint, bool isWidth)
	{
		double num = 96.0;
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

	protected override void OnManipulationStarted()
	{
		base.OnManipulationStarted();
		base.PromotingToOther = false;
	}

	protected override void OnManipulationEnded(bool cancel)
	{
		base.OnManipulationEnded(cancel);
		if (cancel)
		{
			base.PromotingToOther = true;
			_stylusLogic.PromoteStoredItemsToMouse(this);
		}
		else
		{
			base.PromotingToOther = false;
		}
		if (_storedStagingAreaItems != null)
		{
			_storedStagingAreaItems.Clear();
		}
	}

	protected override void OnActivateImpl()
	{
		if (StylusTouchDeviceBase.ActiveDeviceCount == 1)
		{
			_stylusLogic.CurrentMousePromotionStylusDevice = base.StylusDevice;
		}
	}

	protected override void OnDeactivateImpl()
	{
		if (_storedStagingAreaItems != null)
		{
			_storedStagingAreaItems.Clear();
		}
		if (StylusTouchDeviceBase.ActiveDeviceCount == 0)
		{
			_stylusLogic.CurrentMousePromotionStylusDevice = null;
		}
		else if (base.IsPrimary)
		{
			_stylusLogic.CurrentMousePromotionStylusDevice = NoMousePromotionStylusDevice;
		}
	}
}
