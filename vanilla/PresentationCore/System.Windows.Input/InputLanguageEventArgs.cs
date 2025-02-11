using System.Globalization;

namespace System.Windows.Input;

/// <summary>Provides a base class for arguments for events dealing with a change in input language.</summary>
public abstract class InputLanguageEventArgs : EventArgs
{
	private CultureInfo _newLanguageId;

	private CultureInfo _previousLanguageId;

	/// <summary>Gets a <see cref="T:System.Globalization.CultureInfo" /> object representing the new current input language.</summary>
	/// <returns>A <see cref="T:System.Globalization.CultureInfo" /> object representing the new current input language.</returns>
	public virtual CultureInfo NewLanguage => _newLanguageId;

	/// <summary>Gets a <see cref="T:System.Globalization.CultureInfo" /> object representing the previous current input language.</summary>
	/// <returns>A <see cref="T:System.Globalization.CultureInfo" /> object representing the previous current input language.</returns>
	public virtual CultureInfo PreviousLanguage => _previousLanguageId;

	/// <summary>Initializes base class values for a new instance of a deriving class.</summary>
	/// <param name="newLanguageId">A <see cref="T:System.Globalization.CultureInfo" /> object representing a new current input language.</param>
	/// <param name="previousLanguageId">A <see cref="T:System.Globalization.CultureInfo" /> object representing the previous current input language.</param>
	protected InputLanguageEventArgs(CultureInfo newLanguageId, CultureInfo previousLanguageId)
	{
		_newLanguageId = newLanguageId;
		_previousLanguageId = previousLanguageId;
	}
}
