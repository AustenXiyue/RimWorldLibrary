using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;

namespace MS.Internal.Data;

internal class ValueConverterContext : ITypeDescriptorContext, IServiceProvider, IUriContext
{
	private DependencyObject _targetElement;

	private int _nestingLevel;

	private Uri _cachedBaseUri;

	public Uri BaseUri
	{
		get
		{
			if (_cachedBaseUri == null)
			{
				if (_targetElement != null)
				{
					_cachedBaseUri = BaseUriHelper.GetBaseUri(_targetElement);
				}
				else
				{
					_cachedBaseUri = BaseUriHelper.BaseUri;
				}
			}
			return _cachedBaseUri;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	internal bool IsInUse => _nestingLevel > 0;

	public IContainer Container => null;

	public object Instance => null;

	public PropertyDescriptor PropertyDescriptor => null;

	public virtual object GetService(Type serviceType)
	{
		if (serviceType == typeof(IUriContext))
		{
			return this;
		}
		return null;
	}

	internal void SetTargetElement(DependencyObject target)
	{
		if (target != null)
		{
			_nestingLevel++;
		}
		else if (_nestingLevel > 0)
		{
			_nestingLevel--;
		}
		Invariant.Assert(_nestingLevel <= 1, "illegal to recurse/reenter ValueConverterContext.SetTargetElement()");
		_targetElement = target;
		_cachedBaseUri = null;
	}

	public void OnComponentChanged()
	{
	}

	public bool OnComponentChanging()
	{
		return false;
	}
}
