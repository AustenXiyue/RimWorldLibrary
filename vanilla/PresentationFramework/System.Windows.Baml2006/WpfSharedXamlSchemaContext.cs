using System.Collections.Generic;
using System.Xaml;

namespace System.Windows.Baml2006;

internal class WpfSharedXamlSchemaContext : WpfSharedBamlSchemaContext
{
	private Dictionary<Type, XamlType> _masterTypeTable = new Dictionary<Type, XamlType>();

	private readonly object _syncObject = new object();

	private bool _useV3Rules;

	public WpfSharedXamlSchemaContext(XamlSchemaContextSettings settings, bool useV3Rules)
		: base(settings)
	{
		_useV3Rules = useV3Rules;
	}

	public override XamlType GetXamlType(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		XamlType value;
		lock (_syncObject)
		{
			if (!_masterTypeTable.TryGetValue(type, out value))
			{
				RequireRuntimeType(type);
				value = CreateKnownBamlType(type.Name, isBamlType: false, _useV3Rules);
				if (value == null || value.UnderlyingType != type)
				{
					value = new WpfXamlType(type, this, isBamlScenario: false, _useV3Rules);
				}
				_masterTypeTable.Add(type, value);
			}
		}
		return value;
	}

	internal static void RequireRuntimeType(Type type)
	{
		if (!typeof(object).GetType().IsAssignableFrom(type.GetType()))
		{
			throw new ArgumentException(SR.Format(SR.RuntimeTypeRequired, type), "type");
		}
	}

	internal XamlType GetXamlTypeInternal(string xamlNamespace, string name, params XamlType[] typeArguments)
	{
		return GetXamlType(xamlNamespace, name, typeArguments);
	}
}
