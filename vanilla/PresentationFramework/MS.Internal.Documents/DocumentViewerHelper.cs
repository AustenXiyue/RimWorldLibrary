using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal.PresentationFramework;

namespace MS.Internal.Documents;

internal static class DocumentViewerHelper
{
	private static ResourceKey _findToolBarStyleKey;

	private static ResourceKey FindToolBarStyleKey
	{
		get
		{
			if (_findToolBarStyleKey == null)
			{
				_findToolBarStyleKey = new ComponentResourceKey(typeof(PresentationUIStyleResources), "PUIFlowViewers_FindToolBar");
			}
			return _findToolBarStyleKey;
		}
	}

	internal static void ToggleFindToolBar(Decorator findToolBarHost, EventHandler handlerFindClicked, bool enable)
	{
		if (enable)
		{
			FindToolBar findToolBar = (FindToolBar)(findToolBarHost.Child = new FindToolBar());
			findToolBarHost.Visibility = Visibility.Visible;
			KeyboardNavigation.SetTabNavigation(findToolBarHost, KeyboardNavigationMode.Continue);
			FocusManager.SetIsFocusScope(findToolBarHost, value: true);
			findToolBar.SetResourceReference(FrameworkElement.StyleProperty, FindToolBarStyleKey);
			findToolBar.FindClicked += handlerFindClicked;
			findToolBar.DocumentLoaded = true;
			findToolBar.GoToTextBox();
		}
		else
		{
			FindToolBar obj = findToolBarHost.Child as FindToolBar;
			obj.FindClicked -= handlerFindClicked;
			obj.DocumentLoaded = false;
			findToolBarHost.Child = null;
			findToolBarHost.Visibility = Visibility.Collapsed;
			KeyboardNavigation.SetTabNavigation(findToolBarHost, KeyboardNavigationMode.None);
			findToolBarHost.ClearValue(FocusManager.IsFocusScopeProperty);
		}
	}

	internal static ITextRange Find(FindToolBar findToolBar, TextEditor textEditor, ITextView textView, ITextView masterPageTextView)
	{
		ITextPointer textPointer = null;
		ITextRange textRange = null;
		Invariant.Assert(findToolBar != null);
		Invariant.Assert(textEditor != null);
		FindFlags findFlags = FindFlags.None;
		findFlags = (FindFlags)((int)findFlags | (findToolBar.SearchUp ? 2 : 0));
		findFlags = (FindFlags)((int)findFlags | (findToolBar.MatchCase ? 1 : 0));
		findFlags = (FindFlags)((int)findFlags | (findToolBar.MatchWholeWord ? 4 : 0));
		findFlags = (FindFlags)((int)findFlags | (findToolBar.MatchDiacritic ? 8 : 0));
		findFlags = (FindFlags)((int)findFlags | (findToolBar.MatchKashida ? 16 : 0));
		findFlags = (FindFlags)((int)findFlags | (findToolBar.MatchAlefHamza ? 32 : 0));
		ITextContainer textContainer = textEditor.TextContainer;
		ITextRange selection = textEditor.Selection;
		string searchText = findToolBar.SearchText;
		CultureInfo documentCultureInfo = GetDocumentCultureInfo(textContainer);
		ITextPointer textPointer2;
		ITextPointer textPointer3;
		if (selection.IsEmpty)
		{
			if (textView != null && !textView.IsValid)
			{
				textView = null;
			}
			if (textView != null && textView.Contains(selection.Start))
			{
				textPointer2 = (findToolBar.SearchUp ? textContainer.Start : selection.Start);
				textPointer3 = (findToolBar.SearchUp ? selection.Start : textContainer.End);
			}
			else
			{
				if (masterPageTextView != null && masterPageTextView.IsValid)
				{
					foreach (TextSegment textSegment in masterPageTextView.TextSegments)
					{
						if (textSegment.IsNull)
						{
							continue;
						}
						if (textPointer == null)
						{
							textPointer = ((!findToolBar.SearchUp) ? textSegment.Start : textSegment.End);
						}
						else if (!findToolBar.SearchUp)
						{
							if (textSegment.Start.CompareTo(textPointer) < 0)
							{
								textPointer = textSegment.Start;
							}
						}
						else if (textSegment.End.CompareTo(textPointer) > 0)
						{
							textPointer = textSegment.End;
						}
					}
				}
				if (textPointer != null)
				{
					textPointer2 = (findToolBar.SearchUp ? textContainer.Start : textPointer);
					textPointer3 = (findToolBar.SearchUp ? textPointer : textContainer.End);
				}
				else
				{
					textPointer2 = textContainer.Start;
					textPointer3 = textContainer.End;
				}
			}
		}
		else
		{
			textRange = TextFindEngine.Find(selection.Start, selection.End, searchText, findFlags, documentCultureInfo);
			if (textRange != null && textRange.Start != null && textRange.Start.CompareTo(selection.Start) == 0 && textRange.End.CompareTo(selection.End) == 0)
			{
				textPointer2 = (findToolBar.SearchUp ? selection.Start : selection.End);
				textPointer3 = (findToolBar.SearchUp ? textContainer.Start : textContainer.End);
			}
			else
			{
				textPointer2 = (findToolBar.SearchUp ? selection.End : selection.Start);
				textPointer3 = (findToolBar.SearchUp ? textContainer.Start : textContainer.End);
			}
		}
		textRange = null;
		if (textPointer2 != null && textPointer3 != null && textPointer2.CompareTo(textPointer3) != 0)
		{
			if (textPointer2.CompareTo(textPointer3) > 0)
			{
				ITextPointer textPointer4 = textPointer2;
				textPointer2 = textPointer3;
				textPointer3 = textPointer4;
			}
			textRange = TextFindEngine.Find(textPointer2, textPointer3, searchText, findFlags, documentCultureInfo);
			if (textRange != null && !textRange.IsEmpty)
			{
				selection.Select(textRange.Start, textRange.End);
			}
		}
		return textRange;
	}

