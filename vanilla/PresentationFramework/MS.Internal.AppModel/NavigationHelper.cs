using System.Windows.Controls;
using System.Windows.Media;

namespace MS.Internal.AppModel;

internal static class NavigationHelper
{
	internal static Visual FindRootViewer(ContentControl navigator, string contentPresenterName)
	{
		object content = navigator.Content;
		if (content == null || content is Visual)
		{
			return content as Visual;
		}
		ContentPresenter contentPresenter = null;
		if (navigator.Template != null)
		{
			contentPresenter = (ContentPresenter)navigator.Template.FindName(contentPresenterName, navigator);
		}
		if (contentPresenter == null || contentPresenter.InternalVisualChildrenCount == 0)
		{
			return null;
		}
		return contentPresenter.InternalGetVisualChild(0);
	}
}
