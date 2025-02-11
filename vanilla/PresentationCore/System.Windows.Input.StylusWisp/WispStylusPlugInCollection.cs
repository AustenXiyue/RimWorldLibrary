using System.Windows.Input.StylusPlugIns;

namespace System.Windows.Input.StylusWisp;

internal class WispStylusPlugInCollection : StylusPlugInCollectionBase
{
	private PenContexts _penContexts;

	internal override bool IsActiveForInput => _penContexts != null;

	internal override object SyncRoot
	{
		get
		{
			if (_penContexts == null)
			{
				return null;
			}
			return _penContexts.SyncRoot;
		}
	}

	internal PenContexts PenContexts => _penContexts;

	internal override void UpdateState(UIElement element)
	{
		bool flag = true;
		using (element.Dispatcher.DisableProcessing())
		{
			if (element.IsVisible && element.IsEnabled && element.IsHitTestVisible)
			{
				PresentationSource presentationSource = PresentationSource.CriticalFromVisual(element);
				if (presentationSource != null)
				{
					flag = false;
					if (_penContexts == null)
					{
						_ = (InputManager)element.Dispatcher.InputManager;
						PenContexts penContextsFromHwnd = StylusLogic.GetCurrentStylusLogicAs<WispLogic>().GetPenContextsFromHwnd(presentationSource);
						if (penContextsFromHwnd != null)
						{
							_penContexts = penContextsFromHwnd;
							lock (penContextsFromHwnd.SyncRoot)
							{
								penContextsFromHwnd.AddStylusPlugInCollection(base.Wrapper);
								foreach (StylusPlugIn item in base.Wrapper)
								{
									item.InvalidateIsActiveForInput();
								}
								base.Wrapper.OnLayoutUpdated(base.Wrapper, EventArgs.Empty);
							}
						}
					}
				}
			}
			if (flag)
			{
				Unhook();
			}
		}
	}

	internal override void Unhook()
	{
		if (_penContexts == null)
		{
			return;
		}
		lock (_penContexts.SyncRoot)
		{
			_penContexts.RemoveStylusPlugInCollection(base.Wrapper);
			_penContexts = null;
			foreach (StylusPlugIn item in base.Wrapper)
			{
				item.InvalidateIsActiveForInput();
			}
		}
	}
}