	private static CultureInfo GetDocumentCultureInfo(ITextContainer textContainer)
	{
		CultureInfo cultureInfo = null;
		if (textContainer.Parent != null)
		{
			XmlLanguage xmlLanguage = (XmlLanguage)textContainer.Parent.GetValue(FrameworkElement.LanguageProperty);
			if (xmlLanguage != null)
			{
				try
				{
					cultureInfo = xmlLanguage.GetSpecificCulture();
				}
				catch (InvalidOperationException)
				{
					cultureInfo = null;
				}
			}
		}
		if (cultureInfo == null)
		{
			cultureInfo = CultureInfo.CurrentCulture;
		}
		return cultureInfo;
	}

	internal static void ShowFindUnsuccessfulMessage(FindToolBar findToolBar)
	{
		string format = (findToolBar.SearchUp ? SR.DocumentViewerSearchUpCompleteLabel : SR.DocumentViewerSearchDownCompleteLabel);
		format = string.Format(CultureInfo.CurrentCulture, format, findToolBar.SearchText);
		MS.Internal.PresentationFramework.SecurityHelper.ShowMessageBoxHelper((PresentationSource.CriticalFromVisual(findToolBar) is HwndSource hwndSource) ? hwndSource.CriticalHandle : IntPtr.Zero, format, SR.DocumentViewerSearchCompleteTitle, MessageBoxButton.OK, MessageBoxImage.Asterisk);
	}

	internal static bool IsLogicalDescendent(DependencyObject child, DependencyObject parent)
	{
		while (child != null)
		{
			if (child == parent)
			{
				return true;
			}
			child = LogicalTreeHelper.GetParent(child);
		}
		return false;
	}

