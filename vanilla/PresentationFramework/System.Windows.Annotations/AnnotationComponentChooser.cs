using System.Windows.Controls;
using MS.Internal.Annotations;
using MS.Internal.Annotations.Component;

namespace System.Windows.Annotations;

internal sealed class AnnotationComponentChooser
{
	public IAnnotationComponent ChooseAnnotationComponent(IAttachedAnnotation attachedAnnotation)
	{
		if (attachedAnnotation == null)
		{
			throw new ArgumentNullException("attachedAnnotation");
		}
		IAnnotationComponent result = null;
		if (attachedAnnotation.Annotation.AnnotationType == StickyNoteControl.TextSchemaName)
		{
			result = new StickyNoteControl(StickyNoteType.Text);
		}
		else if (attachedAnnotation.Annotation.AnnotationType == StickyNoteControl.InkSchemaName)
		{
			result = new StickyNoteControl(StickyNoteType.Ink);
		}
		else if (attachedAnnotation.Annotation.AnnotationType == HighlightComponent.TypeName)
		{
			result = new HighlightComponent();
		}
		return result;
	}
}
