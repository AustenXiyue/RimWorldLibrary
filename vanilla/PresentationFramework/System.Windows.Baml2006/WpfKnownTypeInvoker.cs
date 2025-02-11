using System.Xaml.Schema;

namespace System.Windows.Baml2006;

internal class WpfKnownTypeInvoker : XamlTypeInvoker
{
	private WpfKnownType _type;

	public WpfKnownTypeInvoker(WpfKnownType type)
		: base(type)
	{
		_type = type;
	}

	public override object CreateInstance(object[] arguments)
	{
		if ((arguments == null || arguments.Length == 0) && _type.DefaultConstructor != null)
		{
			return _type.DefaultConstructor();
		}
		if (_type.IsMarkupExtension)
		{
			if (!_type.Constructors.TryGetValue(arguments.Length, out var value))
			{
				throw new InvalidOperationException(SR.PositionalArgumentsWrongLength);
			}
			return value.Constructor(arguments);
		}
		return base.CreateInstance(arguments);
	}
}
