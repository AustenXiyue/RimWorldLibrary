using System.ComponentModel;

namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="E:System.Windows.UIElement.KeyUp" /> and <see cref="E:System.Windows.UIElement.KeyDown" /> routed events, as well as related attached and Preview events.</summary>
public class KeyEventArgs : KeyboardEventArgs
{
	private Key _realKey;

	private Key _key;

	private PresentationSource _inputSource;

	private bool _isRepeat;

	private int _scanCode;

	private bool _isExtendedKey;

	/// <summary>Gets the input source that provided this input. </summary>
	/// <returns>The input source.</returns>
	public PresentationSource InputSource => UnsafeInputSource;

	/// <summary>Gets the keyboard key associated with the event. </summary>
	/// <returns>The <see cref="T:System.Windows.Input.Key" /> referenced by the event.</returns>
	public Key Key => _key;

	internal Key RealKey => _realKey;

	/// <summary>Gets the keyboard key referenced by the event, if the key will be processed by an Input Method Editor (IME). </summary>
	/// <returns>The <see cref="T:System.Windows.Input.Key" /> referenced by the event.</returns>
	public Key ImeProcessedKey
	{
		get
		{
			if (_key != Key.ImeProcessed)
			{
				return Key.None;
			}
			return _realKey;
		}
	}

	/// <summary>Gets the keyboard key referenced by the event, if the key will be processed by the system. </summary>
	/// <returns>The <see cref="T:System.Windows.Input.Key" /> referenced by the event.</returns>
	public Key SystemKey
	{
		get
		{
			if (_key != Key.System)
			{
				return Key.None;
			}
			return _realKey;
		}
	}

	/// <summary>Gets the key that is part of dead key composition to create a single combined character.</summary>
	/// <returns>The key that is part of dead key composition to create a single combined character.</returns>
	public Key DeadCharProcessedKey
	{
		get
		{
			if (_key != Key.DeadCharProcessed)
			{
				return Key.None;
			}
			return _realKey;
		}
	}

	/// <summary>Gets the state of the keyboard key associated with this event. </summary>
	/// <returns>The state of the key.</returns>
	public KeyStates KeyStates => base.KeyboardDevice.GetKeyStates(_realKey);

	/// <summary>Gets a value that indicates whether the keyboard key referenced by the event is a repeated key. </summary>
	/// <returns>true if the key is repeated; otherwise, false.  There is no default value.</returns>
	public bool IsRepeat => _isRepeat;

	/// <summary>Gets a value that indicates whether the key referenced by the event is in the down state. </summary>
	/// <returns>true if the key is down; otherwise, false.</returns>
	public bool IsDown => base.KeyboardDevice.IsKeyDown(_realKey);

	/// <summary>Gets a value that indicates whether the key referenced by the event is in the up state. </summary>
	/// <returns>true if the key is up; otherwise, false.  There is no default value.</returns>
	public bool IsUp => base.KeyboardDevice.IsKeyUp(_realKey);

	/// <summary>Gets a value that indicates whether the key referenced by the event is in the toggled state. </summary>
	/// <returns>true if the key is toggled; otherwise, false.  There is no default value.</returns>
	public bool IsToggled => base.KeyboardDevice.IsKeyToggled(_realKey);

	internal PresentationSource UnsafeInputSource => _inputSource;

	internal int ScanCode
	{
		get
		{
			return _scanCode;
		}
		set
		{
			_scanCode = value;
		}
	}

	internal bool IsExtendedKey
	{
		get
		{
			return _isExtendedKey;
		}
		set
		{
			_isExtendedKey = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.KeyEventArgs" /> class. </summary>
	/// <param name="keyboard">The logical keyboard device associated with this event.</param>
	/// <param name="inputSource">The input source.</param>
	/// <param name="timestamp">The time when the input occurred.</param>
	/// <param name="key">The key referenced by the event.</param>
	public KeyEventArgs(KeyboardDevice keyboard, PresentationSource inputSource, int timestamp, Key key)
		: base(keyboard, timestamp)
	{
		if (inputSource == null)
		{
			throw new ArgumentNullException("inputSource");
		}
		if (!Keyboard.IsValidKey(key))
		{
			throw new InvalidEnumArgumentException("key", (int)key, typeof(Key));
		}
		_inputSource = inputSource;
		_realKey = key;
		_isRepeat = false;
		MarkNormal();
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((KeyEventHandler)genericHandler)(genericTarget, this);
	}

	internal void SetRepeat(bool newRepeatState)
	{
		_isRepeat = newRepeatState;
	}

	internal void MarkNormal()
	{
		_key = _realKey;
	}

	internal void MarkSystem()
	{
		_key = Key.System;
	}

	internal void MarkImeProcessed()
	{
		_key = Key.ImeProcessed;
	}

	internal void MarkDeadCharProcessed()
	{
		_key = Key.DeadCharProcessed;
	}
}
