using System.ComponentModel;

namespace System.Windows.Annotations;

/// <summary> Provides data for the <see cref="E:System.Windows.Annotations.Annotation.AuthorChanged" /> event. </summary>
public sealed class AnnotationAuthorChangedEventArgs : EventArgs
{
	private Annotation _annotation;

	private object _author;

	private AnnotationAction _action;

	/// <summary> Gets the annotation that raised the event. </summary>
	/// <returns>The annotation that raised the event.</returns>
	public Annotation Annotation => _annotation;

	/// <summary> Gets the author object that is the target of the event. </summary>
	/// <returns>The author object that is the target of the event.</returns>
	public object Author => _author;

	/// <summary> Gets the author change operation for the event. </summary>
	/// <returns>The author change operation: <see cref="F:System.Windows.Annotations.AnnotationAction.Added" />, <see cref="F:System.Windows.Annotations.AnnotationAction.Removed" />, or <see cref="F:System.Windows.Annotations.AnnotationAction.Modified" />.</returns>
	public AnnotationAction Action => _action;

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Annotations.AnnotationAuthorChangedEventArgs" /> class. </summary>
	/// <param name="annotation">The annotation raising the event.</param>
	/// <param name="action">The author operation performed: <see cref="F:System.Windows.Annotations.AnnotationAction.Added" />, <see cref="F:System.Windows.Annotations.AnnotationAction.Removed" />, or <see cref="F:System.Windows.Annotations.AnnotationAction.Modified" />.</param>
	/// <param name="author">The author object being changed by the event.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="annotation" /> or <paramref name="action" /> is a null reference (Nothing in Visual Basic).</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="action" /> is an invalid <see cref="T:System.Windows.Annotations.AnnotationAction" />.</exception>
	public AnnotationAuthorChangedEventArgs(Annotation annotation, AnnotationAction action, object author)
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
		_author = author;
		_action = action;
	}
}
