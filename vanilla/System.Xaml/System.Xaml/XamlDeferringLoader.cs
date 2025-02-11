namespace System.Xaml;

/// <summary>Represents a XAML reader behavior that loads and returns deferred content.</summary>
public abstract class XamlDeferringLoader
{
	/// <summary>Loads XAML content in a deferred mode, which is based on a <see cref="T:System.Xaml.XamlReader" /> and certain required services from a service provider.</summary>
	/// <returns>The root object that is produced by the input <see cref="T:System.Xaml.XamlReader" />.</returns>
	/// <param name="xamlReader">The initiating reader that is returned on calls to <see cref="M:System.Xaml.XamlDeferringLoader.Save(System.Object,System.IServiceProvider)" />.</param>
	/// <param name="serviceProvider">The service provider for the required services.</param>
	public abstract object Load(XamlReader xamlReader, IServiceProvider serviceProvider);

	/// <summary>Commits a value for deferred loading.</summary>
	/// <returns>A XAML reader that can be used to obtain the deferred value as XAML node information.</returns>
	/// <param name="value">The input value to commit for deferred loading.</param>
	/// <param name="serviceProvider">The service provider for the required services.</param>
	public abstract XamlReader Save(object value, IServiceProvider serviceProvider);

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlDeferringLoader" /> class. </summary>
	protected XamlDeferringLoader()
	{
	}
}
