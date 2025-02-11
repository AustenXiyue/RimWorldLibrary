using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Documents;

internal static class TextEditorContextMenu
{
	private class EditorContextMenu : ContextMenu
	{
		private SecurityCriticalDataClass<MS.Win32.UnsafeNativeMethods.ITfCandidateList> _candidateList;

		internal MS.Win32.UnsafeNativeMethods.ITfCandidateList CandidateList
		{
			get
			{
				if (_candidateList == null)
				{
					return null;
				}
				return _candidateList.Value;
			}
		}

		internal void AddMenuItems(TextEditor textEditor)
		{
			if (!textEditor.IsReadOnly && AddReconversionItems(textEditor))
			{
				AddSeparator();
			}
			if (AddSpellerItems(textEditor))
			{
				AddSeparator();
			}
			AddClipboardItems(textEditor);
		}

		~EditorContextMenu()
		{
			ReleaseCandidateList(null);
		}

		protected override void OnClosed(RoutedEventArgs e)
		{
			base.OnClosed(e);
			DelayReleaseCandidateList();
		}

		private void AddSeparator()
		{
			base.Items.Add(new Separator());
		}

		private bool AddSpellerItems(TextEditor textEditor)
		{
			SpellingError spellingErrorAtSelection = textEditor.GetSpellingErrorAtSelection();
			if (spellingErrorAtSelection == null)
			{
				return false;
			}
			bool flag = false;
			MenuItem menuItem;
			foreach (string suggestion in spellingErrorAtSelection.Suggestions)
			{
				menuItem = new EditorMenuItem();
				TextBlock textBlock = new TextBlock();
				textBlock.FontWeight = FontWeights.Bold;
				textBlock.Text = suggestion;
				menuItem.Header = textBlock;
				menuItem.Command = EditingCommands.CorrectSpellingError;
				menuItem.CommandParameter = suggestion;
				base.Items.Add(menuItem);
				menuItem.CommandTarget = textEditor.UiScope;
				flag = true;
			}
			if (!flag)
			{
				menuItem = new EditorMenuItem();
				menuItem.Header = SR.TextBox_ContextMenu_NoSpellingSuggestions;
				menuItem.IsEnabled = false;
				base.Items.Add(menuItem);
			}
			AddSeparator();
			menuItem = new EditorMenuItem();
			menuItem.Header = SR.TextBox_ContextMenu_IgnoreAll;
			menuItem.Command = EditingCommands.IgnoreSpellingError;
			base.Items.Add(menuItem);
			menuItem.CommandTarget = textEditor.UiScope;
			return true;
		}

		private string GetMenuItemDescription(string suggestion)
		{
			if (suggestion.Length == 1)
			{
				if (suggestion[0] == ' ')
				{
					return SR.TextBox_ContextMenu_Description_SBCSSpace;
				}
				if (suggestion[0] == '\u3000')
				{
					return SR.TextBox_ContextMenu_Description_DBCSSpace;
				}
			}
			return null;
		}

		private bool AddReconversionItems(TextEditor textEditor)
		{
			TextStore textStore = textEditor.TextStore;
			if (textStore == null)
			{
				GC.SuppressFinalize(this);
				return false;
			}
			ReleaseCandidateList(null);
			_candidateList = new SecurityCriticalDataClass<MS.Win32.UnsafeNativeMethods.ITfCandidateList>(textStore.GetReconversionCandidateList());
			if (CandidateList == null)
			{
				GC.SuppressFinalize(this);
				return false;
			}
			int nCount = 0;
			CandidateList.GetCandidateNum(out nCount);
			if (nCount > 0)
			{
				for (int i = 0; i < 5 && i < nCount; i++)
				{
					CandidateList.GetCandidate(i, out var candstring);
					candstring.GetString(out var funcName);
					MenuItem menuItem = new ReconversionMenuItem(this, i);
					menuItem.Header = funcName;
					menuItem.InputGestureText = GetMenuItemDescription(funcName);
					base.Items.Add(menuItem);
					Marshal.ReleaseComObject(candstring);
				}
			}
			if (nCount > 5)
			{
				MenuItem menuItem = new EditorMenuItem();
				menuItem.Header = SR.TextBox_ContextMenu_More;
				menuItem.Command = ApplicationCommands.CorrectionList;
				base.Items.Add(menuItem);
				menuItem.CommandTarget = textEditor.UiScope;
			}
			return nCount > 0;
		}

		private bool AddClipboardItems(TextEditor textEditor)
		{
			MenuItem menuItem = new EditorMenuItem();
			menuItem.Header = SR.TextBox_ContextMenu_Cut;
			menuItem.CommandTarget = textEditor.UiScope;
			menuItem.Command = ApplicationCommands.Cut;
			base.Items.Add(menuItem);
			menuItem = new EditorMenuItem();
			menuItem.Header = SR.TextBox_ContextMenu_Copy;
			menuItem.CommandTarget = textEditor.UiScope;
			menuItem.Command = ApplicationCommands.Copy;
			base.Items.Add(menuItem);
			menuItem = new EditorMenuItem();
			menuItem.Header = SR.TextBox_ContextMenu_Paste;
			menuItem.CommandTarget = textEditor.UiScope;
			menuItem.Command = ApplicationCommands.Paste;
			base.Items.Add(menuItem);
			return true;
		}

