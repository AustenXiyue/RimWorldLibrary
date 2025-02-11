namespace System.Windows;

internal sealed class DependencySource
{
	private DependencyObject _d;

	private DependencyProperty _dp;

	public DependencyObject DependencyObject => _d;

	public DependencyProperty DependencyProperty => _dp;

	public DependencySource(DependencyObject d, DependencyProperty dp)
	{
		_d = d;
		_dp = dp;
	}
}
