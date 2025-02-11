namespace System.Windows.Annotations.Storage;

/// <summary>Provides data for the <see cref="E:System.Windows.Annotations.Storage.AnnotationStore.StoreContentChanged" /> event.</summary>
public class StoreContentChangedEventArgs : EventArgs
{
	private StoreContentAction _action;

	private Annotation _annotation;

	/// <summary>Gets the <see cref="T:System.Windows.Annotations.Annotation" /> that changed in the store.</summary>
	/// <returns>The <see cref="T:System.Windows.Annotations.Annotation" /> that changed in the store.</returns>
	public Annotation Annotation => _annotation;

	/// <summary>Gets the action performed.</summary>
	/// <returns>An action <see cref="F:System.Windows.Annotations.Storage.StoreContentAction.Added" /> or <see cref="F:System.Windows.Annotations.Storage.StoreContentAction.Deleted" /> value that identifies the operation performed.</returns>
	public StoreContentAction Action => _action;

	/// <summary>Initializes a new instance of the <see cref="M:System.Windows.Annotations.Storage.StoreContentChangedEventArgs.#ctor(System.Windows.Annotations.Storage.StoreContentAction,System.Windows.Annotations.Annotation)" /> class.</summary>
	/// <param name="action">The action of the event.</param>
	/// <param name="annotation">The annotation added or removed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="annotation" /> or <paramref name="action" /> is null.</exception>
	public StoreContentChangedEventArgs(StoreContentAction action, Annotation annotation)
	{
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		_action = action;
		_annotation = annotation;
	}
}
