using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xaml;
using MS.Internal;

namespace System.Windows.Baml2006;

internal class Baml2006ReaderInternal : Baml2006Reader
{
	internal Baml2006ReaderInternal(Stream stream, Baml2006SchemaContext schemaContext, Baml2006ReaderSettings settings)
		: base(stream, schemaContext, settings)
	{
	}

	internal Baml2006ReaderInternal(Stream stream, Baml2006SchemaContext baml2006SchemaContext, Baml2006ReaderSettings baml2006ReaderSettings, object root)
		: base(stream, baml2006SchemaContext, baml2006ReaderSettings, root)
	{
	}

	internal override ReadOnlySpan<char> GetAssemblyNameForNamespace(Assembly asm)
	{
		return asm.FullName;
	}

	internal override object CreateTypeConverterMarkupExtension(XamlMember property, TypeConverter converter, object propertyValue, Baml2006ReaderSettings settings)
	{
		if (FrameworkAppContextSwitches.AppendLocalAssemblyVersionForSourceUri && property.DeclaringType.UnderlyingType == typeof(ResourceDictionary) && property.Name.Equals("Source"))
		{
			return new SourceUriTypeConverterMarkupExtension(converter, propertyValue, settings.LocalAssembly);
		}
		return base.CreateTypeConverterMarkupExtension(property, converter, propertyValue, settings);
	}
}
