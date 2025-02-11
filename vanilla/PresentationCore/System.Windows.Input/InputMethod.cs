using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace System.Windows.Input;

/// <summary>Provides facilities for managing and interacting with the Text Services Framework, which provides support for alternate text input methods such as speech and handwriting.</summary>
public class InputMethod : DispatcherObject
{
	/// <summary>Identifies the <see cref="P:System.Windows.Input.InputMethod.IsInputMethodEnabled" /> attached property.</summary>
	/// <returns>The identifier for the IsInputMethodEnabled attached property.</returns>
	public static readonly DependencyProperty IsInputMethodEnabledProperty = DependencyProperty.RegisterAttached("IsInputMethodEnabled", typeof(bool), typeof(InputMethod), new PropertyMetadata(true, IsInputMethodEnabled_Changed));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.InputMethod.IsInputMethodSuspended" /> attached property.</summary>
	/// <returns>The identifier for the IsInputMethodSuspended attached property.</returns>
	public static readonly DependencyProperty IsInputMethodSuspendedProperty = DependencyProperty.RegisterAttached("IsInputMethodSuspended", typeof(bool), typeof(InputMethod), new PropertyMetadata(false));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.InputMethod.PreferredImeState" /> attached property.</summary>
	/// <returns>The identifier for the PreferredImeState attached property.</returns>
	public static readonly DependencyProperty PreferredImeStateProperty = DependencyProperty.RegisterAttached("PreferredImeState", typeof(InputMethodState), typeof(InputMethod), new PropertyMetadata(InputMethodState.DoNotCare));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.InputMethod.PreferredImeConversionMode" /> attached property.</summary>
	/// <returns>The identifier for the PreferredImeConversionMode attached property.</returns>
	public static readonly DependencyProperty PreferredImeConversionModeProperty = DependencyProperty.RegisterAttached("PreferredImeConversionMode", typeof(ImeConversionModeValues), typeof(InputMethod), new PropertyMetadata(ImeConversionModeValues.DoNotCare));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.InputMethod.PreferredImeSentenceMode" /> attached property.</summary>
	/// <returns>The identifier for the PreferredImeSentenceMode attached property.</returns>
	public static readonly DependencyProperty PreferredImeSentenceModeProperty = DependencyProperty.RegisterAttached("PreferredImeSentenceMode", typeof(ImeSentenceModeValues), typeof(InputMethod), new PropertyMetadata(ImeSentenceModeValues.DoNotCare));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.InputMethod.InputScope" /> attached property.</summary>
	/// <returns>The identifier for the InputScope attached property.</returns>
	public static readonly DependencyProperty InputScopeProperty = DependencyProperty.RegisterAttached("InputScope", typeof(InputScope), typeof(InputMethod), new PropertyMetadata((object)null));

	private TextServicesCompartmentEventSink _sink;

	private TextServicesContext _textservicesContext;

	private TextServicesCompartmentContext _textservicesCompartmentContext;

	private InputLanguageManager _inputlanguagemanager;

	private DefaultTextStore _defaulttextstore;

	private static bool _immEnabled = SafeSystemMetrics.IsImmEnabled;

	[ThreadStatic]
	private static SecurityCriticalDataClass<nint> _defaultImc;

	/// <summary>Gets a reference to any currently active input method associated with the current context.</summary>
	/// <returns>A reference to an <see cref="T:System.Windows.Input.InputMethod" /> object associated with the current context, or null if there is no active input method.This property has no default value.</returns>
	public static InputMethod Current
	{
		get
		{
			InputMethod inputMethod = null;
			Dispatcher dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
			if (dispatcher != null)
			{
				inputMethod = dispatcher.InputMethod as InputMethod;
				if (inputMethod == null)
				{
					inputMethod = (InputMethod)(dispatcher.InputMethod = new InputMethod());
				}
			}
			return inputMethod;
		}
	}

	/// <summary>Gets or sets the current state of the input method editor associated with this input method.</summary>
	/// <returns>A member of the <see cref="T:System.Windows.Input.InputMethodState" /> enumeration specifying the state of the input method editor associated with this input method.The default value is <see cref="F:System.Windows.Input.InputMethodState.Off" />.</returns>
	public InputMethodState ImeState
	{
		get
		{
			if (!IsImm32ImeCurrent())
			{
				TextServicesCompartment compartment = TextServicesCompartmentContext.Current.GetCompartment(InputMethodStateType.ImeState);
				if (compartment != null)
				{
					if (!compartment.BooleanValue)
					{
						return InputMethodState.Off;
					}
					return InputMethodState.On;
				}
			}
			else
			{
				nint num = HwndFromInputElement(Keyboard.FocusedElement);
				if (num != IntPtr.Zero)
				{
					nint handle = MS.Win32.UnsafeNativeMethods.ImmGetContext(new HandleRef(this, num));
					bool num2 = MS.Win32.UnsafeNativeMethods.ImmGetOpenStatus(new HandleRef(this, handle));
					MS.Win32.UnsafeNativeMethods.ImmReleaseContext(new HandleRef(this, num), new HandleRef(this, handle));
					if (!num2)
					{
						return InputMethodState.Off;
					}
					return InputMethodState.On;
				}
			}
			return InputMethodState.Off;
		}
		set
		{
			TextServicesCompartment compartment = TextServicesCompartmentContext.Current.GetCompartment(InputMethodStateType.ImeState);
			if (compartment != null && compartment.BooleanValue != (value == InputMethodState.On))
			{
				compartment.BooleanValue = value == InputMethodState.On;
			}
			if (!_immEnabled)
			{
				return;
			}
			nint zero = IntPtr.Zero;
			zero = HwndFromInputElement(Keyboard.FocusedElement);
			if (zero != IntPtr.Zero)
			{
				nint handle = MS.Win32.UnsafeNativeMethods.ImmGetContext(new HandleRef(this, zero));
				if (MS.Win32.UnsafeNativeMethods.ImmGetOpenStatus(new HandleRef(this, handle)) != (value == InputMethodState.On))
				{
					MS.Win32.UnsafeNativeMethods.ImmSetOpenStatus(new HandleRef(this, handle), value == InputMethodState.On);
				}
				MS.Win32.UnsafeNativeMethods.ImmReleaseContext(new HandleRef(this, zero), new HandleRef(this, handle));
			}
		}
	}

