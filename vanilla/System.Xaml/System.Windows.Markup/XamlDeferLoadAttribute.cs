namespace System.Windows.Markup;

/// <summary>Indicates that a class or property has a deferred load usage for XAML (such as a template behavior), and reports the class that enables the deferring behavior and its destination/content type.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class XamlDeferLoadAttribute : Attribute
{
	private string _contentTypeName;

	private string _loaderTypeName;

	/// <summary>Gets the string name of the type for the destination/content type of the defer load behavior.</summary>
	/// <returns>The string name of the type for the destination/content type of the defer load behavior.</returns>
	public string LoaderTypeName => _loaderTypeName;

	/// <summary>Gets the string name of the type for the implementation to use for the defer load behavior.</summary>
	/// <returns>The string name of the type for the converter to use for the defer load behavior.</returns>
	public string ContentTypeName => _contentTypeName;

	/// <summary>Gets the CLR <see cref="T:System.Type" /> value for the implementation to use for the defer load behavior.</summary>
	/// <returns>The CLR <see cref="T:System.Type" /> value for the implementation to use for the defer load behavior.</returns>
	public Type LoaderType { get; private set; }

	/// <summary>Gets the CLR <see cref="T:System.Type" /> value for the destination/content type of the defer load behavior.</summary>
	/// <returns>The CLR <see cref="T:System.Type" /> value for the destination/content type of the defer load behavior.</returns>
	public Type ContentType { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlDeferLoadAttribute" /> class, using CLR <see cref="T:System.Type" /> values. </summary>
	/// <param name="loaderType">The CLR <see cref="T:System.Type" /> value for the implementation to use for the defer load behavior.</param>
	/// <param name="contentType">The CLR <see cref="T:System.Type" /> value for the destination/content type of the defer load behavior.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="loaderType" /> or <paramref name="contentType" /> is null.</exception>
	public XamlDeferLoadAttribute(Type loaderType, Type contentType)
	{
		ArgumentNullException.ThrowIfNull(loaderType, "loaderType");
		ArgumentNullException.ThrowIfNull(contentType, "contentType");
		_loaderTypeName = loaderType.AssemblyQualifiedName;
		_contentTypeName = contentType.AssemblyQualifiedName;
		LoaderType = loaderType;
		ContentType = contentType;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlDeferLoadAttribute" /> class, using string names of types. </summary>
	/// <param name="loaderType">The string name of the type for the implementation to use for the defer load behavior.</param>
	/// <param name="contentType">The string name of the type for the destination/content type of the defer load behavior.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="loaderType" /> or <paramref name="contentType" /> is null.</exception>
	public XamlDeferLoadAttribute(string loaderType, string contentType)
	{
		_loaderTypeName = loaderType ?? throw new ArgumentNullException("loaderType");
		_contentTypeName = contentType ?? throw new ArgumentNullException("contentType");
	}
}
