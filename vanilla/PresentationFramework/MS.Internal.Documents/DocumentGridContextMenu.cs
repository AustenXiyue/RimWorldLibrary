using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MS.Internal.Documents;

internal static class DocumentGridContextMenu
{
	private class ViewerContextMenu : ContextMenu
	{
		internal void AddMenuItems(DocumentGrid dg, bool userInitiated)
		{
			base.Name = "ViewerContextMenu";
			SetMenuProperties(new EditorMenuItem(), dg, ApplicationCommands.Copy);
			SetMenuProperties(new MenuItem(), dg, ApplicationCommands.SelectAll);
			AddSeparator();
			SetMenuProperties(new MenuItem(), dg, NavigationCommands.PreviousPage, SR.DocumentApplicationContextMenuPreviousPageHeader, SR.DocumentApplicationContextMenuPreviousPageInputGesture);
			SetMenuProperties(new MenuItem(), dg, NavigationCommands.NextPage, SR.DocumentApplicationContextMenuNextPageHeader, SR.DocumentApplicationContextMenuNextPageInputGesture);
			SetMenuProperties(new MenuItem(), dg, NavigationCommands.FirstPage, null, SR.DocumentApplicationContextMenuFirstPageInputGesture);
			SetMenuProperties(new MenuItem(), dg, NavigationCommands.LastPage, null, SR.DocumentApplicationContextMenuLastPageInputGesture);
			AddSeparator();
			SetMenuProperties(new MenuItem(), dg, ApplicationCommands.Print);
		}

		private void AddSeparator()
		{
			base.Items.Add(new Separator());
		}

		private void SetMenuProperties(MenuItem menuItem, DocumentGrid dg, RoutedUICommand command)
		{
			SetMenuProperties(menuItem, dg, command, null, null);
		}

		private void SetMenuProperties(MenuItem menuItem, DocumentGrid dg, RoutedUICommand command, string header, string inputGestureText)
		{
			menuItem.Command = command;
			menuItem.CommandTarget = dg.DocumentViewerOwner;
			if (header == null)
			{
				menuItem.Header = command.Text;
			}
			else
			{
				menuItem.Header = header;
			}
			if (inputGestureText != null)
			{
				menuItem.InputGestureText = inputGestureText;
			}
			menuItem.Name = "ViewerContextMenu_" + command.Name;
			base.Items.Add(menuItem);
		}
	}

	private class EditorMenuItem : MenuItem
	{
		internal EditorMenuItem()
		{
		}

		internal override void OnClickCore(bool userInitiated)
		{
			OnClickImpl(userInitiated);
		}
	}

	private const double KeyboardInvokedSentinel = -1.0;

	internal static void RegisterClassHandler()
	{
		EventManager.RegisterClassHandler(typeof(DocumentGrid), FrameworkElement.ContextMenuOpeningEvent, new ContextMenuEventHandler(OnContextMenuOpening));
		EventManager.RegisterClassHandler(typeof(DocumentApplicationDocumentViewer), FrameworkElement.ContextMenuOpeningEvent, new ContextMenuEventHandler(OnDocumentViewerContextMenuOpening));
	}

	private static void OnDocumentViewerContextMenuOpening(object sender, ContextMenuEventArgs e)
	{
		if (e.CursorLeft == -1.0 && sender is DocumentViewer { ScrollViewer: not null } documentViewer)
		{
			OnContextMenuOpening(documentViewer.ScrollViewer.Content, e);
		}
	}

	private static void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
	{
		if (sender is DocumentGrid documentGrid && documentGrid.DocumentViewerOwner is DocumentApplicationDocumentViewer && documentGrid.DocumentViewerOwner.ContextMenu == null && documentGrid.DocumentViewerOwner.ScrollViewer.ContextMenu == null)
		{
			ContextMenu contextMenu = documentGrid.ContextMenu;
			if (documentGrid.ReadLocalValue(FrameworkElement.ContextMenuProperty) != null && contextMenu == null)
			{
				contextMenu = new ViewerContextMenu();
				contextMenu.Placement = PlacementMode.RelativePoint;
				contextMenu.PlacementTarget = documentGrid;
				((ViewerContextMenu)contextMenu).AddMenuItems(documentGrid, e.UserInitiated);
				Point point = ((e.CursorLeft != -1.0) ? Mouse.GetPosition(documentGrid) : new Point(0.5 * documentGrid.ViewportWidth, 0.5 * documentGrid.ViewportHeight));
				contextMenu.HorizontalOffset = point.X;
				contextMenu.VerticalOffset = point.Y;
				contextMenu.IsOpen = true;
				e.Handled = true;
			}
		}
	}
}
