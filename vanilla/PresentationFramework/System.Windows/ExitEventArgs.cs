namespace System.Windows;

/// <summary>Event arguments for the <see cref="E:System.Windows.Application.Exit" /> event.</summary>
public class ExitEventArgs : EventArgs
{
	internal int _exitCode;

	/// <summary>Gets or sets the exit code that an application returns to the operating system when the application exits.</summary>
	/// <returns>The exit code that an application returns to the operating system when the application exits.</returns>
	public int ApplicationExitCode
	{
		get
		{
			return _exitCode;
		}
		set
		{
			_exitCode = value;
		}
	}

	internal ExitEventArgs(int exitCode)
	{
		_exitCode = exitCode;
	}
}
