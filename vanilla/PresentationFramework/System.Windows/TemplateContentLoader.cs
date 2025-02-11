using System.Xaml;

namespace System.Windows;

/// <summary>Implements <see cref="T:System.Xaml.XamlDeferringLoader" /> in order to defer loading of the XAML content that is defined for a template in WPF XAML.</summary>
public class TemplateContentLoader : XamlDeferringLoader
{
	/// <summary>Loads XAML content in a deferred mode, based on a <see cref="T:System.Xaml.XamlReader" /> and certain required services from a service provider.</summary>
	/// <returns>The root object for the node stream of the input <see cref="T:System.Xaml.XamlReader" />. Specifically, this is a <see cref="T:System.Windows.TemplateContent" /> instance.</returns>
	/// <param name="xamlReader">The initiating reader that is then returned on calls to <see cref="M:System.Windows.TemplateContentLoader.Save(System.Object,System.IServiceProvider)" />.</param>
	/// <param name="serviceProvider">Service provider for required services.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xamlReader" /> or <paramref name="serviceProvider" /> are null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="serviceProvider" /> does not provide a required service.</exception>
	public override object Load(XamlReader xamlReader, IServiceProvider serviceProvider)
	{
		if (serviceProvider == null)
		{
			throw new ArgumentNullException("serviceProvider");
		}
		if (xamlReader == null)
		{
			throw new ArgumentNullException("xamlReader");
		}
		IXamlObjectWriterFactory factory = RequireService<IXamlObjectWriterFactory>(serviceProvider);
		return new TemplateContent(xamlReader, factory, serviceProvider);
	}

	private static T RequireService<T>(IServiceProvider provider) where T : class
	{
		return (provider.GetService(typeof(T)) as T) ?? throw new InvalidOperationException(SR.Format(SR.DeferringLoaderNoContext, typeof(TemplateContentLoader).Name, typeof(T).Name));
	}

	/// <summary>Do not use; always throws an exception. See Remarks.</summary>
	/// <returns>Always throws an exception. See Remarks.</returns>
	/// <param name="value">The input value to commit for deferred loading.</param>
	/// <param name="serviceProvider">Service provider for required services.</param>
	/// <exception cref="T:System.NotSupportedException">Thrown in all cases.</exception>
	public override XamlReader Save(object value, IServiceProvider serviceProvider)
	{
		throw new NotSupportedException(SR.Format(SR.DeferringLoaderNoSave, typeof(TemplateContentLoader).Name));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.TemplateContentLoader" /> class. </summary>
	public TemplateContentLoader()
	{
	}
}
