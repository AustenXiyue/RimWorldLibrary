using System.Collections;
using System.Windows;
using System.Windows.Media;

namespace MS.Internal.Annotations.Component;

internal interface IAnnotationComponent
{
	IList AttachedAnnotations { get; }

	PresentationContext PresentationContext { get; set; }

	int ZOrder { get; set; }

	bool IsDirty { get; set; }

	UIElement AnnotatedElement { get; }

	GeneralTransform GetDesiredTransform(GeneralTransform transform);

	void AddAttachedAnnotation(IAttachedAnnotation attachedAnnotation);

	void RemoveAttachedAnnotation(IAttachedAnnotation attachedAnnotation);

	void ModifyAttachedAnnotation(IAttachedAnnotation attachedAnnotation, object previousAttachedAnchor, AttachmentLevel previousAttachmentLevel);
}
