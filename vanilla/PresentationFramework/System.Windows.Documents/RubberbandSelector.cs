using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using MS.Internal;
using MS.Internal.Documents;

namespace System.Windows.Documents;

internal sealed class RubberbandSelector
{
	private class TextPositionPair
	{
		public ITextPointer first;

		public ITextPointer second;
	}

	private FixedDocument _panel;

	private FixedPage _page;

	private Rect _selectionRect;

	private bool _isSelecting;

	private Point _origin;

	private UIElement _scope;

	private FrameworkElement _uiScope;

	private int _pageIndex;

	internal FixedPage Page => _page;

	internal Rect SelectionRect => _selectionRect;

	internal bool HasSelection
	{
		get
		{
			if (_page != null && _panel != null)
			{
				return !_selectionRect.IsEmpty;
			}
			return false;
		}
	}

	internal void ClearSelection()
	{
		if (HasSelection)
		{
			FixedPage page = _page;
			_page = null;
			UpdateHighlightVisual(page);
		}
		_selectionRect = Rect.Empty;
	}

	internal void AttachRubberbandSelector(FrameworkElement scope)
	{
		if (scope == null)
		{
			throw new ArgumentNullException("scope");
		}
		ClearSelection();
		scope.MouseLeftButtonDown += OnLeftMouseDown;
		scope.MouseLeftButtonUp += OnLeftMouseUp;
		scope.MouseMove += OnMouseMove;
		scope.QueryCursor += OnQueryCursor;
		scope.Cursor = null;
		if (scope is DocumentGrid)
		{
			_uiScope = ((DocumentGrid)scope).DocumentViewerOwner;
			Invariant.Assert(_uiScope != null, "DocumentGrid's DocumentViewerOwner cannot be null.");
		}
		else
		{
			_uiScope = scope;
		}
		CommandBinding commandBinding = new CommandBinding(ApplicationCommands.Copy);
		commandBinding.Executed += OnCopy;
		commandBinding.CanExecute += QueryCopy;
		_uiScope.CommandBindings.Add(commandBinding);
		_scope = scope;
	}

	internal void DetachRubberbandSelector()
	{
		ClearSelection();
		if (_scope != null)
		{
			_scope.MouseLeftButtonDown -= OnLeftMouseDown;
			_scope.MouseLeftButtonUp -= OnLeftMouseUp;
			_scope.MouseMove -= OnMouseMove;
			_scope.QueryCursor -= OnQueryCursor;
			_scope = null;
		}
		if (_uiScope == null)
		{
			return;
		}
		foreach (CommandBinding commandBinding in _uiScope.CommandBindings)
		{
			if (commandBinding.Command == ApplicationCommands.Copy)
			{
				commandBinding.Executed -= OnCopy;
				commandBinding.CanExecute -= QueryCopy;
			}
		}
		_uiScope = null;
	}

	private void ExtendSelection(Point pt)
	{
		Size size = _panel.ComputePageSize(_page);
		if (pt.X < 0.0)
		{
			pt.X = 0.0;
		}
		else if (pt.X > size.Width)
		{
			pt.X = size.Width;
		}
		if (pt.Y < 0.0)
		{
			pt.Y = 0.0;
		}
		else if (pt.Y > size.Height)
		{
			pt.Y = size.Height;
		}
		_selectionRect = new Rect(_origin, pt);
		UpdateHighlightVisual(_page);
	}

	private void UpdateHighlightVisual(FixedPage page)
	{
		if (page != null)
		{
			HighlightVisual.GetHighlightVisual(page)?.UpdateRubberbandSelection(this);
		}
	}

	private void OnCopy(object sender, ExecutedRoutedEventArgs e)
	{
		if (HasSelection && _selectionRect.Width > 0.0 && _selectionRect.Height > 0.0)
		{
			string text = GetText();
			object obj = null;
			obj = SystemDrawingHelper.GetBitmapFromBitmapSource(GetImage());
			IDataObject dataObject = new DataObject();
			dataObject.SetData(DataFormats.Text, text, autoConvert: true);
			dataObject.SetData(DataFormats.UnicodeText, text, autoConvert: true);
			if (obj != null)
			{
				dataObject.SetData(DataFormats.Bitmap, obj, autoConvert: true);
			}
			try
			{
				Clipboard.SetDataObject(dataObject, copy: true);
			}
			catch (ExternalException)
			{
			}
		}
	}

