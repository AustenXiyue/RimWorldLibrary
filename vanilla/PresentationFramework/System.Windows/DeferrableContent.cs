using System.ComponentModel;
using System.IO;
using System.Windows.Baml2006;
using System.Xaml;

namespace System.Windows;

/// <summary>Represents deferrable content that is held within BAML as a stream.</summary>
[TypeConverter(typeof(DeferrableContentConverter))]
public class DeferrableContent
{
	internal Stream Stream { get; private set; }

	internal Baml2006SchemaContext SchemaContext { get; private set; }

	internal IXamlObjectWriterFactory ObjectWriterFactory { get; private set; }

	internal XamlObjectWriterSettings ObjectWriterParentSettings { get; private set; }

	internal object RootObject { get; private set; }

	internal IServiceProvider ServiceProvider { get; private set; }

	internal DeferrableContent(Stream stream, Baml2006SchemaContext schemaContext, IXamlObjectWriterFactory objectWriterFactory, IServiceProvider serviceProvider, object rootObject)
	{
		ObjectWriterParentSettings = objectWriterFactory.GetParentSettings();
		bool flag = false;
		if (schemaContext.LocalAssembly != null)
		{
			flag = schemaContext.LocalAssembly.ImageRuntimeVersion.StartsWith("v2", StringComparison.Ordinal);
		}
		if (flag)
		{
			ObjectWriterParentSettings.SkipProvideValueOnRoot = true;
		}
		Stream = stream;
		SchemaContext = schemaContext;
		ObjectWriterFactory = objectWriterFactory;
		ServiceProvider = serviceProvider;
		RootObject = rootObject;
	}
}