	internal static void KeyDownHelper(KeyEventArgs e, DependencyObject findToolBarHost)
	{
		if (!e.Handled && findToolBarHost != null && (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down) && Keyboard.FocusedElement is DependencyObject dependencyObject && dependencyObject is Visual && VisualTreeHelper.IsAncestorOf(findToolBarHost, dependencyObject))
		{
			FocusNavigationDirection direction = KeyboardNavigation.KeyToTraversalDirection(e.Key);
			DependencyObject dependencyObject2 = KeyboardNavigation.Current.PredictFocusedElement(dependencyObject, direction);
			if (dependencyObject2 != null && dependencyObject2 is IInputElement && VisualTreeHelper.IsAncestorOf(findToolBarHost, dependencyObject))
			{
				((IInputElement)dependencyObject2).Focus();
				e.Handled = true;
			}
		}
	}

	internal static void OnContextMenuOpening(FlowDocument document, Control viewer, ContextMenuEventArgs e)
	{
		ContextMenu contextMenu = null;
		if (e.TargetElement != null)
		{
			contextMenu = e.TargetElement.GetValue(FrameworkElement.ContextMenuProperty) as ContextMenu;
		}
		if (contextMenu == null)
		{
			contextMenu = viewer.ContextMenu;
		}
		if (contextMenu == null)
		{
			return;
		}
		if (document != null && DoubleUtil.LessThan(e.CursorLeft, 0.0))
		{
			ITextContainer textContainer = (ITextContainer)((IServiceProvider)document).GetService(typeof(ITextContainer));
			ITextPointer textPointer = null;
			if (textContainer.TextSelection != null)
			{
				textPointer = (((!textContainer.TextSelection.IsEmpty && textContainer.TextSelection.TextEditor.UiScope.IsFocused) || !(e.TargetElement is TextElement)) ? textContainer.TextSelection.Start.CreatePointer(LogicalDirection.Forward) : ((TextElement)e.TargetElement).ContentStart);
			}
			else if (e.TargetElement is TextElement)
			{
				textPointer = ((TextElement)e.TargetElement).ContentStart;
			}
			ITextView textView = textContainer.TextView;
			if (textPointer != null && textView != null && textView.IsValid && textView.Contains(textPointer))
			{
				Rect rectangleFromTextPosition = textView.GetRectangleFromTextPosition(textPointer);
				if (rectangleFromTextPosition != Rect.Empty)
				{
					rectangleFromTextPosition = CalculateVisibleRect(rectangleFromTextPosition, textView.RenderScope);
					if (rectangleFromTextPosition != Rect.Empty)
					{
						Point point = textView.RenderScope.TransformToAncestor(viewer).Transform(rectangleFromTextPosition.BottomLeft);
						contextMenu.Placement = PlacementMode.Relative;
						contextMenu.PlacementTarget = viewer;
						contextMenu.HorizontalOffset = point.X;
						contextMenu.VerticalOffset = point.Y;
						contextMenu.IsOpen = true;
						e.Handled = true;
					}
				}
			}
		}
		if (!e.Handled)
		{
			contextMenu.ClearValue(ContextMenu.PlacementProperty);
			contextMenu.ClearValue(ContextMenu.PlacementTargetProperty);
			contextMenu.ClearValue(ContextMenu.HorizontalOffsetProperty);
			contextMenu.ClearValue(ContextMenu.VerticalOffsetProperty);
		}
	}

	internal static Rect CalculateVisibleRect(Rect visibleRect, Visual originalVisual)
	{
		Visual visual = VisualTreeHelper.GetParent(originalVisual) as Visual;
		while (visual != null && visibleRect != Rect.Empty)
		{
			if (VisualTreeHelper.GetClip(visual) != null)
			{
				GeneralTransform inverse = originalVisual.TransformToAncestor(visual).Inverse;
				if (inverse != null)
				{
					Rect bounds = VisualTreeHelper.GetClip(visual).Bounds;
					bounds = inverse.TransformBounds(bounds);
					visibleRect.Intersect(bounds);
				}
				else
				{
					visibleRect = Rect.Empty;
				}
			}
			visual = VisualTreeHelper.GetParent(visual) as Visual;
		}
		return visibleRect;
	}
}
