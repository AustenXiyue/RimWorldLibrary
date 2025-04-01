using System.ComponentModel;

namespace System.Text.RegularExpressions;

/// <summary>Creates a <see cref="T:System.Text.RegularExpressions.RegexRunner" /> class for a compiled regular expression.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class RegexRunnerFactory
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Text.RegularExpressions.RegexRunnerFactory" /> class. </summary>
	protected RegexRunnerFactory()
	{
	}

	/// <summary>When overridden in a derived class, creates a <see cref="T:System.Text.RegularExpressions.RegexRunner" /> object for a specific compiled regular expression.</summary>
	/// <returns>A <see cref="T:System.Text.RegularExpressions.RegexRunner" /> object designed to execute a specific compiled regular expression. </returns>
	protected internal abstract RegexRunner CreateInstance();
}
