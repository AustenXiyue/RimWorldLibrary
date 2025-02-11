using System.Globalization;

namespace System.Windows.Input;

/// <summary>Contains arguments associated with the <see cref="E:System.Windows.Input.InputLanguageManager.InputLanguageChanged" /> event.</summary>
public class InputLanguageChangedEventArgs : InputLanguageEventArgs
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputLanguageChangedEventArgs" /> class.</summary>
	/// <param name="newLanguageId">A <see cref="T:System.Globalization.CultureInfo" /> object representing a new current input language.</param>
	/// <param name="previousLanguageId">A <see cref="T:System.Globalization.CultureInfo" /> object representing the previous current input language.</param>
	public InputLanguageChangedEventArgs(CultureInfo newLanguageId, CultureInfo previousLanguageId)
		: base(newLanguageId, previousLanguageId)
	{
	}
}
