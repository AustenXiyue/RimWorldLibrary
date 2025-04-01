using System.Threading;

namespace System;

/// <summary>Provides a lazy indirect reference to an object and its associated metadata for use by the Managed Extensibility Framework.</summary>
/// <typeparam name="T">The type of the object referenced.</typeparam>
/// <typeparam name="TMetadata">The type of the metadata.</typeparam>
[Serializable]
public class Lazy<T, TMetadata> : Lazy<T>
{
	private TMetadata _metadata;

	/// <summary>Gets the metadata associated with the referenced object.</summary>
	/// <returns>The metadata associated with the referenced object.</returns>
	public TMetadata Metadata => _metadata;

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`2" /> class with the specified metadata that uses the specified function to get the referenced object.</summary>
	/// <param name="valueFactory">A function that returns the referenced object.</param>
	/// <param name="metadata">The metadata associated with the referenced object.</param>
	public Lazy(Func<T> valueFactory, TMetadata metadata)
		: base(valueFactory)
	{
		_metadata = metadata;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`2" /> class with the specified metadata.</summary>
	/// <param name="metadata">The metadata associated with the referenced object.</param>
	public Lazy(TMetadata metadata)
	{
		_metadata = metadata;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`2" /> class with the specified metadata and thread safety value.</summary>
	/// <param name="metadata">The metadata associated with the referenced object.</param>
	/// <param name="isThreadSafe">Indicates whether the <see cref="T:System.Lazy`2" /> object that is created will be thread-safe.</param>
	public Lazy(TMetadata metadata, bool isThreadSafe)
		: base(isThreadSafe)
	{
		_metadata = metadata;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`2" /> class with the specified metadata and thread safety value that uses the specified function to get the referenced object.</summary>
	/// <param name="valueFactory">A function that returns the referenced object.</param>
	/// <param name="metadata">The metadata associated with the referenced object.</param>
	/// <param name="isThreadSafe">Indicates whether the <see cref="T:System.Lazy`2" /> object that is created will be thread-safe.</param>
	public Lazy(Func<T> valueFactory, TMetadata metadata, bool isThreadSafe)
		: base(valueFactory, isThreadSafe)
	{
		_metadata = metadata;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`2" /> class with the specified metadata and thread synchronization mode.</summary>
	/// <param name="metadata">The metadata associated with the referenced object.</param>
	/// <param name="mode">The thread synchronization mode.</param>
	public Lazy(TMetadata metadata, LazyThreadSafetyMode mode)
		: base(mode)
	{
		_metadata = metadata;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`2" /> class with the specified metadata and thread synchronization mode that uses the specified function to get the referenced object.</summary>
	/// <param name="valueFactory">A function that returns the referenced object</param>
	/// <param name="metadata">The metadata associated with the referenced object.</param>
	/// <param name="mode">The thread synchronization mode</param>
	public Lazy(Func<T> valueFactory, TMetadata metadata, LazyThreadSafetyMode mode)
		: base(valueFactory, mode)
	{
		_metadata = metadata;
	}
}
