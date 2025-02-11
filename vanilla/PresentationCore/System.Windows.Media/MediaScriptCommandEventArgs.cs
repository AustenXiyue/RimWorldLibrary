namespace System.Windows.Media;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.MediaElement.ScriptCommand" /> and <see cref="E:System.Windows.Media.MediaPlayer.ScriptCommand" /> events.</summary>
public sealed class MediaScriptCommandEventArgs : EventArgs
{
	private string _parameterType;

	private string _parameterValue;

	/// <summary>Gets the type of script command that was raised.</summary>
	/// <returns>The type of script command.</returns>
	public string ParameterType => _parameterType;

	/// <summary>Gets the arguments associated with the script command type.</summary>
	/// <returns>The arguments associated with the script command type.</returns>
	public string ParameterValue => _parameterValue;

	internal MediaScriptCommandEventArgs(string parameterType, string parameterValue)
	{
		if (parameterType == null)
		{
			throw new ArgumentNullException("parameterType");
		}
		if (parameterValue == null)
		{
			throw new ArgumentNullException("parameterValue");
		}
		_parameterType = parameterType;
		_parameterValue = parameterValue;
	}
}
