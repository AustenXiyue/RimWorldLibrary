using System;
using System.Globalization;
using System.Reflection;

namespace MS.Internal.Data;

internal class IndexerPropertyInfo : PropertyInfo
{
	private static readonly IndexerPropertyInfo _instance = new IndexerPropertyInfo();

	internal static IndexerPropertyInfo Instance => _instance;

	public override PropertyAttributes Attributes
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override bool CanRead => true;

	public override bool CanWrite => false;

	public override Type PropertyType => typeof(object);

	public override Type DeclaringType
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override string Name => "IndexerProperty";

	public override Type ReflectedType
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	private IndexerPropertyInfo()
	{
	}

	public override MethodInfo[] GetAccessors(bool nonPublic)
	{
		throw new NotImplementedException();
	}

	public override MethodInfo GetGetMethod(bool nonPublic)
	{
		return null;
	}

	public override ParameterInfo[] GetIndexParameters()
	{
		return Array.Empty<ParameterInfo>();
	}

	public override MethodInfo GetSetMethod(bool nonPublic)
	{
		return null;
	}

	public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
	{
		return obj;
	}

	public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
	{
		throw new NotImplementedException();
	}

	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		throw new NotImplementedException();
	}

	public override object[] GetCustomAttributes(bool inherit)
	{
		throw new NotImplementedException();
	}

	public override bool IsDefined(Type attributeType, bool inherit)
	{
		throw new NotImplementedException();
	}
}
