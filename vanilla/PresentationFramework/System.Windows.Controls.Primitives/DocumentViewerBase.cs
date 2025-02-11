using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Printing;
using System.Windows.Annotations;
using System.Windows.Automation.Peers;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Xps;
using MS.Internal;
using MS.Internal.Annotations.Anchoring;
using MS.Internal.Commands;
using MS.Internal.Controls;
using MS.Internal.Documents;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls.Primitives;

/// <summary>Provides a base class for viewers that are intended to display fixed or flow content (represented by a <see cref="T:System.Windows.Documents.FixedDocument" /> or <see cref="T:System.Windows.Documents.FlowDocument" />, respectively).</summary>
[ContentProperty("Document")]
public abstract class DocumentViewerBase : Control, IAddChild, IServiceProvider
{
	[Flags]
	private enum Flags
	{
		IsSelectionEnabled = 0x20,
		DocumentAsLogicalChild = 0x40
	}

	private class BringIntoViewState
	{
		internal DocumentViewerBase Source;

		internal ContentPosition ContentPosition;

		internal DependencyObject TargetObject;

		internal Rect TargetRect;

		internal BringIntoViewState(DocumentViewerBase source, ContentPosition contentPosition, DependencyObject targetObject, Rect targetRect)
		{
			Source = source;
			ContentPosition = contentPosition;
			TargetObject = targetObject;
			TargetRect = targetRect;
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.Document" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.Document" /> dependency property.</returns>
	public static readonly DependencyProperty DocumentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.PageCount" /> dependency property key.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.PageCount" /> dependency property key.</returns>
	protected static readonly DependencyPropertyKey PageCountPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.PageCount" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.PageCount" /> dependency property.</returns>
	public static readonly DependencyProperty PageCountProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.MasterPageNumber" /> dependency property key.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.MasterPageNumber" /> dependency property key.</returns>
	protected static readonly DependencyPropertyKey MasterPageNumberPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.MasterPageNumber" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.MasterPageNumber" /> dependency property.</returns>
	public static readonly DependencyProperty MasterPageNumberProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.CanGoToPreviousPage" /> dependency property key.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.CanGoToPreviousPage" /> dependency property key.</returns>
	protected static readonly DependencyPropertyKey CanGoToPreviousPagePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.CanGoToPreviousPage" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.CanGoToPreviousPage" /> dependency property.</returns>
	public static readonly DependencyProperty CanGoToPreviousPageProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.CanGoToNextPage" /> dependency property key.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.CanGoToNextPage" /> dependency property key.</returns>
	protected static readonly DependencyPropertyKey CanGoToNextPagePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.CanGoToNextPage" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.CanGoToNextPage" /> dependency property.</returns>
	public static readonly DependencyProperty CanGoToNextPageProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.IsMasterPage" /> attached property</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.IsMasterPage" /> attached property.</returns>
	public static readonly DependencyProperty IsMasterPageProperty;

	private ReadOnlyCollection<DocumentPageView> _pageViews;

	private FrameworkElement _textEditorRenderScope;

	private MultiPageTextView _textView;

	private TextEditor _textEditor;

	private IDocumentPaginatorSource _document;

	private Flags _flags;

	private XpsDocumentWriter _documentWriter;

	private static bool IsEditingEnabled;

	/// <summary>Gets or sets a <see cref="T:System.Windows.Documents.IDocumentPaginatorSource" /> to be paginated and displayed by the viewer. </summary>
	/// <returns>A <see cref="T:System.Windows.Documents.IDocumentPaginatorSource" /> to be paginated and displayed by the viewer.The default property is null.</returns>
	public IDocumentPaginatorSource Document
	{
		get
		{
			return _document;
		}
		set
		{
			SetValue(DocumentProperty, value);
		}
	}

	/// <summary>Gets the total number of pages in the current <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.Document" />. </summary>
	/// <returns>The number of pages in the current document, or 0 if no document is currently loaded.This property has no default value.</returns>
	public int PageCount => (int)GetValue(PageCountProperty);

	/// <summary>Gets the page number for the current master page. </summary>
	/// <returns>The page number for the current master page, or 0 if no Document is currently loaded.This property has no default value.</returns>
	public virtual int MasterPageNumber => (int)GetValue(MasterPageNumberProperty);

	/// <summary>Gets a value that indicates whether or not the viewer can jump to the previous page in the current <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.Document" />. </summary>
	/// <returns>true if the viewer can jump to the previous page; otherwise, false.This property has no default value.</returns>
	public virtual bool CanGoToPreviousPage => (bool)GetValue(CanGoToPreviousPageProperty);

	/// <summary>Gets a value that indicates whether or not the viewer can jump to the next page in the current <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.Document" />. </summary>
	/// <returns>true if the viewer can jump to the next page; otherwise, false.This property has no default value.</returns>
	public virtual bool CanGoToNextPage => (bool)GetValue(CanGoToNextPageProperty);

	/// <summary>Gets a read-only collection of the active <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> objects contained within the viewer.</summary>
	/// <returns>A read-only collection of the active <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> objects contained within the viewer.This property has no default value.</returns>
	[CLSCompliant(false)]
	public ReadOnlyCollection<DocumentPageView> PageViews => _pageViews;

	/// <summary>Gets an enumerator for the children in the logical tree of the viewer.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to enumerate the logical children of the viewer.This property has no default value.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			if (base.HasLogicalChildren && _document != null)
			{
				return new SingleChildEnumerator(_document);
			}
			return EmptyEnumerator.Instance;
		}
	}

