using System.ComponentModel;

namespace System.Windows.Markup.Primitives;

internal class ValueSerializerContextWrapper : IValueSerializerContext, ITypeDescriptorContext, IServiceProvider
{
	private IValueSerializerContext _baseContext;

	public IContainer Container
	{
		get
		{
			if (_baseContext != null)
			{
				return _baseContext.Container;
			}
			return null;
		}
	}

	public object Instance
	{
		get
		{
			if (_baseContext != null)
			{
				return _baseContext.Instance;
			}
			return null;
		}
	}

	public PropertyDescriptor PropertyDescriptor
	{
		get
		{
			if (_baseContext != null)
			{
				return _baseContext.PropertyDescriptor;
			}
			return null;
		}
	}

	public ValueSerializerContextWrapper(IValueSerializerContext baseContext)
	{
		_baseContext = baseContext;
	}

	public ValueSerializer GetValueSerializerFor(PropertyDescriptor descriptor)
	{
		if (_baseContext != null)
		{
			return _baseContext.GetValueSerializerFor(descriptor);
		}
		return null;
	}

	public ValueSerializer GetValueSerializerFor(Type type)
	{
		if (_baseContext != null)
		{
			return _baseContext.GetValueSerializerFor(type);
		}
		return null;
	}

	public void OnComponentChanged()
	{
		if (_baseContext != null)
		{
			_baseContext.OnComponentChanged();
		}
	}

	public bool OnComponentChanging()
	{
		if (_baseContext != null)
		{
			return _baseContext.OnComponentChanging();
		}
		return true;
	}

	public object GetService(Type serviceType)
	{
		if (_baseContext != null)
		{
			return _baseContext.GetService(serviceType);
		}
		return null;
	}
}
