using System.Globalization;

namespace System.Windows.Input;

/// <summary>Contains arguments associated with the <see cref="E:System.Windows.Input.InputLanguageManager.InputLanguageChanging" /> event.</summary>
public class InputLanguageChangingEventArgs : InputLanguageEventArgs
{
	private bool _rejected;

	/// <summary>Gets or sets a value that indicates whether the initiated change of input language should be accepted or rejected.</summary>
	/// <returns>true to reject the initiated change of input language; otherwise, false.</returns>
	public bool Rejected
	{
		get
		{
			return _rejected;
		}
		set
		{
			_rejected = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputLanguageChangingEventArgs" /> class.</summary>
	/// <param name="newLanguageId">A <see cref="T:System.Globalization.CultureInfo" /> object representing a new current input language.</param>
	/// <param name="previousLanguageId">A <see cref="T:System.Globalization.CultureInfo" /> object representing the previous current input language.</param>
	public InputLanguageChangingEventArgs(CultureInfo newLanguageId, CultureInfo previousLanguageId)
		: base(newLanguageId, previousLanguageId)
	{
		_rejected = false;
	}
}