	/// <summary>Gets or sets the current state of microphone input for this input method.</summary>
	/// <returns>A member of the <see cref="T:System.Windows.Input.InputMethodState" /> enumeration specifying the current input method state for microphone input.The default value is <see cref="F:System.Windows.Input.InputMethodState.Off" />.</returns>
	public InputMethodState MicrophoneState
	{
		get
		{
			TextServicesCompartment compartment = TextServicesCompartmentContext.Current.GetCompartment(InputMethodStateType.MicrophoneState);
			if (compartment != null)
			{
				if (!compartment.BooleanValue)
				{
					return InputMethodState.Off;
				}
				return InputMethodState.On;
			}
			return InputMethodState.Off;
		}
		set
		{
			TextServicesCompartment compartment = TextServicesCompartmentContext.Current.GetCompartment(InputMethodStateType.MicrophoneState);
			if (compartment != null && compartment.BooleanValue != (value == InputMethodState.On))
			{
				compartment.BooleanValue = value == InputMethodState.On;
			}
		}
	}

	/// <summary>Gets or sets the current state of handwriting input for this input method.</summary>
	/// <returns>A member of the <see cref="T:System.Windows.Input.InputMethodState" /> enumeration specifying the current input method state for handwriting input.The default value is <see cref="F:System.Windows.Input.InputMethodState.Off" />.</returns>
	public InputMethodState HandwritingState
	{
		get
		{
			TextServicesCompartment compartment = TextServicesCompartmentContext.Current.GetCompartment(InputMethodStateType.HandwritingState);
			if (compartment != null)
			{
				if (!compartment.BooleanValue)
				{
					return InputMethodState.Off;
				}
				return InputMethodState.On;
			}
			return InputMethodState.Off;
		}
		set
		{
			TextServicesCompartment compartment = TextServicesCompartmentContext.Current.GetCompartment(InputMethodStateType.HandwritingState);
			if (compartment != null && compartment.BooleanValue != (value == InputMethodState.On))
			{
				compartment.BooleanValue = value == InputMethodState.On;
			}
		}
	}

	/// <summary>Gets or sets the speech mode for this input method.</summary>
	/// <returns>A member of the <see cref="T:System.Windows.Input.SpeechMode" /> enumeration specifying the current speech mode.The default value is <see cref="F:System.Windows.Input.SpeechMode.Indeterminate" />.</returns>
	public SpeechMode SpeechMode
	{
		get
		{
			TextServicesCompartment compartment = TextServicesCompartmentContext.Current.GetCompartment(InputMethodStateType.SpeechMode);
			if (compartment != null)
			{
				int intValue = compartment.IntValue;
				if ((intValue & 1) != 0)
				{
					return SpeechMode.Dictation;
				}
				if ((intValue & 8) != 0)
				{
					return SpeechMode.Command;
				}
			}
			return SpeechMode.Indeterminate;
		}
		set
		{
			TextServicesCompartment compartment = TextServicesCompartmentContext.Current.GetCompartment(InputMethodStateType.SpeechMode);
			if (compartment == null)
			{
				return;
			}
			int intValue = compartment.IntValue;
			switch (value)
			{
			case SpeechMode.Dictation:
				intValue &= -9;
				intValue |= 1;
				if (compartment.IntValue != intValue)
				{
					compartment.IntValue = intValue;
				}
				break;
			case SpeechMode.Command:
				intValue &= -2;
				intValue |= 8;
				if (compartment.IntValue != intValue)
				{
					compartment.IntValue = intValue;
				}
				break;
			}
		}
	}

