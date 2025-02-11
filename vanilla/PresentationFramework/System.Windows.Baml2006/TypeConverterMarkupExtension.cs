using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;

namespace System.Windows.Baml2006;

internal class TypeConverterMarkupExtension : MarkupExtension
{
	private class TypeConverterContext : ITypeDescriptorContext, IServiceProvider
	{
		private IServiceProvider _serviceProvider;

		IContainer ITypeDescriptorContext.Container => null;

		object ITypeDescriptorContext.Instance => null;

		PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor => null;

		public TypeConverterContext(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		object IServiceProvider.GetService(Type serviceType)
		{
			return _serviceProvider.GetService(serviceType);
		}

		void ITypeDescriptorContext.OnComponentChanged()
		{
		}

		bool ITypeDescriptorContext.OnComponentChanging()
		{
			return false;
		}
	}

	private TypeConverter _converter;

	private object _value;

	public TypeConverterMarkupExtension(TypeConverter converter, object value)
	{
		_converter = converter;
		_value = value;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return _converter.ConvertFrom(new TypeConverterContext(serviceProvider), CultureInfo.InvariantCulture, _value);
	}
}
