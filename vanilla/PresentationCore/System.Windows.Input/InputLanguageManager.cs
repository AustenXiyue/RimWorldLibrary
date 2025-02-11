using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Threading;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace System.Windows.Input;

/// <summary>Provides facilities for managing input languages in Windows Presentation Foundation (WPF).</summary>
public sealed class InputLanguageManager : DispatcherObject
{
	/// <summary>Identifies the <see cref="P:System.Windows.Input.InputLanguageManager.InputLanguage" />  attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.InputLanguageManager.InputLanguage" /> attached property.</returns>
	public static readonly DependencyProperty InputLanguageProperty = DependencyProperty.RegisterAttached("InputLanguage", typeof(CultureInfo), typeof(InputLanguageManager), new PropertyMetadata(CultureInfo.InvariantCulture));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.InputLanguageManager.RestoreInputLanguage" />  attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.InputLanguageManager.RestoreInputLanguage" /> attached property.</returns>
	public static readonly DependencyProperty RestoreInputLanguageProperty = DependencyProperty.RegisterAttached("RestoreInputLanguage", typeof(bool), typeof(InputLanguageManager), new PropertyMetadata(false));

	private CultureInfo _previousLanguageId;

	private IInputLanguageSource _source;

	/// <summary>Gets the input language manager associated with the current context.</summary>
	/// <returns>An <see cref="T:System.Windows.Input.InputLanguageManager" /> object associated with the current context.This property has no default value.</returns>
	public static InputLanguageManager Current
	{
		get
		{
			if (InputMethod.Current.InputLanguageManager == null)
			{
				InputMethod.Current.InputLanguageManager = new InputLanguageManager();
			}
			return InputMethod.Current.InputLanguageManager;
		}
	}

