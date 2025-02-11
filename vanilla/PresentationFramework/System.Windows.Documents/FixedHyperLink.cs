using System.Windows.Navigation;

namespace System.Windows.Documents;

internal static class FixedHyperLink
{
	public static void OnNavigationServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is FixedDocument)
		{
			NavigationService navigationService = (NavigationService)e.OldValue;
			NavigationService navigationService2 = (NavigationService)e.NewValue;
			if (navigationService != null)
			{
				navigationService.FragmentNavigation -= FragmentHandler;
			}
			if (navigationService2 != null)
			{
				navigationService2.FragmentNavigation += FragmentHandler;
			}
		}
	}

	internal static void FragmentHandler(object sender, FragmentNavigationEventArgs e)
	{
		if (sender is NavigationService navigationService)
		{
			_ = e.Fragment;
			if (navigationService.Content is IFixedNavigate fixedNavigate)
			{
				fixedNavigate.NavigateAsync(e.Fragment);
				e.Handled = true;
			}
		}
	}

	internal static void NavigateToElement(object ElementHost, string elementID)
	{
		FixedPage rootFixedPage = null;
		FrameworkElement frameworkElement = null;
		if (((IFixedNavigate)ElementHost).FindElementByID(elementID, out rootFixedPage) is FrameworkElement frameworkElement2)
		{
			if (frameworkElement2 is FixedPage)
			{
				frameworkElement2.BringIntoView();
			}
			else
			{
				frameworkElement2.BringIntoView(frameworkElement2.VisualContentBounds);
			}
		}
	}
}