	/// <summary>Gets or sets the current conversion mode for the input method editor associated with this input method.</summary>
	/// <returns>A member of the <see cref="T:System.Windows.Input.ImeConversionModeValues" /> enumeration specifying the conversion mode.The default value is <see cref="F:System.Windows.Input.ImeConversionModeValues.Alphanumeric" />.</returns>
	public ImeConversionModeValues ImeConversionMode
	{
		get
		{
			if (!IsImm32ImeCurrent())
			{
				TextServicesCompartment compartment = TextServicesCompartmentContext.Current.GetCompartment(InputMethodStateType.ImeConversionModeValues);
				if (compartment != null)
				{
					int intValue = compartment.IntValue;
					ImeConversionModeValues imeConversionModeValues = (ImeConversionModeValues)0;
					if ((intValue & 3) == 0)
					{
						imeConversionModeValues |= ImeConversionModeValues.Alphanumeric;
					}
					if ((intValue & 1) != 0)
					{
						imeConversionModeValues |= ImeConversionModeValues.Native;
					}
					if ((intValue & 2) != 0)
					{
						imeConversionModeValues |= ImeConversionModeValues.Katakana;
					}
					if ((intValue & 8) != 0)
					{
						imeConversionModeValues |= ImeConversionModeValues.FullShape;
					}
					if ((intValue & 0x10) != 0)
					{
						imeConversionModeValues |= ImeConversionModeValues.Roman;
					}
					if ((intValue & 0x20) != 0)
					{
						imeConversionModeValues |= ImeConversionModeValues.CharCode;
					}
					if ((intValue & 0x100) != 0)
					{
						imeConversionModeValues |= ImeConversionModeValues.NoConversion;
					}
					if ((intValue & 0x200) != 0)
					{
						imeConversionModeValues |= ImeConversionModeValues.Eudc;
					}
					if ((intValue & 0x400) != 0)
					{
						imeConversionModeValues |= ImeConversionModeValues.Symbol;
					}
					if ((intValue & 0x800) != 0)
					{
						imeConversionModeValues |= ImeConversionModeValues.Fixed;
					}
					return imeConversionModeValues;
				}
			}
			else
			{
				nint num = HwndFromInputElement(Keyboard.FocusedElement);
				if (num != IntPtr.Zero)
				{
					int conversion = 0;
					int sentence = 0;
					nint handle = MS.Win32.UnsafeNativeMethods.ImmGetContext(new HandleRef(this, num));
					MS.Win32.UnsafeNativeMethods.ImmGetConversionStatus(new HandleRef(this, handle), ref conversion, ref sentence);
					MS.Win32.UnsafeNativeMethods.ImmReleaseContext(new HandleRef(this, num), new HandleRef(this, handle));
					ImeConversionModeValues imeConversionModeValues2 = (ImeConversionModeValues)0;
					if ((conversion & 3) == 0)
					{
						imeConversionModeValues2 |= ImeConversionModeValues.Alphanumeric;
					}
					if ((conversion & 1) != 0)
					{
						imeConversionModeValues2 |= ImeConversionModeValues.Native;
					}
					if ((conversion & 2) != 0)
					{
						imeConversionModeValues2 |= ImeConversionModeValues.Katakana;
					}
					if ((conversion & 8) != 0)
					{
						imeConversionModeValues2 |= ImeConversionModeValues.FullShape;
					}
					if ((conversion & 0x10) != 0)
					{
						imeConversionModeValues2 |= ImeConversionModeValues.Roman;
					}
					if ((conversion & 0x20) != 0)
					{
						imeConversionModeValues2 |= ImeConversionModeValues.CharCode;
					}
					if ((conversion & 0x100) != 0)
					{
						imeConversionModeValues2 |= ImeConversionModeValues.NoConversion;
					}
					if ((conversion & 0x200) != 0)
					{
						imeConversionModeValues2 |= ImeConversionModeValues.Eudc;
					}
					if ((conversion & 0x400) != 0)
					{
						imeConversionModeValues2 |= ImeConversionModeValues.Symbol;
					}
					if ((conversion & 0x800) != 0)
					{
						imeConversionModeValues2 |= ImeConversionModeValues.Fixed;
					}
					return imeConversionModeValues2;
				}
			}
			return ImeConversionModeValues.Alphanumeric;
		}
		set
		{
			if (!IsValidConversionMode(value))
			{
				throw new ArgumentException(SR.Format(SR.InputMethod_InvalidConversionMode, value));
			}
			nint num = IntPtr.Zero;
			if (_immEnabled)
			{
				num = HwndFromInputElement(Keyboard.FocusedElement);
			}
			TextServicesCompartment compartment = TextServicesCompartmentContext.Current.GetCompartment(InputMethodStateType.ImeConversionModeValues);
			if (compartment != null)
			{
				MS.Win32.UnsafeNativeMethods.ConversionModeFlags conversionModeFlags = ((!_immEnabled) ? ((MS.Win32.UnsafeNativeMethods.ConversionModeFlags)compartment.IntValue) : Imm32ConversionModeToTSFConversionMode(num));
				MS.Win32.UnsafeNativeMethods.ConversionModeFlags conversionModeFlags2 = MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_ALPHANUMERIC;
				if ((value & ImeConversionModeValues.Native) != 0)
				{
					conversionModeFlags2 |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_NATIVE;
				}
				if ((value & ImeConversionModeValues.Katakana) != 0)
				{
					conversionModeFlags2 |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_KATAKANA;
				}
				if ((value & ImeConversionModeValues.FullShape) != 0)
				{
					conversionModeFlags2 |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_FULLSHAPE;
				}
				if ((value & ImeConversionModeValues.Roman) != 0)
				{
					conversionModeFlags2 |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_ROMAN;
				}
				if ((value & ImeConversionModeValues.CharCode) != 0)
				{
					conversionModeFlags2 |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_CHARCODE;
				}
				if ((value & ImeConversionModeValues.NoConversion) != 0)
				{
					conversionModeFlags2 |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_NOCONVERSION;
				}
				if ((value & ImeConversionModeValues.Eudc) != 0)
				{
					conversionModeFlags2 |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_EUDC;
				}
				if ((value & ImeConversionModeValues.Symbol) != 0)
				{
					conversionModeFlags2 |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_SYMBOL;
				}
				if ((value & ImeConversionModeValues.Fixed) != 0)
				{
					conversionModeFlags2 |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_FIXED;
				}
				if (conversionModeFlags != conversionModeFlags2)
				{
					MS.Win32.UnsafeNativeMethods.ConversionModeFlags conversionModeFlags3 = MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_ALPHANUMERIC;
					switch (conversionModeFlags2)
					{
					case MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_NATIVE | MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_FULLSHAPE:
						conversionModeFlags3 = MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_KATAKANA;
						break;
					case MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_NATIVE | MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_KATAKANA:
						conversionModeFlags3 = MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_FULLSHAPE;
						break;
					case MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_FULLSHAPE:
						conversionModeFlags3 = MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_NATIVE | MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_KATAKANA;
						break;
					case MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_ALPHANUMERIC:
						conversionModeFlags3 = MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_NATIVE | MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_KATAKANA | MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_FULLSHAPE;
						break;
					case MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_NATIVE:
						conversionModeFlags3 = MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_FULLSHAPE;
						break;
					}
					conversionModeFlags2 |= conversionModeFlags;
					conversionModeFlags2 &= ~conversionModeFlags3;
					compartment.IntValue = (int)conversionModeFlags2;
				}
			}
			if (!_immEnabled || num == IntPtr.Zero)
			{
				return;
			}
			int conversion = 0;
			int sentence = 0;
			nint handle = MS.Win32.UnsafeNativeMethods.ImmGetContext(new HandleRef(this, num));
			MS.Win32.UnsafeNativeMethods.ImmGetConversionStatus(new HandleRef(this, handle), ref conversion, ref sentence);
			int num2 = 0;
			if ((value & ImeConversionModeValues.Native) != 0)
			{
				num2 |= 1;
			}
			if ((value & ImeConversionModeValues.Katakana) != 0)
			{
				num2 |= 2;
			}
			if ((value & ImeConversionModeValues.FullShape) != 0)
			{
				num2 |= 8;
			}
			if ((value & ImeConversionModeValues.Roman) != 0)
			{
				num2 |= 0x10;
			}
			if ((value & ImeConversionModeValues.CharCode) != 0)
			{
				num2 |= 0x20;
			}
			if ((value & ImeConversionModeValues.NoConversion) != 0)
			{
				num2 |= 0x100;
			}
			if ((value & ImeConversionModeValues.Eudc) != 0)
			{
				num2 |= 0x200;
			}
			if ((value & ImeConversionModeValues.Symbol) != 0)
			{
				num2 |= 0x400;
			}
			if ((value & ImeConversionModeValues.Fixed) != 0)
			{
				num2 |= 0x800;
			}
			if (conversion != num2)
			{
				int num3 = 0;
				switch (num2)
				{
				case 9:
					num3 = 2;
					break;
				case 3:
					num3 = 8;
					break;
				case 8:
					num3 = 3;
					break;
				case 0:
					num3 = 11;
					break;
				case 1:
					num3 = 8;
					break;
				}
				num2 |= conversion;
				num2 &= ~num3;
				MS.Win32.UnsafeNativeMethods.ImmSetConversionStatus(new HandleRef(this, handle), num2, sentence);
			}
			MS.Win32.UnsafeNativeMethods.ImmReleaseContext(new HandleRef(this, num), new HandleRef(this, handle));
		}
	}

