using System.Windows;

namespace MS.Internal.Data;

internal struct LivePropertyInfo
{
	private string _path;

	private DependencyProperty _dp;

	public string Path => _path;

	public DependencyProperty Property => _dp;

	public LivePropertyInfo(string path, DependencyProperty dp)
	{
		_path = path;
		_dp = dp;
	}
}
