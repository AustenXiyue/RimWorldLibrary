using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Baml2006;
using System.Windows.Markup;
using System.Xaml;

namespace System.Windows;

/// <summary>Converts a stream to a <see cref="T:System.Windows.DeferrableContent" /> instance.</summary>
public class DeferrableContentConverter : TypeConverter
{
	/// <summary>Returns whether this converter can convert the specified object to a <see cref="T:System.Windows.DeferrableContent" /> object.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false. </returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from. </param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (typeof(Stream).IsAssignableFrom(sourceType) || sourceType == typeof(byte[]))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Converts the specified stream to a new <see cref="T:System.Windows.DeferrableContent" /> object.</summary>
	/// <returns>A new <see cref="T:System.Windows.DeferrableContent" /> object.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture. </param>
	/// <param name="value">The source stream to convert.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="context" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="context" /> is not able to provide the necessary XAML schema context for BAML.-or-<see cref="T:System.Windows.Markup.IProvideValueTarget" /> service interpretation of <paramref name="context" /> determines that the target object is not a <see cref="T:System.Windows.ResourceDictionary" />.-or-<paramref name="value" /> is not a valid byte stream.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value != null)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (!(RequireService<IXamlSchemaContextProvider>(context).SchemaContext is Baml2006SchemaContext schemaContext))
			{
				throw new InvalidOperationException(SR.ExpectedBamlSchemaContext);
			}
			IXamlObjectWriterFactory objectWriterFactory = RequireService<IXamlObjectWriterFactory>(context);
			IProvideValueTarget provideValueTarget = RequireService<IProvideValueTarget>(context);
			IRootObjectProvider rootObjectProvider = RequireService<IRootObjectProvider>(context);
			if (!(provideValueTarget.TargetObject is ResourceDictionary))
			{
				throw new InvalidOperationException(SR.ExpectedResourceDictionaryTarget);
			}
			Stream stream = value as Stream;
			if (stream == null && value is byte[] buffer)
			{
				stream = new MemoryStream(buffer);
			}
			if (stream == null)
			{
				throw new InvalidOperationException(SR.ExpectedBinaryContent);
			}
			return new DeferrableContent(stream, schemaContext, objectWriterFactory, context, rootObjectProvider.RootObject);
		}
		return base.ConvertFrom(context, culture, value);
	}

	private static T RequireService<T>(IServiceProvider provider) where T : class
	{
		return (provider.GetService(typeof(T)) as T) ?? throw new InvalidOperationException(SR.Format(SR.DeferringLoaderNoContext, typeof(DeferrableContentConverter).Name, typeof(T).Name));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DeferrableContentConverter" /> class. </summary>
	public DeferrableContentConverter()
	{
	}
}
