using System.Collections;
using System.Globalization;

namespace System.Windows.Input;

/// <summary>Defines necessary facilities for an object that intends to behave as an input language source.</summary>
public interface IInputLanguageSource
{
	/// <summary>Gets or sets the current input language for this input language source object.</summary>
	/// <returns>A <see cref="T:System.Globalization.CultureInfo" /> object representing the current input language for this input language source object.</returns>
	CultureInfo CurrentInputLanguage { get; set; }

	/// <summary>Gets a list of input languages supported by this input language source object.</summary>
	/// <returns>An enumerable object that represents the list of input languages supported by this input language source object.</returns>
	IEnumerable InputLanguageList { get; }

	/// <summary>Initializes an input language source object.</summary>
	void Initialize();

	/// <summary>Un-initializes an input language source object.</summary>
	void Uninitialize();
}
