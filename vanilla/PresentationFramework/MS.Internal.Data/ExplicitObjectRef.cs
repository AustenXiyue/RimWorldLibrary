using System;
using System.Windows;

namespace MS.Internal.Data;

internal sealed class ExplicitObjectRef : ObjectRef
{
	private object _object;

	private WeakReference _element;

	protected override bool ProtectedUsesMentor => false;

	internal ExplicitObjectRef(object o)
	{
		if (o is DependencyObject)
		{
			_element = new WeakReference(o);
		}
		else
		{
			_object = o;
		}
	}

	internal override object GetObject(DependencyObject d, ObjectRefArgs args)
	{
		if (_element == null)
		{
			return _object;
		}
		return _element.Target;
	}

	internal override string Identify()
	{
		return "Source";
	}
}
