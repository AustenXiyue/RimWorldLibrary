using MS.Internal;

namespace System.Windows;

/// <summary>Contains the arguments for the <see cref="E:System.Windows.Application.Startup" /> event.</summary>
public class StartupEventArgs : EventArgs
{
	private string[] _args;

	private bool _performDefaultAction;

	/// <summary>Gets command line arguments that were passed to the application from either the command prompt or the desktop.</summary>
	/// <returns>A string array that contains the command line arguments that were passed to the application from either the command prompt or the desktop. If no command line arguments were passed, the string array as zero items.</returns>
	public string[] Args
	{
		get
		{
			if (_args == null)
			{
				_args = GetCmdLineArgs();
			}
			return _args;
		}
	}

	internal bool PerformDefaultAction
	{
		get
		{
			return _performDefaultAction;
		}
		set
		{
			_performDefaultAction = value;
		}
	}

	internal StartupEventArgs()
	{
		_performDefaultAction = true;
	}

	private string[] GetCmdLineArgs()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		Invariant.Assert(commandLineArgs.Length >= 1);
		int num = commandLineArgs.Length - 1;
		num = ((num >= 0) ? num : 0);
		string[] array = new string[num];
		for (int i = 1; i < commandLineArgs.Length; i++)
		{
			array[i - 1] = commandLineArgs[i];
		}
		return array;
	}
}