	private BitmapSource GetImage()
	{
		Visual visual = GetVisual(0.0 - _selectionRect.Left, 0.0 - _selectionRect.Top);
		double num = 96.0;
		double num2 = num / 96.0;
		RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)(num2 * _selectionRect.Width), (int)(num2 * _selectionRect.Height), num, num, PixelFormats.Pbgra32);
		renderTargetBitmap.Render(visual);
		return renderTargetBitmap;
	}

	private Visual GetVisual(double offsetX, double offsetY)
	{
		ContainerVisual containerVisual = new ContainerVisual();
		DrawingVisual drawingVisual = new DrawingVisual();
		containerVisual.Children.Add(drawingVisual);
		drawingVisual.Offset = new Vector(offsetX, offsetY);
		DrawingContext drawingContext = drawingVisual.RenderOpen();
		drawingContext.DrawDrawing(_page.GetDrawing());
		drawingContext.Close();
		foreach (UIElement child in _page.Children)
		{
			CloneVisualTree(drawingVisual, child);
		}
		return containerVisual;
	}

	private void CloneVisualTree(ContainerVisual parent, Visual old)
	{
		DrawingVisual drawingVisual = new DrawingVisual();
		parent.Children.Add(drawingVisual);
		drawingVisual.Clip = VisualTreeHelper.GetClip(old);
		drawingVisual.Offset = VisualTreeHelper.GetOffset(old);
		drawingVisual.Transform = VisualTreeHelper.GetTransform(old);
		drawingVisual.Opacity = VisualTreeHelper.GetOpacity(old);
		drawingVisual.OpacityMask = VisualTreeHelper.GetOpacityMask(old);
		drawingVisual.BitmapEffectInput = VisualTreeHelper.GetBitmapEffectInput(old);
		drawingVisual.BitmapEffect = VisualTreeHelper.GetBitmapEffect(old);
		DrawingContext drawingContext = drawingVisual.RenderOpen();
		drawingContext.DrawDrawing(old.GetDrawing());
		drawingContext.Close();
		int childrenCount = VisualTreeHelper.GetChildrenCount(old);
		for (int i = 0; i < childrenCount; i++)
		{
			Visual old2 = old.InternalGetVisualChild(i);
			CloneVisualTree(drawingVisual, old2);
		}
	}

	private string GetText()
	{
		double top = _selectionRect.Top;
		double bottom = _selectionRect.Bottom;
		double left = _selectionRect.Left;
		double right = _selectionRect.Right;
		double baseline = 0.0;
		double height = 0.0;
		_ = _page.Children.Count;
		ArrayList arrayList = new ArrayList();
		FixedNode[] array = _panel.FixedContainer.FixedTextBuilder.GetFirstLine(_pageIndex);
		while (array != null && array.Length != 0)
		{
			TextPositionPair textPositionPair = null;
			FixedNode[] array2 = array;
			foreach (FixedNode node in array2)
			{
				Glyphs glyphsElement = _page.GetGlyphsElement(node);
				if (glyphsElement == null)
				{
					continue;
				}
				if (IntersectGlyphs(glyphsElement, top, left, bottom, right, out var begin, out var end, out var includeEnd, out baseline, out height))
				{
					if (textPositionPair == null || begin > 0)
					{
						textPositionPair = new TextPositionPair();
						textPositionPair.first = _GetTextPosition(node, begin);
						arrayList.Add(textPositionPair);
					}
					textPositionPair.second = _GetTextPosition(node, end);
					if (!includeEnd)
					{
						textPositionPair = null;
					}
				}
				else
				{
					textPositionPair = null;
				}
			}
			int count = 1;
			array = _panel.FixedContainer.FixedTextBuilder.GetNextLine(array[0], forward: true, ref count);
		}
		string text = "";
		foreach (TextPositionPair item in arrayList)
		{
			text = text + TextRangeBase.GetTextInternal(item.first, item.second) + "\r\n";
		}
		return text;
	}

	private ITextPointer _GetTextPosition(FixedNode node, int charIndex)
	{
		FixedPosition fixedPosition = new FixedPosition(node, charIndex);
		FlowPosition flowPosition = _panel.FixedContainer.FixedTextBuilder.CreateFlowPosition(fixedPosition);
		if (flowPosition != null)
		{
			return new FixedTextPointer(mutable: false, LogicalDirection.Forward, flowPosition);
		}
		return null;
	}

	private bool IntersectGlyphs(Glyphs g, double top, double left, double bottom, double right, out int begin, out int end, out bool includeEnd, out double baseline, out double height)
	{
		begin = 0;
		end = 0;
		includeEnd = false;
		GlyphRun glyphRun = g.ToGlyphRun();
		Rect rect = glyphRun.ComputeAlignmentBox();
		rect.Offset(glyphRun.BaselineOrigin.X, glyphRun.BaselineOrigin.Y);
		baseline = glyphRun.BaselineOrigin.Y;
		height = rect.Height;
		double y = rect.Y + 0.5 * rect.Height;
		GeneralTransform generalTransform = g.TransformToAncestor(_page);
		generalTransform.TryTransform(new Point(rect.Left, y), out var result);
		generalTransform.TryTransform(new Point(rect.Right, y), out var result2);
		bool flag = false;
		if (result.X < left)
		{
			if (result2.X < left)
			{
				return false;
			}
			flag = true;
		}
		else if (result.X > right)
		{
			if (result2.X > right)
			{
				return false;
			}
			flag = true;
		}
		else if (result2.X < left || result2.X > right)
		{
			flag = true;
		}
		double num3;
		double num4;
		if (flag)
		{
			double num = (left - result.X) / (result2.X - result.X);
			double num2 = (right - result.X) / (result2.X - result.X);
			if (num2 > num)
			{
				num3 = num;
				num4 = num2;
			}
			else
			{
				num3 = num2;
				num4 = num;
			}
		}
		else
		{
			num3 = 0.0;
			num4 = 1.0;
		}
		flag = false;
		if (result.Y < top)
		{
			if (result2.Y < top)
			{
				return false;
			}
			flag = true;
		}
		else if (result.Y > bottom)
		{
			if (result2.Y > bottom)
			{
				return false;
			}
			flag = true;
		}
		else if (result2.Y < top || result2.Y > bottom)
		{
			flag = true;
		}
		if (flag)
		{
			double num5 = (top - result.Y) / (result2.Y - result.Y);
			double num6 = (bottom - result.Y) / (result2.Y - result.Y);
			if (num6 > num5)
			{
				if (num5 > num3)
				{
					num3 = num5;
				}
				if (num6 < num4)
				{
					num4 = num6;
				}
			}
			else
			{
				if (num6 > num3)
				{
					num3 = num6;
				}
				if (num5 < num4)
				{
					num4 = num5;
				}
			}
		}
		num3 = rect.Left + rect.Width * num3;
		num4 = rect.Left + rect.Width * num4;
		bool lTR = (glyphRun.BidiLevel & 1) == 0;
		begin = GlyphRunHitTest(glyphRun, num3, lTR);
		end = GlyphRunHitTest(glyphRun, num4, lTR);
		if (begin > end)
		{
			int num7 = begin;
			begin = end;
			end = num7;
		}
		int num8 = ((glyphRun.Characters != null) ? glyphRun.Characters.Count : 0);
		includeEnd = end == num8;
		return true;
	}

	private int GlyphRunHitTest(GlyphRun run, double xoffset, bool LTR)
	{
		double distance = (LTR ? (xoffset - run.BaselineOrigin.X) : (run.BaselineOrigin.X - xoffset));
		bool isInside;
		CharacterHit caretCharacterHitFromDistance = run.GetCaretCharacterHitFromDistance(distance, out isInside);
		return caretCharacterHitFromDistance.FirstCharacterIndex + caretCharacterHitFromDistance.TrailingLength;
	}

	private void QueryCopy(object sender, CanExecuteRoutedEventArgs e)
	{
		if (HasSelection)
		{
			e.CanExecute = true;
		}
	}

	private void OnLeftMouseDown(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
		FixedDocumentPage fixedPanelDocumentPage = GetFixedPanelDocumentPage(e.GetPosition(_scope));
		if (fixedPanelDocumentPage != null)
		{
			_uiScope.Focus();
			_scope.CaptureMouse();
			ClearSelection();
			_panel = fixedPanelDocumentPage.Owner;
			_page = fixedPanelDocumentPage.FixedPage;
			_isSelecting = true;
			_origin = e.GetPosition(_page);
			_pageIndex = fixedPanelDocumentPage.PageIndex;
		}
	}

	private void OnLeftMouseUp(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
		_scope.ReleaseMouseCapture();
		if (_isSelecting)
		{
			_isSelecting = false;
			if (_page != null)
			{
				ExtendSelection(e.GetPosition(_page));
			}
		}
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		e.Handled = true;
		if (e.LeftButton == MouseButtonState.Released)
		{
			_isSelecting = false;
		}
		else if (_isSelecting && _page != null)
		{
			ExtendSelection(e.GetPosition(_page));
		}
	}

	private void OnQueryCursor(object sender, QueryCursorEventArgs e)
	{
		if (_isSelecting || GetFixedPanelDocumentPage(e.GetPosition(_scope)) != null)
		{
			e.Cursor = Cursors.Cross;
		}
		else
		{
			e.Cursor = Cursors.Arrow;
		}
		e.Handled = true;
	}

	private FixedDocumentPage GetFixedPanelDocumentPage(Point pt)
	{
		if (_scope is DocumentGrid documentGrid)
		{
			DocumentPage documentPageFromPoint = documentGrid.GetDocumentPageFromPoint(pt);
			FixedDocumentPage fixedDocumentPage = documentPageFromPoint as FixedDocumentPage;
			if (fixedDocumentPage == null && documentPageFromPoint is FixedDocumentSequenceDocumentPage fixedDocumentSequenceDocumentPage)
			{
				fixedDocumentPage = fixedDocumentSequenceDocumentPage.ChildDocumentPage as FixedDocumentPage;
			}
			return fixedDocumentPage;
		}
		return null;
	}
}
