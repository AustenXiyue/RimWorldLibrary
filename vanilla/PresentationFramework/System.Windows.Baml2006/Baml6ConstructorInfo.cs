using System.Collections.Generic;

namespace System.Windows.Baml2006;

internal struct Baml6ConstructorInfo
{
	private List<Type> _types;

	private Func<object[], object> _constructor;

	public List<Type> Types => _types;

	public Func<object[], object> Constructor => _constructor;

	public Baml6ConstructorInfo(List<Type> types, Func<object[], object> ctor)
	{
		_types = types;
		_constructor = ctor;
	}
}
