using System;

namespace MS.Internal.Annotations;

internal class AttachedAnnotationChangedEventArgs : EventArgs
{
	private AttachedAnnotationAction _action;

	private IAttachedAnnotation _attachedAnnotation;

	private object _previousAttachedAnchor;

	private AttachmentLevel _previousAttachmentLevel;

	public AttachedAnnotationAction Action => _action;

	public IAttachedAnnotation AttachedAnnotation => _attachedAnnotation;

	public object PreviousAttachedAnchor => _previousAttachedAnchor;

	public AttachmentLevel PreviousAttachmentLevel => _previousAttachmentLevel;

	internal AttachedAnnotationChangedEventArgs(AttachedAnnotationAction action, IAttachedAnnotation attachedAnnotation, object previousAttachedAnchor, AttachmentLevel previousAttachmentLevel)
	{
		Invariant.Assert(attachedAnnotation != null);
		_action = action;
		_attachedAnnotation = attachedAnnotation;
		_previousAttachedAnchor = previousAttachedAnchor;
		_previousAttachmentLevel = previousAttachmentLevel;
	}

	internal static AttachedAnnotationChangedEventArgs Added(IAttachedAnnotation attachedAnnotation)
	{
		Invariant.Assert(attachedAnnotation != null);
		return new AttachedAnnotationChangedEventArgs(AttachedAnnotationAction.Added, attachedAnnotation, null, AttachmentLevel.Unresolved);
	}

	internal static AttachedAnnotationChangedEventArgs Loaded(IAttachedAnnotation attachedAnnotation)
	{
		Invariant.Assert(attachedAnnotation != null);
		return new AttachedAnnotationChangedEventArgs(AttachedAnnotationAction.Loaded, attachedAnnotation, null, AttachmentLevel.Unresolved);
	}

	internal static AttachedAnnotationChangedEventArgs Deleted(IAttachedAnnotation attachedAnnotation)
	{
		Invariant.Assert(attachedAnnotation != null);
		return new AttachedAnnotationChangedEventArgs(AttachedAnnotationAction.Deleted, attachedAnnotation, null, AttachmentLevel.Unresolved);
	}

	internal static AttachedAnnotationChangedEventArgs Unloaded(IAttachedAnnotation attachedAnnotation)
	{
		Invariant.Assert(attachedAnnotation != null);
		return new AttachedAnnotationChangedEventArgs(AttachedAnnotationAction.Unloaded, attachedAnnotation, null, AttachmentLevel.Unresolved);
	}

	internal static AttachedAnnotationChangedEventArgs Modified(IAttachedAnnotation attachedAnnotation, object previousAttachedAnchor, AttachmentLevel previousAttachmentLevel)
	{
		Invariant.Assert(attachedAnnotation != null && previousAttachedAnchor != null);
		return new AttachedAnnotationChangedEventArgs(AttachedAnnotationAction.AnchorModified, attachedAnnotation, previousAttachedAnchor, previousAttachmentLevel);
	}
}
