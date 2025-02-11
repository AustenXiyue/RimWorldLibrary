namespace System.Windows.Documents;

internal interface IFixedNavigate
{
	UIElement FindElementByID(string elementID, out FixedPage rootFixedPage);

	void NavigateAsync(string elementID);
}
