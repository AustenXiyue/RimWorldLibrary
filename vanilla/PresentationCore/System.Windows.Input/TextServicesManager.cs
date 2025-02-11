using System.Windows.Threading;
using MS.Internal;

namespace System.Windows.Input;

internal class TextServicesManager : DispatcherObject
{
	private readonly InputManager _inputManager;

	internal TextServicesManager(InputManager inputManager)
	{
		_inputManager = inputManager;
		_inputManager.PreProcessInput += PreProcessInput;
		_inputManager.PostProcessInput += PostProcessInput;
	}

	internal void Focus(DependencyObject focus)
	{
		if (focus == null)
		{
			base.Dispatcher.IsTSFMessagePumpEnabled = false;
			return;
		}
		base.Dispatcher.IsTSFMessagePumpEnabled = true;
		if (!(bool)focus.GetValue(InputMethod.IsInputMethodSuspendedProperty))
		{
			InputMethod.Current.EnableOrDisableInputMethod((bool)focus.GetValue(InputMethod.IsInputMethodEnabledProperty));
		}
	}

	private void PreProcessInput(object sender, PreProcessInputEventArgs e)
	{
		if (!TextServicesLoader.ServicesInstalled || (e.StagingItem.Input.RoutedEvent != Keyboard.PreviewKeyDownEvent && e.StagingItem.Input.RoutedEvent != Keyboard.PreviewKeyUpEvent) || IsSysKeyDown() || InputMethod.IsImm32ImeCurrent() || !(Keyboard.FocusedElement is DependencyObject dependencyObject) || (bool)dependencyObject.GetValue(InputMethod.IsInputMethodSuspendedProperty))
		{
			return;
		}
		KeyEventArgs keyEventArgs = (KeyEventArgs)e.StagingItem.Input;
		if (!keyEventArgs.Handled)
		{
			TextServicesContext dispatcherCurrent = TextServicesContext.DispatcherCurrent;
			if (dispatcherCurrent != null && TextServicesKeystroke(dispatcherCurrent, keyEventArgs, test: true))
			{
				keyEventArgs.MarkImeProcessed();
			}
		}
	}

	private void PostProcessInput(object sender, ProcessInputEventArgs e)
	{
		if (!TextServicesLoader.ServicesInstalled || InputMethod.IsImm32ImeCurrent() || !(Keyboard.FocusedElement is DependencyObject dependencyObject) || (bool)dependencyObject.GetValue(InputMethod.IsInputMethodSuspendedProperty))
		{
			return;
		}
		if (e.StagingItem.Input.RoutedEvent == Keyboard.PreviewKeyDownEvent || e.StagingItem.Input.RoutedEvent == Keyboard.PreviewKeyUpEvent)
		{
			if (IsSysKeyDown())
			{
				return;
			}
			KeyEventArgs keyEventArgs = (KeyEventArgs)e.StagingItem.Input;
			if (!keyEventArgs.Handled && keyEventArgs.Key == Key.ImeProcessed)
			{
				TextServicesContext dispatcherCurrent = TextServicesContext.DispatcherCurrent;
				if (dispatcherCurrent != null && TextServicesKeystroke(dispatcherCurrent, keyEventArgs, test: false))
				{
					keyEventArgs.Handled = true;
				}
			}
		}
		else if (e.StagingItem.Input.RoutedEvent == Keyboard.KeyDownEvent || e.StagingItem.Input.RoutedEvent == Keyboard.KeyUpEvent)
		{
			KeyEventArgs keyEventArgs = (KeyEventArgs)e.StagingItem.Input;
			if (!keyEventArgs.Handled && keyEventArgs.Key == Key.ImeProcessed)
			{
				keyEventArgs.Handled = true;
			}
		}
	}

	private bool TextServicesKeystroke(TextServicesContext context, KeyEventArgs keyArgs, bool test)
	{
		int wParam;
		int num;
		switch (keyArgs.RealKey)
		{
		case Key.RightShift:
			wParam = 16;
			num = 54;
			break;
		case Key.LeftShift:
			wParam = 16;
			num = 42;
			break;
		default:
			wParam = KeyInterop.VirtualKeyFromKey(keyArgs.RealKey);
			num = 0;
			break;
		}
		int num2 = (num << 16) | 1;
		TextServicesContext.KeyOp op;
		if (keyArgs.RoutedEvent == Keyboard.PreviewKeyDownEvent)
		{
			op = (test ? TextServicesContext.KeyOp.TestDown : TextServicesContext.KeyOp.Down);
		}
		else
		{
			num2 |= -1073741824;
			op = ((!test) ? TextServicesContext.KeyOp.Up : TextServicesContext.KeyOp.TestUp);
		}
		return context.Keystroke(wParam, num2, op);
	}

	private bool IsSysKeyDown()
	{
		if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt) || Keyboard.IsKeyDown(Key.F10))
		{
			return true;
		}
		return false;
	}
}
