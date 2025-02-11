using System.Collections.Generic;

namespace System.Windows.Annotations.Storage;

/// <summary>When overridden in a derived class, represents a data store for writing and reading user annotations.    </summary>
public abstract class AnnotationStore : IDisposable
{
	private bool _disposed;

	private readonly object lockObject = new object();

	/// <summary>Gets or sets a value that indicates whether data in annotation buffers is to be written immediately to the physical data store. </summary>
	/// <returns>true if data in annotation buffers is to be written immediately to the physical data store for each operation; otherwise, false if data in the annotation buffers is to be written when the application explicitly calls <see cref="M:System.Windows.Annotations.Storage.AnnotationStore.Flush" />.</returns>
	public abstract bool AutoFlush { get; set; }

	/// <summary>Gets the object to use as a synchronization lock for <see cref="T:System.Windows.Annotations.Storage.AnnotationStore" /> critical sections.</summary>
	/// <returns>The object to use as a synchronization lock for <see cref="T:System.Windows.Annotations.Storage.AnnotationStore" /> critical sections.</returns>
	protected object SyncRoot => lockObject;

	/// <summary>Gets a value that indicates whether <see cref="Overload:System.Windows.Annotations.Storage.AnnotationStore.Dispose" /> has been called.</summary>
	/// <returns>true if <see cref="Overload:System.Windows.Annotations.Storage.AnnotationStore.Dispose" /> has been called; otherwise, false.  The default is false.</returns>
	protected bool IsDisposed => _disposed;

	/// <summary>Occurs when an <see cref="T:System.Windows.Annotations.Annotation" /> is added to or deleted from the store.</summary>
	public event StoreContentChangedEventHandler StoreContentChanged;

	/// <summary>Occurs when an author on any <see cref="T:System.Windows.Annotations.Annotation" /> in the store changes.</summary>
	public event AnnotationAuthorChangedEventHandler AuthorChanged;

	/// <summary>Occurs when an anchor on any <see cref="T:System.Windows.Annotations.Annotation" /> in the store changes.</summary>
	public event AnnotationResourceChangedEventHandler AnchorChanged;

	/// <summary>Occurs when a cargo on any <see cref="T:System.Windows.Annotations.Annotation" /> in the store changes.</summary>
	public event AnnotationResourceChangedEventHandler CargoChanged;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Annotations.Storage.AnnotationStore" /> class.  </summary>
	protected AnnotationStore()
	{
	}

	/// <summary>Guarantees that <see cref="M:System.Windows.Annotations.Storage.AnnotationStore.Dispose(System.Boolean)" /> will eventually be called for this store. </summary>
	~AnnotationStore()
	{
		Dispose(disposing: false);
	}

	/// <summary>Adds a new <see cref="T:System.Windows.Annotations.Annotation" /> to the store.</summary>
	/// <param name="newAnnotation">The annotation to add to the store.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="newAnnotation" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">An <see cref="T:System.Windows.Annotations.Annotation" /> with the same <see cref="P:System.Windows.Annotations.Annotation.Id" /> property value already exists in the store.</exception>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="Overload:System.Windows.Annotations.Storage.AnnotationStore.Dispose" /> has been called on the store.</exception>
	public abstract void AddAnnotation(Annotation newAnnotation);

	/// <summary>Deletes the annotation with the specified <see cref="P:System.Windows.Annotations.Annotation.Id" /> from the store.</summary>
	/// <returns>The annotation that was deleted; otherwise, null if an annotation with the specified <paramref name="annotationId" /> was not found in the store.</returns>
	/// <param name="annotationId">The globally unique identifier (GUID) <see cref="P:System.Windows.Annotations.Annotation.Id" /> property of the annotation to be deleted.</param>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="Overload:System.Windows.Annotations.Storage.AnnotationStore.Dispose" /> has been called on the store.</exception>
	public abstract Annotation DeleteAnnotation(Guid annotationId);

