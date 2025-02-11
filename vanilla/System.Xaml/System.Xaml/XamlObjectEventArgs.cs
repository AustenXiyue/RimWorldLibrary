namespace System.Xaml;

/// <summary>Provides data for callbacks that can be inserted in the sequence for object initialization and property setting. This influences the object graph that is produced by <see cref="T:System.Xaml.XamlObjectWriter" />.</summary>
public class XamlObjectEventArgs : EventArgs
{
	/// <summary>Gets the object instance that is relevant to the event data.</summary>
	/// <returns>The object instance that is relevant to the event data.</returns>
	public object Instance { get; private set; }

	public Uri SourceBamlUri { get; private set; }

	public int ElementLineNumber { get; private set; }

	public int ElementLinePosition { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlObjectEventArgs" /> class. </summary>
	/// <param name="instance">The object instance that is relevant to the event data.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	public XamlObjectEventArgs(object instance)
	{
		Instance = instance ?? throw new ArgumentNullException("instance");
	}

	internal XamlObjectEventArgs(object instance, Uri sourceBamlUri, int elementLineNumber, int elementLinePosition)
		: this(instance)
	{
		SourceBamlUri = sourceBamlUri;
		ElementLineNumber = elementLineNumber;
		ElementLinePosition = elementLinePosition;
	}
}
