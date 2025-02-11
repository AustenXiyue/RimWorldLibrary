namespace System.Windows;

internal sealed class SystemResourceHost
{
	private static SystemResourceHost _instance;

	internal static SystemResourceHost Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new SystemResourceHost();
			}
			return _instance;
		}
	}

	private SystemResourceHost()
	{
	}
}