	/// <summary>Gets or sets the current input language.</summary>
	/// <returns>A <see cref="T:System.Globalization.CultureInfo" /> object representing the currently selected input language.  This property may not be set to null.The default value is <see cref="P:System.Globalization.CultureInfo.InvariantCulture" />.</returns>
	/// <exception cref="T:System.ArgumentNullException">Raised when an attempt is made to set this property to null.</exception>
	[TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
	public CultureInfo CurrentInputLanguage
	{
		get
		{
			if (_source != null)
			{
				return _source.CurrentInputLanguage;
			}
			nint keyboardLayout = SafeNativeMethods.GetKeyboardLayout(0);
			if (keyboardLayout == IntPtr.Zero)
			{
				return CultureInfo.InvariantCulture;
			}
			return new CultureInfo((short)MS.Win32.NativeMethods.IntPtrToInt32(keyboardLayout));
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			SetSourceCurrentLanguageId(value);
		}
	}

	/// <summary>Gets an enumerator for currently available input languages.</summary>
	/// <returns>An enumerator for currently available input languages, or null if no input languages are available.This property has no default value.</returns>
	public IEnumerable AvailableInputLanguages
	{
		get
		{
			if (_source == null)
			{
				return null;
			}
			return _source.InputLanguageList;
		}
	}

	internal IInputLanguageSource Source => _source;

	internal static bool IsMultipleKeyboardLayout => SafeNativeMethods.GetKeyboardLayoutList(0, null) > 1;

	/// <summary>Occurs when a change of input language is completed.</summary>
	public event InputLanguageEventHandler InputLanguageChanged
	{
		add
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (this._InputLanguageChanged == null && this._InputLanguageChanging == null && IsMultipleKeyboardLayout && _source != null)
			{
				_source.Initialize();
			}
			_InputLanguageChanged += value;
		}
		remove
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_InputLanguageChanged -= value;
			if (this._InputLanguageChanged == null && this._InputLanguageChanging == null && IsMultipleKeyboardLayout && _source != null)
			{
				_source.Uninitialize();
			}
		}
	}

	/// <summary>Occurs when a change of input language is initiated.</summary>
	public event InputLanguageEventHandler InputLanguageChanging
	{
		add
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (this._InputLanguageChanged == null && this._InputLanguageChanging == null && IsMultipleKeyboardLayout && _source != null)
			{
				_source.Initialize();
			}
			_InputLanguageChanging += value;
		}
		remove
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_InputLanguageChanging -= value;
			if (this._InputLanguageChanged == null && this._InputLanguageChanging == null && IsMultipleKeyboardLayout && _source != null)
			{
				_source.Uninitialize();
			}
		}
	}

	private event InputLanguageEventHandler _InputLanguageChanged;

	private event InputLanguageEventHandler _InputLanguageChanging;

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.InputLanguageManager.InputLanguage" /> attached property on the specified dependency object.</summary>
	/// <param name="target">The dependency object on which to set the <see cref="P:System.Windows.Input.InputLanguageManager.InputLanguage" /> attached property.</param>
	/// <param name="inputLanguage">A <see cref="T:System.Globalization.CultureInfo" /> object representing the new value for the <see cref="P:System.Windows.Input.InputLanguageManager.InputLanguage" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	public static void SetInputLanguage(DependencyObject target, CultureInfo inputLanguage)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(InputLanguageProperty, inputLanguage);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Input.InputLanguageManager.InputLanguage" />  attached property for a specified dependency object.</summary>
	/// <returns>A <see cref="T:System.Globalization.CultureInfo" /> object representing the input language for the specified dependency object.</returns>
	/// <param name="target">The dependency object for which to retrieve the input language.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	[TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
	public static CultureInfo GetInputLanguage(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (CultureInfo)target.GetValue(InputLanguageProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.InputLanguageManager.RestoreInputLanguage" /> dependency property on the specified dependency object.</summary>
	/// <param name="target">The dependency object for which to set the value of <see cref="P:System.Windows.Input.InputLanguageManager.RestoreInputLanguage" />.</param>
	/// <param name="restore">A Boolean value to set the <see cref="P:System.Windows.Input.InputLanguageManager.RestoreInputLanguage" /> attached property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	public static void SetRestoreInputLanguage(DependencyObject target, bool restore)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(RestoreInputLanguageProperty, restore);
	}

	/// <summary>Returns the value of <see cref="P:System.Windows.Input.InputLanguageManager.RestoreInputLanguage" />  attached property for a specified dependency object.</summary>
	/// <returns>The current value of <see cref="P:System.Windows.Input.InputLanguageManager.RestoreInputLanguage" /> for the specified dependency object.</returns>
	/// <param name="target">The dependency object for which to retrieve the value of <see cref="P:System.Windows.Input.InputLanguageManager.RestoreInputLanguage" />.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="target" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetRestoreInputLanguage(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (bool)target.GetValue(RestoreInputLanguageProperty);
	}

	private InputLanguageManager()
	{
		RegisterInputLanguageSource(new InputLanguageSource(this));
	}

	/// <summary>Registers an input language source with the <see cref="T:System.Windows.Input.InputLanguageManager" />.</summary>
	/// <param name="inputLanguageSource">An object that specifies the input language to register.  This object must implement the <see cref="T:System.Windows.Input.IInputLanguageSource" /> interface.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="inputLanguageSource" /> is null.</exception>
	public void RegisterInputLanguageSource(IInputLanguageSource inputLanguageSource)
	{
		if (inputLanguageSource == null)
		{
			throw new ArgumentNullException("inputLanguageSource");
		}
		_source = inputLanguageSource;
		if ((this._InputLanguageChanged != null || this._InputLanguageChanging != null) && IsMultipleKeyboardLayout)
		{
			_source.Initialize();
		}
	}

	/// <summary>Report the completion of a change of input language to the <see cref="T:System.Windows.Input.InputLanguageManager" />.</summary>
	/// <param name="newLanguageId">A <see cref="T:System.Globalization.CultureInfo" /> object representing the new input language.</param>
	/// <param name="previousLanguageId">A <see cref="T:System.Globalization.CultureInfo" /> object representing the previous input language.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="newLanguageId" /> or <paramref name="previousLanguageId" /> is null.</exception>
	public void ReportInputLanguageChanged(CultureInfo newLanguageId, CultureInfo previousLanguageId)
	{
		if (newLanguageId == null)
		{
			throw new ArgumentNullException("newLanguageId");
		}
		if (previousLanguageId == null)
		{
			throw new ArgumentNullException("previousLanguageId");
		}
		if (!previousLanguageId.Equals(_previousLanguageId))
		{
			_previousLanguageId = null;
		}
		if (this._InputLanguageChanged != null)
		{
			InputLanguageChangedEventArgs e = new InputLanguageChangedEventArgs(newLanguageId, previousLanguageId);
			this._InputLanguageChanged(this, e);
		}
	}

	/// <summary>Report the initiation of a change of input language to the <see cref="T:System.Windows.Input.InputLanguageManager" />.</summary>
	/// <returns>true to indicate that the reported change of input language was accepted; otherwise, false.</returns>
	/// <param name="newLanguageId">A <see cref="T:System.Globalization.CultureInfo" /> object representing the new input language.</param>
	/// <param name="previousLanguageId">A <see cref="T:System.Globalization.CultureInfo" /> object representing the previous input language.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="newLanguageId" /> or <paramref name="previousLanguageId" /> is null.</exception>
	public bool ReportInputLanguageChanging(CultureInfo newLanguageId, CultureInfo previousLanguageId)
	{
		if (newLanguageId == null)
		{
			throw new ArgumentNullException("newLanguageId");
		}
		if (previousLanguageId == null)
		{
			throw new ArgumentNullException("previousLanguageId");
		}
		bool result = true;
		if (this._InputLanguageChanging != null)
		{
			InputLanguageChangingEventArgs inputLanguageChangingEventArgs = new InputLanguageChangingEventArgs(newLanguageId, previousLanguageId);
			this._InputLanguageChanging(this, inputLanguageChangingEventArgs);
			result = !inputLanguageChangingEventArgs.Rejected;
		}
		return result;
	}

	internal void Focus(DependencyObject focus, DependencyObject focused)
	{
		CultureInfo cultureInfo = null;
		if (focus != null)
		{
			cultureInfo = (CultureInfo)focus.GetValue(InputLanguageProperty);
		}
		if (cultureInfo == null || cultureInfo.Equals(CultureInfo.InvariantCulture))
		{
			if (focused != null)
			{
				if (_previousLanguageId != null && (bool)focused.GetValue(RestoreInputLanguageProperty))
				{
					SetSourceCurrentLanguageId(_previousLanguageId);
				}
				_previousLanguageId = null;
			}
		}
		else
		{
			CultureInfo currentInputLanguage = _source.CurrentInputLanguage;
			SetSourceCurrentLanguageId(cultureInfo);
			_previousLanguageId = currentInputLanguage;
		}
	}

	private void SetSourceCurrentLanguageId(CultureInfo languageId)
	{
		if (_source == null)
		{
			throw new InvalidOperationException(SR.InputLanguageManager_NotReadyToChangeCurrentLanguage);
		}
		_source.CurrentInputLanguage = languageId;
	}
}
