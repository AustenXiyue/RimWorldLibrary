using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Annotations.Storage;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Annotations;
using MS.Internal.Annotations.Anchoring;
using MS.Internal.Annotations.Component;
using MS.Utility;

namespace System.Windows.Annotations;

/// <summary>Provides core services of the Microsoft Annotations Framework to manage and display user annotations.</summary>
public sealed class AnnotationService : DispatcherObject
{
	/// <summary>Represents the command to create a highlight annotation on the current selection.</summary>
	/// <returns>The routed command to create a highlight annotation on the current selection.</returns>
	public static readonly RoutedUICommand CreateHighlightCommand;

	/// <summary>Represents the command to create a text-note annotation on the current selection.</summary>
	/// <returns>The routed command to create a text-note annotation on the current selection.</returns>
	public static readonly RoutedUICommand CreateTextStickyNoteCommand;

	/// <summary>Represents the command to create an ink-note annotation on the current selection.</summary>
	/// <returns>The routed command to create an ink-note annotation on the current selection.</returns>
	public static readonly RoutedUICommand CreateInkStickyNoteCommand;

	/// <summary>Represents the command to clear highlight annotations from the current selection.</summary>
	/// <returns>The routed command to clear all highlight annotations from the current selection.</returns>
	public static readonly RoutedUICommand ClearHighlightsCommand;

	/// <summary>Represents the command to delete all ink-note and text-note annotations in the current selection.</summary>
	/// <returns>The routed command to delete all ink-note and text-note annotations in the current selection.</returns>
	public static readonly RoutedUICommand DeleteStickyNotesCommand;

	/// <summary>Represents the command to delete all ink-note, text-note, and highlight annotations in the current selection.</summary>
	/// <returns>The routed command to delete all ink-note, text-note, and highlight annotations in the current selection.</returns>
	public static readonly RoutedUICommand DeleteAnnotationsCommand;

	internal static readonly DependencyProperty ChooserProperty;

	internal static readonly DependencyProperty SubTreeProcessorIdProperty;

	internal static readonly DependencyProperty DataIdProperty;

	internal static readonly DependencyProperty ServiceProperty;

	private static readonly DependencyProperty AttachedAnnotationsProperty;

	private DependencyObject _root;

	private AnnotationMap _annotationMap = new AnnotationMap();

	private AnnotationComponentManager _annotationComponentManager;

	private LocatorManager _locatorManager;

	private bool _isEnabled;

	private AnnotationStore _store;

	private Collection<DocumentPageView> _views = new Collection<DocumentPageView>();

	private DispatcherOperation _asyncLoadOperation;

	private DispatcherOperation _asyncLoadFromListOperation;

	private const int _maxAnnotationsBatch = 10;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Annotations.AnnotationService" /> is enabled.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Annotations.AnnotationService" /> is enabled; otherwise, false.</returns>
	public bool IsEnabled => _isEnabled;

	/// <summary>Gets the <see cref="T:System.Windows.Annotations.Storage.AnnotationStore" /> used by this <see cref="T:System.Windows.Annotations.AnnotationService" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Annotations.Storage.AnnotationStore" /> used by this <see cref="T:System.Windows.Annotations.AnnotationService" />.</returns>
	public AnnotationStore Store => _store;

	internal LocatorManager LocatorManager => _locatorManager;

	internal DependencyObject Root => _root;

	internal event AttachedAnnotationChangedEventHandler AttachedAnnotationChanged;

