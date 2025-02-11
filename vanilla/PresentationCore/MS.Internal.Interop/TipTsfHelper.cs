using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using MS.Internal.WindowsRuntime.Windows.UI.ViewManagement;

namespace MS.Internal.Interop;

internal static class TipTsfHelper
{
	private static bool s_PlatformSupported = true;

	[ThreadStatic]
	private static DispatcherOperation s_KbOperation = null;

	private static bool CheckAndDispatchKbOperation(Action<DependencyObject> kbCall, DependencyObject focusedObject)
	{
		DispatcherOperation dispatcherOperation = s_KbOperation;
		if (dispatcherOperation != null && dispatcherOperation.Status == DispatcherOperationStatus.Pending)
		{
			s_KbOperation.Abort();
		}
		s_KbOperation = null;
		if (Dispatcher.CurrentDispatcher._disableProcessingCount > 0)
		{
			s_KbOperation = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, (Action)delegate
			{
				if (Keyboard.FocusedElement == focusedObject)
				{
					kbCall(focusedObject);
				}
			});
			return true;
		}
		return false;
	}

	internal static void Show(DependencyObject focusedObject)
	{
		if (!s_PlatformSupported || CoreAppContextSwitches.DisableImplicitTouchKeyboardInvocation || !StylusLogic.IsStylusAndTouchSupportEnabled || StylusLogic.IsPointerStackEnabled || CheckAndDispatchKbOperation(Show, focusedObject) || !ShouldShow(focusedObject))
		{
			return;
		}
		try
		{
			InputPane forWindow;
			using (forWindow = InputPane.GetForWindow(GetHwndSource(focusedObject)))
			{
				forWindow?.TryShow();
			}
		}
		catch (PlatformNotSupportedException)
		{
			s_PlatformSupported = false;
		}
	}

	internal static void Hide(DependencyObject focusedObject)
	{
		if (!s_PlatformSupported || !StylusLogic.IsStylusAndTouchSupportEnabled || StylusLogic.IsPointerStackEnabled || CheckAndDispatchKbOperation(Hide, focusedObject))
		{
			return;
		}
		try
		{
			InputPane forWindow;
			using (forWindow = InputPane.GetForWindow(GetHwndSource(focusedObject)))
			{
				forWindow?.TryHide();
			}
		}
		catch (PlatformNotSupportedException)
		{
			s_PlatformSupported = false;
		}
	}

	private static bool ShouldShow(DependencyObject focusedObject)
	{
		AutomationPeer automationPeer = null;
		if (focusedObject is UIElement uIElement)
		{
			automationPeer = uIElement.GetAutomationPeer();
		}
		else if (focusedObject is UIElement3D uIElement3D)
		{
			automationPeer = uIElement3D.GetAutomationPeer();
		}
		else if (focusedObject is ContentElement contentElement)
		{
			automationPeer = contentElement.GetAutomationPeer();
		}
		return automationPeer?.GetPattern(PatternInterface.Text) != null;
	}

	private static HwndSource GetHwndSource(DependencyObject focusedObject)
	{
		return PresentationSource.CriticalFromVisual(focusedObject) as HwndSource;
	}
}