		private void DelayReleaseCandidateList()
		{
			if (CandidateList != null)
			{
				Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ReleaseCandidateList), null);
			}
		}

		private object ReleaseCandidateList(object o)
		{
			if (CandidateList != null)
			{
				Marshal.ReleaseComObject(CandidateList);
				_candidateList = null;
				GC.SuppressFinalize(this);
			}
			return null;
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

	private class ReconversionMenuItem : EditorMenuItem
	{
		private int _index;

		private EditorContextMenu _menu;

		internal ReconversionMenuItem(EditorContextMenu menu, int index)
		{
			_menu = menu;
			_index = index;
		}

		internal override void OnClickCore(bool userInitiated)
		{
			Invariant.Assert(_menu.CandidateList != null);
			try
			{
				_menu.CandidateList.SetResult(_index, MS.Win32.UnsafeNativeMethods.TfCandidateResult.CAND_FINALIZED);
			}
			catch (COMException)
			{
			}
			base.OnClickCore(userInitiated: false);
		}
	}

	internal static void _RegisterClassHandlers(Type controlType, bool registerEventListeners)
	{
		if (registerEventListeners)
		{
			EventManager.RegisterClassHandler(controlType, FrameworkElement.ContextMenuOpeningEvent, new ContextMenuEventHandler(OnContextMenuOpening));
		}
	}

	internal static void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor == null || textEditor.TextView == null)
		{
			return;
		}
		Point position = Mouse.GetPosition(textEditor.TextView.RenderScope);
		ContextMenu contextMenu = null;
		bool flag = false;
		if (textEditor.IsReadOnly)
		{
			if ((e.CursorLeft != -1.0 && !textEditor.Selection.Contains(position)) || (e.CursorLeft == -1.0 && textEditor.Selection.IsEmpty))
			{
				return;
			}
		}
		else if ((textEditor.Selection.IsEmpty || e.TargetElement is TextElement) && e.TargetElement != null)
		{
			contextMenu = (ContextMenu)e.TargetElement.GetValue(FrameworkElement.ContextMenuProperty);
		}
		else if (e.CursorLeft == -1.0 && GetContentPosition(textEditor.Selection.Start) is TextPointer textPointer)
		{
			for (TextElement textElement = textPointer.Parent as TextElement; textElement != null; textElement = textElement.Parent as TextElement)
			{
				contextMenu = (ContextMenu)textElement.GetValue(FrameworkElement.ContextMenuProperty);
				if (contextMenu != null)
				{
					flag = true;
					break;
				}
			}
		}
		if (e.CursorLeft != -1.0)
		{
			if (!TextEditorMouse.IsPointWithinInteractiveArea(textEditor, Mouse.GetPosition(textEditor.UiScope)))
			{
				return;
			}
			if (contextMenu == null || !(e.TargetElement is UIElement))
			{
				using (textEditor.Selection.DeclareChangeBlock())
				{
					if (!textEditor.Selection.Contains(position))
					{
						TextEditorMouse.SetCaretPositionOnMouseEvent(textEditor, position, MouseButton.Right, 1);
					}
				}
			}
		}
		if (contextMenu == null)
		{
			if (textEditor.UiScope.ReadLocalValue(FrameworkElement.ContextMenuProperty) == null)
			{
				return;
			}
			contextMenu = textEditor.UiScope.ContextMenu;
		}
		textEditor.IsContextMenuOpen = true;
		if (contextMenu != null && !flag)
		{
			contextMenu.HorizontalOffset = 0.0;
			contextMenu.VerticalOffset = 0.0;
			contextMenu.Closed += OnContextMenuClosed;
			return;
		}
		textEditor.CompleteComposition();
		if (contextMenu == null)
		{
			contextMenu = new EditorContextMenu();
			((EditorContextMenu)contextMenu).AddMenuItems(textEditor);
		}
		contextMenu.Placement = PlacementMode.RelativePoint;
		contextMenu.PlacementTarget = textEditor.UiScope;
		ITextPointer textPointer2 = null;
		SpellingError spellingError = ((contextMenu is EditorContextMenu) ? textEditor.GetSpellingErrorAtSelection() : null);
		LogicalDirection logicalDirection;
		if (spellingError != null)
		{
			textPointer2 = spellingError.End;
			logicalDirection = LogicalDirection.Backward;
		}
		else if (e.CursorLeft == -1.0)
		{
			textPointer2 = textEditor.Selection.Start;
			logicalDirection = LogicalDirection.Forward;
		}
		else
		{
			logicalDirection = LogicalDirection.Forward;
		}
		if (textPointer2 != null && textPointer2.CreatePointer(logicalDirection).HasValidLayout)
		{
			GetClippedPositionOffsets(textEditor, textPointer2, logicalDirection, out var horizontalOffset, out var verticalOffset);
			contextMenu.HorizontalOffset = horizontalOffset;
			contextMenu.VerticalOffset = verticalOffset;
		}
		else
		{
			Point position2 = Mouse.GetPosition(textEditor.UiScope);
			contextMenu.HorizontalOffset = position2.X;
			contextMenu.VerticalOffset = position2.Y;
		}
		contextMenu.Closed += OnContextMenuClosed;
		contextMenu.IsOpen = true;
		e.Handled = true;
	}

	private static void OnContextMenuClosed(object sender, RoutedEventArgs e)
	{
		UIElement placementTarget = ((ContextMenu)sender).PlacementTarget;
		if (placementTarget != null)
		{
			TextEditor textEditor = TextEditor._GetTextEditor(placementTarget);
			if (textEditor != null)
			{
				textEditor.IsContextMenuOpen = false;
				textEditor.Selection.UpdateCaretAndHighlight();
				((ContextMenu)sender).Closed -= OnContextMenuClosed;
			}
		}
	}

	private static void GetClippedPositionOffsets(TextEditor This, ITextPointer position, LogicalDirection direction, out double horizontalOffset, out double verticalOffset)
	{
		Rect characterRect = position.GetCharacterRect(direction);
		horizontalOffset = characterRect.X;
		verticalOffset = characterRect.Y + characterRect.Height;
		if (This.TextView.RenderScope is FrameworkElement frameworkElement)
		{
			GeneralTransform generalTransform = frameworkElement.TransformToAncestor(This.UiScope);
			if (generalTransform != null)
			{
				ClipToElement(frameworkElement, generalTransform, ref horizontalOffset, ref verticalOffset);
			}
		}
		for (Visual visual = This.UiScope; visual != null; visual = VisualTreeHelper.GetParent(visual) as Visual)
		{
			if (visual is FrameworkElement element)
			{
				GeneralTransform generalTransform2 = visual.TransformToDescendant(This.UiScope);
				if (generalTransform2 != null)
				{
					ClipToElement(element, generalTransform2, ref horizontalOffset, ref verticalOffset);
				}
			}
		}
		PresentationSource presentationSource = PresentationSource.CriticalFromVisual(This.UiScope);
		if (presentationSource is IWin32Window win32Window)
		{
			nint zero = IntPtr.Zero;
			zero = win32Window.Handle;
			MS.Win32.NativeMethods.RECT rect = new MS.Win32.NativeMethods.RECT(0, 0, 0, 0);
			SafeNativeMethods.GetClientRect(new HandleRef(null, zero), ref rect);
			Point point = new Point(rect.left, rect.top);
			Point point2 = new Point(rect.right, rect.bottom);
			CompositionTarget compositionTarget = presentationSource.CompositionTarget;
			point = compositionTarget.TransformFromDevice.Transform(point);
			point2 = compositionTarget.TransformFromDevice.Transform(point2);
			GeneralTransform generalTransform3 = compositionTarget.RootVisual.TransformToDescendant(This.UiScope);
			if (generalTransform3 != null)
			{
				generalTransform3.TryTransform(point, out point);
				generalTransform3.TryTransform(point2, out point2);
				horizontalOffset = ClipToBounds(point.X, horizontalOffset, point2.X);
				verticalOffset = ClipToBounds(point.Y, verticalOffset, point2.Y);
			}
		}
	}

	private static void ClipToElement(FrameworkElement element, GeneralTransform transform, ref double horizontalOffset, ref double verticalOffset)
	{
		Geometry clip = VisualTreeHelper.GetClip(element);
		Point result;
		Point result2;
		if (clip != null)
		{
			Rect bounds = clip.Bounds;
			result = new Point(bounds.X, bounds.Y);
			result2 = new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height);
		}
		else
		{
			if (element.ActualWidth == 0.0 && element.ActualHeight == 0.0)
			{
				return;
			}
			result = new Point(0.0, 0.0);
			result2 = new Point(element.ActualWidth, element.ActualHeight);
		}
		transform.TryTransform(result, out result);
		transform.TryTransform(result2, out result2);
		horizontalOffset = ClipToBounds(result.X, horizontalOffset, result2.X);
		verticalOffset = ClipToBounds(result.Y, verticalOffset, result2.Y);
	}

	private static double ClipToBounds(double min, double value, double max)
	{
		if (min > max)
		{
			double num = min;
			min = max;
			max = num;
		}
		if (value < min)
		{
			value = min;
		}
		else if (value >= max)
		{
			value = max - 1.0;
		}
		return value;
	}

	private static ITextPointer GetContentPosition(ITextPointer position)
	{
		while (position.GetAdjacentElement(LogicalDirection.Forward) is Inline)
		{
			position = position.GetNextContextPosition(LogicalDirection.Forward);
		}
		return position;
	}
}