	/// <summary>Gets or sets the current sentence mode for the input method editor associated with this input method.</summary>
	/// <returns>A member of the <see cref="T:System.Windows.Input.ImeSentenceModeValues" /> enumerations specifying the sentence mode.The default value is <see cref="F:System.Windows.Input.ImeSentenceModeValues.None" />.</returns>
	public ImeSentenceModeValues ImeSentenceMode
	{
		get
		{
			if (!IsImm32ImeCurrent())
			{
				TextServicesCompartment compartment = TextServicesCompartmentContext.Current.GetCompartment(InputMethodStateType.ImeSentenceModeValues);
				if (compartment != null)
				{
					MS.Win32.UnsafeNativeMethods.SentenceModeFlags intValue = (MS.Win32.UnsafeNativeMethods.SentenceModeFlags)compartment.IntValue;
					ImeSentenceModeValues imeSentenceModeValues = ImeSentenceModeValues.None;
					if (intValue == MS.Win32.UnsafeNativeMethods.SentenceModeFlags.TF_SENTENCEMODE_NONE)
					{
						return ImeSentenceModeValues.None;
					}
					if ((intValue & MS.Win32.UnsafeNativeMethods.SentenceModeFlags.TF_SENTENCEMODE_PLAURALCLAUSE) != 0)
					{
						imeSentenceModeValues |= ImeSentenceModeValues.PluralClause;
					}
					if ((intValue & MS.Win32.UnsafeNativeMethods.SentenceModeFlags.TF_SENTENCEMODE_SINGLECONVERT) != 0)
					{
						imeSentenceModeValues |= ImeSentenceModeValues.SingleConversion;
					}
					if ((intValue & MS.Win32.UnsafeNativeMethods.SentenceModeFlags.TF_SENTENCEMODE_AUTOMATIC) != 0)
					{
						imeSentenceModeValues |= ImeSentenceModeValues.Automatic;
					}
					if ((intValue & MS.Win32.UnsafeNativeMethods.SentenceModeFlags.TF_SENTENCEMODE_PHRASEPREDICT) != 0)
					{
						imeSentenceModeValues |= ImeSentenceModeValues.PhrasePrediction;
					}
					if ((intValue & MS.Win32.UnsafeNativeMethods.SentenceModeFlags.TF_SENTENCEMODE_CONVERSATION) != 0)
					{
						imeSentenceModeValues |= ImeSentenceModeValues.Conversation;
					}
					return imeSentenceModeValues;
				}
			}
			else
			{
				nint num = HwndFromInputElement(Keyboard.FocusedElement);
				if (num != IntPtr.Zero)
				{
					ImeSentenceModeValues imeSentenceModeValues2 = ImeSentenceModeValues.None;
					int conversion = 0;
					int sentence = 0;
					nint handle = MS.Win32.UnsafeNativeMethods.ImmGetContext(new HandleRef(this, num));
					MS.Win32.UnsafeNativeMethods.ImmGetConversionStatus(new HandleRef(this, handle), ref conversion, ref sentence);
					MS.Win32.UnsafeNativeMethods.ImmReleaseContext(new HandleRef(this, num), new HandleRef(this, handle));
					if (sentence == 0)
					{
						return ImeSentenceModeValues.None;
					}
					if ((sentence & 1) != 0)
					{
						imeSentenceModeValues2 |= ImeSentenceModeValues.PluralClause;
					}
					if ((sentence & 2) != 0)
					{
						imeSentenceModeValues2 |= ImeSentenceModeValues.SingleConversion;
					}
					if ((sentence & 4) != 0)
					{
						imeSentenceModeValues2 |= ImeSentenceModeValues.Automatic;
					}
					if ((sentence & 8) != 0)
					{
						imeSentenceModeValues2 |= ImeSentenceModeValues.PhrasePrediction;
					}
					if ((sentence & 0x10) != 0)
					{
						imeSentenceModeValues2 |= ImeSentenceModeValues.Conversation;
					}
					return imeSentenceModeValues2;
				}
			}
			return ImeSentenceModeValues.None;
		}
		set
		{
			if (!IsValidSentenceMode(value))
			{
				throw new ArgumentException(SR.Format(SR.InputMethod_InvalidSentenceMode, value));
			}
			TextServicesCompartment compartment = TextServicesCompartmentContext.Current.GetCompartment(InputMethodStateType.ImeSentenceModeValues);
			if (compartment != null)
			{
				MS.Win32.UnsafeNativeMethods.SentenceModeFlags sentenceModeFlags = MS.Win32.UnsafeNativeMethods.SentenceModeFlags.TF_SENTENCEMODE_NONE;
				if ((value & ImeSentenceModeValues.PluralClause) != 0)
				{
					sentenceModeFlags |= MS.Win32.UnsafeNativeMethods.SentenceModeFlags.TF_SENTENCEMODE_PLAURALCLAUSE;
				}
				if ((value & ImeSentenceModeValues.SingleConversion) != 0)
				{
					sentenceModeFlags |= MS.Win32.UnsafeNativeMethods.SentenceModeFlags.TF_SENTENCEMODE_SINGLECONVERT;
				}
				if ((value & ImeSentenceModeValues.Automatic) != 0)
				{
					sentenceModeFlags |= MS.Win32.UnsafeNativeMethods.SentenceModeFlags.TF_SENTENCEMODE_AUTOMATIC;
				}
				if ((value & ImeSentenceModeValues.PhrasePrediction) != 0)
				{
					sentenceModeFlags |= MS.Win32.UnsafeNativeMethods.SentenceModeFlags.TF_SENTENCEMODE_PHRASEPREDICT;
				}
				if ((value & ImeSentenceModeValues.Conversation) != 0)
				{
					sentenceModeFlags |= MS.Win32.UnsafeNativeMethods.SentenceModeFlags.TF_SENTENCEMODE_CONVERSATION;
				}
				if (compartment.IntValue != (int)sentenceModeFlags)
				{
					compartment.IntValue = (int)sentenceModeFlags;
				}
			}
			if (!_immEnabled)
			{
				return;
			}
			nint num = HwndFromInputElement(Keyboard.FocusedElement);
			if (num != IntPtr.Zero)
			{
				int conversion = 0;
				int sentence = 0;
				nint handle = MS.Win32.UnsafeNativeMethods.ImmGetContext(new HandleRef(this, num));
				MS.Win32.UnsafeNativeMethods.ImmGetConversionStatus(new HandleRef(this, handle), ref conversion, ref sentence);
				int num2 = 0;
				if ((value & ImeSentenceModeValues.PluralClause) != 0)
				{
					num2 |= 1;
				}
				if ((value & ImeSentenceModeValues.SingleConversion) != 0)
				{
					num2 |= 2;
				}
				if ((value & ImeSentenceModeValues.Automatic) != 0)
				{
					num2 |= 4;
				}
				if ((value & ImeSentenceModeValues.PhrasePrediction) != 0)
				{
					num2 |= 8;
				}
				if ((value & ImeSentenceModeValues.Conversation) != 0)
				{
					num2 |= 0x10;
				}
				if (sentence != num2)
				{
					MS.Win32.UnsafeNativeMethods.ImmSetConversionStatus(new HandleRef(this, handle), conversion, num2);
				}
				MS.Win32.UnsafeNativeMethods.ImmReleaseContext(new HandleRef(this, num), new HandleRef(this, handle));
			}
		}
	}

