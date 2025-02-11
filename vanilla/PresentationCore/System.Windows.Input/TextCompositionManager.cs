using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Threading;
using Microsoft.Win32;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace System.Windows.Input;

/// <summary>Provides facilities for managing events related to input and text compositions.</summary>
public sealed class TextCompositionManager : DispatcherObject
{
	internal static class NumpadScanCode
	{
		internal const int NumpadDot = 83;

		internal const int NumpadPlus = 78;

		internal const int Numpad0 = 82;

		internal const int Numpad1 = 79;

		internal const int Numpad2 = 80;

		internal const int Numpad3 = 81;

		internal const int Numpad4 = 75;

		internal const int Numpad5 = 76;

		internal const int Numpad6 = 77;

		internal const int Numpad7 = 71;

		internal const int Numpad8 = 72;

		internal const int Numpad9 = 73;

		internal static int DigitFromScanCode(int scanCode)
		{
			return scanCode switch
			{
				82 => 0, 
				79 => 1, 
				80 => 2, 
				81 => 3, 
				75 => 4, 
				76 => 5, 
				77 => 6, 
				71 => 7, 
				72 => 8, 
				73 => 9, 
				_ => -1, 
			};
		}
	}

	/// <summary>Identifies the <see cref="E:System.Windows.Input.TextCompositionManager.PreviewTextInputStart" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.TextCompositionManager.PreviewTextInputStart" /> attached event.</returns>
	public static readonly RoutedEvent PreviewTextInputStartEvent = EventManager.RegisterRoutedEvent("PreviewTextInputStart", RoutingStrategy.Tunnel, typeof(TextCompositionEventHandler), typeof(TextCompositionManager));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.TextCompositionManager.TextInputStart" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.TextCompositionManager.TextInputStart" /> attached event.</returns>
	public static readonly RoutedEvent TextInputStartEvent = EventManager.RegisterRoutedEvent("TextInputStart", RoutingStrategy.Bubble, typeof(TextCompositionEventHandler), typeof(TextCompositionManager));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.TextCompositionManager.PreviewTextInputUpdate" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.TextCompositionManager.PreviewTextInputUpdate" /> attached event.</returns>
	public static readonly RoutedEvent PreviewTextInputUpdateEvent = EventManager.RegisterRoutedEvent("PreviewTextInputUpdate", RoutingStrategy.Tunnel, typeof(TextCompositionEventHandler), typeof(TextCompositionManager));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.TextCompositionManager.TextInputUpdate" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.TextCompositionManager.TextInputUpdate" /> attached event.</returns>
	public static readonly RoutedEvent TextInputUpdateEvent = EventManager.RegisterRoutedEvent("TextInputUpdate", RoutingStrategy.Bubble, typeof(TextCompositionEventHandler), typeof(TextCompositionManager));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.TextCompositionManager.PreviewTextInput" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.TextCompositionManager.PreviewTextInput" /> attached event.</returns>
	public static readonly RoutedEvent PreviewTextInputEvent = EventManager.RegisterRoutedEvent("PreviewTextInput", RoutingStrategy.Tunnel, typeof(TextCompositionEventHandler), typeof(TextCompositionManager));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.TextCompositionManager.TextInput" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.TextCompositionManager.TextInput" /> attached event.</returns>
	public static readonly RoutedEvent TextInputEvent = EventManager.RegisterRoutedEvent("TextInput", RoutingStrategy.Bubble, typeof(TextCompositionEventHandler), typeof(TextCompositionManager));

	private readonly InputManager _inputManager;

	private DeadCharTextComposition _deadCharTextComposition;

	private bool _altNumpadEntryMode;

	private int _altNumpadEntry;

	private AltNumpadConversionMode _altNumpadConversionMode;

	private TextComposition _altNumpadcomposition;

	private static bool _isHexNumpadRegistryChecked = false;

	private static bool _isHexNumpadEnabled = false;

	private const int EncodingBufferLen = 4;

	private bool HexConversionMode
	{
		get
		{
			if (_altNumpadConversionMode == AltNumpadConversionMode.HexDefaultCodePage || _altNumpadConversionMode == AltNumpadConversionMode.HexUnicode)
			{
				return true;
			}
			return false;
		}
	}

