using System.ComponentModel;
using System.Reflection;
using System.Windows.Navigation;

namespace System.Windows.Baml2006;

internal class SourceUriTypeConverterMarkupExtension : TypeConverterMarkupExtension
{
	private Assembly _assemblyInfo;

	public SourceUriTypeConverterMarkupExtension(TypeConverter converter, object value, Assembly assemblyInfo)
		: base(converter, value)
	{
		_assemblyInfo = assemblyInfo;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		object obj = base.ProvideValue(serviceProvider);
		Uri uri = obj as Uri;
		if (uri != null)
		{
			Uri uri2 = BaseUriHelper.AppendAssemblyVersion(uri, _assemblyInfo);
			if (uri2 != null)
			{
				return new ResourceDictionary.ResourceDictionarySourceUriWrapper(uri, uri2);
			}
		}
		return obj;
	}
}