	internal bool IsSelectionEnabled
	{
		get
		{
			return CheckFlags(Flags.IsSelectionEnabled);
		}
		set
		{
			SetFlags(value, Flags.IsSelectionEnabled);
			AttachTextEditor();
		}
	}

	internal TextEditor TextEditor => _textEditor;

	internal FrameworkElement TextEditorRenderScope
	{
		get
		{
			return _textEditorRenderScope;
		}
		set
		{
			_textEditorRenderScope = value;
			AttachTextEditor();
		}
	}

	private ITextContainer TextContainer
	{
		get
		{
			ITextContainer result = null;
			if (_document != null && _document is IServiceProvider && CheckFlags(Flags.IsSelectionEnabled))
			{
				result = ((IServiceProvider)_document).GetService(typeof(ITextContainer)) as ITextContainer;
			}
			return result;
		}
	}

	/// <summary>Occurs when the collection of <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> items associated with this viewer (represented by the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.PageViews" /> property) changes.</summary>
	public event EventHandler PageViewsChanged;

	static DocumentViewerBase()
	{
		DocumentProperty = DependencyProperty.Register("Document", typeof(IDocumentPaginatorSource), typeof(DocumentViewerBase), new FrameworkPropertyMetadata(null, DocumentChanged));
		PageCountPropertyKey = DependencyProperty.RegisterReadOnly("PageCount", typeof(int), typeof(DocumentViewerBase), new FrameworkPropertyMetadata(0));
		PageCountProperty = PageCountPropertyKey.DependencyProperty;
		MasterPageNumberPropertyKey = DependencyProperty.RegisterReadOnly("MasterPageNumber", typeof(int), typeof(DocumentViewerBase), new FrameworkPropertyMetadata(0));
		MasterPageNumberProperty = MasterPageNumberPropertyKey.DependencyProperty;
		CanGoToPreviousPagePropertyKey = DependencyProperty.RegisterReadOnly("CanGoToPreviousPage", typeof(bool), typeof(DocumentViewerBase), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		CanGoToPreviousPageProperty = CanGoToPreviousPagePropertyKey.DependencyProperty;
		CanGoToNextPagePropertyKey = DependencyProperty.RegisterReadOnly("CanGoToNextPage", typeof(bool), typeof(DocumentViewerBase), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		CanGoToNextPageProperty = CanGoToNextPagePropertyKey.DependencyProperty;
		IsMasterPageProperty = DependencyProperty.RegisterAttached("IsMasterPage", typeof(bool), typeof(DocumentViewerBase), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsEditingEnabled = false;
		CreateCommandBindings();
		EventManager.RegisterClassHandler(typeof(DocumentViewerBase), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(HandleRequestBringIntoView));
		TextBoxBase.AutoWordSelectionProperty.OverrideMetadata(typeof(DocumentViewerBase), new FrameworkPropertyMetadata(true));
	}

	/// <summary>Initializes base class values when called by a derived class.</summary>
	protected DocumentViewerBase()
	{
		_pageViews = new ReadOnlyCollection<DocumentPageView>(new List<DocumentPageView>());
		SetFlags(value: true, Flags.IsSelectionEnabled);
	}

	/// <summary>Builds the visual tree for the viewer.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		UpdatePageViews();
	}

	/// <summary>Causes the viewer to jump to the previous page of the current document (represented by the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.Document" /> property).</summary>
	public void PreviousPage()
	{
		OnPreviousPageCommand();
	}

	/// <summary>Causes the viewer to jump to the next page in the current document (represented by the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.Document" /> property).</summary>
	public void NextPage()
	{
		OnNextPageCommand();
	}

	/// <summary>Causes the viewer to jump to the first page of the current document (represented by the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.Document" /> property).</summary>
	public void FirstPage()
	{
		OnFirstPageCommand();
	}

	/// <summary>Causes the viewer to jump to the last page in the current document (represented by the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.Document" /> property).</summary>
	public void LastPage()
	{
		OnLastPageCommand();
	}

	/// <summary>Causes the viewer to jump to a specified page number.</summary>
	/// <param name="pageNumber">The number of the page to jump to.</param>
	public void GoToPage(int pageNumber)
	{
		OnGoToPageCommand(pageNumber);
	}

	/// <summary>Invokes a standard Print dialog which can be used to print the contents of the viewer and configure printing preferences.</summary>
	public void Print()
	{
		OnPrintCommand();
	}

	/// <summary>Cancels any current printing job.</summary>
	public void CancelPrint()
	{
		OnCancelPrintCommand();
	}

	/// <summary>Returns a value that indicates whether or the viewer is able to jump to the specified page number.</summary>
	/// <returns>A Boolean value that indicates whether or the viewer is able to jump to the specified page number.</returns>
	/// <param name="pageNumber">A page number to check for as a valid jump target.</param>
	public virtual bool CanGoToPage(int pageNumber)
	{
		if (pageNumber <= 0 || pageNumber > PageCount)
		{
			if (_document != null && pageNumber - 1 == PageCount)
			{
				return !_document.DocumentPaginator.IsPageCountValid;
			}
			return false;
		}
		return true;
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.IsMasterPage" /> attached property for a specified dependency object.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.IsMasterPage" /> attached property, read from the dependency object specified by element.</returns>
	/// <param name="element">A dependency object from which to retrieve the value of <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.IsMasterPage" />.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised if <paramref name="element" /> is null.</exception>
	public static bool GetIsMasterPage(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsMasterPageProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.IsMasterPage" /> attached property on a specified dependency object.</summary>
	/// <param name="element">A dependency object on which to set the value of <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.IsMasterPage" />.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised if element is null.</exception>
	public static void SetIsMasterPage(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsMasterPageProperty, value);
	}

	/// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this viewer.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this viewer.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new DocumentViewerBaseAutomationPeer(this);
	}

	protected override void OnDpiChanged(DpiScale oldDpiScaleInfo, DpiScale newDpiScaleInfo)
	{
		(_document as FlowDocument)?.SetDpi(newDpiScaleInfo);
	}

	/// <summary>Causes the working <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.PageViews" /> collection to be re-built.</summary>
	protected void InvalidatePageViews()
	{
		UpdatePageViews();
		InvalidateMeasure();
	}

	/// <summary>Returns the current master <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> for this viewer.</summary>
	/// <returns>The current master <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> for this viewer, or null if no master <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> can be found.</returns>
	protected DocumentPageView GetMasterPageView()
	{
		DocumentPageView documentPageView = null;
		for (int i = 0; i < _pageViews.Count; i++)
		{
			if (GetIsMasterPage(_pageViews[i]))
			{
				documentPageView = _pageViews[i];
				break;
			}
		}
		if (documentPageView == null)
		{
			documentPageView = ((_pageViews.Count > 0) ? _pageViews[0] : null);
		}
		return documentPageView;
	}

	/// <summary>Creates and returns a new, read-only collection of <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> objects that are associated with the current display document (represented by the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.Document" /> property).</summary>
	/// <returns>A read-only collection of <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> objects that are associated with the current display document.</returns>
	/// <param name="changed">Returns true on the first call to <see cref="M:System.Windows.Controls.Primitives.DocumentViewerBase.GetPageViewsCollection(System.Boolean@)" /> or if the collection has not changed since the previous <see cref="M:System.Windows.Controls.Primitives.DocumentViewerBase.GetPageViewsCollection(System.Boolean@)" /> call; otherwise, false if the collection has changed since the last <see cref="M:System.Windows.Controls.Primitives.DocumentViewerBase.GetPageViewsCollection(System.Boolean@)" /> call.</param>
	protected virtual ReadOnlyCollection<DocumentPageView> GetPageViewsCollection(out bool changed)
	{
		List<DocumentPageView> list = new List<DocumentPageView>(1);
		FindDocumentPageViews(this, list);
		AdornerDecorator adornerDecorator = FindAdornerDecorator(this);
		TextEditorRenderScope = ((adornerDecorator != null) ? (adornerDecorator.Child as FrameworkElement) : null);
		for (int i = 0; i < _pageViews.Count; i++)
		{
			_pageViews[i].DocumentPaginator = null;
		}
		changed = true;
		return new ReadOnlyCollection<DocumentPageView>(list);
	}

	/// <summary>Called whenever the working set of <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> objects for this viewer (represented by the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.PageViews" /> property) is modified.</summary>
	protected virtual void OnPageViewsChanged()
	{
		if (this.PageViewsChanged != null)
		{
			this.PageViewsChanged(this, EventArgs.Empty);
		}
		OnMasterPageNumberChanged();
	}

	/// <summary>Called whenever the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.MasterPageNumber" /> property is modified.</summary>
	protected virtual void OnMasterPageNumberChanged()
	{
		UpdateReadOnlyProperties(pageCountChanged: true, masterPageChanged: true);
	}

	/// <summary>Invoked whenever the <see cref="E:System.Windows.FrameworkElement.RequestBringIntoView" /> event reaches an element derived from this class in its route.  Implement this method to add class handling for this event.</summary>
	/// <param name="element">The element from which the <see cref="E:System.Windows.FrameworkElement.RequestBringIntoView" /> event originated.</param>
	/// <param name="rect">A rectangular region, in the coordinate space of <paramref name="element" />, which should be made visible.</param>
	/// <param name="pageNumber">The page number for the page that contains <paramref name="element" />.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised if element is null.</exception>
	protected virtual void OnBringIntoView(DependencyObject element, Rect rect, int pageNumber)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		OnGoToPageCommand(pageNumber);
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.NavigationCommands.PreviousPage" /> routed command.</summary>
	protected virtual void OnPreviousPageCommand()
	{
		if (CanGoToPreviousPage)
		{
			ShiftPagesByOffset(-1);
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.NavigationCommands.NextPage" /> routed command.</summary>
	protected virtual void OnNextPageCommand()
	{
		if (CanGoToNextPage)
		{
			ShiftPagesByOffset(1);
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.NavigationCommands.FirstPage" /> routed command.</summary>
	protected virtual void OnFirstPageCommand()
	{
		ShiftPagesByOffset(1 - MasterPageNumber);
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.NavigationCommands.LastPage" /> routed command.</summary>
	protected virtual void OnLastPageCommand()
	{
		ShiftPagesByOffset(PageCount - MasterPageNumber);
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.NavigationCommands.GoToPage" /> routed command.</summary>
	/// <param name="pageNumber">The number of the page to jump to.</param>
	protected virtual void OnGoToPageCommand(int pageNumber)
	{
		if (CanGoToPage(pageNumber))
		{
			ShiftPagesByOffset(pageNumber - MasterPageNumber);
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.ApplicationCommands.Print" /> routed command.</summary>
	protected virtual void OnPrintCommand()
	{
		PrintDocumentImageableArea documentImageableArea = null;
		if (_documentWriter != null || _document == null)
		{
			return;
		}
		XpsDocumentWriter xpsDocumentWriter = PrintQueue.CreateXpsDocumentWriter(ref documentImageableArea);
		if (xpsDocumentWriter != null && documentImageableArea != null)
		{
			_documentWriter = xpsDocumentWriter;
			_documentWriter.WritingCompleted += HandlePrintCompleted;
			_documentWriter.WritingCancelled += HandlePrintCancelled;
			CommandManager.InvalidateRequerySuggested();
			if (_document is FixedDocumentSequence)
			{
				xpsDocumentWriter.WriteAsync(_document as FixedDocumentSequence);
			}
			else if (_document is FixedDocument)
			{
				xpsDocumentWriter.WriteAsync(_document as FixedDocument);
			}
			else
			{
				xpsDocumentWriter.WriteAsync(_document.DocumentPaginator);
			}
		}
	}

	/// <summary>Handles the <see cref="P:System.Windows.Input.ApplicationCommands.CancelPrint" /> routed command.</summary>
	protected virtual void OnCancelPrintCommand()
	{
		if (_documentWriter != null)
		{
			_documentWriter.CancelAsync();
		}
	}

	/// <summary>Called whenever the <see cref="P:System.Windows.Controls.Primitives.DocumentViewerBase.Document" /> property is modified.</summary>
	protected virtual void OnDocumentChanged()
	{
		for (int i = 0; i < _pageViews.Count; i++)
		{
			_pageViews[i].DocumentPaginator = ((_document != null) ? _document.DocumentPaginator : null);
		}
		UpdateReadOnlyProperties(pageCountChanged: true, masterPageChanged: true);
		AttachTextEditor();
	}

	internal bool IsMasterPageView(DocumentPageView pageView)
	{
		Invariant.Assert(pageView != null);
		return pageView == GetMasterPageView();
	}

	internal ITextRange Find(FindToolBar findToolBar)
	{
		ITextView masterPageTextView = null;
		DocumentPageView masterPageView = GetMasterPageView();
		if (masterPageView != null && masterPageView != null)
		{
			masterPageTextView = ((IServiceProvider)masterPageView).GetService(typeof(ITextView)) as ITextView;
		}
		return DocumentViewerHelper.Find(findToolBar, _textEditor, _textView, masterPageTextView);
	}

	private ITextPointer GetMasterPageTextPointer(bool startOfPage)
	{
		ITextPointer textPointer = null;
		ITextView textView = null;
		DocumentPageView masterPageView = GetMasterPageView();
		if (masterPageView != null && masterPageView != null && ((IServiceProvider)masterPageView).GetService(typeof(ITextView)) is ITextView { IsValid: not false } textView2)
		{
			foreach (TextSegment textSegment in textView2.TextSegments)
			{
				if (textSegment.IsNull)
				{
					continue;
				}
				if (textPointer == null)
				{
					textPointer = (startOfPage ? textSegment.Start : textSegment.End);
				}
				else if (startOfPage)
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
		return textPointer;
	}

	private void UpdatePageViews()
	{
		bool changed;
		ReadOnlyCollection<DocumentPageView> pageViewsCollection = GetPageViewsCollection(out changed);
		if (changed)
		{
			VerifyDocumentPageViews(pageViewsCollection);
			_pageViews = pageViewsCollection;
			for (int i = 0; i < _pageViews.Count; i++)
			{
				_pageViews[i].DocumentPaginator = ((_document != null) ? _document.DocumentPaginator : null);
			}
			if (_textView != null)
			{
				_textView.OnPagesUpdated();
			}
			OnPageViewsChanged();
		}
	}

	private void VerifyDocumentPageViews(ReadOnlyCollection<DocumentPageView> pageViews)
	{
		bool flag = false;
		if (pageViews == null)
		{
			throw new ArgumentException(SR.DocumentViewerPageViewsCollectionEmpty);
		}
		for (int i = 0; i < pageViews.Count; i++)
		{
			if (GetIsMasterPage(pageViews[i]))
			{
				if (flag)
				{
					throw new ArgumentException(SR.DocumentViewerOneMasterPage);
				}
				flag = true;
			}
		}
	}

	private void FindDocumentPageViews(Visual root, List<DocumentPageView> pageViews)
	{
		Invariant.Assert(root != null);
		Invariant.Assert(pageViews != null);
		int internalVisualChildrenCount = root.InternalVisualChildrenCount;
		for (int i = 0; i < internalVisualChildrenCount; i++)
		{
			Visual visual = root.InternalGetVisualChild(i);
			if (visual is FrameworkElement frameworkElement)
			{
				if (frameworkElement.TemplatedParent != null)
				{
					if (frameworkElement is DocumentPageView)
					{
						pageViews.Add(frameworkElement as DocumentPageView);
					}
					else
					{
						FindDocumentPageViews(frameworkElement, pageViews);
					}
				}
			}
			else
			{
				FindDocumentPageViews(visual, pageViews);
			}
		}
	}

	private AdornerDecorator FindAdornerDecorator(Visual root)
	{
		Invariant.Assert(root != null);
		AdornerDecorator adornerDecorator = null;
		int internalVisualChildrenCount = root.InternalVisualChildrenCount;
		for (int i = 0; i < internalVisualChildrenCount; i++)
		{
			Visual visual = root.InternalGetVisualChild(i);
			if (visual is FrameworkElement frameworkElement)
			{
				if (frameworkElement.TemplatedParent != null)
				{
					if (frameworkElement is AdornerDecorator)
					{
						adornerDecorator = (AdornerDecorator)frameworkElement;
					}
					else if (!(frameworkElement is DocumentPageView))
					{
						adornerDecorator = FindAdornerDecorator(frameworkElement);
					}
				}
			}
			else
			{
				adornerDecorator = FindAdornerDecorator(visual);
			}
			if (adornerDecorator != null)
			{
				break;
			}
		}
		return adornerDecorator;
	}

	private void AttachTextEditor()
	{
		AnnotationService service = AnnotationService.GetService(this);
		if (_textEditor != null)
		{
			_textEditor.OnDetach();
			_textEditor = null;
			if (_textView.TextContainer.TextView == _textView)
			{
				_textView.TextContainer.TextView = null;
			}
			_textView = null;
		}
		service?.Disable();
		ITextContainer textContainer = TextContainer;
		if (textContainer != null && TextEditorRenderScope != null && textContainer.TextSelection == null)
		{
			_textView = new MultiPageTextView(this, TextEditorRenderScope, textContainer);
			_textEditor = new TextEditor(textContainer, this, isUndoEnabled: false);
			_textEditor.IsReadOnly = !IsEditingEnabled;
			_textEditor.TextView = _textView;
			textContainer.TextView = _textView;
		}
		service?.Enable(service.Store);
	}

	private void HandlePrintCompleted(object sender, WritingCompletedEventArgs e)
	{
		CleanUpPrintOperation();
	}

	private void HandlePrintCancelled(object sender, WritingCancelledEventArgs e)
	{
		CleanUpPrintOperation();
	}

	private void HandlePaginationCompleted(object sender, EventArgs e)
	{
		UpdateReadOnlyProperties(pageCountChanged: true, masterPageChanged: false);
	}

	private void HandlePaginationProgress(object sender, EventArgs e)
	{
		UpdateReadOnlyProperties(pageCountChanged: true, masterPageChanged: false);
	}

	private void HandleGetPageNumberCompleted(object sender, GetPageNumberCompletedEventArgs e)
	{
		UpdateReadOnlyProperties(pageCountChanged: true, masterPageChanged: false);
		if (_document != null && sender == _document.DocumentPaginator && e != null && !e.Cancelled && e.Error == null && e.UserState is BringIntoViewState bringIntoViewState && bringIntoViewState.Source == this)
		{
			OnBringIntoView(bringIntoViewState.TargetObject, bringIntoViewState.TargetRect, e.PageNumber + 1);
		}
	}

	private void HandleRequestBringIntoView(RequestBringIntoViewEventArgs args)
	{
		Rect targetRectangle = Rect.Empty;
		if (args == null || args.TargetObject == null || !(_document is DependencyObject))
		{
			return;
		}
		DependencyObject dependencyObject = _document as DependencyObject;
		if (args.TargetObject == _document)
		{
			OnGoToPageCommand(1);
			args.Handled = true;
		}
		else
		{
			DependencyObject dependencyObject2 = args.TargetObject;
			while (dependencyObject2 != null && dependencyObject2 != dependencyObject)
			{
				dependencyObject2 = ((!(dependencyObject2 is FrameworkElement { TemplatedParent: not null } frameworkElement)) ? LogicalTreeHelper.GetParent(dependencyObject2) : frameworkElement.TemplatedParent);
			}
			if (dependencyObject2 != null)
			{
				if (args.TargetObject is UIElement)
				{
					UIElement uIElement = (UIElement)args.TargetObject;
					if (VisualTreeHelper.IsAncestorOf(this, uIElement))
					{
						targetRectangle = args.TargetRect;
						if (targetRectangle.IsEmpty)
						{
							targetRectangle = new Rect(uIElement.RenderSize);
						}
						targetRectangle = uIElement.TransformToAncestor(this).TransformBounds(targetRectangle);
						targetRectangle.IntersectsWith(new Rect(base.RenderSize));
					}
				}
				if (targetRectangle.IsEmpty && _document.DocumentPaginator is DynamicDocumentPaginator dynamicDocumentPaginator)
				{
					ContentPosition objectPosition = dynamicDocumentPaginator.GetObjectPosition(args.TargetObject);
					if (objectPosition != null && objectPosition != ContentPosition.Missing)
					{
						BringIntoViewState userState = new BringIntoViewState(this, objectPosition, args.TargetObject, args.TargetRect);
						dynamicDocumentPaginator.GetPageNumberAsync(objectPosition, userState);
					}
				}
				args.Handled = true;
			}
		}
		if (args.Handled)
		{
			if (targetRectangle.IsEmpty)
			{
				BringIntoView();
			}
			else
			{
				BringIntoView(targetRectangle);
			}
		}
	}

	private void UpdateReadOnlyProperties(bool pageCountChanged, bool masterPageChanged)
	{
		if (pageCountChanged)
		{
			SetValue(PageCountPropertyKey, (_document != null) ? _document.DocumentPaginator.PageCount : 0);
		}
		bool flag = false;
		if (masterPageChanged)
		{
			int num = 0;
			if (_document != null && _pageViews.Count > 0)
			{
				DocumentPageView masterPageView = GetMasterPageView();
				if (masterPageView != null)
				{
					num = masterPageView.PageNumber + 1;
				}
			}
			SetValue(MasterPageNumberPropertyKey, num);
			SetValue(CanGoToPreviousPagePropertyKey, MasterPageNumber > 1);
			flag = true;
		}
		if (pageCountChanged || masterPageChanged)
		{
			bool value = false;
			if (_document != null)
			{
				value = MasterPageNumber < _document.DocumentPaginator.PageCount || !_document.DocumentPaginator.IsPageCountValid;
			}
			SetValue(CanGoToNextPagePropertyKey, value);
			flag = true;
		}
		if (flag)
		{
			CommandManager.InvalidateRequerySuggested();
		}
	}

	private void ShiftPagesByOffset(int offset)
	{
		if (offset != 0)
		{
			for (int i = 0; i < _pageViews.Count; i++)
			{
				_pageViews[i].PageNumber += offset;
			}
			OnMasterPageNumberChanged();
		}
	}

	private void SetFlags(bool value, Flags flags)
	{
		_flags = (value ? (_flags | flags) : (_flags & ~flags));
	}

	private bool CheckFlags(Flags flags)
	{
		return (_flags & flags) == flags;
	}

	private void DocumentChanged(IDocumentPaginatorSource oldDocument, IDocumentPaginatorSource newDocument)
	{
		_document = newDocument;
		if (oldDocument != null)
		{
			if (CheckFlags(Flags.DocumentAsLogicalChild))
			{
				RemoveLogicalChild(oldDocument);
			}
			if (oldDocument.DocumentPaginator is DynamicDocumentPaginator dynamicDocumentPaginator)
			{
				dynamicDocumentPaginator.PaginationProgress -= HandlePaginationProgress;
				dynamicDocumentPaginator.PaginationCompleted -= HandlePaginationCompleted;
				dynamicDocumentPaginator.GetPageNumberCompleted -= HandleGetPageNumberCompleted;
			}
			if (oldDocument is DependencyObject dependencyObject)
			{
				dependencyObject.ClearValue(PathNode.HiddenParentProperty);
			}
		}
		if (_document is DependencyObject dependencyObject2 && LogicalTreeHelper.GetParent(dependencyObject2) != null && dependencyObject2 is ContentElement)
		{
			ContentOperations.SetParent((ContentElement)dependencyObject2, this);
			SetFlags(value: false, Flags.DocumentAsLogicalChild);
		}
		else
		{
			SetFlags(value: true, Flags.DocumentAsLogicalChild);
		}
		if (_document != null)
		{
			if (CheckFlags(Flags.DocumentAsLogicalChild))
			{
				AddLogicalChild(_document);
			}
			if (_document.DocumentPaginator is DynamicDocumentPaginator dynamicDocumentPaginator2)
			{
				dynamicDocumentPaginator2.PaginationProgress += HandlePaginationProgress;
				dynamicDocumentPaginator2.PaginationCompleted += HandlePaginationCompleted;
				dynamicDocumentPaginator2.GetPageNumberCompleted += HandleGetPageNumberCompleted;
			}
			DependencyObject dependencyObject3 = _document as DependencyObject;
			if (_document is FixedDocument || _document is FixedDocumentSequence)
			{
				ClearValue(AnnotationService.DataIdProperty);
				AnnotationService.SetSubTreeProcessorId(this, FixedPageProcessor.Id);
				dependencyObject3.SetValue(PathNode.HiddenParentProperty, this);
				AnnotationService service = AnnotationService.GetService(this);
				if (service != null)
				{
					service.LocatorManager.RegisterSelectionProcessor(new FixedTextSelectionProcessor(), typeof(TextRange));
					service.LocatorManager.RegisterSelectionProcessor(new FixedTextSelectionProcessor(), typeof(TextAnchor));
				}
			}
			else if (_document is FlowDocument flowDocument)
			{
				flowDocument.SetDpi(GetDpi());
				flowDocument.SetValue(PathNode.HiddenParentProperty, this);
				AnnotationService service2 = AnnotationService.GetService(this);
				if (service2 != null)
				{
					service2.LocatorManager.RegisterSelectionProcessor(new TextSelectionProcessor(), typeof(TextRange));
					service2.LocatorManager.RegisterSelectionProcessor(new TextSelectionProcessor(), typeof(TextAnchor));
					service2.LocatorManager.RegisterSelectionProcessor(new TextViewSelectionProcessor(), typeof(DocumentViewerBase));
				}
				AnnotationService.SetDataId(this, "FlowDocument");
			}
			else
			{
				ClearValue(AnnotationService.SubTreeProcessorIdProperty);
				ClearValue(AnnotationService.DataIdProperty);
			}
		}
		if (UIElementAutomationPeer.FromElement(this) is DocumentViewerBaseAutomationPeer documentViewerBaseAutomationPeer)
		{
			documentViewerBaseAutomationPeer.InvalidatePeer();
		}
		OnDocumentChanged();
	}

	private void CleanUpPrintOperation()
	{
		if (_documentWriter != null)
		{
			_documentWriter.WritingCompleted -= HandlePrintCompleted;
			_documentWriter.WritingCancelled -= HandlePrintCancelled;
			_documentWriter = null;
			CommandManager.InvalidateRequerySuggested();
		}
	}

	private static void CreateCommandBindings()
	{
		ExecutedRoutedEventHandler executedRoutedEventHandler = ExecutedRoutedEventHandler;
		CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = CanExecuteRoutedEventHandler;
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewerBase), NavigationCommands.PreviousPage, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewerBase), NavigationCommands.NextPage, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewerBase), NavigationCommands.FirstPage, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewerBase), NavigationCommands.LastPage, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewerBase), NavigationCommands.GoToPage, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewerBase), ApplicationCommands.Print, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.P, ModifierKeys.Control));
		CommandHelpers.RegisterCommandHandler(typeof(DocumentViewerBase), ApplicationCommands.CancelPrint, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		TextEditor.RegisterCommandHandlers(typeof(DocumentViewerBase), acceptsRichContent: true, !IsEditingEnabled, registerEventListeners: true);
	}

	private static void CanExecuteRoutedEventHandler(object target, CanExecuteRoutedEventArgs args)
	{
		DocumentViewerBase documentViewerBase = target as DocumentViewerBase;
		Invariant.Assert(documentViewerBase != null, "Target of CanExecuteRoutedEventHandler must be DocumentViewerBase.");
		Invariant.Assert(args != null, "args cannot be null.");
		if (args.Command == ApplicationCommands.Print)
		{
			args.CanExecute = documentViewerBase.Document != null && documentViewerBase._documentWriter == null;
			args.Handled = true;
		}
		else if (args.Command == ApplicationCommands.CancelPrint)
		{
			args.CanExecute = documentViewerBase._documentWriter != null;
		}
		else
		{
			args.CanExecute = true;
		}
	}

	private static void ExecutedRoutedEventHandler(object target, ExecutedRoutedEventArgs args)
	{
		DocumentViewerBase documentViewerBase = target as DocumentViewerBase;
		Invariant.Assert(documentViewerBase != null, "Target of ExecuteEvent must be DocumentViewerBase.");
		Invariant.Assert(args != null, "args cannot be null.");
		if (args.Command == NavigationCommands.PreviousPage)
		{
			documentViewerBase.OnPreviousPageCommand();
		}
		else if (args.Command == NavigationCommands.NextPage)
		{
			documentViewerBase.OnNextPageCommand();
		}
		else if (args.Command == NavigationCommands.FirstPage)
		{
			documentViewerBase.OnFirstPageCommand();
		}
		else if (args.Command == NavigationCommands.LastPage)
		{
			documentViewerBase.OnLastPageCommand();
		}
		else if (args.Command == NavigationCommands.GoToPage)
		{
			if (args.Parameter != null)
			{
				int num = -1;
				try
				{
					num = Convert.ToInt32(args.Parameter, CultureInfo.CurrentCulture);
				}
				catch (InvalidCastException)
				{
				}
				catch (OverflowException)
				{
				}
				catch (FormatException)
				{
				}
				if (num >= 0)
				{
					documentViewerBase.OnGoToPageCommand(num);
				}
			}
		}
		else if (args.Command == ApplicationCommands.Print)
		{
			documentViewerBase.OnPrintCommand();
		}
		else if (args.Command == ApplicationCommands.CancelPrint)
		{
			documentViewerBase.OnCancelPrintCommand();
		}
		else
		{
			Invariant.Assert(condition: false, "Command not handled in ExecutedRoutedEventHandler.");
		}
	}

	private static void HandleRequestBringIntoView(object sender, RequestBringIntoViewEventArgs args)
	{
		if (sender != null && sender is DocumentViewerBase)
		{
			((DocumentViewerBase)sender).HandleRequestBringIntoView(args);
		}
	}

	private static void DocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is DocumentViewerBase);
		((DocumentViewerBase)d).DocumentChanged((IDocumentPaginatorSource)e.OldValue, (IDocumentPaginatorSource)e.NewValue);
		CommandManager.InvalidateRequerySuggested();
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="value"> An object to add as a child.</param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (Document != null)
		{
			throw new InvalidOperationException(SR.DocumentViewerCanHaveOnlyOneChild);
		}
		if (!(value is IDocumentPaginatorSource document))
		{
			throw new ArgumentException(SR.DocumentViewerChildMustImplementIDocumentPaginatorSource, "value");
		}
		Document = document;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="text"> A string to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns> A service object of type <paramref name="serviceType" />.</returns>
	/// <param name="serviceType"> An object that specifies the type of service object to get.</param>
	object IServiceProvider.GetService(Type serviceType)
	{
		object result = null;
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (serviceType == typeof(ITextView))
		{
			result = _textView;
		}
		else if (serviceType == typeof(TextContainer) || serviceType == typeof(ITextContainer))
		{
			result = TextContainer;
		}
		return result;
	}
}
