using System;

namespace MS.Internal.Data;

internal sealed class AccessorInfo
{
	private readonly object _accessor;

	private readonly Type _propertyType;

	private readonly object[] _args;

	private int _generation;

	internal object Accessor => _accessor;

	internal Type PropertyType => _propertyType;

	internal object[] Args => _args;

	internal int Generation
	{
		get
		{
			return _generation;
		}
		set
		{
			_generation = value;
		}
	}

	internal AccessorInfo(object accessor, Type propertyType, object[] args)
	{
		_accessor = accessor;
		_propertyType = propertyType;
		_args = args;
	}
}
