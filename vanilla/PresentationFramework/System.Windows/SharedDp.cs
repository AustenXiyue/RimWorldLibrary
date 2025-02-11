namespace System.Windows;

internal class SharedDp
{
	internal DependencyProperty Dp;

	internal object Value;

	internal string ElementName;

	internal SharedDp(DependencyProperty dp, object value, string elementName)
	{
		Dp = dp;
		Value = value;
		ElementName = elementName;
	}
}