	private static bool IsHexNumpadEnabled
	{
		get
		{
			if (!_isHexNumpadRegistryChecked)
			{
				RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Input Method");
				if (registryKey != null)
				{
					object value = registryKey.GetValue("EnableHexNumpad");
					if (value is string && (string)value != "0")
					{
						_isHexNumpadEnabled = true;
					}
				}
				_isHexNumpadRegistryChecked = true;
			}
			return _isHexNumpadEnabled;
		}
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.TextCompositionManager.PreviewTextInputStart" />   attached event.</summary>
	/// <param name="element">A dependency object to add the event handler to.  The dependency object must be a <see cref="T:System.Windows.UIElement" /> or a <see cref="T:System.Windows.ContentElement" />.</param>
	/// <param name="handler">A delegate that designates the handler to add.</param>
	public static void AddPreviewTextInputStartHandler(DependencyObject element, TextCompositionEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		UIElement.AddHandler(element, PreviewTextInputStartEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.TextCompositionManager.TextInputStart" />   attached event.</summary>
	/// <param name="element">A dependency object to remove the event handler from.  The dependency object must be a <see cref="T:System.Windows.UIElement" /> or a <see cref="T:System.Windows.ContentElement" />.</param>
	/// <param name="handler">A delegate that designates the handler to remove.</param>
	public static void RemovePreviewTextInputStartHandler(DependencyObject element, TextCompositionEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		UIElement.RemoveHandler(element, PreviewTextInputStartEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.TextCompositionManager.TextInputStart" />   attached event.</summary>
	/// <param name="element">A dependency object to add the event handler to.  The dependency object must be a <see cref="T:System.Windows.UIElement" /> or a <see cref="T:System.Windows.ContentElement" />.</param>
	/// <param name="handler">A delegate that designates the handler to add.</param>
	public static void AddTextInputStartHandler(DependencyObject element, TextCompositionEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		UIElement.AddHandler(element, TextInputStartEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.TextCompositionManager.TextInputStart" />   attached event.</summary>
	/// <param name="element">A dependency object to remove the event handler from.  The dependency object must be a <see cref="T:System.Windows.UIElement" /> or a <see cref="T:System.Windows.ContentElement" />.</param>
	/// <param name="handler">A delegate that designates the handler to remove.</param>
	public static void RemoveTextInputStartHandler(DependencyObject element, TextCompositionEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		UIElement.RemoveHandler(element, TextInputStartEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.TextCompositionManager.PreviewTextInputUpdate" />   attached event.</summary>
	/// <param name="element">A dependency object to add the event handler to.  The dependency object must be a <see cref="T:System.Windows.UIElement" /> or a <see cref="T:System.Windows.ContentElement" />.</param>
	/// <param name="handler">A delegate that designates the handler to add.</param>
	public static void AddPreviewTextInputUpdateHandler(DependencyObject element, TextCompositionEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		UIElement.AddHandler(element, PreviewTextInputUpdateEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.TextCompositionManager.PreviewTextInputUpdate" />   attached event.</summary>
	/// <param name="element">A dependency object to remove the event handler from.  The dependency object must be a <see cref="T:System.Windows.UIElement" /> or a <see cref="T:System.Windows.ContentElement" />.</param>
	/// <param name="handler">A delegate that designates the handler to remove.</param>
	public static void RemovePreviewTextInputUpdateHandler(DependencyObject element, TextCompositionEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		UIElement.RemoveHandler(element, PreviewTextInputUpdateEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.TextCompositionManager.TextInputUpdate" />   attached event.</summary>
	/// <param name="element">A dependency object to add the event handler to.  The dependency object must be a <see cref="T:System.Windows.UIElement" /> or a <see cref="T:System.Windows.ContentElement" />.</param>
	/// <param name="handler">A delegate that designates the handler to add.</param>
	public static void AddTextInputUpdateHandler(DependencyObject element, TextCompositionEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		UIElement.AddHandler(element, TextInputUpdateEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.TextCompositionManager.TextInputUpdate" />   attached event.</summary>
	/// <param name="element">A dependency object to remove the event handler from.  The dependency object must be a <see cref="T:System.Windows.UIElement" /> or a <see cref="T:System.Windows.ContentElement" />.</param>
	/// <param name="handler">A delegate that designates the handler to remove.</param>
	public static void RemoveTextInputUpdateHandler(DependencyObject element, TextCompositionEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		UIElement.RemoveHandler(element, TextInputUpdateEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.TextCompositionManager.PreviewTextInput" />   attached event.</summary>
	/// <param name="element">A dependency object to add the event handler to.  The dependency object must be a <see cref="T:System.Windows.UIElement" /> or a <see cref="T:System.Windows.ContentElement" />.</param>
	/// <param name="handler">A delegate that designates the handler to add.</param>
	public static void AddPreviewTextInputHandler(DependencyObject element, TextCompositionEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		UIElement.AddHandler(element, PreviewTextInputEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.TextCompositionManager.PreviewTextInput" />   attached event.</summary>
	/// <param name="element">A dependency object to remove the event handler from.  The dependency object must be a <see cref="T:System.Windows.UIElement" /> or a <see cref="T:System.Windows.ContentElement" />.</param>
	/// <param name="handler">A delegate that designates the handler to remove.</param>
	public static void RemovePreviewTextInputHandler(DependencyObject element, TextCompositionEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		UIElement.RemoveHandler(element, PreviewTextInputEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.TextCompositionManager.TextInput" />   attached event.</summary>
	/// <param name="element">A dependency object to add the event handler to.  The dependency object must be a <see cref="T:System.Windows.UIElement" /> or a <see cref="T:System.Windows.ContentElement" />.</param>
	/// <param name="handler">A delegate that designates the handler to add.</param>
	public static void AddTextInputHandler(DependencyObject element, TextCompositionEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		UIElement.AddHandler(element, TextInputEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.TextCompositionManager.TextInput" />   attached event.</summary>
	/// <param name="element">A dependency object to remove the event handler from.  The dependency object must be a <see cref="T:System.Windows.UIElement" /> or a <see cref="T:System.Windows.ContentElement" />.</param>
	/// <param name="handler">A delegate that designates the handler to remove.</param>
	public static void RemoveTextInputHandler(DependencyObject element, TextCompositionEventHandler handler)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		UIElement.RemoveHandler(element, TextInputEvent, handler);
	}

	internal TextCompositionManager(InputManager inputManager)
	{
		_inputManager = inputManager;
		_inputManager.PreProcessInput += PreProcessInput;
		_inputManager.PostProcessInput += PostProcessInput;
	}

	/// <summary>Starts a specified text composition.</summary>
	/// <returns>true if the text composition was successfully started; otherwise, false.</returns>
	/// <param name="composition">A <see cref="T:System.Windows.Input.TextComposition" /> object to start.</param>
	public static bool StartComposition(TextComposition composition)
	{
		return UnsafeStartComposition(composition);
	}

	/// <summary>Updates a specified text composition.</summary>
	/// <returns>true if the text composition was successfully updated; otherwise, false.</returns>
	/// <param name="composition">A <see cref="T:System.Windows.Input.TextComposition" /> object to update.</param>
	public static bool UpdateComposition(TextComposition composition)
	{
		return UnsafeUpdateComposition(composition);
	}

	/// <summary>Completes a specified text composition.</summary>
	/// <returns>true if the text composition was successfully completed; otherwise, false.</returns>
	/// <param name="composition">A <see cref="T:System.Windows.Input.TextComposition" /> object to complete.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when composition is null.</exception>
	/// <exception cref="T:System.ArgumentException">Raised when there is no input manager associated with composition, or when the text composition is already marked as completed.</exception>
	public static bool CompleteComposition(TextComposition composition)
	{
		return UnsafeCompleteComposition(composition);
	}

	private static bool UnsafeStartComposition(TextComposition composition)
	{
		if (composition == null)
		{
			throw new ArgumentNullException("composition");
		}
		if (composition._InputManager == null)
		{
			throw new ArgumentException(SR.Format(SR.TextCompositionManager_NoInputManager, "composition"));
		}
		if (composition.Stage != 0)
		{
			throw new ArgumentException(SR.Format(SR.TextCompositionManager_TextCompositionHasStarted, "composition"));
		}
		composition.Stage = TextCompositionStage.Started;
		TextCompositionEventArgs textCompositionEventArgs = new TextCompositionEventArgs(composition._InputDevice, composition);
		textCompositionEventArgs.RoutedEvent = PreviewTextInputStartEvent;
		textCompositionEventArgs.Source = composition.Source;
		return composition._InputManager.ProcessInput(textCompositionEventArgs);
	}

	private static bool UnsafeUpdateComposition(TextComposition composition)
	{
		if (composition == null)
		{
			throw new ArgumentNullException("composition");
		}
		if (composition._InputManager == null)
		{
			throw new ArgumentException(SR.Format(SR.TextCompositionManager_NoInputManager, "composition"));
		}
		if (composition.Stage == TextCompositionStage.None)
		{
			throw new ArgumentException(SR.Format(SR.TextCompositionManager_TextCompositionNotStarted, "composition"));
		}
		if (composition.Stage == TextCompositionStage.Done)
		{
			throw new ArgumentException(SR.Format(SR.TextCompositionManager_TextCompositionHasDone, "composition"));
		}
		TextCompositionEventArgs textCompositionEventArgs = new TextCompositionEventArgs(composition._InputDevice, composition);
		textCompositionEventArgs.RoutedEvent = PreviewTextInputUpdateEvent;
		textCompositionEventArgs.Source = composition.Source;
		return composition._InputManager.ProcessInput(textCompositionEventArgs);
	}

	private static bool UnsafeCompleteComposition(TextComposition composition)
	{
		if (composition == null)
		{
			throw new ArgumentNullException("composition");
		}
		if (composition._InputManager == null)
		{
			throw new ArgumentException(SR.Format(SR.TextCompositionManager_NoInputManager, "composition"));
		}
		if (composition.Stage == TextCompositionStage.None)
		{
			throw new ArgumentException(SR.Format(SR.TextCompositionManager_TextCompositionNotStarted, "composition"));
		}
		if (composition.Stage == TextCompositionStage.Done)
		{
			throw new ArgumentException(SR.Format(SR.TextCompositionManager_TextCompositionHasDone, "composition"));
		}
		composition.Stage = TextCompositionStage.Done;
		TextCompositionEventArgs textCompositionEventArgs = new TextCompositionEventArgs(composition._InputDevice, composition);
		textCompositionEventArgs.RoutedEvent = PreviewTextInputEvent;
		textCompositionEventArgs.Source = composition.Source;
		return composition._InputManager.ProcessInput(textCompositionEventArgs);
	}

	private static string GetCurrentOEMCPEncoding(int code)
	{
		return CharacterEncoding(MS.Win32.UnsafeNativeMethods.GetOEMCP(), code);
	}

	private static string CharacterEncoding(int cp, int code)
	{
		byte[] array = ConvertCodeToByteArray(code);
		StringBuilder stringBuilder = new StringBuilder(4);
		int num = MS.Win32.UnsafeNativeMethods.MultiByteToWideChar(cp, 5, array, array.Length, stringBuilder, 4);
		if (num == 0)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		stringBuilder.Length = num;
		return stringBuilder.ToString();
	}

	private void PreProcessInput(object sender, PreProcessInputEventArgs e)
	{
		if (e.StagingItem.Input.RoutedEvent == Keyboard.KeyDownEvent)
		{
			KeyEventArgs keyEventArgs = (KeyEventArgs)e.StagingItem.Input;
			if (!keyEventArgs.Handled)
			{
				if (!_altNumpadEntryMode)
				{
					EnterAltNumpadEntryMode(keyEventArgs.RealKey);
				}
				else if (HandleAltNumpadEntry(keyEventArgs.RealKey, keyEventArgs.ScanCode, keyEventArgs.IsExtendedKey))
				{
					if (_altNumpadcomposition == null)
					{
						_altNumpadcomposition = new TextComposition(_inputManager, (IInputElement)keyEventArgs.Source, "", TextCompositionAutoComplete.Off, keyEventArgs.Device);
						keyEventArgs.Handled = UnsafeStartComposition(_altNumpadcomposition);
					}
					else
					{
						_altNumpadcomposition.ClearTexts();
						keyEventArgs.Handled = UnsafeUpdateComposition(_altNumpadcomposition);
					}
					e.Cancel();
				}
				else if (_altNumpadcomposition != null)
				{
					_altNumpadcomposition.ClearTexts();
					_altNumpadcomposition.Complete();
					ClearAltnumpadComposition();
				}
			}
		}
		if (e.StagingItem.Input.RoutedEvent == Keyboard.PreviewKeyDownEvent)
		{
			KeyEventArgs keyEventArgs2 = (KeyEventArgs)e.StagingItem.Input;
			if (!keyEventArgs2.Handled && _deadCharTextComposition != null && _deadCharTextComposition.Stage == TextCompositionStage.Started)
			{
				keyEventArgs2.MarkDeadCharProcessed();
			}
		}
	}

	private void PostProcessInput(object sender, ProcessInputEventArgs e)
	{
		if (e.StagingItem.Input.RoutedEvent == Keyboard.KeyUpEvent)
		{
			KeyEventArgs keyEventArgs = (KeyEventArgs)e.StagingItem.Input;
			if (!keyEventArgs.Handled)
			{
				if ((keyEventArgs.RealKey == Key.LeftAlt || keyEventArgs.RealKey == Key.RightAlt) && (keyEventArgs.KeyboardDevice.Modifiers & ModifierKeys.Alt) == 0 && _altNumpadEntryMode)
				{
					_altNumpadEntryMode = false;
					if (_altNumpadEntry != 0)
					{
						_altNumpadcomposition.ClearTexts();
						if (_altNumpadConversionMode == AltNumpadConversionMode.OEMCodePage)
						{
							_altNumpadcomposition.SetText(GetCurrentOEMCPEncoding(_altNumpadEntry));
						}
						else if (_altNumpadConversionMode == AltNumpadConversionMode.DefaultCodePage || _altNumpadConversionMode == AltNumpadConversionMode.HexDefaultCodePage)
						{
							_altNumpadcomposition.SetText(CharacterEncoding(InputLanguageManager.Current.CurrentInputLanguage.TextInfo.ANSICodePage, _altNumpadEntry));
						}
						else if (_altNumpadConversionMode == AltNumpadConversionMode.HexUnicode)
						{
							char[] value = new char[1] { (char)_altNumpadEntry };
							_altNumpadcomposition.SetText(new string(value));
						}
					}
				}
			}
			else
			{
				_altNumpadEntryMode = false;
				_altNumpadEntry = 0;
				_altNumpadConversionMode = AltNumpadConversionMode.OEMCodePage;
			}
		}
		else if (e.StagingItem.Input.RoutedEvent == PreviewTextInputStartEvent)
		{
			TextCompositionEventArgs textCompositionEventArgs = (TextCompositionEventArgs)e.StagingItem.Input;
			if (!textCompositionEventArgs.Handled)
			{
				TextCompositionEventArgs textCompositionEventArgs2 = new TextCompositionEventArgs(textCompositionEventArgs.Device, textCompositionEventArgs.TextComposition);
				textCompositionEventArgs2.RoutedEvent = TextInputStartEvent;
				textCompositionEventArgs2.Source = textCompositionEventArgs.TextComposition.Source;
				e.PushInput(textCompositionEventArgs2, e.StagingItem);
			}
		}
		else if (e.StagingItem.Input.RoutedEvent == PreviewTextInputUpdateEvent)
		{
			TextCompositionEventArgs textCompositionEventArgs3 = (TextCompositionEventArgs)e.StagingItem.Input;
			if (!textCompositionEventArgs3.Handled)
			{
				TextCompositionEventArgs textCompositionEventArgs4 = new TextCompositionEventArgs(textCompositionEventArgs3.Device, textCompositionEventArgs3.TextComposition);
				textCompositionEventArgs4.RoutedEvent = TextInputUpdateEvent;
				textCompositionEventArgs4.Source = textCompositionEventArgs3.TextComposition.Source;
				e.PushInput(textCompositionEventArgs4, e.StagingItem);
			}
		}
		else if (e.StagingItem.Input.RoutedEvent == PreviewTextInputEvent)
		{
			TextCompositionEventArgs textCompositionEventArgs5 = (TextCompositionEventArgs)e.StagingItem.Input;
			if (!textCompositionEventArgs5.Handled)
			{
				TextCompositionEventArgs textCompositionEventArgs6 = new TextCompositionEventArgs(textCompositionEventArgs5.Device, textCompositionEventArgs5.TextComposition);
				textCompositionEventArgs6.RoutedEvent = TextInputEvent;
				textCompositionEventArgs6.Source = textCompositionEventArgs5.TextComposition.Source;
				e.PushInput(textCompositionEventArgs6, e.StagingItem);
			}
		}
		else if (e.StagingItem.Input.RoutedEvent == TextInputStartEvent)
		{
			TextCompositionEventArgs textCompositionEventArgs7 = (TextCompositionEventArgs)e.StagingItem.Input;
			if (!textCompositionEventArgs7.Handled && textCompositionEventArgs7.TextComposition.AutoComplete == TextCompositionAutoComplete.On)
			{
				textCompositionEventArgs7.Handled = UnsafeCompleteComposition(textCompositionEventArgs7.TextComposition);
			}
		}
		else if (e.StagingItem.Input.RoutedEvent == TextInputUpdateEvent)
		{
			TextCompositionEventArgs textCompositionEventArgs8 = (TextCompositionEventArgs)e.StagingItem.Input;
			if (!textCompositionEventArgs8.Handled && textCompositionEventArgs8.TextComposition == _deadCharTextComposition && _deadCharTextComposition.Composed)
			{
				DeadCharTextComposition deadCharTextComposition = _deadCharTextComposition;
				_deadCharTextComposition = null;
				textCompositionEventArgs8.Handled = UnsafeCompleteComposition(deadCharTextComposition);
			}
		}
		if (!(e.StagingItem.Input is InputReportEventArgs inputReportEventArgs) || inputReportEventArgs.Report.Type != InputType.Text || inputReportEventArgs.RoutedEvent != InputManager.InputReportEvent)
		{
			return;
		}
		RawTextInputReport rawTextInputReport = (RawTextInputReport)inputReportEventArgs.Report;
		string text = new string(rawTextInputReport.CharacterCode, 1);
		bool flag = false;
		if (_altNumpadcomposition != null)
		{
			if (text.Equals(_altNumpadcomposition.Text))
			{
				flag = true;
			}
			else
			{
				_altNumpadcomposition.ClearTexts();
			}
			_altNumpadcomposition.Complete();
			ClearAltnumpadComposition();
		}
		if (flag)
		{
			return;
		}
		if (rawTextInputReport.IsDeadCharacter)
		{
			_deadCharTextComposition = new DeadCharTextComposition(_inputManager, null, text, TextCompositionAutoComplete.Off, InputManager.Current.PrimaryKeyboardDevice);
			if (rawTextInputReport.IsSystemCharacter)
			{
				_deadCharTextComposition.MakeSystem();
			}
			else if (rawTextInputReport.IsControlCharacter)
			{
				_deadCharTextComposition.MakeControl();
			}
			inputReportEventArgs.Handled = UnsafeStartComposition(_deadCharTextComposition);
		}
		else if (_deadCharTextComposition != null)
		{
			inputReportEventArgs.Handled = CompleteDeadCharComposition(text, rawTextInputReport.IsSystemCharacter, rawTextInputReport.IsControlCharacter);
		}
		else
		{
			TextComposition textComposition = new TextComposition(_inputManager, (IInputElement)e.StagingItem.Input.Source, text, TextCompositionAutoComplete.On, InputManager.Current.PrimaryKeyboardDevice);
			if (rawTextInputReport.IsSystemCharacter)
			{
				textComposition.MakeSystem();
			}
			else if (rawTextInputReport.IsControlCharacter)
			{
				textComposition.MakeControl();
			}
			inputReportEventArgs.Handled = UnsafeStartComposition(textComposition);
		}
	}

	internal void CompleteDeadCharComposition()
	{
		CompleteDeadCharComposition(string.Empty, isSystemCharacter: false, isControlCharacter: false);
	}

	private bool CompleteDeadCharComposition(string inputText, bool isSystemCharacter, bool isControlCharacter)
	{
		if (_deadCharTextComposition != null)
		{
			_deadCharTextComposition.ClearTexts();
			_deadCharTextComposition.SetText(inputText);
			_deadCharTextComposition.Composed = true;
			if (isSystemCharacter)
			{
				_deadCharTextComposition.MakeSystem();
			}
			else if (isControlCharacter)
			{
				_deadCharTextComposition.MakeControl();
			}
			return UnsafeUpdateComposition(_deadCharTextComposition);
		}
		return false;
	}

	private bool EnterAltNumpadEntryMode(Key key)
	{
		bool result = false;
		if ((key == Key.LeftAlt || key == Key.RightAlt) && !_altNumpadEntryMode)
		{
			_altNumpadEntryMode = true;
			_altNumpadEntry = 0;
			_altNumpadConversionMode = AltNumpadConversionMode.OEMCodePage;
			result = true;
		}
		return result;
	}

	private bool HandleAltNumpadEntry(Key key, int scanCode, bool isExtendedKey)
	{
		bool result = false;
		if (isExtendedKey)
		{
			return result;
		}
		if (!Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt))
		{
			return false;
		}
		switch (scanCode)
		{
		case 83:
			if (IsHexNumpadEnabled)
			{
				_altNumpadEntry = 0;
				_altNumpadConversionMode = AltNumpadConversionMode.HexDefaultCodePage;
				return true;
			}
			_altNumpadEntry = 0;
			_altNumpadConversionMode = AltNumpadConversionMode.OEMCodePage;
			return false;
		case 78:
			if (IsHexNumpadEnabled)
			{
				_altNumpadEntry = 0;
				_altNumpadConversionMode = AltNumpadConversionMode.HexUnicode;
				return true;
			}
			_altNumpadEntry = 0;
			_altNumpadConversionMode = AltNumpadConversionMode.OEMCodePage;
			return false;
		default:
		{
			int newEntry = GetNewEntry(key, scanCode);
			if (newEntry == -1)
			{
				_altNumpadEntry = 0;
				_altNumpadConversionMode = AltNumpadConversionMode.OEMCodePage;
				return false;
			}
			if (_altNumpadEntry == 0 && newEntry == 0)
			{
				_altNumpadConversionMode = AltNumpadConversionMode.DefaultCodePage;
			}
			if (HexConversionMode)
			{
				_altNumpadEntry = _altNumpadEntry * 16 + newEntry;
			}
			else
			{
				_altNumpadEntry = _altNumpadEntry * 10 + newEntry;
			}
			return true;
		}
		}
	}

	private int GetNewEntry(Key key, int scanCode)
	{
		if (HexConversionMode)
		{
			switch (key)
			{
			case Key.D0:
				return 0;
			case Key.D1:
				return 1;
			case Key.D2:
				return 2;
			case Key.D3:
				return 3;
			case Key.D4:
				return 4;
			case Key.D5:
				return 5;
			case Key.D6:
				return 6;
			case Key.D7:
				return 7;
			case Key.D8:
				return 8;
			case Key.D9:
				return 9;
			case Key.A:
				return 10;
			case Key.B:
				return 11;
			case Key.C:
				return 12;
			case Key.D:
				return 13;
			case Key.E:
				return 14;
			case Key.F:
				return 15;
			}
		}
		return NumpadScanCode.DigitFromScanCode(scanCode);
	}

	private static byte[] ConvertCodeToByteArray(int codeEntry)
	{
		return (codeEntry > 255) ? new byte[2]
		{
			(byte)(codeEntry >> 8),
			(byte)codeEntry
		} : new byte[1] { (byte)codeEntry };
	}

	private void ClearAltnumpadComposition()
	{
		_altNumpadcomposition = null;
		_altNumpadConversionMode = AltNumpadConversionMode.OEMCodePage;
		_altNumpadEntry = 0;
	}
}
