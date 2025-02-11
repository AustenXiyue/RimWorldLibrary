using System.Windows.Input.StylusPlugIns;

namespace System.Windows.Input.StylusPointer;

internal class PointerStylusPlugInCollection : StylusPlugInCollectionBase
{
	private PointerStylusPlugInManager _manager;

	internal override bool IsActiveForInput => _manager != null;

	internal override object SyncRoot => null;

	internal override void UpdateState(UIElement element)
	{
		bool flag = true;
		if (element.IsVisible && element.IsEnabled && element.IsHitTestVisible)
		{
			PresentationSource presentationSource = PresentationSource.CriticalFromVisual(element);
			if (presentationSource != null)
			{
				flag = false;
				if (_manager == null)
				{
					_manager = StylusLogic.GetCurrentStylusLogicAs<PointerLogic>().PlugInManagers[presentationSource];
					if (_manager != null)
					{
						_manager.AddStylusPlugInCollection(base.Wrapper);
						foreach (StylusPlugIn item in base.Wrapper)
						{
							item.InvalidateIsActiveForInput();
						}
						base.Wrapper.OnLayoutUpdated(base.Wrapper, EventArgs.Empty);
					}
				}
			}
		}
		if (flag)
		{
			Unhook();
		}
	}

	internal override void Unhook()
	{
		if (_manager == null)
		{
			return;
		}
		_manager.RemoveStylusPlugInCollection(base.Wrapper);
		_manager = null;
		foreach (StylusPlugIn item in base.Wrapper)
		{
			item.InvalidateIsActiveForInput();
		}
	}
}
