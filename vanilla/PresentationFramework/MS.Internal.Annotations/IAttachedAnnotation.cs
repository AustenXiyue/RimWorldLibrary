using System.Windows;
using System.Windows.Annotations;
using System.Windows.Annotations.Storage;

namespace MS.Internal.Annotations;

internal interface IAttachedAnnotation : IAnchorInfo
{
	object AttachedAnchor { get; }

	object FullyAttachedAnchor { get; }

	AttachmentLevel AttachmentLevel { get; }

	DependencyObject Parent { get; }

	Point AnchorPoint { get; }

	AnnotationStore Store { get; }

	bool IsAnchorEqual(object o);
}
