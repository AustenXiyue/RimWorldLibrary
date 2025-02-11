using System.ComponentModel;
using System.Windows.Threading;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Represents a composition related to text input which includes the composition text itself, any related control or system text, and a state of completion for the composition.</summary>
public class TextComposition : DispatcherObject
{
	private readonly InputManager _inputManager;

	private readonly InputDevice _inputDevice;

	private string _resultText;

	private string _compositionText;

	private string _systemText;

	private string _controlText;

	private string _systemCompositionText;

	private readonly TextCompositionAutoComplete _autoComplete;

	private TextCompositionStage _stage;

	private IInputElement _source;

	/// <summary>Gets or sets the current text for this text composition.</summary>
	/// <returns>A string containing the current text for this text composition.</returns>
	[CLSCompliant(false)]
	public string Text
	{
		get
		{
			return _resultText;
		}
		protected set
		{
			_resultText = value;
		}
	}

	/// <summary>Gets or sets the composition text for this text composition.</summary>
	/// <returns>A string containing the composition text for this text composition.</returns>
	[CLSCompliant(false)]
	public string CompositionText
	{
		get
		{
			return _compositionText;
		}
		protected set
		{
			_compositionText = value;
		}
	}

	/// <summary>Gets or sets the system text for this text composition.</summary>
	/// <returns>A string containing the system text for this text composition.</returns>
	[CLSCompliant(false)]
	public string SystemText
	{
		get
		{
			return _systemText;
		}
		protected set
		{
			_systemText = value;
		}
	}

	/// <summary>Gets or sets any control text associated with this text composition.</summary>
	/// <returns>A string containing any control text associated with this text composition.</returns>
	[CLSCompliant(false)]
	public string ControlText
	{
		get
		{
			return _controlText;
		}
		protected set
		{
			_controlText = value;
		}
	}

	/// <summary>Gets or sets the system composition text for this text composition.</summary>
	/// <returns>A string containing the system composition text for this text composition.</returns>
	[CLSCompliant(false)]
	public string SystemCompositionText
	{
		get
		{
			return _systemCompositionText;
		}
		protected set
		{
			_systemCompositionText = value;
		}
	}

	/// <summary>Gets the auto-complete setting for this text composition.</summary>
	/// <returns>A member of the <see cref="T:System.Windows.Input.TextCompositionAutoComplete" /> enumerations specifying the current auto-complete behavior for this text composition.</returns>
	public TextCompositionAutoComplete AutoComplete => _autoComplete;

	internal IInputElement Source => _source;

	internal InputDevice _InputDevice => _inputDevice;

	internal InputManager _InputManager => _inputManager;

	internal TextCompositionStage Stage
	{
		get
		{
			return _stage;
		}
		set
		{
			_stage = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.TextComposition" /> class, taking a specified <see cref="T:System.Windows.Input.InputManager" />, source element, and composition text as initial values for the new instance.</summary>
	/// <param name="inputManager">An input manager to associate with this text composition.</param>
	/// <param name="source">A source element for this text composition.  The object underlying the source element must implement the <see cref="T:System.Windows.IInputElement" /> interface.</param>
	/// <param name="resultText">A string containing the initial text for the composition.  This parameter will become the value of the <see cref="P:System.Windows.Input.TextComposition.Text" /> property in the new class instance.</param>
	public TextComposition(InputManager inputManager, IInputElement source, string resultText)
		: this(inputManager, source, resultText, TextCompositionAutoComplete.On)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.TextComposition" /> class, taking a specified <see cref="T:System.Windows.Input.InputManager" />, source element, composition text, and a <see cref="T:System.Windows.Input.TextCompositionAutoComplete" /> setting as initial values for the new instance.</summary>
	/// <param name="inputManager">An input manager to associate with this text composition.</param>
	/// <param name="source">A source element for this text composition.  The object underlying the source element must implement the <see cref="T:System.Windows.IInputElement" /> interface.</param>
	/// <param name="resultText">A string containing the initial text for the composition.  This parameter will become the value of the <see cref="P:System.Windows.Input.TextComposition.Text" /> property in the new class instance.</param>
	/// <param name="autoComplete">A member of the <see cref="T:System.Windows.Input.TextCompositionAutoComplete" /> enumerations specifying desired auto-complete behavior for this text composition.</param>
	public TextComposition(InputManager inputManager, IInputElement source, string resultText, TextCompositionAutoComplete autoComplete)
		: this(inputManager, source, resultText, autoComplete, InputManager.Current.PrimaryKeyboardDevice)
	{
		if (autoComplete != 0 && autoComplete != TextCompositionAutoComplete.On)
		{
			throw new InvalidEnumArgumentException("autoComplete", (int)autoComplete, typeof(TextCompositionAutoComplete));
		}
	}

	internal TextComposition(InputManager inputManager, IInputElement source, string resultText, TextCompositionAutoComplete autoComplete, InputDevice inputDevice)
	{
		_inputManager = inputManager;
		_inputDevice = inputDevice;
		if (resultText == null)
		{
			throw new ArgumentException(SR.TextComposition_NullResultText);
		}
		_resultText = resultText;
		_compositionText = "";
		_systemText = "";
		_systemCompositionText = "";
		_controlText = "";
		_autoComplete = autoComplete;
		_stage = TextCompositionStage.None;
		_source = source;
	}

	/// <summary>Completes this text composition.</summary>
	public virtual void Complete()
	{
		TextCompositionManager.CompleteComposition(this);
	}

	internal void SetText(string resultText)
	{
		_resultText = resultText;
	}

	internal void SetCompositionText(string compositionText)
	{
		_compositionText = compositionText;
	}

	internal void MakeSystem()
	{
		_systemText = _resultText;
		_systemCompositionText = _compositionText;
		_resultText = "";
		_compositionText = "";
		_controlText = "";
	}

	internal void MakeControl()
	{
		_controlText = _resultText;
		_resultText = "";
		_systemText = "";
		_compositionText = "";
		_systemCompositionText = "";
	}

	internal void ClearTexts()
	{
		_resultText = "";
		_compositionText = "";
		_systemText = "";
		_systemCompositionText = "";
		_controlText = "";
	}
}
