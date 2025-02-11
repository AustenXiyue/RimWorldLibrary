using System.Windows.Media;

namespace System.Windows.Markup;

internal class ProvideValueServiceProvider : IServiceProvider, IProvideValueTarget, IXamlTypeResolver, IUriContext, IFreezeFreezables
{
	private ParserContext _context;

	private object _targetObject;

	private object _targetProperty;

	object IProvideValueTarget.TargetObject => _targetObject;

	object IProvideValueTarget.TargetProperty => _targetProperty;

	Uri IUriContext.BaseUri
	{
		get
		{
			return _context.BaseUri;
		}
		set
		{
			throw new NotSupportedException(SR.ParserProvideValueCantSetUri);
		}
	}

	bool IFreezeFreezables.FreezeFreezables => _context.FreezeFreezables;

	internal ProvideValueServiceProvider(ParserContext context)
	{
		_context = context;
	}

	internal ProvideValueServiceProvider()
	{
	}

	internal void SetData(object targetObject, object targetProperty)
	{
		_targetObject = targetObject;
		_targetProperty = targetProperty;
	}

	internal void ClearData()
	{
		_targetObject = (_targetProperty = null);
	}

	Type IXamlTypeResolver.Resolve(string qualifiedTypeName)
	{
		return _context.XamlTypeMapper.GetTypeFromBaseString(qualifiedTypeName, _context, throwOnError: true);
	}

	bool IFreezeFreezables.TryFreeze(string value, Freezable freezable)
	{
		return _context.TryCacheFreezable(value, freezable);
	}

	Freezable IFreezeFreezables.TryGetFreezable(string value)
	{
		return _context.TryGetFreezable(value);
	}

	public object GetService(Type service)
	{
		if (service == typeof(IProvideValueTarget))
		{
			return this;
		}
		if (_context != null)
		{
			if (service == typeof(IXamlTypeResolver))
			{
				return this;
			}
			if (service == typeof(IUriContext))
			{
				return this;
			}
			if (service == typeof(IFreezeFreezables))
			{
				return this;
			}
		}
		return null;
	}
}
