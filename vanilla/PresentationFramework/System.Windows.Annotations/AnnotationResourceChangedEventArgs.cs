using System.ComponentModel;

namespace System.Windows.Annotations;

/// <summary>Provides data for the <see cref="E:System.Windows.Annotations.Annotation.AnchorChanged" /> and <see cref="E:System.Windows.Annotations.Annotation.CargoChanged" /> events.</summary>
public sealed class AnnotationResourceChangedEventArgs : EventArgs
{
	private Annotation _annotation;

	private AnnotationResource _resource;

	private AnnotationAction _action;

	/// <summary>Gets the <see cref="T:System.Windows.Annotations.Annotation" /> that raised the event.</summary>
	/// <returns>The <see cref="T:System.Windows.Annotations.Annotation" /> that raised the event.</returns>
	public Annotation Annotation => _annotation;

	/// <summary>Gets the <see cref="P:System.Windows.Annotations.Annotation.Anchors" /> or <see cref="P:System.Windows.Annotations.Annotation.Cargos" /> resource associated with the event.</summary>
	/// <returns>The annotation anchor or cargo resource that was <see cref="F:System.Windows.Annotations.AnnotationAction.Added" />, <see cref="F:System.Windows.Annotations.AnnotationAction.Removed" />, or <see cref="F:System.Windows.Annotations.AnnotationAction.Modified" />.</returns>
	public AnnotationResource Resource => _resource;

	/// <summary>Gets the action of the annotation <see cref="P:System.Windows.Annotations.AnnotationResourceChangedEventArgs.Resource" />.</summary>
	/// <returns>The action of the annotation <see cref="P:System.Windows.Annotations.AnnotationResourceChangedEventArgs.Resource" />.</returns>
	public AnnotationAction Action => _action;

	/// <summary>Initializes a new instance of the <see cref="M:System.Windows.Annotations.AnnotationResourceChangedEventArgs.#ctor(System.Windows.Annotations.Annotation,System.Windows.Annotations.AnnotationAction,System.Windows.Annotations.AnnotationResource)" /> class.</summary>
	/// <param name="annotation">The annotation that raised the event.</param>
	/// <param name="action">The action of the event.</param>
	/// <param name="resource">The <see cref="P:System.Windows.Annotations.Annotation.Anchors" /> or <see cref="P:System.Windows.Annotations.Annotation.Cargos" /> resource of the event.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="annotation" /> or <paramref name="action" /> is null.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="action" /> is not a valid <see cref="T:System.Windows.Annotations.AnnotationAction" /> value.</exception>
	public AnnotationResourceChangedEventArgs(Annotation annotation, AnnotationAction action, AnnotationResource resource)
	{
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		if (action < AnnotationAction.Added || action > AnnotationAction.Modified)
		{
			throw new InvalidEnumArgumentException("action", (int)action, typeof(AnnotationAction));
		}
		_annotation = annotation;
		_resource = resource;
		_action = action;
	}
}
