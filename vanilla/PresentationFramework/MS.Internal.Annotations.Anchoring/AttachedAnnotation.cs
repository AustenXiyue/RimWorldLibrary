using System.Windows;
using System.Windows.Annotations;
using System.Windows.Annotations.Storage;

namespace MS.Internal.Annotations.Anchoring;

internal class AttachedAnnotation : IAttachedAnnotation, IAnchorInfo
{
	private Annotation _annotation;

	private AnnotationResource _anchor;

	private object _attachedAnchor;

	private object _fullyAttachedAnchor;

	private AttachmentLevel _attachmentLevel;

	private DependencyObject _parent;

	private SelectionProcessor _selectionProcessor;

	private LocatorManager _locatorManager;

	private Point _cachedPoint;

	public Annotation Annotation => _annotation;

	public AnnotationResource Anchor => _anchor;

	public object AttachedAnchor => _attachedAnchor;

	public object ResolvedAnchor => FullyAttachedAnchor;

	public object FullyAttachedAnchor
	{
		get
		{
			if (_attachmentLevel == AttachmentLevel.Full)
			{
				return _attachedAnchor;
			}
			return _fullyAttachedAnchor;
		}
	}

	public AttachmentLevel AttachmentLevel => _attachmentLevel;

	public DependencyObject Parent => _parent;

	public Point AnchorPoint
	{
		get
		{
			Point anchorPoint = _selectionProcessor.GetAnchorPoint(_attachedAnchor);
			if (!double.IsInfinity(anchorPoint.X) && !double.IsInfinity(anchorPoint.Y))
			{
				_cachedPoint = anchorPoint;
			}
			return _cachedPoint;
		}
	}

	public AnnotationStore Store => GetStore();

	internal AttachedAnnotation(LocatorManager manager, Annotation annotation, AnnotationResource anchor, object attachedAnchor, AttachmentLevel attachmentLevel)
		: this(manager, annotation, anchor, attachedAnchor, attachmentLevel, null)
	{
	}

	internal AttachedAnnotation(LocatorManager manager, Annotation annotation, AnnotationResource anchor, object attachedAnchor, AttachmentLevel attachmentLevel, DependencyObject parent)
	{
		_annotation = annotation;
		_anchor = anchor;
		_locatorManager = manager;
		Update(attachedAnchor, attachmentLevel, parent);
	}

	public bool IsAnchorEqual(object o)
	{
		return false;
	}

	internal void Update(object attachedAnchor, AttachmentLevel attachmentLevel, DependencyObject parent)
	{
		_attachedAnchor = attachedAnchor;
		_attachmentLevel = attachmentLevel;
		_selectionProcessor = _locatorManager.GetSelectionProcessor(attachedAnchor.GetType());
		if (parent != null)
		{
			_parent = parent;
		}
		else
		{
			_parent = _selectionProcessor.GetParent(_attachedAnchor);
		}
	}

	internal void SetFullyAttachedAnchor(object fullyAttachedAnchor)
	{
		_fullyAttachedAnchor = fullyAttachedAnchor;
	}

	private AnnotationStore GetStore()
	{
		if (Parent != null)
		{
			AnnotationService service = AnnotationService.GetService(Parent);
			if (service != null)
			{
				return service.Store;
			}
		}
		return null;
	}
}