	/// <summary>Gets a value that indicates whether or not this input method can display configuration user interface (UI).</summary>
	/// <returns>true if configuration UI can be displayed; otherwise, false.This property has no default value.</returns>
	public bool CanShowConfigurationUI => _ShowConfigureUI(null, fShow: false);

	/// <summary>Gets a value that indicates whether this input method can display word registration user interface (UI). </summary>
	/// <returns>true if word registration UI can be displayed; otherwise, false.This property has no default value.</returns>
	public bool CanShowRegisterWordUI => _ShowRegisterWordUI(null, fShow: false, "");

	internal TextServicesContext TextServicesContext
	{
		get
		{
			return _textservicesContext;
		}
		set
		{
			_textservicesContext = value;
		}
	}

	internal TextServicesCompartmentContext TextServicesCompartmentContext
	{
		get
		{
			return _textservicesCompartmentContext;
		}
		set
		{
			_textservicesCompartmentContext = value;
		}
	}

	internal InputLanguageManager InputLanguageManager
	{
		get
		{
			return _inputlanguagemanager;
		}
		set
		{
			_inputlanguagemanager = value;
		}
	}

	internal DefaultTextStore DefaultTextStore
	{
		get
		{
			return _defaulttextstore;
		}
		set
		{
			_defaulttextstore = value;
		}
	}

	private nint DefaultImc
	{
		get
		{
			if (_defaultImc == null)
			{
				nint handle = MS.Win32.UnsafeNativeMethods.ImmGetDefaultIMEWnd(new HandleRef(this, IntPtr.Zero));
				nint num = MS.Win32.UnsafeNativeMethods.ImmGetContext(new HandleRef(this, handle));
				_defaultImc = new SecurityCriticalDataClass<nint>(num);
				MS.Win32.UnsafeNativeMethods.ImmReleaseContext(new HandleRef(this, handle), new HandleRef(this, num));
			}
			return _defaultImc.Value;
		}
	}

