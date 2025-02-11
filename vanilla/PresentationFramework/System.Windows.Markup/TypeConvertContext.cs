using System.ComponentModel;

namespace System.Windows.Markup;

internal class TypeConvertContext : ITypeDescriptorContext, IServiceProvider
{
	private ParserContext _parserContext;

	private string _attribStringValue;

	public IContainer Container => null;

	public object Instance => null;

	public PropertyDescriptor PropertyDescriptor => null;

	public ParserContext ParserContext => _parserContext;

	public void OnComponentChanged()
	{
	}

	public bool OnComponentChanging()
	{
		return false;
	}

	public virtual object GetService(Type serviceType)
	{
		if (serviceType == typeof(IUriContext))
		{
			return _parserContext;
		}
		if (serviceType == typeof(string))
		{
			return _attribStringValue;
		}
		return _parserContext.ProvideValueProvider.GetService(serviceType);
	}

	public TypeConvertContext(ParserContext parserContext)
	{
		_parserContext = parserContext;
	}
}