	static AnnotationService()
	{
		CreateHighlightCommand = new RoutedUICommand(SR.CreateHighlight, "CreateHighlight", typeof(AnnotationService), null);
		CreateTextStickyNoteCommand = new RoutedUICommand(SR.CreateTextNote, "CreateTextStickyNote", typeof(AnnotationService), null);
		CreateInkStickyNoteCommand = new RoutedUICommand(SR.CreateInkNote, "CreateInkStickyNote", typeof(AnnotationService), null);
		ClearHighlightsCommand = new RoutedUICommand(SR.ClearHighlight, "ClearHighlights", typeof(AnnotationService), null);
		DeleteStickyNotesCommand = new RoutedUICommand(SR.DeleteNotes, "DeleteStickyNotes", typeof(AnnotationService), null);
		DeleteAnnotationsCommand = new RoutedUICommand(SR.DeleteAnnotations, "DeleteAnnotations", typeof(AnnotationService), null);
		ChooserProperty = DependencyProperty.RegisterAttached("Chooser", typeof(AnnotationComponentChooser), typeof(AnnotationService), new FrameworkPropertyMetadata(new AnnotationComponentChooser(), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior));
		SubTreeProcessorIdProperty = LocatorManager.SubTreeProcessorIdProperty.AddOwner(typeof(AnnotationService));
		DataIdProperty = DataIdProcessor.DataIdProperty.AddOwner(typeof(AnnotationService));
		ServiceProperty = DependencyProperty.RegisterAttached("Service", typeof(AnnotationService), typeof(AnnotationService), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior));
		AttachedAnnotationsProperty = DependencyProperty.RegisterAttached("AttachedAnnotations", typeof(IList<IAttachedAnnotation>), typeof(AnnotationService));
		CommandManager.RegisterClassCommandBinding(typeof(DocumentViewerBase), new CommandBinding(CreateHighlightCommand, AnnotationHelper.OnCreateHighlightCommand, AnnotationHelper.OnQueryCreateHighlightCommand));
		CommandManager.RegisterClassCommandBinding(typeof(DocumentViewerBase), new CommandBinding(CreateTextStickyNoteCommand, AnnotationHelper.OnCreateTextStickyNoteCommand, AnnotationHelper.OnQueryCreateTextStickyNoteCommand));
		CommandManager.RegisterClassCommandBinding(typeof(DocumentViewerBase), new CommandBinding(CreateInkStickyNoteCommand, AnnotationHelper.OnCreateInkStickyNoteCommand, AnnotationHelper.OnQueryCreateInkStickyNoteCommand));
		CommandManager.RegisterClassCommandBinding(typeof(DocumentViewerBase), new CommandBinding(ClearHighlightsCommand, AnnotationHelper.OnClearHighlightsCommand, AnnotationHelper.OnQueryClearHighlightsCommand));
		CommandManager.RegisterClassCommandBinding(typeof(DocumentViewerBase), new CommandBinding(DeleteStickyNotesCommand, AnnotationHelper.OnDeleteStickyNotesCommand, AnnotationHelper.OnQueryDeleteStickyNotesCommand));
		CommandManager.RegisterClassCommandBinding(typeof(DocumentViewerBase), new CommandBinding(DeleteAnnotationsCommand, AnnotationHelper.OnDeleteAnnotationsCommand, AnnotationHelper.OnQueryDeleteAnnotationsCommand));
		CommandManager.RegisterClassCommandBinding(typeof(FlowDocumentScrollViewer), new CommandBinding(CreateHighlightCommand, AnnotationHelper.OnCreateHighlightCommand, AnnotationHelper.OnQueryCreateHighlightCommand));
		CommandManager.RegisterClassCommandBinding(typeof(FlowDocumentScrollViewer), new CommandBinding(CreateTextStickyNoteCommand, AnnotationHelper.OnCreateTextStickyNoteCommand, AnnotationHelper.OnQueryCreateTextStickyNoteCommand));
		CommandManager.RegisterClassCommandBinding(typeof(FlowDocumentScrollViewer), new CommandBinding(CreateInkStickyNoteCommand, AnnotationHelper.OnCreateInkStickyNoteCommand, AnnotationHelper.OnQueryCreateInkStickyNoteCommand));
		CommandManager.RegisterClassCommandBinding(typeof(FlowDocumentScrollViewer), new CommandBinding(ClearHighlightsCommand, AnnotationHelper.OnClearHighlightsCommand, AnnotationHelper.OnQueryClearHighlightsCommand));
		CommandManager.RegisterClassCommandBinding(typeof(FlowDocumentScrollViewer), new CommandBinding(DeleteStickyNotesCommand, AnnotationHelper.OnDeleteStickyNotesCommand, AnnotationHelper.OnQueryDeleteStickyNotesCommand));
		CommandManager.RegisterClassCommandBinding(typeof(FlowDocumentScrollViewer), new CommandBinding(DeleteAnnotationsCommand, AnnotationHelper.OnDeleteAnnotationsCommand, AnnotationHelper.OnQueryDeleteAnnotationsCommand));
		CommandManager.RegisterClassCommandBinding(typeof(FlowDocumentReader), new CommandBinding(CreateHighlightCommand, AnnotationHelper.OnCreateHighlightCommand, AnnotationHelper.OnQueryCreateHighlightCommand));
		CommandManager.RegisterClassCommandBinding(typeof(FlowDocumentReader), new CommandBinding(CreateTextStickyNoteCommand, AnnotationHelper.OnCreateTextStickyNoteCommand, AnnotationHelper.OnQueryCreateTextStickyNoteCommand));
		CommandManager.RegisterClassCommandBinding(typeof(FlowDocumentReader), new CommandBinding(CreateInkStickyNoteCommand, AnnotationHelper.OnCreateInkStickyNoteCommand, AnnotationHelper.OnQueryCreateInkStickyNoteCommand));
		CommandManager.RegisterClassCommandBinding(typeof(FlowDocumentReader), new CommandBinding(ClearHighlightsCommand, AnnotationHelper.OnClearHighlightsCommand, AnnotationHelper.OnQueryClearHighlightsCommand));
		CommandManager.RegisterClassCommandBinding(typeof(FlowDocumentReader), new CommandBinding(DeleteStickyNotesCommand, AnnotationHelper.OnDeleteStickyNotesCommand, AnnotationHelper.OnQueryDeleteStickyNotesCommand));
		CommandManager.RegisterClassCommandBinding(typeof(FlowDocumentReader), new CommandBinding(DeleteAnnotationsCommand, AnnotationHelper.OnDeleteAnnotationsCommand, AnnotationHelper.OnQueryDeleteAnnotationsCommand));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Annotations.AnnotationService" /> class for use with a specified <see cref="T:System.Windows.Controls.DocumentViewer" /> or <see cref="T:System.Windows.Controls.FlowDocumentPageViewer" /> control.</summary>
	/// <param name="viewer">The document viewing control associated with the <see cref="T:System.Windows.Annotations.AnnotationService" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="viewer" /> is null.</exception>
	public AnnotationService(DocumentViewerBase viewer)
	{
		if (viewer == null)
		{
			throw new ArgumentNullException("viewer");
		}
		Initialize(viewer);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Annotations.AnnotationService" /> class for use with a specified <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" /> control.</summary>
	/// <param name="viewer">The document viewing control associated with the <see cref="T:System.Windows.Annotations.AnnotationService" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="viewer" /> is null.</exception>
	public AnnotationService(FlowDocumentScrollViewer viewer)
	{
		if (viewer == null)
		{
			throw new ArgumentNullException("viewer");
		}
		Initialize(viewer);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Annotations.AnnotationService" /> class for use with a specified <see cref="T:System.Windows.Controls.FlowDocumentReader" /> control.</summary>
	/// <param name="viewer">The document reading control associated with the <see cref="T:System.Windows.Annotations.AnnotationService" />.</param>
	public AnnotationService(FlowDocumentReader viewer)
	{
		if (viewer == null)
		{
			throw new ArgumentNullException("viewer");
		}
		Initialize(viewer);
	}

	internal AnnotationService(DependencyObject root)
	{
		if (root == null)
		{
			throw new ArgumentNullException("root");
		}
		if (!(root is FrameworkElement) && !(root is FrameworkContentElement))
		{
			throw new ArgumentException(SR.ParameterMustBeLogicalNode, "root");
		}
		Initialize(root);
	}

	/// <summary>Enables the <see cref="T:System.Windows.Annotations.AnnotationService" /> for use with a given <see cref="T:System.Windows.Annotations.Storage.AnnotationStore" /> and displays all visible annotations.</summary>
	/// <param name="annotationStore">The annotation store to use for reading, writing, and displaying annotations.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="annotationStore" /> is null.</exception>
	public void Enable(AnnotationStore annotationStore)
	{
		if (annotationStore == null)
		{
			throw new ArgumentNullException("annotationStore");
		}
		VerifyAccess();
		if (_isEnabled)
		{
			throw new InvalidOperationException(SR.AnnotationServiceIsAlreadyEnabled);
		}
		VerifyServiceConfiguration(_root);
		_asyncLoadOperation = _root.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(LoadAnnotationsAsync), this);
		_isEnabled = true;
		_root.SetValue(ServiceProperty, this);
		_store = annotationStore;
		if (_root is DocumentViewerBase documentViewerBase)
		{
			bool flag = documentViewerBase.Document is FixedDocument || documentViewerBase.Document is FixedDocumentSequence;
			bool flag2 = !flag && documentViewerBase.Document is FlowDocument;
			if (!(flag || flag2) && documentViewerBase.Document != null)
			{
				throw new InvalidOperationException(SR.OnlyFlowFixedSupported);
			}
			if (flag)
			{
				_locatorManager.RegisterSelectionProcessor(new FixedTextSelectionProcessor(), typeof(TextRange));
				_locatorManager.RegisterSelectionProcessor(new FixedTextSelectionProcessor(), typeof(TextAnchor));
			}
			else if (flag2)
			{
				_locatorManager.RegisterSelectionProcessor(new TextSelectionProcessor(), typeof(TextRange));
				_locatorManager.RegisterSelectionProcessor(new TextSelectionProcessor(), typeof(TextAnchor));
				_locatorManager.RegisterSelectionProcessor(new TextViewSelectionProcessor(), typeof(DocumentViewerBase));
			}
		}
		annotationStore.StoreContentChanged += OnStoreContentChanged;
		annotationStore.AnchorChanged += OnAnchorChanged;
	}

	/// <summary>Disables annotations processing and hides all visible annotations.</summary>
	public void Disable()
	{
		VerifyAccess();
		if (!_isEnabled)
		{
			throw new InvalidOperationException(SR.AnnotationServiceNotEnabled);
		}
		if (_asyncLoadOperation != null)
		{
			_asyncLoadOperation.Abort();
			_asyncLoadOperation = null;
		}
		try
		{
			_store.StoreContentChanged -= OnStoreContentChanged;
			_store.AnchorChanged -= OnAnchorChanged;
		}
		finally
		{
			GetViewerAndDocument(_root, out var documentViewerBase, out var document);
			if (documentViewerBase != null)
			{
				UnregisterOnDocumentViewer(documentViewerBase);
			}
			else if (document != null)
			{
				ITextView textView = GetTextView(document);
				if (textView != null)
				{
					textView.Updated -= OnContentChanged;
				}
			}
			UnloadAnnotations();
			_isEnabled = false;
			_root.ClearValue(ServiceProperty);
		}
	}

	/// <summary>Returns the <see cref="T:System.Windows.Annotations.AnnotationService" /> instance associated with a specified document viewing control.</summary>
	/// <returns>The <see cref="T:System.Windows.Annotations.AnnotationService" /> associated with the given document viewing control; or null if the specified document viewing control has no <see cref="T:System.Windows.Annotations.AnnotationService" />.</returns>
	/// <param name="viewer">The document viewing control to return the <see cref="T:System.Windows.Annotations.AnnotationService" /> instance for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="viewer" /> is null.</exception>
	public static AnnotationService GetService(DocumentViewerBase viewer)
	{
		if (viewer == null)
		{
			throw new ArgumentNullException("viewer");
		}
		return viewer.GetValue(ServiceProperty) as AnnotationService;
	}

	/// <summary>Returns the <see cref="T:System.Windows.Annotations.AnnotationService" /> associated with a specified <see cref="T:System.Windows.Controls.FlowDocumentReader" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Annotations.AnnotationService" /> associated with the given document reader control; or null if the specified document reader has no <see cref="T:System.Windows.Annotations.AnnotationService" />.</returns>
	/// <param name="reader">The document reader control to return the <see cref="T:System.Windows.Annotations.AnnotationService" /> instance for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="reader" /> is null.</exception>
	public static AnnotationService GetService(FlowDocumentReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		return reader.GetValue(ServiceProperty) as AnnotationService;
	}

	/// <summary>Returns the <see cref="T:System.Windows.Annotations.AnnotationService" /> associated with a specified <see cref="T:System.Windows.Controls.FlowDocumentScrollViewer" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Annotations.AnnotationService" /> associated with the given document viewer control; or null if the specified viewer control has no <see cref="T:System.Windows.Annotations.AnnotationService" />.</returns>
	/// <param name="viewer">The document viewer control to return the <see cref="T:System.Windows.Annotations.AnnotationService" /> instance for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="viewer" /> is null.</exception>
	public static AnnotationService GetService(FlowDocumentScrollViewer viewer)
	{
		if (viewer == null)
		{
			throw new ArgumentNullException("viewer");
		}
		return viewer.GetValue(ServiceProperty) as AnnotationService;
	}

	internal void LoadAnnotations(DependencyObject element)
	{
		if (_asyncLoadOperation == null)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			if (!(element is FrameworkElement) && !(element is FrameworkContentElement))
			{
				throw new ArgumentException(SR.ParameterMustBeLogicalNode, "element");
			}
			VerifyAccess();
			if (!_isEnabled)
			{
				throw new InvalidOperationException(SR.AnnotationServiceNotEnabled);
			}
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.LoadAnnotationsBegin);
			IList<IAttachedAnnotation> attachedAnnotations = LocatorManager.ProcessSubTree(element);
			LoadAnnotationsFromList(attachedAnnotations);
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.LoadAnnotationsEnd);
		}
	}

	internal void UnloadAnnotations(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (!(element is FrameworkElement) && !(element is FrameworkContentElement))
		{
			throw new ArgumentException(SR.ParameterMustBeLogicalNode, "element");
		}
		VerifyAccess();
		if (!_isEnabled)
		{
			throw new InvalidOperationException(SR.AnnotationServiceNotEnabled);
		}
		if (!_annotationMap.IsEmpty)
		{
			IList allAttachedAnnotationsFor = GetAllAttachedAnnotationsFor(element);
			UnloadAnnotationsFromList(allAttachedAnnotationsFor);
		}
	}

	private void UnloadAnnotations()
	{
		IList allAttachedAnnotations = _annotationMap.GetAllAttachedAnnotations();
		UnloadAnnotationsFromList(allAttachedAnnotations);
	}

	internal IList<IAttachedAnnotation> GetAttachedAnnotations()
	{
		VerifyAccess();
		if (!_isEnabled)
		{
			throw new InvalidOperationException(SR.AnnotationServiceNotEnabled);
		}
		return _annotationMap.GetAllAttachedAnnotations();
	}

	internal static AnnotationService GetService(DependencyObject d)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		return d.GetValue(ServiceProperty) as AnnotationService;
	}

	internal static AnnotationComponentChooser GetChooser(DependencyObject d)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		return (AnnotationComponentChooser)d.GetValue(ChooserProperty);
	}

	internal static void SetSubTreeProcessorId(DependencyObject d, string id)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		d.SetValue(SubTreeProcessorIdProperty, id);
	}

	internal static string GetSubTreeProcessorId(DependencyObject d)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		return d.GetValue(SubTreeProcessorIdProperty) as string;
	}

	internal static void SetDataId(DependencyObject d, string id)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		d.SetValue(DataIdProperty, id);
	}

	internal static string GetDataId(DependencyObject d)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		return d.GetValue(DataIdProperty) as string;
	}

	private void Initialize(DependencyObject root)
	{
		Invariant.Assert(root != null, "Parameter 'root' is null.");
		_root = root;
		_locatorManager = new LocatorManager();
		_annotationComponentManager = new AnnotationComponentManager(this);
		AdornerPresentationContext.SetTypeZLevel(typeof(StickyNoteControl), 0);
		AdornerPresentationContext.SetTypeZLevel(typeof(MarkedHighlightComponent), 1);
		AdornerPresentationContext.SetTypeZLevel(typeof(HighlightComponent), 1);
		AdornerPresentationContext.SetZLevelRange(0, 1073741824, int.MaxValue);
		AdornerPresentationContext.SetZLevelRange(1, 0, 0);
	}

	private object LoadAnnotationsAsync(object obj)
	{
		Invariant.Assert(_isEnabled, "Service was disabled before attach operation executed.");
		_asyncLoadOperation = null;
		IDocumentPaginatorSource document = null;
		GetViewerAndDocument(Root, out var documentViewerBase, out document);
		if (documentViewerBase != null)
		{
			RegisterOnDocumentViewer(documentViewerBase);
		}
		else if (document != null)
		{
			ITextView textView = GetTextView(document);
			if (textView != null)
			{
				textView.Updated += OnContentChanged;
			}
		}
		IList<IAttachedAnnotation> obj2 = LocatorManager.ProcessSubTree(_root);
		LoadAnnotationsFromListAsync(obj2);
		return null;
	}

	private object LoadAnnotationsFromListAsync(object obj)
	{
		_asyncLoadFromListOperation = null;
		if (!(obj is List<IAttachedAnnotation> list))
		{
			return null;
		}
		if (list.Count > 10)
		{
			List<IAttachedAnnotation> list2 = new List<IAttachedAnnotation>(list.Count);
			list2 = list.GetRange(10, list.Count - 10);
			_asyncLoadFromListOperation = _root.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(LoadAnnotationsFromListAsync), list2);
			list.RemoveRange(10, list.Count - 10);
		}
		LoadAnnotationsFromList(list);
		return null;
	}

	private bool AttachedAnchorsEqual(IAttachedAnnotation firstAttachedAnnotation, IAttachedAnnotation secondAttachedAnnotation)
	{
		object attachedAnchor = firstAttachedAnnotation.AttachedAnchor;
		if (firstAttachedAnnotation.AttachmentLevel == secondAttachedAnnotation.AttachmentLevel && secondAttachedAnnotation.AttachedAnchor is TextAnchor textAnchor && textAnchor.Equals(attachedAnchor))
		{
			return true;
		}
		return false;
	}

	private void LoadAnnotationsFromList(IList<IAttachedAnnotation> attachedAnnotations)
	{
		Invariant.Assert(attachedAnnotations != null, "null attachedAnnotations list");
		List<AttachedAnnotationChangedEventArgs> list = new List<AttachedAnnotationChangedEventArgs>(attachedAnnotations.Count);
		IAttachedAnnotation attachedAnnotation = null;
		foreach (IAttachedAnnotation attachedAnnotation2 in attachedAnnotations)
		{
			Invariant.Assert(attachedAnnotation2 != null && attachedAnnotation2.Annotation != null, "invalid attached annotation");
			attachedAnnotation = FindAnnotationInList(attachedAnnotation2, _annotationMap.GetAttachedAnnotations(attachedAnnotation2.Annotation.Id));
			if (attachedAnnotation != null)
			{
				object attachedAnchor = attachedAnnotation.AttachedAnchor;
				AttachmentLevel attachmentLevel = attachedAnnotation.AttachmentLevel;
				if (attachedAnnotation2.AttachmentLevel != 0 && attachedAnnotation2.AttachmentLevel != AttachmentLevel.Incomplete)
				{
					if (!AttachedAnchorsEqual(attachedAnnotation, attachedAnnotation2))
					{
						((AttachedAnnotation)attachedAnnotation).Update(attachedAnnotation2.AttachedAnchor, attachedAnnotation2.AttachmentLevel, null);
						FullyResolveAnchor(attachedAnnotation);
						list.Add(AttachedAnnotationChangedEventArgs.Modified(attachedAnnotation, attachedAnchor, attachmentLevel));
					}
				}
				else
				{
					DoRemoveAttachedAnnotation(attachedAnnotation2);
					list.Add(AttachedAnnotationChangedEventArgs.Unloaded(attachedAnnotation2));
				}
			}
			else if (attachedAnnotation2.AttachmentLevel != 0 && attachedAnnotation2.AttachmentLevel != AttachmentLevel.Incomplete)
			{
				DoAddAttachedAnnotation(attachedAnnotation2);
				list.Add(AttachedAnnotationChangedEventArgs.Loaded(attachedAnnotation2));
			}
		}
		FireEvents(list);
	}

	private void UnloadAnnotationsFromList(IList attachedAnnotations)
	{
		Invariant.Assert(attachedAnnotations != null, "null attachedAnnotations list");
		List<AttachedAnnotationChangedEventArgs> list = new List<AttachedAnnotationChangedEventArgs>(attachedAnnotations.Count);
		foreach (IAttachedAnnotation attachedAnnotation in attachedAnnotations)
		{
			DoRemoveAttachedAnnotation(attachedAnnotation);
			list.Add(AttachedAnnotationChangedEventArgs.Unloaded(attachedAnnotation));
		}
		FireEvents(list);
	}

	private void OnLayoutUpdated(object sender, EventArgs args)
	{
		if (_root is UIElement uIElement)
		{
			uIElement.LayoutUpdated -= OnLayoutUpdated;
		}
		UpdateAnnotations();
	}

	private void UpdateAnnotations()
	{
		if (_asyncLoadOperation != null || !_isEnabled)
		{
			return;
		}
		IList<IAttachedAnnotation> list = null;
		IList<IAttachedAnnotation> list2 = new List<IAttachedAnnotation>();
		list = LocatorManager.ProcessSubTree(_root);
		List<IAttachedAnnotation> allAttachedAnnotations = _annotationMap.GetAllAttachedAnnotations();
		for (int num = allAttachedAnnotations.Count - 1; num >= 0; num--)
		{
			IAttachedAnnotation attachedAnnotation = FindAnnotationInList(allAttachedAnnotations[num], list);
			if (attachedAnnotation != null && AttachedAnchorsEqual(attachedAnnotation, allAttachedAnnotations[num]))
			{
				list.Remove(attachedAnnotation);
				list2.Add(allAttachedAnnotations[num]);
				allAttachedAnnotations.RemoveAt(num);
			}
		}
		if (allAttachedAnnotations != null && allAttachedAnnotations.Count > 0)
		{
			UnloadAnnotationsFromList(allAttachedAnnotations);
		}
		IList<UIElement> list3 = new List<UIElement>();
		foreach (IAttachedAnnotation item in list2)
		{
			if (item.Parent is UIElement uIElement && !list3.Contains(uIElement))
			{
				list3.Add(uIElement);
				InvalidateAdorners(uIElement);
			}
		}
		if (_asyncLoadFromListOperation != null)
		{
			_asyncLoadFromListOperation.Abort();
			_asyncLoadFromListOperation = null;
		}
		if (list != null && list.Count > 0)
		{
			LoadAnnotationsFromListAsync(list);
		}
	}

	private static void InvalidateAdorners(UIElement element)
	{
		AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(element);
		if (adornerLayer == null)
		{
			return;
		}
		Adorner[] adorners = adornerLayer.GetAdorners(element);
		if (adorners == null)
		{
			return;
		}
		for (int i = 0; i < adorners.Length; i++)
		{
			if (adorners[i] is AnnotationAdorner annotationAdorner)
			{
				Invariant.Assert(annotationAdorner.AnnotationComponent != null, "AnnotationAdorner component undefined");
				annotationAdorner.AnnotationComponent.IsDirty = true;
			}
		}
		adornerLayer.Update(element);
	}

	private static void VerifyServiceConfiguration(DependencyObject root)
	{
		Invariant.Assert(root != null, "Parameter 'root' is null.");
		if (GetService(root) != null)
		{
			throw new InvalidOperationException(SR.AnnotationServiceAlreadyExists);
		}
		new DescendentsWalker<object>(TreeWalkPriority.VisualTree, VerifyNoServiceOnNode, null).StartWalk(root);
	}

	private static void GetViewerAndDocument(DependencyObject root, out DocumentViewerBase documentViewerBase, out IDocumentPaginatorSource document)
	{
		documentViewerBase = root as DocumentViewerBase;
		document = null;
		if (documentViewerBase != null)
		{
			document = documentViewerBase.Document;
		}
		else if (root is FlowDocumentReader flowDocumentReader)
		{
			documentViewerBase = AnnotationHelper.GetFdrHost(flowDocumentReader) as DocumentViewerBase;
			document = flowDocumentReader.Document;
		}
		else if (root is FlowDocumentScrollViewer flowDocumentScrollViewer)
		{
			document = flowDocumentScrollViewer.Document;
		}
	}

	private static ITextView GetTextView(IDocumentPaginatorSource document)
	{
		ITextView result = null;
		if (document is IServiceProvider serviceProvider && serviceProvider.GetService(typeof(ITextContainer)) is ITextContainer textContainer)
		{
			result = textContainer.TextView;
		}
		return result;
	}

	private static bool VerifyNoServiceOnNode(DependencyObject node, object data, bool visitedViaVisualTree)
	{
		Invariant.Assert(node != null, "Parameter 'node' is null.");
		if (node.ReadLocalValue(ServiceProperty) is AnnotationService)
		{
			throw new InvalidOperationException(SR.AnnotationServiceAlreadyExists);
		}
		return true;
	}

	private IAttachedAnnotation FindAnnotationInList(IAttachedAnnotation attachedAnnotation, IList<IAttachedAnnotation> list)
	{
		foreach (IAttachedAnnotation item in list)
		{
			if (item.Annotation == attachedAnnotation.Annotation && item.Anchor == attachedAnnotation.Anchor && item.Parent == attachedAnnotation.Parent)
			{
				return item;
			}
		}
		return null;
	}

	private IList GetAllAttachedAnnotationsFor(DependencyObject element)
	{
		Invariant.Assert(element != null, "Parameter 'element' is null.");
		List<IAttachedAnnotation> list = new List<IAttachedAnnotation>();
		new DescendentsWalker<List<IAttachedAnnotation>>(TreeWalkPriority.VisualTree, GetAttachedAnnotationsFor, list).StartWalk(element);
		return list;
	}

	private bool GetAttachedAnnotationsFor(DependencyObject node, List<IAttachedAnnotation> result, bool visitedViaVisualTree)
	{
		Invariant.Assert(node != null, "Parameter 'node' is null.");
		Invariant.Assert(result != null, "Incorrect data passed - should be a List<IAttachedAnnotation>.");
		if (node.GetValue(AttachedAnnotationsProperty) is List<IAttachedAnnotation> collection)
		{
			result.AddRange(collection);
		}
		return true;
	}

	private void OnStoreContentChanged(object node, StoreContentChangedEventArgs args)
	{
		VerifyAccess();
		switch (args.Action)
		{
		case StoreContentAction.Added:
			AnnotationAdded(args.Annotation);
			break;
		case StoreContentAction.Deleted:
			AnnotationDeleted(args.Annotation.Id);
			break;
		default:
			Invariant.Assert(condition: false, "Unknown StoreContentAction.");
			break;
		}
	}

	private void OnAnchorChanged(object sender, AnnotationResourceChangedEventArgs args)
	{
		VerifyAccess();
		if (args.Resource != null)
		{
			AttachedAnnotationChangedEventArgs attachedAnnotationChangedEventArgs = null;
			switch (args.Action)
			{
			case AnnotationAction.Added:
				attachedAnnotationChangedEventArgs = AnchorAdded(args.Annotation, args.Resource);
				break;
			case AnnotationAction.Removed:
				attachedAnnotationChangedEventArgs = AnchorRemoved(args.Annotation, args.Resource);
				break;
			case AnnotationAction.Modified:
				attachedAnnotationChangedEventArgs = AnchorModified(args.Annotation, args.Resource);
				break;
			default:
				Invariant.Assert(condition: false, "Unknown AnnotationAction.");
				break;
			}
			if (attachedAnnotationChangedEventArgs != null)
			{
				this.AttachedAnnotationChanged(this, attachedAnnotationChangedEventArgs);
			}
		}
	}

	private void AnnotationAdded(Annotation annotation)
	{
		Invariant.Assert(annotation != null, "Parameter 'annotation' is null.");
		if (_annotationMap.GetAttachedAnnotations(annotation.Id).Count > 0)
		{
			throw new Exception(SR.AnnotationAlreadyExistInService);
		}
		List<AttachedAnnotationChangedEventArgs> list = new List<AttachedAnnotationChangedEventArgs>(annotation.Anchors.Count);
		foreach (AnnotationResource anchor in annotation.Anchors)
		{
			if (anchor.ContentLocators.Count != 0)
			{
				AttachedAnnotationChangedEventArgs attachedAnnotationChangedEventArgs = AnchorAdded(annotation, anchor);
				if (attachedAnnotationChangedEventArgs != null)
				{
					list.Add(attachedAnnotationChangedEventArgs);
				}
			}
		}
		FireEvents(list);
	}

	private void AnnotationDeleted(Guid annotationId)
	{
		IList<IAttachedAnnotation> attachedAnnotations = _annotationMap.GetAttachedAnnotations(annotationId);
		if (attachedAnnotations.Count > 0)
		{
			IAttachedAnnotation[] array = new IAttachedAnnotation[attachedAnnotations.Count];
			attachedAnnotations.CopyTo(array, 0);
			List<AttachedAnnotationChangedEventArgs> list = new List<AttachedAnnotationChangedEventArgs>(array.Length);
			IAttachedAnnotation[] array2 = array;
			foreach (IAttachedAnnotation attachedAnnotation in array2)
			{
				DoRemoveAttachedAnnotation(attachedAnnotation);
				list.Add(AttachedAnnotationChangedEventArgs.Deleted(attachedAnnotation));
			}
			FireEvents(list);
		}
	}

	private AttachedAnnotationChangedEventArgs AnchorAdded(Annotation annotation, AnnotationResource anchor)
	{
		Invariant.Assert(annotation != null && anchor != null, "Parameter 'annotation' or 'anchor' is null.");
		AttachedAnnotationChangedEventArgs result = null;
		AttachmentLevel attachmentLevel;
		object obj = FindAttachedAnchor(anchor, out attachmentLevel);
		if (attachmentLevel != 0 && attachmentLevel != AttachmentLevel.Incomplete)
		{
			Invariant.Assert(obj != null, "Must have a valid attached anchor.");
			AttachedAnnotation attachedAnnotation = new AttachedAnnotation(LocatorManager, annotation, anchor, obj, attachmentLevel);
			DoAddAttachedAnnotation(attachedAnnotation);
			result = AttachedAnnotationChangedEventArgs.Added(attachedAnnotation);
		}
		return result;
	}

	private AttachedAnnotationChangedEventArgs AnchorRemoved(Annotation annotation, AnnotationResource anchor)
	{
		Invariant.Assert(annotation != null && anchor != null, "Parameter 'annotation' or 'anchor' is null.");
		AttachedAnnotationChangedEventArgs result = null;
		IList<IAttachedAnnotation> attachedAnnotations = _annotationMap.GetAttachedAnnotations(annotation.Id);
		if (attachedAnnotations.Count > 0)
		{
			IAttachedAnnotation[] array = new IAttachedAnnotation[attachedAnnotations.Count];
			attachedAnnotations.CopyTo(array, 0);
			IAttachedAnnotation[] array2 = array;
			foreach (IAttachedAnnotation attachedAnnotation in array2)
			{
				if (attachedAnnotation.Anchor == anchor)
				{
					DoRemoveAttachedAnnotation(attachedAnnotation);
					result = AttachedAnnotationChangedEventArgs.Deleted(attachedAnnotation);
					break;
				}
			}
		}
		return result;
	}

	private AttachedAnnotationChangedEventArgs AnchorModified(Annotation annotation, AnnotationResource anchor)
	{
		Invariant.Assert(annotation != null && anchor != null, "Parameter 'annotation' or 'anchor' is null.");
		AttachedAnnotationChangedEventArgs result = null;
		bool flag = false;
		AttachmentLevel attachmentLevel;
		object obj = FindAttachedAnchor(anchor, out attachmentLevel);
		List<IAttachedAnnotation> attachedAnnotations = _annotationMap.GetAttachedAnnotations(annotation.Id);
		IAttachedAnnotation[] array = new IAttachedAnnotation[((ICollection<IAttachedAnnotation>)attachedAnnotations).Count];
		((ICollection<IAttachedAnnotation>)attachedAnnotations).CopyTo(array, 0);
		IAttachedAnnotation[] array2 = array;
		foreach (IAttachedAnnotation attachedAnnotation in array2)
		{
			if (attachedAnnotation.Anchor == anchor)
			{
				flag = true;
				if (attachmentLevel != 0)
				{
					Invariant.Assert(obj != null, "AttachedAnnotation with AttachmentLevel != Unresolved should have non-null AttachedAnchor.");
					object attachedAnchor = attachedAnnotation.AttachedAnchor;
					AttachmentLevel attachmentLevel2 = attachedAnnotation.AttachmentLevel;
					((AttachedAnnotation)attachedAnnotation).Update(obj, attachmentLevel, null);
					FullyResolveAnchor(attachedAnnotation);
					result = AttachedAnnotationChangedEventArgs.Modified(attachedAnnotation, attachedAnchor, attachmentLevel2);
				}
				else
				{
					DoRemoveAttachedAnnotation(attachedAnnotation);
					result = AttachedAnnotationChangedEventArgs.Deleted(attachedAnnotation);
				}
				break;
			}
		}
		if (!flag && attachmentLevel != 0 && attachmentLevel != AttachmentLevel.Incomplete)
		{
			Invariant.Assert(obj != null, "AttachedAnnotation with AttachmentLevel != Unresolved should have non-null AttachedAnchor.");
			AttachedAnnotation attachedAnnotation2 = new AttachedAnnotation(LocatorManager, annotation, anchor, obj, attachmentLevel);
			DoAddAttachedAnnotation(attachedAnnotation2);
			result = AttachedAnnotationChangedEventArgs.Added(attachedAnnotation2);
		}
		return result;
	}

	private void DoAddAttachedAnnotation(IAttachedAnnotation attachedAnnotation)
	{
		Invariant.Assert(attachedAnnotation != null, "Parameter 'attachedAnnotation' is null.");
		DependencyObject parent = attachedAnnotation.Parent;
		Invariant.Assert(parent != null, "AttachedAnnotation being added should have non-null Parent.");
		List<IAttachedAnnotation> list = parent.GetValue(AttachedAnnotationsProperty) as List<IAttachedAnnotation>;
		if (list == null)
		{
			list = new List<IAttachedAnnotation>(1);
			parent.SetValue(AttachedAnnotationsProperty, list);
		}
		list.Add(attachedAnnotation);
		_annotationMap.AddAttachedAnnotation(attachedAnnotation);
		FullyResolveAnchor(attachedAnnotation);
	}

	private void DoRemoveAttachedAnnotation(IAttachedAnnotation attachedAnnotation)
	{
		Invariant.Assert(attachedAnnotation != null, "Parameter 'attachedAnnotation' is null.");
		DependencyObject parent = attachedAnnotation.Parent;
		Invariant.Assert(parent != null, "AttachedAnnotation being added should have non-null Parent.");
		_annotationMap.RemoveAttachedAnnotation(attachedAnnotation);
		if (parent.GetValue(AttachedAnnotationsProperty) is List<IAttachedAnnotation> list)
		{
			list.Remove(attachedAnnotation);
			if (list.Count == 0)
			{
				parent.ClearValue(AttachedAnnotationsProperty);
			}
		}
	}

	private void FullyResolveAnchor(IAttachedAnnotation attachedAnnotation)
	{
		Invariant.Assert(attachedAnnotation != null, "Attached annotation cannot be null.");
		if (attachedAnnotation.AttachmentLevel == AttachmentLevel.Full)
		{
			return;
		}
		FixedPageProcessor fixedPageProcessor = null;
		TextSelectionProcessor textSelectionProcessor = null;
		TextSelectionProcessor textSelectionProcessor2 = null;
		bool flag = false;
		FrameworkElement frameworkElement = Root as FrameworkElement;
		if (frameworkElement is DocumentViewerBase)
		{
			flag = ((DocumentViewerBase)frameworkElement).Document is FlowDocument;
		}
		else if (frameworkElement is FlowDocumentScrollViewer || frameworkElement is FlowDocumentReader)
		{
			flag = true;
		}
		else
		{
			frameworkElement = null;
		}
		if (frameworkElement == null)
		{
			return;
		}
		try
		{
			if (flag)
			{
				textSelectionProcessor = LocatorManager.GetSelectionProcessor(typeof(TextRange)) as TextSelectionProcessor;
				Invariant.Assert(textSelectionProcessor != null, "TextSelectionProcessor should be available if we are processing flow content.");
				textSelectionProcessor.Clamping = false;
				textSelectionProcessor2 = LocatorManager.GetSelectionProcessor(typeof(TextAnchor)) as TextSelectionProcessor;
				Invariant.Assert(textSelectionProcessor2 != null, "TextSelectionProcessor should be available if we are processing flow content.");
				textSelectionProcessor2.Clamping = false;
			}
			else
			{
				fixedPageProcessor = LocatorManager.GetSubTreeProcessorForLocatorPart(FixedPageProcessor.CreateLocatorPart(0)) as FixedPageProcessor;
				Invariant.Assert(fixedPageProcessor != null, "FixedPageProcessor should be available if we are processing fixed content.");
				fixedPageProcessor.UseLogicalTree = true;
			}
			AttachmentLevel attachmentLevel;
			object fullyAttachedAnchor = FindAttachedAnchor(attachedAnnotation.Anchor, out attachmentLevel);
			if (attachmentLevel == AttachmentLevel.Full)
			{
				((AttachedAnnotation)attachedAnnotation).SetFullyAttachedAnchor(fullyAttachedAnchor);
			}
		}
		finally
		{
			if (flag)
			{
				textSelectionProcessor.Clamping = true;
				textSelectionProcessor2.Clamping = true;
			}
			else
			{
				fixedPageProcessor.UseLogicalTree = false;
			}
		}
	}

	private object FindAttachedAnchor(AnnotationResource anchor, out AttachmentLevel attachmentLevel)
	{
		Invariant.Assert(anchor != null, "Parameter 'anchor' is null.");
		attachmentLevel = AttachmentLevel.Unresolved;
		object obj = null;
		foreach (ContentLocatorBase contentLocator in anchor.ContentLocators)
		{
			obj = LocatorManager.FindAttachedAnchor(_root, null, contentLocator, out attachmentLevel);
			if (obj != null)
			{
				break;
			}
		}
		return obj;
	}

	private void FireEvents(List<AttachedAnnotationChangedEventArgs> eventsToFire)
	{
		Invariant.Assert(eventsToFire != null, "Parameter 'eventsToFire' is null.");
		if (this.AttachedAnnotationChanged == null)
		{
			return;
		}
		foreach (AttachedAnnotationChangedEventArgs item in eventsToFire)
		{
			this.AttachedAnnotationChanged(this, item);
		}
	}

	private void RegisterOnDocumentViewer(DocumentViewerBase viewer)
	{
		Invariant.Assert(viewer != null, "Parameter 'viewer' is null.");
		Invariant.Assert(_views.Count == 0, "Failed to unregister on a viewer before registering on new viewer.");
		foreach (DocumentPageView pageView in viewer.PageViews)
		{
			pageView.PageConnected += OnContentChanged;
			_views.Add(pageView);
		}
		viewer.PageViewsChanged += OnPageViewsChanged;
	}

	private void UnregisterOnDocumentViewer(DocumentViewerBase viewer)
	{
		Invariant.Assert(viewer != null, "Parameter 'viewer' is null.");
		foreach (DocumentPageView view in _views)
		{
			view.PageConnected -= OnContentChanged;
		}
		_views.Clear();
		viewer.PageViewsChanged -= OnPageViewsChanged;
	}

	private void OnPageViewsChanged(object sender, EventArgs e)
	{
		DocumentViewerBase documentViewerBase = sender as DocumentViewerBase;
		Invariant.Assert(documentViewerBase != null, "Sender for PageViewsChanged event should be a DocumentViewerBase.");
		UnregisterOnDocumentViewer(documentViewerBase);
		try
		{
			UpdateAnnotations();
		}
		finally
		{
			RegisterOnDocumentViewer(documentViewerBase);
		}
	}

	private void OnContentChanged(object sender, EventArgs e)
	{
		if (_root is UIElement uIElement)
		{
			uIElement.LayoutUpdated += OnLayoutUpdated;
		}
	}
}