	/// <summary>Occurs when the input method state (represented by the <see cref="P:System.Windows.Input.InputMethod.ImeState" /> property) changes.</summary>
	public event InputMethodStateChangedEventHandler StateChanged
	{
		add
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (this._StateChanged == null && TextServicesLoader.ServicesInstalled)
			{
				InitializeCompartmentEventSink();
			}
			_StateChanged += value;
		}
		remove
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_StateChanged -= value;
			if (this._StateChanged == null && TextServicesLoader.ServicesInstalled)
			{
				UninitializeCompartmentEventSink();
			}
		}
	}

	private event InputMethodStateChangedEventHandler _StateChanged;

	internal InputMethod()
	{
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.InputMethod.IsInputMethodEnabled" /> attached property on the specified dependency object.</summary>
	/// <param name="target">The dependency object on which to set the <see cref="P:System.Windows.Input.InputMethod.IsInputMethodEnabled" /> attached property.</param>
	/// <param name="value">The new value for the <see cref="P:System.Windows.Input.InputMethod.IsInputMethodEnabled" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	public static void SetIsInputMethodEnabled(DependencyObject target, bool value)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(IsInputMethodEnabledProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Input.InputMethod.IsInputMethodEnabled" />  attached property for a specified dependency object.</summary>
	/// <returns>The current value of <see cref="P:System.Windows.Input.InputMethod.IsInputMethodEnabled" /> for the specified dependency object.</returns>
	/// <param name="target">The dependency object for which to retrieve the value of <see cref="P:System.Windows.Input.InputMethod.IsInputMethodEnabled" />.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetIsInputMethodEnabled(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (bool)target.GetValue(IsInputMethodEnabledProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.InputMethod.IsInputMethodSuspended" /> attached property on the specified dependency object.</summary>
	/// <param name="target">The dependency object on which to set the <see cref="P:System.Windows.Input.InputMethod.IsInputMethodSuspended" /> attached property.</param>
	/// <param name="value">The new value for the <see cref="P:System.Windows.Input.InputMethod.IsInputMethodSuspended" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	public static void SetIsInputMethodSuspended(DependencyObject target, bool value)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(IsInputMethodSuspendedProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Input.InputMethod.IsInputMethodSuspended" />  attached property for a specified dependency object.</summary>
	/// <returns>The current value of <see cref="P:System.Windows.Input.InputMethod.IsInputMethodSuspended" /> for the specified dependency object.</returns>
	/// <param name="target">The dependency object for which to retrieve the value of <see cref="P:System.Windows.Input.InputMethod.IsInputMethodSuspended" />.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetIsInputMethodSuspended(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (bool)target.GetValue(IsInputMethodSuspendedProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.InputMethod.PreferredImeState" /> attached property on the specified dependency object.</summary>
	/// <param name="target">The dependency object on which to set the <see cref="P:System.Windows.Input.InputMethod.PreferredImeState" /> attached property.</param>
	/// <param name="value">A member of the <see cref="T:System.Windows.Input.ImeConversionModeValues" /> enumeration representing the new value for the <see cref="P:System.Windows.Input.InputMethod.PreferredImeState" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	public static void SetPreferredImeState(DependencyObject target, InputMethodState value)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(PreferredImeStateProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Input.InputMethod.PreferredImeState" />  attached property for a specified dependency object.</summary>
	/// <returns>A member of the <see cref="T:System.Windows.Input.InputMethodState" /> enumeration specifying the current <see cref="P:System.Windows.Input.InputMethod.PreferredImeState" /> for the specified dependency object.</returns>
	/// <param name="target">The dependency object for which to retrieve the value of <see cref="P:System.Windows.Input.InputMethod.PreferredImeState" />.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static InputMethodState GetPreferredImeState(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (InputMethodState)target.GetValue(PreferredImeStateProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.InputMethod.PreferredImeConversionMode" /> attached property on the specified dependency object.</summary>
	/// <param name="target">The dependency object on which to set the <see cref="P:System.Windows.Input.InputMethod.PreferredImeConversionMode" /> attached property.</param>
	/// <param name="value">A member of the <see cref="T:System.Windows.Input.ImeConversionModeValues" /> enumeration representing the new value for the <see cref="P:System.Windows.Input.InputMethod.PreferredImeConversionMode" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	public static void SetPreferredImeConversionMode(DependencyObject target, ImeConversionModeValues value)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(PreferredImeConversionModeProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Input.InputMethod.PreferredImeConversionMode" />  attached property for a specified dependency object.</summary>
	/// <returns>A member of the <see cref="T:System.Windows.Input.ImeConversionModeValues" /> enumeration specifying the current <see cref="P:System.Windows.Input.InputMethod.PreferredImeConversionMode" /> for the specified dependency object.</returns>
	/// <param name="target">The dependency object for which to retrieve the value of <see cref="P:System.Windows.Input.InputMethod.PreferredImeConversionMode" />.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static ImeConversionModeValues GetPreferredImeConversionMode(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (ImeConversionModeValues)target.GetValue(PreferredImeConversionModeProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.InputMethod.PreferredImeSentenceMode" /> attached property on the specified dependency object.</summary>
	/// <param name="target">The dependency object on which to set the <see cref="P:System.Windows.Input.InputMethod.PreferredImeSentenceMode" /> attached property.</param>
	/// <param name="value">A member of the <see cref="T:System.Windows.Input.ImeConversionModeValues" /> enumeration representing the new value for the <see cref="P:System.Windows.Input.InputMethod.PreferredImeSentenceMode" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	public static void SetPreferredImeSentenceMode(DependencyObject target, ImeSentenceModeValues value)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(PreferredImeSentenceModeProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Input.InputMethod.PreferredImeSentenceMode" />  attached property for a specified dependency object.</summary>
	/// <returns>A member of the <see cref="T:System.Windows.Input.ImeSentenceModeValues" /> enumeration specifying the current <see cref="P:System.Windows.Input.InputMethod.PreferredImeSentenceMode" /> for the specified dependency object.</returns>
	/// <param name="target">The dependency object for which to retrieve the value of <see cref="P:System.Windows.Input.InputMethod.PreferredImeSentenceMode" />.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static ImeSentenceModeValues GetPreferredImeSentenceMode(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (ImeSentenceModeValues)target.GetValue(PreferredImeSentenceModeProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.InputMethod.InputScope" /> attached property on the specified dependency object.</summary>
	/// <param name="target">The dependency object on which to set the <see cref="P:System.Windows.Input.InputMethod.InputScope" /> attached property.</param>
	/// <param name="value">An <see cref="T:System.Windows.Input.InputScope" /> object representing the new value for the <see cref="P:System.Windows.Input.InputMethod.InputScope" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	public static void SetInputScope(DependencyObject target, InputScope value)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(InputScopeProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Input.InputMethod.InputScope" />  attached property for a specified dependency object.</summary>
	/// <returns>An <see cref="T:System.Windows.Input.InputScope" /> object representing the current input scope for the specified dependency object.</returns>
	/// <param name="target">The dependency object for which to retrieve the input scope.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static InputScope GetInputScope(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (InputScope)target.GetValue(InputScopeProperty);
	}

	/// <summary>Displays configuration user interface (UI) associated with the currently active keyboard text service.</summary>
	public void ShowConfigureUI()
	{
		ShowConfigureUI(null);
	}

	/// <summary>Displays configuration user interface (UI) associated with the currently active keyboard text service, using a specified <see cref="T:System.Windows.UIElement" /> as the parent element for the configuration UI.</summary>
	/// <param name="element">A <see cref="T:System.Windows.UIElement" /> that will be used as the parent element for the configuration UI.  This parameter can be null.</param>
	public void ShowConfigureUI(UIElement element)
	{
		_ShowConfigureUI(element, fShow: true);
	}

	/// <summary>Displays word registration user interface (UI) associated with the currently active keyboard text service.</summary>
	public void ShowRegisterWordUI()
	{
		ShowRegisterWordUI("");
	}

	/// <summary>Displays word registration user interface (UI) associated with the currently active keyboard text service.  Accepts a specified string as the default value to register.</summary>
	/// <param name="registeredText">A string that specifies a value to register.</param>
	public void ShowRegisterWordUI(string registeredText)
	{
		ShowRegisterWordUI(null, registeredText);
	}

	/// <summary>Displays word registration user interface (UI) associated with the currently active keyboard text service.  Accepts a specified string as the default value to register, and a specified <see cref="T:System.Windows.UIElement" /> as the parent element for the configuration UI.</summary>
	/// <param name="element">A <see cref="T:System.Windows.UIElement" /> that will be used as the parent element for the word registration UI.  This parameter can be null.</param>
	/// <param name="registeredText">A string that specifies a value to register.</param>
	public void ShowRegisterWordUI(UIElement element, string registeredText)
	{
		_ShowRegisterWordUI(element, fShow: true, registeredText);
	}

	internal void GotKeyboardFocus(DependencyObject focus)
	{
		if (focus != null)
		{
			object value = focus.GetValue(PreferredImeStateProperty);
			if (value != null && (InputMethodState)value != InputMethodState.DoNotCare)
			{
				ImeState = (InputMethodState)value;
			}
			value = focus.GetValue(PreferredImeConversionModeProperty);
			if (value != null && ((ImeConversionModeValues)value & ImeConversionModeValues.DoNotCare) == 0)
			{
				ImeConversionMode = (ImeConversionModeValues)value;
			}
			value = focus.GetValue(PreferredImeSentenceModeProperty);
			if (value != null && ((ImeSentenceModeValues)value & ImeSentenceModeValues.DoNotCare) == 0)
			{
				ImeSentenceMode = (ImeSentenceModeValues)value;
			}
		}
	}

	internal void OnChange(ref Guid rguid)
	{
		if (this._StateChanged != null)
		{
			InputMethodStateType statetype = InputMethodEventTypeInfo.ToType(ref rguid);
			this._StateChanged(this, new InputMethodStateChangedEventArgs(statetype));
		}
	}

	internal static bool IsImm32ImeCurrent()
	{
		if (!_immEnabled)
		{
			return false;
		}
		return IsImm32Ime(SafeNativeMethods.GetKeyboardLayout(0));
	}

	internal static bool IsImm32Ime(nint hkl)
	{
		if (hkl == IntPtr.Zero)
		{
			return false;
		}
		return (MS.Win32.NativeMethods.IntPtrToInt32(hkl) & 0xF0000000u) == 3758096384u;
	}

	private static void IsInputMethodEnabled_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if ((IInputElement)d == Keyboard.FocusedElement)
		{
			Current.EnableOrDisableInputMethod((bool)e.NewValue);
		}
	}

	internal void EnableOrDisableInputMethod(bool bEnabled)
	{
		if (TextServicesLoader.ServicesInstalled && TextServicesContext.DispatcherCurrent != null)
		{
			if (bEnabled)
			{
				TextServicesContext.DispatcherCurrent.SetFocusOnDefaultTextStore();
			}
			else
			{
				TextServicesContext.DispatcherCurrent.SetFocusOnEmptyDim();
			}
		}
		if (!_immEnabled)
		{
			return;
		}
		nint handle = HwndFromInputElement(Keyboard.FocusedElement);
		if (bEnabled)
		{
			if (DefaultImc != IntPtr.Zero)
			{
				MS.Win32.UnsafeNativeMethods.ImmAssociateContext(new HandleRef(this, handle), new HandleRef(this, _defaultImc.Value));
			}
		}
		else
		{
			MS.Win32.UnsafeNativeMethods.ImmAssociateContext(new HandleRef(this, handle), new HandleRef(this, IntPtr.Zero));
		}
	}

	private MS.Win32.UnsafeNativeMethods.ConversionModeFlags Imm32ConversionModeToTSFConversionMode(nint hwnd)
	{
		MS.Win32.UnsafeNativeMethods.ConversionModeFlags conversionModeFlags = MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_ALPHANUMERIC;
		if (hwnd != IntPtr.Zero)
		{
			int conversion = 0;
			int sentence = 0;
			nint handle = MS.Win32.UnsafeNativeMethods.ImmGetContext(new HandleRef(this, hwnd));
			MS.Win32.UnsafeNativeMethods.ImmGetConversionStatus(new HandleRef(this, handle), ref conversion, ref sentence);
			MS.Win32.UnsafeNativeMethods.ImmReleaseContext(new HandleRef(this, hwnd), new HandleRef(this, handle));
			if ((conversion & 1) != 0)
			{
				conversionModeFlags |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_NATIVE;
			}
			if ((conversion & 2) != 0)
			{
				conversionModeFlags |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_KATAKANA;
			}
			if ((conversion & 8) != 0)
			{
				conversionModeFlags |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_FULLSHAPE;
			}
			if ((conversion & 0x10) != 0)
			{
				conversionModeFlags |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_ROMAN;
			}
			if ((conversion & 0x20) != 0)
			{
				conversionModeFlags |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_CHARCODE;
			}
			if ((conversion & 0x100) != 0)
			{
				conversionModeFlags |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_NOCONVERSION;
			}
			if ((conversion & 0x200) != 0)
			{
				conversionModeFlags |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_EUDC;
			}
			if ((conversion & 0x400) != 0)
			{
				conversionModeFlags |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_SYMBOL;
			}
			if ((conversion & 0x800) != 0)
			{
				conversionModeFlags |= MS.Win32.UnsafeNativeMethods.ConversionModeFlags.TF_CONVERSIONMODE_FIXED;
			}
		}
		return conversionModeFlags;
	}

	private void InitializeCompartmentEventSink()
	{
		for (int i = 0; i < InputMethodEventTypeInfo.InfoList.Length; i++)
		{
			InputMethodEventTypeInfo inputMethodEventTypeInfo = InputMethodEventTypeInfo.InfoList[i];
			TextServicesCompartment textServicesCompartment = null;
			if (inputMethodEventTypeInfo.Scope == CompartmentScope.Thread)
			{
				textServicesCompartment = TextServicesCompartmentContext.Current.GetThreadCompartment(inputMethodEventTypeInfo.Guid);
			}
			else if (inputMethodEventTypeInfo.Scope == CompartmentScope.Global)
			{
				textServicesCompartment = TextServicesCompartmentContext.Current.GetGlobalCompartment(inputMethodEventTypeInfo.Guid);
			}
			if (textServicesCompartment != null)
			{
				if (_sink == null)
				{
					_sink = new TextServicesCompartmentEventSink(this);
				}
				textServicesCompartment.AdviseNotifySink(_sink);
			}
		}
	}

	private void UninitializeCompartmentEventSink()
	{
		for (int i = 0; i < InputMethodEventTypeInfo.InfoList.Length; i++)
		{
			InputMethodEventTypeInfo inputMethodEventTypeInfo = InputMethodEventTypeInfo.InfoList[i];
			TextServicesCompartment textServicesCompartment = null;
			if (inputMethodEventTypeInfo.Scope == CompartmentScope.Thread)
			{
				textServicesCompartment = TextServicesCompartmentContext.Current.GetThreadCompartment(inputMethodEventTypeInfo.Guid);
			}
			else if (inputMethodEventTypeInfo.Scope == CompartmentScope.Global)
			{
				textServicesCompartment = TextServicesCompartmentContext.Current.GetGlobalCompartment(inputMethodEventTypeInfo.Guid);
			}
			textServicesCompartment?.UnadviseNotifySink();
		}
	}

	private bool _ShowConfigureUI(UIElement element, bool fShow)
	{
		bool result = false;
		nint keyboardLayout = SafeNativeMethods.GetKeyboardLayout(0);
		if (!IsImm32Ime(keyboardLayout))
		{
			MS.Win32.UnsafeNativeMethods.TF_LANGUAGEPROFILE tf_profile;
			MS.Win32.UnsafeNativeMethods.ITfFunctionProvider functionPrvForCurrentKeyboardTIP = GetFunctionPrvForCurrentKeyboardTIP(out tf_profile);
			if (functionPrvForCurrentKeyboardTIP != null)
			{
				Guid riid = MS.Win32.UnsafeNativeMethods.IID_ITfFnConfigure;
				Guid guid = MS.Win32.UnsafeNativeMethods.Guid_Null;
				functionPrvForCurrentKeyboardTIP.GetFunction(ref guid, ref riid, out var obj);
				if (obj is MS.Win32.UnsafeNativeMethods.ITfFnConfigure tfFnConfigure)
				{
					result = true;
					if (fShow)
					{
						tfFnConfigure.Show(HwndFromInputElement(element), tf_profile.langid, ref tf_profile.guidProfile);
					}
					Marshal.ReleaseComObject(tfFnConfigure);
				}
				Marshal.ReleaseComObject(functionPrvForCurrentKeyboardTIP);
			}
		}
		else
		{
			result = true;
			if (fShow)
			{
				MS.Win32.UnsafeNativeMethods.ImmConfigureIME(new HandleRef(this, keyboardLayout), new HandleRef(this, HwndFromInputElement(element)), 1, IntPtr.Zero);
			}
		}
		return result;
	}

	private bool _ShowRegisterWordUI(UIElement element, bool fShow, string strRegister)
	{
		bool result = false;
		nint keyboardLayout = SafeNativeMethods.GetKeyboardLayout(0);
		if (!IsImm32Ime(keyboardLayout))
		{
			MS.Win32.UnsafeNativeMethods.TF_LANGUAGEPROFILE tf_profile;
			MS.Win32.UnsafeNativeMethods.ITfFunctionProvider functionPrvForCurrentKeyboardTIP = GetFunctionPrvForCurrentKeyboardTIP(out tf_profile);
			if (functionPrvForCurrentKeyboardTIP != null)
			{
				Guid riid = MS.Win32.UnsafeNativeMethods.IID_ITfFnConfigureRegisterWord;
				Guid guid = MS.Win32.UnsafeNativeMethods.Guid_Null;
				functionPrvForCurrentKeyboardTIP.GetFunction(ref guid, ref riid, out var obj);
				if (obj is MS.Win32.UnsafeNativeMethods.ITfFnConfigureRegisterWord tfFnConfigureRegisterWord)
				{
					result = true;
					if (fShow)
					{
						tfFnConfigureRegisterWord.Show(HwndFromInputElement(element), tf_profile.langid, ref tf_profile.guidProfile, strRegister);
					}
					Marshal.ReleaseComObject(tfFnConfigureRegisterWord);
				}
				Marshal.ReleaseComObject(functionPrvForCurrentKeyboardTIP);
			}
		}
		else
		{
			result = true;
			if (fShow)
			{
				MS.Win32.NativeMethods.REGISTERWORD registerWord = default(MS.Win32.NativeMethods.REGISTERWORD);
				registerWord.lpReading = null;
				registerWord.lpWord = strRegister;
				MS.Win32.UnsafeNativeMethods.ImmConfigureIME(new HandleRef(this, keyboardLayout), new HandleRef(this, HwndFromInputElement(element)), 2, ref registerWord);
			}
		}
		return result;
	}

	private static nint HwndFromInputElement(IInputElement element)
	{
		nint result = 0;
		if (element != null && element is DependencyObject o)
		{
			DependencyObject containingVisual = InputElement.GetContainingVisual(o);
			if (containingVisual != null)
			{
				IWin32Window win32Window = null;
				PresentationSource presentationSource = PresentationSource.CriticalFromVisual(containingVisual);
				if (presentationSource != null && presentationSource is IWin32Window win32Window2)
				{
					result = win32Window2.Handle;
				}
			}
		}
		return result;
	}

	private MS.Win32.UnsafeNativeMethods.ITfFunctionProvider GetFunctionPrvForCurrentKeyboardTIP(out MS.Win32.UnsafeNativeMethods.TF_LANGUAGEPROFILE tf_profile)
	{
		tf_profile = GetCurrentKeybordTipProfile();
		if (tf_profile.clsid.Equals(MS.Win32.UnsafeNativeMethods.Guid_Null))
		{
			return null;
		}
		TextServicesContext.DispatcherCurrent.ThreadManager.GetFunctionProvider(ref tf_profile.clsid, out var funcProvider);
		return funcProvider;
	}

	private MS.Win32.UnsafeNativeMethods.TF_LANGUAGEPROFILE GetCurrentKeybordTipProfile()
	{
		MS.Win32.UnsafeNativeMethods.ITfInputProcessorProfiles tfInputProcessorProfiles = InputProcessorProfilesLoader.Load();
		MS.Win32.UnsafeNativeMethods.TF_LANGUAGEPROFILE result = default(MS.Win32.UnsafeNativeMethods.TF_LANGUAGEPROFILE);
		if (tfInputProcessorProfiles != null)
		{
			CultureInfo currentInputLanguage = InputLanguageManager.Current.CurrentInputLanguage;
			tfInputProcessorProfiles.EnumLanguageProfiles((short)currentInputLanguage.LCID, out var enumIPP);
			MS.Win32.UnsafeNativeMethods.TF_LANGUAGEPROFILE[] array = new MS.Win32.UnsafeNativeMethods.TF_LANGUAGEPROFILE[1];
			int fetched;
			while (enumIPP.Next(1, array, out fetched) == 0)
			{
				if (array[0].fActive && array[0].catid.Equals(MS.Win32.UnsafeNativeMethods.GUID_TFCAT_TIP_KEYBOARD))
				{
					result = array[0];
					break;
				}
			}
			Marshal.ReleaseComObject(enumIPP);
		}
		return result;
	}

	private bool IsValidConversionMode(ImeConversionModeValues mode)
	{
		int num = -2147482625;
		if (((uint)mode & (uint)(~num)) != 0)
		{
			return false;
		}
		return true;
	}

	private bool IsValidSentenceMode(ImeSentenceModeValues mode)
	{
		int num = -2147483617;
		if (((uint)mode & (uint)(~num)) != 0)
		{
			return false;
		}
		return true;
	}
}