	/// <summary>Returns a list of annotations that have <see cref="P:System.Windows.Annotations.Annotation.Anchors" /> with locators that begin with a matching <see cref="T:System.Windows.Annotations.ContentLocatorPart" /> sequence.</summary>
	/// <returns>The list of annotations that have <see cref="P:System.Windows.Annotations.Annotation.Anchors" /> with locators that start and match the given <paramref name="anchorLocator" />; otherwise, null if no matching annotations were found.</returns>
	/// <param name="anchorLocator">The starting <see cref="T:System.Windows.Annotations.ContentLocatorPart" /> sequence to return matching annotations for.</param>
	public abstract IList<Annotation> GetAnnotations(ContentLocator anchorLocator);

	/// <summary>Returns a list of all the annotations in the store.</summary>
	/// <returns>The list of all annotations currently contained in the store.</returns>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="Overload:System.Windows.Annotations.Storage.AnnotationStore.Dispose" /> has been called on the store.</exception>
	public abstract IList<Annotation> GetAnnotations();

	/// <summary>Returns the annotation with the specified <see cref="P:System.Windows.Annotations.Annotation.Id" /> from the store.</summary>
	/// <returns>The annotation with the given <paramref name="annotationId" />; or null, if an annotation with the specified <paramref name="annotationId" /> was not found in the store.</returns>
	/// <param name="annotationId">The globally unique identifier (GUID) <see cref="P:System.Windows.Annotations.Annotation.Id" /> property of the annotation to be returned.</param>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="Overload:System.Windows.Annotations.Storage.AnnotationStore.Dispose" /> has been called on the store.</exception>
	public abstract Annotation GetAnnotation(Guid annotationId);

	/// <summary>Forces any annotation data retained in internal buffers to be written to the underlying storage device.</summary>
	/// <exception cref="T:System.ObjectDisposedException">
	///   <see cref="Overload:System.Windows.Annotations.Storage.AnnotationStore.Dispose" /> has been called on the store.</exception>
	public abstract void Flush();

	/// <summary>Releases all managed and unmanaged resources used by the store.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the store and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		lock (SyncRoot)
		{
			if (!_disposed)
			{
				_disposed = true;
			}
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Annotations.Storage.AnnotationStore.AuthorChanged" /> event.</summary>
	/// <param name="args">The event data.</param>
	protected virtual void OnAuthorChanged(AnnotationAuthorChangedEventArgs args)
	{
		AnnotationAuthorChangedEventHandler annotationAuthorChangedEventHandler = null;
		if (args.Author != null)
		{
			lock (SyncRoot)
			{
				annotationAuthorChangedEventHandler = this.AuthorChanged;
			}
			if (AutoFlush)
			{
				Flush();
			}
			annotationAuthorChangedEventHandler?.Invoke(this, args);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Annotations.Storage.AnnotationStore.AnchorChanged" /> event.</summary>
	/// <param name="args">The event data.</param>
	protected virtual void OnAnchorChanged(AnnotationResourceChangedEventArgs args)
	{
		AnnotationResourceChangedEventHandler annotationResourceChangedEventHandler = null;
		if (args.Resource != null)
		{
			lock (SyncRoot)
			{
				annotationResourceChangedEventHandler = this.AnchorChanged;
			}
			if (AutoFlush)
			{
				Flush();
			}
			annotationResourceChangedEventHandler?.Invoke(this, args);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Annotations.Storage.AnnotationStore.CargoChanged" /> event.</summary>
	/// <param name="args">The event data.</param>
	protected virtual void OnCargoChanged(AnnotationResourceChangedEventArgs args)
	{
		AnnotationResourceChangedEventHandler annotationResourceChangedEventHandler = null;
		if (args.Resource != null)
		{
			lock (SyncRoot)
			{
				annotationResourceChangedEventHandler = this.CargoChanged;
			}
			if (AutoFlush)
			{
				Flush();
			}
			annotationResourceChangedEventHandler?.Invoke(this, args);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Annotations.Storage.AnnotationStore.StoreContentChanged" /> event.</summary>
	/// <param name="e">The event data.</param>
	protected virtual void OnStoreContentChanged(StoreContentChangedEventArgs e)
	{
		StoreContentChangedEventHandler storeContentChangedEventHandler = null;
		lock (SyncRoot)
		{
			storeContentChangedEventHandler = this.StoreContentChanged;
		}
		if (AutoFlush)
		{
			Flush();
		}
		storeContentChangedEventHandler?.Invoke(this, e);
	}
}
